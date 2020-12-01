using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameLoader : MonoBehaviour
{
	[SerializeField]
	private ShutterAnimationManager coverManager;
	[SerializeField]
	private Sprite[] burnAnimation;

	public void Start()
	{
		if (LoadedGameData.BurnAnimation == null)
		{
			LoadedGameData.BurnAnimation = burnAnimation;
			LoadedGameData.DefaultBrickTypes = FileImporter.LoadBricks(null);
		}
		if (LoadedGameData.TestMode != TestMode.None)
			coverManager.Cover(GoToScene("Level"));
	}

	private IEnumerator GoToScene(string sceneName)
	{
		yield return new WaitUntil(() => coverManager.Covered);
		SceneManager.LoadScene(sceneName);
	}

	private IEnumerator Quit()
	{
		yield return new WaitUntil(() => coverManager.Covered);
		Application.Quit();
	}

	private void Update()
	{
		if (Input.GetMouseButtonDown(0))
			coverManager.Cover(GoToScene("Level Set List"));
		else if (Input.GetKeyDown(KeyCode.Escape))
			coverManager.Cover(Quit());
	}
}
