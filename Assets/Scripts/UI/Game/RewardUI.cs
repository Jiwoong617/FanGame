using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardUI : UI_Base
{
    enum Texts
    {
        Slot1_Name, Slot1_Desc, Slot1_Flavor,
        Slot2_Name, Slot2_Desc, Slot2_Flavor,
        Slot3_Name, Slot3_Desc, Slot3_Flavor,
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
    private List<TMP_Text> flavors = new List<TMP_Text>();
    private List<Image> icons = new List<Image>();
    private List<Button> buttons = new List<Button>();

    private RectTransform contentRect;
    private CanvasGroup contentGroup;
    private Vector2 originalPos;

    private bool isClicked = false;

    protected override void Init()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null && canvas.worldCamera == null)
            canvas.worldCamera = Camera.main;

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

        flavors.Add(Get<TMP_Text>(Texts.Slot1_Flavor));
        flavors.Add(Get<TMP_Text>(Texts.Slot2_Flavor));
        flavors.Add(Get<TMP_Text>(Texts.Slot3_Flavor));

        icons.Add(Get<Image>(Images.Slot1_Icon));
        icons.Add(Get<Image>(Images.Slot2_Icon));
        icons.Add(Get<Image>(Images.Slot3_Icon));

        buttons.Add(Get<Button>(Buttons.Slot1_Button));
        buttons.Add(Get<Button>(Buttons.Slot2_Button));
        buttons.Add(Get<Button>(Buttons.Slot3_Button));

        if (buttons.Count > 0)
        {
            contentRect = buttons[0].transform.parent.GetComponent<RectTransform>();
            originalPos = contentRect.anchoredPosition;
            contentGroup = contentRect.GetComponent<CanvasGroup>();
        }

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
            flavors[i].text = rewards[i].FlavorText;
            icons[i].sprite = rewards[i].Icon;
            
            buttons[i].gameObject.SetActive(true);
        }

        if (contentRect != null)
            contentRect.anchoredPosition = originalPos + new Vector2(0, -500f);
        if (contentGroup != null)
            contentGroup.alpha = 0f;
    }

    private void OnSlotClick(int index)
    {
        if (isClicked) return;
        isClicked = true;

        Sequence seq = DOTween.Sequence();

        for (int i = 0; i < buttons.Count; i++)
        {
            if (!buttons[i].gameObject.activeSelf) continue;

            RectTransform rt = buttons[i].GetComponent<RectTransform>();
            CanvasGroup cg = buttons[i].GetComponent<CanvasGroup>();

            if (i == index)
            {
                seq.Join(rt.DOScale(1.15f, 0.4f).SetEase(Ease.OutBack));
                seq.Join(cg.DOFade(1f, 0.2f));
            }
            else
            {
                seq.Join(cg.DOFade(0f, 0.3f));
                seq.Join(rt.DOScale(0.8f, 0.3f));
            }
        }

        var skipBtn = Get<Button>(Buttons.Skip_Button);
        if (skipBtn != null)
        {
            if (index == -1)
            {
                seq.Join(skipBtn.transform.DOScale(1.1f, 0.2f).SetEase(Ease.OutBack));
            }

            CanvasGroup skipCg = skipBtn.GetComponent<CanvasGroup>();
            if (skipCg == null) skipCg = skipBtn.gameObject.AddComponent<CanvasGroup>();
            seq.Join(skipCg.DOFade(0f, 0.3f));
        }

        seq.AppendInterval(0.5f);
        seq.OnComplete(() => {
            GameManager.Reward.SelectReward(index);
        });
    }

    public override void Show()
    {
        base.Show();
        isClicked = false;
        
        ResetButtonsState();
        PlayShowAnimation();
    }

    private void PlayShowAnimation()
    {
        if (contentRect == null) return;

        contentRect.DOKill();
        if (contentGroup != null) contentGroup.DOKill();

        contentRect.anchoredPosition = originalPos + new Vector2(0, -300f);
        if (contentGroup != null) contentGroup.alpha = 0f;
        Sequence seq = DOTween.Sequence();
        seq.Append(contentRect.DOAnchorPos(originalPos, 0.5f).SetEase(Ease.OutBack));
        seq.Join(contentGroup.DOFade(1f, 0.5f));
    }

    private void ResetButtonsState()
    {
        foreach (var btn in buttons)
        {
            if (btn != null)
            {
                btn.transform.localScale = Vector3.one;
                CanvasGroup cg = btn.GetComponent<CanvasGroup>();
                if (cg != null) cg.alpha = 1f;
            }
        }

        var skipBtn = Get<Button>(Buttons.Skip_Button);
        if (skipBtn != null)
        {
            skipBtn.transform.localScale = Vector3.one;
            CanvasGroup cg = skipBtn.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = 1f;
        }
    }
}