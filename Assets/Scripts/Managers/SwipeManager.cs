using GG.Infrastructure.Utils.Swipe;
using System.Collections;
using UnityEngine;

public class SwipeManager : Singleton<SwipeManager>
{
    [SerializeField] private SwipeListener swipeListener;

    // Swipe detection parameters
    public float screenHeight;
    public float startingPointY = -1;
    public float normalizedDistance;
    public float maxSwipeDistance;

    private float timeToSwap = 3f;

    // State
    public enum SwipeState
    {
        WaitForSwipeDetection,
        SwipeDetection,
        SwipeMeasured
    }
    public SwipeState State { get; private set; }
    public void ChangeSwipeState(SwipeState newState)
    {
        State = newState;
    }

    // Coroutine reference for swipe detection timeout
    // todo, needed?
    private Coroutine swipeCoroutine;

    private void OnEnable()
    {
        // Subscribe to swipe events
        swipeListener.OnSwipe.AddListener(OnSwipe);
        swipeListener.OnSwipeCancelled.AddListener(OnSwipeCancelled);

        screenHeight = Screen.height;
        maxSwipeDistance = screenHeight / 2;
        ChangeSwipeState(SwipeState.WaitForSwipeDetection);
        Setup();
    }

    private bool CanMeasureSwipe => State == SwipeState.SwipeDetection;
    private bool SwipeIsMeasured => State == SwipeState.SwipeMeasured;

    public void Setup()
    {
        CanvasManager.Instance.SetFillBar(normalizedDistance);
        ChangeSwipeState(SwipeState.SwipeDetection);
        CanvasManager.Instance.SetSwipeStateText(State.ToString());
    }
    private void OnSwipe(string swipe)
    {
        // Only process swipe if measurement is allowed
        if (!CanMeasureSwipe) return;

        // Initialize starting point and start swipe detection coroutine
        if (startingPointY == -1)
        {
            startingPointY = swipeListener._swipePoint.y;
            swipeCoroutine = StartCoroutine(CoroutineSwipeDetection());
        }
        // Check for downward swipe
        if (swipe.Equals("Down"))
        {
            OnSwipeCancelled();
        }
    }

    private void OnSwipeCancelled()
    {
        EndSwipingPhase();
    }

    public void EndSwipingPhase()
    {
        ResetState();
    }

    private void ResetState()
    {
        // Reset swipe parameters
        startingPointY = -1;
        swipeCoroutine = null; 
        ChangeSwipeState(SwipeState.SwipeMeasured);
        CanvasManager.Instance.SetSwipeStateText(State.ToString());
        normalizedDistance = 0f;
    }

    private void Update()
    {
        // Update swipe parameters and UI if swipe is in progress
        if (startingPointY != -1 && CanMeasureSwipe)
        {
            UpdateSwipeParameters();
            UpdateUI();
        }

        if (Input.GetKeyDown(KeyCode.Space))
            Setup();
    }

    private void UpdateSwipeParameters()
    {
        // Calculate swipe distance and normalize it
        var currentSwipeDistance = Mathf.Abs(swipeListener._swipePoint.y - startingPointY);
        normalizedDistance = Mathf.Clamp(currentSwipeDistance / maxSwipeDistance, 0, 1);
    }

    private void UpdateUI()
    {
        // Update UI with swipe distance
        CanvasManager.Instance.SetText("D: " + normalizedDistance.ToString());
        CanvasManager.Instance.SetFillBar(normalizedDistance);
        CanvasManager.Instance.SetSwipeStateText(State.ToString());
    }

    private void OnDisable()
    {
        // Unsubscribe from swipe events
        swipeListener.OnSwipe.RemoveAllListeners();
        ChangeSwipeState(SwipeState.WaitForSwipeDetection);
    }

    private IEnumerator CoroutineSwipeDetection()
    {
        // Wait for swipe detection timeout
        yield return new WaitForSeconds(timeToSwap);

        // End swipe phase if swipe distance not measured
        if (!SwipeIsMeasured) EndSwipingPhase();
    }

    private IEnumerator CanSwipeAgain()
    {
        // Wait to allow swipe measurement again
        yield return new WaitForSeconds(1.25f);
        //canMeasureSwipe = true;
    }
}