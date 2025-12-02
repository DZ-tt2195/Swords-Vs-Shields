using Photon.Pun;
using UnityEngine;
using TMPro;
using MyBox;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

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
    public Dictionary<string, bool> uiDictionary = new();

    protected override void Awake()
    {
        base.Awake();
        this.bottomType = this.GetType();
        resignButton = GameObject.Find("Resign Button").GetComponent<Button>();

        List<string> toAdd = new() { ConstantStrings.MyHand, ConstantStrings.MyDiscard, ConstantStrings.MyTroops, ConstantStrings.Resources };
        foreach (string next in toAdd)
            uiDictionary.Add(next, true);

        Invoke(nameof(Beginning), 1f);
    }

    void Beginning()
    {
        if (photonView.AmOwner && !initialized)
            DoFunction(() => SendName(PlayerPrefs.GetString(ConstantStrings.MyUserName)), RpcTarget.AllBuffered);
    }

    [PunRPC]
    void SendName(string username)
    {
        this.transform.SetParent(CreateGame.inst.canvas.transform);
        this.transform.localPosition = Vector3.zero;
        this.transform.SetAsFirstSibling();

        initialized = true;
        this.name = username;
        myPosition = (int)GetPlayerProperty(this, ConstantStrings.MyPosition);
        CreateGame.inst.listOfPlayers.Insert(myPosition, this);

        resignButton = GameObject.Find("Resign Button").GetComponent<Button>();
        if (photonView.AmOwner)
        {
            resignButton.onClick.AddListener(() => TurnManager.inst.TextForEnding($"Player Resigned-Player-{this.name}", myPosition));
            StartTurn();
        }
    }

    #endregion

#region Hand

    public List<Card> GetHand() => TurnManager.inst.GetCardList(ConstantStrings.MyHand, this);

    public void DrawCardRPC(int amount, int logged = 0)
    {
        if (amount <= 0)
            return;

        List<Card> myDeck = TurnManager.inst.GetCardList(ConstantStrings.MyDeck, this);
        while (myDeck.Count < amount)
        {
            List<Card> myDiscard = TurnManager.inst.GetCardList(ConstantStrings.MyDiscard);
            myDiscard = myDiscard.Shuffle();
            myDeck.AddRange(myDiscard);
            TurnManager.inst.WillChangePlayerProperty(this, ConstantStrings.MyDiscard, new int[0]);
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

    void AddToHand(List<Card> cardsToAdd)
    {
        List<Card> myHand = GetHand();
        List<Card> myDeck = TurnManager.inst.GetCardList(ConstantStrings.MyDeck, this);

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
                myDeck.Remove(card);
            }
        }
        TurnManager.inst.WillChangePlayerProperty(this, ConstantStrings.MyHand, TurnManager.inst.ConvertCardList(myHand)); uiDictionary[ConstantStrings.MyHand] = true;
        TurnManager.inst.WillChangePlayerProperty(this, ConstantStrings.MyDeck, TurnManager.inst.ConvertCardList(myDeck));
    }

    public void DiscardRPC(Card card, int logged)
    {
        Log.inst.NewRollback(() => DiscardFromHand(card));
        Log.inst.AddMyText($"Discard Card-Player-{this.name}-Card-{card.name}", false, logged);
    }

    void DiscardFromHand(Card card)
    {
        List<Card> myHand = GetHand();
        List<Card> myDiscard = TurnManager.inst.GetCardList(ConstantStrings.MyDiscard, this);

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
        TurnManager.inst.WillChangePlayerProperty(this, ConstantStrings.MyHand, TurnManager.inst.ConvertCardList(myHand)); uiDictionary[ConstantStrings.MyHand] = true;
        TurnManager.inst.WillChangePlayerProperty(this, ConstantStrings.MyDiscard, TurnManager.inst.ConvertCardList(myDiscard)); uiDictionary[ConstantStrings.MyDiscard] = true;
    }

    #endregion

