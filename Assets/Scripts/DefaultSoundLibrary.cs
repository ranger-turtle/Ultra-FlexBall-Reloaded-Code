using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DefaultSoundLibrary : MonoBehaviour
{
	#region Singleton

	public static DefaultSoundLibrary Instance { get; private set; }

	void Awake()
	{
		if (Instance)
			Destroy(Instance);
		else
			Instance = this;
	}
	#endregion

	public AudioClip normalBallBounce;
	public AudioClip bang;
	public AudioClip explosion;
	public AudioClip specialHit;
	public AudioClip powerUpYield;
	public AudioClip hitWall;
	public AudioClip ballFall;
	public AudioClip spaceDjoelFall;
	public AudioClip powerUpFall;
	public AudioClip magnetStick;
	public AudioClip ballSizeChange;
	public AudioClip brickDescend;
	public AudioClip losePaddle;
	public AudioClip bulletShoot;
	public AudioClip bulletBounce;
	public AudioClip ballThrust;
	public AudioClip teleport;
	public AudioClip megaMissileShoot;
	public AudioClip megaExplosion;
	public AudioClip protectiveBarrierHit;
	public AudioClip quickBallBounce;
	public AudioClip win;

	public AudioClip normalBrickBreak;
	public AudioClip explosiveBrickHit;
	public AudioClip indestructibleBrickHit;
	public AudioClip changingBrickHit;
	public AudioClip plateHit;
}
