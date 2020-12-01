using LevelSetData;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
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

	public static BrickType[] LoadBricks(List<string> errorList, string bricksPath = "Default Bricks")
	{
		List<BrickType> brickType = new List<BrickType>();
		string[] brickFilePaths = Directory.GetFiles(bricksPath, "*.brick", SearchOption.TopDirectoryOnly);
		foreach (string brickFilePath in brickFilePaths)
		{
			try
			{
				brickType.Add(new BrickType(Path.GetFileNameWithoutExtension(brickFilePath), bricksPath));
			}
			catch (DirectoryNotFoundException dnfe)
			{
				errorList.Add(dnfe.Message);
			}
			catch (FileNotFoundException)
			{
				errorList.Add($"Brick saved at {brickFilePath} not found.");
			}
			catch (BrickType.InvalidBrickTextureException ibte)
			{
				errorList.Add(ibte.Message);
			}
			catch (IOException)
			{
				errorList.Add($"Brick saved at {brickFilePath} is corrupt.");
			}
		}
		return brickType.OrderBy(bt => bt.Properties.Id).ToArray();
	}

	public static Dictionary<string, Texture2D> LoadBackgroundsFromLevelSet(string levelSetName, string levelSetDirectory)
	{
		string backgroundDirectoryPath = GetDirectoryNameInLevelSetDirectory(levelSetDirectory, levelSetName, "Backgrounds");
		if (Directory.Exists(backgroundDirectoryPath))
		{
			return Directory.EnumerateFiles(backgroundDirectoryPath, "*.png")
				.Select(s => Path.GetFileNameWithoutExtension(s))
				.ToDictionary(bgImageName => bgImageName, bgImageName => LoadTexture(Path.Combine(backgroundDirectoryPath, $"{bgImageName}")));
		}
		else
			return null;
	}

	/// <summary>
	/// Loads texture from disk.
	/// </summary>
	/// <param name="filePath"></param>
	/// <exception cref="IOException"></exception>
	/// <returns></returns>
	public static Texture2D LoadTexture(string filePath)
	{
		Texture2D Tex2D;
		byte[] FileData;

		FileData = File.ReadAllBytes($"{filePath}.png");
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

			if (uwr.isHttpError)
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
}
