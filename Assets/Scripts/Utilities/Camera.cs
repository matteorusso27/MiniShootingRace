using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class Camera : Singleton<Camera>
{
    void Update()
    {
        if (GameManager.Instance.State == GameManager.GameState.PlayerTurn)
        {
            GetComponent<CinemachineVirtualCamera>().LookAt = InstanceManager.Instance.GetBalls()[0].gameObject.transform;
        }
    }
}
