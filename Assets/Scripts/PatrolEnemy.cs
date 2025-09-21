using UnityEngine;

public class PatrolEnemy : MonoBehaviour
{
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    public float detectionRadius = 5f;
    public float chaseStopDistance = 10f;
    public Transform[] waypoints;
    public SpriteRenderer graphics;
    public LayerMask playerLayer;
    public Transform player;

    private Transform target;
    private int destPoint = 0;
    private EnemyHealth2D enemyHealth;

    private enum State { PATROL, CHASE }
    private State currentState;

    void Start()
    {
        enemyHealth = GetComponent<EnemyHealth2D>();
        currentState = State.PATROL;

        if (waypoints.Length > 0)
        {
            target = waypoints[0];
        }

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }
    }

    void Update()
    {
        if (enemyHealth != null && enemyHealth.IsKnockedBack())
        {
            return;
        }

        if (player == null || waypoints.Length == 0)
        {
            return;
        }

        switch (currentState)
        {
            case State.PATROL:
                Patrol();
                CheckForPlayer();
                break;
            case State.CHASE:
                Chase();
                break;
        }
    }

    void Patrol()
    {
        Vector3 dir = target.position - transform.position;
        transform.Translate(dir.normalized * patrolSpeed * Time.deltaTime, Space.World);

        if (Vector3.Distance(transform.position, target.position) < 0.3f)
        {
            destPoint = (destPoint + 1) % waypoints.Length;
            target = waypoints[destPoint];
        }

        //FlipSprite(dir.x);
    }

    void Chase()
    {
        // Hedef pozisyonu oyuncunun x'i ve düşmanın kendi y'si olarak ayarla
        Vector3 targetPosition = new Vector3(player.position.x, transform.position.y, transform.position.z);

        // Sadece X ekseninde hareket et
        Vector3 dir = targetPosition - transform.position;
        transform.Translate(dir.normalized * chaseSpeed * Time.deltaTime, Space.World);

       // FlipSprite(dir.x);

        // İsteğe bağlı: Oyuncu menzilden çıkarsa devriyeye geri dön
        /*
        if (Vector2.Distance(transform.position, player.position) > chaseStopDistance)
        {
            currentState = State.PATROL;
            target = waypoints[destPoint];
        }
        */
    }

    void CheckForPlayer()
    {
        if (Vector2.Distance(transform.position, player.position) < detectionRadius)
        {
            currentState = State.CHASE;
        }
    }

    /**void FlipSprite(float direction)
    {
        if (direction > 0.1f)
        {
            graphics.flipX = false;
        }
        else if (direction < -0.1f)
        {
            graphics.flipX = true;
        }
    }**/

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, chaseStopDistance);
    }
}