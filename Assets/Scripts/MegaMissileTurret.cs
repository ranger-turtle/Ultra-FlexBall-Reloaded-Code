using UnityEngine;

public class MegaMissileTurret : MonoBehaviour
{
	//private GameObject lShooter;

	[SerializeField]
#pragma warning disable CS0649 // Field 'Shooter.bulletPrefab' is never assigned to, and will always have its default value null
	private GameObject megaMissilePrefab;
#pragma warning restore CS0649 // Field 'Shooter.bulletPrefab' is never assigned to, and will always have its default value null

    // Update is called once per frame
    void FixedUpdate()
    { 
        if (Input.GetMouseButtonDown(0) && GameManager.Instance.MegaMissiles > 0)
		{
			SoundManager.Instance.PlaySfx("Mega Missile Shoot");
			Instantiate(megaMissilePrefab, transform.position, Quaternion.identity);
			GameManager.Instance.DecreaseMegaMissiles();
		}
    }
}
