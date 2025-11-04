using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Photon.Realtime;
using System.Collections;
using MyBox;
using TMPro;

public enum RoomProp { Game, CanPlay, JoinAsSpec, MasterDeck, MasterDiscard, CurrentPhase, CurrentRound, P0_Health, P1_Health }

public class PlayerCreator : PhotonCompatible
{

#region Setup

    public static PlayerCreator inst;
    public List<Player> listOfPlayers = new();

    [Foldout("UI and Animation", true)]
    public Camera mainCamera;
    public float opacity { get; private set; }
    bool decrease = true;
    public Canvas canvas { get; private set; }
    [SerializeField] List<PlayerDisplay> playerDisplays = new();
    [SerializeField] List<MiniCardDisplay> cardDisplayOne = new();
    [SerializeField] List<MiniCardDisplay> cardDisplayTwo = new();

    protected override void Awake()
    {
        base.Awake();
        this.bottomType = this.GetType();
        inst = this;
        PhotonNetwork.AutomaticallySyncScene = true;
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();

        foreach (MiniCardDisplay display in cardDisplayOne)
            display.gameObject.SetActive(false);
        foreach (MiniCardDisplay display in cardDisplayTwo)
            display.gameObject.SetActive(false);

        Invoke(nameof(Setup), 0.25f);
    }

    void Setup()
    {
        if (!PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(RoomProp.MasterDeck.ToString()))
            CreateRoom();

        if (!PhotonNetwork.OfflineMode)
        {
            string playerName = PlayerPrefs.GetString("Online Username");

            if (PlayerPrefs.GetString("LastRoom").Equals(PhotonNetwork.CurrentRoom.Name))
            {
                CommHub.inst.ShareMessageRPC($"Player Reconnected-Player-{playerName}", true);
            }
            else if ((bool)GetRoomProperty(RoomProp.JoinAsSpec))
            {
                CommHub.inst.ShareMessageRPC($"Player Spectating-Player-{playerName}", true);
                ExitGames.Client.Photon.Hashtable playerProps = new()
                {
                    [PlayerProp.Waiting.ToString()] = true,
                    [PlayerProp.Position.ToString()] = -1,
                };
                PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);
            }
            else
            {
                CommHub.inst.ShareMessageRPC($"Player Playing-Player-{playerName}", true);
                CreatePlayer();

                PlayerPrefs.SetString("LastRoom", PhotonNetwork.CurrentRoom.Name);
                if (PhotonNetwork.CurrentRoom.Players.Count == (int)PhotonNetwork.CurrentRoom.CustomProperties[RoomProp.CanPlay.ToString()])
                    ChangeRoomProperties(RoomProp.JoinAsSpec, true, false);
            }
        }
        else
        {
            PlayerPrefs.DeleteKey("LastRoom");
            ChangeRoomProperties(RoomProp.CanPlay, 1);
            CreatePlayer();
        }
    }

    [PunRPC]
    void CreateCards(int[] arrayOfPVs, int[] cardNames)
    {
        for (int i = 0; i<arrayOfPVs.Length; i++)
        {
            GameObject obj = PhotonView.Find(arrayOfPVs[i]).gameObject;
            obj.GetComponent<Card>().AssignCard(Translator.inst.playerCardFiles[cardNames[i]]);
        }
    }

    #endregion

#region Online

    public void Leave()
    {
        PhotonNetwork.OfflineMode = false;
        PhotonNetwork.LeaveRoom(false);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        OnLeftRoom();
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("0. Loading");
    }

    #endregion

#region Misc

    private void FixedUpdate()
    {
        if (decrease)
            opacity -= 0.05f;
        else
            opacity += 0.05f;
        if (opacity < 0 || opacity > 1)
            decrease = !decrease;
    }

    void CreateRoom()
    {
        ExitGames.Client.Photon.Hashtable initialProps = new()
        {
            [RoomProp.MasterDiscard.ToString()] = new int[0],
            [RoomProp.CurrentPhase.ToString()] = 0,
            [RoomProp.CurrentRound.ToString()] = 0,
            [RoomProp.P0_Health.ToString()] = 20,
            [RoomProp.P1_Health.ToString()] = 20,
        };
        List<int> startingDeck = new();
        List<int> cardID = new();

        for (int i = 0; i < Translator.inst.playerCardFiles.Count; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                GameObject nextCard = MakeObject(CarryVariables.inst.cardPrefab.gameObject);
                PhotonView cardPV = nextCard.GetComponent<PhotonView>();

                startingDeck.Add(cardPV.ViewID);
                cardID.Add(i);
                initialProps.Add($"{cardPV.ViewID}_Box", 0);
            }
        }
        DoFunction(() => CreateCards(startingDeck.ToArray(), cardID.ToArray()));

        startingDeck = startingDeck.Shuffle();
        initialProps.Add(RoomProp.MasterDeck.ToString(), startingDeck.ToArray());
        PhotonNetwork.CurrentRoom.SetCustomProperties(initialProps);
    }

    void CreatePlayer()
    {
        int count = listOfPlayers.Count;
        ExitGames.Client.Photon.Hashtable playerProps = new()
        {
            [PlayerProp.Waiting.ToString()] = false,
            [PlayerProp.Position.ToString()] = count,

            [PlayerProp.GreenCoin.ToString()] = 0,
            [PlayerProp.RedCoin.ToString()] = 0,
            [PlayerProp.Action.ToString()] = 0,

            [PlayerProp.MyHand.ToString()] = new int[0],
            [PlayerProp.MyDeck.ToString()] = new int[0],
            [PlayerProp.MyDiscard.ToString()] = new int[0],
            [PlayerProp.MyTroops.ToString()] = new int[0],
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);
        MakeObject(CarryVariables.inst.playerPrefab.gameObject);
    }

    public (PlayerDisplay, List<MiniCardDisplay>) PlayerUI(int playerPosition)
    {
        if (playerPosition == 0)
            return (playerDisplays[0], cardDisplayOne);
        else
            return (playerDisplays[1], cardDisplayTwo);
    }

    #endregion

}
