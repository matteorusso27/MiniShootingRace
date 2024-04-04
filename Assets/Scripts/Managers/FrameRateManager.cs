using System.Collections;
using System.Threading;
using UnityEngine;

// Cap the game to a certain amount of Fps
public class FrameRateManager : Singleton<FrameRateManager>
{
    private void Start()
    {
        //Application.targetFrameRate = Screen.currentResolution.refreshRate;
        Application.targetFrameRate = 60;
    }
}