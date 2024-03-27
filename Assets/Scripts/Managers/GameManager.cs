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
    public struct GameVariables
    {
        public int currentScore; //todo add a score manager?
        public float elapsedTime;
        public ShootType currentShoot;
        public bool isBoardSparking;
    }

    GameVariables Game_variables;
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
        Game_variables.elapsedTime = 0f;
        SwipeManager.Instance.Setup();

        Coroutine swipeAgainCoroutine = null;
        var activeBall = InstanceManager.Instance._inGameObjects.Where(go => go.GetComponent<NormalBall>() != null).
            FirstOrDefault()?.GetComponent<NormalBall>();
        CanvasManager.Instance.SetupFillBar();
        activeBall.OnScoreUpdate += OnScoreUpdated;
        activeBall.OnResetBall += HandleSparkingBoard;

        
        while (Game_variables.elapsedTime < PLAYER_TURN_TIME || !activeBall.IsReady)
        {
            activeBall.StartingPosition = new Vector3(GetRandomNumber(-7, 7), 4, -3);
            // Increment the elapsed time by the time passed since the last frame
            Game_variables.elapsedTime += Time.deltaTime;
            CanvasManager.Instance.Canvas.SetTime((int)(PLAYER_TURN_TIME - Game_variables.elapsedTime));

            if (SwipeManager.Instance.SwipeIsMeasured)
            {
                // todo Add if ball animation is playing (avoid this loop multiple times)
                //if (activeBall.IsInParabolicMovement) continue;
                Game_variables.currentShoot = GetShootType(SwipeManager.Instance.normalizedDistance);
                if (activeBall.IsReady)
                {
                    activeBall.Setup();
                    MotionManager.Instance.Setup(activeBall.transform.position, HOOP_POSITION, isPlayerBall:true);
                    if(!MotionManager.Instance.IsPlayerBallInMotion)
                        MotionManager.Instance.StartMotion(IsPlayerBall: true);
                }
                
                // Then start again
                swipeAgainCoroutine = StartCoroutine(SwipeManager.Instance.CanSwipeAgain());
            }
            yield return new WaitForEndOfFrame();
        }
        swipeAgainCoroutine = null;
        // Go to next state
        activeBall.OnScoreUpdate -= OnScoreUpdated;
        activeBall.OnResetBall -= HandleSparkingBoard;
        HandleSparkingBoard(true);
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

    private void HandleSparkingBoard(bool disable = false)
    {
        bool toChange;
        if (disable == true)
            toChange = false;
        else
            toChange = Game_variables.elapsedTime >= SPARKING_BOARD_TIME;
        var board = GameObject.FindGameObjectWithTag(StringTag(GameTag.Board));
        board.GetComponent<MeshRenderer>().enabled = toChange;
        Game_variables.isBoardSparking = toChange;
    }
    public void OnScoreUpdated()
    {
        var score = GetScore(Game_variables.currentShoot, Game_variables.isBoardSparking);
        Game_variables.currentScore += score;
        CanvasManager.Instance.Canvas.SetScore(Game_variables.currentScore);
    }
}
