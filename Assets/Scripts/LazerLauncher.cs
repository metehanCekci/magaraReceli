using System.Collections;
using UnityEngine;

public class LazerLauncher : MonoBehaviour
{
    [Header("Cannon Settings")]
    public GameObject bulletPrefab;
    public float bulletSpeed = 10f;
    public float fireInterval = 0.2f;
    public int health = 5;

    [Header("Direction")]
    public bool shootRight = true; // sað/sol yön

    private bool isShooting = false;

    private void Start()
    {
        // Fade-in sýrasýnda BossController ActivateCannon() çaðýrabilir
    }

    public void ActivateCannon()
    {
        if (!isShooting)
            StartCoroutine(ShootLoop());
    }

    IEnumerator ShootLoop()
    {
        isShooting = true;

        while (true) // sonsuza kadar
        {
            Shoot();
            yield return new WaitForSeconds(fireInterval);
        }
    }

    void Shoot()
    {
        if (!bulletPrefab) return;

        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        bullet.SetActive(true);

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.linearVelocity = (shootRight ? Vector2.right : Vector2.left) * bulletSpeed;
    }

    public void TakeDamage(int dmg)
    {
        health -= dmg;
        if (health <= 0)
        {
            Destroy(gameObject); // yok olduðunda otomatik olarak ateþ durur
        }
    }
}
