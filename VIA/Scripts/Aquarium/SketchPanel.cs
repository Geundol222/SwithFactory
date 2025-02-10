using Cysharp.Threading.Tasks;
using DG.Tweening;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class SketchPanel : MonoBehaviourPun
{
    public GameObject parent;

    [Header("Draw")]
    public GameObject sketchBackGround;
    public RectTransform sketchPanel;
    public Drawing drawing;
    public Image[] penSizeButtonImages;
    public Color32[] colors;

    [Header("Event")]
    public Toggle eraser;
    public Button undoButton;
    public Button redoButton;
    public Button completeButton;
    public Button[] colorButtons;
    public Button mainPenButton;
    public Button[] penSizeButtons;

    [Header("Tutorial")]
    public GameObject curvedArrow;
    public GameObject tutorial;

    Dictionary<Button, int> colorDic;
    GameObject playerControllerPanel;
    Sequence sketchCloseSequence;
    Sequence openPenSizeSequence;
    Sequence cloasePenSizeSequence;

    private void Awake()
    {
        colorDic = new Dictionary<Button, int>();
    }

    private void Start()
    {
        SequenceInit();

        for (int i = 0; i < colorButtons.Length; i++)
        {
            colorDic.Add(colorButtons[i], i);
        }
    }

    private void SequenceInit()
    {
        InitSketchCloseSequence();
        InitOpenPenSizeSequence();
        InitClosePenSizeSequence();
    }

    #region SequenceInit
    private void InitSketchCloseSequence()
    {
        sketchCloseSequence = DOTween.Sequence()
            .SetAutoKill(false)
            .Pause()
            .Append(sketchPanel.DOAnchorPosY(-50f, 0.3f))
            .Append(sketchPanel.DOAnchorPosY(5000f, 0.2f));
    }

    private void InitOpenPenSizeSequence()
    {
        openPenSizeSequence = DOTween.Sequence()
            .SetAutoKill(false)
            .Pause()
            .AppendCallback(() =>
            {
                for (int i = 0; i < penSizeButtons.Length; i++)
                {
                    penSizeButtons[i].GetComponent<RectTransform>().DOAnchorPosX(i * (-110f), 0.1f);
                }

                for (int i = 0; i < penSizeButtons.Length; i++)
                {
                    penSizeButtons[i].GetComponent<RectTransform>().DOAnchorPosX(i * (-100f), 0.1f);
                }
            });
    }

    private void InitClosePenSizeSequence()
    {
        cloasePenSizeSequence = DOTween.Sequence()
            .SetAutoKill(false)
            .Pause()
            .AppendCallback(() =>
            {
                for (int i = 0; i < penSizeButtons.Length; i++)
                {
                    penSizeButtons[i].GetComponent<RectTransform>().DOAnchorPosX(i * (-110f), 0.1f);
                }

                for (int i = 0; i < penSizeButtons.Length; i++)
                {
                    penSizeButtons[i].GetComponent<RectTransform>().DOAnchorPosX(0, 0.1f);
                }
            })
            .OnComplete(() =>
            {
                mainPenButton.gameObject.SetActive(true);

                for (int i = 0; i < penSizeButtons.Length; i++)
                {
                    penSizeButtons[i].gameObject.SetActive(false);
                }
            });
    }
    #endregion

    public void Init(GameObject backGround, GameObject playerPanel)
    {
        sketchBackGround = backGround;
        playerControllerPanel = playerPanel;

        SetColor(Color.black);
        SetPenSize(1);

        TutorialRoutine().Forget();
    }

    async UniTaskVoid TutorialRoutine()
    {
        while (!drawing.startDrawing)
        {
            curvedArrow.transform.rotation = Quaternion.Lerp(curvedArrow.transform.rotation, Quaternion.Euler(new Vector3(0f, 0f, 90f)), Time.deltaTime * 3f);

            if (Mathf.Abs(curvedArrow.transform.rotation.eulerAngles.z - 90f) <= 0.01f)
            {
                curvedArrow.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            }

            await UniTask.Yield();
        }

        curvedArrow.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        tutorial.SetActive(false);
    }

    #region Listner
    public void Eraser()
    {
        if (drawing != null)
            drawing.IsEraser = eraser.isOn;
    }

    public void Complete()
    {
        drawing.ClickCompleteButton(this);
    }

    public void Undo()
    {
        drawing.ClickUndoButton();
    }

    public void Redo()
    {
        drawing.ClickRedoButton();
    }

    public void Back()
    {
        drawing.ClickBackButton(this);
    }
    #endregion

    #region Color
    public void ChangeColor(Button choice)
    {
        SetColor(colors[colorDic[choice]]);

        drawing.ChangeColor(colorDic[choice]);
    }

    private void SetColor(Color color)
    {
        for (int i = 0; i < penSizeButtonImages.Length; i++)
        {
            penSizeButtonImages[i].color = color;
        }
    }
    #endregion

    #region PenSize
    public void PenSize()
    {
        mainPenButton.gameObject.SetActive(false);

        for (int i = 0; i < penSizeButtons.Length; i++)
        {
            penSizeButtons[i].gameObject.SetActive(true);
        }

        if (openPenSizeSequence.IsPlaying() || openPenSizeSequence.IsComplete())
            openPenSizeSequence.Restart();
        else
            openPenSizeSequence.Play();
    }

    public void SelectPenSize(Button button)
    {
        SetPenSize(int.Parse(button.gameObject.name));

        ClosePenSize();
    }

    private void SetPenSize(int size)
    {
        drawing?.SetPenRadius(size + 2);
        drawing.curPenRadius = drawing.penRadius;

        float scale = 0.3f + (0.2f * size);
        mainPenButton.transform.GetChild(0).localScale = new Vector3(scale, scale, scale);
    }

    public void ClosePenSize()
    {
        if (cloasePenSizeSequence.IsPlaying() || cloasePenSizeSequence.IsComplete())
            cloasePenSizeSequence.Restart();
        else
            cloasePenSizeSequence.Play();
    }
    #endregion

    public void CompleteAnimation(bool isBack)
    {
        if (sketchCloseSequence.IsPlaying() || sketchCloseSequence.IsComplete())
            sketchCloseSequence.Restart();
        else
            sketchCloseSequence.Play();

        sketchCloseSequence.OnComplete(null);
        sketchCloseSequence.OnComplete(() =>
        {
            sketchBackGround?.SetActive(false);

            if (isBack)
            {
                PhotonNetwork.Destroy(parent);
            }

            playerControllerPanel?.SetActive(true);
        });
    }
}
