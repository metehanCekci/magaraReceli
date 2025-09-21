using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyHealth2D : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 5;
    public int currentHealth;

    [Header("Invulnerability (i-frames)")]
    public float invulnerableTime = 0.2f;

    [Header("Knockback")]
    public bool canBeKnockedBack = true;
    public float knockbackForce = 5f;
    public float knockbackTime = 0.2f;

    [Header("Refs (optional)")]
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    public AudioSource audioSource;

    public SquareShootng doorScript;

    [Header("Boss Settings")]
    public bool isBoss = false;
    public bool isLastBoss = false;

    private bool invulnerable;
    private Rigidbody2D rb;
    private bool isKnockedBack = false;

    void Awake()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        if (!spriteRenderer) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (!audioSource) audioSource = GetComponent<AudioSource>();
        if (!animator) animator = GetComponent<Animator>();
    }

    public void TakeDamage(int amount)
    {
        if (invulnerable || currentHealth <= 0) return;

        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        if (animator) animator.SetTrigger("Hurt");
        if (SFXPlayer.Instance) SFXPlayer.Instance.PlayGore();

        StartCoroutine(FlashRoutine());
        StartCoroutine(InvulnerabilityRoutine());
    }

    public void ApplyKnockback(Vector2 direction)
    {
        if (canBeKnockedBack && !isKnockedBack)
        {
            StartCoroutine(KnockbackRoutine(direction));
        }
    }

    private IEnumerator KnockbackRoutine(Vector2 direction)
    {
        isKnockedBack = true;

        // Geri tepme öncesi hızı sıfırla
        rb.linearVelocity = Vector2.zero;

        // Geri tepme kuvvetini uygula
        rb.AddForce(direction * knockbackForce, ForceMode2D.Impulse);

        // Geri tepme süresi kadar bekle
        yield return new WaitForSeconds(knockbackTime);

        // Geri tepme sonrası hızı sıfırla
        rb.linearVelocity = Vector2.zero;

        isKnockedBack = false;
    }

    public bool IsKnockedBack()
    {
        return isKnockedBack;
    }

    private IEnumerator InvulnerabilityRoutine()
    {
        invulnerable = true;
        yield return new WaitForSeconds(invulnerableTime);
        invulnerable = false;
    }

    private IEnumerator FlashRoutine()
    {
        if (!spriteRenderer) yield break;

        Color original = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = original;
    }

    private void Die()
    {
        if (SFXPlayer.Instance) SFXPlayer.Instance.PlayKill();

        if (isBoss)
        {
            if (doorScript != null)
            {
                doorScript.enabled = true;
            }
        }
        
        else if(isLastBoss)
        {
            this.GetComponent<LastBossDeathHandler>().LastHandler();
        }

        Destroy(gameObject);
    }
}