using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class LevelSetNameButton : MonoBehaviour, IPointerClickHandler
{
	private ShutterAnimationManager coverAnimator;

	internal string LevelSetName { get; set; }
	internal string LevelSetFileName { get; set; }
	internal bool FirstLevel { get; set; }

	private void Awake()
	{
		coverAnimator = GameObject.Find("Canvas/CoverMask").GetComponent<ShutterAnimationManager>();
	}

	public void OnPointerClick(PointerEventData ped)
	{
		LoadedGameData.LevelSetFileName = LevelSetFileName;
		if (ped.button == PointerEventData.InputButton.Right)
		{
			coverAnimator.Cover(WaitForSceneLoad("Leaderboard"));
		}
		else
		{
			LoadedGameData.Continue = Input.GetKey(KeyCode.LeftControl) || FirstLevel ? false : true;
			CheckSave();
			if (LoadedGameData.Continue || !ContainsIntro())
				coverAnimator.Cover(WaitForSceneLoad("Level"));
			else if (ContainsIntro())
			{
				CutsceneData.CutsceneName = "Intro";
				CutsceneData.NextScene = "Level";
				coverAnimator.Cover(WaitForSceneLoad("Cutscene"));
			}
		}
	}

	private bool ContainsIntro() => Directory.Exists(Path.Combine($"{LoadedGameData.LevelSetDirectory}", $"{LoadedGameData.LevelSetFileName}", "Intro"));

	private void CheckSave()
	{
		string savePath = $"Saves/{LevelSetFileName}.sav";
		if (!LoadedGameData.Continue && File.Exists(savePath))
			File.Delete(savePath);
	}

	private IEnumerator WaitForSceneLoad(string sceneName)
	{
		yield return new WaitUntil(() => coverAnimator.Covered);
		SceneManager.LoadScene(sceneName);
	}
}
