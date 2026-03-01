using System;
using System.Collections.Generic;
using UnityEngine;

public enum BlobMaterials
{
    WATER, ICE,
    LAVA, ROCK,
    ACID, FROZEN_ACID,
    OIL, BURNING_OIL,
    HONEY, BURNING_HONEY, CRYSTAL_HONEY,
    SODA, FROZEN_SODA,
    NITROGEN,
    FERROFLUID,
    RUBBER, BURNING_RUBBER
}

[Flags]
public enum MaterialProperties
{
    WATERY = CAN_EXTINGUISH | FREEZABLE | CONDUCTIVE,
    FIREY = CAN_IGNITE | GLOWING,
    VISCOUS = STICKY | HEAVY,
    ICY = CAN_EXTINGUISH | CAN_FREEZE | SOLID | SLIPPERY | FLAMMABLE,
    OILY = FLAMMABLE | SLIPPERY,
    CAN_IGNITE     = 0b_0000_0000_0000_0001,
    CAN_EXTINGUISH = 0b_0000_0000_0000_0010,
    CAN_FREEZE     = 0b_0000_0000_0000_0100,
    CAN_DISSOLVE   = 0b_0000_0000_0000_1000,
    GLOWING        = 0b_0000_0000_0001_0000,
    FLAMMABLE      = 0b_0000_0000_0010_0000,
    FREEZABLE      = 0b_0000_0000_0100_0000,
    CONDUCTIVE     = 0b_0000_0000_1000_0000,
    MAGENETIC      = 0b_0000_0001_0000_0000,
    SLIPPERY       = 0b_0000_0010_0000_0000,
    STICKY         = 0b_0000_0100_0000_0000,
    SWEET          = 0b_0000_1000_0000_0000,
    LIGHT          = 0b_0001_0000_0000_0000,
    HEAVY          = 0b_0010_0000_0000_0000,
    SOLID          = 0b_0100_0000_0000_0000,
    BOUNCY         = 0b_1000_0000_0000_0000
}

public static class BlobMaterialExtensions
{
    private static readonly Dictionary<BlobMaterials, (Material, Material, MaterialProperties)> map = new()
    {
        {BlobMaterials.WATER, LoadMaterials(
            MaterialProperties.WATERY,
            "Blob Materials/WaterJelly"
        )},
        {BlobMaterials.ICE, LoadMaterials(
            MaterialProperties.ICY,
            "Blob Materials/WaterJelly"
        )},

        {BlobMaterials.LAVA,  LoadMaterials(
            MaterialProperties.FIREY | MaterialProperties.FREEZABLE,
            "Blob Materials/LavaJelly", "Basic Materials/Flame"
        )},
        {BlobMaterials.ROCK,  LoadMaterials(
            MaterialProperties.SOLID | MaterialProperties.HEAVY | MaterialProperties.FLAMMABLE,
            "Blob Materials/LavaJelly", "Basic Materials/Flame"
        )},

        {BlobMaterials.ACID, LoadMaterials(
            MaterialProperties.WATERY | MaterialProperties.CAN_DISSOLVE,
            "Blob Materials/WaterJelly"
        )},
        {BlobMaterials.FROZEN_ACID, LoadMaterials(
            MaterialProperties.ICY,
            "Blob Materials/WaterJelly"
        )},

        {BlobMaterials.OIL, LoadMaterials(
            MaterialProperties.OILY,
            "Blob Materials/WaterJelly"
        )},
        {BlobMaterials.BURNING_OIL, LoadMaterials(
            MaterialProperties.FIREY | MaterialProperties.SLIPPERY | MaterialProperties.FREEZABLE,
            "Blob Materials/WaterJelly"
        )},

        {BlobMaterials.HONEY, LoadMaterials(
            MaterialProperties.VISCOUS | MaterialProperties.SWEET | MaterialProperties.FREEZABLE
            | MaterialProperties.CAN_EXTINGUISH | MaterialProperties.FLAMMABLE,
            "Blob Materials/WaterJelly"
        )},
        {BlobMaterials.BURNING_HONEY, LoadMaterials(
            MaterialProperties.VISCOUS | MaterialProperties.FIREY | MaterialProperties.FREEZABLE,
            "Blob Materials/WaterJelly"
        )},
        {BlobMaterials.CRYSTAL_HONEY, LoadMaterials(
            MaterialProperties.VISCOUS | MaterialProperties.SWEET | MaterialProperties.SOLID
            | MaterialProperties.FLAMMABLE,
            "Blob Materials/WaterJelly"
        )},

        {BlobMaterials.SODA, LoadMaterials(
            MaterialProperties.WATERY | MaterialProperties.SWEET | MaterialProperties.LIGHT,
            "Blob Materials/WaterJelly"
        )},
        {BlobMaterials.FROZEN_SODA, LoadMaterials(
            MaterialProperties.ICY | MaterialProperties.LIGHT,
            "Blob Materials/WaterJelly"
        )},

        {BlobMaterials.NITROGEN, LoadMaterials(
            MaterialProperties.CAN_EXTINGUISH | MaterialProperties.CAN_FREEZE
            | MaterialProperties.LIGHT,
            "Blob Materials/WaterJelly"
        )},

        {BlobMaterials.FERROFLUID, LoadMaterials(
            MaterialProperties.OILY | MaterialProperties.MAGENETIC | MaterialProperties.CONDUCTIVE,
            "Blob Materials/WaterJelly"
        )},

        {BlobMaterials.RUBBER, LoadMaterials(
            MaterialProperties.SOLID | MaterialProperties.BOUNCY | MaterialProperties.FLAMMABLE,
            "Blob Materials/WaterJelly"
        )},
        {BlobMaterials.BURNING_RUBBER, LoadMaterials(
            MaterialProperties.SOLID | MaterialProperties.BOUNCY | MaterialProperties.FIREY
            | MaterialProperties.FREEZABLE,
            "Blob Materials/WaterJelly"
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
    private static (BlobMaterials, BlobMaterials)[] TemperatureTransitions = {
        (BlobMaterials.WATER,          BlobMaterials.ICE),
        (BlobMaterials.LAVA,           BlobMaterials.ROCK),
        (BlobMaterials.ACID,           BlobMaterials.FROZEN_ACID),
        (BlobMaterials.BURNING_OIL,    BlobMaterials.OIL),
        (BlobMaterials.HONEY,          BlobMaterials.CRYSTAL_HONEY),
        (BlobMaterials.BURNING_HONEY,  BlobMaterials.HONEY),
        (BlobMaterials.SODA,           BlobMaterials.FROZEN_SODA),
        (BlobMaterials.BURNING_RUBBER, BlobMaterials.RUBBER)
    };
    public static BlobMaterials TransistionsTo(this BlobMaterials initialMaterial, MaterialProperties transitionProperty)
    {
        bool forwardTransition = true;
        if (transitionProperty == MaterialProperties.FLAMMABLE)
        {
            forwardTransition = false;
        } else if (transitionProperty != MaterialProperties.FREEZABLE)
        {
            return initialMaterial;
        }

        foreach ((BlobMaterials, BlobMaterials) transition in TemperatureTransitions)
        {
            if ((forwardTransition ? transition.Item1 : transition.Item2) == initialMaterial)
            {
                return forwardTransition ? transition.Item2 : transition.Item1;
            }
        }

        return initialMaterial;
    }
}