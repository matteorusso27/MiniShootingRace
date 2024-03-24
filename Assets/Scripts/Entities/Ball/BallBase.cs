using System.Collections;
using UnityEngine;
using static Helpers;
public class BallBase : MonoBehaviour
{
    // Parameters for parabolic movement
    private Vector3 initialVelocity;

    // Time variables
    private float currentTime = 0f;

    // Reference to the Rigidbody component
    private Rigidbody rb;

    public Vector3 currentPosition;
    public Vector3 startingPosition;
    public enum BallState
    {
        Initialized,
        Ready,
        ParabolicMovement,
        PhysicsSimulation,
        Grounded
    }
    public BallState State;
    private float rotationSpeed = 200f;

    public bool IsInitialized => State == BallState.Initialized;
    public bool IsReady => State == BallState.Ready;
    public bool IsGrounded => State == BallState.Grounded;
    public bool IsInParabolicMovement => State == BallState.ParabolicMovement;
    public bool IsInPhisicsSimulation => State == BallState.PhysicsSimulation;
    public bool IsInMovement => IsInParabolicMovement || IsInPhisicsSimulation;

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
        }
        ChangeRigidbodyValues();
        currentPosition = transform.position;
        startingPosition = currentPosition; //todo debug purposes
    }

    public void ChangeRigidbodyValues()
    {
        if (!IsInPhisicsSimulation)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        else
        {
            rb.useGravity = true;
        }
    }

    public void Setup()
    {
        currentTime = 0f;
        transform.position = startingPosition;
        CalculateLaunchParameters();
        ChangeState(BallState.Ready);
    }

    public void StartParabolic()
    {
        rb.isKinematic = false;
        ChangeState(BallState.ParabolicMovement);
    }
    private void Update()
    {
        CanvasManager.Instance.SetBallState(State.ToString());
        if (IsGrounded) return;

        HandleMovement();
        HandleRotation();
    }

    void CalculateLaunchParameters()
    {
        Vector3 displacement = HOOP_POSITION - startingPosition;
        float time = Mathf.Sqrt(2 * Mathf.Abs(displacement.y / GRAVITY));

        float horizontalVelocity = displacement.x / time;
        float verticalVelocity = displacement.y / time;
        float forwardVelocity = displacement.z / time;

        if (displacement.y < 0) verticalVelocity *= -1; 

        initialVelocity = new Vector3(horizontalVelocity, verticalVelocity + 0.5f * GRAVITY * time, forwardVelocity);
    }
    private void HandleMovement()
    {
        if (IsInParabolicMovement)
        {
            currentTime += Time.deltaTime;
            // Calculate current position using parabolic equation
            float currentX = currentPosition.x + initialVelocity.x * currentTime;
            float currentY = currentPosition.y + initialVelocity.y * currentTime - 0.5f * GRAVITY * Mathf.Pow(currentTime, 2);
            float currentZ = currentPosition.z + initialVelocity.z * currentTime;

            transform.position = new Vector3(currentX, currentY, currentZ);

            if (Vector3.Distance(transform.position, HOOP_POSITION) < 0.1f)
            {
                SimulatePhysicsMode();
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
    public void SimulatePhysicsMode()
    {
        // Toggle the flag
        ChangeState(BallState.PhysicsSimulation);
        ChangeRigidbodyValues();
    }

    private void OnTriggerEnter(Collider other)
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(StringTag(GameTag.Board)))
        {
            SimulatePhysicsMode();
        }

        if (collision.gameObject.CompareTag(StringTag(GameTag.Terrain)))
        {
            ChangeState(BallState.Grounded);
            StartCoroutine(ResetBall());
        }
    }

    public IEnumerator ResetBall()
    {
        yield return new WaitForSeconds(1.5f);
        Setup();
        ChangeRigidbodyValues();
    }
}
