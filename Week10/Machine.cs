
/* A unified-stack abstract machine for imperative programs
   sestoft@dina.kvl.dk * 2001-03-21, 2007-03-15

   Compile this file with 

      csc Machine.cs

   To execute a program file using this abstract machine, do:

      Machine <programfile> <arg1> <arg2> ...

   or

      Machine -trace <programfile> <arg1> <arg2> ...
 
   The READ instruction will attempt to read from file a.in if it exists.
*/

using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions; 

class Machine {
  static void Main(string[] args) {
    bool trace = args.Length>0 && (args[0] == "-trace" || args[0] == "/trace");
    if (args.Length==0 || trace && args.Length==1) 
      Console.WriteLine("Usage: Machine [-trace] <programfile> <arg1> ...\n");
    else 
      Execute(args, trace);
  }

  // These numeric instruction codes must agree with code Machine.sml:

  public enum Opcode {
    LABEL = -1, // Unused
    CSTI, ADD, SUB, MUL, DIV, MOD, EQ, LT, NOT,
    DUP, SWAP, LDI, STI, GETBP, GETSP, INCSP,
    GOTO, IFZERO, IFNZRO, CALL, TCALL, RET,
    PRINTI, PRINTC, READ, LDARGS,
    STOP
  }

  // Read code from file and execute it

  const int STACKSIZE = 1000;
  
  static void Execute(String[] args, bool trace) {
    int firstArg = trace ? 1 : 0;
    int[] p = Readfile(args[firstArg]);         // Read the program from file
    int[] s = new int[STACKSIZE];               // The evaluation stack
    IEnumerator<int> inputs = MakeInputReader("a.in").GetEnumerator();
    int[] iargs = new int[args.Length-firstArg-1];
    for (int i=firstArg+1; i<args.Length; i++)  // Commandline arguments
      iargs[i-1] = int.Parse(args[i]);
    DateTime starttime = DateTime.Now;
    ExecCode(p, s, iargs, inputs, trace);       // Execute program
    double dur = (DateTime.Now - starttime).TotalSeconds;
    Console.Error.WriteLine("\nRan " + dur + " seconds");
  }

  // Read a stream of integers from a text file

  public static IEnumerable<int> MakeInputReader(String filename) {
    FileInfo fi = new FileInfo(filename);
    if (fi.Exists) {
      Regex regex = new Regex("[ \\t]+");
      using (TextReader rd = fi.OpenText()) {
        for (String line = rd.ReadLine(); line != null; line = rd.ReadLine())
          foreach (String s in regex.Split(line))
            if (s != "")
              yield return int.Parse(s);
      }
    } else
      yield break;
  }

  // The machine: execute the code starting at p[pc] 

