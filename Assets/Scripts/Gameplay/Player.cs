using Photon.Pun;
using UnityEngine;
using TMPro;
using MyBox;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

public enum PlayerProp { Spectator, Waiting, MyHand, MyDeck, MyDiscard, MyTroops, MyHealth, Coin, Action }

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
    [SerializeField] Transform keepHand;

    protected override void Awake()
    {
        base.Awake();
        this.bottomType = this.GetType();
        playerPropertyToChange = new();
        masterPropertyToChange = new();

        resignButton = GameObject.Find("Resign Button").GetComponent<Button>();
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
        UpdateUI();
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

    #endregion

#region Resources

    public void CoinRPC(int num, int logged)
    {
        if (num == 0)
            return;
        if (num > 0)
            Log.inst.AddMyText($"Add Coin-Player-{this.name}-Num-{num}", false, logged);
        else
            Log.inst.AddMyText($"Lose Coin-Player-{this.name}-Num-{Mathf.Abs(num)}", false, logged);
        Log.inst.NewRollback(this, () => ChangeInt(false, num, PlayerProp.Coin.ToString()));
    }

    void ChangeInt(bool undo, int num, string property)
    {
        int coinTotal = GetInt(property);
        coinTotal += (undo) ? -num : num;
        WillChangePlayerProperty(property, coinTotal);
    }

    public void ActionRPC(int num, int logged)
    {
        if (num == 0)
            return;
        if (num > 0)
            Log.inst.AddMyText($"Add Action-Player-{this.name}-Num-{num}", false, logged);
        else
            Log.inst.AddMyText($"Lose Action-Player-{this.name}-Num-{Mathf.Abs(num)}", false, logged);
        Log.inst.NewRollback(this, () => ChangeInt(false, num, PlayerProp.Action.ToString()));
    }

    public void HealthRPC(int num, int logged)
    {
        if (num == 0)
            return;
        if (num > 0)
            Log.inst.AddMyText($"Add Health Player-Player-{this.name}-Num-{num}", false, logged);
        else
            Log.inst.AddMyText($"Lose Health Player-Player-{this.name}-Num-{Mathf.Abs(num)}", false, logged);
        Log.inst.NewRollback(this, () => ChangeInt(false, num, PlayerProp.MyHealth.ToString()));
    }

    #endregion

#region Decide

    public Popup ChooseButtonInPopup(List<TextButtonInfo> possibleChoices, string instructions, Vector3 position, bool autoResolve = true)
    {
        if (possibleChoices.Count == 0 && autoResolve)
        {
            return null;
        }
        else if (possibleChoices.Count == 1 && autoResolve)
        {
            if (possibleChoices[0].action != null)
                Log.inst.inReaction.Add(possibleChoices[0].action);
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
            return null;
        }
        else if (possibleCards.Count == 1 && autoResolve)
        {
            CardButtonInfo onlyOne = possibleCards[0];
            Log.inst.inReaction.Add(() => onlyOne.action?.Invoke(onlyOne.card));
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
        }
        else if (listOfCards.Count == 1 && autoResolve)
        {
            Log.inst.inReaction.Add(() => action?.Invoke(listOfCards[0]));
        }
        else
        {
            Log.inst.SetUndoPoint(true);
            Log.inst.inReaction.Add(Disable);

            for (int j = 0; j < listOfCards.Count; j++)
            {
                Card nextCard = listOfCards[j];
                int number = j;
                Button cardButton = nextCard.selectMe.button;

                cardButton.onClick.RemoveAllListeners();
                cardButton.interactable = true;
                nextCard.selectMe.border.gameObject.SetActive(true);
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
                    nextCard.selectMe.button.onClick.RemoveAllListeners();
                    nextCard.selectMe.button.interactable = false;
                    nextCard.selectMe.border.gameObject.SetActive(false);
                }
            }
        }
    }

    public SliderChoice ChooseSlider(int min, int max, string instructions, Vector3 position, bool autoResolve = true, Action<int> action = null)
    {
        if (min == max && autoResolve)
        {
            Log.inst.inReaction.Add(() => action?.Invoke(min));
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
        if (endPause)
        {
            if (Log.inst.undosInLog.Count >= 1)
                ChooseButtonInPopup(new() { new("Done", Color.white) }, "Pause to Undo", new(0, 325), false);
            else
                ChooseButtonInPopup(new() { new("Done", Color.white) }, "Pause to Read", new(0, 325), false);
        }

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

        DoFunction(() => UpdateUI(), RpcTarget.All);
    }

    [PunRPC]
    public void UpdateUI()
    {
        List<Card> myHand = GetCardList(PlayerProp.MyHand.ToString());
        float start = -1100;
        float end = 475;
        float gap = 225;
        float midPoint = (start + end) / 2;
        int maxFit = (int)((Mathf.Abs(start) + Mathf.Abs(end)) / gap);

        for (int i = 0; i < myHand.Count; i++)
        {
            Card nextCard = myHand[i];
            nextCard.transform.SetParent(keepHand);
            nextCard.transform.SetSiblingIndex(i);

            float offByOne = myHand.Count - 1;
            float startingX = (myHand.Count <= maxFit) ? midPoint - (gap * (offByOne / 2f)) : (start);
            float difference = (myHand.Count <= maxFit) ? gap : gap * (maxFit / offByOne);

            if (photonView.AmOwner || (bool)GetPlayerProperty(PhotonNetwork.LocalPlayer, PlayerProp.Spectator.ToString()))
            {
                Vector2 newPosition = new(startingX + difference * i, -550);
                nextCard.MoveCardRPC(newPosition, 0.25f, Vector3.one);
                myHand[i].FlipCardRPC(1, 0.25f, 0);
            }
        }

        (PlayerDisplay myDisplay, List<MiniCardDisplay> myTroopDisplays) = PlayerCreator.inst.PlayerUI(photonView.Controller);
        string descriptionText = $"{this.name}\n{myHand.Count} Card\n{GetInt(PlayerProp.Coin.ToString())} Coin\n{GetInt(PlayerProp.Action.ToString())} Action";
        myDisplay.AssignInfo(this, GetInt(PlayerProp.MyHealth.ToString()), KeywordTooltip.instance.EditText(descriptionText));

        List<Card> myTroops = GetCardList(PlayerProp.MyTroops.ToString());
        for (int i = 0; i < myTroopDisplays.Count; i++)
        {
            if (i < myTroops.Count)
            {
                myTroopDisplays[i].gameObject.SetActive(true);
                myTroopDisplays[i].NewCard(this, myTroops[i]);
            }
            else
            {
                myTroopDisplays[i].gameObject.SetActive(false);
            }
        }
    }

    #endregion

}
