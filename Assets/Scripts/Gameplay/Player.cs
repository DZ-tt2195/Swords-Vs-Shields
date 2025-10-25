using Photon.Pun;
using UnityEngine;
using TMPro;
using MyBox;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

public enum PlayerProp { Spectator, MyHand, MyDeck, MyDiscard, Coin, Waiting, UseCoast, UseCity, UseWoods, UseVillage, UseDelay, CardsInCoast, CardsInCity, CardsInWoods, CardsInVillage, CardsInDelay }

public class Player : PhotonCompatible
{

#region Setup

    bool initialized = false;

    [Foldout("Undo/Share", true)]
    ExitGames.Client.Photon.Hashtable playerPropertyToChange;
    ExitGames.Client.Photon.Hashtable masterPropertyToChange;
    int cardsDrawnThisTurn = 0;
    public bool endPause = true;

    [Foldout("UI", true)]
    Button resignButton;
    Transform keepHand;
    TMP_Dropdown playerDropdown;
    [SerializeField] List<MiniCardDisplay> coastCards;
    [SerializeField] List<MiniCardDisplay> cityCards;
    [SerializeField] List<MiniCardDisplay> woodsCards;
    [SerializeField] List<MiniCardDisplay> villageCards;
    [SerializeField] TMP_Text playerText;

    protected override void Awake()
    {
        base.Awake();
        this.bottomType = this.GetType();
        playerPropertyToChange = new();
        masterPropertyToChange = new();

        resignButton = GameObject.Find("Resign Button").GetComponent<Button>();
        playerDropdown = GameObject.Find("Player Dropdown").GetComponent<TMP_Dropdown>();

        if (photonView.AmOwner)
        {
            playerDropdown.onValueChanged.AddListener(Bottom);
            void Bottom(int value)
            {
                foreach (Photon.Realtime.Player player in PlayerCreator.inst.playerDictionary.Keys)
                    PlayerCreator.inst.playerDictionary[player].transform.localPosition = new(-10000, -10000);
            }
        }
    }

    private void Start()
    {
        if (photonView.AmOwner)
        {
            if (!initialized)
                DoFunction(() => SendName(PlayerPrefs.GetString("Online Username")), RpcTarget.AllBuffered);
            Invoke(nameof(StartTurn), 0.5f);
        }
    }

    [PunRPC]
    void SendName(string username)
    {
        this.transform.SetParent(PlayerCreator.inst.canvas.transform);
        this.transform.localPosition = new(-10000, -10000);
        initialized = true;
        this.name = username;
        PlayerCreator.inst.playerDictionary.Add(this.photonView.Owner, this);

        int addedOption = playerDropdown.options.Count;
        playerDropdown.AddOptions(new List<string>() { username });
        playerDropdown.onValueChanged.AddListener(MoveScreen);

        if (this.photonView.AmOwner)
        {
            playerDropdown.value = addedOption;
            if (addedOption == 0)
                MoveScreen(addedOption);
        }

        void MoveScreen(int value)
        {
            if (value == addedOption)
            {
                this.transform.localPosition = Vector3.zero;
                UpdateUI();
            }
        }
    }

    #endregion

#region Draw cards

    public void DrawCardRPC(int amount, int logged)
    {
        Log.inst.groupToWait.StartCoroutine(WaitToDraw());
        AskForCards(amount);

        IEnumerator WaitToDraw()
        {
            List<Card> myDeck = GetCardList(PlayerProp.MyDeck.ToString());
            while (myDeck.Count < (amount+cardsDrawnThisTurn))
            {
                myDeck = GetCardList(PlayerProp.MyDeck.ToString());
                yield return null;
            }

            List<Card> toDraw = new();
            for (int i = 0; i < amount; i++)
            {
                int num = cardsDrawnThisTurn + i;
                Log.inst.AddMyText($"Player Draw-Card-{toDraw[num].name}", false, logged);
                toDraw.Add(myDeck[num]);
            }
            Log.inst.NewRollback(this, () => AddToHand(false, toDraw));
        }
    }

    void AddToHand(bool undo, List<Card> cardsToAdd)
    {
        List<Card> myHand = GetCardList(PlayerProp.MyHand.ToString());
        if (undo)
        {
            cardsDrawnThisTurn -= cardsToAdd.Count;
            foreach (Card card in cardsToAdd)
                myHand.Remove(card);
        }
        else
        {
            cardsDrawnThisTurn += cardsToAdd.Count;
            foreach (Card card in cardsToAdd)
            {
                Debug.Log($"drew {card.photonView.ViewID}");
                myHand.Add(card);
            }
        }
        WillChangePlayerProperty(PlayerProp.MyHand.ToString(), ConvertCardList(myHand));
    }

