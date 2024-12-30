using System.Collections.Generic;
using System.Numerics;
using ExileCore2.Shared;

namespace PathfindSanctum;

/// <summary>
/// Handles the extraction and organization of Sanctum room data from the game's UI elements until floorWindow.RoomsByLayer is available
/// </summary>
public class RoomsByLayerFromUI
{
    #region Mappings

    private static readonly Dictionary<string, string> RewardMapping =
        new()
        {
            { "Awards a Large Sacred Water Fountain", "Large Fountain" },
            { "Awards a Sacred Water Fountain", "Fountain" },
            {
                "Awards a Pledge which can be accepted to change the Trial's Parameters",
                "Pledge to Kochai"
            },
            { "Awards a Shrine to restore Honour and gain Sacred Water", "Honour Halani" },
            {
                "Awards a Shrine that greatly restores Honour and burdens you with an Affliction",
                "Honour Ahkeli"
            },
            { "Awards a Shrine that bestows the fickle Blessings of the Wind", "Honour Galai" },
            { "Awards a Shrine to restore Honour", "Honour Tabana" },
            { "Awards a Shrine that restores Honour and grants you a Boon", "Honour Orbala" },
            { "Contains Merchant", "Merchant" },
            { "Awards Bronze Key", "Bronze Key" },
            { "Awards Silver Key", "Silver Key" },
            { "Awards Gold Key", "Gold Key" },
            { "Awards a Bronze Cache. Requires a Bronze Key to Open", "Bronze Cache" },
            { "Awards a Silver Cache. Requires a Silver Key to Open", "Silver Cache" },
            { "Awards a Gold Cache. Requires a Gold Key to Open", "Gold Cache" }
        };

    private static readonly Dictionary<string, string> RoomTypeMapping =
        new()
        {
            { "Chalice Trial", "Chalice" },
            { "Escape Trial", "Escape" },
            { "Ritual Trial", "Ritual" },
            { "Gauntlet Trial", "Gauntlet" },
            { "Hourglass Trial", "Hourglass" },
            { "Collapsing Cavern", "Boss" },
            { "Ceremonial Chamber", "Boss" },
            { "Sand Pit", "Boss" },
            { "Outside of Time", "Boss" }
        };

    #endregion

    #region Models

    public class FakeSanctumRoomElement
    {
        public RoomData Data { get; private set; } = new();
        public Vector2 Position { get; set; }
        public RectangleF ClientRect { get; set; }

        public RectangleF GetClientRect() => ClientRect;

        public void UpdateFromTooltip(string roomType, string affliction, string reward)
        {
            if (roomType != null)
                Data.FightRoom = new FightRoom { RoomType = new RoomType { Id = roomType } };

            if (affliction != null)
                Data.RoomEffect = new RoomEffect { ReadableName = affliction };

            if (reward != null)
                Data.RewardRoom = new RewardRoom { RoomType = new RoomType { Id = reward } };
        }

        #region Nested Types

        public class RoomType
        {
            public string Id { get; set; }
        }

        public class FightRoom
        {
            public RoomType RoomType { get; set; }
        }

        public class RewardRoom
        {
            public RoomType RoomType { get; set; }
        }

        public class RoomEffect
        {
            public string ReadableName { get; set; }
        }

        public class RoomData
        {
            public FightRoom FightRoom { get; set; }
            public RewardRoom RewardRoom { get; set; }
            public RoomEffect RoomEffect { get; set; }
        }

        #endregion
    }

    #endregion

    public static List<List<FakeSanctumRoomElement>> GetRoomsByLayer(dynamic floorWindow)
    {
        var result = new List<List<FakeSanctumRoomElement>>();

        var layersContainer = floorWindow
            ?.GetChildAtIndex(0)
            ?.GetChildAtIndex(0)
            ?.GetChildAtIndex(1);
        if (layersContainer == null)
            return result;

        // For each layer
        for (int i = 0; i < layersContainer.Children.Count; i++)
        {
            var layer = new List<FakeSanctumRoomElement>();
            var layerElement = layersContainer.Children[i];

            // For each room
            foreach (var roomElement in layerElement.Children)
            {
                var room = ProcessRoomElement(roomElement);
                layer.Add(room);
            }

            result.Add(layer);
        }

        return result;
    }

    private static FakeSanctumRoomElement ProcessRoomElement(dynamic roomElement)
    {
        var sanctumRoom = new FakeSanctumRoomElement
        {
            ClientRect = roomElement.GetClientRect(),
            Position = roomElement.GetClientRect().TopLeft
        };

        if (roomElement?.Tooltip?.Children == null || roomElement.Tooltip.Children.Count == 0)
        {
            sanctumRoom.UpdateFromTooltip(null, null, null);
            return sanctumRoom;
        }

        var tooltipTexts = ExtractTooltipTexts(roomElement);
        var tooltipInfo = ParseTooltipInformation(tooltipTexts);
        string roomType = tooltipInfo.Item1;
        string affliction = tooltipInfo.Item2;
        string reward = tooltipInfo.Item3;
        sanctumRoom.UpdateFromTooltip(roomType, affliction, reward);

        return sanctumRoom;
    }

    private static List<string> ExtractTooltipTexts(dynamic roomElement)
    {
        var tooltipTexts = new List<string>();

        if (roomElement?.Tooltip?.Children == null)
            return tooltipTexts;

        foreach (var tooltipChild in roomElement.Tooltip.Children)
        {
            if (tooltipChild?.Children == null)
                continue;

            foreach (var textChild in tooltipChild.Children)
            {
                var text = textChild?.Text;
                if (!string.IsNullOrEmpty(text))
                {
                    tooltipTexts.Add(text);
                }
            }
        }

        return tooltipTexts;
    }

    private static (string roomType, string affliction, string reward) ParseTooltipInformation(
        List<string> tooltipTexts
    )
    {
        string roomType = null;
        string affliction = null;
        string reward = null;

        foreach (var tooltipText in tooltipTexts)
        {
            if (RewardMapping.TryGetValue(tooltipText, out var mappedReward))
            {
                reward = mappedReward;
            }
            else if (tooltipText.Contains("<sanctumcurse>"))
            {
                var startBrace = tooltipText.IndexOf('{');
                var endBrace = tooltipText.IndexOf('}');
                if (startBrace >= 0 && endBrace > startBrace)
                {
                    affliction = tooltipText[(startBrace + 1)..endBrace];
                }
            }
            else if (RoomTypeMapping.TryGetValue(tooltipText, out var mappedRoomType))
            {
                roomType = mappedRoomType;
            }
        }

        return (roomType, affliction, reward);
    }
}
