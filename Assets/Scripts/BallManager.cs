using System;
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

	private Ball initialBall;

	internal readonly float initialForce = 6.0f;

	private LevelSoundLibrary levelSoundLibrary;

	public int BallNumber => balls.Count;

	public void Start()
	{
		BoxCollider2D paddleCollider = Paddle.Instance.GetComponent<BoxCollider2D>();
		BoxCollider2D ballCollider = ballPrefab.GetComponent<BoxCollider2D>();
		levelSoundLibrary = GameObject.Find("Game").GetComponent<LevelSoundLibrary>();
		InitBall();
	}

	public void InitBall()
	{
		Vector3 paddlePosition = Paddle.Instance.transform.position;
		float StuckY = Paddle.Instance.GetComponent<BoxCollider2D>().bounds.max.y + ballPrefab.GetComponent<BoxCollider2D>().bounds.extents.y + 0.3f;
		Vector3 startingPosition = new Vector3(paddlePosition.x, StuckY);
		initialBall = Instantiate(ballPrefab, startingPosition, Quaternion.identity);
		initialBall.StickToPaddle(PhysicsHelper.GetAngledVelocity(90) * initialForce, 0);
		balls = new List<GameObject>() { initialBall.gameObject };
	}

	public void Remove(GameObject ballObj)
	{
		Destroy(ballObj);
		balls.Remove(ballObj);
		GameManager.Instance.CheckForLosePaddle();
	}


	internal GameObject CloneBall(GameObject originalBall)
	{
		GameObject newBallObject = Instantiate(originalBall, originalBall.transform.position, Quaternion.identity);
		newBallObject.GetComponent<Ball>().SetBallSizeWithoutAnimation((int)GameManager.Instance.BallSize);
		newBallObject.GetComponent<Ball>().StuckToPaddle = originalBall.GetComponent<Ball>().StuckToPaddle;
		return newBallObject;
	}

	public void SplitBall()
	{
		List<GameObject> newBalls = new List<GameObject>();
		foreach (var ball in balls)
		{
			GameObject newBallObject = CloneBall(ball);
			Vector3 originalBallVelocity = ball.GetComponent<Rigidbody2D>().velocity;
			Vector3 newBallVelocity = new Vector3(originalBallVelocity.x, originalBallVelocity.y);
			if (originalBallVelocity.x != 0 && originalBallVelocity.y != 0)
			{
				if (Mathf.Abs(originalBallVelocity.x) > Mathf.Abs(originalBallVelocity.y))
					newBallVelocity.y = -newBallVelocity.y;
				else
					newBallVelocity.x = -newBallVelocity.x;
			}
			newBallObject.GetComponent<Rigidbody2D>().velocity = newBallVelocity;
			newBalls.Add(newBallObject);
		}
		balls = balls.Concat(newBalls).ToList();
	}

	public void MegaSplit()
	{
		GameManager.Instance.ExplosiveBall = false;
		GameManager.Instance.PenetratingBall = false;
		List<GameObject> newBalls = new List<GameObject>();
		GameObject originBall = balls[UnityEngine.Random.Range(0, balls.Count - 1)];
		const int ballNumber = 30;
		for (int i = 0; i < ballNumber; i++)
		{
			Rigidbody2D originBallRb = originBall.GetComponent<Rigidbody2D>();
			float angle = Mathf.PI*2.0f / ballNumber * i + 0.1f;
			//float speed = Mathf.Max(originBallRb.velocity.x, originBallRb.velocity.y);
			//Debug.Log($"Velocity: {originBallRb.velocity}");
			//Debug.Log($"Magnitude: {magnitude}");
			float x = originBall.GetComponent<Rigidbody2D>().velocity.x;
			float y = originBall.GetComponent<Rigidbody2D>().velocity.y;
			//Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.up);
			//Vector3 direction = rotation * Vector3.forward;
			Vector2 direction = new Vector3(Mathf.Cos(angle), Mathf.Sign(angle));
			Debug.Log($"Direction: {direction}");
			GameObject newBallObject = Instantiate(originBall, originBall.transform.position, Quaternion.identity);
			newBallObject.GetComponent<Rigidbody2D>().velocity = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * Ball.maxBallVelocityMagnitude;
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

	private void Update()
	{
		if (Input.GetMouseButtonDown(0) && balls.Any(b => b.GetComponent<Ball>().StuckToPaddle))
		{
			Paddle.Instance.MagnetActive = GameManager.Instance.MagnetPaddle;
			ReleaseBalls();
			levelSoundLibrary.PlaySfx(levelSoundLibrary.normalBallBounce);
		}
	}

	internal void UpdateMagnetVisibility()
	{
		Paddle.Instance.MagnetActive = balls.Any(b => b.GetComponent<Ball>().StuckToPaddle) ? true : false;
	}
}
