using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Buddy.Swtor;
using Buddy.Swtor.Objects;

namespace PureSWTor.Helpers
{
    public static class Tunables
    {
        public delegate Global.PartyRole PartyRoleDelegate(TorCharacter torCharacter);

        #region Class Tunables
        // This unifies handling of class generic buffing & healing...
        // Previously, various (disparate) constants were used inconsistently thoughout the code.
        private static readonly ClassTunables TunablesForCompanion = new ClassTunables()
        {
            IsRejuvenationNeeded = (torPlayer) => { return (torPlayer.IsCompanionInUse() && !torPlayer.Companion.IsDead && (torPlayer.Companion.HealthPercent < 70)); },
            IsRejuvenationComplete = (torPlayer) => { return (!torPlayer.IsCompanionInUse() || torPlayer.Companion.IsDead || (torPlayer.Companion.HealthPercent >= 95)); }
        };

        private static readonly IEnumerable<ClassTunables> TunablesByClass = new List<ClassTunables>
        {
            // Republic
            { new ClassTunables() {
                Class = CharacterClass.Knight,
                RejuvenateAbilityName = "Introspection",
                SelfBuffName = "Force Might",
                IsRejuvenationNeeded = (torPlayer) => { return (torPlayer.HealthPercent < 70); },
                IsRejuvenationComplete = (torPlayer) => { return (torPlayer.HealthPercent >= 95); },
                NormalizedResource = (torPlayer) => { return (torPlayer.ResourceStat); }
                }},

            { new ClassTunables() {
                Class = CharacterClass.Consular,
                RejuvenateAbilityName = "Meditation",
                SelfBuffName = "Force Valor",
                IsRejuvenationNeeded = (torPlayer) => { return ((torPlayer.HealthPercent < 70) || (torPlayer.ResourceStat < 70)); },
                IsRejuvenationComplete = (torPlayer) => { return ((torPlayer.HealthPercent >= 95) && (torPlayer.ResourceStat >= 95)); },
                NormalizedResource = (torPlayer) => { return (torPlayer.ResourceStat); }
                }},

            { new ClassTunables() {
                Class = CharacterClass.Smuggler,
                RejuvenateAbilityName = "Recuperate",
                SelfBuffName = "Lucky Shots",
                IsRejuvenationNeeded = (torPlayer) => { return ((torPlayer.HealthPercent < 70) || (torPlayer.ResourceStat < 70)); },
                IsRejuvenationComplete = (torPlayer) => { return ((torPlayer.HealthPercent >= 95) && (torPlayer.ResourceStat >= 95)); },
                NormalizedResource = (torPlayer) => { return (torPlayer.ResourceStat); }
                }},

            { new ClassTunables() {
                Class = CharacterClass.Trooper,
                RejuvenateAbilityName = "Recharge and Reload",
                SelfBuffName = "Fortification",
                IsRejuvenationNeeded = (torPlayer) => { return ((torPlayer.HealthPercent < 70) || (torPlayer.ResourceStat < 70)); },
                IsRejuvenationComplete = (torPlayer) => { return ((torPlayer.HealthPercent >= 95) && (torPlayer.ResourceStat > 95)); },
                NormalizedResource = (torPlayer) => { return (torPlayer.ResourceStat); }
                }},

            // Empire
            { new ClassTunables() {
                Class = CharacterClass.Warrior,
                RejuvenateAbilityName = "Channel Hatred",
                SelfBuffName = "Unnatural Might",
                IsRejuvenationNeeded = (torPlayer) => { return (torPlayer.HealthPercent < 70); },
                IsRejuvenationComplete = (torPlayer) => { return (torPlayer.HealthPercent >= 95); },
                NormalizedResource = (torPlayer) => { return (torPlayer.ResourceStat); }
                }},

            { new ClassTunables() {
                Class = CharacterClass.Inquisitor,
                RejuvenateAbilityName = "Seethe",
                SelfBuffName = "Mark of Power",
                IsRejuvenationNeeded = (torPlayer) => { return ((torPlayer.HealthPercent < 70) || (torPlayer.ResourceStat < 70)); },
                IsRejuvenationComplete = (torPlayer) => { return ((torPlayer.HealthPercent >= 95) && (torPlayer.ResourceStat >= 95)); },
                NormalizedResource = (torPlayer) => { return (torPlayer.ResourceStat); }
                }},

            { new ClassTunables() {
                Class = CharacterClass.Agent,
                RejuvenateAbilityName = "Recuperate",
                SelfBuffName = "Coordination",
                IsRejuvenationNeeded = (torPlayer) => { return ((torPlayer.HealthPercent < 70) || (torPlayer.ResourceStat < 70)); },
                IsRejuvenationComplete = (torPlayer) => { return ((torPlayer.HealthPercent >= 95) && (torPlayer.ResourceStat >= 95)); },
                NormalizedResource = (torPlayer) => { return (torPlayer.ResourceStat); }
                }},

            { new ClassTunables() {
                Class = CharacterClass.BountyHunter,
                RejuvenateAbilityName="Recharge and Reload",
                SelfBuffName="Hunter's Boon",
                IsRejuvenationNeeded = (torPlayer) => { return ((torPlayer.HealthPercent < 70) || (torPlayer.ResourceStat > 30)); },
                IsRejuvenationComplete = (torPlayer) => { return ((torPlayer.HealthPercent >= 95) && (torPlayer.ResourceStat <= 5)); },
                NormalizedResource = (torPlayer) => { return (100.0f - Math.Min(torPlayer.ResourceStat, 100.0f)); }
                }},

            // Boundary condition --
            { new ClassTunables() {
                Class = CharacterClass.Unknown,
                RejuvenateAbilityName="UNDEFINED",
                SelfBuffName="UNDEFINED",
                IsRejuvenationNeeded = (torPlayer) => { return (false); },
                IsRejuvenationComplete = (torPlayer) => { return (true); },
                NormalizedResource = (torPlayer) => { return (0.0f); }
                }}
        };
        #endregion  // Class and Companion Tunables

