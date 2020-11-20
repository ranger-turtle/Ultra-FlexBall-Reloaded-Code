using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LevelSetLoader : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
		string levelSetDirectory = LoadedGameData.LevelSetDirectory;
		string levelSetFileName = LoadedGameData.LevelSetFileName;
		LevelSetData.LevelSet levelSet;
		levelSet = FileImporter.LoadLevelSet(levelSetDirectory, levelSetFileName);
		BrickType[] DefaultBrickTypes = LoadedGameData.DefaultBrickTypes ?? FileImporter.LoadBricks();
		BrickType[] CustomBrickTypes = new BrickType[0];
		//if loaded level set has external files
		if (FileImporter.LevelSetExternalFileDirectoryExists(levelSetDirectory, levelSetFileName))
		{
			HashSet<string> missingFileNames = new HashSet<string>();
			SoundManager.Instance.LoadSounds(levelSetFileName, LoadedGameData.TestMode, levelSetDirectory);
			string levelSetCustomBricksPath = FileImporter.GetDirectoryNameInLevelSetDirectory(levelSetDirectory, levelSetFileName, "Bricks");
			if (Directory.Exists(levelSetCustomBricksPath))
				CustomBrickTypes = FileImporter.LoadBricks(missingFileNames, levelSetCustomBricksPath);
		}
		if (LoadedGameData.TestMode == TestMode.None)
		{
			LevelPersistentData levelPersistentData = null;
			if (LoadedGameData.Continue)
				levelPersistentData = FileImporter.LoadLevelPersistentData(levelSetFileName);
			GameManager.Instance.InitLevel(levelSet, DefaultBrickTypes, CustomBrickTypes, levelPersistentData);
		}
		else
		{
			GameManager.Instance.InitLevel(levelSet, DefaultBrickTypes, CustomBrickTypes, LoadedGameData.TestLevelNum);
		}
	}
}
