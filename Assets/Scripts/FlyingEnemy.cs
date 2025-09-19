using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class FlyingEnemy : MonoBehaviour
{
    [Header("Target")]
    public Transform player;          // Player’ýn transform’u (Inspector’da sürükle býrak)

    [Header("Movement Settings")]
    public float moveSpeed = 3f;      // Hýz
    public float stopDistance = 1.5f; // Çok yaklaþtýðýnda durmasý için mesafe
    public float smoothFollow = 5f;   // Hareket yumuþatma

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (player == null)
        {
            // Eðer inspector’dan verilmemiþse otomatik player bul
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
            // Player’a doðru yön
            Vector2 direction = (player.position - transform.position).normalized;

            // Rigidbody ile hareket
            Vector2 targetVelocity = direction * moveSpeed;
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, targetVelocity, Time.fixedDeltaTime * smoothFollow);
        }
        else
        {
            // Yaklaþtýðýnda dur
            rb.linearVelocity = Vector2.zero;
        }
    }
}
