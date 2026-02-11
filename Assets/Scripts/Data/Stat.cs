using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Stat
{
    protected float BaseValue;
    protected readonly List<StatModifier> modifiers = new List<StatModifier>();

    public event Action OnStatChanged;

    // 더티 플래그 최적화용 변수
    protected bool _isDirty = true;
    protected float _lastValue;

    public Stat(float baseValue)
    {
        BaseValue = baseValue;
    }

    public float GetValue()
    {
        if (_isDirty)
        {
            _lastValue = CalculateFinalValue();
            _isDirty = false;
        }
        return _lastValue;
    }

    private float CalculateFinalValue()
    {
        float finalValue = BaseValue;
        float sumFlat = 0;
        float sumPercentAdd = 0;
        float sumPercentMult = 1;

        for (int i = 0; i < modifiers.Count; i++)
        {
            StatModifier mod = modifiers[i];

            if (mod.Type == StatModType.Flat)
                sumFlat += mod.Value;
            else if (mod.Type == StatModType.PercentAdd)
                sumPercentAdd += mod.Value;
            else
                sumPercentMult *= (mod.Value < 1 ? (1-mod.Value) : mod.Value);
        }

        //스텟 계산
        finalValue += sumFlat; //깡스텟
        finalValue *= (1 + sumPercentAdd); //합연산 퍼센트
        finalValue *= sumPercentMult; //곱연산 퍼센트

        return (float)Math.Round(finalValue, 4);
    }

    public void AddModifier(StatModifier mod)
    {
        _isDirty = true;
        modifiers.Add(mod);
        OnStatChanged?.Invoke();
    }

    public void RemoveModifier(StatModifier mod)
    {
        if (modifiers.Remove(mod))
        {
            _isDirty = true;
            OnStatChanged?.Invoke();
        }
    }

    public void ClearModifiers()
    {
        if (modifiers.Count > 0)
        {
            modifiers.Clear();
            _isDirty = true;
            OnStatChanged?.Invoke();
        }
    }

    public void SetBaseValue(float value)
    {
        BaseValue = value;
        _isDirty = true;
        OnStatChanged?.Invoke();
    }
}
