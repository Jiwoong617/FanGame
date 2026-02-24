using System.Collections.Generic;
using UnityEngine;

public class VFXManager
{
    private GameObject damageTextPrefab;
    private Queue<DamageText> textPool = new Queue<DamageText>();
    private List<DamageText> activeTexts = new List<DamageText>();

    private float pushHeight = 0.3f;

    public void Init()
    {
        damageTextPrefab = Resources.Load<GameObject>("DamageText");
    }

    public void ShowDamageText(CombatEventContext ctx)
    {
        if (ctx.target == null) return;
        SpawnText(ctx.target.transform, ctx.value, ctx.isCritical, false, ctx.damageType == DamageType.Fixed);
    }

    public void ShowHealText(Transform target, float amount)
    {
        if (target == null) return;
        SpawnText(target, amount, false, true, false);
    }

    private void SpawnText(Transform target, float amount, bool isCrit, bool isHeal, bool isFixed)
    {
        if (damageTextPrefab == null) return;
        if (amount <= 0 && !isHeal) return;

        foreach (var activeText in activeTexts)
        {
            if (activeText.currentTarget == target)
            {
                activeText.PushUp(pushHeight);
            }
        }

        DamageText dt = (textPool.Count > 0) ? textPool.Dequeue() : Object.Instantiate(damageTextPrefab).GetComponent<DamageText>();
        dt.gameObject.SetActive(true);

        activeTexts.Add(dt);
        dt.Setup(target, amount, isCrit, isHeal, isFixed);
    }

    public void ReturnToPool(DamageText dt)
    {
        dt.gameObject.SetActive(false);
        activeTexts.Remove(dt);
        textPool.Enqueue(dt);
    }
}
