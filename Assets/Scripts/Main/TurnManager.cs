using Photon.Pun;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;
using TMPro;
using System.Linq;

public class TurnManager : PhotonCompatible
{

#region Setup

    public static TurnManager inst;
    [SerializeField] List<Turn> turnsInOrder = new();
    Dictionary<Player, ExitGames.Client.Photon.Hashtable> playerPropertyToChange;
    ExitGames.Client.Photon.Hashtable masterPropertyToChange;

    [SerializeField] Transform endScreen;
    [SerializeField] TMP_Text summaryText;

    protected override void Awake()
    {
        base.Awake();
        inst = this;
        this.bottomType = this.GetType();
        endScreen.gameObject.SetActive(false);
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

    public (int, Action) GetTurnAction(Player player)
    {
        int phase = GetCurrentPhase();
        return (phase, () => turnsInOrder[phase].ForPlayer(player));
    }

    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (HasPropertyAndValue(changedProps, PlayerProp.Waiting.ToString(), true))
        {
            int waiting = WaitingOnPlayers();

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

                UpdateWaitingText(isWaiting, playersWaiting);
                return playersWaiting;
            }

            if (PhotonNetwork.IsMasterClient && waiting == 0 && !(bool)GetRoomProperty(RoomProp.GameOver))
                AllPlayersDone();
        }
    }

    void UpdateWaitingText(List<Photon.Realtime.Player> toSend, int playersWaiting)
    {
        foreach (Photon.Realtime.Player player in toSend)
            MakeDecision.inst.DoFunction(() => MakeDecision.inst.Instructions($"Waiting on Players-Num-{playersWaiting}"), player);
    }

    void AllPlayersDone()
    {
        (List<Photon.Realtime.Player> players, List<Photon.Realtime.Player> spectators) = GetPlayers(false);
        foreach (Photon.Realtime.Player nextPlayer in players)
        {
            DoFunction(() => SharePropertyChanges(), nextPlayer);
        }

        turnsInOrder[GetCurrentPhase()].MasterEnd();
        UpdateWaitingText(spectators, players.Count);

        Invoke(nameof(NextPhase), 0.5f);
    }

    void NextPhase()
    {
        PutInDiscard();
        void PutInDiscard()
        {
            List<Card> masterDiscard = GetCardList(RoomProp.MasterDiscard.ToString());
            foreach (Player player in CreateGame.inst.listOfPlayers)
            {
                List<Card> myTroops = player.GetTroops();
                for (int i = myTroops.Count - 1; i >= 0; i--)
                {
                    Card card = myTroops[i];
                    if (card.GetHealth() <= 0)
                    {
                        InstantChangeRoomProp(card.HealthString(), 0);
                        myTroops.RemoveAt(i);
                        masterDiscard.Add(card);
                    }
                }
                InstantChangePlayerProp(player, PlayerProp.MyTroops, ConvertCardList(myTroops));
            }
            InstantChangeRoomProp(RoomProp.MasterDiscard, ConvertCardList(masterDiscard));
            DoFunction(() => DiscardToNull(), RpcTarget.All);
        }

        (Player, int) leastHealth = (null, 1000);
        foreach (Player player in CreateGame.inst.listOfPlayers)
        {
            int health = player.GetHealth();
            if (health < leastHealth.Item2)
                leastHealth = (player, health);
            else if (health == leastHealth.Item2)
                leastHealth = (null, health);
        }

        if (leastHealth.Item1 != null && leastHealth.Item2 <= 0)
        {
            TextForEnding($"Player Lost-Player-{leastHealth.Item1.name}", -1);
            InstantChangeRoomProp(RoomProp.CurrentPhase, turnsInOrder.Count - 1);
        }
        else
        {
            int phaseTracker = (int)GetRoomProperty(RoomProp.CurrentPhase);
            int roundTracker = (int)GetRoomProperty(RoomProp.CurrentRound);

            if (phaseTracker == turnsInOrder.Count - 2 || phaseTracker == 0)
            {
                InstantChangeRoomProp(RoomProp.CurrentRound, roundTracker + 1, roundTracker);
                InstantChangeRoomProp(RoomProp.CurrentPhase, 1, phaseTracker);
            }
            else
            {
                InstantChangeRoomProp(RoomProp.CurrentPhase, phaseTracker + 1, phaseTracker);
            }
        }
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey(RoomProp.CurrentPhase.ToString()))
        {
            if (PhotonNetwork.IsMasterClient)
            {
                turnsInOrder[GetCurrentPhase()].MasterStart();
            }
            foreach (Player player in CreateGame.inst.listOfPlayers)
            {
                if (player.photonView.AmOwner)
                    player.StartTurn();
            }
        }
    }

    [PunRPC]
    void DiscardToNull()
    {
        List<Card> masterDiscard = GetCardList(RoomProp.MasterDiscard.ToString());
        foreach (Card card in masterDiscard)
            card.transform.SetParent(null);
    }

    bool HasPropertyAndValue(ExitGames.Client.Photon.Hashtable changedProps, string propertyName, object expected)
    {
        return (changedProps.ContainsKey(propertyName.ToString()) && changedProps[propertyName.ToString()].Equals(expected));
    }

    #endregion

