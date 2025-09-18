using System;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class SaveSystem : MonoBehaviour
{
    private string savePath => Application.persistentDataPath + "/player_save.json";

    public void Save(GameObject player)
    {
        var abilitiesMgr = player.GetComponent<AbilityManager>();

        PlayerSaveData data = new PlayerSaveData();
        data.health = player.GetComponent<HealthSystem>().currentHealth;

        // maxHealth kontrolü ekleyelim (0 olursa 5'e atayalım)
        data.maxHealth = player.GetComponent<HealthSystem>().maxHealth != 0 ? player.GetComponent<HealthSystem>().maxHealth : 5;

        // Yetenekleri kaydetme
        data.abilities = new List<string>();
        foreach (var ability in abilitiesMgr.GetUnlockedAbilities())
        {
            data.abilities.Add(ability.ToString());  // AbilityType'ı string'e çevirip kaydediyoruz
        }

        // Dash durumu kaydediliyor
        data.dashUnlocked = abilitiesMgr.IsDashUnlocked();

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);

        Debug.Log("Player saved with health: " + data.health + " and dashUnlocked: " + data.dashUnlocked);
        Debug.Log("Saved Health: " + data.health + " MaxHealth: " + data.maxHealth);
    }

    public void Load(GameObject player)
    {
        if (!File.Exists(savePath)) return;

        string json = File.ReadAllText(savePath);
        PlayerSaveData data = JsonUtility.FromJson<PlayerSaveData>(json);

        var health = player.GetComponent<HealthSystem>();  // HealthSystem component'ini alıyoruz
        if (health != null)
        {
            Debug.Log("Loaded health: " + data.health + " maxHealth: " + data.maxHealth); // Yüklenen değerleri kontrol ediyoruz

            health.maxHealth = data.maxHealth;  // maxHealth'i yükle
            health.currentHealth = Mathf.Clamp(data.health, 0, health.maxHealth);  // currentHealth'i yükle
        }
        else
        {
            Debug.LogError("HealthSystem component not found on the player!");
        }
        var abilitiesMgr = player.GetComponent<AbilityManager>();
        if (abilitiesMgr != null && data.abilities != null)
        {
            var set = new HashSet<AbilityTypeList>();
            foreach (var s in data.abilities)
            {
                if (System.Enum.TryParse(s, out AbilityTypeList parsed))
                {
                    set.Add(parsed);  // String'i AbilityType'a çevirip set'e ekliyoruz
                }
            }
            abilitiesMgr.SetUnlockedAbilities(set);  // Yetenekleri set'e ekliyoruz
        }
        if (abilitiesMgr != null)
        {
            abilitiesMgr.SetDashUnlocked(data.dashUnlocked);
        }

        Debug.Log("Player load test completed.");
    }

    [System.Serializable]
    public class PlayerSaveData
    {
        public int health;// Can bilgisi

        public int maxHealth;
        public List<string> abilities;     // Yetenekler
        public bool dashUnlocked;          // Dash yeteneğinin alınıp alınmadığı bilgisi
    }
}
