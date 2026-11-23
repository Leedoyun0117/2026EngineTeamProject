using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace LDY.PixelStyleEditor
{
    /// <summary>새 도트 프레임으로 AnimationClip과 AnimatorController를 만든다. 반복 실행해도 같은 에셋을 갱신한다.</summary>
    public static class GearAnimationBuilder
    {
        public static bool TryLoadFrames(out Sprite[] frames)
        {
            frames = new Sprite[PixelGearPaths.FrameCount];
            for (int i = 0; i < frames.Length; i++)
            {
                frames[i] = AssetDatabase.LoadAssetAtPath<Sprite>(PixelGearPaths.SpritePath(i));
                if (frames[i] == null)
                {
                    Debug.LogError("[PixelStyle] 스프라이트 없음: " + PixelGearPaths.SpritePath(i));
                    return false;
                }
            }
            return true;
        }

        public static AnimatorController Build(Sprite[] frames)
        {
            AnimationClip clip = BuildClip(frames);
            return BuildController(clip);
        }

        private static AnimationClip BuildClip(Sprite[] frames)
        {
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(PixelGearPaths.ClipPath);
            if (clip == null)
            {
                clip = new AnimationClip { name = "gear_pixel_spin" };
                AssetDatabase.CreateAsset(clip, PixelGearPaths.ClipPath);
            }

            // 0.06초는 50fps의 정확히 3프레임이라 키 시간이 격자에 맞는다.
            clip.frameRate = 50f;

            ObjectReferenceKeyframe[] keys = new ObjectReferenceKeyframe[frames.Length];
            for (int i = 0; i < frames.Length; i++)
                keys[i] = new ObjectReferenceKeyframe { time = i * PixelGearPaths.FrameSeconds, value = frames[i] };

            EditorCurveBinding binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
            AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);

            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            settings.stopTime = frames.Length * PixelGearPaths.FrameSeconds;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            EditorUtility.SetDirty(clip);
            return clip;
        }

        private static AnimatorController BuildController(AnimationClip clip)
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(PixelGearPaths.ControllerPath);
            if (controller == null)
                controller = AnimatorController.CreateAnimatorControllerAtPath(PixelGearPaths.ControllerPath);

            AnimatorStateMachine machine = controller.layers[0].stateMachine;
            AnimatorState state = machine.defaultState;
            if (state == null) state = machine.AddState("spin");
            state.motion = clip;
            state.speed = 1f;

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            return controller;
        }
    }
}
