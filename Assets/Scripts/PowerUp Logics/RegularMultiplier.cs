using UnityEngine;

public class RegularMultiplier : PowerUp
{
	public override void TriggerAction()
	{
		base.TriggerAction();
		GameManager.Instance.MultiplyRegulars();
		Debug.Log("Regular Multiplier");
	}
}
