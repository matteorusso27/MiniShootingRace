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

    public bool IsPlayerTurn() { return State == GameState.PlayerTurn; }

    #region GAME VARIABLES
    public struct GameVariables
    {
        public int currentScore; //todo add a score manager?
        public float elapsedTime;
        public ShootType currentShoot;
        public bool isBoardSparking;
    }

    public GameVariables Game_variables;
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
        var playerBall = InstanceManager.Instance.GetBall(IsPlayer:true);
        var enemyBall = InstanceManager.Instance.GetBall(IsPlayer:false);
        playerBall.name = "Playerball";
        enemyBall.name = "Enemyball";
        CanvasManager.Instance.SetupFillBar();
        playerBall.OnScoreUpdate += OnScoreUpdated;
        playerBall.OnResetBall += HandleSparkingBoard;

        
        while (Game_variables.elapsedTime < PLAYER_TURN_TIME || !playerBall.IsReady)
        {
            playerBall.StartingPosition = new Vector3(GetRandomNumber(-7, -5), 4, -3);
            enemyBall.StartingPosition = new Vector3(GetRandomNumber(-6, -4), 4, -3);
            // Increment the elapsed time by the time passed since the last frame
            Game_variables.elapsedTime += Time.deltaTime;
            CanvasManager.Instance.Canvas.SetTime((int)(PLAYER_TURN_TIME - Game_variables.elapsedTime));

            if (SwipeManager.Instance.SwipeIsMeasured)
            {
                // todo Add if ball animation is playing (avoid this loop multiple times)
                //if (activeBall.IsInParabolicMovement) continue;
                var normalizedValue = SwipeManager.Instance.normalizedDistance;
                Game_variables.currentShoot = GetShootType(normalizedValue);
                var finalPosition = GetFinalPosition(Game_variables.currentShoot, normalizedValue);
                SetThrowHeight(Game_variables.currentShoot, normalizedValue);
                if (playerBall.IsReady)
                {
                    playerBall.Setup();
                    MotionManager.Instance.Setup(playerBall.transform.position, finalPosition, isPlayerBall:true);
                    if(!MotionManager.Instance.IsPlayerBallInMotion)
                        MotionManager.Instance.StartMotion(IsPlayerBall: true);
                }
                
                // Then start again
                swipeAgainCoroutine = StartCoroutine(SwipeManager.Instance.CanSwipeAgain());
            }
            // Enemy ball behaviour
            
            if (enemyBall.IsReady)
            {
                var shoot = GetRandomShootType();
                var finalPositionEnemy = GetFinalPosition(shoot, 0.5f); // todo 0.5 togliere
                enemyBall.Setup();
                MotionManager.Instance.Setup(enemyBall.transform.position, finalPositionEnemy, isPlayerBall: false);
                if (!MotionManager.Instance.IsEnemyBallInMotion)
                    MotionManager.Instance.StartMotion(IsPlayerBall: false);
            }

            yield return new WaitForEndOfFrame();
        }
        swipeAgainCoroutine = null;
        // Go to next state
        playerBall.OnScoreUpdate -= OnScoreUpdated;
        playerBall.OnResetBall -= HandleSparkingBoard;
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
