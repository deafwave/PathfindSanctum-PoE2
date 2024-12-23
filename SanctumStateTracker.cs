using ExileCore2.PoEMemory.Elements.Sanctum;
using System.Collections.Generic;

namespace PathfindSanctum;

public class SanctumStateTracker
{
    private uint? currentAreaHash;
    private Dictionary<(int Layer, int Room), RoomState> roomStates = new();

    public bool IsSameSanctum(uint newAreaHash)
    {
        if (currentAreaHash == null)
        {
            currentAreaHash = newAreaHash;
            return false;
        }
        return currentAreaHash == newAreaHash;
    }

    public void UpdateRoomStates(List<List<SanctumRoomElement>> roomsByLayer)
    {
        for (var layer = 0; layer < roomsByLayer.Count; layer++)
        {
            for (var room = 0; room < roomsByLayer[layer].Count; room++)
            {
                var sanctumRoom = roomsByLayer[layer][room];
                if (sanctumRoom == null) continue;

                var key = (layer, room);
                if (!roomStates.ContainsKey(key))
                {
                    // New room discovered
                    roomStates[key] = new RoomState(sanctumRoom);
                }
                else
                {
                    // Update existing room if new data is better
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

    public RoomState GetBestKnownState(int layer, int room)
    {
        return roomStates.TryGetValue((layer, room), out var state) ? state : null;
    }
}

public class RoomState
{
    public string RoomType { get; private set; }
    public string Affliction { get; private set; }
    public string Reward { get; private set; }

    public RoomState(SanctumRoomElement room)
    {
        UpdateRoom(room);
    }

    public void UpdateRoom(SanctumRoomElement newRoom)
    {
        var newRoomType = newRoom.Data.FightRoom?.RoomType.Id;
        var newAffliction = newRoom.Data?.RoomEffect?.ReadableName;
        var newReward = newRoom.Data.RewardRoom?.RoomType.Id;

        // Only update each field if we're getting new information (not null/empty)
        if (!string.IsNullOrEmpty(newRoomType)) RoomType = newRoomType;
        if (!string.IsNullOrEmpty(newAffliction)) Affliction = newAffliction;
        if (!string.IsNullOrEmpty(newReward)) Reward = newReward;
    }
} 
