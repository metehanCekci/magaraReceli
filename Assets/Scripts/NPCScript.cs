using System.Collections;
using TMPro;
using UnityEngine;

public class NPCScript : MonoBehaviour
{
    public TextMeshProUGUI dialogueText;      // UI'deki yaz� eleman�
    public GameObject dialogueBlock;          // Siyah blok
    public string npcDialogue;                // NPC'nin konu�aca�� yaz�
    private bool isNearNPC = false;

    void Start()
    {
        // Ba�lang��ta yaz�y� gizle
        dialogueText.text = "";
        dialogueBlock.SetActive(false); // Blok da gizli olacak
    }

    void Update()
    {
        // E�er NPC'ye yakla��ld�ysa, metni yazd�rmaya ba�la
        if (isNearNPC && dialogueText.text == "")
        {
            StartCoroutine(DisplayDialogue());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Oyuncu objesinin tag'�n� kontrol et
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player NPC'ye yakla�t�");
            isNearNPC = true;
            dialogueBlock.SetActive(true); // NPC'nin �st�ndeki blok g�r�ns�n
            dialogueText.text = ""; // �nceki metni s�f�rla
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Oyuncu objesinin tag'�n� kontrol et
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player NPC'den ayr�ld�");
            isNearNPC = false;
            dialogueBlock.SetActive(false); // NPC'nin �st�ndeki blok gizlensin
            dialogueText.text = ""; // Yaz�y� temizle
        }
    }

    IEnumerator DisplayDialogue()
    {
        dialogueText.text = ""; // �nceden yaz� varsa s�f�rla
        foreach (char letter in npcDialogue.ToCharArray())
        {
            dialogueText.text += letter; // Her harfi yava��a ekle
            yield return new WaitForSeconds(0.05f); // Her harften sonra k�sa bir bekleme
        }
    }
}
