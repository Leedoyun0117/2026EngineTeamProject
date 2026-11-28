using UnityEngine;

namespace LDY.Background {
    public static class BackgroundSorting {
        public const string PreferredLayer = "Background";
        public const string FallbackLayer = "Default";
        public const int Desktop = -110;
        public const int Static = -100;
        public const int Widget = -90;
        public const int Anim = -80;
        public const int Text = -70;

        public static string LayerName {
            get {
                foreach (var layer in SortingLayer.layers)
                    if (layer.name == PreferredLayer)
                        return PreferredLayer;
                return FallbackLayer;
            }
        }
    }
}
