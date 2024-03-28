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
    public float           Speed = 1f; // todo needed?

    public bool            IsPlayerBallInMotion;
    public bool            IsEnemyBallInMotion;
    public                 GameObject[] Balls;
   
    public void Setup(Vector3 startingPoint, Vector3 finalPoint, bool isPlayerBall)
    {
        Balls = InstanceManager.Instance.GetBalls().Select(x=> x.gameObject).ToArray();

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
            CurrentTime  += Time.deltaTime;
            ball.position = curve.Evaluate(CurrentTime);
            ball.forward  = curve.Evaluate(CurrentTime + 0.001f) - ball.position;

            d = Vector3.Distance(ball.transform.position, curve.FinalPoint);
            yield return null;
        }

        CurrentTime = 0f;
        if (IsPlayerBall)
        {
            IsPlayerBallInMotion = false;
            if (GameManager.Instance.Game_variables.currentShoot == ShootType.BoardShoot)
            {
                ball.GetComponent<BallBase>().SimulatePhysicsMode();
            }
            else
                ball.GetComponent<BallBase>().StartReset();
        }
        else
        {
            //todo end motion to take physics
            IsEnemyBallInMotion = false;
        }
    }

    public GameObject GetBall(bool IsPlayerBall) => IsPlayerBall ? Balls[0] : Balls[1];
}
