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

    private List<TMP_Text> names = new List<TMP_Text>();
    private List<TMP_Text> descs = new List<TMP_Text>();
    private List<Image> icons = new List<Image>();
    private List<Button> buttons = new List<Button>();

    protected override void Init()
    {
        names.Clear();
        descs.Clear();
        icons.Clear();
        buttons.Clear();

        Bind<TMP_Text>(typeof(Texts));
        Bind<Image>(typeof(Images));
        Bind<Button>(typeof(Buttons));

        names.Add(Get<TMP_Text>(Texts.Slot1_Name));
        names.Add(Get<TMP_Text>(Texts.Slot2_Name));
        names.Add(Get<TMP_Text>(Texts.Slot3_Name));

        descs.Add(Get<TMP_Text>(Texts.Slot1_Desc));
        descs.Add(Get<TMP_Text>(Texts.Slot2_Desc));
        descs.Add(Get<TMP_Text>(Texts.Slot3_Desc));

        icons.Add(Get<Image>(Images.Slot1_Icon));
        icons.Add(Get<Image>(Images.Slot2_Icon));
        icons.Add(Get<Image>(Images.Slot3_Icon));

        buttons.Add(Get<Button>(Buttons.Slot1_Button));
        buttons.Add(Get<Button>(Buttons.Slot2_Button));
        buttons.Add(Get<Button>(Buttons.Slot3_Button));

        for (int i = 0; i < buttons.Count; i++)
        {
            int index = i;
            buttons[i].onClick.AddListener(() => OnSlotClick(index));
        }
        Get<Button>(Buttons.Skip_Button).onClick.AddListener(() => OnSlotClick(-1));

        GameManager.Reward.SetUI(this);
        Hide();
    }

    public void SetRewards(List<RewardData> rewards)
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            if (i >= rewards.Count)
            {
                buttons[i].gameObject.SetActive(false);
                continue;
            }

            names[i].text = rewards[i].RewardName;
            descs[i].text = rewards[i].Description;
            icons[i].sprite = rewards[i].Icon;
            
            buttons[i].gameObject.SetActive(true);
        }
    }

    private void OnSlotClick(int index)
    {
        GameManager.Reward.SelectReward(index);
    }
}