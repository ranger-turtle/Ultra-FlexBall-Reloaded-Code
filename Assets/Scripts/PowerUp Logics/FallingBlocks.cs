using UnityEngine;

public class FallingBlocks : PowerUp
{
	public override void TriggerAction()
	{
		base.TriggerAction();
		GameManager.Instance.DescendingBricks = true;
		Debug.Log("Falling Blocks");
	}
}
