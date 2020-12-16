using System;
using UnityEngine;
using UnityEngine.UI;

public class MessageManager : MonoBehaviour
{
	[SerializeField]
	private Text characterName;
	[SerializeField]
	private Image avatar;
	[SerializeField]
	private Text characterMessage;
	[SerializeField]
	private Toggle NeverUseTipToggle;

	[SerializeField]
	private Texture2D cursorTexture;

	private bool canShowTips;

	public void Show(string characterName, Sprite avatar, string message, bool isTip)
	{
		canShowTips = SettingsManager.LoadBool("showTips", true);
		if (!isTip || canShowTips)
		{
			this.characterName.text = characterName;
			this.avatar.sprite = avatar;
			characterMessage.text = message;
			NeverUseTipToggle.gameObject.SetActive(isTip);
			gameObject.SetActive(true);
			Cursor.visible = true;
			Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.Auto);
		}
	}

	public void ToggleTip()
	{
		SettingsManager.SaveBool("showTips", !NeverUseTipToggle.isOn);
	}

	// Start is called before the first frame update
	void OnEnable() => Time.timeScale = 0;

	private void OnDisable() => Time.timeScale = 1;

	// Update is called once per frame
	void Update()
    {
		if (gameObject.activeSelf && Input.anyKey && !Input.GetMouseButton(0))
		{
			Cursor.visible = false;
			Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
			gameObject.SetActive(false);
		}
	}

	internal void ToggleShowTips()
	{
		bool newValue = canShowTips = !canShowTips;
		SettingsManager.SaveBool("showTips", newValue);
	}
}
