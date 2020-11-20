using UnityEngine;

public class ShooterIncrease : PowerUp
{
	protected override void TriggerAction()
	{
		base.TriggerAction();
		GameManager.Instance.IncreaseShooterLevel();
		Debug.Log("Shooter");
	}
}
