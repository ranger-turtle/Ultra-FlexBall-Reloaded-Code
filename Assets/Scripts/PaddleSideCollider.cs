using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaddleSideCollider : MonoBehaviour
{
	[SerializeField]
	private SoundManager soundManager;

	[SerializeField]
	private float side = -1;

    private void Collision(GameObject bouncedObject)
    {
		side = Mathf.Sign(side);
		float sideBounceAngle = side < 0 ? 21 : 159;
		if (bouncedObject.GetComponent<Ball>() || bouncedObject.GetComponent<SpaceDjoel>())
		{
			Bounds thisColliderBounds = GetComponent<BoxCollider2D>().bounds;
			Bounds brickBusterBounds = bouncedObject.GetComponent<BoxCollider2D>().bounds;
			IBrickBuster brickBuster = bouncedObject.GetComponent<IBrickBuster>();

			if (side > 0)
			{
				bouncedObject.transform.position = new Vector3(thisColliderBounds.max.x + brickBusterBounds.extents.x + 0.03f, bouncedObject.transform.position.y, bouncedObject.transform.position.z);
				ParticleManager.Instance.GeneratePaddleSideHitSparkles(new Vector3(thisColliderBounds.max.x, thisColliderBounds.max.y, -1));
			}
			else
			{
				bouncedObject.transform.position = new Vector3(thisColliderBounds.min.x - brickBusterBounds.extents.x - 0.03f, bouncedObject.transform.position.y, bouncedObject.transform.position.z);
				ParticleManager.Instance.GeneratePaddleSideHitSparkles(new Vector3(thisColliderBounds.min.x, thisColliderBounds.max.y, -1));
			}
			soundManager.PlaySfx("Paddle Side Bounce");

			if (bouncedObject.GetComponent<Ball>())
			{
				//Debug.Break();
				Ball ball = brickBuster as Ball;
				Debug.Log($"New velocity: x: {ball.CurrentVelocity.x}, y: {ball.CurrentVelocity.y}");
				if (ball.BallSize != (int)GameManager.Instance.BallSize)
					soundManager.PlaySfx("Ball Size Change");
				ball.UpdateSize();
				ball.FinishThrust();
				ParticleManager.Instance.RemoveThrustingFlame(ball.gameObject);
				ball.CurrentVelocity = PhysicsHelper.GetAngledVelocity(sideBounceAngle) * (BallManager.minBallSpeed * 1.5f);
			}
			else if (bouncedObject.GetComponent<SpaceDjoel>())
			{
				brickBuster.CurrentVelocity = PhysicsHelper.GetAngledVelocity(sideBounceAngle) * (SpaceDjoelManager.initialForce * 2.0f);
			}
		}
    }
}
