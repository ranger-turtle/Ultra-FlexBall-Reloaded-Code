using System.IO;
using UnityEngine;

public class LevelSetLoader : MonoBehaviour
{
	[SerializeField]
	private string levelSetFileName = "wert";

    // Start is called before the first frame update
    void Start()
    {
		levelSetFileName = LoadedGameData.LevelSetFileName;
		BrickType[] DefaultBrickTypes = LoadedGameData.DefaultBrickTypes ?? FileImporter.LoadBricks();
		string levelSetCustomBricksPath = FileImporter.GetElementInLevelSetDirectory(levelSetFileName, "Bricks");
		BrickType[] CustomBrickTypes = Directory.Exists(levelSetCustomBricksPath) ? FileImporter.LoadBricks(levelSetCustomBricksPath) : (new BrickType[0]);
		LevelSetData.LevelSet levelSet;
		if (LoadedGameData.TestMode == TestMode.None)
		{
			levelSet = FileImporter.LoadLevelSet(levelSetFileName);
			LevelPersistentData levelPersistentData = null;
			if (LoadedGameData.Continue)
				levelPersistentData = FileImporter.LoadLevelPersistentData(levelSetFileName);
			GameManager.Instance.InitLevel(levelSet, DefaultBrickTypes, CustomBrickTypes, levelPersistentData);
		}
		else
		{
			levelSet = FileImporter.LoadLevelSetWithFullPath(levelSetFileName);
			GameManager.Instance.InitLevel(levelSet, DefaultBrickTypes, CustomBrickTypes, LoadedGameData.TestLevelNum);
		}
	}
}
