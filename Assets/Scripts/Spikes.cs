using System.Collections;
using UnityEngine;

public class Spikes : MonoBehaviour
{
    [Header("References")]
    public Transform dropTrigger;   // Hedef trigger (yavaş yaklaşacağı nokta)
    public float groundY = -3f;     // Zeminin Y koordinatı

    [Header("Settings")]
    public float approachSpeed = 2f;   // Triggera yaklaşma hızı
    public float fallSpeed = 15f;      // Düşüş hızı
    public float waitAtTrigger = 1f;   // Triggerda bekleme süresi
    public float stayOnGround = 3f;    // Düştükten sonra yerde kalma süresi

    private bool isRunning = false;

    private void OnEnable()
    {
        if (!isRunning)
            StartCoroutine(SpikeRoutine());
    }

    IEnumerator SpikeRoutine()
    {
        isRunning = true;

        // 1. DropTrigger’a yavaşça ilerle
        while (Vector2.Distance(transform.position, dropTrigger.position) > 0.1f)
        {
            transform.position = Vector2.MoveTowards(transform.position, dropTrigger.position, approachSpeed * Time.deltaTime);
            yield return null;
        }

        // 2. Trigger noktasında biraz bekle
        yield return new WaitForSeconds(waitAtTrigger);

        // 3. Aşağı düş
        while (transform.position.y > groundY)
        {
            transform.position += Vector3.down * fallSpeed * Time.deltaTime;
            yield return null;
        }

        // 4. Yerde bekle
        yield return new WaitForSeconds(stayOnGround);

        // 5. Kendini kapat
        gameObject.SetActive(false);
        isRunning = false;
    }
}
