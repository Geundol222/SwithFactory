using DG.Tweening;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class GameSceneManager : MonoBehaviourPunCallbacks
{
    public bool isTalkOtter = false;
    public bool isMoveOXRoom = true;

    public Transform playerSpawnPoint;
    public int aquariumMissionCount = 3;

    [Header("PopUp")]
    public RectTransform popUpUI;
    public TMP_Text popUpText;
    public Button yesButton;
    public Button noButton;

    Sequence popUpOpenSequence;
    Sequence popUpCloseSequence;

    private void Start()
    {
        if (!PhotonNetwork.IsConnected)
            PhotonNetwork.ConnectUsingSettings();

        PopupSequenceInit();
    }

    private void PopupSequenceInit()
    {
        InitOpenPopupSequence();
        InitClosePopupSequence();
    }

    #region SequenceInit
    private void InitOpenPopupSequence()
    {
        popUpOpenSequence = DOTween.Sequence()
            .SetAutoKill(false)
            .Pause()
            .Append(popUpUI.DOScale(1.1f, 0.2f))
            .Append(popUpUI.DOScale(1f, 0.1f));
    }

    private void InitClosePopupSequence()
    {
        popUpCloseSequence = DOTween.Sequence()
            .SetAutoKill(false)
            .Pause()
            .Append(popUpUI.DOScale(1.1f, 0.2f))
            .Append(popUpUI.DOScale(0f, 0.1f))
            .OnComplete(() =>
            {
                popUpText.text = "";
                popUpUI.gameObject.SetActive(false);
                yesButton.onClick.RemoveAllListeners();
            });
    }
    #endregion

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();

        print("Connect Success");

        string name = $"QuestRoom";
        RoomOptions options = new RoomOptions { MaxPlayers = 20 };

        PhotonNetwork.JoinOrCreateRoom(roomName: name, roomOptions: options, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        print("JoinRoom Success");

        PhotonNetwork.LocalPlayer.NickName = $"Player{UnityEngine.Random.Range(1000, 9999)}";
        PhotonNetwork.Instantiate("Prefabs/Player", playerSpawnPoint.position, Quaternion.identity);
    }

    //public override void OnLeftRoom()
    //{
    //    base.OnLeftRoom();
    //
    //    if (isMoveOXRoom)
    //    {
    //        PhotonNetwork.LoadLevel("OXRoomScene");
    //    }
    //}

    public void ShowPopUpUI(string text, UnityAction yesListner, UnityAction noListner = null)
    {
        yesButton.onClick.RemoveAllListeners();
        yesButton.onClick.AddListener(yesListner);
        yesButton.onClick.AddListener(ClosePopUpUI);

        if (noListner != null)
        {
            noButton.onClick.AddListener(noListner);
        }
            
        popUpUI.gameObject.SetActive(true);
        popUpText.text = text;

        if (popUpOpenSequence.IsPlaying() || popUpOpenSequence.IsComplete())
            popUpOpenSequence.Restart();
        else
            popUpOpenSequence.Play();
    }

    public void ClosePopUpUI()
    {
        if (popUpCloseSequence.IsPlaying() || popUpCloseSequence.IsComplete())
            popUpCloseSequence.Restart();
        else
            popUpCloseSequence.Play();
    }
}
