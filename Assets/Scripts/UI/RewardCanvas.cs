using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Helpers;
using static GameSelectors;
using TMPro;
using UnityEngine.SceneManagement;

public class RewardCanvas : MonoBehaviour
{
    public Button       RestartGameBtn;
    public Button       QuitGameBtn;
    public TMP_Text     RewardTxt;
    public void Setup(ResultGame result)
    {
        gameObject.SetActive(true);
        string finalTxt = "";
        if (result == ResultGame.Won)
        {
            finalTxt = "You Win\nYou got 50$";
        }
        else if (result == ResultGame.Lost)
        {
            finalTxt = "You lose";
        }
        else if (result == ResultGame.Tie)
        {
            finalTxt = "Tie\nYou got 25$";
        }
        RewardTxt.text = finalTxt;
        RestartGameBtn.onClick.AddListener(delegate () { SceneManager.LoadScene("MainScene"); ; });
        QuitGameBtn.onClick.AddListener(delegate () { Application.Quit(0) ; });
    }
}
