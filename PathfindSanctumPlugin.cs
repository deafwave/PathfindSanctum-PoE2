using ExileCore2;
using ExileCore2.PoEMemory.Elements.Sanctum;
using ExileCore2.Shared.Nodes;
using System.Collections.Generic;

namespace PathfindSanctum;

public class PathfindSanctumPlugin : BaseSettingsPlugin<PathfindSanctumSettings>
{
    private PathFinder pathFinder;
    private SanctumStateTracker stateTracker = new();
    private List<(int, int)> bestPath;

    public override void Render()
    {
        if (!GameController.Game.IngameState.InGame) return;

        var sanctumUI = GameController.Game.IngameState.IngameUi.SanctumFloorWindow;
        if (sanctumUI == null || !sanctumUI.IsVisible) return;

        var roomsByLayer = sanctumUI.RoomsByLayer;
        if (roomsByLayer == null) return;

        var areaHash = GameController.Area.CurrentArea.Hash;
        
        if (!stateTracker.IsSameSanctum(areaHash))
        {
            stateTracker.Reset(areaHash);
        }

        // Update our known states
        stateTracker.UpdateRoomStates(roomsByLayer);

        // Recalculate path using best known states
        pathFinder = new PathFinder(roomsByLayer, GameController, Settings, stateTracker);
        pathFinder.CreateRoomWeightMap();
        bestPath = pathFinder.FindBestPath();

        // Draw debug info if enabled
        if (Settings.DebugEnable)
        {
            pathFinder.DrawDebugInfo();
        }

        // Draw best path
        foreach (var room in bestPath)
        {
            if (room.Item1 == pathFinder.PlayerLayerIndex && 
                room.Item2 == pathFinder.PlayerRoomIndex) continue;

            var sanctumRoom = roomsByLayer[room.Item1][room.Item2];
            Graphics.DrawFrame(
                sanctumRoom.GetClientRectCache, 
                Settings.BestPathColor, 
                Settings.FrameThickness);
        }
    }
} 
