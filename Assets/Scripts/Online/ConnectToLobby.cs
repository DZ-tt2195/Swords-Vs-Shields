using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using System.Collections;
using Photon.Realtime;
using UnityEngine.UI;
using ExitGames.Client.Photon;
using System.Diagnostics;
using System;
using System.Linq;
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
    [SerializeField] Button joinManual;
    [SerializeField] Button disconnectButton;
    List<JoinRoomButton> listOfJoinButtons = new();

    private void Start()
    {
        part2.gameObject.SetActive(true);
        joinManual.onClick.AddListener(() => JoinRoom(joinInput.text));
        disconnectButton.onClick.AddListener(() => PhotonNetwork.Disconnect());

        foreach (Transform child in keepJoinButtons)
            listOfJoinButtons.Add(child.GetComponent<JoinRoomButton>());
        foreach (JoinRoomButton button in listOfJoinButtons)
            button.ClearInfo();

        username.text = PlayerPrefs.GetString("Online Username");
        error.gameObject.SetActive(false);
        part1.gameObject.SetActive(true);
        part2.gameObject.SetActive(false);

        reconnectButton.gameObject.SetActive(PlayerPrefs.HasKey("LastRoom"));

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
            PlayerPrefs.SetString("Online Username", newName);
            PlayerPrefs.Save();
            PhotonNetwork.NickName = PlayerPrefs.GetString("Online Username");
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
        PlayerPrefs.DeleteKey("LastRoom");
        PhotonNetwork.OfflineMode = false;

        error.gameObject.SetActive(false);
        part1.gameObject.SetActive(false);
        part2.gameObject.SetActive(true);
    }

    public void Reconnect()
    {
        StartCoroutine(ErrorMessage("Attempt to reconnect", new() { ("Room", PlayerPrefs.GetString("LastRoom"))}));
        StartCoroutine(Delay());

        IEnumerator Delay()
        {
            yield return new WaitForSeconds(1.5f);
            bool tryReconnect = PhotonNetwork.ReconnectAndRejoin();

            if (!tryReconnect)
                StartCoroutine(ErrorMessage("Failed to reconnect", new() { ("Room", PlayerPrefs.GetString("LastRoom")) }));
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
            if (room.CustomProperties.ContainsKey(RoomProp.Game.ToString()) && room.CustomProperties.ContainsKey(RoomProp.CanPlay.ToString()) && room.CustomProperties.ContainsKey(RoomProp.JoinAsSpec.ToString()))
            {
                if (room.CustomProperties[RoomProp.Game.ToString()].Equals(Application.productName) && room.PlayerCount < room.MaxPlayers && room.MaxPlayers >= 2 && room.IsVisible && counter < listOfJoinButtons.Count)
                {
                    JoinRoomButton nextJoin = listOfJoinButtons[counter];
                    nextJoin.transform.SetParent(keepJoinButtons);
                    nextJoin.button.onClick.AddListener(() => JoinRoom(room.Name));
                    nextJoin.button.image.color = ((bool)room.CustomProperties[RoomProp.JoinAsSpec.ToString()]) ? Color.yellow : Color.white;

                    nextJoin.thisName.text = room.Name;
                    nextJoin.playerCount.text = Translator.inst.Translate($"Player Count", new() { ("Current", $"{room.PlayerCount}"), ("Max", $"{(int)room.CustomProperties[RoomProp.CanPlay.ToString()]}") });
                    counter++;
                }
            }
        }
    }

    public void CreateRoom()
    {
        ExitGames.Client.Photon.Hashtable customProps = new()
        {
            { RoomProp.Game.ToString(), Application.productName },
            { RoomProp.CanPlay.ToString(), 2 },
            { RoomProp.JoinAsSpec.ToString(), false },
        };

        RoomOptions options = new()
        {
            MaxPlayers = 10,
            PlayerTtl = Application.isEditor ? 15000 : 120000,
            EmptyRoomTtl = 10000,
            CustomRoomProperties = customProps,
            CustomRoomPropertiesForLobby = new string[] { RoomProp.Game.ToString(), RoomProp.CanPlay.ToString(), RoomProp.JoinAsSpec.ToString() }
        };

        PhotonNetwork.CreateRoom(PlayerPrefs.GetString("Online Username"), options);
    }

    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        PlayerPrefs.DeleteKey("LastRoom");
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
