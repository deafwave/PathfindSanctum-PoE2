using System.Collections.Generic;
using ExileCore2;

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

        var roomLayout = floorWindow.FloorData.RoomLayout;
        if (roomLayout == null) return;

        // var roomsByLayer = floorWindow.RoomsByLayer;
        // if (roomsByLayer == null || roomsByLayer.Count == 0) return;

        var areaHash = GameController.Area.CurrentArea.Hash;
        
        // TODO: Possibly wait until we know they are done with this Sanctum Floor before resetting
        if (!stateTracker.IsSameSanctum(areaHash))
        {
            stateTracker.Reset(areaHash);
        }

        stateTracker.UpdateRoomStates(floorWindow);

        // TODO: Optimize this so it's not executed on every render (maybe only executed if we updated our known states)
        // Recalculate path using best known states
        pathFinder.CreateRoomWeightMap();

        if (Settings.DebugEnable)
        {
            pathFinder.DrawDebugInfo();
        }

        bestPath = pathFinder.FindBestPath();
        DebugWindow.LogMsg($"Best Path: {string.Join(" -> ", bestPath)}");
        pathFinder.DrawBestPath();
    }
} 
