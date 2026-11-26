using UnityEngine;

public class ChangeMapUI_HTY : MonoBehaviour
{
    [SerializeField] private GameObject[] _tabPanel;


    private void Start()
    {
        foreach (var v in _tabPanel)
        {
            v.SetActive(false);
        }
        _tabPanel[0].SetActive(true);
    }

    public void ChangeTab(int myChapter)
    {
        foreach(var v in _tabPanel)
        {
            v.SetActive(false);
        }
        _tabPanel[myChapter-1].SetActive(true);
    }
}
