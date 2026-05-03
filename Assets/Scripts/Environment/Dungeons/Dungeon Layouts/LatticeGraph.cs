using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
///     A class for creating 3D graphs where each node is at a lattice point for use in generating
///     dungeon layouts.
/// </summary>
public class LatticeGraph
{
    // ---------------------------------------------------------------------------------------------
    // PARAMETERS
    // ---------------------------------------------------------------------------------------------
    /// <summary>
    ///     The x,y,z sizes of the lattice graph's dimensions.
    /// </summary>
    public readonly Vector3Int layoutDimensions;
    /// <summary>
    ///     Counts how many walls on the current level are not set.
    /// </summary>
    public int unsetWalls { get; private set; }
    /// <summary>
    ///     The number of set nodes to wait for before trying to connect to the next layer down in
    ///     the lattice.
    /// </summary>
    private readonly int STAIR_THRESHOLD;
    /// <summary>
    ///     The probability that an edge will be set during each level's generation that creates a
    ///     loop.  
    /// </summary>
    private readonly float RECONNECT_CHANCE;

    // ---------------------------------------------------------------------------------------------
    // ACCESS
    // ---------------------------------------------------------------------------------------------
    /// <summary>
    ///     Contains connectivity information for each node of the lattice graph.
    /// </summary>
    private Walls[,,] walls;
    /// <returns>
    ///     The connectivity information for the node at the given (x, y, z) position.
    /// </returns>
    public Walls this[int x, int y, int z] {
        private set => walls[x, y, z] = value;
        get => walls[x, y, z];
    }
    /// <returns>
    ///     The connectivity information for the given node.
    /// </returns>
    public Walls this[Vector3Int node] {
        private set => walls[node.x, node.y, node.z] = value;
        get => walls[node.x, node.y, node.z];
    }

    /// <summary>
    ///     Creates a new lattice graph of size specified by the given dimensions and rooted at the
    ///     given position.
    /// </summary>
    /// <param name="layoutDimensions">
    ///     The bounds for each dimension of the lattice graph.
    /// </param>
    /// <param name="rootPosition">
    ///     The lattice position that is to be considered the graph's origin.
    /// </param>
    /// <param name="stairTime">
    ///     The point at which to try connecting to the next layer down in the lattice, as a
    ///     proportion of the set nodes on the current layer.
    /// </param>
    /// <param name="reconnectChance">
    ///     The probability that an edge will be set during each level's generation that creates a
    ///     loop. 
    /// </param>
    public LatticeGraph(Vector3Int layoutDimensions, Vector3Int rootPosition,
        float stairTime = 0.5f, float reconnectChance = 0.1f)
    {
        // TODO: allow for 3D random layouts alongside 2D random layouts connected by stairs?
        this.layoutDimensions = layoutDimensions;
        walls = new Walls[layoutDimensions.x, layoutDimensions.y, layoutDimensions.z];

        STAIR_THRESHOLD = (int)(stairTime * layoutDimensions.x * layoutDimensions.z);
        RECONNECT_CHANCE = reconnectChance;
        
        GenerateRandomLayout(rootPosition);
        // SaveLayoutArt("printed_layout.txt"); // debugging
    }

    /// <summary>
    ///     Resets the tally for a floor's unset walls and redefines the threshold at which a stairs
    ///     down will be generated.
    /// </summary>
    private void ResetUnsetWallCount()
    {
        unsetWalls = layoutDimensions.x * layoutDimensions.z;
    }

    /// <summary>
    ///     Sets the node at the given (x, y, z) position and prevents it from being modified again.
    /// </summary>
    /// <param name="clear">
    ///     Resets all connections for the node at the given (x, y, z) position iff <tt>true</tt>.
    /// </param>
    private void Lock(int x, int y, int z, bool clear = false)
    {
        if (!this[x, y, z].IsSet())
        {
            unsetWalls--;
        }

        Walls baseWalls = clear ? Walls.Zero : this[x, y, z];

        this[x, y, z] = baseWalls | Walls.Locked | Walls.Set;
    }
    
    /// <summary>
    ///     Sets the given node and prevents it from being modified again.
    /// </summary>
    /// <param name="clear">
    ///     Resets all connections for the node iff <tt>true</tt>.
    /// </param>
    private void Lock(Vector3Int node, bool clear = false)
    {
        Lock(node.x, node.y, node.z, clear);
    }