#region Property Helpers

    object FindThisProperty(string property, Player player)
    {
        if (player != null && !playerPropertyToChange.ContainsKey(player))
            playerPropertyToChange.Add(player, new());

        if (masterPropertyToChange.ContainsKey(property))
            return masterPropertyToChange[property];
        else if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(property))
            return GetRoomProperty(property);

        if (playerPropertyToChange[player].ContainsKey(property))
            return playerPropertyToChange[player][property];
        else
            return GetPlayerProperty(player.photonView.Owner, property);
    }

    public int GetInt(PlayerProp property, Player player) => (int)FindThisProperty(property.ToString(), player);

    public int GetInt(string property) => (int)FindThisProperty(property, null);

    public List<string> GetStringList(PlayerProp property, Player player)
    {
        string[] stringArray = (string[])FindThisProperty(property.ToString(), player);
        return stringArray.ToList();
    }

    public List<Card> GetCardList(PlayerProp property, Player player) => ConvertIntArray((int[])FindThisProperty(property.ToString(), player));

    public List<Card> GetCardList(string property) => ConvertIntArray((int[])FindThisProperty(property, null));

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

    public void WillChangePlayerProperty(Player player, PlayerProp playerProperty, object changeInto)
    {
        if (!playerPropertyToChange.ContainsKey(player))
            playerPropertyToChange.Add(player, new());

        if (playerPropertyToChange[player].ContainsKey(playerProperty.ToString()))
            playerPropertyToChange[player][playerProperty.ToString()] = changeInto;
        else
            playerPropertyToChange[player].Add(playerProperty.ToString(), changeInto);
    }

    public void WillChangeMasterProperty(RoomProp roomProperty, object changeInto) => WillChangeMasterProperty(roomProperty.ToString(), changeInto);

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
        int currentPosition = (int)PhotonNetwork.LocalPlayer.CustomProperties[PlayerProp.Position.ToString()];

        foreach (var KVP in playerPropertyToChange)
        {
            KVP.Key.photonView.Owner.SetCustomProperties(KVP.Value);
            KVP.Value.Clear();
        }
        PhotonNetwork.CurrentRoom.SetCustomProperties(masterPropertyToChange);
        masterPropertyToChange.Clear();

        //send away discarded cards
        List<Card> masterDiscard = GetCardList(RoomProp.MasterDiscard.ToString());
        masterDiscard.AddRange(GetCardList(PlayerProp.MyDiscard, CreateGame.inst.listOfPlayers[currentPosition]));

        InstantChangePlayerProp(PhotonNetwork.LocalPlayer, PlayerProp.MyDiscard, new int[0]);
        InstantChangeRoomProp(RoomProp.MasterDiscard, ConvertCardList(masterDiscard));
    }

    #endregion

#region Ending

    public void TextForEnding(string logText, int resignPosition)
    {
        Log.inst.MasterText("Blank");
        Log.inst.MasterText(logText);
        InstantChangeRoomProp(RoomProp.GameOver, true);
        DoFunction(() => ShowEnding(resignPosition), RpcTarget.All);
    }

    [PunRPC]
    void ShowEnding(int resignPosition)
    {
        endScreen.gameObject.SetActive(true);
        string text = "";

        foreach (Player player in CreateGame.inst.listOfPlayers)
        {
            text += $"{player.name} - {player.GetHealth()} Health ";
            if (player.myPosition == resignPosition)
                text += Translator.inst.Translate("Resigned");
            text += "\n";

            List<string> cardsPlayed = GetStringList(PlayerProp.AllCardsPlayed, player);
            for (int i = 0; i<cardsPlayed.Count; i++)
            {
                text += Translator.inst.SplitAndTranslate(-1, cardsPlayed[i]);
                text += ",";
            }
            text += "\n\n";
        }
        summaryText.text = KeywordTooltip.instance.EditText(text);
    }

#endregion

}
