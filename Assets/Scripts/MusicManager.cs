using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
	#region Singleton
	public static MusicManager Instance { get; private set; }

	[SerializeField]
	private AudioClip fanfare;

	void Awake()
	{
		if (Instance)
			Destroy(gameObject);
		else
		{
			DontDestroyOnLoad(gameObject);
			Instance = this;
		}
	}
	#endregion

	[SerializeField]
	private AudioSource musicSource;
	[SerializeField]
	private AudioClip mainMenuMusic;

	private Dictionary<string, AudioClip> loadedMusic;
	private AudioClip levelSetDefaultMusic;

	private void Start()
	{
		musicSource.mute = SettingsManager.LoadBool("mute", false);
		musicSource.volume = SettingsManager.LoadFloat("volume", 0.60f);
		musicSource.Play();
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.M))
			ToggleMute();
	}

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

	public void LaunchSlowDown()
	{
		DontDestroyOnLoad(gameObject);
		StartCoroutine(SlowDown());
	}

	public IEnumerator SlowDown()
	{
		while (musicSource.pitch > 0)
		{
			musicSource.pitch -= 0.05f;
			yield return new WaitForSeconds(0.2f);
		}
		yield return new WaitForSeconds(0.3f);
		musicSource.pitch = 1;
		musicSource.clip = mainMenuMusic;
		musicSource.Play();
	}

	public void SwitchToTitle()
	{
		if (musicSource.clip != mainMenuMusic)
		{
			musicSource.clip = mainMenuMusic;
			musicSource.Play();
		}
	}

	public void PlayFanfare()
	{
		musicSource.PlayOneShot(fanfare);
		SwitchToTitle();
	}

	public void PlayMusic(AudioClip audioClip)
	{
		while (audioClip.loadState == AudioDataLoadState.Loading) ;
		musicSource.clip = audioClip;
		musicSource.Play();
	}

	internal void ToggleMute()
	{
		bool newValue = musicSource.mute = !musicSource.mute;
		SettingsManager.SaveBool("mute", newValue);
	}
}
