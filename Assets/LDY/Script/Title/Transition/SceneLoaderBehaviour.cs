using UnityEngine;

namespace LDY.Script
{
    // ISceneLoader를 인스펙터에서 교체할 수 있게 하는 컴포넌트 래퍼.
    public abstract class SceneLoaderBehaviour : MonoBehaviour, ISceneLoader
    {
        public abstract void LoadGameScene();
    }

    // 게임 시작 씬이 생기기 전까지 쓰는 임시 구현.
    public class LoggingSceneLoader : SceneLoaderBehaviour
    {
        public override void LoadGameScene()
        {
            Debug.Log("[Title] 게임 시작 화면 로드 훅 호출 (연결된 씬 없음)");
        }
    }
}
