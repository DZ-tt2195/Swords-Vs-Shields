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

        if (!PhotonNetwork.OfflineMode)
        {
            string playerName = PlayerPrefs.GetString(ConstantStrings.MyUserName);

            if (PlayerPrefs.GetString(ConstantStrings.LastRoom).Equals(PhotonNetwork.CurrentRoom.Name))
            {
                CommHub.inst.ShareMessageRPC("Player_Reconnected", playerName, "", "", true);
            }
            else if ((bool)GetRoomProperty(ConstantStrings.JoinAsSpec))
            {
                CommHub.inst.ShareMessageRPC("Player_Spectating", playerName, "", "", true);
                ExitGames.Client.Photon.Hashtable playerProps = new()
                {
                    [ConstantStrings.Waiting] = true,
                    [ConstantStrings.MyPosition] = -1,
                };
                PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);
                StartCoroutine(Wait());
            }
            else
            {
                CommHub.inst.ShareMessageRPC("Player_Playing", playerName, "", "", true);
                PlayerPrefs.SetString(ConstantStrings.LastRoom, PhotonNetwork.CurrentRoom.Name);
                MakePlayerAndCards();

                if (PhotonNetwork.CurrentRoom.Players.Count == (int)GetRoomProperty(ConstantStrings.CanPlay))
                    InstantChangeRoomProp(ConstantStrings.JoinAsSpec, true, false);
            }
        }
        else
        {
            PlayerPrefs.DeleteKey(ConstantStrings.LastRoom);
            InstantChangeRoomProp(ConstantStrings.CanPlay, 1);
            MakePlayerAndCards();
        }

        IEnumerator Wait()
        {
            yield return new WaitForSeconds(1.5f);
            RefreshUI(true);
        }

        void MakePlayerAndCards()
        {
            int nextPlayerPosition = (int)GetRoomProperty(ConstantStrings.NextPlayerPosition);
            ExitGames.Client.Photon.Hashtable playerProps = new()
            {
                [ConstantStrings.Waiting] = false,
                [ConstantStrings.MyPosition] = nextPlayerPosition,
            };
            InstantChangeRoomProp(ConstantStrings.NextPlayerPosition, nextPlayerPosition + 1);

            List<int> startingDeck = new();
            List<int> cardID = new();

            for (int i = 0; i < GameFiles.inst.playerCardFiles.Count; i++)
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
            playerProps.Add(ConstantStrings.MyDeck, startingDeck.ToArray());
            PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);

            MakeObject(playerPrefab.gameObject);
        }
    }

    [PunRPC]
    void CreateCards(int[] arrayOfPVs, int[] cardNames)
    {
        for (int i = 0; i<arrayOfPVs.Length; i++)
        {
            GameObject obj = PhotonView.Find(arrayOfPVs[i]).gameObject;
            obj.GetComponent<Card>().AssignCard(GameFiles.inst.playerCardFiles[cardNames[i]], 0f);
        }
    }

    public void RefreshUI(bool forced)
    {
        Log.inst.ChangeScrolling();
        foreach (Player player in listOfPlayers)
            player.UpdateUI(forced);
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

    public Player OtherPlayer(int playerPosition)
    {
        if (playerPosition == 0)
            return listOfPlayers[1];
        else
            return listOfPlayers[0];
    }

    public PlayerUI GetUI(int playerPosition)
    {
        int myPosition = (int)GetPlayerProperty(PhotonNetwork.LocalPlayer, ConstantStrings.MyPosition);
        if (myPosition == playerPosition)
            return allUI[0];
        else if (myPosition == -1)
            return allUI[playerPosition];
        else
            return allUI[1];
    }

    #endregion

}
