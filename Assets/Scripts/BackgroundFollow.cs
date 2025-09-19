using UnityEngine;

public class BackgroundFollow : MonoBehaviour
{
    public Transform target; // Takip edilecek obje (Player)
    public float smoothTime = 0.3f;
    private Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        if (target == null) return;

        // Sadece X ve Y ekseninde takip (arka planın Z'si sabit kalsın)
        Vector3 targetPosition = new Vector3(target.position.x, target.position.y, transform.position.z);

        // Smooth takip
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }
}
