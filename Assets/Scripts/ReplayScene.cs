using UnityEngine;
using UnityEngine.SceneManagement;

public class ReplayScene : MonoBehaviour
{
    // Aynı sahneyi tekrar yükleme
    public void ReloadCurrentScene()
    {
        
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        if (FadeInOutManager.Instance != null)
        {
            FadeInOutManager.Instance.FadeOutAndLoadScene(currentSceneIndex);
        }
        else
        {
            DeathMenu.Instance.gameObject.SetActive(false);
            SceneManager.LoadScene(currentSceneIndex);
        }
    }

    // Oyunu kapatmak için
    public void ExitGame()
    {
#if UNITY_EDITOR
        // Editor içinde çalışırken
        Debug.Log("Oyun durduruldu (Editor)");
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Build edilmiş oyunda
        Application.Quit();
#endif
    }
}
