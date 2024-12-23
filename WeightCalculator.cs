using ExileCore;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.Elements.Sanctum;
using System.Text;

namespace PathfindSanctum;

public class WeightCalculator
{
    private readonly GameController gameController;
    private readonly PathfindSanctumSettings settings;
    private readonly StringBuilder debugText = new();

    public WeightCalculator(GameController gameController, PathfindSanctumSettings settings)
    {
        this.gameController = gameController;
        this.settings = settings;
    }

    public (double weight, string debug) CalculateRoomWeight(SanctumRoomElement room)
    {
        if (room == null) return (0, string.Empty);

        debugText.Clear();
        var profile = settings.Profiles[settings.CurrentProfile];
        double weight = 1000000; // Base weight
        debugText.AppendLine($"Base: {weight}");

        weight += CalculateRoomTypeWeight(room, profile);
        weight += CalculateAfflictionWeights(room, profile);
        weight += CalculateRewardWeights(room, profile);
        weight += CalculateConnectivityBonus(room);

        return (weight, debugText.ToString());
    }

    private double CalculateRoomTypeWeight(SanctumRoomElement room, ProfileContent profile)
    {
        var roomType = room.Data.RoomType.ToString();
        if (profile.RoomTypeWeights.TryGetValue(roomType, out float typeWeight))
        {
            debugText.AppendLine($"Room Type ({roomType}): {typeWeight}");
            return typeWeight;
        }
        return 0;
    }

    private double CalculateAfflictionWeights(SanctumRoomElement room, ProfileContent profile)
    {
        double totalWeight = 0;

        foreach (var affliction in room.Data.Afflictions)
        {
            var afflictionName = affliction.ToString();
            var (dynamicWeight, explanation) = CalculateDynamicAfflictionWeight(afflictionName);
            
            if (explanation != string.Empty)
            {
                totalWeight += dynamicWeight;
                debugText.AppendLine($"Affliction ({afflictionName}): {explanation}");
                continue;
            }

            if (profile.AfflictionWeights.TryGetValue(afflictionName, out float afflictionWeight))
            {
                totalWeight += afflictionWeight;
                debugText.AppendLine($"Affliction ({afflictionName}): {afflictionWeight}");
            }
        }

        return totalWeight;
    }

    private (float weight, string explanation) CalculateDynamicAfflictionWeight(string afflictionName)
    {
        switch (afflictionName)
        {
            case "Iron Manacles":
                return CalculateIronManaclesWeight();
                
            // Future dynamic affliction weights can be added here
            // case "Some Other Affliction":
            //     return CalculateSomeOtherAfflictionWeight();
                
            default:
                return (0, string.Empty); // No dynamic modification
        }
    }

    private (float weight, string explanation) CalculateIronManaclesWeight()
    {
        var playerEvasion = gameController.EntityListWrapper.Player?.GetComponent<Life>()?.Evasion ?? 0;
        
        if (playerEvasion > 6000)
        {
            return (-750000, "(High Evasion Build: -750000)");
        }
        
        return (0, "(Low Evasion Build: Neutral)");
    }

    private double CalculateRewardWeights(SanctumRoomElement room, ProfileContent profile)
    {
        double totalWeight = 0;

        foreach (var reward in room.Data.Rewards)
        {
            var rewardName = reward.ToString();
            if (profile.RewardWeights.TryGetValue(rewardName, out float rewardWeight))
            {
                totalWeight += rewardWeight;
                debugText.AppendLine($"Reward ({rewardName}): {rewardWeight}");
            }
        }

        return totalWeight;
    }

    private double CalculateConnectivityBonus(SanctumRoomElement room)
    {
        var connectionBonus = room.GetConnectedRooms().Count * 1000;
        debugText.AppendLine($"Connections: +{connectionBonus}");
        return connectionBonus;
    }
} 
