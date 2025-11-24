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
            ShareMessageRPC(textToSend, false);
        }
    }

    public void ShareMessageRPC(string logText, bool translate)
    {
        DoFunction(() => ShareMessage(logText, translate), RpcTarget.All);
    }

    [PunRPC]
    void ShareMessage(string logText, bool translate)
    {
        string targetText = (translate) ? Translator.inst.SplitAndTranslate(-1, logText) : logText;
        allTexts.text += $"{targetText}\n";
        ChangeScrolling();
    }

    void ChangeScrolling()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(allTexts.rectTransform);
        Invoke(nameof(ScrollDown), 0.2f);
    }

    void ScrollDown()
    {
        if (scroll.value <= 0.1f)
            scroll.value = 0;
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        if (PhotonNetwork.IsMasterClient && (int)GetPlayerProperty(otherPlayer, ConstantStrings.Position) >= 0)
        {
            if (otherPlayer.IsInactive)
            {
                ShareMessageRPC($"Player Disconnected-Player-{otherPlayer.NickName}", true);
                InstantChangePlayerProp(otherPlayer, ConstantStrings.Waiting, false);
            }
            else if ((bool)GetRoomProperty(ConstantStrings.GameOver.ToString()))
            {
                ShareMessageRPC($"Player Quit-Player-{otherPlayer.NickName}", true);
            }
        }
    }
}
