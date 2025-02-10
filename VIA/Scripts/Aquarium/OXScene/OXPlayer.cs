using Cinemachine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class OXPlayer : MonoBehaviourPun
{
    public float TouchSensitivity_x = 10f;
    public float TouchSensitivity_y = 10f;

    public bool isDefeated = false;
    bool isDied = false;
    bool isInvincible = false;
    int answerCount;
    string myAnswer;
    LayerMask OMask;
    LayerMask XMask;

    OXManager oxManager;
    Camera mainCam;
    Canvas inGameCanvas;
    TMP_Text stateText;
    Animator anim;
    PlayerController playerController;
    CharacterController controller;
    GamePlayManager gamePlayManager;

    CinemachineFreeLook watchingCam;
    CinemachineFreeLook playerCam;
    GameObject charaRef;
    GameObject meshRoot;
    [HideInInspector] public Transform camTarget;
    int watchingIndex = 0;

    private void Awake()
    {
        CinemachineFreeLook[] freeLooks = FindObjectsOfType<CinemachineFreeLook>();

        foreach (CinemachineFreeLook freeLook in freeLooks)
        {
            if (freeLook.gameObject.name.Equals("MainCam"))
                playerCam = freeLook;
            else
                watchingCam = freeLook;
        }

        oxManager = FindObjectOfType<OXManager>();

        charaRef = transform.GetChild(0).gameObject;
        meshRoot = transform.GetChild(1).gameObject;
        camTarget = transform.GetChild(2);

        Canvas[] canvass = GetComponentsInChildren<Canvas>();

        for (int i = 0; i < canvass.Length; i++)
        {
            if (canvass[i].gameObject.name.Equals("InGameCanvas"))
                inGameCanvas = canvass[i];
        }

        mainCam = Camera.main;
        stateText = inGameCanvas.GetComponentInChildren<TMP_Text>();
        anim = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        playerController = GetComponent<PlayerController>();
        gamePlayManager = FindObjectOfType<GamePlayManager>();

        answerCount = 0;
        myAnswer = "";
        OMask = LayerMask.GetMask("O");
        XMask = LayerMask.GetMask("X");

        CinemachineCore.GetInputAxis = CameraRotate;
    }

    private void OnEnable()
    {
        gamePlayManager.onRoundEnd += RoundEnd;
    }

    private void Start()
    {
        if (photonView.IsMine)
        {
            playerCam.Priority = 100;
            watchingCam.Priority = 1;

            playerCam.Follow = camTarget;
            playerCam.LookAt = camTarget;

            watchingCam.Follow = camTarget;
            watchingCam.LookAt = camTarget;

        }

        stateText.gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        inGameCanvas.transform.rotation = Quaternion.LookRotation(mainCam.transform.forward);
    }

    public void RoundEnd()
    {
        if (myAnswer.Equals(gamePlayManager.answer))
        {
            answerCount++;
        }
        else if (myAnswer == "")
            return;
        else
        {
            isDied = true;
            Die();
        }
    }

    #region Defeated

    #region Pushed
    public void BePushed(Vector3 caster, float pushPower)
    {
        if (isInvincible)
            photonView.RPC("ShowState", RpcTarget.All, "公利!");
        else
        {
            if (photonView.IsMine && !isDefeated)
            {
                photonView.RPC("ShowState", RpcTarget.All, "剐啡促!");
                isDefeated = true;
                PushedRoutine(caster, pushPower).Forget();
            }
        }
    }

    async UniTaskVoid PushedRoutine(Vector3 caster, float pushPower)
    {
        playerController.enabled = false;
        transform.LookAt(caster);

        Vector3 direction = transform.position + (new Vector3(-transform.forward.x, 0f, -transform.forward.z) * pushPower);
        Vector3 dir = (direction - transform.position).normalized;

        photonView.RPC("SetTrigger", RpcTarget.All, "Pushed");

        while ((direction - transform.position).sqrMagnitude > 0.01f)
        {
            controller.Move(dir * Time.deltaTime * 7f);
            await UniTask.Yield();
        }
        await UniTask.WaitUntil(() => anim.GetCurrentAnimatorStateInfo(0).IsName("REFLESH00") && anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.8f);
        playerController.enabled = true;
        isDefeated = false;
    }
    #endregion

    #region Holded
    public void BeHolded(GameObject caster, float holdTime, Hold casterHold)
    {
        if (isInvincible)
            photonView.RPC("ShowState", RpcTarget.All, "公利!");
        else
        {
            if (photonView.IsMine && !isDefeated)
            {
                int myHold = GetComponentInChildren<Hold>().GetComponent<PhotonView>().ViewID;
                photonView.RPC("FalseCasting", RpcTarget.All, myHold);

                photonView.RPC("ShowState", RpcTarget.All, "棱躯促!");
                isDefeated = true;
                HoldedRoutine(caster, holdTime, casterHold).Forget();
            }
        }
    }

    [PunRPC]
    public void FalseCasting(int viewID)
    {
        Hold casterHold = PhotonView.Find(viewID).GetComponent<Hold>();

        if (casterHold.isCasting)
            casterHold.isCasting = false;
    }

    async UniTaskVoid HoldedRoutine(GameObject caster, float holdTime, Hold casterHold)
    {
        playerController.enabled = false;

        while (holdTime > 0f)
        {
            if (!casterHold.isCasting)
                break;

            transform.position = new Vector3(caster.transform.position.x, caster.transform.position.y, caster.transform.position.z + 0.5f);

            holdTime -= Time.deltaTime;
            await UniTask.Yield();
        }

        photonView.RPC("HoldingOver", RpcTarget.All, casterHold.GetComponent<PhotonView>().ViewID);
        isDefeated = false;
        playerController.enabled = true;
    }

    [PunRPC]
    public void HoldingOver(int viewID)
    {
        Hold casterHold = PhotonView.Find(viewID).GetComponent<Hold>();

        if (casterHold.GetComponent<PhotonView>().IsMine)
        {
            casterHold.ChangeHoldingProp(false);
            casterHold.isCasting = false;
        }
    }
    #endregion

    public void Invincible()
    {
        SetInvincible().Forget();
    }
    
    async UniTaskVoid SetInvincible()
    {
        isInvincible = true;

        await UniTask.Delay(TimeSpan.FromSeconds(3f));

        isInvincible = false;
    }

    #endregion

    [PunRPC]
    public void ShowState(string state)
    {
        SetStateText(state);
    }

    public void SetStateText(string state)
    {
        Color inItColor = stateText.color;

        inItColor.a = 1f;

        Sequence sequence = DOTween.Sequence()
            .OnStart(() =>
            {
                stateText.color = inItColor;
                stateText.text = state;
                stateText.gameObject.SetActive(true);
            })
            .AppendInterval(1f)
            .Append(stateText.DOFade(0f, 3f))
            .OnComplete(() =>
            {
                stateText.text = "";
                stateText.gameObject.SetActive(false);
                stateText.DOFade(1f, 0.1f);
            });
    }

    [PunRPC]
    public void SetTrigger(string name)
    {
        anim.SetTrigger(name);
    }

    private void OnDisable()
    {
        gamePlayManager.onRoundEnd -= RoundEnd;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (((1 << hit.gameObject.layer) & OMask) != 0)
        {
            myAnswer = "O";
        }
        else if (((1 << hit.gameObject.layer) & XMask) != 0)
        {
            myAnswer = "X";
        }
        else
        {
            myAnswer = "";
        }
    }


    #region IsDied

    public void Die()
    {
        playerController.SwitchCanvas(true);

        playerController.DieCanvasSetListner(delegate { WatchNextPlayer(); }, delegate { WatchPrevPlayer(); });

        Destroy(GetComponent<Rigidbody>());
        controller.enabled = false;
        anim.enabled = false;

        charaRef.SetActive(false);
        meshRoot.SetActive(false);

        photonView.RPC("ImDie", RpcTarget.Others, photonView.ViewID);

        playerCam.Priority = 1;
        watchingCam.Priority = 100;

        WatchingPlayer(oxManager.watchingPlayers[watchingIndex]);
    }

    [PunRPC]
    public void ImDie(int viewID)
    {
        OXPlayer diedPlayer = PhotonView.Find(viewID).GetComponent<OXPlayer>();

        Destroy(diedPlayer.GetComponent<Rigidbody>());
        diedPlayer.controller.enabled = false;
        diedPlayer.anim.enabled = false;

        diedPlayer.charaRef.SetActive(false);
        diedPlayer.meshRoot.SetActive(false);

        if (oxManager.watchingPlayers.Count > 0)
            oxManager.watchingPlayers.Remove(diedPlayer.gameObject);

        if (watchingCam.Follow != null && watchingCam.Follow == diedPlayer.camTarget)
            WatchNextPlayer();
    }

    public void WatchingPlayer(GameObject player)
    {
        watchingCam.Follow = player.GetComponent<OXPlayer>().camTarget;
        watchingCam.LookAt = player.GetComponent<OXPlayer>().camTarget;
    }

    public void WatchNextPlayer()
    {
        if (watchingIndex + 1 <= oxManager.watchingPlayers.Count - 1)
        {
            watchingIndex++;
            WatchingPlayer(oxManager.watchingPlayers[watchingIndex]);
        }
        else
        {
            watchingIndex = 0;
            WatchingPlayer(oxManager.watchingPlayers[watchingIndex]);
        }
    }

    public void WatchPrevPlayer()
    {
        if (watchingIndex - 1 >= 0)
        {
            watchingIndex--;
            WatchingPlayer(oxManager.watchingPlayers[watchingIndex]);
        }
        else
        {
            watchingIndex = oxManager.watchingPlayers.Count - 1;
            WatchingPlayer(oxManager.watchingPlayers[watchingIndex]);
        }
    }

    public float CameraRotate(string axis)
    {
        float inputAxis = 0f;

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            if (axis == "Mouse X")
                inputAxis = Input.touches[0].deltaPosition.x / TouchSensitivity_x;
            else if (axis == "Mouse Y")
                inputAxis = Input.touches[0].deltaPosition.y / TouchSensitivity_y;
        }
        else if (Input.GetMouseButton(1))
        {
            if (axis == "Mouse X")
                inputAxis = Input.GetAxis("Mouse X");
            else if (axis == "Mouse Y")
                inputAxis = Input.GetAxis("Mouse Y");
        }

        return inputAxis;
    }

    #endregion
}
