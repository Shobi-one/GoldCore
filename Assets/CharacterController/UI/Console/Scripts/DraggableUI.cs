using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableUI : MonoBehaviour, IDragHandler, IEndDragHandler
{
    [SerializeField]
    RectTransform draggableObject = default;

    [SerializeField]
    Canvas canvas = default;

    void Start()
    {
        if (canvas == null)
        {
            canvas = GetComponentInParent<Canvas>();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggableObject != null)
        {
            draggableObject.anchoredPosition += eventData.delta / (canvas != null ? canvas.scaleFactor : 1f);

            // RectTransform canvas, panel;
            var canvasRectTransform = canvas.GetComponent<RectTransform>();
            var sizeDelta = canvasRectTransform.sizeDelta - draggableObject.sizeDelta;
            var panelPivot = draggableObject.pivot;
            var position = draggableObject.anchoredPosition;
            position.x = Mathf.Clamp(position.x, -sizeDelta.x * panelPivot.x, sizeDelta.x * (1 - panelPivot.x));
            position.y = Mathf.Clamp(position.y, -sizeDelta.y * panelPivot.y, sizeDelta.y * (1 - panelPivot.y));
            draggableObject.anchoredPosition = position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // RectTransform canvas, panel;
        var canvasRectTransform = canvas.GetComponent<RectTransform>();
        var sizeDelta = canvasRectTransform.sizeDelta - draggableObject.sizeDelta;
        var panelPivot = draggableObject.pivot;
        var position = draggableObject.anchoredPosition;
        position.x = Mathf.Clamp(position.x, -sizeDelta.x * panelPivot.x, sizeDelta.x * (1 - panelPivot.x));
        position.y = Mathf.Clamp(position.y, -sizeDelta.y * panelPivot.y, sizeDelta.y * (1 - panelPivot.y));
        draggableObject.anchoredPosition = position;
    }
}
