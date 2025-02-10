using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum ButtonState { Default, Hover, Click, Disable }

public class CreateLinkCalendar : MonoBehaviour
{
    public Sprite[] buttonStateSprite;

    public TMP_Dropdown yearDropDown;
    public TMP_Dropdown monthDropDown;

    public TMP_Text startDay;
    public TMP_Text endDay;
    public TMP_Text totalDay;
    public string startDateTime;
    public string endDateTime;
    public GridLayoutGroup calendarGrid;
    public GameObject dayButtonPrefab;
    public Button confirmButton;
    public Color saturdayColor;
    public Color sundayColor;
    public Color defaultColor;

    private List<GameObject> dayButtons = new List<GameObject>();
    private int startDate = -1;
    private int endDate = -1;
    public FeedManager _feed;

    private void Start()
    {
        DateTime today = DateTime.Now;

        List<TMP_Dropdown.OptionData> yearOptions = new List<TMP_Dropdown.OptionData>();
        List<TMP_Dropdown.OptionData> monthOptions = new List<TMP_Dropdown.OptionData>();

        for (int i = 0; i < 3; i++)
        {
            int index = i;

            if (index == 0)
            {
                yearOptions.Add(new TMP_Dropdown.OptionData($"{today.Year}년"));
                monthOptions.Add(new TMP_Dropdown.OptionData($"{today.Month}월"));
            }
            else
            {
                yearOptions.Add(new TMP_Dropdown.OptionData($"{today.AddYears(index).Year}년"));

                monthOptions.Add(new TMP_Dropdown.OptionData($"{today.AddMonths(index).Month}월"));
            }
        }

        yearDropDown.options = yearOptions;
        monthDropDown.options = monthOptions;

        for (int i = 0; i < calendarGrid.transform.childCount; i++)
        {
            dayButtons.Add(calendarGrid.transform.GetChild(i).gameObject);
        }

        UpdateCalendar();
    }

    public void UpdateCalendar()
    {
        int year = int.Parse(DateTime.Now.Year.ToString());
        int month = int.Parse(DateTime.Now.Month.ToString());

        DateTime firstDay = new DateTime(year, month, 1);
        int daysInMonth = DateTime.DaysInMonth(year, month);
        int startDayOfWeek = (int)firstDay.DayOfWeek;

        int previousMonth = month == 1 ? 12 : month - 1;
        int previousMonthYear = month == 1 ? year - 1 : year;
        int daysInPreviousMonth = DateTime.DaysInMonth(previousMonthYear, previousMonth);
        
        int buttonIndex = 0;

        for (int i = startDayOfWeek - 1; i >= 0; i--)
        {
            Color textColor = defaultColor;

            DateTime currentDate = new DateTime(previousMonthYear, previousMonth, daysInPreviousMonth - i);
            if (currentDate.DayOfWeek == DayOfWeek.Saturday)
            {
                textColor = saturdayColor;
            }
            else if (currentDate.DayOfWeek == DayOfWeek.Sunday)
            {
                textColor = sundayColor;
            }

            SetDayButton(dayButtons[buttonIndex], (daysInPreviousMonth - i), textColor, false, currentDate);
            buttonIndex++;
        }

        for (int day = 1; day <= daysInMonth; day++)
        {
            DateTime currentDate = new DateTime(year, month, day);
            Color textColor = defaultColor;
            bool interactable = true;

            if (currentDate < DateTime.Now.Date || currentDate > DateTime.Now.AddDays(14).Date)
            {
                interactable = false;
            }

            if (currentDate.DayOfWeek == DayOfWeek.Saturday)
            {
                textColor = saturdayColor;
            }
            else if (currentDate.DayOfWeek == DayOfWeek.Sunday)
            {
                textColor = sundayColor;
            }

            SetDayButton(dayButtons[buttonIndex], day, textColor, interactable, currentDate);
            buttonIndex++;
        }

        int nextMonthDaysToShow = 42 - buttonIndex;

        int nextMonth = month == 12 ? 1 : month + 1;
        int nextMonthYear = month == 12 ? year + 1 : year;
        int daysInNextMonth = DateTime.DaysInMonth(nextMonthYear, nextMonth);

        for (int i = 1; i <= nextMonthDaysToShow; i++)
        {
            DateTime currentDate = new DateTime(nextMonthYear, nextMonth, i);
            Color textColor = defaultColor;

            if (currentDate.DayOfWeek == DayOfWeek.Saturday)
            {
                textColor = saturdayColor;
            }
            else if (currentDate.DayOfWeek == DayOfWeek.Sunday)
            {
                textColor = sundayColor;
            }

            SetDayButton(dayButtons[buttonIndex], i, textColor, false, currentDate);
            buttonIndex++;
        }
    }

    public void SetDayButton(GameObject button, int day, Color textColor, bool interactable, DateTime date)
    {
        TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
        buttonText.text = day.ToString();
        buttonText.color = textColor;
        button.GetComponent<Button>().interactable = interactable;
        //button.GetComponent<CalendarButton>().myDate = date.ToString();
        if (button.GetComponent<Button>().interactable)
        {
            int currentDay = day;
            button.GetComponent<Button>().onClick.AddListener(() => OnDayButtonClicked(currentDay, date));
        }
        else
        {
            button.transform.GetChild(0).GetComponent<Image>().sprite = buttonStateSprite[(int)ButtonState.Disable];
        }
    }

    public void OnDayButtonClicked(int day, DateTime date)
    {
        if (startDate == -1 || (startDate != -1 && endDate != -1))
        {
            startDate = day;
            startDateTime = date.ToString("yyyy-MM-dd");
            endDate = -1;
            endDateTime = "";
        }
        else if (startDate != -1 && endDate == -1)
        {
            if (day >= startDate)
            {
                endDate = day;
                endDateTime = date.ToString("yyyy-MM-dd");
            }
            else
            {
                endDate = startDate;
                endDateTime = startDateTime;
                startDate = day;
                startDateTime = date.ToString("yyyy-MM-dd");
            }
        }

        UpdateSelectedDates(date);
    }

    void UpdateSelectedDates(DateTime date)
    {
        foreach (GameObject button in dayButtons)
        {
            if (!button.GetComponent<Button>().interactable)
                continue;

            Image Image = button.transform.GetChild(0).GetComponent<Image>();

            int day;
            if (int.TryParse(button.GetComponentInChildren<TMP_Text>().text, out day))
            {
                if (day == startDate || day == endDate)
                {
                    Image.sprite = buttonStateSprite[(int)ButtonState.Click];
                    Image.rectTransform.sizeDelta = new Vector2(38, 37);
                }
                else if (day > startDate && day < endDate)
                {
                    Image.sprite = buttonStateSprite[(int)ButtonState.Hover];
                    Image.rectTransform.sizeDelta = new Vector2(49, 35);
                }
                else
                {
                    Image.sprite = buttonStateSprite[(int)ButtonState.Default];
                    Image.rectTransform.sizeDelta = new Vector2(38, 37);
                }
            }
        }

        if (startDate != -1)
        {
            startDay.text = $"{startDate}";            
        }

        if (endDate != -1)
        {
            endDay.text = $"{endDate}";
            totalDay.text = $"{(endDate - startDate + 1)}";            
            confirmButton.interactable = true;
        }
        else
        {
            endDay.text = "";
            totalDay.text = "";
            confirmButton.interactable = false;
        }
    }

    public void CompleteButtonClick()
    {
        _feed.SetLinkDay(startDateTime , endDateTime , totalDay.text);
    }
}
