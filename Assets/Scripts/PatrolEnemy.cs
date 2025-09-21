using UnityEngine;

public class PatrolEnemy : MonoBehaviour
{
    [Header("Patrol")]
    public Transform[] patrolPoints;
    public float moveSpeed = 2f;

    [Header("Combat")]
    public float chaseSpeed = 4f;
    public float detectionRange = 10f;

    private int currentPatrolIndex = 0;
    private Transform player;
    private HealthSystem playerHealth;
    private bool isChasing = false;

    // private Animator animator; // Yorum satırı yapıldı
    private Rigidbody2D rb;
    private EnemyHealth2D enemyHealth;

    private void Awake()
    {
        // animator = GetComponent<Animator>(); // Yorum satırı yapıldı
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

    void Update()
    {
        // Oyuncu veya düşman öldüyse hareket etmeyi bırak
        if (player == null || (playerHealth != null && playerHealth.currentHealth <= 0) || (enemyHealth != null && enemyHealth.currentHealth <= 0))
        {
            isChasing = false;
            rb.linearVelocity = Vector2.zero;
            // animator.SetBool("isWalking", false); // Yorum satırı yapıldı
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Takip etmiyorsa ve oyuncu menzile girdiyse takibe başla
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

        // Animasyonu hıza göre ayarla
        // animator.SetBool("isWalking", rb.velocity.x != 0); // Yorum satırı yapıldı
    }

    void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        Transform targetPoint = patrolPoints[currentPatrolIndex];

        // Hedefe ulaşıldıysa bir sonrakine geç
        if (Vector2.Distance(transform.position, targetPoint.position) < 1f)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }

        // Hedefe doğru hareket et
        float direction = Mathf.Sign(targetPoint.position.x - transform.position.x);
        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
        Flip(direction);
    }

    void ChasePlayer()
    {
        // Dümdüz oyuncuya doğru koş
        float direction = Mathf.Sign(player.position.x - transform.position.x);
        rb.linearVelocity = new Vector2(direction * chaseSpeed, rb.linearVelocity.y);
        Flip(direction);
    }

    void Flip(float direction)
    {
        if (direction > 0)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else if (direction < 0)
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }
}