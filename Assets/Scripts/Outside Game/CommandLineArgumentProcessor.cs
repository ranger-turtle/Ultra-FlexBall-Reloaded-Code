using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public class CommandLineArgumentProcessor : MonoBehaviour
{
	[SerializeField]
	private string[] testCommandLineArguments;

	void Start()
	{
        if (!Application.isEditor)
		{
			testCommandLineArguments = Environment.GetCommandLineArgs();
		}
		if (testCommandLineArguments.Length > 1)
		{
			string levSetArgument = testCommandLineArguments[1];
			Regex firstArgRegex = new Regex(@"^-s:.*(\.nlev)$");
			Match firstArgMatch = firstArgRegex.Match(levSetArgument);
			if (firstArgMatch.Success)
			{
				levSetArgument = levSetArgument.Substring(3);
				LoadedGameData.LevelSetDirectory = Path.GetDirectoryName(levSetArgument);
				LoadedGameData.LevelSetFileName = Path.GetFileNameWithoutExtension(levSetArgument);
				LoadedGameData.TestMode = TestMode.TestLevelSet;
			}
			if (testCommandLineArguments.Length > 2)
			{
				string levNumArgument = testCommandLineArguments[2];
				Regex secondArgRegex = new Regex(@"^-l:\d+");
				Match secondArgMatch = secondArgRegex.Match(levNumArgument);
				if (secondArgMatch.Success && firstArgMatch.Success)
				{
					LoadedGameData.TestLevelNum = int.Parse(levNumArgument.Substring(3));
					LoadedGameData.TestMode = TestMode.TestOneLevel;
				}
			}
		}
	}
}
