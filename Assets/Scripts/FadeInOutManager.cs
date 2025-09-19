using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class FadeInOutManager : MonoBehaviour
{
    public static FadeInOutManager Instance;

    [Header("Fade Settings")]
    public Image fadeImage;      // Canvas üzerindeki siyah Image
    public float fadeDuration = 1f; // Fade süresi

    private void Awake()
    {
        // Singleton yapısı
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Sahneler arası devam et
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Başlangıçta siyah ekranı açıp fade in yap
        if (fadeImage)
        {
            fadeImage.gameObject.SetActive(true);
            fadeImage.color = Color.black;
            StartCoroutine(FadeIn());
        }

        // Sahne yüklenince tekrar fade in yap
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(FadeIn());
    }

    // Fade in
    private IEnumerator FadeIn()
    {
        float timer = 0f;
        Color color = fadeImage.color;
        fadeImage.gameObject.SetActive(true); // Başlangıçta aktif et

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            color.a = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        color.a = 0f;
        fadeImage.color = color;
        fadeImage.gameObject.SetActive(false); // Fade in bittiğinde deaktif et
    }

    // Fade out ve sahne yükleme
    public void FadeOutAndLoadScene(int sceneIndex)
    {
        StartCoroutine(FadeOutCoroutine(sceneIndex));
    }

    private IEnumerator FadeOutCoroutine(int sceneIndex)
    {
        float timer = 0f;
        Color color = fadeImage.color;
        fadeImage.gameObject.SetActive(true);
        Time.timeScale = 1;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            color.a = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        color.a = 1f;
        fadeImage.color = color;

        // Sahneyi yükle
        SceneManager.LoadScene(sceneIndex);
    }
}
