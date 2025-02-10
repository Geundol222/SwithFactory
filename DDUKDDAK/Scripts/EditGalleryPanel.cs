using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EditGalleryPanel : MonoBehaviour
{
    [Header("Texts")]
    public TMP_Text nameText;
    public TMP_Text createDate;
    string createCache;
    public TMP_Text deleteDate;
    string deleteCache;
    float remainTime;
    public TMP_Text closeRemainText;

    [Header("Buttons")]
    public Button nameEditButton;
    public Button[] editButtonSet;

    [Space]
    public Image stateImage;
    public OpenState currentState;
    public GalleryButton currentGallery;
    public FeedManager feedManager;
    public BackEndManager backEndManager;
    TimeSpan closeTimeSpan = TimeSpan.Zero;

    private void Start()
    {
        feedManager = FindObjectOfType<FeedManager>();
        backEndManager = FindObjectOfType<BackEndManager>();
    }

    private void Update()
    {
        if (closeRemainText != null)
        {
            if (currentGallery != null && !currentGallery.isOpen && currentGallery.isLinked && currentGallery.canOpen)
            {
                closeRemainText.text = $"<color=#F44F4F>* CLOSE 기간 {currentGallery.closeString}</color> 남음";
            }

            if (!currentGallery.canOpen)
            {
                closeRemainText.text = $"<color=#F44F4F>* CLOSE 기간 3일 0시간 0분</color> 남음";
            }
        }
    }

    public void SettingPanel(OpenState state, string name, string create, string delete, GalleryButton gallery)
    {
        currentGallery = gallery;
        currentState = state;
        nameText.text = name;
        createDate.text = $"{create} 생성됨";
        createCache = create;

        deleteDate.text = delete;
        deleteCache = delete;

        if (currentState == OpenState.Close)
        {
            if (closeRemainText != null)
                closeRemainText.gameObject.SetActive(true);

            closeTimeSpan = TimeSpan.FromHours(currentGallery.closeRemainTime);
            closeRemainText.text = $"<color=#F44F4F>* CLOSE 기간 {closeTimeSpan.Days}일 {closeTimeSpan.Hours}시간 {closeTimeSpan.Minutes}분</color> 남음";
        }
        else
        {
            if (closeRemainText != null)
                closeRemainText.gameObject.SetActive(false);
        }    

        switch(currentState)
        {
            case OpenState.Open:
                {
                    nameEditButton.interactable = false;
                    editButtonSet[0].interactable = false;
                    editButtonSet[0].GetComponent<ModalButton>().enabled = true;
                    editButtonSet[1].interactable = false;
                    editButtonSet[1].GetComponent<ModalButton>().enabled = true;
                    editButtonSet[2].interactable = true;
                    break;
                }
            case OpenState.Close:
                {
                    nameEditButton.interactable = true;
                    editButtonSet[0].interactable = true;
                    editButtonSet[0].GetComponent<ModalButton>().enabled = false;
                    editButtonSet[1].interactable = true;
                    editButtonSet[1].GetComponent<ModalButton>().enabled = false;
                    editButtonSet[2].interactable = false;
                    break;
                }
            case OpenState.Done:
                break;
            case OpenState.NeedToLink:
                break;
        }
    }

    public void SetImage(Sprite sprite, float width)
    {
        stateImage.GetComponent<RectTransform>().sizeDelta = new Vector2(width, stateImage.GetComponent<RectTransform>().sizeDelta.y);
        stateImage.sprite = sprite;
    }

    public void OnClickChangeState()
    {
        if (!currentGallery.canOpen)
        {
            feedManager.SetNotice("시작일이 되지 않았으므로\n갤러리를 오픈할 수 없습니다.");
            return;
        }

        if (currentState == OpenState.Open)
        {
            feedManager.SetNotice("갤러리 오픈 상태를 <color=#4A7CFE>OPEN</color> 에서\n<color=#F44F4F>CLOSE</color> 로 변경할까요?\n(CLOSE 가능 기간 1일 즉시 차감)",
                false,
                () => 
                {
                    Action act = new Action(() =>
                    {
                        feedManager.ChangeState(OpenState.Close);
                        SettingPanel(OpenState.Close , nameText.text , createCache , deleteCache , currentGallery);
                    });

                    WWWForm form = new WWWForm();
                    form.AddField(... , currentGallery.mySeq);
                    form.AddField(... , currentGallery.myCode);
                    form.AddField(... ,"false");
                    StartCoroutine(backEndManager.BaseSQL(backEndManager.updateGalleryURL , form, act));                    
                }
                );
            
        }
        else if (currentState == OpenState.Close)
        {
            feedManager.SetNotice("갤러리 오픈 상태를 <color=#F44F4F>CLOSE</color> 에서\n<color=#4A7CFE>OPEN</color> 로 변경할까요?\n(OPEN 후, 링크 공유를 눌러 새로운\r\n링크를 발급받아 사용해야 합니다.)",
                false,
                () => 
                {
                    Action act = new Action(() =>
                    {
                        SettingPanel(OpenState.Open , nameText.text , createCache , deleteCache , currentGallery);
                        feedManager.ChangeState(OpenState.Open);
                    });

                    WWWForm form = new WWWForm();
                    form.AddField(..., currentGallery.mySeq);
                    form.AddField(..., "true");
                    StartCoroutine(backEndManager.BaseSQL(backEndManager.updateGalleryURL , form , act));                    
                }
                );
        }
        else
            return;
    }

    public void OnClickEditGalleryButton()
    {
        backEndManager.currentGalleryCode = currentGallery.myCode;
    }
}
