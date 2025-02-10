using Photon.Voice.Unity.Demos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MakeGalleryButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public ModalControl modalPanel;
    public Image modalImage;
    public Sprite[] buttonSprites;
    public Vector2 modalOffset;

    bool isHovering = false;
    GameObject currentHoveredButton = null;

    void Start()
    {
        foreach (Transform child in transform)
        {
            Button button = child.GetComponent<Button>();
            if (button != null)
            {
                EventTrigger trigger = button.gameObject.AddComponent<EventTrigger>();

                EventTrigger.Entry entryEnter = new EventTrigger.Entry();
                entryEnter.eventID = EventTriggerType.PointerEnter;
                entryEnter.callback.AddListener((eventData) => { OnPointerEnter((PointerEventData)eventData); });
                trigger.triggers.Add(entryEnter);

                EventTrigger.Entry entryExit = new EventTrigger.Entry();
                entryExit.eventID = EventTriggerType.PointerExit;
                entryExit.callback.AddListener((eventData) => { OnPointerExit((PointerEventData)eventData); });
                trigger.triggers.Add(entryExit);
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        modalPanel.ActivePanel(ModalState.Create);

        if (!modalPanel.isHovering)
        {
            foreach (Transform child in modalPanel.transform)
            {
                child.gameObject.SetActive(false);
            }

            modalPanel.transform.GetChild(0).gameObject.SetActive(true);

            int buttonIndex = eventData.pointerEnter.transform.GetSiblingIndex();
            if (buttonIndex >= 0 && buttonIndex < buttonSprites.Length)
            {
                if (buttonIndex == 3)
                    modalPanel.ChangeModalRect(0f, 98f);
                else
                    modalPanel.ChangeModalRect(0f, 84f);

                modalPanel.ChangeModalImage(buttonSprites[buttonIndex]);
                modalPanel.MouseOn(true);                
                currentHoveredButton = eventData.pointerEnter;                
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (modalPanel.isHovering && eventData.pointerEnter == currentHoveredButton)
        {
            modalPanel.MouseOn(false);
            currentHoveredButton = null;            
        }
    }
}
