using ExileCore.PoEMemory.Elements.Sanctum;
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
                    roomStates[key].UpdateIfBetter(sanctumRoom);
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
    public List<string> Afflictions { get; private set; }
    public List<string> Rewards { get; private set; }
    public bool IsKnown { get; private set; }

    public RoomState(SanctumRoomElement room)
    {
        UpdateFromRoom(room);
    }

    public void UpdateIfBetter(SanctumRoomElement newRoom)
    {
        // If current state is unknown but new room is known, update
        if (!IsKnown && newRoom.IsKnown)
        {
            UpdateFromRoom(newRoom);
            return;
        }

        // If we have fewer afflictions known, update
        if (Afflictions.Count < newRoom.Data.Afflictions.Count)
        {
            UpdateFromRoom(newRoom);
            return;
        }

        // If we have fewer rewards known, update
        if (Rewards.Count < newRoom.Data.Rewards.Count)
        {
            UpdateFromRoom(newRoom);
        }
    }

    private void UpdateFromRoom(SanctumRoomElement room)
    {
        RoomType = room.Data.RoomType.ToString();
        Afflictions = room.Data.Afflictions.Select(a => a.ToString()).ToList();
        Rewards = room.Data.Rewards.Select(r => r.ToString()).ToList();
        IsKnown = room.IsKnown;
    }
} 
