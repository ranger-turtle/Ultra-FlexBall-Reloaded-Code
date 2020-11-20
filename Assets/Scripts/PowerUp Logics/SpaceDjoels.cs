using UnityEngine;

public class SpaceDjoels : PowerUp
{
	protected override void TriggerAction()
	{
		base.TriggerAction();
		SpaceDjoelManager.Instance.GenerateDjoels();
		Debug.Log("Space Djoel");
	}
}
