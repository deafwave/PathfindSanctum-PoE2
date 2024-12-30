using System.Collections.Generic;
using System.Numerics;
using ExileCore2;
using ExileCore2.Shared;

namespace PathfindSanctum;

public class RoomsByLayerFromUI
{
    public class SanctumRoomData
    {
        public string RoomType { get; set; }
        public string Affliction { get; set; }
        public string Reward { get; set; }
        public Vector2 Position { get; set; }
        public RectangleF ClientRect { get; set; }
    }

    public class UIRoom
    {
        public string RoomType { get; set; }
        public string Affliction { get; set; }
        public string Reward { get; set; }
        public SanctumRoomData RoomData { get; set; }
    }

    public static List<List<UIRoom>> GetRoomsByLayer(dynamic floorWindow)
    {
        var result = new List<List<UIRoom>>();
        
        var layersContainer = floorWindow?.GetChildAtIndex(0)?.GetChildAtIndex(0)?.GetChildAtIndex(1);
        if (layersContainer == null) return result;

        // DebugWindow.LogMsg($"Layer children count: {layersContainer.Children.Count}");

        // For every layer
        for (int i = 0; i < layersContainer.Children.Count; i++)
        {
            var layer = new List<UIRoom>();
            var layerElement = layersContainer.Children[i];
            
            // For every room
            foreach (var roomElement in layerElement.Children)
            {
                if (roomElement?.Tooltip?.Children == null || roomElement.Tooltip.Children.Count == 0)
                {
                    layer.Add(null); // Completely unknown room
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

                if (tooltipTexts.Count == 0)
                {
                    layer.Add(null);
                    continue;
                }

                var roomData = new SanctumRoomData 
                { 
                    Position = roomElement.GetClientRect().Center,
                    ClientRect = roomElement.GetClientRect()
                };

                var room = new UIRoom { RoomData = roomData };

                // Parse room type, affliction, and reward from all tooltip texts
                foreach (var tooltipText in tooltipTexts)
                {
                    if (tooltipText.StartsWith("Awards"))
                    {
                        room.Reward = tooltipText.Replace("Awards ", "");
                        roomData.Reward = room.Reward;
                    }
                    else if (tooltipText.Contains("Afflicts you with"))
                    {
                        var afflictionStart = tooltipText.IndexOf("{") + 1;
                        var afflictionEnd = tooltipText.IndexOf("}");
                        if (afflictionStart > 0 && afflictionEnd > afflictionStart)
                        {
                            room.Affliction = tooltipText.Substring(afflictionStart, afflictionEnd - afflictionStart);
                            roomData.Affliction = room.Affliction;
                        }
                    }
                    else
                    {
                        // Assume it's a room type if it doesn't match other patterns
                        room.RoomType = tooltipText;
                        roomData.RoomType = room.RoomType;
                    }
                }

                DebugWindow.LogMsg($"Room: Type={room.RoomType ?? "null"}, Reward={room.Reward ?? "null"}, Affliction={room.Affliction ?? "null"}, Pos={roomData.Position}");
                layer.Add(room);
            }
            
            result.Add(layer);
        }

        return result;
    }
} 