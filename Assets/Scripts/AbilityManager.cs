using UnityEngine;
using System.Collections.Generic;

public enum AbilityType { DoubleJump, Dash, Heal }

public class AbilityManager : MonoBehaviour
{
    private HashSet<AbilityType> unlocked = new HashSet<AbilityType>();

    public void Unlock(AbilityType ability)
    {
        if (!unlocked.Contains(ability))
            unlocked.Add(ability);
    }

    public bool CanDash() => unlocked.Contains(AbilityType.Dash);
    public bool CanHeal() => unlocked.Contains(AbilityType.Heal);

    public int GetMaxJumps()
    {
        return unlocked.Contains(AbilityType.DoubleJump) ? 2 : 1;
    }

    public HashSet<AbilityType> GetUnlockedAbilities() => unlocked;
    public void SetUnlockedAbilities(HashSet<AbilityType> abilities) => unlocked = abilities;
}
