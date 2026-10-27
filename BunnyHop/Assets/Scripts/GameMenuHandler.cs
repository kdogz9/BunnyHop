using UnityEngine;

public class GameMenuHandler : MonoBehaviour
{
    private bool isPaused = false;

    private void Update()
    {
        // Check for Escape key press
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleEscapePress();
        }
    }

    private void HandleEscapePress()
    {
#if UNITY_WEBGL
        // In WebGL (itch.io browser player), Esc natively exits fullscreen.
        // You can toggle time scale pause or show an in-game message.
        TogglePause();
#else
        // In downloadable Desktop builds (Windows/Mac), Esc quits the app.
        Application.Quit();
        Debug.Log("Game Quit!");
#endif
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0f : 1f; // Freeze/Unfreeze game time
    }
}