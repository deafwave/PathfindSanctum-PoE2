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
        ["Default"] = new ProfileContent(),
        ["No-Hit"] = new ProfileContent()
    };

    public string CurrentProfile;

    public ColorNode TextColor { get; set; } = new ColorNode(Color.White);
    public ColorNode BackgroundColor { get; set; } = new ColorNode(Color.Black with { A = 128 });
    public ColorNode BestPathColor { get; set; } = new(Color.Green);
} 
