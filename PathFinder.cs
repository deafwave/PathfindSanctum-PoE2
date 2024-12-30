using System.Collections.Generic;
using System.Linq;
using ExileCore2;

namespace PathfindSanctum;

/// <summary>
/// Handles Dijkstra Pathfinding logic for Sanctum, calculating optimal routes based on room weights.
/// </summary>
public class PathFinder(
    Graphics graphics,
    PathfindSanctumSettings settings,
    SanctumStateTracker sanctumStateTracker,
    WeightCalculator weightCalculator
)
{
    private double[,] roomWeights;
    private readonly Dictionary<(int, int), string> debugTexts = [];

    private List<(int, int)> foundBestPath;

    #region Path Calculation
    public void CreateRoomWeightMap()
    {
        var roomsByLayer = sanctumStateTracker.roomsByLayer;

        roomWeights = new double[roomsByLayer.Count, roomsByLayer.Max(x => x.Count)];

        for (var layer = 0; layer < roomsByLayer.Count; layer++)
        {
            for (var room = 0; room < roomsByLayer[layer].Count; room++)
            {
                var sanctumRoom = roomsByLayer[layer][room];
                if (sanctumRoom == null)
                    continue;

                var stateTrackerRoom = sanctumStateTracker.GetRoom(layer, room);
                var (weight, debug) = weightCalculator.CalculateRoomWeight(stateTrackerRoom);
                roomWeights[layer, room] = weight;
                debugTexts[(layer, room)] = debug;
            }
        }
    }

    public List<(int, int)> FindBestPath()
    {
        int numLayers = sanctumStateTracker.roomLayout.Length;
        var startNode = (7, 0);

        var bestPath = new Dictionary<(int, int), List<(int, int)>>
        {
            {
                startNode,
                new List<(int, int)> { startNode }
            }
        };
        var maxCost = new Dictionary<(int, int), double>();

        // Initialize maxCost for all valid rooms
        for (int i = 0; i < roomWeights.GetLength(0); i++)
        {
            for (int j = 0; j < roomWeights.GetLength(1); j++)
            {
                maxCost[(i, j)] = double.MinValue;
            }
        }
        maxCost[startNode] = roomWeights[startNode.Item1, startNode.Item2];

        var queue = new SortedSet<(int, int)>(
            Comparer<(int, int)>.Create(
                (a, b) =>
                {
                    double costA = maxCost[a];
                    double costB = maxCost[b];
                    if (costA != costB)
                    {
                        // Reverse comparison to prioritize higher weights
                        return costB.CompareTo(costA);
                    }
                    // If costs are equal, break the tie by comparing the nodes
                    return a.CompareTo(b);
                }
            )
        )
        {
            startNode
        };

        while (queue.Any())
        {
            var currentRoom = queue.First();
            queue.Remove(currentRoom);

            foreach (var neighbor in GetNeighbors(currentRoom, sanctumStateTracker.roomLayout))
            {
                double neighborCost =
                    maxCost[currentRoom] + roomWeights[neighbor.Item1, neighbor.Item2];

                if (neighborCost > maxCost[neighbor])
                {
                    queue.Remove(neighbor);
                    maxCost[neighbor] = neighborCost;
                    queue.Add(neighbor);
                    bestPath[neighbor] = new List<(int, int)>(bestPath[currentRoom]) { neighbor };
                }
            }
        }

        var groupedPaths = bestPath.GroupBy(pair => pair.Value.Count());
        var maxCountGroup = groupedPaths.OrderByDescending(group => group.Key).FirstOrDefault();
        var path = maxCountGroup
            ?.OrderByDescending(pair => maxCost.GetValueOrDefault(pair.Key, double.MinValue))
            .FirstOrDefault()
            .Value;

        if (sanctumStateTracker.PlayerLayerIndex != -1 && sanctumStateTracker.PlayerRoomIndex != -1)
        {
            path = bestPath.TryGetValue(
                (sanctumStateTracker.PlayerLayerIndex, sanctumStateTracker.PlayerRoomIndex),
                out var specificPath
            )
                ? specificPath
                : new List<(int, int)>();
        }

        foundBestPath = path ?? new List<(int, int)>();
        return foundBestPath;
    }

    private static IEnumerable<(int, int)> GetNeighbors(
        (int, int) currentRoom,
        byte[][][] connections
    )
    {
        int currentLayerIndex = currentRoom.Item1;
        int currentRoomIndex = currentRoom.Item2;
        int previousLayerIndex = currentLayerIndex - 1;

        if (currentLayerIndex == 0)
        {
            yield break; // No neighbors to yield
        }

        byte[][] previousLayer = connections[previousLayerIndex];

        for (
            int previousLayerRoomIndex = 0;
            previousLayerRoomIndex < previousLayer.Length;
            previousLayerRoomIndex++
        )
        {
            var previousLayerRoom = previousLayer[previousLayerRoomIndex];

            if (previousLayerRoom.Contains((byte)currentRoomIndex))
            {
                yield return (previousLayerIndex, previousLayerRoomIndex);
            }
        }
    }
    #endregion

    #region Visualization
    public void DrawDebugInfo()
    {
        if (!settings.DebugEnable.Value)
            return;

        var roomsByLayer = sanctumStateTracker.roomsByLayer;

        for (var layer = 0; layer < roomsByLayer.Count; layer++)
        {
            for (var room = 0; room < roomsByLayer[layer].Count; room++)
            {
                var sanctumRoom = sanctumStateTracker.GetRoom(layer, room);
                if (sanctumRoom == null)
                    continue;

                var pos = sanctumRoom.Position;

                // DebugWindow.LogMsg($"{layer}, {room}: {pos}");
                var debugText = debugTexts.TryGetValue((layer, room), out var text)
                    ? text
                    : string.Empty;
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
        if (this.foundBestPath == null)
            return;

        foreach (var room in this.foundBestPath)
        {
            if (
                room.Item1 == sanctumStateTracker.PlayerLayerIndex
                && room.Item2 == sanctumStateTracker.PlayerRoomIndex
            )
                continue;

            var sanctumRoom = sanctumStateTracker.roomsByLayer[room.Item1][room.Item2];

            graphics.DrawFrame(
                sanctumRoom.GetClientRect(),
                settings.BestPathColor,
                settings.FrameThickness
            );
        }
    }
    #endregion
}
