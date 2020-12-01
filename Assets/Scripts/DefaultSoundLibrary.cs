using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DefaultSoundLibrary : MonoBehaviour
{
	#region Singleton

	public static DefaultSoundLibrary Instance { get; private set; }

	void Awake()
	{
		if (Instance)
			Destroy(Instance);
		else
		{
			Instance = this;
			SoundLibrary = new Dictionary<string, AudioClip>();
			for (int i = 0; i < Mathf.Min(soundKeys.Count, soundClips.Count); i++)
				SoundLibrary.Add(soundKeys[i], soundClips[i]);
		}
	}
	#endregion

	[SerializeField]
	private List<string> soundKeys;

	[SerializeField]
	private List<AudioClip> soundClips;

	public Dictionary<string, AudioClip> SoundLibrary;

	public AudioClip normalBrickBreak;
	public AudioClip explosiveBrickHit;
	public AudioClip indestructibleBrickHit;
	public AudioClip changingBrickHit;
	public AudioClip plateHit;
}
