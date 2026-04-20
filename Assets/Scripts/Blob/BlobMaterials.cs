using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     The different materials that a blob can be made of.
/// </summary>
public enum BlobMaterials
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
public enum MaterialProperties
{
    Watery = Can_Extinguish | Transitions_With_Cold | Conductive,
    Firey = Can_Ignite | Glowing,
    Viscous = Sticky | Heavy,
    Icy = Can_Extinguish | Can_Freeze | Solid | Slippery | Transitions_With_Heat,
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
    ///     Dictionary associating each blob material to its body/drop material and its properties.
    /// </summary>
    private static readonly Dictionary<BlobMaterials, (Material, Material, MaterialProperties)> map = new()
    {
        {BlobMaterials.Water, LoadMaterials(
            MaterialProperties.Watery,
            Utilities.BLOB_MATERIALS_PATH + "WaterJelly"
        )},
        {BlobMaterials.Ice, LoadMaterials( // TODO: add body/drop materials
            MaterialProperties.Icy,
            Utilities.MISSING_MATERIAL_PATH
        )},

        {BlobMaterials.Lava,  LoadMaterials(
            MaterialProperties.Firey | MaterialProperties.Transitions_With_Cold,
            Utilities.BLOB_MATERIALS_PATH + "LavaJelly",
            Utilities.OBJECT_MATERIALS_PATH + "Flame"
        )},
        {BlobMaterials.Rock,  LoadMaterials( // TODO: add body/drop materials
            MaterialProperties.Solid | MaterialProperties.Heavy
            | MaterialProperties.Transitions_With_Heat,
            Utilities.MISSING_MATERIAL_PATH
        )},

        {BlobMaterials.Acid, LoadMaterials( // TODO: add body/drop materials
            MaterialProperties.Watery | MaterialProperties.Can_Dissolve,
            Utilities.MISSING_MATERIAL_PATH
        )},
        {BlobMaterials.Frozen_Acid, LoadMaterials( // TODO: add body/drop materials
            MaterialProperties.Icy,
            Utilities.MISSING_MATERIAL_PATH
        )},

        {BlobMaterials.Oil, LoadMaterials( // TODO: add body/drop materials
            MaterialProperties.Oily,
            Utilities.MISSING_MATERIAL_PATH
        )},
        {BlobMaterials.Burning_Oil, LoadMaterials( // TODO: add body/drop materials
            MaterialProperties.Firey | MaterialProperties.Slippery
            | MaterialProperties.Transitions_With_Cold,
            Utilities.MISSING_MATERIAL_PATH
        )},

        {BlobMaterials.Honey, LoadMaterials( // TODO: add body/drop materials
            MaterialProperties.Viscous | MaterialProperties.Sweet
            | MaterialProperties.Transitions_With_Cold | MaterialProperties.Can_Extinguish
            | MaterialProperties.Transitions_With_Heat,
            Utilities.MISSING_MATERIAL_PATH
        )},
        {BlobMaterials.Burning_Honey, LoadMaterials( // TODO: add body/drop materials
            MaterialProperties.Viscous | MaterialProperties.Firey
            | MaterialProperties.Transitions_With_Cold,
            Utilities.MISSING_MATERIAL_PATH
        )},
        {BlobMaterials.Crystal_Honey, LoadMaterials( // TODO: add body/drop materials
            MaterialProperties.Viscous | MaterialProperties.Sweet | MaterialProperties.Solid
            | MaterialProperties.Transitions_With_Heat,
            Utilities.MISSING_MATERIAL_PATH
        )},

        {BlobMaterials.Soda, LoadMaterials( // TODO: add body/drop materials
            MaterialProperties.Watery | MaterialProperties.Sweet | MaterialProperties.Light,
            Utilities.MISSING_MATERIAL_PATH
        )},
        {BlobMaterials.Frozen_Soda, LoadMaterials( // TODO: add body/drop materials
            MaterialProperties.Icy | MaterialProperties.Light,
            Utilities.MISSING_MATERIAL_PATH
        )},

        {BlobMaterials.Liquid_Nitrogen, LoadMaterials( // TODO: add body/drop materials
            MaterialProperties.Can_Extinguish | MaterialProperties.Can_Freeze
            | MaterialProperties.Light,
            Utilities.MISSING_MATERIAL_PATH
        )},

        {BlobMaterials.Ferrofluid, LoadMaterials( // TODO: add body/drop materials
            MaterialProperties.Oily | MaterialProperties.Magnetic | MaterialProperties.Conductive,
            Utilities.MISSING_MATERIAL_PATH
        )},

        {BlobMaterials.Rubber, LoadMaterials( // TODO: add body/drop materials
            MaterialProperties.Solid | MaterialProperties.Bouncy
            | MaterialProperties.Transitions_With_Heat,
            Utilities.MISSING_MATERIAL_PATH
        )},
        {BlobMaterials.Burning_Rubber, LoadMaterials( // TODO: add body/drop materials
            MaterialProperties.Solid | MaterialProperties.Bouncy | MaterialProperties.Firey
            | MaterialProperties.Transitions_With_Cold,
            Utilities.MISSING_MATERIAL_PATH
        )},
    };

