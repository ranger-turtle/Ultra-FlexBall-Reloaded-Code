using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LevelSetListManager : MonoBehaviour
{
	[SerializeField]
#pragma warning disable CS0649 // Field 'LevelSetListManager.levelSetListRowPrefab' is never assigned to, and will always have its default value null
	private GameObject levelSetListRowPrefab;
#pragma warning restore CS0649 // Field 'LevelSetListManager.levelSetListRowPrefab' is never assigned to, and will always have its default value null
	[SerializeField]
#pragma warning disable CS0649 // Field 'LevelSetListManager.cursorTexture' is never assigned to, and will always have its default value null
	private Texture2D cursorTexture;
#pragma warning restore CS0649 // Field 'LevelSetListManager.cursorTexture' is never assigned to, and will always have its default value null
	[SerializeField]
#pragma warning disable CS0649 // Field 'LevelSetListManager.finishedMarkSprite' is never assigned to, and will always have its default value null
	private Sprite finishedMarkSprite;
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

	private void Start()
    {
		string[] levelSetFileNames = Directory.GetFiles("Level Sets", "*.nlev", SearchOption.TopDirectoryOnly)
			.Select(lsn => Path.GetFileNameWithoutExtension(lsn)).ToArray();
		IEnumerable<LevelSetData.LevelSet> levelSets = levelSetFileNames.Select(lsn => FileImporter.LoadLevelSet(lsn));
		LevelSetTemporaryData[] levelSetTemporaryData = levelSets.Select(ls => new LevelSetTemporaryData
		{
			levelSetName = ls.LevelSetProperties.Name,
			totalLevelNumber = ls.Levels.Count,
		}).ToArray();
		for (int i = 0; i < levelSetFileNames.Length; i++)
		{
			GameObject levelSetListRow = Instantiate(levelSetListRowPrefab, gameObject.transform);
			GameObject levelSetNameCol = levelSetListRow.transform.Find("LevelSetNameCol").gameObject;
			levelSetNameCol.GetComponent<Text>().text = levelSetTemporaryData[i].levelSetName;
			levelSetNameCol.GetComponent<LevelSetNameButton>().LevelSetName = levelSetTemporaryData[i].levelSetName;
			levelSetNameCol.GetComponent<LevelSetNameButton>().LevelSetFileName = levelSetFileNames[i];
			levelSetListRow.transform.Find("LastLevelCol").GetComponent<Text>().text = (FileImporter.LoadLevelPersistentData(levelSetFileNames[i])?.LevelNum + 1 ?? 1).ToString();
			levelSetListRow.transform.Find("TotalLevelsCol").GetComponent<Text>().text = levelSetTemporaryData[i].totalLevelNumber.ToString();
			LevelPersistentData levelPersistentData = FileImporter.LoadLevelPersistentData(levelSetFileNames[i]);
			if (File.Exists(Path.Combine("High scores", $"{levelSetFileNames[i]}.sco")))
				levelSetListRow.transform.Find("FinishedCol/Image").GetComponent<Image>().sprite = finishedMarkSprite;
		}
    }
}
