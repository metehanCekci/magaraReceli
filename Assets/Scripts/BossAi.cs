using System.Collections;
using UnityEngine;

public class BossAi : MonoBehaviour
{
    [Header("Movement")]
    public Transform leftTrigger;
    public Transform rightTrigger;
    public float moveSpeed = 2f;
    public float wallOffset = 0.3f;
    private bool movingRight = true;
    private bool canMove = true;

    [Header("Spike Attack")]
    public GameObject spikePrefab1;
    public GameObject spikePrefab2;

    [Header("Boulder Attack")]
    public GameObject boulderPrefab;
    public Transform boulderSpawnPoint;
    public Transform player;

    [Header("Lazer Launcher Attack")]
    public GameObject lazerLauncherPrefab;
    public GameObject lazerLauncherPrefab2;
    public GameObject lazerLauncherPrefab3;
    public GameObject lazerLauncherPrefab4;

    [Header("Charge Attack")]
    public float chargeSpeed = 6f;
    public float chargeDuration = 4f;

    [Header("General Settings")]
    public float timeBetweenAttacks = 2f;

    [Header("Attack Chances (percentages)")]
    [Range(0, 100)] public int spikeChance = 0;
    [Range(0, 100)] public int boulderChance = 70;
    [Range(0, 100)] public int lazerChance = 20;
    [Range(0, 100)] public int chargeChance = 10;

    [Header("Visuals")]
    public GameObject exclamationMark; // exclamation mark above boss

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

        int roll = Random.Range(0, 100);
        int cumulative = 0;

        if (roll < (cumulative += spikeChance))
        {
            // Spike attack doesn't stop movement
            yield return StartCoroutine(SpikeAttack());
        }
        else if (roll < (cumulative += boulderChance))
        {
            // Boulder attack doesn't stop movement
            yield return StartCoroutine(BoulderAttack());
        }
        else if (roll < (cumulative += lazerChance))
        {
            if (FindObjectOfType<LazerLauncher>() == null)
            {
                // Stop movement only for Lazer
                canMove = false;
                yield return new WaitForSeconds(0.5f);
                yield return StartCoroutine(LazerLauncherAttack());
                yield return new WaitForSeconds(0.5f);
                canMove = true;
            }
        }
        else if (roll < (cumulative += chargeChance))
        {
            // Stop movement only for Charge
            canMove = false;
            yield return new WaitForSeconds(0.5f);
            yield return StartCoroutine(ChargeAttack());
            yield return new WaitForSeconds(0.5f);
            canMove = true;
        }
    }
}


    #region Attacks

    IEnumerator SpikeAttack()
    {
        GameObject spike1 = Instantiate(spikePrefab1, spikePrefab1.transform.position, Quaternion.identity);
        spike1.SetActive(true);

        yield return new WaitForSeconds(1f);

        GameObject spike2 = Instantiate(spikePrefab2, spikePrefab2.transform.position, Quaternion.identity);
        spike2.SetActive(true);

        yield return null;
    }

    IEnumerator BoulderAttack()
    {
        int boulderCount = 3;
        float delayBetween = 0.5f;

        for (int i = 0; i < boulderCount; i++)
        {
            GameObject boulder = Instantiate(boulderPrefab, boulderSpawnPoint.position, Quaternion.identity);
            boulder.GetComponent<BoulderBehaviour>().Initialize(player.position.x);
            yield return new WaitForSeconds(delayBetween);
        }
    }

    IEnumerator LazerLauncherAttack()
    {
        GameObject[] topCannons = { lazerLauncherPrefab, lazerLauncherPrefab2 };
        GameObject[] bottomCannons = { lazerLauncherPrefab3, lazerLauncherPrefab4 };

        yield return StartCoroutine(ActivateCannons(topCannons));
        yield return new WaitForSeconds(1f);
        yield return StartCoroutine(ActivateCannons(bottomCannons));
    }

    IEnumerator ActivateCannons(GameObject[] cannons)
    {
        float fadeTime = 1.5f;

        foreach (GameObject cannonPrefab in cannons)
        {
            GameObject cannon = Instantiate(cannonPrefab, cannonPrefab.transform.position, Quaternion.identity);
            cannon.SetActive(true);

            SpriteRenderer sr = cannon.GetComponent<SpriteRenderer>();
            StartCoroutine(FadeIn(sr, fadeTime));

            LazerLauncher launcher = cannon.GetComponent<LazerLauncher>();
            launcher.ActivateCannon();

            cannon.tag = "Cannon";
        }

        yield return null;
    }

    IEnumerator FadeIn(SpriteRenderer sr, float time)
    {
        if (!sr) yield break;
        Color c = sr.color;
        c.a = 0;
        sr.color = c;

        float elapsed = 0f;
        while (elapsed < time)
        {
            c.a = Mathf.Lerp(0, 1, elapsed / time);
            sr.color = c;
            elapsed += Time.deltaTime;
            yield return null;
        }

        c.a = 1;
        sr.color = c;
    }

    IEnumerator ChargeAttack()
    {
        // Show exclamation mark before charging
        if (exclamationMark)
            exclamationMark.SetActive(true);

        // Wait so player can react
        yield return new WaitForSeconds(1f);

        // Hide exclamation mark as the charge starts
        

        Debug.Log("Boss started charging!");

        float timer = 0f;

        while (timer < chargeDuration)
        {
            Vector3 targetPos = movingRight ? rightTrigger.position : leftTrigger.position;
            transform.position = Vector2.MoveTowards(transform.position, targetPos, chargeSpeed * Time.deltaTime);

            if (Vector2.Distance(transform.position, targetPos) <= wallOffset)
                movingRight = !movingRight;

            timer += Time.deltaTime;
            yield return null;
        }

        if (exclamationMark)
            exclamationMark.SetActive(false);
        Debug.Log("Boss finished charging!");
    }

    #endregion
}
