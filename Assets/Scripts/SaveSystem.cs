using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class SaveSystem : MonoBehaviour
{
    public HealthBarScript healthBarScript;
    private string savePath => Application.persistentDataPath + "/player_save.json";

    void Awake()
    {
        // Player'daki HealthSystem component'ını almak
        healthBarScript = GameObject.FindGameObjectWithTag("Player").GetComponent<HealthBarScript>();
        if (healthBarScript == null)
        {
            Debug.LogError("HealthSystem component not found on the player!");
        }
    }

    // Save fonksiyonu - Oyuncu verilerini kaydeder
    public void Save(GameObject player)
    {
        var abilitiesMgr = player.GetComponent<AbilityManager>();
        var healthSystem = player.GetComponent<HealthSystem>();

        PlayerSaveData data = new PlayerSaveData();

        // Sağlık verilerini kaydet
        data.health = healthSystem.currentHealth;
        data.maxHealth = healthSystem.maxHealth;

        // Yetenekleri kaydetme
        data.abilities = new List<string>();
        foreach (var ability in abilitiesMgr.GetUnlockedAbilities())
        {
            data.abilities.Add(ability.ToString());
        }

        // Dash yeteneği kaydediliyor
        data.dashUnlocked = abilitiesMgr.IsDashUnlocked(); // Dash durumu kaydedildi

        // Veriyi JSON formatında kaydet
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);

        Debug.Log("Player saved with health: " + data.health + " and dashUnlocked: " + data.dashUnlocked);
    }

    // Load fonksiyonu - Oyuncu verilerini yükler
    public void Load(GameObject player)
    {
        if (!File.Exists(savePath)) return;

        string json = File.ReadAllText(savePath);
        PlayerSaveData data = JsonUtility.FromJson<PlayerSaveData>(json);

        var healthSystem = player.GetComponent<HealthSystem>();
        if (healthSystem != null)
        {
            Debug.Log("Loaded health: " + data.health + " maxHealth: " + data.maxHealth);

            // Sağlık ve max sağlık değerlerini yükle
            healthSystem.maxHealth = data.maxHealth;
            healthSystem.currentHealth = Mathf.Clamp(data.health, 0, healthSystem.maxHealth);
        }

        // Dash yeteneğini yükle
        var abilitiesMgr = player.GetComponent<AbilityManager>();
        if (abilitiesMgr != null)
        {
            // Yetenekleri yükle
            var set = new HashSet<AbilityTypeList>();
            foreach (var s in data.abilities)
            {
                if (System.Enum.TryParse(s, out AbilityTypeList parsed))
                {
                    set.Add(parsed);
                }
            }
            abilitiesMgr.SetUnlockedAbilities(set);

            // Dash durumu yükle
            abilitiesMgr.SetDashUnlocked(data.dashUnlocked);
        }

        Debug.Log("Player load test completed.");
    }

    [System.Serializable]
    public class PlayerSaveData
    {
        public int health;
        public int maxHealth;
        public List<string> abilities;
        public bool dashUnlocked; // Dash durumu kaydedildi
    }
}
