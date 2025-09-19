using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Input (New Input System)")]
    public InputActionReference Move;
    public InputActionReference Jump;
    public InputActionReference Dash;
    public InputActionReference Attack;
    public InputActionReference Heal;
    public InputActionReference Pause;

    [Header("Movement")]
    public float moveSpeed = 8f;

    [Header("Jumping")]
    public float jumpForce = 15f;
    [Range(0f, 1f)] public float variableJumpMultiplier = 0.5f;
    public float coyoteTime = 0.15f;
    public float jumpBufferTime = 0.20f;

    [Header("Double Jump (Ability)")]
    public int baseMaxJumps = 1;
    int maxJumps, jumpCount;
    float coyoteCounter, bufferCounter;
    bool jumpHeld;

    [Header("Dashing")]
    public float dashForce = 20f;
    public float dashDuration = 0.20f;
    public float dashCooldown = 0.50f;
    bool isDashing; 
    float lastDashTime;

    [Header("Combat")]
    public Transform attackHitbox;
    public float attackDuration = 0.20f;
    [SerializeField] float attackCooldown = 0.20f;
    float lastAttackTime;
    int swingId;
    public int CurrentSwingId => swingId;
    bool isAttacking;

    [Header("Healing")]
    public float healTime = 1f;
    public int healAmount = 1;
    bool isHealing;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundRadius = 0.15f;
    public LayerMask groundLayer;

    [Header("References")]
    public Animator animator;
    public AudioSource audioSource;
    public AudioClip jumpSound, dashSound, attackSound, healSound;

    [Header("Pause Menu")]
    public GameObject pauseMenuUI; // Assign a Canvas panel in inspector

    [Header("Soul")]
    public float MaxSoul = 100f;  // Maximum Soul deðeri
    public float Soul = 0f;       // Mevcut Soul deðeri
    //public int currentSoul = 0;

    bool isPaused = false;
    Rigidbody2D rb;
    Vector2 moveInput;
    AbilityManager abilityManager;

    [Header("Soul System")]
    public SoulSystem soulSystem;  // SoulSystem referansý

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        abilityManager = GetComponent<AbilityManager>();
        if (!animator) animator = GetComponent<Animator>();
        if (!audioSource) audioSource = GetComponent<AudioSource>();
        if (attackHitbox) attackHitbox.gameObject.SetActive(false);
        if (pauseMenuUI) pauseMenuUI.SetActive(false);
        RecalcMaxJumps();
    }

    void OnEnable()
    {
        if (Move) Move.action.Enable();
        if (Jump) { Jump.action.Enable(); Jump.action.performed += OnJumpPerformed; Jump.action.canceled += OnJumpCanceled; }
        if (Dash) { Dash.action.Enable(); Dash.action.performed += OnDashPerformed; }
        if (Attack) { Attack.action.Enable(); Attack.action.performed += OnAttackPerformed; }
        if (Heal) { Heal.action.Enable(); Heal.action.performed += OnHealPerformed; }
        if (Pause) { Pause.action.Enable(); Pause.action.performed += OnPausePerformed; }
    }

    void OnDisable()
    {
        if (Move) Move.action.Disable();
        if (Jump) { Jump.action.performed -= OnJumpPerformed; Jump.action.canceled -= OnJumpCanceled; Jump.action.Disable(); }
        if (Dash) { Dash.action.performed -= OnDashPerformed; Dash.action.Disable(); }
        if (Attack) { Attack.action.performed -= OnAttackPerformed; Attack.action.Disable(); }
        if (Heal) { Heal.action.performed -= OnHealPerformed; Heal.action.Disable(); }
        if (Pause) { Pause.action.performed -= OnPausePerformed; Pause.action.Disable(); }
    }

    void RecalcMaxJumps()
    {
        maxJumps = abilityManager ? abilityManager.GetMaxJumps() : baseMaxJumps;
        if (jumpCount > maxJumps) jumpCount = maxJumps;
    }

    void Update()
    {
        if (isPaused) return;

        // Movement input
        float x = 0f;
        if (Move && Move.action != null)
        {
            if (Move.action.expectedControlType == "Vector2") x = Move.action.ReadValue<Vector2>().x;
            else x = Move.action.ReadValue<float>();
        }
        moveInput = new Vector2(Mathf.Clamp(x, -1f, 1f), 0f);

        if (!isDashing)
            rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);

        if (Mathf.Abs(moveInput.x) > 0.01f)
            transform.localScale = new Vector3(Mathf.Sign(moveInput.x), 1, 1);

        if (bufferCounter > 0f && (coyoteCounter > 0f || jumpCount < maxJumps))
        {
            DoJump();
            bufferCounter = 0f;
        }

        if (!jumpHeld && rb.linearVelocity.y > 0f)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * variableJumpMultiplier);

        if (IsGrounded()) { coyoteCounter = coyoteTime; jumpCount = 0; }
        else coyoteCounter -= Time.deltaTime;

        if (animator)
        {
            animator.SetFloat("Speed", Mathf.Abs(moveInput.x));
            animator.SetBool("IsGrounded", IsGrounded());
        }

        // Soul bar'ý güncelle
        UpdateSoulBar();

        RecalcMaxJumps();
    }

    void UpdateSoulBar()
    {
        if (soulSystem != null)
        {
            // MaxSoul ile Soul'ün oranýný alarak Soul bar'ýnýn %'lik deðerini hesaplýyoruz
            float soulPercentage = Soul / MaxSoul;
            soulSystem.UpdateSoulBar(soulPercentage);  // Soul bar'ýný güncelle
        }
    }

    void OnJumpPerformed(InputAction.CallbackContext ctx) { jumpHeld = true; bufferCounter = jumpBufferTime; }
    void OnJumpCanceled(InputAction.CallbackContext ctx) { jumpHeld = false; }

    void OnDashPerformed(InputAction.CallbackContext ctx)
    {
        if (abilityManager && !abilityManager.CanDash()) return;
        if (Time.time < lastDashTime + dashCooldown) return;
        StartDash();
    }

    void OnAttackPerformed(InputAction.CallbackContext ctx)
    {
        if (Time.time < lastAttackTime + attackCooldown) return;
        Physics2D.SyncTransforms();
        rb?.WakeUp();
        swingId++;
        StartCoroutine(AttackSwing());
        lastAttackTime = Time.time;

        // Soul'u artýr
          // Örnek olarak her saldýrý ile 10 Soul arttýrýyoruz, ihtiyaca göre düzenleyebilirsiniz.
    }
    public void IncreaseSoul(int amount)
    {
        Soul += amount;
        Debug.Log($"Soul increased by {amount}. Total soul: {Soul}");
    }
    void OnHealPerformed(InputAction.CallbackContext ctx)
    {
        // Eðer Soul barý dolmuþsa
        if (Soul == MaxSoul)
        {
            // Heal komutunu aldýðýnda, karakterin canýna 60 ekleyelim
            Debug.Log("Heal command received and Soul is full.");

            // HealthSystem bileþenini al
            var healthSystem = GetComponent<HealthSystem>();

            // Eðer HealthSystem bileþeni varsa, caný 60 artýr
            if (healthSystem != null)
            {
                healthSystem.Heal(60); // 60 can ekliyoruz
                Debug.Log("Player's health increased by 60.");
            }

            // Soul barýný sýfýrla
            Soul = 0f;
            Debug.Log("Soul bar reset to 0.");

            // Soul barýný güncelle
            UpdateSoulBar(); // Yeni deðeri güncelliyoruz
        }
        else
        {
            Debug.Log("Cannot heal, Soul is not full.");
        }
    }



    void OnPausePerformed(InputAction.CallbackContext ctx)
    {
        if (isPaused) ResumeGame();
        else PauseGame();
    }

    bool IsGrounded() => groundCheck && Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);

    void DoJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        jumpCount++; coyoteCounter = 0f;
        animator?.SetTrigger("Jump");
        PlayOne(jumpSound);
    }

    void StartDash()
    {
        isDashing = true; lastDashTime = Time.time;
        rb.linearVelocity = new Vector2(transform.localScale.x * dashForce, rb.linearVelocity.y);
        animator?.SetTrigger("Dash");
        PlayOne(dashSound);
        Invoke(nameof(_EndDash), dashDuration);
    }
    void _EndDash() => isDashing = false;

    System.Collections.IEnumerator AttackSwing()
    {
        isAttacking = true;
        animator?.SetTrigger("Attack");
        if (attackHitbox) attackHitbox.gameObject.SetActive(true);
        PlayOne(attackSound);
        yield return new WaitForSeconds(attackDuration);
        if (attackHitbox) attackHitbox.gameObject.SetActive(false);
        isAttacking = false;
    }

    System.Collections.IEnumerator HealRoutine()
    {
        isHealing = true;
        animator?.SetTrigger("Heal");
        PlayOne(healSound);
        yield return new WaitForSeconds(healTime);
        var hs = GetComponent<HealthSystem>(); if (hs) hs.Heal(healAmount);
        isHealing = false;
    }

    void PlayOne(AudioClip clip) { if (clip && audioSource) audioSource.PlayOneShot(clip); }

    void PauseGame()
    {
        Time.timeScale = 0f;
        isPaused = true;
        if (pauseMenuUI) pauseMenuUI.SetActive(true);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        isPaused = false;
        if (pauseMenuUI) pauseMenuUI.SetActive(false);
    }

    public void ResumeButton() => ResumeGame();
    public void RestartButton() => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    public void QuitButton() => Application.Quit();

    void OnDrawGizmosSelected()
    {
        if (groundCheck)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
        }
    }
}
