using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BettingManager : MonoBehaviour
{
    const string PLACE_YOUR_BETS_STRING = "PLACE YOUR BETS";
    const string DRAW_TURN_STRING = "CLICK WHICH CARDS YOU WANT TO HOLD";
    const string WIN_STRING = "YOU WON $";
    const string LOSE_STRING = "YOU LOST $";
    const string ZERO_BALANCE_STRING = "YOU RAN OUT OF MONEY...GAME OVER!";
    const string TOTAL_BET_STRING = "Total Bet: $";
    const string BALANCE_STRING = "Balance: $";
    const string MULTIPLY_STRING = "Multiply By x";

    [Header("Betting Buttons References")]
    [SerializeField] private Button multiplierButton;
    [SerializeField] private Button increaseBetButton;
    [SerializeField] private Button decreaseBetButton;

    [Header("Text References")]
    [SerializeField] private Text stateDescriptionText;
    [SerializeField] private Text baseBetText;
    [SerializeField] private Text totalBetAmountText;
    [SerializeField] private Text currentBalanceText;
    [SerializeField] private Text multiplierText;
    [SerializeField] private Text[] multiplyerValuesArray = new Text[5];

    [Header("Index Holder, Amount, & Multiplier Arrays")]
    [SerializeField] private int baseBetIndex = 0;
    [SerializeField] private int multiplierIndex = 0;
    [SerializeField] private int[] baseBetArray = new int[5];
    [SerializeField] private int[] multiplierArray = new int[5];

    [Header("Waiting Time for Couroutine")]
    [SerializeField] private int waitForSeconds = 3;

    private void Awake()
    {
        // subscribe to event when this script wakes up for the first time
        GameManager.OnGameStateChanged += GameManagerOnGameStateChanged;
    }

    private void OnDestroy()
    {
        // unsubscribe to event when this script is destroyed
        GameManager.OnGameStateChanged -= GameManagerOnGameStateChanged;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        // initialize buttons with listeners
        multiplierButton.onClick.AddListener(delegate { if (GameManager.currentState != GameState.Betting) return; UpdateMultiplyerIndex(); });
        increaseBetButton.onClick.AddListener(delegate { if (GameManager.currentState != GameState.Betting) return; UpdateBaseBetIndex(true); });
        decreaseBetButton.onClick.AddListener(delegate { if (GameManager.currentState != GameState.Betting) return; UpdateBaseBetIndex(false); });

        currentBalanceText.text = BALANCE_STRING + GameManager.totalMoney;
    }

    /// <summary>
    /// listens to specifc game state changes from the GameManager
    /// </summary>
    /// <param name="state"></param>
    private void GameManagerOnGameStateChanged(GameState state)
    {
        if (state == GameState.Betting)
        {
            // reset values
            UpdateMultiplierColumnColor(multiplierIndex, 0);
            baseBetIndex = 0;
            multiplierIndex = 0;

            // update text on UI
            multiplierText.text = MULTIPLY_STRING + multiplierArray[multiplierIndex];
            baseBetText.text = "$" + baseBetArray[baseBetIndex];

            // check to see if player still has money or not, set correct description on UI screen or quit application
            if (GameManager.totalMoney == 0)
            {
                stateDescriptionText.text = ZERO_BALANCE_STRING;
                StartCoroutine(QuitGameWhenOutOfMoney());
            }
            else
                stateDescriptionText.text = PLACE_YOUR_BETS_STRING;

            UpdateTotalBet();
        }

        if (state == GameState.DrawRound)
            stateDescriptionText.text = DRAW_TURN_STRING;

        if (state == GameState.Win || state == GameState.Lose)
        {
            if (state == GameState.Win)
            {
                AudioManager.Instance.PlaySoundEffect(SFXType.Winning);
                stateDescriptionText.text = WIN_STRING + GameManager.currentBet * HandManager.handTypeAndWinings[HandManager.resultHandType] + " FROM " + HandManager.resultHandType.ToString();
            }
            else if (state == GameState.Lose)
            {
                AudioManager.Instance.PlaySoundEffect(SFXType.Loosing);
                stateDescriptionText.text = LOSE_STRING + GameManager.currentBet;
            }

            currentBalanceText.text = BALANCE_STRING + GameManager.totalMoney;
            StartCoroutine(WaitForResultToBeSeen());
        }
    }

    /// <summary>
    /// basic couroutine to wait for so many seconds on win/lose state before moving on to betting state
    /// </summary>
    /// <returns></returns>
    IEnumerator WaitForResultToBeSeen()
    {
        yield return new WaitForSeconds(waitForSeconds);
        GameManager.Instance.UpdateGameState(GameState.Betting);
    }
    /// <summary>
    /// waits for seconds then quits game
    /// </summary>
    /// <returns></returns>
    IEnumerator QuitGameWhenOutOfMoney()
    {
        yield return new WaitForSeconds(waitForSeconds);
        Application.Quit();
    }

    /// <summary>
    /// updates multiplier index, multiplier text, calls UpdateMultiplierColumnColor, and total bet uppon multiplier button being pressed
    /// </summary>
    private void UpdateMultiplyerIndex()
    {
        // holds value of original base bet index in case player does not have enough money
        int indexHolder = multiplierIndex;

        if (multiplierIndex < 4) multiplierIndex++;
        else if (multiplierIndex >= 4) multiplierIndex = 0;

        if (!CheckIfTheresEnoughToBet(GetTotalBet()))
        {
            multiplierIndex = indexHolder;
            return;
        }

        AudioManager.Instance.PlaySoundEffect(SFXType.ButtonPressed);
        multiplierText.text = MULTIPLY_STRING + multiplierArray[multiplierIndex];
        UpdateMultiplierColumnColor(indexHolder, multiplierIndex);
        UpdateTotalBet();
    }

    /// <summary>
    /// updates color of text per multiplier column
    /// </summary>
    /// <param name="previousIndex"></param>
    /// <param name="newIndex"></param>
    private void UpdateMultiplierColumnColor(int previousIndex, int newIndex)
    {
        multiplyerValuesArray[previousIndex].color = Color.black;
        multiplyerValuesArray[newIndex].color = Color.red;
    }

    /// <summary>
    /// updates base bet index on increase or decrease buttons are pressed
    /// increaseBet, if true, increase button preseed, if false, decrease button pressed
    /// </summary>
    /// <param name="increaseBet"></param>
    private void UpdateBaseBetIndex(bool increaseBet)
    {
        // holds value of original base bet index in case player does not have enough money
        int indexHolder = baseBetIndex;

        if (increaseBet)
        {
            if (baseBetIndex < 4) baseBetIndex++;

            if (!CheckIfTheresEnoughToBet(GetTotalBet()))
            {
                baseBetIndex = indexHolder;
                return;
            }
        }
        else
        {
            if (baseBetIndex > 0) baseBetIndex--;

            if (!CheckIfTheresEnoughToBet(GetTotalBet()))
            {
                baseBetIndex = indexHolder;
                return;
            }
        }

        AudioManager.Instance.PlaySoundEffect(SFXType.ButtonPressed);
        baseBetText.text = "$" + baseBetArray[baseBetIndex];
        UpdateTotalBet();
    }

    /// <summary>
    /// updates total bet for static variable in game manager and changes text on the screen
    /// </summary>
    private void UpdateTotalBet()
    {
        GameManager.currentBet = GetTotalBet();
        totalBetAmountText.text = TOTAL_BET_STRING + GameManager.currentBet;
    }

    /// <summary>
    /// returns true if player has enough money, false if not
    /// </summary>
    /// <param name="total"></param>
    /// <returns></returns>
    private bool CheckIfTheresEnoughToBet(int total)
    {
        if (total > GameManager.totalMoney) return false;

        return true;
    }

    /// <summary>
    /// returns the total bet amount
    /// </summary>
    /// <returns></returns>
    private int GetTotalBet()
    {
        return GetBaseBet() * GetMultiplier();
    }

    /// <summary>
    /// returns the multiplier
    /// </summary>
    /// <returns></returns>
    private int GetMultiplier()
    {
        return multiplierArray[multiplierIndex];
    }

    /// <summary>
    /// returns the base bet
    /// </summary>
    /// <returns></returns>
    private int GetBaseBet()
    {
        return baseBetArray[baseBetIndex];
    }
}
