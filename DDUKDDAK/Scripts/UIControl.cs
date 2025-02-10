using Crosstales.FB.Demo;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIControl : MonoBehaviour
{
    BackEndManager _backEndManager;
    public GalleryManager _galleryManager;
    public Examples _aws;
    RectTransform foldingTarget;

    [Header("Panel")]
    public GameObject uploadPanel;
    public GameObject saveGalleryPanel;

    [Header("UI")]
    public InputField galleryNameInput;
    public Sprite[] galleryNameState;
    public Button presetPanelButton;
    public Button uploadPanelButton;
    public Button saveGalleryButton;
    public CanvasGroup[] transparentGroup;
    public Image uploadMaxBar;
    public Slider transparentSlider;
    public TMP_Text transparentValue;
    public GameObject modalPanel;
    public CanvasGroup modalCG;
    public string galleryName;
    public RectTransform sidePanel;
    public RectTransform topPanel;

    [Header("Upload")]
    public GameObject[] editButtonSet;
    public TMP_Text capacityText;
    public Button uploadButton;
    public Sprite[] bigGalleryUpload;
    public Sprite[] normalGalleryUpload;

    [Space]
    public bool isModal;
    public bool isSet;
    public bool canUseName;
    public Vector2 modalOffset;
    bool isDuplicateName;

    private void Start()
    {
        _backEndManager = FindObjectOfType<BackEndManager>();
        if (!string.IsNullOrEmpty(_backEndManager.galleryName))
            galleryName = _backEndManager.galleryName;

        foreach(GameObject button in editButtonSet)
        {
            button.SetActive(_backEndManager.isMake);
        }
        
        Sprite[] selectGallerySprite = _backEndManager.mySize == "대형" ? bigGalleryUpload : normalGalleryUpload;

        uploadButton.image.sprite = selectGallerySprite[0];

        SpriteState spriteState = new SpriteState
        {
            highlightedSprite = selectGallerySprite[1],
            pressedSprite = selectGallerySprite[2]
        };

        uploadButton.spriteState = spriteState;

        transparentSlider.value = 100;
        transparentValue.text = "100";
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightBracket) || Input.GetKeyDown(KeyCode.LeftBracket))
        {
            OnClickFoldButton(sidePanel);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if(_galleryManager._player.isOBJ)
            {
                _galleryManager._player.useOBJ.OBJStop();
            }
            else
            {
                foldingTarget = topPanel;
                FoldingUI(new Vector2(0f, 0));
                _galleryManager.OnClickExitButton();
            }
        }

        if (uploadPanel.activeSelf)
        {
            uploadMaxBar.fillAmount = _aws.nowMB / _aws.maxMB;
            capacityText.text = $"{Math.Round(_aws.nowMB, 2)}MB/{Math.Round(_aws.maxMB, 2)}MB 사용";
        }

        if (isModal)
        {
            Vector2 mousePos = Input.mousePosition;
            modalPanel.transform.position = new Vector2(mousePos.x + modalOffset.x, mousePos.y + modalOffset.y);
        }
    }

    public void OnClickFoldButton(RectTransform target)
    {
        foldingTarget = target;

        if (target.gameObject.name.Contains("Side"))
        {
            if (target.anchoredPosition.x > 0)
                FoldingUI(new Vector2(-24f, 0f));
            else
                FoldingUI(new Vector2(425f, 0f));
        }
        else if (target.gameObject.name.Contains("Top"))
        {
            if (target.anchoredPosition.y == 0)
                FoldingUI(new Vector2(0f, 60f));
            else
                FoldingUI(new Vector2(0f, 0f));
        }
    }

    public void FoldingUI(Vector2 pos)
    {
        foldingTarget.anchoredPosition = pos;
    }

    public void OnValueChangedTransparent(Single value)
    {
        transparentValue.text = ((int)value).ToString();

        foreach (CanvasGroup cg in transparentGroup)
        {
            cg.alpha = (int)value * 0.01f + 0.1f;
        }
    }

    public void PressSaveBtnClick()
    {
        if (FindObjectOfType<ObjectControl>().isBuild)
        {
            _galleryManager.SetNotice("설치를 완료해주세요!");
            return;
        }
        _galleryManager.loadPanel.SetActive(true);
        if (!isSet)
        {
            _galleryManager.SetNotice("현재 제작중인 갤러리를\n저장 하시겠습니까?", false, () => {
                if (string.IsNullOrEmpty(galleryName))
                    saveGalleryPanel.SetActive(true);
                else
                {
                    StartCoroutine(Save(true));
                }
            });
        }
    }

    public void SaveGalleryNameButton()
    {
        galleryName = galleryNameInput.text;

        StartCoroutine(Save(false));
    }

    IEnumerator Save(bool tf)
    {
        isSet = true;

        if(!tf)yield return StartCoroutine(CheckGalleryName(galleryName));        
        if (isDuplicateName)
        {
            _galleryManager.SetNotice("중복된 이름이 있습니다.");
            yield break;
        }        

        _backEndManager.galleryName = galleryName;

        yield return new WaitForFixedUpdate();
        _backEndManager.OnClickGalleryInsertData();
        saveGalleryPanel.SetActive(false);
        isSet = false;
        _galleryManager.loadPanel.SetActive(false);
    }

    public void OnValueChangedInputGalleryName(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            saveGalleryButton.interactable = false;
        }
        else
        {
            saveGalleryButton.interactable = true;
        }
    }

    public void OnEndEditInputGalleryName(string value)
    {
        _backEndManager.GetMyGalleryNames();
        StartCoroutine(CheckGalleryName(value));
    }

    IEnumerator CheckGalleryName(string value)
    {
        yield return new WaitUntil(() => { return _backEndManager.allGalleriesName.Length > 0; });
        yield return new WaitForEndOfFrame();

        for(int i = 0; i < _backEndManager.allGalleriesName.Length; i++)
        {
            if (value.Equals(_backEndManager.allGalleriesName[i]))
            {
                galleryNameInput.GetComponent<Image>().sprite = galleryNameState[1];
                saveGalleryButton.interactable = false;
                isDuplicateName = true;
                break;
            }

            galleryNameInput.GetComponent<Image>().sprite = galleryNameState[0];
            saveGalleryButton.interactable = true;
            isDuplicateName = false;
        }
    }

    public void SetModalEnter(bool isHover)
    {
        isModal = isHover;

        if (isHover)
        {
            modalPanel.SetActive(true);
            StartCoroutine(FadeIn());
        }
        else
        {
            modalPanel.SetActive(false);
            modalCG.alpha = 0f;
        }
    }

    IEnumerator FadeIn()
    {
        float duration = 0.2f; // 디졸브 효과의 시간 (0.5초로 설정)
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            modalCG.alpha = Mathf.Clamp01(elapsedTime / duration);
            yield return null;
        }

        modalCG.alpha = 1f;
    }
}
