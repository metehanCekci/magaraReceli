using UnityEngine;
using UnityEngine.InputSystem;  // InputSystem namespace'ini ekliyoruz

public class SaveTest : MonoBehaviour
{
    public GameObject Player;  // Oyuncu GameObject
    private SaveSystem saveSystem;

    private InputAction saveAction;
    private InputAction loadAction;

    private InputSystem_Actions inputActions;  // SaveLoadInputActions asset'ini buraya ekliyoruz

    void Awake()
    {
        saveSystem = GetComponent<SaveSystem>();

        if (Player == null)
        {
            Player = GameObject.Find("Player");  // Player objesini sahnede bul
            if (Player == null)
            {
                Debug.LogError("Player GameObject not found!");
            }
        }
        if (saveSystem == null)
        {
            Debug.LogError("SaveSystem component is missing on the GameObject!");
        }
        else
        {
            // InputActions asset'ini başlatıyoruz
            inputActions = new InputSystem_Actions();  // SaveLoadInputActions sınıfı otomatik oluşturulacak

            // InputAction'lara abone oluyoruz
            saveAction = inputActions.SaveLoad.Save;  // Save action'ını alıyoruz
            loadAction = inputActions.SaveLoad.Load;  // Load action'ını alıyoruz

            saveAction.Enable();  // Save action'ını enable ediyoruz
            loadAction.Enable();  // Load action'ını enable ediyoruz
        }

    }


    void OnEnable()
    {
        // Save ve Load action'larına basıldığında ne olacağını belirliyoruz
        saveAction.performed += context => SaveGame();
        loadAction.performed += context => LoadGame();
    }

    void OnDisable()
    {
        // Action'ları disable ediyoruz
        saveAction.performed -= context => SaveGame();
        loadAction.performed -= context => LoadGame();
    }

    // Save işlemi
    void SaveGame()
    {
        if (Player != null)
        {
            saveSystem.Save(Player);  // Save fonksiyonunu çağırıyoruz
            Debug.Log("Player save test completed.");
        }
        Debug.Log("Player save test completed.");
    }

    // Load işlemi
    void LoadGame()
    {
        if (Player != null)
        {
            saveSystem.Load(Player);  // Load fonksiyonunu çağırıyoruz
            Debug.Log("Player load test completed.");
        }
        Debug.Log("Player load test completed.");
    }
}
