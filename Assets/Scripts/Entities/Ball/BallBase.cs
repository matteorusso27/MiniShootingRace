using System.Collections;
using UnityEngine;
using static Helpers;
public class BallBase : MonoBehaviour
{
    // For score updates
    private int encounteredTriggers = 0;

    public delegate void ScoreUpdateEventHandler();
    public event ScoreUpdateEventHandler OnScoreUpdate;

    public delegate void ResetBallHandler(bool isForced = false);
    public event ResetBallHandler OnResetBall;
    public enum BallState
    {
        Ready,
        Movement,
        Grounded
    }
    public BallState State;
    private float rotationSpeed = 200f;

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
    
    public void Setup()
    {
        rb.isKinematic = true;
        rb.useGravity = false;
        ChangeState(BallState.Movement);
    }

    private void Update()
    {
        CanvasManager.Instance.Canvas.SetBallState(State.ToString()); //todo remove
        if (IsGrounded) return;
        HandleRotation();
    }

    private void HandleRotation()
    {
        float rotationAngleX = rotationSpeed * Time.deltaTime;
        transform.Rotate(rotationAngleX, 0f, 0f);
    }

    public void SimulatePhysicsMode()
    {
        rb.isKinematic = false;
        rb.useGravity = true;
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
                OnScoreUpdate?.Invoke();
                encounteredTriggers = 0;
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(StringTag(GameTag.HoopTriggers)))
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
    }

    public IEnumerator ResetBall()
    {
        yield return new WaitForSeconds(1f);
        ChangeState(BallState.Ready);
        transform.position = StartingPosition;
        rb.isKinematic = true;
        rb.useGravity = false;
        OnResetBall?.Invoke();
    }
}
