using System.Collections.Generic;
using ExileCore2;

namespace PathfindSanctum;

public class PathfindSanctumPlugin : BaseSettingsPlugin<PathfindSanctumSettings>
{
    private readonly SanctumStateTracker stateTracker = new();
    private PathFinder pathFinder;
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
        if (!GameController.Game.IngameState.InGame)
            return;

        var floorWindow = GameController.Game.IngameState.IngameUi.SanctumFloorWindow;
        if (floorWindow == null || !floorWindow.IsVisible)
            return;

        UpdateSanctumState(floorWindow);
        UpdateAndRenderPath();
    }

    private void UpdateSanctumState(dynamic floorWindow)
    {
        // TODO: If map is visible & area hash is not the same -> Reset()
        // Do not reset if map is not visible because you could be trading
        var areaHash = GameController.Area.CurrentArea.Hash;
        if (!stateTracker.IsSameSanctum(areaHash))
        {
            stateTracker.Reset(areaHash);
        }

        stateTracker.UpdateRoomStates(floorWindow);
    }

    private void UpdateAndRenderPath()
    {
        // TODO: Optimize this so it's not executed on every render (maybe only executed if we updated our known states)
        pathFinder.CreateRoomWeightMap();

        if (Settings.DebugEnable)
        {
            pathFinder.DrawDebugInfo();
        }

        bestPath = pathFinder.FindBestPath();
        pathFinder.DrawBestPath();
    }
}
