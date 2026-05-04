using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
///     The different materials that a blob can be made of.
/// </summary>
public enum BlobMaterial
{
    Water, Ice,
    Lava, Rock,
    Acid, Frozen_Acid,
    Oil, Burning_Oil,
    Honey, Burning_Honey, Crystal_Honey,
    Soda, Frozen_Soda,
    Liquid_Nitrogen,
    Ferrofluid,
    Rubber, Burning_Rubber
}

/// <summary>
///     The different properties that a blob material can have.
/// </summary>
[Flags]
public enum BlobMaterialProperties
{
    Watery = Can_Extinguish | Transitions_With_Cold | Conductive,
    Icy = Can_Extinguish | Can_Freeze | Solid | Slippery | Transitions_With_Heat,
    Firey = Can_Ignite | Glowing | Transitions_With_Cold,
    Viscous = Sticky | Heavy,
    Oily = Transitions_With_Heat | Slippery,
    Can_Ignite            = 0b_0000_0000_0000_0001,
    Can_Extinguish        = 0b_0000_0000_0000_0010,
    Can_Freeze            = 0b_0000_0000_0000_0100,
    Can_Dissolve          = 0b_0000_0000_0000_1000,
    Glowing               = 0b_0000_0000_0001_0000,
    Transitions_With_Heat = 0b_0000_0000_0010_0000,
    Transitions_With_Cold = 0b_0000_0000_0100_0000,
    Conductive            = 0b_0000_0000_1000_0000,
    Magnetic              = 0b_0000_0001_0000_0000,
    Slippery              = 0b_0000_0010_0000_0000,
    Sticky                = 0b_0000_0100_0000_0000,
    Sweet                 = 0b_0000_1000_0000_0000,
    Light                 = 0b_0001_0000_0000_0000,
    Heavy                 = 0b_0010_0000_0000_0000,
    Solid                 = 0b_0100_0000_0000_0000,
    Bouncy                = 0b_1000_0000_0000_0000
}

public static class BlobMaterialExtensions
{
    /// <summary>
    ///     Dictionary associating each blob material to its data.
    /// </summary>
    private static readonly Dictionary<BlobMaterial, BlobMaterialDataStruct> MaterialToData = new()
    {
        {BlobMaterial.Water, new BlobMaterialDataStruct(
            BlobMaterialProperties.Watery,
            Path.Combine(FileUtilities.BLOB_MATERIALS, "WaterJelly")
        )},
        {BlobMaterial.Ice, new BlobMaterialDataStruct( // TODO: add body/drop materials
            BlobMaterialProperties.Icy,
            FileUtilities.MISSING_MATERIAL
        )},

        {BlobMaterial.Lava,  new BlobMaterialDataStruct(
            BlobMaterialProperties.Firey,
            Path.Combine(FileUtilities.BLOB_MATERIALS, "LavaJelly"),
            Path.Combine(FileUtilities.OBJECT_MATERIALS, "Flame")
        )},
        {BlobMaterial.Rock,  new BlobMaterialDataStruct( // TODO: add body/drop materials
            BlobMaterialProperties.Solid | BlobMaterialProperties.Heavy
            | BlobMaterialProperties.Transitions_With_Heat,
            FileUtilities.MISSING_MATERIAL
        )},

        {BlobMaterial.Acid, new BlobMaterialDataStruct( // TODO: add body/drop materials
            BlobMaterialProperties.Watery | BlobMaterialProperties.Can_Dissolve,
            FileUtilities.MISSING_MATERIAL
        )},
        {BlobMaterial.Frozen_Acid, new BlobMaterialDataStruct( // TODO: add body/drop materials
            BlobMaterialProperties.Icy,
            FileUtilities.MISSING_MATERIAL
        )},

        {BlobMaterial.Oil, new BlobMaterialDataStruct( // TODO: add body/drop materials
            BlobMaterialProperties.Oily,
            FileUtilities.MISSING_MATERIAL
        )},
        {BlobMaterial.Burning_Oil, new BlobMaterialDataStruct( // TODO: add body/drop materials
            BlobMaterialProperties.Firey | BlobMaterialProperties.Slippery,
            FileUtilities.MISSING_MATERIAL
        )},

        {BlobMaterial.Honey, new BlobMaterialDataStruct( // TODO: add body/drop materials
            BlobMaterialProperties.Viscous | BlobMaterialProperties.Sweet
            | BlobMaterialProperties.Transitions_With_Cold | BlobMaterialProperties.Can_Extinguish
            | BlobMaterialProperties.Transitions_With_Heat,
            FileUtilities.MISSING_MATERIAL
        )},
        {BlobMaterial.Burning_Honey, new BlobMaterialDataStruct( // TODO: add body/drop materials
            BlobMaterialProperties.Viscous | BlobMaterialProperties.Firey,
            FileUtilities.MISSING_MATERIAL
        )},
        {BlobMaterial.Crystal_Honey, new BlobMaterialDataStruct( // TODO: add body/drop materials
            BlobMaterialProperties.Viscous | BlobMaterialProperties.Sweet | BlobMaterialProperties.Solid
            | BlobMaterialProperties.Transitions_With_Heat,
            FileUtilities.MISSING_MATERIAL
        )},

        {BlobMaterial.Soda, new BlobMaterialDataStruct( // TODO: add body/drop materials
            BlobMaterialProperties.Watery | BlobMaterialProperties.Sweet | BlobMaterialProperties.Light,
            FileUtilities.MISSING_MATERIAL
        )},
        {BlobMaterial.Frozen_Soda, new BlobMaterialDataStruct( // TODO: add body/drop materials
            BlobMaterialProperties.Icy | BlobMaterialProperties.Light,
            FileUtilities.MISSING_MATERIAL
        )},

        {BlobMaterial.Liquid_Nitrogen, new BlobMaterialDataStruct( // TODO: add body/drop materials
            BlobMaterialProperties.Can_Extinguish | BlobMaterialProperties.Can_Freeze
            | BlobMaterialProperties.Light,
            FileUtilities.MISSING_MATERIAL
        )},

        {BlobMaterial.Ferrofluid, new BlobMaterialDataStruct( // TODO: add body/drop materials
            BlobMaterialProperties.Oily | BlobMaterialProperties.Magnetic | BlobMaterialProperties.Conductive,
            FileUtilities.MISSING_MATERIAL
        )},

        {BlobMaterial.Rubber, new BlobMaterialDataStruct( // TODO: add body/drop materials
            BlobMaterialProperties.Solid | BlobMaterialProperties.Bouncy
            | BlobMaterialProperties.Transitions_With_Heat,
            FileUtilities.MISSING_MATERIAL
        )},
        {BlobMaterial.Burning_Rubber, new BlobMaterialDataStruct( // TODO: add body/drop materials
            BlobMaterialProperties.Solid | BlobMaterialProperties.Bouncy | BlobMaterialProperties.Firey,
            FileUtilities.MISSING_MATERIAL
        )},
    };