    void AskForCards(int amount)
    {
        if (amount <= 0)
            return;

        List<Card> myDeck = GetCardList(PlayerProp.MyDeck.ToString());
        int needToGet = cardsDrawnThisTurn + amount - myDeck.Count;
        List<Card> masterDeck = GetCardList(RoomProp.MasterDeck.ToString());

        if (needToGet >= 1)
        {
            if (masterDeck.Count < needToGet)
            {
                List<Card> masterDiscard = GetCardList(RoomProp.MasterDiscard.ToString());
                masterDiscard = masterDiscard.Shuffle();
                masterDeck.AddRange(masterDiscard);
                ChangeRoomProperties(RoomProp.MasterDiscard, new int[0]);
            }

            for (int i = 0; i<needToGet; i++)
            {
                Card card = masterDeck[0];
                myDeck.Add(card);
                masterDeck.RemoveAt(0);
            }
            ChangeRoomProperties(RoomProp.MasterDeck, ConvertCardList(masterDeck));
            ChangePlayerProperties(this, PlayerProp.MyDeck, ConvertCardList(myDeck));
        }
    }

    public int[] ConvertCardList(List<Card> listOfCards)
    {
        int[] arrayOfCards = new int[listOfCards.Count];
        for (int i = 0; i < arrayOfCards.Length; i++)
            arrayOfCards[i] = listOfCards[i].photonView.ViewID;
        return arrayOfCards;
    }

    #endregion

#region Properties

    public int GetInt(string property)
    {
        if (masterPropertyToChange.ContainsKey(property))
            return (int)masterPropertyToChange[property];
        else if (playerPropertyToChange.ContainsKey(property))
            return (int)playerPropertyToChange[property];
        else if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(property))
            return (int)GetRoomProperty(property);
        else if (this.photonView.Owner.CustomProperties.ContainsKey(property))
            return (int)GetPlayerProperty(this.photonView.Owner, property);
        else
            return 0;
    }

    public List<Card> GetCardList(string property)
    {
        if (masterPropertyToChange.ContainsKey(property))
            return ConvertIntArray((int[])masterPropertyToChange[property]);
        else if (playerPropertyToChange.ContainsKey(property))
            return ConvertIntArray((int[])playerPropertyToChange[property]);
        else if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(property))
            return ConvertIntArray((int[])GetRoomProperty(property));
        else if (this.photonView.Owner.CustomProperties.ContainsKey(property))
            return ConvertIntArray((int[])GetPlayerProperty(this.photonView.Owner, property));
        else
            return new List<Card>();

        List<Card> ConvertIntArray(int[] arrayOfPVs)
        {
            List<Card> listOfCards = new();
            foreach (int nextPV in arrayOfPVs)
                listOfCards.Add(PhotonView.Find(nextPV).GetComponent<Card>());
            return listOfCards;
        }
    }

    public void WillChangePlayerProperty(string playerProperty, object changeInto)
    {
        if (playerPropertyToChange.ContainsKey(playerProperty))
            playerPropertyToChange[playerProperty] = changeInto;
        else
            playerPropertyToChange.Add(playerProperty, changeInto);
    }

    public void WillChangeMasterProperty(string masterProperty, object changeInto)
    {
        if (masterPropertyToChange.ContainsKey(masterProperty))
            masterPropertyToChange[masterProperty] = changeInto;
        else
            masterPropertyToChange.Add(masterProperty, changeInto);
    }

    public void CoinRPC(int num, int logged)
    {
        if (num == 0)
            return;
        if (num > 0)
            Log.inst.AddMyText($"Add Coin-Player-{this.name}-Num-{num}", false, logged);
        else
            Log.inst.AddMyText($"Lose Coin-Player-{this.name}-Num-{Mathf.Abs(num)}", false, logged);
        Log.inst.NewRollback(this, () => ChangeCoin(false, num));
    }

    void ChangeCoin(bool undo, int num)
    {
        int coinTotal = GetInt(PlayerProp.Coin.ToString());
        coinTotal += (undo) ? -num : num;
        WillChangePlayerProperty(PlayerProp.Coin.ToString(), coinTotal);
    }

    #endregion

