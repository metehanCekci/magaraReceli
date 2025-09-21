// metehancekci/magarareceli/magaraReceli-c354ca461671bdc0711870d4b7d693c7cf44512b/Assets/Scripts/PatrolEnemy.cs

using UnityEngine;

public class PatrolEnemy : MonoBehaviour
{
    [Header("Patrol")]
    public Transform[] patrolPoints;
    public float moveSpeed = 2f;

    [Header("Combat")]
    public float chaseSpeed = 4f;
    public float detectionRange = 10f;
    public float attackRange = 1.5f;
    public float attackCooldown = 2f;
    public float jumpForce = 12f;

    [Header("Checks")]
    public Transform groundCheck;
    public Transform wallCheck;
    public float checkRadius = 0.2f;
    public LayerMask whatIsGround;

    private int currentPatrolIndex = 0;
    private Transform player;
    private HealthSystem playerHealth;
    private bool isChasing = false;
    private float lastAttackTime = -10f;

    private Animator animator;
    private Rigidbody2D rb;
    private EnemyHealth2D enemyHealth;
    private bool isGrounded;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        enemyHealth = GetComponent<EnemyHealth2D>();
    }

    void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
            playerHealth = playerObject.GetComponent<HealthSystem>();
        }
    }

    private void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, whatIsGround);
    }

    void Update()
    {
        if (player == null || (playerHealth != null && playerHealth.currentHealth <= 0) || (enemyHealth != null && enemyHealth.currentHealth <= 0))
        {
            isChasing = false;
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            animator.SetBool("isWalking", false);
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (!isChasing && distanceToPlayer <= detectionRange)
        {
            isChasing = true;
        }

        if (isChasing)
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
    }

    void Patrol()
    {
        // Eğer hiç patrol noktası atanmamışsa veya liste boşsa, dur ve devam etme.
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            animator.SetBool("isWalking", false);
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        //animator.SetBool("isWalking", true);

        Transform targetPoint = patrolPoints[currentPatrolIndex];

        // Hedefe ulaşıp ulaşmadığımızı kontrol et
        if (Vector2.Distance(transform.position, targetPoint.position) < 1f)
        {
            // Bir sonraki hedefe geç
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }

        // Hedefe doğru hareket et
        float direction = Mathf.Sign(targetPoint.position.x - transform.position.x);
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);

        Flip(direction);
    }

    void ChasePlayer()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        float direction = Mathf.Sign(player.position.x - transform.position.x);

        Flip(direction);

        if (distanceToPlayer > attackRange)
        {
            rb.linearVelocity = new Vector2(direction * chaseSpeed, rb.linearVelocity.y);
            animator.SetBool("isWalking", true);

            bool isHittingWall = Physics2D.OverlapCircle(wallCheck.position, checkRadius, whatIsGround);
            if (isHittingWall && isGrounded)
            {
                if (player.position.y > transform.position.y + 1f)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                }
            }
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            //animator.SetBool("isWalking", false);

            if (Time.time >= lastAttackTime + attackCooldown)
            {
                Attack();
            }
        }
    }

    void Attack()
    {
        lastAttackTime = Time.time;
        animator.SetTrigger("Attack");
    }

    void Flip(float direction)
    {
        if (direction > 0.01f)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (direction < -0.01f)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        if (groundCheck != null)
            Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
        if (wallCheck != null)
            Gizmos.DrawWireSphere(wallCheck.position, checkRadius);
    }
}