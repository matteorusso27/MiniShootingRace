using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class GameCanvas : MonoBehaviour
{
    //todo clean, they were made for debug purposes
    public Image            FillBar;
    public TMP_Text         playerTurnCountDown;
    public Image            FillMarker;
    public Image            PerfectRange;
    public Image            BoardRange;
    public Image            EnergyBar;
    public TMP_Text         PlayerScore;
    public TMP_Text         EnemyScore;
    public TMP_Text         FinalText;
    public TMP_Text         CountDown;
    public Button           RestartBtn;

    public float PerfectRangeHeight => PerfectRange.rectTransform.rect.height;
    public float BoardRangeHeight => BoardRange.rectTransform.rect.height;
    public float FillBarHeight => FillBar.rectTransform.rect.height;

    public void SetTime(int t)
    {
        playerTurnCountDown.text = "Time: "+t.ToString();
    }

    public void SetFillBar(float value)
    {
        FillBar.fillAmount = value;
    }

    public void SetPlayerScore(int s) => PlayerScore.text = "P Score: "+s.ToString();
    public void SetEnemyScore(int s) => EnemyScore.text = "E Score: "+s.ToString();
    public void SetFinalText(string s)
    {
        FinalText.transform.gameObject.SetActive(true);
        FinalText.text = s;
    }

    public void SetCountDownTxt(string c) => CountDown.text = c;
}
