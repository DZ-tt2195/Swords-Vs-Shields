using UnityEngine;
using MyBox;
using UnityEngine.UI;
using Photon.Pun;

[RequireComponent(typeof(Button))]
public class ButtonSelect : MonoBehaviour
{
    public Button button { get; private set; }
    public Image border;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void FixedUpdate()
    {
        try { this.border.SetAlpha(PlayerCreator.inst.opacity); } catch { }
    }
}
