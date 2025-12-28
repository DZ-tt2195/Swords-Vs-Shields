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
            return (int)GetRoomProperty(ConstantStrings.CurrentPhase);
        }
        catch
        {
            return 0;
        }
    }

    public (int, Action) GetTurnAction(Player player)
    {
        int phase = GetCurrentPhase();
        //Debug.Log($"phase {phase}");
        return (phase, () => turnsInOrder[phase].ForPlayer(player));
    }

    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        bool HasPropertyAndValue(ExitGames.Client.Photon.Hashtable changedProps, string propertyName, object expected)
        {
            return (changedProps.ContainsKey(propertyName.ToString()) && changedProps[propertyName.ToString()].Equals(expected));
        }

        if (HasPropertyAndValue(changedProps, ConstantStrings.Waiting, true))
        {
            (List<Photon.Realtime.Player> players, List<Photon.Realtime.Player> spectators) = GetPlayers(false);
            int WaitingOnPlayers()
            {
                int playersWaiting = (int)GetRoomProperty(ConstantStrings.CanPlay);
                List<Photon.Realtime.Player> isWaiting = new();
                isWaiting.AddRange(spectators);

                foreach (Photon.Realtime.Player player in players)
                {
                    if ((bool)GetPlayerProperty(player, ConstantStrings.Waiting))
                    {
                        isWaiting.Add(player);
                        playersWaiting--;
                    }
                }

                UpdateWaitingText(isWaiting, playersWaiting);
                return playersWaiting;
            }

            if (PhotonNetwork.IsMasterClient && WaitingOnPlayers() == 0 && !(bool)GetRoomProperty(ConstantStrings.GameOver))
            {
                foreach (Photon.Realtime.Player nextPlayer in players)
                {
                    DoFunction(() => SharePropertyChanges(), nextPlayer);
                }

                turnsInOrder[GetCurrentPhase()].MasterEnd();
                UpdateWaitingText(spectators, players.Count);

                Invoke(nameof(NextPhase), 0.5f);
            }
        }
    }

    void UpdateWaitingText(List<Photon.Realtime.Player> toSend, int playersWaiting)
    {
        foreach (Photon.Realtime.Player player in toSend)
            MakeDecision.inst.DoFunction(() => MakeDecision.inst.Instructions("Waiting_on_Players", "", "", playersWaiting.ToString()), player);
    }

    void NextPhase()
    {
        //Debug.Log("next phase");
        PutInDiscard();
        void PutInDiscard()
        {
            foreach (Player player in CreateGame.inst.listOfPlayers)
            {
                List<Card> playerDiscard = GetCardList(ConstantStrings.MyDiscard, player);
                List<Card> myTroops = player.GetTroops();
                for (int i = myTroops.Count - 1; i >= 0; i--)
                {
                    Card card = myTroops[i];
                    if (card.GetHealth() <= 0)
                    {
                        InstantChangeRoomProp(card.HealthString(), 0);
                        myTroops.RemoveAt(i);
                        playerDiscard.Add(card);
                    }
                }
                InstantChangePlayerProp(player, ConstantStrings.MyTroops, ConvertCardList(myTroops));
                InstantChangePlayerProp(player, ConstantStrings.MyDiscard, ConvertCardList(playerDiscard));
            }
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

        if (leastHealth.Item2 <= 0)
        {
            if (leastHealth.Item1 != null)
                TextForEnding("Player_Lost", leastHealth.Item1.name, "", "", -1);
            else
                TextForEnding("Tie_Game", "", "", "", -1);
            InstantChangeRoomProp(ConstantStrings.CurrentPhase, turnsInOrder.Count - 1);
        }
        else
        {
            int phaseTracker = (int)GetRoomProperty(ConstantStrings.CurrentPhase);
            int roundTracker = (int)GetRoomProperty(ConstantStrings.CurrentRound);

            if (phaseTracker == turnsInOrder.Count - 2 || phaseTracker == 0)
            {
                InstantChangeRoomProp(ConstantStrings.CurrentRound, roundTracker + 1, roundTracker);
                InstantChangeRoomProp(ConstantStrings.CurrentPhase, 1, phaseTracker);
            }
            else
            {
                InstantChangeRoomProp(ConstantStrings.CurrentPhase, phaseTracker + 1, phaseTracker);
            }
        }
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey(ConstantStrings.CurrentPhase.ToString()))
        {
            if (PhotonNetwork.IsMasterClient)
                turnsInOrder[GetCurrentPhase()].MasterStart();

            CreateGame.inst.RefreshUI(true);
            foreach (Player player in CreateGame.inst.listOfPlayers)
            {
                if (player.photonView.AmOwner)
                    player.StartTurn();
            }
        }
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

    public int GetInt(string property, Player player) => (int)FindThisProperty(property, player);

    public int GetInt(string property) => (int)FindThisProperty(property, null);

    public List<string> GetStringList(string property, Player player)
    {
        string[] stringArray = (string[])FindThisProperty(property, player);
        return stringArray.ToList();
    }

    public List<Card> GetCardList(string property, Player player) => ConvertIntArray((int[])FindThisProperty(property, player));

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

    public void WillChangePlayerProperty(Player player, string playerProperty, object changeInto)
    {
        if (!playerPropertyToChange.ContainsKey(player))
            playerPropertyToChange.Add(player, new());

        if (playerPropertyToChange[player].ContainsKey(playerProperty.ToString()))
            playerPropertyToChange[player][playerProperty.ToString()] = changeInto;
        else
            playerPropertyToChange[player].Add(playerProperty.ToString(), changeInto);
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
        int currentPosition = (int)GetPlayerProperty(PhotonNetwork.LocalPlayer, ConstantStrings.MyPosition);

        foreach (var KVP in playerPropertyToChange)
        {
            KVP.Key.photonView.Owner.SetCustomProperties(KVP.Value);
            KVP.Value.Clear();
        }
        PhotonNetwork.CurrentRoom.SetCustomProperties(masterPropertyToChange);
        masterPropertyToChange.Clear();
    }

    #endregion

#region Ending

    public void TextForEnding(string toFind, string playerName, string cardName, string number, int resignPosition)
    {
        Log.inst.MasterText(true, toFind, playerName, cardName, number);
        Log.inst.MasterText(true, toFind, playerName, cardName, number);
        InstantChangeRoomProp(ConstantStrings.GameOver, true);
        DoFunction(() => ShowEnding(resignPosition), RpcTarget.All);
    }

    [PunRPC]
    void ShowEnding(int resignPosition)
    {
        endScreen.gameObject.SetActive(true);
        string text = "";

        foreach (Player player in CreateGame.inst.listOfPlayers)
        {
            text += $"{player.name} - {player.GetHealth()} {AutoTranslate.DoEnum(ToTranslate.Health)} ";
            if (player.myPosition == resignPosition)
                text += AutoTranslate.DoEnum(ToTranslate.Resigned);
            text += "\n";

            List<string> cardsPlayed = GetStringList(ConstantStrings.AllCardsPlayed, player);
            for (int i = 0; i<cardsPlayed.Count; i++)
            {
                string[] splitUp = cardsPlayed[i].Split('-');

                text += Translator.inst.Packaging("Played_Card_Info", "", splitUp[0], splitUp[1]);
                text += ",";
            }
            text += "\n\n";
        }
        summaryText.text = KeywordTooltip.instance.EditText(text);
    }

#endregion

}
