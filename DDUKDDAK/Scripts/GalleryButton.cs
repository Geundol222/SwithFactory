using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[Serializable]
public class GalleryInfo
{
    
}

public enum OpenState { Open, Close, Done, NeedToLink }

public class GalleryButton : MonoBehaviour, IPointerEnterHandler
{
    [Header("Info")]
    public int mySeq;
    public string myName;
    public string mySize;
    public string myCode;
    public DateTime createDate;
    public DateTime startDate;
    public DateTime endDate;
    public DateTime closeDate;
    public DateTime deleteDate;
    DateTime lastUpdateTime = DateTime.Now;
    public bool isOpen;
    public bool isDone;
    public bool isLinked;
    public bool canOpen;
    public float closeRemainTime = 72f;

    [Header("ButtonParam")]
    public TMP_Text galleryName;
    public TMP_Text createText;
    public OpenState currentstate;
    public string stateString;
    public ModalControl modalPanel;
    public ModalButton modalButton;
    public Sprite[] normalSprites;
    public Sprite[] clickedSprites;

    [Space]
    public Image stateImage;
    public Sprite[] stateSprites;
    RectTransform stateImageRect;
    GalleryFeed _feed;
    BackEndManager _backEndManager;
    FeedManager _feedManager;
    bool isHovering;
    string stateText;
    public string closeString;

    private void Awake()
    {
        currentstate = OpenState.NeedToLink;

        stateImageRect = stateImage.GetComponent<RectTransform>();
        _backEndManager = FindObjectOfType<BackEndManager>();
        _feedManager = FindObjectOfType<FeedManager>();
        _feed = FindAnyObjectByType<GalleryFeed>();
        modalPanel = FindObjectOfType<ModalControl>(true);
    }

    private void OnEnable()
    {
        GetComponent<Button>().onClick.AddListener(() => 
        { 
            _feedManager.OnClickGalleryButton(currentstate, this);
            _feed.SettingCurrentButton(this);
        });
    }

    private void Update()
    {
        if (!isOpen)
        {
            CalculateRemainTime();
        }

        if (modalButton.isHovering)
        {
            if (!isLinked)
            {
                modalPanel.SetInfoText($"갤러리 삭제 예정일 {deleteDate.ToString("MM.dd HH:mm")}\n<b>- 남음</b> {stateText}");
            }
            else if (!isDone)
            {
                DateTime currentTime = DateTime.Now;
                TimeSpan remainingTime = endDate - currentTime;

                modalPanel.SetInfoText($"<b>{remainingTime.Days}일 {remainingTime.Hours.ToString("D2")}:{remainingTime.Minutes.ToString("D2")}분 남음 {stateText}</b>");
            }
            else
            {
                modalPanel.SetInfoText($"갤러리 삭제 예정일 {deleteDate.ToString("MM.dd HH:mm")}\n<b>- 남음</b> {stateText}");
            }
        }
    }

    private void CalculateRemainTime()
    {
        DateTime currentTime = DateTime.Now;
        TimeSpan elapsedTime = currentTime - lastUpdateTime;

        closeRemainTime -= (float)elapsedTime.TotalHours;

        lastUpdateTime = currentTime;

        // 남은 시간이 0보다 작으면 0으로 설정
        if (closeRemainTime < 0)
        {
            closeRemainTime = 0f;
        }

        int days = (int)closeRemainTime / 24;
        int hours = (int)closeRemainTime % 24;
        int minutes = (int)((closeRemainTime - (int)closeRemainTime) * 60);

        closeString = $"{days}일 {hours}시간 {minutes}분";
    }

