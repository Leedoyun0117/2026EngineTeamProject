using LDY.PlayerMerge;
using UnityEngine;

namespace LDY.Script {
    internal sealed class ChildSync {
        private readonly Transform _base;
        private readonly Transform _ours;
        private readonly MergeLog _log;

        public ChildSync(Transform baseRoot, Transform oursRoot, MergeLog log) {
            _base = baseRoot;
            _ours = oursRoot;
            _log = log;
        }

        public void Run() {
            foreach (Transform source in _base) {
                var child = _ours.Find(source.name);
                if (child == null)
                    Create(source);
                else if (MergePolicy.ForChild(source.name) == FieldOwner.Base)
                    SyncPosition(source, child);
            }
        }

        private void Create(Transform source) {
            var created = new GameObject(source.name) { layer = source.gameObject.layer };
            created.transform.SetParent(_ours, false);
            created.transform.localPosition = source.localPosition;
            _log.Changed.Add($"+{source.name} (자식 오브젝트) 위치 {source.localPosition.ToString("0.####")}");
        }

        private void SyncPosition(Transform source, Transform target) {
            if (target.localPosition == source.localPosition)
                return;

            _log.Changed.Add($"{source.name}.localPosition: {target.localPosition.ToString("0.####")} -> {source.localPosition.ToString("0.####")}");
            target.localPosition = source.localPosition;
        }
    }
}
