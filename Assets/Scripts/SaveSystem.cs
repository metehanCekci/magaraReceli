using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class SaveSystem : MonoBehaviour
{
    public HealthBarScript healthBarScript;
    private string savePath => Application.persistentDataPath + "/player_save.json";

    void Awake()
    {

        // player'daki HealthSystem component'ını almak
        healthBarScript = GameObject.FindGameObjectWithTag("Player").GetComponent<HealthBarScript>();
        if (healthBarScript == null)
        {
            Debug.LogError("HealthSystem component not found on the player!");
        }
    }
    public void Save(GameObject player)
    {
        var abilitiesMgr = player.GetComponent<AbilityManager>();
        var healthSystem = player.GetComponent<HealthSystem>();  // HealthSystem component'ını buraya ekliyoruz

        PlayerSaveData data = new PlayerSaveData();
        data.health = healthSystem.currentHealth;  // Sağlık bilgisi
        data.maxHealth = healthSystem.maxHealth;  // MaxHealth

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

        var healthSystem = player.GetComponent<HealthSystem>();  // HealthSystem component'ını alıyoruz
        if (healthSystem != null)
        {
            Debug.Log("Loaded health: " + data.health + " maxHealth: " + data.maxHealth); // Yüklenen değerleri kontrol ediyoruz

            healthSystem.maxHealth = data.maxHealth;  // maxHealth'i yükle
            healthSystem.currentHealth = Mathf.Clamp(data.health, 0, healthSystem.maxHealth);  // currentHealth'i yükle

            // UI kalp barını güncelle
            if (healthBarScript != null)
            {
                healthBarScript.UpdateHealth(healthSystem.currentHealth, healthSystem.maxHealth);
            }
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
        public int health;        // Can bilgisi
        public int maxHealth;     // MaxHealth
        public List<string> abilities;  // Yetenekler
        public bool dashUnlocked;     // Dash yeteneğinin alınıp alınmadığı bilgisi
    }
}
