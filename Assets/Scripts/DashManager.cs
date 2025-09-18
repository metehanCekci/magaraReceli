using UnityEngine;

public class DashManager : MonoBehaviour
{
    private bool dashUnlocked = false;

    // Dash'ın alınıp alınmadığını kontrol et
    public bool IsDashUnlocked() 
    {
        return dashUnlocked;
    }

    // Dash'ı alındığını işaretle
    public void UnlockDash()
    {
        dashUnlocked = true;
    }

    // Dash'ı yükle
    public void SetDashUnlocked(bool isUnlocked)
    {
        dashUnlocked = isUnlocked;
    }

    // Dash'ı kullanıp kullanamayacağını kontrol et
    public bool CanDash()
    {
        return dashUnlocked;
    }
}
