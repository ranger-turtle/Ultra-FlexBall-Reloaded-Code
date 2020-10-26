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
	#endregion

	[SerializeField]
#pragma warning disable CS0649 // Field 'PowerUpManager.powerUpPrefabs' is never assigned to, and will always have its default value null
	private GameObject[] powerUpPrefabs;
#pragma warning restore CS0649 // Field 'PowerUpManager.powerUpPrefabs' is never assigned to, and will always have its default value null
	[SerializeField]
#pragma warning disable CS0649 // Field 'PowerUpManager.powerUpMeterFill' is never assigned to, and will always have its default value null
	private Image powerUpMeterFill;
#pragma warning restore CS0649 // Field 'PowerUpManager.powerUpMeterFill' is never assigned to, and will always have its default value null

	private float powerUpMeter;

	internal void Reset()
	{
		powerUpMeter = 0;
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

	public void YieldPowerUp(Vector3 position, Vector3 velocity, int score = 500)
	{
		//int powerUpIndex = Random.Range(0, powerUpPrefabs.Length);//BONUS do complex index randomization
		int powerUpIndex = 14;
		//int[] indices = new int[] { 4, 13 };
		//int powerUpIndex = indices[Random.Range(0, indices.Length)];
		Vector3 powerUpPosition = new Vector3(position.x, position.y, -3);
		GameObject powerUp = Instantiate(powerUpPrefabs[powerUpIndex], powerUpPosition, Quaternion.identity);
		powerUp.GetComponent<Rigidbody2D>().velocity = velocity;
		LevelSoundLibrary levelSoundLibrary = LevelSoundLibrary.Instance;
		levelSoundLibrary.PlaySfx(levelSoundLibrary.specialHit);
		levelSoundLibrary.PlaySfx(levelSoundLibrary.powerUpYield);
		powerUp.GetComponent<PowerUp>().Score = score;
	}
}