    public void Init(int seq, string name, string size, string code, string createDate, string openDate, string endDate, bool isOpen, bool isDone, bool isIns, string closeDate = "", bool isLinked = false)
    {
        SpriteState spriteState = GetComponent<Button>().spriteState;

        switch (size)
        {
            case "소형":
                {
                    GetComponent<Image>().sprite = normalSprites[0];

                    spriteState.pressedSprite = clickedSprites[0];
                    GetComponent<Button>().spriteState = spriteState;
                    break;
                }
            case "중형":
                {
                    GetComponent<Image>().sprite = normalSprites[1];

                    spriteState.pressedSprite = clickedSprites[1];
                    GetComponent<Button>().spriteState = spriteState;
                    break;
                }
            case "대형":
                {
                    GetComponent<Image>().sprite = normalSprites[2];

                    spriteState.pressedSprite = clickedSprites[2];
                    GetComponent<Button>().spriteState = spriteState;
                    break;
                }
        }


        if (!isLinked)
        {
            mySeq = seq;
            myName = name;
            mySize = size;
            myCode = code;
            this.createDate = DateTime.Parse(createDate);

            galleryName.text = $"{name} [{size}]";

            currentstate = OpenState.NeedToLink;

            stateImage.sprite = stateSprites[3];
            ChangeStateWidth(144f);
            stateText = "<color=#C4C4C4>[Need to LINK]</color>";

            createText.text = $"{this.createDate.ToString("yyyy.MM.dd HH:mm")} 생성됨";

            deleteDate = RoundUpToNextHour(this.createDate.AddDays(14));
            DateTime exchangeDate = RoundUpToNextHour(this.createDate);
            TimeSpan remainingTime = deleteDate - exchangeDate;

            modalPanel.SetGalleryData(myName, mySize, null, null, $"{remainingTime.Days}");
        }
        else
        {
            mySeq = seq;
            myName = name;
            mySize = size;
            myCode = code;
            this.createDate = DateTime.Parse(createDate);
            startDate = DateTime.Parse(openDate);
            this.endDate = RoundUpToNextHour(DateTime.Parse(endDate));
            this.isOpen = isOpen;
            this.isDone = isDone;
            this.isLinked = isLinked;

            if (startDate > DateTime.Now)
            {
                canOpen = false;
            }
            else
            {
                canOpen = true;
            }    

            galleryName.text = $"{name} [{size}]";
            createText.text = $"{this.createDate.ToString("yyyy.MM.dd HH:mm")} 생성됨";

            deleteDate = RoundUpToNextHour(this.endDate.AddDays(7));
            TimeSpan remainingTime = this.endDate - this.createDate;

            modalPanel.SetGalleryData(myName, mySize, $"{startDate.ToString("yyyy.MM.dd HH:mm")}", $"{this.endDate.ToString("yyyy.MM.dd HH:mm")}", $"{remainingTime.Days}");

            if (isDone)
                currentstate = OpenState.Done;
            else if (isOpen && !isDone && canOpen)
                currentstate = OpenState.Open;
            else
                currentstate = OpenState.Close;
            
            if (string.IsNullOrEmpty(closeDate) || DateTime.Parse(closeDate) == DateTime.MinValue)
                ChangeState(currentstate, DateTime.Now, isIns);
            else
                ChangeState(currentstate, DateTime.Parse(closeDate), isIns);
        }
    }

    DateTime RoundUpToNextHour(DateTime dateTime)
    {
        if (dateTime.Minute > 0)
        {
            dateTime = dateTime.AddHours(1);
            dateTime = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, 0, 0);
        }
        return dateTime;
    }

    public void ChangeState(OpenState state, DateTime closeDate, bool isIns)
    {
        currentstate = state;
        this.closeDate = closeDate;

        switch (state)
        {
            case OpenState.Open:    
                ChangedOpenState(isIns);
                break;
            case OpenState.Close:
                ChangedCloseState(this.closeDate);
                break;
            case OpenState.Done:
                stateImage.sprite = stateSprites[2];
                ChangeStateWidth(79f);
                stateText = "<color=#C4C4C4>[DONE]</color>";
                isDone = true;
                break;
        }
    }

    public void ChangedOpenState(bool isIns)
    {
        stateImage.sprite = stateSprites[0];
        ChangeStateWidth(79f);
        stateText = "<color=#4378ff>[OPEN]</color>";
        isOpen = true;
        if (isIns)
            return;
        BackEndManager _BackEndManager = FindObjectOfType<BackEndManager>();
        _BackEndManager.SendtouploadgalleryData(myName, mySize, "", startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"), 0, myCode, true, "", "");
    }

    public void ChangedCloseState(DateTime closeDate)
    {
        if (closeDate != DateTime.Now && canOpen)
        {
            DateTime currentTime = DateTime.Now;

            TimeSpan elapsedTime = currentTime - this.closeDate;

            closeRemainTime -= (float)elapsedTime.TotalHours;

            if (closeRemainTime <= 0f)
                ChangeState(OpenState.Done, DateTime.MinValue, false);
        }
        else
        {
            closeRemainTime = 72f;
        }        

        if (closeRemainTime > 24f)
            closeRemainTime -= 24f;

        stateImage.sprite = stateSprites[1];
        ChangeStateWidth(87f);
        stateText = "<color=#F44F4F>[CLOSE]</color>";

        isOpen = false;
    }

    public void ChangeStateWidth(float width)
    {
        if (stateImageRect == null)
            stateImageRect = stateImage.GetComponent<RectTransform>();

        stateImageRect.sizeDelta = new Vector2(width, stateImageRect.sizeDelta.y);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        switch (currentstate)
        {
            case OpenState.Open:
                modalPanel.ChangeModalRect(0, 108f);
                break;
            case OpenState.Close:
                modalPanel.ChangeModalRect(0, 108f);
                break;
            case OpenState.Done:
                modalPanel.ChangeModalRect(0, 120f);
                break;
            case OpenState.NeedToLink:
                modalPanel.ChangeModalRect(0, 120f);
                break;
        }
    }
}
