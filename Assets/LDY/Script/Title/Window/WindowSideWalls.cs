using JJB.Script;
using UnityEngine;

namespace LDY.Script
{
    // ContentBoundsWalls처럼 지정 레이어를 그대로 쓰고, 벽에 안 붙게 마찰 0 머티리얼 사용
    public sealed class WindowSideWalls : MonoBehaviour
    {
        private const float Thickness = 1f;

        private PhysicsMaterial2D _material;
        private Collider2D[] _colliders;

        // 창 로컬 좌표. ceilingY는 위쪽 경계 아랫면 높이(null이면 위쪽 경계 없음)
        public void Configure(int layer, float xMin, float xMax, float yMin, float yMax, float? ceilingY,
            Collider2D player, IPlayerBody body, bool debug)
        {
            gameObject.layer = layer;
            _material = new PhysicsMaterial2D("WindowWall") { friction = 0f, bounciness = 0f };

            float height = yMax - yMin;
            float centerY = (yMin + yMax) * 0.5f;
            // 설정, 크레딧 창의 UI의 벽을 만듬, 두께와 좌측 우측 계산도 같이 해줌.
            var colliders = new System.Collections.Generic.List<Collider2D>
            {
                CreateWall("Left", new Vector2(xMin - Thickness * 0.5f, centerY), new Vector2(Thickness, height)),
                CreateWall("Right", new Vector2(xMax + Thickness * 0.5f, centerY), new Vector2(Thickness, height)) 
            };

            if (ceilingY.HasValue)
            {
                colliders.Add(CreateWall("Ceiling", new Vector2((xMin + xMax) * 0.5f, ceilingY.Value + Thickness * 0.5f),
                    new Vector2(xMax - xMin + Thickness * 2f, Thickness)));
            }

            _colliders = colliders.ToArray();
            ConfigureDebug(player, body, debug);
        }

        public void SetActive(bool active)
        {
            gameObject.SetActive(active);
        }

        private Collider2D CreateWall(string wallName, Vector2 localCenter, Vector2 size)
        {
            var wall = new GameObject(wallName) { layer = gameObject.layer };
            wall.transform.SetParent(transform, false);
            wall.transform.localPosition = localCenter;
            var box = wall.AddComponent<BoxCollider2D>();
            box.size = size;
            box.sharedMaterial = _material;
            return box;
        }

        private void OnDestroy()
        {
            if (_material != null)
                Destroy(_material);
        }

#if UNITY_EDITOR
        private Collider2D _player;
        private IPlayerBody _body;
        private JJBPlayerJump _jump;
        private bool _debug;
        private bool _hadState;
        private bool _lastTouching;
        private bool _lastGrounded;
        private int _lastMotion;

        private void ConfigureDebug(Collider2D player, IPlayerBody body, bool debug)
        {
            _player = player;
            _body = body;
            _debug = debug;
            _jump = player != null ? player.GetComponentInParent<JJBPlayerJump>() : null;
        }

        private void OnDisable()
        {
            _hadState = false;
        }

        // 벽에 붙은 채 점프/낙하할 때 땅 판정이 켜지는지 확인용, 상태 바뀔 때만 로그
        private void FixedUpdate()
        {
            if (!_debug || _player == null || _colliders == null)
                return;

            bool touching = false;
            foreach (Collider2D wall in _colliders)
            {
                if (_player.IsTouching(wall))
                {
                    touching = true;
                    break;
                }
            }

            bool grounded = _jump != null && _jump.IsGrounded;
            float vy = _body.Velocity.y;
            int motion = vy > 0.5f ? 1 : vy < -0.5f ? -1 : 0;
            if (_hadState && touching == _lastTouching && grounded == _lastGrounded && motion == _lastMotion)
                return;

            _hadState = true;
            _lastTouching = touching;
            _lastGrounded = grounded;
            _lastMotion = motion;

            string motionText = motion > 0 ? "상승" : motion < 0 ? "낙하" : "정지";
            Debug.Log($"[WindowSideWalls] 벽 접촉={touching} 땅 판정={grounded} 움직임={motionText} vy={vy:F2}");
            if (touching && grounded && motion != 0)
            {
                Debug.LogWarning("[WindowSideWalls] 벽에 붙은 채 공중인데 땅 판정이 켜짐, 벽 레이어를 땅 판정에서 제외 필요(WindowPanel의 벽 레이어)");
            }
        }
#else
        private void ConfigureDebug(Collider2D player, IPlayerBody body, bool debug) { }
#endif
    }
}