        // Derived data structure for quick lookup --
        private static readonly Dictionary<CharacterClass, ClassTunables> TunablesMap = TunablesByClass.ToDictionary(x => x.Class, x => x);


        #region Party Roles
        public static Dictionary<Tuple<CharacterClass, SkillTreeId>, Global.PartyRole> PartyRole_BySpec = new Dictionary<Tuple<CharacterClass, SkillTreeId>, Global.PartyRole>()
        {
            // Basic Classes, Empire
            { Tuple.Create(CharacterClass.Warrior, SkillTreeId.None), Global.PartyRole.MeleeDPS },
            { Tuple.Create(CharacterClass.Inquisitor, SkillTreeId.None), Global.PartyRole.RangedDPS },
            { Tuple.Create(CharacterClass.Agent, SkillTreeId.None), Global.PartyRole.RangedDPS },
            { Tuple.Create(CharacterClass.BountyHunter, SkillTreeId.None), Global.PartyRole.RangedDPS },

            // Advanced Classes, Empire
            { Tuple.Create(CharacterClass.Juggernaut, SkillTreeId.JuggernautImmortal), Global.PartyRole.MeleeTank },
            { Tuple.Create(CharacterClass.Juggernaut, SkillTreeId.JuggernautRage), Global.PartyRole.MeleeDPS },
            { Tuple.Create(CharacterClass.Juggernaut, SkillTreeId.JuggernautVengeance), Global.PartyRole.MeleeDPS  },

            { Tuple.Create(CharacterClass.Marauder, SkillTreeId.MarauderAnnihilation), Global.PartyRole.MeleeDPS },
            { Tuple.Create(CharacterClass.Marauder, SkillTreeId.MarauderCarnage), Global.PartyRole.MeleeDPS },
            { Tuple.Create(CharacterClass.Marauder, SkillTreeId.MarauderRage),  Global.PartyRole.MeleeDPS },

            { Tuple.Create(CharacterClass.Assassin, SkillTreeId.AssassinDarkness), Global.PartyRole.MeleeTank },
            { Tuple.Create(CharacterClass.Assassin, SkillTreeId.AssassinDeception), Global.PartyRole.MeleeDPS },
            { Tuple.Create(CharacterClass.Assassin, SkillTreeId.AssassinMadness), Global.PartyRole.MeleeDPS },

            { Tuple.Create(CharacterClass.Sorcerer, SkillTreeId.SorcererCorruption), Global.PartyRole.Healer },
            { Tuple.Create(CharacterClass.Sorcerer, SkillTreeId.SorcererLightning), Global.PartyRole.RangedDPS },
            { Tuple.Create(CharacterClass.Sorcerer, SkillTreeId.SorcererMadness), Global.PartyRole.RangedDPS },

            { Tuple.Create(CharacterClass.Sniper, SkillTreeId.SniperMarksmanship), Global.PartyRole.RangedDPS },
            { Tuple.Create(CharacterClass.Sniper, SkillTreeId.SniperEngineering), Global.PartyRole.RangedDPS },
            { Tuple.Create(CharacterClass.Sniper, SkillTreeId.SniperLethality), Global.PartyRole.RangedDPS },

            { Tuple.Create(CharacterClass.Operative, SkillTreeId.OperativeMedic), Global.PartyRole.Healer },
            { Tuple.Create(CharacterClass.Operative, SkillTreeId.OperativeConcealment), Global.PartyRole.RangedDPS },
            { Tuple.Create(CharacterClass.Operative, SkillTreeId.OperativeLethality), Global.PartyRole.RangedDPS },

            { Tuple.Create(CharacterClass.Mercenary, SkillTreeId.MercenaryArsenal), Global.PartyRole.RangedDPS },
            { Tuple.Create(CharacterClass.Mercenary, SkillTreeId.MercenaryBodyguard), Global.PartyRole.Healer },
            { Tuple.Create(CharacterClass.Mercenary, SkillTreeId.MercenaryFirebug), Global.PartyRole.RangedDPS },

            { Tuple.Create(CharacterClass.Powertech, SkillTreeId.PowertechAdvanced), Global.PartyRole.RangedDPS },
            { Tuple.Create(CharacterClass.Powertech, SkillTreeId.PowertechFirebug), Global.PartyRole.RangedDPS },
            { Tuple.Create(CharacterClass.Powertech, SkillTreeId.PowertechShieldTech), Global.PartyRole.RangedTank },

            // Basic Classes, Republic
            { Tuple.Create(CharacterClass.Knight, SkillTreeId.None), Global.PartyRole.MeleeDPS },
            { Tuple.Create(CharacterClass.Consular, SkillTreeId.None), Global.PartyRole.RangedDPS },
            { Tuple.Create(CharacterClass.Smuggler, SkillTreeId.None), Global.PartyRole.RangedDPS },
            { Tuple.Create(CharacterClass.Trooper, SkillTreeId.None), Global.PartyRole.RangedDPS },

            // Advanced Classes, Republic
            { Tuple.Create(CharacterClass.Guardian, SkillTreeId.GuardianDefense), Global.PartyRole.MeleeTank  },
            { Tuple.Create(CharacterClass.Guardian, SkillTreeId.GuardianFocus), Global.PartyRole.MeleeDPS  },
            { Tuple.Create(CharacterClass.Guardian, SkillTreeId.GuardianVigilance), Global.PartyRole.MeleeDPS  },

            { Tuple.Create(CharacterClass.Sentinel, SkillTreeId.SentinelCombat), Global.PartyRole.MeleeDPS  },
            { Tuple.Create(CharacterClass.Sentinel, SkillTreeId.SentinelFocus), Global.PartyRole.MeleeDPS  },
            { Tuple.Create(CharacterClass.Sentinel, SkillTreeId.SentinelWatchman), Global.PartyRole.MeleeDPS  },

            { Tuple.Create(CharacterClass.Shadow, SkillTreeId.ShadowCombat), Global.PartyRole.MeleeTank },
            { Tuple.Create(CharacterClass.Shadow, SkillTreeId.ShadowInfiltration), Global.PartyRole.MeleeDPS },
            { Tuple.Create(CharacterClass.Shadow, SkillTreeId.ShadowBalance), Global.PartyRole.MeleeDPS },

            { Tuple.Create(CharacterClass.Sage, SkillTreeId.SageBalance), Global.PartyRole.Healer },
            { Tuple.Create(CharacterClass.Sage, SkillTreeId.SageSeer), Global.PartyRole.RangedDPS },
            { Tuple.Create(CharacterClass.Sage, SkillTreeId.SageTelekinetics), Global.PartyRole.RangedDPS },

            { Tuple.Create(CharacterClass.Gunslinger, SkillTreeId.GunslingerDirtyFighting), Global.PartyRole.RangedDPS },
            { Tuple.Create(CharacterClass.Gunslinger, SkillTreeId.GunslingerSaboteur), Global.PartyRole.RangedDPS },
            { Tuple.Create(CharacterClass.Gunslinger, SkillTreeId.GunslingerSharpshooter), Global.PartyRole.RangedDPS },

            { Tuple.Create(CharacterClass.Scoundrel, SkillTreeId.ScoundrelSawbones), Global.PartyRole.Healer },
            { Tuple.Create(CharacterClass.Scoundrel, SkillTreeId.ScoundrelScrapper), Global.PartyRole.RangedDPS },
            { Tuple.Create(CharacterClass.Scoundrel, SkillTreeId.ScoundrelDirtyFighting), Global.PartyRole.RangedDPS },

            { Tuple.Create(CharacterClass.Commando, SkillTreeId.CommandoCombatMedic), Global.PartyRole.Healer },
            { Tuple.Create(CharacterClass.Commando, SkillTreeId.CommandoGunnery), Global.PartyRole.RangedDPS },
            { Tuple.Create(CharacterClass.Commando, SkillTreeId.CommandoAssaultSpecialist), Global.PartyRole.RangedDPS },

            { Tuple.Create(CharacterClass.Vanguard, SkillTreeId.VanguardAssaultSpecialist), Global.PartyRole.RangedDPS},
            { Tuple.Create(CharacterClass.Vanguard, SkillTreeId.VanguardShieldSpecialist), Global.PartyRole.RangedTank },
            { Tuple.Create(CharacterClass.Vanguard, SkillTreeId.VanguardTactics), Global.PartyRole.RangedDPS }
        };

