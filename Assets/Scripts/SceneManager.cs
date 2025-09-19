using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public int sceneIndex = 1;

    // Dışarıdan sahne indexi ile yükleme
    public void LoadSceneByIndex()
    {

        FadeInOutManager.Instance.FadeOutAndLoadScene(sceneIndex);
        
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
