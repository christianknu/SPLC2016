// sestoft@itu.dk 2007-03-12, 2010-02-08
// Modified by mogel@itu.dk Oct 2011

using System;
using System.Collections.Generic;
using System.IO;

namespace Expressions {
  class MainProgram {
    static void Main(string[] args) {
      if (args.Length >= 1) {
        Scanner scanner = new Scanner(args[0]);
        Parser parser = new Parser(scanner);
        parser.Parse();
        if (args.Length >= 2 && args[1]=="run") 
			Console.WriteLine("result is {0}", parser.res);
	  } else 
      Console.WriteLine("Usage: Expressions <expression.txt>");
    }
  }
}
