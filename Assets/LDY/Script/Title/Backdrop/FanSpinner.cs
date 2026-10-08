using UnityEngine;

namespace LDY.Script
{
    public class FanSpinner : MonoBehaviour
    {
        [SerializeField] private TitleClock clock;
        [SerializeField] private float degreesPerSecond = 540f;

        private void Update()
        {
            transform.Rotate(0f, 0f, -degreesPerSecond * clock.Delta);
        }
    }
}
