#define DEBUG
using LevelSetData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum BallSize
{
	Normal, Big, Megajocke
}

//UNDONE secure errors with popup windows
//FIXME check megajockes after MegaSplit collect
public class GameManager : MonoBehaviour
{
	#region Singleton
	public static GameManager Instance { get; private set; }

	void Awake()
	{
		if (Instance)
			Destroy(Instance);
		else
			Instance = this;
	}
	#endregion

	private const int RegularExplosiveId = 18;
	private const int IceBrickId = 87;//Id of brick type to which bricks hit by Space Djoels turn to

	[SerializeField]
#pragma warning disable CS0649 // Field 'GameManager.shutterAnimator' is never assigned to, and will always have its default value null
	private ShutterAnimationManager shutterAnimator;
#pragma warning restore CS0649 // Field 'GameManager.shutterAnimator' is never assigned to, and will always have its default value null
	[SerializeField]
#pragma warning disable CS0649 // Field 'GameManager.scoreText' is never assigned to, and will always have its default value null
	private Text scoreText;
#pragma warning restore CS0649 // Field 'GameManager.scoreText' is never assigned to, and will always have its default value null
	[SerializeField]
#pragma warning disable CS0649 // Field 'GameManager.paddleNumberText' is never assigned to, and will always have its default value null
	private Text paddleNumberText;
#pragma warning restore CS0649 // Field 'GameManager.paddleNumberText' is never assigned to, and will always have its default value null
	[SerializeField]
#pragma warning disable CS0649 // Field 'GameManager.hud' is never assigned to, and will always have its default value null
	private GameObject hud;
#pragma warning restore CS0649 // Field 'GameManager.hud' is never assigned to, and will always have its default value null
	[SerializeField]
#pragma warning disable CS0649 // Field 'GameManager.hudManager' is never assigned to, and will always have its default value null
	private HUDManager hudManager;
#pragma warning restore CS0649 // Field 'GameManager.hudManager' is never assigned to, and will always have its default value null
	[SerializeField]
	private ErrorMessage errorMessage;

	private LevelSet CurrentLevelSet;
	private int LevelIndex;

	private int numberOfBricksRequiredToComplete;
	private bool levelCompleted;

	private int paddles = 3;
	[SerializeField]
	private BallSize ballSize = BallSize.Normal;
	public BallSize BallSize {
		get => ballSize;
		private set
		{
			ballSize = value;
			if (value != BallSize.Normal)
				hudManager.AddOrUpdateBallSizeDisplay("BallSizeDisplay", value);
			else
				hudManager.RemoveDisplay("BallSizeDisplay");
		}
	}
	[SerializeField]
	private bool magnetPaddle;
	public bool MagnetPaddle
	{
		get => magnetPaddle;
		set
		{
			magnetPaddle = value;
			if (value == true)
				hudManager.AddDisplay("MagnetDisplay");
			else
				hudManager.RemoveDisplay("MagnetDisplay");
		}
	}
	[SerializeField]
	private bool explosiveBall;
	public bool ExplosiveBall
	{
		get => explosiveBall;
		set
		{
			explosiveBall = value;
			if (value == true)
				hudManager.AddDisplay("ExplosiveBallDisplay");
			else
				hudManager.RemoveDisplay("ExplosiveBallDisplay");
		}
	}
	[SerializeField]
	private bool penetratingBall;
	public bool PenetratingBall
	{
		get => penetratingBall;
		set
		{
			penetratingBall = value;
			if (value == true)
				hudManager.AddDisplay("PenetratingBallDisplay");
			else
				hudManager.RemoveDisplay("PenetratingBallDisplay");
		}
	}
	[SerializeField]
	private bool descendingBricks;
	public bool DescendingBricks
	{
		get => descendingBricks;
		set
		{
			descendingBricks = value;
			if (value == true)
				hudManager.AddDisplay("DescendingBricksDisplay");
			else
				hudManager.RemoveDisplay("DescendingBricksDisplay");
		}
	}
	[SerializeField]
	private int shooterLevel;
	public int ShooterLevel
	{
		get => shooterLevel;
		private set
		{
			shooterLevel = value;
			if (value > 0)
				hudManager.AddOrUpdateDisplay("ShooterDisplay", value);
			else
				hudManager.RemoveDisplay("ShooterDisplay");
		}
	}
	[SerializeField]
	private int protectiveBarrierLevel;
	public int ProtectiveBarrierLevel
	{
		get => protectiveBarrierLevel;
		private set
		{
			protectiveBarrierLevel = value;
			if (value > 0)
				hudManager.AddOrUpdateDisplay("ProtectiveBarrierDisplay", value);
			else
				hudManager.RemoveDisplay("ProtectiveBarrierDisplay");
		}
	}
	[SerializeField]
	private int megaMissiles;
	public int MegaMissiles
	{
		get => megaMissiles;
		private set
		{
			megaMissiles = value;
			if (value > 0)
				hudManager.AddOrUpdateDisplay("MegaMissileDisplay", value);
			else
				hudManager.RemoveDisplay("MegaMissileDisplay");
		}
	}
	[SerializeField]
	private int paddleLengthLevel = 1;
	public int PaddleLengthLevel
	{
		get => paddleLengthLevel;
		private set => paddleLengthLevel = value;
	}

