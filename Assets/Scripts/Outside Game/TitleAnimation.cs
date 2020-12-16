using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TitleAnimation : MonoBehaviour
{
	[SerializeField]
	private TextMeshProUGUI titleObject;

	// Start is called before the first frame update
	void Start() => StartCoroutine(AnimateTitle());

	private IEnumerator AnimateTitle()
    {
		Gradient hueGradient = CreateWholeHueGradient();
		float angleChange = Random.Range(0, 6.28f);
		float gradientTime = Random.Range(0, 1.0f);
		Material textMat = new Material(titleObject.fontMaterial);

		while (true)
		{
			//titleObject.fontMaterial = textMat;
			titleObject.fontMaterial.SetFloat("_LightAngle", angleChange);
			titleObject.color = hueGradient.Evaluate(gradientTime);
			yield return new WaitForSeconds(Time.deltaTime * 4.0f);
			angleChange -= 0.3f;
			if (angleChange < 0)
				angleChange = 6.28f;
			gradientTime += 0.01f;
			if (gradientTime > 1.0f)
				gradientTime = 0;
		}
    }

	private Gradient CreateWholeHueGradient()
	{
		GradientColorKey[] hueGradientColorKeys = new GradientColorKey[7];
		hueGradientColorKeys[0] = new GradientColorKey(Color.red, 0.0f);
		hueGradientColorKeys[1] = new GradientColorKey(Color.yellow, 0.167f);
		hueGradientColorKeys[2] = new GradientColorKey(Color.green, 0.334f);
		hueGradientColorKeys[3] = new GradientColorKey(Color.cyan, 0.511f);
		hueGradientColorKeys[4] = new GradientColorKey(Color.blue, 0.678f);
		hueGradientColorKeys[5] = new GradientColorKey(Color.magenta, 0.845f);
		hueGradientColorKeys[6] = new GradientColorKey(Color.red, 1.0f);
		return new Gradient()
		{
			colorKeys = hueGradientColorKeys
		};
	}
}
