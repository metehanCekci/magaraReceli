using UnityEngine;
using UnityEngine.UI;

public class SoulSystem : MonoBehaviour
{
    public Image soulImage;  // Soul bar'� temsil eden Image UI eleman�

    // Fill amount'� do�rudan g�ncelleyen fonksiyon
    public void UpdateSoulBar(float soulPercentage)
    {
        soulImage.fillAmount = Mathf.Clamp(soulPercentage, 0f, 1f);  // FillAmount 0 ile 1 aras�nda olacak �ekilde g�ncelleniyor
    }
}
