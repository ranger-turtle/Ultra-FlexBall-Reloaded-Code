using UnityEngine;

public class ProtectiveBarrier : MonoBehaviour
{
	private GameObject leftPaddleElectrode;
	private GameObject rightPaddleElectrode;

	private readonly float indestructibleBarrierLength = 80.0f / 48.0f;

	private void OnEnable()
	{
		leftPaddleElectrode = GameObject.Find("Paddle/barrierElectrodes/left");
		rightPaddleElectrode = GameObject.Find("Paddle/barrierElectrodes/right");
	}

	private void Collision(GameObject bouncedObject)
	{
		IBrickBuster brickBuster = bouncedObject.GetComponent<IBrickBuster>();
		if (brickBuster is Ball || brickBuster is SpaceDjoel)
		{
			float contactX = brickBuster.LastHitPoint.x;
			float secondPointXToCompare = contactX < leftPaddleElectrode.transform.position.x ? leftPaddleElectrode.transform.position.x : rightPaddleElectrode.transform.position.x;
			if (Mathf.Abs(contactX - secondPointXToCompare) > indestructibleBarrierLength)
			{
				Debug.Log("Deactivate");
				GameManager.Instance.DecreaseProtectiveBarrierLevel();
			}
			else
				Debug.Log("Withstood");
			SoundManager.Instance.PlaySfx("Protective Barrier Hit");
		}
	}
}