        public static Dictionary<string, PartyRoleDelegate> PartyRole_OfCompanion = new Dictionary<string, PartyRoleDelegate>()
        {
            // -----Republic-----
            { "C2-N2", companion => Global.PartyRole.Healer },

            // Jedi Knight
            { "Doc", companion => companion.HasBuff("Med Watch") ? Global.PartyRole.Healer : Global.PartyRole.RangedDPS },
            { "Kira Carsen", companion => Global.PartyRole.MeleeDPS },
            { "Lord Scourge", companion => companion.HasBuff("Guard Stance") ? Global.PartyRole.MeleeTank : Global.PartyRole.MeleeDPS },
            { "Sergeant Rusk", companion => Global.PartyRole.RangedDPS },
            { "T7-O1", companion => companion.HasBuff("Sentry Process") ? Global.PartyRole.RangedTank : Global.PartyRole.RangedDPS },

            // Jedi Consular
            { "Lieutenant Iresso", companion => companion.HasBuff("Guard Stance") ? Global.PartyRole.RangedTank : Global.PartyRole.RangedDPS },
            { "Nadia Grell", companion => Global.PartyRole.MeleeDPS },
            { "Qyzen Fess", companion => companion.HasBuff("Trandoshan Regeneration") ? Global.PartyRole.MeleeTank : Global.PartyRole.MeleeDPS },
            { "Tharan Cedrax", companion => companion.HasBuff("Med Watch") ? Global.PartyRole.Healer : Global.PartyRole.RangedDPS },
            { "Zenith", companion => Global.PartyRole.RangedDPS },

            // Smuggler
            { "Akaavi Spar", companion => Global.PartyRole.MeleeDPS },
            { "Bowdaar", companion => companion.HasBuff("Guard Stance") ? Global.PartyRole.MeleeTank : Global.PartyRole.MeleeDPS },
            { "Corso Riggs", companion => companion.HasBuff("Guard Stance") ? Global.PartyRole.RangedTank : Global.PartyRole.RangedDPS },
            { "Guss Tuno", companion => companion.HasBuff("Med Watch") ? Global.PartyRole.Healer : Global.PartyRole.RangedDPS },
            { "Risha", companion => Global.PartyRole.RangedDPS },

            // Trooper
            { "Aric Jorgan", companion => Global.PartyRole.RangedDPS },
            { "Elara Dorne", companion => companion.HasBuff("Med Watch") ? Global.PartyRole.Healer : Global.PartyRole.RangedDPS },
            { "M1-4X", companion => companion.HasBuff("Sentry Process") ? Global.PartyRole.RangedTank : Global.PartyRole.RangedDPS },
            { "Tanno Vik", companion => companion.HasBuff("Guard Stance") ? Global.PartyRole.MeleeTank : Global.PartyRole.MeleeDPS },
            { "Yuun", companion => Global.PartyRole.MeleeDPS },

            // -----Empire-----
            { "2V-R8", companion => Global.PartyRole.Healer },

            // Sith Warrior
            { "Broonmark", companion => companion.HasBuff("Guard Stance") ? Global.PartyRole.MeleeTank : Global.PartyRole.MeleeDPS },
            { "Jaesa Willsaam", companion => Global.PartyRole.MeleeDPS },
            { "Lieutenant Pierce", companion => companion.HasBuff("Guard Stance") ? Global.PartyRole.RangedTank : Global.PartyRole.RangedDPS },
            { "Malavai Quinn", companion => companion.HasBuff("Med Watch") ? Global.PartyRole.Healer : Global.PartyRole.RangedDPS },
            { "Vette", companion => Global.PartyRole.RangedDPS },

            // Sith Inquisitor
            { "Andronikos Revel", companion => Global.PartyRole.RangedDPS },
            { "Ashara Zavros", companion => Global.PartyRole.MeleeDPS },
            { "Khem Val", companion => companion.HasBuff("Shadow Killer") ? Global.PartyRole.MeleeTank : Global.PartyRole.MeleeDPS },
            { "Talos Drellik", companion => companion.HasBuff("Med Watch") ? Global.PartyRole.Healer : Global.PartyRole.RangedDPS },
            { "Xalek", companion => companion.HasBuff("Force Barrier") ? Global.PartyRole.RangedTank : Global.PartyRole.MeleeDPS },

            // BountyHunter
            { "Blizz", companion => companion.HasBuff("Guard Stance") ? Global.PartyRole.RangedTank : Global.PartyRole.RangedDPS },
            { "Gault", companion => Global.PartyRole.RangedDPS },
            { "Mako", companion => companion.HasBuff("Med Watch") ? Global.PartyRole.Healer : Global.PartyRole.RangedDPS },
            { "Skadge", companion => companion.HasBuff("Guard Stance") ? Global.PartyRole.MeleeTank : Global.PartyRole.MeleeDPS },
            { "Torian Cadera", companion => Global.PartyRole.MeleeDPS },

            // Imperial Agent
            { "Doctor Lokin", companion => companion.HasBuff("Med Watch") ? Global.PartyRole.Healer : Global.PartyRole.MeleeDPS },
            { "Transformed Lokin", companion => companion.HasBuff("Rakghoul Transformation Mode") ? Global.PartyRole.MeleeTank : Global.PartyRole.MeleeDPS },
            { "Ensign Temple", companion => Global.PartyRole.RangedDPS },
            { "Kaliyo", companion => companion.HasBuff("Guard Stance") ? Global.PartyRole.RangedTank : Global.PartyRole.RangedDPS },
            { "SCORPIO", companion => companion.HasBuff("Guard Stance") ? Global.PartyRole.MeleeTank : Global.PartyRole.MeleeDPS },
            { "Vector", companion => Global.PartyRole.MeleeDPS }
        };

