using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptTutorial
{
	class Program
	{
		static void Main(string[] args)
		{
			var vm = new ScriptVirtualMachine(@"..\..\..\ScriptCompiler\bin\Debug\Output.dat");
			vm.Execute();

			Console.WriteLine("Done executing!");
			Console.ReadKey();
		}
	}
}
