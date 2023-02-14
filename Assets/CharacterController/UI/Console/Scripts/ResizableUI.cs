using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ResizableUI : MonoBehaviour, IDragHandler
{
    [SerializeField]
    RectTransform resizableObject = default;

    [SerializeField]
    Canvas canvas = default;

    public Vector2 minSize;
    public Vector2 maxSize;

    // Start is called before the first frame update
    void Start()
    {
        if (canvas == null)
        {
            canvas = GetComponentInParent<Canvas>();
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (resizableObject == null)
            return;

        Vector2 sizeDelta = resizableObject.sizeDelta;

        Vector2 resizeValue = eventData.delta / (canvas != null ? canvas.scaleFactor : 1f);

        sizeDelta += new Vector2(resizeValue.x, -resizeValue.y);
        sizeDelta = new Vector2(
            Mathf.Clamp(sizeDelta.x, minSize.x, maxSize.x),
            Mathf.Clamp(sizeDelta.y, minSize.y, maxSize.y)
            );

        resizableObject.sizeDelta = sizeDelta;

        // RectTransform canvas, panel;
        var canvasRectTransform = canvas.GetComponent<RectTransform>();
        sizeDelta = canvasRectTransform.sizeDelta - resizableObject.sizeDelta;
        var panelPivot = resizableObject.pivot;
        var position = resizableObject.anchoredPosition;
        position.x = Mathf.Clamp(position.x, -sizeDelta.x * panelPivot.x, sizeDelta.x * (1 - panelPivot.x));
        position.y = Mathf.Clamp(position.y, -sizeDelta.y * panelPivot.y, sizeDelta.y * (1 - panelPivot.y));
        resizableObject.anchoredPosition = position;
    }
}
