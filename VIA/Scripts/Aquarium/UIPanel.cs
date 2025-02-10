using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UIPanel : MonoBehaviour
{
    Sequence dialogueOpenSequence;
    Sequence dialogueCloseSequence;

    protected virtual void Start()
    {
        SequenceInit(GetComponent<RectTransform>());
    }

    private void SequenceInit(RectTransform target)
    {
        InitOpenDialogueSequence(target);
        InitCloseDialogueSequence(target);
    }

    #region SequenceInit
    private void InitOpenDialogueSequence(RectTransform target)
    {
        dialogueOpenSequence = DOTween.Sequence()
            .SetAutoKill(false)
            .Pause()
            .Append(target.DOAnchorPosY(350f, 0.1f))
            .Append(target.DOAnchorPosY(300f, 0.3f));
    }

    private void InitCloseDialogueSequence(RectTransform target)
    {
        dialogueCloseSequence = DOTween.Sequence()
            .SetAutoKill(false)
            .Pause()
            .Append(target.DOAnchorPosY(350f, 0.3f))
            .Append(target.DOAnchorPosY(-800f, 0.1f));
    }
    #endregion

    public void OpenDialogueUI()
    {
        if (dialogueOpenSequence.IsPlaying() || dialogueOpenSequence.IsComplete())
            dialogueOpenSequence.Restart();
        else
            dialogueOpenSequence.Play();
    }

    public void CloseDialogueUI()
    {
        if (dialogueCloseSequence.IsPlaying() || dialogueCloseSequence.IsComplete())
            dialogueCloseSequence.Restart();
        else
            dialogueCloseSequence.Play();        
    }
}