        // PartyRolePriorities are used to determine priorities such as "who should get the heal first, all other things being equivalent".
        // There are separate weights for Players (PC), and Companions (NPC).  The weights should be on the closed interval [0.00f..1.00f].
        public static Dictionary<Global.PartyRole, Tuple<float, float>> PartyRolePriorities = new Dictionary<Global.PartyRole, Tuple<float, float>>()
        {
            // Role                                       PC-weight NPC-weight
            { Global.PartyRole.MeleeTank,    Tuple.Create(1.00f,    0.95f) },
            { Global.PartyRole.RangedTank,   Tuple.Create(1.00f,    0.95f) },
            { Global.PartyRole.Healer,       Tuple.Create(0.90f,    0.85f) },            
            { Global.PartyRole.MeleeDPS,     Tuple.Create(0.75f,    0.50f) },
            { Global.PartyRole.RangedDPS,    Tuple.Create(0.70f,    0.50f) },
        };
        #endregion

        #region Debuff Names
        // This table contains the list of debuff names which represent crowd control...
        // A crowd control debuff is one that last longer than 30 secs.  If the debuff under consideration
        // is short-lived (e.g., 6-10secs), it is not a crowd-control, but an interrupt spell.
        // Some specs vary the spell duration based on the Advanced Spec--such as Whirlwind (60sec for Sorcerer,
        // 6secs for Assassin).  If the spell can be used as crowd-control in any spec, please list it here.
        // Also note the crowd-control ability name is frequently not the name of the crowd-control debuff.
        // For instance, "Whirlwind" CC ability produces the "Lifted (Force)" debuff on the target.
        public static string[] DebuffNames_CrowdControl = {
            "Afraid (Mental)", // from Intimidating Roar / Awe
            "Blinded (Tech)", // from Flash Grenade / Flash Bang
            "Lifted (Force)", // from Whirlwind / Force Lift
            "Sliced", // from Slice Droid
            "Asleep (Mental)", // from Concussive Round & Mind Maze
            "Sleeping", // from Sleep Dart & Tranquilizer
            "Stunned", // from Debilitate / Dirty Kick
        };

