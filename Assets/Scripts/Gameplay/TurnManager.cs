using Photon.Pun;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;
using TMPro;

public class TurnManager : PhotonCompatible
{
    public static TurnManager inst;
    [SerializeField] List<Turn> turnsInOrder = new();
    [SerializeField] TMP_Text instructions;

    protected override void Awake()
    {
        base.Awake();
        inst = this;
        this.bottomType = this.GetType();
    }

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

    public void DoTurnAction(Player player)
    {
        turnsInOrder[GetCurrentPhase()].ForPlayer(player);
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
            {
                (List<Photon.Realtime.Player> players, List<Photon.Realtime.Player> spectators) = GetPlayers(false);

                foreach (var KVP in PlayerCreator.inst.playerDictionary)
                    KVP.Value.DoFunction(() => KVP.Value.SharePropertyChanges(), KVP.Key);
                foreach (Photon.Realtime.Player player in spectators)
                    DoFunction(() => Instructions(-1, $"Waiting on Players-Num-{players.Count}"), player);

                int phaseTracker = (int)GetRoomProperty(RoomProp.CurrentPhase);
                int roundTracker = (int)GetRoomProperty(RoomProp.CurrentRound);
                if (phaseTracker == 3)
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
        }
    }

    void NewPrompt()
    {
        turnsInOrder[GetCurrentPhase()].ForMaster();
        foreach (var KVP in PlayerCreator.inst.playerDictionary)
            KVP.Value.DoFunction(() => KVP.Value.StartTurn(), KVP.Key);
    }

    bool HasPropertyAndValue(ExitGames.Client.Photon.Hashtable changedProps, string propertyName, object expected)
    {
        return (changedProps.ContainsKey(propertyName) && changedProps[propertyName].Equals(expected));
    }
    /*
    void PlayerDraw(Player player)
    {
        Log.inst.NewDecisionContainer(this, () => InstantDraw(player), 0);
    }

    void InstantDraw(Player player)
    {
        player.ChooseButtonInPopup(new() { new("Draw") }, "Draw a card?", new(0, 325), false);
        Log.inst.inReaction.Add(ChoiceMade);

        void ChoiceMade()
        {
            player.DrawCardRPC(1, 0);
        }
    }
    */
}
