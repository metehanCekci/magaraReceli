using UnityEngine;

public class HomingMissile : MonoBehaviour
{
    public Transform target;
    public float speed;
    public float lifetime;

    private float timer;

    private void Update()
    {
        if (target != null)
        {
            // Move towards target
            Vector2 direction = (target.position - transform.position).normalized;
            transform.position += (Vector3)(direction * speed * Time.deltaTime);

            // Rotate to face target
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        // Lifetime countdown
        timer += Time.deltaTime;
        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}
