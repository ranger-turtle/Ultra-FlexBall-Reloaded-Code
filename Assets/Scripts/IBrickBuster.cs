using UnityEngine;

public interface IBrickBuster
{
	Vector2 LastFrameVelocity { get; set; }
	Vector2 CurrentVelocity { get; set; }
	Vector3 LastFramePosition { get; set; }
	Vector3 LastHitPoint { get; set; }
	Vector2 LastHitNormal { get; set; }

	bool Teleport { get; set; }

	//void IncreasePowerUpMeter(Brick brick);
}
