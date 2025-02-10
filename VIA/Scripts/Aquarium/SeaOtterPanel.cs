using DG.Tweening;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SeaOtterPanel : UIPanel
{
    public GameSceneManager gameSceneManager;

    [Header("Dialogue")]
    public TMP_Text dialogueText;
    public TMP_Text explainText;
    public GameObject yesOrNoButton;
    public GameObject okButton;

    [Header("Drawing")]
    public GameObject sketchBackGround;
    public RectTransform sketchPanel;
    public SketchPanel sketchGroup;
    public Drawing drawing;
    GameObject playerControllerPanel;
    Canvas worldCanvas;

    Sequence sketchOpenSequence;

    protected override void Start()
    {
        base.Start();

        InitSketchPanelSequence();
    }

    private void InitSketchPanelSequence()
    {
        sketchOpenSequence = DOTween.Sequence()
            .SetAutoKill()
            .Pause()
            .Append(sketchPanel.DOAnchorPosY(0f, 0.3f))
            .Append(sketchPanel.DOAnchorPosY(50f, 0.2f))
            .Append(sketchPanel.DOAnchorPosY(0f, 0.1f));
    }

    public void Init(GameObject Obj, GameObject npc)
    {
        worldCanvas = npc.GetComponent<NPC>().worldCanvas;
        
        playerControllerPanel = Obj;

        if(gameSceneManager.isTalkOtter)
        {
            dialogueText.text = "<color=#124A5C>�ش�</color>��.";
            explainText.text = $"(�� �������� 1�� 3ȸ �÷��� �����մϴ�.) ���� {gameSceneManager.aquariumMissionCount} / 3 ȸ ���� ����.";
            okButton.SetActive(false);
            yesOrNoButton.SetActive(true);
        }
        else
        {
            dialogueText.text = "...(<color=#124A5C>�ش�</color>�� ��ſ��� ���ɾ���δ�)";
            explainText.text = "";
            okButton.SetActive(true);
            yesOrNoButton.SetActive(false);
        }
    }

    public void ClickYesButton()
    {
        CloseDialogueUI();

        sketchBackGround.SetActive(true);

        sketchGroup.Init(sketchBackGround, playerControllerPanel);
        sketchGroup.tutorial.SetActive(true);

        drawing.Init();

        if (sketchOpenSequence.IsPlaying() || sketchOpenSequence.IsComplete())
            sketchOpenSequence.Restart();
        else
            sketchOpenSequence.Play();

        //Sequence sequence = DOTween.Sequence()
        //    .OnStart(() =>
        //    {
        //        CloseDialogueUI();

        //        sketchBackGround.SetActive(true);

        //        sketchGroup.Init(sketchBackGround, playerControllerPanel);
        //        sketchGroup.tutorial.SetActive(true);

        //        drawing.Init();
        //    })
        //    .Append(sketchPanel.DOAnchorPosY(0f, 0.3f))
        //    .Append(sketchPanel.DOAnchorPosY(50f, 0.2f))
        //    .Append(sketchPanel.DOAnchorPosY(0f, 0.1f));
    }

    public void ClickNoButton()
    {
        playerControllerPanel?.SetActive(true);
        CloseDialogueUI();
    }
}
