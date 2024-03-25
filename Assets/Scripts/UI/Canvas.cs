using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class Canvas : MonoBehaviour
{
    public TMP_Text         tmpPro;
    public Image            fillBar;
    public TMP_Text         swipeState;
    public TMP_Text         gameState;
    public TMP_Text         playerTurnCountDown;
    public TMP_Text         ballState;
    public Image            FillMarker;
    public Image            PerfectRange;
    public Image            BoardRange;
    public TMP_Text         Score;

    public void SetText(string text)
    {
        tmpPro.text = text;
    }

    public void SetSwipeStateText(string text)
    {
        swipeState.text = text;
    }

    public void SetTime(int t)
    {
        playerTurnCountDown.text = t.ToString();
    }

    public void SetFillBar(float value)
    {
        fillBar.fillAmount = value;
    }

    public void SetGameState(string t) => gameState.text = t;
    public void SetBallState(string t) => ballState.text = t;

    public void SetScore(int s) => Score.text = "Score: "+s.ToString();
}
