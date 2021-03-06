using System.Collections.Generic;



using System;

namespace Expressions {



public class Parser {
	public const int _EOF = 0;
	public const int _ident = 1;
	public const int _number = 2;
	public const int maxT = 11;

	const bool T = true;
	const bool x = false;
	const int minErrDist = 2;
	
	public Scanner scanner;
	public Errors  errors;

	public Token t;    // last recognized token
	public Token la;   // lookahead token
	int errDist = minErrDist;

public int res;

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

	
	void Expr(out int n) {
		int n1, n2; 
		TermF(out n1);
		n = n1; 
		while (la.kind == 2 || la.kind == 3) {
			if (la.kind == 3) {
				Get();
				TermF(out n2);
				n = (n1 == 1 || n2 == 1) ? 1 : 0; 
			} else {
				TermF(out n2);
				n = n2; 
			}
		}
	}

	void TermF(out int n) {
		int n1, n2; 
		TermG(out n1);
		n = n1; 
		while (la.kind == 4) {
			Get();
			TermG(out n2);
			n = (n1 == 1 && n2 == 1) ? 1 : 0; 
		}
	}

	void TermG(out int n) {
		int n1, n2; 
		TermH(out n1);
		n = n1; 
		if (la.kind == 5) {
			Get();
			TermH(out n2);
			n = (n1 == n2) ? 1 : 0; 
		} else if (la.kind == 6) {
			Get();
			TermH(out n2);
			n = (n1 < n2) ? 1 : 0; 
		} else if (la.kind == 7) {
			Get();
			TermH(out n2);
			n = (n1 > n2) ? 1 : 0; 
		} else if (StartOf(1)) {
		} else SynErr(12);
	}

	void TermH(out int n) {
		int n1, n2; 
		TermI(out n1);
		n = n1; 
		while (la.kind == 8 || la.kind == 9) {
			if (la.kind == 8) {
				Get();
				TermI(out n2);
				n = n+n2; 
			} else {
				Get();
				TermI(out n2);
				n = n-n2; 
			}
		}
	}

	void TermI(out int n) {
		Expect(2);
		n = Convert.ToInt32(t.val); 
		while (la.kind == 10) {
			Get();
			Expect(2);
			n = n*Convert.ToInt32(t.val); 
		}
	}

	void Expressions() {
		int n; 
		Expr(out n);
		res = n; 
	}



	public void Parse() {
		la = new Token();
		la.val = "";		
		Get();
		Expressions();
		Expect(0);

	}
	
	static readonly bool[,] set = {
		{T,x,x,x, x,x,x,x, x,x,x,x, x},
		{T,x,T,T, T,x,x,x, x,x,x,x, x}

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
			case 3: s = "\"|\" expected"; break;
			case 4: s = "\"&\" expected"; break;
			case 5: s = "\"==\" expected"; break;
			case 6: s = "\"<\" expected"; break;
			case 7: s = "\">\" expected"; break;
			case 8: s = "\"+\" expected"; break;
			case 9: s = "\"-\" expected"; break;
			case 10: s = "\"*\" expected"; break;
			case 11: s = "??? expected"; break;
			case 12: s = "invalid TermG"; break;

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