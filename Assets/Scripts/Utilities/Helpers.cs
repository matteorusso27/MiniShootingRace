using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Helpers
{
    public static float TIME_TO_SWIPE = 1.5f;
    public static float PLAYER_TURN_TIME = 10f;
    public static float GRAVITY = 9.8f;
    public static Vector3 HOOP_POSITION = new Vector3(0.25f, 8.732f, 5.43f);
    public static float MIN_SWIPE = 0.2f;
    public enum GameTag
    {
        Terrain,
        Board
    }

    public static string StringTag(GameTag t) => t.ToString();

    #region SHOOT PARAMETERS
    public static void CalculatePerfectRange(int difficulty)
    {
        System.Random rnd = new System.Random();
        var numb = rnd.Next(2, difficulty);
        START_RANGE_PERFECT_SHOOT = (1f / numb);
        END_RANGE_PERFECT_SHOOT = START_RANGE_PERFECT_SHOOT + CanvasManager.Instance.Canvas.PerfectRange.rectTransform.rect.height / CanvasManager.Instance.Canvas.fillBar.rectTransform.rect.height;
    }

    public static float START_RANGE_PERFECT_SHOOT;
    public static float END_RANGE_PERFECT_SHOOT;

    public static bool IsInsidePerfectRange(float value)
    {
        float adjustValueAtEnd = 0.01f;
        return value >= START_RANGE_PERFECT_SHOOT && value <= END_RANGE_PERFECT_SHOOT - adjustValueAtEnd;
    }

    public static bool IsRegularRange(float value)
    {
        float tolerance = 0.05f;
        return value >= START_RANGE_PERFECT_SHOOT - tolerance && value <= END_RANGE_PERFECT_SHOOT + tolerance;
    }

    public static ShootType GetShootType(float value)
    {
        if (IsInsidePerfectRange(value)) return ShootType.PerfectShoot;
        if (IsRegularRange(value)) return ShootType.RegularShoot;
        //todo board shoot
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
