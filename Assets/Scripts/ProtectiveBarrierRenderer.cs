using UnityEngine;

public class ProtectiveBarrierRenderer : MonoBehaviour
{
	[SerializeField]
#pragma warning disable CS0649 // Field 'ProtectiveBarrierRenderer.variableElectrode' is never assigned to, and will always have its default value null
	private GameObject variableElectrode;
#pragma warning restore CS0649 // Field 'ProtectiveBarrierRenderer.variableElectrode' is never assigned to, and will always have its default value null
	private LineRenderer lineRenderer;
	[SerializeField]
#pragma warning disable CS0649 // Field 'ProtectiveBarrierRenderer.indexOfVertexToChange' is never assigned to, and will always have its default value 0
	private int indexOfVertexToChange;
#pragma warning restore CS0649 // Field 'ProtectiveBarrierRenderer.indexOfVertexToChange' is never assigned to, and will always have its default value 0

	// Start is called before the first frame update
	void Start()
    {
		lineRenderer = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
		Vector3[] vertices = new Vector3[lineRenderer.positionCount];
		int positionNumber = lineRenderer.GetPositions(vertices);
		lineRenderer.SetPosition(indexOfVertexToChange, variableElectrode.transform.position);
    }

	//TODO Make Coroutine procedurally generating lightning effect in base class Lightning Renderer
}