  static int ExecCode(int[] p, int[] s, int[] iargs,
                      IEnumerator<int> inputs, bool trace) {
    int bp = -999;  // Base pointer, for accessing local
    int sp = -1;    // Stack top pointer: current top of stack
    int pc = 0;     // Program counter: next instruction
    for (;;) {
      if (trace) 
        PrintSpPc(s, bp, sp, p, pc);
      switch ((Opcode)p[pc++]) {
      case Opcode.CSTI:
        s[sp+1] = p[pc++]; sp++; break;
      case Opcode.ADD: 
        s[sp-1] = s[sp-1] + s[sp]; sp--; break;
      case Opcode.SUB: 
        s[sp-1] = s[sp-1] - s[sp]; sp--; break;
      case Opcode.MUL: 
        s[sp-1] = s[sp-1] * s[sp]; sp--; break;
      case Opcode.DIV: 
        s[sp-1] = s[sp-1] / s[sp]; sp--; break;
      case Opcode.MOD: 
        s[sp-1] = s[sp-1] % s[sp]; sp--; break;
      case Opcode.EQ: 
        s[sp-1] = (s[sp-1] == s[sp] ? 1 : 0); sp--; break;
      case Opcode.LT: 
        s[sp-1] = (s[sp-1] < s[sp] ? 1 : 0); sp--; break;
      case Opcode.NOT: 
        s[sp] = (s[sp] == 0 ? 1 : 0); break;
      case Opcode.DUP: 
        s[sp+1] = s[sp]; sp++; break;
      case Opcode.SWAP: 
        { int tmp = s[sp];  s[sp] = s[sp-1];  s[sp-1] = tmp; } break; 
      case Opcode.LDI:                 // load indirect
        s[sp] = s[s[sp]]; break;
      case Opcode.STI:                 // store indirect, keep value on top
        s[s[sp-1]] = s[sp]; s[sp-1] = s[sp]; sp--; break;
      case Opcode.GETBP:
        s[sp+1] = bp; sp++; break;
      case Opcode.GETSP:
        s[sp+1] = sp; sp++; break;
      case Opcode.INCSP:
        sp = sp+p[pc++]; break;
      case Opcode.GOTO:
        pc = p[pc]; break;
      case Opcode.IFZERO:
        pc = (s[sp--] == 0 ? p[pc] : pc+1); break;
      case Opcode.IFNZRO:
        pc = (s[sp--] != 0 ? p[pc] : pc+1); break;
      case Opcode.CALL: { 
        int argc = p[pc++];
        for (int i=0; i<argc; i++)	   // Make room for return address
          s[sp-i+2] = s[sp-i];		   // and old base pointer
        s[sp-argc+1] = pc+1; sp++; 
        s[sp-argc+1] = bp;   sp++; 
        bp = sp+1-argc;
        pc = p[pc]; 
      } break; 
      case Opcode.TCALL: { 
        int argc = p[pc++];                // Number of new arguments
        int pop  = p[pc++];		   // Number of variables to discard
        for (int i=argc-1; i>=0; i--)	   // Discard variables
          s[sp-i-pop] = s[sp-i];
        sp = sp - pop; pc = p[pc]; 
      } break; 
      case Opcode.RET: { 
        int res = s[sp]; 
        sp = sp-p[pc]; bp = s[--sp]; pc = s[--sp]; 
        s[sp] = res; 
      } break; 
      case Opcode.PRINTI:
        Console.Write(s[sp] + " "); break; 
      case Opcode.PRINTC:
        Console.Write((char)(s[sp])); break; 
      case Opcode.LDARGS:
        foreach (int x in iargs)
          s[++sp] = x;
        break;
      case Opcode.READ:
        if (inputs.MoveNext())
          s[++sp] = inputs.Current;
        else
          throw new Exception("No more input");
        break;
      case Opcode.STOP:
        return sp;
      default:                  
        throw new Exception("Illegal instruction " + p[pc-1] 
			    + " at address " + (pc-1));
      }
    }
  }

  // Print the stack machine instruction at p[pc]

  static String InsName(int[] p, int pc) {
    switch ((Opcode)p[pc]) {
    case Opcode.CSTI:   return "CST " + p[pc+1]; 
    case Opcode.ADD:    return "ADD";
    case Opcode.SUB:    return "SUB";
    case Opcode.MUL:    return "MUL";
    case Opcode.DIV:    return "DIV";
    case Opcode.MOD:    return "MOD";
    case Opcode.EQ:     return "EQ";
    case Opcode.LT:     return "LT";
    case Opcode.NOT:    return "NOT";
    case Opcode.DUP:    return "DUP";
    case Opcode.SWAP:   return "SWAP";
    case Opcode.LDI:    return "LDI";
    case Opcode.STI:    return "STI";
    case Opcode.GETBP:  return "GETBP";
    case Opcode.GETSP:  return "GETSP";
    case Opcode.INCSP:  return "INCSP " + p[pc+1];
    case Opcode.GOTO:   return "GOTO " + p[pc+1];
    case Opcode.IFZERO: return "IFZERO " + p[pc+1];
    case Opcode.IFNZRO: return "IFNZRO " + p[pc+1];
    case Opcode.CALL:   return "CALL " + p[pc+1] + " " + p[pc+2];
    case Opcode.TCALL:  return "TCALL " + p[pc+1] + " " + p[pc+2] + " " + p[pc+3];
    case Opcode.RET:    return "RET " + p[pc+1];
    case Opcode.PRINTI: return "PRINTI";
    case Opcode.PRINTC: return "PRINTC";
    case Opcode.LDARGS: return "LDARGS";
    case Opcode.READ:   return "READ";
    case Opcode.STOP:   return "STOP";
    default:     return "<unknown>";
    }
  }

  // Print current stack and current instruction

  static void PrintSpPc(int[] s, int bp, int sp, int[] p, int pc) {
    Console.Write("[ ");
    for (int i=0; i<=sp; i++)
      Console.Write(s[i] + " ");
    Console.Write("]");
    Console.WriteLine("{" + pc + ": " + InsName(p, pc) + "}"); 
  }

  // Read instructions from a file

  public static int[] Readfile(String filename) {
    List<int> rawprogram = new List<int>();
    rawprogram.AddRange(MakeInputReader(filename));
    return rawprogram.ToArray();
  }
}
