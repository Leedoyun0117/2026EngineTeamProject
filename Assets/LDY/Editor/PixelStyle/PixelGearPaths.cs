namespace LDY.PixelStyleEditor
{
    public static class PixelGearPaths
    {
        public const string PrefabPath = "Assets/LDY/Prefabs/Stage1/Stage1_Gear.prefab";

        public const string SpriteFolder = "Assets/LDY/Sprites/PixelStyle/Gear";
        public const string ClipPath = SpriteFolder + "/gear_pixel_spin.anim";
        public const string ControllerPath = SpriteFolder + "/gear_pixel.controller";

        public const int FrameCount = 4;
        public const float FrameSeconds = 0.06f;

        public static string SpritePath(int frame)
        {
            return SpriteFolder + "/gear_pixel_" + frame + ".png";
        }

        // 원본 gear.aseprite의 프리팹 참조. 원복용.
        public const string OriginalAsepritePath = "Assets/LDY/Sprites/stage1_music_objects/stage1_music_objects/gear.aseprite";
        public const long OriginalSpriteFileId = 2946477816058973573;
        public const long OriginalControllerFileId = 5980690146391866922;
    }
}
