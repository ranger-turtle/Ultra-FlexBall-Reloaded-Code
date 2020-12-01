using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class Logger
{
	public static void SaveLevelSetErrorLog(string levelSetDirectory, string levelSetFileName, List<string> errors)
	{
		SaveLog(errors, $"{levelSetDirectory}", $"{levelSetFileName}/levelSet_errors.errorlog");
	}

	public static void SaveLevelErrorLog(string levelSetDirectory, string levelSetFileName, int levelNum, string levelName, List<string> errors)
	{
		SaveLog(errors, $"{levelSetDirectory}/{levelSetFileName}", $"level{levelNum} ({levelName}).errorlog");
	}

	public static void SaveGameErrorLog(string directory, List<string> errors)
	{
		SaveLog(errors, $"{directory}", $"Level set choose errors.errorlog");
	}

	private static void SaveLog(List<string> errors, string levelSetDirectory, string logFileName)
	{
		Directory.CreateDirectory(levelSetDirectory);
		using (FileStream fileStream = File.Create(Path.Combine(levelSetDirectory, logFileName)))
		using (StreamWriter logWriter = new StreamWriter(fileStream))
			foreach (string error in errors)
				logWriter.WriteLine(error);
	}
}