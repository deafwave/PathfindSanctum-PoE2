# PathfindSanctum

If you like it, you can donate via:

BTC: bc1qke67907s6d5k3cm7lx7m020chyjp9e8ysfwtuz

ETH: 0x3A37B3f57453555C2ceabb1a2A4f55E0eB969105 

## What does it do?

Helps players navigate the Sanctum by calculating and visualizing the optimal path through rooms based on customizable weights for different room types, afflictions, rewards, and connectivity.

Automatically updates weights as you progress through the Sanctum, taking into account your current position and providing a visual guide for the most beneficial route based on the profiles preferences and risk tolerance.

## Known exCore2 Weirdness

RoomsByLayer
- POOR [WORKAROUND](./RoomsByLayerFromUI.cs) WAS CREATED
- Offset Missing - [Empty]

RoomData
- UNUSED DUE TO HAVING NO WAY TO TARGET WHAT ROOM IT MEANS
- Seems to be missing rooms? Only showed 10 of ~25

Max Honour
- Player.Stats.TotalSanctumHonour

Sacred Water
- FloorData.Gold (just needs to be renamed)

Inspiration
- No longer exists (can be deleted from EC2)

Current Honour
- Offset Missing - 0
