using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
	private Dictionary<string, Sprite> displaySprites;
	public bool Paused { get; private set; }

	[SerializeField]
#pragma warning disable CS0649 // Field 'HUDManager.singlePowerUpDisplayPrefab' is never assigned to, and will always have its default value null
	private GameObject singlePowerUpDisplayPrefab;
#pragma warning restore CS0649 // Field 'HUDManager.singlePowerUpDisplayPrefab' is never assigned to, and will always have its default value null
	[SerializeField]
#pragma warning disable CS0649 // Field 'HUDManager.levelPowerUpDisplayPrefab' is never assigned to, and will always have its default value null
	private GameObject levelPowerUpDisplayPrefab;
#pragma warning restore CS0649 // Field 'HUDManager.levelPowerUpDisplayPrefab' is never assigned to, and will always have its default value null

	[SerializeField]
#pragma warning disable CS0649 // Field 'HUDManager.bigBallDisplaySprite' is never assigned to, and will always have its default value null
	private Sprite bigBallDisplaySprite;
#pragma warning restore CS0649 // Field 'HUDManager.bigBallDisplaySprite' is never assigned to, and will always have its default value null
	[SerializeField]
#pragma warning disable CS0649 // Field 'HUDManager.megaBallDisplaySprite' is never assigned to, and will always have its default value null
	private Sprite megaBallDisplaySprite;
#pragma warning restore CS0649 // Field 'HUDManager.megaBallDisplaySprite' is never assigned to, and will always have its default value null
	[SerializeField]
#pragma warning disable CS0649 // Field 'HUDManager.magnetDisplaySprite' is never assigned to, and will always have its default value null
	private Sprite magnetDisplaySprite;
#pragma warning restore CS0649 // Field 'HUDManager.magnetDisplaySprite' is never assigned to, and will always have its default value null
	[SerializeField]
#pragma warning disable CS0649 // Field 'HUDManager.explosiveBallDisplaySprite' is never assigned to, and will always have its default value null
	private Sprite explosiveBallDisplaySprite;
#pragma warning restore CS0649 // Field 'HUDManager.explosiveBallDisplaySprite' is never assigned to, and will always have its default value null
	[SerializeField]
#pragma warning disable CS0649 // Field 'HUDManager.penetratingBallDisplaySprite' is never assigned to, and will always have its default value null
	private Sprite penetratingBallDisplaySprite;
#pragma warning restore CS0649 // Field 'HUDManager.penetratingBallDisplaySprite' is never assigned to, and will always have its default value null
	[SerializeField]
#pragma warning disable CS0649 // Field 'HUDManager.descendingBricksDisplaySprite' is never assigned to, and will always have its default value null
	private Sprite descendingBricksDisplaySprite;
#pragma warning restore CS0649 // Field 'HUDManager.descendingBricksDisplaySprite' is never assigned to, and will always have its default value null
	[SerializeField]
#pragma warning disable CS0649 // Field 'HUDManager.shooterDisplaySprite' is never assigned to, and will always have its default value null
	private Sprite shooterDisplaySprite;
#pragma warning restore CS0649 // Field 'HUDManager.shooterDisplaySprite' is never assigned to, and will always have its default value null
	[SerializeField]
#pragma warning disable CS0649 // Field 'HUDManager.protectiveBarrierDisplaySprite' is never assigned to, and will always have its default value null
	private Sprite protectiveBarrierDisplaySprite;
#pragma warning restore CS0649 // Field 'HUDManager.protectiveBarrierDisplaySprite' is never assigned to, and will always have its default value null
	[SerializeField]
#pragma warning disable CS0649 // Field 'HUDManager.megaMissileDisplaySprite' is never assigned to, and will always have its default value null
	private Sprite megaMissileDisplaySprite;
#pragma warning restore CS0649 // Field 'HUDManager.megaMissileDisplaySprite' is never assigned to, and will always have its default value null

	[SerializeField]
#pragma warning disable CS0649 // Field 'HUDManager.LevelNameDisplay' is never assigned to, and will always have its default value null
	private GameObject LevelNameDisplay;
#pragma warning restore CS0649 // Field 'HUDManager.LevelNameDisplay' is never assigned to, and will always have its default value null
	[SerializeField]
#pragma warning disable CS0649 // Field 'HUDManager.PAUSEDisplay' is never assigned to, and will always have its default value null
	private GameObject PAUSEDisplay;
#pragma warning restore CS0649 // Field 'HUDManager.PAUSEDisplay' is never assigned to, and will always have its default value null

	private void Start()
	{
		displaySprites = new Dictionary<string, Sprite>
		{
			//{"BigBallDisplay", bigBallDisplaySprite },
			//{"MegaBallDisplay", megaBallDisplaySprite },
			{"BallSizeDisplay", bigBallDisplaySprite },
			{"MagnetDisplay", magnetDisplaySprite },
			{"ExplosiveBallDisplay", explosiveBallDisplaySprite },
			{"PenetratingBallDisplay", penetratingBallDisplaySprite },
			{"DescendingBricksDisplay", descendingBricksDisplaySprite },
			{"ShooterDisplay", shooterDisplaySprite },
			{"ProtectiveBarrierDisplay", protectiveBarrierDisplaySprite  },
			{"MegaMissileDisplay", megaMissileDisplaySprite }
		};
	}

	public void AddDisplay(string displayKey)
	{
		if (!transform.Find(displayKey))
		{
			GameObject HUDDisplay = Instantiate(singlePowerUpDisplayPrefab, gameObject.transform);
			HUDDisplay.transform.Find("Icon").GetComponent<Image>().sprite = displaySprites[displayKey];
			HUDDisplay.name = displayKey;
		}
	}

	public void AddOrUpdateDisplay(string displayKey, int value)
	{
		GameObject HUDDisplay = transform.Find(displayKey)?.gameObject;
		if (!HUDDisplay)
		{
			HUDDisplay = Instantiate(levelPowerUpDisplayPrefab, gameObject.transform);
			HUDDisplay.transform.Find("Icon").GetComponent<Image>().sprite = displaySprites[displayKey];
			HUDDisplay.name = displayKey;
		}
		HUDDisplay.transform.Find("Number").GetComponent<Text>().text = value.ToString();
	}

	public void AddOrUpdateBallSizeDisplay(string displayKey, BallSize ballSize)
	{
		GameObject HUDDisplay = transform.Find(displayKey)?.gameObject;
		if (!transform.Find(displayKey) && !HUDDisplay)
		{
			HUDDisplay = Instantiate(singlePowerUpDisplayPrefab, gameObject.transform);
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
			default:
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
