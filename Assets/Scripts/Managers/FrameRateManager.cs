using System.Collections;
using System.Threading;
using UnityEngine;
public class FrameRateManager : Singleton<FrameRateManager>
{
    private void Start()
    {
        //Application.targetFrameRate = Screen.currentResolution.refreshRate;
        Application.targetFrameRate = 60;
    }
}