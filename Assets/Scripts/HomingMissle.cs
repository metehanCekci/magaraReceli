using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class HomingMissile : MonoBehaviour
{
    [Header("Target")]
    public Transform target; // Assign in inspector, or it will auto-find the player

    [Header("Movement")]
    public float speed = 6f;            // Forward movement speed
    public float rotateSpeed = 720f;    // Degrees per second (higher = snappier turns)

    [Header("Rotation")]
    [Tooltip("Adjust if sprite art points up/left/etc. 0 = right, -90 = up, 90 = down, 180 = left")]
    public float rotationOffset = -90f;
    public bool useVelocityForRotation = true;

    [Header("Lifetime")]
    public float lifetime = 3f;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnEnable()
    {
        // If no target assigned, auto-find Player tagged object
        if (target == null)
        {
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) target = p.transform;
        }

        // Auto-destroy after lifetime
        if (lifetime > 0)
            Destroy(gameObject, lifetime);
    }

    void FixedUpdate()
    {
        if (target == null)
        {
            // No target? Just fly forward
            rb.linearVelocity = transform.right * speed;
            return;
        }

        // Move toward target
        Vector2 dir = (target.position - transform.position).normalized;
        rb.linearVelocity = dir * speed;

        // Calculate desired facing angle
        float desiredAngle;
        if (useVelocityForRotation)
            desiredAngle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg + rotationOffset;
        else
            desiredAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + rotationOffset;

        // Smoothly rotate toward desired angle
        float newAngle = Mathf.MoveTowardsAngle(rb.rotation, desiredAngle, rotateSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(newAngle);
    }
}
