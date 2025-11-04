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
        string targetText = "\n";

        if (translate)
        {
            string[] splitUp = logText.Split('-');
            List<(string, string)> toTranslate = new();

            for (int i = 1; i < splitUp.Length; i += 2)
            {
                string first = splitUp[i];
                string second = first switch
                {
                    "Card" => Translator.inst.Translate(splitUp[i + 1]),
                    _ => splitUp[i + 1]
                };
                toTranslate.Add((first, second));
            }
            targetText += Translator.inst.Translate($"{splitUp[0]}", toTranslate);
            targetText = KeywordTooltip.instance.EditText(targetText);
        }
        else
        {
            targetText += logText;
        }
        allTexts.text += targetText;
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
        if (PhotonNetwork.IsMasterClient && (int)GetPlayerProperty(otherPlayer, PlayerProp.Position.ToString()) >= 0)
        {
            if (otherPlayer.IsInactive)
            {
                ShareMessageRPC($"Player Disconnected-Player-{otherPlayer.NickName}", true);
                ChangePlayerProperties(otherPlayer, PlayerProp.Waiting, false);
            }
            else
            {
                ShareMessageRPC($"Player Quit-Player-{otherPlayer.NickName}", true);
            }
        }
    }
}
