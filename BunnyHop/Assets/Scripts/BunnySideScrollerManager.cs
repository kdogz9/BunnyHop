using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BunnySideScrollerManager : MonoBehaviour
{
    [Header("Level Progression Settings")]
    [SerializeField] private int targetCarrotsToWin = 5;
    [SerializeField] private float hitTimingWindow = 0.22f;

    [Header("Side-Scroller Hop Settings")]
    [SerializeField] private float hopDistanceX = 2.0f; 
    [SerializeField] private float hopHeightY = 1.2f;    
    [SerializeField] private float hopDuration = 0.25f;  

    [Header("UI & Visual Elements")]
    [SerializeField] private TextMeshProUGUI carrotHUDText;
    [SerializeField] private TextMeshProUGUI levelStatusText;
    [SerializeField] private Image beatVisualIndicator;
    [SerializeField] private Transform bunnyTransform;
    [SerializeField] private GameObject carrotPrefab;

    private int collectedCarrots = 0;
    private float lastBeatTime;
    private bool beatHitThisCycle = false;
    private GameObject activeCarrot;

    private Vector3 currentTargetGroundPosition;
    private Coroutine activeHopCoroutine;
    private bool levelCompleted = false;

    private void OnEnable() => ProceduralAudioGenerator.OnBeatTick += HandleBeatTick;
    private void OnDisable() => ProceduralAudioGenerator.OnBeatTick -= HandleBeatTick;

    private void Awake()
    {
        // 1. Grab initial ground position BEFORE Start() runs
        if (bunnyTransform != null)
        {
            currentTargetGroundPosition = bunnyTransform.position;
        }
    }

    private void Start()
    {
        UpdateUI();
        // 2. Immediately spawn the first carrot right in front of player
        SpawnNextCarrot();
    }

    private void HandleBeatTick()
    {
        lastBeatTime = Time.time;
        beatHitThisCycle = false;

        if (beatVisualIndicator != null)
        {
            StartCoroutine(FlashBeatIndicator());
        }
    }

    private IEnumerator FlashBeatIndicator()
    {
        beatVisualIndicator.color = Color.yellow;
        yield return new WaitForSeconds(0.1f);
        beatVisualIndicator.color = Color.white;
    }

    private void Update()
    {
        if (levelCompleted) return;

        bool spacePressed = Input.GetKeyDown(KeyCode.Space);
#if ENABLE_INPUT_SYSTEM
        if (UnityEngine.InputSystem.Keyboard.current != null)
        {
            spacePressed |= UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame;
        }
#endif

        if (spacePressed)
        {
            RegisterPlayerHop();
        }
    }

    private void RegisterPlayerHop()
    {
        float timeSinceBeat = Time.time - lastBeatTime;

        if (timeSinceBeat <= hitTimingWindow && !beatHitThisCycle)
        {
            SuccessfulForwardHop();
        }
        else
        {
            FailedHop();
        }
    }

    private void SuccessfulForwardHop()
    {
        beatHitThisCycle = true;
        currentTargetGroundPosition += new Vector3(hopDistanceX, 0f, 0f);

        if (bunnyTransform != null)
        {
            if (activeHopCoroutine != null) StopCoroutine(activeHopCoroutine);
            activeHopCoroutine = StartCoroutine(AnimateForwardHopRoutine());
        }

        // Check if bunny reached/passed the active carrot
        if (activeCarrot != null)
        {
            float distToCarrot = Vector2.Distance(bunnyTransform.position, activeCarrot.transform.position);
            
            // If bunny gets within 1 hop distance of the carrot, eat it
            if (distToCarrot <= hopDistanceX * 1.2f)
            {
                CollectCarrot();
            }
        }
    }

    private void FailedHop()
    {
        Debug.Log("MISSED BEAT!");

        if (collectedCarrots > 0)
        {
            collectedCarrots--;
            UpdateUI();
        }

        if (bunnyTransform != null && activeHopCoroutine == null)
        {
            StartCoroutine(StumbleRoutine());
        }
    }

    private void CollectCarrot()
    {
        collectedCarrots++;
        
        if (activeCarrot != null)
        {
            Destroy(activeCarrot);
        }

        UpdateUI();

        if (collectedCarrots >= targetCarrotsToWin)
        {
            CompleteLevel();
        }
        else
        {
            SpawnNextCarrot();
        }
    }

    private IEnumerator AnimateForwardHopRoutine()
    {
        float elapsedTime = 0f;
        Vector3 startPos = bunnyTransform.position;

        while (elapsedTime < hopDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsedTime / hopDuration);

            float currentX = Mathf.Lerp(startPos.x, currentTargetGroundPosition.x, normalizedTime);
            float verticalArc = Mathf.Sin(normalizedTime * Mathf.PI) * hopHeightY;
            float currentY = startPos.y + verticalArc;

            bunnyTransform.position = new Vector3(currentX, currentY, startPos.z);
            yield return null;
        }

        bunnyTransform.position = currentTargetGroundPosition;
        activeHopCoroutine = null;
    }

    private IEnumerator StumbleRoutine()
    {
        Vector3 orig = bunnyTransform.position;
        bunnyTransform.position = orig + new Vector3(-0.15f, 0f, 0f);
        yield return new WaitForSeconds(0.08f);
        bunnyTransform.position = orig;
    }

    private void SpawnNextCarrot()
    {
        if (carrotPrefab == null) return;

        if (activeCarrot != null)
        {
            Destroy(activeCarrot);
        }

        // Spawn carrot 2 hops ahead on the floor
        Vector3 spawnPos = currentTargetGroundPosition + new Vector3(hopDistanceX * 2.0f, 0f, 0f);
        
        // Retain Z-index so carrot renders in front of background
        spawnPos.z = bunnyTransform != null ? bunnyTransform.position.z : 0f;

        activeCarrot = Instantiate(carrotPrefab, spawnPos, Quaternion.identity);
    }

    private void UpdateUI()
    {
        if (carrotHUDText != null)
        {
            carrotHUDText.text = $"Carrots: {collectedCarrots} / {targetCarrotsToWin}";
        }
    }

    private void CompleteLevel()
    {
        levelCompleted = true;
        if (levelStatusText != null)
        {
            levelStatusText.text = "LEVEL COMPLETE!";
        }

        Invoke(nameof(LoadNextLevel), 2.0f);
    }

    private void LoadNextLevel()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            SceneManager.LoadScene(0);
        }
    }
}