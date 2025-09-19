using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyAttackHitbox : MonoBehaviour
{
    public int damage = 20;
    public LayerMask playerLayers; // Player layer'ını seç

    void Awake()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other) => TryHit(other);
    void OnTriggerStay2D(Collider2D other)  => TryHit(other); // Sürekli hasar vermek için OnTriggerStay2D kullanıyoruz

    void TryHit(Collider2D other)
    {
        // Layer filtresi (opsiyonel ama iyi olur)
        if (((1 << other.gameObject.layer) & playerLayers) == 0) return;

        var hs = other.GetComponentInParent<HealthSystem>();
        if (!hs) return;

        if (!hs.IsInvulnerable()) // Player invulnerable değilse hasar uygula
        {
            hs.TakeDamage(damage, transform.position);
        }
    }
}
