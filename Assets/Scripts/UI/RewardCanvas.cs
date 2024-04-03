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
    public void Setup(ResultGame result)
    {
        gameObject.SetActive(true);
        string s = "";
        if (result == ResultGame.Won)
            s = "You Win";
        else if (result == ResultGame.Lost)
            s = "You lose";
        else if (result == ResultGame.Tie)
            s = "Tie";
        ResultTxt.text = s;
        RestartGameBtn.onClick.AddListener(delegate () { GameM.StartGame(); });
    }
}
