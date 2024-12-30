using System.Collections.Generic;

namespace PathfindSanctum;

public class ProfileContent
{
    public Dictionary<string, float> RoomTypeWeights { get; set; }
    public Dictionary<string, float> AfflictionWeights { get; set; }
    public Dictionary<string, float> RewardWeights { get; set; }

    private static ProfileContent CreateBaseProfile()
    {
        return new ProfileContent
        {
            RoomTypeWeights = new()
            {
                ["Gauntlet"] = -1000, // Annoying
                ["Hourglass"] = -250, // Dense mobs but manageable with defenses
                ["Chalice"] = 0, // Neutral
                ["Ritual"] = 0, // Neutral
                ["Escape"] = 100, // Safe, controlled environment
                ["Boss"] = 0, // Required
            },
            AfflictionWeights = new()
            {
                // Major afflictions - normal profile
                ["Corrosive Concoction"] = 0, // You have no [Defences]
                ["Glass Shard"] = 0, // The next [Boons|Boon] you gain is converted into a random Minor [Afflictions|Affliction]
                ["Chiselled Stone"] = 0, // Monsters [Petrify] on Hit
                ["Orb of Negation"] = 0, // Non-[ItemRarity|Unique] [Relic|Relics] have no Effect
                ["Unquenched Thirst"] = 0, // You cannot gain [SacredWater|Sacred Water]
                ["Unassuming Brick"] = 0, // You cannot gain any more [Boons]
                ["Orbala's Leathers"] = 0, // Unknown
                ["Ghastly Scythe"] = 0, // Losing [Honour] ends the Trial (removed after 3 rooms)
                ["Veiled Sight"] = 0, // Rooms are unknown on the Trial Map
                ["Branded Balbalakh"] = 0, // Cannot restore [Honour]
                ["Death Toll"] = -500, // Take {0} [Physical] Damage after completing the next Room || ? Monsters no longer drop [SacredWater|Sacred Water]
                ["Deadly Snare"] = 0, // Traps deal Triple Damage
                // Minor afflictions - normal profile
                ["Shattered Shield"] = 0, // You have no [EnergyShield|Energy Shield]
                ["Sharpened Arrowhead"] = 0, // You have no [Armour]
                ["Iron Manacles"] = 0, // You have no [Evasion]
                ["Tradition's Demand"] = 0, // The Merchant only offers one choice
                ["Worn Sandals"] = -250, // 25% reduced Movement Speed
                ["Fiendish Wings"] = 0, // Monsters' Action Speed cannot be slowed below base
                ["Weakened Flesh"] = 0, // 25% less Maximum [Honour]
                ["Rusted Mallet"] = 0, // Monsters always [Knockback]
                ["Untouchable"] = 0, // You are [Curse|Cursed] with [Enfeeble]
                ["Chains of Binding"] = 0, // Monsters inflict [BindingChains|Binding Chains] on [HitDamage|Hit]
                ["Costly Aid"] = 0, // Gain a random Minor [Afflictions|Affliction] when you venerate a Maraketh Shrine
                ["Myriad Aspersions"] = 0, // When you gain an [Afflictions|Affliction], gain an additional random Minor [Afflictions|Affliction]
                ["Season of Famine"] = 0, // The Merchant offers 50% fewer choices
                ["Forgotten Traditions"] = 0, // 50% reduced Effect of your Non-[ItemRarity|Unique] [Relic|Relics]
                ["Haemorrhage"] = 0, // You cannot restore [Honour] (removed after killing the next Boss)
                ["Deceptive Mirror"] = 0, // You are not always taken to the room you select
                ["Dishonoured Tattoo"] = 0, // 100% increased Damage Taken while on [LowLife|Low Life]
                ["Tattered Blindfold"] = 0, // 90% reduced Light Radius
                ["Spiked Exit"] = 0, // Take {0} [Physical] Damage on Room Completion
                ["Purple Smoke"] = 0, // [Afflictions] are unknown on the Trial Map
                ["Golden Smoke"] = 0, // Rewards are unknown on the Trial Map
                ["Red Smoke"] = 0, // Room types are unknown on the Trial Map
                ["Winter Drought"] = 0, // Lose all [SacredWater|Sacred Water] on floor completion
                ["Trade Tariff"] = 0, // 50% increased Merchant prices
                ["Black Smoke"] = 0, // You can see one fewer room ahead on the Trial Map
                ["Suspected Sympathiser"] = 0, // 50% reduced [Honour] restored
                ["Hungry Fangs"] = 0, // Monsters remove 5% of your Life, Mana and [EnergyShield|Energy Shield] on [HitDamage|Hit]
                ["Spiked Shell"] = 0, // Monsters have 50% increased Maximum Life
                ["Exhausted Wells"] = 0, // Chests no longer grant [SacredWater|Sacred Water]
                ["Gate Toll"] = 0, // Lose 30 [SacredWater|Sacred Water] on room completion
                ["Leaking Waterskin"] = 0, // Lose 20 [SacredWater|Sacred Water] when you take Damage from an Enemy [HitDamage|Hit]
                ["Rapid Quicksand"] = -300, // Traps are faster
                ["Blunt Sword"] = 0, // You and your Minions deal 40% less Damage
                ["Low Rivers"] = 0, // 50% less [SacredWater|Sacred Water] found
                ["Dark Pit"] = 0, // Traps deal 100% increased Damage
                ["Honed Claws"] = 0, // Monsters deal 30% more Damage
            },
            RewardWeights = new()
            {
                ["Gold Key"] = 0,
                ["Silver Key"] = 0,
                ["Bronze Key"] = 0,
                ["Large Fountain"] = 0,
                ["Fountain"] = 0,
                ["Pledge to Kochai"] = 0,
                ["Honour Halani"] = 0,
                ["Honour Ahkeli"] = 0,
                ["Honour Orbala"] = 0,
                ["Honour Galai"] = 0,
                ["Honour Tabana"] = 0,
                ["Golden Cache"] = 0,
                ["Silver Cache"] = 0,
                ["Bronze Cache"] = 0,
                ["Merchant"] = 0
            }
        };
    }

    public static ProfileContent CreateDefaultProfile()
    {
        var profile = CreateBaseProfile();

        return profile;
    }

    public static ProfileContent CreateNoHitProfile()
    {
        var profile = CreateBaseProfile();

        profile.RoomTypeWeights["Gauntlet"] = -250; // Predictable traps
        profile.RoomTypeWeights["Hourglass"] = -750; // Dangerous mob density

        profile.AfflictionWeights["Death Toll"] = -500000; // Run-Ending
        profile.AfflictionWeights["Spiked Exit"] = -600000; // Run-Ending

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
