using Cinemachine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class PlayerController : MonoBehaviourPun
{
    public float TouchSensitivity_x = 10f;
    public float TouchSensitivity_y = 10f;

    [Header("Player")]
    public float jumpPower;
    public CinemachineFreeLook freeLookCam;
    public Transform camTarget;
    public Canvas controllerCanvas;
    public Button dieNextButton;
    public Button diePrevButton;
    public GameObject charaRef;
    public GameObject meshRoot;

    [Header("Move")]
    public LayerMask groundMask;
    public JoyStick joyStick;
    public float moveSpeed;
    public float backMoveSpeed;
    CharacterController controller;
    Animator anim;
    Vector3 moveDir;
    Vector3 dir;
    float ySpeed = 0f;
    float animMoveSpeed = 0f;
    float animBackMoveSpeed = 0f;
    bool isGround = false;
    public bool isHolding = false;

    [Header("Interaction")]
    public bool cameraLerping;
    public LayerMask interactionMask;
    public GameObject interactionButton;
    GameObject interactionTarget;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();

        CinemachineCore.GetInputAxis = CameraRotate;
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().name.Equals("GameScene"))
            freeLookCam = FindObjectOfType<CinemachineFreeLook>();

        interactionButton.SetActive(false);

        if (photonView.IsMine)
        {
            if (freeLookCam != null)
            {
                freeLookCam.Follow = camTarget;
                freeLookCam.LookAt = camTarget;
            }

            if (SceneManager.GetActiveScene().name.Equals("OXRoomScene"))
            {
                GameObject skillObj = PhotonNetwork.Instantiate("Prefabs/SkillPoint", Vector3.zero, Quaternion.identity);

                photonView.RPC("SetSkillTransform", RpcTarget.AllBuffered, skillObj?.GetPhotonView().ViewID, photonView.ViewID);
            }

            SwitchCanvas(false);


            dieNextButton.onClick.RemoveAllListeners();
            diePrevButton.onClick.RemoveAllListeners();
        }
        else
        {
            Destroy(controllerCanvas.gameObject);
            Destroy(this);
        }
    }

    [PunRPC]
    public void SetSkillTransform(int viewID, int ownerViewID)
    {
        GameObject skillObj = PhotonView.Find(viewID).gameObject;
        GameObject owner = PhotonView.Find(ownerViewID).gameObject;

        skillObj.transform.SetParent(owner.transform, false);
        skillObj.transform.localPosition = new Vector3(0f, 1f, 0f);
    }

    private void FixedUpdate()
    {
        if (isHolding)
            HoldMove();
        else
            Move();

        Jump();
    }

    #region Move
    public void Move()
    {
        moveDir = new Vector3(joyStick.inputVector.x, 0f, joyStick.inputVector.y);

        dir = Camera.main.transform.localRotation * Vector3.forward * moveDir.z + Camera.main.transform.localRotation * Vector3.right * moveDir.x;

        if (anim.GetLayerWeight(1) != 0f)
        {
            anim.SetLayerWeight(1, 0f);
            animBackMoveSpeed = 0f;
        }

        if (joyStick.isDrag)
        {
            if (controller.enabled)
                controller.Move(dir * Time.deltaTime * moveSpeed);

            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(new Vector3(0f, Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg, 0f)), 0.1f);

            animMoveSpeed = Mathf.Lerp(animMoveSpeed, 1f, Time.deltaTime * 5f);
        }
        else
        {
            animMoveSpeed = Mathf.Lerp(animMoveSpeed, 0f, Time.deltaTime * 5f);
        }

        if (Mathf.Abs(animMoveSpeed - anim.GetFloat("MoveSpeed")) > 0.01f)
        {
            anim.SetFloat("MoveSpeed", animMoveSpeed, 0.1f, Time.deltaTime);
        }
    }

    public void HoldMove()
    {
        moveDir = new Vector3(joyStick.inputVector.x, 0f, joyStick.inputVector.y);

        dir = Camera.main.transform.localRotation * Vector3.forward * moveDir.z + Camera.main.transform.localRotation * Vector3.right * moveDir.x;

        if (anim.GetLayerWeight(1) != 1f)
        {
            anim.SetLayerWeight(1, 1f);
            animMoveSpeed = 0f;
        }

        if (joyStick.isDrag)
        {
            if (controller.enabled)
                controller.Move(dir * Time.deltaTime * backMoveSpeed);

            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(new Vector3(0f, Mathf.Atan2(-dir.x, -dir.z) * Mathf.Rad2Deg, 0f)), 0.1f);

            animBackMoveSpeed = Mathf.Lerp(animBackMoveSpeed, 1f, Time.deltaTime * 5f);
        }
        else
        {
            animBackMoveSpeed = Mathf.Lerp(animBackMoveSpeed, 0f, Time.deltaTime * 5f);
        }

        if (Mathf.Abs(animBackMoveSpeed - anim.GetFloat("BackMoveSpeed")) > 0.01f)
        {
            anim.SetFloat("BackMoveSpeed", animBackMoveSpeed, 0.1f, Time.deltaTime);
        }
    }

    public void Jump()
    {
        ySpeed += Physics.gravity.y * Time.deltaTime;

        if (isGround && ySpeed < 0)
            ySpeed = 0f;

        if (controller.enabled)
            controller.Move(Vector3.up * ySpeed * Time.deltaTime);
    }

    public void ClickJumpButton()
    {
        if (isGround)
        {
            ySpeed = jumpPower;
            anim.SetTrigger("Jump");
            isGround = false;
        }
    }

    public void Warp(Vector3 point)
    {
        controller.enabled = false;
        transform.position = point;
        controller.enabled = true;

        if (!cameraLerping)
        {
            freeLookCam.PreviousStateIsValid = false;
            freeLookCam.OnTargetObjectWarped(camTarget, point);            
        }
    }
    #endregion

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

    public void ClickTalkButton(Player player)
    {
        if (player == PhotonNetwork.LocalPlayer)
        {
            controllerCanvas.gameObject.SetActive(false);
            IInteractable interactable = interactionTarget?.GetComponent<IInteractable>();
            interactable?.Interact(controllerCanvas.gameObject);
            interactionButton.gameObject.SetActive(false);
        }
        else
            return;
    }

    public void SwitchCanvas(bool isDied)
    {
        controllerCanvas.transform.GetChild(0).gameObject.SetActive(!isDied);
        controllerCanvas.transform.GetChild(1).gameObject.SetActive(isDied);
    }

    public void DieCanvasSetListner(UnityAction nextListner, UnityAction prevListner)
    {
        dieNextButton.onClick.AddListener(nextListner);
        diePrevButton.onClick.AddListener(prevListner);
    }

    #region Collision
    private void OnTriggerEnter(Collider other)
    {
        if (!photonView.IsMine)
            return;

        if (((1 << other.gameObject.layer) & interactionMask) != 0)
        {
            interactionButton.gameObject.SetActive(true);

            interactionButton.GetComponent<Button>().onClick.AddListener(delegate { ClickTalkButton(PhotonNetwork.LocalPlayer); });

            interactionTarget = other.gameObject;
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (!photonView.IsMine)
            return;

        if (((1 << other.gameObject.layer) & interactionMask) != 0)
        {
            interactionButton.gameObject.SetActive(false);
            interactionButton.GetComponent<Button>().onClick.RemoveListener(delegate { ClickTalkButton(PhotonNetwork.LocalPlayer); });
            interactionTarget = null;
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (((1 << hit.gameObject.layer) & groundMask) != 0)
        {
            isGround = true;
        }
    }
    #endregion

}