    /// <returns>
    ///     The body material associated with the given blob material.
    /// </returns>
    public static Material Body(this BlobMaterial blobMaterial)
    {
        return MaterialToData[blobMaterial].bodyMaterial;
    }

    /// <returns>
    ///     The droplet material associated with the given blob material.
    /// </returns>
    public static Material Drops(this BlobMaterial blobMaterial)
    {
        return MaterialToData[blobMaterial].dropletMaterial;
    }

    /// <returns>
    ///     The property flags associated with the given blob material.
    /// </returns>
    public static BlobMaterialProperties GetProperties(this BlobMaterial blobMaterial)
    {
        return MaterialToData[blobMaterial].properties;
    }

    /// <summary>
    ///     Tests if the given blob material has the given properties.
    /// </summary>
    /// <param name="blobMaterial">
    ///     The blob material whose properties to test.
    /// </param>
    /// <param name="properties">
    ///     The properties to test for.
    /// </param>
    /// <returns>
    ///     <tt>True</tt> iff the blob material has all of the requested properties.
    /// </returns>
    public static bool HasProperty(this BlobMaterial blobMaterial, BlobMaterialProperties properties)
    {
        return (MaterialToData[blobMaterial].properties & properties) > 0;
    }

    /// <summary>
    ///     Associations between pairs of blob materials indicating transitions based on
    ///     temperature.
    /// </summary>
    private static readonly BlobMaterialTemperaturePair[] TemperatureTransitions = {
        new(BlobMaterial.Water,          BlobMaterial.Ice),
        new(BlobMaterial.Lava,           BlobMaterial.Rock),
        new(BlobMaterial.Acid,           BlobMaterial.Frozen_Acid),
        new(BlobMaterial.Burning_Oil,    BlobMaterial.Oil),
        new(BlobMaterial.Honey,          BlobMaterial.Crystal_Honey),
        new(BlobMaterial.Burning_Honey,  BlobMaterial.Honey),
        new(BlobMaterial.Soda,           BlobMaterial.Frozen_Soda),
        new(BlobMaterial.Burning_Rubber, BlobMaterial.Rubber)
    };

    /// <summary>
    ///     Determines which other material the given blob material can transition to, based on the
    ///     given transition property.
    /// </summary>
    /// <param name="transitionProperty">
    ///     Ideally one of <tt>MaterialProperties.Transitions_With_Heat</tt> or
    ///     <tt>MaterialProperties.Transitions_With_Cold</tt>. Otherwise, no transition will be
    ///     made.
    /// </param>
    /// <returns>
    ///     The other blob material specified in the <tt>TemperatureTransitions</tt> table if the
    ///     transition property is one of <tt>MaterialProperties.Transitions_With_Heat</tt> or
    ///     <tt>MaterialProperties.Transitions_With_Cold</tt>. Otherwise, returns the initial
    ///     material.
    /// </returns>
    public static BlobMaterial TransistionsTo(this BlobMaterial initialMaterial, BlobMaterialProperties transitionProperty)
    {
        bool forwardTransition = true;
        if (transitionProperty == BlobMaterialProperties.Transitions_With_Heat)
        {
            forwardTransition = false;
        }
        else if (transitionProperty != BlobMaterialProperties.Transitions_With_Cold)
        {
            return initialMaterial;
        }

        foreach (BlobMaterialTemperaturePair transition in TemperatureTransitions)
        {
            if ((forwardTransition ? transition.warmerMaterial : transition.colderMaterial) == initialMaterial)
            {
                return forwardTransition ? transition.colderMaterial : transition.warmerMaterial;
            }
        }

        return initialMaterial;
    }
}