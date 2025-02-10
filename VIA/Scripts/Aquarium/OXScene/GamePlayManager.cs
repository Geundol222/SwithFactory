using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;
using Cysharp.Threading.Tasks;
using System;

public class GamePlayManager : MonoBehaviourPun
{
    [Header("Intro")]
    public OXManager oxManager;
    public float countDownTimer;
    public TMP_Text text;

    [Header("Game")]
    OXQuiz oxQuiz;
    public string answer { get; private set; }
    public float gameCountDown = 20f;
    public TMP_Text gameTimeText;
    public TMP_Text questionText;
    public GameObject gameGroup;
    List<OXData> receiveData;
    int quizIndex;

    public UnityAction onRoundEnd;

    private void Awake()
    {
        oxQuiz = Resources.Load<OXQuiz>("Data/OXQuizData");

        receiveData = oxQuiz.QuizData.ToList();
    }

    private void Start()
    {
        text.text = "";
    }

    public void GameStart()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            //PhotonNetwork.CurrentRoom.IsOpen = false;

            if (receiveData.Count > 0)
            {
                oxManager.customProperty.SetLoadTime(PhotonNetwork.CurrentRoom, PhotonNetwork.ServerTimestamp);
                photonView.RPC("SetTime", RpcTarget.AllBuffered, oxManager.customProperty.GetLoadTime(PhotonNetwork.CurrentRoom));
            }
            else
            {
                photonView.RPC("QuizOver", RpcTarget.AllBuffered);
            }
        }
    }

    [PunRPC]
    public void QuizOver()
    {
        text.gameObject.SetActive(true);
        text.text = "Quiz Over!!";

        QuizOverRoutine().Forget();
    }

    async UniTaskVoid QuizOverRoutine()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(5f));

        oxManager.LeaveRoom();
    }

    [PunRPC]
    public void SetTime(int loadTime)
    {
        GameStartTimer(loadTime).Forget();
    }

    async UniTaskVoid GameStartTimer(int loadTime)
    {
        text.gameObject.SetActive(true);

        while (countDownTimer > (PhotonNetwork.ServerTimestamp - loadTime) / 1000f)
        {
            int remainTime = (int)(countDownTimer - (PhotonNetwork.ServerTimestamp - loadTime) / 1000f);
            text.text = $"{++remainTime}";
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
        }

        text.text = "시작!";
        await UniTask.Delay(TimeSpan.FromSeconds(1f));
        text.gameObject.SetActive(false);

        if (PhotonNetwork.IsMasterClient)
            SetQuestion();
    }

    public void SetQuestion()
    {
        int randomIndex = UnityEngine.Random.Range(0, receiveData.Count);
        int setTime = PhotonNetwork.ServerTimestamp;

        photonView.RPC("Question", RpcTarget.AllBuffered, randomIndex);
        photonView.RPC("SettingGameTime", RpcTarget.AllBuffered, setTime);
    }

    [PunRPC]
    public void Question(int randomIndex)
    {
        quizIndex = randomIndex;
        gameGroup.SetActive(true);

        questionText.text = receiveData[quizIndex].question;
    }

    [PunRPC]
    public void SettingGameTime(int setTime)
    {
        float countDown = gameCountDown;

        UpdateTimerRoutine(countDown, setTime).Forget();
    }

    async UniTaskVoid UpdateTimerRoutine(float countDown, int setTime)
    {        
        while (countDown > (PhotonNetwork.ServerTimestamp - setTime) * 0.001f)
        {
            int remainTime = (int)(countDown - (PhotonNetwork.ServerTimestamp - setTime) / 1000f);

            gameTimeText.text = $"{++remainTime}";
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
        }

        gameTimeText.text = "Time Out!";
        questionText.fontSize = 5;
        questionText.text = "정답은...";
        await UniTask.Delay(TimeSpan.FromSeconds(1.5f));

        answer = receiveData[quizIndex].answer;
        onRoundEnd.Invoke();
        questionText.text = $"{receiveData[quizIndex].answer}!";
        receiveData.RemoveAt(quizIndex);
        await UniTask.Delay(TimeSpan.FromSeconds(5f));

        gameGroup.SetActive(false);
        questionText.fontSize = 2;
        GameStart();
    }
}
