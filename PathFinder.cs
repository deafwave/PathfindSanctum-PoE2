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

    // TODO: Validate
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

    // TODO: Validate
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
                
                var textSize = graphics.MeasureText(displayText);
                graphics.DrawBox(pos, textSize + pos, settings.BackgroundColor);
                graphics.DrawText(displayText, pos, settings.TextColor);
            }
        }
    }
}
