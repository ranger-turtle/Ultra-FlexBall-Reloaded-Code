using UnityEngine;

public class MegaSplit : PowerUp
{
	protected override void TriggerAction()
	{
		base.TriggerAction();
		BallManager.Instance.MegaSplit();
		Debug.Log("Mega Split");
	}
}
