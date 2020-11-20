using UnityEngine;

public class MagnetPaddle : PowerUp
{
	protected override void TriggerAction()
	{
		base.TriggerAction();
		GameManager.Instance.ActivateMagnet();
		Debug.Log("Magnet Paddle");
	}
}
