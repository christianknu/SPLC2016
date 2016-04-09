using System.Collections.Generic;



using System;

namespace Expressions {



public class Parser {
	public const int _EOF = 0;
	public const int _ident = 1;
	public const int _number = 2;
	public const int maxT = 25;

	const bool T = true;
	const bool x = false;
	const int minErrDist = 2;
	
	public Scanner scanner;
	public Errors  errors;

	public Token t;    // last recognized token
	public Token la;   // lookahead token
	int errDist = minErrDist;

public Program program;

/*--------------------------------------------------------------------------*/


	public Parser(Scanner scanner) {
		this.scanner = scanner;
		errors = new Errors();
	}

	void SynErr (int n) {
		if (errDist >= minErrDist) errors.SynErr(la.line, la.col, n);
		errDist = 0;
	}

	public void SemErr (string msg) {
		if (errDist >= minErrDist) errors.SemErr(t.line, t.col, msg);
		errDist = 0;
	}
	
	void Get () {
		for (;;) {
			t = la;
			la = scanner.Scan();
			if (la.kind <= maxT) { ++errDist; break; }

			la = t;
		}
	}
	
	void Expect (int n) {
		if (la.kind==n) Get(); else { SynErr(n); }
	}
	
	bool StartOf (int s) {
		return set[s, la.kind];
	}
	
	void ExpectWeak (int n, int follow) {
		if (la.kind == n) Get();
		else {
			SynErr(n);
			while (!StartOf(follow)) Get();
		}
	}


	bool WeakSeparator(int n, int syFol, int repFol) {
		int kind = la.kind;
		if (kind == n) {Get(); return true;}
		else if (StartOf(repFol)) {return false;}
		else {
			SynErr(n);
			while (!(set[syFol, kind] || set[repFol, kind] || set[0, kind])) {
				Get();
				kind = la.kind;
			}
			return StartOf(syFol);
		}
	}

	
	void Program(out Program p) {
		p = null; FuncDef f = null; String name = null; Expression e = null;
		    Dictionary<String,FuncDef> functions = new Dictionary<string, FuncDef>(); 
		while (la.kind == 7 || la.kind == 8) {
			FuncDef(out f, out name);
			functions.Add(name, f); 
		}
		IfElseExpr(out e);
		p = new Program(functions, e); 
	}

	void FuncDef(out FuncDef f, out String name) {
		f = null; String an = null; Expression e = null;
		                                   Type at = null; Type rt = null;
		TypeExpr(out rt);
		Ident(out name);
		Expect(3);
		TypeExpr(out at);
		Ident(out an);
		Expect(4);
		Expect(5);
		Expr(out e);
		Expect(6);
		f = new FuncDef(rt, name, at, an, e); 
	}

	void IfElseExpr(out Expression e) {
		Expression e1, e2, e3; e = null; 
		if (la.kind == 0 || la.kind == 9) {
			while (la.kind == 9) {
				Get();
				SimBoolExpr(out e1);
				Expect(10);
				Expr(out e2);
				Expect(11);
				Expr(out e3);
				e = new IfElseExpression(e1, e2, e3); 
			}
		} else if (StartOf(1)) {
			Expr(out e);
		} else SynErr(26);
	}

	void TypeExpr(out Type t) {
		t = null; 
		if (la.kind == 7) {
			Get();
			t = Type.intType; 
		} else if (la.kind == 8) {
			Get();
			t = Type.boolType; 
		} else SynErr(27);
	}

	void Ident(out String name) {
		Expect(1);
		name = t.val; 
	}

	void Expr(out Expression e) {
		Expression e1, e2; Operator op; e = null; 
		BoolTerm(out e1);
		e = e1; 
		while (la.kind == 12) {
			AndOp(out op);
			BoolTerm(out e2);
			e = new BinOp(op, e, e2); 
		}
	}

	void BoolTerm(out Expression e) {
		Expression e1, e2; Operator op; e = null; 
		SimBoolExpr(out e1);
		e = e1; 
		while (la.kind == 13) {
			OrOp(out op);
			SimBoolExpr(out e2);
			e = new BinOp(op, e, e2); 
		}
	}

	void AndOp(out Operator op) {
		op = Operator.Bad; 
		Expect(12);
		op = Operator.And; 
	}

	void SimBoolExpr(out Expression e) {
		Expression e1, e2; Operator op; e = null; 
		SimExpr(out e1);
		e = e1; 
		if (StartOf(2)) {
			RelOp(out op);
			SimExpr(out e2);
			e = new BinOp(op, e, e2); 
		}
	}

	void OrOp(out Operator op) {
		op = Operator.Bad; 
		Expect(13);
		op = Operator.Or; 
	}

	void SimExpr(out Expression e) {
		Expression e1, e2; Operator op; 
		Term(out e1);
		e = e1; 
		while (la.kind == 20 || la.kind == 21 || la.kind == 22) {
			if (la.kind == 20 || la.kind == 21) {
				AddOp(out op);
			} else {
				ModOp(out op);
			}
			Term(out e2);
			e = new BinOp(op, e, e2); 
		}
	}