#region Resources

    public int GetSword() => TurnManager.inst.GetInt(ConstantStrings.Sword, this);

    public int GetShield() => TurnManager.inst.GetInt(ConstantStrings.Shield, this);

    public int GetAction() => TurnManager.inst.GetInt(ConstantStrings.Action, this);

    public int GetHealth() => TurnManager.inst.GetInt(ConstantStrings.MyHealth, this);

    void ChangeResource(int num, string property)
    {
        int total = TurnManager.inst.GetInt(property, this);
        total += (!Log.inst.forward) ? -num : num;
        TurnManager.inst.WillChangePlayerProperty(this, property, total); uiDictionary[ConstantStrings.Resources] = true;
    }

    public void ShieldRPC(int num, int logged = 0)
    {
        if (num == 0)
            return;
        if (num > 0)
            Log.inst.AddMyText($"Add Shield-Player-{this.name}-Num-{num}", false, logged);
        else
            Log.inst.AddMyText($"Lose Shield-Player-{this.name}-Num-{Mathf.Abs(num)}", false, logged);
        Log.inst.NewRollback(() => ChangeResource(num, ConstantStrings.Shield));
    }

    public void SwordRPC(int num, int logged = 0)
    {
        if (num == 0)
            return;
        if (num > 0)
            Log.inst.AddMyText($"Add Sword-Player-{this.name}-Num-{num}", false, logged);
        else
            Log.inst.AddMyText($"Lose Sword-Player-{this.name}-Num-{Mathf.Abs(num)}", false, logged);
        Log.inst.NewRollback(() => ChangeResource(num, ConstantStrings.Sword));
    }

    public void ActionRPC(int num, int logged = 0)
    {
        if (num == 0)
            return;
        if (num > 0)
            Log.inst.AddMyText($"Add Action-Player-{this.name}-Num-{num}", false, logged);
        else
            Log.inst.AddMyText($"Lose Action-Player-{this.name}-Num-{Mathf.Abs(num)}", false, logged);
        Log.inst.NewRollback(() => ChangeResource(num, ConstantStrings.Action));
    }

    public void HealthRPC(int num, int logged = 0)
    {
        if (num == 0)
            return;
        if (num > 0)
            Log.inst.AddMyText($"Add Health Player-Player-{this.name}-Num-{num}", false, logged);
        else
            Log.inst.AddMyText($"Lose Health Player-Player-{this.name}-Num-{Mathf.Abs(num)}", false, logged);
        Log.inst.NewRollback(() => ChangeResource(num, ConstantStrings.MyHealth));
    }

    public void NextRoundShield(int num) => Log.inst.NewRollback(() => ChangeResource(num, ConstantStrings.NextRoundShield));

    public void NextRoundAction(int num) => Log.inst.NewRollback(() => ChangeResource(num, ConstantStrings.NextRoundAction));

    public void NextRoundSword(int num) => Log.inst.NewRollback(() => ChangeResource(num, ConstantStrings.NextRoundSword));

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
        InstantChangePlayerProp(this, ConstantStrings.Waiting, false);
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
            string instructions = (Log.inst.undosInLog.Count >= 1) ? "Pause to Undo" : "Pause to Read";
            MakeDecision.inst.ChooseTextButton(new() { new("Done", Color.white) }, instructions, false);
        }

        void Done()
        {
            Log.inst.DoneWithTurn();
            InstantChangePlayerProp(this, ConstantStrings.Waiting, true);
        }
    }

    #endregion

#region UI

    public List<Card> GetTroops() => TurnManager.inst.GetCardList(ConstantStrings.MyTroops, this);

    public void UpdateUI(bool forcedUpdate)
    {
        List<string> uiKeys = uiDictionary.Keys.ToList();

        myUI = CreateGame.inst.GetUI(myPosition);
        myUI.image.color = (myPosition == 0) ? Color.blue : Color.red;
        onBottom = myUI.image.transform.parent.name.Equals("Bottom Player");

        if (forcedUpdate)
        {
            foreach (var key in uiKeys)
                uiDictionary[key] = true;
        }

        List<Card> myHand = GetHand();
        if (uiDictionary[ConstantStrings.MyHand])
        {
            List<Vector2> handPositions = ObjectPositions(myHand.Count, -700, 475, 225, (onBottom ? -550 : 550), true);

            int thisPlayerPosition = (int)GetPlayerProperty(PhotonNetwork.LocalPlayer, ConstantStrings.MyPosition.ToString());
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
        }

        if (uiDictionary[ConstantStrings.MyDiscard])
        {
            foreach (Card card in TurnManager.inst.GetCardList(ConstantStrings.MyDiscard, this))
                card.transform.SetParent(null);
        }

        if (uiDictionary[ConstantStrings.Resources] || uiDictionary[ConstantStrings.MyHand])
        {
            myUI.infoText.text = KeywordTooltip.instance.EditText
            ($"{this.name}: {GetHealth()} {Translator.inst.Translate("Health")}\n\n" +
            $"{myHand.Count} {Translator.inst.Translate("Card")} " +
            $"{GetAction()} {Translator.inst.Translate("Action")}\n" +
            $"{GetShield()} {Translator.inst.Translate("Shield")} " +
            $"{GetSword()} {Translator.inst.Translate("Sword")}");
        }

        AliveTroops();

        foreach (var key in uiKeys)
            uiDictionary[key] = false;
    }

    List<Vector2> ObjectPositions(int objectAmount, float start, float end, float gap, float fixedPosition, bool useX)
    {
        float midPoint = (start + end) / 2f;
        int maxFit = (int)((Mathf.Abs(start) + Mathf.Abs(end)) / gap);
        float offByOne = objectAmount - 1;

        List<Vector2> toReturn = new();
        for (int i = 0; i<objectAmount; i++)
        {
            float starting = (objectAmount <= maxFit) ? midPoint - (gap * (offByOne / 2f)) : start;
            float difference = (objectAmount <= maxFit) ? gap : gap * (maxFit / offByOne);

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

        if (uiDictionary[ConstantStrings.MyTroops])
        {
            foreach (Card card in myTroops)
            {
                card.transform.SetParent(null);
            }
        }

        for (int i = 0; i < myUI.cardDisplays.Count; i++)
        {
            if (i < myTroops.Count)
            {
                if (uiDictionary[ConstantStrings.MyTroops])
                {
                    myUI.cardDisplays[i].gameObject.SetActive(true);
                    myUI.cardDisplays[i].NewCard(myTroops[i]);
                }
                if (myTroops[i].GetHealth() >= 1)
                    toReturn.Add(myUI.cardDisplays[i]);
            }
            else
            {
                myUI.cardDisplays[i].gameObject.SetActive(false);
            }
        }

        return toReturn;
    }

    #endregion

}
