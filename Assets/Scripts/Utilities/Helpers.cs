using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Helpers
{
    public static float     TIME_TO_SWIPE = 1.5f;
    public static float     PLAYER_TURN_TIME = 15f;
    public static float     GRAVITY = 9.8f;
    public static float     MIN_SWIPE = 0.2f;
    public static float     RANGES_DISTANCE = 0.2f;

    public static Vector3   HOOP_POSITION = new Vector3(0.25f, 8.732f, 5.43f);
    public static Vector3   BOARD_HIT_POSITION = new Vector3(0.43f, 10f, 6.35f);
    public enum GameTag
    {
        Terrain,
        Board,
        HoopTriggers
    }

    public static string StringTag(GameTag t) => t.ToString();

    public static int GetScore(ShootType shootType)
    {
        switch (shootType)
        {
            case ShootType.PerfectShoot:
                return 3;
            case ShootType.RegularShoot:
            case ShootType.BoardShoot:
                return 2;
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

    public static ShootType GetShootType(float value)
    {
        if (IsInsidePerfectRange(value)) return ShootType.PerfectShoot;
        if (IsRegularRange(value)) return ShootType.RegularShoot;
        if (IsBoardShoot(value)) return ShootType.BoardShoot;
        return ShootType.FailedShoot;
    }

    public enum ShootType
    {
        PerfectShoot,
        RegularShoot,
        BoardShoot,
        FailedShoot
    }
    #endregion
}
