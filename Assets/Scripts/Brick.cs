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

	private int currentAnimationSpriteIndex;
	private bool hidden;

	public bool Broken { get; private set; }
	public bool SpaceDjoelBrick { get; private set; }

	private SoundManager soundManager;

	internal BrickProperties BrickProperties => brickType.Properties;

	private IEnumerator CurrentBrickAnimationCoroutine;

	private ParticleSystem chimneyParticles;

    // Start is called before the first frame update
    void Start()
    {
		soundManager = SoundManager.Instance;

		if (BrickProperties.Hidden)
		{
			GetComponent<SpriteRenderer>().enabled = false;
			hidden = true;
		}
		PrepareParticles();
		PrepareBrickAnimation();
	}

	private void ChangeToNextBrick() => ChangeBrickType(BrickProperties.NextBrickId);

	private void ChangeToBrickAfterBeingPressed() => ChangeBrickType(BrickProperties.DescendingPressTurnId);

	internal void ChangeBrickType(int newBrickId)
	{
		bool previousRequiredToComplete = brickType.Properties.RequiredToComplete;
		brickType = GameManager.Instance.GetBrickById(newBrickId);
		//hitSound = FileImporter.LoadAudioClip("classic", "brickbreak");
		if (brickType.Properties.RequiredToComplete != previousRequiredToComplete)
		{
			if (!brickType.Properties.RequiredToComplete && previousRequiredToComplete)
				GameManager.Instance.DecrementRequiredBricks();
			else
				GameManager.Instance.IncrementRequiredBricks();
		}
		PrepareParticles();
		PrepareBrickAnimation();
	}

	internal void TryMoveBlockDown(int yMove)
	{
		if (y == LevelSet.ROWS - 1)
		{
			if (BrickProperties.DescendingBottomTurnId != 0)
				ChangeBrickType(BrickProperties.DescendingBottomTurnId);
			Debug.Log($"x: {x}, y: {y}: At the bottom");
		}
		else
		{
			float height = GetComponent<BoxCollider2D>().size.y;

			Brick hitBrick = GameManager.Instance.GetBrickByCoordinates(x, y + yMove);

			if (hitBrick)
			{
				if (hitBrick.BrickProperties.DescendingBottomTurnId != 0)
					hitBrick.ChangeBrickType(hitBrick.BrickProperties.DescendingBottomTurnId);
				Debug.Log($"x: {x}, y: {y}: No change");
			}
			else
			{
				Vector3 move = new Vector3(0, height * yMove, 0);
				transform.position -= move;
				if (chimneyParticles)
					chimneyParticles.transform.position -= move;
				y += yMove;
				Debug.Log($"x: {x}, y: {y}: Move");
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
			GetComponent<SpriteRenderer>().sprite = brickType.Sprites[currentAnimationSpriteIndex];
		}
	}

	private void PrepareParticles()
	{
		chimneyParticles = ParticleManager.Instance.GenererateBrickParticle(BrickProperties);
		if (chimneyParticles != null)
		{
			float x = transform.position.x + brickType.BrickUnityWidth * BrickProperties.ParticleX / 100;
			float y = transform.position.y + brickType.BrickUnityHeight * BrickProperties.ParticleY / 100;
			chimneyParticles.transform.position = new Vector3(x, y, transform.position.z - 0.1f);
		}
	}

	private void MakeExplosion(int explosionRadius)
	{
		Vector2 spriteBounds = GetComponent<BoxCollider2D>().size;
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
			if (!brickBuster.Teleport)
			{
				if (!isSpaceDjoel)
				{
					if (GameManager.Instance.PenetratingBall && !BrickProperties.PenetrationResistant)
						Break(BrickProperties.Points / 2, brickBusterObject, true);
					else if (hidden)
						Reveal();
					else if (!BrickProperties.NormalResistant)
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
					if (isBrickBusterABall && !GameManager.Instance.PenetratingBall && BrickProperties.IsBallThrusting)
						BreakBrickPointedByBallThrustingBrick(BrickProperties.BallThrustDirection);
				}
				else
					Break(BrickProperties.Points, brickBusterObject, true);
			}
			if (BrickProperties.AlwaysSpecialHit)
				GameManager.Instance.PerformSpecialHit();
			ParticleManager.Instance.GenerateBrickHitEffect(new Vector3(brickBuster.LastHitPoint.x, brickBuster.LastHitPoint.y, transform.position.z - 0.1f), brickBuster.LastHitNormal);
		}
	}

	public void Break(int score, GameObject brickBusterObject = null, bool force = false)
	{
		if (!Broken)
		{
			//Debug.Log($"Brick break x: {x}, y: {y}");
			//Debug.Log($"Brick Disabled");
			if (BrickProperties.NextBrickId == 0 || force)
			{
				Reveal();
				DecrementRequiredBrickNumberIfNeeded();
				if (BrickProperties.IsExplosive && TriggersOnDestroy(BrickProperties.ExplosionTrigger))
				{
					//Debug.Break();
					MakeExplosion(BrickProperties.ExplosionRadius);
				}
				if (BrickProperties.IsDetonator && TriggersOnDestroy(BrickProperties.DetonationTrigger))
					GameManager.Instance.DetonateBrick(BrickProperties.DetonateId, BrickProperties.DetonationRange);
				//TODO do custom animation support
				//TODO try making distinctive trigger on destroy or hit
				if (BrickProperties.FuseDirection != Direction.None && TriggersOnDestroy(BrickProperties.FuseTrigger))
					StartCoroutine(PrepareForFuseBurn());
				if (brickBusterObject?.GetComponent<SpaceDjoel>() == false)
				{
					StartCoroutine(FadingCoroutine());
					Broken = true;
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
		//TODO make power-up to jump to far-left, near-left, near-right and far-right
		Vector2 powerUpVelocity = new Vector2(0.3f, 0.5f);
		float powerUpYieldX = GetComponent<BoxCollider2D>().bounds.center.x;
		float powerUpYieldY = GetComponent<BoxCollider2D>().bounds.center.y;
		Vector2 powerUpPosition = new Vector3(powerUpYieldX, powerUpYieldY);
		int outputPowerUpMeterUnits = BrickProperties.PowerUpMeterUnits;
		if (brickBusterObject)//If brick is destroyed by brick buster
		{
			if (brickBusterObject.GetComponent<Ball>() || brickBusterObject.GetComponent<Bullet>())
			{
				if (GameManager.Instance.PenetratingBall)
					outputPowerUpMeterUnits /= 2;
				if (brickBusterObject.GetComponent<Ball>())
					powerUpVelocity = brickBusterObject.GetComponent<Ball>().LastFrameVelocity;
				else if (brickBusterObject.GetComponent<Bullet>())
					powerUpVelocity = brickBusterObject.GetComponent<Bullet>().VelocityBeforeHit;
			}
			else if (brickBusterObject.GetComponent<SpaceDjoel>())
			{
				powerUpVelocity = brickBusterObject.GetComponent<SpaceDjoel>().LastFrameVelocity;
				outputPowerUpMeterUnits /= 2;
			}
		}
		else
			outputPowerUpMeterUnits = 1;
		if (!BrickProperties.AlwaysPowerUpYielding)
			PowerUpManager.Instance.IncreaseMeter(outputPowerUpMeterUnits, powerUpPosition, powerUpVelocity);
		else
			PowerUpManager.Instance.YieldPowerUp(powerUpPosition, powerUpVelocity, 9);
	}

	private void Reveal(bool performSpecialHit = true)
	{
		if (hidden)
		{
			if (performSpecialHit)
				GameManager.Instance.PerformSpecialHit();
			GetComponent<SpriteRenderer>().enabled = true;
			if (!BrickProperties.RequiredToCompleteWhenHidden && BrickProperties.RequiredToComplete)
				GameManager.Instance.IncrementRequiredBricks();
			else if (BrickProperties.RequiredToCompleteWhenHidden && !BrickProperties.RequiredToComplete)
				GameManager.Instance.DecrementRequiredBricks();
			hidden = false;
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
		//UNDONE make function work properly when brick is animated
		SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
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
		GameManager.Instance.DestroyBrickPointedByFuseType(this);
	}

	public IEnumerator BrickAnimationCoroutine()
	{
		while (true)
		{
			GetComponent<SpriteRenderer>().sprite = brickType.Sprites[currentAnimationSpriteIndex];
			currentAnimationSpriteIndex = (currentAnimationSpriteIndex + 1) % brickType.Sprites.Length;
			yield return new WaitForSeconds(BrickProperties.FrameDurations[currentAnimationSpriteIndex]);
		}
	}

	public IEnumerator FadingCoroutine()
	{
		while (GetComponent<SpriteRenderer>().color.a > 0)
		{
			Color color = GetComponent<SpriteRenderer>().color;
			float newAlpha = color.a - fadingDecrement * Time.deltaTime;
			color = new Color(color.r, color.g, color.b, newAlpha);
			GetComponent<SpriteRenderer>().color = color;

			yield return null;
		}
		GameManager.Instance.DisposeBrick(this);
	}

	//fading coroutine used when Space Djoel hits the brick
	public IEnumerator IcyFadingCoroutine()
	{
		brickType = GameManager.Instance.SpaceDjoelBrickType;
		GetComponent<SpriteRenderer>().sprite = brickType.FirstSprite;
		SpaceDjoelBrick = true;
		while (GetComponent<SpriteRenderer>().color.a > 0)
		{
			Color color = GetComponent<SpriteRenderer>().color;
			float newAlpha = color.a - (fadingDecrement / 3) * Time.deltaTime;
			color = new Color(color.r, color.g, color.b, newAlpha);
			GetComponent<SpriteRenderer>().color = color;

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
}
