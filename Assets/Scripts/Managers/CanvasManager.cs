using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
public class CanvasManager : Singleton<CanvasManager>
{
    public TMP_Text tmpPro;
    public Image fillBar;
    public TMP_Text swipeState;

    public void SetText(string text)
    {
        tmpPro.text = text;
    }

    public void SetSwipeStateText(string text)
    {
        swipeState.text = text;
    }

    public void SetFillBar(float value)
    {
        fillBar.fillAmount = value;
    }
}
