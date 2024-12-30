using System.Collections.Generic;
using System.Text;
using ExileCore2;
using ExileCore2.Shared.Enums;

namespace PathfindSanctum;

public class WeightCalculator(GameController gameController, PathfindSanctumSettings settings)
{
    private readonly GameController gameController = gameController;
    private readonly PathfindSanctumSettings settings = settings;
    private readonly StringBuilder debugText = new();

    public (double weight, string debug) CalculateRoomWeight(RoomState room)
    {
        if (room == null)
            return (0, string.Empty);

        debugText.Clear();
        var profile = settings.Profiles[settings.CurrentProfile];
        double weight = 1000000;

        weight += CalculateRoomTypeWeight(room, profile);
        weight += CalculateAfflictionWeights(room, profile);
        weight += CalculateRewardWeights(room, profile);
        weight += CalculateConnectivityBonus(room);

        return (weight, debugText.ToString());
    }

    private double CalculateRoomTypeWeight(RoomState room, ProfileContent profile)
    {
        var roomType = room.RoomType;
        if (roomType == null)
            return 0;

        if (profile.RoomTypeWeights.TryGetValue(roomType, out float typeWeight))
        {
            debugText.AppendLine($"{roomType}:{typeWeight}");
            return typeWeight;
        }

        debugText.AppendLine($"Room Type ({roomType}): 0 (not found in weights)");
        return 0;
    }

    private double CalculateAfflictionWeights(RoomState room, ProfileContent profile)
    {
        var affliction = room.Affliction;
        if (affliction == null)
            return 0;

        var afflictionName = affliction.ToString();
        var dynamicWeight = CalculateDynamicAfflictionWeight(afflictionName);
        if (dynamicWeight != null)
        {
            if (settings.DebugEnable.Value)
                debugText.AppendLine($"{afflictionName}:{dynamicWeight}");
            return (double)dynamicWeight;
        }
        else if (profile.AfflictionWeights.TryGetValue(afflictionName, out float afflictionWeight))
        {
            if (settings.DebugEnable.Value)
                debugText.AppendLine($"{afflictionName}:{afflictionWeight}");
            return afflictionWeight;
        }

        debugText.AppendLine($"Affliction ({afflictionName}): 0 (not found in weights)");
        return 0;
    }

    private double? CalculateDynamicAfflictionWeight(string afflictionName)
    {
        return afflictionName switch
        {
            "Iron Manacles" => CalculateIronManaclesWeight(),
            _ => null // No dynamic modification
        };
    }

    private double? CalculateIronManaclesWeight()
    {
        var playerEvasion = gameController.Player.Stats.GetValueOrDefault(
            GameStat.EvasionRating,
            0
        );

        if (playerEvasion > 6000)
        {
            return -750;
        }

        return null;
    }

    private double CalculateRewardWeights(RoomState room, ProfileContent profile)
    {
        if (
            room?.Reward != null
            && profile.RewardWeights.TryGetValue(room.Reward, out float rewardWeight)
        )
        {
            if (settings.DebugEnable.Value)
                debugText.AppendLine($"{room.Reward}:{rewardWeight}");
            return rewardWeight;
        }

        return 0;
    }

    private double CalculateConnectivityBonus(RoomState room)
    {
        var connectionBonus = (room.Connections - 1) * 100;
        if (settings.DebugEnable.Value)
            debugText.AppendLine($"Connectivity:{connectionBonus}");
        return connectionBonus;
    }
}
