// Abstract syntax, checking and compilation of MicroC
// sestoft@itu.dk 2007-03-22, 2008-02-24

// Compile with:
//   coco -namespace MicroC MicroC.ATG
//   csc MicroC.cs Scanner.cs Parser.cs

// Usage: 
//    MicroC test1.c check
// or
//    MicroC test1.c compile
//    Machine a.out

// A runtime value is an integer; it may represent an integer, a
// Boolean (0,1) or a pointer, where a pointer is just an address in
// the store of a variable or pointer or the base address of an array.
   
// The compile-time environment maps a global variable to a fixed
// store address and maps a local variable to an offset into the
// store, relative to the bottom of the stack frame.  The run-time
// store maps a location to an integer.  This freely permits pointer
// arithmetics, as in real C.  A compile-time function environment
// maps a function name to a label.  In the generated code, labels are
// replaced by absolute code addresses.
   
// Expressions may have side effects.  A function takes a list of
// typed arguments but cannot return a result.
   
// Arrays can be one-dimensional and constant-size only.  For
// simplicity, we represent an array as a variable that holds the
// address of the first array element.  This is consistent with the
// way array-type parameters are handled in C, but not with the way
// array-type variables are handled.  Actually, this was how B (the
// predecessor of C) represented array variables.
   
// The store behaves as a stack, so all data are stack allocated:
// variables, function parameters and arrays.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace MicroC {
  class MainProgram {
    static void Main(string[] args) {
      if (args.Length != 2) {
	Usage();
	return;
      }
      Scanner scanner = new Scanner(args[0]);
      Parser parser = new Parser(scanner);
      parser.Parse();
      if (parser.errors.count != 0) 
	return;
      switch (args[1]) {
      case "check":
	try {
	  parser.program.Check();
	  Console.WriteLine("Program OK!");
	} catch (CompileException exn) {
	  Console.WriteLine("ERROR in program: " + exn.Message);
	}
	break;
      case "compile":
	parser.program.Compile();
	break;
      default: 
	Usage();
	break;
      }
    }

    private static void Usage() {
      Console.WriteLine("Usage: MicroC <program.c> [check|compile]");
    }

    public static IEnumerator<int> MakeInputReader(String filename) {
      Regex regex = new Regex("[^0-9]+");
      using (TextReader rd = new StreamReader(filename)) {
        for (String line = rd.ReadLine(); line != null; line = rd.ReadLine())
          foreach (String s in regex.Split(line))
            if (s != "")
              yield return int.Parse(s);
      }
    }
  }

  // ----------------------------------------------------------------------
  // MicroC expressions

  public abstract class Expression {
    abstract public Type Check(TEnv env);
    abstract public void Compile(CEnv env, Generator gen);
    // Only Access and its subtypes override this method:
    public virtual LvalueExpression MakeLvalue() { 
      throw new Exception("Illegal expression used as lvalue");
    }      
  }

  public class Constant : Expression {
    private readonly int value;
    private readonly Type type;

    public Constant(int value, Type type) {
      this.value = value;
      this.type = type;
    }

    public override Type Check(TEnv env) {
      return type;
    }

    public override void Compile(CEnv env, Generator gen) {
      gen.Emit(new CSTI(value));
    }
  }

  // Lvalue expression: variable, pointer deref, or index access.  The
  // net effect of executing the compiled code for an lvalue
  // expression is to leave an address (lvalue) on the stack.

  public class LvalueExpression : Expression {
    private readonly Access access;

    public LvalueExpression(Access access) {
      this.access = access;
    }

    public override Type Check(TEnv env) {
      return access.Check(env);
    }

    public override void Compile(CEnv env, Generator gen) {
      access.CompileLvalue(env, gen);
    }

    public override LvalueExpression MakeLvalue() { 
      return this;
    }
  }

  public enum Operator {
    Add, Sub, Mul, Div, Neg, Eq, Ne, Lt, Le, Gt, Ge, Not, Bad, And, Or, WriteI
  }

  public class BinOp : Expression {
    private readonly Operator op;
    private readonly Expression e1, e2;

    public BinOp(Operator op, Expression e1, Expression e2) {
      this.op = op;
      this.e1 = e1;
      this.e2 = e2;
    }

    public override Type Check(TEnv env) {
      Type t1 = e1.Check(env);
      Type t2 = e2.Check(env);
      switch (op) {
        case Operator.Add:
        case Operator.Sub:
          if (t1 is PointerType && t2.Equals(Type.intType))
            return t1;
          else if (t1.Equals(Type.intType) && t2.Equals(Type.intType))
            return Type.intType;
          else
            throw new CompileException("Arguments to (+) and (-) must be int");
        case Operator.Div:
        case Operator.Mul:
          if (t1.Equals(Type.intType) && t2.Equals(Type.intType))
            return Type.intType;
          else
            throw new CompileException("Arguments to (/) and (*) must be int");
		case Operator.Ge:
		case Operator.Gt:
		case Operator.Le:
		case Operator.Lt:
		case Operator.Eq:
		case Operator.Ne:
			if (t1.Equals(Type.intType) && t2.Equals(Type.intType))
				return Type.boolType;
			else
				throw new CompileException("Arguments to ==, !=, >, <, >=, <= must be int");
		default:
          throw new Exception("Unknown binary operator: " + op);
      }
    }

    public override void Compile(CEnv env, Generator gen) {
      e1.Compile(env, gen);
      e2.Compile(env, gen);
      switch (op) {
        case Operator.Add:
          gen.Emit(Instruction.ADD);
          break;
        case Operator.Div:
          gen.Emit(Instruction.DIV);
          break;
        case Operator.Mul:
          gen.Emit(Instruction.MUL);
          break;
        case Operator.Sub:
          gen.Emit(Instruction.SUB);
          break;
        case Operator.Eq:
          gen.Emit(Instruction.EQ);
          break;
        case Operator.Ne:
          gen.Emit(Instruction.EQ);
          gen.Emit(Instruction.NOT);
          break;
        case Operator.Ge:
          gen.Emit(Instruction.LT);
          gen.Emit(Instruction.NOT);
          break;
        case Operator.Gt:
          gen.Emit(Instruction.SWAP);
          gen.Emit(Instruction.LT);
          break;
        case Operator.Le:
          gen.Emit(Instruction.SWAP);
          gen.Emit(Instruction.LT);
          gen.Emit(Instruction.NOT);
          break;
        case Operator.Lt:
          gen.Emit(Instruction.LT);
          break;
        default:
          throw new Exception("Unknown binary operator: " + op);
      }
    }
  }

  public class UnOp : Expression {
    private readonly Operator op;
    private readonly Expression e1;

    public UnOp(Operator op, Expression e1) {
      this.op = op;
      this.e1 = e1;
    }

    public override Type Check(TEnv env) {
      Type t1 = e1.Check(env);
      switch (op) {
        case Operator.Neg:
          if (t1 == Type.intType)
	          return Type.intType;
	        else
            throw new CompileException("Argument to unary minus must be int");
        case Operator.Not:
          if (t1 == Type.boolType)
            return Type.boolType;
          else
            throw new CompileException("Argument to logical not must be bool");
        case Operator.WriteI:
          if (t1 == Type.intType || t1 is PointerType)
            return t1;
	        else
            throw new CompileException("Argument to write must be int or pointer");
        default:
          throw new Exception("Unknown unary operator: " + op);
      }
    }

    public override void Compile(CEnv env, Generator gen) {
      e1.Compile(env, gen);
      switch (op) {
        case Operator.Neg:
          gen.Emit(new CSTI(0));
          gen.Emit(Instruction.SWAP);
          gen.Emit(Instruction.SUB);
          break;
        case Operator.Not:
          gen.Emit(Instruction.NOT);
          break;
        case Operator.WriteI:
          gen.Emit(Instruction.PRINTI);
          break;
        default:
          throw new Exception("Unknown unary operator: " + op);
      }
    }
  }

  // The address-of operator (&)

  public class AddressOf : Expression {
    private readonly LvalueExpression e;

    public AddressOf(LvalueExpression e) {
      this.e = e;
    }

    public override Type Check(TEnv env) {
      return new PointerType(e.Check(env));
    }

    public override void Compile(CEnv env, Generator gen) {
      e.Compile(env, gen);
    }
  }

  public class Assignment : Expression {
    private readonly LvalueExpression lhs;
    private readonly Expression rhs;

    public Assignment(LvalueExpression lhs, Expression rhs) {
      this.lhs = lhs;
      this.rhs = rhs;
    }

    public override Type Check(TEnv env) {
      Type lhsType = lhs.Check(env), rhsType = rhs.Check(env);
      if (!lhsType.Equals(rhsType))
        throw new CompileException(String.Format("Assignment of ({0}) to ({1})", 
                                                 rhsType, lhsType));
      else
        return lhsType;
    }

    public override void Compile(CEnv env, Generator gen) {
      lhs.Compile(env, gen);
      rhs.Compile(env, gen);
      gen.Emit(Instruction.STI);
    }
  }

  public class Call : Expression {
    public readonly String funName;
    public readonly Expression[] arguments;

    public Call(String funName, Expression[] arguments) {
      this.funName = funName;
      this.arguments = arguments;
    }

    public override Type Check(TEnv env) {
      FunDecl funDecl = env.GetFun(funName);
      for (int i=0; i<arguments.Length; i++)
        if (!funDecl.parameters[i].type.Equals(arguments[i].Check(env)))
          throw new CompileException("Type mismatch in argument {0} of {1}", 
                                     funDecl.parameters[i].name, funName);
      return Type.voidType;
    }

    public override void Compile(CEnv env, Generator gen) {
      int n = arguments.Length;
      for (int i=0; i<n; i++)
        arguments[i].Compile(env, gen);
      String funLabel = env.GetFun(funName);
      gen.Emit(new CALL(n, funLabel));
    }
  }

  // ----------------------------------------------------------------------
  // MicroC statements 

  abstract public class Statement {
    abstract public void Check(TEnv env);
    abstract public void Compile(CEnv env, Generator gen);
  }

  // An expression statement is an expression followed by semicolon.
  // Examples: "x = e;" or "foo(6);"

  public class ExprStatement : Statement {
    private readonly Expression e;

    public ExprStatement(Expression e) {
      this.e = e;
    }

    public override void Check(TEnv env) {
      e.Check(env);
    }

    public override void Compile(CEnv env, Generator gen) {
      e.Compile(env, gen);
      gen.Emit(new INCSP(-1));
    }
  }

  // A block is a new scope with a sequence of statements.
  // Example: "{ int i; i=15; int sum; sum = 2*i; }"

  public class Block : Statement {
    public static readonly Block Empty = new Block();
    public readonly Statement[] statements;

    public Block(params Statement[] body) {
      this.statements = body;
    }

    public override void Check(TEnv env) {
      env.PushEnv();
      foreach (Statement stmt in statements)
        stmt.Check(env);
      env.PopEnv();
    }

    public override void Compile(CEnv env, Generator gen) {
      env.PushEnv();
      foreach (Statement stmt in statements)
        stmt.Compile(env, gen);
      gen.Emit(new INCSP(-env.MostLocalSize));
      env.PopEnv();
    }
  }

  // An if-else statement is a two-way conditional statement.
  // Example: "if (x>y) max=x; else max=y;"

  public class IfElse : Statement {
    private readonly Expression condition;
    private readonly Statement thenStmt, elseStmt;

    public IfElse(Expression condition, Statement thenStmt, Statement elseStmt) {
      this.condition = condition;
      this.thenStmt = thenStmt;
      this.elseStmt = elseStmt;
    }

    public override void Check(TEnv env) {
      if (condition.Check(env) == Type.boolType) {
        thenStmt.Check(env);
        elseStmt.Check(env);
      } else
        throw new CompileException("Non-bool condition {0} in if", condition);
    }

    public override void Compile(CEnv env, Generator gen) {
      // <condition>; IFZERO falseLab; <thenStmt>; GOTO endLab; 
      // falseLab: <elseStmt>; endLab:
      String falseLabel = Label.Fresh();
      String endLabel = Label.Fresh();
      condition.Compile(env, gen);
      gen.Emit(new IFZERO(falseLabel));
      thenStmt.Compile(env, gen);
      gen.Emit(new GOTO(endLabel));
      gen.Label(falseLabel);
      elseStmt.Compile(env, gen);
      gen.Label(endLabel);
    }
  }

  // A while statements
  // Example: "while (i<n) { sum = sum+i; i=i+1; }"

  public class While : Statement {
    private readonly Expression condition;
    private readonly Statement body;

    public While(Expression condition, Statement body) {
      this.condition = condition;
      this.body = body;
    }

    public override void Check(TEnv env) {
      if (condition.Check(env) == Type.boolType)
        body.Check(env);
      else
        throw new CompileException("Non-bool condition {0} in while", condition);
    }

    public override void Compile(CEnv env, Generator gen) {
      // startLab: <condition>; IFZERO endLab; <body>; GOTO startLab; endLab:
      String startLab = Label.Fresh();
      String endLab = Label.Fresh();
      gen.Label(startLab);
      condition.Compile(env, gen);
      gen.Emit(new IFZERO(endLab));
      body.Compile(env, gen);
      gen.Emit(new GOTO(startLab));
      gen.Label(endLab);
    }
  }

  // A read statement reads an integer to variable, pointer or array element.
  // Example: "read a[i];"

  public class Read : Statement {
    public readonly LvalueExpression e;

    public Read(LvalueExpression e) {
      this.e = e;
    }

    public override void Check(TEnv env) {
      if (e.Check(env) != Type.intType)
        throw new CompileException("Non-int recipient {0} in read", e);
    }

    public override void Compile(CEnv env, Generator gen) {
      e.Compile(env, gen);
      gen.Emit(Instruction.READ);
      gen.Emit(Instruction.STI);
      gen.Emit(new INCSP(-1));
    }
  }

  // ----------------------------------------------------------------------
  // An Access is a variable, pointer dereferencing or array indexing.
  // The net effect of the code generated for an Access is to leave an
  // rvalue on the stack top.  An Access can be turned into an
  // LvalueExpression, whose net effect is to leave the corresponding
  // lvalue on the stack top.

  public abstract class Access : Expression {
    public abstract void CompileLvalue(CEnv env, Generator gen);

    public override void Compile(CEnv env, Generator gen) {
      CompileLvalue(env, gen);
      gen.Emit(Instruction.LDI);
    }

    public override LvalueExpression MakeLvalue() {
      return new LvalueExpression(this);
    }
  }

  // A local or global variable

  public class VariableAccess : Access {
    public readonly String name;

    public VariableAccess(String name) {
      this.name = name;
    }

    public override Type Check(TEnv env) {
      return env.GetVariable(name);
    }

    public override void CompileLvalue(CEnv env, Generator gen) {
      env.CompileVariableAccess(gen, name);
    }
  }

  // An access of the form *e where e is a pointer expression

  public class DereferenceAccess : Access {
    private readonly Expression e;

    public DereferenceAccess(Expression e) {
      this.e = e;
    }

    public override Type Check(TEnv env) {
      PointerType ptrType = e.Check(env) as PointerType;
      if (ptrType != null)
        return ptrType.itemType;
      else
        throw new CompileException("Dereferencing non-pointer expression {0}", e);
    }

    // Compiling *e as lvalue is the same as compiling e as rvalue:
    public override void CompileLvalue(CEnv env, Generator gen) {
      e.Compile(env, gen);
    }
  }

  // An indexing of the form e1[e2] where e1 is array or pointer

  public class IndexAccess : Access {
    private readonly Expression e1;
    private readonly Expression e2;

    public IndexAccess(Expression e1, Expression e2) {
      this.e1 = e1;
      this.e2 = e2;
    }

    public override Type Check(TEnv env) {
      if (!e2.Check(env).Equals(Type.intType))
        throw new CompileException("Non-int index expression {0}", e2);
      Type e1Type = e1.Check(env);
      if (e1Type is ArrayType) 
        return ((ArrayType)e1Type).elementType;
      else if (e1Type is PointerType)
        return ((PointerType)e1Type).itemType;
      else
        throw new CompileException("Indexing on non-array/non-pointer type {0}", e1);
    }

    // Compiling e1[e2] is done by compiling e1 and e2 as rvalue and adding them:
    public override void CompileLvalue(CEnv env, Generator gen) {
      e1.Compile(env, gen);
      e2.Compile(env, gen);
      gen.Emit(Instruction.ADD);
    }
  }

  // Declarations

  public class VarDecl : Statement {
    public readonly String name;
    public readonly Type type;

    public VarDecl(String name, Type type) {
      this.name = name;
      this.type = type;
    }

    public override void Check(TEnv env) {
      env.DeclareLocal(this);
    }

    public override void Compile(CEnv env, Generator gen) {
      type.CompileAllocation(gen);    // Generate code
      env.DeclareLocal(this);         // Add to compilation environment
    }
  }

  // Void functions only

  public class FunDecl {
    public readonly String name;
    public readonly VarDecl[] parameters;
    public readonly Block body;

    public FunDecl(String name, VarDecl[] parameters, Block body) {
      this.name = name;
      this.parameters = parameters;
      this.body = body;
    }

    public void Check(TEnv env) {
      env.PushEnv();
      foreach (VarDecl parameter in parameters) 
        env.DeclareLocal(parameter);
      body.Check(env);
      env.PopEnv();
    }

    public void Compile(CEnv env, Generator gen) {
      env.PushEnv();
      foreach (VarDecl parameter in parameters) 
        env.DeclareLocal(parameter);
      gen.Label(env.GetFun(name));
      body.Compile(env, gen);
      gen.Emit(new RET(parameters.Length-1));
      env.PopEnv();
    }
  }

  // ----------------------------------------------------------------------
  // Complete MicroC programs 

  public class Program {
    private readonly Dictionary<String, VarDecl> globals;
    private readonly Dictionary<String, FunDecl> functions;

    public Program() {
      globals = new Dictionary<string, VarDecl>();
      functions = new Dictionary<string, FunDecl>();
    }

    public void AddVar(VarDecl decl) {
      globals.Add(decl.name, decl);
    }

    public void AddFun(FunDecl decl) {
      functions.Add(decl.name, decl);
    }

    public void Check() {
      TEnv env = new TEnv(globals, functions);
      foreach (FunDecl funDecl in functions.Values)
        funDecl.Check(env);
    }

    public void Compile() {
      CEnv env = new CEnv(globals, functions);
      Generator gen = new Generator();
      String mainLabel = env.GetFun("main");
      foreach (VarDecl varDecl in globals.Values)
        env.CompileAndDeclare(gen, varDecl);
      gen.Emit(new CALL(0, mainLabel));
      gen.Emit(Instruction.STOP);
      foreach (FunDecl funDecl in functions.Values)
        funDecl.Compile(new CEnv(env), gen);
      gen.PrintCode();
      int[] bytecode = gen.ToBytecode();
      using (TextWriter wr = new StreamWriter("a.out")) {
        foreach (int b in bytecode) {
          wr.Write(b);
          wr.Write(" ");
        }
      }
    }
  }

  // ----------------------------------------------------------------------
  // Types 

  abstract public class Type : IEquatable<Type> {
    public static readonly Type intType   = new PrimitiveType("int");
    public static readonly Type boolType  = new PrimitiveType("bool");
    public static readonly Type voidType  = new PrimitiveType("void");
    public static readonly Type errorType = new PrimitiveType("*ERROR*");

    public abstract bool Equals(Type that);

    // Size is the stack pointer increase caused by executing the code
    // generated by CompileAllocation
    public abstract void CompileAllocation(Generator gen);
    public abstract int Size { get; }
  }

  public class PrimitiveType : Type {
    public readonly String name;

    public PrimitiveType(String name) {
      this.name = name;
    }

    public override void CompileAllocation(Generator gen) {
      gen.Emit(new INCSP(1));
    }      

    public override int Size {
      get { return 1; }
    }

    public override bool Equals(Type that) {
      PrimitiveType primType = that as PrimitiveType;
      return primType != null && primType.name == this.name;
    }

    public override String ToString() {
      return name;
    }
  }

  public class PointerType : Type {
    public readonly Type itemType;

    public PointerType(Type itemType) {
      this.itemType = itemType;
    }

    public override void CompileAllocation(Generator gen) {
      gen.Emit(new INCSP(1));
    }      

    public override int Size {
      get { return 1; }
    }

    public override bool Equals(Type that) {
      PointerType ptrType = that as PointerType;
      return ptrType != null && ptrType.itemType.Equals(this.itemType);
    }

    public override String ToString() {
      return "pointer to " + itemType;
    }
  }

  public class ArrayType : Type {
    public readonly Type elementType;
    public readonly int? size;

    public ArrayType(Type elementType, int? size) {
      this.elementType = elementType;
      this.size = size;
    }

    public ArrayType(Type elementType) : this(elementType, null) { }

    // Array allocation sets aside size+1 cells, where the last one
    // holds the address of the first one

    public override void CompileAllocation(Generator gen) {
      if (size.HasValue) {
        gen.Emit(new INCSP(size.Value));
        gen.Emit(Instruction.GETSP);
        gen.Emit(new CSTI(size.Value-1));
        gen.Emit(Instruction.SUB);
      } else
        gen.Emit(new INCSP(1));
    }

    public override int Size {
      get { 
        return size.HasValue ? size.Value+1 : 1; 
      }
    }

    public override bool Equals(Type that) {
      ArrayType arrType = that as ArrayType;
      return arrType != null && arrType.elementType.Equals(this.elementType);
    }

    public override String ToString() {
      return "array " + (size.HasValue ? size.Value + " " : "") + "of " + elementType;
    }
  }

  // ----------------------------------------------------------------------
  // Auxiliary classes for type checking and compiletime environments 

  // Type checking environments

  public class TEnv {
    private readonly Dictionary<String, Type> globals;
    private readonly Stack<Dictionary<String, Type>> locals;
    private readonly Dictionary<String, FunDecl> functions;

    public TEnv(Dictionary<String, VarDecl> globalVarDecs,
                Dictionary<String, FunDecl> functions) {
      globals = new Dictionary<String, Type>();
      foreach (VarDecl varDecl in globalVarDecs.Values)
        globals.Add(varDecl.name, varDecl.type);
      locals = new Stack<Dictionary<String, Type>>();
      this.functions = functions;
    }

    public void PushEnv() {
      locals.Push(new Dictionary<String, Type>());
    }

    public void PopEnv() {
      locals.Pop();
    }

    public void DeclareLocal(VarDecl varDecl) {
      locals.Peek().Add(varDecl.name, varDecl.type);
    }

    public Type GetVariable(String name) {
      foreach (Dictionary<String, Type> localScope in locals)
        if (localScope.ContainsKey(name))
          return localScope[name];
      if (globals.ContainsKey(name))
        return globals[name];
      else
        throw new Exception("Undeclared variable: " + name);
    }

    public FunDecl GetFun(String name) {
      if (functions.ContainsKey(name)) 
        return functions[name];
      else 
        throw new Exception("Unknown function " + name);
    }
  }

  // Compilation environments

  public class CEnv {
    // The globals and functions do not change after initialization:
    private readonly Dictionary<String, int> globals;
    private readonly Dictionary<String, String> functions;
    private int globalsCount;
    // The locals stack is reset for each function:
    private readonly Stack<LocalsLocation> locals;
    private int nextOffset;

    public CEnv(Dictionary<String, VarDecl> globalVarDecls,
                Dictionary<String, FunDecl> funDecls) {
      globals = new Dictionary<String, int>();
      functions = new Dictionary<string, string>();
      foreach (String funName in funDecls.Keys)
        functions.Add(funName, Label.Fresh());
      locals = new Stack<LocalsLocation>();
      nextOffset = 0;
    }

    public CEnv(CEnv env) {
      this.globals = env.globals;
      this.functions = env.functions;
      locals = new Stack<LocalsLocation>();
      nextOffset = 0;
    }

    public void CompileAndDeclare(Generator gen, VarDecl varDecl) {
      varDecl.type.CompileAllocation(gen);
      globalsCount += varDecl.type.Size;
      globals.Add(varDecl.name, globalsCount-1);
    }

    public void PushEnv() {
      locals.Push(new LocalsLocation(nextOffset));
    }

    public void PopEnv() {
      nextOffset = locals.Peek().firstOffset;
      locals.Pop();
    }

    public int MostLocalSize {
      get {
        return nextOffset - locals.Peek().firstOffset;
      }
    }

    // Add local function parameter to compilation environment

    public int DeclareLocal(VarDecl varDecl) {
      int size = varDecl.type.Size;
      nextOffset += size;
      locals.Peek().Add(varDecl.name, nextOffset-1);
      return size;
    }

    // Generates code to compute the variable's address (lvalue):

    public void CompileVariableAccess(Generator gen, String name) {
      foreach (Dictionary<String, int> localScope in locals)
        if (localScope.ContainsKey(name)) {
          int offset = localScope[name];
          gen.Emit(Instruction.GETBP);
          gen.Emit(new CSTI(offset));
          gen.Emit(Instruction.ADD);
          return;
        }
      if (globals.ContainsKey(name)) {
        int offset = globals[name];
        gen.Emit(new CSTI(offset));
      } else
        throw new Exception("Undeclared variable: " + name);
    }

    public String GetFun(String name) {
      return functions[name];
    }
  }

  // Description of a runtime stack segment (scope) for locals

  public class LocalsLocation : Dictionary<String, int> {
    public readonly int firstOffset;

    public LocalsLocation(int firstOffset) {
      this.firstOffset = firstOffset;
    }
  }

  // ----------------------------------------------------------------------
  // Exceptions

  class CompileException : Exception {
    public CompileException(String msg, params Object[] args) 
           : base(String.Format(msg, args)) { }
  }

  // ----------------------------------------------------------------------
  // Auxiliary classes to perform bytecode generation

  public class Generator {
    private readonly List<Instruction> instructions;

    public Generator() {
      instructions = new List<Instruction>();
    }

    public void Emit(Instruction instr) {
      instructions.Add(instr);
    }

    public void Label(String label) {
      instructions.Add(new Label(label));
    }

    public int[] ToBytecode() {
      // Pass 1: Build mapping from labels to absolute addresses
      Dictionary<String, int> labelMap = new Dictionary<string, int>();
      int address = 0;
      foreach (Instruction instr in instructions) {
        if (instr is Label)
          labelMap.Add(((Label)instr).name, address);
        else
          address += instr.Size;
      }
      // Pass 2: Use mapping to convert symbolic code to bytes
      List<int> bytecode = new List<int>();
      foreach (Instruction instr in instructions)
        instr.ToBytecode(labelMap, bytecode);
      return bytecode.ToArray();
    }

    public void PrintCode() {
      int address = 0;
      foreach (Instruction instr in instructions) {
        Console.WriteLine("{0,5} {1}", address, instr);
        address += instr.Size;
      }
    }
  }

  public abstract class Instruction {
    public readonly Opcode opcode;
    public static readonly Instruction
      ADD    = new SimpleInstruction(Opcode.ADD),
      SUB    = new SimpleInstruction(Opcode.SUB),
      MUL    = new SimpleInstruction(Opcode.MUL),
      DIV    = new SimpleInstruction(Opcode.DIV),
      MOD    = new SimpleInstruction(Opcode.MOD),
      EQ     = new SimpleInstruction(Opcode.EQ),
      LT     = new SimpleInstruction(Opcode.LT),
      NOT    = new SimpleInstruction(Opcode.NOT),
      DUP    = new SimpleInstruction(Opcode.DUP),
      SWAP   = new SimpleInstruction(Opcode.SWAP),
      LDI    = new SimpleInstruction(Opcode.LDI),
      STI    = new SimpleInstruction(Opcode.STI),
      GETBP  = new SimpleInstruction(Opcode.GETBP),
      GETSP  = new SimpleInstruction(Opcode.GETSP),
      PRINTC = new SimpleInstruction(Opcode.PRINTC),
      PRINTI = new SimpleInstruction(Opcode.PRINTI),
      READ   = new SimpleInstruction(Opcode.READ),
      LDARGS = new SimpleInstruction(Opcode.LDARGS),
      STOP   = new SimpleInstruction(Opcode.STOP);

    public Instruction(Opcode opcode) {
      this.opcode = opcode;
    }

    public abstract int Size { get; }

    public abstract void ToBytecode(Dictionary<string, int> labelMap, List<int> bytecode);

    public override string ToString() {
      return opcode.ToString();
    }
  }

  public class Label : Instruction {  // Pseudo-instruction
    public readonly String name;
    private static int last = 0;  // For generating fresh labels

    public Label(String name)
      : base(Opcode.LABEL) {
      this.name = name;
    }

    public override int Size {
      get { return 0; }
    }

    public override void ToBytecode(Dictionary<string, int> labelMap, List<int> bytecode) {
      // No bytecode for a label
    }

    public static String Fresh() {
      last++;
      return "L" + last.ToString();
    }

    public override string ToString() {
      return name + ":";
    }
  }

  public class SimpleInstruction : Instruction {
    public SimpleInstruction(Opcode opcode) : base(opcode) { }

    public override int Size {
      get { return 1; }
    }

    public override void ToBytecode(Dictionary<string, int> labelMap, List<int> bytecode) {
      bytecode.Add((int)opcode);
    }

    public override string ToString() {
      return opcode.ToString();
    }
  }

  public class JumpInstruction : Instruction {
    public readonly String target;

    public JumpInstruction(Opcode opcode, String target)
      : base(opcode) {
      this.target = target;
    }

    public override int Size {
      get { return 2; }
    }

    public override void ToBytecode(Dictionary<string, int> labelMap, List<int> bytecode) {
      bytecode.Add((int)opcode);
      bytecode.Add(labelMap[target]);
    }

    public override string ToString() {
      return base.ToString() + " " + target;
    }
  }

  public class GOTO : JumpInstruction {
    public GOTO(String target) : base(Opcode.GOTO, target) { }
  }

  public class IFZERO : JumpInstruction {
    public IFZERO(String target) : base(Opcode.IFZERO, target) { }
  }

  public class IFNZRO : JumpInstruction {
    public IFNZRO(String target) : base(Opcode.IFNZRO, target) { }
  }

  public class CALL : Instruction {
    public readonly int argCount;
    public readonly String target;

    public CALL(int argCount, String target)
      : base(Opcode.CALL) {
      this.argCount = argCount;
      this.target = target;
    }

    public override int Size {
      get { return 3; }
    }

    public override void ToBytecode(Dictionary<string, int> labelMap, List<int> bytecode) {
      bytecode.Add((int)opcode);
      bytecode.Add(argCount);
      bytecode.Add(labelMap[target]);
    }

    public override string ToString() {
      return base.ToString() + " " + argCount.ToString() + " " + target;
    }
  }

  public class TCALL : Instruction {
    public readonly int argCount;
    public readonly int slideBy;
    public readonly String target;

    public TCALL(int argCount, int slideBy, String target)
      : base(Opcode.TCALL) {
      this.argCount = argCount;
      this.slideBy = slideBy;
      this.target = target;
    }

    public override int Size {
      get { return 4; }
    }

    public override void ToBytecode(Dictionary<string, int> labelMap, List<int> bytecode) {
      bytecode.Add((int)opcode);
      bytecode.Add(argCount);
      bytecode.Add(slideBy);
      bytecode.Add(labelMap[target]);
    }
  }

  public class IntArgInstr : Instruction {
    public readonly int argument;

    public IntArgInstr(Opcode opcode, int argument)
      : base(opcode) {
      this.argument = argument;
    }

    public override int Size {
      get { return 2; }
    }

    public override void ToBytecode(Dictionary<string, int> labelMap, List<int> bytecode) {
      bytecode.Add((int)opcode);
      bytecode.Add(argument);
    }

    public override string ToString() {
      return base.ToString() + " " + argument.ToString();
    }
  }

  public class CSTI : IntArgInstr {
    public CSTI(int argument) : base(Opcode.CSTI, argument) { }
  }

  public class INCSP : IntArgInstr {
    public INCSP(int argument) : base(Opcode.INCSP, argument) { }
  }

  public class RET : IntArgInstr {
    public RET(int argument) : base(Opcode.RET, argument) { }
  }

  public enum Opcode {
    LABEL = -1, // Unused
    CSTI, ADD, SUB, MUL, DIV, MOD, EQ, LT, NOT,
    DUP, SWAP, LDI, STI, GETBP, GETSP, INCSP,
    GOTO, IFZERO, IFNZRO, CALL, TCALL, RET,
    PRINTI, PRINTC, READ, LDARGS,
    STOP
  }
}
