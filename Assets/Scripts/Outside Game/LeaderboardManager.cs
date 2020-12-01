using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LeaderboardPosition
{
	public string Nick { get; set; }
	public int LastLevel { get; set; }
	public int Score { get; set; }
	public bool PlayerWon { get; set; }
}

public class LeaderboardManager : MonoBehaviour
{
	public const string WonPlaceholder = "Won!";
	private const string highScoreMagicNumber = "hisco";
	private const int maxPositions = 30;
#pragma warning disable CS0649 // Field 'LeaderboardManager.NickFieldObject' is never assigned to, and will always have its default value null
	[SerializeField]
	private GameObject nickInsertGUIObject;
	[SerializeField]
	private InputField nickField;
	[SerializeField]
	private GameObject leaderboardSection;
	[SerializeField]
	private GameObject leaderboardObject;
	[SerializeField]
	private GameObject leaderboardRowPrefab;
	[SerializeField]
	private ShutterAnimationManager coverAnimator;
#pragma warning restore CS0649 // Field 'LeaderboardManager.NickFieldObject' is never assigned to, and will always have its default value null

	private List<LeaderboardPosition> leaderboardPositions;
	private int currentIndex = -1;

	private bool LeaderboardSectionActive => leaderboardSection.activeSelf;

    // Start is called before the first frame update
    void Start()
    {
		//if (Application.isEditor)
		//LoadedGameData.DefaultBrickTypes = FileImporter.LoadBricks(null);
		leaderboardPositions = LoadHighScores(LoadedGameData.LevelSetFileName);
		if (EndGameData.HighScoreChange)
			currentIndex = ProcessLeaderboard(leaderboardPositions);
		if (currentIndex >= 0)
			ActivateNickField();
		else
			DisplayLeaderboard(false);
    }

	public void ActivateNickField()
    {
		leaderboardSection.SetActive(false);
		nickInsertGUIObject.SetActive(true);
		nickField.ActivateInputField();
		nickField.Select();
    }

	private void DisplayLeaderboard(bool save)
	{
		nickInsertGUIObject.SetActive(false);
		leaderboardSection.SetActive(true);
		PopulateLeaderboard(leaderboardPositions);
		if (save)
			SaveHighScores(leaderboardPositions, LoadedGameData.LevelSetFileName, EndGameData.Won);
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Return) && !LeaderboardSectionActive)
		{
			leaderboardPositions[currentIndex].Nick = nickField.text;
			DisplayLeaderboard(true);
		}
		else if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return)) && LeaderboardSectionActive)
			coverAnimator.Cover(WaitForLevelListMenuLoad());
	}

	/// <summary>
	/// Processes leader board and returns new position
	/// </summary>
	/// <param name="leaderboardPositions"></param>
	/// <returns></returns>
	private int ProcessLeaderboard(List<LeaderboardPosition> leaderboardPositions)
	{
		LeaderboardPosition newLeaderboardPosition = new LeaderboardPosition() { Nick = string.Empty, LastLevel = EndGameData.LastLevel, Score = EndGameData.Score, PlayerWon = EndGameData.Won };
		int index = leaderboardPositions.FindIndex(
			lp => lp.Score <= EndGameData.Score && (lp.PlayerWon || lp.LastLevel <= EndGameData.LastLevel)
		);
		if (index >= 0)
		{
			leaderboardPositions.Insert(index, newLeaderboardPosition);
			if (leaderboardPositions.Count > maxPositions)
				leaderboardPositions.RemoveAt(maxPositions);
			return index;
		}
		else if (leaderboardPositions.Count < maxPositions)
		{
			leaderboardPositions.Add(newLeaderboardPosition);
			return leaderboardPositions.Count - 1;
		}
		else
			return -1;
	}

	private void PopulateLeaderboard(List<LeaderboardPosition> leaderboardPositions)
	{
		for (int i = 0; i < leaderboardPositions.Count; i++)
		{
			GameObject levelSetListRow = Instantiate(leaderboardRowPrefab, leaderboardObject.transform);
			levelSetListRow.transform.Find("RankCol").GetComponent<Text>().text = $"{i + 1}";
			levelSetListRow.transform.Find("NickCol").GetComponent<Text>().text = leaderboardPositions[i].Nick;
			levelSetListRow.transform.Find("LastLevelCol").GetComponent<Text>().text = leaderboardPositions[i].PlayerWon ? WonPlaceholder : $"{leaderboardPositions[i].LastLevel + 1}";
			levelSetListRow.transform.Find("ScoreCol").GetComponent<Text>().text = leaderboardPositions[i].Score.ToString();
		}
	}

	private IEnumerator WaitForLevelListMenuLoad()
	{
		yield return new WaitUntil(() => coverAnimator.Covered);
		SceneManager.LoadScene("Level Set List");
	}

	#region file handling
	private List<LeaderboardPosition> LoadHighScores(string levelSetFileName)
	{
		if (File.Exists(Path.Combine("High scores", $"{levelSetFileName}.sco")))
			using (FileStream fileStream = File.OpenRead(Path.Combine("High scores", $"{levelSetFileName}.sco")))
			{
				using (BinaryReader highScoreReader = new BinaryReader(fileStream))
				{
					string magicNumber = highScoreReader.ReadString();
					if (magicNumber != highScoreMagicNumber)
						return null;

					highScoreReader.ReadBoolean();//general win information

					List<LeaderboardPosition> leaderboardPositions = new List<LeaderboardPosition>();
					int leaderboardPositionCount = highScoreReader.ReadInt32();
					for (int i = 0; i < leaderboardPositionCount; i++)
					{
						LeaderboardPosition leaderboardPosition = new LeaderboardPosition()
						{
							Nick = highScoreReader.ReadString(),
							Score = highScoreReader.ReadInt32(),
							LastLevel = highScoreReader.ReadInt32(),
							PlayerWon = highScoreReader.ReadBoolean()
						};
						leaderboardPositions.Add(leaderboardPosition);
					}
					return leaderboardPositions;
				}
			}
		else
			return new List<LeaderboardPosition>();
	}

	private void SaveHighScores(List<LeaderboardPosition> leaderboardPositions, string levelSetFileName, bool won)
	{
		using (FileStream fileStream = File.Create(Path.Combine("High scores", $"{levelSetFileName}.sco")))
		{
			using (BinaryWriter highScoreWriter = new BinaryWriter(fileStream))
			{
				highScoreWriter.Write(highScoreMagicNumber);
				highScoreWriter.Write(won);//write general information about win

				highScoreWriter.Write(leaderboardPositions.Count);
				foreach (LeaderboardPosition leaderboardPosition in leaderboardPositions)
				{
					highScoreWriter.Write(leaderboardPosition.Nick);
					highScoreWriter.Write(leaderboardPosition.Score);
					highScoreWriter.Write(leaderboardPosition.LastLevel);
					highScoreWriter.Write(leaderboardPosition.PlayerWon);
				}
			}
		}
	}

	public static bool GetGeneralWinInfo(string levelSetFileName)
	{
		if (File.Exists(Path.Combine("High scores", $"{levelSetFileName}.sco")))
			using (FileStream fileStream = File.OpenRead(Path.Combine("High scores", $"{levelSetFileName}.sco")))
			{
				using (BinaryReader highScoreReader = new BinaryReader(fileStream))
				{
					string magicNumber = highScoreReader.ReadString();
					if (magicNumber != highScoreMagicNumber)
						return false;

					return highScoreReader.ReadBoolean();//general win information
				}
			}
		else
			return false;
	}
	#endregion
}
