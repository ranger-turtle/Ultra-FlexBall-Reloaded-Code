using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LeaderboardPosition
{
	public string Nick { get; set; }
	public string LastLevel { get; set; }
	public int Score { get; set; }
}

public class LeaderboardManager : MonoBehaviour
{
	public const string WonPlaceholder = "Won!";

	public static bool addNewRecord;

	[SerializeField]
	private GameObject nickInsertGUIObject;
	[SerializeField]
#pragma warning disable CS0649 // Field 'LeaderboardManager.NickFieldObject' is never assigned to, and will always have its default value null
	private GameObject nickFieldObject;
#pragma warning restore CS0649 // Field 'LeaderboardManager.NickFieldObject' is never assigned to, and will always have its default value null
	[SerializeField]
	private GameObject scrollView;
	[SerializeField]
	private GameObject leaderboardObject;
	[SerializeField]
	private GameObject leaderboardRowPrefab;
	[SerializeField]
	private ShutterAnimationManager coverAnimator;

	private bool LeaderboardSectionActive => scrollView.activeSelf;

    // Start is called before the first frame update
    void Start()
    {
		if (!Application.isEditor)
			LoadedGameData.DefaultBrickTypes = FileImporter.LoadBricks();
		if (addNewRecord)
			ActivateNickField();
		else
		{
			nickInsertGUIObject.SetActive(false);
			scrollView.SetActive(true);
			List<LeaderboardPosition> leaderboardPositions = FileImporter.LoadHighScores(LoadedGameData.LevelSetFileName);
			PopulateLeaderboard(leaderboardPositions);
		}
    }

    public void ActivateNickField()
    {
		InputField nickField = nickFieldObject.GetComponent<InputField>();
		nickField.ActivateInputField();
		nickField.Select();
    }

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Return))
		{
			nickInsertGUIObject.SetActive(false);
			scrollView.SetActive(true);
			List<LeaderboardPosition> leaderboardPositions = FileImporter.LoadHighScores(LoadedGameData.LevelSetFileName);
			ProcessLeaderboard(leaderboardPositions);
			PopulateLeaderboard(leaderboardPositions);
		}
		else if (Input.GetKeyDown(KeyCode.Escape) || (Input.GetKeyDown(KeyCode.Return) && LeaderboardSectionActive))
		{
			coverAnimator.Cover(WaitForLevelListMenuLoad());
		}
	}

	private void ProcessLeaderboard(List<LeaderboardPosition> leaderboardPositions)
	{
		string nick = nickFieldObject.GetComponent<InputField>().text;
		LeaderboardPosition newLeaderboardPosition = new LeaderboardPosition() { Nick = nick, LastLevel = EndGameData.LastLevel, Score = EndGameData.Score };
		int index = leaderboardPositions.FindIndex(
			lp => lp.Score <= EndGameData.Score && (lp.LastLevel == WonPlaceholder || int.Parse(lp.LastLevel) <= int.Parse(EndGameData.LastLevel))
		);
		if (index >= 0)
			leaderboardPositions.Insert(index, newLeaderboardPosition);
		else
			leaderboardPositions.Add(newLeaderboardPosition);
		using (FileStream fs = File.Open(Path.Combine("High scores", $"{LoadedGameData.LevelSetFileName}.sco"), FileMode.Create))
			new BinaryFormatter().Serialize(fs, leaderboardPositions);
	}

	private void PopulateLeaderboard(List<LeaderboardPosition> leaderboardPositions)
	{
		for (int i = 0; i < leaderboardPositions.Count; i++)
		{
			GameObject levelSetListRow = Instantiate(leaderboardRowPrefab, leaderboardObject.transform);
			levelSetListRow.transform.Find("RankCol").GetComponent<Text>().text = $"{i + 1}";
			levelSetListRow.transform.Find("NickCol").GetComponent<Text>().text = leaderboardPositions[i].Nick;
			levelSetListRow.transform.Find("LastLevelCol").GetComponent<Text>().text = leaderboardPositions[i].LastLevel;
			levelSetListRow.transform.Find("ScoreCol").GetComponent<Text>().text = leaderboardPositions[i].Score.ToString();
		}
	}

	private IEnumerator WaitForLevelListMenuLoad()
	{
		yield return new WaitUntil(() => coverAnimator.Covered);
		SceneManager.LoadScene("Level Set List");
	}
}
