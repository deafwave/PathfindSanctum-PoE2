using ExileCore2;

namespace PathfindSanctum;

public class PathfindSanctumPlugin : BaseSettingsPlugin<PathfindSanctumSettings>
{
    private readonly SanctumStateTracker stateTracker = new();
    private PathFinder pathFinder;
    private WeightCalculator weightCalculator;

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

        if (
            GameController.Area.CurrentArea.Area.RawName == "G2_13"
            || GameController.Area.CurrentArea.IsHideout
        )
            return;

        if (
            stateTracker.HasRoomData()
            && !stateTracker.IsSameSanctum(GameController.Area.CurrentArea.Hash)
        )
        {
            stateTracker.Reset(GameController.Area.CurrentArea.Hash);
            return;
        }

        var floorWindow = GameController.Game.IngameState.IngameUi.SanctumFloorWindow;
        if (floorWindow == null || !floorWindow.IsVisible)
            return;

        stateTracker.UpdateRoomStates(floorWindow);
        UpdateAndRenderPath();
    }

    private void UpdateAndRenderPath()
    {
        // TODO: Optimize this so it's not executed on every render (maybe only executed if we updated our known states)
        pathFinder.CreateRoomWeightMap();

        if (Settings.DebugEnable)
        {
            pathFinder.DrawDebugInfo();
        }

        pathFinder.FindBestPath();
        pathFinder.DrawBestPath();
    }
}
