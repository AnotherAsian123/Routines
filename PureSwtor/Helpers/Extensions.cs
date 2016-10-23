using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Buddy.Common;
using Buddy.CommonBot;
using Buddy.Common.Math;
using Buddy.Swtor;
using Buddy.Swtor.Objects;
using Buddy.BehaviorTree;

using Action = Buddy.BehaviorTree.Action;

namespace PureSWTor.Helpers
{
    public static class Extensions
    {
        public delegate bool TorCharacterPredicateDelegate(TorCharacter torCharacter);
        public delegate bool TorEffectPredicateDelegate(TorEffect torEffect);
        public delegate bool TorPlayerPredicateDelegate(TorPlayer torPlayer);

        public static int GetStacks(this TorEffect t)
        {
            int result = 0;
            try
            {
                result = t.Stacks;
            }
            catch
            {
                result = 1;
            }
            return result;
        }

        public static int BuffCount(this TorCharacter p, string buffName){
            return !p.HasBuff(buffName) ? 0 : p.Buffs.FirstOrDefault(b => b.Name == buffName).GetStacks();
        }

        public static int DebuffCount(this TorCharacter p, string buffName)
        {
            return !p.HasDebuff(buffName) ? 0 : p.Debuffs.FirstOrDefault(b => b.Name == buffName).GetStacks();
        }

        public static double BuffTimeLeft(this TorCharacter p, string buffName)
        {
            return !p.HasBuff(buffName) ? 0 : p.Buffs.FirstOrDefault(b => b.Name.Contains(buffName)).TimeLeft.TotalSeconds;
        }

        public static double DebuffTimeLeft(this TorCharacter p, string buffName)
        {
            return !p.HasDebuff(buffName) ? 0 : p.Debuffs.FirstOrDefault(b => b.Name.Contains(buffName)).TimeLeft.TotalSeconds;
        }

        public static bool ContainsBuff(this TorCharacter p, string buffName)
        {
            foreach (TorEffect t in p.Buffs)
            {
                if (t.Name.Contains(buffName))
                    return true;
            }

            return false;
        }