    /// <summary>
    ///     Sets the given node as an unconnected, locked node, updating its neighbor nodes as well.
    /// </summary>
    private void SetAsEmpty(Vector3Int node)
    {
        Lock(node, true);
        foreach (Vector3Int dir in Vector3IntExtensions.Directions(true))
        {
            Vector3Int neighbor = node + dir;
            try {
                if (this[neighbor].IsSet() && !this[neighbor].IsLocked()) {
                    this[neighbor] |= (-dir).GetWall();
                }
            }
            catch { /* continue */ }
        }
    }

    /// <summary>
    ///     Attempts to connect the <tt>fromNode</tt> to its neighbor node in the given direction.
    /// </summary>
    /// <param name="direction">
    /// </param>
    /// <returns>
    ///     <tt>True</tt> iff the connection was successfully made.
    /// </returns>
    public bool Connect(Vector3Int fromNode, Vector3Int direction)
    {
        // check bounds
        if ((fromNode + direction).OutOfBounds(layoutDimensions - Vector3Int.one))
        {
            return false;
        }

        // check if the connection already exists
        if (this[fromNode].IsSet() && !this[fromNode].HasWalls(direction.GetWall()))
        {
            return false;
        }

        Vector3Int toNode = fromNode + direction;
        // don't modify locked nodes
        if (this[fromNode].IsLocked() || this[toNode].IsLocked())
        {
            return false;
        }
        
        // remove the wall between the connection on the from-side.
        if (!this[fromNode].IsSet()) {
            this[fromNode] = Walls.All;
            unsetWalls--;
        }
        this[fromNode] &= ~direction.GetWall();

        // remove the wall between the connection on the to-side.
        if (!this[toNode].IsSet()) {
            this[toNode] = Walls.All;
            unsetWalls--;
        }
        this[toNode] &= ~(-direction).GetWall();

        return true;
    }

    /// <summary>
    ///     Randomly form connections between the <tt>fromNode</tt> and its neighbors by considering
    ///     one neighbor at a time.
    /// </summary>
    /// <param name="connectChance">
    ///     A function defining the probability that an attempted connection will succeed, taking as
    ///     input the amount of connections made so far, the amount of connections remaining to
    ///     attempt, and the current attempt's connection direction.
    ///     <br/>
    ///     One additional connection attempt will be made at the end with both counts equal to 0 if
    ///     no connection succeeded in the initial loop.
    /// </param>
    /// <param name="newConnections">
    ///     The set in which to place the nodes that have been newly connected to <tt>fromNode</tt>.
    /// </param>
    /// <param name="planarOnly">
    ///     If <tt>true</tt>, attempt connections using only the 4 planar cardinal directions.
    ///     Otherwise use all 6 cardinal directions.
    /// </param>
    public void ConnectRandom(Vector3Int fromNode, Func<int, int, Vector3Int, float> connectChance,
        HashSet<Vector3Int> newConnections = null, bool planarOnly = true)
    {
        int connected = 0;
        int remaining = planarOnly ? 4 : 6;
        Vector3Int? rejectedDir = null;

        foreach (Vector3Int dir in Vector3IntExtensions.Directions(planarOnly).Shuffled())
        {
            Vector3Int to = fromNode + dir;
            bool noLoops = true;
            
            try {
                if (to.OutOfBounds(layoutDimensions - Vector3Int.one))
                {
                    continue;
                }

                if (this[to].IsSet())
                {
                    // should the connection proceed, possibly creating a loop?
                    if (Random.Range(0f, 1f) < RECONNECT_CHANCE)
                    {
                        noLoops = false;
                    } else {
                        continue;
                    }
                }
                
                if (Random.Range(0f, 1f) < connectChance(connected, remaining, dir))
                {
                    // try making the connection
                    if (Connect(fromNode, dir) && noLoops) {
                        newConnections?.Add(to);
                        connected++;
                    }
                }
                else {
                    rejectedDir = dir;
                }
            }
            catch
            { /* continue */ }
            finally {
                remaining--;
            }
        }

        // try one last time if all attempts failed and at least one was tried
        if (connected == 0 && rejectedDir != null)
        {
            Vector3Int dir = (Vector3Int)rejectedDir;
            if (Random.Range(0f, 1f) < connectChance(0, 0, dir)
                && Connect(fromNode, dir))
            {
                newConnections?.Add(fromNode + dir);
            }
        }
    }

