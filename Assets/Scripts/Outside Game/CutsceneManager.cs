using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CutsceneManager : MonoBehaviour
{
	private Sprite[] frames;
	private string[] dialogues;
	private int currentFrameIndex;

	[SerializeField]
	private Image sceneImage;
	[SerializeField]
	private Text sceneText;
	[SerializeField]
	private Text instructionText;

	[SerializeField]
	private ShutterAnimationManager shutterAnimationManager;

    // Start is called before the first frame update
    void Start()
    {
		string cutsceneName = CutsceneData.CutsceneName;
		string cutsceneDirectory = Path.Combine(LoadedGameData.LevelSetDirectory, LoadedGameData.LevelSetFileName, cutsceneName);
		string cutscenePath = Path.Combine(cutsceneDirectory, $"{cutsceneName}.cutscene");
		dialogues = FileImporter.LoadCutsceneDialogues(cutscenePath);
		frames = FileImporter.LoadCutsceneFrames(cutsceneDirectory);
		if (File.Exists(Path.Combine(cutsceneDirectory, "music.ogg")))
		{
			AudioClip music = FileImporter.LoadCutsceneMusic(cutsceneName, LoadedGameData.LevelSetFileName, LoadedGameData.LevelSetDirectory);
			while (music.loadState != AudioDataLoadState.Loaded) ;
			MusicManager.Instance.PlayMusic(music);
		}
		if (CutsceneData.CutsceneType == CutsceneType.Outro)
			instructionText.text = "Left - Back, Right - Forward, Enter or ESC - Skip";
		UpdateScene();
    }

    // Update is called once per frame
    void Update()
    {
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			switch (CutsceneData.CutsceneType)
			{
				case CutsceneType.Intro:
					shutterAnimationManager.Cover(GoToLevelSetList());
					break;
				case CutsceneType.Outro:
					shutterAnimationManager.Cover(GoToScene("Leaderboard"));
					break;
			}
		}
		else if (Input.GetKeyDown(KeyCode.Return))
			shutterAnimationManager.Cover(GoToScene(CutsceneData.NextScene));
		else if (Input.GetKeyDown(KeyCode.LeftArrow))
			GoBackward();
		else if (Input.GetKeyDown(KeyCode.RightArrow))
			GoForward();
    }

	private void GoForward()
	{
		if (currentFrameIndex < dialogues.Length - 1)
		{
			currentFrameIndex++;
			UpdateScene();
		}
		else
			shutterAnimationManager.Cover(GoToScene(CutsceneData.NextScene));
	}

	private void GoBackward()
	{
		if (currentFrameIndex > 0)
		{
			currentFrameIndex--;
			UpdateScene();
		}
	}

	private void UpdateScene()
	{
		sceneImage.sprite = frames[currentFrameIndex];
		sceneText.text = dialogues[currentFrameIndex];
	}

	private IEnumerator GoToLevelSetList()
	{
		yield return new WaitUntil(() => shutterAnimationManager.Covered);
		SceneManager.LoadScene("Level Set List");
	}

	private IEnumerator GoToScene(string sceneName)
	{
		yield return new WaitUntil(() => shutterAnimationManager.Covered);
		SceneManager.LoadScene(sceneName);
	}
}
