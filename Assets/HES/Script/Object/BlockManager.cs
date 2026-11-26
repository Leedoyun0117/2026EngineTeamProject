using System.Collections.Generic;
using JJB.Script;
using UnityEngine;

namespace HES.Script.Object {
    public class BlockManager : MonoSingleton<BlockManager> {
        [SerializeField] private List<Block> blocks;
        [SerializeField] private List<GameObject> realReceiver;

        protected override void Awake() {
            base.Awake();
            foreach (var block in blocks) {
                block.OnPressReal += OnRealRun;
            }
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            foreach (var block in blocks) {
                block.OnPressReal -= OnRealRun;
            }
        }

        private void OnRealRun(bool isPressed) {
            foreach (var item in realReceiver) item.SetActive(isPressed);
        }
    }
}