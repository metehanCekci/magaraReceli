using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyHealth2D : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 5;
    public int currentHealth;

    [Header("Invulnerability (i-frames)")]
    public float invulnerableTime = 0.2f;

    [Header("Knockback")]
    public float knockbackForce = 6f;
    [Tooltip("X: yatay itiş çarpanı, Y: dikey itiş çarpanı")]
    public Vector2 knockbackScale = new Vector2(1f, 0.5f);

    [Header("Refs (opsiyonel)")]
    public Rigidbody2D rb;
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    public AudioSource audioSource;
    public AudioClip hurtSfx;
    public AudioClip deathSfx;

    bool invulnerable;

    void Awake()
    {
        currentHealth = maxHealth;
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!spriteRenderer) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (!audioSource) audioSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// hasar uygula. hitFrom: saldırının geldiği nokta (ör: player hitbox pozisyonu)
    /// </summary>
    public void TakeDamage(int amount, Vector2 hitFrom)
    {
        if (invulnerable) { Debug.Log("Hit blocked: i-frame active"); return; }
        if (invulnerable || currentHealth <= 0) return;

        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        // Hurt feedback
        if (animator) animator.SetTrigger("Hurt");
        if (audioSource && hurtSfx) audioSource.PlayOneShot(hurtSfx);
        StartCoroutine(FlashRoutine());

        // Knockback
        ApplyKnockback(hitFrom);

        // i-frames
        StartCoroutine(InvulnerabilityRoutine());
    }

    void ApplyKnockback(Vector2 hitFrom)
    {
        if (!rb) return;

        // darbeyi vuran taraftan uzağa doğru
        Vector2 dir = ((Vector2)transform.position - hitFrom).normalized;
        Vector2 force = new Vector2(dir.x * knockbackForce * knockbackScale.x,
                                    Mathf.Abs(dir.y) * knockbackForce * knockbackScale.y);

        // yatay hızı sıfırla, sonra it
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        rb.AddForce(force, ForceMode2D.Impulse);
    }

    IEnumerator InvulnerabilityRoutine()
    {
        invulnerable = true;
        yield return new WaitForSeconds(invulnerableTime);
        invulnerable = false;
    }

    IEnumerator FlashRoutine()
    {
        if (!spriteRenderer) yield break;
        Color original = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = original;
    }

    void Die()
    {
        if (animator) animator.SetTrigger("Die");
        if (audioSource && deathSfx) audioSource.PlayOneShot(deathSfx);

        // İstersen burada loot/puan vb. ekle
        Destroy(gameObject, 0.25f);
    }
}
