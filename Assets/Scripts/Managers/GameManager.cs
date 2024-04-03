using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using static Helpers;
using static GameSelectors;
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
        
        public float ElapsedPlayerTime;
        
        public ShootType CurrentPlayerShoot;
        public ShootType CurrentEnemyShoot;
        
        public bool IsBoardSparking;
        public bool IsBallReady;
        
        public BallBase PlayerBall;
        public BallBase EnemyBall;

        public Coroutine FireBallRoutine;
        public Coroutine SwipeAgainRoutine;
    }

    public GameData Data;
    #endregion

    public void StartGame()
    {
        ChangeState(GameState.Start);
    }
    private void ChangeState(GameState newState)
    {
        OnBeforeStateChanged?.Invoke(newState);
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

    private void InitGameData()
    {
        Data.PlayerScore = 0;
        Data.EnemyScore = 0;
        Data.ElapsedPlayerTime = 0f;
        Data.PlayerBall = InstanceM.GetBall(IsPlayer: true);
        Data.EnemyBall = InstanceM.GetBall(IsPlayer: false);
        Data.PlayerBall.name = "Playerball";
        Data.EnemyBall.name = "Enemyball";

        Data.SwipeAgainRoutine = null;

        Data.IsBallReady = Data.PlayerBall.IsReady;

        Data.PlayerBall.OnScoreUpdate += OnPlayerScoreUpdated;
        Data.EnemyBall.OnScoreUpdate += OnEnemyScoreUpdated;
        Data.PlayerBall.OnResetBall += OnBallReset;

        Data.FireBallRoutine = null;
        ChangePlayerBallTo(BallType.NormalBall);
    }

    private void InitGameUI()
    {
        CanvasM.Canvas.gameObject.SetActive(true);
        CanvasM.Canvas.SetPlayerScore(0);
        CanvasM.Canvas.SetEnemyScore(0);
        CanvasM.Canvas.SetTime((int)PLAYER_TURN_TIME);
        CanvasM.Canvas.FinalText.transform.gameObject.SetActive(false);
        CanvasM.Canvas.RestartBtn.transform.gameObject.SetActive(false);
        CanvasM.Canvas.CountDown.transform.gameObject.SetActive(true);

        CanvasM.SetupFillBar();
        CanvasM.SetEnergyBar(0f);
    }
    private void Init()
    {
        InitGameData();
        InitGameUI();

        //Setup managers
        SwipeM.Setup();
        CameraM.Init(Data.PlayerBall.transform);
    }
    private void HandleStart()
    {
        CanvasM.StartCanvas.gameObject.SetActive(false);
        ChangeState(GameState.SpawningPlayer);
    }

    private void HandleSpawningPlayer()
    {
        InstanceM.SpawnPlayerAndBalls();
        Init();

        ChangeState(GameState.PlayerTurn);
    }

    private void HandlePlayerBall()
    {
        // Handle starting positions
        Data.PlayerBall.StartingPosition = new Vector3(GetRandomNumber(6, 8), 4, -3); //todo
        Data.EnemyBall.StartingPosition = new Vector3(GetRandomNumber(-6, -4), 4, -3);

        if (SwipeM.SwipeIsMeasured)
        {
            if (Data.PlayerBall.IsReady)
            {
                // Get value based on the swipe
                var normalizedValue = SwipeM.normalizedDistance;

                // Extract data based on the normalized value
                Data.CurrentPlayerShoot = GetShootType(normalizedValue);
                var finalPosition = GetFinalPosition(Data.CurrentPlayerShoot, normalizedValue);
                SetThrowHeight(Data.CurrentPlayerShoot, normalizedValue);

                // Init and start the parabolic movement
                Data.PlayerBall.SetupMotionValues();
                MotionM.Setup(Data.PlayerBall.transform.position, finalPosition, isPlayerBall: true);
                MotionM.StartMotion(IsPlayerBall: true);
            }
            // Swipe is available once again
            Data.SwipeAgainRoutine = StartCoroutine(SwipeManager.Instance.CanSwipeAgain());
        }
    }

    private void HandleEnemyBall()
    {
        if (Data.EnemyBall.IsReady)
        {
            var shoot = GetRandomShootType();
            var finalPositionEnemy = GetFinalPosition(shoot, 0.5f); // todo 0.5 togliere
            Data.EnemyBall.SetupMotionValues();
            MotionManager.Instance.Setup(Data.EnemyBall.transform.position, finalPositionEnemy, isPlayerBall: false);
            if (!MotionManager.Instance.IsEnemyBallInMotion)
                MotionManager.Instance.StartMotion(IsPlayerBall: false);
        }
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
            CanvasM.Canvas.SetCountDownTxt("Start!");
            yield return new WaitForSeconds(1f);
            CanvasM.Canvas.CountDown.transform.gameObject.SetActive(false);
        }

        yield return StartCountDown();

        // Main game loop
        while (Data.ElapsedPlayerTime < PLAYER_TURN_TIME)
        {
            // Update time
            Data.ElapsedPlayerTime += Time.deltaTime;
            CanvasM.Canvas.SetTime((int)(PLAYER_TURN_TIME - Data.ElapsedPlayerTime));

            HandlePlayerBall();
            HandleEnemyBall();

            yield return new WaitForEndOfFrame();
        }
        ResetPlayerTurn();
        ChangeState(GameState.End);
    }

    private void ResetPlayerTurn()
    {
        Data.SwipeAgainRoutine = null;
        Data.FireBallRoutine = null;
        Data.EnemyBall.OnScoreUpdate -= OnEnemyScoreUpdated;
        Data.PlayerBall.OnResetBall -= OnBallReset;
    }

    private IEnumerator HandleEnd()
    {
        if (Data.PlayerBall.IsInMovement)
            yield return new WaitForSeconds(4f);

        //Release delegate subscriptions
        Data.PlayerBall.OnScoreUpdate -= OnPlayerScoreUpdated;
        Data.EnemyBall.OnScoreUpdate -= OnEnemyScoreUpdated;

        string s;
        if (Data.PlayerScore > Data.EnemyScore)
            s = "You Win";
        else if (Data.PlayerScore < Data.EnemyScore)
            s = "You lose";
        else
            s = "Tie";
        CanvasM.Canvas.SetFinalText(s);
        CanvasM.Canvas.RestartBtn.transform.gameObject.SetActive(true);
    }

    public enum GameState
    {
        Start = 0,
        SpawningPlayer = 1,
        PlayerTurn = 2,
        End = 3
    }

    private void CheckSparkingBoard()
    {
        var toChange = Data.ElapsedPlayerTime >= SPARKING_BOARD_TIME;
        var board = GameObject.FindGameObjectWithTag(StringTag(GameTag.Board));
        board.GetComponent<MeshRenderer>().enabled = toChange;
        Data.IsBoardSparking = toChange;
    }
    private void OnBallReset()
    {
        // Check if board need to be highlighted
        CheckSparkingBoard();
        CameraM.Camera.m_Lens.FieldOfView = DEFAULT_FOV;

        if (IsScoreShoot(Data.CurrentPlayerShoot))
        {
            CanvasM.FillEnergyBar();
            if (CanvasM.GetEnergyBarFill() >= 1f && Data.FireBallRoutine == null)
            {
                Data.FireBallRoutine = StartCoroutine(FireBall());
                ChangePlayerBallTo(BallType.FireBall);
            }
        }
        else
        {
            CanvasM.SetEnergyBar(0f);
            Data.FireBallRoutine = null;
            ChangePlayerBallTo(BallType.NormalBall);
        }
    }
    public void OnPlayerScoreUpdated()
    {
        var score = GetScore(Data.CurrentPlayerShoot, Data.IsBoardSparking);
        if (Data.PlayerBall.BallType == BallType.FireBall)
        {
            score *= 2;
        }
        Data.PlayerScore += score;
        CanvasM.Canvas.SetPlayerScore(Data.PlayerScore);
    }
    
    public void OnEnemyScoreUpdated()
    {
        var score = GetScore(Data.CurrentEnemyShoot, Data.IsBoardSparking);
        Data.EnemyScore += score;
        CanvasM.Canvas.SetEnemyScore(Data.EnemyScore);
    }

    //Called by the button
    public void OnRestartingGame()
    {
        Init();
        ChangeState(GameState.PlayerTurn);
    }

    public void ChangePlayerBallTo(BallType type)
    {
        var path = type == BallType.NormalBall ? "Materials/NormalBallMat" : "Materials/FireBallMat";
        var mat = Resources.Load(path, typeof(Material)) as Material;
        Data.PlayerBall.gameObject.GetComponent<MeshRenderer>().material = mat;
        Data.PlayerBall.BallType = type;
    }

    public IEnumerator FireBall()
    {
        var time = 0f;
        while (CanvasManager.Instance.GetEnergyBarFill() > 0)
        {
            time += Time.deltaTime * 0.00005f; //todo I don't like this value
            CanvasManager.Instance.SetEnergyBar(CanvasManager.Instance.GetEnergyBarFill() - time);
            yield return null;
        }
        ChangePlayerBallTo(BallType.NormalBall);
    }
}
