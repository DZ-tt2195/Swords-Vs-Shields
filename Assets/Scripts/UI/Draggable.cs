using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Draggable : MonoBehaviour
{
    RectTransform rect;
    float XCap;
    float YCap;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    public void DragHangler(BaseEventData data)
    {
        PointerEventData pointer = (PointerEventData)data;
        RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)CreateGame.inst.canvas.transform,
            pointer.position, CreateGame.inst.canvas.worldCamera, out Vector2 position);
        transform.position = CreateGame.inst.canvas.transform.TransformPoint(position);
        this.transform.SetAsLastSibling();
    }

    private void Update()
    {
        XCap = 1280 - (rect.sizeDelta.x / (2 + (1 - CreateGame.inst.canvas.transform.localScale.x)));
        YCap = 720 - (rect.sizeDelta.y / (2 + (1 - CreateGame.inst.canvas.transform.localScale.y)));

        this.transform.localPosition = new Vector3(
            Mathf.Clamp(transform.localPosition.x, -XCap, XCap),
            Mathf.Clamp(transform.localPosition.y, -YCap, YCap), 0);
    }
}