        public static bool ContainsDebuff(this TorCharacter p, string buffName)
        {
            foreach (TorEffect t in p.Debuffs)
            {
                if (t.Name.Contains(buffName))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns TRUE if the character is behind the TARGET or FALSE if not.
        /// </summary>
        /// Neo93
        public static bool IsBehind(this TorCharacter torCharacter, TorCharacter Target)
        {
            return (Math.Abs(BuddyTor.Me.Heading - Target.Heading) <= 150); // && CurrentTarget.IsInRange(0.35f)
        }

        /// <summary>
        /// <para>Returns true, if the TorCharacter has CASTTIMEREMAINING or greater remaining on its spell cast.</para>
        /// <para>Notes:<list type="bullet">
        /// <item><description><para> * If the TorCharacter is not casting, false is returned.</para></description></item>
        /// </list></para>
        /// </summary>
        /// <param name="torCharacter"></param>
        /// <param name="castTimeRemaining">may not be null</param>
        /// <returns></returns>
        public static bool IsCastTimeRemaining(this TorCharacter torCharacter, TimeSpan castTimeRemaining)
        {
            return (torCharacter.IsCasting && ((torCharacter.CastTimeEnd - DateTime.Now) >= castTimeRemaining));
        }

        /// <summary>
        /// <para>True, if companion is in use by TORPLAYER.  The companion may not be nearby, or may be dead, but it is present in the gameworld.</para>
        /// <para>Notes:<list type="bullet">
        /// <item><description><para> * Due to game mechanics, this method returns 'false' when the toon is mounted.</para></description></item>
        /// <item><description><para> * If you use this test and it passes, you don't have to also check for Companion != null.</para></description></item>
        /// </list></para>
        /// </summary>
        public static bool IsCompanionInUse(this TorPlayer torPlayer)
        {
            // Extension methods guarantee the 'this' argument is never null, so no need to check a contract here
            return ((torPlayer.CompanionUnlocked > 0) && (torPlayer.Companion != null));
        }

        /// <summary>
        /// <para>True, if the TorCharacter has one of the recognized crowd control debuffs.</para>
        /// </summary>
        /// <param name="torCharacter"></param>
        /// <returns></returns>
        public static bool IsCrowdControlled(this TorCharacter torCharacter)
        {
            return (torCharacter.Debuffs.FirstOrDefault(d => Tunables.DebuffNames_CrowdControl.Contains(d.Name)) != null);
        }

        /// <summary>
        /// Returns true, if the TorCharacter's health percentage is &gt;= MINHEALTHPERCENT and &lt;= MAXHEALTHPERCENT
        /// </summary>
        /// <param name="torCharacter"></param>
        /// <param name="minHealthPercent"></param>
        /// <param name="maxHealthPercent"></param>
        /// <returns></returns>
        public static bool IsHealthInRange(this TorCharacter torCharacter, float minHealthPercent, float maxHealthPercent = 100.0f)
        {
            float characterHealth = torCharacter.HealthPercent;
            return ((characterHealth > minHealthPercent) && (characterHealth <= maxHealthPercent));
        }

        /// <summary>
        /// <para>Returns true, if the TorCharacter has the "Crouch" or "Cover" buff.</para>
        /// <para>Notes:<list type="bullet">
        /// <item><description><para> * TorCharacter provides an IsCoverAffected property.  Alas, this property does not properly
        /// reflect the state for coverage of BuddyTor.Me.  Thus, the need for this method.</para></description></item>
        /// </list></para>
        /// </summary>
        /// <param name="torCharacter"></param>
        /// <returns></returns>
        /// 15Jul2012-00:21UTC chinajade
        public static bool IsInCover(this TorCharacter torCharacter)
        {
            // Written this way to minimize slow LINQ queries (e.g., HasBuff()) to SWtOR client...
            return (torCharacter.Buffs.Any(b => BuffNamesForCoverVariants.Contains(b.Name)));
        }
        private static string[] BuffNamesForCoverVariants = { "Crouch", "Cover" };

        /// <summary>
        /// <para>Returns true if the TorPlayer is in a group.</para>
        /// <para>Notes:<list type="bullet">
        /// <item><description><para> * "In a party" is defined to be two or more TorPlayers together.
        /// By this definition, you and your pet are not a group.</para></description></item>
        /// <item><description><para> * You can apply this extension method to any TorPlayer.  If you ask about another
        /// TorPlayer, he may be in _a_ group, but may not be in _your_ group.</para></description></item>
        /// </list></para>
        /// </summary>
        /// <param name="torPlayer"></param>
        /// <returns></returns>
        public static bool IsInParty(this TorPlayer torPlayer)
        {
            // NB: Since we implement this in terms of PartyPlayers(), we need to make certain that PartyPlayers()
            // isn't implemented in terms of IsInParty().  Otherwise, infinite recursive descent will occur.
            return (torPlayer.PartyPlayers().Count() >= 2);
        }

        /// <summary>
        /// <para>Returns true if the provided TORPLAYER is in the same group as <c>BuddyTor.Me</c></para>
        /// </summary>
        /// <param name="torPlayer"></param>
        /// <returns></returns>
        public static bool IsInMyParty(this TorPlayer torPlayer)
        {
            ulong myGroupId = BuddyTor.Me.GroupId;
            return ((myGroupId != 0) && !torPlayer.IsDeleted && (myGroupId == torPlayer.GroupId));
        }

        /// <summary>
        /// <para>Returns 'true' if the TORCHARACTERTARGET's distance from TORCHARACTERREFERENCE is &gt;= MINRANGE and &lt;= MAXRANGE.</para>
        /// <para>Notes:<list type="bullet">
        /// <item><description><para> * MINRANGE and MAXRANGE must be non-negative.</para></description></item>
        /// <item><description><para> * MINRANGE must be less than or equal to MAXRANGE.</para></description></item>
        /// <item><description><para> * If MINRANGE is omitted, a value of 0.0f is assumed.</para></description></item>
        /// <item><description><para> * This method is simply a range-check, and provides no checks for !IsDead, !IsFriendy, etc.</para></description></item>
        /// </list></para>
        /// </summary>
        /// <param name="maxRange">must be zero or positive. If omitted, a value of float.MaxValue is used.</param>
        /// <param name="minRange">must be zero or positive. If omitted, a value of zero is used.</param>
        /// <param name="torCharacterTarget">may not be null</param>
        /// <returns>'true', if current target distance is greater than or equal to minRange, and less than or equal to maxRange.
        /// Otherwise, returns 'false'.</returns>
        public static bool IsInRange(this TorCharacter torCharacterReference,
                                      TorCharacter torCharacterTarget,
                                      float maxRange,
                                      float minRange = 0.0f)
        {
            float distance = Vector3.Distance(torCharacterReference.Position, torCharacterTarget.Position);
            return ((distance >= minRange) && (distance <= maxRange));
        }

        /// <summary>
        /// <para>Convenience wrapper around IsInRange(this TorCharacter torCharacterReference, TorCharacter torCharacterTarget, float maxRange, float minRange = 0.0f)</para>
        /// <para>where TORCHARACTERREFERENCE is provided as <c>BuddyTor.Me</c>.</para>
        /// </summary>
        /// <param name="torCharacter"></param>
        /// <param name="maxRange"></param>
        /// <param name="minRange"></param>
        /// <returns></returns>
        public static bool IsInRange(this TorCharacter torCharacter, float maxRange, float minRange = 0.0f)
        {
            return (BuddyTor.Me.IsInRange(torCharacter, maxRange, minRange));
        }

        public static bool StrongOrGreater(this TorCharacter unit)
        {
            if (unit != null) if (unit.Toughness >= CombatToughness.Strong) return true;
            return false;
        }

        public static bool BossOrGreater(this TorCharacter unit)
        {
            if (unit != null) if (unit.Toughness >= CombatToughness.Boss1) return true;
            return false;
        }

        /// <summary>
        /// <para>Returns true, if ABILITY is instacast (has zero cast time, and is not channeled).</para>
        /// </summary>
        /// <param name="ability"></param>
        /// <returns></returns>
        /// 10Jul2012-20:33UTC chinajade
        public static bool IsInstaCast(this TorAbility ability)
        {
            return ((ability.CastingTime == 0) && (ability.ChannelingTime == 0));
        }

        /// <summary>
        /// Returns true, if party role is melee- or ranged-tank.
        /// </summary>
        /// <param name="torCharacter"></param>
        /// <returns></returns>
        /// 23Jul2012-05:11UTC chinajade
        public static bool IsPartyRoleTank(this TorCharacter torCharacter)
        {
            Global.PartyRole role = torCharacter.PartyRole();
            return ((role == Global.PartyRole.MeleeTank) || (role == Global.PartyRole.RangedTank));
        }

        /// <summary>
        /// <para>Convenience wrapper around MobsAround(this TorCharacter torCharacter, float distance, TorCharacterPredicateDelegate mobQualifier)</para>
        /// </summary>
        /// <param name="torCharacter"></param>
        /// <param name="distance"></param>
        /// <param name="mobQualifier"></param>
        /// <returns></returns>
        /// 18Jul2012-04:02UTC chinajade
        public static int MobCountAround(this TorCharacter torCharacter,
                                           float distance,
                                           TorCharacterPredicateDelegate mobQualifier = null)
        {
            return torCharacter.MobsAround(distance, mobQualifier).Count();
        }

        /// <summary>
        /// <para>Returns the count of mobs attacking the party.</para>
        /// <para>Notes:<list type="bullet">
        /// <item><description><para> * The count includes mobs at all ranges--melee and ranged.</para></description></item>
        /// </list></para>
        /// </summary>
        /// <param name="torPlayer"></param>
        /// <returns></returns>
        /// 18Jul2012-04:02UTC chinajade
        public static int MobCountAttacking(this TorPlayer torPlayer)
        {
            return (torPlayer.EnemiesAttackers.Count());
        }

        /// <summary>
        /// <para>Returns a list of hostile, non-dead Npcs within DISTANCE of the TorCharacter. Mobs may be further qualified with MOBQUALIFIER.</para>
        /// <para>Notes:<list type="bullet">
        /// <item><description><para> * If MOBQUALIFIER is omitted, all hostile, non-dead mobs in the area will be included.</para></description></item>
        /// <item><description><para> * The MOBQUALIFIER is frequently used to exclude stunned mobs, or mobs with a certain debuff from the count.</para></description></item>
        /// <item><description><para> * The area considered is that of a 'filled cicle' around the TorCharacter.</para></description></item>
        /// </list></para>
        /// </summary>
        /// <param name="torCharacter"></param>
        /// <param name="distance">Must be non-negative. Maximum distance from the player in which an Npc should be counted.</param>
        /// <param name="mobQualifier">If omitted, no additional requirements will be imposed on a mob to have it counted.</param>
        /// <returns></returns>
        /// 18Jul2012-04:02UTC chinajade
        public static IEnumerable<TorNpc> MobsAround(this TorCharacter torCharacter,
                                                     float distance,
                                                     TorCharacterPredicateDelegate mobQualifier = null)
        {
            mobQualifier = mobQualifier ?? (m => true);     // Resolve null mobQualifier to something sane

            // Note we use a !IsFriendly test instead of an IsHostile test such that 'neutral' targets will be
            // appropriately counted. Since any cleaving or AoE type attack will affect neutral mobs, this is a must for a default.
            // We also have a defensive check for 'targets == null' here, because the BWcore is returning 'null'
            // instead of empty lists in some circumstances.
            // Splitting the Where clauses is a performance optimization, as the mobQualifier may have some ray-cast predicates,
            // and this would cause horrible performance (on the order of seconds) in high mob-density areas.
            // 27Jun2012-21:44UTC chinajade
            return (ObjectManager.GetObjects<Buddy.Swtor.Objects.TorNpc>()
                    .Where(t => !t.IsDeleted && torCharacter.IsInRange(t, distance))
                    .Where(t => t.IsHostile && !t.IsDead && mobQualifier(t)));
        }

        /// <summary>
        /// <para>Returns the companions in the TorPlayer's group that adhere to the constraints imposed by COMPANIONQUALIFIER.</para>
        /// <para>Notes:<list type="bullet">
        /// <item><description><para> * If COMPANIONQUALIFIER is omitted, no additional constraints are imposed.</para></description></item>
        /// <item><description><para> * For performance reasons, the SWtOR client limits this list to companions within 75-100 yards or so of the toon.
        /// If a companions is not within this distance, he will not be part of the returned value.</para></description></item>
        /// </list></para>
        /// </summary>
        /// <param name="playerQualifier"></param>
        /// <returns></returns>
        ///  7Jul2012-21:10UTC chinajade
        public static IEnumerable<TorNpc> PartyCompanions(this TorPlayer torPlayer, TorCharacterPredicateDelegate companionQualifier = null)
        {
            // Extension methods guarantee the 'this' argument is never null, so no need to check a contract here

            companionQualifier = companionQualifier ?? (ctx => true);       // resolve playerQualifier to something sane

            return (torPlayer.PartyPlayers(p => p.IsCompanionInUse() && companionQualifier(p.Companion)).Select(p => p.Companion));
        }

        /// <summary>
        /// <para>Returns all members in the TorPlayer's group--whether they be Players or Companions--that adhere
        /// to the constraints imposed by MEMBERQUALIFIER.  With INCLUDECOMPANIONS, you may arrange to emit the companions from the returned results.</para>
        /// <para>Notes:<list type="bullet">
        /// <item><description><para> * IF INCLUDECOMPANIONS is omitted, companions will be returned as party members.</para></description></item>
        /// <item><description><para> * If MEMBERQUALIFIER is omitted, no additional constraints are imposed.</para></description></item>
        /// <item><description><para> * For performance reasons, the SWtOR client limits this list to characters within 75-100 yards or so of the toon.
        /// If a party member is not within this distance, he will not be part of the returned value.</para></description></item>
        /// <item><description><para> * Related Methods: PartyMembers(), PartyPlayers()</para></description></item>
        /// </list></para>
        /// </summary>
        /// <param name="playerQualifier"></param>
        /// <returns></returns>
        /// 18Jul2012-21:25UTC chinajade
        public static IEnumerable<TorCharacter> PartyMembers(this TorPlayer torPlayer, bool includeCompanions = true, TorCharacterPredicateDelegate memberQualifier = null)
        {
            memberQualifier = memberQualifier ?? (c => true);   // resolve playerQualifier to something sane

            IEnumerable<TorCharacter> partyMembers = torPlayer.PartyPlayers();

            if (includeCompanions)
            { partyMembers = partyMembers.Concat(torPlayer.PartyCompanions()); }

            return (partyMembers.Where(c => memberQualifier(c)));
        }

        /// <summary>
        /// <para>Returns all players in the TorPlayer's group that adhere to the constraints imposed by PLAYERQUALIFIER.</para>
        /// <para>Notes:<list type="bullet">
        /// <item><description><para> * If PLAYERQUALIFIER is omitted, no additional constraints are imposed.</para></description></item>
        /// <item><description><para> * Only the Players in the group are returned. (I.e., only PCs, not NPCs like companions).</para></description></item>
        /// <item><description><para> * For performance reasons, the SWtOR client limits this list to characters within 75-100 yards or so of the toon.
        /// If a player is not within this distance, he will not be part of the returned value.</para></description></item>
        /// <item><description><para> * Related Methods: PartyMembers(), PartyPlayers()</para></description></item>
        /// </list></para>
        /// </summary>
        /// <param name="playerQualifier"></param>
        /// <returns></returns>
        ///  7Jul2012-21:10UTC chinajade
        public static IEnumerable<TorPlayer> PartyPlayers(this TorPlayer torPlayer, TorPlayerPredicateDelegate playerQualifier = null)
        {
            playerQualifier = playerQualifier ?? (ctx => true);       // resolve playerQualifier to something sane

            ulong playerGroupId = torPlayer.GroupId;

            // If we're solo, only have the torPlayer on the list...
            // We can't build this list using the 'normal' query, as all solo players have the common GroupId of zero.
            // We don't want a list of 'solo' players (what the normal query would do), we want a list with only the solo torPlayer on it.
            if (playerGroupId == 0)
            { return (ObjectManager.GetObjects<TorPlayer>().Where(p => (p == BuddyTor.Me) && playerQualifier(p))); }

            // NB: IsInParty() is implemented in terms of PartyPlayers().  Be careful not to implement this method in terms of
            // IsInParty(); otherwise, infinite recursive descent will occur.
            return (ObjectManager.GetObjects<TorPlayer>().Where(p => !p.IsDeleted && playerQualifier(p) && (p.GroupId == playerGroupId)));
        }

        /// <summary>
        /// <para>Determines the role of a Player or Npc in the party.</para>
        /// </summary>
        /// <param name="torNpc"></param>
        /// <returns></returns>
        public static Global.PartyRole PartyRole(this TorCharacter torCharacter)
        {
            //Idk if this was working anyway.
            return Global.PartyRole.RangedDPS;
        }

        /// <summary>
        /// <para>Returns the relative importants of the TorCharacter's party role to the party.</para>
        /// <para>This is used to determine which members should receive critical services first (e.g., shields, heals)
        /// when all other things are equal.</para>
        /// </summary>
        /// <param name="torCharacter"></param>
        /// <returns></returns>
        public static float PartyRoleWeight(this TorCharacter torCharacter)
        {
            Global.PartyRole partyRole = torCharacter.PartyRole();
            Tuple<float, float> partyRoleWeights;

            if (!Tunables.PartyRolePriorities.TryGetValue(partyRole, out partyRoleWeights))
            {
                return (0.00f);
            }

            float levelAdjust = torCharacter.Level / Tunables.MaxPlayerLevel;
            float roleWeight = ((torCharacter as TorPlayer) != null) ? partyRoleWeights.Item1 /*player*/ : partyRoleWeights.Item2 /*companion*/;

            // We favor higher level toons since they deal more, and can take more damage...
            return (roleWeight * levelAdjust);
        }

        /// <summary>
        /// <para>Returns a list of players within DISTANCE of the TorPlayer. Players may be further qualified with PLAYERQUALIFIER.</para>
        /// <para>Notes:<list type="bullet">
        /// <item><description><para> * If PLAYERQUALIFIER is omitted, all players in the area will be included.
        /// This includes friendly and hostile, alive and dead players.</para></description></item>
        /// <item><description><para> * The PLAYERQUALIFIER is frequently used to exclude stunned players, or players with a certain debuff from the count.</para></description></item>
        /// <item><description><para> * The area considered is that of a 'filled cicle' around the TorPlayer.</para></description></item>
        /// </list></para>
        /// </summary>
        /// <param name="torCharacter"></param>
        /// <param name="distance">Must be non-negative. Maximum distance from the player in which an TorCharacter should be counted.</param>
        /// <param name="mobQualifier">If omitted, no additional requirements will be imposed on a mob to have it counted.</param>
        /// <returns></returns>
        /// <returns></returns>
        ///  2Aug2012-21:15UTC chinajade
        public static IEnumerable<TorPlayer> PlayersAround(this TorPlayer torPlayer, float distance, TorPlayerPredicateDelegate playerQualifier = null)
        {
            playerQualifier = playerQualifier ?? (ctx => true);     // Resolve null argument to something sane

            return ObjectManager.GetObjects<Buddy.Swtor.Objects.TorPlayer>()
                    .Where(p => !p.IsDeleted && torPlayer.IsInRange(p, distance) && playerQualifier(p));
        }

        /// <summary>
        /// <para>Convenience wrapper around PlayersAround_All(this TorPlayer torPlayer, float distance, TorPlayerPredicateDelegate playerQualifier = null)
        /// that returns a list of friendly, non-dead players within DISTANCE of the TorPlayer.</para>
        /// </summary>
        /// <param name="torCharacter"></param>
        /// <param name="distance"></param>
        /// <param name="mobQualifier"></param>
        /// <returns></returns>
        /// 18Jul2012-04:03UTC chinajade
        public static IEnumerable<TorPlayer> PlayersAround_Friendly(this TorPlayer torPlayer, float distance, TorPlayerPredicateDelegate playerQualifier = null)
        {
            playerQualifier = playerQualifier ?? (ctx => true);     // Resolve null argument to something sane

            return PlayersAround(torPlayer, distance, p => playerQualifier(p) && !p.IsDead && p.IsFriendly);
        }

        /// <summary>
        /// <para>Convenience wrapper around PlayersAround_All(this TorPlayer torPlayer, float distance, TorPlayerPredicateDelegate playerQualifier = null)
        /// that returns a list of hostile, non-dead players within DISTANCE of the TorPlayer.</para>
        /// </summary>
        /// <param name="torCharacter"></param>
        /// <param name="distance"></param>
        /// <param name="mobQualifier"></param>
        /// <returns></returns>
        ///  2Aug2012-21:15UTC chinajade
        public static IEnumerable<TorPlayer> PlayersAround_Hostile(this TorPlayer torPlayer, float distance, TorPlayerPredicateDelegate playerQualifier = null)
        {
            playerQualifier = playerQualifier ?? (ctx => true);     // Resolve null argument to something sane

            return PlayersAround(torPlayer, distance, p => playerQualifier(p) && !p.IsDead && p.IsHostile);
        }

        /// <summary>
        /// <para>Returns target strength on a scale [0.0..1.0] where 1.0 is the most difficult target.</para>
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        /// 17Jul2012-18:46UTC chinajade
        public static float RelativeDifficulty(this TorCharacter torCharacter)
        {
            // NB: level disparity is actual a logarithmic scale, not linear.  But for now, we ignore that detail.
            // NB: "Level" is unsigned...<sigh>.  You can't get a sane result by subtracting unsigned values, so we must cast to "int" first.
            float levelDisparityMultiplier = 1.0f + (((int)torCharacter.Level - (int)BuddyTor.Me.Level) / (int)BuddyTor.Me.Level);
            float torCharacterDifficulty = torCharacter.HealthPercent * Tunables.RelativeDifficulty[torCharacter.Toughness];
            float maxDifficulty = (100.0f * Tunables.RelativeDifficulty[CombatToughness.RaidBoss]);

            return (torCharacterDifficulty * levelDisparityMultiplier / maxDifficulty);
        }

        /// <summary>
        /// <para>Returns the advanced class for TorPlayer, if one exists.  If the toon has yet to acquire
        /// an advanced class, the basic class is returned.</para>
        /// <para>Notes:<list type="bullet">
        /// <item><description><para> * If an advanced class is returned, it is returned as a <c>CharacterClass</c> enumeration
        /// value--not an <c>AdvancedClass</c> enumeration.  This is what is meant by 'normalizing'.</para></description></item>
        /// </list>
        /// </para>
        /// </summary>
        /// <returns></returns>
        /// 21Jun2012-20:47UTC chinajade
        public static CharacterClass NormalizedClass(this TorPlayer torPlayer)
        {
            if (torPlayer.AdvancedClass == AdvancedClass.None)
            { return (torPlayer.Class); }

            return (AdvancedToCharacterClass[torPlayer.AdvancedClass]);
        }

        /// <summary>
        /// <para>Please don't access this information directly, use <c>Mirror.GetNormalizedClass()</c> instead.</para>
        /// </summary>
        public static Dictionary<AdvancedClass, CharacterClass> AdvancedToCharacterClass
            = new Dictionary<AdvancedClass, CharacterClass>()
            {
                { AdvancedClass.Juggernaut, CharacterClass.Juggernaut },
                { AdvancedClass.Marauder, CharacterClass.Marauder },
                { AdvancedClass.Assassin, CharacterClass.Assassin },
                { AdvancedClass.Sorcerer, CharacterClass.Sorcerer },
                { AdvancedClass.Operative, CharacterClass.Operative },
                { AdvancedClass.Sniper, CharacterClass.Sniper },
                { AdvancedClass.Mercenary, CharacterClass.Mercenary },
                { AdvancedClass.Powertech, CharacterClass.Powertech },

                { AdvancedClass.Guardian, CharacterClass.Guardian },
                { AdvancedClass.Sentinel, CharacterClass.Sentinel },
                { AdvancedClass.Shadow, CharacterClass.Shadow },
                { AdvancedClass.Sage, CharacterClass.Sage },
                { AdvancedClass.Scoundrel, CharacterClass.Scoundrel },
                { AdvancedClass.Gunslinger, CharacterClass.Gunslinger },
                { AdvancedClass.Commando, CharacterClass.Commando },
                { AdvancedClass.Vanguard, CharacterClass.Vanguard }
            };

    }
}