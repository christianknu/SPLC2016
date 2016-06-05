using System.Collections.Generic;



using System;

namespace MicroC {



public class Parser {
	public const int _EOF = 0;
	public const int _ident = 1;
	public const int _number = 2;
	public const int _lpar = 3;
	public const int maxT = 34;

	const bool T = true;
	const bool x = false;
	const int minErrDist = 2;
	
	public Scanner scanner;
	public Errors  errors;

	public Token t;    // last recognized token
	public Token la;   // lookahead token
	int errDist = minErrDist;

public Program program;
  

  // A T2V transforms a type to a variable declaration.  Used
  // when parsing the obnoxious C variable declarations in VarDesc
  public delegate VarDecl T2V(Type t);



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

	
	void Ident(out String name) {
		Expect(1);
		name = t.val; 
	}

	void Number(out int val) {
		Expect(2);
		val = Convert.ToInt32(t.val); 
	}

	void MicroC() {
		program = new Program(); VarDecl varDecl; 
		while (la.kind == 4 || la.kind == 6 || la.kind == 7) {
			if (la.kind == 4) {
				Get();
				FunctionDef(program);
			} else {
				VarDec(out varDecl);
				Expect(5);
				program.AddVar(varDecl); 
			}
		}
		Expect(0);
	}

	void FunctionDef(Program program) {
		String name; Block body; 
		List<VarDecl> parList = new List<VarDecl>(); 
		
		Ident(out name);
		Expect(3);
		if (la.kind == 6 || la.kind == 7) {
			FormalParamList(parList);
		}
		Expect(10);
		BlockStmt(out body);
		VarDecl[] parameters = parList.ToArray(); 
		program.AddFun(new FunDecl(name, parameters, body));
		
	}

	void VarDec(out VarDecl varDecl) {
		Type ty; T2V t2v; 
		Typ(out ty);
		VarDesc(out t2v);
		varDecl = t2v(ty); 
	}

	void Typ(out Type ty) {
		ty = Type.errorType; 
		if (la.kind == 6) {
			Get();
			ty = Type.intType;   
		} else if (la.kind == 7) {
			Get();
			ty = Type.boolType;  
		} else SynErr(35);
	}

	void ArraySize(out int? size) {
		int n; size = null; 
		Expect(8);
		if (la.kind == 2) {
			Number(out n);
			size = n; 
		}
		Expect(9);
	}

	void FormalParamList(List<VarDecl> parList) {
		VarDecl parameter; 
		VarDec(out parameter);
		parList.Add(parameter); 
		while (la.kind == 11) {
			Get();
			VarDec(out parameter);
			parList.Add(parameter); 
		}
	}

	void BlockStmt(out Block block) {
		Statement stmt; 
		List<Statement> stmtList = new List<Statement>(); 
		
		Expect(13);
		while (StartOf(1)) {
			Stmt(out stmt);
			stmtList.Add(stmt); 
		}
		Expect(14);
		block = new Block(stmtList.ToArray()); 
	}

	void Stmt(out Statement stmt) {
		stmt = null; Block block; 
		VarDecl varDecl; Expression e; 
		switch (la.kind) {
		case 1: case 2: case 3: case 18: case 20: case 29: case 30: case 32: case 33: {
			ExprStmt(out stmt);
			break;
		}
		case 13: {
			BlockStmt(out block);
			stmt = block; 
			break;
		}
		case 15: {
			IfStmt(out stmt);
			break;
		}
		case 5: {
			NullStmt(out stmt);
			break;
		}
		case 17: {
			WhileStmt(out stmt);
			break;
		}
		case 12: {
			Get();
			Expr(out e);
			Expect(5);
			stmt = new Read(e.MakeLvalue()); 
			break;
		}
		case 6: case 7: {
			VarDec(out varDecl);
			Expect(5);
			stmt = varDecl; 
			break;
		}
		default: SynErr(36); break;
		}
	}

	void ExprStmt(out Statement stmt) {
		Expression e; 
		Expr(out e);
		Expect(5);
		stmt = new ExprStatement(e); 
	}

	void IfStmt(out Statement stmt) {
		Expression e; Statement s1, s2 = Block.Empty; 
		Expect(15);
		Expect(3);
		Expr(out e);
		Expect(10);
		Stmt(out s1);
		if (la.kind == 16) {
			Get();
			Stmt(out s2);
		}
		stmt = new IfElse(e, s1, s2); 
	}

