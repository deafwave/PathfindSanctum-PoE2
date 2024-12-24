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
        int numLayers = roomLayout.Length;
        var startNode = (7, 0);

        var bestPath = new Dictionary<(int, int), List<(int, int)>> { { startNode, new List<(int, int)> { startNode } } };
        var minCost = new Dictionary<(int, int), int>();
        foreach (var room in roomWeightMap.Keys)
        {
            minCost[room] = int.MaxValue;
        }
        minCost[startNode] = roomWeightMap[startNode];

        var queue = new SortedSet<(int, int)>(Comparer<(int, int)>.Create((a, b) =>
        {
            int costA = minCost[a];
            int costB = minCost[b];
            if (costA != costB)
            {
                return costA.CompareTo(costB);
            }
            // If costs are equal, break the tie by comparing the nodes
            return a.CompareTo(b);
        }))
    {
        startNode
    };

        while (queue.Any())
        {
            var currentRoom = queue.First();
            queue.Remove(currentRoom); // Remove the processed node from the queue

            foreach (var neighbor in GetNeighbors(currentRoom, roomLayout))
            {
                int neighborCost = minCost[currentRoom] + roomWeightMap[neighbor];

                if (neighborCost < minCost[neighbor])
                {
                    // Remove the old entry before adding the updated one
                    queue.Remove(neighbor);

                    // Update the minimum cost and best path
                    minCost[neighbor] = neighborCost;

                    // Add the neighbor to the queue at the correct position
                    queue.Add(neighbor);

                    // Create a new list for the neighbor node and copy the path from the current node
                    bestPath[neighbor] = new List<(int, int)>(bestPath[currentRoom]) { neighbor };
                }
            }
        }

        // DEBUGGING
        /*foreach (var kvp in bestPath)
        {
            var key = kvp.Key;
            var value = kvp.Value;

            // Output the key and value to LogError
            LogError($"Key: {key}, Value: {string.Join(", ", value)}, minCost: {minCost[key]}");
        }*/

        var groupedPaths = bestPath.GroupBy(pair => pair.Value.Count());
        var maxCountGroup = groupedPaths.OrderByDescending(group => group.Key).FirstOrDefault();
        var path = maxCountGroup.OrderBy(pair => minCost.GetValueOrDefault(pair.Key, int.MaxValue)).FirstOrDefault().Value;
        if (PlayerLayerIndex != -1 && PlayerRoomIndex != -1)
        {
            path = bestPath.TryGetValue((PlayerLayerIndex, PlayerRoomIndex), out var specificPath) ? specificPath : new List<(int, int)>();
        }


        if (path == null)
        {
            return new List<(int, int)>();
        }

        return path;
    }

    private static IEnumerable<(int, int)> GetNeighbors((int, int) currentRoom, byte[][][] connections)
    {
        int currentLayerIndex = currentRoom.Item1;
        int currentRoomIndex = currentRoom.Item2;
        int previousLayerIndex = currentLayerIndex - 1;

        if (currentLayerIndex == 0)
        {
            yield break; // No neighbors to yield
        }

        byte[][] previousLayer = connections[previousLayerIndex];

        for (int previousLayerRoomIndex = 0; previousLayerRoomIndex < previousLayer.Length; previousLayerRoomIndex++)
        {
            var previousLayerRoom = previousLayer[previousLayerRoomIndex];

            if (previousLayerRoom.Contains((byte)currentRoomIndex))
            {
                yield return (previousLayerIndex, previousLayerRoomIndex);
            }
        }
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
