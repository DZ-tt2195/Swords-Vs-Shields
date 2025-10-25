using UnityEngine;
using UnityEngine.UI;
using MyBox;
using System.Collections;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System;
using Photon.Pun;
using System.Text.RegularExpressions;

public enum CardAreas { Coast, City, Woods, Village, Delay, None }

public class Card : PhotonCompatible
{

#region Setup

    public Button button;
    public Image border;
    public CardLayout layout;

    bool flipping;
    public CardType thisCard { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        this.bottomType = this.GetType();

        Canvas canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        this.transform.localScale = Vector3.Lerp(Vector3.one, canvas.transform.localScale, 0.5f);

        if (!PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(BoxString()))
        {
            ExitGames.Client.Photon.Hashtable initialProps = new()
            {
                [BoxString()] = 0,
                [AreaString()] = (int)CardAreas.None
            };
            PhotonNetwork.CurrentRoom.SetCustomProperties(initialProps);
        }
    }

    public string AreaString()
    {
        return $"{this.photonView.ViewID}_Area";
    }

    public string BoxString()
    {
        return $"{this.photonView.ViewID}_Box";
    }

    public void AssignCard(CardData dataFile)
    {
        string noSpaces = dataFile.cardName.Replace(" ", "");
        thisCard = (CardType)Activator.CreateInstance(Type.GetType(noSpaces), dataFile);
        this.layout.FillInCards(dataFile, 0, 0);
        this.name = dataFile.cardName;
        KeywordTooltip.instance.NewCardRC(dataFile.cardName, this.layout);
    }

    #endregion

#region Animations

    public void MoveCardRPC(Vector3 newPos, float waitTime, Vector3 newScale)
    {
        StopAllCoroutines();
        StartCoroutine(MoveCard(newPos, waitTime, newScale));
    }

    IEnumerator MoveCard(Vector3 newPos, float waitTime, Vector3 newScale)
    {
        float elapsedTime = 0;
        Vector2 originalPos = this.transform.localPosition;
        Vector2 originalScale = this.transform.localScale;

        while (elapsedTime < waitTime)
        {
            this.transform.localPosition = Vector3.Lerp(originalPos, newPos, elapsedTime / waitTime);
            this.transform.localScale = Vector3.Lerp(originalScale, newScale, elapsedTime / waitTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        this.transform.localPosition = newPos;
    }

    public void FlipCardRPC(float newAlpha, float totalTime, float rotation)
    {
        StartCoroutine(FlipCard(newAlpha, totalTime, rotation));
    }

    IEnumerator FlipCard(float newAlpha, float totalTime, float rotation)
    {
        if (flipping)
            yield break;

        flipping = true;
        transform.localEulerAngles = new Vector3(0, 0, rotation);
        float elapsedTime = 0f;

        Vector3 originalRot = this.transform.localEulerAngles;
        Vector3 newRot = new(0, 90, rotation);

        while (elapsedTime < totalTime)
        {
            this.transform.localEulerAngles = Vector3.Lerp(originalRot, newRot, elapsedTime / totalTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        this.layout.FillInCards(thisCard.dataFile, newAlpha, rotation);
        elapsedTime = 0f;

        while (elapsedTime < totalTime)
        {
            this.transform.localEulerAngles = Vector3.Lerp(newRot, originalRot, elapsedTime / totalTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        this.transform.localEulerAngles = originalRot;
        flipping = false;
    }

    private void FixedUpdate()
    {
        try { this.border.SetAlpha(PlayerCreator.inst.opacity); } catch { }
    }

    #endregion

#region Properties

    public void ChangeBoxRPC(Player player, int num, int logged)
    {
        if (num == 0)
            return;

        Log.inst.NewRollback(this, () => ChangeBox(false, player, num));
        if (num > 0)
            Log.inst.AddMyText($"Add Box-Player-{player.name}-Num-{num}-Card-{this.name}", false, logged);
        else
            Log.inst.AddMyText($"Remove Box-Player-{player.name}-Num-{Mathf.Abs(num)}-Card-{this.name}", false, logged);

        thisCard.WhenBoxOnThis(player, this, num, logged+1);
    }

    void ChangeBox(bool undo, Player player, int num)
    {
        int boxesHere = player.GetInt(BoxString());
        boxesHere += (undo) ? -num : num;
        player.WillChangeMasterProperty(BoxString(), boxesHere);
    }

    public void PlayToAreaRPC(Player player, int logged)
    {
        int currentArea = player.GetInt(AreaString());
        PlayerProp toPlay = AreaToListName(thisCard.dataFile.startingArea);
        foreach (Card card in player.GetCardList(toPlay.ToString()))
            card.thisCard.WhenPlayOther(player, card, this, logged + 1);

        Log.inst.NewRollback(this, () => PlayToArea(false, player, currentArea, thisCard.dataFile.startingArea));
        this.ChangeBoxRPC(player, thisCard.dataFile.startingBox, logged + 1);
        thisCard.WhenPlayThis(player, this, logged + 1);
    }

    void PlayToArea(bool undo, Player player, int oldLocation, CardAreas newArea)
    {
        List<Card> listOfArea = player.GetCardList(AreaToListName(newArea).ToString());
        if (undo)
        {
            listOfArea.Remove(this);
            player.WillChangeMasterProperty(AreaString(), oldLocation);
        }
        else
        {
            listOfArea.Add(this);
            player.WillChangeMasterProperty(AreaString(), (int)newArea);
        }
        player.WillChangeMasterProperty(newArea.ToString(), player.ConvertCardList(listOfArea));
    }

    public void MoveToAreaRPC(Player player, CardAreas newArea, int logged)
    {
        CardAreas myArea = (CardAreas)player.GetInt(AreaString());
        if (myArea == newArea)
            return;

        Log.inst.NewRollback(this, () => MoveToArea(false, player, myArea, newArea));
        this.thisCard.WhenThisMove(player, this, logged + 1);

        foreach (Card card in player.GetCardList(myArea.ToString()))
        {
            if (card != this)
                card.thisCard.WhenOtherMove(player, card, this, logged + 1);
        }

        foreach (Card card in player.GetCardList(newArea.ToString()))
        {
            if (card != this)
                card.thisCard.WhenOtherMove(player, card, this, logged + 1);
        }
    }

    void MoveToArea(bool undo, Player player, CardAreas removedFrom, CardAreas movedInto)
    {
        List<Card> listOfOldArea = player.GetCardList(AreaToListName(removedFrom).ToString());
        List<Card> listOfNewArea = player.GetCardList(AreaToListName(movedInto).ToString());

        if (undo)
        {
            listOfOldArea.Add(this);
            listOfNewArea.Remove(this);
            player.WillChangeMasterProperty(AreaString(), (int)removedFrom);
        }
        else
        {
            listOfOldArea.Remove(this);
            listOfNewArea.Add(this);
            player.WillChangeMasterProperty(AreaString(), (int)movedInto);
        }
        player.WillChangeMasterProperty(removedFrom.ToString(), player.ConvertCardList(listOfOldArea));
        player.WillChangeMasterProperty(movedInto.ToString(), player.ConvertCardList(listOfNewArea));
    }

    public void MakeDecision(Action action)
    {
        action();
    }

    #endregion

}