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
    public void Save(GameObject player, Vector3 savePointPosition)
    {
        var abilitiesMgr = player.GetComponent<AbilityManager>();
        var healthSystem = player.GetComponent<HealthSystem>();

        PlayerSaveData data = new PlayerSaveData();
        data.health = healthSystem.currentHealth;
        data.maxHealth = healthSystem.maxHealth;

        // Bonfire pozisyonunu kaydet
        data.position = new float[3];
        data.position[0] = savePointPosition.x;
        data.position[1] = savePointPosition.y;
        data.position[2] = savePointPosition.z;

        // Yetenekleri kaydetme
        data.abilities = new List<string>();
        foreach (var ability in abilitiesMgr.GetUnlockedAbilities())
        {
            data.abilities.Add(ability.ToString());
        }

        data.dashUnlocked = abilitiesMgr.IsDashUnlocked();

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);

        Debug.Log("Player saved with health: " + data.health + " and dashUnlocked: " + data.dashUnlocked);
        Debug.Log("Saved Health: " + data.health + " MaxHealth: " + data.maxHealth);
        Debug.Log($"Saved Bonfire Position: {data.position[0]}, {data.position[1]}, {data.position[2]}");
    }

    public void Load(GameObject player)
    {
        if (!File.Exists(savePath)) return;

        string json = File.ReadAllText(savePath);
        PlayerSaveData data = JsonUtility.FromJson<PlayerSaveData>(json);

        var healthSystem = player.GetComponent<HealthSystem>();
        if (healthSystem != null)
        {
            Debug.Log("Loaded health: " + data.health + " maxHealth: " + data.maxHealth);

            healthSystem.maxHealth = data.maxHealth;
            healthSystem.currentHealth = Mathf.Clamp(data.health, 0, healthSystem.maxHealth);

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

        // Pozisyonu yükle
        if (data.position != null && data.position.Length == 3)
        {
            player.transform.position = new Vector3(data.position[0], data.position[1], data.position[2]);
            Debug.Log($"Loaded Position: {data.position[0]}, {data.position[1]}, {data.position[2]}");
        }

        var abilitiesMgr = player.GetComponent<AbilityManager>();
        if (abilitiesMgr != null && data.abilities != null)
        {
            var set = new HashSet<AbilityTypeList>();
            foreach (var s in data.abilities)
            {
                if (System.Enum.TryParse(s, out AbilityTypeList parsed))
                {
                    set.Add(parsed);
                }
            }
            abilitiesMgr.SetUnlockedAbilities(set);
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
        public int health;
        public int maxHealth;
        public float[] position; // [x, y, z]
        public List<string> abilities;
        public bool dashUnlocked;
    }
}
