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
        // Ýlk lazer launcher'ýný oluþtur
        GameObject launcher = Instantiate(lazerLauncherPrefab, lazerLauncherPrefab.transform.position, Quaternion.identity);
        launcher.SetActive(true);

        // Ýkinci lazer launcher'ýný oluþtur
        GameObject launcher2 = Instantiate(lazerLauncherPrefab2, lazerLauncherPrefab2.transform.position, Quaternion.identity);
        launcher2.SetActive(true);

        // Ýlk launcher için fade-in efekti
        SpriteRenderer sr1 = launcher.GetComponent<SpriteRenderer>();
        Color color1 = sr1.color;
        color1.a = 0;
        sr1.color = color1;

        // Ýkinci launcher için fade-in efekti
        SpriteRenderer sr2 = launcher2.GetComponent<SpriteRenderer>();
        Color color2 = sr2.color;
        color2.a = 0;
        sr2.color = color2;

        // Fade-in zamanlamasý
        float fadeTime = 1f;
        float elapsed = 0f;

        // Ýlk lazer launcher'ý için fade-in
        while (elapsed < fadeTime)
        {
            color1.a = Mathf.Lerp(0, 1, elapsed / fadeTime);
            sr1.color = color1;
            color2.a = Mathf.Lerp(0, 1, elapsed / fadeTime);
            sr2.color = color2;
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Fade-in tamamlandýðýnda son halini ayarla
        color1.a = 1;
        sr1.color = color1;
        color2.a = 1;
        sr2.color = color2;

        yield return null;
    }


    #endregion
}
