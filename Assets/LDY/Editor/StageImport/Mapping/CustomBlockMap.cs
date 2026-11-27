using System;
using System.Collections.Generic;
using LDY.StageImport.Core;
using UnityEngine;

namespace LDY.StageImportEditor {
    // 툴의 custom 블록(c_*)을 이름으로 프리팹에 잇는 표. 사람이 고쳐도 된다.
    // 다시 실행하면 빈 칸(프리팹·별칭·메모)만 채우고, locked를 켠 항목은 아예 건드리지 않는다.
    [CreateAssetMenu(menuName = "LDY/Stage Import/Custom Block Map", fileName = "CustomBlockMap")]
    internal sealed class CustomBlockMap : ScriptableObject {
        [Serializable]
        public sealed class Entry {
            [Tooltip("툴의 custom 이름. 공백·물음표·대소문자는 무시하고 비교한다")]
            public string name;

            [Tooltip("같은 항목으로 볼 다른 이름")]
            public string[] aliases = new string[0];

            [Tooltip("비워 두면 이름표가 붙은 분홍 임시 블록을 놓는다(충돌은 툴의 col 값)")]
            public GameObject prefab;

            [Tooltip("PerCell: 칸마다 / Group4: 상하좌우로 이어진 칸 묶음 / HorizontalRun: 가로로 이어진 칸 / Area: 묶음을 트리거 영역으로")]
            public CustomBinding binding;

            [Tooltip("묶음일 때. Stretch: 하나를 Tiled로 늘림 / RepeatPerCell: 칸마다 복제 / OnePerGroup: 묶음 가운데 하나(원래 크기) / " +
                     "OneAtLeft: 묶음의 가장 왼쪽 칸에 하나 / HangFromTop: 맨 위 칸 윗변(천장 면) 가운데가 고정점, 그 열의 칸 수가 길이(밧줄)")]
            public CustomSizing sizing;

            [Tooltip("타일로 대체(Ground/Platform/Spike). None이 아니면 프리팹·묶는 방식은 쓰지 않는다")]
            public CustomTileReplace tile;

            [Tooltip("프리팹이 비었을 때 놓을 충돌 없는 장식 그림. 묶음 크기로 Tiled로 늘린다")]
            public Sprite decoration;

            [Tooltip("켜면 프리팹·장식 그림이 모두 없을 때 분홍 블록 대신 건너뛴다")]
            public bool skipIfEmpty;

            [Tooltip("켜면 다시 실행해도 자동 채우기가 이 항목을 건드리지 않는다")]
            public bool locked = false;

            [TextArea(1, 4)] public string note;
        }

        [SerializeField] private List<Entry> entries = new List<Entry>();

        public List<Entry> Entries => entries;

        public Entry Find(string entryName) {
            foreach (var entry in entries)
                if (entry.name == entryName)
                    return entry;
            return null;
        }

        public CustomNameIndex BuildIndex() {
            var index = new CustomNameIndex();
            foreach (var entry in entries) {
                if (string.IsNullOrWhiteSpace(entry.name)) continue;
                index.Add(new CustomRule(entry.name, entry.binding, entry.sizing,
                    entry.prefab != null || entry.decoration != null, entry.tile, entry.skipIfEmpty), entry.aliases);
            }

            return index;
        }
    }
}
