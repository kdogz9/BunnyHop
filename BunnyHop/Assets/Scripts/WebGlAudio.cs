using UnityEngine;

public class WebGLAudio : MonoBehaviour
{
    private bool audioUnlocked = false;

    private void Update()
    {
        // Detect initial click or keypress to wake up browser WebAudio context
        if (!audioUnlocked && (Input.anyKeyDown || Input.GetMouseButtonDown(0)))
        {
            UnlockAudio();
        }
    }

    private void UnlockAudio()
    {
        AudioListener.pause = false;

        // Force audio settings refresh to trigger browser audio context
        AudioConfiguration config = AudioSettings.GetConfiguration();
        AudioSettings.Reset(config);

        audioUnlocked = true;
        Debug.Log("WebGL Audio Context Unlocked!");
    }
}