#region Decide

    public Popup ChooseButtonInPopup(List<TextButtonInfo> possibleChoices, string instructions, Vector3 position, bool autoResolve = true)
    {
        if (possibleChoices.Count == 0 && autoResolve)
        {
            Log.inst.PopStack();
            return null;
        }
        else if (possibleChoices.Count == 1 && autoResolve)
        {
            if (possibleChoices[0].action != null)
                Log.inst.inReaction.Add(possibleChoices[0].action);
            Log.inst.PopStack();
            return null;
        }
        else
        {
            Log.inst.SetUndoPoint(true);
            TextPopup popup = Instantiate(CarryVariables.inst.textPopup);
            string header = TurnManager.inst.Instructions(this.photonView.ControllerActorNr, instructions);
            popup.StatsSetup(true, header, position);

            for (int i = 0; i < possibleChoices.Count; i++)
                popup.AddTextButton(possibleChoices[i]);

            Log.inst.inReaction.Add(() => Destroy(popup.gameObject));
            return popup;
        }
    }

    public Popup ChooseCardInPopup(List<CardButtonInfo> possibleCards, string instructions, Vector3 position, bool autoResolve = true)
    {
        if (possibleCards.Count == 0 && autoResolve)
        {
            Log.inst.PopStack();
            return null;
        }
        else if (possibleCards.Count == 1 && autoResolve)
        {
            CardButtonInfo onlyOne = possibleCards[0];
            Log.inst.inReaction.Add(() => onlyOne.action?.Invoke(onlyOne.card));
            Log.inst.PopStack();
            return null;
        }
        else
        {
            Log.inst.SetUndoPoint(true);
            CardPopup popup = Instantiate(CarryVariables.inst.cardPopup);
            string header = TurnManager.inst.Instructions(this.photonView.ControllerActorNr, instructions);
            popup.StatsSetup(true, header, position);

            for (int i = 0; i < possibleCards.Count; i++)
                popup.AddCardButton(possibleCards[i]);

            Log.inst.inReaction.Add(() => Destroy(popup.gameObject));
            return popup;
        }
    }

    public void ChooseCardOnScreen(List<Card> listOfCards, Action<Card> action = null, bool autoResolve = true)
    {
        if (listOfCards.Count == 0 && autoResolve)
        {
            Log.inst.PopStack();
        }
        else if (listOfCards.Count == 1 && autoResolve)
        {
            Log.inst.inReaction.Add(() => action?.Invoke(listOfCards[0]));
            Log.inst.PopStack();
        }
        else
        {
            Log.inst.SetUndoPoint(true);
            Log.inst.inReaction.Add(Disable);

            for (int j = 0; j < listOfCards.Count; j++)
            {
                Card nextCard = listOfCards[j];
                int number = j;
                Button cardButton = nextCard.button;

                cardButton.onClick.RemoveAllListeners();
                cardButton.interactable = true;
                nextCard.border.gameObject.SetActive(true);
                cardButton.onClick.AddListener(ClickedThis);

                void ClickedThis()
                {
                    Log.inst.inReaction.Add(() => action?.Invoke(nextCard));
                    Log.inst.PopStack();
                }
            }

            void Disable()
            {
                foreach (Card nextCard in listOfCards)
                {
                    nextCard.button.onClick.RemoveAllListeners();
                    nextCard.button.interactable = false;
                    nextCard.border.gameObject.SetActive(false);
                }
            }
        }
    }

    public SliderChoice ChooseSlider(int min, int max, string instructions, Vector3 position, bool autoResolve = true, Action<int> action = null)
    {
        if (min == max && autoResolve)
        {
            Log.inst.inReaction.Add(() => action?.Invoke(min));
            Log.inst.PopStack();
            return null;
        }
        else
        {
            Log.inst.SetUndoPoint(true);
            SliderChoice slider = Instantiate(CarryVariables.inst.sliderPopup);
            string header = TurnManager.inst.Instructions(this.photonView.ControllerActorNr, instructions);
            slider.StatsSetup(header, min, max, position, true, action);

            Log.inst.inReaction.Add(() => Destroy(slider.gameObject));
            return slider;
        }
    }

    #endregion

