using Photon.Pun;
using UnityEngine;
using TMPro;
using MyBox;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

public enum PlayerProp { Position, Waiting, MyHand, MyDeck, MyDiscard, MyTroops, Shield, Sword, Action }

public class Player : PhotonCompatible
{

#region Setup

    bool initialized = false;
    public bool endPause = true;
    public int myPosition { get; private set; }

    Button resignButton;
    [SerializeField] Transform keepHand;
    public PlayerDisplay myPlayerDisplay { get; private set; }
    List<MiniCardDisplay> allMyTroopDisplays;

    protected override void Awake()
    {
        base.Awake();
        this.bottomType = this.GetType();
        resignButton = GameObject.Find("Resign Button").GetComponent<Button>();
    }

    private void Start()
    {
        if (photonView.AmOwner)
        {
            if (!initialized)
            {
                DoFunction(() => SendName(PlayerPrefs.GetString("Online Username")), RpcTarget.AllBuffered);
            }
            Invoke(nameof(StartTurn), 0.5f);
        }
    }

    [PunRPC]
    void SendName(string username)
    {
        this.transform.SetParent(PlayerCreator.inst.canvas.transform);

        if (photonView.AmOwner)
            this.transform.localPosition = Vector3.zero;
        else
            this.transform.localPosition = new(10000, 10000);

        initialized = true;
        this.name = username;
        myPosition = (int)GetPlayerProperty(this, PlayerProp.Position);
        PlayerCreator.inst.listOfPlayers.Insert(myPosition, this);

        (PlayerDisplay myDisplay, List<MiniCardDisplay> myTroopDisplays) = PlayerCreator.inst.PlayerUI(myPosition);
        myPlayerDisplay = myDisplay;
        allMyTroopDisplays = myTroopDisplays;
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
            List<Card> myDeck = TurnManager.inst.GetCardList(PlayerProp.MyDeck, this);
            while (myDeck.Count < amount)
            {
                myDeck = TurnManager.inst.GetCardList(PlayerProp.MyDeck, this);
                yield return null;
            }

            List<Card> toDraw = new();
            for (int i = 0; i < amount; i++)
            {
                Card card = myDeck[i];
                Log.inst.AddMyText($"Draw Card-Player-{this.name}-Card-{card.name}", false, logged);
                toDraw.Add(card);
            }
            Log.inst.NewRollback(() => AddToHand(toDraw));
        }
    }

    void AddToHand(List<Card> cardsToAdd)
    {
        List<Card> myHand = TurnManager.inst.GetCardList(PlayerProp.MyHand, this);
        List<Card> myDeck = TurnManager.inst.GetCardList(PlayerProp.MyDeck, this);

        if (!Log.inst.forward)
        {
            for (int i = cardsToAdd.Count-1; i>= 0; i--)
            {
                Card card = cardsToAdd[i];
                myHand.Remove(card);
                myDeck.Insert(0, card);
            }
        }
        else
        {
            for (int i = 0; i < cardsToAdd.Count; i++)
            {
                Card card = cardsToAdd[i];
                myHand.Add(card);
                myDeck.RemoveAt(0);
            }
        }
        TurnManager.inst.WillChangePlayerProperty(PlayerProp.MyHand, TurnManager.inst.ConvertCardList(myHand));
        TurnManager.inst.WillChangePlayerProperty(PlayerProp.MyDeck, TurnManager.inst.ConvertCardList(myDeck));
    }

    void AskForCards(int amount)
    {
        if (amount <= 0)
            return;

        List<Card> myDeck = TurnManager.inst.GetCardList(PlayerProp.MyDeck, this);
        int needToGet = amount - myDeck.Count;
        List<Card> masterDeck = TurnManager.inst.GetCardList(RoomProp.MasterDeck.ToString());

        if (needToGet >= 1)
        {
            if (masterDeck.Count < needToGet)
            {
                List<Card> masterDiscard = TurnManager.inst.GetCardList(RoomProp.MasterDiscard.ToString());
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
            ChangeRoomProperties(RoomProp.MasterDeck, TurnManager.inst.ConvertCardList(masterDeck));
            ChangePlayerProperties(this, PlayerProp.MyDeck, TurnManager.inst.ConvertCardList(myDeck));
        }
    }

    #endregion

#region Resources

    void ChangeInt(int num, string property, bool player)
    {
        int total = TurnManager.inst.GetInt(property);
        total += (!Log.inst.forward) ? -num : num;
        if (player)
            TurnManager.inst.WillChangePlayerProperty(property, total);
        else
            TurnManager.inst.WillChangeMasterProperty(property, total);
    }

    public void ShieldRPC(int num, int logged)
    {
        if (num == 0)
            return;
        if (num > 0)
            Log.inst.AddMyText($"Add Shield-Player-{this.name}-Num-{num}", false, logged);
        else
            Log.inst.AddMyText($"Lose Shield-Player-{this.name}-Num-{Mathf.Abs(num)}", false, logged);
        Log.inst.NewRollback(() => ChangeInt(num, PlayerProp.Shield.ToString(), true));
    }

    public void SwordRPC(int num, int logged)
    {
        if (num == 0)
            return;
        if (num > 0)
            Log.inst.AddMyText($"Add Sword-Player-{this.name}-Num-{num}", false, logged);
        else
            Log.inst.AddMyText($"Lose Sword-Player-{this.name}-Num-{Mathf.Abs(num)}", false, logged);
        Log.inst.NewRollback(() => ChangeInt(num, PlayerProp.Sword.ToString(), true));
    }

    public void ActionRPC(int num, int logged)
    {
        if (num == 0)
            return;
        if (num > 0)
            Log.inst.AddMyText($"Add Action-Player-{this.name}-Num-{num}", false, logged);
        else
            Log.inst.AddMyText($"Lose Action-Player-{this.name}-Num-{Mathf.Abs(num)}", false, logged);
        Log.inst.NewRollback(() => ChangeInt(num, PlayerProp.Action.ToString(), true));
    }

    public void HealthRPC(int num, int logged)
    {
        if (num == 0)
            return;
        if (num > 0)
            Log.inst.AddMyText($"Add Health Player-Player-{this.name}-Num-{num}", false, logged);
        else
            Log.inst.AddMyText($"Lose Health Player-Player-{this.name}-Num-{Mathf.Abs(num)}", false, logged);
        Log.inst.NewRollback(() => ChangeInt(num, $"P{myPosition}_Health", false));
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
            string header = TurnManager.inst.Instructions(myPosition, instructions);
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
            string header = TurnManager.inst.Instructions(myPosition, instructions);
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
            string header = TurnManager.inst.Instructions(myPosition, instructions);
            slider.StatsSetup(header, min, max, position, true, action);

            Log.inst.inReaction.Add(() => Destroy(slider.gameObject));
            return slider;
        }
    }

    public void ChooseDisplayOnScreen(List<MiniCardDisplay> listOfDisplays, Action<Card> action = null, bool autoResolve = true)
    {
        if (listOfDisplays.Count == 0 && autoResolve)
        {
        }
        else if (listOfDisplays.Count == 1 && autoResolve)
        {
            Log.inst.inReaction.Add(() => action?.Invoke(listOfDisplays[0].card));
        }
        else
        {
            Log.inst.SetUndoPoint(true);
            Log.inst.inReaction.Add(Disable);

            for (int j = 0; j < listOfDisplays.Count; j++)
            {
                MiniCardDisplay nextCard = listOfDisplays[j];
                int number = j;
                Button cardButton = nextCard.selectMe.button;

                cardButton.onClick.RemoveAllListeners();
                cardButton.interactable = true;
                nextCard.selectMe.border.gameObject.SetActive(true);
                cardButton.onClick.AddListener(ClickedThis);

                void ClickedThis()
                {
                    Log.inst.inReaction.Add(() => action?.Invoke(nextCard.card));
                    Log.inst.PopStack();
                }
            }

            void Disable()
            {
                foreach (MiniCardDisplay nextCard in listOfDisplays)
                {
                    nextCard.selectMe.button.onClick.RemoveAllListeners();
                    nextCard.selectMe.button.interactable = false;
                    nextCard.selectMe.border.gameObject.SetActive(false);
                }
            }
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
        TurnManager.inst.Instructions((int)GetPlayerProperty(this, PlayerProp.Position), "Blank");
        ChangePlayerProperties(this, PlayerProp.Waiting, false);
        endPause = true;

        Action action = TurnManager.inst.GetTurnAction(this);
        Log.inst.NewDecisionContainer(() => action(), 0);
        Log.inst.NewDecisionContainer(() => EndTurn(), 0);
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

    #endregion

#region UI

    public void UpdateUI()
    {
        List<Card> myHand = TurnManager.inst.GetCardList(PlayerProp.MyHand, this);
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

            if (photonView.AmOwner || (int)GetPlayerProperty(PhotonNetwork.LocalPlayer, PlayerProp.Position.ToString()) == -1)
            {
                Vector2 newPosition = new(startingX + difference * i, -525);
                nextCard.MoveCardRPC(newPosition, 0.25f, Vector3.one);
                myHand[i].FlipCardRPC(1, 0.25f, 0);
            }
        }

        string descriptionText = $"{this.name}" +
            $"\n{myHand.Count} Card, " +
            $"{TurnManager.inst.GetInt(PlayerProp.Action, this)} {PlayerProp.Action}" +
            $"\n{TurnManager.inst.GetInt(PlayerProp.Shield, this)} {PlayerProp.Shield}, " +
            $"{TurnManager.inst.GetInt(PlayerProp.Sword, this)} {PlayerProp.Sword}";
        myPlayerDisplay.AssignInfo(this, TurnManager.inst.GetInt($"P{myPosition}_Health", this.photonView.Owner), KeywordTooltip.instance.EditText(descriptionText));

        List<Card> myTroops = TurnManager.inst.GetCardList(PlayerProp.MyTroops, this);
        for (int i = 0; i < allMyTroopDisplays.Count; i++)
        {
            if (i < myTroops.Count)
            {
                allMyTroopDisplays[i].gameObject.SetActive(true);
                allMyTroopDisplays[i].NewCard(this, myTroops[i]);
            }
            else
            {
                allMyTroopDisplays[i].gameObject.SetActive(false);
            }
        }
    }

    public List<MiniCardDisplay> AliveTroops()
    {
        List<MiniCardDisplay> toReturn = new();
        List<Card> myTroops = TurnManager.inst.GetCardList(PlayerProp.MyTroops, this);
        for (int i = 0; i<myTroops.Count; i++)
        {
            Card card = myTroops[i];
            if (TurnManager.inst.GetInt(card.HealthString()) >= 1)
                toReturn.Add(allMyTroopDisplays[i]);
        }
        return toReturn;
    }

    #endregion

}
