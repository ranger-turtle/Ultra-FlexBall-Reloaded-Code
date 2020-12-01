using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSetListManager : MonoBehaviour
{
	[SerializeField]
#pragma warning disable CS0649 // Field 'LevelSetListManager.levelSetListRowPrefab' is never assigned to, and will always have its default value null
	private GameObject levelSetListRowPrefab;
	[SerializeField]
	private Texture2D cursorTexture;
	[SerializeField]
	private Sprite finishedMarkSprite;
	[SerializeField]
	private ErrorMessage errorMessage;
	[SerializeField]
	private ShutterAnimationManager shutterAnimationManager;
#pragma warning restore CS0649 // Field 'LevelSetListManager.finishedMarkSprite' is never assigned to, and will always have its default value null

	private struct LevelSetTemporaryData
	{
		internal string levelSetName;
		internal int totalLevelNumber;
	}

	private void Awake()
	{
		Cursor.visible = true;
		Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.Auto);
	}

	//TODO [FlexEd] Optimize file loading setting number of levels to beginning
	private void Start()
	{
		EndGameData.HighScoreChange = false;

		List<LevelSetData.LevelSet> levelSets = new List<LevelSetData.LevelSet>();
		List<string> correctLevelSetFileNames = new List<string>();
		LoadLevelSets(levelSets, correctLevelSetFileNames);
		LevelSetTemporaryData[] levelSetTemporaryData = levelSets.Select(ls => new LevelSetTemporaryData
		{
			levelSetName = ls.LevelSetProperties.Name,
			totalLevelNumber = ls.Levels.Count,
		}).ToArray();
		for (int i = 0; i < levelSetTemporaryData.Length; i++)
		{
			GameObject levelSetListRow = Instantiate(levelSetListRowPrefab, gameObject.transform);
			GameObject levelSetNameCol = levelSetListRow.transform.Find("LevelSetNameCol").gameObject;
			LevelSetNameButton levelSetNameButton = levelSetNameCol.GetComponent<LevelSetNameButton>();
			DateTime levelSetModificationTime = File.GetLastWriteTime($"Level sets/{correctLevelSetFileNames[i]}.nlev");
			bool levelSetCompleted = LeaderboardManager.GetGeneralWinInfo(correctLevelSetFileNames[i]);
			int levelIndex = LevelPersistentData.ReadLastLevelNum(correctLevelSetFileNames[i], levelSetModificationTime);
			if (levelIndex < 0)
			{
				File.Delete($"Saves/{correctLevelSetFileNames[i]}.sav");
				levelIndex = 0;
			}
			levelSetNameCol.GetComponent<Text>().text = levelSetTemporaryData[i].levelSetName;
			levelSetNameButton.LevelSetName = levelSetTemporaryData[i].levelSetName;
			levelSetNameButton.LevelSetFileName = correctLevelSetFileNames[i];
			levelSetNameButton.FirstLevel = levelIndex > 0 && !levelSetCompleted ? false : true;
			levelSetListRow.transform.Find("LastLevelCol").GetComponent<Text>().text = $"{levelIndex + 1}";
			levelSetListRow.transform.Find("TotalLevelsCol").GetComponent<Text>().text = levelSetTemporaryData[i].totalLevelNumber.ToString();
			if (levelSetCompleted)
				levelSetListRow.transform.Find("FinishedCol/Image").GetComponent<Image>().sprite = finishedMarkSprite;
		}
	}

	private void LoadLevelSets(List<LevelSetData.LevelSet> levelSets, List<string> correctLevelSetFileNames)
	{
		List<string> levelSetFileNames = Directory.GetFiles("Level sets", "*.nlev", SearchOption.TopDirectoryOnly)
			.Select(lsn => Path.GetFileNameWithoutExtension(lsn)).ToList();
		List<string> corruptLevelSetList = new List<string>();
		foreach (string lsn in levelSetFileNames)
		{
			try
			{
				LevelSetData.LevelSet levelSet = FileImporter.LoadLevelSet("Level sets", lsn);
				correctLevelSetFileNames.Add(lsn);
				levelSets.Add(levelSet);
			}
			catch (IOException)
			{
				corruptLevelSetList.Add($"Corrupt level set: {lsn}");
			}
		}
		if (corruptLevelSetList.Count > 0)
		{
			Logger.SaveGameErrorLog("Level sets", corruptLevelSetList);
			errorMessage.Show("Some level sets are corrupt. See more details in log in level set folder.");
		}
	}

	private IEnumerator GoToSplash2()
	{
		yield return new WaitUntil(() => shutterAnimationManager.Covered);
		SceneManager.LoadScene("Splash2");
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
			shutterAnimationManager.Cover(GoToSplash2());
	}
}
