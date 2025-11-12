using Photon.Pun;
using UnityEngine;
using TMPro;
using MyBox;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

public enum PlayerProp { Position, Waiting, MyHealth, MyHand, MyDeck, MyDiscard, MyTroops, Shield, Sword, Action, NextRoundShield, NextRoundSword }

public class Player : PhotonCompatible
{

#region Setup

    bool initialized = false;
    public bool endPause = true;
    public int myPosition { get; private set; }

    Button resignButton;
    [SerializeField] Transform keepHand;
    PlayerUI myUI;

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

        if (photonView.AmOwner)
            this.transform.localPosition = Vector3.zero;
        else
            this.transform.localPosition = new(10000, 10000);

        initialized = true;
        this.name = username;
        myPosition = (int)GetPlayerProperty(this, PlayerProp.Position);
        CreateGame.inst.listOfPlayers.Insert(myPosition, this);

        myUI = CreateGame.inst.GetUI(myPosition);
        UpdateUI();
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
                card.transform.position = new(0, -1000);
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
                ChangeRoomProperties(RoomProp.MasterDiscard, new int[0]);
            }

            for (int i = 0; i<needToGet; i++)
            {
                Card card = masterDeck[0];
                myDeck.Add(card);
                masterDeck.RemoveAt(0);
            }
            ChangeRoomProperties(RoomProp.MasterDeck, TurnManager.inst.ConvertCardList(masterDeck));
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

    public void NextRoundSword(int num) => Log.inst.NewRollback(() => ChangeInt(num, PlayerProp.NextRoundSword));

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
        ChangePlayerProperties(this, PlayerProp.Waiting, false);
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
            ChangePlayerProperties(this, PlayerProp.Waiting, true);
        }
    }

    #endregion

#region UI

    public List<Card> GetTroops() => TurnManager.inst.GetCardList(PlayerProp.MyTroops, this);

    public void UpdateUI()
    {
        List<Card> myHand = GetHand();
        float start = -1100;
        float end = 475;
        float gap = 225;
        float midPoint = (start + end) / 2;
        int maxFit = (int)((Mathf.Abs(start) + Mathf.Abs(end)) / gap);

        for (int i = 0; i < myHand.Count; i++)
        {
            Card nextCard = myHand[i];

            if (nextCard.transform.parent != keepHand)
            {
                nextCard.transform.SetParent(keepHand);
                nextCard.transform.localPosition = new(0, -1000);
            }
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

        myUI.infoText.text = KeywordTooltip.instance.EditText($"{this.name}: " +
            $"{myHand.Count} Card, " +
            $"{GetAction()} {PlayerProp.Action}, " +
            $"{GetShield()} {PlayerProp.Shield}, " +
            $"{GetSword()} {PlayerProp.Sword}");

        List<Card> myTroops = GetTroops();
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
