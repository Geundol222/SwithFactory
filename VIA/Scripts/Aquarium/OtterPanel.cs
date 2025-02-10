using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OtterPanel : UIPanel
{
    public GameSceneManager gameSceneManager;
    public TMP_Text dialogueText;
    public Button nextButton;
    public Button okButton;

    GameObject playerControllerPanel;
    NPCScript otterScript;
    Canvas worldCanvas;
    int dialogueIndex;
    bool talkFinish = false;
    Dictionary<string, string[]> dialogueDic;

    private void Awake()
    {
        dialogueIndex = 0;
        dialogueDic = new Dictionary<string, string[]>();
        otterScript = Resources.Load<NPCScript>("Data/OtterScriptData");
    }

    protected override void Start()
    {
        base.Start();

        for(int i = 0; i < otterScript.Dialogue.Length; i++)
        {
            dialogueDic.Add(otterScript.Dialogue[i].name, otterScript.Dialogue[i].description);
        }
    }

    public void Init(GameObject Obj, GameObject npc)
    {
        worldCanvas = npc.GetComponent<NPC>().worldCanvas;
        
        if (!talkFinish)
            worldCanvas.gameObject.SetActive(true);
        else
            worldCanvas.gameObject.SetActive(false);

        playerControllerPanel = Obj;
        dialogueText.text = dialogueDic["Intro"][0];
        ButtonTextSet(false);
    }

    public void ClickNextButton()
    {
        if (!talkFinish)
        {
            dialogueIndex++;

            dialogueText.text = dialogueDic["Intro"][dialogueIndex];

            if (dialogueIndex >= dialogueDic["Intro"].Length - 1)
            {
                ButtonTextSet(true);
                talkFinish = true;
                gameSceneManager.isTalkOtter = true;
            }
        }
    }

    public void ClickOkButton()
    {
        playerControllerPanel?.SetActive(true);
        talkFinish = false;
        dialogueIndex = 0;
        CloseDialogueUI();
        worldCanvas.gameObject.SetActive(false);
    }

    public void ButtonTextSet(bool isFinish)
    {
        if (!isFinish)
        {
            nextButton.gameObject.SetActive(true);
            okButton.gameObject.SetActive(false);
        }
        else
        {
            nextButton.gameObject.SetActive(false);
            okButton.gameObject.SetActive(true);
        }
    }
}
