using System.Collections;
using UnityEngine;

public class KafaBossAi : MonoBehaviour
{
    [Header("References")]
    private Transform player;

    public Transform leftHand;
    public Transform rightHand;
    public Transform head;                
    public GameObject laserPrefab;        
    public GameObject missilePrefab;      

    [Header("Arena Bounds")]
    public Transform arenaLeft;           
    public Transform arenaRight;          

    [Header("General Attack Settings")]
    public float handAttackInterval = 3f;   // time between hand attacks
    public float headAttackInterval = 4f;   // time between head attacks

    [Header("Hand Settings")]
    public float hoverHeight = 5f;        
    public float moveSpeed = 5f;          
    public float hoverDuration = 2f;      

    [Header("Swipe Animation Triggers")]
    public string leftSwipeTrigger = "SwipeLeft";
    public string rightSwipeTrigger = "SwipeRight";

    [Header("Laser Settings")]
    public float laserSpeed = 10f;
    public int laserBursts = 3;           
    public float laserInterval = 0.5f;    

    [Header("Missile Settings")]
    public float missileSpeed = 6f;
    public float missileLifetime = 3f;

    private bool useLeftNext = true; // alternate hands

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // Start both loops at once
        StartCoroutine(HandAttackLoop());
        StartCoroutine(HeadAttackLoop());
    }

    IEnumerator HandAttackLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(handAttackInterval);

            Transform currentHand = useLeftNext ? leftHand : rightHand;
            useLeftNext = !useLeftNext;

            // Randomly pick hover or swipe
            if (Random.value > 0.5f)
                StartCoroutine(HandHover(currentHand));
            else
                StartCoroutine(HandSwipe(currentHand));
        }
    }

    IEnumerator HeadAttackLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(headAttackInterval);

            // Randomly pick laser or missile
            if (Random.value > 0.5f)
                StartCoroutine(HeadLaserAttack());
            else
                StartCoroutine(HomingMissileAttack());
        }
    }

    IEnumerator HandHover(Transform hand)
    {
        Vector3 originalPos = hand.position;
        float targetY = originalPos.y + hoverHeight;

        // Move hand up while tracking player
        while (hand.position.y < targetY)
        {
            Vector3 targetPos = new Vector3(player.position.x, targetY, hand.position.z);
            hand.position = Vector3.MoveTowards(hand.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        // Trigger slam anim
        Animator handAnimator = hand.GetComponentInChildren<Animator>();
        if (handAnimator != null)
            handAnimator.SetTrigger("Slam");

        // Hover above player
        float timer = 0f;
        while (timer < hoverDuration)
        {
            Vector3 hoverPos = new Vector3(player.position.x, targetY, hand.position.z);
            hand.position = Vector3.MoveTowards(hand.position, hoverPos, moveSpeed * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }

        // Enable collider briefly
        CircleCollider2D handCollider = hand.GetComponentInChildren<CircleCollider2D>();
        if (handCollider != null)
        {
            SFXPlayer.Instance.PlayBleep();
            yield return new WaitForSeconds(0.2f);
            handCollider.enabled = true;
            yield return new WaitForSeconds(0.1f);
            SFXPlayer.Instance.PlaySlam();
            yield return new WaitForSeconds(0.2f);
            handCollider.enabled = false;
        }

        yield return new WaitForSeconds(0.4f);

        // Return to original pos
        while (Vector3.Distance(hand.position, originalPos) > 0.01f)
        {
            hand.position = Vector3.MoveTowards(hand.position, originalPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
    }

    IEnumerator HandSwipe(Transform hand)
    {
        Animator handAnimator = hand.GetComponentInChildren<Animator>();
        if (handAnimator != null)
        {
            if (hand == leftHand)
            {
                handAnimator.SetTrigger(leftSwipeTrigger);
                yield return new WaitForSeconds(1.1f);

                CircleCollider2D handCollider = hand.GetComponentInChildren<CircleCollider2D>();
                if (handCollider != null)
                {
                    SFXPlayer.Instance.PlayBleep();
                    handCollider.enabled = true;
                    yield return new WaitForSeconds(0.3f);
                    handCollider.enabled = false;
                }
                yield return new WaitForSeconds(1f);
            }
            else
            {
                handAnimator.SetTrigger(rightSwipeTrigger);
                yield return new WaitForSeconds(0.3f);

                CircleCollider2D handCollider = hand.GetComponentInChildren<CircleCollider2D>();
                if (handCollider != null)
                {
                    handCollider.enabled = true;
                    yield return new WaitForSeconds(3f);
                    handCollider.enabled = false;
                }
            }
        }
    }

    IEnumerator HeadLaserAttack()
    {
        for (int i = 0; i < laserBursts; i++)
        {
            // LEFT ARENA LASER
            if (laserPrefab != null && arenaLeft != null && player != null)
            {
                GameObject laser = Instantiate(laserPrefab, arenaLeft.position, Quaternion.identity);
                laser.SetActive(true);
                SFXPlayer.Instance.PlayLazerFire();

                Vector2 direction = (player.position - arenaLeft.position).normalized;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                laser.transform.rotation = Quaternion.Euler(0, 0, angle);

                Rigidbody2D rb = laser.GetComponent<Rigidbody2D>();
                if (rb != null)
                    rb.linearVelocity = direction * laserSpeed;
            }

            // RIGHT ARENA LASER
            if (laserPrefab != null && arenaRight != null && player != null)
            {
                GameObject laser = Instantiate(laserPrefab, arenaRight.position, Quaternion.identity);
                laser.SetActive(true);

                Vector2 direction = (player.position - arenaRight.position).normalized;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                laser.transform.rotation = Quaternion.Euler(0, 0, angle);

                Rigidbody2D rb = laser.GetComponent<Rigidbody2D>();
                if (rb != null)
                    rb.linearVelocity = direction * laserSpeed;
            }

            yield return new WaitForSeconds(laserInterval);
        }
    }

    IEnumerator HomingMissileAttack()
    {
        if (missilePrefab != null && head != null)
        {
            GameObject missile = Instantiate(missilePrefab, head.position, Quaternion.identity);
            missile.SetActive(true);
            SFXPlayer.Instance.PlayGunFire();

            HomingMissile hm = missile.AddComponent<HomingMissile>();
            hm.target = player;
            hm.speed = missileSpeed;
            hm.lifetime = missileLifetime;
        }

        yield return null;
    }
}
