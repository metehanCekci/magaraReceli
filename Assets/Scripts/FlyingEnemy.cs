using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class FlyingEnemy : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float stopDistance = 1.5f;
    public float smoothFollow = 5f;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void FixedUpdate()
    {
        if (player == null) return;

        Vector2 direction = (player.position - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, player.position);

        if (distance > stopDistance)
        {
            // Move towards player
            Vector2 targetVelocity = direction * moveSpeed;
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, targetVelocity, Time.fixedDeltaTime * smoothFollow);

            // Flip transform to face horizontal direction only
            FaceDirection(direction.x);
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void FaceDirection(float horizontal)
    {
        Vector3 scale = transform.localScale;
        if (horizontal > 0.1f) scale.x = Mathf.Abs(scale.x);      // Facing right
        else if (horizontal < -0.1f) scale.x = -Mathf.Abs(scale.x); // Facing left
        transform.localScale = scale;
    }
}
