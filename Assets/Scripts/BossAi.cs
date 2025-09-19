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
    [Range(0, 100)] public int spikeChance = 0;
    [Range(0, 100)] public int boulderChance = 100;
    [Range(0, 100)] public int lazerChance = 0;

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
            yield return new WaitForSeconds(0.5f);

            // Pick attack based on weighted chances
            int roll = Random.Range(0, 100);
            int cumulative = 0;

            if (roll < (cumulative += spikeChance))
            {
                yield return StartCoroutine(SpikeAttack());
            }
            else if (roll < (cumulative += boulderChance))
            {
                yield return StartCoroutine(BoulderAttack());
            }
            else if (roll < (cumulative += lazerChance))
            {
                if (FindObjectOfType<LazerLauncher>() == null)
                    yield return StartCoroutine(LazerLauncherAttack());
            }

            else if (roll < spikeChance + boulderChance + lazerChance)
            {
                if (FindObjectOfType<LazerLauncher>() == null)
                    yield return StartCoroutine(LazerLauncherAttack());
            }

            // Small delay before moving again
            yield return new WaitForSeconds(0.5f);
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
        GameObject[] topCannons = { lazerLauncherPrefab, lazerLauncherPrefab2 };
        GameObject[] bottomCannons = { lazerLauncherPrefab3, lazerLauncherPrefab4 };

        // �st cannonlar aktif edilir
        yield return StartCoroutine(ActivateCannons(topCannons));

        // 1 saniye bekle
        yield return new WaitForSeconds(1f);

        // Alt cannonlar aktif edilir
        yield return StartCoroutine(ActivateCannons(bottomCannons));
    }

    IEnumerator ActivateCannons(GameObject[] cannons)
    {
        float fadeTime = 1.5f;

        foreach (GameObject cannonPrefab in cannons)
        {
            GameObject cannon = Instantiate(cannonPrefab, cannonPrefab.transform.position, Quaternion.identity);
            cannon.SetActive(true);

            // Fade-in
            SpriteRenderer sr = cannon.GetComponent<SpriteRenderer>();
            StartCoroutine(FadeIn(sr, fadeTime));

            // Cannon�u aktive et
            LazerLauncher launcher = cannon.GetComponent<LazerLauncher>();
            launcher.ActivateCannon();

            // Tag ekle ki bulletler vurabilsin
            cannon.tag = "Cannon";
        }

        // Bu sefer s�re yok ��nk� s�rekli ate� edecekler
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




    #endregion
}
