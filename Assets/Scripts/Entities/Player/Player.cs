using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Player : CharacterBase
{
    private bool canMove;
    private void Awake() => GameManager.OnBeforeStateChanged += OnStateChanged;
    private void OnDestroy() => GameManager.OnAfterStateChanged -= OnStateChanged;

    private void OnStateChanged(GameManager.GameState newState)
    {
        if (newState == GameManager.GameState.PlayerTurn)
            canMove = true;
        else
            canMove = false;
    }

    private void OnMouseDown()
    {
        if (GameManager.Instance.State != GameManager.GameState.PlayerTurn) return;
        if (!canMove)
        {
            Debug.Log("you cannot move");
            return;
        }

        Debug.Log("Clicked");
    }
    public virtual void ExecuteMove()
    {
        canMove = false;
    }

    public void Update()
    {
        if (!canMove) return;
    }
}
