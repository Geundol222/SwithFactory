using Cysharp.Threading.Tasks;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DrawObj : MonoBehaviourPun
{
    WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();

    public MeshRenderer modeling;
    public float lifeTime = 90f;
    public Canvas canvas;
    public TMP_Text timeText;
    public Material targetMat;

    Animator anim;
    public Animator modelAnim;
    Transform aquariumTransform;
    Drawing drawing;
    float randomOffset;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        drawing = FindObjectOfType<Drawing>();
        aquariumTransform = GameObject.FindGameObjectWithTag("Aquarium").transform;

        anim.StopRecording();

        if (!photonView.IsMine)
            anim.enabled = false;
    }

    private void Start()
    {
        anim.speed = Random.Range(0.6f, 1.1f);
        randomOffset = Random.Range(0f, 1f);
        transform.localScale = new Vector3((Random.Range(0, 2) * 2 + (-1)) * 0.05f, 0.05f, 0.05f);

        modelAnim.Play("Fish", 0, randomOffset);

        AnimationRoutine().Forget();
    }

    async UniTaskVoid AnimationRoutine()
    {
        await UniTask.WaitUntil(() => anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1);

        if (transform.localScale.x < 0 && anim.recorderMode != AnimatorRecorderMode.Offline)
            anim.speed *= -1;

        anim.SetBool("IsDrop", true);
    }

    private void LateUpdate()
    {
        UpdateTimeTextRotation();
    }

    private void UpdateTimeTextRotation()
    {
        if (transform.localScale.x < 0)
            timeText.transform.rotation = Quaternion.LookRotation(-Camera.main.transform.forward);
        else
            timeText.transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
    }

    public void OnComplete(Texture drawImage)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Texture2D tex = drawImage as Texture2D;

            byte[] bytes = tex.EncodeToPNG();

            int remainTime = PhotonNetwork.ServerTimestamp;

            photonView.RPC("SettingLifeTime", RpcTarget.AllBuffered, remainTime);
            photonView.RPC("ReceiveMat", RpcTarget.AllBuffered, bytes, tex.width, tex.height);
        }
        
    }

    #region RPC    
    [PunRPC]
    public void SettingLifeTime(int remainTime)
    {
        UpdateTimerRoutine(remainTime).Forget();
    }

    async UniTaskVoid UpdateTimerRoutine(int remainTime)
    {
        while (lifeTime > (PhotonNetwork.ServerTimestamp - remainTime) * 0.001f)
        {
            int minutes = (int)(lifeTime - (PhotonNetwork.ServerTimestamp - remainTime) * 0.001f) / 60;
            int seconds = (int)(lifeTime - (PhotonNetwork.ServerTimestamp - remainTime) * 0.001f) % 60;

            timeText.text = $"{minutes:00} : {seconds:00}";

            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
        }

        if (PhotonNetwork.IsMasterClient)
        {
            drawing.fishCount--;

            if (drawing.fishQueue.Count > 0)
            {
                DrawObj fish = PhotonNetwork.InstantiateRoomObject("Prefabs/DrawPlane", drawing.spawnPoint.position, Quaternion.identity).GetComponent<DrawObj>();

                fish?.OnComplete(drawing.fishQueue.Dequeue());
                drawing.fishCount++;
            }
            
            PhotonNetwork.Destroy(gameObject);
        }
    }

    [PunRPC]
    public void ReceiveMat(byte[] bytes, int width, int height)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);

        transform.SetParent(aquariumTransform, false);
        transform.localPosition = Vector3.zero;

        tex.LoadImage(bytes);

        Material mat = Instantiate(targetMat);
        mat.SetTexture("_DrawBaseMap", tex);

        modeling.material = mat;
    }
    #endregion
}
