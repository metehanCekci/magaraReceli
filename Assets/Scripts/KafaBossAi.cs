using System.Collections;
using UnityEngine;

public class KafaBossAi : MonoBehaviour
{
    [Header("References")]
    private Transform player;

    public Transform leftHand;
    public Transform rightHand;

    [Header("Attack Settings")]
    public float timeBetweenAttacks = 3f;
    public float hoverHeight = 5f;      // height above current hand position
    public float moveSpeed = 5f;        // speed of hand movement
    public float hoverDuration = 2f;    // time to stay above player

    [Header("Swipe Animation Triggers")]
    public string leftSwipeTrigger = "SwipeLeft";
    public string rightSwipeTrigger = "SwipeRight";

    private bool useLeftNext = true; // alternate hands

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        StartCoroutine(AttackLoop());
    }

    IEnumerator AttackLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(timeBetweenAttacks);

            // Decide which hand to use
            Transform currentHand = useLeftNext ? leftHand : rightHand;
            useLeftNext = !useLeftNext;

            // Randomly decide attack type: 0 = hover/slam, 1 = swipe
            int attackType = Random.Range(0, 2);

            if (attackType == 0)
            {
                // Hover + slam attack
                StartCoroutine(HandHover(currentHand));
            }
            else
            {

                // Swipe attack
                StartCoroutine(HandSwipe(currentHand));
            }
        }
    }

    IEnumerator HandHover(Transform hand)
    {
        Vector3 originalPos = hand.position; // store starting position
        float targetY = originalPos.y + hoverHeight;

        // Move hand up to target Y while tracking player's X
        while (hand.position.y < targetY)
        {
            Vector3 targetPos = new Vector3(player.position.x, targetY, hand.position.z);
            hand.position = Vector3.MoveTowards(hand.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        // Hover above player
        float timer = 0f;
        Animator handAnimator = hand.GetComponentInChildren<Animator>();
        if (handAnimator != null)
        {
            handAnimator.SetTrigger("Slam");
        }

        while (timer < hoverDuration)
        {
            Vector3 hoverPos = new Vector3(player.position.x, targetY, hand.position.z);
            hand.position = Vector3.MoveTowards(hand.position, hoverPos, moveSpeed * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }

        // Enable the hand's child CircleCollider2D for 0.3 seconds
        CircleCollider2D handCollider = hand.GetComponentInChildren<CircleCollider2D>();
        if (handCollider != null)
        {
            yield return new WaitForSeconds(0.2f);
            handCollider.enabled = true;
            yield return new WaitForSeconds(0.3f);
            handCollider.enabled = false;
        }

        // Optional extra wait before returning
        yield return new WaitForSeconds(0.4f);

        // Return to original position
        while (Vector3.Distance(hand.position, originalPos) > 0.01f)
        {
            hand.position = Vector3.MoveTowards(hand.position, originalPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        Debug.Log(hand.name + " finished attack and returned!");
    }

    // Swipe attack coroutine
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
                    handCollider.enabled = true;
                    yield return new WaitForSeconds(0.3f);
                    handCollider.enabled = false;
                }
                yield return new WaitForSeconds(1);
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

        // Wait for the swipe animation to finish
    }
}
