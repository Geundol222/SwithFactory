using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour, IInteractable
{
    public GameSceneManager gameSceneManager;
    public RectTransform ownScript;
    public Canvas worldCanvas;
    Camera mainCam;

    protected void Awake()
    {
        mainCam = Camera.main;
    }

    protected void LateUpdate()
    {
        worldCanvas.transform.rotation = Quaternion.LookRotation(mainCam.transform.forward);
    }

    public virtual void Interact(GameObject Obj)
    {
        ownScript.GetComponent<UIPanel>().OpenDialogueUI();
    }
}
