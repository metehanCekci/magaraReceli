using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BoulderBehaviour : MonoBehaviour
{
    [Header("Settings")]
    public float fallSpeed = 20f;       // Düşüş hızı
    public float bounceForce = 10f;     // Sekme kuvveti
    public float bounceDistance = 3f;   // Sekme mesafesi
    public float stayAfterBounce = 1f;  // Sekmeden sonra kısa bekleme
    public float destroyDelay = 2f;     // Alt sınırdan sonra yok olma süresi

    private Rigidbody2D rb;
    private bool hasBounced = false;
    private bool isActive = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.isKinematic = true; // Başta kendiliğinden düşmesin
    }

    public void Initialize(float spawnX)
    {
        Vector3 spawnPos = new Vector3(spawnX, transform.position.y, transform.position.z);
        transform.position = spawnPos;
        gameObject.SetActive(true);
        hasBounced = false;
        isActive = true;
        rb.isKinematic = false;
        rb.linearVelocity = Vector2.down * fallSpeed;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isActive) return;

        // Ground ile çarpıştı
        if (collision.gameObject.CompareTag("Ground") && !hasBounced)
        {
            hasBounced = true;
            StartCoroutine(BounceRoutine());
        }

        // Ground ile çarpıştı ve zaten sekmişse yok et
        else if (collision.gameObject.CompareTag("Ground") && hasBounced)
        {
            StartCoroutine(DestroyAfterDelay());
        }
    }

    IEnumerator BounceRoutine()
    {
        // Sekme yönü: sağ veya sol rastgele
        float dir = Random.value > 0.5f ? 1f : -1f;
        rb.linearVelocity = new Vector2(dir * bounceDistance / 0.3f, bounceForce); // x ve y hız

        yield return new WaitForSeconds(stayAfterBounce);

        rb.linearVelocity = Vector2.down * fallSpeed; // tekrar aşağı düş
    }

    IEnumerator DestroyAfterDelay()
    {
        isActive = false;
        rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(destroyDelay);
        gameObject.SetActive(false);
    }
}
