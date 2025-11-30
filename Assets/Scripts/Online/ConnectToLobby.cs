using UnityEngine;
using Photon.Pun;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using Photon.Realtime;
using UnityEngine.UI;
using MyBox;

public class ConnectToLobby : MonoBehaviourPunCallbacks
{

#region Setup

    [Foldout("General", true)]
    [SerializeField] TMP_Text error;

    [Foldout("Part 1", true)]
    [SerializeField] Transform part1;
    [SerializeField] TMP_InputField username;
    [SerializeField] Button reconnectButton;
    [SerializeField] TMP_Dropdown regionDropdown;
    List<(string, string)> regionAndCode = new();

    [Foldout("Part 2", true)]
    [SerializeField] Transform part2;
    [SerializeField] Transform keepJoinButtons;
    [SerializeField] TMP_InputField joinInput;
    [SerializeField] Button joinManually;
    [SerializeField] Button disconnectButton;
    List<JoinRoomButton> listOfJoinButtons = new();

    private void Start()
    {
        part2.gameObject.SetActive(true);
        joinManually.onClick.AddListener(() => JoinRoom(joinInput.text));
        disconnectButton.onClick.AddListener(() => PhotonNetwork.Disconnect());

        foreach (Transform child in keepJoinButtons)
            listOfJoinButtons.Add(child.GetComponent<JoinRoomButton>());
        foreach (JoinRoomButton button in listOfJoinButtons)
            button.ClearInfo();

        username.text = PlayerPrefs.GetString(ConstantStrings.MyUserName);
        error.gameObject.SetActive(false);
        part1.gameObject.SetActive(true);
        part2.gameObject.SetActive(false);

        reconnectButton.gameObject.SetActive(PlayerPrefs.HasKey(ConstantStrings.LastRoom));

        regionAndCode = new()
        {
            (Translator.inst.Translate("US West Coast"), "usw"),
            (Translator.inst.Translate("US East Coast"), "us"),
            (Translator.inst.Translate("Europe"), "eu"),
            (Translator.inst.Translate("Asia"), "asia")
        };
        foreach ((string, string) var in regionAndCode)
            regionDropdown.AddOptions(new List<string>() { var.Item1 });
    }

