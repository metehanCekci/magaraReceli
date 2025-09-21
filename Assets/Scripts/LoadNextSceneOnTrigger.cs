using UnityEngine;
using UnityEngine.SceneManagement;

public class NextSceneLoaderOnTrigger : MonoBehaviour
{
    // Load the next scene in build settings
    public void LoadNextScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        // Loop back to first scene if last scene is reached
        if (nextSceneIndex >= SceneManager.sceneCountInBuildSettings)
            nextSceneIndex = 0;

        FadeInOutManager.Instance.FadeOutAndLoadScene(nextSceneIndex);
    }

    // Exit the game
    public void ExitGame()
    {
#if UNITY_EDITOR
        Debug.Log("Oyun durduruldu (Editor)");
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

        void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.CompareTag("Player"))
        LoadNextScene();
    }
}
