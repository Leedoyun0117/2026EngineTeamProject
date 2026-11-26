using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingMapUI_HTY : MonoBehaviour
{
    [SerializeField] private GameObject _uiPrefab;
    [SerializeField] private ChapterListSO_HTY _chapterList;
    [SerializeField] private Transform[] _parentTran;

    private void Start()
    {
        for(int i =0; i < 3; i++)
        {
            foreach (var stageSo in _chapterList._list[i]._stageList)
            {
                GameObject btn = Instantiate(_uiPrefab, _parentTran[i]);//생성
                btn.GetComponent<Button>()?.onClick.AddListener(() => CanOpen(stageSo));
                if (btn.GetComponent<Button>() != null)
                {
                    btn.GetComponent<Image>().sprite = stageSo._icon;
                }
                
                btn.GetComponentInChildren<TextMeshProUGUI>().text = stageSo._stageName;
            }
        } 
    }

    public void CanOpen(StageSO_HTY stage)
    {
        if (stage._unLock)
        {
            Debug.Log($"{stage._stageName}로 이동");
        }
        else
        {
            Debug.Log($"{stage._stageName}는 미해금");
        }
    }
}