#region Turns

    void Update()
    {
        if (photonView.AmOwner)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
                GetPlayers(true);
            if (Input.GetKeyDown(KeyCode.Alpha4))
                PhotonNetwork.Disconnect();
        }
    }

    public Photon.Realtime.Player OtherPlayerInteraction(int advance)
    {
        List<Photon.Realtime.Player> allPlayers = GetPlayers(false).Item1;
        int myIndex = allPlayers.IndexOf(photonView.Owner);
        allPlayers.RemoveAt(myIndex);

        if (allPlayers.Count == 0)
        {
            return this.photonView.Owner;
        }
        else if (advance > 0)
        {
            int firstAnswer = (myIndex - 1 + advance) % allPlayers.Count;
            return allPlayers[firstAnswer];
        }
        else if (advance < 0)
        {
            int secondAnswer = (myIndex - Mathf.Abs(advance));
            while (secondAnswer < 0)
                secondAnswer += (allPlayers.Count);
            return allPlayers[secondAnswer];
        }
        return null;
    }

    [PunRPC]
    internal void StartTurn()
    {
        //this.DoFunction(() => this.ChangeButtonColor(false));
        TurnManager.inst.Instructions(photonView.ControllerActorNr, "Blank");
        ChangePlayerProperties(this, PlayerProp.Waiting, false);
        endPause = true;

        TurnManager.inst.DoTurnAction(this);
        Log.inst.NewDecisionContainer(this, () => EndTurn(), -1);
        Log.inst.PopStack();
    }

    void EndTurn()
    {
        Log.inst.inReaction.Add(Done);
        if (!endPause)
            Log.inst.PopStack();
        else if (Log.inst.undosInLog.Count >= 1)
            ChooseButtonInPopup(new() { new("Done", Color.white) }, "Pause to Undo", new(0, 325), false);
        else
            ChooseButtonInPopup(new() { new("Done", Color.white) }, "Pause to Read", new(0, 325), false);

        void Done()
        {
            Log.inst.undosInLog.Clear();
            ChangePlayerProperties(this, PlayerProp.Waiting, true);
        }
    }

    [PunRPC]
    internal void SharePropertyChanges()
    {
        Debug.Log($"shared {this.name} changes");
        Log.inst.ShareTexts();
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerPropertyToChange);
        playerPropertyToChange.Clear();
        PhotonNetwork.CurrentRoom.SetCustomProperties(masterPropertyToChange);
        masterPropertyToChange.Clear();

        //change personal deck
        List<Card> myDeck = GetCardList(PlayerProp.MyDeck.ToString());
        for (int i = 0; i < cardsDrawnThisTurn; i++)
            myDeck.RemoveAt(0);
        cardsDrawnThisTurn = 0;
        ChangePlayerProperties(this, PlayerProp.MyDeck, ConvertCardList(myDeck));

        //send away discarded cards
        List<Card> masterDiscard = GetCardList(RoomProp.MasterDiscard.ToString());
        masterDiscard.AddRange(GetCardList(PlayerProp.MyDiscard.ToString()));
        ChangePlayerProperties(this, PlayerProp.MyDiscard, new int[0]);
        ChangeRoomProperties(RoomProp.MasterDiscard, ConvertCardList(masterDiscard));
    }

    public void UpdateUI()
    {
        List<Card> myHand = GetCardList(PlayerProp.MyHand.ToString());
        for (int i = 0; i < myHand.Count; i++)
        {
            myHand[i].MoveCardRPC(new(-1000 + i * 200, -550), 0.25f, Vector3.one);
            if (PhotonNetwork.LocalPlayer.ActorNumber == this.photonView.ControllerActorNr && myHand[i].layout.GetAlpha() == 0)
                myHand[i].FlipCardRPC(1, 0.25f, 0);
        }

        playerText.text = $"{this.name} - {GetInt(PlayerProp.Coin.ToString())} Coin\n";
        playerText.text += $"{GetInt(PlayerProp.UseCoast.ToString())} {Translator.inst.Translate("Coast")} " +
            $"{GetInt(PlayerProp.UseCity.ToString())} {Translator.inst.Translate("City")} " +
            $"{GetInt(PlayerProp.UseWoods.ToString())} {Translator.inst.Translate("Woods")} " +
            $"{GetInt(PlayerProp.UseVillage.ToString())} {Translator.inst.Translate("Village")} ";

        ChangeArea(coastCards, GetCardList(PlayerProp.CardsInCoast.ToString()));
        ChangeArea(cityCards, GetCardList(PlayerProp.CardsInCity.ToString()));
        ChangeArea(woodsCards, GetCardList(PlayerProp.CardsInWoods.ToString()));
        ChangeArea(villageCards, GetCardList(PlayerProp.CardsInVillage.ToString()));

        void ChangeArea(List<MiniCardDisplay> displays, List<Card> cards)
        {
            for (int i = 0; i<displays.Count; i++)
            {
                if (i < cards.Count)
                {
                    displays[i].gameObject.SetActive(true);
                    displays[i].NewCard(cards[i], GetInt(cards[i].BoxString()));
                }
                else
                {
                    displays[i].gameObject.SetActive(false);
                }
            }
        }
    }

    #endregion

}
