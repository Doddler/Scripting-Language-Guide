using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCompiler
{
	class ScriptBuilder
	{
		private List<string> variableNames = new List<string>();
		private List<int> labelLocations = new List<int>();
		private List<int> scriptData = new List<int>();
		private List<string> stringList = new List<string>();
		private List<string> debugOutput = new List<string>();

		public void OutputOperation(Operation op, int value=0)
		{
			scriptData.Add((int)op);
			scriptData.Add(value);

			debugOutput.Add($"{op.ToString().Substring(2)} {value}");
		}

		private int GetScriptPosition()
		{
			return scriptData.Count / 2;
		}

		public void DeclareVariable(string name)
		{
			if(variableNames.Contains(name))
				throw new Exception($"Variable of name {name} is already defined.");
			debugOutput.Add($"//var {name} is id {variableNames.Count}");
			variableNames.Add(name);
		}

		public int GetVariableId(string name)
		{
			if(!variableNames.Contains(name))
				throw new Exception($"Variable of name {name} not declared before use.");
			return variableNames.IndexOf(name);
		}

		public int CreateLabel()
		{
			var id = labelLocations.Count;
			labelLocations.Add(0);
			return id;
		}

		public void SetLabelPosition(int id)
		{
			labelLocations[id] = GetScriptPosition();
			debugOutput.Add($":Label{id}");
		}

		public int CreateLabelAndSet()
		{
			var id = CreateLabel();
			SetLabelPosition(id);
			return id;
		}

		public int AddStringReference(string text)
		{
			if (stringList.Contains(text))
				return stringList.IndexOf(text);

			var id = stringList.Count;
			stringList.Add(text);
			return id;
		}

		public void OutputScriptFile(string path, bool outputDebugFile)
		{
			if (outputDebugFile)
			{
				var outputpath = Path.GetDirectoryName(path);
				var filename = Path.GetFileNameWithoutExtension(path);
				var debugPath = Path.Combine(outputpath, filename + ".txt");

				File.WriteAllLines(debugPath, debugOutput);
			}

			using (var fs = new FileStream(path, FileMode.Create))
			using (var bw = new BinaryWriter(fs))
			{
				bw.Write("SCPT".ToCharArray()); //file signature

				bw.Write(scriptData.Count/2); //number of operations
				foreach(var i in scriptData)
					bw.Write(i);    //write script data

				bw.Write(stringList.Count); //number of strings
				foreach (var s in stringList)
					bw.Write(s); //write string

				bw.Write(labelLocations.Count); //number of labels
				foreach(var l in labelLocations)
					bw.Write(l); //write label location

				bw.Write(variableNames.Count); //number of variables

				bw.Flush();
				fs.Close();
			}
		}
	}
}
