using ExileCore2.Shared.Interfaces;
using ExileCore2.Shared.Nodes;
using System.Collections.Generic;
using System.Drawing;

namespace PathfindSanctum;

public class PathfindSanctumSettings : ISettings
{
    public ToggleNode Enable { get; set; } = new ToggleNode(true);
    public ToggleNode DebugEnable { get; set; } = new ToggleNode(false);
    
    public Dictionary<string, ProfileContent> Profiles = new()
    {
        ["Default"] = ProfileContent.CreateDefaultProfile(),
        ["No-Hit"] = ProfileContent.CreateNoHitProfile()
    };

    public ListNode CurrentProfile { get; set; }

    public PathfindSanctumSettings()
    {
        CurrentProfile = new ListNode { Values = [.. Profiles.Keys], Value = "Default" };
    }

    public ColorNode TextColor { get; set; } = new ColorNode(Color.White);
    public ColorNode BackgroundColor { get; set; } = new ColorNode(Color.FromArgb(128, 0, 0, 0));
    public ColorNode BestPathColor { get; set; } = new(Color.Green);

    public RangeNode<int> FrameThickness { get; set; } = new RangeNode<int>(5, 0, 10);
} 
