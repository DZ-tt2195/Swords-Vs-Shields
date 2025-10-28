using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class SliderChoice : MonoBehaviour
{
    [SerializeField] TMP_Text textbox;
    [SerializeField] Button confirmButton;
    public bool beDestroyed { get; private set; }

    [SerializeField] Slider slider;
    int currentSliderValue = 0;

    [SerializeField] TMP_Text minimumText;
    [SerializeField] TMP_Text maximumText;
    [SerializeField] TMP_Text currentText;

    private void Awake()
    {
        slider.onValueChanged.AddListener(UpdateText);
    }

    internal void StatsSetup(string header, int min, int max, Vector3 position, bool beDestroyed, Action<int> action)
    {
        this.textbox.text = KeywordTooltip.instance.EditText(header);
        this.transform.SetParent(PlayerCreator.inst.canvas.transform);
        this.transform.localPosition = position;
        this.transform.localScale = new Vector3(1, 1, 1);

        minimumText.text = min.ToString();
   	    slider.minValue = min;

        maximumText.text = max.ToString();
        slider.maxValue = max;

        slider.value = min;
        UpdateText(slider.minValue);
        this.beDestroyed = beDestroyed;

        confirmButton.onClick.AddListener(DecisionMade);
        void DecisionMade()
        {
            Log.inst.inReaction.Add(() => action?.Invoke(min));
            Log.inst.PopStack();
        }
    }

    void UpdateText(float value)
    {
        currentText.text = KeywordTooltip.instance.EditText($"{(int)value}");
        currentSliderValue = (int)value;
    }
}
