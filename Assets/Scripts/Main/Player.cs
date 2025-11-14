using Photon.Pun;
using UnityEngine;
using TMPro;
using MyBox;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

public enum PlayerProp { Position, Waiting, MyHealth, MyHand, MyDeck, MyDiscard, MyTroops, Shield, Sword, Action, NextRoundShield, NextRoundSword, NextRoundAction, AllCardsPlayed }

public class Player : PhotonCompatible
{

#region Setup

    bool initialized = false;
    public bool endPause = true;
    public int myPosition { get; private set; }

    Button resignButton;
    [SerializeField] Transform keepHand;
    PlayerUI myUI;
    bool onBottom;

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
        this.transform.SetParent(CreateGame.inst.canvas.transform);
        this.transform.localPosition = Vector3.zero;
        this.transform.SetAsFirstSibling();

        initialized = true;
        this.name = username;
        myPosition = (int)GetPlayerProperty(this, PlayerProp.Position);
        CreateGame.inst.listOfPlayers.Insert(myPosition, this);

        myUI = CreateGame.inst.GetUI(myPosition);
        myUI.image.color = (myPosition == 0) ? Color.blue : Color.red;
        onBottom = myUI.image.transform.parent.name.Equals("Bottom Player");
        UpdateUI();

        resignButton = GameObject.Find("Resign Button").GetComponent<Button>();
        if (photonView.AmOwner)
            resignButton.onClick.AddListener(() => TurnManager.inst.TextForEnding($"Player Resigned-Player-{this.name}", myPosition));
    }

    #endregion

#region Hand

    public List<Card> GetHand() => TurnManager.inst.GetCardList(PlayerProp.MyHand, this);

    public void DrawCardRPC(int amount, int logged = 0)
    {
        if (amount <= 0)
            return;

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
        List<Card> myHand = GetHand();
        List<Card> myDeck = TurnManager.inst.GetCardList(PlayerProp.MyDeck, this);

        if (!Log.inst.forward)
        {
            for (int i = cardsToAdd.Count-1; i>= 0; i--)
            {
                Card card = cardsToAdd[i];
                card.transform.SetParent(null);
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
        TurnManager.inst.WillChangePlayerProperty(this, PlayerProp.MyHand, TurnManager.inst.ConvertCardList(myHand));
        TurnManager.inst.WillChangePlayerProperty(this, PlayerProp.MyDeck, TurnManager.inst.ConvertCardList(myDeck));
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
                InstantChangeRoomProp(RoomProp.MasterDiscard, new int[0]);
            }

            for (int i = 0; i<needToGet; i++)
            {
                Card card = masterDeck[0];
                myDeck.Add(card);
                masterDeck.RemoveAt(0);
            }
            InstantChangeRoomProp(RoomProp.MasterDeck, TurnManager.inst.ConvertCardList(masterDeck));
            TurnManager.inst.WillChangePlayerProperty(this, PlayerProp.MyDeck, TurnManager.inst.ConvertCardList(myDeck));
        }
    }

    public void DiscardRPC(Card card, int logged)
    {
        Log.inst.NewRollback(() => DiscardFromHand(card));
        Log.inst.AddMyText($"Discard Card-Player-{this.name}-Card-{card.name}", false, logged);
    }

    void DiscardFromHand(Card card)
    {
        List<Card> myHand = GetHand();
        List<Card> myDiscard = TurnManager.inst.GetCardList(PlayerProp.MyDiscard, this);

        if (!Log.inst.forward)
        {
            myHand.Add(card);
            myDiscard.Remove(card);
        }
        else
        {
            myHand.Remove(card);
            myDiscard.Add(card);
            card.transform.SetParent(null);
        }
        TurnManager.inst.WillChangePlayerProperty(this, PlayerProp.MyHand, TurnManager.inst.ConvertCardList(myHand));
        TurnManager.inst.WillChangePlayerProperty(this, PlayerProp.MyDiscard, TurnManager.inst.ConvertCardList(myDiscard));
    }

    #endregion

#region Resources

    public int GetSword() => TurnManager.inst.GetInt(PlayerProp.Sword, this);

    public int GetShield() => TurnManager.inst.GetInt(PlayerProp.Shield, this);

    public int GetAction() => TurnManager.inst.GetInt(PlayerProp.Action, this);

    public int GetHealth() => TurnManager.inst.GetInt(PlayerProp.MyHealth, this);

    void ChangeInt(int num, PlayerProp property)
    {
        int total = TurnManager.inst.GetInt(property, this);
        total += (!Log.inst.forward) ? -num : num;
        TurnManager.inst.WillChangePlayerProperty(this, property, total);
    }

    public void ShieldRPC(int num, int logged = 0)
    {
        if (num == 0)
            return;
        if (num > 0)
            Log.inst.AddMyText($"Add Shield-Player-{this.name}-Num-{num}", false, logged);
        else
            Log.inst.AddMyText($"Lose Shield-Player-{this.name}-Num-{Mathf.Abs(num)}", false, logged);
        Log.inst.NewRollback(() => ChangeInt(num, PlayerProp.Shield));
    }

    public void SwordRPC(int num, int logged = 0)
    {
        if (num == 0)
            return;
        if (num > 0)
            Log.inst.AddMyText($"Add Sword-Player-{this.name}-Num-{num}", false, logged);
        else
            Log.inst.AddMyText($"Lose Sword-Player-{this.name}-Num-{Mathf.Abs(num)}", false, logged);
        Log.inst.NewRollback(() => ChangeInt(num, PlayerProp.Sword));
    }

    public void ActionRPC(int num, int logged = 0)
    {
        if (num == 0)
            return;
        if (num > 0)
            Log.inst.AddMyText($"Add Action-Player-{this.name}-Num-{num}", false, logged);
        else
            Log.inst.AddMyText($"Lose Action-Player-{this.name}-Num-{Mathf.Abs(num)}", false, logged);
        Log.inst.NewRollback(() => ChangeInt(num, PlayerProp.Action));
    }

    public void HealthRPC(int num, int logged = 0)
    {
        if (num == 0)
            return;
        if (num > 0)
            Log.inst.AddMyText($"Add Health Player-Player-{this.name}-Num-{num}", false, logged);
        else
            Log.inst.AddMyText($"Lose Health Player-Player-{this.name}-Num-{Mathf.Abs(num)}", false, logged);
        Log.inst.NewRollback(() => ChangeInt(num, PlayerProp.MyHealth));
    }

    public void NextRoundShield(int num) => Log.inst.NewRollback(() => ChangeInt(num, PlayerProp.NextRoundShield));

    public void NextRoundAction(int num) => Log.inst.NewRollback(() => ChangeInt(num, PlayerProp.NextRoundAction));

    public void NextRoundSword(int num) => Log.inst.NewRollback(() => ChangeInt(num, PlayerProp.NextRoundSword));

    #endregion

