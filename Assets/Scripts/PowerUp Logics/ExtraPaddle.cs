using UnityEngine;

public class ExtraPaddle : PowerUp
{
	public override void TriggerAction()
	{
		base.TriggerAction();
		GameManager.Instance.AddExtraPaddle();
		Debug.Log("Extra Paddle");
	}
}
