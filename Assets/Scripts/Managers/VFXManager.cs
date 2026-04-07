using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum AttackVFXType
{
    Slash,
    Smash,
    Magic,

    Hasiyo,
    Mone,
    Rose,
    Popo,
    Ryusiho
}

public class VFXManager
{
    //DamageText 관련
    private GameObject damageTextPrefab;
    private Queue<DamageText> textPool = new Queue<DamageText>();
    private List<DamageText> activeTexts = new List<DamageText>();
    private float pushHeight = 0.3f;

    //Effect 관련
    private GameObject effectPrefab;
    private Queue<SimpleEffect> effectPool = new Queue<SimpleEffect>();
    private Dictionary<AttackVFXType, Sprite> commonSprites = new Dictionary<AttackVFXType, Sprite>();

    public void Init()
    {
        damageTextPrefab = Resources.Load<GameObject>("DamageText");

        CreateEffectPrefab();

        LoadCommonSprite(AttackVFXType.Slash, "Sprites/VFX/Slash");
        LoadCommonSprite(AttackVFXType.Smash, "Sprites/VFX/Smash");
        LoadCommonSprite(AttackVFXType.Magic, "Sprites/VFX/Magic");
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

    public void ClearPools()
    {
        effectPool.Clear();
        textPool.Clear();
        activeTexts.Clear();
    }
    #endregion

    private void LoadCommonSprite(AttackVFXType type, string path)
    {
        Sprite s = Resources.Load<Sprite>(path);
        if (s != null)
            commonSprites[type] = s;
    }

    private void CreateEffectPrefab()
    {
        effectPrefab = new GameObject("VFXPrefab");
        effectPrefab.AddComponent<SpriteRenderer>();
        effectPrefab.AddComponent<SimpleEffect>();
        effectPrefab.SetActive(false);
        UnityEngine.Object.DontDestroyOnLoad(effectPrefab);
    }

    private SimpleEffect GetEffect()
    {
        if (effectPool.Count > 0)
        {
            SimpleEffect eff = effectPool.Dequeue();
            if (eff != null && eff.gameObject != null)
            {
                eff.gameObject.SetActive(true);
                return eff;
            }
        }

        // 풀이 비었으면 새로 생성
        GameObject go = UnityEngine.Object.Instantiate(effectPrefab);
        go.SetActive(true);
        return go.GetComponent<SimpleEffect>();
    }

    public void ReturnEffect(SimpleEffect effect)
    {
        effect.gameObject.SetActive(false);
        effectPool.Enqueue(effect);
    }

    public void ShowGenericEffect(Vector3 pos, AttackVFXType type, float hitDelay, Color color)
    {
        if (!commonSprites.TryGetValue(type, out Sprite sprite)) return;

        SimpleEffect effect = GetEffect();

        effect.Play(pos, sprite, (t, sr, onComplete) =>
        {
            sr.color = new Color(color.r, color.g, color.b, 0f);
            Sequence seq = DOTween.Sequence();

            if (hitDelay > 0)
                seq.AppendInterval(hitDelay);
            seq.AppendCallback(() => sr.color = color);

            switch (type)
            {
                case AttackVFXType.Slash:
                    float angle = UnityEngine.Random.Range(0, 360f);
                    t.rotation = Quaternion.Euler(0, 0, angle);

                    seq.Append(sr.DOFade(0f, 0.25f).SetEase(Ease.InQuad));
                    break;

                case AttackVFXType.Smash:
                    t.localScale = Vector3.one * 0.5f;

                    seq.Append(t.DOScale(1.1f, 0.15f).SetEase(Ease.OutBack));
                    seq.Join(sr.DOFade(0f, 0.2f).SetEase(Ease.InQuad));
                    break;

                default:
                    seq.Append(t.DOScale(1.1f, 0.3f));
                    seq.Join(sr.DOFade(0f, 0.3f));
                    break;
            }
            seq.OnComplete(() => onComplete());

        }, color);
    }

    public void ShowCustomEffect(Vector3 pos, Sprite sprite, Action<Transform, SpriteRenderer, Action> customAnim)
    {
        if (sprite == null) return;
        SimpleEffect effect = GetEffect();

        effect.Play(pos, sprite, (t, sr, onComplete) =>
        {
            customAnim(t, sr, onComplete);
        }, Color.white);
    }

    public void PlayerAttackEffect(Vector3 attackerPos, Vector3 targetPos, AttackVFXType type, Sprite sprite, float hitDelay)
    {
        switch(type)
        {
            case AttackVFXType.Hasiyo:
                HasiyoEffect(attackerPos, targetPos, sprite, hitDelay);
                break;
            case AttackVFXType.Mone:
                MoneEffect(attackerPos, targetPos, sprite, hitDelay);
                break;
            case AttackVFXType.Popo:
                PopoEffect(attackerPos, targetPos, sprite, hitDelay);
                break;
            case AttackVFXType.Rose:
                RoseEffect(attackerPos, targetPos, sprite, hitDelay);
                break;
            case AttackVFXType.Ryusiho:
                RyusihoEffect(attackerPos, targetPos, sprite, hitDelay);
                break;
        }
    }

    private void HasiyoEffect(Vector3 attackerPos, Vector3 targetPos, Sprite sprite, float hitDelay)
    {
        if (sprite == null) return;

        hitDelay *= 2;

        SimpleEffect effect = GetEffect();
        effect.Play(targetPos, sprite, (t, sr, onComplete) =>
        {
            Vector3 dir = targetPos - attackerPos;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            t.position = targetPos + new Vector3(0.5f, 0.5f);
            t.rotation = Quaternion.Euler(0, 0, angle - 225f);

            Sequence seq = DOTween.Sequence();
            if (hitDelay > 0)
                seq.Append(t.DORotate(new Vector3(0, 0, angle - 90f), hitDelay).SetEase(Ease.InExpo));
            else
                t.rotation = Quaternion.Euler(0, 0, angle - 90f);

            seq.Append(sr.DOFade(0f, 0.5f));
            seq.OnComplete(() => onComplete());
        }, Color.white);
    }

    private void MoneEffect(Vector3 attackerPos, Vector3 targetPos, Sprite sprite, float hitDelay)
    {
        if (sprite == null) return;

        SimpleEffect effect = GetEffect();
        effect.Play(targetPos, sprite, (t, sr, onComplete) =>
        {
            t.position = targetPos;
            t.localScale = new Vector3(1.5f, 1.5f, 1f);
            sr.color = new Color(1, 1, 1, 0.6f);
            t.rotation = Quaternion.Euler(0, 0, UnityEngine.Random.Range(0f, 360f));


            Sequence seq = DOTween.Sequence();

            seq.Append(t.DOScale(new Vector3(0.8f, 0.8f, 1f), 0.1f).SetEase(Ease.OutQuad));
            seq.Join(sr.DOFade(1f, 0.1f).SetEase(Ease.OutQuad));
            seq.Join(t.DORotate(new Vector3(0, 0, t.rotation.eulerAngles.z + 20f), 0.1f, RotateMode.FastBeyond360).SetEase(Ease.OutQuad));

            seq.AppendInterval(0.05f);
            seq.Join(sr.DOFade(0f, 0.05f).SetEase(Ease.InQuad));

            seq.OnComplete(() => onComplete());
        }, Color.white);
    }

    private void RoseEffect(Vector3 attackerPos, Vector3 targetPos, Sprite sprite, float hitDelay)
    {
        if (sprite == null) return;

        SimpleEffect effect = GetEffect();
        Vector3 randPos = new Vector3(UnityEngine.Random.Range(-0.2f, 0.2f), UnityEngine.Random.Range(-0.2f, 0.2f), 0);
        effect.Play(targetPos + randPos, sprite, (t, sr, onComplete) =>
        {
            Vector3 dir = targetPos - attackerPos;
            float angle = UnityEngine.Random.Range(0, 360f);
            t.rotation = Quaternion.Euler(0, 0, angle);

            t.position = targetPos - (dir.normalized * 0.3f);
            sr.color = new Color(1, 1, 1, 0);
            t.localScale = new Vector3(0.2f, 0.1f, 1f);

            Sequence seq = DOTween.Sequence();
            if (hitDelay > 0)
                seq.AppendInterval(hitDelay);

            seq.AppendCallback(() => sr.color = Color.white);
            seq.Append(t.DOScale(new Vector3(1.2f, 0.8f, 1f), 0.05f).SetEase(Ease.OutBack));

            seq.Append(t.DOScale(new Vector3(0.1f, 0.1f, 1f), 0.05f).SetEase(Ease.InQuad));
            seq.Join(t.DOMove(targetPos - (dir.normalized * 0.2f), 0.05f).SetEase(Ease.InQuad));
            seq.Join(sr.DOFade(0f, 0.05f).SetEase(Ease.InQuad));
            seq.OnComplete(() => onComplete());
        }, Color.orange);
    }

    private void PopoEffect(Vector3 attackerPos, Vector3 targetPos, Sprite sprite, float hitDelay)
    {
        if (sprite == null) return;

        SimpleEffect effect = GetEffect();
        effect.Play(targetPos, sprite, (t, sr, onComplete) =>
        {
            sr.color = new Color(1f, 1f, 1f, 0f);

            Sequence seq = DOTween.Sequence();
            if (hitDelay > 0)
                seq.AppendInterval(hitDelay);
            seq.AppendCallback(() => sr.color = Color.white);

            t.localScale = Vector3.one * 0.5f;
            float angle = UnityEngine.Random.Range(0, 360f);
            t.rotation = Quaternion.Euler(0, 0, angle);

            seq.Append(t.DOScale(1.1f, 0.15f).SetEase(Ease.OutBack));
            seq.Join(sr.DOFade(0f, 0.2f).SetEase(Ease.InQuad));
            seq.OnComplete(() => onComplete());
        }, Color.white);
    }

    private void RyusihoEffect(Vector3 attackerPos, Vector3 targetPos, Sprite sprite, float hitDelay)
    {
        if (sprite == null) return;

        SimpleEffect effect = GetEffect();
        effect.Play(targetPos, sprite, (t, sr, onComplete) =>
        {
            sr.color = new Color(1f, 1f, 1f, 0f);

            Vector3 dir = targetPos - attackerPos;
            float baseAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            float randomOffset = UnityEngine.Random.Range(-30f, 30f);
            t.rotation = Quaternion.Euler(0, 0, baseAngle + randomOffset);

            t.localScale = new Vector3(0.5f, 0.1f, 1f);
            Sequence seq = DOTween.Sequence();
            if (hitDelay > 0)
                seq.AppendInterval(hitDelay);
            seq.AppendCallback(() => sr.color = Color.white);

            seq.Append(t.DOScale(new Vector3(1.5f, 0.5f, 1f), 0.15f).SetEase(Ease.OutExpo));

            seq.Join(sr.DOFade(0f, 0.2f).SetEase(Ease.InQuad));

            seq.OnComplete(() => onComplete());
        }, Color.white);
    }
}
