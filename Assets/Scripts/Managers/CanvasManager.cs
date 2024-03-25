using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
using static Helpers;

public class CanvasManager : Singleton<CanvasManager>
{
    [SerializeField]
    public Canvas Canvas;

    public void SetupFillBar()
    {
        CalculateRanges(difficulty: 6);
        DrawRanges();
    }

    public void CalculateRanges(int difficulty)
    {
        // todo should not be random but based on the court position, however is good to be a number 
        // between {2,3,4,5}
        System.Random rnd = new System.Random();
        var numb = rnd.Next(2, difficulty);
        START_RANGE_PERFECT_SHOOT = (1f / numb);
        END_RANGE_PERFECT_SHOOT = START_RANGE_PERFECT_SHOOT + Canvas.PerfectRange.rectTransform.rect.height / Canvas.fillBar.rectTransform.rect.height;

        START_RANGE_BOARD_SHOOT = START_RANGE_PERFECT_SHOOT + RANGES_DISTANCE;
        END_RANGE_BOARD_SHOOT = START_RANGE_BOARD_SHOOT + Canvas.BoardRange.rectTransform.rect.height / Canvas.fillBar.rectTransform.rect.height;
    }

    public void DrawRanges()
    {
        var startingPerfectRangePositionY = Canvas.fillBar.rectTransform.rect.height * START_RANGE_PERFECT_SHOOT;
        Canvas.PerfectRange.rectTransform.anchoredPosition = new Vector3(0, startingPerfectRangePositionY, 0);

        var startingBoardRangePositionY = Canvas.fillBar.rectTransform.rect.height * START_RANGE_BOARD_SHOOT;
        Canvas.BoardRange.rectTransform.anchoredPosition = new Vector3(0, startingBoardRangePositionY, 0);
    }
}
