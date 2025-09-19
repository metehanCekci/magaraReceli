using UnityEngine;
using UnityEngine.UI;

public class PlayerFinder : MonoBehaviour
{
    void Start()
    {
        // Try to find the Player object by tag
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player not found in the scene! Make sure the Player has the 'Player' tag.");
            return;
        }

        // Get the PlayerController component
        PlayerController controller = player.GetComponent<PlayerController>();
        if (controller == null)
        {
            Debug.LogError("PlayerController component not found on Player!");
            return;
        }

        // Get the Button component attached to this GameObject
        Button btn = GetComponent<Button>();
        if (btn == null)
        {
            Debug.LogError("No Button component found on this GameObject!");
            return;
        }

        // Remove existing listeners to avoid duplicates
        btn.onClick.RemoveAllListeners();

        // Add ResumeGame function to the Button onClick
        btn.onClick.AddListener(controller.ResumeGame);
    }
}
