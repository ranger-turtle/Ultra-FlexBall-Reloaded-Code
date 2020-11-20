using UnityEngine;

public class RegularMultiplier : PowerUp
{
	protected override void TriggerAction()
	{
		base.TriggerAction();
		GameManager.Instance.MultiplyRegulars();
		Debug.Log("Regular Multiplier");
	}
}
