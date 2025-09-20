using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;

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

    [Header("Combat Facing Lock")]
    public float facingLockDuration = 0.3f; // Duration to lock facing after attack
    bool facingLocked = false;
    float facingLockTimer = 0f;
    float lockedFacing = 1f;

    [Header("Healing")]
    public float healTime = 3f;
    public int healAmount = 1;
    bool isHealing;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundRadius = 0.15f;
    public LayerMask groundLayer;

    [Header("Wall Check")]
    public float wallCheckDistance = 0.5f;
    public LayerMask wallLayer;

    [Header("References")]
    public Animator animator;
    public AudioSource audioSource;
    public AudioClip jumpSound, dashSound, attackSound, healSound;

    [Header("Pause Menu")]
    public GameObject pauseMenuUI;

    [Header("Soul")]
    public float MaxSoul = 100f;
    public float Soul = 0f;

    bool isPaused = false;
    Rigidbody2D rb;
    Vector2 moveInput;
    AbilityManager abilityManager;

    [Header("Soul System")]
    public SoulSystem soulSystem;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        abilityManager = GetComponent<AbilityManager>();
        if (!animator) animator = GetComponent<Animator>();
        if (!audioSource) audioSource = GetComponent<AudioSource>();
        if (attackHitbox) attackHitbox.gameObject.SetActive(false);
        if (pauseMenuUI) pauseMenuUI.SetActive(false);
        RecalcMaxJumps();

        // Ensure default scale is 2,2,1
        transform.localScale = new Vector3(5f, 1f, 1f);
    }

    void OnEnable()
    {
        attackHitbox.gameObject.SetActive(false);

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
        if (isHealing) return;

        float x = 0f;
        if (Move != null && Move.action != null)
        {
            if (Move.action.expectedControlType == "Vector2")
            {
                x = Move.action.ReadValue<Vector2>().x;
            }
            else
            {
                x = Move.action.ReadValue<float>();
            }

            animator.SetBool("Walking", Mathf.Abs(x) > 0.01f);
        }

        moveInput = new Vector2(Mathf.Clamp(x, -1f, 1f), 0f);

        if (!isDashing)
        {
            rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
        }

        // Handle facing
        if (!facingLocked && Mathf.Abs(moveInput.x) > 0.01f)
        {
            lockedFacing = Mathf.Sign(moveInput.x);
        }
        transform.localScale = new Vector3(lockedFacing * 5f, 1f, 1f);

        // Update facing lock timer
        if (facingLocked)
        {
            facingLockTimer -= Time.deltaTime;
            if (facingLockTimer <= 0f)
                facingLocked = false;
        }

        if (bufferCounter > 0f && (coyoteCounter > 0f || jumpCount < maxJumps))
        {
            //animator.SetBool("Jump", true); // Zıplama başladığında Jump bool'unu aç
            DoJump();
            Debug.Log("Zıpladım ANA");
            
            bufferCounter = 0f;
        }

        if (!jumpHeld && rb.linearVelocity.y > 0f)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * variableJumpMultiplier);

        if (IsGrounded()) {
            coyoteCounter = coyoteTime;
            jumpCount = 0;
        }
        else coyoteCounter -= Time.deltaTime;

        animator.SetBool("IsGrounded", IsGrounded());


        // Wall check logic
        bool isOnWall = false;
        if (!IsGrounded()) {
            RaycastHit2D hitRight = Physics2D.Raycast(transform.position, Vector2.right, wallCheckDistance, wallLayer);
            RaycastHit2D hitLeft = Physics2D.Raycast(transform.position, Vector2.left, wallCheckDistance, wallLayer);
            if ((hitRight && Mathf.Abs(hitRight.normal.x) > 0.9f) || (hitLeft && Mathf.Abs(hitLeft.normal.x) > 0.9f)) {
                isOnWall = true;
            }
        }
        animator.SetBool("WallJump", isOnWall);


        UpdateSoulBar();
        RecalcMaxJumps();
    }

    void UpdateSoulBar()
    {
        if (soulSystem != null)
        {
            float soulPercentage = Soul / MaxSoul;
            soulSystem.UpdateSoulBar(soulPercentage);
        }
    }

    void OnJumpPerformed(InputAction.CallbackContext ctx) { jumpHeld = true; bufferCounter = jumpBufferTime; }
    void OnJumpCanceled(InputAction.CallbackContext ctx) { jumpHeld = false; }

    void OnDashPerformed(InputAction.CallbackContext ctx)
    {
        StartDash();
    }

    void OnAttackPerformed(InputAction.CallbackContext ctx)
    {
        if (isHealing) return;
        if (Time.time < lastAttackTime + attackCooldown) return;
        Physics2D.SyncTransforms();
        rb?.WakeUp();
        swingId++;
        attackHitbox.gameObject.SetActive(false);
        StartCoroutine(AttackSwing());
        lastAttackTime = Time.time;
        attackHitbox.gameObject.SetActive(false);
    }

    public void IncreaseSoul(int amount)
    {
        Soul += amount;
        Debug.Log($"Soul increased by {amount}. Total soul: {Soul}");
    }

    void OnHealPerformed(InputAction.CallbackContext ctx)
    {
        if (Soul == MaxSoul)
        {
            var healthSystem = GetComponent<HealthSystem>();
            rb.linearVelocity = Vector2.zero;
            if (healthSystem != null)
            {
                StartCoroutine(HealWaitRoutine());
                Debug.Log("Player's health increased by 60.");
            }
            Soul = 0f;
            UpdateSoulBar();
        }
    }

    IEnumerator HealRoutine()
    {
        isHealing = true;
        animator.SetTrigger("Heal");

        rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(healTime);
        isHealing = false;
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
    }

    IEnumerator HealWaitRoutine()
    {
        var healthSystem = GetComponent<HealthSystem>();
        Move.action.Disable();
        isHealing = true;
        animator.SetTrigger("Heal");

        PlayOne(healSound);

        yield return new WaitForSeconds(healTime);
        healthSystem.Heal(60);
        isHealing = false;
        Move.action.Enable();
    }

    void OnPausePerformed(InputAction.CallbackContext ctx)
    {
        if (isPaused) ResumeGame();
        else PauseGame();
    }

    bool IsGrounded() => groundCheck && Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);

    void DoJump()
    {
    Debug.Log("Zıpladım");
    rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
    rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    jumpCount++;
    coyoteCounter = 0f;
    
    PlayOne(jumpSound);
    animator.SetBool("Jump", true); // Zıplama başladığında Jump bool'unu aç
    }

    void StartDash()
    {
        isDashing = true; lastDashTime = Time.time;
        rb.linearVelocity = new Vector2(transform.localScale.x * dashForce, rb.linearVelocity.y);
        animator?.SetTrigger("Dash");
        Invoke(nameof(_EndDash), dashDuration);
    }

    void _EndDash() => isDashing = false;

    IEnumerator AttackSwing()
    {
        isAttacking = true;

        // Lock facing during attack
        facingLocked = true;
        facingLockTimer = facingLockDuration;

        animator?.SetTrigger("Swing1");

        if (SFXPlayer.Instance) SFXPlayer.Instance.PlayWhoosh();

        yield return new WaitForSeconds(attackDuration);

        animator?.SetTrigger("Swing2");
        if (attackHitbox)
            attackHitbox.gameObject.SetActive(false);

        isAttacking = false;
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
        if (groundCheck){
            animator.SetBool("Jump", false); // Yere değdiğinde Jump bool'unu kapat
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
        }
    }

    public void AttackHitboxEnable()
    {
        if (attackHitbox)
            attackHitbox.gameObject.SetActive(true);
    }

    public void AttackHitboxDisable()
    {
        if (attackHitbox)
            attackHitbox.gameObject.SetActive(false);
    }
}
