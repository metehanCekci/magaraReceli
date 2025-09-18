using System.Collections;
using UnityEngine;

public class BossAi : MonoBehaviour
{
    [Header("Movement Attack")]
    public Transform leftTrigger;
    public Transform rightTrigger;
    public float moveSpeed = 5f;
    public float moveAttackDuration = 5f;
    public float wallOffset = 0.3f;
    public float timeBetweenAttacks = 2f;

    private bool movingRight = true;

    [Header("References")]
    public Animator animator;
    public AudioSource audioSource;
    public AudioClip moveAttackSound;

    private void Start()
    {
        StartCoroutine(MoveAttackCycle());
    }

    IEnumerator MoveAttackCycle()
    {
        while (true)
        {
            yield return StartCoroutine(MoveAroundArena());
            yield return new WaitForSeconds(timeBetweenAttacks);
        }
    }

    IEnumerator MoveAroundArena()
    {
        Debug.Log("Move attack started");
        //animator?.SetTrigger("MoveAttack");
        //PlaySound(moveAttackSound);

        float elapsed = 0f;
        movingRight = true;

        while (elapsed < moveAttackDuration)
        {
            Vector3 targetPos = movingRight ? rightTrigger.position : leftTrigger.position;

            transform.position = Vector2.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

            if (Vector2.Distance(transform.position, targetPos) <= wallOffset)
            {
                movingRight = !movingRight;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        Debug.Log("Move attack ended");
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
