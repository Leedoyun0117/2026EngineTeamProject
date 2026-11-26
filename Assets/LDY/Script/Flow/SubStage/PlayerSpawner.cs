using System.Linq;
using LDY.Flow.Core;
using UnityEngine;

namespace LDY.Flow {
    public static class PlayerSpawner {
        public static Transform FindSpawn(GameObject root) {
            var transforms = root.GetComponentsInChildren<Transform>(true);
            var candidates = transforms
                .Select(t => new SpawnCandidate(t.name, t.GetComponent<StageSpawnPoint>() != null))
                .ToArray();
            var best = StageObjectNames.BestSpawn(candidates);
            return best < 0 ? null : transforms[best];
        }

        // 시작 지점이 없으면 플레이어를 옮기지 않는다.
        public static bool MoveTo(GameObject player, Transform spawn) {
            if (player == null || spawn == null) return false;
            var body = player.GetComponent<Rigidbody2D>();
            player.transform.position = new Vector3(spawn.position.x, spawn.position.y, player.transform.position.z);
            if (body != null) {
                body.position = spawn.position;
                body.linearVelocity = Vector2.zero;
                body.angularVelocity = 0f;
            }

            Physics2D.SyncTransforms();
            return true;
        }
    }
}
