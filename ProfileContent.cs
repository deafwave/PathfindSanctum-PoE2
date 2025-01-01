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
                ["Hourglass"] = -200, // Dense mobs but manageable with defenses
                ["Chalice"] = 0, // Neutral
                ["Ritual"] = 0, // Neutral
                ["Escape"] = 100, // Safe, controlled environment
                ["Boss"] = 0, // Required
            },
            AfflictionWeights = new()
            {
                ["Orbala's Leathers"] = 0, // Unknown
                // Can get trapped
                ["Glass Shard"] = -4000, // The next [Boons|Boon] you gain is converted into a random Minor [Afflictions|Affliction] ||| (Average weight of all afflictions you don't have)
                ["Ghastly Scythe"] = -4000, // Losing [Honour] ends the Trial (removed after 3 rooms)
                ["Veiled Sight"] = -4000, // Rooms are unknown on the Trial Map
                ["Myriad Aspersions"] = -4000, // When you gain an [Afflictions|Affliction], gain an additional random Minor [Afflictions|Affliction]
                ["Deceptive Mirror"] = -4000, // You are not always taken to the room you select
                ["Purple Smoke"] = -4000, // [Afflictions] are unknown on the Trial Map
                ["Golden Smoke"] = -400, // Rewards are unknown on the Trial Map
                ["Red Smoke"] = -4000, // Room types are unknown on the Trial Map
                ["Black Smoke"] = -4000, // You can see one fewer room ahead on the Trial Map
                // Quickly makes you lose honour
                ["Rapid Quicksand"] = -1000, // Traps are faster
                ["Deadly Snare"] = -1000, // Traps deal Triple Damage
                // Less profit (worse for no-hit runs)
                ["Forgotten Traditions"] = -1000, // 50% reduced Effect of your Non-[ItemRarity|Unique] [Relic|Relics]
                ["Season of Famine"] = -1000, // The Merchant offers 50% fewer choices
                ["Orb of Negation"] = -1000, // Non-[ItemRarity|Unique] [Relic|Relics] have no Effect
                ["Winter Drought"] = -1000, // Lose all [SacredWater|Sacred Water] on floor completion
                // Problematic if build is weak
                ["Branded Balbalakh"] = -1000, // Cannot restore [Honour]
                ["Chiselled Stone"] = -1000, // Monsters [Petrify] on Hit
                ["Weakened Flesh"] = -100, // 25% less Maximum [Honour]
                ["Untouchable"] = -1000, // You are [Curse|Cursed] with [Enfeeble]
                ["Costly Aid"] = -1000, // Gain a random Minor [Afflictions|Affliction] when you venerate a Maraketh Shrine
                ["Blunt Sword"] = -1000, // You and your Minions deal 40% less Damage
                ["Spiked Shell"] = -1000, // Monsters have 50% increased Maximum Life
                ["Suspected Sympathiser"] = -200, // 50% reduced [Honour] restored
                ["Haemorrhage"] = -100, // You cannot restore [Honour] (removed after killing the next Boss)
                // Problematic for certain builds (handled Dynamically)
                ["Corrosive Concoction"] = 0, // You have no [Defences] ||| Only matters if you're EV or ES based
                ["Iron Manacles"] = 0, // You have no [Evasion] ||| Only matters if you're EV based
                ["Shattered Shield"] = 0, // You have no [EnergyShield|Energy Shield] ||| Only matters if you're ES based
                // Sucks
                ["Unquenched Thirst"] = -200, // You cannot gain [SacredWater|Sacred Water] ||| Floor 4 this is nearly-free, depends on how many merchants you got left
                ["Unassuming Brick"] = -1000, // You cannot gain any more [Boons] ||| Floor 4 this is nearly-free, depends on how many merchants you got left
                ["Tradition's Demand"] = -800, // The Merchant only offers one choice
                ["Fiendish Wings"] = -400, // Monsters' Action Speed cannot be slowed below base ||| Matters more if you're freezing/electrocuting the target
                ["Hungry Fangs"] = -600, // Monsters remove 5% of your Life, Mana and [EnergyShield|Energy Shield] on [HitDamage|Hit]
                ["Worn Sandals"] = -400, // 25% reduced Movement Speed
                ["Trade Tariff"] = -300, // 50% increased Merchant prices
                // Nearly Free
                ["Death Toll"] = -400, // Take {0} [Physical] Damage after completing the next Room || ? Monsters no longer drop [SacredWater|Sacred Water]
                ["Spiked Exit"] = -300, // Take {0} [Physical] Damage on Room Completion
                ["Exhausted Wells"] = 0, // Chests no longer grant [SacredWater|Sacred Water]
                ["Gate Toll"] = -100, // Lose 30 [SacredWater|Sacred Water] on room completion
                ["Leaking Waterskin"] = -100, // Lose 20 [SacredWater|Sacred Water] when you take Damage from an Enemy [HitDamage|Hit]
                ["Low Rivers"] = -100, // 50% less [SacredWater|Sacred Water] found
                // Free
                ["Sharpened Arrowhead"] = 0, // You have no [Armour]
                ["Rusted Mallet"] = 0, // Monsters always [Knockback]
                ["Chains of Binding"] = 0, // Monsters inflict [BindingChains|Binding Chains] on [HitDamage|Hit]
                ["Dishonoured Tattoo"] = 0, // 100% increased Damage Taken while on [LowLife|Low Life]
                ["Tattered Blindfold"] = 0, // 90% reduced Light Radius
                ["Dark Pit"] = 0, // Traps deal 100% increased Damage
                ["Honed Claws"] = 0, // Monsters deal 30% more Damage
                // Free Non-Melee
            },
            RewardWeights = new()
            {
                ["Gold Key"] = 0,
                ["Silver Key"] = 0,
                ["Bronze Key"] = 0,
                ["Large Fountain"] = 100,
                ["Fountain"] = 20,
                ["Pledge to Kochai"] = 7,
                ["Honour Halani"] = 8,
                ["Honour Ahkeli"] = -1,
                ["Honour Orbala"] = 9,
                ["Honour Galai"] = 10, // Need to understand what this does
                ["Honour Tabana"] = 0,
                ["Golden Cache"] = 0,
                ["Silver Cache"] = 0,
                ["Bronze Cache"] = 0,
                ["Merchant"] = 20 // Not important if sacred water is below ~360 (less with relics)
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

        profile.RoomTypeWeights["Gauntlet"] = -200; // Predictable traps
        profile.RoomTypeWeights["Hourglass"] = -1000; // Dangerous mob density

        profile.AfflictionWeights["Death Toll"] = -500000; // Run-Ending
        profile.AfflictionWeights["Spiked Exit"] = -600000; // Run-Ending
        profile.AfflictionWeights["Deceptive Mirror"] = -400000; // Run-Ending

        profile.AfflictionWeights["Glass Shard"] = -50000; // ~3/58 chance of ending your run
        profile.AfflictionWeights["Myriad Aspersions"] = -50000; // ~3/58 chance of ending your run every time you mess up

        // Free
        profile.AfflictionWeights["Ghastly Scythe"] = 0;
        profile.AfflictionWeights["Deadly Snare"] = 0;
        profile.AfflictionWeights["Branded Balbalakh"] = 0;
        profile.AfflictionWeights["Chiselled Stone"] = 0;
        profile.AfflictionWeights["Weakened Flesh"] = 0;
        profile.AfflictionWeights["Costly Aid"] = 0;
        profile.AfflictionWeights["Suspected Sympathiser"] = 0;
        profile.AfflictionWeights["Haemorrhage"] = 0;
        profile.AfflictionWeights["Leaking Waterskin"] = 0;
        profile.AfflictionWeights["Rusted Mallet"] = 0;
        profile.AfflictionWeights["Chains of Binding"] = 0;
        profile.AfflictionWeights["Dishonoured Tattoo"] = 0;
        profile.AfflictionWeights["Dark Pit"] = 0;
        profile.AfflictionWeights["Honed Claws"] = 0;
        profile.AfflictionWeights["Hungry Fangs"] = 0;

        // Depends if the relic strategy works
        // ["Forgotten Traditions"] = -1000, // 50% reduced Effect of your Non-[ItemRarity|Unique] [Relic|Relics]
        // ["Orb of Negation"] = -1000, // Non-[ItemRarity|Unique] [Relic|Relics] have no Effect

        // Depends if you can't get your combo off in tim
        // ["Fiendish Wings"] = -600, // Monsters' Action Speed cannot be slowed below base ||| Matters more if you're freezing/electrocuting the target
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