	void NullStmt(out Statement stmt) {
		Expect(5);
		stmt = new Block(); 
	}

	void WhileStmt(out Statement stmt) {
		Expression e; Statement body; 
		Expect(17);
		Expect(3);
		Expr(out e);
		Expect(10);
		Stmt(out body);
		stmt = new While(e, body); 
	}

	void Expr(out Expression e) {
		Expression rhs; e = null; 
		if (StartOf(2)) {
			LogOrExp(out e);
			if (la.kind == 19) {
				Get();
				Expr(out rhs);
				e = new Assignment(e.MakeLvalue(), rhs); 
			}
		} else if (la.kind == 20) {
			Get();
			Expr(out e);
			e = new UnOp(Operator.WriteI, e);  
		} else SynErr(37);
	}

	void VarDesc(out T2V t2v) {
		String name; int? size; t2v = null; 
		if (la.kind == 1) {
			Ident(out name);
			t2v = delegate(Type ty) { return new VarDecl(name, ty); }; 
		} else if (la.kind == 18) {
			Get();
			VarDesc(out t2v);
			T2V outer = t2v; 
			t2v = delegate(Type ty) { return outer(new PointerType(ty)); }; 
			
		} else if (la.kind == 3) {
			Get();
			VarDesc(out t2v);
			Expect(10);
		} else SynErr(38);
		if (la.kind == 8) {
			ArraySize(out size);
			T2V outer = t2v; 
			t2v = delegate(Type ty) { return outer(new ArrayType(ty, size)); }; 
			
		}
	}

	void LogOrExp(out Expression e) {
		Expression e2; 
		LogAndExp(out e);
		while (la.kind == 21) {
			Get();
			LogAndExp(out e2);
			e = new BinOp(Operator.Or, e, e2); 
		}
	}

	void LogAndExp(out Expression e) {
		Expression e2; 
		EqualExp(out e);
		while (la.kind == 22) {
			Get();
			EqualExp(out e2);
			e = new BinOp(Operator.And, e, e2); 
		}
	}

	void EqualExp(out Expression e) {
		Expression e2; Operator op; 
		RelationExp(out e);
		while (la.kind == 23 || la.kind == 24) {
			if (la.kind == 23) {
				Get();
				op = Operator.Eq; 
			} else {
				Get();
				op = Operator.Ne; 
			}
			RelationExp(out e2);
			e = new BinOp(op, e, e2); 
		}
	}

	void RelationExp(out Expression e) {
		Expression e2; Operator op; 
		AddExp(out e);
		while (StartOf(3)) {
			if (la.kind == 25) {
				Get();
				op = Operator.Lt; 
			} else if (la.kind == 26) {
				Get();
				op = Operator.Gt; 
			} else if (la.kind == 27) {
				Get();
				op = Operator.Le; 
			} else {
				Get();
				op = Operator.Ge; 
			}
			AddExp(out e2);
			e = new BinOp(op, e, e2); 
		}
	}

	void AddExp(out Expression e) {
		Expression e2; Operator op; 
		MultExp(out e);
		while (la.kind == 29 || la.kind == 30) {
			if (la.kind == 29) {
				Get();
				op = Operator.Add; 
			} else {
				Get();
				op = Operator.Sub; 
			}
			MultExp(out e2);
			e = new BinOp(op, e, e2); 
		}
	}

	void MultExp(out Expression e) {
		Expression e2; Operator op; 
		UnaryExp(out e);
		while (la.kind == 18 || la.kind == 31) {
			if (la.kind == 18) {
				Get();
				op = Operator.Mul; 
			} else {
				Get();
				op = Operator.Div; 
			}
			UnaryExp(out e2);
			e = new BinOp(op, e, e2); 
		}
	}

	void UnaryExp(out Expression e) {
		Expression e1; e = null; 
		switch (la.kind) {
		case 1: case 2: case 3: {
			PostFixExp(out e);
			break;
		}
		case 29: {
			Get();
			UnaryExp(out e);
			break;
		}
		case 30: {
			Get();
			UnaryExp(out e1);
			e = new UnOp(Operator.Neg, e1);     
			break;
		}
		case 32: {
			Get();
			UnaryExp(out e1);
			e = new UnOp(Operator.Not, e1);     
			break;
		}
		case 18: {
			Get();
			UnaryExp(out e1);
			e = new DereferenceAccess(e1);      
			break;
		}
		case 33: {
			Get();
			UnaryExp(out e1);
			e = new AddressOf(e1.MakeLvalue()); 
			break;
		}
		default: SynErr(39); break;
		}
	}

