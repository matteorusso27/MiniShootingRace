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

    // For score updates
    private int encounteredTriggers = 0;

    public delegate void ScoreUpdateEventHandler(int score);
    public event ScoreUpdateEventHandler OnScoreUpdate;
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
        encounteredTriggers = 0;
        transform.position = startingPosition;
        ChangeState(BallState.Ready);
    }

    public void StartParabolic()
    {
        rb.isKinematic = false;
        ChangeState(BallState.ParabolicMovement);
    }
    private void Update()
    {
        CanvasManager.Instance.Canvas.SetBallState(State.ToString());
        if (IsGrounded) return;

        HandleMovement();
        HandleRotation();
    }

    public void CalculateLaunchParameters(ShootType shootType)
    {
        Vector3 finalPosition;
        if (shootType == ShootType.PerfectShoot || shootType == ShootType.RegularShoot)
            finalPosition = HOOP_POSITION;
        else if (shootType == ShootType.BoardShoot)
        {
            finalPosition = BOARD_HIT_POSITION;
        }
        else
        {
            //to do random and based on the force to the ball
            finalPosition = new Vector3(3.5f, HOOP_POSITION.y, HOOP_POSITION.z); 
        }
        Vector3 displacement = finalPosition - startingPosition;
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

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag(StringTag(GameTag.HoopTriggers)))
        {
            encounteredTriggers++;
            if (encounteredTriggers == 2)
            {
                Debug.Log("update score encountered");
                OnScoreUpdate?.Invoke(2);
                encounteredTriggers = 0;
            }
        }
    }

    public IEnumerator ResetBall()
    {
        //yield return new WaitForSeconds(1.5f);
        yield return new WaitForEndOfFrame();
        Setup();
        ChangeRigidbodyValues();
    }
}
