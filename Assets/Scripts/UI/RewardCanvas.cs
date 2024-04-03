using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Helpers;
using static GameSelectors;
using TMPro;

public class RewardCanvas : MonoBehaviour
{
    public Button       RestartGameBtn;
    public TMP_Text     ResultTxt;
    public TMP_Text     RewardTxt;
    public void Setup(ResultGame result)
    {
        gameObject.SetActive(true);
        string finalTxt = "";
        string reward = "";
        if (result == ResultGame.Won)
        {
            finalTxt = "You Win";
            reward = "You got 50$";
        }
        else if (result == ResultGame.Lost)
        {
            finalTxt = "You lose";
        }
        else if (result == ResultGame.Tie)
        {
            finalTxt = "Tie";
            reward = "25$";
        }
        ResultTxt.text = finalTxt;
        RewardTxt.text = reward;
        RestartGameBtn.onClick.AddListener(delegate () { GameM.StartGame(); });
    }
}
