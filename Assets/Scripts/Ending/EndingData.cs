using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEnding", menuName = "Scriptable Objects/EndingData")]
public class EndingData : ScriptableObject
{
    [Tooltip("순서대로 보여줄 페이지 프리팹")]
    public List<GameObject> pagePrefabs;
}