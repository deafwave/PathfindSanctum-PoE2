using System.Collections.Generic;
using System.Numerics;
using ExileCore2;
using ExileCore2.Shared;
using ExileCore2.PoEMemory.Elements.Sanctum;
using System.Linq;

namespace PathfindSanctum;

public class RoomsByLayerFromUI
{
    private static readonly Dictionary<string, string> RewardMapping = new Dictionary<string, string>
    {
        { "Awards a Large Sacred Water Fountain", "Large Fountain" },
        { "Awards a Sacred Water Fountain", "Fountain" },
        { "Awards a Pledge which can be accepted to change the Trial's Parameters", "Pledge to Kochai"},
        { "Awards a Shrine to restore Honour and gain Sacred Water", "Honour Halani" },
        { "Awards a Shrine that greatly restores Honour and burdens you with an Affliction", "Honour Ahkeli" },
        { "Awards a Shrine that bestows the fickle Blessings of the Wind", "Honour Galai" },
        { "Awards a Shrine to restore Honour", "Honour Tabana" },
        { "Contains Merchant", "Merchant" },
        { "Awards Bronze Key", "Bronze Key" },
        { "Awards Silver Key", "Silver Key" },
        { "Awards Gold Key", "Gold Key" },
        { "Awards a Bronze Cache. Requires a Bronze Key to Open", "Bronze Cache" },
        { "Awards a Silver Cache. Requires a Silver Key to Open", "Silver Cache" },
        { "Awards a Gold Cache. Requires a Gold Key to Open", "Gold Cache" }
    };

    private static readonly Dictionary<string, string> RoomTypeMapping = new Dictionary<string, string>
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

    public class FakeSanctumRoomElement
    {
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

        public RoomData Data { get; private set; } = new RoomData();
        public Vector2 Position { get; set; }
        public RectangleF ClientRect { get; set; }

        public RectangleF GetClientRect() => ClientRect;

        public void UpdateFromTooltip(string roomType, string affliction, string reward)
        {
            if (roomType != null)
            {
                Data.FightRoom = new FightRoom { RoomType = new RoomType { Id = roomType } };
            }
            if (affliction != null)
            {
                Data.RoomEffect = new RoomEffect { ReadableName = affliction };
            }
            if (reward != null)
            {
                Data.RewardRoom = new RewardRoom { RoomType = new RoomType { Id = reward } };
            }
        }
    }

    public static List<List<FakeSanctumRoomElement>> GetRoomsByLayer(dynamic floorWindow)
    {
        var result = new List<List<FakeSanctumRoomElement>>();

        var layersContainer = floorWindow?.GetChildAtIndex(0)?.GetChildAtIndex(0)?.GetChildAtIndex(1);
        if (layersContainer == null) return result;

        // For every layer
        for (int i = 0; i < layersContainer.Children.Count; i++)
        {
            var layer = new List<FakeSanctumRoomElement>();
            var layerElement = layersContainer.Children[i];
            
            // For every room
            foreach (var roomElement in layerElement.Children)
            {
                if (roomElement?.Tooltip?.Children == null || roomElement.Tooltip.Children.Count == 0)
                {
                    // Completely unknown room
                    var sanctumRoomTemp = new FakeSanctumRoomElement
                    {
                        ClientRect = roomElement.GetClientRect(),
                        Position = roomElement.GetClientRect().TopLeft
                    };
                    sanctumRoomTemp.UpdateFromTooltip(null, null, null);
                    layer.Add(sanctumRoomTemp);
                    continue;
                }

                // Process all tooltip texts
                var tooltipTexts = new List<string>();
                if (roomElement?.Tooltip?.Children != null)
                {
                    for (int tooltipChildIndex = 0; tooltipChildIndex < roomElement.Tooltip.Children.Count; tooltipChildIndex++)
                    {
                        var tooltipChild = roomElement.Tooltip.Children[tooltipChildIndex];
                        if (tooltipChild?.Children != null)
                        {
                            for (int textChildIndex = 0; textChildIndex < tooltipChild.Children.Count; textChildIndex++)
                            {
                                var text = tooltipChild.Children[textChildIndex]?.Text;
                                if (!string.IsNullOrEmpty(text))
                                {
                                    tooltipTexts.Add(text);
                                }
                            }
                        }
                    }
                }

                // if (tooltipTexts.Count == 0)
                // {
                //     layer.Add(null);
                //     continue;
                // }

                string roomType = null;
                string affliction = null;
                string reward = null;

                // Parse room type, affliction, and reward from all tooltip texts
                foreach (var tooltipText in tooltipTexts)
                {
                    if (RewardMapping.TryGetValue(tooltipText, out var mappedReward))
                    {
                        reward = mappedReward;
                    } else if (tooltipText.Contains("<sanctumcurse>"))
                    {
                        // Extract affliction from between curly braces
                        var startBrace = tooltipText.IndexOf("{");
                        var endBrace = tooltipText.IndexOf("}");
                        if (startBrace >= 0 && endBrace > startBrace)
                        {
                            affliction = tooltipText[(startBrace + 1)..endBrace];
                        }
                    }
                    else if (RoomTypeMapping.TryGetValue(tooltipText, out var mappedRoomType))
                    {
                        roomType = mappedRoomType;
                    }
                    // else
                    // {
                    //     DebugWindow.LogMsg($"Unknown Tooltip: {tooltipText}", 10);
                    // }
                }

                var sanctumRoom = new FakeSanctumRoomElement
                {
                    ClientRect = roomElement.GetClientRect(),
                    Position = roomElement.GetClientRect().TopLeft
                };
                sanctumRoom.UpdateFromTooltip(roomType, affliction, reward);

                // DebugWindow.LogMsg($"Room: Type={roomType ?? "null"}, Reward={reward ?? "null"}, Affliction={affliction ?? "null"}, Pos={sanctumRoom.Position}");
                layer.Add(sanctumRoom);
            }
            result.Add(layer);
        }
        return result;
    }
} 