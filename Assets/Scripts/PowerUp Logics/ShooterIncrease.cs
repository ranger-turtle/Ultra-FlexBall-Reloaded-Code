using UnityEngine;

public class ShooterIncrease : PowerUp
{
	public override void TriggerAction()
	{
		base.TriggerAction();
		GameManager.Instance.IncreaseShooterLevel();
		Debug.Log("Shooter");
	}
}
