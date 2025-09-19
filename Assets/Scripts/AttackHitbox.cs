using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class AttackHitbox2D : MonoBehaviour
{
    public int damage = 1;

    PlayerController owner;
    Dictionary<EnemyHealth2D, int> lastHitSwing = new Dictionary<EnemyHealth2D, int>();

    void Awake()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
        owner = GetComponentInParent<PlayerController>();
    }

    void OnEnable()  { /* yeni swing başlıyor, dictionary kalabilir; swingId zaten değiştiğinde tekrar vurur */ }
    void OnDisable() { /* optional: lastHitSwing.Clear(); */ }

    void OnTriggerEnter2D(Collider2D other) => TryHit(other);
    void OnTriggerStay2D (Collider2D other) => TryHit(other);

    void TryHit(Collider2D other)
    {
        if (owner == null) return;
        var enemy = other.GetComponentInParent<EnemyHealth2D>();
        if (!enemy) return;

        int currentSwing = owner.CurrentSwingId;

        if (lastHitSwing.TryGetValue(enemy, out int last) && last == currentSwing)
            return; // aynı swing'de tekrar vurma

        enemy.TakeDamage(damage, transform.position);
        lastHitSwing[enemy] = currentSwing;
        Debug.Log($"Hit {enemy.name} on swing {currentSwing}");
        CameraShake.Instance.Shake(0.1f, 0.1f); // duration 0.3s, intensity 0.5

    }
}
