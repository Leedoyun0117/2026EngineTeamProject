using UnityEngine;

public class MapLoader_HTY : MonoBehaviour
{
    public GameObject _currentMap;

    public void ChangeMap(StageSO_HTY stage)
    {
        if(_currentMap != null)
        {
            Destroy(_currentMap);
        }

        if (stage._stageMap == null) return;
        _currentMap = Instantiate(stage._stageMap);
    }
}
