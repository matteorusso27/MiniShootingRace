using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class Camera : Singleton<Camera>
{
    void Update()
    {
        var ball = InstanceManager.Instance.GetBalls()[0].gameObject.transform;
        if (ball != null)
        {
            GetComponent<CinemachineVirtualCamera>().LookAt = ball; 
        }
    }
}
