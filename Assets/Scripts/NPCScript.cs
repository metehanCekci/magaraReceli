using System.Collections;
using TMPro;
using UnityEngine;

public class NPCScript : MonoBehaviour
{
    public TextMeshProUGUI dialogueText;      // UI'deki yazý elemaný
    public GameObject dialogueBlock;          // Siyah blok
    public string npcDialogue;                // NPC'nin konuþacaðý yazý
    private bool isNearNPC = false;

    void Start()
    {
        // Baþlangýçta yazýyý gizle
        dialogueText.text = "";
        dialogueBlock.SetActive(false); // Blok da gizli olacak
    }

    void Update()
    {
        // Eðer NPC'ye yaklaþýldýysa, metni yazdýrmaya baþla
        if (isNearNPC && dialogueText.text == "")
        {
            StartCoroutine(DisplayDialogue());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Oyuncu objesinin tag'ýný kontrol et
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player NPC'ye yaklaþtý");
            isNearNPC = true;
            dialogueBlock.SetActive(true); // NPC'nin üstündeki blok görünsün
            dialogueText.text = ""; // Önceki metni sýfýrla
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Oyuncu objesinin tag'ýný kontrol et
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player NPC'den ayrýldý");
            isNearNPC = false;
            dialogueBlock.SetActive(false); // NPC'nin üstündeki blok gizlensin
            dialogueText.text = ""; // Yazýyý temizle
        }
    }

    IEnumerator DisplayDialogue()
    {
        dialogueText.text = ""; // Önceden yazý varsa sýfýrla
        foreach (char letter in npcDialogue.ToCharArray())
        {
            dialogueText.text += letter; // Her harfi yavaþça ekle
            yield return new WaitForSeconds(0.05f); // Her harften sonra kýsa bir bekleme
        }
    }
}
