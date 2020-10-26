using UnityEngine;

public class PenetratingBall : PowerUp
{
	public override void TriggerAction()
	{
		base.TriggerAction();
		GameManager.Instance.PenetratingBall = true;
		Debug.Log("Penetrating Ball");
	}
}
