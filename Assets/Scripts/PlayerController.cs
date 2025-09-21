using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Wall Jump")]
    public float wallJumpHorizontalForce = 10f;
    public float wallJumpVerticalForce = 12f;
    private bool isOnWall = false;
    private int wallDir = 0; // -1: sol duvar, 1: sağ duvar
    private int lastWallDir = 0;
    private float lastWallCoyoteCounter = 0f;
    [Header("Wall Coyote Time")]
    public float wallCoyoteTime = 0.15f;
    private float wallCoyoteCounter = 0f;
    [Header("Input (New Input System)")]
    public InputActionReference Move;
    public InputActionReference Jump;
    public InputActionReference Dash;
    public InputActionReference Attack;
    public InputActionReference Heal;
    public InputActionReference Pause;
    public InputActionReference HeavyAttack;
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
    public float facingLockDuration = 0.3f;
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
    private SaveSystem saveSystem;
    [Header("Wall Raycast Offsets")]
    public float wallRaycastVerticalOffset = 0.3f;
    private bool wallJumpRequested = false;
    private float wallJumpBufferTime = 0.15f;
    private float wallJumpBufferCounter = 0f;
    [Header("Heavy Attack")]
    public float heavyAttackHoldTime = 0.5f;
    public float heavyAttackCooldown = 1.0f;
    private float lastHeavyAttackTime = -10f;
    private float attackButtonHeldTime = 0f;
    private bool heavyAttackQueued = false;
    private bool isHeavying = false;
    [Header("Limb Throw (Fırlayan El)")]
    public float frameTime = 0.05f;
    public GameObject limbPrefab;
    private GameObject currentLimb;
    private EnemyHealth2D hitEnemy;
    public float limbThrowSpeed = 15f;
    public float limbPullDuration = 0.5f;
    public LayerMask enemyLayer;
    public InputActionReference Pull;
    public float pullSpeed = 15f;
    private bool heavyAttackButtonHeld = false;

    void Awake()
    {
        Time.timeScale = 1;
        rb = GetComponent<Rigidbody2D>();
        abilityManager = GetComponent<AbilityManager>();
        saveSystem = GetComponent<SaveSystem>();

        if (!animator) animator = GetComponent<Animator>();
        if (!audioSource) audioSource = GetComponent<AudioSource>();
        if (attackHitbox) attackHitbox.gameObject.SetActive(false);
        if (pauseMenuUI) pauseMenuUI.SetActive(false);
        RecalcMaxJumps();
        transform.localScale = new Vector3(1f, 1f, 1f);
    }
    void Start()
    {
        if (saveSystem != null)
            saveSystem.Load(gameObject);

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var enemy in enemies)
        {
            Debug.Log($"Enemy {enemy.name} is on layer: {LayerMask.LayerToName(enemy.layer)}");
        }
    }
    void OnApplicationQuit()
    {
        if (saveSystem != null)
            saveSystem.Save(gameObject);
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
        if (Pull) { Pull.action.Enable(); Pull.action.performed += OnPullPerformed; }
        if (HeavyAttack != null)
            HeavyAttack.action.performed += OnHeavyAttack;
    }
    void OnDisable()
    {
        if (Move) Move.action.Disable();
        if (Jump) { Jump.action.performed -= OnJumpPerformed; Jump.action.canceled -= OnJumpCanceled; Jump.action.Disable(); }
        if (Dash) { Dash.action.performed -= OnDashPerformed; Dash.action.Disable(); }
        if (Attack) { Attack.action.performed -= OnAttackPerformed; Attack.action.Disable(); }
        if (Heal) { Heal.action.performed -= OnHealPerformed; Heal.action.Disable(); }
        if (Pause) { Pause.action.performed -= OnPausePerformed; Pause.action.Disable(); }
        if (Pull) { Pull.action.performed -= OnPullPerformed; Pull.action.Disable(); }
        if (HeavyAttack != null)
            HeavyAttack.action.performed -= OnHeavyAttack;
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
        if (!facingLocked && Mathf.Abs(moveInput.x) > 0.01f)
        {
            lockedFacing = Mathf.Sign(moveInput.x);
        }
        transform.localScale = new Vector3(lockedFacing * 1f, 1f, 1f);
        if (facingLocked)
        {
            facingLockTimer -= Time.deltaTime;
            if (facingLockTimer <= 0f)
                facingLocked = false;
        }
        if (bufferCounter > 0f && (coyoteCounter > 0f || jumpCount < maxJumps))
        {
            wallJumpRequested = true;
            wallJumpBufferCounter = wallJumpBufferTime;
            bufferCounter = 0f;
        }
        if (wallJumpBufferCounter > 0f)
            wallJumpBufferCounter -= Time.deltaTime;
        if (!jumpHeld && rb.linearVelocity.y > 0f)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * variableJumpMultiplier);
        if (IsGrounded())
        {
            coyoteCounter = coyoteTime;
            jumpCount = 0;
        }
        else coyoteCounter -= Time.deltaTime;
        animator.SetBool("IsGrounded", IsGrounded());
        bool wasOnWall = isOnWall;
        isOnWall = false;
        wallDir = 0;
        if (!IsGrounded())
        {
            Vector2 pos = transform.position;
            Vector2 upOffset = pos + Vector2.up * wallRaycastVerticalOffset;
            Vector2 downOffset = pos + Vector2.down * wallRaycastVerticalOffset;
            RaycastHit2D hitRight = Physics2D.Raycast(pos, Vector2.right, wallCheckDistance, wallLayer);
            if (hitRight.collider == null)
                hitRight = Physics2D.Raycast(upOffset, Vector2.right, wallCheckDistance, wallLayer);
            if (hitRight.collider == null)
                hitRight = Physics2D.Raycast(downOffset, Vector2.right, wallCheckDistance, wallLayer);
            RaycastHit2D hitLeft = Physics2D.Raycast(pos, Vector2.left, wallCheckDistance, wallLayer);
            if (hitLeft.collider == null)
                hitLeft = Physics2D.Raycast(upOffset, Vector2.left, wallCheckDistance, wallLayer);
            if (hitLeft.collider == null)
                hitLeft = Physics2D.Raycast(downOffset, Vector2.left, wallCheckDistance, wallLayer);
            if (hitRight.collider != null)
            {
                wallDir = 1;
                isOnWall = true;
                coyoteCounter = coyoteTime;
                jumpCount = 0;
            }
            else if (hitLeft.collider != null)
            {
                wallDir = -1;
                isOnWall = true;
                coyoteCounter = coyoteTime;
                jumpCount = 0;
            }
        }
        if (isOnWall)
        {
            wallCoyoteCounter = wallCoyoteTime;
            lastWallDir = wallDir != 0 ? wallDir : lastWallDir;
            lastWallCoyoteCounter = wallCoyoteTime;
        }
        else
        {
            wallCoyoteCounter -= Time.deltaTime;
            lastWallCoyoteCounter -= Time.deltaTime;
        }
        animator.SetBool("WallJump", isOnWall);
        UpdateSoulBar();
        RecalcMaxJumps();
        if (Attack != null && Attack.action != null)
        {
            if (Attack.action.IsPressed())
            {
                if (!heavyAttackButtonHeld)
                {
                    heavyAttackButtonHeld = true;
                    attackButtonHeldTime = 0f;
                }
                attackButtonHeldTime += Time.deltaTime;
            }
            else if (heavyAttackButtonHeld)
            {
                if (attackButtonHeldTime >= heavyAttackHoldTime && !isHeavying && Time.time > lastHeavyAttackTime + heavyAttackCooldown)
                {
                    StartCoroutine(HeavyAttackRoutine());
                }
                heavyAttackButtonHeld = false;
                attackButtonHeldTime = 0f;
            }
        }
    }
    void FixedUpdate()
    {
        if (wallJumpRequested && wallJumpBufferCounter > 0f)
        {
            DoJump();
            wallJumpRequested = false;
            wallJumpBufferCounter = 0f;
        }
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
        // AbilityManager'dan dash yeteneğinin açık olup olmadığını kontrol et
        if (abilityManager != null && abilityManager.CanDash())
        {
            StartDash();
        }
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
    public void IncreaseSoul(int amount) { Soul += amount; Debug.Log($"Soul increased by {amount}. Total soul: {Soul}"); }
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
    void OnPausePerformed(InputAction.CallbackContext ctx) { if (isPaused) ResumeGame(); else PauseGame(); }
    bool IsGrounded() => groundCheck && Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);
    void DoJump()
    {
        if (((isOnWall || wallCoyoteCounter > 0f || lastWallCoyoteCounter > 0f)) && !IsGrounded())
        {
            int jumpWallDir = wallDir;
            if (jumpWallDir == 0) jumpWallDir = lastWallDir;
            if (jumpWallDir == 0) jumpWallDir = (int)Mathf.Sign(transform.localScale.x);
            rb.linearVelocity = new Vector2(wallJumpHorizontalForce * -jumpWallDir, wallJumpVerticalForce);
            wallCoyoteCounter = 0f;
            lastWallCoyoteCounter = 0f;
        }
        else
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
        jumpCount++;
        coyoteCounter = 0f;
        PlayOne(jumpSound);
    }
    // metehancekci/magarareceli/magaraReceli-c354ca461671bdc0711870d4b7d693c7cf44512b/Assets/Scripts/PlayerController.cs

    void StartDash()
    {
        // Dash'in kilidi açık değilse, zaten dash yapıyorsa veya cooldown'daysa işlemi iptal et
        if (!abilityManager.CanDash() || isDashing || Time.time < lastDashTime + dashCooldown)
        {
            return;
        }

        isDashing = true;
        lastDashTime = Time.time;
        rb.linearVelocity = new Vector2(transform.localScale.x * dashForce, rb.linearVelocity.y);
        animator?.SetTrigger("Dash");
        PlayOne(dashSound); // Dash sesini de ekledim
        Invoke(nameof(_EndDash), dashDuration);
    }
    void _EndDash() => isDashing = false;
    IEnumerator AttackSwing()
    {
        isAttacking = true;
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
    private void OnHeavyAttack(InputAction.CallbackContext context)
    {
        if (!isHeavying && Time.time > lastHeavyAttackTime + heavyAttackCooldown)
        {
            StartCoroutine(HeavyAttackRoutine());
        }
    }
    private IEnumerator HeavyAttackRoutine()
    {
        isHeavying = true;
        lastHeavyAttackTime = Time.time;
        if (animator != null)
            animator.SetTrigger("HeavyAttack");
        yield return new WaitForSeconds(1.15f);
        isHeavying = false;
    }
    public void HeavyStart() { AttackHitboxEnable(); lastHeavyAttackTime = Time.time; }
    public void HeavyEnd() { AttackHitboxDisable(); animator.SetBool("Heavying", false); isHeavying = false; }
    void PlayOne(AudioClip clip) { if (clip && audioSource) audioSource.PlayOneShot(clip); }
    void PauseGame() { Time.timeScale = 0f; isPaused = true; if (pauseMenuUI) pauseMenuUI.SetActive(true); }
    public void ResumeGame() { Time.timeScale = 1f; isPaused = false; if (pauseMenuUI) pauseMenuUI.SetActive(false); }
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
        Gizmos.color = Color.cyan;
        Vector2 checkPos = transform.position + new Vector3(transform.localScale.x * 1f, 0, 0);
        Gizmos.DrawWireSphere(checkPos, 0.5f);
    }
    public void AttackHitboxEnable() { if (attackHitbox) attackHitbox.gameObject.SetActive(true); }
    public void AttackHitboxDisable() { if (attackHitbox) attackHitbox.gameObject.SetActive(false); }

    // --- PULL MEKANİĞİ BAŞLANGIÇ ---

    void OnPullPerformed(InputAction.CallbackContext ctx)
    {
        if (isAttacking || isHealing || isDashing) return; // Başka bir aksiyon sırasında engelle
        if (animator != null && currentLimb == null)
        {
            animator.SetTrigger("Pull");
        }
    }

    // Event 1: Animasyonda eli oluşturur
    public void LimbSpawn()
    {
        if (limbPrefab == null) return;

        currentLimb = Instantiate(limbPrefab, transform.position, Quaternion.identity);
        Rigidbody2D limbRb = currentLimb.GetComponent<Rigidbody2D>();
        if (limbRb != null)
        {
            limbRb.linearVelocity = new Vector2(transform.localScale.x * limbThrowSpeed, 0);
        }
        currentLimb.transform.localScale = new Vector3(transform.localScale.x, 1f, 1f);

        Destroy(currentLimb, 3f); // El 3 saniye sonra her türlü yok olsun
    }

    // Event 2: Düşmanı kontrol eder
    public void LimbHitboxCheck()
    {
        if (enemyLayer.value == 0) return;

        Vector2 checkPos = transform.position + new Vector3(transform.localScale.x * 1f, 0, 0);

        Debug.Log($"Checking hitbox at position: {checkPos}, radius: 0.5f");

        Collider2D[] hits = Physics2D.OverlapCircleAll(checkPos, 0.5f, enemyLayer);
        if (hits.Length > 0)
        {
            Debug.Log($"Found {hits.Length} colliders.");
        }

        foreach (var col in hits)
        {
            EnemyHealth2D enemy = col.GetComponentInParent<EnemyHealth2D>();
            if (enemy != null)
            {
                hitEnemy = enemy;
                Debug.Log("Enemy hit: " + enemy.name);
                if (currentLimb != null) Destroy(currentLimb);
                break;
            }
        }
    }

    // Event 3: Eli geri çeker ve düşmanı çeker
    public void LimbReturn()
    {
        if (currentLimb != null)
        {
            Destroy(currentLimb);
            currentLimb = null;
        }

        if (hitEnemy != null)
        {
            StartCoroutine(PullEnemyCoroutine(hitEnemy.transform));
            hitEnemy = null;
        }
    }

    // YENİ VE GARANTİLİ ÇEKME FONKSİYONU
    private IEnumerator PullEnemyCoroutine(Transform enemyTransform)
    {
        if (enemyTransform == null)
        {
            yield break;
        }

        // Örnek: var enemyAI = enemyTransform.GetComponent<YourEnemyAIScript>();
        // if (enemyAI != null) enemyAI.enabled = false;

        float journeyTime = limbPullDuration;
        float elapsedTime = 0f;

        while (elapsedTime < journeyTime)
        {
            if (enemyTransform == null)
            {
                yield break;
            }

            Vector3 targetPosition = new Vector3(transform.position.x, enemyTransform.position.y, enemyTransform.position.z);

            enemyTransform.position = Vector3.MoveTowards(
                enemyTransform.position,
                targetPosition,
                pullSpeed * Time.deltaTime);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // if (enemyAI != null) enemyAI.enabled = true;
    }
    // --- PULL MEKANİĞİ BİTİŞ ---
}

