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
		MusicManager.Instance.SwitchToTitle();
		EndGameData.HighScoreChange = false;

		List<string> correctLevelSetFileNames = new List<string>();
		LevelSetTemporaryData[] levelSetTemporaryData = LoadLevelSetTemporaryData(correctLevelSetFileNames);
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
			levelSetNameButton.FirstLevel = levelIndex > 0 ? false : true;
			levelSetListRow.transform.Find("LastLevelCol").GetComponent<Text>().text = $"{levelIndex + 1}";
			levelSetListRow.transform.Find("TotalLevelsCol").GetComponent<Text>().text = levelSetTemporaryData[i].totalLevelNumber.ToString();
			if (levelSetCompleted)
				levelSetListRow.transform.Find("FinishedCol/Image").GetComponent<Image>().sprite = finishedMarkSprite;
		}
	}

	private LevelSetTemporaryData[] LoadLevelSetTemporaryData(List<string> correctLevelSetFileNames)
	{
		List<LevelSetTemporaryData> levelSetTemporaryData = new List<LevelSetTemporaryData>();
		List<string> levelSetFileNames = Directory.GetFiles("Level sets", "*.nlev", SearchOption.TopDirectoryOnly)
			.Select(lsn => Path.GetFileNameWithoutExtension(lsn)).ToList();
		List<string> corruptLevelSetList = new List<string>();
		foreach (string lsn in levelSetFileNames)
		{
			try
			{
				LevelSetTemporaryData levelSet = LoadLevelSetNumber($"Level sets/{lsn}.nlev");
				correctLevelSetFileNames.Add(lsn);
				levelSetTemporaryData.Add(levelSet);
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
		return levelSetTemporaryData.ToArray();
	}

	private LevelSetTemporaryData LoadLevelSetNumber(string levelSetFilePath)
	{
		using (FileStream fileStream = File.OpenRead(levelSetFilePath))
		{
			using (BinaryReader levelSetReader = new BinaryReader(fileStream))
			{
				string fileSignature = levelSetReader.ReadString();
				if (fileSignature == "nuLev")
				{
					string levelSetName = levelSetReader.ReadString();
					levelSetReader.ReadString();
					levelSetReader.ReadString();
					//BONUS write internal function after upgrade to next C# version
					int customSoundInLevelSetSoundLibraryCount = levelSetReader.ReadInt32();
					for (int i = 0; i < customSoundInLevelSetSoundLibraryCount; i++)
					{
						levelSetReader.ReadString();
						levelSetReader.ReadString();
					}
					int levelNumber = levelSetReader.ReadInt32();
					return new LevelSetTemporaryData()
					{
						levelSetName = levelSetName,
						totalLevelNumber = levelNumber
					};
				}
				else
					throw new IOException("Invalid Ultra FlexBall Reloaded level set file loaded.");
			}

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
