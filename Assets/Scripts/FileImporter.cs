using LevelSetData;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

internal class FileImporter
{
	internal static string GetDirectoryNameInLevelSetDirectory(string levelSetDirectoryPath, string levelSetFileName, string elementName) => Path.Combine(levelSetDirectoryPath, levelSetFileName, elementName);

	internal static bool LevelSetResourceDirectoryExists(string levelSetDirectoryPath, string levelSetFileName) => Directory.Exists(Path.Combine(levelSetDirectoryPath, levelSetFileName));

	public static LevelSet LoadLevelSet(string levelSetDirectoryPath, string levelSetFileName) =>
		UltraFlexBallReloadedFileLoader.LoadLevelSet(Path.Combine(levelSetDirectoryPath, $"{levelSetFileName}.nlev"));

	public static BrickProperties LoadBrickProperties(string brickFilePath) =>
		UltraFlexBallReloadedFileLoader.LoadBrick($"{brickFilePath}.brick");

	public static Dictionary<string, Texture2D> LoadTexturesFromLevelSet(string levelSetName, string levelSetDirectory, string assetDirectory)
	{
		string backgroundDirectoryPath = GetDirectoryNameInLevelSetDirectory(levelSetDirectory, levelSetName, assetDirectory);
		if (Directory.Exists(backgroundDirectoryPath))
		{
			return Directory.EnumerateFiles(backgroundDirectoryPath, "*.png")
				.Select(s => Path.GetFileNameWithoutExtension(s))
				.ToDictionary(bgImageName => bgImageName, bgImageName => LoadTexture(Path.Combine(backgroundDirectoryPath, $"{bgImageName}")));
		}
		else
			return null;
	}

	public static Dictionary<string, Texture2D> LoadBackgroundsFromLevelSet(string levelSetName, string levelSetDirectory)
	{
		return LoadTexturesFromLevelSet(levelSetName, levelSetDirectory, "Backgrounds");
	}

	public static Dictionary<string, Texture2D> LoadWallTexturesFromLevelSet(string levelSetName, string levelSetDirectory)
	{
		return LoadTexturesFromLevelSet(levelSetName, levelSetDirectory, "Walls");
	}

	/// <summary>
	/// Loads texture from disk.
	/// </summary>
	/// <param name="filePath"></param>
	/// <exception cref="IOException"></exception>
	/// <returns></returns>
	public static Texture2D LoadTexture(string filePath, string extension = ".png")
	{
		Texture2D Tex2D;
		byte[] FileData;

		FileData = File.ReadAllBytes($"{filePath}{extension}");
		Tex2D = new Texture2D(2, 2);
		if (Tex2D.LoadImage(FileData))
			return Tex2D;
		else
			throw new IOException();
	}

	public static Dictionary<string, AudioClip> LoadAudioClipsFromLevelSet(string levelSetName, TestMode testMode, string levelSetDirectory)
	{
		string levelSetPath = testMode == TestMode.None ? Path.Combine(Application.dataPath, $"../Level sets/") : levelSetDirectory;
		string soundDirectoryPath = GetDirectoryNameInLevelSetDirectory(levelSetPath, levelSetName, "Sounds");
		if (Directory.Exists(soundDirectoryPath))
		{
			return Directory.EnumerateFiles(soundDirectoryPath, "*.wav")
				.Select(s => Path.GetFileNameWithoutExtension(s))
				.ToDictionary(soundFileName => soundFileName, soundFileName => LoadAudioClip(levelSetName, soundFileName, testMode, levelSetDirectory));
		}
		else
			return null;
	}

	public static AudioClip LoadCutsceneMusic(string cutsceneName, string levelSetName, string levelSetDirectory)
	{
		UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(Path.Combine("file://", Application.dataPath, $"../Level sets/", levelSetName, cutsceneName, $"music.ogg"), AudioType.OGGVORBIS);
		uwr.SendWebRequest();

		if (uwr.result == UnityWebRequest.Result.ProtocolError)
			throw new FileNotFoundException();

		while (uwr.downloadProgress < 1.0f) ;

		return (uwr.downloadHandler as DownloadHandlerAudioClip)?.audioClip;
	}

	public static Dictionary<string, AudioClip> LoadMusicFromLevelSet(string levelSetName, TestMode testMode, string levelSetDirectory)
	{
		string levelSetPath = testMode == TestMode.None ? Path.Combine(Application.dataPath, $"../Level sets/") : levelSetDirectory;
		string soundDirectoryPath = GetDirectoryNameInLevelSetDirectory(levelSetPath, levelSetName, "Music");
		if (Directory.Exists(soundDirectoryPath))
		{
			return Directory.EnumerateFiles(soundDirectoryPath, "*.ogg")
				.Select(s => Path.GetFileNameWithoutExtension(s))
				.ToDictionary(musicFileName => musicFileName, musicFileName => LoadAudioClip(levelSetName, musicFileName, testMode, levelSetDirectory, AudioType.OGGVORBIS));
		}
		else
			return null;
	}

	private static AudioClip LoadAudioClip(string levelSetName, string soundName, TestMode testMode, string levelSetDirectory, AudioType audioType = AudioType.WAV)
	{
		/*try
		{*/
		string extension = audioType == AudioType.WAV ? "wav" : "ogg";
		string resourceFolderName = audioType == AudioType.WAV ? "Sounds" : "Music";
		string levelSetPath = testMode == TestMode.None ? Path.Combine(Application.dataPath, $"../Level sets/") : levelSetDirectory;
		UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(Path.Combine("file://", GetDirectoryNameInLevelSetDirectory(levelSetPath, levelSetName, resourceFolderName), $"{soundName}.{extension}"), audioType);
			uwr.SendWebRequest();

			if (uwr.result == UnityWebRequest.Result.ProtocolError)
				throw new FileNotFoundException();

				while (uwr.downloadProgress < 1.0f) ;
			//	{
					//Debug.LogFormat($"Progress loading {uwr.url}: {uwr.downloadProgress * 100} %");
				//}

			return (uwr.downloadHandler as DownloadHandlerAudioClip)?.audioClip;
		/*}
		catch (FileNotFoundException)
		{
			NotFoundFiles.Add(soundName);
			return null;
		}*/
	}

	public static string[] LoadCutsceneDialogues(string cutscenePath) => UltraFlexBallReloadedFileLoader.LoadCutsceneDialogues(cutscenePath);

	public static Sprite[] LoadCutsceneFrames(string cutsceneDirectory)
	{
		List<Sprite> frameSprites = new List<Sprite>();
		for (int i = 1; File.Exists(Path.Combine(cutsceneDirectory, $"frame{i}.jpg")); i++)
		{
			Texture2D frameTexture = LoadTexture(Path.Combine(cutsceneDirectory, $"frame{i}"), ".jpg");
			Sprite frameSprite = Sprite.Create(frameTexture, Rect.MinMaxRect(0, 0, frameTexture.width, frameTexture.height), Vector2.zero, 48.0f, 1, SpriteMeshType.FullRect);
			frameSprites.Add(frameSprite);
		}
		return frameSprites.ToArray();
	}
}
