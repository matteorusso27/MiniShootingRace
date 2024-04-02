using GG.Infrastructure.Utils.Swipe;
using System.Collections;
using UnityEngine;
using static Helpers;
using static GameSelectors;

public class SwipeManager : Singleton<SwipeManager>
{
    [SerializeField] private SwipeListener swipeListener;

    // Swipe detection parameters
    public float screenHeight;
    public float startingPointY = -1;
    public float normalizedDistance;
    public float maxSwipeDistance;
    
    // Coroutine reference for swipe detection timeout
    private Coroutine swipeCoroutine;

    public enum SwipeState
    {
        WaitForSwipeDetection,
        SwipeDetection,
        SwipeMeasured
    }
    public SwipeState State;
    public void ChangeSwipeState(SwipeState newState)
    {
        State = newState;
    }

    private void OnEnable()
    {
        // Subscribe to swipe events
        swipeListener.OnSwipe.AddListener(OnSwipe);
        swipeListener.OnSwipeCancelled.AddListener(OnSwipeCancelled);

        screenHeight = Screen.height;
        maxSwipeDistance = screenHeight / 2;
        //todo add minswipedistance
        ChangeSwipeState(SwipeState.WaitForSwipeDetection);
    }

    public bool CanMeasureSwipe => State == SwipeState.SwipeDetection && GameM.Data.IsBallReady;
    public bool SwipeIsMeasured => State == SwipeState.SwipeMeasured;

    public void Setup()
    {
        normalizedDistance = 0f;
        CanvasM.Canvas.SetFillBar(normalizedDistance);
        CanvasM.Canvas.SetText(normalizedDistance.ToString());
        CanvasM.Canvas.SetSwipeStateText(State.ToString());
        ChangeSwipeState(SwipeState.SwipeDetection);
    }
    private void OnSwipe(string swipe)
    {
        // Only process swipe if measurement is allowed
        if (!CanMeasureSwipe) return;

        if (swipe.Equals("Down"))
        {
            OnSwipeCancelled();
        }
        if (startingPointY == -1) // todo change to delegate that subscribes to first swipe interaction
        {
            startingPointY = swipeListener._swipePoint.y;
            swipeCoroutine = StartCoroutine(CoroutineSwipeDetection());
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
        CanvasM.Canvas.SetSwipeStateText(State.ToString());
    }

    private void Update()
    {
        // Update swipe parameters and UI if swipe is in progress
        if (startingPointY != -1 && CanMeasureSwipe)
        {
            UpdateSwipeParameters();
            UpdateUI();
        }
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
        CanvasM.Canvas.SetText("D: " + normalizedDistance.ToString());
        CanvasM.Canvas.SetFillBar(normalizedDistance);
        CanvasM.Canvas.SetSwipeStateText(State.ToString());
        var markerPositionY = CanvasM.Canvas.FillBarHeight * normalizedDistance;
        CanvasM.Canvas.FillMarker.rectTransform.anchoredPosition = new Vector3(0, markerPositionY, 0);
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
        yield return new WaitForSeconds(TIME_TO_SWIPE);

        // End swipe phase if swipe distance not measured
        if (!SwipeIsMeasured) EndSwipingPhase();
    }

    public IEnumerator CanSwipeAgain()
    {
        // Wait to allow swipe measurement again
        yield return new WaitForSeconds(1f);
        Setup();
    }
}
