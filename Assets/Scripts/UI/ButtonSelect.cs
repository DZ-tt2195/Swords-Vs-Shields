using UnityEngine;
using MyBox;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonSelect : MonoBehaviour
{
    public Button button { get; private set; }
    [SerializeField] Image border;

    private void Awake()
    {
        button = GetComponent<Button>();
        SetBorder(false);
    }

    public void SetBorder(bool border)
    {
        this.border.gameObject.SetActive(border);
    }

    private void FixedUpdate()
    {
        try { this.border.SetAlpha(CreateGame.inst.opacity); } catch { }
    }
}
