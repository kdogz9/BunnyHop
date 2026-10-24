using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class BunnyGameManager : MonoBehaviour
{
    [Header("Game Rules")]
    [SerializeField] private int maxCarrots = 3;
    [SerializeField] private float hitTimingWindow = 0.2f;

    [Header("UI & Visual References")]
    [SerializeField] private TextMeshProUGUI carrotHUDText; // Optional UI Text (e.g. "Carrots: 3/3")
    [SerializeField] private Transform bunnyTransform;
    [SerializeField] private GameObject carrotPrefab;
    [SerializeField] private Transform carrotSpawnPoint;

    private int currentCarrots;
    private float lastBeatTime;
    private bool beatHitThisCycle = false;
    private GameObject activeCarrot;

    private void OnEnable() => ProceduralAudioGenerator.OnBeatTick += HandleBeatTick;
    private void OnDisable() => ProceduralAudioGenerator.OnBeatTick -= HandleBeatTick;

    private void Start()
    {
        currentCarrots = maxCarrots;
        UpdateUI();
        SpawnCarrot();
    }

    private void HandleBeatTick()
    {
        // Check if player missed previous beat
        if (!beatHitThisCycle && Time.timeSinceLevelLoad > 1f)
        {
            LoseCarrot();
        }

        lastBeatTime = Time.time;
        beatHitThisCycle = false;

        if (activeCarrot == null && currentCarrots > 0)
        {
            SpawnCarrot();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            RegisterPlayerHop();
        }
    }

    private void RegisterPlayerHop()
    {
        float timeSinceBeat = Time.time - lastBeatTime;

        if (timeSinceBeat <= hitTimingWindow && !beatHitThisCycle)
        {
            SuccessfulHop();
        }
        else
        {
            LoseCarrot();
        }
    }

    private void SuccessfulHop()
    {
        beatHitThisCycle = true;

        if (bunnyTransform != null)
        {
            bunnyTransform.position += Vector3.up * 0.4f;
            Invoke(nameof(ResetBunnyPos), 0.08f);
        }

        if (activeCarrot != null)
        {
            Destroy(activeCarrot);
        }
    }

    private void ResetBunnyPos()
    {
        if (bunnyTransform != null)
            bunnyTransform.position = Vector3.zero;
    }

    private void LoseCarrot()
    {
        currentCarrots--;
        UpdateUI();

        if (currentCarrots <= 0)
        {
            Invoke(nameof(RestartGame), 0.3f);
        }
    }

    private void SpawnCarrot()
    {
        if (carrotPrefab != null && carrotSpawnPoint != null)
        {
            if (activeCarrot != null) Destroy(activeCarrot);
            activeCarrot = Instantiate(carrotPrefab, carrotSpawnPoint.position, Quaternion.identity);
        }
    }

    private void UpdateUI()
    {
        if (carrotHUDText != null)
        {
            carrotHUDText.text = $"Hunger (Carrots): {currentCarrots} / {maxCarrots}";
        }
    }

    private void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}