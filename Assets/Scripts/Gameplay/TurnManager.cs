using Photon.Pun;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;
using TMPro;

public class TurnManager : PhotonCompatible
{

#region Setup

    public static TurnManager inst;
    [SerializeField] List<Turn> turnsInOrder = new();
    [SerializeField] TMP_Text instructions;
    ExitGames.Client.Photon.Hashtable playerPropertyToChange;
    ExitGames.Client.Photon.Hashtable masterPropertyToChange;

    protected override void Awake()
    {
        base.Awake();
        inst = this;
        this.bottomType = this.GetType();
        playerPropertyToChange = new();
        masterPropertyToChange = new();
    }

    #endregion

#region Turns

    int GetCurrentPhase()
    {
        try
        {
            return (int)GetRoomProperty(RoomProp.CurrentPhase);
        }
        catch
        {
            return 0;
        }
    }

    public Action GetTurnAction(Player player)
    {
        return () => turnsInOrder[GetCurrentPhase()].ForPlayer(player);
    }

    [PunRPC]
    public string Instructions(int owner, string logText)
    {
        string answer = Translator.inst.SplitAndTranslate(owner, logText, 0);
        instructions.text = answer;
        return answer;
    }

    int WaitingOnPlayers()
    {
        (List<Photon.Realtime.Player> players, List<Photon.Realtime.Player> spectators) = GetPlayers(false);
        int playersWaiting = (int)GetRoomProperty(RoomProp.CanPlay);

        List<Photon.Realtime.Player> isWaiting = new();
        isWaiting.AddRange(spectators);

        foreach (Photon.Realtime.Player player in players)
        {
            if ((bool)GetPlayerProperty(player, PlayerProp.Waiting.ToString()))
            {
                isWaiting.Add(player);
                playersWaiting--;
            }
        }

        foreach (Photon.Realtime.Player player in isWaiting)
            DoFunction(() => Instructions(-1, $"Waiting on Players-Num-{playersWaiting}"), player);
        return playersWaiting;
    }

    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (HasPropertyAndValue(changedProps, PlayerProp.Waiting.ToString(), true))
        {
            int waiting = WaitingOnPlayers();
            if (PhotonNetwork.IsMasterClient && waiting == 0)
                AllPlayersDone();
        }
    }

    void AllPlayersDone()
    {
        (List<Photon.Realtime.Player> players, List<Photon.Realtime.Player> spectators) = GetPlayers(false);

        foreach (Photon.Realtime.Player nextPlayer in players)
            DoFunction(() => SharePropertyChanges(), nextPlayer);
        foreach (Photon.Realtime.Player player in spectators)
            DoFunction(() => Instructions(-1, $"Waiting on Players-Num-{players.Count}"), player);

        int phaseTracker = (int)GetRoomProperty(RoomProp.CurrentPhase);
        int roundTracker = (int)GetRoomProperty(RoomProp.CurrentRound);
        if (phaseTracker == 3 || phaseTracker == 0)
        {
            ChangeRoomProperties(RoomProp.CurrentPhase, 1, phaseTracker);
            ChangeRoomProperties(RoomProp.CurrentRound, roundTracker + 1, roundTracker);
        }
        else if (phaseTracker != 4)
        {
            ChangeRoomProperties(RoomProp.CurrentPhase, phaseTracker + 1, phaseTracker);
        }
        Invoke(nameof(NewPrompt), 0.25f);
    }

    void NewPrompt()
    {
        List<Card> masterDiscard = GetCardList(RoomProp.MasterDiscard.ToString());
        foreach (Player player in PlayerCreator.inst.listOfPlayers)
        {
            Photon.Realtime.Player photonPlayer = player.photonView.Controller;
            List<Card> myTroops = ConvertIntArray((int[])photonPlayer.CustomProperties[PlayerProp.MyTroops.ToString()]);
            for (int i = myTroops.Count - 1; i >= 0; i--)
            {
                Card card = myTroops[i];
                if (GetInt(card.HealthString().ToString()) <= 0)
                {
                    ChangeRoomProperties(card.HealthString(), 0);
                    myTroops.RemoveAt(i);
                    masterDiscard.Add(card);
                }
            }
            ChangePlayerProperties(photonPlayer, PlayerProp.MyTroops, ConvertCardList(myTroops));
        }
        ChangeRoomProperties(RoomProp.MasterDiscard, ConvertCardList(masterDiscard));

        turnsInOrder[GetCurrentPhase()].ForMaster();
        foreach (Player player in PlayerCreator.inst.listOfPlayers)
            player.DoFunction(() => player.StartTurn(), player.photonView.Owner);
    }

    bool HasPropertyAndValue(ExitGames.Client.Photon.Hashtable changedProps, string propertyName, object expected)
    {
        return (changedProps.ContainsKey(propertyName) && changedProps[propertyName].Equals(expected));
    }

    #endregion

