using System.IO;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class LevelSoundLibrary : MonoBehaviour
{
	#region Singleton
	public static LevelSoundLibrary Instance { get; private set; }

	void Awake()
	{
		if (Instance)
			Destroy(Instance);
		else
			Instance = this;
	}
	#endregion

	public AudioClip normalBallBounce;
	public AudioClip bang;
	public AudioClip explosion;
	public AudioClip specialHit;
	public AudioClip powerUpYield;
	public AudioClip hitWall;
	public AudioClip ballFall;
	public AudioClip spaceDjoelFall;
	public AudioClip powerUpFall;
	public AudioClip magnetStick;
	public AudioClip ballSizeChange;
	public AudioClip brickDescend;
	public AudioClip losePaddle;
	public AudioClip bulletShoot;
	public AudioClip ballPush;
	public AudioClip teleport;
	public AudioClip protectiveBarrierHit;
	public AudioClip win;

	public AudioClip normalBrickBreak;
	public AudioClip indestructibleBrickHit;
	public AudioClip changingBrickHit;

	private AudioSource audioSource;

	private void Start()
	{
		audioSource = GetComponent<AudioSource>();
	}

	public void PlaySfx(AudioClip clip)
	{
		if (clip)
			audioSource.PlayOneShot(clip);
	}

	public void LoadCustomAudio(string levelName)
	{
		try
		{
			//UNDONE custom audio import
			normalBallBounce = FileImporter.LoadAudioClip(levelName, "laser bounce");
		}
		catch (FileNotFoundException fnfe)
		{
			Debug.LogError($"File {fnfe.Message}.wav is missing. Please erase sound file name in level editor or find file.");
		}
	}
}
