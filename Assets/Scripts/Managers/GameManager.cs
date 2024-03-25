using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using static Helpers;

public class GameManager : Singleton<GameManager>
{
    public static event Action<GameState> OnBeforeStateChanged;
    public static event Action<GameState> OnAfterStateChanged;

    public GameState State { get; private set; }

    #region GAME VARIABLES
    private int currentScore = 0; //todo add a score manager?
    private ShootType currentShoot;
    #endregion

    void Start()
    {
        ChangeState(GameState.Start);
    }
    private void ChangeState(GameState newState)
    {
        OnBeforeStateChanged?.Invoke(newState);
        //CanvasManager.Instance.SetGameState(newState.ToString());
        State = newState;
        switch (newState)
        {
            case GameState.Start:
                HandleStart();
                break;
            case GameState.SpawningPlayer:
                HandleSpawningPlayer();
                break;
            case GameState.PlayerTurn:
                StartCoroutine(HandlePlayerTurn());
                break;
            case GameState.End:
                StartCoroutine(HandleEnd());
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }

        OnAfterStateChanged?.Invoke(newState);
    }

    private void HandleStart()
    {
        // Do things

        // Go to next state
        ChangeState(GameState.SpawningPlayer);
    }

    private void HandleSpawningPlayer()
    {
        InstanceManager.Instance.SpawnPlayerAndBall();
        // Do things

        // Go to next state
        ChangeState(GameState.PlayerTurn);
    }

    private IEnumerator HandlePlayerTurn()
    {
        SwipeManager.Instance.Setup();

        Coroutine swipeAgainCoroutine = null;
        var activeBall = InstanceManager.Instance._inGameObjects.Where(go => go.GetComponent<NormalBall>() != null).
            FirstOrDefault()?.GetComponent<NormalBall>();
        CalculateRanges(difficulty: 6);
        var startingPerfectRangePositionY = CanvasManager.Instance.Canvas.fillBar.rectTransform.rect.height * START_RANGE_PERFECT_SHOOT;
        CanvasManager.Instance.Canvas.PerfectRange.rectTransform.anchoredPosition = new Vector3(0, startingPerfectRangePositionY, 0);

        var startingBoardRangePositionY = CanvasManager.Instance.Canvas.fillBar.rectTransform.rect.height * START_RANGE_BOARD_SHOOT;
        CanvasManager.Instance.Canvas.BoardRange.rectTransform.anchoredPosition = new Vector3(0, startingBoardRangePositionY, 0);
        activeBall.OnScoreUpdate += OnScoreUpdated;
        float elapsedTime = 0f;
        while (elapsedTime < PLAYER_TURN_TIME)
        {
            // Increment the elapsed time by the time passed since the last frame
            elapsedTime += Time.deltaTime;
            CanvasManager.Instance.Canvas.SetTime((int)(PLAYER_TURN_TIME - elapsedTime));

            if (SwipeManager.Instance.SwipeIsMeasured)
            {
                // todo Add if ball animation is playing (avoid this loop multiple times)
                //if (activeBall.IsInParabolicMovement) continue;
                currentShoot = GetShootType(SwipeManager.Instance.normalizedDistance);
                if (activeBall.IsInitialized || activeBall.IsReady)
                {
                    activeBall.Setup();
                    activeBall.CalculateLaunchParameters(currentShoot);
                    activeBall.StartParabolic();
                }
                
                // Then start again
                swipeAgainCoroutine = StartCoroutine(SwipeManager.Instance.CanSwipeAgain());
            }
            // Wait for the next frame
            yield return null;
        }
        swipeAgainCoroutine = null;
        // Go to next state
        activeBall.OnScoreUpdate -= OnScoreUpdated;
        ChangeState(GameState.End);
    }

    private IEnumerator HandleEnd()
    {
        yield return new WaitForSeconds(0.1f);
        // Go to next state
      
        ChangeState(GameState.PlayerTurn);
    }

    public enum GameState
    {
        Start = 0,
        SpawningPlayer = 1,
        PlayerTurn = 2,
        End = 3
    }

    public void OnScoreUpdated(int score)
    {
        Debug.Log("Score: " + currentScore);
        Debug.Log("New Score: " + (currentScore + score));
        Debug.Log("-------------------------------------");
        currentScore += score;
        CanvasManager.Instance.Canvas.SetScore(currentScore);
    }
}
