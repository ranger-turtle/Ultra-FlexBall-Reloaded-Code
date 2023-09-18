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
		//if loaded level set has external files
		if (FileImporter.LevelSetResourceDirectoryExists(levelSetDirectory, levelSetFileName))
		{
			SoundManager.Instance.LoadSounds(levelSetFileName, LoadedGameData.TestMode, levelSetDirectory);
			TextureManager.Instance.LoadLevelSetTextures(levelSetDirectory, levelSetFileName);
			MusicManager.Instance.LoadLevelSetTextures(levelSetDirectory, levelSetFileName, LoadedGameData.TestMode);
		}
		BrickManager.Instance.ImportBricks();
		if (LoadedGameData.TestMode == TestMode.None)
		{
			LevelPersistentData levelPersistentData;
			System.DateTime levelSetDateTime = File.GetLastWriteTime(Path.Combine(levelSetDirectory, $"{levelSetFileName}.nlev"));
			levelPersistentData = new LevelPersistentData(levelSetFileName, levelSetDateTime);
			if (LoadedGameData.Continue)
				levelPersistentData.Load();
			GameManager.Instance.InitLevelSet(levelSet, levelPersistentData);
		}
		else
		{
			GameManager.Instance.InitLevelSet(levelSet, LoadedGameData.TestLevelNum);
		}
	}
}
