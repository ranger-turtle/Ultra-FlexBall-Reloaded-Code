using LevelSetData;
using System.Collections;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Brick : MonoBehaviour
{
	public BrickType brickType;
	public int x;
	public int y;

	private int currentAnimationSpriteId;

	public bool Broken { get; private set; }

	private LevelSoundLibrary levelSoundLibrary;

	internal BrickProperties BrickProperties => brickType.Properties;

    // Start is called before the first frame update
    void Start()
    {
		levelSoundLibrary = GameObject.Find("Game").GetComponent<LevelSoundLibrary>();
		//hitSound = FileImporter.LoadAudioClip("classic", "brickbreak");//TODO In FlexEd, make choose to none, brickbreak, bang, metalhit and custom sound
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
		levelSoundLibrary.PlaySfx(levelSoundLibrary.changingBrickHit);
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
				transform.position = new Vector3(transform.position.x, transform.position.y - (height * yMove), transform.position.z);
				y += yMove;
				Debug.Log($"x: {x}, y: {y}: Move");
			}
		}
	}

	private void PrepareBrickAnimation()
	{
		if (brickType.Sprites.Length > 1)
		{
			currentAnimationSpriteId = BrickProperties.StartAnimationFromRandomFrame ? Mathf.RoundToInt(Random.Range(0, brickType.Sprites.Length)) : 0;
			StartCoroutine(BrickAnimationCoroutine());
		}
		else
		{
			currentAnimationSpriteId = 0;
			GetComponent<SpriteRenderer>().sprite = brickType.Sprites[currentAnimationSpriteId];
		}
	}

	private void MakeExplosion(int explosionRadius)
	{
		Vector2 spriteBounds = GetComponent<BoxCollider2D>().size;
		Vector3 explosionPosition = new Vector3(gameObject.transform.position.x + spriteBounds.x / 2, gameObject.transform.position.y - spriteBounds.y / 2, -4);
		GameManager.Instance.MakeExplosion(explosionRadius, explosionPosition, spriteBounds);
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		//Debug.Log("Brick hit");
		GameObject brickBusterObject = collision.gameObject;
		Rigidbody2D brickBusterRigidBody = brickBusterObject.GetComponent<Rigidbody2D>();
		bool isBrickBusterABall = brickBusterObject.GetComponent<Ball>();
		bool isBrickBusterABullet = brickBusterObject.GetComponent<Bullet>();
		if (isBrickBusterABall || isBrickBusterABullet)
		{
			int outputPowerUpMeterUnits = BrickProperties.PowerUpMeterUnits;
			bool teleport = false;
			if (BrickProperties.IsTeleporter)
			{
				teleport = GameManager.Instance.TryTeleportBrickBuster(brickBusterObject, BrickProperties.TeleportExits, collision.GetContact(0).normal);
				if (BrickProperties.TeleportType == TeleportType.All)
					GameManager.Instance.CloneBallsToTeleporters(brickBusterObject, BrickProperties.TeleportExits, collision.GetContact(0).normal);
			}
			if (!(BrickProperties.IsTeleporter && teleport))
			{
				if (!BrickProperties.AlwaysPowerUpYielding)
				{
					if (GameManager.Instance.PenetratingBall)
						outputPowerUpMeterUnits /= 2;
					float powerUpYieldX = transform.position.x + GetComponent<BoxCollider2D>().bounds.extents.x;
					float powerUpYieldY = transform.position.y + GetComponent<BoxCollider2D>().bounds.extents.y;
					Vector2 powerUpVelocity = brickBusterObject.GetComponent<Ball>() ? brickBusterObject.GetComponent<Ball>().LastFrameVelocity : brickBusterObject.GetComponent<Bullet>().VelocityBeforeHit;
					PowerUpManager.Instance.IncreaseMeter(outputPowerUpMeterUnits, new Vector3(powerUpYieldX, powerUpYieldY), powerUpVelocity);
				}
				else
					PowerUpManager.Instance.YieldPowerUp(brickBusterObject.transform.position, brickBusterRigidBody.velocity, 9);
				if (GameManager.Instance.PenetratingBall)
				{
					if (!BrickProperties.PenetrationResistant)
						Break(BrickProperties.Points / 2, true);
				}
				else if (!BrickProperties.NormalResistant)
					Break(BrickProperties.Points);
				else
				{
					if (BrickProperties.FuseDirection != Direction.None && TriggersOnHit(BrickProperties.FuseTrigger))
						StartCoroutine(PrepareForFuseBurn());
				}
				if (GameManager.Instance.ExplosiveBall)
				{
					MakeExplosion(1);
					levelSoundLibrary.PlaySfx(levelSoundLibrary.bang);
				}
			}
		}
	}

	public void Break(int score, bool force = false)
	{
		if (!Broken)
		{
			//Debug.Log($"Brick break x: {x}, y: {y}");
			//Debug.Log($"Brick Disabled");
			if (BrickProperties.NextBrickId == 0 || force)
			{
				Broken = true;
				if (BrickProperties.RequiredToComplete)
					GameManager.Instance.DecrementRequiredBricks();
				GetComponent<Collider2D>().enabled = false;
				if (BrickProperties.IsExplosive)
				{
					StartCoroutine(WaitForAWhile());
					//Debug.Break();
					levelSoundLibrary.PlaySfx(levelSoundLibrary.bang);
					MakeExplosion(BrickProperties.ExplosionRadius);
				}
				else
					levelSoundLibrary.PlaySfx(levelSoundLibrary.normalBrickBreak);
				if (BrickProperties.IsDetonator)
					GameManager.Instance.DetonateBrick(BrickProperties.DetonateId);
				//TODO try making distinctive trigger on destroy or hit
				if (BrickProperties.FuseDirection != Direction.None && TriggersOnDestroy(BrickProperties.FuseTrigger))
					StartCoroutine(PrepareForFuseBurn());
				//TODO do custom animation support
				StartCoroutine(FadingCoroutine());
			}
			else
			{
				ChangeToNextBrick();
				PrepareBrickAnimation();
			}
			GameManager.Instance.AddToScore(score);
		}
	}
	private IEnumerator WaitForAWhile()
	{
		yield return new WaitForSeconds(0.2f);
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
			GetComponent<SpriteRenderer>().sprite = brickType.Sprites[currentAnimationSpriteId];
			currentAnimationSpriteId = (currentAnimationSpriteId + 1) % brickType.Sprites.Length;
			yield return new WaitForSeconds(0.05f);
		}
	}

	public IEnumerator FadingCoroutine()
	{
		while (GetComponent<SpriteRenderer>().color.a > 0)
		{
			Color color = GetComponent<SpriteRenderer>().color;

			float newAlpha = color.a - 3.0f * Time.deltaTime;

			color = new Color(color.r, color.g, color.b, newAlpha);

			GetComponent<SpriteRenderer>().color = color;

			yield return null;
		}
		GameManager.Instance.DisposeBrick(this);
	}

	public bool HaveEqualCoordinates(int x, int y) => this.x == x && this.y == y;

	public bool TriggersOnHit(EffectTrigger effectTrigger) => effectTrigger == EffectTrigger.Hit || effectTrigger == EffectTrigger.Both;

	public bool TriggersOnDestroy(EffectTrigger effectTrigger) => effectTrigger == EffectTrigger.Destroy || effectTrigger == EffectTrigger.Both;
}
