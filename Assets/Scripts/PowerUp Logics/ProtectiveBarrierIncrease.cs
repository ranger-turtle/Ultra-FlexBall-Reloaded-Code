using UnityEngine;

public class ProtectiveBarrierIncrease : PowerUp
{
	protected override void TriggerAction()
	{
		base.TriggerAction();
		GameManager.Instance.IncreaseProtectiveBarrierLevel();
		Debug.Log("Protective Barrier");
	}
}
