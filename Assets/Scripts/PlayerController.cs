using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerControllerInput : MonoBehaviour
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
    public Transform attackHitbox;
    public float attackDuration = 0.25f;
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

    void Awake(){
        rb = GetComponent<Rigidbody2D>();
        abilityManager = GetComponent<AbilityManager>();
        if (!animator) animator = GetComponent<Animator>();
        if (!audioSource) audioSource = GetComponent<AudioSource>();
        RecalcMaxJumps();
    }

    void OnEnable(){
        if (Move) Move.action.Enable();
        if (Jump){ Jump.action.Enable(); Jump.action.performed += OnJumpPerformed; Jump.action.canceled += OnJumpCanceled; }
        if (Dash){ Dash.action.Enable(); Dash.action.performed += OnDashPerformed; }
        if (Attack){ Attack.action.Enable(); Attack.action.performed += OnAttackPerformed; }
        if (Heal){ Heal.action.Enable(); Heal.action.performed += OnHealPerformed; }
    }

    void OnDisable(){
        if (Move) Move.action.Disable();
        if (Jump){ Jump.action.performed -= OnJumpPerformed; Jump.action.canceled -= OnJumpCanceled; Jump.action.Disable(); }
        if (Dash){ Dash.action.performed -= OnDashPerformed; Dash.action.Disable(); }
        if (Attack){ Attack.action.performed -= OnAttackPerformed; Attack.action.Disable(); }
        if (Heal){ Heal.action.performed -= OnHealPerformed; Heal.action.Disable(); }
    }

    void RecalcMaxJumps(){
        maxJumps = abilityManager ? abilityManager.GetMaxJumps() : baseMaxJumps;
        if (jumpCount > maxJumps) jumpCount = maxJumps;
    }

    void Update(){
        // Move read (Vector2 önerilir)
        float x = 0f;
        if (Move && Move.action != null){
            if (Move.action.expectedControlType == "Vector2") x = Move.action.ReadValue<Vector2>().x;
            else x = Move.action.ReadValue<float>();
        }
        moveInput = new Vector2(Mathf.Clamp(x, -1f, 1f), 0f);

        // Flip
        if (Mathf.Abs(moveInput.x) > 0.01f)
            transform.localScale = new Vector3(Mathf.Sign(moveInput.x), 1, 1);

        // Jump buffer tüket
        if (bufferCounter > 0f && (coyoteCounter > 0f || jumpCount < maxJumps)){
            DoJump();
            bufferCounter = 0f;
        }

        // Variable jump
        if (!jumpHeld && rb.linearVelocity.y > 0f)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * variableJumpMultiplier);

        // Anim
        animator.SetFloat("Speed", Mathf.Abs(moveInput.x));
        animator.SetBool("IsGrounded", IsGrounded());

        RecalcMaxJumps();
    }

    void FixedUpdate(){
        // ANİ hızlan/dur (smoothing yok)
        if (!isDashing){
            float targetX = moveInput.x * moveSpeed; // basınca anında bu hız
            rb.linearVelocity = new Vector2(targetX, rb.linearVelocity.y);
        }

        // Ground & coyote
        if (IsGrounded()){
            coyoteCounter = coyoteTime;
            jumpCount = 0;
        } else {
            coyoteCounter -= Time.fixedDeltaTime;
        }

        // Dash zamanlayıcı
        if (isDashing){
            dashTimer -= Time.fixedDeltaTime;
            if (dashTimer <= 0f) isDashing = false;
        }
    }

    // ---- Input callbacks ----
    void OnJumpPerformed(InputAction.CallbackContext ctx){
        jumpHeld = true;
        bufferCounter = jumpBufferTime;
    }
    void OnJumpCanceled(InputAction.CallbackContext ctx){
        jumpHeld = false;
    }
    void OnDashPerformed(InputAction.CallbackContext ctx){
        if (abilityManager && !abilityManager.CanDash()) return;
        if (Time.time < lastDashTime + dashCooldown) return;
        StartDash();
    }
    void OnAttackPerformed(InputAction.CallbackContext ctx){
        Debug.Log("Saldırdım");
        if (!isAttacking) StartCoroutine(AttackRoutine());
    }
    void OnHealPerformed(InputAction.CallbackContext ctx){
        if (abilityManager && !abilityManager.CanHeal()) return;
        if (!isHealing) StartCoroutine(HealRoutine());
    }

    // ---- Core ----
    bool IsGrounded(){
        if (!groundCheck) return false;
        return Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);
    }

    void DoJump(){
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        jumpCount++;
        coyoteCounter = 0f;
        animator.SetTrigger("Jump");
        PlayOne(jumpSound);
    }

    void StartDash(){
        isDashing = true;
        dashTimer = dashDuration;
        lastDashTime = Time.time;
        rb.linearVelocity = new Vector2(transform.localScale.x * dashForce, rb.linearVelocity.y);
        animator.SetTrigger("Dash");
        PlayOne(dashSound);
    }

    System.Collections.IEnumerator AttackRoutine(){
        isAttacking = true;
        animator.SetTrigger("Attack");
        if (attackHitbox) attackHitbox.gameObject.SetActive(true);
        PlayOne(attackSound);
        yield return new WaitForSeconds(attackDuration);
        if (attackHitbox) attackHitbox.gameObject.SetActive(false);
        isAttacking = false;
    }

    System.Collections.IEnumerator HealRoutine(){
        isHealing = true;
        animator.SetTrigger("Heal");
        PlayOne(healSound);
        yield return new WaitForSeconds(healTime);
        var hs = GetComponent<HealthSystem>();
        if (hs) hs.Heal(healAmount);
        isHealing = false;
    }

    void PlayOne(AudioClip clip){
        if (clip && audioSource) audioSource.PlayOneShot(clip);
    }

    void OnDrawGizmosSelected(){
        if (groundCheck){
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
        }
    }
}
