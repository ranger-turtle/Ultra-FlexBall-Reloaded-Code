using UnityEngine;

public class ExtraMegaMissile : PowerUp
{
	protected override void TriggerAction()
	{
		base.TriggerAction();
		GameManager.Instance.IncreaseMegaMissiles();
		Debug.Log("Mega Missile!");
	}
}
