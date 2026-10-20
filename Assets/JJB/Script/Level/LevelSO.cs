using UnityEngine;

namespace JJB.Script.Level
{
    [CreateAssetMenu(fileName = "LevelData", menuName = "JJB/LevelData")]
    public class LevelSO : ScriptableObject
    {
        [SerializeField] private string levelName;
        [SerializeField] private LevelInstance levelPrefab;
        
        public string LevelName => levelName;
        public LevelInstance LevelPrefab => levelPrefab;
    }
}