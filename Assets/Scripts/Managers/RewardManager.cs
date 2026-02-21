using System;
using System.Collections.Generic;
using UnityEngine;

public class RewardManager
{
    public event Action OnRewardSelected;

    private RewardUI rewardUI;
    private List<RewardData> currentRewards = new List<RewardData>();

    public void SetUI(RewardUI ui)
    {
        rewardUI = ui;
    }

    public void ShowRewardUI(StageData stageData, NodeType battleType)
    {
        if (rewardUI == null)
        {
            Debug.LogError("[RewardManager] RewardUI is not set!");
            return;
        }

        List<RewardData> targetPool = GetRewardPool(stageData, battleType);

        if (stageData == null || targetPool == null || targetPool.Count == 0)
        {
            Debug.LogError("[RewardManager] No rewards available in StageData!");

            rewardUI.SetRewards(new List<RewardData>());
            rewardUI.Show();
            return;
        }

        GetRandomRewards(targetPool, 3);
        rewardUI.SetRewards(currentRewards);
        rewardUI.Show();
    }

    // UI 버튼 클릭 시 호출 (0, 1, 2)
    public void SelectReward(int index)
    {
        if (index < 0 || index >= currentRewards.Count)
        {
            rewardUI.Hide();
            OnRewardSelected?.Invoke();
            return;
        }

        RewardData selected = currentRewards[index];
        Debug.Log($"[RewardManager] Selected: {selected.RewardName}");

        if (GameManager.Instance.Player != null)
        {
            selected.Apply(GameManager.Instance.Player);
            if (selected.isItem)
            {
                GameManager.Inventory.AddItem(selected);
            }
        }
        
        rewardUI.Hide();
        OnRewardSelected?.Invoke();
    }

    private void GetRandomRewards(List<RewardData> pool, int count)
    {
        currentRewards.Clear();

        if (pool == null) return;

        int n = pool.Count;
        if (n <= count)
        {
            currentRewards.AddRange(pool);
            return;
        }

        HashSet<int> selectedIndices = new HashSet<int>();
        for (int j = n - count; j < n; j++)
        {
            int t = UnityEngine.Random.Range(0, j + 1);
            
            if (!selectedIndices.Add(t))
            {
                selectedIndices.Add(j);
            }
        }

        foreach (int index in selectedIndices)
        {
            currentRewards.Add(pool[index]);
        }

        //FisherYates - 필요없을듯
        //for (int i = 0; i < currentRewards.Count; i++)
        //{
        //    int rnd = UnityEngine.Random.Range(i, currentRewards.Count);
        //    (currentRewards[i], currentRewards[rnd]) = (currentRewards[rnd], currentRewards[i]);
        //}
    }

    private List<RewardData> GetRewardPool(StageData stageData, NodeType type)
    {
        switch (type)
        {
            case NodeType.Monster: return stageData.normalRewards;
            case NodeType.Elite: return stageData.eliteRewards;
            case NodeType.Boss: return stageData.bossRewards;
            default: return stageData.normalRewards;
        }
    }
}
