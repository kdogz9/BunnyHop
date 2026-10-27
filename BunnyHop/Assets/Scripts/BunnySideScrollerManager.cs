using System.Collections;
using UnityEngine;
using TMPro;
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

    [Header("Rhythm Visual Cues & Ground Alignment")]
    [Tooltip("Assign 2D prefabs for Grass or Trunks that spawn on beat.")]
    [SerializeField] private GameObject[] beatMarkerPrefabs; 
    [Tooltip("Adjust this to move spawned props up or down to align with your tilemap ground surface.")]
    [SerializeField] private float groundYOffset = -0.5f; 
    [SerializeField] private float propDestroyDelay = 6.0f; 

    [Header("UI & Visual References")]
    [SerializeField] private TextMeshProUGUI carrotHUDText;
    [SerializeField] private TextMeshProUGUI levelStatusText;
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
        if (bunnyTransform != null)
        {
            currentTargetGroundPosition = bunnyTransform.position;
        }
    }

    private void Start()
    {
        UpdateUI();
        SpawnNextCarrot();
    }

    private void HandleBeatTick()
    {
        lastBeatTime = Time.time;
        beatHitThisCycle = false;

        // Spawn obstacle between landing spots so the bunny hops OVER it
        SpawnBeatObstacle();
    }

    private void SpawnBeatObstacle()
    {
        if (beatMarkerPrefabs == null || beatMarkerPrefabs.Length == 0) return;

        int randomIndex = Random.Range(0, beatMarkerPrefabs.Length);
        GameObject selectedPrefab = beatMarkerPrefabs[randomIndex];

        if (selectedPrefab != null)
        {
            // Position the obstacle halfway between current position and next landing spot (midpoint of the jump arc)
            float midpointX = currentTargetGroundPosition.x + (hopDistanceX * 0.5f);
            
            // Apply the manual ground Y offset so it sits flush on the tilemap
            float groundY = currentTargetGroundPosition.y + groundYOffset;
            float zPos = bunnyTransform != null ? bunnyTransform.position.z : 0f;

            Vector3 spawnPos = new Vector3(midpointX, groundY, zPos);

            GameObject newObstacle = Instantiate(selectedPrefab, spawnPos, Quaternion.identity);
            Destroy(newObstacle, propDestroyDelay);
        }
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

        if (activeCarrot != null)
        {
            float distToCarrot = Vector2.Distance(bunnyTransform.position, activeCarrot.transform.position);
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
        if (activeCarrot != null) Destroy(activeCarrot);

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

        if (activeCarrot != null) Destroy(activeCarrot);

        // Spawn carrots at landing spots (2 hops ahead)
        Vector3 spawnPos = currentTargetGroundPosition + new Vector3(hopDistanceX * 2.0f, 0f, 0f);
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