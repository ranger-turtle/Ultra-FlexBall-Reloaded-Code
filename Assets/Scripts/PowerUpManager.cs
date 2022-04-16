using LevelSetData;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PowerUpManager : MonoBehaviour
{
	#region Singleton
	public static PowerUpManager Instance { get; private set; }

	void Awake()
	{
		if (Instance)
			Destroy(Instance);
		else
			Instance = this;
	}

	private const int maxSeconds = 60;
	#endregion

	[SerializeField]
#pragma warning disable CS0649 // Field 'PowerUpManager.powerUpPrefabs' is never assigned to, and will always have its default value null
	private GameObject[] powerUpPrefabs;
#pragma warning restore CS0649 // Field 'PowerUpManager.powerUpPrefabs' is never assigned to, and will always have its default value null
	[SerializeField]
#pragma warning disable CS0649 // Field 'PowerUpManager.powerUpMeterFill' is never assigned to, and will always have its default value null
	private Image powerUpMeterFill;
#pragma warning restore CS0649 // Field 'PowerUpManager.powerUpMeterFill' is never assigned to, and will always have its default value null

	private int secondsLeft = maxSeconds;

	private int powerUpMeter;

	private bool timeLimit;

	private void Start() => StartCoroutine(CountToYieldHelperPowerUp());

	internal void Reset()
	{
		powerUpMeter = 0;
		secondsLeft = maxSeconds;
		UpdatePowerUpMeterFill();
	}

	private void UpdatePowerUpMeterFill()
	{
		float fillPercent = powerUpMeter / 100.0f;
		powerUpMeterFill.fillAmount = fillPercent;
	}

	/// <summary>
	/// Increases power up meter.
	/// 
	/// <para>Returns true if increase resulted in power up yield.</para>
	/// </summary>
	/// <param name="increment"></param>
	/// <param name="position"></param>
	/// <param name="velocity"></param>
	/// <returns>True if increase resulted in power up yield.</returns>
	public bool IncreaseMeter(int increment, Vector3 position, Vector3 velocity, YieldedPowerUp powerUpType)
	{
		bool yield = false;
		powerUpMeter += increment;
		if (powerUpMeter > 100 && !timeLimit)
		{
			powerUpMeter = 0;
			YieldPowerUp(position, velocity, powerUpType);
			yield = true;
		}
		UpdatePowerUpMeterFill();
		return yield;
	}

	public void YieldPowerUp(Vector2 position, Vector3 velocity, YieldedPowerUp powerUpType = YieldedPowerUp.Any, int score = 500)
	{
		//Debug.Break();
		int powerUpIndex = powerUpType == YieldedPowerUp.Any ? Random.Range(0, powerUpPrefabs.Length) : (int)powerUpType;//BONUS do complex index randomization
		//int powerUpIndex = (int)YieldedPowerUp.MegaSplit;
		//int[] indices = new int[] { 2, 3, 4 };
		//int powerUpIndex = indices[Random.Range(0, indices.Length)];
		Vector3 powerUpPosition = new Vector3(position.x, position.y, -3);
		GameObject powerUp = Instantiate(powerUpPrefabs[powerUpIndex], powerUpPosition, Quaternion.identity);
		powerUp.GetComponent<PowerUp>().CurrentVelocity = velocity;
		SoundManager soundManager = SoundManager.Instance;
		soundManager.PlaySfx("Power Up Yield");
		powerUp.GetComponent<PowerUp>().Score = score;
		StartCoroutine(SuppresYieldGenerationForAMoment());
	}

	public void YieldHelperPowerUp()
	{
		int[] indices = new int[] { (int)YieldedPowerUp.BrickDescend, (int)YieldedPowerUp.SpaceDjoel, (int)YieldedPowerUp.Shooter, (int)YieldedPowerUp.MegaSplit };
		int powerUpIndex = indices[Random.Range(0, indices.Length)];
		GameObject powerUp = Instantiate(powerUpPrefabs[powerUpIndex]);
		powerUp.GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0.05f);
		SoundManager soundManager = SoundManager.Instance;
		soundManager.PlaySfx("Power Up Yield");
		powerUp.GetComponent<PowerUp>().Score = 0;
	}

	public void ResetHelpCountdown() => secondsLeft = maxSeconds;

	public IEnumerator CountToYieldHelperPowerUp()
	{
		while (true)
		{
			secondsLeft--;
			yield return new WaitForSeconds(1.0f);
			if (secondsLeft == 0)
			{
				GameManager.Instance.AddToScore(-500);
				ParticleManager.Instance.GenerateHelperPowerUpHalo();
				YieldHelperPowerUp();
				Reset();
			}
		}
	}

	public IEnumerator SuppresYieldGenerationForAMoment()
	{
		timeLimit = true;
		yield return new WaitForSeconds(1.0f);
		timeLimit = false;
	}
}
