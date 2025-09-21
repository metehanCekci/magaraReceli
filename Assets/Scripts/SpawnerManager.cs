using UnityEngine;
using System.Collections.Generic;

public class SpawnerManager : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject[] spawnPrefabs;   // 4 prefab buraya atanacak
    public int totalSpawns = 10;        // Kaç tane spawn etsin?
    private int spawnedCount = 0;       // Şu ana kadar kaç spawn etti

    public DoorScript door;

    private List<GameObject> spawnedObjects = new List<GameObject>();

    void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    private System.Collections.IEnumerator SpawnRoutine()
    {
        while (spawnedCount < totalSpawns)
        {
            // Rastgele prefab seç
            int index = Random.Range(0, spawnPrefabs.Length);

            // Prefab'ın kopyasını oluştur
            GameObject obj = Instantiate(spawnPrefabs[index], spawnPrefabs[index].transform.position, Quaternion.identity);
            obj.SetActive(true);
            // Listeye ekle
            spawnedObjects.Add(obj);

            spawnedCount++;

            yield return new WaitForSeconds(2f); // spawn arası bekleme (istersen ayarlanabilir)
        }

        // Tüm spawnlar yapıldıktan sonra sahnedekilerin bitmesini bekle
        yield return new WaitUntil(() => spawnedObjects.TrueForAll(o => o == null));

        door.enabled = true; 
    }
}
