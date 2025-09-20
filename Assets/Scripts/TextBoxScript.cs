using System.Collections;
using UnityEngine;
using TMPro;

public class NpcTextBox : MonoBehaviour
{
    [Header("Settings")]
    [TextArea] public string message;        // Message to display
    public float fadeDuration = 0.5f;        // Time to fade in/out
    public float typingSpeed = 0.05f;        // Delay between letters

    private CanvasGroup canvasGroup;
    private TMP_Text textDisplay;
    private GameObject canvasObj;
    private Coroutine typingCoroutine;
    private Coroutine fadeCoroutine;
    private bool isShowing = false;

    private void Awake()
    {
        // Find Canvas inside NPC
        canvasObj = transform.Find("Canvas").gameObject;
        if (canvasObj == null)
        {
            Debug.LogError("Canvas not found under NPC!");
            return;
        }

        // Ensure CanvasGroup
        canvasGroup = canvasObj.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = canvasObj.AddComponent<CanvasGroup>();

        // Find "Text (TMP)" inside Canvas
        textDisplay = canvasObj.transform.Find("Text (TMP)").GetComponent<TMP_Text>();
        if (textDisplay == null)
            Debug.LogError("TMP_Text not found inside Canvas!");

        // Hide at start
        canvasGroup.alpha = 0;
        canvasObj.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isShowing)
        {
            isShowing = true;
            canvasObj.SetActive(true);

            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeCanvas(1));

            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            typingCoroutine = StartCoroutine(TypeText(message));
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && isShowing)
        {
            isShowing = false;

            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeOutAndDisable());
        }
    }

    private IEnumerator TypeText(string textToType)
    {
        textDisplay.text = "";
        foreach (char c in textToType)
        {
            textDisplay.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    private IEnumerator FadeCanvas(float targetAlpha)
    {
        float startAlpha = canvasGroup.alpha;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = targetAlpha;
    }

    private IEnumerator FadeOutAndDisable()
    {
        yield return FadeCanvas(0);
        canvasObj.SetActive(false);
    }
}