	void RelOp(out Operator op) {
		op = Operator.Bad; 
		switch (la.kind) {
		case 14: {
			Get();
			op = Operator.Eq;  
			break;
		}
		case 15: {
			Get();
			op = Operator.Ne;  
			break;
		}
		case 16: {
			Get();
			op = Operator.Lt;  
			break;
		}
		case 17: {
			Get();
			op = Operator.Le;  
			break;
		}
		case 18: {
			Get();
			op = Operator.Gt;  
			break;
		}
		case 19: {
			Get();
			op = Operator.Ge;  
			break;
		}
		default: SynErr(28); break;
		}
	}

	void Term(out Expression e) {
		Operator op; Expression e1, e2; 
		Factor(out e1);
		e = e1;                         
		while (la.kind == 23 || la.kind == 24) {
			MulOp(out op);
			Factor(out e2);
			e = new BinOp(op, e, e2);       
		}
	}

	void AddOp(out Operator op) {
		op = Operator.Bad; 
		if (la.kind == 20) {
			Get();
			op = Operator.Add; 
		} else if (la.kind == 21) {
			Get();
			op = Operator.Sub; 
		} else SynErr(29);
	}

	void ModOp(out Operator op) {
		op = Operator.Bad; 
		Expect(22);
		op = Operator.Mod; 
	}

	void Factor(out Expression e) {
		String name; Expression e1; e = null; 
		if (la.kind == 1) {
			Ident(out name);
			e = new Variable(name); 
			if (la.kind == 3) {
				Get();
				Expr(out e1);
				Expect(4);
				e = new FuncCall(name, e1); 
			}
		} else if (la.kind == 2) {
			Get();
			e = new Constant(Convert.ToInt32(t.val),
			                Type.intType); 
		} else if (la.kind == 21) {
			Get();
			Factor(out e1);
			e = new UnOp(Operator.Neg, e1); 
		} else if (la.kind == 3) {
			Get();
			Expr(out e1);
			Expect(4);
			e = e1; 
		} else SynErr(30);
	}

	void MulOp(out Operator op) {
		op = Operator.Bad; 
		if (la.kind == 23) {
			Get();
			op = Operator.Mul; 
		} else if (la.kind == 24) {
			Get();
			op = Operator.Div; 
		} else SynErr(31);
	}

	void Expressions() {
		Program p; 
		Program(out p);
		program = p; 
	}



	public void Parse() {
		la = new Token();
		la.val = "";		
		Get();
		Expressions();
		Expect(0);

	}
	
	static readonly bool[,] set = {
		{T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x},
		{x,T,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,x,x, x,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,T, T,T,T,T, x,x,x,x, x,x,x}

	};
} // end Parser


public class Errors {
	public int count = 0;                                    // number of errors detected
	public System.IO.TextWriter errorStream = Console.Out;   // error messages go to this stream
	public string errMsgFormat = "-- line {0} col {1}: {2}"; // 0=line, 1=column, 2=text

	public virtual void SynErr (int line, int col, int n) {
		string s;
		switch (n) {
			case 0: s = "EOF expected"; break;
			case 1: s = "ident expected"; break;
			case 2: s = "number expected"; break;
			case 3: s = "\"(\" expected"; break;
			case 4: s = "\")\" expected"; break;
			case 5: s = "\"=\" expected"; break;
			case 6: s = "\";\" expected"; break;
			case 7: s = "\"int\" expected"; break;
			case 8: s = "\"bool\" expected"; break;
			case 9: s = "\"if\" expected"; break;
			case 10: s = "\"then\" expected"; break;
			case 11: s = "\"else\" expected"; break;
			case 12: s = "\"&\" expected"; break;
			case 13: s = "\"|\" expected"; break;
			case 14: s = "\"==\" expected"; break;
			case 15: s = "\"!=\" expected"; break;
			case 16: s = "\"<\" expected"; break;
			case 17: s = "\"<=\" expected"; break;
			case 18: s = "\">\" expected"; break;
			case 19: s = "\">=\" expected"; break;
			case 20: s = "\"+\" expected"; break;
			case 21: s = "\"-\" expected"; break;
			case 22: s = "\"%\" expected"; break;
			case 23: s = "\"*\" expected"; break;
			case 24: s = "\"/\" expected"; break;
			case 25: s = "??? expected"; break;
			case 26: s = "invalid IfElseExpr"; break;
			case 27: s = "invalid TypeExpr"; break;
			case 28: s = "invalid RelOp"; break;
			case 29: s = "invalid AddOp"; break;
			case 30: s = "invalid Factor"; break;
			case 31: s = "invalid MulOp"; break;

			default: s = "error " + n; break;
		}
		errorStream.WriteLine(errMsgFormat, line, col, s);
		count++;
	}

	public virtual void SemErr (int line, int col, string s) {
		errorStream.WriteLine(errMsgFormat, line, col, s);
		count++;
	}
	
	public virtual void SemErr (string s) {
		errorStream.WriteLine(s);
		count++;
	}
	
	public virtual void Warning (int line, int col, string s) {
		errorStream.WriteLine(errMsgFormat, line, col, s);
	}
	
	public virtual void Warning(string s) {
		errorStream.WriteLine(s);
	}
} // Errors


public class FatalError: Exception {
	public FatalError(string m): base(m) {}
}
}