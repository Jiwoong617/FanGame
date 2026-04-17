using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectUI : UI_Base
{

    #region Enum
    private enum Buttons
    {
        StartButton,
        BackButton
    }

    private enum Texts
    {
        Name,
        Description,
        Hp,
        Attack,
        AttackSpeed,
        Stamina,
        Fp,
        CriticalChance,
        SkillName,
        SkillDescription
    }

    private enum Images
    {
        CharacterList,
        CharacterSprite,
        SkillIcon
    }
    #endregion

    [Header("Data")]
    [SerializeField] private List<PlayerData> characterDatas;
    [SerializeField] private GameObject slotPrefab;

    private Transform slotContainer;
    private RectTransform rectTransform;
    private float screenWidth;

    protected override void Init()
    {
        rectTransform = GetComponent<RectTransform>();
        screenWidth = rectTransform.rect.width;

        Bind<Button>(typeof(Buttons));
        Bind<TMP_Text>(typeof(Texts));
        Bind<Image>(typeof(Images));

        Get<Button>(Buttons.StartButton).onClick.AddListener(OnClickStart);
        Get<Button>(Buttons.BackButton).onClick.AddListener(OnClickBack);

        slotContainer = Get<Image>(Images.CharacterList).transform;

        InitializeSlots();

        base.Hide();
    }

    private void InitializeSlots()
    {
        foreach (Transform child in slotContainer)
            Destroy(child.gameObject);

        if (slotPrefab == null)
            return;

        for(int i = 0; i<characterDatas.Count; i++)
        {
            int index = i;
            GameObject go = Instantiate(slotPrefab, slotContainer);
            go.GetComponent<Image>().sprite = characterDatas[i].unitSprite;
            go.GetComponent<Button>().onClick.AddListener(() => OnClickCharacterButton(index));
        }

        OnClickCharacterButton(0);
    }

    private void OnClickStart()
    {
        Debug.Log("Start;");
        GameManager.Sound.PlaySFX(SFX.StartButton);
        GameManager.Scene.LoadScene(SceneType.Game);
    }

    private void OnClickBack()
    {
        Hide();
    }

    private void OnClickCharacterButton(int idx)
    {
        GameManager.Instance.SetPlayerData(characterDatas[idx]);

        Get<Image>(Images.CharacterSprite).sprite = characterDatas[idx].unitSprite;

        //캐릭터 인포
        Get<TMP_Text>(Texts.Name).text = characterDatas[idx].unitName;
        Get<TMP_Text>(Texts.Description).text = characterDatas[idx].unitDescription;
        Get<TMP_Text>(Texts.Hp).text = characterDatas[idx].hp.ToString();
        Get<TMP_Text>(Texts.Attack).text = characterDatas[idx].attackDamage.ToString();
        Get<TMP_Text>(Texts.AttackSpeed).text = characterDatas[idx].attackSpeed.ToString();
        Get<TMP_Text>(Texts.Stamina).text = characterDatas[idx].stamina.ToString();
        Get<TMP_Text>(Texts.Fp).text = characterDatas[idx].fp.ToString();
        Get<TMP_Text>(Texts.CriticalChance).text = characterDatas[idx].criticalChance.ToString();

        //스킬 정보
        Get<TMP_Text>(Texts.SkillName).text = characterDatas[idx].skillName;
        Get<TMP_Text>(Texts.SkillDescription).text = characterDatas[idx].skillDesc;
        Get<Image>(Images.SkillIcon).sprite = characterDatas[idx].skillIcon;
    }

    public override void Show()
    {
        base.Show();

        rectTransform.anchoredPosition = new Vector2(-screenWidth, 0);

        rectTransform.DOKill();
        rectTransform.DOAnchorPosX(0, 1f).SetEase(Ease.OutBounce);
    }

    public override void Hide()
    {
        rectTransform.DOKill();

        rectTransform.DOAnchorPosX(-screenWidth, 0.5f)
            .SetEase(Ease.InQuad)
            .OnComplete(() =>
            {
                base.Hide(); // 애니메이션 끝나면 gameObject.SetActive(false)
            });
    }
}
