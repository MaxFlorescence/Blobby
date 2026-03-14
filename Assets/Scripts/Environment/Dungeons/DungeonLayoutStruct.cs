using System;
using UnityEngine;

[Serializable]
public struct DungeonTileStruct
{
    public string tileName;
    public string tileOrientation;
}

[Serializable]
public struct DungeonLayoutStruct
{
    public string dungeonName;
    public Vector3Int layoutDimensions;
    public Vector3Int rootPosition;
    public DungeonTileStruct[] layout;
}