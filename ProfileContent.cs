public class ProfileContent
{
    public Dictionary<string, float> RoomTypeWeights { get; set; }
    public Dictionary<string, float> AfflictionWeights { get; set; }
    public Dictionary<string, float> RewardWeights { get; set; }

    public static ProfileContent CreateDefaultProfile()
    {
        var profile = new ProfileContent
        {
            RoomTypeWeights = new()
            {
                ["Gauntlet"] = 0,
                ["Chalice"] = 1000,
                ["Hourglass"] = 500,
                ["Escape"] = 750
            },
            AfflictionWeights = new()
            {
                // Major negative afflictions for normal profile
                ["Death Toll"] = -500,        // Less severe for normal
                ["Spiked Exit"] = -500,       // Less severe for normal
                
                // Normal profile cares about these
                ["Hungry Fangs"] = -750,
                ["Leaking Waterskin"] = -750,
                ["Chiselled Stone"] = -750,
                ["Chains of Binding"] = -750,
                ["Dark Pit"] = -750,
                ["Dishonoured Tattoo"] = -500,
                ["Rusted Mallet"] = -500,
                ["Honed Claws"] = -500
            },
            RewardWeights = new()
            {
                // [Previous reward weights remain unchanged]
                ["Gold Key"] = 1000000,
                ["Silver Key"] = 500000,
                ["Bronze Key"] = 200000,
                ["Large Fountain"] = 50000,
                ["Fountain"] = 25000,
                ["Pledge to Kochai"] = 45000,
                ["Honour Halani"] = 40000,
                ["Honour Ahkeli"] = 40000,
                ["Honour Orbala"] = 40000,
                ["Honour Galai"] = 40000,
                ["Honour Tabana"] = 40000,
                ["Golden Cache"] = 35000,
                ["Silver Cache"] = 20000,
                ["Bronze Cache"] = 10000,
                ["Merchant"] = 30000
            }
        };
        return profile;
    }

    public static ProfileContent CreateNoHitProfile()
    {
        var profile = new ProfileContent
        {
            RoomTypeWeights = new()
            {
                ["Gauntlet"] = -1000,         // More negative for no-hit
                ["Chalice"] = 1000,
                ["Hourglass"] = -750,         // More negative for no-hit
                ["Escape"] = 750
            },
            AfflictionWeights = new()
            {
                // Critical afflictions for no-hit
                ["Death Toll"] = -1000000,    // Extremely negative for no-hit
                ["Spiked Exit"] = -1000000,   // Extremely negative for no-hit
                
                // Neutral afflictions for no-hit
                ["Hungry Fangs"] = 0,
                ["Leaking Waterskin"] = 0,
                ["Chiselled Stone"] = 0,
                ["Chains of Binding"] = 0,
                ["Dark Pit"] = 0,
                ["Dishonoured Tattoo"] = 0,
                ["Rusted Mallet"] = 0,
                ["Honed Claws"] = 0
            },
            RewardWeights = new()
            {
                // Same reward weights as default profile
                ["Gold Key"] = 1000000,
                ["Silver Key"] = 500000,
                ["Bronze Key"] = 200000,
                ["Large Fountain"] = 50000,
                ["Fountain"] = 25000,
                ["Pledge to Kochai"] = 45000,
                ["Honour Halani"] = 40000,
                ["Honour Ahkeli"] = 40000,
                ["Honour Orbala"] = 40000,
                ["Honour Galai"] = 40000,
                ["Honour Tabana"] = 40000,
                ["Golden Cache"] = 35000,
                ["Silver Cache"] = 20000,
                ["Bronze Cache"] = 10000,
                ["Merchant"] = 30000
            }
        };
        return profile;
    }

    public ProfileContent Clone()
    {
        return new ProfileContent
        {
            RoomTypeWeights = new Dictionary<string, float>(RoomTypeWeights),
            AfflictionWeights = new Dictionary<string, float>(AfflictionWeights),
            RewardWeights = new Dictionary<string, float>(RewardWeights)
        };
    }
}