#region Turns

    void Update()
    {
        if (photonView.AmOwner && Application.isEditor)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
                GetPlayers(true);
            if (Input.GetKeyDown(KeyCode.Alpha4))
                PhotonNetwork.Disconnect();
        }
    }

    public void StartTurn()
    {
        //this.DoFunction(() => this.ChangeButtonColor(false));
        InstantChangePlayerProp(this, PlayerProp.Waiting, false);
        endPause = true;

        (int phase, Action action) = TurnManager.inst.GetTurnAction(this);
        if (phase >= 1)
            Log.inst.AddMyText("Blank", true);

        Log.inst.NewDecisionContainer(() => action(), 0);
        Log.inst.NewDecisionContainer(() => EndTurn(), -1);
        Log.inst.PopStack();
    }

    void EndTurn()
    {
        Log.inst.inReaction.Add(Done);
        if (endPause)
        {
            MakeDecision.inst.ChooseTextButton(new() { new("Done", Color.white) }, false);
            if (Log.inst.undosInLog.Count >= 1)
                MakeDecision.inst.Instructions("Pause to Undo");
            else
                MakeDecision.inst.Instructions("Pause to Read");
        }

        void Done()
        {
            Log.inst.DoneWithTurn();
            InstantChangePlayerProp(this, PlayerProp.Waiting, true);
        }
    }

    #endregion

#region UI

    public List<Card> GetTroops() => TurnManager.inst.GetCardList(PlayerProp.MyTroops, this);

    public void UpdateUI()
    {
        List<Card> myHand = GetHand();
        List<Vector2> handPositions = ObjectPositions(myHand.Count, -875, 475, 225, (onBottom ? -525 : 525), true);

        int thisPlayerPosition = (int)GetPlayerProperty(PhotonNetwork.LocalPlayer, PlayerProp.Position.ToString());
        for (int i = 0; i < myHand.Count; i++)
        {
            Card nextCard = myHand[i];
            if (nextCard.transform.parent != keepHand)
            {
                nextCard.transform.SetParent(keepHand);
                nextCard.transform.localPosition = new(0, (onBottom ? -1000 : 1000));
            }
            nextCard.transform.SetSiblingIndex(i);
            nextCard.MoveCardRPC(handPositions[i], 0.25f, Vector3.one);

            if (thisPlayerPosition == -1 || thisPlayerPosition == myPosition)
                nextCard.FlipCardRPC(1, 0.25f, 0);
        }

        myUI.infoText.text = KeywordTooltip.instance.EditText
            ($"{this.name}: {GetHealth()} Health\n\n" +
            $"{myHand.Count} Card, " +
            $"{GetAction()} {PlayerProp.Action}\n" +
            $"{GetShield()} {PlayerProp.Shield}, " +
            $"{GetSword()} {PlayerProp.Sword}");

        List<Card> myTroops = GetTroops();
        foreach (Card card in myTroops)
            card.transform.SetParent(null);
        for (int i = 0; i < myUI.cardDisplays.Count; i++)
        {
            if (i < myTroops.Count)
            {
                myUI.cardDisplays[i].gameObject.SetActive(true);
                myUI.cardDisplays[i].NewCard(myTroops[i]);
            }
            else
            {
                myUI.cardDisplays[i].gameObject.SetActive(false);
            }
        }
    }

    List<Vector2> ObjectPositions(int objectAmount, float start, float end, float gap, float fixedPosition, bool useX)
    {
        float midPoint = (start + end) / 2f;
        int maxFit = (int)((Mathf.Abs(start) + Mathf.Abs(end)) / gap);
        int offByOne = objectAmount - 1;

        List<Vector2> toReturn = new();
        for (int i = 0; i<objectAmount; i++)
        {
            float starting = (objectAmount <= maxFit) ? midPoint - (gap * (offByOne / 2f)) : start;
            float difference = (objectAmount <= maxFit) ? gap : gap * (maxFit / (float)offByOne);

            if (useX)
                toReturn.Add(new(starting + difference * i, fixedPosition));
            else
                toReturn.Add(new(fixedPosition, starting + difference * i));
        }
        return toReturn;
    }

    public List<MiniCardDisplay> AliveTroops()
    {
        List<MiniCardDisplay> toReturn = new();
        List<Card> myTroops = GetTroops();
        for (int i = 0; i<myTroops.Count; i++)
        {
            Card card = myTroops[i];
            if (card.GetHealth() >= 1)
                toReturn.Add(myUI.cardDisplays[i]);
        }
        return toReturn;
    }

    #endregion

}