    /// <summary>
    ///     Loads the named material properties.
    /// </summary>
    /// <param name="bodyName">
    ///     The name of the body material to load.
    /// </param>
    /// <param name="dropName">
    ///     The name of the droplet material to load. If <tt>null</tt>, this is set to match the
    ///     body material.
    /// </param>
    /// <returns>
    ///     <tt>(Material, Material, MaterialProperties)</tt> (body material, drop material,
    ///     material properties).
    /// </returns>
    private static (Material, Material, MaterialProperties) LoadMaterials(MaterialProperties properties, string bodyName, string dropName = null)
    {
        Material body =  Resources.Load<Material>(bodyName);
        Material drop = body;

        if (dropName != null)
        {
            drop =  Resources.Load<Material>(dropName);
        }

        return (body, drop, properties);
    }

    /// <returns>
    ///     The body material associated with the given blob material.
    /// </returns>
    public static Material Body(this BlobMaterials blobMaterial)
    {
        return map[blobMaterial].Item1;
    }

    /// <returns>
    ///     The droplet material associated with the given blob material.
    /// </returns>
    public static Material Drops(this BlobMaterials blobMaterial)
    {
        return map[blobMaterial].Item2;
    }

    /// <returns>
    ///     The property flags associated with the given blob material.
    /// </returns>
    public static MaterialProperties GetProperties(this BlobMaterials blobMaterial)
    {
        return map[blobMaterial].Item3;
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
    public static bool HasProperty(this BlobMaterials blobMaterial, MaterialProperties properties)
    {
        return (map[blobMaterial].Item3 & properties) > 0;
    }

    /// <summary>
    ///     Associations between pairs of blob materials indicating transitions based on
    ///     temperature. Each element is of the form <tt>(warmerMaterial, colderMaterial)</tt>.
    ///     <para/>
    ///     The <tt>warmerMaterial</tt> has the <tt>MaterialProperties.Transitions_With_Cold</tt>
    ///     property and can transition to the <tt>colderMaterial</tt>.
    ///     <para/>
    ///     The <tt>colderMaterial</tt> has the <tt>MaterialProperties.Transitions_With_Heat</tt>
    ///     property and can transition to the <tt>warmerMaterial</tt>.
    /// </summary>
    private static (BlobMaterials, BlobMaterials)[] TemperatureTransitions = {
        (BlobMaterials.Water,          BlobMaterials.Ice),
        (BlobMaterials.Lava,           BlobMaterials.Rock),
        (BlobMaterials.Acid,           BlobMaterials.Frozen_Acid),
        (BlobMaterials.Burning_Oil,    BlobMaterials.Oil),
        (BlobMaterials.Honey,          BlobMaterials.Crystal_Honey),
        (BlobMaterials.Burning_Honey,  BlobMaterials.Honey),
        (BlobMaterials.Soda,           BlobMaterials.Frozen_Soda),
        (BlobMaterials.Burning_Rubber, BlobMaterials.Rubber)
    };

    /// <summary>
    ///     Determines which other material the given blob material can transition to, based on the
    ///     given transition property.
    /// </summary>
    /// <param name="transitionProperty">
    ///     Ideally one of <tt>MaterialProperties.Transitions_With_Heat</tt> or
    ///     <tt>MaterialProperties.Transitions_With_Cold</tt>. If otherwise, no transition will be
    ///     made.
    /// </param>
    /// <returns>
    ///     The other blob material specified in the <tt>TemperatureTransitions</tt> table if the
    ///     transition property is one of <tt>MaterialProperties.Transitions_With_Heat</tt> or
    ///     <tt>MaterialProperties.Transitions_With_Cold</tt>. Otherwise, returns the initial
    ///     material.
    /// </returns>
    public static BlobMaterials TransistionsTo(this BlobMaterials initialMaterial, MaterialProperties transitionProperty)
    {
        bool forwardTransition = true;
        if (transitionProperty == MaterialProperties.Transitions_With_Heat)
        {
            forwardTransition = false;
        } else if (transitionProperty != MaterialProperties.Transitions_With_Cold)
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