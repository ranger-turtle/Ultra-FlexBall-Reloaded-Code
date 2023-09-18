using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLoader : MonoBehaviour
{
	[SerializeField]
	private ShutterAnimationManager coverManager;
	[SerializeField]
	private Sprite[] burnAnimation;

	public void Start()
	{
		Cursor.visible = false;
		Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
		if (LoadedGameData.BurnAnimation == null)
		{
			LoadedGameData.BurnAnimation = burnAnimation;
			LoadedGameData.DefaultBrickTypes = BrickType.LoadBricks(null);
		}
		if (LoadedGameData.TestMode != TestMode.None)
			StartCoroutine(WaitForUncover());
	}

	private IEnumerator WaitForUncover()
	{
		yield return new WaitWhile(() => !coverManager.Uncovered);
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
