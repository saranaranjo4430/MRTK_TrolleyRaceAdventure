using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("Car Movement")]
    public float speed = 0f;  // Speed starts at 0
    public float defaultSpeed = 1f;  // Default speed when game starts
    public float maxSpeed = 10f;
    public float minSpeed = 2f;
    public float turnSpeed = 100f;

    private bool turnLeft = false;
    private bool turnRight = false;
    private bool isGameStarted = false;  // Track if the game has started
    private float trackYPosition;
    private Vector3 lastSafePosition;
    private Vector3 positionStart;
    private Quaternion rotationStart;

    // Reference to the track GameObject
    public GameObject track;

    private GameController gameController;

    private void Start()
    {
        gameController = FindObjectOfType<GameController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Cherry")) // 🍒 If car hits a cherry
        {
            gameController.AddScore(1); // Add 1 point
            Destroy(other.gameObject); // Remove cherry from scene
            Debug.Log("✅ Collected a Cherry! Score +1");
        }
        else if (other.CompareTag("Banana")) // 🍌 If car hits a banana
        {
            gameController.AddScore(-1); // Subtract 1 point
            Destroy(other.gameObject); // Remove banana from scene
            Debug.Log("🚨 Hit a Banana! Score -1");
        }
    }

    private void Update()
    {
        if (!isGameStarted) return;

        // 🔒 Keep the car at y = 1.2 (prevent flying)
        Vector3 position = transform.position;
        position.y = 1.22f; // Lock Y-axis to track height
        transform.position = position;

        // Move forward
        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        // Handle turning
        if (turnLeft)
        {
            transform.Rotate(Vector3.up, -turnSpeed * Time.deltaTime);
        }
        else if (turnRight)
        {
            transform.Rotate(Vector3.up, turnSpeed * Time.deltaTime);
        }

        // Speed control
        if (Input.GetKey(KeyCode.UpArrow) && speed < maxSpeed)
        {
            speed += 0.1f;
        }
        else if (Input.GetKey(KeyCode.DownArrow) && speed > minSpeed)
        {
            speed -= 0.1f;
        }
    }




    // Called by GameController when StartGame() is pressed
    public void StartMoving()
    {
        speed = defaultSpeed;
        isGameStarted = true;

        // Set track height dynamically
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 10f))
        {
            trackYPosition = hit.point.y; // Lock car to track height
        }
        else
        {
            Debug.LogWarning("⚠️ No track detected below car!");
        }
    }

    private bool IsCarOnTrack()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 10f))
        {
            return hit.collider.CompareTag("SafeZone"); // Allow movement within Safe Zone
        }
        return false;
    }



    public void SetStartPosition(Vector3 startPosition, Quaternion startRotation)
    {
        positionStart = new Vector3(startPosition.x, 1.2f, startPosition.z); // Ensure correct height
        rotationStart = startRotation; // Store initial rotation
        lastSafePosition = positionStart; // First safe position is the start
    }

  

    private void LateUpdate()
    {
        // Keep track of the last safe position on the track
        if (IsCarOnTrack())
        {
            lastSafePosition = transform.position;
        }
    }

    // UI Button Functions for Turning
    public void StartTurningLeft() { turnLeft = true; }
    public void StopTurningLeft() { turnLeft = false; }

    public void StartTurningRight() { turnRight = true; }
    public void StopTurningRight() { turnRight = false; }
}
