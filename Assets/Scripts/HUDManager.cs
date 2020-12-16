using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
	private Dictionary<string, GameObject> displayPrefabs;
	public bool Paused { get; private set; }

	[SerializeField]
	private GameObject bigBallDisplayPrefab;
	[SerializeField]
	private GameObject magnetDisplayPrefab;
	[SerializeField]
	private GameObject explosiveBallDisplayPrefab;
	[SerializeField]
	private GameObject penetratingBallDisplayPrefab;
	[SerializeField]
	private GameObject descendingBricksDisplayPrefab;
	[SerializeField]
	private GameObject shooterDisplayPrefab;
	[SerializeField]
	private GameObject protectiveBarrierDisplayPrefab;
	[SerializeField]
	private GameObject megaMissileDisplayPrefab;

	[SerializeField]
	private Sprite bigBallDisplaySprite;
	[SerializeField]
	private Sprite megaBallDisplaySprite;

	[SerializeField]
	private GameObject LevelNameDisplay;
	[SerializeField]
	private GameObject PAUSEDisplay;
#pragma warning restore CS0649 // Field 'HUDManager.PAUSEDisplay' is never assigned to, and will always have its default value null

	private void Start()
	{
		displayPrefabs = new Dictionary<string, GameObject>
		{
			//{"BigBallDisplay", bigBallDisplaySprite },
			//{"MegaBallDisplay", megaBallDisplaySprite },
			{"BallSizeDisplay", bigBallDisplayPrefab },
			{"MagnetDisplay", magnetDisplayPrefab },
			{"ExplosiveBallDisplay", explosiveBallDisplayPrefab },
			{"PenetratingBallDisplay", penetratingBallDisplayPrefab },
			{"DescendingBricksDisplay", descendingBricksDisplayPrefab },
			{"ShooterDisplay", shooterDisplayPrefab },
			{"ProtectiveBarrierDisplay", protectiveBarrierDisplayPrefab  },
			{"MegaMissileDisplay", megaMissileDisplayPrefab }
		};
	}

	public void AddDisplay(string displayKey)
	{
		if (!transform.Find(displayKey))
		{
			GameObject HUDDisplay = Instantiate(displayPrefabs[displayKey], gameObject.transform);
			HUDDisplay.name = displayKey;
		}
	}

	public void AddOrUpdateDisplay(string displayKey, int value)
	{
		GameObject HUDDisplay = transform.Find(displayKey)?.gameObject;
		if (!HUDDisplay)
		{
			HUDDisplay = Instantiate(displayPrefabs[displayKey], gameObject.transform);
			HUDDisplay.name = displayKey;
		}
		Transform numberLabel = HUDDisplay.transform.Find("Number");
		if (numberLabel)
			numberLabel.GetComponent<Text>().text = value.ToString();
	}

	public void AddOrUpdateBallSizeDisplay(string displayKey, BallSize ballSize)
	{
		GameObject HUDDisplay = transform.Find(displayKey)?.gameObject;
		if (!transform.Find(displayKey) && !HUDDisplay)
		{
			HUDDisplay = Instantiate(bigBallDisplayPrefab, gameObject.transform);
			HUDDisplay.name = displayKey;
		}
		switch (ballSize)
		{
			case BallSize.Big:
				HUDDisplay.transform.Find("Icon").GetComponent<Image>().sprite = bigBallDisplaySprite;
				break;
			case BallSize.Megajocke:
				HUDDisplay.transform.Find("Icon").GetComponent<Image>().sprite = megaBallDisplaySprite;
				break;
		}
	}

	public void RemoveDisplay(string displayKey)
	{
		GameObject HUDDisplay = transform.Find(displayKey)?.gameObject;
		if (HUDDisplay)
			Destroy(HUDDisplay);
	}

	public void UpdateAndDisplayLevelNameForAMinute(int levelNum, string levelName)
	{
		LevelNameDisplay.SetActive(true);
		LevelNameDisplay.GetComponent<TextMeshProUGUI>().text = $"{levelNum}. {levelName}";
		StartCoroutine(WaitForLevelNameDisappear());
	}

	private IEnumerator WaitForLevelNameDisappear()
	{
		yield return new WaitForSeconds(4);
		LevelNameDisplay.SetActive(false);
	}

	public void Pause()
	{
		Paused = !Paused;
		LevelNameDisplay.SetActive(Paused);
		PAUSEDisplay.SetActive(Paused);
		Time.timeScale = Paused ? 0 : 1;
	}
}
