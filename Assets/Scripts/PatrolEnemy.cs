using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))] // Rigidbody2D bileşenini zorunlu hale getir
public class PatrolEnemy : MonoBehaviour
{
    public float patrolSpeed = 2f;
    public float chaseSpeed = 4f;
    public float detectionRadius = 5f;
    public float chaseStopDistance = 10f;
    public Transform[] waypoints;
    public SpriteRenderer graphics;
    public Transform player;

    private Rigidbody2D rb; // Rigidbody2D referansı
    private Transform target;
    private int destPoint = 0;
    private EnemyHealth2D enemyHealth;

    private enum State { PATROL, CHASE }
    private State currentState;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); // Rigidbody2D bileşenini al
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
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); // Knockback sırasında X hızını durdur
            return;
        }

        if (player == null || waypoints.Length == 0)
        {
            return;
        }

        switch (currentState)
        {
            case State.PATROL:
                CheckForPlayer();
                break;
            case State.CHASE:
                // Chase(); // Fizik güncellemeleri için Update yerine FixedUpdate kullanacağız
                break;
        }
    }

    // Fizik işlemleri için FixedUpdate kullanmak daha doğrudur.
    void FixedUpdate()
    {
        if (enemyHealth != null && enemyHealth.IsKnockedBack())
        {
            return; // Knockback sırasında hareket etme
        }

        if (currentState == State.PATROL)
        {
            Patrol();
        }
        else if (currentState == State.CHASE)
        {
            Chase();
        }
    }


    void Patrol()
    {
        float directionX = target.position.x - transform.position.x;
        rb.linearVelocity = new Vector2(Mathf.Sign(directionX) * patrolSpeed, rb.linearVelocity.y);

        if (Mathf.Abs(directionX) < 0.3f)
        {
            destPoint = (destPoint + 1) % waypoints.Length;
            target = waypoints[destPoint];
        }

        //FlipSprite(rb.linearVelocity.x);
    }

    void Chase()
    {
        float directionX = player.position.x - transform.position.x;
        rb.linearVelocity = new Vector2(Mathf.Sign(directionX) * chaseSpeed, rb.linearVelocity.y);

        //FlipSprite(rb.linearVelocity.x);

        // İsteğe bağlı: Oyuncu menzilden çıkarsa devriyeye geri dön
        if (Vector2.Distance(transform.position, player.position) > chaseStopDistance)
        {
            currentState = State.PATROL;
        }
    }

    void CheckForPlayer()
    {
        if (Vector2.Distance(transform.position, player.position) < detectionRadius)
        {
            currentState = State.CHASE;
        }
    }

    // Sprite'ın yönünü çevirmek için bu fonksiyonu tekrar aktif edelim
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