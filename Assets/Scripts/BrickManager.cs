using LevelSetData;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class BrickManager : MonoBehaviour
{
	#region Singleton
	public static BrickManager Instance { get; private set; }

	void Awake()
	{
		if (Instance)
			Destroy(Instance);
		else
			Instance = this;
	}
	#endregion

	[SerializeField]
	private ErrorMessage errorMessage;

	internal const int RegularExplosiveId = 18;
	internal const int IceBrickId = 87;//Id of brick type to which bricks hit by Space Djoels turn to

	private List<string> errorList = new List<string>();
	public BrickType[] DefaultBrickTypes { get; private set; }
	public BrickType[] CustomBrickTypes { get; private set; }

	private BrickType[] LevelSetBrickTypes;

	internal BrickType SpaceDjoelBrickType => LevelSetBrickTypes[IceBrickId - 1];

	internal BrickType GetBrickTypeById(int newBrickId) => LevelSetBrickTypes.First(b => b.Properties.Id == newBrickId);
    
    public void ImportBricks()
	{
		DefaultBrickTypes = LoadedGameData.DefaultBrickTypes ?? BrickType.LoadBricks(errorList);
		string levelSetCustomBricksPath = FileImporter.GetDirectoryNameInLevelSetDirectory(LoadedGameData.LevelSetDirectory, LoadedGameData.LevelSetFileName, "Bricks");
		if (Directory.Exists(levelSetCustomBricksPath))
			CustomBrickTypes = BrickType.LoadBricks(errorList, levelSetCustomBricksPath);
	}

	public void PrepareBrickTypes(LevelSet levelSet)
	{
		if (!(CustomBrickTypes is null))
			LevelSetBrickTypes = DefaultBrickTypes.Concat(CustomBrickTypes).ToArray();
		else
			LevelSetBrickTypes = DefaultBrickTypes;
		CheckIfIdsArePresent(levelSet);
		ParticleManager.Instance.CreateBrickParticles(LevelSetBrickTypes);
	}
	private void CheckIfIdsArePresent(LevelSet levelSet)
	{
		bool anyMissingId = false;
		IEnumerable<int> loadedBrickTypeIds = LevelSetBrickTypes.Select(bt => bt.Properties.Id);
		foreach (Level level in levelSet.Levels)
		{
			for (int i = 0; i < LevelSet.ROWS; i++)
			{
				for (int j = 0; j < LevelSet.COLUMNS; j++)
				{
					if (level.Bricks[i, j].BrickId != 0 && !loadedBrickTypeIds.Contains(level.Bricks[i, j].BrickId))
					{
						level.Bricks[i, j].BrickId = 0;
						anyMissingId = true;
					}
				}
			}
		}
		//TODO do more precise brick error handling (no info about failed bricks is so far)
		if (anyMissingId)
			errorMessage.Show("Some bricks in levels belong to types which are not loaded. They will be ignored. Please check your bricks with level editor.");
	}
}
