using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

	private Dictionary<string, AudioClip> gameSoundLibrary = new Dictionary<string, AudioClip>()
		{
			{ "Normal Ball Bounce" , DefaultSoundLibrary.Instance.normalBallBounce },
			{ "Bang" , DefaultSoundLibrary.Instance.bang },
			{ "Explosion" , DefaultSoundLibrary.Instance.explosion },
			{ "Special Hit" , DefaultSoundLibrary.Instance.specialHit },
			{ "Power Up Yield" , DefaultSoundLibrary.Instance.powerUpYield },
			{ "Hit Wall" , DefaultSoundLibrary.Instance.hitWall },
			{ "Ball Fall" , DefaultSoundLibrary.Instance.ballFall },
			{ "Space Djoel Fall" , DefaultSoundLibrary.Instance.spaceDjoelFall },
			{ "Power Up Fall" , DefaultSoundLibrary.Instance.powerUpFall },
			{ "Magnet Stick" , DefaultSoundLibrary.Instance.magnetStick },
			{ "Ball Size Change" , DefaultSoundLibrary.Instance.ballSizeChange },
			{ "Brick Descend" , DefaultSoundLibrary.Instance.brickDescend },
			{ "Lose Paddle" , DefaultSoundLibrary.Instance.losePaddle },
			{ "Bullet Shoot" , DefaultSoundLibrary.Instance.bulletShoot },
			{ "Bullet Bounce" , DefaultSoundLibrary.Instance.bulletBounce },
			{ "Ball Thrust" , DefaultSoundLibrary.Instance.ballThrust },
			{ "Teleport" , DefaultSoundLibrary.Instance.teleport },
			{ "Mega Missile Shoot" , DefaultSoundLibrary.Instance.megaMissileShoot },
			{ "Mega Explosion" , DefaultSoundLibrary.Instance.megaExplosion },
			{ "Protective Barrier Hit" , DefaultSoundLibrary.Instance.protectiveBarrierHit },
			{ "Quick Ball Bounce" , DefaultSoundLibrary.Instance.quickBallBounce },
			{ "Win" , DefaultSoundLibrary.Instance.win },
			{ "Normal Brick Break" , DefaultSoundLibrary.Instance.normalBrickBreak },
			{ "Explosive Brick Hit" , DefaultSoundLibrary.Instance.explosiveBrickHit },
			{ "Indestructible Brick Hit" , DefaultSoundLibrary.Instance.indestructibleBrickHit },
			{ "Changing Brick Hit" , DefaultSoundLibrary.Instance.changingBrickHit },
			{ "Plate Hit" , DefaultSoundLibrary.Instance.plateHit },
		};
	private Dictionary<string, AudioClip> loadedFiles;

	private Dictionary<string, AudioClip> levelSetSoundLibrary;
	private Dictionary<string, AudioClip> levelSoundLibrary;

	private AudioSource audioSource;

	private void Start()
	{
		audioSource = GetComponent<AudioSource>();
	}

	public void LoadSounds(string levelSetName, TestMode testMode, string levelSetDirectory)
	{
		loadedFiles = FileImporter.LoadAudioClipsFromLevelSet(levelSetName, testMode, levelSetDirectory);
	}

	public void UpdateLevelSetSounds(LevelSetData.LevelSet levelSet, HashSet<string> missingSoundNames)
	{
		IEnumerable<string> soundFileKeys = LevelSetData.SoundLibrary.SoundNames;
		LevelSetData.SoundLibrary loadedLevelSetSoundLibrary = levelSet.LevelSetProperties.DefaultSoundLibrary;
		levelSetSoundLibrary = new Dictionary<string, AudioClip>(gameSoundLibrary);
		foreach (string soundKey in soundFileKeys)
		{
			string loadedLevelSoundKey = null;
			try
			{
				loadedLevelSoundKey = loadedLevelSetSoundLibrary.FromStringKey(soundKey).GetValue();
				if (loadedLevelSoundKey == "<none>")
					levelSetSoundLibrary[soundKey] = null;
				else if (loadedLevelSoundKey == "<game-default>")
					levelSetSoundLibrary[soundKey] = gameSoundLibrary[soundKey];
				else
					levelSetSoundLibrary[soundKey] = loadedFiles[loadedLevelSoundKey];
			}
			catch (KeyNotFoundException)
			{
				missingSoundNames.Add($"{loadedLevelSoundKey}.wav");
			}
		}
	}

	public void UpdateLevelSounds(LevelSetData.Level level, string levelSetName, HashSet<string> missingSoundNames)
	{
		IEnumerable<string> soundFileKeys = LevelSetData.SoundLibrary.SoundNames;
		LevelSetData.SoundLibrary loadedLevelSoundLibrary = level.LevelProperties.SoundLibrary;
		levelSoundLibrary = new Dictionary<string, AudioClip>(levelSetSoundLibrary);
		foreach (string soundKey in soundFileKeys)
		{
			string loadedLevelSoundKey = null;
			try
			{
				loadedLevelSoundKey = loadedLevelSoundLibrary.FromStringKey(soundKey).GetValue();
				if (loadedLevelSoundKey == "<none>")
					levelSoundLibrary[soundKey] = null;
				else if (loadedLevelSoundKey == "<game-default>")
					levelSoundLibrary[soundKey] = gameSoundLibrary[soundKey];
				else if (loadedLevelSoundKey == "<level-set-default>")
					levelSoundLibrary[soundKey] = levelSetSoundLibrary[soundKey];
				else
					levelSoundLibrary[soundKey] = loadedFiles[loadedLevelSoundKey];
			}
			catch (KeyNotFoundException)
			{
				missingSoundNames.Add($"{soundKey}: {loadedLevelSoundKey}.wav");
			}
		}
	}

	public AudioClip FromLoadedSoundFiles(string audioKey, HashSet<string> missingFileNames)
	{
		try
		{
			return loadedFiles[audioKey];
		}
		catch (KeyNotFoundException)
		{
			missingFileNames.Add(audioKey);
			return null;
		}
			//throw new FileNotFoundException($"File {loadedFiles[audioKey]}.wav not found");
	}

	public void PlaySfx(string audioKey)
	{
		if (audioKey != null && levelSoundLibrary[audioKey] != null)
			audioSource.PlayOneShot(levelSoundLibrary[audioKey]);
	}

	public void PlaySfx(AudioClip audioClip)
	{
		if (audioClip != null)
			audioSource.PlayOneShot(audioClip);
	}
}
