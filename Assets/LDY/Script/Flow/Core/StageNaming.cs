using System;

namespace LDY.Flow.Core {
    public static class StageNaming {
        public const int TutorialNumber = 0;
        public const int LastNumber = 6;
        public const string RootPath = @"C:\GAME\STAGES";
        public const string TutorialFileName = "튜토리얼.exe";
        private const string TutorialMark = "T";

        public static string Id(int world, int number) =>
            number == TutorialNumber ? $"{world}-{TutorialMark}" : $"{world}-{number}";

        public static string FileName(int world, int number) =>
            number == TutorialNumber ? TutorialFileName : Id(world, number) + ".exe";

        public static string WorldPath(string folderName) => RootPath + @"\" + folderName;

        public static bool TryParse(string id, out int world, out int number) {
            world = 0;
            number = 0;
            if (string.IsNullOrEmpty(id)) return false;
            var parts = id.Split('-');
            if (parts.Length != 2 || !int.TryParse(parts[0], out world)) return false;
            if (parts[1] == TutorialMark) return true;
            return int.TryParse(parts[1], out number) && number > TutorialNumber && number <= LastNumber;
        }

        public static string FormatTime(double seconds) {
            var total = (int)Math.Floor(Math.Max(0d, seconds));
            return $"{total / 60:00}:{total % 60:00}";
        }
    }
}
