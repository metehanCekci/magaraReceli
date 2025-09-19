using UnityEngine;
using UnityEngine.UI;

public class HealthBarScript : MonoBehaviour
{
    public Image[] hearts;  // UI'daki kalp görselleri
    public Sprite fullHeart; // Dolu kalp görseli
    public Sprite emptyHeart; // Boş kalp görseli

    public void UpdateHealth(int currentHealth, int maxHealth)
    {
        // 5 adet kalp varsa, her kalp 20 canı temsil eder
        int heartsToFill = Mathf.FloorToInt((float)currentHealth / maxHealth * hearts.Length);  // Doldurulacak kalp sayısı

        // Kalpleri güncelle
        for (int i = 0; i < hearts.Length; i++)
        {
            if (i < heartsToFill)
                hearts[i].sprite = fullHeart; // Dolu kalp
            else
                hearts[i].sprite = emptyHeart; // Boş kalp
        }
    }

}
