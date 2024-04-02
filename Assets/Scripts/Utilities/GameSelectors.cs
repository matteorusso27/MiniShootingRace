using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameSelectors 
{
    public static GameManager GameM => GameManager.Instance;
    public static InstanceManager InstanceM => InstanceManager.Instance;
    public static MotionManager MotionM => MotionManager.Instance;
    public static CanvasManager CanvasM => CanvasManager.Instance;
    public static SwipeManager SwipeM => SwipeManager.Instance;
    public static CameraManager CameraM => CameraManager.Instance;
}
