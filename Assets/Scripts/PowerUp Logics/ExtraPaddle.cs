using UnityEngine;

public class ExtraPaddle : PowerUp
{
	protected override void TriggerAction()
	{
		base.TriggerAction();
		GameManager.Instance.AddExtraPaddle();
		Debug.Log("Extra Paddle");
	}
}
