using UnityEngine;

public class SavePoint : MonoBehaviour
{
    public SaveSystem saveSystem;

    void Awake()
    {
        if (saveSystem == null)
            saveSystem = FindObjectOfType<SaveSystem>();
    }

    private float saveCooldown = 2f; // 2 saniye arayla kayıt yapılabilir
    private float lastSaveTime = -10f;

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (Time.time > lastSaveTime + saveCooldown)
            {
                saveSystem.Save(other.gameObject, transform.position);
                lastSaveTime = Time.time;
                Debug.Log("Game saved at save point! (Bonfire position: " + transform.position + ")");
            }
        }
    }
}
