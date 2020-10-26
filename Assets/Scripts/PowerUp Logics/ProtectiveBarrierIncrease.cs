using UnityEngine;

public class ProtectiveBarrierIncrease : PowerUp
{
	public override void TriggerAction()
	{
		base.TriggerAction();
		GameManager.Instance.IncreaseProtectiveBarrierLevel();
		Debug.Log("Protective Barrier");
	}
}
