using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using ScriptTutorial;

namespace ScriptCompiler
{
	static class ScriptCompiler
	{
		public static void CompileScript(string inputpath, string outputpath)
		{
			using (var fs = new StreamReader(inputpath))
			{
				var input = new AntlrInputStream(fs);

				var lexer = new ScriptTutorialLexer(input);
				var tokenStream = new CommonTokenStream(lexer);
				var parser = new ScriptTutorialParser(tokenStream);

				var builder = new ScriptBuilder();
				var walker = new ScriptTreeWalker(builder, parser);
				
				builder.OutputScriptFile(outputpath, true);
			}
		}
	}
}
