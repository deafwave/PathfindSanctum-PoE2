# PathfindSanctum

If you like it, you can donate via:

BTC: bc1qke67907s6d5k3cm7lx7m020chyjp9e8ysfwtuz

ETH: 0x3A37B3f57453555C2ceabb1a2A4f55E0eB969105 

## Profile Content Update
https://poe2db.tw/Trial_of_the_Sekhemas#Affliction
```JavaScript
const allAfflictions = document.querySelectorAll("div#Affliction div.flex-grow-1.ms-2");
let afflictionParsedList = [];

allAfflictions.forEach((oneAffliction) => {
    const splitData = oneAffliction.innerText.split("\n");
    afflictionParsedList.push(`["${splitData[0]}"] = 0, // ${splitData[2]}`);
});

copy(afflictionParsedList);
```

## Known weirdness

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
