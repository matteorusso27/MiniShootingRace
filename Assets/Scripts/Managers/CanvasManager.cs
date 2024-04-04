using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
using static Helpers;

public class CanvasManager : Singleton<CanvasManager>
{
    [SerializeField]
    public GameCanvas   GameCanvas;
    public StartCanvas  StartCanvas;
    public RewardCanvas RewardCanvas;

    public void SetupGameFillBar()
    {
        CalculateGameRanges(DIFFICULTY == GameDifficulty.Normal ? 3 : 4);
        DrawRanges();
    }

    public void CalculateGameRanges(int difficulty)
    {
        // todo should not be random but based on the court position, however is good to be a number 
        // between {2,3,4}
        var numb = GetRandomNumber(2, difficulty);
        START_RANGE_PERFECT_SHOOT = (1f / numb);
        END_RANGE_PERFECT_SHOOT = START_RANGE_PERFECT_SHOOT + GameCanvas.PerfectRangeHeight / GameCanvas.FillBarHeight;

        START_RANGE_BOARD_SHOOT = START_RANGE_PERFECT_SHOOT + RANGES_DISTANCE;
        END_RANGE_BOARD_SHOOT = START_RANGE_BOARD_SHOOT + GameCanvas.BoardRangeHeight / GameCanvas.FillBarHeight;
    }

    public void DrawRanges()
    {
        var startingPerfectRangePositionY = GameCanvas.FillBarHeight * START_RANGE_PERFECT_SHOOT;
        GameCanvas.PerfectRange.rectTransform.anchoredPosition = new Vector3(0, startingPerfectRangePositionY, 0);

        var startingBoardRangePositionY = GameCanvas.FillBarHeight * START_RANGE_BOARD_SHOOT;
        GameCanvas.BoardRange.rectTransform.anchoredPosition = new Vector3(0, startingBoardRangePositionY, 0);
    }

    public void FillEnergyBar()
    {
        var currentAmount = GameCanvas.EnergyBar.fillAmount;
        if (currentAmount >= 1)
        {
            SetGameEnergyBar(1f);
        }
        else
        {
            GameCanvas.EnergyBar.fillAmount += ENERGY_BAR_FILL;
        } 
    }

    public void SetGameEnergyBar(float amount)
    {
        GameCanvas.EnergyBar.fillAmount = amount;
    }

    public float GetEnergyBarFill() => GameCanvas.EnergyBar.fillAmount;

    public void InitGameCanvas()
    {
        GameCanvas.gameObject.SetActive(true);
        GameCanvas.SetPlayerScore(0);
        GameCanvas.SetEnemyScore(0);
        GameCanvas.SetTime((int)PLAYER_TURN_TIME);
        GameCanvas.FinalText.transform.gameObject.SetActive(false);
        GameCanvas.CountDown.transform.gameObject.SetActive(true);

        SetupGameFillBar();
        SetGameEnergyBar(0f);
    }
}
