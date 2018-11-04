using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCompiler
{
	class Program
	{
		static void Main(string[] args)
		{
			ScriptCompiler.CompileScript("SampleScript.txt", "Output.dat");
			Console.WriteLine("Done! Press any key to close.");
			Console.ReadKey();
		}
	}
}
