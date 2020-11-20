using LevelSetData;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

//TODO do auto generate power-up
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

	private const int maxSeconds = 90;
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

	public void IncreaseMeter(int increment, Vector3 position, Vector3 velocity)
	{
		powerUpMeter += increment;
		if (powerUpMeter > 100)
		{
			powerUpMeter = 0;
			YieldPowerUp(position, velocity);
		}
		UpdatePowerUpMeterFill();
	}

	public void YieldPowerUp(Vector2 position, Vector3 velocity, int score = 500)
	{
		//Debug.Break();
		int powerUpIndex = Random.Range(0, powerUpPrefabs.Length);//BONUS do complex index randomization
		//int powerUpIndex = (int)YieldedPowerUp.MegaSplit - 1;
		//int[] indices = new int[] { 2, 3, 4 };
		//int powerUpIndex = indices[Random.Range(0, indices.Length)];
		Vector3 powerUpPosition = new Vector3(position.x, position.y, -3);
		GameObject powerUp = Instantiate(powerUpPrefabs[powerUpIndex], powerUpPosition, Quaternion.identity);
		powerUp.GetComponent<PowerUp>().CurrentVelocity = velocity;
		SoundManager soundManager = SoundManager.Instance;
		soundManager.PlaySfx("Power Up Yield");
		powerUp.GetComponent<PowerUp>().Score = score;
	}

	public void YieldHelperPowerUp()
	{
		//FIXME remove -1's when you assign last number to any
		int[] indices = new int[] { (int)YieldedPowerUp.BrickDescend - 1, (int)YieldedPowerUp.SpaceDjoel - 1, (int)YieldedPowerUp.Shooter - 1, (int)YieldedPowerUp.MegaSplit - 1 };//FIXME change to enum constants after ordering them
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
}
