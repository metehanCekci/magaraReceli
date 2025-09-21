// metehancekci/magarareceli/magaraReceli-c354ca461671bdc0711870d4b7d693c7cf44512b/Assets/Scripts/TextBoxScript.cs

using UnityEngine;
using TMPro;
using System.Collections;

public class TextBoxScript : MonoBehaviour
{
    [Header("Diyalog Ayarlarý")]
    public GameObject textBox;
    public TextMeshProUGUI textDisplay;
    [TextArea(3, 10)]
    public string dialogueString;
    public float typingSpeed = 0.04f;

    [Header("Tetiklenecek Olay")]
    public GameObject objectToReveal;

    // Bu deðiþken artýk sadece objenin ortaya çýkýp çýkmadýðýný kontrol edecek.
    private bool hasObjectBeenRevealed = false;
    private Coroutine typingCoroutine;

    void Start()
    {
        if (objectToReveal != null)
        {
            objectToReveal.SetActive(false);
        }
        if (textBox != null)
        {
            textBox.SetActive(false);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // DEÐÝÞÝKLÝK: Buradaki "tek seferlik" kontrolünü kaldýrdýk.
        // Artýk oyuncu alana her girdiðinde diyalog baþlayacak.
        if (other.CompareTag("Player"))
        {
            textBox.SetActive(true);
            typingCoroutine = StartCoroutine(TypeText(dialogueString));
        }
    }

    IEnumerator TypeText(string text)
    {
        textDisplay.text = "";
        foreach (char letter in text.ToCharArray())
        {
            textDisplay.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }

            textDisplay.text = "";
            textBox.SetActive(false);

            // DEÐÝÞÝKLÝK: Bu kontrol artýk sadece objenin 1 kere ortaya çýkmasýný saðlýyor.
            // Diyaloðun gösterilmesini engellemiyor.
            if (!hasObjectBeenRevealed)
            {
                if (objectToReveal != null)
                {
                    objectToReveal.SetActive(true);
                }
                hasObjectBeenRevealed = true;
            }
        }
    }
}