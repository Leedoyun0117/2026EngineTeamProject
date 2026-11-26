using LDY.StageImport.Core;
using NUnit.Framework;

namespace LDY.Tests {
    public sealed class StageImportParserTests {
        [Test]
        public void Parse_ReadsSample() {
            var file = StageImportSample.Load();
            Assert.AreEqual(3, file.Version);
            Assert.AreEqual(4, file.Stages.Count);
            Assert.AreEqual(4, file.Customs.Count);
            Assert.AreEqual(3, file.Slots.Count);

            var ghost = file.FindStage("m_ghost");
            Assert.AreEqual("Ghost Room", ghost.Name);
            Assert.AreEqual(6, ghost.Width);
            Assert.AreEqual(4, ghost.Height);
            Assert.AreEqual("투명화 소개", ghost.Memo);
            Assert.AreEqual(1, ghost.Notes.Count);
            Assert.AreEqual("solid", file.FindCustom("c_wall").Col);
        }

        [Test]
        public void Parse_MissingDarkMaskBecomesZeros() {
            var stage = StageImportSample.Load().FindStage("m_exp");
            Assert.AreEqual(12, stage.DarkMask.Count);
            CollectionAssert.DoesNotContain(stage.DarkMask, 1);
            Assert.IsTrue(stage.Dark);
        }

        [TestCase("{\"format\":\"other\",\"version\":3,\"stages\":[]}", "format")]
        [TestCase("{\"version\":3,\"stages\":[]}", "format")]
        [TestCase("{\"format\":\"cursor-stage\",\"version\":2,\"stages\":[]}", "version")]
        [TestCase("{\"format\":\"cursor-stage\",\"stages\":[]}", "version")]
        [TestCase("{\"format\":\"cursor-stage\",\"version\":3,\"stages\":[{\"id\":\"a\",\"w\":2,\"h\":2,\"cells\":[\"\"]}]}", "cells")]
        [TestCase("{\"format\":\"cursor-stage\",\"version\":3,\"stages\":[{\"id\":\"a\",\"w\":1,\"h\":1,\"cells\":[\"\"],\"darkMask\":[0,1]}]}", "darkMask")]
        [TestCase("not json", "JSON")]
        public void Parse_RejectsInvalid(string json, string expectedWord) {
            var e = Assert.Throws<StageFileException>(() => CursorStageParser.Parse(json));
            StringAssert.Contains(expectedWord, e.Message);
        }
    }

    public sealed class StageImportCoordTests {
        [Test]
        public void ToUnity_FlipsY() {
            Assert.AreEqual(new GridPoint(0, 4), CellCoords.ToUnity(0, 0, 5));
            Assert.AreEqual(new GridPoint(3, 0), CellCoords.ToUnity(3, 4, 5));
            Assert.AreEqual(new GridPoint(2, 2), CellCoords.ToUnity(2, 2, 5));
        }

        [Test]
        public void Grid_ReadsToolRowsBottomUp() {
            var stage = StageImportSample.Load().FindStage("m_tut");
            var keys = StageGrids.Keys(stage);
            Assert.AreEqual("ground", keys[0, 0]);
            Assert.AreEqual("spike", keys[3, 0]);
            Assert.AreEqual("start", keys[1, 2]);
            Assert.AreEqual("goal", keys[6, 3]);
            Assert.AreEqual("", keys[0, 4]);
        }
    }
}
