using System.Collections;
using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("Movement Attack")]
    public Transform leftTrigger;
    public Transform rightTrigger;
    public float moveSpeed = 2f;
    public float moveAttackDuration = 5f;
    public float wallOffset = 0.3f;

    [Header("Spike Attack")]
    public GameObject spikePrefab;       // Original spike prefab

    [Header("Boulder Attack")]
    public GameObject boulderPrefab;     // Original boulder prefab
    public Transform boulderSpawnPoint;  
    public Transform player;             

    [Header("General Settings")]
    public float timeBetweenAttacks = 2f;

    [Header("References")]
    public Animator animator;
    public AudioSource audioSource;
    public AudioClip moveAttackSound;
    public AudioClip spikeAttackSound;
    public AudioClip boulderAttackSound;

    private bool movingRight = true;

    private void Start()
    {
        StartCoroutine(AttackCycle());
    }

    IEnumerator AttackCycle()
    {
        while (true)
        {
            int attackIndex = Random.Range(0, 3); // 0=Move, 1=Spike, 2=Boulder

            if (attackIndex == 0)
                StartCoroutine(MoveAroundArena());
            else if (attackIndex == 1)
                StartCoroutine(SpikeAttack());
            else
                StartCoroutine(BoulderAttack());

            yield return new WaitForSeconds(timeBetweenAttacks);
        }
    }

    #region Attacks

    IEnumerator MoveAroundArena()
    {
        float elapsed = 0f;
        movingRight = true;

        while (elapsed < moveAttackDuration)
        {
            Vector3 targetPos = movingRight ? rightTrigger.position : leftTrigger.position;
            transform.position = Vector2.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

            if (Vector2.Distance(transform.position, targetPos) <= wallOffset)
                movingRight = !movingRight;

            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator SpikeAttack()
    {
        GameObject spike = Instantiate(spikePrefab, spikePrefab.transform.position, Quaternion.identity);
        spike.SetActive(true);

        // Do NOT wait for spike to finish. Boss can start next attack after cooldown.
        yield return null;
    }

    IEnumerator BoulderAttack()
    {
        GameObject boulder = Instantiate(boulderPrefab, boulderSpawnPoint.position, Quaternion.identity);
        boulder.GetComponent<BoulderBehaviour>().Initialize(player.position.x);

        // Do NOT wait for boulder to finish
        yield return null;
    }

    #endregion

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);
    }
}
