using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MyBox;
using System.Linq;

public class Popup : MonoBehaviour
{
    [SerializeField] protected TMP_Text textbox;
    [SerializeField] protected Transform storeThings;
    protected RectTransform textWidth;
    protected RectTransform imageWidth;

    public List<Button> buttonsInCollector { get; private set; }
    public bool beDestroyed { get; private set; }

    void Awake()
    {
        textWidth = textbox.GetComponent<RectTransform>();
        imageWidth = this.transform.GetComponent<RectTransform>();
        buttonsInCollector = new();
    }

    public virtual void StatsSetup(bool destroyMe, string header, Vector2 position)
    {
        this.transform.SetParent(PlayerCreator.inst.canvas.transform);
        this.transform.localPosition = position;
        this.transform.localScale = new Vector3(1, 1, 1);
        beDestroyed = destroyMe;
    }

    public void Resize()
    {
        imageWidth.sizeDelta = new Vector2(Mathf.Max(buttonsInCollector.Count, 2) * 300, imageWidth.sizeDelta.y);
        textWidth.sizeDelta = new Vector2(Mathf.Max(buttonsInCollector.Count, 2) * 300, textWidth.sizeDelta.y);
        for (int i = 0; i < buttonsInCollector.Count; i++)
        {
            Transform nextTransform = buttonsInCollector[i].transform;
            nextTransform.transform.localPosition = new Vector2((buttonsInCollector.Count - 1) * -150 + (300 * i), -100);
        }
    }

    public void DisableButton(int number)
    {
        if (number < buttonsInCollector.Count)
        {
            buttonsInCollector[number].onClick.RemoveAllListeners();
            buttonsInCollector[number].interactable = false;
        }
    }

    public void RemoveButton(int number)
    {
        if (number < buttonsInCollector.Count)
        {
            Button button = buttonsInCollector[number];
            buttonsInCollector.RemoveAt(number);
            Destroy(button.gameObject);
        }
        Resize();
    }
}
