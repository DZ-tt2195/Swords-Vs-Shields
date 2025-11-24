using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System;
using Photon.Realtime;
using System.Collections;
using MyBox;
using UnityEngine.UI;
using TMPro;

[Serializable]
public class PlayerUI
{
    public Image image;
    public TMP_Text infoText;
    public List<MiniCardDisplay> cardDisplays = new();
}

public class CreateGame : PhotonCompatible
{

#region Setup

    public static CreateGame inst;
    [Foldout("Players", true)]
    [ReadOnly] public List<Player> listOfPlayers = new();
    [SerializeField] Player playerPrefab;
    [SerializeField] Card cardPrefab;

    [Foldout("UI and Animation", true)]
    public Camera mainCamera;
    public float opacity { get; private set; }
    bool decrease = true;
    public Canvas canvas { get; private set; }
    [SerializeField] List<PlayerUI> allUI = new();

    protected override void Awake()
    {
        base.Awake();
        this.bottomType = this.GetType();
        inst = this;
        PhotonNetwork.AutomaticallySyncScene = true;
        canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        foreach (PlayerUI ui in allUI)
        {
            foreach (MiniCardDisplay display in ui.cardDisplays)
                display.gameObject.SetActive(false);
        }
        Invoke(nameof(Setup), 0.25f);
    }

    void Setup()
    {
        if (!PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(ConstantStrings.MasterDeck))
            CreateRoom();

        if (!PhotonNetwork.OfflineMode)
        {
            string playerName = PlayerPrefs.GetString("Online Username");

            if (PlayerPrefs.GetString("LastRoom").Equals(PhotonNetwork.CurrentRoom.Name))
            {
                CommHub.inst.ShareMessageRPC($"Player Reconnected-Player-{playerName}", true);
            }
            else if ((bool)GetRoomProperty(ConstantStrings.JoinAsSpec))
            {
                CommHub.inst.ShareMessageRPC($"Player Spectating-Player-{playerName}", true);
                ExitGames.Client.Photon.Hashtable playerProps = new()
                {
                    [ConstantStrings.Waiting] = true,
                    [ConstantStrings.Position] = -1,
                };
                PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);
            }
            else
            {
                CommHub.inst.ShareMessageRPC($"Player Playing-Player-{playerName}", true);
                CreatePlayer();

                PlayerPrefs.SetString("LastRoom", PhotonNetwork.CurrentRoom.Name);
                if (PhotonNetwork.CurrentRoom.Players.Count == (int)GetRoomProperty(ConstantStrings.CanPlay))
                    InstantChangeRoomProp(ConstantStrings.JoinAsSpec, true, false);
            }
        }
        else
        {
            PlayerPrefs.DeleteKey("LastRoom");
            InstantChangeRoomProp(ConstantStrings.CanPlay, 1);
            CreatePlayer();
        }
    }

    [PunRPC]
    void CreateCards(int[] arrayOfPVs, int[] cardNames)
    {
        for (int i = 0; i<arrayOfPVs.Length; i++)
        {
            GameObject obj = PhotonView.Find(arrayOfPVs[i]).gameObject;
            obj.GetComponent<Card>().AssignCard(Translator.inst.playerCardFiles[cardNames[i]], 0f);
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
            [ConstantStrings.GameOver] = false,
            [ConstantStrings.MasterDiscard] = new int[0],
            [ConstantStrings.CurrentPhase] = 0,
            [ConstantStrings.CurrentRound] = 0,
        };
        List<int> startingDeck = new();
        List<int> cardID = new();

        for (int i = 0; i < Translator.inst.playerCardFiles.Count; i++)
        {
            for (int j = 0; j < 1; j++)
            {
                GameObject nextCard = MakeObject(cardPrefab.gameObject);
                PhotonView cardPV = nextCard.GetComponent<PhotonView>();

                startingDeck.Add(cardPV.ViewID);
                cardID.Add(i);
            }
        }
        DoFunction(() => CreateCards(startingDeck.ToArray(), cardID.ToArray()));

        startingDeck = startingDeck.Shuffle();
        initialProps.Add(ConstantStrings.MasterDeck, startingDeck.ToArray());
        PhotonNetwork.CurrentRoom.SetCustomProperties(initialProps);
    }

    void CreatePlayer()
    {
        int count = listOfPlayers.Count;
        ExitGames.Client.Photon.Hashtable playerProps = new()
        {
            [ConstantStrings.Waiting] = false,
            [ConstantStrings.Position] = count,
            [ConstantStrings.MyHealth] = 20,

            [ConstantStrings.Shield] = 0,
            [ConstantStrings.Sword] = 0,
            [ConstantStrings.Action] = 0,

            [ConstantStrings.NextRoundSword] = 0,
            [ConstantStrings.NextRoundShield] = 0,
            [ConstantStrings.NextRoundAction] = 0,

            [ConstantStrings.MyHand] = new int[0],
            [ConstantStrings.MyDeck] = new int[0],
            [ConstantStrings.MyDiscard] = new int[0],
            [ConstantStrings.MyTroops] = new int[0],
            [ConstantStrings.AllCardsPlayed] = new string[0],
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);
        MakeObject(playerPrefab.gameObject);
    }

    public Player OtherPlayer(int playerPosition)
    {
        if (playerPosition == 0)
            return listOfPlayers[1];
        else
            return listOfPlayers[0];
    }

    public PlayerUI GetUI(int playerPosition)
    {
        int myPosition = (int)GetPlayerProperty(PhotonNetwork.LocalPlayer, ConstantStrings.Position);
        if (myPosition == playerPosition)
            return allUI[0];
        else if (myPosition == -1)
            return allUI[playerPosition];
        else
            return allUI[1];
    }

    #endregion

}
