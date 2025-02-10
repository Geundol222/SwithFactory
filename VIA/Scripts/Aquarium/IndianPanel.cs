using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IndianPanel : UIPanel
{
    public GameSceneManager gameSceneManager;
    public TMP_Text dialogueText;
    public TMP_Text explainText;
    public GameObject nextButton;
    public GameObject OKButton;
    public GameObject yesOrNoButton;

    public GameObject skewerBackGround;
    public RectTransform domaPanel;

    GameObject playerControllerPanel;
    NPCScript indianScript;
    int dialogueIndex;
    bool talkFinish = false;
    Dictionary<string, string[]> dialogueDic;
    Canvas worldCanvas;

    Sequence GameStartSequence;
    Sequence GameCompleteSequence;

    private void Awake()
    {
        dialogueIndex = 0;
        dialogueDic = new Dictionary<string, string[]>();
        indianScript = Resources.Load<NPCScript>("Data/IndianScriptData");
        explainText.text = "";
    }

    protected override void Start()
    {
        base.Start();

        for (int i = 0; i < indianScript.Dialogue.Length; i++)
        {
            dialogueDic.Add(indianScript.Dialogue[i].name, indianScript.Dialogue[i].description);
        }

        GameSequenceUIInit();
    }

    private void GameSequenceUIInit()
    {
        InitGameStartSequence();
        InitGameCompleteSequence();
    }

    #region SequenceInit
    private void InitGameStartSequence()
    {
        GameStartSequence = DOTween.Sequence()
            .SetAutoKill(false)
            .Pause()
            .Append(domaPanel.DOAnchorPosY(0f, 0.3f))
            .Append(domaPanel.DOAnchorPosY(50f, 0.2f))
            .Append(domaPanel.DOAnchorPosY(0f, 0.1f));
    }

    public void InitGameCompleteSequence()
    {
        GameCompleteSequence = DOTween.Sequence()
            .SetAutoKill(false)
            .Pause()
            .Append(domaPanel.GetComponent<RectTransform>().DOAnchorPosY(-50f, 0.3f))
            .Append(domaPanel.GetComponent<RectTransform>().DOAnchorPosY(5000f, 0.2f))
            .OnComplete(() =>
            {
                skewerBackGround.SetActive(false);
                domaPanel.gameObject.SetActive(false);
            });
    }
    #endregion

    public void Init(GameObject Obj, GameObject npc)
    {
        worldCanvas = npc.GetComponent<NPC>().worldCanvas;

        OKButton.SetActive(false);

        playerControllerPanel = Obj;

        if (talkFinish)
        {
            dialogueText.text = dialogueDic["Intro"][dialogueDic["Intro"].Length - 1];
            explainText.text = $"(본 콘텐츠는 1일 3회 플레이 가능합니다.) 현재 {gameSceneManager.aquariumMissionCount} / 3 회 진행 가능.";
            ButtonTextSet(true);
        }
        else
        {
            dialogueText.text = dialogueDic["Intro"][0];
            explainText.text = "";
            ButtonTextSet(false);
        }
    }

    public void ClickNextButton()
    {
        if (!talkFinish)
        {
            dialogueIndex++;

            dialogueText.text = dialogueDic["Intro"][dialogueIndex];

            if (dialogueIndex >= dialogueDic["Intro"].Length - 1)
            {
                explainText.text = $"(본 콘텐츠는 1일 3회 플레이 가능합니다.) 현재 {gameSceneManager.aquariumMissionCount} / 3 회 진행 가능.";
                ButtonTextSet(true);
                talkFinish = true;
            }
        }
    }

    public void ButtonTextSet(bool isFinish)
    {
        if (!isFinish)
        {
            nextButton.SetActive(true);
            yesOrNoButton.SetActive(false);
        }
        else
        {
            nextButton.SetActive(false);
            yesOrNoButton.SetActive(true);
        }
    }

    public void ClickYesButton()
    {
        dialogueIndex = 0;

        // 도전횟수 차감

        CloseDialogueUI();

        skewerBackGround.SetActive(true);
        domaPanel.gameObject.SetActive(true);

        domaPanel?.GetChild(0).GetComponent<StickGame>().Init(playerControllerPanel);

        if (GameStartSequence.IsPlaying() || GameStartSequence.IsComplete())
            GameStartSequence.Restart();
        else
            GameStartSequence.Play();
    }

    public void CompleteAnimation()
    {
        if (GameCompleteSequence.IsPlaying() || GameCompleteSequence.IsComplete())
            GameCompleteSequence.Restart();
        else
            GameCompleteSequence.Play();
    }

    public void ResultUI(int answerCount)
    {
        playerControllerPanel?.SetActive(false);
        explainText.text = "";

        if (answerCount > 7)
        {
            dialogueText.text = dialogueDic["Perfect"][0];
        }
        else if (answerCount > 3)
        {
            dialogueText.text = dialogueDic["Good"][0];
        }
        else
        {
            dialogueText.text = dialogueDic["Bad"][0];
        }

        yesOrNoButton.SetActive(false);
        nextButton.SetActive(false);
        OKButton.SetActive(true);

        OpenDialogueUI();
    }

    public void ClickOkButton()
    {
        playerControllerPanel?.SetActive(true);
        talkFinish = false;
        dialogueIndex = 0;
        CloseDialogueUI();
    }
}
