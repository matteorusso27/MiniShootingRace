using System;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public static event Action<GameState> OnBeforeStateChanged;
    public static event Action<GameState> OnAfterStateChanged;

    public GameState State { get; private set; }
    void Start()
    {
        ChangeState(GameState.Start);
        Debug.Log("Game Started");
    }

    private void ChangeState(GameState newState)
    {
        OnBeforeStateChanged?.Invoke(newState);

        State = newState;
        switch (newState)
        {
            case GameState.Start:
                HandleStart();
                break;
            case GameState.SpawningPlayer:
                HandleSpawningPlayer();
                break;
            case GameState.PlayerTurn:
                break;
            case GameState.End:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }

        OnAfterStateChanged?.Invoke(newState);
        Debug.Log("New State: " + newState);
    }

    private void HandleStart()
    {
        // Do things

        // Go to next state
        ChangeState(GameState.SpawningPlayer);
    }

    private void HandleSpawningPlayer()
    {
        InstanceManager.Instance.SpawnPlayerAndBall();
        // Do things

        // Go to next state
        ChangeState(GameState.PlayerTurn);
    }

    public enum GameState
    {
        Start = 0,
        SpawningPlayer = 1,
        PlayerTurn = 2,
        End = 3
    }
}
