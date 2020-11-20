﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BallManager : MonoBehaviour
{
	#region Singleton

	public static BallManager Instance { get; private set; }

	void Awake()
	{
		if (Instance)
			Destroy(Instance);
		else
			Instance = this;
	}
	#endregion

	private List<GameObject> balls;

	[SerializeField]
#pragma warning disable CS0649 // Field 'BallManager.ballPrefab' is never assigned to, and will always have its default value null
	private Ball ballPrefab;
#pragma warning restore CS0649 // Field 'BallManager.ballPrefab' is never assigned to, and will always have its default value null

	[SerializeField]
	private HUDManager hudManager;

	private Ball initialBall;

	internal const float minBallSpeed = 0.10f;
	internal const float maxBallSpeed = 0.17f;
	internal const float acceleration = 1.001f;

	private SoundManager soundManager;

	public int BallNumber => balls.Count;

	public void Start()
	{
		soundManager = SoundManager.Instance;
		InitBall();
	}

	public void InitBall()
	{
		Vector3 paddlePosition = Paddle.Instance.transform.position;
		float StuckY = Paddle.Instance.GetComponent<BoxCollider2D>().bounds.max.y + ballPrefab.GetComponent<BoxCollider2D>().bounds.extents.y + 0.3f;
		Vector3 startingPosition = new Vector3(paddlePosition.x, StuckY);
		initialBall = Instantiate(ballPrefab, startingPosition, Quaternion.identity);
		initialBall.StickToPaddle(Vector2.up * minBallSpeed, 0);
		balls = new List<GameObject>() { initialBall.gameObject };
	}

	public void Remove(GameObject ballObj)
	{
		Destroy(ballObj);
		balls.Remove(ballObj);
		GameManager.Instance.CheckForLosePaddle();
	}


	internal GameObject CloneBall(GameObject originalBall, bool addToCollection = true)
	{
		GameObject newBallObject = Instantiate(originalBall, originalBall.transform.position, Quaternion.identity);
		newBallObject.GetComponent<Ball>().CloneProperties(originalBall.GetComponent<Ball>());
		if (addToCollection)
			balls.Add(newBallObject);
		return newBallObject;
	}

	private void ApplyParticlesToBalls(Action<GameObject, BallSize> applyParticlesToBall)
	{
		foreach (var ball in balls)
			applyParticlesToBall(ball, (BallSize)ball.GetComponent<Ball>().BallSize);
	}

	internal void ApplyParticlesToExplosiveBalls() => ApplyParticlesToBalls(ParticleManager.Instance.GenerateExplosiveBallParticles);

	internal void ApplyParticlesToPenetratingBalls() => ApplyParticlesToBalls(ParticleManager.Instance.GeneratePenetratingBallParticles);

	internal void RemoveParticlesFromBalls()
	{
		foreach (var ball in balls)
			ParticleManager.Instance.RemoveBallUpgradeParticles(ball);
	}

	public void SplitBall()
	{
		List<GameObject> newBalls = new List<GameObject>();
		foreach (var ball in balls)
		{
			GameObject newBallObject = CloneBall(ball, false);
			Vector3 originalBallVelocity = ball.GetComponent<Ball>().CurrentVelocity;
			Vector3 newBallVelocity = new Vector3(originalBallVelocity.x, originalBallVelocity.y);
			if (originalBallVelocity.x != 0 && originalBallVelocity.y != 0)
			{
				if (Mathf.Abs(originalBallVelocity.x) > Mathf.Abs(originalBallVelocity.y))
					newBallVelocity.y = -newBallVelocity.y;
				else
					newBallVelocity.x = -newBallVelocity.x;
			}
			newBallObject.GetComponent<Ball>().CurrentVelocity = newBallVelocity;
			newBalls.Add(newBallObject);
		}
		balls = balls.Concat(newBalls).ToList();
	}

	public void MegaSplit()
	{
		GameManager.Instance.ExplosiveBall = false;
		GameManager.Instance.PenetratingBall = false;
		List<GameObject> newBalls = new List<GameObject>();
		RemoveParticlesFromBalls();
		GameObject originBall = balls[UnityEngine.Random.Range(0, balls.Count)];
		const int ballNumber = 30;
		for (int i = 0; i < ballNumber; i++)
		{
			Ball originBallScript = originBall.GetComponent<Ball>();
			float angle = Mathf.PI*2.0f / ballNumber * i + 0.1f;
			//float speed = Mathf.Max(originBallRb.velocity.x, originBallRb.velocity.y);
			//Debug.Log($"Velocity: {originBallRb.velocity}");
			//Debug.Log($"Magnitude: {magnitude}");
			float x = originBallScript.CurrentVelocity.x;
			float y = originBallScript.CurrentVelocity.y;
			//Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
			//Vector3 direction = rotation * Vector3.forward;
			Vector2 direction = new Vector3(Mathf.Cos(angle), Mathf.Sign(angle));
			GameObject newBallObject = Instantiate(originBall, originBall.transform.position, Quaternion.identity);
			newBallObject.GetComponent<Ball>().CurrentVelocity = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * maxBallSpeed;
			newBalls.Add(newBallObject);
		}
		balls = balls.Concat(newBalls).ToList();
	}

	public void UpdateSizeOfAllStuckToPaddleBalls()
	{
		IEnumerable<Ball> stuckBalls = balls.Select(b => b.GetComponent<Ball>()).Where(b => b.StuckToPaddle);
		foreach (Ball ball in stuckBalls)
			ball.UpdateSize();
	}

	public void UpdateBallPositionsWhenStuckToPaddle()
	{
		IEnumerable<Ball> stuckBalls = balls.Select(b => b.GetComponent<Ball>()).Where(b => b.StuckToPaddle);
		foreach (Ball ball in stuckBalls)
			ball.UpdateBallOnPaddle();
	}

	private void ReleaseBalls()
	{
		IEnumerable<GameObject> stuckBalls = balls.Where(b => b.GetComponent<Ball>().StuckToPaddle);
		foreach (GameObject ballObject in stuckBalls)
		{
			ballObject.GetComponent<Ball>().LaunchFromPaddle();
		}
	}

	private void FixedUpdate()
	{
		if (Input.GetMouseButtonDown(0) && balls.Any(b => b.GetComponent<Ball>().StuckToPaddle))
		{
			Paddle.Instance.MagnetActive = GameManager.Instance.MagnetPaddle;
			ReleaseBalls();
			soundManager.PlaySfx("Normal Ball Bounce");
		}
	}

	internal void UpdateMagnetVisibility()
	{
		Paddle.Instance.MagnetActive = balls.Any(b => b.GetComponent<Ball>().StuckToPaddle) ? true : false;
	}
}
