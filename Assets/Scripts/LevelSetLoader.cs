using System.Collections.Generic;
using System.IO;
using UnityEngine;

//BONUS make assets load in coroutines and add progress bar
public class LevelSetLoader : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
		Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
		string levelSetDirectory = LoadedGameData.LevelSetDirectory;
		string levelSetFileName = LoadedGameData.LevelSetFileName;
		LevelSetData.LevelSet levelSet;
		levelSet = FileImporter.LoadLevelSet(levelSetDirectory, levelSetFileName);
		List<string> errorList = new List<string>();
		BrickType[] DefaultBrickTypes = LoadedGameData.DefaultBrickTypes ?? FileImporter.LoadBricks(errorList);
		BrickType[] CustomBrickTypes = new BrickType[0];
		//if loaded level set has external files
		if (FileImporter.LevelSetResourceDirectoryExists(levelSetDirectory, levelSetFileName))
		{
			SoundManager.Instance.LoadSounds(levelSetFileName, LoadedGameData.TestMode, levelSetDirectory);
			TextureManager.Instance.LoadLevelSetTextures(levelSetDirectory, levelSetFileName);
			MusicManager.Instance.LoadLevelSetTextures(levelSetDirectory, levelSetFileName, LoadedGameData.TestMode);
			//TODO make brick manager to handle errors more elegantly
			string levelSetCustomBricksPath = FileImporter.GetDirectoryNameInLevelSetDirectory(levelSetDirectory, levelSetFileName, "Bricks");
			if (Directory.Exists(levelSetCustomBricksPath))
				CustomBrickTypes = FileImporter.LoadBricks(errorList, levelSetCustomBricksPath);
		}
		if (LoadedGameData.TestMode == TestMode.None)
		{
			LevelPersistentData levelPersistentData = null;
			System.DateTime levelSetDateTime = File.GetLastWriteTime(Path.Combine(levelSetDirectory, $"{levelSetFileName}.nlev"));
			levelPersistentData = new LevelPersistentData(levelSetFileName, levelSetDateTime);
			if (LoadedGameData.Continue)
				levelPersistentData.Load();
			GameManager.Instance.InitLevelSet(levelSet, DefaultBrickTypes, CustomBrickTypes, levelPersistentData);
		}
		else
		{
			GameManager.Instance.InitLevelSet(levelSet, DefaultBrickTypes, CustomBrickTypes, LoadedGameData.TestLevelNum);
		}
	}
}
