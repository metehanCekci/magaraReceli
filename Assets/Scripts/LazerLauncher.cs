using System.Collections;
using UnityEngine;

public class LazerLauncher : MonoBehaviour
{
    [Header("Lazer Settings")]
    public GameObject lazerPrefab;       // Bullet prefab
    public float shootInterval = 0.1f;
    public float bulletCooldown = 1; // Delay between bullets
    public float shootDuration = 3f;     // Shoot for this many seconds each volley
    public float shootAngle = 15f;       // Angle between bullets
    public int bulletCount = 5;          // Number of bullets per volley
    public bool shootRight = true;       // Shooting direction

    private void Start()
    {
        StartCoroutine(ShootLoop());
    }

    IEnumerator ShootLoop()
    {
        yield return new WaitForSeconds(3);
        while (true)
        {
            float timer = 0f;

            while (timer < shootDuration)
            {
                ShootBulletPattern();
                timer += shootInterval * bulletCount;
                yield return new WaitForSeconds(shootInterval * bulletCount);
            }

            // Wait 1 second before next volley
            yield return new WaitForSeconds(bulletCooldown);
        }
    }

    void ShootBulletPattern()
    {
        float startAngle = -(shootAngle * (bulletCount - 1) / 2f);
        for (int i = 0; i < bulletCount; i++)
        {
            float angle = startAngle + i * shootAngle;
            Vector3 dir = Quaternion.Euler(0, 0, shootRight ? angle : 180 - angle) * Vector3.right;

            GameObject bullet = Instantiate(lazerPrefab, transform.position, Quaternion.identity);
            bullet.SetActive(true);
            bullet.GetComponent<Rigidbody2D>().linearVelocity = dir * 10f; // speed
        }
    }
}
