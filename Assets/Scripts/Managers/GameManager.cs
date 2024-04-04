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

    #region GAME MANAGER VARIABLES
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

    #region STATE MANAGER
    public enum GameState
    {
        StartPage = 0,
        Start = 1,
        SpawningPlayer = 2,
        PlayerTurn = 3,
        End = 4
    }
    public void Start() => ChangeState(GameState.StartPage);
    public void StartGame() => ChangeState(GameState.Start);
    private void ChangeState(GameState newState)
    {
        OnBeforeStateChanged?.Invoke(newState);
        State = newState;
        switch (newState)
        {
            case GameState.StartPage:
                EnableStartPage();
                break;
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
    private void EnableStartPage() => CanvasM.StartCanvas.gameObject.SetActive(true);
    private void HandleStart()
    {
        CanvasM.StartCanvas.gameObject.SetActive(false);
        CanvasM.RewardCanvas.gameObject.SetActive(false);
        ChangeState(GameState.SpawningPlayer);
    }

    private void HandleSpawningPlayer()
    {
        InstanceM.SpawnPlayerAndBalls();
        Init();

        ChangeState(GameState.PlayerTurn);
    }
    private IEnumerator HandlePlayerTurn()
    {
        IEnumerator StartCountDown()
        {
            for (int countDownTime = COUNTDOWN; countDownTime > 0; countDownTime--)
            {
                CanvasM.GameCanvas.SetCountDownTxt(countDownTime.ToString());
                yield return new WaitForSeconds(0.5f);
            }
            CanvasM.GameCanvas.SetCountDownTxt("Start!");
            yield return new WaitForSeconds(1f);
            CanvasM.GameCanvas.CountDown.transform.gameObject.SetActive(false);
        }

        yield return StartCountDown();

        // Main game loop
        while (Data.ElapsedPlayerTime < PLAYER_TURN_TIME)
        {
            Data.ElapsedPlayerTime += Time.deltaTime;
            CanvasM.GameCanvas.SetTime((int)(PLAYER_TURN_TIME - Data.ElapsedPlayerTime));

            // Handle starting positions
            var startingZ = GetRandomNumber(-3, -1);
            Data.PlayerBall.StartingPosition = new Vector3(GetRandomNumber(6, 8), 4, startingZ); 
            Data.EnemyBall.StartingPosition = new Vector3(GetRandomNumber(-6, -4), 4, startingZ);

            HandlePlayerBall();
            HandleEnemyBall();

            yield return new WaitForEndOfFrame();
        }
        ResetPlayerTurn();
        ChangeState(GameState.End);
    }

    private IEnumerator HandleEnd()
    {
        if (Data.PlayerBall.IsInMovement)
            yield return new WaitForSeconds(4f);

        //Release delegate subscriptions
        Data.PlayerBall.OnScoreUpdate -= OnPlayerScoreUpdated;
        Data.EnemyBall.OnScoreUpdate -= OnEnemyScoreUpdated;
        Data.PlayerBall.OnTriggerPhysics -= OnSimulationMode;
        Data.EnemyBall.OnTriggerPhysics -= OnSimulationMode;

        var result = ResultGame.None;

        if (Data.PlayerScore < Data.EnemyScore) result = ResultGame.Lost;
        else if (Data.PlayerScore > Data.EnemyScore) result = ResultGame.Won;
        else result = ResultGame.Tie;

        CanvasM.GameCanvas.gameObject.SetActive(false);
        CanvasM.RewardCanvas.Setup(result);
    }
    #endregion

    #region Initialization
    private void Init()
    {
        InitGameData();
        CanvasM.InitGameCanvas();

        SwipeM.Init();
        CameraM.Init(Data.PlayerBall.transform);
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
        Data.PlayerBall.OnTriggerPhysics += OnSimulationMode;
        Data.EnemyBall.OnTriggerPhysics += OnSimulationMode;

        Data.FireBallRoutine = null;
        ChangePlayerBallTo(BallType.NormalBall);
        InstanceM.DestroyFire();

        AudioM.Setup();
        BoardSparkingTo(false);
    }

    #endregion

    private void HandlePlayerBall()
    {
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
                MotionM.Init(Data.PlayerBall.transform.position, finalPosition, isPlayerBall: true);
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
            var finalPositionEnemy = GetFinalPosition(shoot, GetFloatRandomNumber(0,1));
            Data.EnemyBall.SetupMotionValues();
            MotionM.Init(Data.EnemyBall.transform.position, finalPositionEnemy, isPlayerBall: false);
            if (!MotionManager.Instance.IsEnemyBallInMotion)
                MotionManager.Instance.StartMotion(IsPlayerBall: false);
        }
    }
    
    private void ResetPlayerTurn()
    {
        Data.SwipeAgainRoutine = null;
        Data.FireBallRoutine = null;
        Data.EnemyBall.OnScoreUpdate -= OnEnemyScoreUpdated;
        Data.PlayerBall.OnResetBall -= OnBallReset;

        Data.PlayerBall.OnTriggerPhysics -= OnSimulationMode;
        Data.EnemyBall.OnTriggerPhysics -= OnSimulationMode;
    }

    private void CheckSparkingBoard()
    {
        var toChange = !InstanceM.BoardVFX.activeInHierarchy && GetRandomNumber(0,10) < 3;
        BoardSparkingTo(toChange);
        Data.IsBoardSparking = toChange;
    }
    private void OnBallReset()
    {
        CheckSparkingBoard();
        CheckForFireBall();

        CameraM.Camera.m_Lens.FieldOfView = DEFAULT_FOV;
    }

    private void OnSimulationMode(bool isPlayer)
    {
        if (isPlayer && Data.CurrentPlayerShoot == ShootType.BoardShoot)
            AudioM.PlayBoardSound();
        if (!isPlayer && Data.CurrentEnemyShoot == ShootType.BoardShoot)
            AudioM.PlayBoardSound();
    }
    public void OnPlayerScoreUpdated()
    {
        AudioM.PlayBasketBallSound();
        var score = GetScore(Data.CurrentPlayerShoot, Data.IsBoardSparking);
        if (Data.PlayerBall.BallType == BallType.FireBall)
        {
            score *= 2;
        }
        Data.PlayerScore += score;
        CanvasM.GameCanvas.SetPlayerScore(Data.PlayerScore);

        if (IsScoreShoot(Data.CurrentPlayerShoot))
        {
            CanvasM.FillEnergyBar();
            if (Data.CurrentPlayerShoot == ShootType.BoardShoot && InstanceM.BoardVFX.activeInHierarchy)
                BoardSparkingTo(false);
        }
        else
        {
            CanvasM.SetGameEnergyBar(0f);
        }
    }
    
    public void OnEnemyScoreUpdated()
    {
        var score = GetScore(Data.CurrentEnemyShoot, Data.IsBoardSparking);
        Data.EnemyScore += score;
        if (Data.CurrentEnemyShoot == ShootType.BoardShoot && InstanceM.BoardVFX.activeInHierarchy)
            BoardSparkingTo(false);
        CanvasM.GameCanvas.SetEnemyScore(Data.EnemyScore);
    }

    public void ChangePlayerBallTo(BallType type)
    {
        if (Data.PlayerBall.BallType == type) return;
        var path = type == BallType.NormalBall ? "Materials/Regular/NormalBallMat" : "Materials/Regular/FireBallMat";
        var mat = Resources.Load(path, typeof(Material)) as Material;
        Data.PlayerBall.gameObject.GetComponent<MeshRenderer>().material = mat;
        Data.PlayerBall.BallType = type;
    }

    public IEnumerator FireBall()
    {
        CanvasM.GameCanvas.ChangeFireBallTxt(true);
        AudioM.PlayFireSound();
        var time = 0f;
        while (CanvasManager.Instance.GetEnergyBarFill() > 0)
        {
            time += Time.deltaTime * FIRE_BALL_SPEED_TIME;
            CanvasManager.Instance.SetGameEnergyBar(CanvasManager.Instance.GetEnergyBarFill() - time);
            yield return null;
        }
        if (Data.PlayerBall.BallType != BallType.NormalBall)
        {
            ChangePlayerBallTo(BallType.NormalBall);
            InstanceM.DestroyFire();
            AudioM.StopFireSound();
        }
        CanvasM.GameCanvas.ChangeFireBallTxt(false);
    }

    private void BoardSparkingTo(bool toChange)
    {
        InstanceM.BoardVFX.SetActive(toChange);
        CanvasM.GameCanvas.ChangeBoardTxt(toChange);
    }

    private void CheckForFireBall()
    {
        if (IsScoreShoot(Data.CurrentPlayerShoot))
        {
            if (CanvasM.GetEnergyBarFill() >= 1f && Data.FireBallRoutine == null)
            {
                Data.FireBallRoutine = StartCoroutine(FireBall());
                ChangePlayerBallTo(BallType.FireBall);
                InstanceM.SpawnFire();
            }
        }
        else
        {
            CanvasM.SetGameEnergyBar(0f);
            Data.FireBallRoutine = null;
            ChangePlayerBallTo(BallType.NormalBall);
            InstanceM.DestroyFire();
            AudioM.StopFireSound();
        }
    }
}
