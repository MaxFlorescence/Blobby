using UnityEngine;

/// <summary>
///     A class for generating a dungeon specified in a layout file.
/// </summary>
class LoadedDungeon : Dungeon
{
    /// <summary>
    ///     The file to read the layout from. The contents of the layout file are not case 
    ///     sensitive.
    ///     <para/>
    ///     The following is an example file layout for a 2x2x4 dungeon with entrance at position 0,0,0:
    ///     <code>
    ///     line  1 | 2 2 3 # layout x,y,z dimensions
    ///     line  2 | 0 0 0 # entrance x,y,z position
    ///     line  3 | stairs_up forward # 0,0,0 (left-top-front position)
    ///     line  4 | hallway forward   # 0,0,1
    ///     line  5 | hallway forward   # 0,0,2
    ///     line  6 | corner right      # 0,0,3
    ///     line  7 | none forward      # 1,0,0
    ///     line  8 | none forward      # 1,0,1
    ///     line  9 | stairs_down back  # 1,0,2
    ///     line 10 | corner forward    # 1,0,3
    ///     line 11 | corner back       # 0,1,0 (begin next floor down)
    ///     line 12 | hallway forward   # 0,1,1
    ///     line 13 | junction right    # 0,1,2
    ///     line 14 | dead_end forward  # 0,1,3
    ///     line 15 | corner left       # 1,1,0
    ///     line 16 | stairs_up back    # 1,1,1
    ///     line 17 | corner left       # 1,1,2
    ///     line 18 | dead_end forward  # 1,1,3
    ///     </code>
    /// </summary>
    public TextAsset layoutFile;

    void Start()
    {
        isRandomlyGenerated = false;
        GenerateLayoutFromFile(layoutFile);
        UpdateActiveLevel(entranceTilePosition.y);
    }
}