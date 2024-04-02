using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Helpers;
[CreateAssetMenu(fileName = "New ScriptableBallObject")]
public class ScriptableBallBase: ScriptableObject
{
    public BallType Ball;

    public BallBase prefab;
}
