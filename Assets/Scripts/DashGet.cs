using UnityEngine;

public class DashGet : MonoBehaviour
{
    private void Awake()
    {

    }
    private void FixedUpdate()
    {
        // Oyuncunun AbilityManager'ýný al
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            AbilityManager abilityManager = player.GetComponent<AbilityManager>();

            // Eðer Dash yeteneði zaten alýndýysa, objeyi yok et
            if (abilityManager != null && abilityManager.IsDashUnlocked())
            {
                Destroy(gameObject);  // Dash alýndýysa objeyi yok et
            }
            else
            {
                Debug.Log("Dash yeteneði alýnmadý, obje sahnede kalacak.");
            }
        }
        else
        {
            Debug.LogError("Player GameObject bulunamadý!");
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Oyuncu ile temas etti mi?
        if (other.CompareTag("Player"))
        {
            // Oyuncunun AbilityManager'ýný bul
            AbilityManager abilityManager = other.GetComponent<AbilityManager>();

            if (abilityManager != null && !abilityManager.IsDashUnlocked())
            {
                abilityManager.UnlockDash(); // Dash kilidini aç
                Debug.Log("Dash yeteneði açýldý!");

                // Pickup objesini yok et
                Destroy(gameObject);
            }
            else
            {
                Debug.Log("Dash zaten alýndý, obje yok edilmedi.");
            }
        }
    }
}
