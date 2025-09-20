using UnityEngine;

public class SFXPlayer : MonoBehaviour
{
    [Header("Clips")]
    public AudioClip gore;
    public AudioClip hurt;
    public AudioClip whoosh;
    public AudioClip lazerfire;
    public AudioClip gunfire;
    public AudioClip kill;
    public AudioClip slam;

    public static SFXPlayer Instance;

    private AudioSource audioSource;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Ensure AudioSource exists
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void PlayWhoosh()
    {
        audioSource.PlayOneShot(whoosh);
    }

    public void PlayGore()
    {
        audioSource.PlayOneShot(gore);
    }

    public void PlayHurt()
    {
        audioSource.PlayOneShot(hurt);
    }

    public void PlayKill()
    {
        audioSource.PlayOneShot(kill);
    }

    public void PlayLazerFire()
    {
        audioSource.PlayOneShot(lazerfire);
    }

    public void PlayGunFire()
    {
        audioSource.PlayOneShot(gunfire);
    }

    public void PlaySlam()
    {
        audioSource.PlayOneShot(slam);
    }
}
