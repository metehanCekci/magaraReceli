using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class SaveSystem : MonoBehaviour
{
    private string savePath => Application.persistentDataPath + "/player_save.json";

    public void Save(GameObject player)
    {
        var health = player.GetComponent<HealthSystem>();
        var abilitiesMgr = player.GetComponent<AbilityManager>();

        PlayerSaveData data = new PlayerSaveData();
        data.health = health != null ? health.currentHealth : 0;

        // HashSet<AbilityType> -> List<string> (LINQ'suz)
        data.abilities = new List<string>();
        if (abilitiesMgr != null)
        {
            var set = abilitiesMgr.GetUnlockedAbilities();
            if (set != null)
            {
                foreach (var a in set)
                    data.abilities.Add(a.ToString());
            }
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
#if UNITY_EDITOR
        Debug.Log("Saved: " + savePath);
#endif
    }

    public void Load(GameObject player)
    {
        if (!File.Exists(savePath)) return;

        string json = File.ReadAllText(savePath);
        PlayerSaveData data = JsonUtility.FromJson<PlayerSaveData>(json);

        var health = player.GetComponent<HealthSystem>();
        if (health != null)
            health.currentHealth = Mathf.Clamp(data.health, 0, health.maxHealth);

        // List<string> -> HashSet<AbilityType>
        var abMgr = player.GetComponent<AbilityManager>();
        if (abMgr != null && data.abilities != null)
        {
            var set = new HashSet<AbilityType>();
            foreach (var s in data.abilities)
            {
                if (System.Enum.TryParse(s, out AbilityType parsed))
                    set.Add(parsed);
            }
            abMgr.SetUnlockedAbilities(set);
        }
#if UNITY_EDITOR
        Debug.Log("Loaded: " + savePath);
#endif
    }
}

[System.Serializable]
public class PlayerSaveData
{
    public int health;
    public List<string> abilities;
}
