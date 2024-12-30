using ExileCore2.PoEMemory.Elements.Sanctum;
using System.Collections.Generic;
using System.Linq;
using ExileCore2;
using System.Numerics;
using static PathfindSanctum.RoomsByLayerFromUI;

namespace PathfindSanctum;

public class SanctumStateTracker
{
    private uint? currentAreaHash;
    private Dictionary<(int Layer, int Room), RoomState> roomStates = new();

    public List<List<FakeSanctumRoomElement>> roomsByLayer;
    public byte[][][] roomLayout;

    public int PlayerLayerIndex = -1;
    public int PlayerRoomIndex = -1;
    
    public bool IsSameSanctum(uint newAreaHash)
    {
        if (currentAreaHash == null)
        {
            currentAreaHash = newAreaHash;
            return false;
        }
        return currentAreaHash == newAreaHash;
    }

    public void UpdateRoomStates(SanctumFloorWindow floorWindow)
    {
        // DebugWindow.LogMsg("Starting UpdateRoomStates...", 10);
        
        // Temporary solution for RoomsByLayer offset not being updated
        this.roomsByLayer = RoomsByLayerFromUI.GetRoomsByLayer(floorWindow);
        // DebugWindow.LogMsg($"RoomsByLayer retrieved: {(roomsByLayer != null ? "Success" : "Failed")}", 10);
        if (roomsByLayer == null || roomsByLayer.Count == 0)
        {
            // DebugWindow.LogMsg("Early exit: roomsByLayer is null or empty", 10);
            return;
        }

        // DebugWindow.LogMsg($"Number of layers: {roomsByLayer.Count}", 10);
        
        // Update Layout Data
        this.roomLayout = floorWindow.FloorData.RoomLayout;
        // DebugWindow.LogMsg($"RoomLayout retrieved: {(roomLayout != null ? "Success" : "Failed")}", 10);

        // Update Player Data
        PlayerLayerIndex = floorWindow.FloorData.RoomChoices.Count - 1;
        PlayerRoomIndex = floorWindow.FloorData.RoomChoices.Count > 0 ? floorWindow.FloorData.RoomChoices.Last() : -1;
        // DebugWindow.LogMsg($"Player position updated - Layer: {PlayerLayerIndex}, Room: {PlayerRoomIndex}", 10);

        // Update Room Data
        for (var layer = 0; layer < roomsByLayer.Count; layer++)
        {
            // DebugWindow.LogMsg($"Processing Layer {layer} - Contains {roomsByLayer[layer].Count} rooms", 10);
            for (var room = 0; room < roomsByLayer[layer].Count; room++)
            {
                var sanctumRoom = roomsByLayer[layer][room];
                if (sanctumRoom == null)
                {
                    // DebugWindow.LogMsg($"  Room [{layer},{room}] is null, skipping");
                    continue;
                }

                var key = (layer, room);
                if (!roomStates.ContainsKey(key))
                {
                    // New room discovered
                    int numConnections = roomLayout[layer][room].Length;
                    roomStates[key] = new RoomState(sanctumRoom, numConnections);
                    // DebugWindow.LogMsg($"  Room [{layer},{room}] - New room added with {numConnections} connections");
                    // DebugWindow.LogMsg($"    Type: {roomStates[key].RoomType}, Affliction: {roomStates[key].Affliction}, Reward: {roomStates[key].Reward}");
                }
                else
                {
                    // Update existing room
                    var oldState = roomStates[key].ToString();
                    roomStates[key].UpdateRoom(sanctumRoom);
                    // DebugWindow.LogMsg($"  Room [{layer},{room}] - Updated existing room");
                    // DebugWindow.LogMsg($"    Before: {oldState}");
                    // DebugWindow.LogMsg($"    After: {roomStates[key]}");
                }
            }
        }
        // DebugWindow.LogMsg("UpdateRoomStates completed\n");
    }

    public void Reset(uint newAreaHash)
    {
        currentAreaHash = newAreaHash;
        roomStates.Clear();
    }

    public RoomState GetRoom(int layer, int room)
    {
        return roomStates.TryGetValue((layer, room), out var state) ? state : null;
    }
}

public class RoomState
{
    public string RoomType { get; private set; }
    public string Affliction { get; private set; }
    public string Reward { get; private set; }
    public int Connections { get; private set; }

    public Vector2 Position { get; internal set; }

    public RoomState(FakeSanctumRoomElement room, int numConnections)
    {
        Connections = numConnections;
        UpdateRoom(room);
    }

    public void UpdateRoom(FakeSanctumRoomElement newRoom)
    {
        var newRoomType = newRoom.Data.FightRoom?.RoomType.Id;
        var newAffliction = newRoom.Data?.RoomEffect?.ReadableName;
        var newReward = newRoom.Data.RewardRoom?.RoomType.Id;

        // Only update each field if we're getting new information (not null/empty)
        if (!string.IsNullOrEmpty(newRoomType)) RoomType = newRoomType;
        if (!string.IsNullOrEmpty(newAffliction)) Affliction = newAffliction;
        if (!string.IsNullOrEmpty(newReward)) Reward = newReward;
        Position = newRoom.Position;
        // DebugWindow.LogMsg($"UpdateRoom: Position={Position}, Type={RoomType ?? "null"}, Affliction={Affliction ?? "null"}, Reward={Reward ?? "null"}, Connections={Connections}");
    }

    public override string ToString()
    {
        return $"Type: {RoomType}, Affliction: {Affliction}, Reward: {Reward}, Connections: {Connections}";
    }
} 
