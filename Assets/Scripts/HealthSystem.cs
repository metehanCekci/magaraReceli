using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HealthSystem : MonoBehaviour
{
    [Header("Layer Ignore (i-frame)")]
    public int playerLayer = -1;
    public int enemyAttackLayer = -1;

    [Header("Health")]
    public int maxHealth = 5;
    public int currentHealth;

    [Header("Hurt Feedback")]
    public float invulnerableTime = 0.15f;
    public SpriteRenderer spriteRenderer;
    public Animator animator;



    [Header("Knockback")]
    public Rigidbody2D rb;

    [Header("Health UI")]
    public HealthBarScript heartSystem;

    bool invulnerable;

    void Awake()
    {
        currentHealth = maxHealth;

        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!spriteRenderer) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (!animator) animator = GetComponent<Animator>();
        if (playerLayer < 0) playerLayer = gameObject.layer;
        if (heartSystem == null) heartSystem = FindObjectOfType<HealthBarScript>();
    }

    public bool IsInvulnerable() => invulnerable;

    public void TakeDamage(int amount, Vector2 hitFrom)
    {
        if (invulnerable || currentHealth <= 0) return;

        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        if (animator) animator.SetTrigger("Hurt");
        SFXPlayer.Instance.PlayHurt();

        if (heartSystem != null)
            heartSystem.UpdateHealth(currentHealth, maxHealth);

        if (rb)
        {
            Vector2 dir = ((Vector2)transform.position - hitFrom).normalized;
            rb.AddForce(dir * 5f, ForceMode2D.Impulse);
        }

        StartCoroutine(InvulnerabilityRoutine());
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);

        if (heartSystem != null)
            heartSystem.UpdateHealth(currentHealth, maxHealth);

        Debug.Log($"Player healed by {amount}. Current health: {currentHealth}");
    }

    IEnumerator InvulnerabilityRoutine()
    {
        invulnerable = true;
        if (enemyAttackLayer >= 0)
            Physics2D.IgnoreLayerCollision(playerLayer, enemyAttackLayer, true);

        StartCoroutine(FlashRedRoutine());

        yield return new WaitForSeconds(invulnerableTime);

        invulnerable = false;
        if (enemyAttackLayer >= 0)
            Physics2D.IgnoreLayerCollision(playerLayer, enemyAttackLayer, false);
    }

    IEnumerator FlashRedRoutine()
    {
        if (!spriteRenderer) yield break;
        Color original = spriteRenderer.color;
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(invulnerableTime);
        spriteRenderer.color = original;
    }

    void Die()
    {
        if (heartSystem != null)
            heartSystem.UpdateHealth(0, maxHealth);

        SFXPlayer.Instance.PlayHurt();

        if (DeathMenu.Instance != null)
            DeathMenu.Instance.gameObject.SetActive(true);

        Time.timeScale = 0;
    }
}
