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

    public void ShowRewardUI(StageData stageData)
    {
        if (rewardUI == null)
        {
            Debug.LogError("[RewardManager] RewardUI is not set!");
            return;
        }

        if (stageData == null || stageData.rewards == null || stageData.rewards.Count == 0)
        {
            Debug.LogError("[RewardManager] No rewards available in StageData!");

            rewardUI.SetRewards(new List<RewardData>());
            rewardUI.Show();
            return;
        }

        GetRandomRewards(stageData, 3);
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

    private void GetRandomRewards(StageData stageData, int count)
    {
        currentRewards.Clear();

        if (stageData.rewards == null) return;

        int n = stageData.rewards.Count;
        if (n <= count)
        {
            currentRewards.AddRange(stageData.rewards);
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
            currentRewards.Add(stageData.rewards[index]);
        }

        //FisherYates - 필요없을듯
        //for (int i = 0; i < currentRewards.Count; i++)
        //{
        //    int rnd = UnityEngine.Random.Range(i, currentRewards.Count);
        //    (currentRewards[i], currentRewards[rnd]) = (currentRewards[rnd], currentRewards[i]);
        //}
    }
}
