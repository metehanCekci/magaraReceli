using UnityEngine;
using UnityEngine.SceneManagement;

public class LastBossDeathHandler : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LastHandler()
    {
        Destroy(this.transform.parent.gameObject);
        LoadNextScene();
    }

    public void LoadNextScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        // Loop back to first scene if last scene is reached
        if (nextSceneIndex >= SceneManager.sceneCountInBuildSettings)
            nextSceneIndex = 0;

        FadeInOutManager.Instance.FadeOutAndLoadScene(nextSceneIndex);
    }
}
