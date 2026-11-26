using System.Collections.Generic;
using JJB.Script;
using UnityEngine;

namespace HES.Script.Object {
    public class BlockManager : MonoSingleton<BlockManager> {
        [SerializeField] private List<Block> blocks;
        [SerializeField] private List<GameObject> realReceiver;
        [SerializeField] private List<GameObject> fakeReceiver;

        protected override void Awake() {
            base.Awake();
            foreach (Block block in blocks) {
                block.OnPressFake += OnFakeRun;
                block.OnPressReal += OnRealRun;
            }
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            foreach (Block block in blocks) {
                block.OnPressFake -= OnFakeRun;
                block.OnPressReal -= OnRealRun;
            }
        }

        private void OnRealRun(bool isPressed) {
            foreach (var item in realReceiver) {
                item.SetActive(isPressed);
            }
        }

        private void OnFakeRun(bool isPressed) {
            foreach (var item in fakeReceiver) {
                item.SetActive(isPressed);
            }
        }
    }
}