using UnityEngine;

public class DashGet : MonoBehaviour
{
    private void Awake()
    {

    }
    private void FixedUpdate()
    {
        // Oyuncunun AbilityManager'�n� al
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            AbilityManager abilityManager = player.GetComponent<AbilityManager>();

            // E�er Dash yetene�i zaten al�nd�ysa, objeyi yok et
            if (abilityManager != null && abilityManager.IsDashUnlocked())
            {
                Destroy(gameObject);  // Dash al�nd�ysa objeyi yok et
            }
            else
            {
                Debug.Log("Dash yetene�i al�nmad�, obje sahnede kalacak.");
            }
        }
        else
        {
            Debug.LogError("Player GameObject bulunamad�!");
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Oyuncu ile temas etti mi?
        if (other.CompareTag("Player"))
        {
            // Oyuncunun AbilityManager'�n� bul
            AbilityManager abilityManager = other.GetComponent<AbilityManager>();

            if (abilityManager != null && !abilityManager.IsDashUnlocked())
            {
                abilityManager.UnlockDash(); // Dash kilidini a�
                Debug.Log("Dash yetene�i a��ld�!");

                // Pickup objesini yok et
                Destroy(gameObject);
            }
            else
            {
                Debug.Log("Dash zaten al�nd�, obje yok edilmedi.");
            }
        }
    }
}
