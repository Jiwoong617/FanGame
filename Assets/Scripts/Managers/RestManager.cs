using System;
using UnityEngine;

public class RestManager
{
    public event Action OnRestFinished;

    public void StartRest()
    {
        Debug.Log("[RestManager] Resting at Campfire...");
        // TODO: 휴식 UI 표시 (회복/강화 선택)
        
        // 임시 자동 완료 (테스트용)
        HealPlayer();
    }

    public void HealPlayer()
    {
        Debug.Log("[RestManager] Player Healed.");
        FinishRest();
    }

    private void FinishRest()
    {
        OnRestFinished?.Invoke();
    }
}
