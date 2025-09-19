using UnityEngine;
using UnityEngine.UI;

public class SoulSystem : MonoBehaviour
{
    public Image soulImage;  // Soul bar'ý temsil eden Image UI elemaný

    // Fill amount'ý doðrudan güncelleyen fonksiyon
    public void UpdateSoulBar(float soulPercentage)
    {
        soulImage.fillAmount = Mathf.Clamp(soulPercentage, 0f, 1f);  // FillAmount 0 ile 1 arasýnda olacak þekilde güncelleniyor
    }
}
