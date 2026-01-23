using MyBox;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CommHub : PhotonCompatible
{
    public static CommHub inst;

    [SerializeField] Scrollbar scroll;
    [SerializeField] TMP_Text allTexts;

    [SerializeField] TMP_InputField inputMessage;
    [SerializeField] Button uploadMessage;

    protected override void Awake()
    {
        base.Awake();
        this.bottomType = this.GetType();
        inst = this;
        uploadMessage.onClick.AddListener(SendMyMessage);
    }

    void SendMyMessage()
    {
        string textToSend = inputMessage.text.Trim();
        if (textToSend != "")
        {
            inputMessage.text = "";
            ShareMessageRPC($"{PhotonNetwork.LocalPlayer.NickName}: {textToSend}", false);
        }
    }

    public void ShareMessageRPC(string text, bool translate)
    {
        DoFunction(() => ShareMessage(text, translate), RpcTarget.All);
    }

    [PunRPC]
    void ShareMessage(string text, bool translate)
    {
        string targetText = (translate) ? Translator.inst.UnPackage(text) : text;
        allTexts.text += $"{targetText}\n";
        ChangeScrolling();
    }

    void ChangeScrolling()
    {
        if (scroll.value <= 0.2f)
            Invoke(nameof(ScrollDown), 0.1f);
        LayoutRebuilder.ForceRebuildLayoutImmediate(allTexts.rectTransform);
    }

    void ScrollDown()
    {
        scroll.value = 0;
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        int playerPosition = (int)GetPlayerProperty(otherPlayer, ConstantStrings.MyPosition);
        if (PhotonNetwork.IsMasterClient && playerPosition >= 0)
        {
            if (otherPlayer.IsInactive)
            {
                ShareMessageRPC(OnlineTranslate.Online_Player_Disconnected(otherPlayer.NickName), true);
                InstantChangePlayerProp(otherPlayer, ConstantStrings.Waiting, false);
            }
            else if (!(bool)GetRoomProperty(ConstantStrings.GameOver.ToString()))
            {
                ShareMessageRPC(OnlineTranslate.Online_Player_Quit(otherPlayer.NickName), true);
                TurnManager.inst.TextForEnding(OnlineTranslate.Online_Player_Resigned(otherPlayer.NickName), playerPosition); 
            }
        }
    }
}
