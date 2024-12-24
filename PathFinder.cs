using System;
using System.Collections.Generic;
using System.Linq;
using ExileCore2.PoEMemory.Elements.Sanctum;
using Vector2 = System.Numerics.Vector2;
using ExileCore2;
using ExileCore2.PoEMemory.Components;

namespace PathfindSanctum;

public class PathFinder
{
    private readonly List<List<SanctumRoomElement>> roomsByLayer;
    private readonly byte[][][] roomLayout;
    private readonly WeightCalculator weightCalculator;
    private readonly Graphics graphics;
    private readonly double[,] roomWeights;
    private readonly PathfindSanctumSettings settings;
    private readonly Dictionary<(int, int), string> debugTexts = new();
    private readonly SanctumStateTracker sanctumStateTracker;
    
    public readonly int PlayerLayerIndex = -1;
    public readonly int PlayerRoomIndex = -1;

    public PathFinder(
        SanctumFloorWindow floorWindow, 
        Graphics graphics,
        GameController gameController, 
        PathfindSanctumSettings settings,
        SanctumStateTracker stateTracker)
    {
        this.graphics = graphics;
        this.roomsByLayer = floorWindow.RoomsByLayer;
        this.roomLayout = floorWindow.FloorData.RoomLayout;
        this.settings = settings;
        this.weightCalculator = new WeightCalculator(gameController, settings);
        this.sanctumStateTracker = stateTracker;
        
        roomWeights = new double[roomsByLayer.Count, roomsByLayer.Max(x => x.Count)];

        // Find player position
        PlayerLayerIndex = floorWindow.FloorData.RoomChoices.Count - 1;
        if (floorWindow.FloorData.RoomChoices.Count > 0)
        {
            PlayerRoomIndex = floorWindow.FloorData.RoomChoices.Last();
        }
        // var CurrentRoom = floorWindow.RoomsByLayer[PlayerLayerIndex][PlayerRoomIndex];
    }

    public void CreateRoomWeightMap()
    {
        for (var layer = 0; layer < roomsByLayer.Count; layer++)
        {
            for (var room = 0; room < roomsByLayer[layer].Count; room++)
            {
                var sanctumRoom = roomsByLayer[layer][room];
                if (sanctumRoom == null) continue;

                var stateTrackerRoom = sanctumStateTracker.GetRoom(layer, room);
                var (weight, debug) = weightCalculator.CalculateRoomWeight(stateTrackerRoom);
                roomWeights[layer, room] = weight;
                debugTexts[(layer, room)] = debug;
            }
        }
    }

    // TODO: Validate
    public List<(int, int)> FindBestPath()
    {
        var startLayer = roomsByLayer.Count - 1;
        var startRoom = PlayerRoomIndex;
        var startNode = (startLayer, startRoom);

        var bestPath = new Dictionary<(int, int), List<(int, int)>> { { startNode, new List<(int, int)> { startNode } } };
        var minCost = new Dictionary<(int, int), double>();

        // Initialize all possible room positions
        for (var layer = 0; layer < roomsByLayer.Count; layer++)
        {
            for (var room = 0; room < roomsByLayer[layer].Count; room++)
            {
                if (roomsByLayer[layer][room] != null)
                {
                    minCost[(layer, room)] = double.MaxValue;
                }
            }
        }
        minCost[startNode] = roomWeights[startLayer, startRoom];

        var queue = new SortedSet<(int, int)>(Comparer<(int, int)>.Create((a, b) =>
        {
            int compareResult = minCost[a].CompareTo(minCost[b]);
            if (compareResult != 0)
            {
                return compareResult;
            }
            // If costs are equal, break ties consistently
            return (a.Item1 != b.Item1) ? a.Item1.CompareTo(b.Item1) : a.Item2.CompareTo(b.Item2);
        }))
        {
            startNode
        };

        while (queue.Any())
        {
            var currentNode = queue.First();
            queue.Remove(currentNode);

            // Get valid neighbors from the room layout
            var neighbors = GetNeighbors(currentNode);
            foreach (var neighbor in neighbors)
            {
                double neighborCost = minCost[currentNode] + roomWeights[neighbor.Item1, neighbor.Item2];

                if (neighborCost < minCost[neighbor])
                {
                    queue.Remove(neighbor);
                    minCost[neighbor] = neighborCost;
                    queue.Add(neighbor);

                    bestPath[neighbor] = new List<(int, int)>(bestPath[currentNode]) { neighbor };
                }
            }
        }

        // Find the path that reaches the lowest layer (0) with the best cost
        var lowestLayerPaths = bestPath.Where(kvp => kvp.Key.Item1 == 0);
        if (!lowestLayerPaths.Any())
        {
            return new List<(int, int)>();
        }

        return lowestLayerPaths.OrderBy(kvp => minCost[kvp.Key]).First().Value;
    }

    private List<(int, int)> GetNeighbors((int layer, int room) current)
    {
        var neighbors = new List<(int, int)>();
        
        // Can only move up one layer at a time
        if (current.layer <= 0) return neighbors;
        
        var targetLayer = current.layer - 1;
        var connections = roomLayout[current.layer][current.room];
        
        foreach (var connectedRoom in connections)
        {
            // Verify the room exists in our data structure
            if (connectedRoom < roomsByLayer[targetLayer].Count && 
                roomsByLayer[targetLayer][connectedRoom] != null)
            {
                neighbors.Add((targetLayer, connectedRoom));
            }
        }
        
        return neighbors;
    }

    public void DrawDebugInfo()
    {
        if (!settings.DebugEnable.Value) return;

        for (var layer = 0; layer < roomsByLayer.Count; layer++)
        {
            for (var room = 0; room < roomsByLayer[layer].Count; room++)
            {
                var sanctumRoom = roomsByLayer[layer][room];
                if (sanctumRoom == null) continue;

                var pos = sanctumRoom.GetClientRect().Center;
                var debugText = debugTexts.TryGetValue((layer, room), out var text) ? text : string.Empty;
                var displayText = $"Weight: {roomWeights[layer, room]:F0}\n{debugText}";
                
                var textSize = graphics.MeasureText(displayText);
                graphics.DrawBox(pos, textSize + pos, settings.BackgroundColor);
                graphics.DrawText(displayText, pos, settings.TextColor);
            }
        }
    }
}