#region Property Helpers

    object FindProperty(string property, Photon.Realtime.Player thisPlayer)
    {
        if (masterPropertyToChange.ContainsKey(property))
        {
            return masterPropertyToChange[property];
        }
        else if (playerPropertyToChange.ContainsKey(property))
        {
            return playerPropertyToChange[property];
        }
        else if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(property))
        {
            return GetRoomProperty(property);
        }
        else if (thisPlayer != null)
        {
            return GetPlayerProperty(thisPlayer, property);
        }
        else
        {
            foreach (Player player in PlayerCreator.inst.listOfPlayers)
            {
                if (player.photonView.Owner.CustomProperties.ContainsKey(property))
                    return GetPlayerProperty(player.photonView.Owner, property);
            }
            return null;
        }
    }

    public int GetInt(PlayerProp property, Player player)
    {
        return (int)FindProperty(property.ToString(), player.photonView.Owner);
    }

    public int GetInt(string property, Photon.Realtime.Player player = null)
    {
        return (int)FindProperty(property, player);
    }

    public List<Card> GetCardList(PlayerProp property, Player player)
    {
        return ConvertIntArray((int[])FindProperty(property.ToString(), player.photonView.Owner));
    }

    public List<Card> GetCardList(string property, Photon.Realtime.Player player = null)
    {
        return ConvertIntArray((int[])FindProperty(property, player));
    }

    List<Card> ConvertIntArray(int[] arrayOfPVs)
    {
        if (arrayOfPVs == null)
            return new();

        List<Card> listOfCards = new();
        foreach (int nextPV in arrayOfPVs)
            listOfCards.Add(PhotonView.Find(nextPV).GetComponent<Card>());
        return listOfCards;
    }

    public int[] ConvertCardList(List<Card> listOfCards)
    {
        int[] arrayOfCards = new int[listOfCards.Count];
        for (int i = 0; i < arrayOfCards.Length; i++)
            arrayOfCards[i] = listOfCards[i].photonView.ViewID;
        return arrayOfCards;
    }

    #endregion

#region Change Properties

    public void WillChangePlayerProperty(PlayerProp playerProperty, object changeInto)
    {
        WillChangePlayerProperty(playerProperty.ToString(), changeInto);
    }

    public void WillChangePlayerProperty(string playerProperty, object changeInto)
    {
        if (playerPropertyToChange.ContainsKey(playerProperty))
            playerPropertyToChange[playerProperty] = changeInto;
        else
            playerPropertyToChange.Add(playerProperty, changeInto);
    }

    public void WillChangeMasterProperty(RoomProp roomProperty, object changeInto)
    {
        WillChangeMasterProperty(roomProperty.ToString(), changeInto);
    }

    public void WillChangeMasterProperty(string masterProperty, object changeInto)
    {
        if (masterPropertyToChange.ContainsKey(masterProperty))
            masterPropertyToChange[masterProperty] = changeInto;
        else
            masterPropertyToChange.Add(masterProperty, changeInto);
    }

    [PunRPC]
    void SharePropertyChanges()
    {
        Log.inst.ShareTexts();
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerPropertyToChange);
        playerPropertyToChange.Clear();
        PhotonNetwork.CurrentRoom.SetCustomProperties(masterPropertyToChange);
        masterPropertyToChange.Clear();

        //send away discarded cards
        List<Card> masterDiscard = GetCardList(RoomProp.MasterDiscard.ToString());
        masterDiscard.AddRange(GetCardList(PlayerProp.MyDiscard.ToString()));

        ChangePlayerProperties(PhotonNetwork.LocalPlayer, PlayerProp.MyDiscard, new int[0]);
        ChangeRoomProperties(RoomProp.MasterDiscard, ConvertCardList(masterDiscard));
    }

    #endregion

}
