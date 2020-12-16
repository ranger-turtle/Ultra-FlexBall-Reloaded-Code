using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LevelSetData;
using System.Linq;

public class ParticleManager : MonoBehaviour
{
	#region Singleton
	public static ParticleManager Instance { get; private set; }

	void Awake()
	{
		if (Instance)
			Destroy(Instance);
		else
			Instance = this;
	}

	private const string ExplodingBallParticleObjectName = "Exploding Ball Particles";
	private const string PenetratingBallParticleObjectName = "Penetrating Ball Particles";
	private const string ThrustedBallParticleObjectName = "Thrusted Ball Flame";
	#endregion

	[SerializeField]
	private ParticleSystem VerticalParticleSystemPrefab;
	[SerializeField]
	private ParticleSystem SprinklerParticleSystemPrefab;

	[SerializeField]
	private ParticleSystem BrickHitParticleSystemPrefab;
	[SerializeField]
	private ParticleSystem SpecialHitParticleSystemPrefab;

	[SerializeField]
	private ParticleSystem ShootParticleSystemPrefab;

	[SerializeField]
	private ParticleSystem ExplosiveParticleSystemPrefab;
	[SerializeField]
	private ParticleSystem PenetratingParticleSystemPrefab;

	[SerializeField]
	private ParticleSystem ThrustedBallFlamePowerUpHaloPrefab;

	[SerializeField]
	private ParticleSystem HelperPowerUpHaloPrefab;

	[SerializeField]
	private ParticleSystem PaddleSideHitSparkles;
	[SerializeField]
	private ParticleSystem HighSpeedPaddleBounceSparkles;

	private readonly Gradient redBlueThrustGradient = new Gradient();
	private readonly Gradient yellowCyanThrustGradient = new Gradient();

	private Dictionary<int, Gradient> brickParticleGradients;

	public void Start()
	{
		GradientColorKey[] redBlueGradientColorKeys = new GradientColorKey[2];
		redBlueGradientColorKeys[0] = new GradientColorKey(Color.red, 0.0f);
		redBlueGradientColorKeys[1] = new GradientColorKey(Color.blue, 1.0f);
		redBlueThrustGradient.colorKeys = redBlueGradientColorKeys;
		GradientColorKey[] yellowCyanGradientColorKeys = new GradientColorKey[2];
		yellowCyanGradientColorKeys[0] = new GradientColorKey(Color.yellow, 0.0f);
		yellowCyanGradientColorKeys[1] = new GradientColorKey(Color.cyan, 1.0f);
		yellowCyanThrustGradient.colorKeys = yellowCyanGradientColorKeys;
	}

	public void CreateBrickParticles(IEnumerable<BrickType> brickProperties)
	{
		brickParticleGradients = brickProperties.Where(b => b.Properties.IsChimneyLike && b.Properties.ChimneyColourSchemeType == ChimneyColourSchemeType.TwoColours).ToDictionary(k => k.Properties.Id, v => GenerateBrickParticleGradient(v.Properties));
	}

	public ParticleSystem GenerateBrickParticle(BrickProperties brickProperties)
	{
		ParticleSystem particleSystem;
		switch (brickProperties.ChimneyType)
		{
			case ChimneyType.Vertical:
				particleSystem = Instantiate(VerticalParticleSystemPrefab);
				break;
			case ChimneyType.Sprinkling:
				particleSystem = Instantiate(SprinklerParticleSystemPrefab);
				break;
			default:
				return null;
		}
		if (brickProperties.ChimneyColourSchemeType == ChimneyColourSchemeType.TwoColours)
		{
			Gradient gradient = GetBrickParticleGradient(brickProperties.Id);
			ParticleSystem.MainModule mainModule = particleSystem.main;
			mainModule.startColor = new ParticleSystem.MinMaxGradient(gradient) { mode = ParticleSystemGradientMode.RandomColor };
		}
		return particleSystem;
	}

