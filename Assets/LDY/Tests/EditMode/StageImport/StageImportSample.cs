using System.IO;
using LDY.StageImport.Core;

namespace LDY.Tests {
    internal static class StageImportSample {
        public const string Path = "Assets/LDY/StageData/Samples/stages.sample.json";

        public static CursorStageFile Load() => CursorStageParser.Parse(File.ReadAllText(Path));
    }
}
