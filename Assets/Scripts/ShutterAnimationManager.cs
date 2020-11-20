using System;
using System.Collections;
using UnityEngine;

public class ShutterAnimationManager : MonoBehaviour
{
	[SerializeField]
	private bool covered = true;

	public bool Covered => covered;
	public bool Uncovered { get; private set; } = true;

	[SerializeField]
#pragma warning disable CS0649 // Field 'ShutterAnimationManager.coverMaskTransform' is never assigned to, and will always have its default value null
	private RectTransform coverMaskTransform;
#pragma warning restore CS0649 // Field 'ShutterAnimationManager.coverMaskTransform' is never assigned to, and will always have its default value null
	[SerializeField]
#pragma warning disable CS0649 // Field 'ShutterAnimationManager.coverTransform' is never assigned to, and will always have its default value null
	private RectTransform coverTransform;
#pragma warning restore CS0649 // Field 'ShutterAnimationManager.coverTransform' is never assigned to, and will always have its default value null
	[SerializeField]
#pragma warning disable CS0649 // Field 'ShutterAnimationManager.canvasTransform' is never assigned to, and will always have its default value null
	private RectTransform canvasTransform;
#pragma warning restore CS0649 // Field 'ShutterAnimationManager.canvasTransform' is never assigned to, and will always have its default value null

	private void Start()
	{
		//coverMaskTransform.sizeDelta = new Vector2(canvasTransform.rect.width * 0.8f, canvasTransform.rect.height * 0.8f);
		coverMaskTransform.sizeDelta = new Vector2(0, 0);
		coverTransform.sizeDelta = new Vector2(canvasTransform.rect.width, canvasTransform.rect.height);
		Uncover();
		Debug.Log($"Screen Width: {Screen.width}, Screen Height: {Screen.height}");
	}

	public void Cover(IEnumerator doAfterCover)
	{
		if (Uncovered)
		{
			Uncovered = false;
			StartCoroutine(CoverCoroutine());
			StartCoroutine(doAfterCover);
		}
	}

	public void Uncover() => StartCoroutine(UncoverCoroutine());

	private IEnumerator CoverCoroutine()
	{
		float screenWidth = canvasTransform.rect.width;
		float screenHeight = canvasTransform.rect.height;
		float iterations = 30.0f;
		float increment = 1.0f / iterations;
		for (float scale = 1.0f - increment, i = 0; i < iterations; scale -= increment, i++)
		{
			coverMaskTransform.sizeDelta = new Vector2(screenWidth * scale, screenHeight * scale);
			yield return new WaitForSeconds(increment / 4);
		}
		covered = true;
	}

	private IEnumerator UncoverCoroutine()
	{
		covered = false;
		float screenWidth = canvasTransform.rect.width;
		float screenHeight = canvasTransform.rect.height;
		float iterations = 30.0f;
		float increment = 1.0f / iterations;
		for (float scale = 0 + increment, i = 0; i < iterations; scale += increment, i++)
		{
			coverMaskTransform.sizeDelta = new Vector2(screenWidth * scale, screenHeight * scale);
			yield return new WaitForSeconds(increment / 4);
		}
		Uncovered = true;
	}
}
