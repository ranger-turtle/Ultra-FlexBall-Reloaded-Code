using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class SpaceDjoelManager : MonoBehaviour
{
	#region Singleton

	public static SpaceDjoelManager Instance { get; private set; }

	void Awake()
	{
		if (Instance)
			Destroy(Instance);
		else
			Instance = this;
	}
	#endregion

	private List<GameObject> spaceDjoels;

	[SerializeField]
	private GameObject spaceDjoelPrefab;

	[SerializeField]
	private Transform leftBound;
	[SerializeField]
	private Transform rightBound;
	[SerializeField]
	private Transform generationYObject;

	internal const float initialForce = 0.05f;
	internal const float paddleBounceForce = 0.17f;

	public int SpaceDjoelNumber => spaceDjoels.Count;

	private float djoelGenerationY;
	private float djoelInstantiateY;

	public void Start()
	{
		djoelInstantiateY = generationYObject.position.y;
		spaceDjoels = new List<GameObject>();
	}

	public void GenerateDjoels()
	{
		Bounds djoelColliderBounds = spaceDjoelPrefab.GetComponent<BoxCollider2D>().bounds;
		float djoelWidth = djoelColliderBounds.size.x;
		for (int i = 0; i < 2; i++)
		{
			float djoelX = UnityEngine.Random.Range(leftBound.position.x + djoelColliderBounds.extents.x + 0.5f, rightBound.position.x - djoelColliderBounds.extents.x - 0.5f);
			Vector3 startingPosition = new Vector3(djoelX, djoelInstantiateY);
			GameObject spaceDjoel = Instantiate(spaceDjoelPrefab, startingPosition, Quaternion.identity);
			spaceDjoels.Add(spaceDjoel);
		}
	}

	public void ResetDjoels() => spaceDjoels.Clear();

	public void Remove(GameObject spaceDjoelObj)
	{
		Destroy(spaceDjoelObj);
		spaceDjoels.Remove(spaceDjoelObj);
		GameManager.Instance.CheckForLosePaddle();
	}
}
