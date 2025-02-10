using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StickGame : MonoBehaviour
{
    public enum Difficulty { Easy = 0, Middle, Hard, VeryHard }

    WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();

    [Header("Timer")]
    public Slider timer;
    public float remainTime;
    public TMP_Text startTimerText;

    [Header("Explain")]
    public Image indianImg;
    public Image stuffImg;
    public TMP_Text explainText;
    public TMP_Text resultText;
    public TMP_Text scoreText;

    [Header("List")]
    public List<Button> stuffButtons;
    public List<string> answerStickString;
    public List<TMP_Text> answerTexts;
    public List<GameObject> stuffs;

    public IndianPanel indianPanel;
    public CanvasGroup canvasGroup;
    public GameSceneManager gameSceneManager;

    int startTime = 3;
    int answerCount = 0;
    Difficulty curDifficulty = Difficulty.Easy;
    int round = 10;
    bool isGameStart = false;
    int stuffIndex = 0;
    bool isPause = false;

    GameObject playerControllerPanel;

    Stack<string> answerStack;
    Stack<string> respondStack;

    StickAnswerData stickData;

    private void Awake()
    {
        stickData = Resources.Load<StickAnswerData>("Data/StickData");
        answerStack = new Stack<string>();
        respondStack = new Stack<string>();
    }

    private void Start()
    {
        indianImg.gameObject.SetActive(true);
        stuffImg.gameObject.SetActive(false);
        explainText.text = "내가 주문이 들어온 꼬치를 보여주면,\n따라서 만들고, 완성을 누르면 돼! 제한시간은 짧으니, 서둘러서 만드는게 좋을꺼야!";
    }

    public void Init(GameObject playerControllerPanel)
    {
        this.playerControllerPanel = playerControllerPanel;

        // 도전횟수 차감

        isPause = false;
        answerCount = 0;
        round = 10;
        curDifficulty = Difficulty.Easy;
        answerCount = 0;
        stuffIndex = 0;
        startTime = 3;

        timer.minValue = 0;
        timer.maxValue = remainTime;
        timer.value = remainTime;

        StartExplainRoutine().Forget();
    }

    async UniTaskVoid StartExplainRoutine()
    {
        scoreText.text = $"{answerCount} / 10";

        canvasGroup.interactable = false;
        explainText.transform.parent.gameObject.SetActive(true);
        startTimerText.gameObject.SetActive(true);
        indianImg.gameObject.SetActive(true);
        stuffImg.gameObject.SetActive(false);

        while (startTime > 0)
        {
            startTimerText.text = $"{startTime}";
            DOTween.To(() => 0f, x => startTimerText.transform.localScale = new Vector3(x, x, x), 1f, 0.3f);

            await UniTask.Delay(TimeSpan.FromSeconds(1f));
            startTime--;
        }

        indianImg.gameObject.SetActive(false);
        stuffImg.gameObject.SetActive(true);
        InitAnswer();

        startTimerText.gameObject.SetActive(false);
        explainText.transform.parent.gameObject.SetActive(false);

        isGameStart = true;
        canvasGroup.interactable = true;
        GameTimer().Forget();
    }

    async UniTaskVoid GameTimer()
    {
        while (isGameStart)
        {
            if (timer.value > 0f)
            {
                if (!isPause)
                    timer.value -= Time.deltaTime;
            }
            else
                isGameStart = false;

            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
        }

        GameOverUI(false);
        remainTime = 60f;
    }

    public void ResetButtons()
    {
        respondStack.Clear();

        stuffIndex = 0;

        for(int i = 0; i < stuffs.Count; i++)
        {
            stuffs[i].GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            stuffs[i].SetActive(false);
        }
    }

    public void ClickStuffButton(Button button)
    {
        if (respondStack.Count < stickData.Stick[(int)curDifficulty].difficulty)
        {
            stuffs[stuffIndex].SetActive(true);
            stuffs[stuffIndex].GetComponent<Image>().sprite = button.transform.GetChild(0).GetComponent<Image>().sprite;
            
            respondStack.Push(stuffs[stuffIndex].GetComponent<Image>().sprite.name);

            Tweener tween = stuffs[stuffIndex].GetComponent<RectTransform>().DOAnchorPosX(550f + (-50f * stuffIndex++), 0.5f);
        }        
    }

    public void InitAnswer()
    {
        if (round > 6)
        {
            curDifficulty = Difficulty.Easy;            
        }
        else if (round > 4)
        {
            curDifficulty = Difficulty.Middle;
        }
        else if (round > 2)
        {
            curDifficulty = Difficulty.Hard;
        }
        else
        {
            curDifficulty = Difficulty.VeryHard;
        }

        int index = UnityEngine.Random.Range(0, 3);
        int length = stickData.Stick[(int)curDifficulty].stuffData[index].stuff.Length;

        for (int i = 0; i < length; i++)
        {
            answerStack.Push(stickData.Stick[(int)curDifficulty].stuffData[index].stuff[i]);
            answerTexts[i].text = stickData.Stick[(int)curDifficulty].stuffData[index].stuff[i];
            answerTexts[i].gameObject.SetActive(true);
        }
    }

    public void ClickCompleteButton()
    {
        round--;

        CheckAnswer();
        scoreText.text = $"{answerCount} / 10";

        if (round <= 0)
        {
            GameOverUI(false);
            remainTime = 60f;
        }
    }

    private void CheckAnswer()
    {
        int answer = answerStack.Count;
        int count = 0;

        if (respondStack.Count == answerStack.Count)
        {
            for (int i = 0; i < answer; i++)
            {
                if (answerStack.Pop() == respondStack.Pop())
                    count++;
            }

            if (count == answer)
            {
                if (round > 0)
                    ResultUI("정답!", true);
                answerCount++;
            }
            else
            {
                if (round > 0)
                    ResultUI("오답!", false);
            }
        }
        else
        {
            if (round > 0)
                ResultUI("오답!", false);
        }
    }

    public void ClickGiveUpButton()
    {
        isPause = true;
        canvasGroup.interactable = false;

        gameSceneManager.ShowPopUpUI("게임을 포기하시겠습니까?\n도전횟수는 차감됩니다.",
            delegate
            {
                GameOverUI(true);
            },
            delegate
            {
                isPause = false;
                canvasGroup.interactable = true;
            });
    }

    public void ResultUI(string result, bool isCorrect)
    {
        Sequence sequence = DOTween.Sequence()
            .OnStart(() =>
            {
                isPause = true;

                canvasGroup.interactable = false;

                resultText.gameObject.SetActive(true);
                resultText.text = result;

                if (isCorrect)
                    resultText.color = Color.green;
                else
                    resultText.color = Color.red;
            })
            .Append(resultText.GetComponent<RectTransform>().DOAnchorPosY(0f, 0.3f))
            .Append(resultText.GetComponent<RectTransform>().DOAnchorPosY(30f, 0.2f))
            .Append(resultText.GetComponent<RectTransform>().DOAnchorPosY(0f, 0.1f))
            .AppendInterval(0.5f)
            .OnComplete(() =>
            {
                for (int i = 0; i < answerTexts.Count; i++)
                {
                    answerTexts[i].gameObject.SetActive(false);
                }

                respondStack.Clear();
                answerStack.Clear();

                resultText.gameObject.SetActive(false);
                resultText.GetComponent<RectTransform>().anchoredPosition = new Vector2(-130f, 720f);
                ResetButtons();
                InitAnswer();

                canvasGroup.interactable = true;
                isPause = false;
            });
    }

    public void GameOverUI(bool isGiveUp)
    {
        Sequence sequence = DOTween.Sequence()
            .OnStart(() =>
            {
                isPause = true;

                canvasGroup.interactable = false;

                resultText.gameObject.SetActive(true);
                resultText.color = Color.red;
                resultText.text = "GameOver!";
            })
            .Append(resultText.GetComponent<RectTransform>().DOAnchorPosY(0f, 0.3f))
            .Append(resultText.GetComponent<RectTransform>().DOAnchorPosY(30f, 0.2f))
            .Append(resultText.GetComponent<RectTransform>().DOAnchorPosY(0f, 0.1f))
            .AppendInterval(0.5f)
            .OnComplete(() =>
            {
                for (int i = 0; i < answerTexts.Count; i++)
                {
                    answerTexts[i].gameObject.SetActive(false);
                }

                respondStack.Clear();
                answerStack.Clear();

                resultText.gameObject.SetActive(false);
                resultText.GetComponent<RectTransform>().anchoredPosition = new Vector2(-130f, 720f);
                ResetButtons();

                indianPanel.CompleteAnimation();
                canvasGroup.interactable = true;

                if (!isGiveUp)
                    indianPanel.ResultUI(answerCount);
                else
                    playerControllerPanel?.SetActive(true);
            });
    }
}
