using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ExileCore2.PoEMemory.Elements.Sanctum;
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

    public bool HasRoomData()
    {
        return roomStates.Count > 0;
    }

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
        // FIXME: Use floorWindow.RoomsByLayer once it is available.
        this.roomsByLayer = RoomsByLayerFromUI.GetRoomsByLayer(floorWindow);
        if (roomsByLayer == null || roomsByLayer.Count == 0)
        {
            return;
        }

        // Update Layout Data
        this.roomLayout = floorWindow.FloorData.RoomLayout;

        // Update Player Data
        PlayerLayerIndex = floorWindow.FloorData.RoomChoices.Count - 1;
        PlayerRoomIndex =
            floorWindow.FloorData.RoomChoices.Count > 0
                ? floorWindow.FloorData.RoomChoices.Last()
                : -1;

        // Update Room Data
        for (var layer = 0; layer < roomsByLayer.Count; layer++)
        {
            for (var room = 0; room < roomsByLayer[layer].Count; room++)
            {
                var sanctumRoom = roomsByLayer[layer][room];
                if (sanctumRoom == null)
                {
                    continue;
                }

                var key = (layer, room);
                if (!roomStates.ContainsKey(key))
                {
                    int numConnections = roomLayout[layer][room].Length;
                    roomStates[key] = new RoomState(sanctumRoom, numConnections);
                }
                else
                {
                    roomStates[key].UpdateRoom(sanctumRoom);
                }
            }
        }
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
        if (!string.IsNullOrEmpty(newRoomType))
            RoomType = newRoomType;
        if (!string.IsNullOrEmpty(newAffliction))
            Affliction = newAffliction;
        if (!string.IsNullOrEmpty(newReward))
            Reward = newReward;
        Position = newRoom.Position;
    }

    public override string ToString()
    {
        return $"Type: {RoomType}, Affliction: {Affliction}, Reward: {Reward}, Connections: {Connections}";
    }
}
