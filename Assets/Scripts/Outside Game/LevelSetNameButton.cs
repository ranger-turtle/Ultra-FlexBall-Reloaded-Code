using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelSetNameButton : MonoBehaviour
{
	private ShutterAnimationManager coverAnimator;

	internal string LevelSetName { get; set; }
	internal string LevelSetFileName { get; set; }

	private void Awake()
	{
		coverAnimator = GameObject.Find("Canvas/CoverMask").GetComponent<ShutterAnimationManager>();
	}

	public void DoAction()
	{
#pragma warning disable CS0642 // Possible mistaken empty statement
		if (Input.GetMouseButtonDown(1))
		{
			coverAnimator.Cover(WaitForSceneLoad("Leaderboard"));
		}
#pragma warning restore CS0642 // Possible mistaken empty statement
		else
		{
			Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
			if (Input.GetKey(KeyCode.LeftControl))
				LoadedGameData.Continue = false;
			LoadedGameData.LevelSetFileName = LevelSetFileName;
			//LoadedGameData.LevelSetName = LevelSetName;
			coverAnimator.Cover(WaitForSceneLoad("Level"));
		}
	}

	private IEnumerator WaitForSceneLoad(string sceneName)
	{
		yield return new WaitUntil(() => coverAnimator.Covered);
		SceneManager.LoadScene(sceneName);
	}
}
