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

    [Header("Refs (optional)")]
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    public AudioSource audioSource;

    private bool invulnerable;

    void Awake()
    {
        currentHealth = maxHealth;
        if (!spriteRenderer) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (!audioSource) audioSource = GetComponent<AudioSource>();
        if (!animator) animator = GetComponent<Animator>();
    }

    /// <summary>
    /// Apply damage to the enemy
    /// </summary>
    /// <param name="amount">Damage amount</param>
    public void TakeDamage(int amount)
    {
        if (invulnerable || currentHealth <= 0) return;

        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        // Hurt feedback
        if (animator) animator.SetTrigger("Hurt");
        if (SFXPlayer.Instance) SFXPlayer.Instance.PlayGore();
        StartCoroutine(FlashRoutine());

        // i-frames
        StartCoroutine(InvulnerabilityRoutine());
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
        if (animator) animator.SetTrigger("Die");

        // Destroy the enemy immediately
        Destroy(gameObject);
    }
}
