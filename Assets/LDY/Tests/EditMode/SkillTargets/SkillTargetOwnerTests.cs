using LDY.SkillTargets.Core;
using NUnit.Framework;

namespace LDY.Tests {
    public sealed class SkillTargetOwnerTests {
        [TestCase("Assets/LDY/Prefabs/Stage1/Stage1_CutBlock.prefab", FolderOwner.Ldy, ApplyAccess.Direct)]
        [TestCase("Assets/Main/02.Prefabs/Obj/Box.prefab", FolderOwner.Main, ApplyAccess.Direct)]
        [TestCase("Assets/HES/prefab/Elevator.prefab", FolderOwner.Hes, ApplyAccess.NeedsConfirmation)]
        [TestCase("Assets/JJB/Prefab/RotateBlock.prefab", FolderOwner.Jjb, ApplyAccess.NeedsConfirmation)]
        [TestCase("Assets/HTY/02.Prefabs/Obj/Elevator.prefab", FolderOwner.Other, ApplyAccess.ReadOnly)]
        [TestCase("Assets/prefab/Player.prefab", FolderOwner.Other, ApplyAccess.ReadOnly)]
        [TestCase("Assets\\Main\\02.Prefabs\\Obj\\Box.prefab", FolderOwner.Main, ApplyAccess.Direct)]
        [TestCase("Packages/LDY/x.prefab", FolderOwner.Other, ApplyAccess.ReadOnly)]
        [TestCase("Assets/LDYExtra/x.prefab", FolderOwner.Other, ApplyAccess.ReadOnly)]
        [TestCase("", FolderOwner.Other, ApplyAccess.ReadOnly)]
        [TestCase(null, FolderOwner.Other, ApplyAccess.ReadOnly)]
        public void Resolve_ByTopFolder(string path, FolderOwner owner, ApplyAccess access) {
            Assert.AreEqual(owner, FolderOwners.Resolve(path));
            Assert.AreEqual(access, FolderOwners.Access(FolderOwners.Resolve(path)));
        }

        [TestCase("Box*", "BoxStage2", true)]
        [TestCase("*box*", "Stage1_BoxCdCase", true)]
        [TestCase("Box", "BoxStage2", false)]
        [TestCase("Stage1_?ut*", "Stage1_CutBlock", true)]
        [TestCase("", "Box", false)]
        public void NamePattern_Glob(string pattern, string name, bool expected) {
            Assert.AreEqual(expected, NamePattern.Matches(pattern, name));
        }
    }
}