	[SerializeField]
#pragma warning disable CS0649 // Field 'GameManager.ExplosionPrefab' is never assigned to, and will always have its default value null
	private GameObject explosionPrefab;
#pragma warning restore CS0649 // Field 'GameManager.ExplosionPrefab' is never assigned to, and will always have its default value null
	[SerializeField]
#pragma warning disable CS0649 // Field 'GameManager.FirstBrickPos' is never assigned to, and will always have its default value null
	private GameObject firstBrickPos;
#pragma warning restore CS0649 // Field 'GameManager.FirstBrickPos' is never assigned to, and will always have its default value null
	[SerializeField]
#pragma warning disable CS0649 // Field 'GameManager.BrickPrefab' is never assigned to, and will always have its default value null
	private GameObject brickPrefab;
#pragma warning restore CS0649 // Field 'GameManager.BrickPrefab' is never assigned to, and will always have its default value null

	private BrickType[] LevelSetBrickTypes;

	private Brick[] bricks = new Brick[LevelSet.ROWS * LevelSet.COLUMNS];

	private bool levelTestMode;
	private bool levelSetTestMode;

	internal BrickType SpaceDjoelBrickType => LevelSetBrickTypes[IceBrickId - 1];

	private LevelPersistentData levelPersistentData;

	private void Update()
	{
		if (shutterAnimator.Uncovered)
		{
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				if (hudManager.Paused)
					CoverSceneAndDoAction(ExitGame);
				else
					hudManager.Pause();
			}

			if (Input.GetKeyDown(KeyCode.H))
				hud.SetActive(!hud.activeSelf);
			if (Input.GetKeyDown(KeyCode.P))
				hudManager.Pause();
		}
	}

	private void FixedUpdate() => CheckIfLevelIsCompleted();

	private void ResetOnLoseLife()
	{
		BallManager.Instance.InitBall();
		BallSize = 0;
		MagnetPaddle = false;
		DescendingBricks = false;
		RemoveBallUpgrades();
		ShooterLevel = 0;
		ProtectiveBarrierLevel = 0;
		MegaMissiles = 0;
		paddleLengthLevel = 1;
		PowerUpManager.Instance.Reset();
		paddleNumberText.text = $"{paddles}";
		Paddle.Instance.SetLength(1);
		Paddle.Instance.ShooterActive = false;
		Paddle.Instance.ProtectiveBarrierActive = false;
		Paddle.Instance.MagnetActive = true;
		Paddle.Instance.MegaMissileActive = false;
	}

	private void ResetOnNextLevel()
	{
		BallManager.Instance.InitBall();
		DecreaseBall();
		Paddle.Instance.MagnetActive = true;
		PowerUpManager.Instance.Reset();
		DecreaseProtectiveBarrierLevel();
		DecreasePaddleLength();
	}

	public void LoadLevelSetExternalFiles()
	{
		List<string> errorList = new List<string>();
		SoundManager.Instance.UpdateLevelSetSounds(CurrentLevelSet, errorList);
		TextureManager.Instance.UpdateLevelSetTextures(CurrentLevelSet, errorList);
		MusicManager.Instance.UpdateLevelSetMusic(CurrentLevelSet, errorList);
		if (errorList.Count > 0)
		{
#if DEBUG
			foreach (string min in errorList)
			{
				Debug.LogError(min);
			}
#endif
			Logger.SaveLevelSetErrorLog($"{LoadedGameData.LevelSetDirectory}", $"{LoadedGameData.LevelSetFileName}", errorList);
			errorMessage.Show();
		}
	}

	public void LoadLevelExternalFiles()
	{
		List<string> errorList = new List<string>();
		SoundManager.Instance.UpdateLevelSounds(CurrentLevelSet.Levels[LevelIndex], errorList);
		TextureManager.Instance.UpdateLevelTextures(CurrentLevelSet.Levels[LevelIndex], errorList);
		MusicManager.Instance.UpdateLevelMusic(CurrentLevelSet.Levels[LevelIndex], errorList);
		if (errorList.Count > 0)
		{
			foreach (string min in errorList)
			{
				Debug.LogError($"File {min} is missing.");
			}
			Logger.SaveLevelErrorLog($"{LoadedGameData.LevelSetDirectory}", $"{LoadedGameData.LevelSetFileName}", LevelIndex, CurrentLevelSet.Levels[LevelIndex].LevelProperties.Name, errorList);
			errorMessage.Show();
		}
	}

	private void CheckIfIdsArePresent()
	{
		bool anyMissingId = false;
		IEnumerable<int> loadedBrickTypeIds = LevelSetBrickTypes.Select(bt => bt.Properties.Id);
		foreach (Level level in CurrentLevelSet.Levels)
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

	public void InitLevelSet(LevelSet levelSet, BrickType[] defaultBrickTypes, BrickType[] customBrickTypes, LevelPersistentData levelPersistentData)
	{
		CurrentLevelSet = levelSet;
		LoadLevelSetExternalFiles();

		this.levelPersistentData = levelPersistentData;
		LevelIndex = levelPersistentData.LevelNum;
		paddles = levelPersistentData.Paddles;
		paddleNumberText.text = paddles.ToString();
		AddToScore(levelPersistentData.CurrentScore);

		PrepareBrickTypes(defaultBrickTypes, customBrickTypes);

		InitLevel();
	}

	public void InitLevelSet(LevelSet levelSet, BrickType[] defaultBrickTypes, BrickType[] customBrickTypes, int startLevelNum)
	{
		CurrentLevelSet = levelSet;
		LevelIndex = startLevelNum;
		LoadLevelSetExternalFiles();

		PrepareBrickTypes(defaultBrickTypes, customBrickTypes);

		InitLevel();
	}

	private void PrepareBrickTypes(BrickType[] defaultBrickTypes, BrickType[] customBrickTypes)
	{
		LevelSetBrickTypes = defaultBrickTypes.Concat(customBrickTypes).ToArray();
		CheckIfIdsArePresent();
		ParticleManager.Instance.CreateBrickParticles(LevelSetBrickTypes);
	}

	private void InitLevel()
	{
		//TextureManager.Instance.LoadLevelTextures(LoadedGameData.LevelSetFileName, CurrentLevelSet.Levels[LevelIndex]);
		LoadLevelExternalFiles();
		hudManager.UpdateAndDisplayLevelNameForAMinute(LevelIndex + 1, CurrentLevelSet.Levels[LevelIndex].LevelProperties.Name);
		for (int brickY = 0; brickY < LevelSet.ROWS; brickY++)
		{
			for (int brickX = 0; brickX < LevelSet.COLUMNS; brickX++)
			{
				int idOfBrickInCoordinates = CurrentLevelSet.Levels[LevelIndex].Bricks[brickY, brickX].BrickId;
				if (idOfBrickInCoordinates != 0)
					GenerateBrick(idOfBrickInCoordinates, brickX, brickY);
			}
		}
		SetBricksRequiredToCompleteNumber();
	}

	private void GenerateBrick(int brickTypeId, int brickX, int brickY, bool tryHide = true, bool spaceDjoelBrick = false)
	{
		BrickType brickType = GetBrickTypeById(brickTypeId);
		Vector3 position = new Vector3(firstBrickPos.transform.position.x + (brickX * BrickType.BrickUnityWidth), firstBrickPos.transform.position.y - (brickY * BrickType.BrickUnityHeight), 2);
		GameObject brick = Instantiate(brickPrefab, position: position, Quaternion.identity);
		brick.GetComponent<SpriteRenderer>().sprite = brickType.FirstSprite;
		Brick brickScript = brick.GetComponent<Brick>();
		brickScript.brickType = brickType;
		brickScript.x = brickX;
		brickScript.y = brickY;
		if (tryHide)
			brickScript.TryHide();
		else if (spaceDjoelBrick)
			brickScript.StartIcyFading();
		brick.name = $"Brick nr {GetBrickIndex(brickX, brickY)}";
		bricks[GetBrickIndex(brickX, brickY)] = brickScript;
	}

	private void DecreasePaddlePowerUpLevels()
	{
		DecreaseShooterLevel();
		DecreaseMegaMissiles();
	}

	internal Brick GetBrickByCoordinates(int x, int y) => bricks[GetBrickIndex(x, y)];

	internal BrickType GetBrickTypeById(int newBrickId) => LevelSetBrickTypes.First(b => b.Properties.Id == newBrickId);

	private int GetBrickIndex(int x, int y) => x + (y * LevelSet.COLUMNS);

	//points can be negative so Mathf.Max invocation is necessary
	internal void AddToScore(int points) => scoreText.text = $"{Mathf.Max(int.Parse(scoreText.text) + points, 0)}";

	public void IncreaseBall()
	{
		if (BallSize != BallSize.Megajocke)
			BallSize++;
		BallManager.Instance.UpdateSizeOfAllBalls();
	}

	public void DecreaseBall()
	{
		if (BallSize != BallSize.Normal)
			BallSize--;
		RemoveBallUpgrades();
		BallManager.Instance.UpdateSizeOfAllBalls();
	}

	private void RemoveBallUpgrades()
	{
		ExplosiveBall = false;
		PenetratingBall = false;
		BallManager.Instance.RemoveParticlesFromBalls();
	}

	internal void IncreasePaddleLength()
	{
		if (paddleLengthLevel < 6)
		{
			paddleLengthLevel++;
			Paddle.Instance.SetLength(paddleLengthLevel);
		}
	}

	internal void DecreasePaddleLength()
	{
		if (paddleLengthLevel > 1)
		{
			paddleLengthLevel--;
			Paddle.Instance.SetLength(paddleLengthLevel);
		}
		DescendingBricks = false;
		MagnetPaddle = false;
		BallManager.Instance.UpdateMagnetVisibility();
		DecreasePaddlePowerUpLevels();
	}

	internal void ActivateMagnet() => MagnetPaddle = Paddle.Instance.MagnetActive = true;

	public void MakeExplosion(int radius, Vector3 explosionPosition, Vector2 spriteBounds)
	{
		GameObject explosion = Instantiate(explosionPrefab, explosionPosition, Quaternion.identity);
		explosion.GetComponent<BoxCollider2D>().size = new Vector2(spriteBounds.x * (radius * 2 + 1) - 0.1f, spriteBounds.y * (radius * 2 + 1) - 0.1f);
		///explosion.GetComponent<Explosion>().Activate();
	}

	public void DetonateOrChangeBrick(int oldBrickTypeId, int newBrickTypeId, DetonationRange detonationRange)
	{
		List<Brick> bricks = this.bricks.Where(b => b?.brickType.Properties.Id == oldBrickTypeId).ToList();
		if (bricks.Count > 0)
		{
			SoundManager.Instance.PlaySfx("Bang");
			switch (detonationRange)
			{
				case DetonationRange.One:
					int chosenIndex = UnityEngine.Random.Range(0, bricks.Count);
					Brick chosenBrick = bricks[chosenIndex];
					//BONUS make internal function with it
					if (newBrickTypeId == 0)//If it's detonating brick
					{
						chosenBrick.TryIncreasePowerUpField();
						chosenBrick.Break(chosenBrick.BrickProperties.Points, null, true);
					}
					else
						chosenBrick.ChangeBrickType(newBrickTypeId);
					break;
				case DetonationRange.All:
					foreach (Brick brick in bricks)
					{
						if (newBrickTypeId == 0)//If it's detonating brick
						{
							brick.TryIncreasePowerUpField();
							brick.Break(brick.BrickProperties.Points, null, true);
						}
						else
							brick.ChangeBrickType(newBrickTypeId);
					}
					break;
				default:
					break;
			}
		}
	}

	public void DestroyBrickPointedByFuseType(Brick brickScript)
	{
		int brickX = -1;
		int brickY = -1;
		switch (brickScript.brickType.Properties.FuseDirection)
		{
			case Direction.Up when brickScript.y > 0:
				brickX = brickScript.x;
				brickY = brickScript.y - 1;
				break;
			case Direction.Right when brickScript.x < LevelSet.COLUMNS + 1:
				brickX = brickScript.x + 1;
				brickY = brickScript.y;
				break;
			case Direction.Down when brickScript.y < LevelSet.ROWS + 1:
				brickX = brickScript.x;
				brickY = brickScript.y + 1;
				break;
			case Direction.Left when brickScript.x > 0:
				brickX = brickScript.x - 1;
				brickY = brickScript.y;
				break;
		}
		if (brickX >= 0 && brickY >= 0)
		{
			Brick pointedBrick = bricks[GetBrickIndex(brickX, brickY)];
			if (pointedBrick?.brickType.Properties.ExplosionResistant == false)
				pointedBrick.Break(pointedBrick.brickType.Properties.Points / 2, force: true);
		}
	}

	public void EraseBricks()
	{
		for (int i = 0; i < bricks.Length; i++)
			if (bricks[i])
				DisposeBrick(bricks[i]);
	}

	public void CleanLevel()
	{
		PowerUp[] powerUpsToClean = FindObjectsOfType<PowerUp>();
		Ball[] ballsToClean = FindObjectsOfType<Ball>();
		Bullet[] bulletsToClean = FindObjectsOfType<Bullet>();
		SpaceDjoel[] djoelsToClean = FindObjectsOfType<SpaceDjoel>();
		ParticleSystem[] particleSystems = FindObjectsOfType<ParticleSystem>().Where(p => p.gameObject.tag == "particlesToClean").ToArray();
		foreach (var powerUp in powerUpsToClean)
			Destroy(powerUp.gameObject);
		foreach (var ball in ballsToClean)
			Destroy(ball.gameObject);
		foreach (var bullet in bulletsToClean)
			Destroy(bullet.gameObject);
		foreach (var djoel in djoelsToClean)
			Destroy(djoel.gameObject);
		foreach (var particleSystem in particleSystems)
			Destroy(particleSystem.gameObject);
		SpaceDjoelManager.Instance.ResetDjoels();
	}

	public void EraseExplosions()
	{
		Explosion[] explosionsToErase = FindObjectsOfType<Explosion>();
		MegaExplosion[] megaExplosionsToErase = FindObjectsOfType<MegaExplosion>();
		foreach (var explosion in explosionsToErase)
			Destroy(explosion.gameObject);
		foreach (var megaExplosionToErase in megaExplosionsToErase)
			Destroy(megaExplosionToErase.gameObject);
	}

	public void DisposeBrick(Brick brick)
	{
		bricks[brick.x + (brick.y * LevelSet.COLUMNS)] = null;
		Destroy(brick.gameObject);
		//Debug.Log(bricks.Count);
	}

	public void DecrementRequiredBricks()
	{
		numberOfBricksRequiredToComplete--;
		Debug.Log($"Bricks left: {numberOfBricksRequiredToComplete}");
	}

	public void CheckIfLevelIsCompleted()
	{
		if (numberOfBricksRequiredToComplete <= 0 && !levelCompleted)
		{
			levelCompleted = true;
			CoverSceneAndDoAction(CleanAfterFinishingLevel, "Win");
		}
	}

	private void SetBricksRequiredToCompleteNumber()
	{
		numberOfBricksRequiredToComplete = bricks.Count(b => ((b?.Hidden == false && b?.BrickProperties.RequiredToComplete == true) || (b?.Hidden == true && b?.BrickProperties.RequiredToCompleteWhenHidden == true)) && b?.SpaceDjoelBrick == false);
	}

	public void IncrementRequiredBricks()
	{
		numberOfBricksRequiredToComplete++;
		Debug.Log($"Bricks left: {numberOfBricksRequiredToComplete}");
	}

	public void IncreaseShooterLevel()
	{
		ShooterLevel++;
		Paddle.Instance.ShooterActive = true;
	}

	public void DecreaseShooterLevel()
	{
		ShooterLevel = Mathf.Max(ShooterLevel - 1, 0);
		if (ShooterLevel == 0)
			Paddle.Instance.ShooterActive = false;
	}

	public void IncreaseProtectiveBarrierLevel()
	{
		ProtectiveBarrierLevel++;
		Paddle.Instance.ProtectiveBarrierActive = true;
	}

	public void DecreaseProtectiveBarrierLevel()
	{
		ProtectiveBarrierLevel = Mathf.Max(ProtectiveBarrierLevel - 1, 0);
		if (ProtectiveBarrierLevel == 0)
			Paddle.Instance.ProtectiveBarrierActive = false;
	}

	public void IncreaseMegaMissiles()
	{
		MegaMissiles++;
		Paddle.Instance.MegaMissileActive = true;
	}

	public void DecreaseMegaMissiles()
	{
		MegaMissiles = Mathf.Max(MegaMissiles - 1, 0);
		if (MegaMissiles == 0)
			Paddle.Instance.MegaMissileActive = false;
	}

	public void DescendBrickRows()
	{
		for (int i = bricks.Length - 1; i >= 0; i--)
		{
			Brick brick = bricks[i];
			if (brick && brick.brickType.Properties.IsDescending)
			{
				int yIndexInIteration = i / LevelSet.COLUMNS;
				brick.TryMoveBlockDown(1);
				if (brick.y != yIndexInIteration)
				{
					int indexOfBrickBelow = i + LevelSet.COLUMNS;
					bricks[indexOfBrickBelow] = bricks[i];
					bricks[i] = null;
				}
			}
		}
	}

	private void CoverSceneAndDoAction(Func<IEnumerator> action, string audioClipKey = null)
	{
		SoundManager.Instance.PlaySfx(audioClipKey);
		shutterAnimator.Cover(action());
	}

	public Brick[] GetTeleportOutputs(int[] teleportExitIds) => bricks.Where(b => b != null && teleportExitIds.Contains(b.brickType.Properties.Id)).ToArray();

	public bool TryTeleportBrickBuster(GameObject brickBuster, int[] teleportExitIds, Vector2 collisionNormal)
	{
		Brick[] teleportExits = GetTeleportOutputs(teleportExitIds);
		if (teleportExits.Length > 0)
		{
			Brick finalTeleportOutput = teleportExits[UnityEngine.Random.Range(0, teleportExits.Length)];
			TeleportBrickBuster(finalTeleportOutput, brickBuster, collisionNormal);
			return true;
		}
		return false;
	}

	public void GenerateBallsFromTeleporters(GameObject ball, int[] teleportExitIds, Vector2 collisionNormal)
	{
		Brick[] teleporterOutputs = GetTeleportOutputs(teleportExitIds);
		for (int i = 0; i < teleporterOutputs.Length && BallManager.Instance.BallNumber < BallManager.maxBallNumber; i++)
		{
			GameObject newBall = BallManager.Instance.CreateNewBall();
			TeleportBrickBuster(teleporterOutputs[i], newBall, collisionNormal);
		}
	}

	private void TeleportBrickBuster(Brick teleporter, GameObject brickBusterObject, Vector2 collisionNormal)
	{
		BoxCollider2D teleportCollider = teleporter.GetComponent<BoxCollider2D>();
		BoxCollider2D ballCollider = brickBusterObject.GetComponent<BoxCollider2D>();
		SoundManager.Instance.PlaySfx("Teleport");
		IBrickBuster brickBuster = brickBusterObject.GetComponent<IBrickBuster>();

		if (brickBuster != null)
		{
			if (brickBuster is Ball)
				brickBusterObject.GetComponent<Ball>().FinishThrust();
			float minAngle = 20.0f;
			float maxAngle = 80.0f;
			float angleRange = maxAngle - minAngle;
			float ballSpeedRange = BallManager.maxBallSpeed - BallManager.minBallSpeed;
			float angleFactor = (brickBuster.CurrentVelocity.magnitude - BallManager.minBallSpeed) / ballSpeedRange;
			float angle = maxAngle - angleRange * angleFactor;

			float brickBusterZ = brickBusterObject.transform.position.z;
			float magnitude = brickBuster.CurrentVelocity.magnitude > 0 ? brickBuster.CurrentVelocity.magnitude : BallManager.minBallSpeed;
			//Hit from bottom
			if (brickBuster is Ball || brickBuster is Bullet)
			{
				if (collisionNormal == Vector2.down)
				{
					brickBusterObject.transform.position = new Vector3(teleportCollider.bounds.center.x, teleportCollider.bounds.min.y - ballCollider.bounds.extents.y - 0.03f, brickBusterZ);
					brickBuster.CurrentVelocity = PhysicsHelper.GetAngledVelocity(270.0f + angle) * magnitude;
				}
				//Hit from left
				else if (collisionNormal == Vector2.left)
				{
					brickBusterObject.transform.position = new Vector3(teleportCollider.bounds.min.x - ballCollider.bounds.extents.x - 0.03f, teleportCollider.bounds.center.y, brickBusterZ);
					brickBuster.CurrentVelocity = PhysicsHelper.GetAngledVelocity(0 - angle) * magnitude;
				}
				//Hit from top
				else if (collisionNormal == Vector2.up)
				{
					brickBusterObject.transform.position = new Vector3(teleportCollider.bounds.center.x, teleportCollider.bounds.max.y + ballCollider.bounds.extents.y + 0.03f, brickBusterZ);
					brickBuster.CurrentVelocity = PhysicsHelper.GetAngledVelocity(90.0f + angle) * magnitude;
				}
				//Hit from right
				else if (collisionNormal == Vector2.right)
				{
					brickBusterObject.transform.position = new Vector3(teleportCollider.bounds.max.x + ballCollider.bounds.extents.x + 0.03f, teleportCollider.bounds.center.y, brickBusterZ);
					brickBuster.CurrentVelocity = PhysicsHelper.GetAngledVelocity(180.0f - angle) * magnitude;
				}
			}
			else if (brickBuster is SpaceDjoel)
			{
				if (collisionNormal == Vector2.down)
				{
					brickBusterObject.transform.position = new Vector3(teleportCollider.bounds.center.x, teleportCollider.bounds.max.y + ballCollider.bounds.extents.y + 0.07f, brickBusterZ);
				}
				//Hit from left
				else if (collisionNormal == Vector2.left)
				{
					brickBusterObject.transform.position = new Vector3(teleportCollider.bounds.max.x + ballCollider.bounds.extents.x + 0.07f, teleportCollider.bounds.center.y, brickBusterZ);
				}
				//Hit from top
				else if (collisionNormal == Vector2.up)
				{
					brickBusterObject.transform.position = new Vector3(teleportCollider.bounds.center.x, teleportCollider.bounds.min.y - ballCollider.bounds.extents.y - 0.07f, brickBusterZ);
				}
				//Hit from right
				else if (collisionNormal == Vector2.right)
				{
					brickBusterObject.transform.position = new Vector3(teleportCollider.bounds.min.x - ballCollider.bounds.extents.x - 0.07f, teleportCollider.bounds.center.y, brickBusterZ);
				}
			}
		}
	}

	internal void AddExtraPaddle()
	{
		paddles++;
		paddleNumberText.text = $"{paddles}";
	}

	internal void CheckForLosePaddle()
	{
		if (BallManager.Instance.BallNumber == 0 && SpaceDjoelManager.Instance.SpaceDjoelNumber == 0)
			LosePaddle();
	}

	internal void LosePaddle()
	{
		paddles--;
		CoverSceneAndDoAction(CleanAfterLoseLife, "Lose Paddle");
	}

	#region Multiplying
	private struct TemporaryLayerData
	{
		public int Id { get; set; }
		public bool Hidden { get; set; }
		public bool SpaceDjoelBrick { get; set; }

		public TemporaryLayerData(int id, bool hidden = false, bool spaceDjoelBrick = false)
		{
			Id = id;
			Hidden = hidden;
			SpaceDjoelBrick = spaceDjoelBrick;
		}

		/*public static bool operator ==(TemporaryLayerData temporaryLayerData, Brick brick)
		{
			return brick.BrickProperties.Id == temporaryLayerData.Id && brick.Hidden == temporaryLayerData.Hidden && brick.Hidden == temporaryLayerData.SpaceDjoelBrick;
		}

		public static bool operator !=(TemporaryLayerData temporaryLayerData, Brick brick)
		{
			return brick.BrickProperties.Id != temporaryLayerData.Id || brick.Hidden != temporaryLayerData.Hidden || brick.Hidden != temporaryLayerData.SpaceDjoelBrick;
		}

		public override bool Equals(object obj)
		{
			if (!(obj is TemporaryLayerData))
			{
				return false;
			}

			var data = (TemporaryLayerData)obj;
			return Id == data.Id &&
				   Hidden == data.Hidden &&
				   SpaceDjoelBrick == data.SpaceDjoelBrick;
		}

		public override int GetHashCode()
		{
			var hashCode = 1008773611;
			hashCode = hashCode * -1521134295 + Id.GetHashCode();
			hashCode = hashCode * -1521134295 + Hidden.GetHashCode();
			hashCode = hashCode * -1521134295 + SpaceDjoelBrick.GetHashCode();
			return hashCode;
		}*/
	}

	internal void MultiplyRegulars()
	{
		TemporaryLayerData[] tmpBrickLayer = bricks.Select(b =>
		{
			if (b && !b.Broken)
				return new TemporaryLayerData(b.BrickProperties.Id, b.Hidden, b.SpaceDjoelBrick);
			else
				return new TemporaryLayerData(0);
		}).ToArray();
		for (int y = 0; y < LevelSet.ROWS; y++)
		{
			for (int x = 0; x < LevelSet.COLUMNS; x++)
			{
				bool hidden = bricks[GetBrickIndex(x, y)]?.Hidden ?? false;
				bool spaceDjoelBrick = bricks[GetBrickIndex(x, y)]?.SpaceDjoelBrick ?? false;
				BrickProperties brickProperties = bricks[GetBrickIndex(x, y)]?.BrickProperties;
				if (brickProperties?.IsRegular == true)
				{
					if (y != 0 && bricks[GetBrickIndex(x, y - 1)]?.BrickProperties.CanBeOverridenByStandardMultiplier != false)
						tmpBrickLayer[GetBrickIndex(x, y - 1)] = new TemporaryLayerData(brickProperties.Id, hidden, spaceDjoelBrick);
					if (x != LevelSet.COLUMNS - 1 && bricks[GetBrickIndex(x + 1, y)]?.BrickProperties.CanBeOverridenByStandardMultiplier != false)
						tmpBrickLayer[GetBrickIndex(x + 1, y)] = new TemporaryLayerData(brickProperties.Id, hidden, spaceDjoelBrick);
					if (y != LevelSet.ROWS - 1 && bricks[GetBrickIndex(x, y + 1)]?.BrickProperties.CanBeOverridenByStandardMultiplier != false)
						tmpBrickLayer[GetBrickIndex(x, y + 1)] = new TemporaryLayerData(brickProperties.Id, hidden, spaceDjoelBrick);
					if (x != 0 && bricks[GetBrickIndex(x - 1, y)]?.BrickProperties.CanBeOverridenByStandardMultiplier != false)
						tmpBrickLayer[GetBrickIndex(x - 1, y)] = new TemporaryLayerData(brickProperties.Id, hidden, spaceDjoelBrick);
				}
			}
		}
		RenderTemporaryBrickLayer(tmpBrickLayer);
	}

	internal void MultiplyExplosives()
	{
		TemporaryLayerData[] tmpBrickLayer = bricks.Select(b => new TemporaryLayerData() { Id = b?.BrickProperties.Id ?? 0, Hidden = b?.Hidden ?? false }).ToArray();
		int explosives = 0;//Number of explosives needed to determine if additional explosives should not be added.
		for (int y = 0; y < LevelSet.ROWS; y++)
		{
			for (int x = 0; x < LevelSet.COLUMNS; x++)
			{
				bool hidden = bricks[GetBrickIndex(x, y)]?.Hidden ?? false;
				bool spaceDjoelBrick = bricks[GetBrickIndex(x, y)]?.SpaceDjoelBrick ?? false;
				BrickProperties brickProperties = bricks[GetBrickIndex(x, y)]?.BrickProperties;
				if (brickProperties?.IsExplosive == true && brickProperties.CanBeMultipliedByExplosiveMultiplier)
				{
					if (y != 0 && bricks[GetBrickIndex(x, y - 1)]?.BrickProperties.CanBeOverridenByExplosiveMultiplier != false)
						tmpBrickLayer[GetBrickIndex(x, y - 1)] = new TemporaryLayerData(brickProperties.Id, hidden, spaceDjoelBrick);
					if (x != LevelSet.COLUMNS - 1 && bricks[GetBrickIndex(x + 1, y)]?.BrickProperties.CanBeOverridenByExplosiveMultiplier != false)
						tmpBrickLayer[GetBrickIndex(x + 1, y)] = new TemporaryLayerData(brickProperties.Id, hidden, spaceDjoelBrick);
					if (y != LevelSet.ROWS - 1 && bricks[GetBrickIndex(x, y + 1)]?.BrickProperties.CanBeOverridenByExplosiveMultiplier != false)
						tmpBrickLayer[GetBrickIndex(x, y + 1)] = new TemporaryLayerData(brickProperties.Id, hidden, spaceDjoelBrick);
					if (x != 0 && bricks[GetBrickIndex(x - 1, y)]?.BrickProperties.CanBeOverridenByExplosiveMultiplier != false)
						tmpBrickLayer[GetBrickIndex(x - 1, y)] = new TemporaryLayerData(brickProperties.Id, hidden, spaceDjoelBrick);
					explosives++;
				}
			}
		}
		//Debug.Log($"Explosives: {explosives}");
		for (int additionalExplosivePluses = 10 - explosives; additionalExplosivePluses > 0; additionalExplosivePluses--)
		{
			int x = UnityEngine.Random.Range(0, LevelSet.COLUMNS);
			int y = UnityEngine.Random.Range(0, LevelSet.ROWS);
			tmpBrickLayer[GetBrickIndex(x, y)] = new TemporaryLayerData(RegularExplosiveId);
			if (y != 0 && bricks[GetBrickIndex(x, y - 1)]?.BrickProperties.CanBeOverridenByExplosiveMultiplier != false)
				tmpBrickLayer[GetBrickIndex(x, y - 1)] = new TemporaryLayerData(RegularExplosiveId);
			if (x != LevelSet.COLUMNS - 1 && bricks[GetBrickIndex(x + 1, y)]?.BrickProperties.CanBeOverridenByExplosiveMultiplier != false)
				tmpBrickLayer[GetBrickIndex(x + 1, y)] = new TemporaryLayerData(RegularExplosiveId);
			if (y != LevelSet.ROWS - 1 && bricks[GetBrickIndex(x, y + 1)]?.BrickProperties.CanBeOverridenByExplosiveMultiplier != false)
				tmpBrickLayer[GetBrickIndex(x, y + 1)] = new TemporaryLayerData(RegularExplosiveId);
			if (x != 0 && bricks[GetBrickIndex(x - 1, y)]?.BrickProperties.CanBeOverridenByExplosiveMultiplier != false)
				tmpBrickLayer[GetBrickIndex(x - 1, y)] = new TemporaryLayerData(RegularExplosiveId);
		}
		RenderTemporaryBrickLayer(tmpBrickLayer);
	}

	private void RenderTemporaryBrickLayer(TemporaryLayerData[] tmpBrickLayer)
	{
		bool change = false;
		for (int y = 0; y < LevelSet.ROWS; y++)
			for (int x = 0; x < LevelSet.COLUMNS; x++)
			{
				int index = GetBrickIndex(x, y);
				TemporaryLayerData tld = tmpBrickLayer[index];
				//BONUS try to optimize it with condition similar to commented
				if (tld.Id != 0)// && (tld.Id != bricks[index]?.BrickProperties.Id || tld.Hidden != bricks[index]?.Hidden || tld.SpaceDjoelBrick || bricks[index]?.Broken == true))
				{
					if (bricks[index])
					{
						//bricks[index].transform.position = new Vector3(bricks[index].transform.position.x, bricks[index].transform.position.y, 40);
						bricks[index].StopAllCoroutines();
						Destroy(bricks[index].gameObject);
					}
					GenerateBrick(tld.Id, x, y, tld.Hidden, tld.SpaceDjoelBrick);
					change = true;
				}
			}
		if (change)
			SetBricksRequiredToCompleteNumber();
	}
	#endregion

	internal void SaveGame()
	{
		levelPersistentData.Paddles = paddles;
		levelPersistentData.CurrentScore = int.Parse(scoreText.text);
		levelPersistentData.LevelNum = LevelIndex;
		levelPersistentData.Save();
	}

	internal void DeleteSave()
	{
		if (System.IO.File.Exists($"Saves/{LoadedGameData.LevelSetFileName}.sav"))
			System.IO.File.Delete($"Saves/{LoadedGameData.LevelSetFileName}.sav");
	}

	IEnumerator CleanAfterLoseLife()
	{
		yield return new WaitUntil(() => shutterAnimator.Covered);
		CleanLevel();
		if (paddles >= 0)
		{
			ResetOnLoseLife();
			shutterAnimator.Uncover();
		}
		else
		{
			if (LoadedGameData.TestMode == TestMode.None)
			{
				DeleteSave();
				GoToLeaderboard(won: false);
			}
			else
				Application.Quit();
		}
	}

	IEnumerator CleanAfterFinishingLevel()
	{
		yield return new WaitUntil(() => shutterAnimator.Covered);
		CleanLevel();
		EraseBricks();
		EraseExplosions();
		LevelIndex++;
		if (LevelIndex < CurrentLevelSet.Levels.Count)
		{
			if (LoadedGameData.TestMode == TestMode.None)
				SaveGame();
			if (LoadedGameData.TestMode == TestMode.TestOneLevel)
				Application.Quit();
			ResetOnNextLevel();
			InitLevel();
			levelCompleted = false;
			shutterAnimator.Uncover();
		}
		else
		{
			Debug.Log("The End.");
			if (LoadedGameData.TestMode == TestMode.None)
			{
				DeleteSave();
				GoToLeaderboard(won: true);
			}
			else
				Application.Quit();
		}
	}

	private void GoToLeaderboard(bool won)
	{
		EndGameData.HighScoreChange = true;
		EndGameData.LastLevel = LevelIndex;
		EndGameData.Score = int.Parse(scoreText.text);
		EndGameData.Won = won;
		SceneManager.LoadScene("Leaderboard");
	}

	private IEnumerator ExitGame()
	{
		if (hudManager.Paused)
			hudManager.Pause();
		yield return new WaitUntil(() => shutterAnimator.Covered);
		if (LoadedGameData.TestMode == TestMode.None)
			SceneManager.LoadScene("Level Set List");
		else
			Application.Quit();
	}
}
