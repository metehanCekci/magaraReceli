// metehancekci/magarareceli/magaraReceli-c354ca461671bdc0711870d4b7d693c7cf44512b/Assets/Scripts/TextBoxScript.cs

using UnityEngine;
using TMPro;
using System.Collections; // Coroutine için bu satýr gerekli

public class TextBoxScript : MonoBehaviour
{
    [Header("Diyalog Ayarlarý")]
    public GameObject textBox;
    public TextMeshProUGUI textDisplay;
    [TextArea(3, 10)]
    public string dialogueString;
    public float typingSpeed = 0.04f; // Yazýnýn yazýlma hýzý

    [Header("Tetiklenecek Olay")]
    public GameObject objectToReveal;

    private bool hasDialogueFinished = false;
    private Coroutine typingCoroutine; // Yazma iþlemini kontrol etmek için

    void Start()
    {
        if (objectToReveal != null)
        {
            objectToReveal.SetActive(false);
        }
        // Baþlangýçta diyalog kutusunun kapalý olduðundan emin ol
        if (textBox != null)
        {
            textBox.SetActive(false);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasDialogueFinished)
        {
            textBox.SetActive(true);
            // Direkt yazýyý göstermek yerine, yavaþ yavaþ yazma efektini baþlat
            typingCoroutine = StartCoroutine(TypeText(dialogueString));
        }
    }

    // Yazýyý harf harf yazdýran fonksiyon (Coroutine)
    IEnumerator TypeText(string text)
    {
        textDisplay.text = ""; // Baþlamadan önce metin kutusunu temizle
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
            // Eðer hala devam eden bir yazma iþlemi varsa durdur
            if (typingCoroutine != null)
            {
                StopCoroutine(typingCoroutine);
            }

            // SORUN 1 ÇÖZÜMÜ: Diyalog kutusunu kapatmadan önce içindeki yazýyý temizle
            textDisplay.text = "";
            textBox.SetActive(false);

            if (!hasDialogueFinished)
            {
                if (objectToReveal != null)
                {
                    objectToReveal.SetActive(true);
                }
                hasDialogueFinished = true;
            }
        }
    }
}