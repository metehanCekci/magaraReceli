using UnityEngine;

public class PatrolEnemy : MonoBehaviour
{
    public Transform[] patrolPoints;  // Düşmanın gideceği noktalar
    public float moveSpeed = 3f;      // Düşmanın hızını ayarlıyoruz
    public float detectionRadius = 5f; // Düşmanın algılama yarıçapı
    public LayerMask playerLayer;     // Oyuncu layer'ı
    private int currentPatrolIndex = 0; // Şu anki gidiş noktası
    private bool isChasingPlayer = false; // Oyuncuya kitlenme durumu

    private Transform player; // Oyuncu objesi

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;  // Oyuncuyu buluyoruz
    }

    void Update()
    {
        if (isChasingPlayer)
        {
            // Eğer oyuncu görüldüyse, onu takip et
            ChasePlayer();
        }
        else
        {
            // Oyuncu görünmüyorsa, patrol yap
            Patrol();
        }
    }

    void Patrol()
    {
        if (patrolPoints.Length == 0)
            return;

        // Hedef noktaya doğru hareket
        Transform targetPatrolPoint = patrolPoints[currentPatrolIndex];
        Vector3 direction = targetPatrolPoint.position - transform.position;

        transform.position = Vector3.MoveTowards(transform.position, targetPatrolPoint.position, moveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPatrolPoint.position) < 0.2f)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;  // Gelecek noktaya geç (dönüp başa sar)
        }

        // Oyuncuyu algılamaya çalış
        DetectPlayer();
    }

    void DetectPlayer()
    {
        // Oyuncuyu algılamak için bir alanda kontrol yapıyoruz
        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, detectionRadius, playerLayer);

        if (playerCollider != null)
        {
            // Eğer oyuncu bu alandaysa, düşman ona doğru hareket etmeye başlar
            isChasingPlayer = true;
        }
        else
        {
            isChasingPlayer = false;
        }
    }

    void ChasePlayer()
    {
        // Oyuncu görüldü, ona doğru hareket et
        Vector3 direction = player.position - transform.position;
        transform.position = Vector3.MoveTowards(transform.position, player.position, moveSpeed * Time.deltaTime);

        // Hedefe yaklaşırken, yönünü oyuncuya çevir
        if (direction.x > 0) 
            transform.localScale = new Vector3(1, 1, 1); // Sağ
        else 
            transform.localScale = new Vector3(-1, 1, 1); // Sol
    }
}
