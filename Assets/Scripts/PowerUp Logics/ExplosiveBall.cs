using UnityEngine;

public class ExplosiveBall : PowerUp
{
	protected override void TriggerAction()
	{
		base.TriggerAction();
		GameManager.Instance.ExplosiveBall = true;
		BallManager.Instance.ApplyParticlesToExplosiveBalls();
		Debug.Log("Explosive Ball");
	}
}
