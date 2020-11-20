using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestBall : MonoBehaviour
{
	public LayerMask layerMask;
	public Vector2 movement;
	public float incrementX = 0.01f;
	public float incrementY = 0.01f;

	private int hits;

    // Update is called once per frame
    void FixedUpdate()
    {
		if (Input.GetKeyDown(KeyCode.Keypad1))
			movement = new Vector2(-incrementX, -incrementY);
		else if (Input.GetKeyDown(KeyCode.Keypad2))
			movement = new Vector2(0, -incrementY);
		else if (Input.GetKeyDown(KeyCode.Keypad3))
			movement = new Vector2(incrementX, -incrementY);
		else if (Input.GetKeyDown(KeyCode.Keypad4))
			movement = new Vector2(-incrementX, 0);
		else if (Input.GetKeyDown(KeyCode.Keypad6))
			movement = new Vector2(incrementX, 0);
		else if (Input.GetKeyDown(KeyCode.Keypad7))
			movement = new Vector2(-incrementX, incrementY);
		else if (Input.GetKeyDown(KeyCode.Keypad8))
			movement = new Vector2(0, incrementY);
		else if (Input.GetKeyDown(KeyCode.Keypad9))
			movement = new Vector2(incrementX, incrementY);
		else
			movement = Vector2.zero;
		if (movement != Vector2.zero)
		{
			RaycastHit2D[] boxCastHit = Physics2D.BoxCastAll(transform.position, GetComponent<BoxCollider2D>().size, 0, movement, movement.magnitude, layerMask);
			if (boxCastHit.Length > 0)
			{
				hits++;
				Debug.Log($"Hit: {hits}");
			}
			foreach (var bch in boxCastHit)
			{
				TestBrick testBrick = bch.collider.GetComponent<TestBrick>();
				Debug.Log($"X: {testBrick.x}, Y: {testBrick.y} Point X: {bch.point.x}, Point Y: {bch.point.y} Normal: {bch.normal} Centroid X: {bch.centroid.x}, Centroid Y: {bch.centroid.y}");
			}
			RaycastHit2D raycastHit2D = Physics2D.Raycast(transform.position, movement, movement.magnitude, layerMask);
			if (raycastHit2D)
				Debug.Log($"Raycast Point X: {raycastHit2D.point.x}, Raycast Point Y: {raycastHit2D.point.y} Normal: {raycastHit2D.normal}");
		}
		transform.position += new Vector3(movement.x, movement.y, 0);
    }
}
