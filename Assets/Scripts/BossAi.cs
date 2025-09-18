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
    public GameObject lazerLauncherPrefab3;
    public GameObject lazerLauncherPrefab4;


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
        int boulderCount = 3;          // How many boulders to drop
        float delayBetween = 0.5f;     // Time between drops

        for (int i = 0; i < boulderCount; i++)
        {
            GameObject boulder = Instantiate(boulderPrefab, boulderSpawnPoint.position, Quaternion.identity);
            boulder.GetComponent<BoulderBehaviour>().Initialize(player.position.x);
            yield return new WaitForSeconds(delayBetween);
        }
    }

    IEnumerator LazerLauncherAttack()
    {
        // Launcher 1
        GameObject launcher1 = Instantiate(lazerLauncherPrefab, lazerLauncherPrefab.transform.position, Quaternion.identity);
        launcher1.SetActive(true);
        SpriteRenderer sr1 = launcher1.GetComponent<SpriteRenderer>();

        // Launcher 2
        GameObject launcher2 = Instantiate(lazerLauncherPrefab2, lazerLauncherPrefab2.transform.position, Quaternion.identity);
        launcher2.SetActive(true);
        SpriteRenderer sr2 = launcher2.GetComponent<SpriteRenderer>();

        // Launcher 3
        GameObject launcher3 = Instantiate(lazerLauncherPrefab3, lazerLauncherPrefab3.transform.position, Quaternion.identity);
        launcher3.SetActive(true);
        SpriteRenderer sr3 = launcher3.GetComponent<SpriteRenderer>();

        // Launcher 4
        GameObject launcher4 = Instantiate(lazerLauncherPrefab4, lazerLauncherPrefab4.transform.position, Quaternion.identity);
        launcher4.SetActive(true);
        SpriteRenderer sr4 = launcher4.GetComponent<SpriteRenderer>();

        // Hepsinin transparan olarak baþlamasý
        Color color1 = sr1.color; color1.a = 0; sr1.color = color1;
        Color color2 = sr2.color; color2.a = 0; sr2.color = color2;
        Color color3 = sr3.color; color3.a = 0; sr3.color = color3;
        Color color4 = sr4.color; color4.a = 0; sr4.color = color4;

        // Fade-in ayarlarý
        float fadeTime = 1.5f;
        float elapsed = 0f;

        while (elapsed < fadeTime)
        {
            float alpha = Mathf.Lerp(0, 1, elapsed / fadeTime);

            color1.a = alpha; sr1.color = color1;
            color2.a = alpha; sr2.color = color2;
            color3.a = alpha; sr3.color = color3;
            color4.a = alpha; sr4.color = color4;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Son alpha'yý garantiye al
        color1.a = 1; sr1.color = color1;
        color2.a = 1; sr2.color = color2;
        color3.a = 1; sr3.color = color3;
        color4.a = 1; sr4.color = color4;

        yield return null;
    }



    #endregion
}
