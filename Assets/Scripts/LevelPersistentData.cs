using System;
using System.IO;

public class LevelPersistentData
{
	public int LevelNum { get; set; } = 0;
	public int CurrentScore { get; set; } = 0;
	public int Paddles { get; set; } = 3;

	private const string magicNumber = "ufbsav";
	private readonly string saveFileName;
	private readonly DateTime levelSetDateTime;

	public LevelPersistentData(string saveFileName, DateTime levelSetDateTime)
	{
		this.saveFileName = Path.Combine($"Saves", $"{saveFileName}.sav");
		this.levelSetDateTime = levelSetDateTime;
	}

	public bool Load()
	{
		if (File.Exists(saveFileName))
			using (FileStream fileStream = File.OpenRead(saveFileName))
			using (BinaryReader saveReader = new BinaryReader(fileStream))
			{
				string readMagicNumber = saveReader.ReadString();
				if (readMagicNumber == magicNumber)
				{
					LevelNum = saveReader.ReadInt32();
					DateTime dateTime = new DateTime(saveReader.ReadInt64());
					if (levelSetDateTime != dateTime)
						return false;
					else
					{
						CurrentScore = saveReader.ReadInt32();
						Paddles = saveReader.ReadInt32();
						return true;
					}
				}
				else
					return false;
			}
		else
			return false;
	}

	public void Save()
	{
		using (FileStream fs = File.Create(saveFileName))
		using (BinaryWriter saveWriter = new BinaryWriter(fs))
		{
			saveWriter.Write(magicNumber);
			saveWriter.Write(LevelNum);
			saveWriter.Write(levelSetDateTime.Ticks);
			saveWriter.Write(CurrentScore);
			saveWriter.Write(Paddles);
		}
	}
	
	public static int ReadLastLevelNum(string saveFilename, DateTime levelSetModificationTime)
	{
		string savePath = Path.Combine($"Saves", $"{saveFilename}.sav");
		if (File.Exists(savePath))
			using (FileStream fileStream = File.OpenRead(savePath))
			using (BinaryReader saveReader = new BinaryReader(fileStream))
			{
				string readMagicNumber = saveReader.ReadString();
				if (readMagicNumber == magicNumber)
				{
					int levelNum = saveReader.ReadInt32();
					DateTime readModificationTime = new DateTime(saveReader.ReadInt64());
					if (levelSetModificationTime == readModificationTime)
						return levelNum;
					else
						return -1;
				}
				else
					return 0;
			}
		else
			return 0;
	}
}
