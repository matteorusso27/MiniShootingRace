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
        public int PlayerScore; 
        public int EnemyScore; 
        public ShootType CurrentPlayerShoot;
        public ShootType CurrentEnemyShoot;
        public bool IsBoardSparking;
        public float ElapsedPlayerTime;
        public bool IsBallReady;
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
        gameData.PlayerScore = 0;
        gameData.EnemyScore = 0;
        CanvasManager.Instance.Canvas.SetPlayerScore(0);
        CanvasManager.Instance.Canvas.SetEnemyScore(0);
        CanvasManager.Instance.Canvas.FinalText.transform.gameObject.SetActive(false);
        CanvasManager.Instance.Canvas.RestartBtn.transform.gameObject.SetActive(false);
        CanvasManager.Instance.Canvas.CountDown.transform.gameObject.SetActive(true);
        SwipeManager.Instance.Setup();
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
            for (int countDownTime = COUNTDOWN; countDownTime > 0; countDownTime--)
            {
                CanvasManager.Instance.Canvas.SetCountDownTxt(countDownTime.ToString());
                yield return new WaitForSeconds(1f);
            }
            CanvasManager.Instance.Canvas.SetCountDownTxt("Start!");
            yield return new WaitForSeconds(1f);
            CanvasManager.Instance.Canvas.CountDown.transform.gameObject.SetActive(false);
        }
        
        gameData.ElapsedPlayerTime = 0f;
        SwipeManager.Instance.Setup();

        Coroutine swipeAgainCoroutine = null;
        var playerBall = InstanceManager.Instance.GetBall(IsPlayer:true);
        var enemyBall = InstanceManager.Instance.GetBall(IsPlayer:false);
        playerBall.name = "Playerball";
        enemyBall.name = "Enemyball";
        CanvasManager.Instance.SetupFillBar();
        playerBall.OnScoreUpdate += OnPlayerScoreUpdated;
        enemyBall.OnScoreUpdate += OnEnemyScoreUpdated;
        playerBall.OnResetBall += OnBallReset;
        CameraManager.Instance.Init(playerBall.transform);
        yield return StartCountDown();

        while (gameData.ElapsedPlayerTime < PLAYER_TURN_TIME)
        {
            playerBall.StartingPosition = new Vector3(GetRandomNumber(6, 8), 4, -3);
            enemyBall.StartingPosition = new Vector3(GetRandomNumber(-6, -4), 4, -3);
            gameData.ElapsedPlayerTime += Time.deltaTime;
            CanvasManager.Instance.Canvas.SetTime((int)(PLAYER_TURN_TIME - gameData.ElapsedPlayerTime));
            gameData.IsBallReady = playerBall.IsReady;
            if (SwipeManager.Instance.SwipeIsMeasured)
            {
                if (playerBall.IsReady)
                {
                    var normalizedValue = SwipeManager.Instance.normalizedDistance;
                    gameData.CurrentPlayerShoot = GetShootType(normalizedValue);
                    var finalPosition = GetFinalPosition(gameData.CurrentPlayerShoot, normalizedValue);
                    SetThrowHeight(gameData.CurrentPlayerShoot, normalizedValue);
                    playerBall.SetupMotionValues();
                    MotionManager.Instance.Setup(playerBall.transform.position, finalPosition, isPlayerBall:true);
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
                enemyBall.SetupMotionValues();
                MotionManager.Instance.Setup(enemyBall.transform.position, finalPositionEnemy, isPlayerBall: false);
                if (!MotionManager.Instance.IsEnemyBallInMotion)
                    MotionManager.Instance.StartMotion(IsPlayerBall: false);
            }

            yield return new WaitForEndOfFrame();
        }
        swipeAgainCoroutine = null;
        // Go to next state
        
        enemyBall.OnScoreUpdate -= OnEnemyScoreUpdated;
        playerBall.OnResetBall -= OnBallReset;
        ChangeState(GameState.End);
    }

    private IEnumerator HandleEnd()
    {
        var playerBall = InstanceManager.Instance.GetBall(IsPlayer: true);
        var enemyBall = InstanceManager.Instance.GetBall(IsPlayer: false);
        if (playerBall.IsInMovement)
            yield return new WaitForSeconds(4f);

        //Release delegate subscriptions
        playerBall.OnScoreUpdate -= OnPlayerScoreUpdated;
        enemyBall.OnScoreUpdate -= OnEnemyScoreUpdated;

        string s;
        if (gameData.PlayerScore > gameData.EnemyScore)
            s = "You Win";
        else if (gameData.PlayerScore < gameData.EnemyScore)
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

    private void OnBallReset()
    {
        // Check if board need to be highlighted
        var toChange = gameData.ElapsedPlayerTime >= SPARKING_BOARD_TIME;
        var board = GameObject.FindGameObjectWithTag(StringTag(GameTag.Board));
        board.GetComponent<MeshRenderer>().enabled = toChange;
        gameData.IsBoardSparking = toChange;
        CameraManager.Instance.Camera.m_Lens.FieldOfView = DEFAULT_FOV;
    }
    public void OnPlayerScoreUpdated()
    {
        var score = GetScore(gameData.CurrentPlayerShoot, gameData.IsBoardSparking);
        gameData.PlayerScore += score;
        CanvasManager.Instance.Canvas.SetPlayerScore(gameData.PlayerScore);
    }
    
    public void OnEnemyScoreUpdated()
    {
        var score = GetScore(gameData.CurrentEnemyShoot, gameData.IsBoardSparking);
        gameData.EnemyScore += score;
        CanvasManager.Instance.Canvas.SetEnemyScore(gameData.EnemyScore);
    }

    public void OnRestartingGame()
    {
        Init();
        ChangeState(GameState.PlayerTurn);
    }
}
