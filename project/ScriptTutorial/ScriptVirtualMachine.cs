using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScriptCompiler;

namespace ScriptTutorial
{
	class ScriptVirtualMachine
	{
		private List<int> scriptData = new List<int>();
		private List<int> labelLocations = new List<int>();
		private List<string> stringData = new List<string>();

		private int[] r = new int[2];
		private int position;
		private Stack<int> stack = new Stack<int>();
		private int[] variables;

		public ScriptVirtualMachine(string path)
		{
			using (var fs = new FileStream(path, FileMode.Open))
			using (var br = new BinaryReader(fs))
			{
				var sig = br.ReadChars(4);
				if (new string(sig) != "SCPT")
					throw new Exception("File is not a valid script file!");

				var datalen = br.ReadInt32();
				for (var i = 0; i < datalen * 2; i++)
					scriptData.Add(br.ReadInt32());

				var stringlen = br.ReadInt32();
				for (var i = 0; i < stringlen; i++)
					stringData.Add(br.ReadString());

				var labellen = br.ReadInt32();
				for (var i = 0; i < labellen; i++)
					labelLocations.Add(br.ReadInt32());

				var varcount = br.ReadInt32();
				variables = new int[varcount];
			}
		}

		private void Advance()
		{
			var op = (Operation)scriptData[position * 2];
			var val = scriptData[position * 2 + 1];
			position++;

			switch (op)
			{
				case Operation.OpVal:
					r[0] = val;
					break;
				case Operation.OpPush:
					stack.Push(r[val]);
					break;
				case Operation.OpPop:
					r[val] = stack.Pop();
					break;
				case Operation.OpJumpIf:
					if (r[0] == 0)
						position = labelLocations[val];
					break;
				case Operation.OpJumpNotIf:
					if (r[0] != 0)
						position = labelLocations[val];
					break;
				case Operation.OpJump:
					position = labelLocations[val];
					break;
				case Operation.OpFunc:
					r[0] = RemoteFunctionCall(val);
					break;
				case Operation.OpAssign:
					variables[val] = r[0];
					break;
				case Operation.OpGetVar:
					r[0] = variables[val];
					break;
				case Operation.OpAdd:
					r[0] = r[1] + r[0];
					break;
				case Operation.OpSub:
					r[0] = r[1] - r[0];
					break;
				case Operation.OpMul:
					r[0] = r[1] * r[0];
					break;
				case Operation.OpDiv:
					r[0] = r[1] / r[0];
					break;
				case Operation.OpAnd:
					r[0] = (r[1] == 0 && r[0] == 0) ? 0 : 1;
					break;
				case Operation.OpOr:
					r[0] = (r[1] == 0 || r[1] == 0) ? 0 : 1;
					break;
				case Operation.OpInc:
					variables[val]++;
					break;
				case Operation.OpDec:
					variables[val]--;
					break;
				case Operation.OpEquals:
					r[0] = (r[1] == r[0]) ? 0 : 1;
					break;
				case Operation.OpNotEquals:
					r[0] = (r[1] != r[0]) ? 0 : 1;
					break;
				case Operation.OpGreaterThan:
					r[0] = (r[1] > r[0]) ? 0 : 1;
					break;
				case Operation.OpLessThan:
					r[0] = (r[1] < r[0]) ? 0 : 1;
					break;
				case Operation.OpGreaterOrEquals:
					r[0] = (r[1] >= r[0]) ? 0 : 1;
					break;
				case Operation.OpLessThanOrEquals:
					r[0] = (r[1] <= r[0]) ? 0 : 1;
					break;
				default:
					throw new Exception($"Unknown operation {op}!");
			}
		}

		private int RemoteFunctionCall(int id)
		{
			var function = (RemoteFunction)id;
			var parameters = new int[r[0]];
			for (var i = parameters.Length - 1; i >= 0; i--)
				parameters[i] = stack.Pop();

			if (function == RemoteFunction.OutputText)
			{
				var text = stringData[parameters[0]];
				Console.WriteLine(text);
				return 0;
			}

			if (function == RemoteFunction.OutputValue)
			{
				Console.WriteLine(parameters[0]);
				return 0;
			}

			throw new Exception($"Unhandled function id {id}.");
		}

		public void Execute()
		{
			while (position < scriptData.Count / 2)
				Advance();
		}
	}
}
