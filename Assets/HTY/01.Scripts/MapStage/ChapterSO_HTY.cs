using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ChapterSO", menuName = "So_HTY/ChapterSO")]
public class ChapterSO_HTY : ScriptableObject
{
    public List<StageSO_HTY> _stageList;
    public bool _unLock = false;
}
