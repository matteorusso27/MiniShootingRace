using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Helpers;
[CreateAssetMenu(fileName = "New ScriptableBallObject")]

// This is the base to create different types of scriptable object of ball type
public class ScriptableBallBase: ScriptableObject
{
    public BallType Ball;

    public BallBase prefab;
}
