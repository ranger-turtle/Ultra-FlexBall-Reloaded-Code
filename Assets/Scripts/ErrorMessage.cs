using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ErrorMessage : MonoBehaviour
{
	[SerializeField]
	private Text text;

	public void Show()
	{
		gameObject.SetActive(true);
	}

	public void Show(string message)
	{
		text.text = message;
		Show();
	}

	// Start is called before the first frame update
	void OnEnable() => Time.timeScale = 0;

	private void OnDisable() => Time.timeScale = 1;

	// Update is called once per frame
	void Update()
    {
		if (Input.anyKey)
			gameObject.SetActive(false);
	}
}
