using System.Collections.Generic;
using System.Linq;

namespace LDY.Script {
    internal readonly struct TagSpec {
        public readonly string Name;
        public readonly PlayerAnimId Id;
        public readonly bool Loop;

        public TagSpec(string name, PlayerAnimId id, bool loop) {
            Name = name;
            Id = id;
            Loop = loop;
        }
    }

    // player_anim_cursor.aseprite 태그 이름과 PlayerAnimId, 반복 여부. 범위와 프레임 시간은 파일에서 읽는다.
    internal static class CursorTagSpecs {
        public static readonly TagSpec[] All = {
            new("idle", PlayerAnimId.Idle, true),
            new("walk", PlayerAnimId.Walk, true),
            new("run", PlayerAnimId.Run, true),
            new("sit_down", PlayerAnimId.SitDown, false),
            new("sit_loop", PlayerAnimId.SitLoop, true),
            new("stand_up", PlayerAnimId.StandUp, false),
            new("jump", PlayerAnimId.Jump, false),
            new("fall", PlayerAnimId.Fall, true),
            new("grab", PlayerAnimId.Grab, true),
            new("rope_hang", PlayerAnimId.RopeHang, true),
            new("rope_climb", PlayerAnimId.RopeClimb, true),
            new("rope_climb_down", PlayerAnimId.RopeClimbDown, true),
            new("push", PlayerAnimId.Push, true),
            new("land", PlayerAnimId.Land, false),
            new("trapped", PlayerAnimId.Trapped, false),
            new("death", PlayerAnimId.Death, false)
        };

        public static IReadOnlyCollection<string> Names => All.Select(s => s.Name).ToArray();

        public static string NameOf(PlayerAnimId id) {
            foreach (var spec in All)
                if (spec.Id == id)
                    return spec.Name;

            return id.ToString();
        }
    }
}
