using System;
using System.Collections.Generic;
using JJB.Script.Player;
using JJB.Script.Player.Interface;
using LDY.Hud.Core;
using UnityEngine;

namespace LDY.Hud.Adapters {
    // 지빈 JJBPlayerAbsorb와 같은 원·레이어로 흡수 대상을 찾는다. 보유 여부는 JJBPlayerGimmick.HasGimmick(흡수를 막는 바로 그 값).
    internal sealed class JjbAbsorbTargetSource : IAbsorbTargetSource, IHudRefreshable, IDisposable {
        private static readonly PrivateField<Transform> AbsorbPoint =
            PrivateField<Transform>.Find(typeof(JJBPlayerAbsorb), "absorbPoint");
        private static readonly PrivateField<LayerMask> AbsorbLayer =
            PrivateField<LayerMask>.Find(typeof(JJBPlayerAbsorb), "absorbLayer");
        private static readonly PrivateField<float> AbsorbSize =
            PrivateField<float>.Find(typeof(JJBPlayerAbsorb), "absorbSize");

        private readonly List<Collider2D> _hits = new List<Collider2D>();
        private readonly JJBPlayerAbsorb _absorb;
        private readonly JJBPlayerGimmick _gimmick;
        private readonly JJBPlayerInput _input;

        private JjbAbsorbTargetSource(JJBPlayerAbsorb absorb, JJBPlayerGimmick gimmick, JJBPlayerInput input) {
            _absorb = absorb;
            _gimmick = gimmick;
            _input = input;
            _input.OnAbsorbPressed += RaisePressed;
        }

        public static JjbAbsorbTargetSource TryCreate(GameObject player, out string reason) {
            reason = null;
            if (AbsorbPoint == null || AbsorbLayer == null || AbsorbSize == null)
                reason = "JJBPlayerAbsorb의 absorbPoint/absorbLayer/absorbSize 필드를 찾지 못함";
            else if (!player.TryGetComponent<JJBPlayerAbsorb>(out _))
                reason = $"{player.name}에 JJBPlayerAbsorb 없음";
            else if (!player.TryGetComponent<JJBPlayerGimmick>(out _))
                reason = $"{player.name}에 JJBPlayerGimmick 없음(이 플레이어는 흡수 자체가 동작하지 않음)";
            else if (!player.TryGetComponent<JJBPlayerInput>(out _))
                reason = $"{player.name}에 JJBPlayerInput 없음";
            if (reason != null)
                return null;

            return new JjbAbsorbTargetSource(player.GetComponent<JJBPlayerAbsorb>(),
                player.GetComponent<JJBPlayerGimmick>(), player.GetComponent<JJBPlayerInput>());
        }

        public bool HasTarget { get; private set; }
        public HudRect TargetBounds { get; private set; }
        public bool IsHolding { get; private set; }
        public event Action AbsorbPressed;

        public void Refresh() {
            IsHolding = _gimmick != null && _gimmick.HasGimmick;
            var target = _absorb != null ? FindTarget() : null;
            HasTarget = target != null;
            TargetBounds = HasTarget ? WorldBounds.Of(target.gameObject) : default;
        }

        public void Dispose() {
            if (_input != null)
                _input.OnAbsorbPressed -= RaisePressed;
        }

        private void RaisePressed() => AbsorbPressed?.Invoke();

        private Component FindTarget() {
            var point = AbsorbPoint.Read(_absorb);
            if (point == null)
                return null;

            var filter = new ContactFilter2D {
                useLayerMask = true,
                layerMask = AbsorbLayer.Read(_absorb),
                useTriggers = Physics2D.queriesHitTriggers
            };
            Physics2D.OverlapCircle(point.position, AbsorbSize.Read(_absorb), filter, _hits);
            foreach (var hit in _hits) {
                if (hit == null)
                    continue;

                var absorbable = hit.GetComponentInParent<IAbsorbable>();
                if (absorbable is Component component && component != null && !IsPlayer(component))
                    return component;
            }

            return null;
        }

        // 본체·복제체(또는 그 자식)는 흡수 대상으로 표시하지 않는다.
        private static bool IsPlayer(Component component) => component.GetComponentInParent<JJBPlayer>() != null;
    }
}
