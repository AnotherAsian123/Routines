using System;
using System.Collections.Generic;
using System.Timers;
using System.Linq;
using System.Text;
using Buddy.Common;
using Buddy.BehaviorTree;
using Buddy.Common.Math;
using Buddy.CommonBot;
using Buddy.Swtor;
using Buddy.Swtor.Objects;
using PureSWTor.Helpers;
using PureSWTor.Managers;

using Action = Buddy.BehaviorTree.Action;

namespace PureSWTor.Core
{
    public static class Spell
    {
        private static Buddy.Swtor.Objects.TorPlayer Me { get { return BuddyTor.Me; } }
        public delegate TorCharacter UnitSelectionDelegate(object context);
        public delegate T Selection<out T>(object context);
        public static List<ExpiringItem> BlackListedSpells = new List<ExpiringItem>();

        public static Composite WaitForCast()
        {
            return new Decorator(ret => BuddyTor.Me.IsCasting,
                new Action(ret => RunStatus.Success));
        }
        #region Cast - by name

        public static Composite Cast(string spell, Selection<bool> reqs = null)
        {
            return Cast(spell, ret => Me.CurrentTarget, reqs);
        }
        
        public static Composite Cast(string spell, UnitSelectionDelegate onUnit, Selection<bool> reqs = null)
        {
            return
                new Decorator(
                    ret => (onUnit != null && onUnit(ret) != null && (reqs == null || reqs(ret)) && AbilityManager.CanCast(spell, onUnit(ret))),
                    new PrioritySelector(
                        new Action(delegate {
                            Logging.Write(">> Casting <<   " + spell);
                            return RunStatus.Failure; }),
                        new Action(ret => AbilityManager.Cast(spell, onUnit(ret))))

                );
        }

        public static bool CastCheck(string spell, TorCharacter onUnit)
        {
            var abl = AbilityManager.KnownAbilities.FirstOrDefault(a => a.Name == spell);

            if(abl != null)
                Logging.Write("[" + onUnit.Name + "][" + abl.Name + "][" + Me.IsAbilityReady(abl, onUnit).ToString() + "]");
            
            return AbilityManager.CanCast(spell, onUnit);
        }
        
        public static Composite MultiDot(string dot, string debuff)
        {
            TorCharacter MultiDotTarget  = ObjectManager.GetObjects<TorCharacter>().Where(t =>
                        t.InCombat
                        && !t.IsDead
                        && t.IsHostile
                        && !t.IsStunned
                        && !t.HasDebuff(debuff)).FirstOrDefault();

                if (MultiDotTarget == null)
                     return new PrioritySelector();
                else
                {
                    Logging.Write(" <-----> MultiDot Target Casting <-----> ");
                    return Cast(dot, on => MultiDotTarget);
                }
            
        }

        public static Composite Debuff(string spell)
        {
            return Cast(spell, ret => Me.CurrentTarget != null && Me.CurrentTarget.IsHostile && !Me.CurrentTarget.HasDebuff(spell));
        }

        public static Composite CastOnGround(string spell, Selection<bool> reqs = null)
        {
            return
                new Decorator(
                    ret =>
                    (reqs == null || reqs(ret)) && Me.CurrentTarget != null,
                    CastOnGround(spell, ctx => Me.CurrentTarget.Position, ctx => true));
        }

        public static Composite CastOnGround(string spell, CommonBehaviors.Retrieval<Vector3> location, BooleanValueDelegate requirements)
        {
            return
                new Decorator(
                    ret =>
                    requirements != null && requirements(ret) && location != null && location(ret) != Vector3.Zero &&
                    AbilityManager.CanCast(spell, BuddyTor.Me.CurrentTarget),
                    new Action(ret => AbilityManager.Cast(spell, location(ret))));
        }
        #endregion

        #region Buff


        public static Composite Buff(string spell, Selection<bool> reqs = null)
        {
            return
                new Decorator(
                    ret => (reqs == null || reqs(ret)) && !Me.HasBuff(spell),
                    Spell.Cast(spell, ret => Me, ret => true));
                    
        }
        #endregion

        #region DoT

        public static Composite DoT(string spell, string debuff, float time = 0, Selection<bool> reqs = null)
        {
            return DoT(spell, ret => Me.CurrentTarget, debuff, time, reqs);
        }

