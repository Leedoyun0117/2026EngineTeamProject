using System.Collections.Generic;
using UnityEngine;

namespace JJB.Script.Level
{
    [CreateAssetMenu(fileName = "LevelList", menuName = "JJB/LevelList")]
    public class LevelListSO : ScriptableObject
    {
        [SerializeField] private List<LevelSO> levels = new List<LevelSO>();

        public int Count => levels.Count;

        public LevelSO GetLevel(int index)
        {
            if (index < 0 || index >= levels.Count)
                return null;

            return levels[index];
        }
    }
}