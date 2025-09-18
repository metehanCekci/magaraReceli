using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class FlyingEnemy : MonoBehaviour
{
    [Header("Target")]
    public Transform player;          // Player��n transform�u (Inspector�da s�r�kle b�rak)

    [Header("Movement Settings")]
    public float moveSpeed = 3f;      // H�z
    public float stopDistance = 1.5f; // �ok yakla�t���nda durmas� i�in mesafe
    public float smoothFollow = 5f;   // Hareket yumu�atma

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (player == null)
        {
            // E�er inspector�dan verilmemi�se otomatik player bul
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void FixedUpdate()
    {
        if (player == null) return;

        // Player ile aradaki mesafeyi bul
        float distance = Vector2.Distance(transform.position, player.position);

        if (distance > stopDistance)
        {
            // Player�a do�ru y�n
            Vector2 direction = (player.position - transform.position).normalized;

            // Rigidbody ile hareket
            Vector2 targetVelocity = direction * moveSpeed;
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, targetVelocity, Time.fixedDeltaTime * smoothFollow);
        }
        else
        {
            // Yakla�t���nda dur
            rb.linearVelocity = Vector2.zero;
        }
    }
}
