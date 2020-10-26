using UnityEngine;

public class MagnetPaddle : PowerUp
{
	public override void TriggerAction()
	{
		base.TriggerAction();
		GameManager.Instance.ActivateMagnet();
		Debug.Log("Magnet Paddle");
	}
}