	private Gradient GenerateBrickParticleGradient(BrickProperties brickProperties)
	{
		Color color1 = new Color(brickProperties.Color1.Red / 255.0f, brickProperties.Color1.Green / 255.0f, brickProperties.Color1.Blue / 255.0f);
		Color color2 = new Color(brickProperties.Color2.Red / 255.0f, brickProperties.Color2.Green / 255.0f, brickProperties.Color2.Blue / 255.0f);
		Gradient gradient = new Gradient();
		GradientColorKey[] gradientColorKey = new GradientColorKey[2];
		gradientColorKey[0].color = color1;
		gradientColorKey[0].time = 0;
		gradientColorKey[1].color = color2;
		gradientColorKey[1].time = 1.0f;
		gradient.SetKeys(gradientColorKey, new GradientAlphaKey[2] { new GradientAlphaKey(1.0f, 0), new GradientAlphaKey(1.0f, 1.0f) });
		return gradient;
	}

	private Gradient GetBrickParticleGradient(int brickId) => brickParticleGradients[brickId];

	public void GenerateBrickHitEffect(Vector3 brickHitPosition, Vector2 normal)
	{
		ParticleSystem particleSystem = Instantiate(BrickHitParticleSystemPrefab, brickHitPosition, Quaternion.identity);
		ParticleSystem.ShapeModule shapeModule = particleSystem.shape;
		float angle = 0;
		if (normal.x > 0)
			angle = 0;
		else if (normal.y < 0)
			angle = 90;
		else if (normal.x < 0)
			angle = 180;
		else if (normal.y > 0)
			angle = 270;
		shapeModule.rotation = new Vector3(angle, shapeModule.rotation.y, shapeModule.rotation.z);
		Destroy(particleSystem.gameObject, particleSystem.main.startLifetime.constant);
	}
	
	public void GenerateSpecialHitEffect(Vector3 specialHitPosition)
	{
		SoundManager.Instance.PlaySfx("Special Hit");
		ParticleSystem particleSystem = Instantiate(SpecialHitParticleSystemPrefab, specialHitPosition, Quaternion.identity);
		Destroy(particleSystem.gameObject, particleSystem.main.startLifetime.constant);
	}
	
	public void GenerateShootEffect(Vector3 shootPosition)
	{
		ParticleSystem particleSystem = Instantiate(ShootParticleSystemPrefab, shootPosition, Quaternion.identity);
		Destroy(particleSystem.gameObject, particleSystem.main.startLifetime.constant);
	}

	private void GenerateBallParticles(ParticleSystem ballParticleSystem, GameObject ball, BallSize ballSize, string name)
	{
		if (!ball.transform.Find(name))
		{
			ParticleSystem particleSystem = Instantiate(ballParticleSystem, ball.transform);
			particleSystem.gameObject.name = name;
			ParticleSystem.ShapeModule shapeModule = particleSystem.shape;
			shapeModule.radius = GetRadiusBasedOnBallSize(ballSize);
		}
	}

	private float GetRadiusBasedOnBallSize(BallSize ballSize)
	{
		switch (ballSize)//BONUS change to switch expression after C# version upgrade
		{
			case BallSize.Normal:
				return 0.09f;
			case BallSize.Big:
				return 0.15f;
			case BallSize.Megajocke:
				return 0.78f;
			default:
				return 0;
		}
	}

	public void GenerateExplosiveBallParticles(GameObject ball, BallSize ballSize)
		=> GenerateBallParticles(ExplosiveParticleSystemPrefab, ball, ballSize, ExplodingBallParticleObjectName);

	public void GeneratePenetratingBallParticles(GameObject ball, BallSize ballSize)
		=> GenerateBallParticles(PenetratingParticleSystemPrefab, ball, ballSize, PenetratingBallParticleObjectName);

