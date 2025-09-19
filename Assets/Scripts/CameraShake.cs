using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    private Vector3 originalPos;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        originalPos = transform.localPosition;
    }

    /// <summary>
    /// Triggers a camera shake.
    /// </summary>
    /// <param name="duration">How long the shake lasts (seconds)</param>
    /// <param name="magnitude">Shake intensity</param>
    public void Shake(float duration = 0.2f, float magnitude = 0.3f)
    {
        StopAllCoroutines();
        StartCoroutine(ShakeCoroutine(duration, magnitude));
    }

    private IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime; // unscaled to ignore timeScale

            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = originalPos + new Vector3(x, y, 0);

            yield return null;
        }

        transform.localPosition = originalPos;
    }
}
