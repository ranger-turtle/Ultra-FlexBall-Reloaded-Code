using UnityEngine;

public class ExplosiveMultiplier : PowerUp
{
	protected override void TriggerAction()
	{
		base.TriggerAction();
		GameManager.Instance.MultiplyExplosives();
		Debug.Log("Explosive Multiplier");
	}
}
