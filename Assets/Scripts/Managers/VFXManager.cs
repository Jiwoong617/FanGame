using DG.Tweening;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum AttackVFXType
{
    //가장 평범한 공격 이펙트
    Slash = 0,
    Smash = 1,
    Magic = 2,

    //플레이어 공격 이펙트
    Hasiyo = 11,
    Mone = 12,
    Rose= 13,
    Popo = 14,
    Ryusiho = 15,

    //플레이어 스킬 이펙트
    HasiyoPlant = 16,
    HasiyoIce = 17,
    HasiyoMeteo = 18,
    RoseSkill = 19,

    //버프 관련 이펙트
    Buff = 30,
    Debuff = 31,
    Taunt = 32,
    Heal = 33,
    IronFortress = 34,
    Disarm = 35,
    Shackle = 36,
    Reflect = 37,

    //어빌리티 이펙트
    FireBaba = 50,
    BabaExplode = 51,
}

public class VFXManager
{
    //DamageText 관련
    private GameObject damageTextPrefab;
    private Queue<DamageText> textPool = new Queue<DamageText>();
    private List<DamageText> activeTexts = new List<DamageText>();
    private float pushHeight = 0.3f;

    //Effect 관련
    private Dictionary<AttackVFXType, Queue<BaseVFX>> effectPool = new Dictionary<AttackVFXType, Queue<BaseVFX>>();


    public void Init()
    {
        damageTextPrefab = Resources.Load<GameObject>("DamageText");
    }

    #region DamageText
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

    public void ShowText(Transform target, string message, Color color, float scaleMultiplier = 1.5f)
    {
        if (target == null) return;
        SpawnText(target, message, color, scaleMultiplier);
    }

    private void SpawnText(Transform target, float amount, bool isCrit, bool isHeal, bool isFixed)
    {
        if (damageTextPrefab == null) return;
        if (amount <= 0 && !isHeal) return;

        activeTexts.RemoveAll(t => t == null || t.gameObject == null);

        foreach (var activeText in activeTexts)
        {
            if (activeText.currentTarget == target)
            {
                activeText.PushUp(pushHeight);
            }
        }

        DamageText dt = (textPool.Count > 0) ? textPool.Dequeue() : UnityEngine.Object.Instantiate(damageTextPrefab).GetComponent<DamageText>();
        dt.gameObject.SetActive(true);

        activeTexts.Add(dt);
        dt.Setup(target, amount, isCrit, isHeal, isFixed);
    }

    private void SpawnText(Transform target, string message, Color color, float scaleMultiplier)
    {
        if (damageTextPrefab == null) return;
        if (string.IsNullOrEmpty(message)) return;

        activeTexts.RemoveAll(t => t == null || t.gameObject == null);

        foreach (var activeText in activeTexts)
        {
            if (activeText.currentTarget == target)
            {
                activeText.PushUp(pushHeight);
            }
        }

        DamageText dt = (textPool.Count > 0) ? textPool.Dequeue() : UnityEngine.Object.Instantiate(damageTextPrefab).GetComponent<DamageText>();
        dt.gameObject.SetActive(true);

        activeTexts.Add(dt);
        dt.Setup(target, message, color, scaleMultiplier);
    }

    public void ReturnToPool(DamageText dt)
    {
        dt.gameObject.SetActive(false);
        activeTexts.Remove(dt);
        textPool.Enqueue(dt);
    }
    #endregion

    #region VFX

    private BaseVFX GetEffect(AttackVFXType type)
    {
        if (effectPool.TryGetValue(type, out Queue<BaseVFX> pool) && pool.Count > 0)
        {
            BaseVFX eff = pool.Dequeue();
            eff.gameObject.SetActive(true);
            return eff;
        }

        // 풀에 없으면 Resources 폴더에서 동적 로드 
        // Resources/VFXPrefabs/Hasiyo.prefab   enum이랑 이름 같게 해야됨
        string path = $"VFXPrefabs/{type.ToString()}";
        GameObject prefab = Resources.Load<GameObject>(path);

        if (prefab == null)
        {
            Debug.LogError($"[VFXManager] {path} 프리팹을 찾을 수 없습니다!");
            return null;
        }

        GameObject go = UnityEngine.Object.Instantiate(prefab);
        BaseVFX vfx = go.GetComponent<BaseVFX>();
        if (vfx == null)
        {
            Debug.LogError($"[VFXManager] {prefab.name} 프리팹에 BaseVFX를 상속받은 스크립트가 없습니다!");
            return null;
        }

        vfx.vfxType = type;
        go.SetActive(true);
        return vfx;
    }

    public void ReturnEffect(BaseVFX effect)
    {
        effect.gameObject.SetActive(false);

        if (!effectPool.ContainsKey(effect.vfxType))
            effectPool[effect.vfxType] = new Queue<BaseVFX>();

        effectPool[effect.vfxType].Enqueue(effect);
    }

    public void PlayEffect(Vector3 attackerPos, Vector3 targetPos, AttackVFXType type, float hitDelay, Color color, Action onHit = null, Action onComplete = null)
    {
        BaseVFX effect = GetEffect(type);
        if (effect != null)
            effect.PlayEffect(attackerPos, targetPos, hitDelay, color, onHit, onComplete);
    }
    #endregion

    public void ClearPools()
    {
        effectPool.Clear();
        textPool.Clear();
        activeTexts.Clear();
    }
}
