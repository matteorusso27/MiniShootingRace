using System.Collections;
using UnityEngine;
using static Helpers;
public class BallBase : MonoBehaviour
{
    // Parameters for parabolic movement
    public float initialVelocity = 20f;
    public float launchAngle = 45f;

    // Time variables
    private float currentTime = 0f;
    private float maxTime = 3f; // Maximum time of flight

    // Reference to the Rigidbody component
    private Rigidbody rb;

    // Flag to indicate whether to use parabolic movement or kinematic gravity
    public bool useParabolicMovement = true;

    public Vector3 currentPosition;
    public Vector3 startingPosition;
    public enum BallState
    {
        Initialized,
        Ready,
        Grounded
    }
    public BallState State;
    private float rotationSpeed = 200f;

    public bool IsInitialized => State == BallState.Initialized;
    public bool IsReady => State == BallState.Ready;
    public bool IsGrounded => State == BallState.Grounded;

    public void ChangeState(BallState newState)
    {
        State = newState;
    }
    private void Awake()
    {
        // Get reference to the Rigidbody component or add it if it doesn't exist
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        currentPosition = transform.position;
        startingPosition = currentPosition;
    }

    private void Start()
    {
        StartCoroutine(GoStart());
    }

    private IEnumerator GoStart()
    {
        Debug.Log(transform.position);
        yield return new WaitForSeconds(0.5f);
        ChangeState(BallState.Ready);
    }
    private void Update()
    {
        if (!IsReady) return;

        HandleMovement();
        HandleRotation();
    }

    private void HandleMovement()
    {
        if (useParabolicMovement)
        {
            // Increment time
            currentTime += Time.deltaTime;

            // Calculate current position using parabolic equation
            float currentX = currentPosition.x;
            float currentY = currentPosition.y + initialVelocity * Mathf.Sin(Mathf.Deg2Rad * launchAngle) * currentTime - 0.5f * GRAVITY * Mathf.Pow(currentTime, 2);
            float currentZ = currentPosition.z + initialVelocity * Mathf.Cos(Mathf.Deg2Rad * launchAngle) * currentTime;

            // Update object's position
            transform.position = new Vector3(currentX, currentY, currentZ);

            // Check if time exceeds maximum time of flight
            if (currentTime >= maxTime)
            {
                // Reset time and position
                currentTime = 0f;
                //transform.position = startingPosition;
            }
        }
    }
    private void HandleRotation()
    {
        float rotationAngleX = rotationSpeed * Time.deltaTime;

        // Apply rotation
        transform.Rotate(rotationAngleX, 0f, 0f);
    }

    // Method to toggle between parabolic movement and kinematic gravity
    public void ToggleMovementMode()
    {
        // Toggle the flag
        useParabolicMovement = false;

        // Enable or disable kinematic mode accordingly
        
        rb.isKinematic = false;
        rb.useGravity = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("trigger collision");
        ToggleMovementMode();
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("enter collision");
        if (collision.gameObject.CompareTag(StringTag(GameTag.Terrain)))
            ChangeState(BallState.Grounded);
    }
}
