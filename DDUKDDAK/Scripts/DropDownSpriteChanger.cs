using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DropDownSpriteChanger : MonoBehaviour
{
    [Header("Main Sprite Change")]
    public TMP_Dropdown dropDown;
    public Image dropDownImage;
    public Sprite defaultSprite;
    public Sprite expandSprite;
    public Image arrowImage;
    public Sprite downArrow;
    public Sprite upArrow;

    [Header("Option Sprite Change")]
    public Sprite defaultOptionSprite;
    public Sprite lastOptionSprite;
    public string[] options;

    bool isExpanded = false;

    void Start()
    {
        dropDownImage.sprite = defaultSprite;
        arrowImage.sprite = downArrow;

        dropDown.onValueChanged.AddListener(delegate { OnValueChanged(); });

        InitOptions();
    }

    public void InitOptions()
    {
        dropDown.options.Clear();

        for (int i = 0; i < options.Length; i++)
        {
            TMP_Dropdown.OptionData option;

            int index = i;

            if (index == options.Length - 1)
                option = new TMP_Dropdown.OptionData(options[index], lastOptionSprite);
            else
                option = new TMP_Dropdown.OptionData(options[index], defaultOptionSprite);

            dropDown.options.Add(option);
        }

        dropDown.RefreshShownValue();
    }

    private void Update()
    {
        if (isExpanded != dropDown.IsExpanded)
        {
            isExpanded = dropDown.IsExpanded;

            UpdateDropdownState();
        }
    }

    void UpdateDropdownState()
    {
        if (isExpanded)
        {
            dropDownImage.sprite = expandSprite;
            arrowImage.sprite = upArrow;
        }
        else
        {
            dropDownImage.sprite = defaultSprite;
            arrowImage.sprite = downArrow;
        }
    }

    void OnValueChanged()
    {
        isExpanded = false;
        UpdateDropdownState();
    }
}
