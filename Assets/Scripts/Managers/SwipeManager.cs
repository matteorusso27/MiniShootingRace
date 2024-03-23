using GG.Infrastructure.Utils.Swipe;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwipeManager : Singleton<SwipeManager>
{
    [SerializeField]
    private SwipeListener swipeListener;

    public float screenHeight;
    public float currentSwipeDistance;
    public float startingPointY = -1;

    public bool canMeasureSwipe = true;
    public bool distanceMeasured;
    public Coroutine swipeCoroutine;

    public float normalizedDistance;
    public bool hasSwipedEnough;
    private void OnEnable()
    {
        swipeListener.OnSwipe.AddListener(OnSwipe);
        swipeListener.OnSwipeCancelled.AddListener(OnSwipeCancelled);
        screenHeight = Screen.height;
    }
    private void OnSwipe(string swipe)
    {
        if (!canMeasureSwipe) return;

        if (startingPointY == -1) 
            startingPointY = swipeListener._swipePoint.y;
        //swipeCoroutine = StartCoroutine(CoroutineSwipeDetection());
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
        UpdateUIBar();
        ResetState();
    }

    private void ResetState()
    {
        startingPointY = -1;
        swipeCoroutine = null;
        distanceMeasured = true;
        normalizedDistance = 0f;
    }
    private void UpdateUIBar()
    {
        CanvasManager.Instance.SetText("D: "+ currentSwipeDistance);
    }

    private void Update()
    {
        if (startingPointY != -1 && canMeasureSwipe)
        {
            currentSwipeDistance = Mathf.Abs(swipeListener._swipePoint.y - startingPointY);
            var maxSwipeDistance = screenHeight / 2;
            normalizedDistance = Mathf.Clamp(currentSwipeDistance / maxSwipeDistance, 0, 1);
            CanvasManager.Instance.SetFillBar(normalizedDistance);
        }
    }

    private void OnDisable()
    {
        swipeListener.OnSwipe.RemoveAllListeners();
    }

    private IEnumerator CoroutineSwipeDetection()
    {
        var timeToSwap = 1.25f;
        yield return new WaitForSeconds(timeToSwap);
        // You could have released before the timeToSwap has expired
        if (!distanceMeasured) EndSwipingPhase();
    }

    private IEnumerator CanSwipeAgain()
    {
        yield return new WaitForSeconds(1.25f);
        canMeasureSwipe = true;
    }
}
