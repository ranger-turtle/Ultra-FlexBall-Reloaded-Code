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

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision.gameObject.GetComponent<Ball>())
		{
			float contactX = (collision.GetContact(0).point.x + collision.GetContact(1).point.x) / 2;
			float secondPointXToCompare = contactX < leftPaddleElectrode.transform.position.x ? leftPaddleElectrode.transform.position.x : rightPaddleElectrode.transform.position.x;
			if (Mathf.Abs(contactX - secondPointXToCompare) > indestructibleBarrierLength)
			{
				Debug.Log("Deactivate");
				GameManager.Instance.DecreaseProtectiveBarrierLevel();
			}
			else
				Debug.Log("Withstood");
			LevelSoundLibrary.Instance.PlaySfx(LevelSoundLibrary.Instance.protectiveBarrierHit);
		}
	}
}
