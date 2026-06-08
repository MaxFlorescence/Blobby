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
    Rubber, Burning_Rubber,
    Aerogel
}

/// <summary>
///     The different properties that a blob material can have.
/// </summary>
[Flags]
public enum BlobMaterialProperties
{
    None            = 0,
    Can_Ignite      = 0b_0000_0000_0000_0000_0001,
    Can_Extinguish  = 0b_0000_0000_0000_0000_0010,
    Can_Freeze      = 0b_0000_0000_0000_0000_0100,
    Can_Dissolve    = 0b_0000_0000_0000_0000_1000,
    Glowing         = 0b_0000_0000_0000_0001_0000,
    Heat_Transition = 0b_0000_0000_0000_0010_0000,
    Cold_Transition = 0b_0000_0000_0000_0100_0000,
    Wet_Transition  = 0b_0000_0000_0000_1000_0000,
    Conductive      = 0b_0000_0000_0001_0000_0000,
    Magnetic        = 0b_0000_0000_0010_0000_0000,
    Non_Stick       = 0b_0000_0000_0100_0000_0000,
    Sticky          = 0b_0000_0000_1000_0000_0000,
    Sweet           = 0b_0000_0001_0000_0000_0000,
    Light           = 0b_0000_0010_0000_0000_0000,
    Heavy           = 0b_0000_0100_0000_0000_0000,
    Solid           = 0b_0000_1000_0000_0000_0000,
    Bouncy          = 0b_0001_0000_0000_0000_0000,
    Low_Friction    = 0b_0010_0000_0000_0000_0000,

