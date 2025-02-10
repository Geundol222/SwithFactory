using System;
using UnityEngine;
using UnityEngine.UI;

public class PDAManager : MonoBehaviour
{
    [Header("General")]
    GamePlayManager _manager;

    [Header("UI")]
    public GameObject[] tapBtnHLs;
    public GameObject[] bgs;

    [Header("BloodUI")]
    public Button[] bloodButtons;
    public Transform bloodPopupTR;

    [Header("ExplainUI")]
    public GameObject categoryObj;
    public GameObject explainObj;
    public Button[] explainButtons;
    public GameObject[] explainPanels;

    [Header("ToolUI")]
    public Button[] toolButtons;
    public ScrollRect toolsScrollRect;
    RectTransform toolsButtons_TR_Rect;
    Transform toolsButtons_TR;

    [Header("Evidence")]
    public ScrollRect evidenceScrollRect;
    RectTransform evidence_TR_Rect;
    Transform evidence_TR;
    public GameObject evidence_Orig;
    public GameObject evedence_EmptyPanel;

    [Header("Popups")]
    public GameObject quitPopup;
    public GameObject testingPopup;
    public GameObject sitePopup;

    [Header("Option")]
    public bool isChanged;
    int index_tmp;
    public GameObject optionChangedPopup;
    public GameObject optionSavedPopup;
    public Slider masterVolumeSlider;
    public Slider sfxVolumeSlider;
    public Slider voiceVolumeSlider;
    public Text masterVolumeValue;
    public Text sfxVolumeValue;
    public Text voiceVolumeValue;

    void Awake()
    {
        isChanged = false;
        _manager = FindObjectOfType<GamePlayManager>();
    }

    private void Start()
    {
        BloodUIInit();
        ExplainUIInit();
        ToolUIInit();
    }

    private void OnEnable()
    {
        ClearInit();
    }

    public void ClearInit()
    {
        //PDA 켜질시 초기화 기능
        AllOff();

        toolsButtons_TR = toolsScrollRect.content.transform;
        toolsButtons_TR_Rect = toolsButtons_TR.GetComponent<RectTransform>();

        evidence_TR = evidenceScrollRect.content.transform;
        evidence_TR_Rect = evidence_TR.GetComponent<RectTransform>();

        tapBtnHLs[0].SetActive(true);
        bgs[0].SetActive(true);

        //음량 초기화
        masterVolumeSlider.value = PlayerPrefs.GetInt("MasterVolume", -1) != -1 ? PlayerPrefs.GetInt("MasterVolume", -1) : 50;
        sfxVolumeSlider.value = PlayerPrefs.GetInt("SFXVolume", -1) != -1 ? PlayerPrefs.GetInt("SFXVolume", -1) : 50;
        voiceVolumeSlider.value = PlayerPrefs.GetInt("VoiceVolume", -1) != -1 ? PlayerPrefs.GetInt("VoiceVolume", -1) : 50;
        //음량 문구 설정
        masterVolumeValue.text = ((int)masterVolumeSlider.value).ToString();
        sfxVolumeValue.text = ((int)sfxVolumeSlider.value).ToString();
        voiceVolumeValue.text = ((int)voiceVolumeSlider.value).ToString();

        isChanged = false;
    }

    #region UI
    public void BloodUIInit()
    {
        //혈흔 분석 항목버튼 기능 연결
        for (int i = 0; i < bloodButtons.Length; i++)
        {
            int index = i;
            bloodButtons[index].onClick.RemoveAllListeners();
            bloodButtons[index].onClick.AddListener(() => OnClickBloodButton(index));
        }

        //혈흔 분석 실행 여부 팝업 내부 버튼 연결
        for (int i = 0; i < bloodPopupTR.childCount; i++)
        {
            int index = i;
            Transform popup = bloodPopupTR.GetChild(index);
            if (popup.childCount <= 0) continue;

            //예 버튼 onclick 연결
            popup.GetChild(0).GetChild(0).GetComponent<Button>().onClick.RemoveAllListeners();
            popup.GetChild(0).GetChild(0).GetComponent<Button>().onClick.AddListener(() =>
            {
                popup.gameObject.SetActive(false);
                OnClickBloodPlay(index);
            });

            //아니오 버튼 onclick 연결
            popup.GetChild(0).GetChild(1).GetComponent<Button>().onClick.RemoveAllListeners();
            popup.GetChild(0).GetChild(1).GetComponent<Button>().onClick.AddListener(() =>
            {
                popup.gameObject.SetActive(false);
            });

        }
    }

    public void ExplainUIInit()
    {
        //혈흔 분석 항목버튼 기능 연결
        for (int i = 0; i < explainButtons.Length; i++)
        {
            int index = i;
            explainButtons[index].onClick.RemoveAllListeners();
            explainButtons[index].onClick.AddListener(() => OnClickExplainButton(index));
        }
    }

    public void ToolUIInit()
    {
        for (int i = 0; i < toolButtons.Length; i++)
        {
            int index = i;
            toolButtons[index].onClick.RemoveAllListeners();
            toolButtons[index].onClick.AddListener(() => OnClickToolButton(index));
        }
    }

