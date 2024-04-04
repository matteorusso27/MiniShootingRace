using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static GameSelectors;
using static Helpers;
public class StartCanvas : MonoBehaviour
{
    public Button       StartGameBtn;
    public Button       DifficultyBtn;

    void Start()
    {
        CanvasM.Canvas.gameObject.SetActive(false);
        CanvasM.RewardCanvas.gameObject.SetActive(false);

        StartGameBtn.onClick.AddListener(delegate () { StartGameLoop(); });
        DifficultyBtn.onClick.AddListener(delegate () {
            DIFFICULTY = DIFFICULTY == GameDifficulty.Normal ? GameDifficulty.High : GameDifficulty.Normal;
            DifficultyBtn.GetComponentInChildren<TMP_Text>().text = DIFFICULTY.ToString();
        });
    }

    public void StartGameLoop()
    {
        GameM.StartGame();
    }
}
