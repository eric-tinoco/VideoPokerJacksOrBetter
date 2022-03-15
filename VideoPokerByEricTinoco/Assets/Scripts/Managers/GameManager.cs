using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static GameState currentState;
    public static event Action<GameState> OnGameStateChanged;
    public static int totalMoney = 10000;
    public static int currentBet = 0;

    [SerializeField] private List<Card> deckOfCards = new List<Card>();
    public List<Card> DeckOfCards { get => deckOfCards; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this.gameObject);
    }

    void Start()
    {
        UpdateGameState(GameState.Betting);
    }

    /// <summary>
    /// updates game state to new state and handle anything needing to handle on here
    /// also, invokes event for other scripts to update their scripts respectively
    /// </summary>
    /// <param name="newState"></param>
    public void UpdateGameState(GameState newState)
    {
        currentState = newState;

        switch (newState)
        {
            case GameState.Betting:
                HandleBettingState();
                break;
            case GameState.DrawRound:
                HandleDrawRoundState();
                break;
            case GameState.Results:
                HandleResultsState();
                break;
            case GameState.Win:
                HandleWinState();
                break;
            case GameState.Lose:
                HandleLoseState();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }

        OnGameStateChanged?.Invoke(newState);
    }

    private void HandleBettingState()
    {
        // do something for handling the betting state
    }

    private void HandleDrawRoundState()
    {
        // do something for handling the draw round state
    }

    private void HandleResultsState()
    {
        // do something for handling the results state
    }

    private void HandleWinState()
    {
        totalMoney += currentBet * HandManager.handTypeAndWinings[HandManager.resultHandType];
    }

    private void HandleLoseState()
    {
        totalMoney -= currentBet;
    }
}

public enum GameState
{
    Betting,
    DrawRound,
    Results,
    Win,
    Lose
}
