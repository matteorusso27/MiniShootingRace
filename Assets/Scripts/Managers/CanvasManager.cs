using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
public class CanvasManager : Singleton<CanvasManager>
{
    public TMP_Text tmpPro;
    public Image fillBar;
    public TMP_Text swipeState;
    public TMP_Text gameState;
    public TMP_Text playerTurnCountDown;

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
}
