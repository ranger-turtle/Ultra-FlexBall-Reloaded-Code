public enum TestMode
{
	None, TestLevelSet, TestOneLevel
}

public enum CutsceneType
{
	Intro, Outro
}

public class LoadedGameData
{
	public static BrickType[] DefaultBrickTypes { get; set; }
	public static UnityEngine.Sprite[] BurnAnimation { get; set; }
	public static string LevelSetDirectory { get; set; } = "Level sets";
	public static string LevelSetFileName { get; set; } = "wert";
	public static bool Continue { get; set; } = true;
	public static int TestLevelNum { get; set; }
	public static TestMode TestMode { get; set; } = TestMode.None;
}

public class CutsceneData
{
	public static string CutsceneName { get; set; }
	public static string NextScene { get; set; }
	public static CutsceneType CutsceneType { get; set; }
}

public class EndGameData
{
	public static bool HighScoreChange { get; set; }
	public static int Score { get; set; }
	public static int LastLevel { get; set; }
	public static bool Won { get; set; }
}