    Slippery = Low_Friction | Non_Stick,
    Watery = Can_Extinguish | Cold_Transition | Conductive,
    Icy = Can_Extinguish | Can_Freeze | Solid | Slippery | Heat_Transition,
    Firey = Can_Ignite | Glowing | Wet_Transition | Cold_Transition,
    Slimy = Sticky | Heavy
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
            Path.Combine(FileUtilities.BLOB_MATERIALS, "IceJelly"),
            Path.Combine(FileUtilities.BLOB_MATERIALS, "Invisible")
        )},

        {BlobMaterial.Lava,  new BlobMaterialDataStruct(
            BlobMaterialProperties.Firey,
            Path.Combine(FileUtilities.BLOB_MATERIALS, "LavaJelly"),
            Path.Combine(FileUtilities.OBJECT_MATERIALS, "Flame")
        )},
        {BlobMaterial.Rock,  new BlobMaterialDataStruct( // TODO: add body/drop materials
            BlobMaterialProperties.Solid | BlobMaterialProperties.Heavy
            | BlobMaterialProperties.Non_Stick | BlobMaterialProperties.Heat_Transition,
            Path.Combine(FileUtilities.DUNGEON_MATERIALS, "Dungeon Stone")
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
            BlobMaterialProperties.Heat_Transition | BlobMaterialProperties.Slippery,
            FileUtilities.MISSING_MATERIAL
        )},
        {BlobMaterial.Burning_Oil, new BlobMaterialDataStruct( // TODO: add body/drop materials
            BlobMaterialProperties.Can_Ignite | BlobMaterialProperties.Glowing
            | BlobMaterialProperties.Cold_Transition | BlobMaterialProperties.Slippery,
            FileUtilities.MISSING_MATERIAL
        )},

        {BlobMaterial.Honey, new BlobMaterialDataStruct( // TODO: add body/drop materials
            BlobMaterialProperties.Slimy | BlobMaterialProperties.Sweet
            | BlobMaterialProperties.Cold_Transition | BlobMaterialProperties.Can_Extinguish
            | BlobMaterialProperties.Heat_Transition,
            Path.Combine(FileUtilities.BLOB_MATERIALS, "HoneyJelly")
        )},
        {BlobMaterial.Burning_Honey, new BlobMaterialDataStruct( // TODO: add body/drop materials
            BlobMaterialProperties.Slimy | BlobMaterialProperties.Firey,
            FileUtilities.MISSING_MATERIAL
        )},
        {BlobMaterial.Crystal_Honey, new BlobMaterialDataStruct( // TODO: add body/drop materials
            BlobMaterialProperties.Slimy | BlobMaterialProperties.Sweet
            | BlobMaterialProperties.Solid | BlobMaterialProperties.Heat_Transition,
            FileUtilities.MISSING_MATERIAL
        )},

        {BlobMaterial.Soda, new BlobMaterialDataStruct( // TODO: add body/drop materials
            BlobMaterialProperties.Watery | BlobMaterialProperties.Sweet
            | BlobMaterialProperties.Light,
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
            BlobMaterialProperties.Slippery | BlobMaterialProperties.Magnetic
            | BlobMaterialProperties.Conductive,
            FileUtilities.MISSING_MATERIAL
        )},

        {BlobMaterial.Rubber, new BlobMaterialDataStruct( // TODO: add body/drop materials
            BlobMaterialProperties.Solid | BlobMaterialProperties.Bouncy
            | BlobMaterialProperties.Non_Stick | BlobMaterialProperties.Heat_Transition,
            FileUtilities.MISSING_MATERIAL
        )},
        {BlobMaterial.Burning_Rubber, new BlobMaterialDataStruct( // TODO: add body/drop materials
            BlobMaterialProperties.Solid | BlobMaterialProperties.Bouncy
            | BlobMaterialProperties.Non_Stick | BlobMaterialProperties.Firey,
            FileUtilities.MISSING_MATERIAL
        )},
        
        {BlobMaterial.Aerogel, new BlobMaterialDataStruct( // TODO: add body/drop materials
            BlobMaterialProperties.Solid | BlobMaterialProperties.Light
            | BlobMaterialProperties.Non_Stick | BlobMaterialProperties.Wet_Transition,
            FileUtilities.MISSING_MATERIAL
        )}
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
    public static AtomParticleDataStruct Drops(this BlobMaterial blobMaterial)
    {
        return MaterialToData[blobMaterial].particles;
    }

    /// <returns>
    ///     The property flags associated with the given blob material.
    /// </returns>
    public static BlobMaterialProperties GetProperties(this BlobMaterial blobMaterial)
    {
        return MaterialToData[blobMaterial].properties;
    }

    /// <summary>
    ///     Tests if the given blob material has all of the given properties.
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
    public static bool HasAll(this BlobMaterial blobMaterial, BlobMaterialProperties properties)
    {
        return MaterialToData[blobMaterial].properties.Includes(properties);
    }

    /// <summary>
    ///     Tests if the given blob material has all of the given properties.
    /// </summary>
    /// <param name="other">
    ///     The properties to test for.
    /// </param>
    /// <returns>
    ///     <tt>True</tt> iff the blob material has all of the requested properties.
    /// </returns>
    public static bool Includes(this BlobMaterialProperties properties, BlobMaterialProperties other)
    {
        return (properties & other) == other;
    }

    /// <summary>
    ///     Tests if the given blob material has any of the given properties.
    /// </summary>
    /// <param name="blobMaterial">
    ///     The blob material whose properties to test.
    /// </param>
    /// <param name="properties">
    ///     The properties to test for.
    /// </param>
    /// <returns>
    ///     <tt>True</tt> iff the blob material has any of the requested properties.
    /// </returns>
    public static bool HasAny(this BlobMaterial blobMaterial, BlobMaterialProperties properties)
    {
        return MaterialToData[blobMaterial].properties.Intersects(properties);
    }

    /// <summary>
    ///     Tests if the given blob material has any of the given properties.
    /// </summary>
    /// <param name="other">
    ///     The properties to test for.
    /// </param>
    /// <returns>
    ///     <tt>True</tt> iff the blob material has any of the requested properties.
    /// </returns>
    public static bool Intersects(this BlobMaterialProperties properties, BlobMaterialProperties other)
    {
        return (properties & other) > 0;
    }

    /// <summary>
    ///     Associations between pairs of blob materials indicating transitions based on
    ///     temperature.
    /// </summary>
    private static readonly BlobMaterialTransitionPair[] MaterialTransitions = {
        new(BlobMaterial.Water, BlobMaterialProperties.Cold_Transition, BlobMaterial.Ice),
        new(BlobMaterial.Aerogel, BlobMaterialProperties.Wet_Transition, BlobMaterial.Water),
        new(BlobMaterial.Ice, BlobMaterialProperties.Heat_Transition, BlobMaterial.Water),
        
        new(BlobMaterial.Lava, BlobMaterialProperties.Cold_Transition
                               | BlobMaterialProperties.Wet_Transition, BlobMaterial.Rock),
        new(BlobMaterial.Rock, BlobMaterialProperties.Heat_Transition, BlobMaterial.Lava),
        
        new(BlobMaterial.Acid, BlobMaterialProperties.Cold_Transition, BlobMaterial.Frozen_Acid),
        new(BlobMaterial.Frozen_Acid, BlobMaterialProperties.Heat_Transition, BlobMaterial.Acid),
        
        new(BlobMaterial.Burning_Oil, BlobMaterialProperties.Cold_Transition, BlobMaterial.Oil),
        new(BlobMaterial.Oil, BlobMaterialProperties.Heat_Transition, BlobMaterial.Burning_Oil),
        
        new(BlobMaterial.Honey, BlobMaterialProperties.Cold_Transition, BlobMaterial.Crystal_Honey),
        new(BlobMaterial.Honey, BlobMaterialProperties.Heat_Transition, BlobMaterial.Burning_Honey),
        new(BlobMaterial.Burning_Honey, BlobMaterialProperties.Cold_Transition
                                        | BlobMaterialProperties.Wet_Transition, BlobMaterial.Honey),
        new(BlobMaterial.Crystal_Honey, BlobMaterialProperties.Heat_Transition, BlobMaterial.Honey),
        
        new(BlobMaterial.Soda, BlobMaterialProperties.Cold_Transition, BlobMaterial.Frozen_Soda),
        new(BlobMaterial.Frozen_Soda, BlobMaterialProperties.Heat_Transition, BlobMaterial.Soda),
        
        new(BlobMaterial.Burning_Rubber, BlobMaterialProperties.Cold_Transition, BlobMaterial.Rubber),
        new(BlobMaterial.Rubber, BlobMaterialProperties.Heat_Transition, BlobMaterial.Burning_Rubber)
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
    public static BlobMaterial TransistionsTo(this BlobMaterial fromMaterial, BlobMaterialProperties transitionProperty)
    {
        if (!fromMaterial.HasAll(transitionProperty)) return fromMaterial;

        foreach (BlobMaterialTransitionPair transition in MaterialTransitions)
        {
            if (transition.fromMaterial == fromMaterial
                && transition.transitionProperties.Includes(transitionProperty))
            {
                return transition.toMaterial;
            }
        }

        return fromMaterial;
    }
}