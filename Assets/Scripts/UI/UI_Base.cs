using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class UI_Base : MonoBehaviour
{
    private readonly Dictionary<Type, Dictionary<string, Component>> _components = new();

    private void Awake()
    {
        Init();
    }

    protected abstract void Init();

    protected void Bind<T>(Type enumType) where T : Component
    {
        if (!enumType.IsEnum)
        {
            Debug.LogError("Bind failed: type is not enum");
            return;
        }

        if (_components.ContainsKey(typeof(T)))
            return;

        var map = new Dictionary<string, Component>();
        _components.Add(typeof(T), map);

        foreach (string name in Enum.GetNames(enumType))
        {
            var comp = GetComponentsInChildren<T>(true).FirstOrDefault(x => x.name == name);
            if (comp == null)
            {
                Debug.LogError($"[UI] {name} has no {typeof(T)}");
                continue;
            }

            map.Add(name, comp);
        }
    }

    protected T Get<T>(Enum key) where T : Component
    {
        if (_components.TryGetValue(typeof(T), out var map) == false)
            return null;

        return map[key.ToString()] as T;
    }

    public virtual void Show() => gameObject.SetActive(true);
    public virtual void Hide() => gameObject.SetActive(false);
}
