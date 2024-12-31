# TODO

## Sanctum State Tracker
- Test SanctumStateTracker when you acquire a smoke affliction

## Features
- Find a way to read active afflictions
    - Account for active afflictions (if we have +1 affliction, it makes getting afflictions significantly worse)
- Find a way to read active relic bonuses
    - [If 100% reduced trap damage taken -> 0 honour taken] -> set to 0 a bunch of afflictions (can reduce them dynamically by a %, but I don't think it's a high priority)

## Weights
- Finish dynamic weights
- Check if weights are actually accurate / useful
- Swap to RoomsByLayer once it's available
- Replace the static dictionaries with e.g. files.SanctumPersistentEffects so it has less maintenance costs as poe2 updates

## Settings
- Expose ProfileContent weights
- Expose Dynamic Weights
- Add Factory Reset Weights button with confirmation