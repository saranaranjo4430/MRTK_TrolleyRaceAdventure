using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class GameController : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject startButton;
    public GameObject instructionsButton;
    public Slider carSelectionSlider;
    public TMP_Dropdown circuitDropdown;
    public GameObject carPreviews;
    public Button turnLeftButton;
    public Button turnRightButton;
    public TextMeshProUGUI scoreText;

   

    private int score = 0;

    [Header("Game Elements")]
    public GameObject circuit;
    public GameObject track;
    public GameObject applePrefab;
    public GameObject bananaPrefab;

    [Header("Car Selection")]
    public GameObject[] carPrefabs; // Car prefabs
    public Transform carSpawnPoint; // Spawn position

    [Header("Spawn Settings")]
    private int appleCount;
    private int bananaCount;
    public int maxSpawnAttempts = 50;

    private Bounds trackBounds;
    private GameObject spawnedCar; // Store the spawned car reference

    [Header("Game Over & Win UI")]
    public GameObject winDialog; // Win screen
    public GameObject gameOverDialog; // Game Over screen

    private int totalCherries; // Count of cherries at the start
    private int cherriesCollected = 0; // Track how many cherries were collected


    private void Start()
    {
        MeshRenderer trackRenderer = track.GetComponent<MeshRenderer>();
        if (trackRenderer != null)
        {
            trackBounds = trackRenderer.bounds;
        }
        else
        {
            Debug.LogError("Track object does not have a MeshRenderer!");
        }

        circuitDropdown.onValueChanged.AddListener(UpdateDifficulty);
        UpdateDifficulty(circuitDropdown.value);
        UpdateScoreUI();
    }

    private void UpdateDifficulty(int index)
    {
        if (index == 0)
        {
            appleCount = 10;
            bananaCount = 5;
            Debug.Log("Difficulty set to Easy (Circuit 1)");
        }
        else if (index == 1)
        {
            appleCount = 20;
            bananaCount = 10;
            Debug.Log("Difficulty set to Hard (Circuit 2)");
        }
    }

    public void StartGame()
    {
        startButton.SetActive(false);
        instructionsButton.SetActive(false);
        carSelectionSlider.gameObject.SetActive(false);
        circuitDropdown.gameObject.SetActive(false);
        carPreviews.SetActive(false);

        circuit.SetActive(true);
        scoreText.gameObject.SetActive(true);

        // Spawn the selected car and store it
        SpawnSelectedCar();

        // Spawn objects
        SpawnObjects(applePrefab, appleCount);
        SpawnObjects(bananaPrefab, bananaCount);

        Debug.Log($"Game Started: UI hidden, Objects spawned ({appleCount} apples, {bananaCount} bananas).");
    }

    private void SpawnSelectedCar()
    {
        int carIndex = Mathf.RoundToInt(carSelectionSlider.value);
        if (carIndex < 0 || carIndex >= carPrefabs.Length)
        {
            Debug.LogError("Invalid car index!");
            return;
        }

        spawnedCar = Instantiate(carPrefabs[carIndex], carSpawnPoint.position, Quaternion.identity);

        // 🔄 Rotate the car to face the correct direction
        spawnedCar.transform.rotation = Quaternion.Euler(0, 90, 0); // Adjust the Y-axis as needed

        Debug.Log($"Spawned car: {spawnedCar.name}");

        // Ensure the car starts moving
        CarController carController = spawnedCar.GetComponent<CarController>();
        if (carController != null)
        {
            carController.StartMoving();
            carController.SetStartPosition(carSpawnPoint.position, spawnedCar.transform.rotation); // Store Position & Rotation
        }
        else
        {
            Debug.LogError("CarController script is missing on the spawned car!");
        }

        // Attach UI buttons dynamically
        AssignControlButtons();
    }





    private void AssignControlButtons()
    {
        if (spawnedCar == null) return;

        CarController carController = spawnedCar.GetComponent<CarController>();

        if (carController == null)
        {
            Debug.LogError("CarController script is missing on the car prefab!");
            return;
        }

        // Remove previous listeners to prevent duplication
        turnLeftButton.onClick.RemoveAllListeners();
        turnRightButton.onClick.RemoveAllListeners();

        // Add Event Triggers for holding down & releasing buttons
        AddEventTrigger(turnLeftButton.gameObject, EventTriggerType.PointerDown, carController.StartTurningLeft);
        AddEventTrigger(turnLeftButton.gameObject, EventTriggerType.PointerUp, carController.StopTurningLeft);

        AddEventTrigger(turnRightButton.gameObject, EventTriggerType.PointerDown, carController.StartTurningRight);
        AddEventTrigger(turnRightButton.gameObject, EventTriggerType.PointerUp, carController.StopTurningRight);
    }

    // Utility function to add event triggers to buttons
    private void AddEventTrigger(GameObject button, EventTriggerType eventType, UnityEngine.Events.UnityAction action)
    {
        EventTrigger trigger = button.GetComponent<EventTrigger>() ?? button.AddComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry { eventID = eventType };
        entry.callback.AddListener((eventData) => action());
        trigger.triggers.Add(entry);
    }



    private void SpawnObjects(GameObject prefab, int count)
    {
        int spawned = 0;
        int attempts = 0;

        while (spawned < count && attempts < maxSpawnAttempts)
        {
            Vector3 spawnPosition = GetRandomPointOnTrack();

            if (spawnPosition != Vector3.zero)
            {
                GameObject obj = Instantiate(prefab, spawnPosition, Quaternion.identity);

                // Count total cherries at the start
                if (prefab == applePrefab)
                {
                    totalCherries++;
                }

                spawned++;
            }

            attempts++;
        }
    }


    private Vector3 GetRandomPointOnTrack()
    {
        float x = Random.Range(trackBounds.min.x, trackBounds.max.x);
        float z = Random.Range(trackBounds.min.z, trackBounds.max.z);
        float y = trackBounds.max.y + 1f;

        RaycastHit hit;
        if (Physics.Raycast(new Vector3(x, y, z), Vector3.down, out hit, 10f))
        {
            if (hit.collider.gameObject == track)
            {
                return hit.point;
            }
        }

        return Vector3.zero;
    }


    public void AddScore(int amount)
    {
        score += amount;
        UpdateScoreUI();

        // 🍒 If collecting a cherry, increase count
        if (amount > 0)
        {
            cherriesCollected++;
        }

        // ✅ Check if all cherries are collected (Win Condition)
        if (cherriesCollected >= totalCherries && totalCherries > 0)
        {
            Debug.Log("All cherries collected! You win!");
            ShowWinScreen();
        }

        // ❌ If score is -5 or lower, trigger Game Over
        if (score <= -2)
        {
            Debug.Log("Score is too low! Game Over!");
            ShowGameOverScreen();
        }
    }

    private void UpdateScoreUI() 
    {
        scoreText.text = "Score: " + score;
    }

    private void ShowWinScreen()
    {
        winDialog.SetActive(true);
        Time.timeScale = 0; // Pause the game
    }

    private void ShowGameOverScreen()
    {
        gameOverDialog.SetActive(true);
        Time.timeScale = 0; // Pause the game
    }

}
