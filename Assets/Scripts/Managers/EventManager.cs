using System;
using System.Collections.Generic;
using UnityEngine;

public class EventManager
{
    public event Action OnEventFinished;

    private EventUI eventUI;
    private List<EventData> currentEventPool = new List<EventData>();
    private EventData currentEventData;

    public void SetUI(EventUI ui)
    {
        eventUI = ui;
    }

    public void LoadEvents(string characterName)
    {
        currentEventPool.Clear();

        //공용 이벤트
        var commonEvents = Resources.LoadAll<EventData>("Events/Common");
        if (commonEvents != null)
        {
            currentEventPool.AddRange(commonEvents);
        }

        if (!string.IsNullOrEmpty(characterName))
        {
            var charEvents = Resources.LoadAll<EventData>($"Events/{characterName}");
            if (charEvents != null)
            {
                currentEventPool.AddRange(charEvents);
            }
        }
    }

    public void SetupEvent(EventData data)
    {
        if(data == null) return;

        currentEventData = data;    
    }

    public void StartEvent()
    {
        if (eventUI == null)
        {
            Debug.LogError("EventUI is not set!");
            CompleteEvent(); 
            return;
        }

        if (currentEventData != null)
        {
            eventUI.ShowEvent(currentEventData);
        }
        else
        {
            Debug.LogWarning("[EventManager] No events available in the pool. Check Resources/Events folder.");
            // 이벤트가 없어도 게임이 멈추지 않도록 완료 처리
            CompleteEvent();
        }
    }

    public EventData SelectRandomEvent()
    {
        if (currentEventPool == null || currentEventPool.Count == 0)
            return null;

        return currentEventPool[UnityEngine.Random.Range(0, currentEventPool.Count)];
    }

    public void CompleteEvent()
    {
        Debug.Log("[EventManager] Event Completed");
        OnEventFinished?.Invoke();
    }
}
