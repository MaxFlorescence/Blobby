using UnityEngine;

/// <summary>
///     A class for generating a randomized dungeon.
/// </summary>
class RandomDungeon : Dungeon
{
    /// <summary>
    ///     The random seed to use for dungeon generation.
    /// </summary>
    public int randomSeed = 0;
    /// <summary>
    ///     The minimum size of a randomly generated dungeon.
    /// </summary>
    public Vector3Int minLayoutDimensions = new(5, 5, 5);
    /// <summary>
    ///     The maximum size of a randomly generated dungeon.
    /// </summary>
    public Vector3Int maxLayoutDimensions = new(20, 20, 20);

    void Start()
    {
        isRandomlyGenerated = true;
        GenerateRandomLayout(minLayoutDimensions, maxLayoutDimensions, randomSeed);
        UpdateActiveLevel(entranceTilePosition.y);
    }
}