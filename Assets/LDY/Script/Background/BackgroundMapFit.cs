using LDY.Background.Core;
using UnityEngine;

namespace LDY.Background {
    public sealed class BackgroundMapFit : MonoBehaviour {
        public const string DesktopName = "Desktop";

        [SerializeField] private BackgroundFitMode mode = BackgroundFitMode.FixedScale;
        [SerializeField, Min(1)] private int fixedScale = MapFit.DefaultFixedScale;
        [SerializeField] private BackgroundAnchor anchor = BackgroundAnchor.BottomLeft;
        [SerializeField] private Rect fittedBounds;
        [SerializeField] private int scale;
        [SerializeField] private string boundsSource;

        public BackgroundFitMode Mode => mode;
        public int FixedScale => fixedScale;
        public BackgroundAnchor Anchor => anchor;
        public Rect FittedBounds => fittedBounds;
        public int Scale => scale;
        public string BoundsSource => boundsSource;

        public void Record(Rect bounds, int fittedScale, string source) {
            fittedBounds = bounds;
            scale = fittedScale;
            boundsSource = source;
        }
    }
}
