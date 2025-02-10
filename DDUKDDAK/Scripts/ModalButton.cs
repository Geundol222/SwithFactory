using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ModalButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public ModalControl controller;
    public ModalState state;
    public Vector2 offset;
    public bool isHovering;
    GalleryButton myGallery;

    [Header("Need Change Sprite?")]
    public Sprite changeSprite;

    [Header("Need Change Rect?")]
    public float width;
    public float height;

    private void Awake()
    {
        if (GetComponent<GalleryButton>())
            myGallery = GetComponent<GalleryButton>();

        controller = FindObjectOfType<ModalControl>(true);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (myGallery != null)
        {
            DateTime currentTime = DateTime.Now;
            TimeSpan remainingTime = myGallery.endDate - currentTime;

            controller.SetGalleryData(myGallery.myName, myGallery.mySize, myGallery.startDate.ToString("yyyy.MM.dd HH:mm"), myGallery.endDate.ToString("yyyy.MM.dd HH:mm"), $"{remainingTime.Days}");
        }

        isHovering = true;

        if (changeSprite != null)
        {
            controller.ChangeModalImage(changeSprite);
        }

        if (width != 0 || height != 0)
        {
            controller.ChangeModalRect(width, height);
        }

        controller.SetOffset(offset);
        controller.ActivePanel(state);
        controller.MouseOn(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        controller.MouseOn(false);
    }
}
