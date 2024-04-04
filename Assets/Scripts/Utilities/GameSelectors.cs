using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Handy way to handle the Managers
public static class GameSelectors 
{
    public static GameManager GameM => GameManager.Instance;
    public static InstanceManager InstanceM => InstanceManager.Instance;
    public static MotionManager MotionM => MotionManager.Instance;
    public static CanvasManager CanvasM => CanvasManager.Instance;
    public static SwipeManager SwipeM => SwipeManager.Instance;
    public static CameraManager CameraM => CameraManager.Instance;
    public static AudioSystem AudioM => AudioSystem.Instance;
}