        // This table contains the list of debuff names which prevent us from being shielded...
        public static string[] DebuffNames_Shielded = {
            "Deionized",        // Result of Sorcerer shielding
            "Force-imbalance"   // Result of Sage shielding
        };
        #endregion

        #region Interrupt
        // This table contains the list of spell names which we should not interrupt...
        // The spells contained in this table should do little damage, or be very low priority.
        // In short, we'd like to "save" our interrupt for important spells, and not waste it on the
        // spells contained in this table.
        public static string[] Interrupt_IgnoreSpells = {
        };
        #endregion

        #region Relative Difficulty
        // The maximum player level supported by the game...
        public const int MaxPlayerLevel = 50;

        // This assesses the relative toughness of a target (roughly measured as 'time to kill')...
        // The scale is a multiplier with the reference of a player defined to be 1.0.
        public static Dictionary<CombatToughness, float> RelativeDifficulty = new Dictionary<CombatToughness, float>() {
            { CombatToughness.RaidBoss,     5.00f },
            { CombatToughness.Boss4,        3.50f },
            { CombatToughness.Boss3,        3.00f },
            { CombatToughness.Boss2,        2.50f },
            { CombatToughness.Boss1,        2.00f },
            { CombatToughness.Strong,       1.40f },
            { CombatToughness.Player,       1.00f },    // scale reference -- always defined as 1.0
            { CombatToughness.Companion,    0.85f },
            { CombatToughness.Standard,     0.70f },
            { CombatToughness.Weak,         0.50f },
            { CombatToughness.None,         0.00f },
        };
        #endregion

