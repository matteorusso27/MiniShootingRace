using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using static Helpers;
public class CameraManager : Singleton<CameraManager>
{
    public CinemachineVirtualCamera Camera;

    public void Init(Transform go)
    {
        if (Camera == null)
            Camera = GameObject.FindGameObjectWithTag(StringTag(GameTag.VirtualCamera)).GetComponent<CinemachineVirtualCamera>();
        Camera.LookAt = go;
    }
}
