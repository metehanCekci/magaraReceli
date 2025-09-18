using System.Collections;
using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("Movement")]
    public Transform leftTrigger;
    public Transform rightTrigger;
    public float moveSpeed = 2f;
    public float wallOffset = 0.3f;
    private bool movingRight = true;
    private bool canMove = true;

    [Header("Spike Attack")]
    public GameObject spikePrefab;

    [Header("Boulder Attack")]
    public GameObject boulderPrefab;
    public Transform boulderSpawnPoint;
    public Transform player;

    [Header("Lazer Launcher Attack")]
    public GameObject lazerLauncherPrefab;
    public GameObject lazerLauncherPrefab2;

    [Header("General Settings")]
    public float timeBetweenAttacks = 2f;

    [Header("Attack Chances (percentages)")]
    [Range(0,100)] public int spikeChance = 50;
    [Range(0,100)] public int boulderChance = 40;
    [Range(0,100)] public int lazerChance = 10;

    private void Start()
    {
        StartCoroutine(MovementLoop());
        StartCoroutine(AttackCycle());
    }

    IEnumerator MovementLoop()
    {
        while (true)
        {
            if (canMove)
            {
                Vector3 targetPos = movingRight ? rightTrigger.position : leftTrigger.position;
                transform.position = Vector2.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

                if (Vector2.Distance(transform.position, targetPos) <= wallOffset)
                    movingRight = !movingRight;
            }
            yield return null;
        }
    }

    IEnumerator AttackCycle()
    {
        while (true)
        {
            yield return new WaitForSeconds(timeBetweenAttacks);

            // Stop before attack
            canMove = false;
            yield return new WaitForSeconds(1f);

            // Pick attack based on weighted chances
            int roll = Random.Range(0, 100);
            if (roll < spikeChance)
                yield return StartCoroutine(SpikeAttack());
            else if (roll < spikeChance + boulderChance)
                yield return StartCoroutine(BoulderAttack());
            else if (roll < spikeChance + boulderChance + lazerChance)
            {
                if (FindObjectOfType<LazerLauncher>() == null)
                    yield return StartCoroutine(LazerLauncherAttack());
            }

            // Small delay before moving again
            yield return new WaitForSeconds(1f);
            canMove = true;
        }
    }

    #region Attacks

    IEnumerator SpikeAttack()
    {
        GameObject spike = Instantiate(spikePrefab, spikePrefab.transform.position, Quaternion.identity);
        spike.SetActive(true);
        yield return null;
    }

    IEnumerator BoulderAttack()
    {
        GameObject boulder = Instantiate(boulderPrefab, boulderSpawnPoint.position, Quaternion.identity);
        boulder.GetComponent<BoulderBehaviour>().Initialize(player.position.x);
        yield return null;
    }

    IEnumerator LazerLauncherAttack()
    {
        GameObject launcher = Instantiate(lazerLauncherPrefab, lazerLauncherPrefab.transform.position, Quaternion.identity);
        launcher.SetActive(true);

        GameObject launcher2 = Instantiate(lazerLauncherPrefab2, lazerLauncherPrefab2.transform.position, Quaternion.identity);
        launcher2.SetActive(true);

        // Fade in only first launcher (can repeat for launcher2 if you want)
        SpriteRenderer sr = launcher.GetComponent<SpriteRenderer>();
        Color color = sr.color;
        color.a = 0;
        sr.color = color;

        float fadeTime = 1f;
        float elapsed = 0f;
        while (elapsed < fadeTime)
        {
            color.a = Mathf.Lerp(0, 1, elapsed / fadeTime);
            sr.color = color;
            elapsed += Time.deltaTime;
            yield return null;
        }
        color.a = 1;
        sr.color = color;

        yield return null;
    }

    #endregion
}
