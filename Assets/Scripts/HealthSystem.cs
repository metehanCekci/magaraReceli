using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HealthSystem : MonoBehaviour
{
    [Header("Layer Ignore (i-frame)")]
    [Tooltip("Player'ın layer'ı (boş bırakılırsa bu objenin layer'ı kullanılır)")]
    public int playerLayer = -1;
    [Tooltip("Düşman saldırı/hitbox layer'ı (Inspector'dan atayın: EnemyAttack vb.)")]
    public int enemyAttackLayer = -1;


    [Header("Health")]
    public int maxHealth = 5;
    public int currentHealth;

    [Header("Hurt Feedback")]
    public float invulnerableTime = 0.15f;     // i-frame
    public float flashTime = 0.08f;            // kırmızı kalma süresi
    public SpriteRenderer spriteRenderer;      // yoksa otomatik bulmaya çalışır
    public Animator animator;                  // opsiyonel: "Hurt"/"Die" trigger
    public AudioSource audioSource;            // opsiyonel
    public AudioClip hurtSfx, deathSfx;        // opsiyonel

    [Header("Knockback")]
    public Rigidbody2D rb;                     // yoksa otomatik bulur
    public float knockbackForce = 8f;          // yatay baz kuvvet
    public float knockbackUpFactor = 0.35f;    // dikey itiş oranı

    bool invulnerable;

    void Awake()
    {
        currentHealth = maxHealth;
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!spriteRenderer) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (!audioSource) audioSource = GetComponent<AudioSource>();
        if (!animator) animator = GetComponent<Animator>();
        if (playerLayer < 0) playerLayer = gameObject.layer; // auto
    }

    public void TakeDamage(int amount, Vector2 hitFrom, Collider2D source)
    {
        if (invulnerable || currentHealth <= 0) return;

        currentHealth -= amount;
        if (currentHealth <= 0) { Die(); return; }

        animator?.SetTrigger("Hurt");
        if (audioSource && hurtSfx) audioSource.PlayOneShot(hurtSfx);
        StartCoroutine(FlashRedRoutine());
        ApplyKnockback(hitFrom);
        StartCoroutine(InvulnerabilityWithColliderRoutine(source));
    }
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }

    void ApplyKnockback(Vector2 hitFrom)
    {
        if (!rb) return;

        Vector2 dir = ((Vector2)transform.position - hitFrom).normalized;
        Vector2 force = new Vector2(dir.x * knockbackForce,
                                    Mathf.Abs(dir.y) * knockbackForce * knockbackUpFactor);

        // yatay hızı sıfırla, sonra it
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        rb.AddForce(force, ForceMode2D.Impulse);
    }


    IEnumerator InvulnerabilityWithColliderRoutine(Collider2D source)
    {
        invulnerable = true;

        // player tarafındaki tüm collider’larla bu 'source'u geçici olarak ignore et
        var myCols = GetComponentsInChildren<Collider2D>(includeInactive: false);
        foreach (var c in myCols) if (c && source) Physics2D.IgnoreCollision(c, source, true);

        yield return new WaitForSeconds(invulnerableTime);

        invulnerable = false;
        foreach (var c in myCols) if (c && source) Physics2D.IgnoreCollision(c, source, false);
    }
    IEnumerator InvulnerabilityRoutine()
    {
        invulnerable = true;

        bool canIgnore = (enemyAttackLayer >= 0);
        if (canIgnore)
            Physics2D.IgnoreLayerCollision(playerLayer, enemyAttackLayer, true);

        yield return new WaitForSeconds(invulnerableTime);

        invulnerable = false;
        if (canIgnore)
            Physics2D.IgnoreLayerCollision(playerLayer, enemyAttackLayer, false);
    }

    IEnumerator FlashRedRoutine()
    {
        if (!spriteRenderer) yield break;
        Color original = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(flashTime);
        spriteRenderer.color = original;
    }

    void Die()
    {
        if (animator) animator.SetTrigger("Die");
        if (audioSource && deathSfx) audioSource.PlayOneShot(deathSfx);
        Debug.Log($"{name} died");
        // TODO: respawn / game over / disable input vb.
    }
}
