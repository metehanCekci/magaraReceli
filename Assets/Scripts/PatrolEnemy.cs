using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PatrolEnemy : MonoBehaviour
{
    [Header("Movement")]
    public Transform[] patrolPoints;
    public float patrolSpeed = 1f;
    public float chaseSpeed = 2f;

    [Header("Detection")]
    public float detectionRadius = 5f;
    public LayerMask playerLayer;

    private int currentPatrolIndex = 0;
    private bool isChasingPlayer = false;

    private Transform player;
    private Rigidbody2D rb;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();

        // Freeze all movement during patrol
        rb.constraints = RigidbodyConstraints2D.FreezeAll;

    }

    void Update()
    {
        DetectPlayer();

        if (isChasingPlayer)
        {
            // Set constraints for chasing
            if (rb.constraints != RigidbodyConstraints2D.FreezeRotation)
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            ChasePlayer();
        }
        else
        {
            // Freeze all movement during patrol
            if (rb.constraints != RigidbodyConstraints2D.FreezeAll)
                rb.constraints = RigidbodyConstraints2D.FreezeAll;

            Patrol();
        }
    }

    void Patrol()
    {
        if (patrolPoints.Length == 0) return;

        Transform target = patrolPoints[currentPatrolIndex];
        Vector3 direction = target.position - transform.position;

        transform.position = Vector3.MoveTowards(transform.position, target.position, patrolSpeed * Time.deltaTime);

        if (direction.x > 0) transform.localScale = new Vector3(0.5f, 0.5f, 1);
        else if (direction.x < 0) transform.localScale = new Vector3(-0.5f, 0.5f, 1);

        if (Vector3.Distance(transform.position, target.position) < 0.2f)
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    void DetectPlayer()
    {
        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, detectionRadius, playerLayer);
        isChasingPlayer = playerCollider != null;
    }

    void ChasePlayer()
    {
        Vector3 targetPosition = new Vector3(player.position.x, transform.position.y, transform.position.z);
        Vector3 direction = targetPosition - transform.position;

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, chaseSpeed * Time.deltaTime);

        if (direction.x > 0) transform.localScale = new Vector3(0.5f, 0.5f, 1);
        else if (direction.x < 0) transform.localScale = new Vector3(-0.5f, 0.5f, 1);
    }
}
