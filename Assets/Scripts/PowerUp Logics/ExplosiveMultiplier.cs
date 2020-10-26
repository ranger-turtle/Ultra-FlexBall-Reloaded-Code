using UnityEngine;

public class ExplosiveMultiplier : PowerUp
{
	public override void TriggerAction()
	{
		base.TriggerAction();
		GameManager.Instance.MultiplyExplosives();
		Debug.Log("Explosive Multiplier");
	}
}