    IEnumerator ErrorMessage(string text, List<(string, string)> toReplace = null)
    {
        error.text = Translator.inst.Translate(text, toReplace);
        float elapsedTime = 0f;
        while (elapsedTime < 3f)
        {
            error.gameObject.SetActive(true);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        error.gameObject.SetActive(false);
    }

    #endregion

#region Part 1

    bool CheckUsername()
    {
        string newName = username.text.Trim();
        if (newName == "")
        {
            StartCoroutine(ErrorMessage("Type in username"));
            return false;
        }
        else
        {
            PlayerPrefs.SetString(ConstantStrings.MyUserName, newName);
            PlayerPrefs.Save();
            PhotonNetwork.NickName = PlayerPrefs.GetString(ConstantStrings.MyUserName);
            return true;
        }
    }

    public void Join()
    {
        if (CheckUsername())
        {
            foreach ((string, string) var in regionAndCode)
            {
                if (var.Item1.Equals(regionDropdown.options[regionDropdown.value].text))
                    PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = var.Item2;
            }
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public void OfflineRoom()
    {
        if (CheckUsername())
        { 
            PhotonNetwork.OfflineMode = true;
            PhotonNetwork.CreateRoom("");
            PhotonNetwork.LoadLevel("2. Game");
        }
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        PlayerPrefs.DeleteKey(ConstantStrings.LastRoom);
        PhotonNetwork.OfflineMode = false;

        error.gameObject.SetActive(false);
        part1.gameObject.SetActive(false);
        part2.gameObject.SetActive(true);
    }

    public void Reconnect()
    {
        StartCoroutine(ErrorMessage("Attempt to reconnect", new() { ("Room", PlayerPrefs.GetString(ConstantStrings.LastRoom))}));
        StartCoroutine(Delay());

        IEnumerator Delay()
        {
            yield return new WaitForSeconds(1.5f);
            bool tryReconnect = PhotonNetwork.ReconnectAndRejoin();

            if (!tryReconnect)
                StartCoroutine(ErrorMessage("Failed to reconnect", new() { ("Room", PlayerPrefs.GetString(ConstantStrings.LastRoom)) }));
        }
    }

    #endregion

#region Part 2

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (JoinRoomButton button in listOfJoinButtons)
            button.ClearInfo();

        int counter = 0;
        foreach (RoomInfo room in roomList)
        {
            if (room.CustomProperties.ContainsKey(ConstantStrings.GameName)
                && room.CustomProperties.ContainsKey(ConstantStrings.CanPlay)
                && room.CustomProperties.ContainsKey(ConstantStrings.JoinAsSpec)
                && room.CustomProperties.ContainsKey(ConstantStrings.GameOver))
            {
                if (room.CustomProperties[ConstantStrings.GameName].Equals(Application.productName)
                    && room.PlayerCount < room.MaxPlayers && room.MaxPlayers >= 2 && room.IsVisible
                    && counter < listOfJoinButtons.Count && !(bool)room.CustomProperties[ConstantStrings.GameOver])
                {
                    JoinRoomButton nextJoin = listOfJoinButtons[counter];
                    nextJoin.transform.SetParent(keepJoinButtons);
                    nextJoin.button.onClick.AddListener(() => JoinRoom(room.Name));
                    nextJoin.button.image.color = ((bool)room.CustomProperties[ConstantStrings.JoinAsSpec]) ? Color.yellow : Color.white;

                    nextJoin.thisName.text = room.Name;
                    nextJoin.playerCount.text = Translator.inst.Translate($"Player Count", new()
                    { ("Current", $"{room.PlayerCount}"), ("Max", $"{(int)room.CustomProperties[ConstantStrings.CanPlay]}") });
                    counter++;
                }
            }
        }
    }

    public void CreateRoom()
    {
        ExitGames.Client.Photon.Hashtable customProps = new()
        {
            { ConstantStrings.GameName, Application.productName },
            { ConstantStrings.CurrentPhase, 0 },
            { ConstantStrings.CurrentRound, 0 },
            { ConstantStrings.CanPlay, 2 },
            { ConstantStrings.JoinAsSpec, false },
            { ConstantStrings.GameOver, false },
            { ConstantStrings.NextPlayerPosition, 0 },
        };

        RoomOptions options = new()
        {
            MaxPlayers = 10,
            PlayerTtl = Application.isEditor ? 15000 : 120000,
            EmptyRoomTtl = 10000,
            CustomRoomProperties = customProps,
            CustomRoomPropertiesForLobby = new string[] { ConstantStrings.GameName, ConstantStrings.CanPlay, ConstantStrings.JoinAsSpec, ConstantStrings.GameOver }
        };
        SetInitialPlayerProps();
        PhotonNetwork.CreateRoom(PlayerPrefs.GetString(ConstantStrings.MyUserName), options);
    }

    void SetInitialPlayerProps()
    {
        ExitGames.Client.Photon.Hashtable playerProps = new()
        {
            [ConstantStrings.Waiting] = false,
            [ConstantStrings.MyHealth] = 20,
            [ConstantStrings.MyPosition] = -1,

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
    }

    public void JoinRoom(string roomName)
    {
        SetInitialPlayerProps();
        PhotonNetwork.JoinRoom(roomName);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        PlayerPrefs.DeleteKey(ConstantStrings.LastRoom);
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("2. Game");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        if (part1.gameObject.activeSelf)
            StartCoroutine(ErrorMessage("Failed to connect to server"));
        else
            StartCoroutine(ErrorMessage("Disconnected from server"));

        part1.gameObject.SetActive(true);
        part2.gameObject.SetActive(false);
    }

#endregion

}
