using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager
{
    public event Action<RewardBase> OnItemAdded;

    private List<RewardBase> Items = new List<RewardBase>();

    public void AddItem(RewardBase item)
    {
        if (item == null || !item.isItem) return;

        Items.Add(item);
        Debug.Log($"[InventoryManager] 아이템 획득: {item.RewardName}");

        // UI 등 갱신을 위한 이벤트 호출
        OnItemAdded?.Invoke(item);
    }

    public void ClearInventory()
    {
        Items.Clear();
    }
}