	void PostFixExp(out Expression e) {
		Expression e1; Expression[] es = new Expression[0]; 
		Primary(out e);
		if (la.kind == 3 || la.kind == 8) {
			if (la.kind == 8) {
				Get();
				Expr(out e1);
				e = new IndexAccess(e, e1); 
				Expect(9);
			} else {
				Get();
				if (StartOf(4)) {
					ActualPars(out es);
				}
				Expect(10);
				VariableAccess varAcc = e as VariableAccess;
				if (varAcc != null) 
				 e = new Call(varAcc.name, es); 
				else
				 throw new Exception("Calling non-name expression");
				
			}
		}
	}

	void Primary(out Expression e) {
		String name; int n; e = null;      
		if (la.kind == 1) {
			Ident(out name);
			e = new VariableAccess(name);      
		} else if (la.kind == 2) {
			Number(out n);
			e = new Constant(n, Type.intType); 
		} else if (la.kind == 3) {
			Get();
			Expr(out e);
			Expect(10);
		} else SynErr(40);
	}

	void ActualPars(out Expression[] es) {
		Expression e; List<Expression> eList = new List<Expression>(); 
		Expr(out e);
		eList.Add(e); 
		while (la.kind == 11) {
			Get();
			Expr(out e);
			eList.Add(e); 
		}
		es = eList.ToArray(); 
	}



	public void Parse() {
		la = new Token();
		la.val = "";		
		Get();
		MicroC();
		Expect(0);

	}
	
	static readonly bool[,] set = {
		{T,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x},
		{x,T,T,T, x,T,T,T, x,x,x,x, T,T,x,T, x,T,T,x, T,x,x,x, x,x,x,x, x,T,T,x, T,T,x,x},
		{x,T,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, x,x,x,x, x,x,x,x, x,T,T,x, T,T,x,x},
		{x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,x,x,x, x,T,T,T, T,x,x,x, x,x,x,x},
		{x,T,T,T, x,x,x,x, x,x,x,x, x,x,x,x, x,x,T,x, T,x,x,x, x,x,x,x, x,T,T,x, T,T,x,x}

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
			case 3: s = "lpar expected"; break;
			case 4: s = "\"void\" expected"; break;
			case 5: s = "\";\" expected"; break;
			case 6: s = "\"int\" expected"; break;
			case 7: s = "\"bool\" expected"; break;
			case 8: s = "\"[\" expected"; break;
			case 9: s = "\"]\" expected"; break;
			case 10: s = "\")\" expected"; break;
			case 11: s = "\",\" expected"; break;
			case 12: s = "\"read\" expected"; break;
			case 13: s = "\"{\" expected"; break;
			case 14: s = "\"}\" expected"; break;
			case 15: s = "\"if\" expected"; break;
			case 16: s = "\"else\" expected"; break;
			case 17: s = "\"while\" expected"; break;
			case 18: s = "\"*\" expected"; break;
			case 19: s = "\"=\" expected"; break;
			case 20: s = "\"write\" expected"; break;
			case 21: s = "\"||\" expected"; break;
			case 22: s = "\"&&\" expected"; break;
			case 23: s = "\"==\" expected"; break;
			case 24: s = "\"!=\" expected"; break;
			case 25: s = "\"<\" expected"; break;
			case 26: s = "\">\" expected"; break;
			case 27: s = "\"<=\" expected"; break;
			case 28: s = "\">=\" expected"; break;
			case 29: s = "\"+\" expected"; break;
			case 30: s = "\"-\" expected"; break;
			case 31: s = "\"/\" expected"; break;
			case 32: s = "\"!\" expected"; break;
			case 33: s = "\"&\" expected"; break;
			case 34: s = "??? expected"; break;
			case 35: s = "invalid Typ"; break;
			case 36: s = "invalid Stmt"; break;
			case 37: s = "invalid Expr"; break;
			case 38: s = "invalid VarDesc"; break;
			case 39: s = "invalid UnaryExp"; break;
			case 40: s = "invalid Primary"; break;

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