using System.Collections.Generic;
using UnityEngine;

public class AbilityManager : MonoBehaviour
{
    private HashSet<AbilityTypeList> unlockedAbilities = new HashSet<AbilityTypeList>();
    private bool dashUnlocked = false;

    // Dash'ın kilidini açma
    public void UnlockDash()
    {
        dashUnlocked = true;
        unlockedAbilities.Add(AbilityTypeList.Dash);
    }

    // Dash'ın kilidini açıp açmadığını kontrol et
    public bool IsDashUnlocked() => dashUnlocked;

    // Dash durumu ayarla (Load sırasında)
    public void SetDashUnlocked(bool value) => dashUnlocked = value;

    // Yeteneklerin kilidini açma
    public void UnlockAbility(AbilityTypeList ability)
    {
        unlockedAbilities.Add(ability);
    }

    // Yeteneklerin tümünü al
    public HashSet<AbilityTypeList> GetUnlockedAbilities() => unlockedAbilities;

    // Yeteneklerin kilidini ayarlama
    public void SetUnlockedAbilities(HashSet<AbilityTypeList> abilities)
    {
        unlockedAbilities = abilities;
    }

    // MaxJumps gibi bir metodu ekleyelim (varsayalım ki bu max zıplama sayısını döndüren bir metod)
    public int GetMaxJumps()
    {
        return unlockedAbilities.Contains(AbilityTypeList.DoubleJump) ? 2 : 1; // Eğer DoubleJump yeteneği varsa 2, yoksa 1 zıplama
    }

    // Dash yeteneği kontrolü
    public bool CanDash()
    {
        return dashUnlocked;
    }

    // Heal yeteneği kontrolü
    public bool CanHeal()
    {
        return unlockedAbilities.Contains(AbilityTypeList.Heal);
    }
}
