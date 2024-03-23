using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New ScriptableBallObject")]
public class ScriptableBallBase: ScriptableObject
{
    public BallType Ball;

    public BallBase prefab;
    
    public enum BallType
    {
        NormalBall,
        FireBall,
        BlueBall
    }
}
