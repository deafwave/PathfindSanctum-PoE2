using System.Collections.Generic;
using System.Numerics;
using ExileCore2;
using ExileCore2.Shared;
using ExileCore2.PoEMemory.Elements.Sanctum;

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

    public static List<List<SanctumRoomElement>> GetRoomsByLayer(dynamic floorWindow)
    {
        var result = new List<List<SanctumRoomElement>>();
        
        var layersContainer = floorWindow?.GetChildAtIndex(0)?.GetChildAtIndex(0)?.GetChildAtIndex(1);
        if (layersContainer == null) return result;

        // For every layer
        for (int i = 0; i < layersContainer.Children.Count; i++)
        {
            var layer = new List<SanctumRoomElement>();
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

                // Create a new SanctumRoomElement with the data we have
                var roomData = new SanctumRoomElement();

                string roomType = null;
                string affliction = null;
                string reward = null;

                // Parse room type, affliction, and reward from all tooltip texts
                foreach (var tooltipText in tooltipTexts)
                {
                    if (tooltipText.StartsWith("Awards"))
                    {
                        reward = tooltipText.Replace("Awards ", "");
                    }
                    else if (tooltipText.Contains("Afflicts you with"))
                    {
                        var afflictionStart = tooltipText.IndexOf("{") + 1;
                        var afflictionEnd = tooltipText.IndexOf("}");
                        if (afflictionStart > 0 && afflictionEnd > afflictionStart)
                        {
                            affliction = tooltipText.Substring(afflictionStart, afflictionEnd - afflictionStart);
                        }
                    }
                    else
                    {
                        // Assume it's a room type if it doesn't match other patterns
                        roomType = tooltipText;
                    }
                }

                // Since we can't set properties directly, we'll just use the room element itself
                // The properties will be null/default but at least we have the client rect
                var sanctumRoom = roomElement as SanctumRoomElement;
                if (sanctumRoom != null)
                {
                    DebugWindow.LogMsg($"Room: Type={roomType ?? "null"}, Reward={reward ?? "null"}, Affliction={affliction ?? "null"}, Pos={sanctumRoom.GetClientRect().Center}");
                    layer.Add(sanctumRoom);
                }
                else
                {
                    layer.Add(null);
                }
            }
            
            result.Add(layer);
        }

        return result;
    }
} 