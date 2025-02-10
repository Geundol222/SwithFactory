using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

public enum ToolsEnum
{
    handPick,
    camcoder,

    homie,
    tProbe,

    brush,
    stainPack,

    rope,
    strainer,//거름채

    GPS,
    rater,//측정자

    clock,
    compass, //나침반

    tapeline, //줄자
    numberPlate, //증거번호판

    shavel,
    cottonSwab, //면봉

    digital_rectal_thermometer, //디지털 직장 온도계
    footprint_taker, //족적 촬영자

    scissors, //가위
    photographic_measurer, //촬영자

    evidence_envelope,//증거물 봉투
    flag,
}


public enum EvidenceEnum
{


}

public class PDAManager_BB : MonoBehaviour
{
//    #region Parameter

//    [Header("General")]
//    Manager_BB _manager_bb;
//    public Transform pivot;
//    public float lerp_co = 0.9f;
//    [Space]
//    [Header("UI")]
//    public Button[] tapBtns;
//    public GameObject[] tapBtnHLs;
//    public GameObject[] bgs;

//    //[Header("BloodUI")]
//    //public Button[] bloodButtons;
//    //public Transform bloodPopupTR;

//    [Space]
//    [Header("BuriedBodyUI")]
//    //public Button[] buriedBodyButtons;
//    [Space]
//    [Header("Tools")]
//    public ScrollRect toolsScrollRect;
//    RectTransform toolsButtons_TR_Rect;
//    Transform toolsButtons_TR;
//    List<GameObject> buriedBodyToolsHLs = new List<GameObject>();
//    public GameObject wrongToolPopup;
//    int selectedTool;


//    [Space]
//    [Header("Popups")]
//    public GameObject buriedBodyPlayPopup;
//    public GameObject quitPopup;
//    public GameObject testingPopup;

//    [Space]
//    [Header("Option")]
//    bool isChanged;
//    int index_tmp;
//    public GameObject optionChangedPopup;
//    public GameObject optionSavedPopup;
//    public Slider masterVolumeSlider;
//    public Slider sfxVolumeSlider;
//    public Slider voiceVolumeSlider;
//    public Text masterVolumeValue;
//    public Text sfxVolumeValue;
//    public Text voiceVolumeValue;

//    #endregion

//    void Awake()
//    {
//        UIInit();
//        BuriedBodyUIInit();
//        isChanged = false;
//        _manager_bb = FindObjectOfType<Manager_BB>();
//    }

//    private void Start()
//    {
//        if (pivot == null) pivot = GameObject.Find("LeftHandAnchor").transform;
//        transform.parent = null;

//    }

//    // Update is called once per frame
//    void Update()
//    {
//        transform.position = Vector3.Lerp(transform.position, pivot.position, lerp_co);
//        transform.rotation = Quaternion.Lerp(transform.rotation, pivot.rotation, lerp_co);
//    }

//    private void OnEnable()
//    {
//        SetPos();
//        ClearInit();
//    }


//    public void ClearInit()
//    {
//        //PDA 켜질시 초기화 기능
//        //AllOff();
//        OnClickTapButton(0);
//        //tapBtnHLs[0].SetActive(true);
//        //bgs[0].SetActive(true);

//        //음량 초기화
//        masterVolumeSlider.value = PlayerPrefs.GetInt("MasterVolume", -1) != -1 ? PlayerPrefs.GetInt("MasterVolume", -1) : 50;
//        sfxVolumeSlider.value = PlayerPrefs.GetInt("SFXVolume", -1) != -1 ? PlayerPrefs.GetInt("SFXVolume", -1) : 50;
//        voiceVolumeSlider.value = PlayerPrefs.GetInt("VoiceVolume", -1) != -1 ? PlayerPrefs.GetInt("VoiceVolume", -1) : 50;
//        //음량 문구 설정
//        masterVolumeValue.text = ((int)masterVolumeSlider.value).ToString();
//        sfxVolumeValue.text = ((int)sfxVolumeSlider.value).ToString();
//        voiceVolumeValue.text = ((int)voiceVolumeSlider.value).ToString();


//        isChanged = false;


//    }

//    public void SetPos()
//    {
//        transform.position = pivot.position;
//        transform.rotation = pivot.rotation;

//    }



//    #region UI
//    public void UIInit()
//    {
//        for (int i = 0; i < tapBtns.Length; i++)
//        {
//            int index = i;
//            tapBtns[i].onClick.AddListener(() => OnClickTapButton(index));
//        }



//        toolsButtons_TR = toolsScrollRect.content.transform;
//        toolsButtons_TR_Rect = toolsButtons_TR.GetComponent<RectTransform>();

//        evidence_TR = evidenceScrollRect.content.transform;
//        evidence_TR_Rect = evidence_TR.GetComponent<RectTransform>();



//        foreach (Transform tr in evidence_TR)
//        {
//            Destroy(tr.gameObject);
//        }
//        evedence_EmptyPanal.SetActive(true);

//    }
//    public void OnClickTapButton(int i)
//    {
//        //만일 옵션이 변경된 상태에서 다른 창으로 이동하려 할 시에.
//        if (i != 0 && isChanged)
//        {
//            index_tmp = i;
//            optionChangedPopup.SetActive(true);
//            return;
//        }

//        AllOff();

//        toolsScrollRect.velocity = Vector2.zero;
//        var pos_T = toolsButtons_TR_Rect.anchoredPosition;
//        pos_T.x = 0;
//        toolsButtons_TR_Rect.anchoredPosition = pos_T;

//        evidenceScrollRect.velocity = Vector2.zero;
//        var pos_E = evidence_TR_Rect.anchoredPosition;
//        pos_E.y = 0;
//        evidence_TR_Rect.anchoredPosition = pos_E;

//        //tapBtnHLs[i].SetActive(true);
//        switch (i)
//        {
//            case 0:
//            case 1:
//            case 2:
//            case 3:
//            case 4:
//                {
//                    tapBtnHLs[i].SetActive(true);
//                    bgs[i].SetActive(true);
//                    break;
//                }
//            case 5:
//                {
//                    //PDA끄기
//                    gameObject.SetActive(false);
//                    break;
//                }
//            case 6:
//                {
//                    //tapBtnHLs[i].SetActive(true);
//                    //bgs[0].SetActive(true);
//                    quitPopup.SetActive(true);
//                    break;
//                }
//        }
//    }
//    //모든  UI 초기화 : PDA를 새로 열때와 탭버튼 누를때 사용
//    void AllOff()
//    {

//        foreach (var bg in bgs)
//        {
//            bg.SetActive(false);
//        }


//        foreach (var tapBtnHL in tapBtnHLs)
//        {
//            tapBtnHL.SetActive(false);
//        }

//        //foreach (Transform bloodPopup in bloodPopupTR)
//        //{
//        //    bloodPopup.gameObject.SetActive(false);
//        //}

//        selectedTool = -1;
//        foreach (var hl in buriedBodyToolsHLs)
//        {
//            hl.SetActive(false);
//        }
//        //buriedBodyToolsScrollRect.


//        quitPopup.SetActive(false);
//        testingPopup.SetActive(false);
//        buriedBodyPlayPopup.SetActive(false);
//        wrongToolPopup.SetActive(false);

//    }



//    public void BuriedBodyUIInit()
//    {
//        for (int i = 0; i < toolsButtons_TR.childCount; i++)
//        {
//            int index = i;
//            toolsButtons_TR.GetChild(index).GetComponent<Button>().onClick.AddListener(() => OnClickToolsButton(index));
//            buriedBodyToolsHLs.Add(toolsButtons_TR.GetChild(index).GetChild(0).gameObject);
//        }
//    }

//    public void OnClickBB1Play()
//    {
//        _manager_bb.BB_Start();
//    }
//    public void OnClickQuitYesButton()
//    {
//#if UNITY_EDITOR || UNITY_EDITOR_64 || UNITY_EDITOR_WIN
//        UnityEditor.EditorApplication.isPlaying = false;
//#else
//        Application.Quit();
//#endif
//    }
//    #endregion
//    #region Tools


//    void OnClickToolsButton(int i)
//    {
//        foreach (var hl in buriedBodyToolsHLs)
//        {
//            hl.SetActive(false);
//        }
//        buriedBodyToolsHLs[i].SetActive(true);
//        selectedTool = i;
//    }

//    public void OnClickToolsConfirm()
//    {
//        if (selectedTool == -1) return;
//        foreach (var hl in buriedBodyToolsHLs)
//        {
//            hl.SetActive(false);
//        }

//        switch (selectedTool)
//        {
//            case (int)ToolsEnum.camcoder:
//                {
//                    //switch (_manager_bb._buriedBodyState)
//                    //{
//                    //	case BuriedBodyState.Capture_Info:
//                    //	case BuriedBodyState.Capture_GPS:
//                    //		_manager_bb.camcoder.SetActive();
//                    //		gameObject.SetActive(false);
//                    //		return;
//                    //}

//                    _manager_bb.camcoder.SetActive();
//                    gameObject.SetActive(false);
//                    break;
//                }
//            case (int)ToolsEnum.tProbe:
//                {
//                    switch (_manager_bb._buriedBodyState)
//                    {
//                        case BuriedBodyState.T_Probe:
//                            _manager_bb.t_Probe.SetActive();
//                            gameObject.SetActive(false);
//                            return;
//                    }
//                    break;
//                }
//            case (int)ToolsEnum.flag:
//                {

//                    switch (_manager_bb._buriedBodyState)
//                    {
//                        case BuriedBodyState.Flag:
//                            _manager_bb.flag.SetActive();
//                            gameObject.SetActive(false);
//                            return;
//                    }
//                    break;
//                }
//            case (int)ToolsEnum.GPS:
//                {

//                    switch (_manager_bb._buriedBodyState)
//                    {
//                        case BuriedBodyState.Capture_GPS:
//                            _manager_bb.GPS.SetActive();
//                            gameObject.SetActive(false);
//                            return;
//                    }
//                    break;
//                }
//            case (int)ToolsEnum.stainPack:
//                {

//                    switch (_manager_bb._buriedBodyState)
//                    {
//                        case BuriedBodyState.stainPack:
//                            _manager_bb.stainPack.SetActive();
//                            gameObject.SetActive(false);
//                            return;
//                    }
//                    break;
//                }
//            case (int)ToolsEnum.rope:
//                {

//                    switch (_manager_bb._buriedBodyState)
//                    {
//                        case BuriedBodyState.rope:
//                            _manager_bb.rope.SetActive();
//                            gameObject.SetActive(false);
//                            return;
//                    }
//                    break;
//                }
//            case (int)ToolsEnum.numberPlate:
//                {

//                    switch (_manager_bb._buriedBodyState)
//                    {
//                        case BuriedBodyState.numberPlate:
//                            _manager_bb.numberPlate.SetActive();
//                            gameObject.SetActive(false);
//                            return;
//                    }
//                    break;
//                }
//            case (int)ToolsEnum.shavel:
//                {

//                    switch (_manager_bb._buriedBodyState)
//                    {
//                        case BuriedBodyState.shavel:
//                            _manager_bb.shavel.SetActive();
//                            gameObject.SetActive(false);
//                            return;
//                    }
//                    break;
//                }
//            case (int)ToolsEnum.rater:
//                {

//                    switch (_manager_bb._buriedBodyState)
//                    {
//                        case BuriedBodyState.rater:
//                            _manager_bb.rater.SetActive();
//                            gameObject.SetActive(false);
//                            return;
//                    }
//                    break;
//                }

//            case (int)ToolsEnum.homie:
//                {

//                    switch (_manager_bb._buriedBodyState)
//                    {
//                        case BuriedBodyState.homie:
//                            _manager_bb.homie.SetActive();
//                            gameObject.SetActive(false);
//                            return;
//                    }
//                    break;
//                }

//            case (int)ToolsEnum.brush:
//                {

//                    switch (_manager_bb._buriedBodyState)
//                    {
//                        case BuriedBodyState.brush:
//                            _manager_bb.brush.SetActive();
//                            gameObject.SetActive(false);
//                            return;
//                    }
//                    break;
//                }


//                //default:
//                //	{
//                //		return;
//                //	}

//        }
//        wrongToolPopup.SetActive(true);
//        //gameObject.SetActive(false);S
//    }

//    #endregion
//    #region Evidence

//    [Space]
//    [Header("Evidence")]
//    public ScrollRect evidenceScrollRect;
//    RectTransform evidence_TR_Rect;
//    Transform evidence_TR;
//    public GameObject evidence_Orig;
//    public GameObject evedence_EmptyPanal;
//    public void AddEvidence(Texture2D _tex)
//    {
//        evedence_EmptyPanal.SetActive(false);

//        //Texture2D _tex = tex;
//        var evidence_clone = Instantiate(evidence_Orig, evidence_TR);
//        var _evidence = evidence_clone.GetComponent<Evidence>();
//        _evidence.Set(_tex);
//        //foreach (var e in FindObjectsOfType<Evidence>())
//        //{
//        //	e.SetOrder();
//        //}
//        //Destroy(tex);
//    }



//    #endregion
//    //public void BloodUIInit()
//    //{
//    //    //혈흔 분석 항목버튼 기능 연결
//    //    for (int i = 0; i < bloodButtons.Length; i++)
//    //    {
//    //        int index = i;
//    //        bloodButtons[index].onClick.RemoveAllListeners();
//    //        bloodButtons[index].onClick.AddListener(() => OnClickBloodButton(index));
//    //    }

//    //    //혈흔 분석 실행 여부 팝업 내부 버튼 연결
//    //    for (int i = 0; i < bloodPopupTR.childCount; i++)
//    //    {
//    //        int index = i;
//    //        Transform popup = bloodPopupTR.GetChild(index);
//    //        if (popup.childCount <= 0) continue;

//    //        //예 버튼 onclick 연결
//    //        popup.GetChild(0).GetChild(0).GetComponent<Button>().onClick.RemoveAllListeners();
//    //        popup.GetChild(0).GetChild(0).GetComponent<Button>().onClick.AddListener(() =>
//    //        {
//    //            popup.gameObject.SetActive(false);
//    //            OnClickBloodPlay(index);
//    //        });

//    //        //아니오 버튼 onclick 연결
//    //        popup.GetChild(0).GetChild(1).GetComponent<Button>().onClick.RemoveAllListeners();
//    //        popup.GetChild(0).GetChild(1).GetComponent<Button>().onClick.AddListener(() =>
//    //        {
//    //            popup.gameObject.SetActive(false);
//    //        });

//    //    }
//    //}

//    //좌상단 탭버튼 눌릴시


//    //public void OnClickBloodButton(int index)
//    //{
//    //    switch ((BloodEnum)index)
//    //    {

//    //        //팝업 준비 완료된 항목들은 해당 부분에 case문을 쌓아서 구축
//    //        case BloodEnum.Impact_Spatter:
//    //            {
//    //                //팝업 오픈
//    //                bloodPopupTR.GetChild(index).gameObject.SetActive(true);
//    //                break;
//    //            }

//    //        default:
//    //            {
//    //                //준비중일시 준비중 팝업 on
//    //                testingPopup.SetActive(true);
//    //                break;
//    //            }
//    //    }
//    //}




//    //public void OnClickBloodPlay(int index)
//    //{
//    //    switch ((BloodEnum)index)
//    //    {

//    //        //플레이 준비 완료된 항목들은 해당 부분에 case문을 쌓아서 구축
//    //        case BloodEnum.Impact_Spatter:
//    //            {
//    //                _manager.BloodPlay(index);
//    //                break;
//    //            }

//    //        default:
//    //            {
//    //                //준비중일시 준비중 팝업 on
//    //                testingPopup.SetActive(true);
//    //                return;
//    //            }
//    //    }
//    //}
//    #region Option    

//    public void OptionSavedYesButton()
//    {
//        //음량 정보 저장
//        PlayerPrefs.SetInt("MasterVolume", (int)masterVolumeSlider.value);
//        PlayerPrefs.SetInt("SFXVolume", (int)sfxVolumeSlider.value);
//        PlayerPrefs.SetInt("VoiceVolume", (int)voiceVolumeSlider.value);

//        isChanged = false;//변화 여부 초기화

//        optionSavedPopup.SetActive(false);

//    }

//    public void OptionChangedYesButton()
//    {
//        isChanged = false;
//        OnClickTapButton(index_tmp);//탭변경


//        //이전 변화 버전으로 변경
//        //onValueChanged로 인해 음량 설정은 자동세팅
//        masterVolumeSlider.value = PlayerPrefs.GetInt("MasterVolume", -1) != -1 ? PlayerPrefs.GetInt("MasterVolume", -1) : 50;
//        sfxVolumeSlider.value = PlayerPrefs.GetInt("SFXVolume", -1) != -1 ? PlayerPrefs.GetInt("SFXVolume", -1) : 50;
//        voiceVolumeSlider.value = PlayerPrefs.GetInt("VoiceVolume", -1) != -1 ? PlayerPrefs.GetInt("VoiceVolume", -1) : 50;

//        isChanged = false;//변화 여부 재초기화

//        optionChangedPopup.SetActive(false);
//    }

//    public void AdjustMasterVolume(Single s)
//    {
//        masterVolumeValue.text = ((int)masterVolumeSlider.value).ToString();
//        isChanged = true;
//        _manager_bb.SetSfxVolume(masterVolumeSlider.value * sfxVolumeSlider.value);
//    }
//    public void AdjustSFXVolume(Single s)
//    {
//        sfxVolumeValue.text = ((int)sfxVolumeSlider.value).ToString();
//        isChanged = true;
//        _manager_bb.SetSfxVolume(masterVolumeSlider.value * sfxVolumeSlider.value);
//    }
//    public void AdjustVoiceVolume(Single s)
//    {
//        voiceVolumeValue.text = ((int)voiceVolumeSlider.value).ToString();
//        isChanged = true;
//        //음성 audio 설정되었을떄 변경
//    }


//    #endregion
}
