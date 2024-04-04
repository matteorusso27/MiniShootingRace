using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Helpers;
using static GameSelectors;
public class MotionManager : Singleton<MotionManager>
{
    public QuadraticCurve  PlayerCurve;
    public QuadraticCurve  EnemyCurve;
    public float           CurrentPlayerTime;
    public float           CurrentTimeEnemy;

    public bool            IsPlayerBallInMotion;
    public bool            IsEnemyBallInMotion;
    public                 GameObject[] Balls;
   
    public void Init(Vector3 startingPoint, Vector3 finalPoint, bool isPlayerBall)
    {
        Balls = InstanceM.GetBalls().Select(x=> x.gameObject).ToArray();

        if (isPlayerBall)
        {
            PlayerCurve = new QuadraticCurve(startingPoint, finalPoint);
            IsPlayerBallInMotion = false;
        }
        else
        {
            EnemyCurve = new QuadraticCurve(startingPoint, finalPoint);
            IsEnemyBallInMotion = false;
        }
    }

    public void StartMotion(bool IsPlayerBall)
    {
        var coroutine = IsPlayerBall ? ParabolicMotion(PlayerCurve, IsPlayerBall) : ParabolicMotion(EnemyCurve, IsPlayerBall);
        StartCoroutine(coroutine);
    }
    public IEnumerator ParabolicMotion(QuadraticCurve curve, bool IsPlayerBall)
    {
        Transform ball = GetBall(IsPlayerBall).transform;
        if (IsPlayerBall)
        {
            IsPlayerBallInMotion = true;
        }
        else
        {
            IsEnemyBallInMotion = true;
        }
        var distance = Vector3.Distance(curve.StartingPoint, curve.FinalPoint);
        var time = IsPlayerBall ? CurrentPlayerTime : CurrentTimeEnemy;
        var cameraFOV = CameraM.Camera.m_Lens.FieldOfView;
        while ( distance >= 0.5f)
        {
            time += Time.deltaTime;
            ball.position = curve.Evaluate(time);
            ball.forward  = curve.Evaluate(time + 0.001f) - ball.position;

            distance = Vector3.Distance(ball.transform.position, curve.FinalPoint);

            if (IsPlayerBall)
            {
                CameraM.Camera.m_Lens.FieldOfView = Mathf.Lerp(cameraFOV, CAMERA_FOV, time);
            }
            yield return null;
        }

        if (IsPlayerBall) IsPlayerBallInMotion = false;
        else IsEnemyBallInMotion = false;
        
        ball.GetComponent<BallBase>().SimulatePhysicsMode();
    }

    public GameObject GetBall(bool IsPlayerBall) => IsPlayerBall ? Balls[0] : Balls[1];
}
