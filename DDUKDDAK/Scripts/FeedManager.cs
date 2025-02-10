using Crosstales.FB;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FeedManager : MonoBehaviour
{
    public Sprite[] openStateSprite;
    public GameObject makeGalleryPanel;
    public GameObject openAndClosePanel;
    public GameObject doneAndLinkPanel;
    public GameObject noticeChild;
    public GalleryButton currentGallery;
    public GameObject editor;
    public GameObject guest;

    public void EditorButtonClick()
    {
        editor.SetActive(true);
        _BackEndManager.GetFeedGallery();
    }

    public void GuestButtonClick()
    {
        guest.SetActive(true);
        anyBodyHere.SetActive(false);
        thereIsNoResult.SetActive(false);
        OnNowEventButtonClick("Like");
    }

    public void InstantiateGalleryButton(JToken token)
    {
        GameObject obj = Instantiate(eventButton, myGalleries);
        GalleryButton galleryButton = obj.GetComponent<GalleryButton>();

        obj.transform.SetAsFirstSibling();
        int seq = int.Parse(token[...].ToString());
        string name = token[...].ToString().Split('_')[1];
        string createDate = token[...].ToString();
        string dateStart = token[...].ToString();
        string dateEnd = token[...].ToString();
        bool isOpen = true;
        if (!string.IsNullOrEmpty(token[...].ToString()))
            isOpen = bool.Parse(token[...].ToString());

        bool isDone = false;
        if (!string.IsNullOrEmpty(token[...].ToString()))
            isDone = bool.Parse(token[...].ToString());

        string code = token[...].ToString();
        string size = token[...].ToString();
        string closeDate = token[...].ToString();
        bool isLinked = bool.Parse(token[...].ToString());
        galleryButton.Init(seq, name, size, code, createDate, dateStart, dateEnd, isOpen, isDone, true, closeDate , isLinked);
    }

    public void InstantiateEmptyButton()
    {
        if (galleryCount >= 4)
            return;

        for (int i = 0; i < 4 - galleryCount; ++i)
        {
            Instantiate(emptyImage, myGalleries);
        }
    }

    public void SettingGallerySize(string size)
    {
        if (galleryCount == 4)
        {
            SetNotice("갤러리 목록이 가득 차 더이상\n갤러리를 생성할 수 없습니다.");
            return;
        }

        _feed.GalleryMakeButtonClick(size);
        makeGalleryPanel.SetActive(true);
    }

    public void OnClickGalleryButton(OpenState state, GalleryButton gallery)
    {
        currentGallery = gallery;

        doneAndLinkPanel.SetActive(false);
        openAndClosePanel.SetActive(false);

        switch (state)
        {
            case OpenState.Open:
                SetUpStatePanel(openAndClosePanel, state, gallery, openStateSprite[0], 87f);
                break;
            case OpenState.Close:
                SetUpStatePanel(openAndClosePanel, state, gallery, openStateSprite[1], 87f);
                break;
            case OpenState.Done:
                SetUpStatePanel(doneAndLinkPanel, state, gallery, openStateSprite[2], 79f);
                break;
            case OpenState.NeedToLink:
                SetUpStatePanel(doneAndLinkPanel, state, gallery, openStateSprite[3], 144f);
                break;
        }
    }

    public void SetUpStatePanel(GameObject panel, OpenState state, GalleryButton gallery, Sprite sprite, float imageHeight)
    {
        EditGalleryPanel editPanel = panel.GetComponent<EditGalleryPanel>();
        string startDate = gallery.createDate.ToString("yyyy.MM.dd HH:mm");
        string endDate = gallery.deleteDate.ToString("yyyy.MM.dd HH:mm");
        editPanel.SettingPanel(state, gallery.myName, startDate, endDate, gallery);
        editPanel.SetImage(sprite, imageHeight);
        panel.SetActive(true);
    }

    public void ChangeState(OpenState state)
    {
        currentGallery.ChangeState(state, currentGallery.closeDate, false);
        openAndClosePanel.GetComponent<EditGalleryPanel>().SetImage(openStateSprite[(int)state], 87f);
    }

    public void OpenLinkPopUp()
    {
        linkText.text = $"{linkbase}{currentGallery.myCode}";
    }

    public void OnClickCopyLink()
    {
        GUIUtility.systemCopyBuffer = linkText.text;

        SetNotice("갤러리 링크가 복사 되었습니다.\n게스트에게 링크를 전달해 주세요.");
    }

    public void SaveCloseState()
    {
        for (int i = 0; i < myGalleries.childCount; i++)
        {
            if (myGalleries.GetChild(i).GetComponent<GalleryButton>())
            {
                GalleryButton gallery = myGalleries.GetChild(i).GetComponent<GalleryButton>();
                _BackEndManager.UpdateCloseDate(gallery.mySeq, gallery.closeDate.ToString("yyyy-MM-dd HH:mm"));
            }
        }
    }
}