        #region Extension methods
        public static bool IsCompanionRejuvenationComplete(this TorPlayer torPlayer)
        {
            return (TunablesForCompanion.IsRejuvenationComplete(torPlayer));
        }

        public static bool IsCompanionRejuvenationNeeded(this TorPlayer torPlayer)
        {
            return (TunablesForCompanion.IsRejuvenationNeeded(torPlayer));
        }

        /// <summary>
        /// <para>Predicate returns 'true' if the provided TORPLAYER no longer needs healing.</para>
        /// <para>Notes:<list type="bullet">
        /// <item><description><para> * IsRejuvenationNeeded and IsRejuvenationComplete allow hysteresis to occur for healing decisions.</para>
        /// <para> -- healing should start any time IsRejuvenationNeeded is true; healing should cease any time IsRejuvenationComplete is true.</para>
        /// <para> -- The hysteresis prevents decision-stutter.</para></description></item>
        /// </list></para>
        /// </summary>
        ///  1Jul2012-11:50UTC chinajade
        public static bool IsRejuvenationComplete(this TorPlayer torPlayer)
        {
            // Extension methods can never have a null 'this' argument, so no need to enforce a contract here
            return (TunablesMap[torPlayer.Class].IsRejuvenationComplete(torPlayer));
        }

        /// <summary>
        /// <para>Predicate returns 'true' if the provided TORPLAYER needs healing.</para>
        /// <para>Notes:<list type="bullet">
        /// <item><description><para> * IsRejuvenationNeeded and IsRejuvenationComplete allow hysteresis to occur for healing decisions.</para>
        /// <para> -- healing should start any time IsRejuvenationNeeded is true; healing should cease any time IsRejuvenationComplete is true.</para>
        /// <para> -- The hysteresis prevents decision-stutter.</para></description></item>
        /// </list></para>
        /// </summary>
        ///  1Jul2012-11:49UTC chinajade
        public static bool IsRejuvenationNeeded(this TorPlayer torPlayer)
        {
            // Extension methods can never have a null 'this' argument, so no need to enforce a contract here
            return (TunablesMap[torPlayer.Class].IsRejuvenationNeeded(torPlayer));
        }

