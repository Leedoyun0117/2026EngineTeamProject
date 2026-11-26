using UnityEngine;

namespace LDY.Script {
    // 제거 대기 스텁. StageSelect.unity의 FileBoard 오브젝트가 아직 이 스크립트(GUID)를 참조한다.
    // LDY/Flow/Build Stage Select Scene을 다시 실행해 씬 참조가 0이 된 뒤 이 파일(.meta 포함)을 삭제한다.
    [AddComponentMenu("")]
    public sealed class StageSelectPlayerBootstrap : MonoBehaviour {
    }
}
