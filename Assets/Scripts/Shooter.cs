using System.Collections;
using UnityEngine;

public class Shooter : MonoBehaviour
{
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
			SoundManager.Instance.PlaySfx("Bullet Shoot");
			Instantiate(bulletPrefab, lShooter.transform.position, Quaternion.identity);
			ParticleManager.Instance.GenerateShootEffect(lShooter.transform.position);
			Instantiate(bulletPrefab, rShooter.transform.position, Quaternion.identity);
			ParticleManager.Instance.GenerateShootEffect(rShooter.transform.position);
			yield return new WaitForSeconds(seconds);
		} while (Input.GetMouseButton(0));
		canStartShoot = true;
	}

	// Update is called once per frame
	void FixedUpdate()
    {
        if (Input.GetMouseButtonDown(0) && canStartShoot)
		{
			canStartShoot = false;
			StartCoroutine(WaitForNextShoot());
		}
    }
}
