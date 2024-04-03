using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Helpers
{
    public static float     TIME_TO_SWIPE = 0.25f;
    public static float     PLAYER_TURN_TIME = 5f;
    public static float     GRAVITY = 9.81f;
    public static float     MIN_SWIPE = 100f;
    public static float     RANGES_DISTANCE = 0.2f; //Distance between perfect range and board range
    public static float     SPARKING_BOARD_TIME = PLAYER_TURN_TIME / 2f;
    public static float     ENEMY_SHOOT_FREQUENCY = 1.5f;
    public static int       COUNTDOWN = 1;
    public static float     ENERGY_BAR_FILL = 0.5f;
    #region Camera
    public static float     CAMERA_FOV = 24f;
    public static float     DEFAULT_FOV = 40f;
    #endregion

    #region ScorePoints
    public static int       PERFECT_SHOOT_SCORE = 3;
    public static int       REGULAR_SHOOT_SCORE = 2;
    #endregion

    public static Vector3   HOOP_POSITION = new Vector3(0.27f, 7.62f, 5.92f);
    public static Vector3   BOARD_HIT_POSITION = new Vector3(0.43f, 10f, 5.88f);
    public static Vector3   MISS_BOARD_HIT_POSITION = new Vector3(3.43f, 10f, 6.35f);
    public static Vector3   LOW_THROW_POSITION = new Vector3(0.43f, 0f, 2.35f);
    public static Vector3   HIGH_THROW_POSITION = new Vector3(0f, 0f, 20);


    public static float THROW_HEIGHT = 15f;
    public static System.Random RANDOM = new System.Random();

    #region TAGS
    public enum GameTag
    {
        Terrain,
        Board,
        HoopTriggers,
        TriggerGravity,
        VirtualCamera
    }

    public static string StringTag(GameTag t) => t.ToString();

    #endregion
    public static int GetScore(ShootType shootType, bool isBoardSparking)
    {
        switch (shootType)
        {
            case ShootType.PerfectShoot:
                return 3;
            case ShootType.RegularShoot:
                return 2;
            case ShootType.BoardShoot:
                return isBoardSparking ? GetRandomNumber(4,5) : REGULAR_SHOOT_SCORE;
            default:
                return 0;
        }
    }
    #region SHOOT PARAMETERS
    
    public static float START_RANGE_PERFECT_SHOOT;
    public static float END_RANGE_PERFECT_SHOOT;

    public static float START_RANGE_BOARD_SHOOT;
    public static float END_RANGE_BOARD_SHOOT;

    public static float TOLERANCE = 0.02f;
    public static bool IsInsidePerfectRange(float value)
    {
        float adjustValueAtEnd = 0.01f;
        return value >= START_RANGE_PERFECT_SHOOT && value <= END_RANGE_PERFECT_SHOOT - adjustValueAtEnd;
    }

    public static bool IsRegularRange(float value)
    { 
        return value >= START_RANGE_PERFECT_SHOOT - TOLERANCE && value <= END_RANGE_PERFECT_SHOOT + TOLERANCE;
    }

    public static bool IsBoardShoot(float value)
    {
        return value >= START_RANGE_BOARD_SHOOT - TOLERANCE && value <= END_RANGE_BOARD_SHOOT + TOLERANCE;
    }
    public static bool IsScoreShoot(ShootType type) => type == ShootType.PerfectShoot || type == ShootType.RegularShoot || type == ShootType.BoardShoot; 
    public static ShootType GetShootType(float value)
    {
        if (IsInsidePerfectRange(value)) return ShootType.PerfectShoot;
        if (IsRegularRange(value)) return ShootType.RegularShoot;
        if (IsBoardShoot(value)) return ShootType.BoardShoot;
        return ShootType.FailedShoot;
    }

    public static ShootType GetRandomShootType() => (ShootType) GetRandomNumber(0, 3);
    
    public static void SetThrowHeight(ShootType shootype, float chargedValue)
    {
        switch (shootype)
        {
            case ShootType.PerfectShoot:
            case ShootType.RegularShoot:
            case ShootType.BoardShoot:
                THROW_HEIGHT = 15f;
                break;
            case ShootType.FailedShoot:
            default:
                if(chargedValue < START_RANGE_PERFECT_SHOOT)
                    THROW_HEIGHT = 5f;
                else
                    THROW_HEIGHT = 20;
                break;
        }
    }

    public enum ShootType
    {
        PerfectShoot,
        RegularShoot,
        BoardShoot,
        FailedShoot
    }

    public enum BallType
    {
        NormalBall,
        FireBall,
        BlueBall
    }

    public static int GetRandomNumber(int min, int max) => RANDOM.Next(min, max + 1);

    public static Vector3 GetFinalPosition(ShootType shootType, float value)
    {
        switch (shootType)
        {
            case ShootType.PerfectShoot:
            case ShootType.RegularShoot:
                return HOOP_POSITION;
            case ShootType.BoardShoot:
                return BOARD_HIT_POSITION;
            case ShootType.FailedShoot:
                if (value <= START_RANGE_PERFECT_SHOOT)
                    return LOW_THROW_POSITION;
                if (value <= START_RANGE_BOARD_SHOOT)
                    return MISS_BOARD_HIT_POSITION;
                else
                    return HIGH_THROW_POSITION;
            default:
                return Vector3.zero;
        }
        #endregion
    }

    public static bool IsPlayerBallOfType(BallType balltype) => GameManager.Instance.Data.PlayerBall.BallType == balltype;
}