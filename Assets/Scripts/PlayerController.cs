using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Input (New Input System)")]
    public InputActionReference Move;   // Vector2 (x) veya 1D Axis
    public InputActionReference Jump;   // Button
    public InputActionReference Dash;   // Button
    public InputActionReference Attack; // Button
    public InputActionReference Heal;   // Button

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
    bool isDashing; float dashTimer, lastDashTime;

    [Header("Combat")]
    public Transform attackHitbox;          // child; Collider2D isTrigger=true
    public float attackDuration = 0.20f;    // hitbox açık kalma süresi
    [SerializeField] float attackCooldown = 0.20f; // basışlar arası
    float lastAttackTime;
    int swingId;                            // <<< her basışta artar
    public int CurrentSwingId => swingId;   // <<< hitbox bunu okur
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

    Rigidbody2D rb;
    Vector2 moveInput;
    AbilityManager abilityManager;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        abilityManager = GetComponent<AbilityManager>();
        if (!animator) animator = GetComponent<Animator>();
        if (!audioSource) audioSource = GetComponent<AudioSource>();
        if (attackHitbox) attackHitbox.gameObject.SetActive(false); // güvenlik
        RecalcMaxJumps();
    }

    void OnEnable()
    {
        if (Move)  Move.action.Enable();
        if (Jump) { Jump.action.Enable(); Jump.action.performed += OnJumpPerformed; Jump.action.canceled += OnJumpCanceled; }
        if (Dash) { Dash.action.Enable(); Dash.action.performed += OnDashPerformed; }
        if (Attack){ Attack.action.Enable(); Attack.action.performed += OnAttackPerformed; } // sadece performed
        if (Heal)  { Heal.action.Enable(); Heal.action.performed += OnHealPerformed; }
    }

    void OnDisable()
    {
        if (Move)  Move.action.Disable();
        if (Jump) { Jump.action.performed -= OnJumpPerformed; Jump.action.canceled -= OnJumpCanceled; Jump.action.Disable(); }
        if (Dash) { Dash.action.performed -= OnDashPerformed; Dash.action.Disable(); }
        if (Attack){ Attack.action.performed -= OnAttackPerformed; Attack.action.Disable(); }
        if (Heal)  { Heal.action.performed -= OnHealPerformed; Heal.action.Disable(); }
    }

    void RecalcMaxJumps()
    {
        maxJumps = abilityManager ? abilityManager.GetMaxJumps() : baseMaxJumps;
        if (jumpCount > maxJumps) jumpCount = maxJumps;
    }

    void Update()
{
    float x = 0f;

    if (Move != null && Move.action != null)
    {
        // Move action'ını kullanarak hareket input'unu alıyoruz
        if (Move.action.expectedControlType == "Vector2")
        {
            x = Move.action.ReadValue<Vector2>().x;
        }
        else
        {
            x = Move.action.ReadValue<float>();
        }

        // Yürüyüş animasyonunu kontrol ediyoruz
        if (Mathf.Abs(x) > 0.01f)
        {
            animator.SetBool("Walking", true); // Hareket ediyoruz, yürüyüş animasyonunu oynat
        }
        else
        {
            animator.SetBool("Walking", false); // Hareket etmiyoruz, yürüyüş animasyonu durur
        }
    }

    moveInput = new Vector2(Mathf.Clamp(x, -1f, 1f), 0f);

    // Anlık hareket
    if (!isDashing)
    {
        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
    }

    // Yön değiştirme
    if (Mathf.Abs(moveInput.x) > 0.01f)
    {
        transform.localScale = new Vector3(Mathf.Sign(moveInput.x), 1, 1); // Yönü değiştiriyoruz
    }

    // Jump buffer consume
    if (bufferCounter > 0f && (coyoteCounter > 0f || jumpCount < maxJumps))
    {
        DoJump();
        bufferCounter = 0f;
    }

    // Variable jump (daha uzun zıplama)
    if (!jumpHeld && rb.linearVelocity.y > 0f)
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * variableJumpMultiplier);

    // Ground / Coyote Check
    if (IsGrounded())
    {
        coyoteCounter = coyoteTime;
        jumpCount = 0;
    }
    else
    {
        coyoteCounter -= Time.deltaTime;
    }

    // Animator state
    animator.SetBool("IsGrounded", IsGrounded());
}


    // ===== Input Callbacks =====
    void OnJumpPerformed(InputAction.CallbackContext ctx){ jumpHeld = true; bufferCounter = jumpBufferTime; }
    void OnJumpCanceled (InputAction.CallbackContext ctx){ jumpHeld = false; }

    void OnDashPerformed(InputAction.CallbackContext ctx)
    {
        if (abilityManager && !abilityManager.CanDash()) return;
        if (Time.time < lastDashTime + dashCooldown) return;
        StartDash();
    }

    void OnAttackPerformed(InputAction.CallbackContext ctx)
    {
        if (Time.time < lastAttackTime + attackCooldown) return;

        Physics2D.SyncTransforms(); // fizik senkron
        if (rb) rb.WakeUp();

        swingId++; // <<< her basışta yeni swing
        StartCoroutine(AttackSwing());
        lastAttackTime = Time.time;
    }

    void OnHealPerformed(InputAction.CallbackContext ctx)
    {
        if (abilityManager && !abilityManager.CanHeal()) return;
        if (!isHealing) StartCoroutine(HealRoutine());
    }

    // ===== Core =====
    bool IsGrounded()
    {
        if (!groundCheck) return false;
        return Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);
    }

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
        isDashing = true; dashTimer = dashDuration; lastDashTime = Time.time;
        rb.linearVelocity = new Vector2(transform.localScale.x * dashForce, rb.linearVelocity.y);
        animator?.SetTrigger("Dash");
        PlayOne(dashSound);
        // dash bitişini Update/Fixed'de değil, korutinle de kapatabilirsin; gerek yoksa böyle kalabilir.
        Invoke(nameof(_EndDash), dashDuration);
    }
    void _EndDash(){ isDashing = false; }

    System.Collections.IEnumerator AttackSwing()
{
    isAttacking = true;

    // İlk animasyonu tetikle
    animator?.SetTrigger("Swing1"); // İlk animasyonu tetikleyen trigger

    if (attackHitbox) 
        attackHitbox.gameObject.SetActive(true);

    PlayOne(attackSound);

    yield return new WaitForSeconds(attackDuration);

    // İlk animasyon bittiğinde, ikinci animasyonu tetikle
    animator?.SetTrigger("Swing2"); // İkinci animasyonu tetikleyen trigger

    if (attackHitbox) 
        attackHitbox.gameObject.SetActive(false);
    
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

    void PlayOne(AudioClip clip){ if (clip && audioSource) audioSource.PlayOneShot(clip); }

    void OnDrawGizmosSelected()
    {
        if (groundCheck)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
        }
    }
}
