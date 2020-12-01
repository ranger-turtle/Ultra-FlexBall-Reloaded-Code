using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
	#region Singleton
	public static MusicManager Instance { get; private set; }

	void Awake()
	{
		if (Instance)
			Destroy(Instance);
		else
		{
			Instance = this;
		}
	}
	#endregion

	[SerializeField]
	private AudioSource musicSource;

	private Dictionary<string, AudioClip> loadedMusic;
	private AudioClip levelSetDefaultMusic;

	public void LoadLevelSetTextures(string levelSetDirectory, string levelSetFileName, TestMode testMode)
	{
		loadedMusic = FileImporter.LoadMusicFromLevelSet(levelSetFileName, testMode, levelSetDirectory);
	}

	public void UpdateLevelSetMusic(LevelSetData.LevelSet levelSet, List<string> errorList)
	{
		string backgroundName = levelSet.LevelSetProperties.DefaultMusic;
		if (loadedMusic != null)
		{
			if (backgroundName == "<none>")
				levelSetDefaultMusic = null;
			else
			{
				try
				{
					levelSetDefaultMusic = loadedMusic[backgroundName];
				}
				catch (KeyNotFoundException)
				{
					errorList.Add($"Music {backgroundName} not found.");
				}
			}
		}
		else if (backgroundName[0] != '<')
			errorList.Add($"Music {backgroundName} not found.");
	}

	public void UpdateLevelMusic(LevelSetData.Level level, List<string> errorList)
	{
		AudioClip previousClip = musicSource.clip;
		AudioClip newClip = null;
		string backgroundName = level.LevelProperties.Music;
		if (loadedMusic != null)
		{
			if (backgroundName == "<level-set-default>")
				newClip = levelSetDefaultMusic;
			else if (backgroundName != "<none>")
			{
				try
				{
					newClip = loadedMusic[backgroundName];
				}
				catch (KeyNotFoundException)
				{
					errorList.Add($"Music {backgroundName} not found.");
				}
			}
		}
		else if (backgroundName[0] != '<')
			errorList.Add($"Music {backgroundName} not found.");
		while (newClip?.loadState == AudioDataLoadState.Loading) ;
		if (previousClip != newClip)
		{
			musicSource.clip = newClip;
			musicSource.Play();
		}
	}
}
