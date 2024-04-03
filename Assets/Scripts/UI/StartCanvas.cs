using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static GameSelectors;
public class StartCanvas : MonoBehaviour
{
    public Button StartGameBtn;

    void Start()
    {
        StartGameBtn.onClick.AddListener(delegate () { StartGameLoop(); });
    }

    public void StartGameLoop()
    {
        GameM.StartGame();
    }
}
