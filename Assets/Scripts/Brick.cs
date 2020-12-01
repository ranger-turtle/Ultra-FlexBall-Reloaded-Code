using LevelSetData;
using System.Collections;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Brick : MonoBehaviour
{
	private const float fadingDecrement = 3.0f;
	public BrickType brickType;
	public int x;
	public int y;
	private float height;

	private int currentAnimationSpriteIndex;

	public bool Broken { get; private set; }
	public bool SpaceDjoelBrick { get; private set; }
	public bool Hidden { get; private set; }

	private SoundManager soundManager;
	private SpriteRenderer spriteRenderer;
	private BoxCollider2D boxCollider2D;

	internal BrickProperties BrickProperties => brickType.Properties;

	private IEnumerator CurrentBrickAnimationCoroutine;

	private ParticleSystem chimneyParticles;

	private bool performSpecialHit = true;

	// Start is called before the first frame update
	void Start()
    {
		soundManager = SoundManager.Instance;
		spriteRenderer = GetComponent<SpriteRenderer>();

		boxCollider2D = GetComponent<BoxCollider2D>();
		height = boxCollider2D.size.y;
		PrepareParticles();
		PrepareBrickAnimation();
	}

	internal void TryHide()
	{
		if (BrickProperties.Hidden)
		{
			GetComponent<SpriteRenderer>().enabled = false;
			Hidden = true;
		}
	}

	private void ChangeToNextBrick() => ChangeBrickType(BrickProperties.NextBrickTypeId);

	private void ChangeToBrickAfterBeingPressed() => ChangeBrickType(BrickProperties.DescendingPressTurnId);

	internal void ChangeBrickType(int newBrickId)
	{
		bool previousRequiredToComplete = (brickType.Properties.RequiredToComplete && !Hidden) || (brickType.Properties.RequiredToCompleteWhenHidden && Hidden);
		brickType = GameManager.Instance.GetBrickTypeById(newBrickId);
		bool nextRequiredToComplete = (brickType.Properties.RequiredToComplete && !brickType.Properties.Hidden) || brickType.Properties.RequiredToCompleteWhenHidden;
		//hitSound = FileImporter.LoadAudioClip("classic", "brickbreak");
		if (nextRequiredToComplete != previousRequiredToComplete)
		{
			if (!nextRequiredToComplete && previousRequiredToComplete)
				GameManager.Instance.DecrementRequiredBricks();
			else if (nextRequiredToComplete && !previousRequiredToComplete)
				GameManager.Instance.IncrementRequiredBricks();
		}
		if (brickType.Properties.Hidden)
			TryHide();
		else
		{
			spriteRenderer.enabled = true;
			Hidden = false;
		}
		PrepareParticles();
		PrepareBrickAnimation();
	}

	internal void TryMoveBlockDown(int yMove)
	{
		if (y < LevelSet.ROWS - 1)
		{

			Brick hitBrick = GameManager.Instance.GetBrickByCoordinates(x, y + yMove);

			if (hitBrick)
			{
				if (hitBrick.BrickProperties.DescendingPressTurnId != 0)
					hitBrick.ChangeBrickType(hitBrick.BrickProperties.DescendingPressTurnId);
				//Debug.Log($"x: {x}, y: {y}: No change");
			}
			else
			{
				Vector3 move = new Vector3(0, height * yMove, 0);
				transform.position -= move;
				if (chimneyParticles)
					chimneyParticles.transform.position -= move;
				y = Mathf.Min(y + yMove, LevelSet.ROWS - 1);
				if (y == LevelSet.ROWS - 1)
				{
					if (BrickProperties.DescendingBottomTurnId != 0)
						ChangeBrickType(BrickProperties.DescendingBottomTurnId);
					//Debug.Log($"x: {x}, y: {y}: At the bottom");
				}
				//Debug.Log($"x: {x}, y: {y}: Move");
			}
		}
	}

	private void PrepareBrickAnimation()
	{
		if (brickType.Sprites.Length > 1)
		{
			currentAnimationSpriteIndex = BrickProperties.StartAnimationFromRandomFrame ? Mathf.RoundToInt(Random.Range(0, brickType.Sprites.Length)) : 0;
			StartCoroutine(CurrentBrickAnimationCoroutine = BrickAnimationCoroutine());
		}
		else
		{
			currentAnimationSpriteIndex = 0;
			spriteRenderer.sprite = brickType.Sprites[currentAnimationSpriteIndex];
		}
	}

	private void PrepareParticles()
	{
		chimneyParticles = ParticleManager.Instance.GenerateBrickParticle(BrickProperties);
		if (chimneyParticles)
		{
			float x = transform.position.x + BrickType.BrickUnityWidth * BrickProperties.ParticleX / 100;
			float y = transform.position.y + BrickType.BrickUnityHeight * BrickProperties.ParticleY / 100;
			chimneyParticles.transform.position = new Vector3(x, y, transform.position.z - 0.1f);
		}
	}

	private void MakeExplosion(int explosionRadius)
	{
		Vector2 spriteBounds = boxCollider2D.size;
		Vector3 explosionPosition = new Vector3(gameObject.transform.position.x + spriteBounds.x / 2, gameObject.transform.position.y - spriteBounds.y / 2, -4);
		GameManager.Instance.MakeExplosion(explosionRadius, explosionPosition, spriteBounds);
	}

	private void PlayHitSound() => soundManager.PlaySfx(brickType.hitAudio);

	//private void OnCollisionEnter2D(Collision2D collision)
	private void Collision(GameObject brickBusterObject)
	{
		//Debug.Log("Brick hit");
		bool isBrickBusterABall = brickBusterObject.GetComponent<Ball>();
		bool isBrickBusterABullet = brickBusterObject.GetComponent<Bullet>();
		bool isSpaceDjoel = brickBusterObject.GetComponent<SpaceDjoel>();
		IBrickBuster brickBuster = brickBusterObject.GetComponent<IBrickBuster>();
		if (isBrickBusterABall || isBrickBusterABullet || isSpaceDjoel)
		{
			PlayHitSound();
			TryIncreasePowerUpField(brickBusterObject);
			if (BrickProperties.IsTeleporter)
			{
				brickBuster.Teleport = GameManager.Instance.TryTeleportBrickBuster(brickBusterObject, BrickProperties.TeleportExits, brickBuster.LastHitNormal);
				if (BrickProperties.TeleportType == TeleportType.All)
					GameManager.Instance.GenerateBallsFromTeleporters(brickBusterObject, BrickProperties.TeleportExits, brickBuster.LastHitNormal);
			}
			if (!brickBuster.Teleport)//if brickBuster is not teleported
			{
				bool previouslyHidden = false;
				if (Hidden)
				{
					Reveal();
					previouslyHidden = true;
				}
				if (!isSpaceDjoel)
				{
					if (GameManager.Instance.PenetratingBall && !BrickProperties.PenetrationResistant)
						Break(BrickProperties.Points / 2, brickBusterObject, true);
					else if (!BrickProperties.NormalResistant && !previouslyHidden)
						Break(BrickProperties.Points, brickBusterObject);
					else if (brickType.HasHitSprite)
						StartCoroutine(DisplayHitSprite());
					if (BrickProperties.FuseDirection != Direction.None && TriggersOnHit(BrickProperties.FuseTrigger))
						StartCoroutine(PrepareForFuseBurn());
					if (GameManager.Instance.ExplosiveBall)
					{
						MakeExplosion(1);
						soundManager.PlaySfx("Bang");
					}
					if (isBrickBusterABall && BrickProperties.IsBallThrusting && BrickProperties.NormalResistant)
						BreakBrickPointedByBallThrustingBrick(BrickProperties.BallThrustDirection);
				}
				else
					Break(BrickProperties.Points, brickBusterObject, true);
			}
			if (BrickProperties.AlwaysSpecialHit)
				PerformSpecialHit();
			ParticleManager.Instance.GenerateBrickHitEffect(new Vector3(brickBuster.LastHitPoint.x, brickBuster.LastHitPoint.y, transform.position.z - 0.1f), brickBuster.LastHitNormal);
			performSpecialHit = true;
		}
	}

	public void Break(int score, GameObject brickBusterObject = null, bool force = false)
	{
		if (!Broken)
		{
			if (BrickProperties.NextBrickTypeId == 0 || force)
			{
				Reveal(false);
				DecrementRequiredBrickNumberIfNeeded();
				if (BrickProperties.IsExplosive && TriggersOnDestroy(BrickProperties.ExplosionTrigger))
				{
					if (brickBusterObject)
						soundManager.PlaySfx("Bang");
					MakeExplosion(BrickProperties.ExplosionRadius);
				}
				if (BrickProperties.IsDetonator && TriggersOnDestroy(BrickProperties.DetonationTrigger))
					GameManager.Instance.DetonateOrChangeBrick(BrickProperties.OldBrickTypeId, BrickProperties.NewBrickTypeId, BrickProperties.DetonationRange);
				if (BrickProperties.FuseDirection != Direction.None && TriggersOnDestroy(BrickProperties.FuseTrigger))
					StartCoroutine(PrepareForFuseBurn());
				if (brickBusterObject?.GetComponent<SpaceDjoel>() == false || BrickProperties.IsExplosive || BrickProperties.IsFuse)
				{
					Broken = true;
					PlayBreakAnimation(brickBusterObject);
					GetComponent<Collider2D>().enabled = false;
				}
				else
				{
					StopBrickAnimation();
					currentAnimationSpriteIndex = 0;
					StartCoroutine(IcyFadingCoroutine());
				}
				DestroyParticles();
			}
			else
			{
				ChangeToNextBrick();
				PrepareBrickAnimation();
			}
			GameManager.Instance.AddToScore(score);
			if (BrickProperties.RequiredToComplete)
				PowerUpManager.Instance.ResetHelpCountdown();
		}
	}

	private void PlayBreakAnimation(GameObject brickBusterObject)
	{
		BreakAnimationType breakAnimationType = BrickProperties.BallBreakAnimationType;
		Sprite[] sprites = brickType.BallBreakAnimationSprites;
		if (!brickBusterObject)
		{
			breakAnimationType = BrickProperties.ExplosionBreakAnimationType;
			sprites = brickType.ExplosionBreakAnimationSprites;
		}
		else if (brickBusterObject.GetComponent<Bullet>())
		{
			breakAnimationType = BrickProperties.BulletBreakAnimationType;
			sprites = brickType.BulletBreakAnimationSprites;
		}
		switch (breakAnimationType)
		{
			case BreakAnimationType.Fade:
				StartCoroutine(FadingCoroutine());
				break;
			case BreakAnimationType.Burn:
			case BreakAnimationType.Custom:
				StopBrickAnimation();
				StartCoroutine(BreakAnimationCoroutine(sprites));
				break;
			default:
				break;
		}
	}

	private void DecrementRequiredBrickNumberIfNeeded()
	{
		if (BrickProperties.RequiredToComplete && !SpaceDjoelBrick)
			GameManager.Instance.DecrementRequiredBricks();
	}

	private void StopBrickAnimation()
	{
		if (CurrentBrickAnimationCoroutine != null)
			StopCoroutine(CurrentBrickAnimationCoroutine);
	}

	public void TryIncreasePowerUpField(GameObject brickBusterObject = null)
	{
		Vector2 powerUpVelocity = PhysicsHelper.GetAngledVelocity(Random.Range(20, 160)) * 0.1f;
		float powerUpYieldX = boxCollider2D.bounds.center.x;
		float powerUpYieldY = boxCollider2D.bounds.center.y;
		Vector2 powerUpPosition = new Vector3(powerUpYieldX, powerUpYieldY);
		int outputPowerUpMeterUnits = BrickProperties.PowerUpMeterUnits;
		if (brickBusterObject)//If brick is destroyed by brick buster
		{
			Ball ball = brickBusterObject.GetComponent<Ball>();
			Bullet bullet = brickBusterObject.GetComponent<Bullet>();
			SpaceDjoel spaceDjoel = brickBusterObject.GetComponent<SpaceDjoel>();
			if (ball || bullet)
			{
				if (GameManager.Instance.PenetratingBall)
					outputPowerUpMeterUnits /= 2;
				if (ball && !ball.Thrust)
					powerUpVelocity = ball.LastFrameVelocity;
				else if (bullet)
					powerUpVelocity = bullet.VelocityBeforeHit;
			}
			else if (spaceDjoel)
			{
				powerUpVelocity = spaceDjoel.LastFrameVelocity;
				outputPowerUpMeterUnits /= 2;
			}
		}
		else
			outputPowerUpMeterUnits = 1;
		if (!BrickProperties.AlwaysPowerUpYielding)
		{
			bool yield = PowerUpManager.Instance.IncreaseMeter(outputPowerUpMeterUnits, powerUpPosition, powerUpVelocity, BrickProperties.YieldedPowerUp);
			if (yield)
				PerformSpecialHit();
		}
		else
		{
			PowerUpManager.Instance.YieldPowerUp(powerUpPosition, powerUpVelocity, BrickProperties.YieldedPowerUp, 9);
			PerformSpecialHit();
		}
	}

	internal void Reveal(bool specialHit = true)
	{
		if (Hidden)
		{
			if (specialHit)
				PerformSpecialHit();
			spriteRenderer.enabled = true;
			if (!BrickProperties.RequiredToCompleteWhenHidden && BrickProperties.RequiredToComplete)
				GameManager.Instance.IncrementRequiredBricks();
			else if (BrickProperties.RequiredToCompleteWhenHidden && !BrickProperties.RequiredToComplete)
				GameManager.Instance.DecrementRequiredBricks();
			Hidden = false;
		}
	}

	private void PerformSpecialHit()
	{
		if (performSpecialHit)
		{
			Bounds bounds = boxCollider2D.bounds;
			float xExtents = bounds.extents.x;
			float yExtents = bounds.extents.y;
			SoundManager.Instance.PlaySfx("Bang");
			ParticleManager.Instance.GenerateSpecialHitEffect(new Vector3(transform.position.x + xExtents, transform.position.y - yExtents, transform.position.z - 0.1f));
			performSpecialHit = false;
		}
	}

	private void BreakBrickPointedByBallThrustingBrick(Direction direction)
	{
		switch (direction)
		{
			case Direction.Up when y - 1 >= 0:
				GameManager.Instance.GetBrickByCoordinates(x, y - 1)?.FadeBrickAway();
				break;
			case Direction.Down when y + 1 < LevelSet.ROWS:
				GameManager.Instance.GetBrickByCoordinates(x, y + 1)?.FadeBrickAway();
				break;
			case Direction.Left when x - 1 >= 0:
				GameManager.Instance.GetBrickByCoordinates(x - 1, y)?.FadeBrickAway();
				break;
			case Direction.Right when x + 1 < LevelSet.COLUMNS:
				GameManager.Instance.GetBrickByCoordinates(x + 1, y)?.FadeBrickAway();
				break;
		}
	}

	internal void FadeBrickAway()
	{
		DecrementRequiredBrickNumberIfNeeded();
		DestroyParticles();
		GetComponent<Collider2D>().enabled = false;
		Broken = true;
		StartCoroutine(FadingCoroutine());
	}

	private IEnumerator DisplayHitSprite()
	{
		int lastIndex = currentAnimationSpriteIndex;
		Sprite lastSprite = brickType.Sprites[lastIndex];
		spriteRenderer.sprite = brickType.HitSprite;
		StopBrickAnimation();
		yield return new WaitForSeconds(0.1f);
		spriteRenderer.sprite = lastSprite;
	}

	private IEnumerator PrepareForFuseBurn()
	{
		yield return new WaitForSeconds(0.1f);
		soundManager.PlaySfx("Explosion");
		GameManager.Instance.DestroyBrickPointedByFuseType(this);
	}

	private IEnumerator BrickAnimationCoroutine()
	{
		while (true)
		{
			spriteRenderer.sprite = brickType.Sprites[currentAnimationSpriteIndex];
			currentAnimationSpriteIndex = (currentAnimationSpriteIndex + 1) % brickType.Sprites.Length;
			yield return new WaitForSeconds(BrickProperties.FrameDurations[currentAnimationSpriteIndex]);
		}
	}

	private IEnumerator BreakAnimationCoroutine(Sprite[] animationSprites)
	{
		for (int i = 0; i < animationSprites.Length; i++)
		{
			spriteRenderer.sprite = animationSprites[i];
			yield return new WaitForSeconds(0.02f);
		}
		GameManager.Instance.DisposeBrick(this);
	}

	private IEnumerator FadingCoroutine()
	{
		while (spriteRenderer.color.a > 0)
		{
			Color color = spriteRenderer.color;
			float newAlpha = color.a - fadingDecrement * Time.deltaTime;
			color = new Color(color.r, color.g, color.b, newAlpha);
			spriteRenderer.color = color;

			yield return null;
		}
		GameManager.Instance.DisposeBrick(this);
	}

	//Coroutine cannot be directly started from other class because stopAllCoroutines won't work properly
	public void StartIcyFading() => StartCoroutine(IcyFadingCoroutine());

	//fading coroutine used when Space Djoel hits the brick
	private IEnumerator IcyFadingCoroutine()
	{
		brickType = GameManager.Instance.SpaceDjoelBrickType;
		SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
		spriteRenderer.sprite = brickType.FirstSprite;
		SpaceDjoelBrick = true;
		while (spriteRenderer.color.a > 0)
		{
			Color color = spriteRenderer.color;
			float newAlpha = color.a - (fadingDecrement / 3) * Time.deltaTime;
			color = new Color(color.r, color.g, color.b, newAlpha);
			spriteRenderer.color = color;

			yield return null;
		}
		GameManager.Instance.DisposeBrick(this);
	}

	public void DestroyParticles()
	{
		if (chimneyParticles)
		{
			chimneyParticles.Stop();
			Destroy(chimneyParticles.gameObject, chimneyParticles.main.duration);
		}
	}

	public bool HaveEqualCoordinates(int x, int y) => this.x == x && this.y == y;

	public bool TriggersOnHit(EffectTrigger effectTrigger) => effectTrigger == EffectTrigger.Hit || effectTrigger == EffectTrigger.Both;

	public bool TriggersOnDestroy(EffectTrigger effectTrigger) => effectTrigger == EffectTrigger.Destroy || effectTrigger == EffectTrigger.Both;

	private void OnDestroy()
	{
		DestroyParticles();
	}
}
