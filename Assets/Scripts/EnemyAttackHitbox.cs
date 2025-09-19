using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyAttackHitbox : MonoBehaviour
{
    public int damage = 20;
    public LayerMask playerLayers; // Player layer'ını seç
    private Collider2D hitboxCollider;

    void Awake()
    {
        hitboxCollider = GetComponent<Collider2D>();

        // Eğer bu objenin tag'ı "Projectile" ise, collider'ı Trigger yapıyoruz
        if (CompareTag("Projectile") || GameObject.Find("Player"))
        {
            hitboxCollider.isTrigger = true;  // "Projectile" tag'ine sahip objeler için Trigger yapıyoruz
        }
        else
        {
            hitboxCollider.isTrigger = false; // Diğer objeler için Trigger yapmıyoruz
        }
    }

    // Trigger olduğunda Player'a hasar vermek
    void OnTriggerEnter2D(Collider2D other)
    {
        // Eğer çarpan obje Player'a aitse
        if (((1 << other.gameObject.layer) & playerLayers) != 0)
        {
            TryHit(other);
        }
    }

    // Collision olduğunda Player'a hasar vermek
    void OnCollisionEnter2D(Collision2D other)
    {
        // Eğer çarpan obje Player'a aitse ve bu obje Trigger değilse
        if (((1 << other.gameObject.layer) & playerLayers) != 0 && !CompareTag("Projectile"))
        {
            TryHit(other.collider);
        }
    }

    void TryHit(Collider2D other)
    {
        var hs = other.GetComponentInParent<HealthSystem>();
        if (!hs) return;

        if (!hs.IsInvulnerable()) // Eğer Player invulnerable değilse hasar uygula
        {
            hs.TakeDamage(damage, transform.position);
        }
    }

    private void OnCollisionStay2D(Collision2D other)
    {
        if (((1 << other.gameObject.layer) & playerLayers) != 0 && !CompareTag("Projectile"))
        {
            TryHit(other.collider);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & playerLayers) != 0)
        {
            TryHit(other);
        }
    }
}