    //좌상단 탭버튼 눌릴시
    public void OnClickTapButton(int i)
    {
        //만일 옵션이 변경된 상태에서 다른 창으로 이동하려 할 시에.
        if (i != 0 && isChanged)
        {
            index_tmp = i;
            optionChangedPopup.SetActive(true);
            return;
        }

        toolsScrollRect.velocity = Vector2.zero;
        var pos_T = toolsButtons_TR_Rect.anchoredPosition;
        pos_T.x = 0;
        toolsButtons_TR_Rect.anchoredPosition = pos_T;

        evidenceScrollRect.velocity = Vector2.zero;
        var pos_E = evidence_TR_Rect.anchoredPosition;
        pos_E.y = 0;
        evidence_TR_Rect.anchoredPosition = pos_E;

        AllOff();
        //tapBtnHLs[i].SetActive(true);
        switch (i)
        {
            case 6:
                {
                    //PDA끄기
                    gameObject.SetActive(false);
                    break;
                }
            case 7:
                {
                    
                    quitPopup.SetActive(true);
                    break;
                }
            default:
                tapBtnHLs[i].SetActive(true);
                bgs[i].SetActive(true);
                break;
        }
    }

    //모든  UI 초기화 : PDA를 새로 열때와 탭버튼 누를때 사용
    void AllOff()
    {
        foreach (GameObject bg in bgs)
        {
            bg.SetActive(false);
        }


        foreach (GameObject tapBtnHL in tapBtnHLs)
        {
            tapBtnHL.SetActive(false);
        }

        foreach (Transform bloodPopup in bloodPopupTR)
        {
            bloodPopup.gameObject.SetActive(false);
        }

        ClearExplainUI();

        quitPopup.SetActive(false);
        testingPopup.SetActive(false);

    }

    public void ClearExplainUI()
    {
        explainObj.GetComponent<ScrollRect>().velocity = Vector2.zero;

        categoryObj.SetActive(true);
        explainObj.SetActive(false);

        foreach (GameObject obj in explainPanels)
        {
            obj.SetActive(false);
        }

    }

    public void OnClickQuitYesButton()
    {
#if UNITY_EDITOR || UNITY_EDITOR_64 || UNITY_EDITOR_WIN
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OnClickSiteAnalysis()
    {
        sitePopup.SetActive(true);
    }

    public void OnClickBloodButton(int index)
    {
        if (_manager.bloodPlayerPos[index] != null)
            bloodPopupTR.GetChild(index).gameObject.SetActive(true);
        else
            testingPopup.SetActive(true);
    }

    public void OnClickExplainButton(int index)
    {
        ClearExplainUI();

        categoryObj.SetActive(false);
        explainObj.SetActive(true);
        explainPanels[index].SetActive(true);
    }

    public void OnClickToolButton(int index)
    {
        _manager.ActiveTool(index);
    }

    public void OnClickBloodPlay(int index)
    {
        _manager.BloodPlay(index);
    }
    #endregion

    public void AddEvidence(Texture2D _tex)
    {
        evedence_EmptyPanel.SetActive(false);

        //Texture2D _tex = tex;
        var evidence_clone = Instantiate(evidence_Orig, evidence_TR);
        var _evidence = evidence_clone.GetComponent<Evidence>();
        _evidence.Set(_tex);
        //foreach (var e in FindObjectsOfType<Evidence>())
        //{
        //	e.SetOrder();
        //}
        //Destroy(tex);
    }

    #region Option
    public void OptionSavedYesButton()
    {
        //음량 정보 저장
        PlayerPrefs.SetInt("MasterVolume", (int)masterVolumeSlider.value);
        PlayerPrefs.SetInt("SFXVolume", (int)sfxVolumeSlider.value);
        PlayerPrefs.SetInt("VoiceVolume", (int)voiceVolumeSlider.value);

        isChanged = false;//변화 여부 초기화
        
        optionSavedPopup.SetActive(false);
    }

    public void OptionChangedYesButton()
    {
        isChanged = false;
        OnClickTapButton(index_tmp);//탭변경


        //이전 변화 버전으로 변경
        //onValueChanged로 인해 음량 설정은 자동세팅
        masterVolumeSlider.value = PlayerPrefs.GetInt("MasterVolume", -1) != -1 ? PlayerPrefs.GetInt("MasterVolume", -1) : 50;
        sfxVolumeSlider.value = PlayerPrefs.GetInt("SFXVolume", -1) != -1 ? PlayerPrefs.GetInt("SFXVolume", -1) : 50;
        voiceVolumeSlider.value = PlayerPrefs.GetInt("VoiceVolume", -1) != -1 ? PlayerPrefs.GetInt("VoiceVolume", -1) : 50;

        isChanged = false;//변화 여부 재초기화

        optionChangedPopup.SetActive(false);
    }

    public void AdjustMasterVolume(Single s)
    {
        masterVolumeValue.text = ((int)masterVolumeSlider.value).ToString();
        isChanged = true;
        _manager.SetSfxVolume(masterVolumeSlider.value * sfxVolumeSlider.value);
    }

    public void AdjustSFXVolume(Single s)
    {
        sfxVolumeValue.text = ((int)sfxVolumeSlider.value).ToString();
        isChanged = true;
        _manager.SetSfxVolume(masterVolumeSlider.value * sfxVolumeSlider.value);
    }

    public void AdjustVoiceVolume(Single s)
    {
        voiceVolumeValue.text = ((int)voiceVolumeSlider.value).ToString();
        isChanged = true;
        //음성 audio 설정되었을떄 변경
    }
    #endregion
}
