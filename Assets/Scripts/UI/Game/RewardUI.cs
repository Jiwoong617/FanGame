using System.Collections.Generic;
using System.Xml.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardUI : UI_Base
{
    enum Texts
    {
        Slot1_Name, Slot1_Desc,
        Slot2_Name, Slot2_Desc,
        Slot3_Name, Slot3_Desc
    }

    enum Images
    {
        Slot1_Icon,
        Slot2_Icon,
        Slot3_Icon
    }

    enum Buttons
    {
        Slot1_Button,
        Slot2_Button,
        Slot3_Button,
        Skip_Button
    }

    private List<TextMeshProUGUI> _names = new List<TextMeshProUGUI>();
    private List<TextMeshProUGUI> _descs = new List<TextMeshProUGUI>();
    private List<Image> _icons = new List<Image>();
    private List<Button> _buttons = new List<Button>();

    protected override void Init()
    {
        _names.Clear();
        _descs.Clear();
        _icons.Clear();
        _buttons.Clear();

        Bind<TextMeshProUGUI>(typeof(Texts));
        Bind<Image>(typeof(Images));
        Bind<Button>(typeof(Buttons));

        // 리스트 초기화 (Enum 순서에 의존)
        _names.Add(Get<TextMeshProUGUI>(Texts.Slot1_Name));
        _names.Add(Get<TextMeshProUGUI>(Texts.Slot2_Name));
        _names.Add(Get<TextMeshProUGUI>(Texts.Slot3_Name));

        _descs.Add(Get<TextMeshProUGUI>(Texts.Slot1_Desc));
        _descs.Add(Get<TextMeshProUGUI>(Texts.Slot2_Desc));
        _descs.Add(Get<TextMeshProUGUI>(Texts.Slot3_Desc));

        _icons.Add(Get<Image>(Images.Slot1_Icon));
        _icons.Add(Get<Image>(Images.Slot2_Icon));
        _icons.Add(Get<Image>(Images.Slot3_Icon));

        _buttons.Add(Get<Button>(Buttons.Slot1_Button));
        _buttons.Add(Get<Button>(Buttons.Slot2_Button));
        _buttons.Add(Get<Button>(Buttons.Slot3_Button));

        for (int i = 0; i < _buttons.Count; i++)
        {
            int index = i;
            _buttons[i].onClick.AddListener(() => OnSlotClick(index));
        }
        Get<Button>(Buttons.Skip_Button).onClick.AddListener(() => OnSlotClick(-1));

        GameManager.Reward.SetUI(this);
        gameObject.SetActive(false);
    }

    public void SetRewards(List<RewardBase> rewards)
    {
        for (int i = 0; i < _buttons.Count; i++)
        {
            if (i >= rewards.Count)
            {
                _buttons[i].gameObject.SetActive(false);
                continue;
            }

            _names[i].text = rewards[i].RewardName;
            _descs[i].text = rewards[i].Description;
            _icons[i].sprite = rewards[i].Icon;
            
            _buttons[i].gameObject.SetActive(true);
        }
    }

    private void OnSlotClick(int index)
    {
        GameManager.Reward.SelectReward(index);
    }
}