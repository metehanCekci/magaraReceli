// metehancekci/magarareceli/magaraReceli-c354ca461671bdc0711870d4b7d693c7cf44512b/Assets/Scripts/DashGet.cs

using UnityEngine;

public class DashGet : MonoBehaviour
{
    private AbilityManager abilityManager;

    // YENÝ: Sahne baþladýðýnda bu fonksiyon çalýþýr
    void Start()
    {
        // Oyuncuyu bul ve AbilityManager'ýna eriþ
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            abilityManager = player.GetComponent<AbilityManager>();

            // KONTROL: Eðer oyuncunun AbilityManager'ý varsa VE Dash zaten açýksa...
            if (abilityManager != null && abilityManager.IsDashUnlocked())
            {
                // Dash zaten alýnmýþ olduðu için bu objeyi sahnede göstermeye gerek yok.
                gameObject.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (abilityManager == null)
            {
                abilityManager = other.GetComponent<AbilityManager>();
            }

            // Dash'in kilidini aç
            if (abilityManager != null)
            {
                abilityManager.UnlockDash();
                Debug.Log("Dash yeteneði kazanýldý!");
            }

            // Oyuncu Dash'i aldýðý için bu objeyi yok et
            Destroy(gameObject);
        }
    }
}