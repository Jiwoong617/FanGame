using System;
using System.Collections.Generic;
using UnityEngine;

public class EventManager
{
    public event Action OnEventFinished;

    private EventUI eventUI;
    private Queue<EventData> eventDeck = new Queue<EventData>();

    public void SetUI(EventUI ui)
    {
        eventUI = ui;
    }

    public void LoadEvents(string characterName)
    {
        eventDeck.Clear();
        List<EventData> tempList = new List<EventData>();

        //공용 이벤트
        var commonEvents = Resources.LoadAll<EventData>("Events/Common");
        if (commonEvents != null)
        {
            tempList.AddRange(commonEvents);
        }

        if (!string.IsNullOrEmpty(characterName))
        {
            var charEvents = Resources.LoadAll<EventData>($"Events/{characterName}");
            if (charEvents != null)
            {
                tempList.AddRange(charEvents);
            }
        }

        ShuffleList(tempList);
        foreach (var evt in tempList)
            eventDeck.Enqueue(evt);
    }

    public void StartEvent()
    {
        if (eventUI == null)
        {
            Debug.LogError("EventUI is not set!");
            CompleteEvent(); 
            return;
        }

        EventData selectedEvent = SelectRandomEvent();
        if (selectedEvent != null)
        {
            eventUI.ShowEvent(selectedEvent);
        }
        else
        {
            Debug.LogWarning("[EventManager] No events available in the pool. Check Resources/Events folder.");
            // 이벤트가 없어도 게임이 멈추지 않도록 완료 처리
            CompleteEvent();
        }
    }

    private EventData SelectRandomEvent()
    {
        if (eventDeck.Count > 0)
            return eventDeck.Dequeue();
        else
            return null;
    }

    public void CompleteEvent()
    {
        Debug.Log("[EventManager] Event Completed");
        OnEventFinished?.Invoke();
    }

    private void ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = UnityEngine.Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}