        public static Composite DoT(string spell, UnitSelectionDelegate onUnit, string debuff, float time,  Selection<bool> reqs = null)
        {
            return
                new Decorator(
                    ret => (onUnit != null && onUnit(ret) != null && (reqs == null || reqs(ret)) && AbilityManager.CanCast(spell, onUnit(ret)))
                        && onUnit(ret).InLineOfSight
                        && !SpellBlackListed(spell)
                        && !onUnit(ret).HasDebuff(debuff),
                    new PrioritySelector(
                        new Action(ctx =>
                        {
                            BlackListedSpells.Add(new ExpiringItem(spell, (GetCooldown(spell) + 25 + time), onUnit(ctx).Guid));
                            Logging.Write(">> Casting <<   " + spell);
                            return RunStatus.Failure;
                        }),
                        new Action(ret => AbilityManager.Cast(spell, onUnit(ret)))));
        }

        public static float GetCastTime(string spell)
        {
            float castTime = 0;
            var v = AbilityManager.KnownAbilities.FirstOrDefault(a => a.Name.Contains(spell)).CastingTime;
            
            castTime += v * 1000;

            //Logging.Write(castTime.ToString());

            return castTime;
        }

        public static float GetCooldown(string spell)
        {
            float time = 0;
            var v = AbilityManager.KnownAbilities.FirstOrDefault(a => a.Name.Contains(spell)).CooldownTime;

            time += v * 1000;

            return time;
        }

        public static bool SpellBlackListed(string spell)
        {
            PruneBlackList();
            return BlackListedSpells.Any(s => s.item.Equals(spell));
        }
        public static void PruneBlackList()
        {
            BlackListedSpells.RemoveAll(s => s.item.Equals(""));

            if (Me.CurrentTarget == null)
                BlackListedSpells.Clear();
            else
                BlackListedSpells.RemoveAll(s => s.targetGuid != Me.CurrentTarget.Guid);
        }  
        #endregion

        #region Heal(Cast) - by Name (Healing Abilities)

        public static Composite Cleanse(string spell, Selection<bool> reqs = null)
        {
            return new Decorator(
                ret => (HealingManager.DispelTarget != null && (reqs == null || reqs(ret))),
                Cast(spell, ret => HealingManager.DispelTarget, reqs));
        }
        

        public static Composite Heal(string spell, int HP = 100, Selection<bool> reqs = null)
        {
            return Heal(spell, onUnit => HealingManager.HealTarget, HP, reqs);
        }

        public static Composite Heal(string spell, UnitSelectionDelegate onUnit, int HP = 100, Selection<bool> reqs = null)
        {
            return new Decorator(
                ret => (onUnit != null && onUnit(ret) != null && (reqs == null || reqs(ret)) && onUnit(ret).HealthPercent <= HP),
                Cast(spell, onUnit, reqs));
        }

        public static Composite HealAOE(string spell, Selection<bool> reqs = null)
        {
            return new Decorator(
                ret => ((reqs == null || reqs(ret)) && HealingManager.ShouldAOE && HealingManager.AOEHealTarget != null),
                Cast(spell, onUnit => HealingManager.AOEHealTarget, reqs));
        }

        public static Composite HoT(string spell, int HP = 100, Selection<bool> reqs = null)
        {
            return HoT(spell, onUnit => HealingManager.HealTarget, HP, reqs);
        }

        public static Composite HoT(string spell, UnitSelectionDelegate onUnit, int HP = 100, Selection<bool> reqs = null)
        {
            return new Decorator(
                ret => (onUnit != null && onUnit(ret) != null && (reqs == null || reqs(ret)) && !HasMyAura(spell, onUnit(ret)) && onUnit(ret).HealthPercent <= HP),
                Cast(spell, onUnit, reqs));
        }

        public static Composite HealGround(string spell, CanRunDecoratorDelegate reqs = null)
        {
            return new Decorator(
                ret => (HealingManager.AOEHealPoint != null && HealingManager.AOEHealPoint != Vector3.Zero  && (reqs == null || reqs(ret)) && HealingManager.ShouldAOE),
                CastOnGround(spell, ret => HealingManager.AOEHealPoint, ret => true));
        }

        public static bool HasMyBuff(string aura, TorCharacter u)
        {
            return u.Buffs.Any(a => a.Name == aura && a.CasterGuid == Me.Guid);
        }
        public static bool HasMyDebuff(string aura, TorCharacter u)
        {
            return u.Debuffs.Any(a => a.Name == aura && a.CasterGuid == Me.Guid);
        }
        public static bool HasMyAura(string aura, TorCharacter u)
        {
            return HasMyBuff(aura, u) || HasMyDebuff(aura, u);
        }

        #endregion
    }

    public class ExpiringItem
    {
        public string item;
        public ulong targetGuid;
        Timer t;

        public ExpiringItem(string str, float milisecs, ulong g)
        {
            item = str;
            t = new Timer(milisecs);
            targetGuid = g;
            t.Elapsed += new ElapsedEventHandler(Elapsed_Event);
            t.Start();
        }
        private void Elapsed_Event(object sender, ElapsedEventArgs e)
        {
            item = "";
        }

    }
}