    /// <summary>
    ///     Search for a node on the given y level from which connections can potentially continue.
    ///     <br/>
    ///     This is a bridge node, a set and unlocked node that is adjacent to an unset node.
    /// </summary>
    /// <param name="y">
    ///     The y-level to search for a bridge node on.
    /// </param>
    /// <param name="planarOnly">
    ///     If <tt>true</tt>, look for bridge points using only the 4 planar cardinal directions.
    ///     Otherwise use all 6 cardinal directions.
    /// </param>
    /// <returns>
    ///     The first bridge node found, or null if none is found.
    /// </returns>
    public Vector3Int? FindBridgeNode(int y, bool planarOnly = true)
    {
        foreach(Vector2Int index in layoutDimensions.Indices2D()) {
            if (this[index.x, y, index.y].IsSet()) continue;

            foreach (Vector3Int direction in Vector3IntExtensions.Directions(planarOnly).Shuffled())
            {
                Vector3Int bridge = direction + new Vector3Int(index.x, y, index.y);
                if (!bridge.OutOfBounds(layoutDimensions - Vector3Int.one)
                    && this[bridge].IsSet() && !this[bridge].IsLocked())
                {
                    return bridge;
                }
            }
        }

        return null;
    }

    /// <param name="y">
    ///     The y-level to get the bridge node from.
    /// </param>
    /// <returns>
    ///     The first node on the given y level from which connections can potentially continue.
    ///     <br/>
    ///     This is a bridge node, a set and unlocked node that is adjacent to an unset node.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if a bridge node does not exist.
    /// </exception>
    public Vector3Int GetBridgeNode(int y)
    {
        return FindBridgeNode(y) ?? throw new InvalidOperationException(
            $"Expected to find a bridge node, but none existed (UnsetWalls = {unsetWalls})"
        );
    }

    /// <summary>
    ///     Randomly interconnects the lattice graph's nodes such that each level has exactly one
    ///     connection to the level above it and one to the level below it.
    /// </summary>
    /// <param name="root">
    ///     The position of the entrance node on the top level.
    /// </param>
    public void GenerateRandomLayout(Vector3Int root)
    {
        int topLevel = layoutDimensions.y - 1;
        root.y = topLevel;

        Vector3Int? entrance = root;
        Vector3Int? direction = null;

        for (int y = topLevel; y >= 0; y--) {
            (entrance, direction) = GenerateRandomLevel((Vector3Int)entrance, direction);
        }
    }

    /// <summary>
    ///     Randomly interconnects one level of the lattice graph.
    ///     <para/>
    ///     This method uses a random walk with multiple "heads" to connect nodes. One head is born
    ///     at the node neighboring the entrance position, in the entrance direction. Steps are
    ///     performed until all heads are dead. At each step, a head can:<br/>
    ///     - Survive/multiply by randomly creating new connections to adjacent nodes<br/>
    ///     - Die if it fails to create any new connections<br/>
    ///     - Ressurect if all heads are dead but some nodes remain unset.
    /// </summary>
    /// <param name="entrancePosition">
    ///     The position of the connection between this level and the one above it.
    /// </param>
    /// <param name="entranceDirection">
    ///     Which direction to start the generation in.
    /// </param>
    /// <returns>
    ///     The entrance position and direction for the next level down.
    /// </returns>
    private (Vector3Int?, Vector3Int?) GenerateRandomLevel(Vector3Int entrancePosition, Vector3Int? entranceDirection = null) {
        ResetUnsetWallCount();
        (Vector3Int?, Vector3Int?) lowerEntranceInfo = (null, null);

        HashSet<Vector3Int> heads = new() { AddEntrance(entrancePosition, entranceDirection) };
        HashSet<Vector3Int> newHeads;

        // randomly connect the rest of the nodes
        while (heads.Count > 0)
        {
            // advance the heads' walks by one step
            newHeads = new();
            foreach (Vector3Int head in heads)
            {
                ConnectRandom(head,
                    (_, remaining, _) => { return (remaining <= 1) ? 1f : 1f / remaining; },
                newHeads);
            }
            heads = newHeads;

            // connect to lower level at the first moment at least half of the nodes are set
            if (entrancePosition.y > 0
                && lowerEntranceInfo == (null, null)
                && unsetWalls <= STAIR_THRESHOLD)
            {
                lowerEntranceInfo = AddRandomStairsDown(entrancePosition.y);
            }

            // ressurect a head if all have died but there are still unset nodes
            if (heads.Count == 0 && unsetWalls > 0)
            {
                heads.Add(GetBridgeNode(entrancePosition.y));
            }
        }

        return lowerEntranceInfo;
    }

