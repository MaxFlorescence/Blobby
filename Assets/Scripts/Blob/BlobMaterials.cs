using System;
using System.Collections.Generic;
using UnityEngine;

public enum BlobMaterials
{
    WATER, LAVA
}

[Flags]
public enum MaterialProperties
{
    CAN_IGNITE     = 0b_0000_0001,
    CAN_EXTINGUISH = 0b_0000_0010,
    GLOWS          = 0b_0000_0100
}

public static class BlobMaterialExtensions
{
    private static readonly Dictionary<BlobMaterials, (Material, Material, MaterialProperties)> map = new()
    {
        {BlobMaterials.WATER, LoadMaterials(
            MaterialProperties.CAN_EXTINGUISH,
            "Blob Materials/WaterJelly"
        )},
        {BlobMaterials.LAVA,  LoadMaterials(
            MaterialProperties.CAN_IGNITE | MaterialProperties.GLOWS,
            "Blob Materials/LavaJelly", "Basic Materials/Flame"
        )},
    };
    public static (Material, Material, MaterialProperties) LoadMaterials(MaterialProperties properties, string name, string dropName = null)
    {
        Material body =  Resources.Load("Materials/" + name, typeof(Material)) as Material;
        Material drop = body;

        if (dropName != null)
        {
            drop =  Resources.Load("Materials/" + dropName, typeof(Material)) as Material;
        }

        return (body, drop, properties);
    }
    public static Material Body(this BlobMaterials blobMaterial)
    {
        return map[blobMaterial].Item1;
    }
    public static Material Drops(this BlobMaterials blobMaterial)
    {
        return map[blobMaterial].Item2;
    }
    public static bool HasProperty(this BlobMaterials blobMaterial, MaterialProperties property)
    {
        return (map[blobMaterial].Item3 & property) > 0;
    }
    public static MaterialProperties GetProperties(this BlobMaterials blobMaterial)
    {
        return map[blobMaterial].Item3;
    }
}