using UnityEngine;

namespace LDY.Script
{
    // 벽이 이 오브젝트 레이어를 그대로 씀(플레이어 ground check 레이어와 맞출 것)
    public class ContentBoundsWalls : MonoBehaviour
    {
        [SerializeField] private Camera contentCamera;
        [SerializeField, Min(0.1f)] private float thickness = 1f;

        private void Start()
        {
            float halfHeight = contentCamera.orthographicSize;
            RenderTexture texture = contentCamera.targetTexture;
            float aspect = texture != null ? (float)texture.width / texture.height : contentCamera.aspect;
            float halfWidth = halfHeight * aspect;
            Vector2 center = contentCamera.transform.position;

            CreateWall("Floor", center + new Vector2(0f, -halfHeight - thickness * 0.5f),
                new Vector2(halfWidth * 2f + thickness * 2f, thickness));
            CreateWall("Ceiling", center + new Vector2(0f, halfHeight + thickness * 0.5f),
                new Vector2(halfWidth * 2f + thickness * 2f, thickness));
            CreateWall("Left", center + new Vector2(-halfWidth - thickness * 0.5f, 0f),
                new Vector2(thickness, halfHeight * 2f));
            CreateWall("Right", center + new Vector2(halfWidth + thickness * 0.5f, 0f),
                new Vector2(thickness, halfHeight * 2f));
        }

        private void CreateWall(string wallName, Vector2 position, Vector2 size)
        {
            var wall = new GameObject(wallName) { layer = gameObject.layer };
            wall.transform.SetParent(transform, false);
            wall.transform.position = position;
            wall.AddComponent<BoxCollider2D>().size = size;
        }
    }
}
