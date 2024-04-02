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
        public int  CurrentPlayerStreak;
        public BallBase PlayerBall;
        public BallBase EnemyBall;
        public Coroutine FireBallRoutine;
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
        gameData.PlayerBall = InstanceManager.Instance.GetBall(IsPlayer:true);
        gameData.EnemyBall = InstanceManager.Instance.GetBall(IsPlayer:false);
        gameData.PlayerBall.name = "Playerball";
        gameData.EnemyBall.name = "Enemyball";
        CanvasManager.Instance.SetupFillBar();
        gameData.PlayerBall.OnScoreUpdate += OnPlayerScoreUpdated;
        gameData.EnemyBall.OnScoreUpdate += OnEnemyScoreUpdated;
        gameData.PlayerBall.OnResetBall += OnBallReset;
        CameraManager.Instance.Init(gameData.PlayerBall.transform);
        CanvasManager.Instance.SetEnergyBar(0f);
        ChangePlayerBallTo(BallType.NormalBall);
        gameData.FireBallRoutine = null;
        yield return StartCountDown();

        while (gameData.ElapsedPlayerTime < PLAYER_TURN_TIME)
        {
            gameData.PlayerBall.StartingPosition = new Vector3(GetRandomNumber(6, 8), 4, -3);
            gameData.EnemyBall.StartingPosition = new Vector3(GetRandomNumber(-6, -4), 4, -3);
            gameData.ElapsedPlayerTime += Time.deltaTime;
            CanvasManager.Instance.Canvas.SetTime((int)(PLAYER_TURN_TIME - gameData.ElapsedPlayerTime));
            gameData.IsBallReady = gameData.PlayerBall.IsReady;
            if (SwipeManager.Instance.SwipeIsMeasured)
            {
                if (gameData.PlayerBall.IsReady)
                {
                    var normalizedValue = SwipeManager.Instance.normalizedDistance;
                    gameData.CurrentPlayerShoot = GetShootType(normalizedValue);
                    if (IsScoreShoot(gameData.CurrentPlayerShoot))
                    {
                        CanvasManager.Instance.FillEnergyBar();
                        if (CanvasManager.Instance.GetEnergyBarFill() >= 1f && gameData.FireBallRoutine == null)
                        {
                            gameData.FireBallRoutine = StartCoroutine(FireBall());
                            ChangePlayerBallTo(BallType.FireBall);
                        }
                    }
                    else
                    {
                        CanvasManager.Instance.SetEnergyBar(0f);
                        gameData.FireBallRoutine = null;
                    }

                    var finalPosition = GetFinalPosition(gameData.CurrentPlayerShoot, normalizedValue);
                    SetThrowHeight(gameData.CurrentPlayerShoot, normalizedValue);
                    gameData.PlayerBall.SetupMotionValues();
                    MotionManager.Instance.Setup(gameData.PlayerBall.transform.position, finalPosition, isPlayerBall:true);
                    MotionManager.Instance.StartMotion(IsPlayerBall: true);
                }
                // Then start again
                swipeAgainCoroutine = StartCoroutine(SwipeManager.Instance.CanSwipeAgain());
            }
            // Enemy ball behaviour
            if (gameData.EnemyBall.IsReady)
            {
                var shoot = GetRandomShootType();
                var finalPositionEnemy = GetFinalPosition(shoot, 0.5f); // todo 0.5 togliere
                gameData.EnemyBall.SetupMotionValues();
                MotionManager.Instance.Setup(gameData.EnemyBall.transform.position, finalPositionEnemy, isPlayerBall: false);
                if (!MotionManager.Instance.IsEnemyBallInMotion)
                    MotionManager.Instance.StartMotion(IsPlayerBall: false);
            }

            yield return new WaitForEndOfFrame();
        }
        swipeAgainCoroutine = null;
        // Go to next state
        gameData.FireBallRoutine = null;
        gameData.EnemyBall.OnScoreUpdate -= OnEnemyScoreUpdated;
        gameData.PlayerBall.OnResetBall -= OnBallReset;
        gameData.CurrentPlayerStreak = 0;
        ChangeState(GameState.End);
    }

    private IEnumerator HandleEnd()
    {
        if (gameData.PlayerBall.IsInMovement)
            yield return new WaitForSeconds(4f);

        //Release delegate subscriptions
        gameData.PlayerBall.OnScoreUpdate -= OnPlayerScoreUpdated;
        gameData.EnemyBall.OnScoreUpdate -= OnEnemyScoreUpdated;

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

        if (CanvasManager.Instance.GetEnergyBarFill() >= 1f && !IsPlayerBallOfType(BallType.FireBall))
        {
            ChangePlayerBallTo(BallType.FireBall);
        }
        
        if (IsPlayerBallOfType(BallType.FireBall) && CanvasManager.Instance.GetEnergyBarFill() <= 0f && gameData.FireBallRoutine != null)
        {
            ChangePlayerBallTo(BallType.NormalBall);
            gameData.FireBallRoutine = null;
        }
    }
    public void OnPlayerScoreUpdated()
    {
        var score = GetScore(gameData.CurrentPlayerShoot, gameData.IsBoardSparking);
        if (gameData.PlayerBall.BallType == BallType.FireBall)
        {
            score *= 2;
        }
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

    public void ChangePlayerBallTo(BallType type)
    {
        var path = type == BallType.NormalBall ? "Materials/NormalBallMat" : "Materials/FireBallMat";
        var mat = Resources.Load(path, typeof(Material)) as Material;
        gameData.PlayerBall.gameObject.GetComponent<MeshRenderer>().material = mat;
        gameData.PlayerBall.BallType = type;
    }

    public IEnumerator FireBall()
    {
        var time = 0f;
        while (CanvasManager.Instance.GetEnergyBarFill() > 0)
        {
            time += Time.deltaTime * 0.00005f;
            CanvasManager.Instance.SetEnergyBar(CanvasManager.Instance.GetEnergyBarFill() - time);
            yield return null;
        }
        ChangePlayerBallTo(BallType.NormalBall);
    }
}
