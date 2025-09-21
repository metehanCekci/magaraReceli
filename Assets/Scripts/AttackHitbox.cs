using System.Collections.Generic;
using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    public int damage = 1;
    public int soulGainOnHit = 10;
    PlayerController owner;
    Dictionary<EnemyHealth2D, int> lastHitSwing = new Dictionary<EnemyHealth2D, int>();

    void Awake()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
        owner = GetComponentInParent<PlayerController>();
    }

    void OnTriggerEnter2D(Collider2D other) => TryHit(other);
    void OnTriggerStay2D(Collider2D other) => TryHit(other);

    void TryHit(Collider2D other)
    {
        if (owner == null) return;
        var enemy = other.GetComponentInParent<EnemyHealth2D>();
        if (!enemy) return;

        int currentSwing = owner.CurrentSwingId;

        if (lastHitSwing.TryGetValue(enemy, out int last) && last == currentSwing)
            return;

        enemy.TakeDamage(damage);
        lastHitSwing[enemy] = currentSwing;

        Vector2 knockbackDirection = (other.transform.position - owner.transform.position).normalized;
        enemy.ApplyKnockback(knockbackDirection);

        owner.IncreaseSoul(soulGainOnHit);

        Debug.Log($"Hit {enemy.name} on swing {currentSwing}. Soul gained.");
        CameraShake.Instance.Shake(0.1f, 0.1f);
        gameObject.SetActive(false);
    }
}