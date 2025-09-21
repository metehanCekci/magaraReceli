using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public int sceneIndex = 1;

    /// <summary>
    /// Ana menüdeki "Oyna" butonu için. Kayıt varsa yükler, yoksa yeni oyun başlatır.
    /// </summary>
    public void PlayGame()
    {
        Time.timeScale = 1f; // Pause menüsünden gelme ihtimaline karşı zamanı normale döndür.
        
        Debug.Log("Yeni oyun başlatılıyor...");
        // Yeni oyun için belirlenen sahneyi yükle.
        // Kaydetme sistemi kaldırıldığı için her zaman yeni oyun başlatılır.
        if (FadeInOutManager.Instance != null)
        {
            FadeInOutManager.Instance.FadeOutAndLoadScene(sceneIndex);
        }
        else
        {
            // FadeInOutManager bulunamazsa, doğrudan sahneyi yükle.
            Debug.LogWarning("FadeInOutManager bulunamadı! Sahne doğrudan yükleniyor.");
            SceneManager.LoadScene(sceneIndex);
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
