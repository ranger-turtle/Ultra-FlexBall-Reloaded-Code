using UnityEngine;

public class MegaSplit : PowerUp
{
	public override void TriggerAction()
	{
		base.TriggerAction();
		BallManager.Instance.MegaSplit();
		Debug.Log("Mega Split");
	}
}
