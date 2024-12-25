using System.Collections.Generic;
using System.Linq;
using ExileCore2;

namespace PathfindSanctum;

public class PathFinder(
    Graphics graphics,
    PathfindSanctumSettings settings,
    SanctumStateTracker sanctumStateTracker,
    WeightCalculator weightCalculator)
{
    private readonly WeightCalculator weightCalculator = weightCalculator;
    private readonly Graphics graphics = graphics;
    private double[,] roomWeights;
    private readonly PathfindSanctumSettings settings = settings;
    private readonly Dictionary<(int, int), string> debugTexts = new();
    private readonly SanctumStateTracker sanctumStateTracker = sanctumStateTracker;
    
    private List<(int, int)> foundBestPath;

    public void CreateRoomWeightMap()
    {
        var roomsByLayer = sanctumStateTracker.roomsByLayer;

        roomWeights = new double[roomsByLayer.Count, roomsByLayer.Max(x => x.Count)];
        
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
        var roomsByLayer = sanctumStateTracker.roomsByLayer;

        var startLayer = 7;
        var startRoom = 0;
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

        this.foundBestPath = lowestLayerPaths.OrderBy(kvp => minCost[kvp.Key]).First().Value;
        return lowestLayerPaths.OrderBy(kvp => minCost[kvp.Key]).First().Value;
    }

    private List<(int, int)> GetNeighbors((int layer, int room) current)
    {
        var roomsByLayer = sanctumStateTracker.roomsByLayer;
        var roomLayout = sanctumStateTracker.roomLayout;

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

        var roomsByLayer = sanctumStateTracker.roomsByLayer;
        
        for (var layer = 0; layer < roomsByLayer.Count; layer++)
        {
            for (var room = 0; room < roomsByLayer[layer].Count; room++)
            {
                var sanctumRoom = roomsByLayer[layer][room];
                if (sanctumRoom == null) continue;

                var pos = sanctumRoom.GetClientRect().Center;
                var debugText = debugTexts.TryGetValue((layer, room), out var text) ? text : string.Empty;
                var displayText = $"Weight: {roomWeights[layer, room]:F0}\n{debugText}";

                graphics.DrawTextWithBackground(
                    displayText,
                    pos,
                    settings.TextColor,
                    settings.BackgroundColor
                );
            }
        }
    }

    public void DrawBestPath()
    {
        if (this.foundBestPath == null) return;

        foreach (var room in this.foundBestPath)
        {
            if (room.Item1 == sanctumStateTracker.PlayerLayerIndex && 
                room.Item2 == sanctumStateTracker.PlayerRoomIndex) continue;

            var sanctumRoom = sanctumStateTracker.roomsByLayer[room.Item1][room.Item2];

            graphics.DrawFrame(
                sanctumRoom.GetClientRectCache, 
                settings.BestPathColor, 
                settings.FrameThickness
            );
        }
    }
}
