using UnityEngine;
using static Helpers;

public class QuadraticCurve
{
    public Vector3 StartingPoint;
    public Vector3 FinalPoint;
    public Vector3 ControlPoint;

    public QuadraticCurve(Vector3 startingPoint, Vector3 finalPoint)
    {
        StartingPoint = startingPoint;
        FinalPoint = finalPoint;
        var halfway = (StartingPoint + FinalPoint) / 2;

        ControlPoint = new Vector3(halfway.x, halfway.y + THROW_HEIGHT, halfway.z);
    }
    public Vector3 Evaluate(float t)
    {
        Vector3 ac = Vector3.Lerp(StartingPoint, ControlPoint, t);
        Vector3 bc = Vector3.Lerp(ControlPoint, FinalPoint, t);
        return Vector3.Lerp(ac, bc, t);
    }

    //Debug purposes
    private void OnDrawGizmos()
    {
        if (StartingPoint == null || FinalPoint == null || ControlPoint == null) return;

        for (int i = 0; i < 20; i++)
            Gizmos.DrawWireSphere(Evaluate(i / 20f), 0.1f);
    }
}