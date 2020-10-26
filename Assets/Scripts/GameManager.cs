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

//TODO secure errors with popup windows
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

	private const int RegularExplosiveId = 18;
	#endregion

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

	private LevelSet CurrentLevelSet;
	private int LevelIndex;

	private int NumberOfBricksRequiredToComplete;

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
	private GameObject ExplosionPrefab;
#pragma warning restore CS0649 // Field 'GameManager.ExplosionPrefab' is never assigned to, and will always have its default value null
	[SerializeField]
#pragma warning disable CS0649 // Field 'GameManager.FirstBrickPos' is never assigned to, and will always have its default value null
	private GameObject FirstBrickPos;
#pragma warning restore CS0649 // Field 'GameManager.FirstBrickPos' is never assigned to, and will always have its default value null
	[SerializeField]
#pragma warning disable CS0649 // Field 'GameManager.BrickPrefab' is never assigned to, and will always have its default value null
	private GameObject BrickPrefab;
#pragma warning restore CS0649 // Field 'GameManager.BrickPrefab' is never assigned to, and will always have its default value null

	private BrickType[] LevelSetBrickTypes;

	private Brick[] bricks = new Brick[LevelSet.ROWS * LevelSet.COLUMNS];

	private bool levelTestMode;
	private bool levelSetTestMode;

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
			CoverSceneAndDoAction(ExitGame);
		if (Input.GetKeyDown(KeyCode.H))
			hud.SetActive(!hud.activeSelf);
		if (Input.GetKeyDown(KeyCode.P))//TODO do pause on ESC
			hudManager.Pause();
	}

	private void ResetOnLoseLife()
	{
		BallManager.Instance.InitBall();
		BallSize = 0;
		MagnetPaddle = false;
		ExplosiveBall = false;
		PenetratingBall = false;
		DescendingBricks = false;
		ShooterLevel = 0;
		ProtectiveBarrierLevel = 0;
		paddleLengthLevel = 1;
		PowerUpManager.Instance.Reset();
		paddleNumberText.text = $"{paddles}";
		Paddle.Instance.SetLength(1);
		Paddle.Instance.ShooterActive = false;
		Paddle.Instance.ProtectiveBarrierActive = false;
		Paddle.Instance.MagnetActive = true;
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

	public void InitLevel(LevelSet levelSet, BrickType[] defaultBrickTypes, BrickType[] customBrickTypes, LevelPersistentData levelPersistentData = null)
	{
		CurrentLevelSet = levelSet;
		if (levelPersistentData != null)
		{
			LevelIndex = levelPersistentData.LevelNum;
			paddles = levelPersistentData.Paddles;
			paddleNumberText.text = paddles.ToString();
			AddToScore(levelPersistentData.CurrentScore);
		}

		LevelSetBrickTypes = defaultBrickTypes.Concat(customBrickTypes).ToArray();

		InitLevel();
	}

	public void InitLevel(LevelSet levelSet, BrickType[] defaultBrickTypes, BrickType[] customBrickTypes, int levelNum)
	{
		CurrentLevelSet = levelSet;
		LevelIndex = levelNum;

		LevelSetBrickTypes = defaultBrickTypes.Concat(customBrickTypes).ToArray();

		InitLevel();
	}

	private void InitLevel()
	{
		TextureManager.Instance.LoadGameTextures(LoadedGameData.LevelSetFileName, CurrentLevelSet.Levels[LevelIndex]);
		hudManager.UpdateAndDisplayLevelNameForAMinute(LevelIndex + 1, CurrentLevelSet.Levels[LevelIndex].LevelProperties.Name);
		float brickWidth = LevelSetBrickTypes[0].BrickUnityWidth;
		float brickHeight = LevelSetBrickTypes[0].BrickUnityHeight;
		for (int brickY = 0; brickY < LevelSet.ROWS; brickY++)
		{
			for (int brickX = 0; brickX < LevelSet.COLUMNS; brickX++)
			{
				int idOfBrickInCoordinates = CurrentLevelSet.Levels[LevelIndex].Bricks[brickY, brickX].BrickId;
				if (idOfBrickInCoordinates != 0)
					GenerateBrick(idOfBrickInCoordinates, brickWidth, brickHeight, brickX, brickY);
			}
		}
		SetBricksRequiredToCompleteNumber();
	}

	private void GenerateBrick(int brickTypeId, float brickWidth, float brickHeight, int brickX, int brickY)
	{
		BrickType brickType = LevelSetBrickTypes.First(bt => bt.Properties.Id == brickTypeId);
		Vector3 position = new Vector3(FirstBrickPos.transform.position.x + (brickX * brickWidth), FirstBrickPos.transform.position.y - (brickY * brickHeight), 2);
		GameObject brick = Instantiate(BrickPrefab, position: position, Quaternion.identity);
		brick.GetComponent<SpriteRenderer>().sprite = brickType.Sprites[0];
		Brick brickScript = brick.GetComponent<Brick>();
		brickScript.brickType = brickType;
		brickScript.x = brickX;
		brickScript.y = brickY;
		brick.name = $"Brick nr {brickX + (brickY * LevelSet.COLUMNS)}";
		bricks[brickX + (brickY * LevelSet.COLUMNS)] = brickScript;
	}

	private void DecreasePaddlePowerUpLevels()
	{
		DecreaseShooterLevel();
		DecreaseMegaMissiles();
	}

	internal Brick GetBrickByCoordinates(int x, int y) => bricks[x + (y * LevelSet.COLUMNS)];

	internal BrickType GetBrickById(int newBrickId) => LevelSetBrickTypes.First(b => b.Properties.Id == newBrickId);

	internal void AddToScore(int score) => scoreText.text = $"{int.Parse(scoreText.text) + score}";

	public void IncreaseBall()
	{
		if (BallSize != BallSize.Megajocke)
			BallSize++;
		BallManager.Instance.UpdateSizeOfAllStuckToPaddleBalls();
	}

	public void DecreaseBall()
	{
		if (BallSize != BallSize.Normal)
			BallSize--;
		ExplosiveBall = false;
		PenetratingBall = false;
		BallManager.Instance.UpdateSizeOfAllStuckToPaddleBalls();
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
		GameObject explosion = Instantiate(ExplosionPrefab, explosionPosition, Quaternion.identity);
		explosion.GetComponent<BoxCollider2D>().size = new Vector2(spriteBounds.x * (radius * 2 + 1) - 0.1f, spriteBounds.y * (radius * 2 + 1) - 0.1f);
	}

	public void DetonateBrick(int detonateId)
	{
		LevelSoundLibrary.Instance.PlaySfx(LevelSoundLibrary.Instance.bang);
		List<Brick> bricks = this.bricks.Where(b => b.GetComponent<Brick>().brickType.Properties.Id == detonateId).ToList();
		if (bricks.Count > 0)
		{
			LevelSoundLibrary.Instance.PlaySfx(LevelSoundLibrary.Instance.bang);
			int chosenIndex = UnityEngine.Random.Range(0, bricks.Count);
			Brick chosenBrick = bricks[chosenIndex];
			chosenBrick.Break(chosenBrick.brickType.Properties.Points, true);
		}
	}

	public void DestroyBrickPointedByFuseType(Brick brickScript)
	{
		int brickX = -1;
		int brickY = -1;
		switch (brickScript.brickType.Properties.FuseDirection)
		{
			case Direction.Up:
				brickX = brickScript.x;
				brickY = brickScript.y - 1;
				break;
			case Direction.Right:
				brickX = brickScript.x + 1;
				brickY = brickScript.y;
				break;
			case Direction.Down:
				brickX = brickScript.x;
				brickY = brickScript.y + 1;
				break;
			case Direction.Left:
				brickX = brickScript.x - 1;
				brickY = brickScript.y;
				break;
		}
		Brick pointedBrick = bricks[brickX + (brickY * LevelSet.COLUMNS)];
		if (pointedBrick?.brickType.Properties.ExplosionResistant == false)
			pointedBrick.Break(pointedBrick.brickType.Properties.Points / 2, true);
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
		foreach (var powerUp in powerUpsToClean)
			Destroy(powerUp.gameObject);
		foreach (var ball in ballsToClean)
			Destroy(ball.gameObject);
		foreach (var bullet in bulletsToClean)
			Destroy(bullet.gameObject);
	}

	public void EraseExplosions()
	{
		Explosion[] explosionsToErase = FindObjectsOfType<Explosion>();
		foreach (var explosion in explosionsToErase)
			Destroy(explosion.gameObject);
	}

	public void DisposeBrick(Brick brick)
	{
		bricks[brick.x + (brick.y * LevelSet.COLUMNS)] = null;
		Destroy(brick.gameObject);
		//Debug.Log(bricks.Count);
	}

	public void DecrementRequiredBricks()
	{
		NumberOfBricksRequiredToComplete--;
		//Debug.Log($"Bricks left: {NumberOfBricksRequiredToComplete}");
		CheckIfLevelIsCompleted();
	}

	public void CheckIfLevelIsCompleted()
	{
		if (NumberOfBricksRequiredToComplete == 0)
		{
			CoverSceneAndDoAction(CleanAfterFinishingLevel, LevelSoundLibrary.Instance.win);
		}
	}

	private void SetBricksRequiredToCompleteNumber()
	{
		NumberOfBricksRequiredToComplete = bricks.Count(b => b?.brickType.Properties.RequiredToComplete == true);
		CheckIfLevelIsCompleted();
	}

	public void IncrementRequiredBricks() => NumberOfBricksRequiredToComplete++;

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
		//Paddle.Instance.MegaMissileActive = true;
		//TODO do Megamissile turret on paddle
	}

	public void DecreaseMegaMissiles()
	{
		MegaMissiles = Mathf.Max(MegaMissiles - 1, 0);
		//if (MegaMissiles == 0)
			//Paddle.Instance.MegaMissileActive = false;
	}

	public void DescendBrickRows()
	{
		for (int i = LevelSet.ROWS - 1; i >= 0; i--)
		{
			for (int j = LevelSet.COLUMNS - 1; j >= 0; j--)
			{
				Brick brick = bricks[j + (i * LevelSet.COLUMNS)];
				if (brick && !brick.brickType.Properties.IsDescending)
				{
					brick.TryMoveBlockDown(1);
					if (brick.y != i)
					{
						int indexOfBrickBelow = j + ((i + 1) * LevelSet.COLUMNS);
						bricks[indexOfBrickBelow] = bricks[j + (i * LevelSet.COLUMNS)];
						bricks[j + (i * LevelSet.COLUMNS)] = null;
					}
				}
			}
		}
	}

	private void CoverSceneAndDoAction(Func<IEnumerator> action, AudioClip audioClip = null)
	{
		LevelSoundLibrary.Instance.PlaySfx(audioClip);
		shutterAnimator.Cover(action());
	}

	public Brick[] GetTeleportOutputs(int[] teleportExitIds) => bricks.Where(b => b != null && teleportExitIds.Contains(b.brickType.Properties.Id)).ToArray();

	//BONUS try implementing teleport logic the same as in original
	public bool TryTeleportBrickBuster(GameObject brickBuster, int[] teleportExitIds, Vector3 collisionNormal)
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

	public void CloneBallsToTeleporters(GameObject ball, int[] teleportExitIds, Vector2 collisionNormal)
	{
		Brick[] teleporterOutputs = GetTeleportOutputs(teleportExitIds);
		foreach (Brick teleporterOutput in teleporterOutputs)
		{
			GameObject newBall = BallManager.Instance.CloneBall(ball.gameObject);
			TeleportBrickBuster(teleporterOutput, newBall, collisionNormal);
		}
	}

	private void TeleportBrickBuster(Brick teleporter, GameObject brickBuster, Vector2 collisionNormal)
	{
		BoxCollider2D teleportCollider = teleporter.GetComponent<BoxCollider2D>();
		BoxCollider2D ballCollider = brickBuster.GetComponent<BoxCollider2D>();
		LevelSoundLibrary.Instance.PlaySfx(LevelSoundLibrary.Instance.teleport);
		Rigidbody2D brickBusterRb = brickBuster.GetComponent<Rigidbody2D>();
		float brickBusterZ = brickBuster.transform.position.z;
		float magnitude = brickBusterRb.velocity.magnitude;
		//Hit from bottom
		if (collisionNormal == Vector2.down)
		{
			brickBuster.transform.position = new Vector3(teleportCollider.bounds.center.x, teleportCollider.bounds.min.y + ballCollider.bounds.extents.y, brickBusterZ);
			brickBusterRb.velocity = PhysicsHelper.GetAngledVelocity(160) * magnitude;
		}
		//Hit from left
		else if (collisionNormal == Vector2.left)
		{
			brickBuster.transform.position = new Vector3(teleportCollider.bounds.min.x - ballCollider.bounds.extents.x, teleportCollider.bounds.center.y, brickBusterZ);
			brickBusterRb.velocity = PhysicsHelper.GetAngledVelocity(20) * magnitude;
		}
		//Hit from top
		else if (collisionNormal == Vector2.up)
		{
			brickBuster.transform.position = new Vector3(teleportCollider.bounds.center.x, teleportCollider.bounds.max.y - ballCollider.bounds.extents.y, brickBusterZ);
			brickBusterRb.velocity = PhysicsHelper.GetAngledVelocity(110) * magnitude;
		}
		//Hit from right
		else if (collisionNormal == Vector2.right)
		{
			brickBuster.transform.position = new Vector3(teleportCollider.bounds.max.x + ballCollider.bounds.extents.x, teleportCollider.bounds.center.y, brickBusterZ);
			brickBusterRb.velocity = PhysicsHelper.GetAngledVelocity(290) * magnitude;
		}
	}

	internal void AddExtraPaddle()
	{
		paddles++;
		paddleNumberText.text = $"{paddles}";
	}

	internal void CheckForLosePaddle()
	{
		if (BallManager.Instance.BallNumber == 0)
			LosePaddle();
	}

	internal void LosePaddle()
	{
		paddles--;
		CoverSceneAndDoAction(CleanAfterLoseLife, LevelSoundLibrary.Instance.losePaddle);
	}

	private int GetBrickIndex(int x, int y)
	{
		return x + (y * LevelSet.COLUMNS);
	}

	internal void MultiplyRegulars()
	{
		int[] tmpBrickLayer = bricks.Select(b => b?.BrickProperties.Id ?? 0).ToArray();
		for (int y = 0; y < LevelSet.ROWS; y++)
		{
			for (int x = 0; x < LevelSet.COLUMNS; x++)
			{
				BrickProperties brickProperties = bricks[GetBrickIndex(x, y)]?.BrickProperties;
				if (brickProperties?.IsRegular == true)
				{
					if (y != 0 && bricks[GetBrickIndex(x, y - 1)]?.BrickProperties.CanBeOverridenByStandardMultiplier != false)
						tmpBrickLayer[GetBrickIndex(x, y - 1)] = brickProperties.Id;
					if (x != LevelSet.COLUMNS - 1 && bricks[GetBrickIndex(x + 1, y)]?.BrickProperties.CanBeOverridenByStandardMultiplier != false)
						tmpBrickLayer[GetBrickIndex(x + 1, y)] = brickProperties.Id;
					if (y != LevelSet.ROWS - 1 && bricks[GetBrickIndex(x, y + 1)]?.BrickProperties.CanBeOverridenByStandardMultiplier != false)
						tmpBrickLayer[GetBrickIndex(x, y + 1)] = brickProperties.Id;
					if (x != 0 && bricks[GetBrickIndex(x - 1, y)]?.BrickProperties.CanBeOverridenByStandardMultiplier != false)
						tmpBrickLayer[GetBrickIndex(x - 1, y)] = brickProperties.Id;
				}
			}
		}
		RenderTemporaryBrickLayer(tmpBrickLayer);
	}

	internal void MultiplyExplosives()
	{
		int[] tmpBrickLayer = bricks.Select(b => b?.BrickProperties.Id ?? 0).ToArray();
		int explosives = 0;//Number of explosives needed to determine if additional explosives should not be added.
		for (int y = 0; y < LevelSet.ROWS; y++)
		{
			for (int x = 0; x < LevelSet.COLUMNS; x++)
			{
				BrickProperties brickProperties = bricks[GetBrickIndex(x, y)]?.BrickProperties;
				if (brickProperties?.IsExplosive == true && brickProperties.CanBeMultipliedByExplosiveMultiplier)
				{
					if (y != 0 && bricks[GetBrickIndex(x, y - 1)]?.BrickProperties.CanBeOverridenByExplosiveMultiplier != false)
						tmpBrickLayer[GetBrickIndex(x, y - 1)] = brickProperties.Id;
					if (x != LevelSet.COLUMNS - 1 && bricks[GetBrickIndex(x + 1, y)]?.BrickProperties.CanBeOverridenByExplosiveMultiplier != false)
						tmpBrickLayer[GetBrickIndex(x + 1, y)] = brickProperties.Id;
					if (y != LevelSet.ROWS - 1 && bricks[GetBrickIndex(x, y + 1)]?.BrickProperties.CanBeOverridenByExplosiveMultiplier != false)
						tmpBrickLayer[GetBrickIndex(x, y + 1)] = brickProperties.Id;
					if (x != 0 && bricks[GetBrickIndex(x - 1, y)]?.BrickProperties.CanBeOverridenByExplosiveMultiplier != false)
						tmpBrickLayer[GetBrickIndex(x - 1, y)] = brickProperties.Id;
					explosives++;
				}
			}
		}
		//Debug.Log($"Explosives: {explosives}");
		for (int additionalExplosivePluses = 10 - explosives; additionalExplosivePluses > 0; additionalExplosivePluses--)
		{
			int x = UnityEngine.Random.Range(0, LevelSet.COLUMNS);
			int y = UnityEngine.Random.Range(0, LevelSet.ROWS);
			tmpBrickLayer[GetBrickIndex(x, y)] = RegularExplosiveId;
			if (y != 0 && bricks[GetBrickIndex(x, y - 1)]?.BrickProperties.CanBeOverridenByExplosiveMultiplier != false)
				tmpBrickLayer[GetBrickIndex(x, y - 1)] = RegularExplosiveId;
			if (x != LevelSet.COLUMNS - 1 && bricks[GetBrickIndex(x + 1, y)]?.BrickProperties.CanBeOverridenByExplosiveMultiplier != false)
				tmpBrickLayer[GetBrickIndex(x + 1, y)] = RegularExplosiveId;
			if (y != LevelSet.ROWS - 1 && bricks[GetBrickIndex(x, y + 1)]?.BrickProperties.CanBeOverridenByExplosiveMultiplier != false)
				tmpBrickLayer[GetBrickIndex(x, y + 1)] = RegularExplosiveId;
			if (x != 0 && bricks[GetBrickIndex(x - 1, y)]?.BrickProperties.CanBeOverridenByExplosiveMultiplier != false)
				tmpBrickLayer[GetBrickIndex(x - 1, y)] = RegularExplosiveId;
		}
		RenderTemporaryBrickLayer(tmpBrickLayer);
	}

	private void RenderTemporaryBrickLayer(int[] tmpBrickLayer)
	{
		for (int y = 0; y < LevelSet.ROWS; y++)
			for (int x = 0; x < LevelSet.COLUMNS; x++)
			{
				if (bricks[GetBrickIndex(x, y)] != null)
					Destroy(bricks[GetBrickIndex(x, y)].gameObject);
				float brickWidth = LevelSetBrickTypes[0].BrickUnityWidth;
				float brickHeight = LevelSetBrickTypes[0].BrickUnityHeight;
				if (tmpBrickLayer[GetBrickIndex(x, y)] != 0)
					GenerateBrick(tmpBrickLayer[GetBrickIndex(x, y)], brickWidth, brickHeight, x, y);
			}
		SetBricksRequiredToCompleteNumber();
	}

	internal void SaveGame()
	{
		new LevelPersistentData { Paddles = paddles, LevelNum = LevelIndex, CurrentScore = int.Parse(scoreText.text) }.Save(LoadedGameData.LevelSetFileName);
	}

	internal void DeleteSave()
	{
		if (System.IO.File.Exists($"{CurrentLevelSet.LevelSetProperties.Name}.sav"))
			System.IO.File.Delete($"{CurrentLevelSet.LevelSetProperties.Name}.sav");
	}

	IEnumerator CleanAfterLoseLife()
	{
		yield return new WaitUntil(() => shutterAnimator.GetComponent<ShutterAnimationManager>().Covered);
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
				GoToLeaderboard();
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
			shutterAnimator.Uncover();
		}
		else
		{
			Debug.Log("The End.");
			if (LoadedGameData.TestMode == TestMode.None)
			{
				DeleteSave();
				GoToLeaderboard();
			}
			else
				Application.Quit();
		}
	}

	private void GoToLeaderboard()
	{
		EndGameData.LastLevel = LeaderboardManager.WonPlaceholder;
		EndGameData.Score = int.Parse(scoreText.text);
		LeaderboardManager.addNewRecord = true;
		SceneManager.LoadScene("Leaderboard");
	}

	private IEnumerator ExitGame()
	{
		yield return new WaitUntil(() => shutterAnimator.Covered);
		if (LoadedGameData.TestMode == TestMode.None)
			SceneManager.LoadScene("Level Set List");
		else
			Application.Quit();
	}
}
