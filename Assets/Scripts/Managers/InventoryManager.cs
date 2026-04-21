using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager
{
    public event Action<RewardData> OnItemAdded;
    public event Action<RewardData> OnItemRemoved;

    private List<RewardData> Items = new List<RewardData>();

    public void AddItem(RewardData item)
    {
        if (item == null || !item.isItem) return;

        Items.Add(item);
        Debug.Log($"[InventoryManager] 아이템 획득: {item.RewardName}");

        // UI 등 갱신을 위한 이벤트 호출
        OnItemAdded?.Invoke(item);
    }

    public void RemoveItem(RewardData item)
    {
        if (Items.Contains(item))
        {
            Items.Remove(item);
            OnItemRemoved?.Invoke(item);
        }
    }

    public bool HasAbility<T>() where T : Ability
    {
        foreach (var item in Items)
        {
            if (item.abilities == null) continue;
            foreach (var ability in item.abilities)
            {
                if (ability is T)
                    return true;
            }
        }
        return false;
    }

    public void RemoveArtifact<T>() where T : Ability
    {
        RewardData targetItem = null;

        foreach (var item in Items)
        {
            if (item.abilities != null)
            {
                foreach (var ability in item.abilities)
                {
                    if (ability is T)
                    {
                        targetItem = item;
                        break;
                    }
                }
            }
            if (targetItem != null) break;
        }

        if (targetItem != null)
        {
            RemoveItem(targetItem);
        }
    }

    public void ClearInventory()
    {
        Items.Clear();
    }
}