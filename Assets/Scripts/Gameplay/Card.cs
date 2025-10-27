using UnityEngine;
using UnityEngine.UI;
using MyBox;
using System.Collections;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System;
using Photon.Pun;
using System.Text.RegularExpressions;

public class Card : PhotonCompatible
{

#region Setup

    public Button button;
    public Image border;
    public CardLayout layout;

    bool flipping;
    public CardType thisCard { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        this.bottomType = this.GetType();

        Canvas canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        this.transform.localScale = Vector3.Lerp(Vector3.one, canvas.transform.localScale, 0.5f);

        if (!PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(HealthString()))
        {
            ExitGames.Client.Photon.Hashtable initialProps = new()
            {
                [HealthString()] = 0,
            };
            PhotonNetwork.CurrentRoom.SetCustomProperties(initialProps);
        }
    }

    public string HealthString()
    {
        return $"{this.photonView.ViewID}_Health";
    }

    public void AssignCard(CardData dataFile)
    {
        string noSpaces = dataFile.cardName.Replace(" ", "");
        thisCard = (CardType)Activator.CreateInstance(Type.GetType(noSpaces), dataFile);
        this.layout.FillInCards(dataFile, 0, 0);
        this.name = dataFile.cardName;
        KeywordTooltip.instance.NewCardRC(dataFile.cardName, this.layout);
    }

    #endregion

#region Animations

    public void MoveCardRPC(Vector3 newPos, float waitTime, Vector3 newScale)
    {
        StopAllCoroutines();
        StartCoroutine(MoveCard(newPos, waitTime, newScale));
    }

    IEnumerator MoveCard(Vector3 newPos, float waitTime, Vector3 newScale)
    {
        float elapsedTime = 0;
        Vector2 originalPos = this.transform.localPosition;
        Vector2 originalScale = this.transform.localScale;

        while (elapsedTime < waitTime)
        {
            this.transform.localPosition = Vector3.Lerp(originalPos, newPos, elapsedTime / waitTime);
            this.transform.localScale = Vector3.Lerp(originalScale, newScale, elapsedTime / waitTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        this.transform.localPosition = newPos;
    }

    public void FlipCardRPC(float newAlpha, float totalTime, float rotation)
    {
        StartCoroutine(FlipCard(newAlpha, totalTime, rotation));
    }

    IEnumerator FlipCard(float newAlpha, float totalTime, float rotation)
    {
        if (flipping)
            yield break;

        flipping = true;
        transform.localEulerAngles = new Vector3(0, 0, rotation);
        float elapsedTime = 0f;

        Vector3 originalRot = this.transform.localEulerAngles;
        Vector3 newRot = new(0, 90, rotation);

        while (elapsedTime < totalTime)
        {
            this.transform.localEulerAngles = Vector3.Lerp(originalRot, newRot, elapsedTime / totalTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        this.layout.FillInCards(thisCard.dataFile, newAlpha, rotation);
        elapsedTime = 0f;

        while (elapsedTime < totalTime)
        {
            this.transform.localEulerAngles = Vector3.Lerp(newRot, originalRot, elapsedTime / totalTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        this.transform.localEulerAngles = originalRot;
        flipping = false;
    }

    private void FixedUpdate()
    {
        try { this.border.SetAlpha(PlayerCreator.inst.opacity); } catch { }
    }

    #endregion

#region Properties

    public void MakeDecision(Action action)
    {
        action();
    }

    #endregion

}