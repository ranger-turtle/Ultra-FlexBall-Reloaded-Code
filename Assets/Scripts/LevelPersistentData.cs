using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[Serializable]
public class LevelPersistentData
{
	public int LevelNum { get; set; }
	public int CurrentScore { get; set; }
	public int Paddles { get; set; }

	public static string GetSaveFilePath(string saveFileName) => Path.Combine($"Saves", $"{saveFileName}.sav");

	public void Save(string fileName)
	{
		using (FileStream fs = File.Open(GetSaveFilePath(fileName), FileMode.Create))
			new BinaryFormatter().Serialize(fs, this);
	}
}