	private void UpdateBallParticles(GameObject ball, string name, BallSize ballSize)
	{
		GameObject particles = ball.transform.Find(name)?.gameObject;
		if (particles)
		{
			ParticleSystem particleSystem = particles.GetComponent<ParticleSystem>();
			ParticleSystem.ShapeModule shapeModule = particleSystem.shape;
			shapeModule.radius = GetRadiusBasedOnBallSize(ballSize);
			//TODO increase particle rate with ball size
		}
	}

	//I have had to add parameter "ballsize" cause ball GameObject has not been updated yet when this is called
	public void UpdateAllBallParticles(GameObject ball, BallSize ballSize)
	{
		UpdateBallParticles(ball, ExplodingBallParticleObjectName, ballSize);
		UpdateBallParticles(ball, PenetratingBallParticleObjectName, ballSize);
	}

	private void DestroyBallParticles(GameObject ball, string name)
	{
		GameObject particles = ball.transform.Find(name)?.gameObject;
		if (particles)
		{
			ParticleSystem particleSystem = particles.GetComponent<ParticleSystem>();
			particleSystem.Stop(false, ParticleSystemStopBehavior.StopEmitting);
			particles.transform.SetParent(null);
			Destroy(particles, particleSystem.main.startLifetime.constant);
		}
	}

	public void RemoveBallUpgradeParticles(GameObject ball)
	{
		DestroyBallParticles(ball, ExplodingBallParticleObjectName);
		DestroyBallParticles(ball, PenetratingBallParticleObjectName);
	}
	
	public void GenerateHelperPowerUpHalo()
	{
		ParticleSystem particleSystem = Instantiate(HelperPowerUpHaloPrefab);
		Destroy(particleSystem.gameObject, particleSystem.main.startLifetime.constant);
	}

	public IEnumerator GenerateThrustedBallFlame(GameObject ball, float seconds, float playbackPosition)
	{
		ParticleSystem flame = Instantiate(ThrustedBallFlamePowerUpHaloPrefab, ball.transform);
		flame.gameObject.name = ThrustedBallParticleObjectName;
		flame.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
		ParticleSystem.MainModule mainModule = flame.main;
		mainModule.duration = seconds;
		flame.Play();
		flame.time = playbackPosition;
		ParticleSystem.ShapeModule shapeModule = flame.shape;
		shapeModule.rotation = new Vector3(shapeModule.rotation.x, Random.Range(0, 360.0f), shapeModule.rotation.z);
		float step = Time.deltaTime;
		while (flame && flame.time < flame.main.duration)
		{
			float gradientPosition = flame.time * (1.0f / flame.main.duration);
			ParticleSystem.MinMaxGradient gradient = new ParticleSystem.MinMaxGradient(redBlueThrustGradient.Evaluate(gradientPosition), yellowCyanThrustGradient.Evaluate(gradientPosition));
			mainModule.startColor = gradient;
			yield return new WaitForSeconds(step);
		}
		if (flame)
		{
			ball.GetComponent<Ball>().FinishThrust(force: false);
			Destroy(flame.gameObject, flame.main.startLifetime.constant);
		}
	}

	public void RemoveThrustingFlame(GameObject ball)
	{
		GameObject particles = ball.transform.Find(ThrustedBallParticleObjectName)?.gameObject;
		if (particles)
		{
			ParticleSystem particleSystem = particles.GetComponent<ParticleSystem>();
			particleSystem.Stop();
			Destroy(particles, particleSystem.main.startLifetime.constant);// + (particleSystem.main.duration - particleSystem.time));
		}
	}

	public void GeneratePaddleSideHitSparkles(Vector3 position)
	{
		ParticleSystem particles = Instantiate(PaddleSideHitSparkles, position, Quaternion.identity);
		Destroy(particles.gameObject, particles.main.startLifetime.constant);
	}

	public void GenerateHighSpeedPaddleBounceSparkles(Vector3 position)
	{
		ParticleSystem particles = Instantiate(HighSpeedPaddleBounceSparkles, position, Quaternion.identity);
		Destroy(particles.gameObject, particles.main.startLifetime.constant);
	}
}