    /// <summary>
    ///     Creates a connection starting from the given entrance position to the level above it.
    ///     The connection is directional: dictated by the given entrance direction, or chosen
    ///     randomly if it is <tt>null</tt>. 
    /// </summary>
    /// <param name="entrancePosition">
    ///     The node that will have an upwards connection created.
    /// </param>
    /// <param name="entranceDirection">
    ///     The direction that the entrance points in. Chosen randomly if <tt>null</tt>.
    /// </param>
    /// <returns>
    ///     The node neighboring the entrance position, in the entrance's direction.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if the entrance could not be created.
    /// </exception>
    private Vector3Int AddEntrance(Vector3Int entrancePosition, Vector3Int? entranceDirection = null)
    {
        foreach (Vector3Int dir in Vector3IntExtensions.Directions(true).Shuffled())
        {
            // force the direction choice if the given direction is not null
            if (entranceDirection != null && entranceDirection != dir) {
                continue;
            }

            try
            {
                if (Connect(entrancePosition, dir)) {
                    this[entrancePosition] &= ~Walls.Up;
                    Lock(entrancePosition);
                    SetAsEmpty(entrancePosition - dir);

                    return entrancePosition + dir;
                }
            } catch { /* continue */ }
        }

        throw new InvalidOperationException(
            $"[AddEntrance] Could not add an entrance at {entrancePosition}!"
        );
    }

    private (Vector3Int, Vector3Int) AddRandomStairsDown(int y)
    {
        HashSet<(Vector3Int, Vector3Int)> foundSpots = FindStairLocations(y);

        // randomly iterate through the indices found 
        (Vector3Int, Vector3Int)[] possibleSpots = foundSpots.ToArray();
        foreach ((Vector3Int stairsDownIndex, Vector3Int stairDir) in possibleSpots.Shuffled())
        {
            // make sure that the node immediately after the downward connection is in the dungeon
            Vector3Int exit = stairsDownIndex + Vector3Int.down + 2*stairDir;
            if (!exit.OutOfBounds(layoutDimensions - Vector3Int.one)) {
                if(Connect(stairsDownIndex, -stairDir))
                {
                    this[stairsDownIndex] &= ~Walls.Down;
                    Lock(stairsDownIndex);

                    SetAsEmpty(stairsDownIndex + stairDir);

                    return (stairsDownIndex + stairDir + Vector3Int.down, stairDir);
                }
            }
        }

        throw new InvalidOperationException(
            $"[AddRandomStairsDown] Could not add stairs down from level {y}!"
        );
    }

    /// <summary>
    ///     Searches the given y level for all nodes that can potentially have a connection down to
    ///     the lower level.
    /// </summary>
    /// <param name="y">
    ///     The level to search.
    /// </param>
    /// <returns>
    ///     A set of (node, direction) pairs such that the node and its direction-neighbor are both
    ///     unset and the node's (-direction)-neighbor is set.
    /// </returns>
    private HashSet<(Vector3Int, Vector3Int)> FindStairLocations(int y)
    {
        HashSet<(Vector3Int, Vector3Int)> foundSpots = new();

        // 
        foreach (Vector2Int index2D in layoutDimensions.Indices2D())
        {
            if (this[index2D.x, y, index2D.y].IsSet()) continue;
            Vector3Int index = new(index2D.x, y, index2D.y);

            foreach (Vector3Int axis in Vector3IntExtensions.Directions(true))
            {
                try
                {
                    if (!this[index + axis].IsSet() & this[index - axis].IsSet()) {
                        foundSpots.Add((index, axis));
                    }
                } catch { /* continue */ }
            }
        }

        return foundSpots;
    }

    /// <summary>
    ///     Writes to the given file an Ascii art representation of the lattice graph.
    /// </summary>
    private void SaveLayoutArt(string fileName)
    {
        string layout = "↑ Front\n\n";

        for (int y = layoutDimensions.y-1; y >= 0; y--)
        {
            layout += $"Level {y}\n";
            for (int z = layoutDimensions.z-1; z >= 0; z--)
            {
                for (int x = 0; x < layoutDimensions.x; x++) {
                    layout += this[x, y, z].ToChar();
                }
                layout += '\n';
            }
            layout += '\n';
        }

        File.WriteAllText("Ignore/" + fileName, layout);
    }
}