using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using TMPro;
using Unity.VisualScripting;
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
        AttackInterval,
        Stamina,
        Fp
    }

    private enum Images
    {
        CharacterList,
        CharacterSprite
    }
    #endregion

    [Header("Data")]
    [SerializeField] private List<UnitData> characterDatas;
    [SerializeField] private List<CombatResourceData> combatDatas;

    [SerializeField] private GameObject slotPrefab;

    private Transform slotContainer;

    private void Start()
    {
        InitializeSlots();
        Hide();
    }

    protected override void Init()
    {
        Bind<Button>(typeof(Buttons));
        Bind<TMP_Text>(typeof(Texts));
        Bind<Image>(typeof(Images));

        Get<Button>(Buttons.StartButton).onClick.AddListener(OnClickStart);
        Get<Button>(Buttons.BackButton).onClick.AddListener(OnClickBack);

        slotContainer = Get<Image>(Images.CharacterList).transform;
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
    }

    private void OnClickBack()
    {
        Hide();
    }

    private void OnClickCharacterButton(int idx)
    {
        GameManager.Instance.SetPlayerData(characterDatas[idx], combatDatas[idx]);

        Get<Image>(Images.CharacterSprite).sprite = characterDatas[idx].unitSprite;

        Get<TMP_Text>(Texts.Name).text = characterDatas[idx].unitName;
        Get<TMP_Text>(Texts.Description).text = characterDatas[idx].unitDescription;
        Get<TMP_Text>(Texts.Hp).text = characterDatas[idx].hp.ToString();
        Get<TMP_Text>(Texts.Attack).text = characterDatas[idx].attackDamage.ToString();
        Get<TMP_Text>(Texts.AttackInterval).text = characterDatas[idx].attackInterval.ToString();
        Get<TMP_Text>(Texts.Stamina).text = combatDatas[idx].stamina.ToString();
        Get<TMP_Text>(Texts.Fp).text = combatDatas[idx].fp.ToString();
    }
}
