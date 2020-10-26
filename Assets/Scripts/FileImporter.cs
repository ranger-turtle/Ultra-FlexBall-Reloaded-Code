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
					throw new IOException("Could not read *.brick file.");
			}
			return new BinaryFormatter().Deserialize(fs) as T;
		}
	}

	internal static string GetElementInLevelSetDirectory(string levelSetFileName, string elementName) => Path.Combine("Level sets", levelSetFileName, elementName);

	public static Texture2D GetBackgroundTexture(string levelSetFileName, string backgroundName)
	{
		string backgroundPath = Path.Combine(GetElementInLevelSetDirectory(levelSetFileName, "Backgrounds"), $"{backgroundName}.png");
		return LoadTexture(backgroundPath);
	}

	public static LevelSet LoadLevelSet(string levelSetFileName) =>
		LoadFromBinaryFile<LevelSet>(Path.Combine("Level sets", $"{levelSetFileName}.nlev"), "nuLev");

	public static LevelSet LoadLevelSetWithFullPath(string levelSetFilePath) =>
		LoadFromBinaryFile<LevelSet>($"{levelSetFilePath}.nlev", "nuLev");

	public static BrickProperties LoadBrickProperties(string brickFilePath) =>
		LoadFromBinaryFile<BrickProperties>($"{brickFilePath}.brick");

	public static BrickType[] LoadBricks(string bricksPath = "Default Bricks")
	{
		return Directory.GetFiles(bricksPath, "*.brick", SearchOption.TopDirectoryOnly)
			.Select(fn => new BrickType(Path.GetFileNameWithoutExtension(fn)))
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

	public static AudioClip LoadAudioClip(string levelSet, string soundName)
	{
		UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(Path.Combine("file://", Application.dataPath, $"../Level sets/{levelSet}/Sounds", $"{soundName}.wav"), AudioType.WAV);
		uwr.SendWebRequest();

		if (uwr.isHttpError)
			throw new FileNotFoundException(soundName);

		while (uwr.downloadProgress < 1.0f)
		{
			//Debug.LogFormat($"Progress loading {uwr.url}: {uwr.downloadProgress * 100} %");
		}

		return (uwr.downloadHandler as DownloadHandlerAudioClip)?.audioClip;
	}

	public static List<LeaderboardPosition> LoadHighScores(string levelSetFileName) =>
		LoadFromBinaryFile<List<LeaderboardPosition>>(Path.Combine("High scores", $"{levelSetFileName}.sco"));
}
