using UnityEngine;
using UnityEngine.Serialization;

namespace JJB.Script.Level
{
    public class LevelInstance : MonoBehaviour
    {
        [SerializeField] private Transform spawnPoint;

        public Vector3 SpawnPosition => spawnPoint != null ? spawnPoint.position : transform.position;
    }
}