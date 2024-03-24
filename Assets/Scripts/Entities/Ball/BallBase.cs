using System.Collections;
using UnityEngine;
using static Helpers;
public class BallBase : MonoBehaviour
{
    // Parameters for parabolic movement
    public float initialVelocity = 10f;
    public float launchAngle = 45f;

    public float initialHeight = 10f;
    // Time variables
    private float currentTime = 0f;
    public float maxTime = 1f; // Maximum time of flight

    // Reference to the Rigidbody component
    private Rigidbody rb;

    // Flag to indicate whether to use parabolic movement or kinematic gravity
    public bool useParabolicMovement = true;

    public enum BallState
    {
        Initialized,
        Ready
    }
    public BallState State;
    public bool IsInitialized => State == BallState.Initialized;
    public bool IsReady => State == BallState.Ready;

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
    }

    private void Start()
    {
        StartCoroutine(GoStart());
    }

    private IEnumerator GoStart()
    {
        yield return new WaitForSeconds(0.5f);
        ChangeState(BallState.Ready);
    }
    private void Update()
    {
        if (!IsReady) return;
        if (useParabolicMovement)
        {
            // Increment time
            currentTime += Time.deltaTime;

            // Calculate current position using parabolic equation
            float currentZ = initialVelocity * Mathf.Cos(Mathf.Deg2Rad * launchAngle) * currentTime;
            float currentY = initialHeight + initialVelocity * Mathf.Sin(Mathf.Deg2Rad * launchAngle) * currentTime - 0.5f * GRAVITY * Mathf.Pow(currentTime, 2);

            // Update object's position
            transform.position = new Vector3(0f, currentY, currentZ);

            // Check if time exceeds maximum time of flight
            if (currentTime >= maxTime)
            {
                // Reset time and position
                currentTime = 0f;
                transform.position = new Vector3(0f, initialHeight, 0f);
            }
        }
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
        Debug.Log("collision");
        ToggleMovementMode();
    }
}
