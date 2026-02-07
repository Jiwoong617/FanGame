using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EventUI : UI_Base
{
    #region enums
    enum Texts
    {
        EventName,
        EventDescription
    }

    enum Verticals
    {
        Vertical
    }

    enum Images
    {
        EventImage,
    }

    enum Buttons
    {
        NextButton,
    }
    #endregion

    
    [SerializeField] private GameObject selectBtn;

    private EventData currentEvent;
    private List<EventOptionButton> activeButtons = new List<EventOptionButton>();

    protected override void Init()
    {
        Bind<TMP_Text>(typeof(Texts));
        Bind<VerticalLayoutGroup>(typeof(Verticals));
        Bind<Image>(typeof(Images));
        Bind<Button>(typeof(Buttons));

        GameManager.Event.SetUI(this);

        Get<Button>(Buttons.NextButton).onClick.AddListener(OnNextButtonClicked);
        Get<Button>(Buttons.NextButton).gameObject.SetActive(false);

        Hide();
    }

    public void ShowEvent(EventData eventData)
    {
        Show();
        currentEvent = eventData;

        // UI 갱신
        Get<TMP_Text>(Texts.EventName).text = eventData.title;
        Get<TMP_Text>(Texts.EventDescription).text = eventData.description;
        Get<Image>(Images.EventImage).sprite = eventData.eventImage;

        Transform parent = Get<VerticalLayoutGroup>(Verticals.Vertical).transform;
        for (int i = 0; i < eventData.options.Count; i++)
        {
            EventOptionButton btn;

            if (i < activeButtons.Count)
            {
                btn = activeButtons[i];
                btn.gameObject.SetActive(true);
            }
            else
            {
                GameObject go = Instantiate(selectBtn, parent);
                go.SetActive(true);
                btn = go.GetComponent<EventOptionButton>();
                activeButtons.Add(btn);
            }

            var option = eventData.options[i];
            btn.Init(option.buttonText, () => OnOptionSelected(option));
            
            btn.transform.SetAsLastSibling();
        }

        for (int i = eventData.options.Count; i < activeButtons.Count; i++)
        {
            activeButtons[i].gameObject.SetActive(false);
        }

        Get<VerticalLayoutGroup>(Verticals.Vertical).gameObject.SetActive(true);
        Get<Button>(Buttons.NextButton).gameObject.SetActive(false);
    }

    private void OnOptionSelected(EventOption option)
    {
        Get<VerticalLayoutGroup>(Verticals.Vertical).gameObject.SetActive(false);
        Get<TMP_Text>(Texts.EventDescription).text = option.resultText;

        foreach(var op in option.outcomes)
            op.Apply(GameManager.Instance.Player);

        Get<Button>(Buttons.NextButton).gameObject.SetActive(true);
    }

    private void OnNextButtonClicked()
    {
        Hide();
        GameManager.Event.CompleteEvent();
    }
}