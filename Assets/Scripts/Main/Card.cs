using UnityEngine;
using MyBox;
using System.Collections;
using System.Linq;
using System;
using Photon.Pun;
using System.Collections.Generic;

public class Card : PhotonCompatible
{

#region Setup

    public CardLayout layout { get; private set; }
    bool flipping;
    public CardType thisCard { get; private set; }
    public ButtonSelect selectMe { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        this.bottomType = this.GetType();

        Canvas canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        this.transform.localScale = Vector3.Lerp(Vector3.one, canvas.transform.localScale, 0.5f);
        selectMe = GetComponent<ButtonSelect>();
        layout = GetComponent<CardLayout>();

        if (PhotonNetwork.IsConnected && !PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(HealthString()))
        {
            ExitGames.Client.Photon.Hashtable initialProps = new()
            {
                [HealthString()] = 0,
                [StunString()] = new int[0],
                [ProtectString()] = new int[0],
            };
            PhotonNetwork.CurrentRoom.SetCustomProperties(initialProps);
        }
    }

    public string HealthString() => $"{this.photonView.ViewID}_Health";

    public string StunString() => $"{this.photonView.ViewID}_Stun";

    public string ProtectString() => $"{this.photonView.ViewID}_Protect";

    public void AssignCard(CardData dataFile, float startingAlpha)
    {
        string noSpaces = dataFile.cardName.Replace(" ", "");
        thisCard = (CardType)Activator.CreateInstance(Type.GetType(noSpaces), dataFile);

        this.layout.FillInCards(dataFile, startingAlpha, 0);
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
        if (flipping || this.layout.GetAlpha() == newAlpha)
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

    #endregion

#region Properties

    public void StunRPC(int increment, int logged = 0)
    {
        int roundNumber = (int)GetRoomProperty(RoomProp.CurrentRound) + increment;
        Log.inst.AddMyText($"Stun Card-Card-{this.name}-Num-{roundNumber}", false, logged);
        Log.inst.NewRollback(() => AddToArray(roundNumber, StunString()));
    }

    public bool CanUseAbility()
    {
        int currentRound = (int)GetRoomProperty(RoomProp.CurrentRound);
        int[] stunArray = (int[])GetRoomProperty(StunString());
        return !(stunArray.Contains(currentRound));
    }

    public void ProtectRPC(int increment, int logged = 0)
    {
        int roundNumber = (int)GetRoomProperty(RoomProp.CurrentRound) + increment;
        Log.inst.AddMyText($"Protect Card-Card-{this.name}-Num-{roundNumber}", false, logged);
        Log.inst.NewRollback(() => AddToArray(roundNumber, ProtectString()));
    }

    public bool CanTakeDamage()
    {
        int currentRound = (int)GetRoomProperty(RoomProp.CurrentRound);
        int[] protectArray = (int[])GetRoomProperty(ProtectString());
        return !(protectArray.Contains(currentRound));
    }

    void AddToArray(int round, string list)
    {
        List<int> convertedList = ((int[])GetRoomProperty(list)).ToList();
        if (Log.inst.forward)
            convertedList.Add(round);
        else
            convertedList.Remove(round);
        TurnManager.inst.WillChangeMasterProperty(list, convertedList.ToArray());
    }

    public void HealthRPC(Player player, int num, int logged = 0)
    {
        if (num == 0 || !CanTakeDamage())
            return;
        if (num > 0)
            Log.inst.AddMyText($"Add Health Card-Player-{player.name}-Card-{this.name}-Num-{num}", false, logged);
        else
            Log.inst.AddMyText($"Lose Health Card-Player-{player.name}-Card-{this.name}-Num-{Mathf.Abs(num)}", false, logged);
        Log.inst.NewRollback(() => ChangeInt(num, HealthString()));
    }

    public int GetHealth() => TurnManager.inst.GetInt(this.HealthString());

    void ChangeInt(int num, string property)
    {
        int total = TurnManager.inst.GetInt(property);
        total += (!Log.inst.forward) ? -num : num;
        TurnManager.inst.WillChangeMasterProperty(property, total);
    }

    #endregion

}