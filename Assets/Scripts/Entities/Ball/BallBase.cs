using System.Collections;
using UnityEngine;
using static Helpers;
using static GameSelectors;
public class BallBase : MonoBehaviour
{
    public delegate void ScoreUpdateEventHandler();
    public event ScoreUpdateEventHandler OnScoreUpdate;

    public delegate void ResetBallHandler();
    public event ResetBallHandler OnResetBall;

    public delegate void OnTriggerPhysicsSimulation(bool isPlayer);
    public event OnTriggerPhysicsSimulation OnTriggerPhysics;

    // For score updates
    private int encounteredTriggers = 0;
    public enum BallState
    {
        Ready,
        Movement,
        Grounded
    }

    public BallType BallType;

    public bool IsPlayer;
    public BallState State;
    private float RotationSpeed = 200f;

    public bool IsReady => State == BallState.Ready;
    public bool IsGrounded => State == BallState.Grounded;
    public bool IsInMovement => State == BallState.Movement;

    public Vector3 startingPosition;
    public Vector3 StartingPosition { get { return startingPosition; } set { startingPosition = value; } }
    private Rigidbody rb;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        StartingPosition = transform.position;
    }
    public void ChangeState(BallState newState) => State = newState;
    
    public void SetupMotionValues()
    {
        rb.isKinematic = true;
        rb.useGravity = false;
        ChangeState(BallState.Movement);
    }

    private void Update()
    {
        if (IsGrounded) return;
        if (BallType != BallType.FireBall)
            HandleRotation();
    }

    private void HandleRotation()
    {
        float rotationAngleX = RotationSpeed * Time.deltaTime;
        transform.Rotate(rotationAngleX, 0f, 0f);
    }

    // Ball movement is split in a deterministic parabolic movement
    // and a simulation of physics until it touches the ground
    public void SimulatePhysicsMode()
    {
        rb.isKinematic = false;
        rb.useGravity = true;
        OnTriggerPhysics?.Invoke(IsPlayer);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(StringTag(GameTag.Terrain)))
        {
            StartReset();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // To score the ball needs to touch both triggers
        if (other.gameObject.CompareTag(StringTag(GameTag.HoopTriggers)))
        {
            encounteredTriggers++;
            if (encounteredTriggers == 2)
            {
                OnScoreUpdate?.Invoke();
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(StringTag(GameTag.Board)))
        {
            AudioM.PlayBoardSound();
        }
    }

    public void StartReset()
    {
        ChangeState(BallState.Grounded);
        StartCoroutine(ResetBall());
    }
    public IEnumerator ResetBall()
    {
        yield return new WaitForSeconds(0.3f);
        ChangeState(BallState.Ready);
        transform.position = StartingPosition;
        rb.isKinematic = true;
        rb.useGravity = false;
        encounteredTriggers = 0;
        OnResetBall?.Invoke();
    }
}
