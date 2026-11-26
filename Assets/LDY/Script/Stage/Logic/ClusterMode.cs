namespace LDY.Stage {
    public enum ClusterMode {
        PerCell,
        Fixed,
        StretchHorizontal,
        StretchVertical,
        Connected4
    }

    public enum TileLayerKind {
        Ground,
        Platform,
        Hazard,
        Invisible,
        Decoration
    }

    public enum EntryKind {
        Tile,
        Prefab
    }
}
