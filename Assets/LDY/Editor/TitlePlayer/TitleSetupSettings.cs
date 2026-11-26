using UnityEditor;
using UnityEngine;

namespace LDY.Script.Editor {
    // Setup Title Scene이 쓰는 외부(팀원) 에셋 참조. 경로 문자열 대신 참조로 들고 있어서 팀원이 파일 이름을 바꿔도 GUID로 따라간다.
    // 에디터 폴더 아래에 있어 빌드에 들어가지 않는다.
    public sealed class TitleSetupSettings : ScriptableObject {
        public const string Folder = "Assets/LDY/Editor/Settings";
        public const string AssetPath = Folder + "/TitleSetupSettings.asset";

        // 에셋을 처음 만들 때 한 번만 쓰는 기본값: JJB 플레이어 물리 재질(현재 Assets/JJB/JJBPM2D.physicsMaterial2D)
        private const string DefaultPhysicsMaterialGuid = "019de775d98a4c648ace45e888bef39e";

        [Tooltip("AltPlayer Rigidbody2D의 물리 재질. 비우면 Setup Title Scene이 기존 재질을 덮어쓰지 않고 경고만 남긴다")]
        public PhysicsMaterial2D playerPhysicsMaterial;

        public static TitleSetupSettings LoadOrCreate() {
            var settings = AssetDatabase.LoadAssetAtPath<TitleSetupSettings>(AssetPath);
            if (settings != null) return settings;

            settings = CreateInstance<TitleSetupSettings>();
            var materialPath = AssetDatabase.GUIDToAssetPath(DefaultPhysicsMaterialGuid);
            settings.playerPhysicsMaterial = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(materialPath);
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets/LDY/Editor", "Settings");
            AssetDatabase.CreateAsset(settings, AssetPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"[Title] {AssetPath} 만듦. 물리 재질 = " +
                      (settings.playerPhysicsMaterial != null ? materialPath : $"없음(GUID {DefaultPhysicsMaterialGuid}를 찾지 못함)"));
            return settings;
        }
    }
}