        /// <summary>
        /// <para>Name of the channelled ability that restores health and resources.</para>
        /// </summary>
        ///  1Jul2012-11:49UTC chinajade
        public static string RejuvenateAbilityName(this TorPlayer torPlayer)
        {
            // Extension methods can never have a null 'this' argument, so no need to enforce a contract here
            return (TunablesMap[torPlayer.Class].RejuvenateAbilityName);
        }

        /// <summary>
        /// <para>Returns a Resource state as a percentage of 'full resources' (on the closed interval [0.0..100.0]).</para>
        /// <para>This mapping of <c>TorPlayer.ResourceStat</c> to a percentage is accomplished in a class-specific fashion</para>
        /// <para>where, 0.0 represents 'empty' or no resources available, and 100.0 represents full resources available.</para>
        /// <para>Notes:<list type="bullet">
        /// <item><description><para> * The Bounty Hunter's resource (heat) is reversed by this paradigm.</para>
        /// <para> -- When heat is 0, the Bounty Hunter has full resources, so this method returns 100.0.</para>
        /// <para> -- When heat is 40, this method returns 60.0. And, when heat is 100, there are no resources left, and this method returns 0.0. </para></description></item>
        /// <item><description><para> * The Trooper's resource (ammo) is scaled up from 12 to represent a percentage of full ammo load.</para>
        /// <para> -- You will observe the trooper's resources changing at a more coarse granularity than other classes.</para>
        /// <para> -- 100% divided by 12 ammo represents an 8.333% change in ResourcePercent.</para></description></item>
        /// </list></para>
        /// </summary>
        /// <param name="torPlayer"></param>
        /// <returns></returns>
        ///  1Jul2012-10:37UTC chinajade
        public static float ResourcePercent(this TorPlayer torPlayer)
        {
            // Extension methods can never have a null 'this' argument, so no need to enforce a contract here
            return (TunablesMap[torPlayer.Class].NormalizedResource(torPlayer));
        }

        /// <summary>
        /// <para>Name of the 60-minute buff applied to self and all party members.</para>
        /// </summary>
        ///  1Jul2012-11:49UTC chinajade
        public static string SelfBuffName(this TorPlayer torPlayer)
        {
            // Extension methods can never have a null 'this' argument, so no need to enforce a contract here
            return (TunablesMap[torPlayer.Class].SelfBuffName);
        }
        #endregion  // Extension methods


        #region Table Schemas
        private delegate bool PredicateDelegate(TorPlayer torPlayer);
        private delegate float NormalizedResourceDelegate(TorPlayer torPlayer);

        private class ClassTunables
        {
            /// <summary>
            /// The <c>CharacterClass</c> to which this set of <c>ClassTunables</c> applies.
            /// </summary>
            public CharacterClass Class = CharacterClass.Unknown;

            public string RejuvenateAbilityName = "UNDEFINED";
            public string SelfBuffName = "UNDEFINED";
            public PredicateDelegate IsRejuvenationNeeded = (torPlayer) => { return (false); };
            public PredicateDelegate IsRejuvenationComplete = (torPlayer) => { return (true); };
            public NormalizedResourceDelegate NormalizedResource = (torPlayer) => { return (0.0f); };
        };
        #endregion  // Table Schemas
    }
}
