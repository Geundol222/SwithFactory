using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum ModalState { Create, List, IsOpen, Schedule, Delete, UserCount, NeedClose }

public class ModalControl : MonoBehaviour
{
    public RectTransform rectTransform;
    public CanvasGroup canvasGroup;
    public Image modalImage;
    public bool isHovering;
    public Vector2 modalOffset;

    [Header("GalleryData")]
    public TMP_Text nameText;
    public TMP_Text startDateText;
    public TMP_Text endDateText;
    public TMP_Text modalText;

    private void Start()
    {
        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (isHovering)
        {
            Vector2 mousePos = Input.mousePosition;
            transform.position = new Vector2(mousePos.x + modalOffset.x, mousePos.y + modalOffset.y);
        }
    }

    public void SetOffset(Vector2 offset)
    {
        modalOffset = offset;
    }

    public void SetGalleryData(string name, string size, string startDate, string endDate, string remainTime)
    {
        nameText.text = $"{name} [{size}] {remainTime}일";

        if (string.IsNullOrEmpty(startDate))
            startDateText.text = "-";
        else
            startDateText.text = $"{startDate} 시작";

        if (string.IsNullOrEmpty(endDate))
            endDateText.text = "-";
        else
            endDateText.text = $"{endDate} 종료";
    }

    public void SetInfoText(string text)
    {
        modalText.text = text;
    }

    public void ActivePanel(ModalState state)
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }

        Vector2 childSize = transform.GetChild((int)state).GetComponent<RectTransform>().sizeDelta;

        ChangeModalRect(childSize.x, childSize.y);
        transform.GetChild((int)state).gameObject.SetActive(true);
    }

    public void ChangeModalRect(float width = 0f, float height = 0f)
    {
        if(rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        if (height == 0f)
            rectTransform.sizeDelta = new Vector2(width, rectTransform.sizeDelta.y);
        else if (width == 0f)
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, height);
        else
            rectTransform.sizeDelta = new Vector2(width, height);
    }

    public void ChangeModalImage(Sprite sprite)
    {
        modalImage.sprite = sprite;
    }

    public void MouseOn(bool on)
    {
        gameObject.SetActive(on);
        isHovering = on;

        if (on)
        {
            StartCoroutine(FadeIn());
        }
        else
        {
            canvasGroup.alpha = 0f;
        }
    }

    private IEnumerator FadeIn()
    {
        float duration = 0.2f; // 디졸브 효과의 시간 (0.5초로 설정)
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsedTime / duration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }
}
