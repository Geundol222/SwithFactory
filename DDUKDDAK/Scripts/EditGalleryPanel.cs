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
                closeRemainText.text = $"<color=#F44F4F>* CLOSE �Ⱓ {currentGallery.closeString}</color> ����";
            }

            if (!currentGallery.canOpen)
            {
                closeRemainText.text = $"<color=#F44F4F>* CLOSE �Ⱓ 3�� 0�ð� 0��</color> ����";
            }
        }
    }

    public void SettingPanel(OpenState state, string name, string create, string delete, GalleryButton gallery)
    {
        currentGallery = gallery;
        currentState = state;
        nameText.text = name;
        createDate.text = $"{create} ������";
        createCache = create;

        deleteDate.text = delete;
        deleteCache = delete;

        if (currentState == OpenState.Close)
        {
            if (closeRemainText != null)
                closeRemainText.gameObject.SetActive(true);

            closeTimeSpan = TimeSpan.FromHours(currentGallery.closeRemainTime);
            closeRemainText.text = $"<color=#F44F4F>* CLOSE �Ⱓ {closeTimeSpan.Days}�� {closeTimeSpan.Hours}�ð� {closeTimeSpan.Minutes}��</color> ����";
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
            feedManager.SetNotice("�������� ���� �ʾ����Ƿ�\n�������� ������ �� �����ϴ�.");
            return;
        }

        if (currentState == OpenState.Open)
        {
            feedManager.SetNotice("������ ���� ���¸� <color=#4A7CFE>OPEN</color> ����\n<color=#F44F4F>CLOSE</color> �� �����ұ��?\n(CLOSE ���� �Ⱓ 1�� ��� ����)",
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
            feedManager.SetNotice("������ ���� ���¸� <color=#F44F4F>CLOSE</color> ����\n<color=#4A7CFE>OPEN</color> �� �����ұ��?\n(OPEN ��, ��ũ ������ ���� ���ο�\r\n��ũ�� �߱޹޾� ����ؾ� �մϴ�.)",
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
