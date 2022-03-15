using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandManager : MonoBehaviour
{
    public static HandType resultHandType = HandType.AllOther;
    public static Dictionary<HandType, int> handTypeAndWinings = new Dictionary<HandType, int>(){
        {HandType.RoyalFlush, 800},
        {HandType.StraightFlush, 50},
        {HandType.FourOfAKind, 25},
        {HandType.FullHouse, 9},
        {HandType.Flush, 6},
        {HandType.Straight, 4},
        {HandType.ThreeOfAKind, 3},
        {HandType.TwoPair, 2},
        {HandType.JacksOrBetter, 1},
        {HandType.AllOther, 0},
    };

    const int MAX_CARDS_IN_HAND = 5;
    const string DEAL_STRING = "Deal";
    const string DRAW_STRING = "Draw";

    [Header("Hand Buttons")]
    [SerializeField] private Button[] cardButton;

    [Header("Held Card Images")]
    [SerializeField] private Image[] heldImage;

    [Header("Options Buttons")]
    [SerializeField] private Button dealHandButton;
    [SerializeField] private Text dealHandText;

    [Header("Deck/DealtCards/HeldCards/PlayerHand References")]
    [SerializeField] private List<Card> deckList;
    [SerializeField] private List<Card> cardsDealtList;
    [SerializeField] private List<int> heldCardsList;
    [SerializeField] private Card[] currentHand = new Card[MAX_CARDS_IN_HAND];

    private void Awake()
    {
        GameManager.OnGameStateChanged += GameManagerOnGameStateChanged;
    }

    private void OnDestroy()
    {
        GameManager.OnGameStateChanged -= GameManagerOnGameStateChanged;
    }

    void Start()
    {
        // initialize buttons with listeners
        cardButton[0].onClick.AddListener(delegate { if (GameManager.currentState != GameState.DrawRound) return; HoldCard(0); });
        cardButton[1].onClick.AddListener(delegate { if (GameManager.currentState != GameState.DrawRound) return; HoldCard(1); });
        cardButton[2].onClick.AddListener(delegate { if (GameManager.currentState != GameState.DrawRound) return; HoldCard(2); });
        cardButton[3].onClick.AddListener(delegate { if (GameManager.currentState != GameState.DrawRound) return; HoldCard(3); });
        cardButton[4].onClick.AddListener(delegate { if (GameManager.currentState != GameState.DrawRound) return; HoldCard(4); });
        dealHandButton.onClick.AddListener(delegate {
            if (GameManager.currentState == GameState.Betting || GameManager.currentState == GameState.DrawRound)
                DealCards();
        });

        deckList = new List<Card>(GameManager.Instance.DeckOfCards);
        cardsDealtList = new List<Card>();
        heldCardsList = new List<int>();
    }

    /// <summary>
    /// listens to specifc game state changes from the GameManager
    /// </summary>
    /// <param name="state"></param>
    private void GameManagerOnGameStateChanged(GameState state)
    {
        if (state == GameState.Betting)
        {
            // shuffle deck
            ShuffleDeck();

            // update deal button from draw to deal
            dealHandText.text = DEAL_STRING;

            // remove held image above card from previuos game
            for (int i = 0; i < MAX_CARDS_IN_HAND; i++)
            {
                if (heldImage[i].gameObject.activeInHierarchy)
                    heldImage[i].gameObject.SetActive(false);
            }
        }

        // update deal button from deal to draw
        if (state == GameState.DrawRound)
        {
            dealHandText.text = DRAW_STRING;
        }


        // update balance after winning or losing
        if (state == GameState.Results)
        {
            if (CheckHandForResults())
                GameManager.Instance.UpdateGameState(GameState.Win);
            else
                GameManager.Instance.UpdateGameState(GameState.Lose);
        }
    }

    /// <summary>
    /// Can only be called on GameState.Betting & GameState.DrawCard
    /// </summary>
    private void DealCards()
    {
        // return if player has no money or doesn't have enough money for the next bet
        if (GameManager.totalMoney == 0 || GameManager.totalMoney < GameManager.currentBet) return;

        // play button pressed sound
        AudioManager.Instance.PlaySoundEffect(SFXType.ButtonPressed);

        int randomIndexByDecklist = 0;
        Card randomCard = null;

        for (int i = 0; i < MAX_CARDS_IN_HAND; i++)
        {
            // skips over cards that are chosen to be held
            if (heldCardsList.Contains(i)) continue;

            // get random index from current decklist count
            randomIndexByDecklist = Random.Range(0, deckList.Count);

            // get random card, add to hand, change image to current card
            randomCard = deckList[randomIndexByDecklist];
            currentHand[i] = randomCard;
            cardButton[i].image.sprite = randomCard.cardSprite;

            // remove card from deck and add to cards dealt list
            deckList.Remove(randomCard);
            cardsDealtList.Add(randomCard);
        }

        if (GameManager.currentState == GameState.Betting)
            GameManager.Instance.UpdateGameState(GameState.DrawRound);
        else if (GameManager.currentState == GameState.DrawRound)
            GameManager.Instance.UpdateGameState(GameState.Results);
    }

    /// <summary>
    /// When on GameState.DrawCard
    /// buttons clicks allow to hold specific cards adding them to a held cards list
    /// activates and deactivates hold image accordingly
    /// </summary>
    /// <param name="imageIndex"></param>
    private void HoldCard(int imageIndex)
    {
        Image img = heldImage[imageIndex];

        if (!img.gameObject.activeInHierarchy)
        {
            img.gameObject.SetActive(true);
            heldCardsList.Add(imageIndex);
        }
        else {
            img.gameObject.SetActive(false);
            heldCardsList.Remove(imageIndex);
        }
    }

    /// <summary>
    /// suffles deck
    /// adds cards from cards dealth back list into deck list
    /// clears both card dealt list & held cards list
    /// </summary>
    private void ShuffleDeck()
    {
        if (cardsDealtList.Count == 0) return;

        deckList.AddRange(cardsDealtList);
        cardsDealtList.Clear();
        heldCardsList.Clear();
    }


    /// <summary>
    /// Check current hand on GameState.Results phase
    /// sorted dictionary for sorting keys for sequential use later
    /// dictionary for finding a flush
    /// </summary>
    /// <returns></returns>
    private bool CheckHandForResults()
    {
        SortedDictionary<int, int> valueSets = new SortedDictionary<int, int>();
        Dictionary<CardType, int> typeSets = new Dictionary<CardType, int>();
        bool isFlush = false;

        foreach (Card card in currentHand)
        {
            int cardValue = card.cardValue;
            if (!valueSets.ContainsKey(cardValue)) valueSets.Add(cardValue, 1);
            else valueSets[cardValue]++;
            
            CardType cardType = card.cardType;
            // if another suit comes into play then just continue  to next itteration as there is no possible flush
            if (!typeSets.ContainsKey(cardType))
            {
                if (typeSets.Count == 1)
                    continue;

                typeSets.Add(cardType, 1);
            }
            else
            {
                typeSets[cardType]++;

                if (typeSets[cardType] == 5)
                    isFlush = true;
            }
        }

        // check for the following hands: Royal Flush, Straight Flush, Flush, Straight
        if (valueSets.Count == 5)
        {
            // if 1, high straight, if 0, low straight, if -1, all other cards
            int num = CheckIfHandIsSequential(valueSets);

            if(num == 1)
            {
                if(isFlush) resultHandType = HandType.RoyalFlush;
                else resultHandType = HandType.Straight;
                return true;
            }
            else if (num == 0)
            {
                if (isFlush) resultHandType = HandType.StraightFlush;
                else resultHandType = HandType.Straight;
                return true;
            }
            else if (num == -1)
            {
                if (isFlush)
                {
                    resultHandType = HandType.Flush;
                    return true;
                }

                return false;
            }

        }
        // check for the following hands: Jacks or Better
        else if (valueSets.Count == 4)
        {
            if (CheckForJacksOrBetter(valueSets))
            {
                resultHandType = HandType.JacksOrBetter;
                return true;
            }
            else
            {
                resultHandType = HandType.AllOther;
                return false;
            }
        }
        // check for the following hands: Three of a kind, Two Pair
        else if (valueSets.Count == 3)
        {
            // if 1, three of a kind, if 0, two pairs
            int num = CheckForThreeOfAKindOrTwoPair(valueSets);
            
            if(num == 1)
            {
                resultHandType = HandType.ThreeOfAKind;
                return true;
            }
            else if(num == 0)
            {
                resultHandType = HandType.TwoPair;
                return true;
            }
        }
        // check for the following hands: Full House, Four of a kind
        else if (valueSets.Count == 2)
        {
            int num = CheckForFourOfAKindOrFullHouse(valueSets);

            if (num == 1)
            {
                resultHandType = HandType.FourOfAKind;
                return true;
            }
            else if (num == 0)
            {
                resultHandType = HandType.FullHouse;
                return true;
            }
        }

        resultHandType = HandType.AllOther;
        return false;
    }

    /// <summary>
    /// Checks for two type of hands: Four of a Kind & Full House
    /// Four of a Kind, return 1
    /// Full House, return 0
    /// </summary>
    /// <param name="valueSets"></param>
    /// <returns></returns>
    private int CheckForFourOfAKindOrFullHouse(SortedDictionary<int, int> valueSets)
    {
        foreach (KeyValuePair<int, int> val in valueSets)
        {
            if (val.Value == 4)
                return 1;
        }

        return 0;
    }

    /// <summary>
    /// Checks for two type of hands: Three of a kind & Two Pair
    /// Three of a Kind, return 1
    /// Two Pair, return 0
    /// </summary>
    /// <param name="valueSets"></param>
    /// <returns></returns>
    private int CheckForThreeOfAKindOrTwoPair(SortedDictionary<int, int> valueSets)
    {
        foreach (KeyValuePair<int, int> val in valueSets)
        {
            if(val.Value == 3)
                return 1;
        }

        return 0;
    }

    /// <summary>
    /// Checks to see if pair is Jacks or better
    /// </summary>
    /// <param name="valueSets"></param>
    /// <returns></returns>
    private bool CheckForJacksOrBetter(SortedDictionary<int, int> valueSets)
    {
        bool isJacksOrBetter = false;

        foreach (KeyValuePair<int, int> val in valueSets)
        {
            if (val.Value < 2) continue;

            if (val.Key == 1 || val.Key >= 11)
            {
                isJacksOrBetter = true;
                break;
            }
        }

        return isJacksOrBetter;
    }

    /// <summary>
    /// Checks if key numbers are sequential
    /// High straight: 10, Jack, Queen, King, Ace
    /// Non-High Straight: All 5 cards are in sequential order
    /// All Other: return -1 as non-straight
    /// </summary>
    /// <param name="valueSets"></param>
    /// <returns></returns>
    private int CheckIfHandIsSequential(SortedDictionary<int, int> valueSets)
    {
        int firstValue = 0;
        bool isAce = false;

        foreach (KeyValuePair<int, int> val in valueSets)
        {
            if (firstValue == 0 && val.Key == 1)
            {
                isAce = true;
                continue;
            }
            else if (firstValue == 0 && isAce && val.Key == 10) return 1;
            
            if(firstValue == 0)
            {
                firstValue = val.Key;
                continue;
            }


            if (firstValue + 1 == val.Key) firstValue = val.Key;
            else return -1;
        }

        return 0;
    }


}

public enum HandType
{
    RoyalFlush,
    StraightFlush,
    FourOfAKind,
    FullHouse,
    Flush,
    Straight,
    ThreeOfAKind,
    TwoPair,
    JacksOrBetter,
    AllOther
}