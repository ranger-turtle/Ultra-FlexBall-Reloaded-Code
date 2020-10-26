using System.Collections;
using UnityEngine;

public class Shooter : MonoBehaviour
{
	#region Singleton

	public static Shooter Instance { get; private set; }

	void Awake()
	{
		if (Instance)
			Destroy(Instance);
		else
			Instance = this;
	}
	#endregion

	private GameObject lShooter;
	private GameObject rShooter;

	[SerializeField]
#pragma warning disable CS0649 // Field 'Shooter.bulletPrefab' is never assigned to, and will always have its default value null
	private GameObject bulletPrefab;
#pragma warning restore CS0649 // Field 'Shooter.bulletPrefab' is never assigned to, and will always have its default value null

	private bool canStartShoot = true;

	// Start is called before the first frame update
	void Start()
    {
		lShooter = transform.Find("left").gameObject;
		rShooter = transform.Find("right").gameObject;
	}

	private void OnEnable()
	{
		canStartShoot = true;
	}

	//public void RemoveBullets()
	//{
	//	foreach (GameObject bullet in bullets)
	//	{
	//		bullets.Remove(bullet);
	//		Destroy(bullet);
	//	}
	//}

	public IEnumerator WaitForNextShoot()
	{
		do
		{
			float seconds = 0.6f / GameManager.Instance.ShooterLevel;
			LevelSoundLibrary.Instance.PlaySfx(LevelSoundLibrary.Instance.bulletShoot);
			Instantiate(bulletPrefab, lShooter.transform.position, Quaternion.identity);
			Instantiate(bulletPrefab, rShooter.transform.position, Quaternion.identity);
			yield return new WaitForSeconds(seconds);
		} while (Input.GetMouseButton(0));
		canStartShoot = true;
	}

	// Update is called once per frame
	void Update()
    {
        if (Input.GetMouseButtonDown(0) && canStartShoot)
		{
			canStartShoot = false;
			StartCoroutine(WaitForNextShoot());
		}
    }
}
