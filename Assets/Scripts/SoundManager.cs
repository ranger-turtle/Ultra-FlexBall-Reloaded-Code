using System.Collections.Generic;
using System.IO;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
	#region Singleton

	public static SoundManager Instance { get; private set; }

	void Awake()
	{
		if (Instance)
			Destroy(Instance);
		else
			Instance = this;
	}
	#endregion

	private Dictionary<string, AudioClip> loadedFiles;

	private Dictionary<string, AudioClip> levelSetSoundLibrary;
	private Dictionary<string, AudioClip> levelSoundLibrary;

	private AudioSource audioSource;

	private void Start()
	{
		audioSource = GetComponent<AudioSource>();
		/*levelSetSoundLibrary = DefaultSoundLibrary.Instance.SoundLibrary;
		levelSoundLibrary = DefaultSoundLibrary.Instance.SoundLibrary;*/
	}

	public void LoadSounds(string levelSetName, TestMode testMode, string levelSetDirectory)
	{
		loadedFiles = FileImporter.LoadAudioClipsFromLevelSet(levelSetName, testMode, levelSetDirectory);
	}

	public void UpdateLevelSetSounds(LevelSetData.LevelSet levelSet, List<string> errorList)
	{
		IEnumerable<string> soundFileKeys = LevelSetData.SoundLibrary.GetSoundKeys();
		Dictionary<string, AudioClip> gameSoundLibrary = DefaultSoundLibrary.Instance.SoundLibrary;
		LevelSetData.SoundLibrary loadedLevelSetSoundLibrary = levelSet.LevelSetProperties.DefaultSoundLibrary;
		levelSetSoundLibrary = new Dictionary<string, AudioClip>();
		foreach (string soundKey in soundFileKeys)
		{
			string loadedLevelSetSoundName = null;
			try
			{
				loadedLevelSetSoundName = loadedLevelSetSoundLibrary.FromStringKey(soundKey);
				if (loadedLevelSetSoundName == "<none>")
					levelSetSoundLibrary[soundKey] = null;
				else if (loadedLevelSetSoundName == "<game-default>")
					levelSetSoundLibrary[soundKey] = gameSoundLibrary[soundKey];
				else
					levelSetSoundLibrary[soundKey] = loadedFiles[loadedLevelSetSoundName];
			}
			catch (KeyNotFoundException)
			{
				errorList.Add($"{loadedLevelSetSoundName}.wav");
			}
		}
	}

	public void UpdateLevelSounds(LevelSetData.Level level, List<string> errorList)
	{
		IEnumerable<string> soundFileKeys = LevelSetData.SoundLibrary.GetSoundKeys();
		Dictionary<string, AudioClip> gameSoundLibrary = DefaultSoundLibrary.Instance.SoundLibrary;
		LevelSetData.SoundLibrary loadedLevelSoundLibrary = level.LevelProperties.SoundLibrary;
		levelSoundLibrary = new Dictionary<string, AudioClip>(levelSetSoundLibrary);
		foreach (string soundKey in soundFileKeys)
		{
			string loadedLevelSoundName = null;
			try
			{
				loadedLevelSoundName = loadedLevelSoundLibrary.FromStringKey(soundKey);
				if (loadedLevelSoundName == "<none>")
					levelSoundLibrary[soundKey] = null;
				else if (loadedLevelSoundName == "<game-default>")
					levelSoundLibrary[soundKey] = gameSoundLibrary[soundKey];
				else if (loadedLevelSoundName == "<level-set-default>")
					levelSoundLibrary[soundKey] = levelSetSoundLibrary[soundKey];
				else
					levelSoundLibrary[soundKey] = loadedFiles[loadedLevelSoundName];
			}
			catch (KeyNotFoundException)
			{
				errorList.Add($"{soundKey}: {loadedLevelSoundName}.wav");
			}
		}
	}

	public AudioClip FromLoadedSoundFiles(string soundName)
	{
		try
		{
			while (loadedFiles[soundName].loadState != AudioDataLoadState.Loaded) ;
			return loadedFiles[soundName];
		}
		catch (KeyNotFoundException)
		{
			throw new FileNotFoundException($"File {loadedFiles[soundName]}.wav not found");
		}
	}

	public void PlaySfx(string audioKey)
	{
		if (audioKey != null && levelSoundLibrary[audioKey] != null)
		{
			while (levelSoundLibrary[audioKey].loadState == AudioDataLoadState.Loading) ;
			audioSource.PlayOneShot(levelSoundLibrary[audioKey]);
		}
	}

	public void PlaySfx(AudioClip audioClip)
	{
		if (audioClip != null)
			audioSource.PlayOneShot(audioClip);
	}

	public AudioClip GetClipByKey(string key) => levelSoundLibrary[key];
}
