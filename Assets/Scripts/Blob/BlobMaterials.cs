using System.Collections.Generic;
using UnityEngine;

public enum BlobMaterials
{
    WATER,
    LAVA
}

public static class BlobMaterialExtensions
{
    private static readonly Dictionary<BlobMaterials, (Material, Material)> map = new()
    {
        {BlobMaterials.WATER, LoadMaterials("Blob Materials/WaterJelly")},
        {BlobMaterials.LAVA,  LoadMaterials("Blob Materials/LavaJelly", "Basic Materials/Flame")},
    };
    public static (Material, Material) LoadMaterials(string name, string dropName = null)
    {
        Material body =  Resources.Load("Materials/" + name, typeof(Material)) as Material;
        Material drop = body;

        if (dropName != null)
        {
            drop =  Resources.Load("Materials/" + dropName, typeof(Material)) as Material;
        }

        return (body, drop);
    }
    public static Material Body(this BlobMaterials blobMaterial)
    {
        return map[blobMaterial].Item1;
    }
    public static Material Drops(this BlobMaterials blobMaterial)
    {
        return map[blobMaterial].Item2;
    }
}