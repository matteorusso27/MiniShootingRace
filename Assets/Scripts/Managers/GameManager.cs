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
    public struct GameData
    {
        public int playerScore; 
        public int enemyScore; 
        public ShootType currentPlayerShoot;
        public ShootType currentEnemyShoot;
        public bool isBoardSparking;
        public float elapsedPlayerTime;
    }

    public GameData gameData;
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
    private void Init()
    {
        gameData.playerScore = 0;
        gameData.enemyScore = 0;
        CanvasManager.Instance.Canvas.SetPlayerScore(0);
        CanvasManager.Instance.Canvas.SetEnemyScore(0);
        CanvasManager.Instance.Canvas.FinalText.transform.gameObject.SetActive(false);
        CanvasManager.Instance.Canvas.RestartBtn.transform.gameObject.SetActive(false);
        CanvasManager.Instance.Canvas.CountDown.transform.gameObject.SetActive(true);
    }
    private void HandleStart()
    {
        Init();
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
        IEnumerator StartCountDown()
        {
            for (int countDownTime = 3; countDownTime > 0; countDownTime--)
            {
                CanvasManager.Instance.Canvas.SetCountDownTxt(countDownTime.ToString());
                yield return new WaitForSeconds(1f);
            }
            CanvasManager.Instance.Canvas.SetCountDownTxt("Start!");
            yield return new WaitForSeconds(1f);
            CanvasManager.Instance.Canvas.CountDown.transform.gameObject.SetActive(false);
        }
        yield return StartCountDown();
        gameData.elapsedPlayerTime = 0f;
        SwipeManager.Instance.Setup();

        Coroutine swipeAgainCoroutine = null;
        var playerBall = InstanceManager.Instance.GetBall(IsPlayer:true);
        var enemyBall = InstanceManager.Instance.GetBall(IsPlayer:false);
        playerBall.name = "Playerball";
        enemyBall.name = "Enemyball";
        CanvasManager.Instance.SetupFillBar();
        playerBall.OnScoreUpdate += OnPlayerScoreUpdated;
        enemyBall.OnScoreUpdate += OnEnemyScoreUpdated;
        playerBall.OnResetBall += HandleSparkingBoard;

        
        while (gameData.elapsedPlayerTime < PLAYER_TURN_TIME || !playerBall.IsReady)
        {
            playerBall.StartingPosition = new Vector3(GetRandomNumber(-7, -5), 4, -3);
            enemyBall.StartingPosition = new Vector3(GetRandomNumber(-6, -4), 4, -3);
            // Increment the elapsed time by the time passed since the last frame
            gameData.elapsedPlayerTime += Time.deltaTime;
            CanvasManager.Instance.Canvas.SetTime((int)(PLAYER_TURN_TIME - gameData.elapsedPlayerTime));

            if (SwipeManager.Instance.SwipeIsMeasured)
            {
                // todo Add if ball animation is playing (avoid this loop multiple times)
                //if (activeBall.IsInParabolicMovement) continue;
                var normalizedValue = SwipeManager.Instance.normalizedDistance;
                gameData.currentPlayerShoot = GetShootType(normalizedValue);
                var finalPosition = GetFinalPosition(gameData.currentPlayerShoot, normalizedValue);
                SetThrowHeight(gameData.currentPlayerShoot, normalizedValue);
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
        playerBall.OnScoreUpdate -= OnPlayerScoreUpdated;
        enemyBall.OnScoreUpdate -= OnEnemyScoreUpdated;
        playerBall.OnResetBall -= HandleSparkingBoard;
        ChangeState(GameState.End);
    }

    private IEnumerator HandleEnd()
    {
        yield return new WaitForSeconds(0.1f);
        // Go to next state
        string s;
        if (gameData.playerScore > gameData.enemyScore)
            s = "You Win";
        else if (gameData.playerScore < gameData.enemyScore)
            s = "You lose";
        else
            s = "Tie";
        CanvasManager.Instance.Canvas.SetFinalText(s);
        CanvasManager.Instance.Canvas.RestartBtn.transform.gameObject.SetActive(true);
    }

    public enum GameState
    {
        Start = 0,
        SpawningPlayer = 1,
        PlayerTurn = 2,
        End = 3
    }

    private void HandleSparkingBoard()
    {
        var toChange = gameData.elapsedPlayerTime >= SPARKING_BOARD_TIME;
        var board = GameObject.FindGameObjectWithTag(StringTag(GameTag.Board));
        board.GetComponent<MeshRenderer>().enabled = toChange;
        gameData.isBoardSparking = toChange;
    }
    public void OnPlayerScoreUpdated()
    {
        var score = GetScore(gameData.currentPlayerShoot, gameData.isBoardSparking);
        gameData.playerScore += score;
        CanvasManager.Instance.Canvas.SetPlayerScore(gameData.playerScore);
    }
    
    public void OnEnemyScoreUpdated()
    {
        var score = GetScore(gameData.currentEnemyShoot, gameData.isBoardSparking);
        gameData.enemyScore += score;
        CanvasManager.Instance.Canvas.SetEnemyScore(gameData.enemyScore);
    }

    public void OnRestartingGame()
    {
        Init();
        ChangeState(GameState.PlayerTurn);
    }
}
