using System;
using UnityEngine;

public class RewardManager
{
    public event Action OnRewardSelected;

    public void ShowRewardUI()
    {
        Debug.Log("[RewardManager] Showing Reward UI...");
        // TODO: UI 띄우기 (카드 3장 등)
    }

    // UI 버튼 등에서 호출될 메서드
    public void SelectReward(int rewardIndex)
    {
        Debug.Log($"[RewardManager] Reward {rewardIndex} selected.");
        // TODO: 보상 적용 로직 (아이템 획득, 스텟 증가 등)
        
        OnRewardSelected?.Invoke();
    }
}
