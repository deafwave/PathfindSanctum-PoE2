using ExileCore2;
using System.Collections.Generic;

namespace PathfindSanctum;

public class PathfindSanctumPlugin : BaseSettingsPlugin<PathfindSanctumSettings>
{
    private PathFinder pathFinder;
    private readonly SanctumStateTracker stateTracker = new();
    private WeightCalculator weightCalculator;
    private List<(int, int)> bestPath;

    public override bool Initialise()
    {
        weightCalculator = new WeightCalculator(GameController, Settings);
        pathFinder = new PathFinder(Graphics, Settings, stateTracker, weightCalculator);
        return base.Initialise();
    }

    public override void Render()
    {
        if (!GameController.Game.IngameState.InGame) return;

        var floorWindow = GameController.Game.IngameState.IngameUi.SanctumFloorWindow;
        if (floorWindow == null || !floorWindow.IsVisible) return;

        var roomsByLayer = floorWindow.RoomsByLayer;
        if (roomsByLayer == null || roomsByLayer.Count == 0) return;

        var roomLayout = floorWindow.FloorData.RoomLayout;
        if (roomLayout == null) return;

        var areaHash = GameController.Area.CurrentArea.Hash;
        
        if (!stateTracker.IsSameSanctum(areaHash))
        {
            stateTracker.Reset(areaHash);
        }

        // Update our known states
        stateTracker.UpdateRoomStates(floorWindow);

        // Recalculate path using best known states
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
                Settings.FrameThickness
            );
        }
    }
} 
