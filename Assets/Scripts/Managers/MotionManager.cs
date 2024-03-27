using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Helpers;
public class MotionManager : Singleton<MotionManager>
{
    public QuadraticCurve  PlayerCurve;
    public QuadraticCurve  EnemyCurve;
    public float           CurrentTime;
    public float           Speed = 5f;

    public bool            IsPlayerBallInMotion;
    public bool            IsEnemyBallInMotion;
    public                 GameObject[] Balls;
   
    public void Setup(Vector3 startingPoint, Vector3 finalPoint, bool isPlayerBall)
    {
        Balls = InstanceManager.Instance._inGameObjects.Where(x => x.GetComponent<BallBase>() != null).ToArray();

        if (isPlayerBall)
            PlayerCurve = new QuadraticCurve(startingPoint, finalPoint);
        else
            EnemyCurve = new QuadraticCurve(startingPoint, finalPoint);
    }

    public void StartMotion(bool IsPlayerBall)
    {
        var coroutine = IsPlayerBall ? ParabolicMotion(PlayerCurve, IsPlayerBall) : ParabolicMotion(EnemyCurve, IsPlayerBall);
        StartCoroutine(coroutine);
    }
    public IEnumerator ParabolicMotion(QuadraticCurve curve, bool IsPlayerBall)
    {
        Transform ball = GetBall(IsPlayerBall).transform;
        if (IsPlayerBall) IsPlayerBallInMotion = true;
        else IsEnemyBallInMotion = true;
        var d = Vector3.Distance(curve.StartingPoint, curve.FinalPoint);
        while ( d >= 0.5)
        {
            CurrentTime  += Time.deltaTime * Speed;
            ball.position = curve.Evaluate(CurrentTime);
            ball.forward  = curve.Evaluate(CurrentTime + 0.001f) - ball.position;

            d = Vector3.Distance(ball.transform.position, curve.FinalPoint);
            yield return null;
        }
        Debug.Log("Finished");
        CurrentTime = 0f;
        if (IsPlayerBall) IsPlayerBallInMotion = false;
        else IsEnemyBallInMotion = false;
    }

    public GameObject GetBall(bool IsPlayerBall) => IsPlayerBall ? Balls[0] : Balls[1];
}
