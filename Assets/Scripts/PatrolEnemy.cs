using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PatrolEnemy : MonoBehaviour
{
    [Header("Movement Settings")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    public float detectionRadius = 5f;
    public float chaseStopDistance = 10f;

    [Header("References")]
    public Transform[] waypoints;
    public Transform player;

    private Rigidbody2D rb;
    private Transform target;
    private int destPoint = 0;
    private EnemyHealth2D enemyHealth;

    private enum State { PATROL, CHASE }
    private State currentState;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        enemyHealth = GetComponent<EnemyHealth2D>();
        currentState = State.PATROL;

        if (waypoints.Length > 0)
        {
            target = waypoints[0];
            FaceTowards(target.position);
        }

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
                player = playerObject.transform;
        }
    }

    void Update()
    {
        if (enemyHealth != null && enemyHealth.IsKnockedBack())
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        if (player == null || waypoints.Length == 0) return;

        switch (currentState)
        {
            case State.PATROL:
                CheckForPlayer();
                break;
            case State.CHASE:
                break; // Use FixedUpdate for movement
        }
    }

    void FixedUpdate()
    {
        if (enemyHealth != null && enemyHealth.IsKnockedBack()) return;

        if (currentState == State.PATROL)
            Patrol();
        else if (currentState == State.CHASE)
            Chase();
    }

    void Patrol()
    {
        Vector2 direction = (target.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * patrolSpeed, rb.linearVelocity.y);

        FaceTowards(target.position);

        if (Vector2.Distance(transform.position, target.position) < 0.3f)
        {
            destPoint = (destPoint + 1) % waypoints.Length;
            target = waypoints[destPoint];
        }
    }

    void Chase()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * chaseSpeed, rb.linearVelocity.y);

        FaceTowards(player.position);

        if (Vector2.Distance(transform.position, player.position) > chaseStopDistance)
            currentState = State.PATROL;
    }

    void CheckForPlayer()
    {
        if (Vector2.Distance(transform.position, player.position) < detectionRadius)
            currentState = State.CHASE;
    }

    void FaceTowards(Vector3 targetPos)
    {
        Vector3 scale = transform.localScale;
        scale.x = (targetPos.x > transform.position.x) ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, chaseStopDistance);
    }
}
