using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Helpers
{
    public static float TIME_TO_SWIPE = 1.5f;
    public static float PLAYER_TURN_TIME = 5f;
    public static float GRAVITY = 9.8f;

    public enum GameTag
    {
        Terrain
    }

    public static string StringTag(GameTag t) => t.ToString();
}
