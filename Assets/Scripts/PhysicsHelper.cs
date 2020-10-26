using UnityEngine;

internal class PhysicsHelper
{
	public static Vector2 GenerateReflectedVelocity(Vector2 lastFrameVelocity, Collision2D collision)
	{
		Vector2 collisionNormal = collision.GetContact(0).normal;
		//Debug.Log($"Collisions: {collision.contactCount}");
		//if (collision.GetContact(0).normal == new Vector2(0, 1) || collision.GetContact(1).normal == new Vector2(0, 1))
		//	Debug.Break();
		return Vector3.Reflect(lastFrameVelocity, collisionNormal);
	}

	public static Vector2 GenerateReflectedVelocity(Vector2 lastFrameVelocity, Vector2 normal)
	{
		if (normal.x == 0 || normal.y == 0)
			return Vector2.Reflect(lastFrameVelocity, normal);
		else
			return new Vector2(-lastFrameVelocity.x, -lastFrameVelocity.y);
	}

	public static Vector2 GetAngledVelocity(float angle)
	{
		angle *= Mathf.Deg2Rad;
		return new Vector2(-Mathf.Cos(angle), Mathf.Sin(angle));
	}

	public static float GetAngleInFullTurnRange(Vector2 vector)
		=> Vector2.Angle(Vector2.up, vector);
}