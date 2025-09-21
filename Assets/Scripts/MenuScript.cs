using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuScript : MonoBehaviour
{
    /// <summary>
    /// Oyunu devam ettirir. Pause menüsünü kapatır ve zamanı tekrar akıtır.
    /// Bu fonksiyonu UI'daki "Resume" butonuna bağla.
    /// </summary>
    public void Resume()
    {
        // Sahnedeki PlayerController'ı buluyoruz.
        PlayerController playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
        {
            // PlayerController'daki ResumeGame fonksiyonunu çağırıyoruz.
            // Bu fonksiyon zaten zamanı başlatma ve menüyü kapatma işini yapıyor.
            playerController.ResumeGame();
        }
        else
        {
            Debug.LogError("PlayerController sahnede bulunamadı!");
        }
    }

    /// <summary>
    /// Oyunu mevcut sahneyi yeniden yükleyerek yeniden başlatır.
    /// Bu fonksiyonu UI'daki "Restart" butonuna bağla.
    /// </summary>
    public void Restart()
    {
        // Yeni sahne yüklenmeden önce zamanın aktığından emin olalım.
        Time.timeScale = 1f;

        // Mevcut sahnenin build index'ini alıp yeniden yüklüyoruz.
        // FadeInOutManager varsa daha yumuşak bir geçiş sağlar.
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        FadeInOutManager.Instance.FadeOutAndLoadScene(currentSceneIndex);
    }

    /// <summary>
    /// Ana menüye döner.
    /// Bu fonksiyonu UI'daki "Exit" veya "Main Menu" butonuna bağla.
    /// </summary>
    public void ExitToMainMenu()
    {
        // Yeni sahne yüklenmeden önce zamanın aktığından emin olalım.
        Time.timeScale = 1f;

        // Ana menü sahnesini yüklüyoruz. Genellikle build index'i 0 olur.
        // Eğer ana menünüz farklı bir index'te ise bu sayıyı değiştirin.
        FadeInOutManager.Instance.FadeOutAndLoadScene(0);
    }
}
