using System.Collections;
using UnityEngine;

public class AirSlamBossAi : MonoBehaviour
{
    [Header("References")]
    public Animator animator;          // Animator controlling boss
    public Transform slamTarget;       // Object with X position where boss slams
    public GameObject slamEffect1;     // First slam effect
    public GameObject slamEffect2;     // Second slam effect
    public Transform bossRoot;         // Parent object to move
    public Collider2D slashHitbox;     // Hitbox for the slash attack
    public Transform player;           // Player transform for facing

    [Header("Settings")]
    public float moveSpeedX = 5f;           // Horizontal speed for Air Slam
    public float approachSpeedX = 3f;       // Horizontal speed for closing distance before slash
    public float slamWaitTime = 1f;         // How long to stay after slam
    public float slashForwardDistance = 1f; // How far boss moves during slash
    public float slashDistanceThreshold = 1.5f; // How close boss wants to be before slashing

    [Header("General Settings")]
    public float timeBetweenAttacks = 2f;

    [Header("Attack Chances (percentages)")]
    [Range(0, 100)] public int airSlamChance = 100; // Only Air Slam for now
    [Range(0, 100)] public int slashChance = 0;     // Slash attack chance

    private void Start()
    {
        if (bossRoot == null)
            bossRoot = transform;

        if (slashHitbox != null)
            slashHitbox.enabled = false;

        StartCoroutine(AttackCycle());
    }

    IEnumerator AttackCycle()
    {
        while (true)
        {
            yield return new WaitForSeconds(timeBetweenAttacks);

            int roll = Random.Range(0, 100);
            int cumulative = 0;

            if (roll < (cumulative += airSlamChance))
            {
                yield return StartCoroutine(AirSlamAttack());
            }
            else if (roll < (cumulative += slashChance))
            {
                yield return StartCoroutine(ApproachAndSlash());
            }
        }
    }

    #region Attacks

    IEnumerator AirSlamAttack()
    {
        if (animator)
            animator.SetTrigger("AirAttack");

        float targetX = slamTarget.position.x;
        Vector3 pos = bossRoot.position;

        while (Mathf.Abs(bossRoot.position.x - targetX) > 0.05f)
        {
            pos = bossRoot.position;
            pos.x = Mathf.MoveTowards(pos.x, targetX, moveSpeedX * Time.deltaTime);
            bossRoot.position = pos;
            yield return null;
        }

        yield return new WaitForSeconds(slamWaitTime);
    }

    #endregion

    public void ShockWave()
    {
        if (slamEffect1)
        {
            GameObject clone = Instantiate(slamEffect1);
            clone.SetActive(true);
        }

        if (slamEffect2)
        {
            GameObject clone2 = Instantiate(slamEffect2);
            clone2.SetActive(true);
        }
    }

    private float lockedSlashTargetX; // store player X before attack

    private IEnumerator ApproachAndSlash()
    {
        Vector3 startPos = bossRoot.position;

        // Determine direction toward player (X only)
        float directionX = (player.position.x - startPos.x) >= 0 ? 1f : -1f;

        // Flip bossRoot to face player
        Vector3 localScale = bossRoot.localScale;
        localScale.x = (directionX < 0) ? -Mathf.Abs(localScale.x) : Mathf.Abs(localScale.x);
        bossRoot.localScale = localScale;

        // Enable walking animation
        if (animator)
            animator.SetBool("Walking", true);

        // Move toward player until within desired distance using approachSpeedX
        while (Mathf.Abs(player.position.x - bossRoot.position.x) > slashDistanceThreshold)
        {
            float moveStep = approachSpeedX * Time.deltaTime;
            float targetX = player.position.x - directionX * slashDistanceThreshold;
            float newX = Mathf.MoveTowards(bossRoot.position.x, targetX, moveStep);
            bossRoot.position = new Vector3(newX, bossRoot.position.y, bossRoot.position.z);
            yield return null;
        }

        // Stop walking animation
        if (animator)
            animator.SetBool("Walking", false);

        // Capture player position here for attack
        lockedSlashTargetX = player.position.x;

        // Trigger slash animation
        if (animator)
            animator.SetTrigger("Slash");
    }

    public void PerformSlash()
    {
        StartCoroutine(SlashCoroutine());
    }

    private IEnumerator SlashCoroutine()
    {
        Vector3 startPos = bossRoot.position;

        // Determine direction toward locked target
        float directionX = (lockedSlashTargetX - startPos.x) >= 0 ? 1f : -1f;

        // Flip bossRoot to face target
        Vector3 localScale = bossRoot.localScale;
        localScale.x = (directionX < 0) ? -Mathf.Abs(localScale.x) : Mathf.Abs(localScale.x);
        bossRoot.localScale = localScale;

        // Forward position toward locked target
        Vector3 forwardPos = new Vector3(startPos.x + directionX * slashForwardDistance, startPos.y, startPos.z);

        float hitDuration = 0.1f;
        float timer = 0f;

        if (slashHitbox != null)
            slashHitbox.enabled = true;

        // Move forward while hitbox is active
        while (timer < hitDuration)
        {
            float newX = Mathf.Lerp(startPos.x, forwardPos.x, timer / hitDuration);
            bossRoot.position = new Vector3(newX, startPos.y, startPos.z);
            timer += Time.deltaTime;
            yield return null;
        }

        bossRoot.position = forwardPos;
        if (slashHitbox != null)
            slashHitbox.enabled = false;


    }
}
