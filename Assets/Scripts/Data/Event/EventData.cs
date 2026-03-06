using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Event", menuName = "EventData/Event")]
public class EventData : ScriptableObject
{
    [Header("Basic Info")]
    public string title;
    public Sprite eventImage;
    [TextArea] public string description;
    
    [Header("Options")]
    public List<EventOption> options;
}

[System.Serializable]
public class EventOption
{
    public string buttonText;
    [TextArea] public string resultText;

    [SerializeReference, SerializeReferenceDropdown]
    public List<EventOutcome> outcomes = new List<EventOutcome>();
}
