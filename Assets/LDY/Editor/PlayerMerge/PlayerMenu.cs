using UnityEditor;

namespace LDY.Script {
    internal static class PlayerMenu {
        private const string Root = "LDY/Player/";
        private const string Merge = Root + "Merge/";
        private const string Apply = Merge + "3. Swap Scenes (Apply)/";

        [MenuItem(Root + "Compare Prefabs")]
        private static void Compare() => PlayerCompareReport.Run();

        [MenuItem(Root + "Validate")]
        private static void Validate() => PlayerValidateRunner.ValidateOpen();

        [MenuItem(Root + "Validate (C Scenes)")]
        private static void ValidateScenes() => PlayerValidateRunner.ValidateScenes();

        [MenuItem(Merge + "1. Build Final Player")]
        private static void BuildFinal() => FinalPlayerBuilder.Run();

        [MenuItem(Merge + "2. Swap Scenes (Dry Run)")]
        private static void DryRun() => SceneSwapper.DryRun();

        [MenuItem(Apply + "JJBLevel")]
        private static void ApplyJJBLevel() => SceneSwapper.Apply("Assets/JJB/JJBLevel.unity");

        [MenuItem(Apply + "JJBScene")]
        private static void ApplyJJBScene() => SceneSwapper.Apply("Assets/JJB/JJBScene.unity");

        [MenuItem(Apply + "Stage1_ArtTest")]
        private static void ApplyStage1() => SceneSwapper.Apply("Assets/LDY/Scenes/Stage1_ArtTest.unity");

        [MenuItem(Apply + "Main_HTY")]
        private static void ApplyMainHty() => SceneSwapper.Apply("Assets/HTY/00.Scenes/Main_HTY.unity");

        [MenuItem(Apply + "RopeTest")]
        private static void ApplyRopeTest() => SceneSwapper.Apply("Assets/LDY/Scenes/RopeTest.unity");

        [MenuItem(Apply + "Stage2_SpiderTest")]
        private static void ApplyStage2() => SceneSwapper.Apply("Assets/LDY/Scenes/Stage2_SpiderTest.unity");

        [MenuItem(Apply + "HES Test")]
        private static void ApplyHesTest() => SceneSwapper.Apply("Assets/HES/Scene/Test.unity");
    }
}
