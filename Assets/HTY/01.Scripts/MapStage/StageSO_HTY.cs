using UnityEngine;

[CreateAssetMenu(fileName = "StageSO", menuName = "So_HTY/StageSO")]
public class StageSO_HTY : ScriptableObject
{
    public string _stageName;
    public GameObject _stageMap;
    public Sprite _icon;
    public bool _unLock=false;
}
