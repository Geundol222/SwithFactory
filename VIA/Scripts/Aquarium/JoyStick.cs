using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class JoyStick : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    public RectTransform handleTransform;
    public RectTransform backGroundTransform;

    public Vector2 inputVector;
    public bool isDrag;

    [SerializeField, Range(0, 100)] float handleLimit;

    public void OnBeginDrag(PointerEventData eventData)
    {
        DragEvent(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        DragEvent(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDrag = false;
        handleTransform.anchoredPosition = Vector2.zero;
    }

    public void DragEvent(PointerEventData eventData)
    {
        isDrag = true;

        Vector2 inputPos = eventData.position - backGroundTransform.anchoredPosition;

        Vector2 rangeDir = inputPos.magnitude < handleLimit ? inputPos : inputPos.normalized * handleLimit;
        handleTransform.anchoredPosition = rangeDir;

        inputVector = rangeDir.normalized;        
    }
}
