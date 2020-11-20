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
	private static T LoadFromBinaryFile<T>(string filePath, string fileSignature = null) where T : class
	{
		using (FileStream fs = File.Open(filePath, FileMode.Open))
		{
			if (!(fileSignature is null))
			{
				byte[] signatureBytes = new byte[fileSignature.Length];
				fs.Read(signatureBytes, 0, signatureBytes.Length);
				if (Encoding.Default.GetString(signatureBytes) != fileSignature)
					throw new IOException("Could not read file.");
			}
			return new BinaryFormatter().Deserialize(fs) as T;
		}
	}

	internal static string GetDirectoryNameInLevelSetDirectory(string levelSetDirectoryPath, string levelSetFileName, string elementName) => Path.Combine(levelSetDirectoryPath, levelSetFileName, elementName);

	internal static bool LevelSetExternalFileDirectoryExists(string levelSetDirectoryPath, string levelSetFileName) => Directory.Exists(Path.Combine(levelSetDirectoryPath, levelSetFileName));

	public static Texture2D GetBackgroundTexture(string levelSetDirectoryPath, string levelSetFileName, string backgroundName)
	{
		string backgroundPath = Path.Combine(GetDirectoryNameInLevelSetDirectory(levelSetDirectoryPath, levelSetFileName, "Backgrounds"), $"{backgroundName}.png");
		return LoadTexture(backgroundPath);
	}

	public static LevelSet LoadLevelSet(string levelSetDirectoryPath, string levelSetFileName) =>
		LoadFromBinaryFile<LevelSet>(Path.Combine(levelSetDirectoryPath, $"{levelSetFileName}.nlev"), "nuLev");

	public static BrickProperties LoadBrickProperties(string brickFilePath) =>
		LoadFromBinaryFile<BrickProperties>($"{brickFilePath}.brick");

	public static BrickType[] LoadBricks(HashSet<string> missingFileNames = null, string bricksPath = "Default Bricks")
	{
		return Directory.GetFiles(bricksPath, "*.brick", SearchOption.TopDirectoryOnly)
			.Select(fn => new BrickType(Path.GetFileNameWithoutExtension(fn), missingFileNames, bricksPath))
			.OrderBy(bt => bt.Properties.Id).ToArray();
	}

	public static LevelPersistentData LoadLevelPersistentData(string fileName)
	{
		string filePath = LevelPersistentData.GetSaveFilePath(fileName);
		return File.Exists(filePath) ? LoadFromBinaryFile<LevelPersistentData>(filePath) : null;
	}

	public static Texture2D LoadTexture(string FilePath)
	{
		Texture2D Tex2D;
		byte[] FileData;

		if (File.Exists(FilePath))
		{
			FileData = File.ReadAllBytes(FilePath);
			Tex2D = new Texture2D(2, 2);
			if (Tex2D.LoadImage(FileData))
				return Tex2D;
		}
		return null;
	}

	public static Dictionary<string, AudioClip> LoadAudioClipsFromLevelSet(string levelSetName, TestMode testMode, string levelSetDirectory)
	{
		Dictionary<string, AudioClip> existingAudioClips = new Dictionary<string, AudioClip>();
		string levelSetPath = testMode == TestMode.None ? Path.Combine(Application.dataPath, $"../Level sets/") : levelSetDirectory;
		string soundDirectoryPath = GetDirectoryNameInLevelSetDirectory(levelSetPath, levelSetName, "Sounds");
		if (Directory.Exists(soundDirectoryPath))
		{
			IEnumerable<string> soundFileNames = Directory.EnumerateFiles(soundDirectoryPath, "*.wav").Select(s => Path.GetFileNameWithoutExtension(s));
			foreach (string soundFileName in soundFileNames)
			{
				existingAudioClips.Add(soundFileName, LoadAudioClip(levelSetName, soundFileName, testMode, levelSetDirectory));
			}
			return existingAudioClips;
		}
		else
			return null;
	}

	public static AudioClip LoadAudioClip(string levelSetName, string soundName, TestMode testMode, string levelSetDirectory)
	{
		/*try
		{*/
		string levelSetPath = testMode == TestMode.None ? Path.Combine(Application.dataPath, $"../Level sets/") : levelSetDirectory;
		UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(Path.Combine("file://", GetDirectoryNameInLevelSetDirectory(levelSetPath, levelSetName, "Sounds"), $"{soundName}.wav"), AudioType.WAV);
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

	public static List<LeaderboardPosition> LoadHighScores(string levelSetFileName) =>
		LoadFromBinaryFile<List<LeaderboardPosition>>(Path.Combine("High scores", $"{levelSetFileName}.sco"));
}
