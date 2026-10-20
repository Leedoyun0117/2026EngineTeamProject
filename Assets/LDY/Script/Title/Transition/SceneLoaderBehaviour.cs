using UnityEngine;

namespace LDY.Script
{
    // 인스펙터에서 교체할 수 있게 컴포넌트로 감쌈
    public abstract class SceneLoaderBehaviour : MonoBehaviour, ISceneLoader
    {
        public abstract void LoadGameScene();
    }

    // 본게임 씬이 생기기 전까지 쓰는 임시 구현
    public class LoggingSceneLoader : SceneLoaderBehaviour
    {
        public override void LoadGameScene()
        {
            Debug.Log("[Title] 게임 씬 로드 호출됨(연결된 씬 없음)");
        }
    }
}
