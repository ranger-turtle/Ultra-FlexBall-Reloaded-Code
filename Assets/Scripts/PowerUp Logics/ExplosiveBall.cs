using UnityEngine;

public class ExplosiveBall : PowerUp
{
	public override void TriggerAction()
	{
		base.TriggerAction();
		GameManager.Instance.ExplosiveBall = true;
		Debug.Log("Explosive Ball");
	}
}
