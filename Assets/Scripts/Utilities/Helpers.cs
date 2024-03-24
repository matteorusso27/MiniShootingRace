using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Helpers
{
    public static float TIME_TO_SWIPE = 1.5f;
    public static float PLAYER_TURN_TIME = 25f;
    public static float GRAVITY = 9.8f;
    public static Vector3 HOOP_POSITION = new Vector3(0.25f, 8.732f, 5.43f);

    public enum GameTag
    {
        Terrain,
        Board
    }

    public static string StringTag(GameTag t) => t.ToString();
}
