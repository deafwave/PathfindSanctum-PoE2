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
    private readonly WeightCalculator weightCalculator;
    private readonly double[,] roomWeights;
    private readonly PathfindSanctumSettings settings;
    private readonly Dictionary<(int, int), string> debugTexts = new();
    private readonly SanctumStateTracker sanctumStateTracker;
    
    public readonly int PlayerLayerIndex;
    public readonly int PlayerRoomIndex;

    public PathFinder(
        List<List<SanctumRoomElement>> roomsByLayer, 
        GameController gameController, 
        PathfindSanctumSettings settings,
        SanctumStateTracker stateTracker)
    {
        this.roomsByLayer = roomsByLayer;
        this.settings = settings;
        this.weightCalculator = new WeightCalculator(gameController, settings);
        this.sanctumStateTracker = stateTracker;
        
        roomWeights = new double[roomsByLayer.Count, roomsByLayer.Max(x => x.Count)];

        // Find player position
        for (var layer = 0; layer < roomsByLayer.Count; layer++)
        {
            for (var room = 0; room < roomsByLayer[layer].Count; room++)
            {
                if (roomsByLayer[layer][room].IsCurrentRoom)
                {
                    PlayerLayerIndex = layer;
                    PlayerRoomIndex = room;
                    break;
                }
            }
        }
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

    public List<(int, int)> FindBestPath()
    {
        var visited = new HashSet<(int, int)>();
        var paths = new Dictionary<(int, int), (int, int)>();
        var weights = new Dictionary<(int, int), double>();
        var queue = new PriorityQueue<(int, int), double>();

        // Start from player position
        queue.Enqueue((PlayerLayerIndex, PlayerRoomIndex), 0);
        weights[(PlayerLayerIndex, PlayerRoomIndex)] = 0;

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (!visited.Add(current)) continue;

            var currentRoom = roomsByLayer[current.Item1][current.Item2];
            foreach (var nextRoom in currentRoom.GetConnectedRooms())
            {
                var next = (nextRoom.Layer, nextRoom.RoomIndex);
                var newWeight = weights[current] + roomWeights[next.Layer, next.RoomIndex];

                if (!weights.ContainsKey(next) || newWeight > weights[next])
                {
                    weights[next] = newWeight;
                    paths[next] = current;
                    queue.Enqueue(next, -newWeight); // Negative for max-heap behavior
                }
            }
        }

        return ReconstructPath(paths);
    }

    private List<(int, int)> ReconstructPath(Dictionary<(int, int), (int, int)> paths)
    {
        var path = new List<(int, int)>();
        var current = FindEndPoint();

        while (paths.ContainsKey(current))
        {
            path.Add(current);
            current = paths[current];
        }
        path.Add((PlayerLayerIndex, PlayerRoomIndex));
        path.Reverse();
        return path;
    }

    private (int, int) FindEndPoint()
    {
        // Find the room with the highest weight in the last accessible layer
        var maxWeight = double.MinValue;
        var endPoint = (0, 0);

        for (var layer = 0; layer < roomsByLayer.Count; layer++)
        {
            for (var room = 0; room < roomsByLayer[layer].Count; room++)
            {
                if (roomWeights[layer, room] > maxWeight)
                {
                    maxWeight = roomWeights[layer, room];
                    endPoint = (layer, room);
                }
            }
        }

        return endPoint;
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
                
                Graphics.DrawText(
                    displayText,
                    pos,
                    settings.TextColor,
                    settings.BackgroundColor);
            }
        }
    }
}
