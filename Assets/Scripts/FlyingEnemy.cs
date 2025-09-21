using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class FlyingEnemy : MonoBehaviour
{
    [Header("Target")]
    public Transform player;          // Player'ın transformu

    [Header("Movement Settings")]
    public float moveSpeed = 3f;      // Hız
    public float stopDistance = 1.5f; // Çok yaklaştığında durma mesafesi
    public float smoothFollow = 5f;   // Hareket yumuşatma

    private Rigidbody2D rb;
    private SpriteRenderer sr;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void FixedUpdate()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance > stopDistance)
        {
            Vector2 direction = (player.position - transform.position).normalized;

            // Move with Rigidbody
            Vector2 targetVelocity = direction * moveSpeed;
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, targetVelocity, Time.fixedDeltaTime * smoothFollow);

            // 🔥 Flip sprite depending on horizontal direction
            if (direction.x > 0.1f)
                sr.flipX = false; // Facing right
            else if (direction.x < -0.1f)
                sr.flipX = true;  // Facing left
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
}
