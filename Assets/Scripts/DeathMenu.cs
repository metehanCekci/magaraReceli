using UnityEngine;

public class DeathMenu : MonoBehaviour
{
    private static DeathMenu instance;

    void Awake()
    {
        // Eğer sahnede zaten bir DeathMenu varsa yenisini yok et
        if (instance != null && instance != this)
        {
            DeathMenu.Instance.gameObject.SetActive(false);
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        DeathMenu.Instance.gameObject.SetActive(false);
    }

    

    // Public şekilde erişmek için property
    public static DeathMenu Instance => instance;
}
