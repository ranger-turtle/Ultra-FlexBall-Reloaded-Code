using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ZapRenderer : MonoBehaviour
{
#pragma warning disable CS0649
	[SerializeField]
	private GameObject electrodeOne;
	[SerializeField]
	private GameObject electrodeTwo;
	private LineRenderer lineRenderer;
#pragma warning restore CS0649 

	private const float maxVerticalRange = 9.0f / 48.0f;
	private const float maxLength = 512.0f / 48.0f;

	private const float maxFragmentLength = 100.0f / 48.0f;
	private const float minFragmentLength = 20.0f / 48.0f;

	private const int middlePointCount = 10;

	[SerializeField]
	private AnimationCurve curve;

	// Start is called before the first frame update
	void Start()
	{
		lineRenderer = GetComponent<LineRenderer>();
		StartCoroutine(Animate());
	}

	private void OnEnable() => Start();

	// Update is called once per frame
	private IEnumerator Animate()
    {
		while (true)
		{
			Vector3[] vertices = new Vector3[middlePointCount + 2];
			vertices[0] = new Vector3(electrodeOne.transform.position.x, electrodeOne.transform.position.y, 2.0f);
			vertices[vertices.Length - 1] = new Vector3(electrodeTwo.transform.position.x, electrodeTwo.transform.position.y, 2.0f);
			Vector3 firstVertex = vertices[0];
			Vector3 lastVertex = vertices[vertices.Length - 1];
			if (firstVertex.x < lastVertex.x)
			{
				float actualLengthToMaxLengthRatio = (lastVertex.x - firstVertex.x) / maxLength;
				float yFactor = curve.Evaluate(actualLengthToMaxLengthRatio);
				float zapXPosition = vertices[0].x;
				float zapYPosition = 0;
				for (int i = 1; i < vertices.Length - 1; i++)
				{
					float increment = Random.Range(minFragmentLength, maxFragmentLength) * actualLengthToMaxLengthRatio; 
					while (zapXPosition + increment > lastVertex.x)
						increment /= 2.0f;
					zapXPosition += increment;
					zapYPosition = firstVertex.y + Random.Range(-maxVerticalRange, maxVerticalRange) * yFactor;
					vertices[i] = new Vector3(zapXPosition, zapYPosition, 2.0f);
				}
				lineRenderer.positionCount = vertices.Length;
				lineRenderer.SetPositions(vertices);
			}
			yield return new WaitForSeconds(0.05f);
		}
    }

	//BONUS Try making last nodes not too close to the last point
}
