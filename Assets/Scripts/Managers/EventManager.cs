using System;
using UnityEngine;

public class EventManager
{
    public event Action OnEventFinished;

    public void StartEvent()
    {
        Debug.Log("[EventManager] Event Started");
        // TODO: 이벤트 UI 표시
    }

    public void CompleteEvent()
    {
        Debug.Log("[EventManager] Event Completed");
        OnEventFinished?.Invoke();
    }
}
