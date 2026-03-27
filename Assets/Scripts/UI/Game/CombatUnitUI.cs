using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CombatUnitUI : UI_Base
{
    enum Images
    {
        ActionImage,
    }

    enum Sliders
    {
        HpBar,
        ActionBar
    }

    enum Transforms
    {
        BuffGrid,
    }

    [SerializeField] private GameObject buffSlotPrefab;

    private Sprite BasicAttackSprite;
    private Image ActionImage;
    private Slider HpBar;
    private Slider ActionBar;
    private Transform buffGrid;

    private CombatUnit ownerUnit;
    private Dictionary<Ability, BuffSlotUI> activeBuffs = new Dictionary<Ability, BuffSlotUI>();
    private Queue<BuffSlotUI> buffSlotPool = new Queue<BuffSlotUI>();

    protected override void Init()
    {
        Bind<Slider>(typeof(Sliders));
        Bind<Image>(typeof(Images));
        Bind<Transform>(typeof(Transforms));

        ActionImage = Get<Image>(Images.ActionImage);
        HpBar = Get<Slider>(Sliders.HpBar);
        ActionBar = Get<Slider>(Sliders.ActionBar);
        buffGrid = Get<Transform>(Transforms.BuffGrid);
        BasicAttackSprite = ActionImage.sprite;
    }

    private void OnDestroy()
    {
        if (ownerUnit != null)
        {
            var stats = ownerUnit.GetStat<UnitStats>();
            if (stats != null) stats.OnHpChanged -= UpdateHp;

            ownerUnit.OnActionBarUpdated -= UpdateActionBar;
            ownerUnit.OnBuffAdded -= AddBuffSlot;
            ownerUnit.OnBuffRemoved -= RemoveBuffSlot;
            ownerUnit.OnBuffUpdated -= UpdateBuffSlot;
        }
    }

    public void UpdateHp(float current, float max)
    {
        HpBar.value = max > 0 ? current / max : 0;
    }

    public void UpdateActionBar(float progress)
    {
        if (ActionBar == null) return;
        ActionBar.value = progress;
    }

    public void SetIntentIcon(Sprite sprite)
    {
        if (sprite != null)
            ActionImage.sprite = sprite;
        else
            ActionImage.sprite = BasicAttackSprite;
    }

    public void SetOwner(CombatUnit unit)
    {
        if (ownerUnit != null)
        {
            var oldStats = ownerUnit.GetStat<UnitStats>();
            if (oldStats != null) oldStats.OnHpChanged -= UpdateHp;

            ownerUnit.OnActionBarUpdated -= UpdateActionBar;
            ownerUnit.OnBuffAdded -= AddBuffSlot;
            ownerUnit.OnBuffRemoved -= RemoveBuffSlot;
            ownerUnit.OnBuffUpdated -= UpdateBuffSlot;
            if (ownerUnit is EnemyUnit oldEnemy)
                oldEnemy.OnPassiveAdded -= AddPassiveSlot;
        }

        ownerUnit = unit;

        var stats = unit.GetStat<UnitStats>();
        if (stats != null)
        {
            stats.OnHpChanged += UpdateHp;
            UpdateHp(stats.hp, stats.maxHp.GetValue());
        }

        unit.OnActionBarUpdated += UpdateActionBar;
        unit.OnBuffAdded += AddBuffSlot;
        unit.OnBuffRemoved += RemoveBuffSlot;
        unit.OnBuffUpdated += UpdateBuffSlot;
        if (unit is EnemyUnit newEnemy)
            newEnemy.OnPassiveAdded += AddPassiveSlot;

        ClearBuffs();
    }

    //버프 관련
    private BuffSlotUI GetOrCreateBuffSlot()
    {
        if (buffSlotPool.Count > 0)
        {
            BuffSlotUI slot = buffSlotPool.Dequeue();
            slot.gameObject.SetActive(true);
            slot.transform.SetAsLastSibling();
            return slot;
        }

        GameObject go = Instantiate(buffSlotPrefab, buffGrid);
        return go.GetComponent<BuffSlotUI>();
    }

    private void AddBuffSlot(StatusEffect effect)
    {
        if (buffSlotPrefab == null || buffGrid == null) return;

        BuffSlotUI slotUI = GetOrCreateBuffSlot();
        slotUI.Init(effect);

        activeBuffs.Add(effect, slotUI);
    }

    private void AddPassiveSlot(PassiveAbility passive)
    {
        if (buffSlotPrefab == null || buffGrid == null || passive.passiveIcon == null) return;

        BuffSlotUI slotUI = GetOrCreateBuffSlot();
        slotUI.Init(passive);

        activeBuffs.Add(passive, slotUI);
    }

    private void UpdateBuffSlot(StatusEffect effect)
    {
        if (activeBuffs.TryGetValue(effect, out BuffSlotUI slotUI))
        {
            slotUI.UpdateSlot();
        }
    }

    private void RemoveBuffSlot(StatusEffect effect)
    {
        if (activeBuffs.TryGetValue(effect, out BuffSlotUI slotUI))
        {
            slotUI.gameObject.SetActive(false);
            buffSlotPool.Enqueue(slotUI);
            activeBuffs.Remove(effect);
        }
    }


    private void ClearBuffs()
    {
        foreach (var slot in activeBuffs.Values)
        {
            if (slot != null)
            {
                slot.gameObject.SetActive(false);
                buffSlotPool.Enqueue(slot);
            }
        }
        activeBuffs.Clear();
    }
}
