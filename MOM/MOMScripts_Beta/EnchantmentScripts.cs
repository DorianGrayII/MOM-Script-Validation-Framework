#if !USE_DEBUG_SCRIPT || !UNITY_EDITOR

using DBDef;
using DBEnum;
using MHUtils;
using MOM;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorldCode;

namespace MOMScripts
{
    public class EnchantmentScripts : ScriptBase
    {
        #region EnchantmentApplication

        /// <summary>
        /// After visibility enhancement is applied, update currently focused plane to ensure visibility fog is up to date with new range of view
        /// </summary>
        /// <param name="target"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        static public void EAPP_DefaultApplicator(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            foreach (var v in e.scripts)
                ScriptLibrary.Call(v.script, target, v, ei, null);
        }
        static public void EAPP_VisibilityApplicator(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            var plane = World.GetActivePlane();
            if (plane.battlePlane) return;

            FOW.Get().UpdateFogForPlane(plane);
        }
        static public void EAPP_AllChaosUnitsAttributeDirty(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            var groups = GameManager.Get().registeredGroups;
            if (groups != null)
            {
                foreach (var v in groups)
                {

                    if (v.GetLocationHost()?.otherPlaneLocation?.Get() != null && v.plane.arcanusType) continue;
                    var units = v.GetUnits();
                    if (units != null)
                    {
                        foreach (var u in units)
                        {
                            if(u.Get().race == (Race)RACE.REALM_CHAOS)
                                u.Get().GetAttributes().SetDirty();
                        }
                    }
                }
            }
        }
        static public void EAPP_AllOwnUnitsAttributeDirty(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            if(target.GetWizardOwner() == null)
                Debug.LogError("You try set dirty all neutral units.");

            var groups = GameManager.GetGroupsOfWizard(target.GetWizardOwner().ID);
            if (groups != null)
            {
                foreach (var v in groups)
                {
                    var units = v.GetUnits();
                    if (units != null)
                    {
                        foreach (var u in units)
                        {
                            u.Get().GetAttributes().SetDirty();
                        }
                    }
                }
            }
        }
        static public void EAPP_UnitUpdateMove(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            if (target is MOM.Unit)
            {
                (target as MOM.Unit).GetAttributes().SetDirty();
                (target as MOM.Unit).group?.Get().UpdateMovementFlags();
            }
            if (target is BattleUnit)
            {
                BattleUnit bu = target as BattleUnit;
                bu.GetAttributes().SetDirty();
                if (bu.canMove == false)
                    bu.Mp = FInt.ZERO;

                if (BattleHUD.GetSelectedUnit() == bu && bu.GetWizardOwnerID() == 1)
                {
                    MHEventSystem.TriggerEvent<BattleHUD>(BattleHUD.Get(), null);
                }
            }

        }
        static public void EAPP_Haste(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            if (target is BattleUnit)
            {
                BattleUnit bu = target as BattleUnit;
                if(bu.canMove)
                {
                    int unitMove = bu.GetAttFinal((Tag)TAG.MOVEMENT_POINTS).ToInt();
                    bu.Mp += unitMove;

                    if (BattleHUD.GetSelectedUnit() == bu && bu.GetWizardOwnerID() == 1)
                    {
                        MHEventSystem.TriggerEvent<BattleHUD>(BattleHUD.Get(), null);
                    }
                }
            }

        }
        static public void EAPP_WindWalkingOwner(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            // similar to OnJoin effect - add enchantment to every unit in the group
            var unit = target as MOM.Unit;
            if (unit == null) return;
            foreach (var script in e.scripts)
            {
                if (script.triggerType == EEnchantmentType.GroupChange)
                {
                    var enchToAdd = (Enchantment)DataBase.Get(script.stringData, false);

                    if (enchToAdd == null)
                        Debug.LogError("SJOIN_WindWalking StringData is not a ench.");
                    else
                    {
                        var group = unit.group?.Get();
                        if (group == null || group.GetUnits().Count == 0) continue;
                        if ((Enchantment)ENCH.WIND_WALKING_UNIT != enchToAdd) continue;
                        var ownerMove = unit.GetAttFinal(TAG.MOVEMENT_POINTS).ToInt();

                        foreach (var u in group.GetUnits())
                        {
                            if (unit == u.Get()) continue;
                            var unitEnch = u.Get().GetEnchantments().Find(o => o.source == enchToAdd);
                            if (unitEnch == null)
                            {
                                u.Get().AddEnchantment(enchToAdd, unit, -1, ownerMove.ToString());
                            }
                            else
                            {
                                try
                                {
                                    var orginalMove = Convert.ToInt32(unitEnch.parameters);
                                    if (ownerMove < orginalMove)
                                    {
                                        unitEnch.parameters = orginalMove.ToString();
                                        u.Get().GetAttributes().SetDirty();
                                        //u.Get().group?.Get().UpdateMovementFlags();
                                    }
                                }
                                catch
                                {
                                }
                            }
                        }
                    }
                }
            }
            EAPP_UnitUpdateMove(target, e, ei);
        }
        static public void EAPP_BattleUnitAttributeDirty(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            var bu = target as BattleUnit;
            if (bu != null)
                bu.GetAttributes().SetDirty();
        }
        static public void EAPP_AllBattleUnitsAttributeDirty(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            var b = Battle.GetBattle();
            if(b != null)
            {
                foreach(var v in b.buToSource)
                {
                    v.Key.GetAttributes().SetDirty();
                }
            }
        }
        static public void EAPP_OwnBattleUnitsAttributeDirty(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            var b = Battle.GetBattle();
            if (b != null)
            {
                int casterID = GetSpellCasterOwnerID(ei);
                bool attacker = casterID == b.attacker.GetID();

                if (attacker)
                {
                    foreach (var bu in b.attackerUnits)
                    {
                        bu.GetAttributes().SetDirty();
                    }
                }
                else
                {
                    foreach (var bu in b.defenderUnits)
                    {
                        bu.GetAttributes().SetDirty();
                    }
                }
            }
        }

        static public void EAPP_MetalFires(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            EAPP_AllBattleUnitsAttributeDirty(target, e, ei);

            var b = Battle.GetBattle();
            if (b == null) return;

            var effect = ((Spell)SPELL.METAL_FIRES).castEffect;
            if (b.attacker == target)
            {
                foreach(var v in b.attackerUnits)
                {
                    if (v == null || !v.IsAlive()) continue;
                    FSMBattleTurn.instance?.CastEffect(v.GetPosition(), effect);
                }
            }
            else if (b.defender == target)
            {
                foreach (var v in b.defenderUnits)
                {
                    if (v == null || !v.IsAlive()) continue;
                    FSMBattleTurn.instance?.CastEffect(v.GetPosition(), effect);
                }
            }
        }

        static public void EAPP_BlackPrayer(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            EAPP_AllBattleUnitsAttributeDirty(target, e, ei);

            var b = Battle.GetBattle();
            if (b == null) return;

            var effect = ((Spell)SPELL.BLACK_PRAYER).castEffect;
            if (b.attacker == target)
            {
                foreach (var v in b.attackerUnits)
                {
                    if (v == null || !v.IsAlive()) continue;
                    FSMBattleTurn.instance?.CastEffect(v.GetPosition(), effect);
                }
            }
            else if (b.defender == target)
            {
                foreach (var v in b.defenderUnits)
                {
                    if (v == null || !v.IsAlive()) continue;
                    FSMBattleTurn.instance?.CastEffect(v.GetPosition(), effect);
                }
            }
        }

        static public void EAPP_WarpReality(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            EAPP_AllBattleUnitsAttributeDirty(target, e, ei);

            var b = Battle.GetBattle();
            if (b == null) return;

            var effect = ((Spell)SPELL.WARP_REALITY).castEffect;

            if (b.attackerUnits != null)
            {
                foreach (var v in b.attackerUnits)
                {
                    if (v == null || !v.IsAlive()) continue;
                    FSMBattleTurn.instance?.CastEffect(v.GetPosition(), effect);
                }
            }

            if (b.defenderUnits != null)
            {
                foreach (var v in b.defenderUnits)
                {
                    if (v == null || !v.IsAlive()) continue;
                    FSMBattleTurn.instance?.CastEffect(v.GetPosition(), effect);
                }
            }
            
        }

        static public void EAPP_TerrorWizzard(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            var b = Battle.GetBattle();
            if (b != null)
            {
                //instantly apply effect onto units as it is later applied at the end of the turn if the parent enchantment stays in effect
                ECH_TerrorWizard(target, null, ei, null);

                foreach (var v in b.buToSource)
                {
                    v.Key.GetAttributes().SetDirty();
                }
            }
        }
        static public void EAPP_EarthToMud(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            var b = Battle.GetBattle();
            var bu = target as BattleUnit;
            if (bu != null)
            {
                bu.Mp = (bu.Mp / 2).ReturnRoundedFloor();

                var selectedUnit = BattleHUD.GetSelectedUnit();
                if(selectedUnit == bu && selectedUnit.ownerID == GameManager.GetHumanWizard().ID)
                {
                    MHEventSystem.TriggerEvent<BattleHUD>(bu, null);
                }
            }
            if (b != null)
            {
                foreach (var v in b.buToSource)
                {
                    v.Key.GetAttributes().SetDirty();
                }
            }
        }
        static public void EAPP_SetAllowPlaneSwitchFalse(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            GameManager.Get().AllowPlaneSwitch(false);

            foreach(var v in GameManager.Get().registeredLocations)
            {
                if(v.otherPlaneLocation != null)
                {
                    if(v.model != null)
                    {
                        var g = GameObjectUtils.FindByName(v.model, "PlanarSeal");
                        if (g != null) g.SetActive(true);
                    }
                }
            }
        }
        static public void EAPP_AuraOfMajesty(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            if (ei == null && ei.owner == null)
            {
                Debug.LogError("EAPP_AuraOfMajesty ei or ei.owner is null");
                return;
            }

            var spellCaster = GameManager.GetWizard(ei.owner.ID);
            if (spellCaster.discoveredWizards == null) return;

            foreach (var wizard in spellCaster.discoveredWizards)
            {
                var diplomacyStatus = spellCaster.GetDiplomacy().GetStatusToward(wizard.ID);
                if(diplomacyStatus == null)
                {
                    Debug.LogError("EAPP_AuraOfMajesty diplomacyStatus is null");
                    return;
                }
                diplomacyStatus.ChangeRelationshipBy(10, true);
            }
        }

        static public void EAPP_JustCause(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            if (!(target is MOM.PlayerWizard)) return;

            var wizard = target as MOM.PlayerWizard;
            wizard.AddTemporaryFame(10);
            if (wizard.IsHuman)
            {
                HUD.Get().UpdateHUD();
            }
            wizard.rebelModifier += FInt.N_ONE;
        }
        static public void EAPP_Crusade(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            if (!(target is MOM.PlayerWizard)) return;

            var wizard = target as MOM.PlayerWizard;
            wizard.unitLevelIncrease += 1;

            var wizardGroups = GameManager.GetGroupsOfWizard(wizard.GetID());
            foreach (var g in wizardGroups)
            {
                foreach (var u in g.GetUnits())
                {
                    u.Get().GetAttributes().SetDirty();
                    u.Get().group?.Get()?.TriggerOnJoinScripts(u);
                }
            }
        }

        static public void EAPP_AddSkill(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            if (target is ISkillable)
            {
                var u = target as ISkillable;
                Skill skill = DataBase.Get(e.applicationScript.stringData, false) as Skill;
                if (skill == null)
                {
                    Debug.LogError(e.applicationScript.stringData + " is not a Skill. You have a typo?");
                    return;
                }
                u.AddSkill(skill);
            }
        }

        static public void EAPP_Heroism(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            if (target is BaseUnit)
            {
                var u = target as BaseUnit;
                u.levelOverride = 4;
                u.GetAttributes().SetDirty();
            }
        }

        static public void EAPP_MindControl(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            if (target is BattleUnit)
            {
                BattleUnit bu = target as BattleUnit;
                if (!bu.IsAlive()) return;

                Battle b = Battle.GetBattle();
                if (b.attackerUnits.Contains(bu))
                {
                    b.AttackerRemoveUnit(bu);
                    b.DefenderAddUnit(bu);
                    bu.ownerID = b.defender.GetID();
                    bu.attackingSide = false;
                }
                else if (b.defenderUnits.Contains(bu))
                {
                    b.DefenderRemoveUnit(bu);
                    b.AttackerAddUnit(bu);
                    bu.ownerID = b.attacker.GetID();
                    bu.attackingSide = true;
                }
                else return;

                b.UnitListsDirty();
                BattleHUD.Get()?.BaseUpdate();
                MHUtils.UI.VerticalMarkerManager.Get()?.UpdateMarkerColors(target);
            }
        }
        static public void EAPP_AddWindMasteryOnWizards(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            if (!(target is GameManager)) return;

            var wizardID = ei.owner.ID;

            var wizards = GameManager.Get().wizards;
            var enchPositive = (Enchantment)DataBase.Get("ENCH-WIND_MASTERY_UNIT_POSITIVE", false);
            var enchNegative = (Enchantment)DataBase.Get("ENCH-WIND_MASTERY_UNIT_NEGATIVE", false);

            foreach (var w in wizards)
            {
                if(w.ID == wizardID)
                    w.AddEnchantment(enchPositive, w as IEnchantable, e.lifeTime, null, 0);
                else
                    w.AddEnchantment(enchNegative, w as IEnchantable, e.lifeTime, null, 0);
            }
        }
        static public void EAPP_DoomMasteryWizard(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            if (!(target is MOM.PlayerWizard)) return;

            var wizard = target as MOM.PlayerWizard;
            var wizardTowns = GameManager.Get().registeredLocations.FindAll(o => o.GetOwnerID() == wizard.ID);

            var ench = (Enchantment)DataBase.Get("ENCH-DOOM_MASTERY_CITY", false);

            foreach (var t in wizardTowns)
            {
                t.AddEnchantment(ench, wizard as IEnchantable, ench.lifeTime, null, 0);
            }
        }
        static public void EAPP_MeteorStorm(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            if (!(target is GameManager)) return;

            var wizardID = 0;
            if (ei.owner != null)
                wizardID = ei.owner.ID;

            var wizardOwner = GameManager.GetWizard(wizardID);

            List<MOM.Location> wizardsLocations = GameManager.Get().registeredLocations;
            List<MOM.Group> wizardsGroups = GameManager.Get().registeredGroups;

            var enchTown = (Enchantment)DataBase.Get("ENCH-METEOR_STORM_CITY", false);

            //Find all towns that are not caster town and not neutral
            foreach (var l in wizardsLocations)
            {
                if (!(l is TownLocation) || l.GetOwnerID() == wizardID || l.GetOwnerID() == 0) continue;

                var town = l as TownLocation;
                if (IsTownProtected(town.GetWizardOwner(), ei.source, town)) continue;

                l.AddEnchantment(enchTown, wizardOwner, enchTown.lifeTime, null, 0);

            }

            foreach (var w in GameManager.Get().wizards)
            {
                if (w.GetID() == wizardID) continue;
                w.GetAttributes().AddToBase((Tag)TAG.OUTPOST_WARNING, FInt.ONE);
            }
/*

            //Find all groups that are not caster group not neutral
            foreach (var g in wizardsGroups) 
            {
                / *if (g.GetOwnerID() == wizardID || g.GetOwnerID() == 0) continue;* /
                if (g.GetLocationHost()?.otherPlaneLocation?.Get() != null && g.plane.arcanusType) continue;
                

                foreach (var u in g.GetUnits())
                {
                    u.Get().attributes.AddToBase(TAG.METEOR_STORM_AFFECTED, FInt.ONE);
                    u.Get().GetAttributes().SetDirty();
                }
            }*/
        }
        static public void EAPP_NaturesWrath(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            if (!(target is GameManager)) return;

            var wizardID = ei.owner.ID;
            var wizardOwner = GameManager.GetWizard(wizardID);

            List<MOM.Location> wizardsLocations = GameManager.Get().registeredLocations;

            var enchTown = (Enchantment)DataBase.Get("ENCH-NATURES_WRATH_CITY", false);

            //Find all towns that are not caster
            foreach (var l in wizardsLocations)
            {
                if (!(l is TownLocation) || l.GetOwnerID() == wizardID || l.GetOwnerID() == 0) continue;

                var town = l as TownLocation;
                if (IsTownProtected(town.GetWizardOwner(), ei.source, town)) continue;

                var locationOwnerAtt = l.GetWizardOwner().GetAttributes();
                if (locationOwnerAtt.Contains(TAG.CHAOS_MAGIC_BOOK) ||
                    locationOwnerAtt.Contains(TAG.DEATH_MAGIC_BOOK))
                {
                    l.AddEnchantment(enchTown, wizardOwner, enchTown.lifeTime, null, 0);
                }
            }
        }
        static public void EAPP_GreatWastingCitiesUnrest(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            if (!(target is GameManager)) return;

            var wizardID = ei.owner.ID;
            var wizardOwner = GameManager.GetWizard(wizardID);

            List<MOM.Location> wizardsLocations = GameManager.Get().registeredLocations;

            var enchTown = (Enchantment)DataBase.Get("ENCH-GREAT_WASTING_CITY", false);

            foreach (var l in wizardsLocations)
            {
                if (!(l is TownLocation) || l.GetOwnerID() == wizardID || l.GetOwnerID() == 0) continue;

                var town = l as TownLocation;
                if (IsTownProtected(town.GetWizardOwner(), ei.source, town)) continue;
                l.AddEnchantment(enchTown, wizardOwner, enchTown.lifeTime, null, 0);
            }

        }
        static public void EAPP_BlurGlobal(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            //obsolete script, not in use right now
            var battle = Battle.GetBattle();

            if (ei == null) return;
            var ownerID = ei.owner.ID;
            var hero = battle.GetAllUnits().Find(o => o.ID == ownerID);
            PlayerWizard spellCasterowner = null;

            if (hero == null)
            {
                spellCasterowner = GameManager.Get().wizards.Find(o => o.ID == ownerID);
            }
            else
            {
                spellCasterowner = hero.GetWizardOwner();
            }

            List<BattleUnit> ownerUnits;
            Enchantment ownerUnitsEnch = (Enchantment)DataBase.Get("ENCH-BLUR", false);

            if (battle.attacker.GetID() == spellCasterowner.ID)
            {
                ownerUnits = battle.attackerUnits;
            }
            else
            {
                ownerUnits = battle.defenderUnits;
            }

            //Add Negative Enchantment On Unit
            foreach (var u in ownerUnits)
            {
                if (!u.IsAlive()) continue;
                u.AddEnchantment(ownerUnitsEnch, spellCasterowner as IEnchantable, ownerUnitsEnch.lifeTime);
            }

            if (battle != null)
            {
                foreach (var v in battle.buToSource)
                {
                    v.Key.GetAttributes().SetDirty();
                }
            }
        }
        static public void EAPP_PrayerGlobal(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            var battle = Battle.GetBattle();
            Enchantment positiveEnch = (Enchantment)ENCH.PRAYER_POSITIVE;
            Enchantment negativeEnch = (Enchantment)ENCH.PRAYER_NEGATIVE;

            if (battle == null || ei == null)
            {
                Debug.LogWarning("EAPP_HighPrayerGlobal: battle or ench instance is null");
                return;
            }
            
            PlayerWizard casterOwner = GetSpellCasterOwner(ei);

            if (battle.activeTurn.isAttackerTurn)
            {
                battle.attacker.AddEnchantment(positiveEnch, casterOwner, positiveEnch.lifeTime);
                battle.defender.AddEnchantment(negativeEnch, casterOwner, negativeEnch.lifeTime);
            }
            else
            {
                battle.defender.AddEnchantment(positiveEnch, casterOwner, positiveEnch.lifeTime);
                battle.attacker.AddEnchantment(negativeEnch, casterOwner, negativeEnch.lifeTime);
            }
        }
        static public void EAPP_HighPrayerGlobal(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            var battle = Battle.GetBattle();
            Enchantment positiveEnch = (Enchantment)ENCH.HIGH_PRAYER_POSITIVE;
            Enchantment negativeEnch = (Enchantment)ENCH.HIGH_PRAYER_NEGATIVE;

            if (battle == null || ei == null) 
            {
                Debug.LogWarning("EAPP_HighPrayerGlobal: battle or ench instance is null");
                return; 
            }
            PlayerWizard casterOwner = GetSpellCasterOwner(ei);

            if (battle.activeTurn.isAttackerTurn)
            {
                battle.attacker.AddEnchantment(positiveEnch, casterOwner, positiveEnch.lifeTime);
                battle.defender.AddEnchantment(negativeEnch, casterOwner, negativeEnch.lifeTime);
            }
            else
            {
                battle.defender.AddEnchantment(positiveEnch, casterOwner, positiveEnch.lifeTime);
                battle.attacker.AddEnchantment(negativeEnch, casterOwner, negativeEnch.lifeTime);
            }
        }
        static public void EAPP_TrueLightGlobal(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            var battle = Battle.GetBattle();
            EAPP_AllBattleUnitsAttributeDirty(target, e, ei);

            foreach (var bu in battle.GetAllUnits())
            {
                if (!bu.IsAlive()) continue;

                if (bu.race == (Race)RACE.REALM_LIFE)
                {
                    FSMBattleTurn.instance?.CastEffect(bu.GetPosition(), "Effect_TrueLightPositive");
                }
                else if (bu.race == (Race)RACE.REALM_DEATH)
                {
                    FSMBattleTurn.instance?.CastEffect(bu.GetPosition(), "Effect_TrueLightNegative");
                }
            }
        }
        static public void EAPP_DarknessBattle(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            //obsolete script, not in use right now
            var battle = Battle.GetBattle();
            if (battle == null)
            {
                Debug.LogError("EAPP_DarknessBattle battle is null");
                return;
            }

            var enchInstance = target.GetEnchantmentManager().GetEnchantments().Find(o => o.source == (Enchantment)ENCH.DARKNESS_BATTLE);
            if (enchInstance == null)
            {
                Debug.LogError("EAPP_DarknessBattle enchInstance is null");
                return;
            }

            var ownerID = enchInstance.owner.ID;

            PlayerWizard spellCaster = GameManager.Get().wizards.Find(o => o.ID == ownerID);
            Enchantment positiveUnitsEnch = (Enchantment)DataBase.Get("ENCH-DARKNESS_POSITIVE", false);
            Enchantment negativeUnitsEnch = (Enchantment)DataBase.Get("ENCH-DARKNESS_NEGATIVE", false);

            //Add Negative Enchantment On Unit
            foreach (var u in battle.attackerUnits)
            {
                if (!u.IsAlive()) continue;
                if (u.race == (Race)RACE.REALM_DEATH)
                    u.AddEnchantment(positiveUnitsEnch, spellCaster as IEnchantable, positiveUnitsEnch.lifeTime);
                if (u.race == (Race)RACE.REALM_LIFE )
                    u.AddEnchantment(negativeUnitsEnch, spellCaster as IEnchantable, negativeUnitsEnch.lifeTime);
            }
            foreach (var u in battle.defenderUnits)
            {
                if (!u.IsAlive()) continue;
                if (u.race == (Race)RACE.REALM_DEATH)
                    u.AddEnchantment(positiveUnitsEnch, spellCaster as IEnchantable, positiveUnitsEnch.lifeTime);
                if (u.race == (Race)RACE.REALM_LIFE)
                    u.AddEnchantment(negativeUnitsEnch, spellCaster as IEnchantable, negativeUnitsEnch.lifeTime);
            }

            foreach (var v in battle.buToSource)
            {
                v.Key.GetAttributes().SetDirty();
            }

        }

        static public void EAPP_NatureProtection(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            var tl = target as TownLocation;
            if (tl == null) return;

            tl.isNatureProtected++;
        }
        static public void EAPP_SorceryProtection(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            var tl = target as TownLocation;
            if (tl == null) return;

            tl.isSorceryProtected ++;
        }
        static public void EAPP_ChaosProtection(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            var tl = target as TownLocation;
            if (tl == null) return;

            tl.isChaosProtected++;
        }
        static public void EAPP_LifeProtection(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            var tl = target as TownLocation;
            if (tl == null) return;

            tl.isLifeProtected++;
        }
        static public void EAPP_DeathProtection(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            var tl = target as TownLocation;
            if (tl == null) return;

            tl.isDeathProtected++;
        }
        static public void EAPP_Consecration(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            var tl = target as TownLocation;
            if (tl == null) return;

            tl.isChaosProtected++;
            tl.isDeathProtected++;
        }
        static public void EAPP_AddCounterMagic(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            if (target is GameManager)
            {
                var gameManager = GameManager.Get();

                if (gameManager == null) return;

                gameManager.worldCounterMagic++ ;
            }

            if (target is Battle)
            {
                if (!(target is Battle)) return;

                var battle = target as Battle;

                if (battle == null) return;

                battle.battleCounterMagic++;
            }
        }
        static public void EAPP_Armageddon(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            var gm = target as GameManager;
            if (gm == null) return;

            var wizards = GameManager.GetWizards();

            foreach (var w in wizards)
            {
                if (w != ei.owner.GetEntity())
                {
                    List<MOM.Location> locs = GameManager.GetLocationsOfWizard(w.GetID());

                    foreach (var l in locs)
                    {
                        if (l is TownLocation)
                        {
                            var t = l as TownLocation;
                            Enchantment ench = (Enchantment)ENCH.ADD_ARMAGEDDON_UNREST;
                            t.AddEnchantment(ench, ei.owner.GetEntity(), ench.lifeTime);
                        }
                    }
                }
            }
        }
        static public void EAPP_FortressLightningBolt(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            var town = target as TownLocation;
            TAG wizardMagic;
            realmToTag.TryGetValue(ei.source.Get().realm, out wizardMagic);
            var wizard = town.GetWizardOwner();
            if (wizard != null &&
                wizard.GetAttributes().DoesNotContains((Tag)wizardMagic))
            {
                town.RemoveEnchantment(e);
            }
        }
        static public void EAPP_EvilOmens(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            var gameMenager = GameManager.Get();

            if (gameMenager == null ) Debug.LogError("EAPP_EternalNight gameMenager null.");
            if (ei == null) Debug.LogError("EAPP_EternalNight ei is null.");

            var spellCaster = GameManager.GetWizard(ei.owner.ID);

            foreach (var wizard in gameMenager.wizards)
            {
                if (wizard == spellCaster) continue;
                wizard.castCostPercentDiscountRealms[ERealm.Life] -= 0.50f;
                wizard.castCostPercentDiscountRealms[ERealm.Nature] -= 0.50f;
            }
        }
        static public void EAPP_WallOfFireBattle(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            if (!(target is Battle)) return;

            var b = target as Battle;
            if (b.fireWall) return;

            var ench = b.GetEnchantments();            
            var fire = ench.Find(o => o.source.Get() == (Enchantment)ENCH.WALL_OF_FIRE) != null;
            
            if (fire)
            {
                var objcs = b.AddWalls("_Fire");
                b.fireWallGo = objcs;
                b.fireWall = true;
            }
        }
        static public void EAPP_WallOfDarknessBattle(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            if (!(target is Battle)) return;

            var b = target as Battle;
            if (b.darknessWall) return;

            var ench = b.GetEnchantments();
            var dark = ench.Find(o => o.source.Get() == (Enchantment)ENCH.WALL_OF_DARKNESS) != null;

            if (dark)
            {
                var objcs = b.AddWalls("_Darkness");
                b.darnkessWallGo = objcs;
                b.darknessWall = true;
            }
        }
        static public void EAPP_AstralGate(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            if (!(target is TownLocation)) return;

            TownLocation tl = target as TownLocation;
            if(!tl.HaveBuilding((Building)BUILDING.ASTRAL_GATE))
            {
                tl.AddBuilding((Building)BUILDING.ASTRAL_GATE);
            }
            tl.InitializeAstralGate();
        }
        static public void EAPP_EarthGate(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            if (!(target is TownLocation)) return;

            TownLocation tl = target as TownLocation;
            if (!tl.HaveBuilding((Building)BUILDING.EARTH_GATE))
            {
                tl.AddBuilding((Building)BUILDING.EARTH_GATE);
            }
        }

        static public void EAPP_NaturesEye(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            if (!(target is TownLocation)) return;

            TownLocation tl = target as TownLocation;
            if (!tl.HaveBuilding((Building)BUILDING.NATURES_EYE))
            {
                tl.AddBuilding((Building)BUILDING.NATURES_EYE);
            }
        }

        static public void EAPP_StreamOfLife(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            if (!(target is TownLocation)) return;

            TownLocation tl = target as TownLocation;
            if (!tl.HaveBuilding((Building)BUILDING.STREAM_OF_LIFE))
            {
                tl.AddBuilding((Building)BUILDING.STREAM_OF_LIFE);
            }
        }

        static public void EAPP_AltarOfBattle(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            if (!(target is TownLocation)) return;

            TownLocation tl = target as TownLocation;
            if (!tl.HaveBuilding((Building)BUILDING.ALTAR_OF_BATTLE))
            {
                tl.AddBuilding((Building)BUILDING.ALTAR_OF_BATTLE);
            }
        }
        static public void EAPP_FlyingFortress(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            if (!(target is TownLocation)) return;

            var town = target as TownLocation;

            foreach (var u in town.GetUnits())
            {
                if (u.Get().GetAttFinal(TAG.CAN_FLY) <= FInt.ZERO)
                    u.Get().GetAttributes().AddToBase(TAG.CAN_FLY, FInt.ONE);
            }

        }

        static public void EAPP_AddTimeStop(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            var wiz = ei.owner.Get<PlayerWizard>();
            var gm = GameManager.Get();
            if (wiz != null && gm != null)
            {
                gm.timeStopMaster = wiz;
                if(TurnManager.Get().playerTurn) HUD.Get()?.UpdateHUD();
            }
        }
        static public void EAPP_EternalNight(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            for(int i = 1; i < GameManager.GetWizards().Count; i++)
            {
                foreach(var l in GameManager.GetTownsOfWizard(i))
                {
                    var ench = l.GetEnchantments().Find(o => o.source == (Enchantment)ENCH.CLOUD_OF_SHADOW);
                    if(ench != null)
                    {
                        l.RemoveEnchantment(ench);
                        if(i == PlayerWizard.HumanID())
                        {
                            // TODO
                        }
                    }
                }
            }

        }
        static public void EAPP_MassShield(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            var battle = Battle.GetBattle();
            if (battle == null)
            {
                Debug.LogError("EAPP_MassShield battle is null");
                return;
            }

            int casterID = GetSpellCasterOwnerID(ei);
            List<BattleUnit> ownerUnits;

            if (battle.attacker.GetID() == casterID)
            {
                ownerUnits = battle.attackerUnits;
            }
            else
            {
                ownerUnits = battle.defenderUnits;
            }
            var massShield = (Spell)SPELL.MASS_SHIELD;
            var largeShield = (Skill)SKILL.LARGE_SHIELD;
            foreach (var u in ownerUnits)
            {
                if (!u.IsAlive()) continue;

                if (!u.GetSkills().Contains(largeShield))
                {
                    u.GetSkills().Add(largeShield);
                    u.GetAttributes().SetDirty();
                    if (!u.simulated && battle != null)
                    {
                        var effect = massShield.castEffect;
                        FSMBattleTurn.instance?.CastEffect(u.GetPosition(), effect);
                    }
                }
            }
        }
        static public void EAPP_MassPiercing(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            var battle = Battle.GetBattle();
            if (battle == null)
            {
                Debug.LogError("EAPP_MassPiercing battle is null");
                return;
            }

            int casterID = GetSpellCasterOwnerID(ei);
            List<BattleUnit> ownerUnits;

            if (battle.attacker.GetID() == casterID)
            {
                ownerUnits = battle.attackerUnits;
            }
            else
            {
                ownerUnits = battle.defenderUnits;
            }

            foreach (var u in ownerUnits)
            {
                if (!u.IsAlive()) continue;

                if (!u.GetSkills().Contains((Skill)SKILL.ARMOR_PIERCING))
                {
                    u.GetSkills().Add((Skill)SKILL.ARMOR_PIERCING);
                    u.GetAttributes().SetDirty();
                    if (!u.simulated && u.IsAlive() && battle != null)
                    {
                        var effect = ((Spell)SPELL.MASS_PIERCING).castEffect;
                        FSMBattleTurn.instance?.CastEffect(u.GetPosition(), effect);
                    }
                }
            }
        }
        static public void EAPP_CastingBlock(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            var battle = Battle.GetBattle();
            if (battle == null)
            {
                Debug.LogError("EAPP_CastingBlock battle is null");
                return;
            }

            if(target is BattlePlayer)
            {
                (target as BattlePlayer).castingBlock = true;
            }
        }
        static public void EAPP_PowerPeople(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            if (!(target is PlayerWizard)) return;

            var wizardOwner = target as PlayerWizard;

            List<MOM.Location> wizardsLocations = GameManager.Get().registeredLocations;

            var enchTown = (Enchantment)DataBase.Get("ENCH-POWER_PEOPLE_CITY", false);

            foreach (var l in wizardsLocations)
            {
                //Find all locations that are not town or that are not casters town and not neutral
                if (!(l is TownLocation) || wizardOwner == null || l.GetOwnerID() == 0 ||
                    l.GetOwnerID() != wizardOwner.GetID() ||
                    wizardOwner.wizardTower.Get() != l) continue;

                if(l.GetEnchantments().Find( o => o.source == enchTown) != null) continue;
                l.AddEnchantment(enchTown, wizardOwner, enchTown.lifeTime, null, 0);
            }
        }
        #endregion
        #region EnchantmentRemoval


        static public void EREM_NatureProtection(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            var town = target as TownLocation;
            if (town == null)
                Debug.LogError("Designed to remove ENCH effect from town.");

            town.isNatureProtected--;
            RemoveSpellWardEnfeeblingHex(ref town);

        }
        static public void EREM_SorceryProtection(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            var town = target as TownLocation;
            if (town == null)
                Debug.LogError("Designed to remove ENCH effect from town.");

            town.isSorceryProtected--;
            RemoveSpellWardEnfeeblingHex(ref town);
        }
        static public void EREM_ChaosProtection(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            var town = target as TownLocation;
            if (town == null)
                Debug.LogError("Designed to remove ENCH effect from town.");

            town.isChaosProtected--;
            RemoveSpellWardEnfeeblingHex(ref town);
        }
        static public void EREM_LifeProtection(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            var town = target as TownLocation;
            if (town == null)
                Debug.LogError("Designed to remove ENCH effect from town.");

            town.isLifeProtected--;
            RemoveSpellWardEnfeeblingHex(ref town);
        }
        static public void EREM_DeathProtection(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            var town = target as TownLocation;
            if (town == null)
                Debug.LogError("Designed to remove ENCH effect from town.");

            town.isDeathProtected--;
            RemoveSpellWardEnfeeblingHex(ref town);
        }
        static public void EREM_Consecration(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            var town = target as TownLocation;
            if (town == null)
                Debug.LogError("Designed to remove ENCH effect from town.");

            town.isChaosProtected--;
            town.isDeathProtected--;
        }
        static public void EREM_VisibilityApplicator(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            var plane = World.GetActivePlane();
            if (plane.battlePlane) return;

            FOW.Get().UpdateFogForPlane(plane);
        }
        static public void EREM_AllOwnUnitsAttributeDirty(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            if (target.GetWizardOwner() == null)
                Debug.LogError("You try set dirty all neutral units.");

            var groups = GameManager.GetGroupsOfWizard(target.GetWizardOwner().ID);
            if (groups != null)
            {
                foreach (var v in groups)
                {
                    var units = v.GetUnits();
                    if (units != null)
                    {
                        foreach (var u in units)
                        {
                            u.Get().GetAttributes().SetDirty();
                        }
                    }
                }
            }
        }
        static public void EREM_UnitUpdateMove(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            if (target is MOM.Unit)
            {
                var u = target as MOM.Unit;
                u.GetAttributes().SetDirty();
                u.group?.Get().UpdateMovementFlags();
                if (FSMSelectionManager.Get().GetSelectedGroup() == u.group?.Get() && 
                    u.GetWizardOwnerID() == 1)
                {
                    MHEventSystem.TriggerEvent<HUD>(HUD.Get(), null);
                }
            }
            if (target is BattleUnit)
            {
                BattleUnit bu = target as BattleUnit;
                bu.GetAttributes().SetDirty();

                if (BattleHUD.GetSelectedUnit() == bu && bu.GetWizardOwnerID() == 1)
                {
                    MHEventSystem.TriggerEvent<BattleHUD>(BattleHUD.Get(), null);
                }
            }
        }
        static public void EREM_Haste(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
           
            if (target is BattleUnit)
            {
                BattleUnit bu = target as BattleUnit;
                if(bu.canMove)
                {
                    var maxMP = bu.GetAttFinal((Tag)TAG.MOVEMENT_POINTS);
                    var newMP = FInt.Max(bu.Mp - maxMP, FInt.ZERO);
                    bu.Mp = newMP;

                    if (BattleHUD.GetSelectedUnit() == bu && bu.GetWizardOwnerID() == 1)
                    {
                        MHEventSystem.TriggerEvent<BattleHUD>(BattleHUD.Get(), null);
                    }
                }
            }
        }
        static public void EREM_AllChaosUnitsAttributeDirty(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            var groups = GameManager.Get().registeredGroups;
            if (groups != null)
            {
                foreach (var v in groups)
                {
                    if (v.GetLocationHost()?.otherPlaneLocation?.Get() != null && v.plane.arcanusType) continue;
                    var units = v.GetUnits();
                    if (units != null)
                    {
                        foreach (var u in units)
                        {
                            if (u.Get().race == (Race)RACE.REALM_CHAOS)
                            {
                                u.Get().GetAttributes().SetDirty();
                            }
                        }
                    }
                }
            }
        }
        static public void EREM_BattleUnitAttributeDirty(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            var bu = target as BattleUnit;
            if (bu != null)
            {
                bu.GetAttributes().SetDirty();
            }
        }
        static public void EREM_OwnBattleUnitsAttributeDirty(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            var b = Battle.GetBattle();
            if (b != null)
            {
                int casterID = GetSpellCasterOwnerID(ei);
                bool attacker = casterID == b.attacker.GetID();

                if (attacker)
                {
                    foreach (var bu in b.attackerUnits)
                    {
                        bu.GetAttributes().SetDirty();
                    }
                }
                else
                {
                    foreach (var bu in b.defenderUnits)
                    {
                        bu.GetAttributes().SetDirty();
                    }
                }
            }
        }
        static public void EREM_AllBattleUnitsAttributeDirty(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            var b = Battle.GetBattle();
            if (b != null)
            {
                foreach (var v in b.buToSource)
                {
                    v.Key.GetAttributes().SetDirty();
                }
            }
        }
        static public void EREM_AllBattleUnitsAttributeDirtyAndRemoveEnch(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            var b = Battle.GetBattle();
            if (b != null)
            {
                foreach (var v in b.buToSource)
                {
                    v.Key.GetAttributes().SetDirty();
                }
                if(ei != null)
                {
                    b.RemoveEnchantment(ei);
                }
                else
                {
                    foreach (var ench in b.GetEnchantments())
                    {
                        DBDef.Enchantment enchToRemove = (DBDef.Enchantment)DataBase.Get(e.removalScript.stringData, false);
                        if (ench.source == enchToRemove && ench.owner.ID == ei.owner.ID)
                        {
                            b.RemoveEnchantment(ench.source);
                        }
                    }
                }
            }
        }
        static public void EREM_BattleUnitsAttributeDirtyUpdateInvisibility(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            var b = Battle.GetBattle();
            if (b != null)
            {

                foreach (var v in b.buToSource)
                {
                    v.Key.GetAttributes().SetDirty();
                }
                b.UpdateInvisibility();
            }
        }
        static public void EREM_SetAllowPlaneSwitchTrue(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            var planarSeal = GameManager.Get().GetEnchantments().Find(o => o.source == (Enchantment)ENCH.PLANAR_SEAL && 
                                                                           o.owner != ei.owner); 

            if(planarSeal != null)
            {
                //someone still have planar seal, planes are sealed
                return;
            }
            
            GameManager.Get().AllowPlaneSwitch(true);

            foreach (var v in GameManager.Get().registeredLocations)
            {
                if (v.otherPlaneLocation != null)
                {
                    if (v.model != null)
                    {
                        var g = GameObjectUtils.FindByName(v.model, "PlanarSeal");
                        if (g != null) g.SetActive(false);
                    }
                }
            }
        }

        static public void EREM_Invisibility(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            if(target is BattleUnit)
            {
                var bu = target as BattleUnit;
                bu.GetAttributes().SetDirty();

                var b = Battle.Get();
                if(b != null)
                {
                    b.UpdateInvisibility();
                }
            }
            else if(target is MOM.Unit)
            {
                var u = target as MOM.Unit;
                u.GetAttributes().SetDirty();

                var gr = u.group;
                if(gr != null)
                {
                    gr.Get().UpdateMarkers();
                    gr.Get().UpdateMapFormation(false);
                }
            }
        }
        static public void EREM_JustCause(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            if (!(target is MOM.PlayerWizard)) return;

            var wizard = target as MOM.PlayerWizard;
            wizard.TakeTemporaryFame(10);
            if (wizard.IsHuman)
            {
                HUD.Get().UpdateHUD();
            }
            wizard.rebelModifier += FInt.ONE;
        }
        static public void EREM_Crusade(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            if (!(target is MOM.PlayerWizard)) return;

            var wizard = target as MOM.PlayerWizard;
            wizard.unitLevelIncrease -= 1;

            var wizardGroups = GameManager.GetGroupsOfWizard(wizard.GetID());
            foreach (var g in wizardGroups)
            {
                foreach (var u in g.GetUnits())
                {
                    u.Get().GetAttributes().SetDirty();
                    u.Get().group?.Get()?.TriggerOnJoinScripts(u);
                }
            }
        }

        static public void EREM_RemoveSkill(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            if (!(target is ISkillable)) return;

            var u = target as ISkillable;
            Skill skill = DataBase.Get(e.removalScript.stringData, false) as Skill;
            if (skill == null)
            {
                Debug.LogError(e.removalScript.stringData + " is not a Skill. You have a typo?");
                return;
            }
            u.RemoveSkill(skill);
        }
        static public void EREM_Heroism(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            if (!(target is MOM.BaseUnit)) return;

            var u = target as MOM.BaseUnit;

            if (u.GetWizardOwner()?.GetEnchantments().Find(o => o.source == (Enchantment)ENCH.HERO_TRAINING) != null)
                u.levelOverride = 3;
            else
                u.levelOverride = 0;

            u.GetAttributes().SetDirty();
        }

        static public void EREM_MindControl(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            Battle b = Battle.GetBattle();
            if (!(target is BattleUnit) || b == null) return;

            BattleUnit bu = target as BattleUnit;
            if (!bu.IsAlive()) return;
            
            if (b.attackerUnits.Contains(bu))
            {
                //do not modify battlefield during simulations, simulations does not own it!
                if (!bu.simulated) b.AttackerRemoveUnit(bu);
                if (!bu.simulated) b.DefenderAddUnit(bu);
                bu.ownerID = b.defender.GetID();
                bu.attackingSide = false;
            }
            else
            {
                //do not modify battlefield during simulations, simulations does not own it!
                if (!bu.simulated) b.DefenderRemoveUnit(bu);
                if (!bu.simulated) b.AttackerAddUnit(bu);
                bu.ownerID = b.attacker.GetID();
                bu.attackingSide = true;
            }
            if (!bu.simulated) b.UnitListsDirty();

            if(BattleHUD.Get() != null) BattleHUD.Get().BaseUpdate();
            if(bu.IsAlive())
            {
                MHUtils.UI.VerticalMarkerManager.Get().UpdateMarkerColors(target);
            }

            bu.UpdateUnitMP();
        }
        static public void EREM_RemoveWindMasteryFromWizards(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            if (!(target is GameManager)) return;

            var wizardID = ei.owner.ID;

            var wizards = GameManager.Get().wizards;
            var enchPositive = (Enchantment)DataBase.Get("ENCH-WIND_MASTERY_UNIT_POSITIVE", false);
            var enchNegative = (Enchantment)DataBase.Get("ENCH-WIND_MASTERY_UNIT_NEGATIVE", false);

            foreach (var w in wizards)
            {
                if (w.ID == wizardID)
                    w.RemoveEnchantment(enchPositive);
                else
                    w.RemoveEnchantment(enchNegative);
            }
        }
        static public void EREM_DoomMasteryWizard(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            if (!(target is MOM.PlayerWizard)) return;

            var wizard = target as MOM.PlayerWizard;
            var wizardTowns = GameManager.Get().registeredLocations.FindAll(o => o.GetOwnerID() == wizard.ID);

            var ench = (Enchantment)DataBase.Get("ENCH-DOOM_MASTERY_CITY", false);

            foreach (var t in wizardTowns)
            {
                t.RemoveEnchantment(ench);
            }
        }
        static public void EREM_MeteorStorm(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            if (!(target is GameManager)) return;

            List<MOM.Location> wizardsLocations = GameManager.Get().registeredLocations;
            List<MOM.Group> wizardsGroups = GameManager.Get().registeredGroups;
            var enchTown = (Enchantment)DataBase.Get("ENCH-METEOR_STORM_CITY", false);

            foreach (var l in wizardsLocations)
            {
                var town = l as TownLocation;
                if (town == null || l.GetOwnerID() == 0) continue;
                var meteorStormOnCity = town.GetEnchantments().Find(o => o.source == enchTown);
                if (meteorStormOnCity != null && ei.owner == meteorStormOnCity.owner) 
                    l.RemoveEnchantment(enchTown);
            }

            foreach (var g in wizardsGroups)
            {
                /*if ((ei != null && g.GetOwnerID() == ei.owner.ID) || g.GetOwnerID() == 0) continue;*/
                if (g.GetLocationHost()?.otherPlaneLocation?.Get() != null && g.plane.arcanusType) continue;

                foreach (var u in g.GetUnits())
                {
                    if (u.Get().attributes.GetFinal(TAG.METEOR_STORM_AFFECTED) > 0)
                    {
                        u.Get().GetAttributes().AddToBase((Tag)TAG.METEOR_STORM_AFFECTED, FInt.N_ONE);
                        u.Get().GetAttributes().SetDirty();
                    }
                }
            }
            foreach (var w in GameManager.Get().wizards)
            {
                if (ei.owner != null && w.GetID() == ei.owner.ID) continue;
                w.GetAttributes().AddToBase((Tag)TAG.OUTPOST_WARNING, FInt.N_ONE);
            }
        }
        static public void EREM_NaturesWrath(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            if (!(target is GameManager)) return;

            List<MOM.Location> wizardsLocations = GameManager.Get().registeredLocations;
            var enchTown = (Enchantment)DataBase.Get("ENCH-NATURES_WRATH_CITY", false);

            foreach (var l in wizardsLocations)
            {
                if (!(l is TownLocation) || l.GetOwnerID() == 0) continue;
                l.RemoveEnchantment(enchTown);
            }
        }
        static public void EREM_GreatWastingCitiesUnrest(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            if (!(target is GameManager)) return;

            List<MOM.Location> wizardsLocations = GameManager.Get().registeredLocations;
            var enchTown = (Enchantment)DataBase.Get("ENCH-GREAT_WASTING_CITY", false);

            foreach (var l in wizardsLocations)
            {
                if (!(l is TownLocation) || l.GetOwnerID() == 0) continue;
                l.RemoveEnchantment(enchTown);
            }
        }
        static public void EREM_BlurGlobal(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            //obsolete script, not in use right now
            var b = Battle.GetBattle();
            if (b != null)
            {
                List<BattleUnit> buA;
                if (ei.owner.ID == b.attacker.GetID())
                {
                    buA = b.attackerUnits;
                }
                else
                {
                    buA = b.defenderUnits;
                }

                foreach (var v in buA)
                {
                    var baEnch = v.GetEnchantments();
                    if (baEnch.Find(o => o.source.Get() == (Enchantment)ENCH.BLUR) != null)
                        v.RemoveEnchantment((Enchantment)ENCH.BLUR);

                    v.GetAttributes().SetDirty();
                }
            }
        }
        static public void EREM_PrayerGlobal(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            var b = Battle.GetBattle();
            if (b == null)
            {
                Debug.LogError("EREM_PrayerGlobal battle is null");
                return;
            }

            Enchantment positiveEnch = (Enchantment)ENCH.PRAYER_POSITIVE;
            Enchantment negativeEnch = (Enchantment)ENCH.PRAYER_NEGATIVE;

            BattlePlayer casterOwner = null;
            if (ei.owner.ID == b.attacker.GetID())
            {
                casterOwner = b.attacker;
            }
            else if (ei.owner.ID == b.defender.GetID())
            {
                casterOwner = b.defender;
            }
            else
            {
                var wizard = b.GetAllUnits().Find(o => o.GetID() == ei.owner.ID)?.GetWizardOwner();
                if (wizard != null)
                    casterOwner = b.GetBattlePlayerForWizard(wizard);
            }
            if (casterOwner == null)
            {
                Debug.LogWarning("EREM_PrayerGlobal caster is null");
                return;
            }

            if (casterOwner.GetEnchantments().Find(o => o.source.Get() == positiveEnch) != null)
            {
                casterOwner.RemoveEnchantment(positiveEnch);
            }
            if (b.GetOtherPlayer(casterOwner).GetEnchantments().Find(o => o.source.Get() == negativeEnch) != null)
            {
                b.GetOtherPlayer(casterOwner).RemoveEnchantment(negativeEnch);
            }
            EREM_AllBattleUnitsAttributeDirty(target, e, ei);
        }
        static public void EREM_HighPrayerGlobal(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            var b = Battle.GetBattle();
            if (b == null)
            {
                Debug.LogError("EREM_HighPrayerGlobal battle is null");
                return;
            }

            Enchantment positiveEnch = (Enchantment)ENCH.HIGH_PRAYER_POSITIVE;
            Enchantment negativeEnch = (Enchantment)ENCH.HIGH_PRAYER_NEGATIVE;

            BattlePlayer casterOwner = null;
            if(ei.owner.ID == b.attacker.GetID())
            {
                casterOwner = b.attacker;
            }
            else if (ei.owner.ID == b.defender.GetID())
            {
                casterOwner = b.defender;
            }
            else
            {
                var wizard = b.GetAllUnits().Find(o => o.GetID() == ei.owner.ID)?.GetWizardOwner();
                if (wizard != null)
                    casterOwner = b.GetBattlePlayerForWizard(wizard);
            }
            if(casterOwner == null)
            {
                Debug.LogWarning("EREM_HighPrayerGlobal caster is null");
                return;
            }

            if (casterOwner.GetEnchantments().Find(o => o.source.Get() == positiveEnch) != null)
            {
                casterOwner.RemoveEnchantment(positiveEnch);
            }
            if (b.GetOtherPlayer(casterOwner).GetEnchantments().Find(o => o.source.Get() == negativeEnch) != null)
            {
                b.GetOtherPlayer(casterOwner).RemoveEnchantment(negativeEnch);
            }
            EREM_AllBattleUnitsAttributeDirty(target, e, ei);
        }
        static public void EREM_TrueLightGlobal(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            var b = Battle.GetBattle();
            if (b != null)
            {
                foreach (var v in b.buToSource)
                {
                    var baEnch = v.Key.GetEnchantments();
                    if (baEnch.Find(o => o.source.Get() == (Enchantment)ENCH.TRUE_LIGHT_REALM_LIFE) != null)
                        v.Key.RemoveEnchantment((Enchantment)ENCH.TRUE_LIGHT_REALM_LIFE);
                    if (baEnch.Find(o => o.source.Get() == (Enchantment)ENCH.TRUE_LIGHT_REALM_DEATH) != null)
                        v.Key.RemoveEnchantment((Enchantment)ENCH.TRUE_LIGHT_REALM_DEATH);

                    v.Key.GetAttributes().SetDirty();
                }
            }
        }
        static public void EREM_RemoveCounterMagic(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            if (target is GameManager)
            {
                var gameManager = GameManager.Get();

                if (gameManager.GetEnchantments().Find(
                    o => o.source == ei.source) == null)
                {
                    gameManager.worldCounterMagic--;
                }
            }

            if (target is Battle)
            {
                var battle = target as Battle;

                if (battle == null) return;

                if (battle.GetEnchantments().Find(
                    o => o.source == ei.source) == null)
                {
                    battle.battleCounterMagic--;
                }
            }

        }
        static public void EREM_Armageddon(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            var gm = target as GameManager;
            if (gm == null) return;

            var wizards = GameManager.GetWizards();

            foreach (var w in wizards)
            {
                if (w != ei.owner.GetEntity())
                {
                    List<MOM.Location> locs = GameManager.GetLocationsOfWizard(w.GetID());

                    foreach (var l in locs)
                    {
                        if (l is TownLocation)
                        {
                            var t = l as TownLocation;
                            Enchantment ench = (Enchantment)ENCH.ADD_ARMAGEDDON_UNREST;
                            var armageddonEnch = t.GetEnchantments().Find(o => o.source == ench && o.owner == ei.owner);
                            if (armageddonEnch != null)
                            {
                                t.RemoveEnchantment(armageddonEnch);
                            }
                        }
                    }
                }
            }
        }
        static public void EREM_Confusion(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            var ba = target as BattleUnit;

            if (ba == null)
            {
                Debug.LogError("EREM_Confusion try to target non battle unit.");
            }

            ba.canAttack = true;
            ba.canCastSpells = true;
            ba.canMove = true;

            var possession = ba.GetEnchantments().Find(o => o.source == (Enchantment)ENCH.POSSESSION);
            if(possession != null)
            {
                ba.RemoveEnchantment(possession);
            }

            if (ba.IsAlive())
            {
                ba.UpdateUnitMP();
            }

        }
        static public void EREM_DetectMagic(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            if (ei == null) Debug.LogError("EREM_DetectMagic enchantment instance null");

            var wizardOwner = GameManager.GetWizard(ei.owner.ID);

            wizardOwner.detectMagic = false;
        }

        static public void EREM_DarknessBattle(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            //obsolete script, not in use right now
            var battle = Battle.GetBattle();
            if (battle == null)
            {
                Debug.LogError("EREM_DarknessGlobal battle is null");
                return;
            }

            if (ei == null)
            {
                Debug.LogError("EREM_DarknessGlobal enchInstance is null");
                return;
            }

            Enchantment positiveUnitsEnch = (Enchantment)DataBase.Get("ENCH-DARKNESS_POSITIVE", false);
            Enchantment negativeUnitsEnch = (Enchantment)DataBase.Get("ENCH-DARKNESS_NEGATIVE", false);

            //Add Negative Enchantment On Unit
            foreach (var u in battle.attackerUnits)
            {
                if (u.race == (Race)RACE.REALM_DEATH)
                    u.RemoveEnchantment(positiveUnitsEnch);
                if (u.race == (Race)RACE.REALM_LIFE)
                    u.RemoveEnchantment(negativeUnitsEnch);
            }

            foreach (var u in battle.defenderUnits)
            {
                if (u.race == (Race)RACE.REALM_DEATH)
                    u.RemoveEnchantment(positiveUnitsEnch);
                if (u.race == (Race)RACE.REALM_LIFE)
                    u.RemoveEnchantment(negativeUnitsEnch);
            }

            foreach (var v in battle.buToSource)
            {
                v.Key.GetAttributes().SetDirty();
            }
        }
        static public void EREM_EvilOmens(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            var gameMenager = GameManager.Get();

            if (gameMenager == null) Debug.LogError("EAPP_EternalNight gameMenager null.");
            if (ei == null) Debug.LogError("EAPP_EternalNight ei is null.");

            var spellCaster = GameManager.GetWizard(ei.owner.ID);

            foreach (var wizard in gameMenager.wizards)
            {
                if (wizard == spellCaster) continue;
                wizard.castCostPercentDiscountRealms[ERealm.Life] += 0.50f;
                wizard.castCostPercentDiscountRealms[ERealm.Nature] += 0.50f;
            }
        }
        static public void EREM_WallOfFireBattle(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            if (!(target is Battle)) return;

            var b = target as Battle;
            if (!b.fireWall) return;

            if(b.fireWallGo != null)
            {
                foreach(var g in b.fireWallGo)
                {
                    GameObject.Destroy(g);
                }
                b.fireWallGo = null;
            }

            b.fireWall = false;
        }
        static public void EREM_WallOfDarknessBattle(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            if (!(target is Battle)) return;

            var b = target as Battle;
            if (!b.darknessWall) return;

            if (b.darnkessWallGo != null)
            {
                foreach (var g in b.darnkessWallGo)
                {
                    GameObject.Destroy(g);
                }
                b.darnkessWallGo = null;
            }

            b.darknessWall = false;
        }

        static public void EREM_AstralGate(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            if (!(target is TownLocation)) return;

            TownLocation tl = target as TownLocation;
            if (tl.HaveBuilding((Building)BUILDING.ASTRAL_GATE))
            {
                if (TownScreen.Get() != null)
                {
                    var ts = TownScreen.Get();
                    if (ts.GetTown() == tl)
                    {
                        tl.RemoveBuilding((Building)BUILDING.ASTRAL_GATE);
                        ts.UpdateAll();
                    }
                }
                else
                {
                    tl.RemoveBuildingSpecial((Building)BUILDING.ASTRAL_GATE);
                }

                var plane = World.GetOtherPlane(tl.GetPlane());
                Hex hex = plane.GetHexAt(tl.GetPosition());
                if (hex.additionalDecorInstance != null)
                {
                    GameObject.Destroy(hex.additionalDecorInstance);
                    hex.additionalDecorInstance = null;
                }
            }
        }
        static public void EREM_EarthGate(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            if (!(target is TownLocation)) return;

            TownLocation tl = target as TownLocation;
            if (tl.HaveBuilding((Building)BUILDING.EARTH_GATE))
            {
                if (TownScreen.Get() != null)
                {
                    var ts = TownScreen.Get();
                    if (ts.GetTown() == tl)
                    {
                        tl.RemoveBuilding((Building)BUILDING.EARTH_GATE);
                        ts.UpdateAll();
                    }
                }
                else
                {
                    tl.RemoveBuildingSpecial((Building)BUILDING.EARTH_GATE);
                }
            }
        }

        static public void EREM_NaturesEye(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            if (!(target is TownLocation)) return;

            TownLocation tl = target as TownLocation;
            if (tl.HaveBuilding((Building)BUILDING.NATURES_EYE))
            {
                if (TownScreen.Get() != null)
                {
                    var ts = TownScreen.Get();
                    if (ts.GetTown() == tl)
                    {
                        tl.RemoveBuilding((Building)BUILDING.NATURES_EYE);
                        ts.UpdateAll();
                    }
                }
                else
                {
                    tl.RemoveBuildingSpecial((Building)BUILDING.NATURES_EYE);
                }
            }
        }

        static public void EREM_StreamOfLife(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            if (!(target is TownLocation)) return;

            TownLocation tl = target as TownLocation;
            if (tl.HaveBuilding((Building)BUILDING.STREAM_OF_LIFE))
            {
                if (TownScreen.Get() != null)
                {
                    var ts = TownScreen.Get();
                    if (ts.GetTown() == tl)
                    {
                        tl.RemoveBuilding((Building)BUILDING.STREAM_OF_LIFE);
                        ts.UpdateAll();
                    }
                }
                else
                {
                    tl.RemoveBuildingSpecial((Building)BUILDING.STREAM_OF_LIFE);
                }
            }
        }

        static public void EREM_AltarOfBattle(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            if (!(target is TownLocation)) return;

            TownLocation tl = target as TownLocation;
            if (tl.HaveBuilding((Building)BUILDING.ALTAR_OF_BATTLE))
            {
                if (TownScreen.Get() != null)
                {
                    var ts = TownScreen.Get();
                    if (ts.GetTown() == tl)
                    {
                        tl.RemoveBuilding((Building)BUILDING.ALTAR_OF_BATTLE);
                        ts.UpdateAll();
                    }
                }
                else
                {
                    tl.RemoveBuildingSpecial((Building)BUILDING.ALTAR_OF_BATTLE);
                }
            }
        }
        static public void EREM_TerrorWizard(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            var battle = Battle.GetBattle();
            if (battle == null)
            {
                Debug.LogError("EREM_TerrorWizard battle is null");
                return;
            }

            var enchInstance = target.GetEnchantmentManager().GetEnchantments().Find(o => o.source == (Enchantment)ENCH.TERROR_WIZZARD);
            if (enchInstance == null)
            {
                Debug.LogError("EREM_TerrorWizard enchInstance is null");
                return;
            }
            
            Enchantment unitsEnch = (Enchantment)ENCH.TERROR_UNIT;

            //Add Negative Enchantment On Unit
            if (battle.attacker == target)
            {
                foreach (var u in battle.attackerUnits)
                {
                    u.RemoveEnchantment(unitsEnch);
                }
            }
            else
            {
                foreach (var u in battle.defenderUnits)
                {
                    u.RemoveEnchantment(unitsEnch);
                }
            }

            foreach (var v in battle.buToSource)
            {
                v.Key.GetAttributes().SetDirty();
            }
        }
		static public void EREM_WindWalkingOwner(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            MOM.Unit owner = target as MOM.Unit;
            if (owner == null) return;
            foreach (var script in e.scripts)
            {
                if (script.triggerType == EEnchantmentType.GroupChange)
                {
                    var enchToRemove = (Enchantment)DataBase.Get(script.stringData, false);
                    if (enchToRemove == null)
                        Debug.LogError("SLEAVE_WindWalking StringData is not a ench.");
                    else
                    {
                        bool otherBonusSource = false;
                        var group = owner.group?.Get();
                        if (group != null && group.GetUnits().Count > 0)
                        {
                            //try find in the group other source of Wind Walk, skill, or enchantment
                            otherBonusSource = group.GetUnits().Find(o => o.Get().GetSkills().Contains((Skill)SKILL.WIND_WALKING) ||
                                                                          o.Get().GetEnchantments().Find(en => en.source == (Enchantment)ENCH.WIND_WALKING_OWNER) != null) != null;

                            if (!otherBonusSource)
                            {
                                foreach (var u in group.GetUnits())
                                {
                                    if (u.Get().GetEnchantments().Find(o => o.source == enchToRemove) != null)
                                    {
                                        u.Get().RemoveEnchantment(enchToRemove);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            EREM_UnitUpdateMove(target, e, ei);
        }
        static public void EREM_RemoveTimeStop(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            var wiz = ei.owner.Get<PlayerWizard>();
            var gm = GameManager.Get();
            if (wiz != null && gm != null)
            {
                gm.timeStopMaster =null;
                if (TurnManager.Get().playerTurn) HUD.Get()?.UpdateHUD();
            }
        }
        static public void EREM_MassShield(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            var battle = Battle.GetBattle();
            if (battle == null)
            {
                Debug.LogError("EREM_MassShield battle is null");
                return;
            }

            int casterID = GetSpellCasterOwnerID(ei);
            List<BattleUnit> ownerUnits;

            if (battle.attacker.GetID() == casterID)
            {
                ownerUnits = battle.attackerUnits;
            }
            else
            {
                ownerUnits = battle.defenderUnits;
            }

            foreach (var u in ownerUnits)
            {
                var shieldSkill = Array.Find(u.dbSource.Get().skills, o => o == (Skill)SKILL.LARGE_SHIELD);
                if (shieldSkill == null &&
                    u.GetSkills().Contains((Skill)SKILL.LARGE_SHIELD))
                {
                    u.GetSkills().Remove((Skill)SKILL.LARGE_SHIELD);
                    u.GetAttributes().SetDirty();
                }
            }
        }
        static public void EREM_MassPiercing(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            var battle = Battle.GetBattle();
            if (battle == null)
            {
                Debug.LogError("EREM_MassShield battle is null");
                return;
            }

            int casterID = GetSpellCasterOwnerID(ei);
            List<BattleUnit> ownerUnits;

            if (battle.attacker.GetID() == casterID)
            {
                ownerUnits = battle.attackerUnits;
            }
            else
            {
                ownerUnits = battle.defenderUnits;
            }

            foreach (var u in ownerUnits)
            {
                var shieldSkill = Array.Find(u.dbSource.Get().skills, o => o == (Skill)SKILL.ARMOR_PIERCING);
                if (shieldSkill == null &&
                    u.GetSkills().Contains((Skill)SKILL.ARMOR_PIERCING))
                {
                    u.GetSkills().Remove((Skill)SKILL.ARMOR_PIERCING);
                    u.GetAttributes().SetDirty();
                }
            }
        }
        static public void EREM_CastingBlock(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            var battle = Battle.GetBattle();
            if (battle == null)
            {
                Debug.LogError("EAPP_CastingBlock battle is null");
                return;
            }

            if (target is BattlePlayer)
            {
                var bp = target as BattlePlayer;
                if(bp.GetEnchantments().Find(o => o.source == ENCH.CASTING_BLOCK) == null)
                {
                    bp.castingBlock = false;
                }
            }
        }
        static public void EREM_HeroTraining(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            var pw = target as PlayerWizard;

            if (pw != null)
            {
                var groups = GameManager.GetGroupsOfWizard(pw.GetID());
                foreach (var group in groups)
                {
                    foreach (var unit in group.GetUnits())
                    {
                        if(unit.Get().IsHero())
                        {

                            if (unit.Get().GetEnchantments().Find(o => o.source == (Enchantment)ENCH.HEROISM) == null)
                            {
                                unit.Get().levelOverride = 0;
                                unit.Get().GetAttributes().SetDirty();
                            }
                        }
                    }
                }
            }
        }
        static public void EREM_PowerPeople(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            if (!(target is GameManager)) return;

            List<MOM.Location> wizardsLocations = GameManager.Get().registeredLocations;
            var enchTown = (Enchantment)DataBase.Get("ENCH-POWER_PEOPLE_CITY", false);

            foreach (var l in wizardsLocations)
            {
                if (!(l is TownLocation) || l.GetOwnerID() == 0) continue;
                l.RemoveEnchantment(enchTown);
            }
        }
        #endregion

        #region EnchantmentIntScript        
        // This Group covers scripts which are used to recalculate integer values
        // for example money, population, food, mana...


        /// <summary>
        /// 
        /// </summary>
        /// <param name="source">owner of the enchantment</param>
        /// <param name="e">enchantment activating</param>
        /// <param name="value">input value</param>
        /// <returns>processed value</returns>
        [ScriptType(ScriptType.Type.EnchantmentActivatorIntScript)]
        static public int ECHIAddResource(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, int value)
        {
            int modifier = EditorScripts.UTIL_GetStringParameterValue(ei.parameters);

            return value + modifier;
        }

        [ScriptType(ScriptType.Type.EnchantmentActivatorIntScript)]
        static public int ECHITakeResource(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, int value)
        {
            int modifier = EditorScripts.UTIL_GetStringParameterValue(ei.parameters);

            return value - modifier;
        }

        [ScriptType(ScriptType.Type.EnchantmentActivatorIntScript)]
        static public int ECHIAddValue(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, int value)
        {
            return value + e.fIntData.ToInt();
        }

        [ScriptType(ScriptType.Type.EnchantmentActivatorIntScript)]
        static public int ECHITakeValue(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, int value)
        {
            return value - e.fIntData.ToInt();
        }

        [ScriptType(ScriptType.Type.EnchantmentActivatorIntScript)]
        static public int ECHIValueMP(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, int value)
        {
            FInt temp = value * e.fIntData;
            return temp.ToInt();
        }
        static public int ECHI_AddPercentValue(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, int value)
        {
            FInt temp = value + (value * e.fIntData / 100);
            return temp.ReturnRoundedCeil().ToInt();
        }

        [ScriptType(ScriptType.Type.EnchantmentActivatorIntScript)]
        static public int ECHIAnimistsGuildFood(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, int value)
        {
            var town = target as TownLocation;
            var farmers = town.GetFarmers();
            return value += farmers;
        }

        [ScriptType(ScriptType.Type.EnchantmentActivatorIntScript)]
        [ScriptParameters(typeof(FInt))]
        static public int ECHIManaDrain(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, int value)
        {
            if (ei == null)
            {
                Debug.LogError("ECHIManaDrain ei is null");
                return value;
            }
            float multiplier = EditorScripts.UTIL_GetRandomFromStringParameter(ei.parameters);
            multiplier /= 100f;
            value = Mathf.RoundToInt(value - (value * multiplier));

            return value;
        }

        [ScriptType(ScriptType.Type.EnchantmentActivatorIntScript)]
        [ScriptParameters(typeof(FInt))]
        static public object ECHITakePopulationPercentage(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, int value)
        {
            var tl = target as TownLocation;
            if (tl == null)
            {
                Debug.LogError("Enchantment designed to work for towns!");
                return value;
            }
            float multiplier = EditorScripts.UTIL_GetRandomFromStringParameter(ei.parameters);
            multiplier /= 100f;
            value = Mathf.RoundToInt(value - (value * multiplier));

            return value;
        }

        [ScriptType(ScriptType.Type.EnchantmentActivatorIntScript)]
        [ScriptParameters(typeof(FInt))]
        static public object ECHIAddPopulationPercentage(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, int value)
        {
            var tl = target as TownLocation;
            if (tl == null)
            {
                Debug.LogError("Enchantment designed to work for towns!");
                return value;
            }
            float multiplier = EditorScripts.UTIL_GetRandomFromStringParameter(ei.parameters);
            multiplier /= 100f;
            value = Mathf.RoundToInt(value + ( value * multiplier));

            return value;
        }
        
        [ScriptType(ScriptType.Type.EnchantmentActivatorIntScript)]
        static public int ECHIAddProductionFromForestMP(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, int value)
        {
            int cityRange = 2;
            WorldCode.Hex hex;
            var tl = target as IPlanePosition;

            FInt forestNum = FInt.ZERO;

            WorldCode.Plane plane = tl.GetPlane();
            var positions = tl.GetSurroundingArea(cityRange);

            foreach (var pos in positions)
            {
                hex = plane.GetHexAt(pos);
                if (hex != null && hex.GetTerrain().terrainType == ETerrainType.Forest) 
                    forestNum += FInt.ONE;
            }

            forestNum = (value * forestNum * 0.03f).ReturnRounded();

            return value + forestNum.ToInt();
        }

        [ScriptType(ScriptType.Type.EnchantmentActivatorIntScript)]
        static public int ECHI_SetVisibilityRange(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, int value)
        {
            if (value > e.fIntData)
                return value;

            return e.fIntData.ToInt();
        }
        static public int ECHI_WarpNode(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, int value)
        {
            var p = target as MOM.Location;
            if (p == null || p.locationType != ELocationType.Node)
            {
                Debug.LogError("Enchantment designed to work on Nodes!");
                return value;
            }

            int warpStr = p.power ;
            if (e != null)
            {
                warpStr += e.fIntData.ToInt();
            }

            value -= warpStr;

            return value;
        }
        static public int ECHI_CancleReligiousUnrestMod(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, int value)
        {
            var tl = target as TownLocation;
            int unrestMod = 0;

            if (tl == null)
                Debug.LogError("Enchantment designed to work on town!");

            foreach (var b in tl.buildings)
            {
                if (b.Get().enchantments == null) continue;
                if (b.Get().tags == null || Array.FindAll(b.Get().tags, o => o == (Tag)TAG.RELIGIOUS).Length == 0) continue;
                foreach (var ench in b.Get().enchantments)
                {
                    if (ench.scripts == null) continue;
                    foreach (var script in ench.scripts)
                    {
                        if (script.triggerType == EEnchantmentType.RebelsModifier)
                            unrestMod += script.fIntData.ToInt();
                    }
                }
            }

            return value + unrestMod;
        }
        static public int ECHI_AfinityForMinerals(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, int value)
        {
            int growBonus = e.fIntData.ToInt();
            int mineralsCount = 0;
            if (target is TownLocation)
            {
                mineralsCount = (target as TownLocation).GetResources().FindAll(o => o.mineral).Count;
                value += mineralsCount * growBonus;
            }

            return value;
        }
        [ScriptType(ScriptType.Type.EnchantmentActivatorIntScript)]
        static public int ECHI_GreateWastingUnrest(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, int value)
        {
            var town = target as TownLocation;
            if (town == null)
            {
                Debug.LogError("ECHI_GreateWastingUnrest enchantment designed to work from town!");
            }

            int data = 0;
            if (e.fIntData == null)
            {
                Debug.LogError("ECHI_GreateWastingUnrest enchantment scrip fIntData == null");
            }
            else
            {
                data = e.fIntData.ToInt();
            }

            foreach (var building in town.buildings)
            {
                if (building == (Building)BUILDING.SHRINE ||
                    building == (Building)BUILDING.TEMPLE ||
                    building == (Building)BUILDING.PARTHENON ||
                    building == (Building)BUILDING.CATHEDRAL)
                {
                    data += 1;
                }
            }

            return value + data;
        }

        #endregion

        #region EnchantmentFIntScript   
        [ScriptType(ScriptType.Type.EnchantmentActivatorFIntScript)]
        static public FInt ECHFFortressPower(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, FInt value)
        {
            int spellBookCount = 0;

            var town = target as TownLocation;
            var plane = town.GetPlane();
            var wizard = town.GetWizardOwner();

            spellBookCount += wizard.GetAttributes().GetFinal(DBEnum.TAG.MAGIC_BOOK).ToInt();

            if (plane.planeSource == (DBDef.Plane)DBEnum.PLANE.MYRROR)
                spellBookCount += 5;

            return value + spellBookCount;
        }

        [ScriptType(ScriptType.Type.EnchantmentActivatorFIntScript)]
        static public FInt ECHFAddPowerReligious(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, FInt value)
        {
            var tl = target as TownLocation;
            FInt fIncome = e.fIntData;

            if (tl == null)
            {
                Debug.LogError("Enchantment designed to work from town!");
                return value + fIncome;
            }

            var w = tl.GetWizardOwner();
            if (w == null)
            {
                Debug.LogError("Enchantment designed to work for wizards!");
                return value + fIncome;
            }
            
            return value + fIncome;
        }

        [ScriptType(ScriptType.Type.EnchantmentActivatorFIntScript)]
        static public FInt ECHFAddValue(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, FInt value)
        {
            return value + e.fIntData;
        }

        [ScriptType(ScriptType.Type.EnchantmentActivatorFIntScript)]
        static public FInt ECHFValueMP(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, FInt value)
        {
            if (target is TownLocation)
            {
                var t = target as TownLocation;
                foreach (var b in t.buildings)
                {
                    if (b.Get() == (Building)BUILDING.SHRINE)
                    {
                        value -= FInt.ONE;
                    }
                    if (b.Get() == (Building)BUILDING.TEMPLE)
                    {
                        value -= (FInt)2.0;
                    }
                    if (b.Get() == (Building)BUILDING.PARTHENON)
                    {
                        value -= (FInt)3.0;
                    }
                    if (b.Get() == (Building)BUILDING.CATHEDRAL)
                    {
                        value -= (FInt)4.0;
                    }
                }
            }
            return value ;
        }
        [ScriptType(ScriptType.Type.EnchantmentActivatorFIntScript)]
        static public FInt ECHFAdditionalNodePower(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, FInt value)
        {
            var town = target as TownLocation;
            var tLoc = town.GetTownLocations();
            FInt nodesAddPower = FInt.ZERO;
            if (e.fIntData != null && e.fIntData > FInt.ZERO)
            {
                var nodesCount = tLoc.FindAll(o => o.locationType == ELocationType.Node).Count;
                // each node above first add additional point.
                nodesAddPower = nodesCount * e.fIntData + nodesCount - 1;

            }

            value = value + nodesAddPower;

            return value;
        }
        static public FInt ECHFAddBonusFromPoP(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, FInt value)
        {
            if (target is TownLocation)
            {
                var town = target as TownLocation;
                var pop = town.GetPopUnits();
                value += e.fIntData * pop;
            }

            return value;
        }


        #endregion
        #region EnchantmentFloatScript
        [ScriptType(ScriptType.Type.EnchantmentActivatorFloatScript)]
        static public float ECHFLTakeUnrest(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, float value)
        {
            return value - e.fIntData.ToFloat();
        }

        [ScriptType(ScriptType.Type.EnchantmentActivatorFloatScript)]
        static public float ECHFLAddUnrest(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, float value)
        {
            return value + e.fIntData.ToFloat();
        }
        [ScriptType(ScriptType.Type.EnchantmentActivatorFloatScript)]
        static public float ECHFLTakeUnrestPercentage(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, float value)
        {
            if(ei == null)
            {
                Debug.LogError("ECHFLTakeUnrestPercentage ei is null.");
                return value;
            }

            var multiplier = EditorScripts.UTIL_StringParameterProcessor(ei.parameters).t1;
            multiplier /= 100f;
            value = value - (value * multiplier);

            return value;
        }

        [ScriptType(ScriptType.Type.EnchantmentActivatorFloatScript)]
        static public float ECHFLAddUnrestPercenntage(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, float value)
        {
            if (ei == null)
                Debug.LogError("ECHFLAddUnrestPercenntage ei is null.");

            var multiplier = EditorScripts.UTIL_StringParameterProcessor(ei.parameters).t1;
            multiplier /= 100f;
            value = value + (value * multiplier);

            return value;
        }
        [ScriptType(ScriptType.Type.EnchantmentActivatorIntScript)]
        static public int ECHITakeRebels(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, int value)
        {
            if (ei == null)
            {
                Debug.LogError("ECHFLTakeRebels ei is null.");
                return value;
            }

            var modifier = EditorScripts.UTIL_GetStringParameterValue(ei.parameters);
            value -= modifier;

            return value;
        }

        [ScriptType(ScriptType.Type.EnchantmentActivatorIntScript)]
        static public int ECHIAddRebels(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, int value)
        {
            if (ei == null)
                Debug.LogError("ECHFLAddRebels ei is null.");

            var modifier = EditorScripts.UTIL_GetStringParameterValue(ei.parameters);
            value += modifier;

            return value;
        }

        [ScriptType(ScriptType.Type.EnchantmentActivatorFloatScript)]
        static public float ECHFLValueMP(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, float value)
        {
            var data = e.fIntData;
            return value * data.ToFloat();
        }


        [ScriptType(ScriptType.Type.EnchantmentActivatorFloatScript)]
        static public float ECHFL_FamineUnrest(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, float value)
        {
            var town = target as TownLocation;
            if (town == null)
                Debug.LogError("ECHFL_FamineUnrest enchantment designed to work from town!");

            FInt data = FInt.ZERO;
            if (e.fIntData == null)
            {
                Debug.LogError("ECHFL_FamineUnrest enchantment scrip fIntData == null");
            }
            else
            {
                data = e.fIntData;
            }

            var unrest = data;

            return value + unrest.ToFloat();
        }
        #endregion
        #region Enchantment Activators (TODO detail)

        static public void ECH_Empty(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            //That script do not do nothing else then allow tu use trigger that show ench in battle
        }

        [ScriptType(ScriptType.Type.EnchantmentActivatorScript)]        
        static public void ECHHeal(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            var healPercent = e.fIntData;

            MOM.Group g = null;
            if (target is TownLocation)
            {
                var t = target as TownLocation;
                g = t.localGroup;
                
            }
            else if(target is MOM.Group)
            {
                g = target as MOM.Group;
            }

            if (g == null || g.GetUnits() == null) return;
            foreach (var v in g.GetUnits())
            {
                //heal quarter of the unit hp/figures
                v.Get().Heal(healPercent.ToFloat());
            }

        }

        [ScriptType(ScriptType.Type.EnchantmentActivatorScript)]
        static public void ECHNightshadeUse(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            var town = target as TownLocation;
            foreach (var res in town.GetResources())
            {
                if (res == (DBDef.Resource)DBEnum.RESOURCE.NIGHTSHADE)
                {
                    ScriptLibrary.Call("SWG_DisenchantAreaNightshade", GameManager.GetWizard(town.owner), town.GetPosition(), town.GetPlane(), null);
                }
            }

        }

        [ScriptType(ScriptType.Type.EnchantmentActivatorScript)]
        static public void ECHEnchantedWeaponAdd(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            if (!(target is TownLocation)) return;

            var town = target as TownLocation;
            if (town.GetResources().Contains((Resource)RESOURCE.ADAMANTINE_ORE))
            {
                if (data is MOM.Unit)
                {
                    var unit = data as MOM.Unit;
                    unit.AddSkill((Skill)SKILL.ENCHANTED_WEAPON4);
                    return;
                }
            }
            else if (town.GetResources().Contains((Resource)RESOURCE.MITHRIL_ORE))
            {
                if (data is MOM.Unit)
                {
                    var unit = data as MOM.Unit;
                    unit.AddSkill((Skill)SKILL.ENCHANTED_WEAPON3);
                    return;
                }
            }
            else
            {
                if (data is MOM.Unit)
                {
                    var unit = data as MOM.Unit;
                    unit.AddSkill((Skill)SKILL.ENCHANTED_WEAPON2);
                    return;
                }
            }
        }

        [ScriptType(ScriptType.Type.EnchantmentActivatorScript)]
        static public void ECH_PestilencePopulationRemover(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            var random = new MHRandom();
            var town = target as TownLocation;

            if(town.GetPopUnits() > 1)
            {
                if (random.GetFloat(1, 10) < town.GetPopUnits())
                    town.Population -= 1000;
            }
        }
        [ScriptType(ScriptType.Type.EnchantmentActivatorScript)]
        static public void ECH_ChaosRift(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            var random = new MHRandom();
            var town = target as TownLocation;
            int numberOfAttacks = 5;

            for (int i = town.buildings.Count - 1; i >= 0; i--)
            {
                if(random.GetFloat(0, 1) < 0.05f)
                    town.RemoveBuildingSpecial(town.buildings[i]);
            }

            int[] dmg = new int[1] { e.fIntData.ToInt() };
            var group = town.GetLocalGroup();
            var units = group.GetUnits();
            if(group != null && units.Count > 0)
            {
                dmg[0] = random.GetSuccesses(0.3f, dmg[0]);
                MOM.Unit unit;
                for (int i = 0; i < numberOfAttacks; i++)
                {
                    unit = units[random.GetInt(0, units.Count)];

                    if (unit.attributes.Contains(TAG.FIRE_IMMUNITY) ||
                        unit.attributes.Contains(TAG.MAGIC_IMMUNITY) ||
                        unit.attributes.Contains(TAG.RIGHTEOUSNESS))
                    {
                        unit.ApplyDamage(dmg, random, null, 50, true, null, null, e);
                    }
                    else if (unit.attributes.Contains(TAG.ELEMENTAL_ARMOR))
                    {
                        unit.ApplyDamage(dmg, random, null, 10, false, null, null, e);
                    }
                    else if (unit.attributes.Contains(TAG.RESIST_ELEMENTS) ||
                             unit.attributes.Contains(TAG.BLESS))
                    {
                        unit.ApplyDamage(dmg, random, null, 3, false, null, null, e);
                    }
                    else if (unit.attributes.Contains(TAG.LARGE_SHIELD))
                    {
                        unit.ApplyDamage(dmg, random, null, 2, false, null, null, e);
                    }
                    else
                    {
                        unit.ApplyDamage(dmg, random, null, 0, false, null, null, e);
                    }
                    group.UpdateGroupUnits();

                    if (units.Count <= 0) return;
                }
            }
        }

        static public void ECH_SetStartExp(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            if (data is MOM.Unit)
            {
                var unit = data as MOM.Unit;
                unit.xp = e.fIntData.ToInt();
            }
        }
        static public void ECH_DoomMasteryUnit(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            if (data is MOM.Unit)
            {
                var unit = data as MOM.Unit;
                var spell = (Spell)DataBase.Get("SPELL-CHAOS_CHANNELS", false);
                ScriptLibrary.Call("SWG_ChaosChannels", null, unit, spell);
            }
        }
        static public void ECH_TerrorWizard(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            var random = new MHRandom();
            var b = Battle.GetBattle();
            List<BattleUnit> targetUnits;
            var ench = DataBase.Get<DBDef.Enchantment>(ENCH.TERROR_UNIT, false);
            var spell = DataBase.Get<DBDef.Spell>(SPELL.TERROR, false);

            if (target is BattlePlayer)
            {
                var bp = target as BattlePlayer;
                MOM.PlayerWizard spellOwner;

                if (b.attacker.wizard != bp.GetWizardOwner())
                {
                    targetUnits = b.defenderUnits.FindAll(o => o.IsAlive());
                    spellOwner = b.attacker.GetWizardOwner();
                }
                else
                {
                    targetUnits = b.attackerUnits.FindAll(o => o.IsAlive());
                    spellOwner = b.defender.GetWizardOwner();
                }

                foreach (var u in targetUnits)
                {
                    if (u.GetAttFinal(TAG.DEATH_IMMUNITY) > 0) continue;

                    var resistRollFailed = random.GetInt(0, 11) >= 1 + u.attributes.GetFinal(TAG.RESIST) + ResitModFromEnch(null, u, spell);
                    if (resistRollFailed && u.GetEnchantments().Find(o => o.source == ench) == null)
                    {                        
                        u.AddEnchantment(ench, spellOwner as IEnchantable, 1, null, spell.battleCost);
                        if (!u.simulated && b != null)
                        {
                            var effect = ((Spell)SPELL.TERROR).castEffect;
                            FSMBattleTurn.instance?.CastEffect(u.GetPosition(), effect);
                            AudioLibrary.RequestSFX("SpellTerror");
                        }
                    }
                }
            }
            else return;

        }
        static public void ECH_CallLightning(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            var random = new MHRandom();
            int numberOfAttacks = random.GetInt(3, 6);
            int[] dmg = new int[1] { e.fIntData.ToInt() };
            var b = Battle.GetBattle();
            List<BattleUnit> enemyUnits = new List<BattleUnit>();

            if (target is BattlePlayer)
            {
                var bp = target as BattlePlayer;

                if (b.attacker.wizard != bp.GetWizardOwner())
                {
                    enemyUnits = b.defenderUnits.FindAll( o => o.IsAlive());
                }
                else
                {
                    enemyUnits = b.attackerUnits.FindAll(o => o.IsAlive());
                }
            }


            if (enemyUnits.Count > 0)
            {
                BattleUnit unit;
                var effect = ((Spell)SPELL.CALL_LIGHTNING).castEffect;
                AudioLibrary.RequestSFX("SpellCallLightning");

                for (int i = 0; i < numberOfAttacks; i++)
                {
                    unit = enemyUnits[random.GetInt(0, enemyUnits.Count)];
                    if (!unit.IsAlive())
                    {
                        unit = enemyUnits.Find(o => o.IsAlive());
                        if (unit == null || !unit.IsAlive()) break;
                    }
                    if (!unit.simulated && b != null)
                    {
                        FSMBattleTurn.instance?.CastEffect(unit.GetPosition(), effect);
                    }
					
                    dmg[0] = random.GetSuccesses(0.3f, dmg[0]);
                    if (unit.attributes.Contains(TAG.LIGHTNING_WEAKNESS))
                    {
                        dmg[0] = dmg[0] * 2;
                    }

                    if (unit.attributes.Contains(TAG.MAGIC_IMMUNITY))
                    {
                        unit.ApplyDamage(dmg, random, null, 50, true, null, null, e);                     
                    }
                    else if (unit.attributes.Contains(TAG.ELEMENTAL_ARMOR))
                    {
                        unit.ApplyDamage(dmg, random, null, 10, false, null, null, e);                     
                    }
                    else if (unit.attributes.Contains(TAG.RESIST_ELEMENTS))
                    {
                        unit.ApplyDamage(dmg, random, null, 3, false, null, null, e);                        
                    }
                    else if (unit.attributes.Contains(TAG.LARGE_SHIELD))
                    {
                        unit.ApplyDamage(dmg, random, null, 2, false, null, null, e);                        
                    }
                    else
                    {
                        unit.ApplyDamage(dmg, random, null, 0, false, null, null, e);                        
                    }

                    if (enemyUnits.Count <= 0) return;
                }
            }
        }
        static public void ECH_CallFortressBolt(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            var random = new MHRandom();
            PlayerWizard wizard = target as PlayerWizard;
            if(wizard == null)
            {
                if (target is Battle)
                {
                    wizard = GameManager.GetWizard((target as Battle).defender.GetID());
                }
                else if(target is BattlePlayer)
                {
                    wizard = GameManager.GetWizard((target as BattlePlayer).GetID());
                }
            }
            
            TAG wizardMagic; 
            realmToTag.TryGetValue(ei.source.Get().realm, out wizardMagic);
            int[] dmg = new int[1] { wizard.GetAttFinal(wizardMagic).ToInt() };

            BattleUnit unit = data as BattleUnit;
            if (unit == null || !unit.IsAlive()) return;
            
            var unitAtt = unit.attributes;

            if (unitAtt.Contains(TAG.MAGIC_IMMUNITY) ||
                (unitAtt.Contains(TAG.RIGHTEOUSNESS) && (wizardMagic == TAG.CHAOS_MAGIC_BOOK
                || wizardMagic == TAG.DEATH_MAGIC_BOOK))
                )
            {
                unit.ApplyDamage(dmg, random, null, 50, true, null, null, e);
            }
            else if (unitAtt.Contains(TAG.ELEMENTAL_ARMOR) && (wizardMagic == TAG.CHAOS_MAGIC_BOOK
                || wizardMagic == TAG.NATURE_MAGIC_BOOK))
            {
                unit.ApplyDamage(dmg, random, null, 10, false, null, null, e);
            }
            else if ((unitAtt.Contains(TAG.RESIST_ELEMENTS) && (wizardMagic == TAG.CHAOS_MAGIC_BOOK
                || wizardMagic == TAG.NATURE_MAGIC_BOOK)) ||
                        (unitAtt.Contains(TAG.BLESS) && (wizardMagic == TAG.CHAOS_MAGIC_BOOK
                || wizardMagic == TAG.DEATH_MAGIC_BOOK)))
            {
                unit.ApplyDamage(dmg, random, null, 3, false, null, null, e);
            }
            else if (unit.attributes.Contains(TAG.LARGE_SHIELD))
            {
                unit.ApplyDamage(dmg, random, null, 2, false, null, null, e);
            }
            else
            {
                unit.ApplyDamage(dmg, random, null, 0, false, null, null, e);
            }
            
        }
        static public void ECH_ManaLeak(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            var b = Battle.GetBattle();
            List<BattleUnit> enemyUnits;
            var manaLost = e.fIntData.ToInt();


            if (target is BattlePlayer)
            {
                var bp = target as BattlePlayer; 
                if (bp.mana > 0)
                {
                    if (bp.mana > manaLost)
                        bp.mana = bp.mana - manaLost;
                    else
                        bp.mana = 0;
                }

                MOM.PlayerWizard spellOwner;

                if (b.attacker.wizard != bp.GetWizardOwner())
                {
                    enemyUnits = b.defenderUnits.FindAll(o => o.IsAlive());
                    spellOwner = b.defenderUnits[0].GetWizardOwner();
                }
                else
                {
                    enemyUnits = b.attackerUnits.FindAll(o => o.IsAlive());
                    spellOwner = b.attackerUnits[0].GetWizardOwner();
                }

                foreach (var u in enemyUnits)
                {
                    if (u.mana > 0)
                    {
                        if (u.mana > manaLost)
                            u.mana = u.mana - manaLost;
                        else
                            u.mana = 0;

                        if (!u.simulated && b != null)
                        {
                            var effect = ((Spell)SPELL.MANA_LEAK).castEffect;
                            FSMBattleTurn.instance?.CastEffect(u.GetPosition(), effect);
                            AudioLibrary.RequestSFX("SpellManaLeak");
                        }
                    }

                    if (u.GetAttributes().Contains(TAG.MAGIC_RANGE) && 
                        u.GetCurentFigure().rangedAmmo > 0 )
                    {
                        u.GetCurentFigure().rangedAmmo--;
                        if (!u.simulated && b != null)
                        {
                            var effect = ((Spell)SPELL.MANA_LEAK).castEffect;
                            FSMBattleTurn.instance?.CastEffect(u.GetPosition(), effect);
                            AudioLibrary.RequestSFX("SpellManaLeak");
                        }
                    }


                }
            }
            else return;

        }
        static public void ECH_Wrack(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            var random = new MHRandom();
            var b = Battle.GetBattle();
            List<BattleUnit> enemyUnits;
            var resistMod = e.fIntData.ToInt();
            var spell = DataBase.Get<DBDef.Spell>(SPELL.WRACK, false);


            if (target is BattlePlayer)
            {
                var bp = target as BattlePlayer;

                if (b.attacker.wizard != bp.GetWizardOwner())
                {
                    enemyUnits = b.defenderUnits.FindAll(o => o.IsAlive());
                }
                else
                {
                    enemyUnits = b.attackerUnits.FindAll(o => o.IsAlive());
                }


                foreach (var u in enemyUnits)
                {
                    var unitResist = u.attributes.GetFinal(TAG.RESIST) + ResitModFromEnch(null, u, spell) + resistMod;
                    int[] dmg = new int[1];
                    dmg[0] = random.GetSuccesses(((10 - unitResist).ToFloat() * 0.1f), u.FigureCount());

                    if (dmg[0] > 0 )
                        
                    {
                        u.ApplyDamage(dmg, random, null, 0, true);
                        if (!u.simulated && b!=null)
                        {
                            var effect = ((Spell)SPELL.WRACK).castEffect;
                            FSMBattleTurn.instance?.CastEffect(u.GetPosition(), effect);
                            AudioLibrary.RequestSFX("SpellWrack");
                        }
                    }
                }
            }
            else return;

        }
        static public void ECH_DoomMasteryWizard(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            if (!(target is MOM.PlayerWizard)) return;

            var wizard = target as MOM.PlayerWizard;
            var wizardTowns = GameManager.Get().registeredLocations.FindAll(o => o.GetOwnerID() == wizard.ID);

            var ench = (Enchantment)DataBase.Get("ENCH-DOOM_MASTERY_CITY", false);

            foreach (var t in wizardTowns)
            {
                if (!(t is TownLocation) ||
                    (t as TownLocation).GetEnchantments().Find(o => o.source == (Enchantment)ENCH.DOOM_MASTERY_CITY) != null) continue;
                t.AddEnchantment(ench, wizard, ench.lifeTime, null, 0);
            }

        }
        static public void ECH_MeteorStormMainCity(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            if (!(data is MOM.TownLocation))
            {
                Debug.LogWarning("ECH_MeteorStormMainCity is not targeting TownLocation");
                return;
            }

            var town = data as MOM.TownLocation;

            var ench = (Enchantment)DataBase.Get("ENCH-METEOR_STORM_CITY", false);

            if (ei.owner.ID != town.GetOwnerID() && town.GetWizardOwner().ID != 0)
            {
                if (IsTownProtected(town.GetWizardOwner(), ei.source, town)) return; 
                town.AddEnchantment(ench, ei.owner as IEnchantable, ench.lifeTime, null, 0);
            }
        }


        static public void ECH_MeteorStormCity(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            if (!(target is TownLocation))
            {
                Debug.LogWarning("ECH_MeteorStormCity is not targeting TownLocation");
                return;
            }

            MHRandom random = new MHRandom();
            var town = target as TownLocation;

            if (town.IsAnOutpost())
            {
                town.Destroy();
                return;
            }

            for (int i = town.buildings.Count - 1; i >= 0; i--)
            {
                if (random.GetFloat(0f, 1f) <= 0.01f)
                    town.RemoveBuildingSpecial(town.buildings[i]);
            }
        }
        static public void ECH_NaturesWrathEnchCity(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            if (!(data is MOM.TownLocation))
            {
                Debug.LogWarning("ECH_NaturesWrathEnchCity is not targeting TownLocation");
                return;
            }

            var town = data as MOM.TownLocation;

            var ench = (Enchantment)DataBase.Get("ENCH-NATURES_WRATH_CITY", false);
            var townOwnerAtt = town.GetWizardOwner().GetAttributes();

            if (ei.owner.ID != town.GetWizardOwner().ID && town.GetWizardOwner().ID != 0) 
            { 
                if(townOwnerAtt.GetFinal(TAG.DEATH_MAGIC_BOOK) > 0 || townOwnerAtt.GetFinal(TAG.CHAOS_MAGIC_BOOK) > 0)
                {
                    if (IsTownProtected(town.GetWizardOwner(), ei.source, town)) return;
                    town.AddEnchantment(ench, ei.owner as IEnchantable, ench.lifeTime, null, 0);
                }
            }
        }

        static public void ECH_NatureWrathCity(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            if (!(target is TownLocation))
            {
                Debug.LogWarning("ECH_NatureWrathCity is not targeting TownLocation");
                return;
            }

            MHRandom random = new MHRandom();
            var town = target as TownLocation;

            for (int i = town.buildings.Count - 1; i >= 0; i--)
            {
                var dep = town.CanRemoveBuilding(town.buildings[i]);
                if (dep == null && random.GetFloat(0f, 1f) <= 0.05f)
                {
                    town.RemoveBuildingSpecial(town.buildings[i]);
                }
            }
            for (int i = town.GetUnits().Count - 1; i >= 0; i--)
            {
                if (town.GetUnits()[i].Get().GetAttFinal(TAG.CAN_FLY) > 0 ||
                    town.GetUnits()[i].Get().GetAttFinal(TAG.NON_CORPOREAL) > 0) continue;
                if (random.GetFloat(0f, 1f) <= 0.15f)
                {
                    town.GetUnits()[i].Get().Destroy();
                    //town.RemoveUnit(town.GetUnits()[i]);
                }
            }
        }

        static public void ECH_CorruptionCleanAroundTown (IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            var town = target as TownLocation;
            FInt cleaningChance = e.fIntData;
            if (town == null)
            {
                Debug.LogWarning("ECH_CorruptionCleanAroundTown town do not exist");
                return;
            }

            foreach (var h in town.GetSurroundingArea(town.GetTownRange()))
            {
                if (cleaningChance == FInt.ZERO)
                {
                    cleaningChance = FInt.ONE;
                }
                var random = new MHRandom();
                if (random.GetSuccesses(cleaningChance.ToFloat(), 1) > 0)
                {
                    WorldCode.Plane.Get().GetHexAt(h).ActiveHex = true;
                }
            }
        }

        static public void ECH_MagicCounterBattle(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            if (!(target is Battle)) return;

            var battle = target as Battle;

            if (battle == null) return;

            battle.battleCounterMagic++;
        }

        static public void ECH_GreatWastingCorruption(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            if (!(target is GameManager)) return;
            PlayerWizard owner = ei.owner.GetEntity() as PlayerWizard;

            MHRandom random = new MHRandom();
            if (random.GetFloat(0.0f, 1.0f) > 0.5f)
            {
                RandomHexesDisable(World.GetArcanus(), random, 3, 6, ei);
            }
            else
            {
                RandomHexesDisable(World.GetMyrror(), random, 3, 6, ei);
            }
        }

        static void RandomHexesDisable(WorldCode.Plane plane, MHRandom random, int minHexes, int maxHexes, EnchantmentInstance ei)
        {
            if (World.GetArcanus().GetLandHexes().Count == 0)
            {
                Debug.LogError("Hex count in " + plane.ToString() + " plane = 0");
                return;
            }

            int corruptedActual = 0;
            var hexesList = plane.GetLandHexes();

            int corruptionsNeeded = random.GetInt(minHexes, maxHexes + 1);
                
            int enchOwner = ei.owner != null ? ei.owner.ID : 0;                
            var list = new List<Vector3i>(hexesList);
            list.RandomSort();

            foreach (var h in list)
            {
                var hex = plane.GetHexAt(h);
                if (!hex.ActiveHex) continue;

                var loc = GameManager.Get().registeredLocations.Find(o => o.GetDistanceTo(h) <= 2 && o is TownLocation);
                var tl = loc as TownLocation;
                if (loc == null ||
                    tl.GetOwnerID() != enchOwner &&
                    !IsTownProtected(EntityManager.GetEntity(enchOwner) as PlayerWizard, ei.source, tl))
                {
                    corruptedActual++;
                    hex.ActiveHex = false;
                }

                if (corruptedActual >= corruptionsNeeded) break;
            }
        }
    
        static public void ECH_GreatWastingCitiesUnrest(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            if (!(data is MOM.TownLocation)) return;

            var town = data as MOM.TownLocation;

            var ench = (Enchantment)DataBase.Get("ENCH-GREAT_WASTING_CITY", false);

            if (ei.owner.ID != town.GetWizardOwner().ID && town.GetWizardOwner().ID != 0)
            {
                if (IsTownProtected(town.GetWizardOwner(), ei.source, town)) return;
                town.AddEnchantment(ench, ei.owner as IEnchantable, ench.lifeTime, null, 0);
            }
        }
        static public bool ECH_Confusion(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            var ba = target as BattleUnit;

            if (ba == null)
            {
                Debug.LogError("ECH_Confusion try to target non battle unit.");
            }

            if (!ba.IsAlive()) return true;

            if (ba.GetEnchantments().Find(o => o.source == (Enchantment)ENCH.CONFUSION_POSSESSION) != null) return true;
            
            var random = new MHRandom();
            var num = random.GetFloat(0, 1);
            ba.canAttack = true;
            ba.canCastSpells = true;
            ba.canMove = true;
            ba.UpdateUnitMP();
             
            if (num < 0.25)
            {
                ba.canAttack = false;
                ba.canCastSpells = false;
                ba.canMove = false;
                ba.Mp = FInt.ZERO;
                ba.UpdateUnitMP();
            }
            else if (num < 0.5)
            {
                var battle = Battle.Get();
                if(battle != null && ba.IsAlive())
                {
                    if(!ba.simulated) battle.confusedList.Add(ba);
                }
            }
            else if (num < 0.75)
            {
                //Enemy control unit
                ba.AddEnchantment((Enchantment)ENCH.CONFUSION_POSSESSION, ei.owner.GetEntity(), 1);
            }

            //num above 0.75 do nothing to the unit.

            return true;
        }
        [ScriptType(ScriptType.Type.EnchantmentActivatorScript)]
        static public int ECHIBabyBoom(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, int value)
        {
            var tl = target as TownLocation;
            if (tl == null)
            {
                Debug.LogError("Enchantment designed to work for towns!");
            }
            int bonus = 1000;
            float modifier = EditorScripts.UTIL_GetStringParameterValue(ei.parameters);
            modifier /= 100f;
            bonus = Mathf.RoundToInt(bonus * modifier);
            return value + bonus;
        }
        [ScriptType(ScriptType.Type.EnchantmentActivatorScript)]
        static public void ECHIPlague(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            var tl = target as TownLocation;
            if (tl == null)
            {
                Debug.LogError("Enchantment designed to work for towns!");
            }
            if (tl.IsAnOutpost()) return;
            int value = 1000;
            float modifier = EditorScripts.UTIL_GetStringParameterValue(ei.parameters);
            modifier /= 100f;
            value = Mathf.RoundToInt(value * modifier);
            tl.Population -= value; // Accessor will limit this to 1000
        }
        static public bool ECH_EternalNight(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            var battle = Battle.GetBattle();
            if (battle == null)
            {
                Debug.LogError("ECH_EternalNight battle is null");
                return false;
            }

            if (ei == null && ei.owner != null)
            {
                Debug.LogError("ECH_EternalNight ei is null or ei.owner is null");
                return false;
            }

            var wizardOwner = GameManager.GetWizard(ei.owner.ID);

            Enchantment ench = (Enchantment)DataBase.Get("ENCH-DARKNESS_BATTLE", false);
            if (ench != null)
            {
                battle.AddEnchantment(ench, wizardOwner, ench.lifeTime, null, 0);
            }

            return true;
        }
        static public void ECH_AuraOfMajesty(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {                
            if (ei == null && ei.owner == null)
            {
                Debug.LogError("ECH_AuraOfMajesty ei or ei.owner is null");
                return;
            }

            var spellCaster = GameManager.GetWizard(ei.owner.ID);
            if (spellCaster.discoveredWizards == null) return;

            foreach (var wizard in spellCaster.discoveredWizards)
            {
                var diplomacyStatus = spellCaster.GetDiplomacy().GetStatusToward(wizard.ID);
                if (diplomacyStatus == null)
                {
                    Debug.LogError("ECH_AuraOfMajesty diplomacyStatus is null");
                    return;
                }
                diplomacyStatus.ChangeRelationshipBy(1, true);
            }
        }
        static public void ECH_GaiasBlessingTransmute(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            if (!(target is TownLocation))
            {
                Debug.LogWarning("ECH_GaiasBlessingTransmute is not targeting TownLocation");
                return;
            }

            WorldCode.Plane p = World.GetActivePlane();
            if (p == null)
            {
                Debug.LogError("ECH_GaiasBlessingTransmute: cannot find proper Plane");
                return;
            }

            var town = target as TownLocation;
            if (town.Position == null)
            {
                Debug.LogError("ECH_GaiasBlessingTransmute: cannot find proper Hex");
                return;
            }


            var transmuteChance = e.fIntData;
            var random = new MHRandom();
            foreach (var hex in town.GetSurroundingArea(town.GetTownRange()))
            {
                var h = p.GetHexAt(hex);
                if ( h.GetTerrain().terrainType == ETerrainType.Desert || 
                    h.GetTerrain().terrainType == ETerrainType.Mountain)
                {
                    if (h.GetTerrain().transmuteTo == null) continue;
                    if (transmuteChance == FInt.ZERO)
                    {
                        transmuteChance = FInt.ONE;
                    }

                    if (random.GetSuccesses(transmuteChance.ToFloat(), 1) > 0)
                    {
                        var changeTo = h.GetTerrain().transmuteTo;
                        h.SetTerrain(changeTo, p);

                        HashSet<Vector3i> rebuildRequired = new HashSet<Vector3i>();
                        rebuildRequired.Add(hex);
                        p.RebuildUpdatedTerrains(rebuildRequired);
                        p.UpdateHeightsAfterTerrainChange(hex);
                    }
                }
            }
        }
        static public void ECH_WallOfDarknessBattle(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            EAPP_WallOfDarknessBattle(target, null, ei);
        }
        static public void ECH_WallOfFireBattle(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            EAPP_WallOfFireBattle(target, null, ei);
        }
        static public void ECH_FlyingFortress(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            var battle = target as Battle;

            if (battle == null)
            {
                Debug.LogWarning("ECH_FlyingFortress is not targeting Battle");
                return;
            }

            foreach (var u in battle.defenderUnits)
            {
                if (u.GetAttFinal(TAG.CAN_FLY) <= FInt.ZERO)
                {
                    u.GetAttributes().SetBaseTo((Tag)TAG.CAN_FLY, FInt.ONE);
                    u.GetAttributes().SetBaseTo((Tag)TAG.CAN_SWIM, FInt.ZERO);
                    u.GetAttributes().SetBaseTo((Tag)TAG.CAN_WALK, FInt.ZERO);
                    u.GetAttributes().AddToBase((Tag)TAG.DEFENCE_CHANCE, new FInt(0.1f));
                    u.GetAttributes().SetDirty();
                }
            }
        }
        static public void ECH_PowerPeoleEnchCity(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            if (!(data is MOM.TownLocation))
            {
                Debug.LogWarning("ECH_PowerPeoleEnchCity is not targeting TownLocation");
                return;
            }

            var town = data as MOM.TownLocation;

            var ench = (Enchantment)DataBase.Get("ENCH-POWER_PEOPLE_CITY", false);

            var wiz = GameManager.GetWizard(ei.owner.ID);
            if (wiz == town.GetWizardOwner() && town.race.Get() == wiz.mainRace.Get())
            {
                if (town.GetEnchantments().Find(o => o.source == ench) == null)
                {
                    town.AddEnchantment(ench, wiz as IEnchantable, ench.lifeTime, null, 0);
                }
            }

        }
        static public void ECH_SeaMaster(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            if (!(data is MOM.TownLocation))
            {
                Debug.LogWarning("ECH_SeaMaster is not targeting TownLocation");
                return;
            }

            var town = data as MOM.TownLocation;

            var ench = (Enchantment)ENCH.SHIP_DISCOUNT;

            var wiz = GameManager.GetWizard(ei.owner.ID);
            if (wiz == town.GetWizardOwner() && (town.seaside || town.race == (Race)RACE.BLUEORCS))
            {
                town.AddEnchantment(ench, wiz as IEnchantable);
            }
        }

        #endregion
        #region Attribute Passive Enchantment
        static public void ECH_None(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            //That scr give posibility to add ench on unit with use of TriggerType="RemoteUnitAttributeChangeMP"
        }
        static public void ECH_MeteorStormUnit(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            var unit = target as MOM.Unit;

            if (unit == null)
            {
                //KHASH: warning removed as it was thrown many times after enchantment is inherited by unit into the battle,
                //and during activations and simulations it was overflowing logs in thousands
                //Debug.LogWarning("ECH_MeteorStormUnit is not targeting MOM.Unit");
                return;
            }

            if (unit.group == null ) return;

            ret.AddFinal((Tag)TAG.METEOR_STORM_AFFECTED, 1);
        }
        static public void ECH_GiantStrength(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            ret.AddFinal((Tag)TAG.MELEE_ATTACK, 1);
            ret.AddFinal((Tag)TAG.THROW_BONUS, 1);
        }

        static public void ECH_Shatter(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            if(ret.GetFinal((Tag)TAG.MELEE_ATTACK) > 1)
                ret.SetFinal((Tag)TAG.MELEE_ATTACK, FInt.ONE);

            if (ret.GetFinal((Tag)TAG.RANGE_ATTACK) > 1)
                ret.SetFinal((Tag)TAG.RANGE_ATTACK, FInt.ONE);

            if (ret.GetFinal((Tag)TAG.THROW_BONUS) > 1)
                ret.SetFinal((Tag)TAG.THROW_BONUS, FInt.ONE);

            if (ret.GetFinal((Tag)TAG.FIRE_BREATH_BONUS) > 1)
                ret.SetFinal((Tag)TAG.FIRE_BREATH_BONUS, FInt.ONE);
        }

        static public void ECH_EldritchWeapon(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            var bu = target as BaseUnit;
            bu.targetDefMod += es.fIntData.ToFloat();

            ret.AddFinal((Tag)TAG.ENCHANTED_WEAPON, FInt.ONE);
        }

        static public void ECH_WarpCreature1(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            var bu = target as BattleUnit;
            var value = bu.attributes.GetBase(TAG.MELEE_ATTACK);
            value = value / 2;

            ret.SetFinal((Tag)TAG.MELEE_ATTACK, value);
        }
        static public void ECH_WarpCreature2(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            var bu = target as BattleUnit;
            var value = bu.attributes.GetBase(TAG.DEFENCE);
            value = value / 2;

            ret.SetFinal((Tag)TAG.DEFENCE, value);
        }
        static public void ECH_WarpCreature3(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            ret.SetFinal((Tag)TAG.RESIST, FInt.ZERO);
        }
        static public void ECH_FlameBlade(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            ret.AddFinal((Tag)TAG.MELEE_ATTACK, 2);
            ret.AddFinal((Tag)TAG.THROW_BONUS, 2);
            ret.SetFinal((Tag)TAG.ENCHANTED_WEAPON, FInt.ONE);
            if (!ret.ContainsKey((Tag)TAG.MAGIC_RANGE))
            {
                ret.AddFinal((Tag)TAG.RANGE_ATTACK, 2);
            }

        }


        static public void ECH_HolyWeapon(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            FInt data = new FInt(0.1f);
            ret.AddFinal((Tag)TAG.MELEE_ATTACK_CHANCE, data);
            ret.AddFinal((Tag)TAG.RANGE_ATTACK_CHANCE, data);
            ret.AddFinal((Tag)TAG.ENCHANTED_WEAPON, 1);
        }

        static public void ECH_Weakness(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            ret.AddFinal((Tag)TAG.MELEE_ATTACK, -2);
            ret.AddFinal((Tag)TAG.RANGE_ATTACK, -2);
            ret.AddFinal((Tag)TAG.THROW_BONUS, -2);
        }

        static public void ECH_Berserk(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            var unitAttack = ret[(Tag)TAG.MELEE_ATTACK];
            ret.AddFinal((Tag)TAG.MELEE_ATTACK, unitAttack);
            ret.SetFinal((Tag)TAG.DEFENCE, FInt.ZERO);
        }

        static public void ECH_Flight(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            ret.AddFinal((Tag)TAG.CAN_FLY, 1);
            ret.AddFinal((Tag)TAG.SIGHT_RANGE_BONUS, 1);

            if (ret.GetFinal((Tag)TAG.MOVEMENT_POINTS) <=2)
            {
                ret.AddFinal((Tag)TAG.MOVEMENT_POINTS, (FInt)(1.0));
            }
        }

        static public void ECH_WindWalking(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            if(target is BattleUnit)
            {
                // doesn't apply in battle
                ret.SetFinal((Tag)TAG.WIND_WALKING, FInt.ZERO);
                return;
            }

            ret.AddFinal((Tag)TAG.WIND_WALKING, 1);
            if(instance.parameters != null)
            {
                try
                {
                    var ownerMove = Convert.ToInt32(instance.parameters);
                    var delta = ownerMove - ret.GetFinal((Tag)TAG.MOVEMENT_POINTS);

                    if (delta > 0)
                    {
                        ret.AddFinal((Tag)TAG.MOVEMENT_POINTS, delta);
                    }
                }
                catch
                {
                }
            }
        }

        static public void ECH_LionHeart(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            ret.AddFinal((Tag)TAG.MELEE_ATTACK, 3);
            ret.AddFinal((Tag)TAG.RESIST, 3);
            ret.AddFinal((Tag)TAG.HIT_POINTS, 3);
            ret.AddFinal((Tag)TAG.THROW_BONUS, 3);
            if (!ret.ContainsKey((Tag)TAG.MAGIC_RANGE))
            {
                ret.AddFinal((Tag)TAG.RANGE_ATTACK, 3);
            }
        }

        static public void ECH_Vertigo(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            FInt data1 = new FInt (-0.2f);
            FInt data2 = new FInt (-1.0f);
            ret.AddFinal((Tag)TAG.MELEE_ATTACK_CHANCE, data1);
            ret.AddFinal((Tag)TAG.RANGE_ATTACK_CHANCE, data1);

            ret.AddFinal((Tag)TAG.DEFENCE, data2);
        }

        static public void ECH_MindStorm(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            FInt data = new FInt(-5);
            ret.AddFinal((Tag)TAG.MELEE_ATTACK, data);
            ret.AddFinal((Tag)TAG.RANGE_ATTACK, data);
            ret.AddFinal((Tag)TAG.DEFENCE, data);
            ret.AddFinal((Tag)TAG.RESIST, data);
        }

        static public void ECH_WraithForm(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            FInt data = FInt.ONE;
            ret.AddFinal((Tag)TAG.NON_CORPOREAL, data);
            ret.AddFinal((Tag)TAG.CAN_SWIM, data);
            ret.AddFinal((Tag)TAG.WEAPON_IMMUNITY, data);
        }

        static public void ECH_SpellLock(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            var bu = target as BaseUnit;
            bu.isSpellLock = true;
        }

        static public void ECH_Invulnerablity(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            var bu = target as BaseUnit;
            bu.invulnerabilityProtection += es.fIntData.ToInt();
        }

        static public void ECH_Blur(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            var bu = target as BaseUnit;
            bu.blurProtection += es.fIntData.ToFloat();
        }

        static public void ECH_InvisibilityProtection(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            var bu = target as BaseUnit;
            bu.invisibilityProtection = es.fIntData.ToFloat();
        }
        static public void ECH_MassInvisibility(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            var bu = target as BaseUnit;
           
            bu.invisibilityProtection = es.fIntData.ToFloat();
            ret.AddFinal((Tag)TAG.INVISIBILITY, FInt.ONE);
        }

        static public void ECH_Stasis(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            var bu = target as BaseUnit;
            bu.canMove = false;
        }

        static public void ECH_BlackSleep(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            var bu = target as BattleUnit;
            bu.canAttack = false;
            bu.canContrAttack = false;
            bu.canCastSpells = false;
            bu.canMove = false;
            bu.canDefend = false;
            bu.Mp = FInt.ZERO;
            if (ret.ContainsKey((Tag)TAG.CAN_FLY))
            {
                ret.SetFinal((Tag)TAG.CAN_WALK, FInt.ONE);
                ret.SetFinal((Tag)TAG.CAN_FLY, FInt.ZERO);
            }
        }

        static public void ECH_Terror(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            var bu = target as BattleUnit;
            bu.canAttack = false;
            bu.canCastSpells = false;
            bu.canMove = false;
            bu.Mp = FInt.ZERO;
        }

        static public void ECH_AddTag(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            Tag tag = (Tag)DataBase.Get(es.stringData, false);
            if (tag != null)
            {
                int defMod = es.fIntData.ToInt();
                ret.AddFinal(tag, defMod);
            }
            else Debug.LogWarning(es.stringData + " is not a Tag. You have a typo?");
        }
        static public void ECHF_AddTag(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            Tag tag = (Tag)DataBase.Get(es.stringData, false);
            if (tag != null)
            {
                FInt defMod = es.fIntData;
                ret.AddFinal(tag, defMod);
            }
            else Debug.LogWarning(es.stringData + " is not a Tag. You have a typo?");
        }

        static public void ECH_SpellWardEnfeeblingHex(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            ret.SetFinal((Tag)TAG.DEFENCE, FInt.ZERO);
            ret.SetFinal((Tag)TAG.RESIST, FInt.ZERO);
            ret.SetFinal((Tag)TAG.MELEE_ATTACK, FInt.ZERO);
            ret.SetFinal((Tag)TAG.RANGE_ATTACK, FInt.ZERO);
            ret.SetFinal((Tag)TAG.HIT_POINTS, FInt.ONE);
        }

        static public void ECH_Heroism(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            if (!(target is MOM.Unit)) return;

            var unit = target as MOM.Unit;
            if (unit.xp >= 120)
            {
                instance.countDown = 0;
            }
        }

        static public void ECH_CharmOfLife(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            var t = (Tag)TAG.HIT_POINTS;
            var val = ret[t];
            if(val < 4)
            {
                ret.AddFinal(t, 1);
            }
            else
            {
                int extralife = (val * 0.25f).ToInt();
                ret.AddFinal(t, extralife);
            }
        }

        static public void ECH_HerbMastery(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            if (!(target is PlayerWizard)) return;

            var playerWizard = target as PlayerWizard;
            var wizardGroups = GameManager.GetGroupsOfWizard(playerWizard.ID);

            foreach (var g in wizardGroups)
            {
                var units = g.GetUnits();
                for (int i = 0; i < units.Count; i++)
                {
                    units[i].Get().Heal(100f);
                }
            }
        }
        static public void ECH_HolyArms(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            ret.AddFinal((Tag)TAG.MELEE_ATTACK_CHANCE, (FInt)0.1);
            ret.AddFinal((Tag)TAG.ENCHANTED_WEAPON, FInt.ONE);
            if (!ret.ContainsKey((Tag)TAG.MAGIC_RANGE))
            {
                ret.AddFinal((Tag)TAG.RANGE_ATTACK_CHANCE, (FInt)0.1);
            }

            var t = target as MOM.Unit; 
            if (t != null)
            {
                var e = t.GetEnchantments().FindAll(o => o.source == (Enchantment)ENCH.HOLY_WEAPON);
                if (e != null && e.Count > 0)
                {
                    foreach (var i in e)
                    {
                        t.GetEnchantmentManager().Remove(i);
                    }
                }
            }
        }
        static public void ECH_HighPrayerPositive(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            if (target is BaseUnit)
            {
                ret.AddFinal((Tag)TAG.MELEE_ATTACK, 2);
                ret.AddFinal((Tag)TAG.DEFENCE, 2);
                ret.AddFinal((Tag)TAG.RESIST, 3);
                ret.AddFinal((Tag)TAG.MELEE_ATTACK_CHANCE, new FInt(0.1f));
                ret.AddFinal((Tag)TAG.RANGE_ATTACK_CHANCE, new FInt(0.1f));
                ret.AddFinal((Tag)TAG.DEFENCE_CHANCE, new FInt(0.1f));
            }
        }
        static public void ECH_HighPrayerNegative(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            if (target is BaseUnit)
            {
                ret.AddFinal((Tag)TAG.MELEE_ATTACK_CHANCE, new FInt(-0.1f));
            }
        }
        static public void ECH_PrayerPositive(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            if (target is BaseUnit)
            {
                if (target.GetEnchantmentManager().GetEnchantmentsWithRemotes().FindIndex(o => o.source.Get() == (Enchantment)ENCH.HIGH_PRAYER_POSITIVE) < 0)
                {
                    ret.AddFinal((Tag)TAG.RESIST, FInt.ONE);
                    ret.AddFinal((Tag)TAG.MELEE_ATTACK_CHANCE, new FInt(0.1f));
                    ret.AddFinal((Tag)TAG.RANGE_ATTACK_CHANCE, new FInt(0.1f));
                    ret.AddFinal((Tag)TAG.DEFENCE_CHANCE, new FInt(0.1f));
                }
            }
        }
        static public void ECH_PrayerNegative(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            if (target is BaseUnit)
            {
                if (target.GetEnchantmentManager().GetEnchantmentsWithRemotes().FindIndex(o => o.source.Get() == (Enchantment)ENCH.HIGH_PRAYER_NEGATIVE) < 0)
                {
                    ret.AddFinal((Tag)TAG.MELEE_ATTACK_CHANCE, new FInt(-0.1f));
                }
            }
        }
        static public void ECH_Entangle(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            ret.AddFinal((Tag)TAG.MOVEMENT_POINTS, (FInt)(-2.0));

            if (target is BattleUnit)
            {
                var bu = target as BattleUnit;
                
                if (!bu.simulated)
                {
                    var effect = ((Spell)SPELL.ENTANGLE).castEffect;
                    FSMBattleTurn.instance?.CastEffect(bu.GetPosition(), effect);
                    AudioLibrary.RequestSFX("SpellEntangle");
                }                
            }
        }
        static public void ECH_Tactician(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            if (target is BattleUnit)
            {
                var bu = target as BattleUnit;

                ret.AddFinal((Tag)TAG.MOVEMENT_POINTS, (FInt)(1.0));
            }
        }
        static public void ECH_WaterSpeed(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            if (ret.GetFinal((Tag)TAG.CAN_SWIM) <= 0) return;

            if (target is MOM.Unit || target is BattleUnit)
            {
                ret.AddFinal((Tag)TAG.MOVEMENT_POINTS, (FInt)(1.0));
            }
        }
        static public void ECH_Darkness(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            if (target is BattleUnit)
            {
                var bu = target as BattleUnit;
                if (bu == null || !bu.IsAlive()) return;
                if (bu.darknessEffect) return;
                
                if (bu.race == (Race)RACE.REALM_DEATH)
                {
                    bu.darknessEffect = true;
                    ret.AddFinal((Tag)TAG.RESIST, FInt.ONE);
                    ret.AddFinal((Tag)TAG.DEFENCE, FInt.ONE);
                    ret.AddFinal((Tag)TAG.MELEE_ATTACK, FInt.ONE);
                    ret.AddFinal((Tag)TAG.RANGE_ATTACK, FInt.ONE);
                    ret.AddFinal((Tag)TAG.FIRE_BREATH_BONUS, FInt.ONE);
                    ret.AddFinal((Tag)TAG.THROW_BONUS, FInt.ONE);
                }
                else if (bu.race == (Race)RACE.REALM_LIFE)
                {
                    bu.darknessEffect = true;
                    ret.AddFinal((Tag)TAG.RESIST, FInt.N_ONE);
                    ret.AddFinal((Tag)TAG.DEFENCE, FInt.N_ONE);
                    ret.AddFinal((Tag)TAG.MELEE_ATTACK, FInt.N_ONE);
                    ret.AddFinal((Tag)TAG.RANGE_ATTACK, FInt.N_ONE);
                    ret.AddFinal((Tag)TAG.FIRE_BREATH_BONUS, FInt.N_ONE);
                    ret.AddFinal((Tag)TAG.THROW_BONUS, FInt.N_ONE);
                }
            }
        }
        static public void ECH_DarknessPositive(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            //obsolete script, not in use right now
            if (target is BaseUnit)
            {
                var u = target as BaseUnit;
                if (u.darknessEffect) return;

                u.darknessEffect = true;
                ret.AddFinal((Tag)TAG.RESIST, FInt.ONE);
                ret.AddFinal((Tag)TAG.DEFENCE, FInt.ONE);
                ret.AddFinal((Tag)TAG.MELEE_ATTACK, FInt.ONE);
                ret.AddFinal((Tag)TAG.RANGE_ATTACK, FInt.ONE);
                ret.AddFinal((Tag)TAG.FIRE_BREATH_BONUS, FInt.ONE);
                ret.AddFinal((Tag)TAG.THROW_BONUS, FInt.ONE);
            }
        }
        static public void ECH_DarknessNegative(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            //obsolete script, not in use right now
            if (target is BaseUnit)
            {
                var u = target as BaseUnit;
                if (u.darknessEffect) return;

                u.darknessEffect = true;
                ret.AddFinal((Tag)TAG.RESIST, FInt.N_ONE);
                ret.AddFinal((Tag)TAG.DEFENCE, FInt.N_ONE);
                ret.AddFinal((Tag)TAG.MELEE_ATTACK, FInt.N_ONE);
                ret.AddFinal((Tag)TAG.RANGE_ATTACK, FInt.N_ONE);
                ret.AddFinal((Tag)TAG.FIRE_BREATH_BONUS, FInt.N_ONE);
                ret.AddFinal((Tag)TAG.THROW_BONUS, FInt.N_ONE);
            }
        }
        static public void ECH_BlackPrayer(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            ret.AddFinal((Tag)TAG.MELEE_ATTACK, -1);
            ret.AddFinal((Tag)TAG.RANGE_ATTACK, -1);
            ret.AddFinal((Tag)TAG.DEFENCE, -1);
            ret.AddFinal((Tag)TAG.RESIST, -2);       
            ret.AddFinal((Tag)TAG.THROW_BONUS, -1);       
            ret.AddFinal((Tag)TAG.FIRE_BREATH_BONUS, -1);
        }
        static public void ECH_MetalFires(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            ret.AddFinal((Tag)TAG.MELEE_ATTACK, 1);
            ret.AddFinal((Tag)TAG.RANGE_ATTACK, 1);
            ret.AddFinal((Tag)TAG.THROW_BONUS, 1);
            ret.AddFinal((Tag)TAG.ENCHANTED_WEAPON, 1);
        }
        static public void ECH_TrueLightPositive(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            //obsolete script, not in use right now
            if(target is BaseUnit)
            {
                var bu = target as BaseUnit;
                if(bu.race == (Race)RACE.REALM_LIFE)
                {
                    ret.AddFinal((Tag)TAG.MELEE_ATTACK, 1);
                    ret.AddFinal((Tag)TAG.DEFENCE, 1);
                    ret.AddFinal((Tag)TAG.RESIST, 1);
                }
            }
        }
        static public void ECH_TrueLight(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            var bu = target as BattleUnit;
            if (bu == null || !bu.IsAlive()) return;

            if (bu.race == (Race)RACE.REALM_LIFE)
            {
                ret.AddFinal((Tag)TAG.MELEE_ATTACK, FInt.ONE);
                ret.AddFinal((Tag)TAG.DEFENCE, FInt.ONE);
                ret.AddFinal((Tag)TAG.RESIST, FInt.ONE);
            }
            else if (bu.race == (Race)RACE.REALM_DEATH)
            {
                ret.AddFinal((Tag)TAG.MELEE_ATTACK, FInt.N_ONE);
                ret.AddFinal((Tag)TAG.DEFENCE, FInt.N_ONE);
                ret.AddFinal((Tag)TAG.RESIST, FInt.N_ONE);
            }
        }
        static public void ECH_TrueLightNegative(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            //obsolete script, not in use right now
            if (target is BaseUnit)
            {
                var bu = target as BaseUnit;
                if (bu.race == (Race)RACE.REALM_DEATH)
                {
                    ret.AddFinal((Tag)TAG.MELEE_ATTACK, -1);
                    ret.AddFinal((Tag)TAG.DEFENCE, -1);
                    ret.AddFinal((Tag)TAG.RESIST, -1);
                }
            }
        }
        static public void ECH_WarpReality(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            if(target is BattleUnit)
            {
                var bu = target as BattleUnit;
                if(bu.race != (Race)RACE.REALM_CHAOS)
                {
                    ret.AddFinal((Tag)TAG.MELEE_ATTACK_CHANCE, (FInt)(-0.2));
                    ret.AddFinal((Tag)TAG.RANGE_ATTACK_CHANCE, (FInt)(-0.2));
                }
            }
        }
        static public void ECH_ChaosSurgeUnit(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            var u = target as MOM.Unit;
            var b = target as MOM.BattleUnit;
            if (u != null && u.chaosSurgeEffect == false)
            {
                u.chaosSurgeEffect = true;
                ret.AddFinal((Tag)TAG.MELEE_ATTACK, 2);
                ret.AddFinal((Tag)TAG.RANGE_ATTACK, 2);
                ret.AddFinal((Tag)TAG.THROW_BONUS, 2);
                ret.AddFinal((Tag)TAG.FIRE_BREATH_BONUS, 2);
                ret.AddFinal((Tag)TAG.DOOM_GAZE_BONUS, 2);
            }
            else if (b != null && b.chaosSurgeEffect == false)
            {
                b.chaosSurgeEffect = true;
                ret.AddFinal((Tag)TAG.MELEE_ATTACK, 2);
                ret.AddFinal((Tag)TAG.RANGE_ATTACK, 2);
                ret.AddFinal((Tag)TAG.THROW_BONUS, 2);
                ret.AddFinal((Tag)TAG.FIRE_BREATH_BONUS, 2);
                ret.AddFinal((Tag)TAG.DOOM_GAZE_BONUS, 2);
            }
        }
        static public void ECH_WindMastery_Positive(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            if (target is MOM.Unit)
            {
                var t = (Tag)TAG.MOVEMENT_POINTS;
                var val = ret[t];
                val = (val * 1.5f).ReturnRoundedFloor();

                ret.SetFinal(t, val);
            }
        }
        static public void ECH_WindMastery_Negative(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            if (target is MOM.Unit)
            {
                var u = target as MOM.Unit;
                if (u.windMasteryNegative) return;

                u.windMasteryNegative = true;
                var t = (Tag)TAG.MOVEMENT_POINTS;
                var val = ret[t];
                val = val * 0.5f;
                val = val.ReturnRoundedCeil();
                if (val < FInt.ONE) val = FInt.ONE;

                ret.SetFinal(t, val);
            }
        }

        static public void ECH_Haste(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            if(target is BattleUnit)
            {
                var bu = target as BattleUnit;
                bu.haste = true;
            }
        }

        static public void ECH_Web(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            var bu = target as BattleUnit;
            bu.canMove = false;
            bu.canCastSpells = false;
            bu.canAttack = false;
        }
        static public void ECH_Web_NoFly(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            ret.AddFinal((Tag)TAG.CAN_WALK, 1);
            ret.AddFinal((Tag)TAG.CAN_FLY, -1);
            ret.AddFinal((Tag)TAG.SIGHT_RANGE_BONUS, - 1);
        }
        static public void ECH_EarthToMud(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            if (!(target is BattleUnit)) return;

            ret.SetFinal((Tag)TAG.MOVEMENT_POINTS, FInt.ONE);
        }
        static public void ECH_Praymaster(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {            
            ret.AddFinal((Tag)TAG.RESIST, instance.intParametr);
        }
        static public void ECH_Leadership(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            ret.AddFinal((Tag)TAG.MELEE_ATTACK, instance.intParametr);
            ret.AddFinal((Tag)TAG.THROW_BONUS, instance.intParametr / 2);
            ret.AddFinal((Tag)TAG.FIRE_BREATH_BONUS, instance.intParametr / 2);
            if (!ret.ContainsKey((Tag)TAG.MAGIC_RANGE))
            {
                ret.AddFinal((Tag)TAG.RANGE_ATTACK, instance.intParametr / 2);
            }
        }
        static public void ECH_Armsmaster(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            BaseUnit u = target as BaseUnit;
            if (u == null)
            {
                Debug.LogError("Enchantment target should be BaseUnit");
                return;
            }
            u.xp = u.xp + instance.intParametr;
        }
        static public void ECH_HolyBonus(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            BaseUnit u = target as BaseUnit;
            if (u == null)
            {
                Debug.LogError("Enchantment target should be BaseUnit");
                return;
            }
            
            ret.AddFinal((Tag)TAG.MELEE_ATTACK, es.fIntData);
            ret.AddFinal((Tag)TAG.DEFENCE, es.fIntData);
            ret.AddFinal((Tag)TAG.RESIST, es.fIntData);
        }
        static public void ECH_HeavenlyLight(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            if (!(target is MOM.BaseUnit)) return;

            var u = target as MOM.BaseUnit;

            if (u.race == (Race)RACE.REALM_LIFE)
            {
                ret.AddFinal((Tag)TAG.MELEE_ATTACK, 1);
                ret.AddFinal((Tag)TAG.DEFENCE, 1);
                ret.AddFinal((Tag)TAG.RESIST, 1);
            }
            if (u.race == (Race)RACE.REALM_DEATH )
            {
                ret.AddFinal((Tag)TAG.MELEE_ATTACK, -1);
                ret.AddFinal((Tag)TAG.DEFENCE, -1);
                ret.AddFinal((Tag)TAG.RESIST, -1);
            }
        }

        static public void ECH_NodeBonus(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            ret.AddFinal((Tag)TAG.MELEE_ATTACK, 2);
            ret.AddFinal((Tag)TAG.RANGE_ATTACK, 2);
            ret.AddFinal((Tag)TAG.DEFENCE, 2);
            ret.AddFinal((Tag)TAG.RESIST, 2);
        }

        static public void ECH_UndeadUpkeepModifier(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            BaseUnit u = target as BaseUnit;

            if(u == null)
            {
                Debug.LogError("Enchantment target should be BaseUnit");
                return;
            }
            if (u.GetAttributes().GetBase((Tag)TAG.NORMAL_CLASS) > 0)
            {
                ret.SetFinal((Tag)TAG.UPKEEP_GOLD, FInt.ZERO);
                ret.SetFinal((Tag)TAG.UPKEEP_FOOD, FInt.ZERO);
                ret.SetFinal((Tag)TAG.UPKEEP_MANA, FInt.ZERO);
            }
            else
            {
                FInt mod = es.fIntData;
                var value = u.GetAttributes().GetBase(TAG.UPKEEP_GOLD) * mod;
                if (value > 0)
                    ret.SetFinal((Tag)TAG.UPKEEP_GOLD, value);

                value = u.GetAttributes().GetBase(TAG.UPKEEP_FOOD) * mod;
                if (value > 0)
                    ret.SetFinal((Tag)TAG.UPKEEP_FOOD, value);

                value = u.GetAttributes().GetBase(TAG.UPKEEP_MANA) * mod;
                if (value > 0)
                    ret.SetFinal((Tag)TAG.UPKEEP_MANA, value);
            }
        }
        static public void ECH_DetectMagic(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            if (!(target is PlayerWizard)) return;

            (target as PlayerWizard).detectMagic = true;
        }
        static public void ECH_Cursed(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            var value = instance.intParametr * -1;
            ret.AddFinal((Tag)TAG.RESIST, value);
        }
        static public void ECH_FantasticWarlord(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            ret.AddFinal((Tag)TAG.MELEE_ATTACK, FInt.ONE);
            ret.AddFinal((Tag)TAG.RANGE_ATTACK, FInt.ONE);
            ret.AddFinal((Tag)TAG.DEFENCE, FInt.ONE);
            ret.AddFinal((Tag)TAG.RESIST, FInt.ONE);
            ret.AddFinal((Tag)TAG.FIRE_BREATH_BONUS, FInt.ONE);
        }
        static public void ECH_Stun(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            FInt meleeAtt = -1 * ret.GetFinal((Tag)TAG.MELEE_ATTACK) / 2;
            FInt rangeAtt = -1 * ret.GetFinal((Tag)TAG.RANGE_ATTACK) / 2;
            ret.AddFinal((Tag)TAG.MELEE_ATTACK, meleeAtt);
            ret.AddFinal((Tag)TAG.RANGE_ATTACK, rangeAtt);
        }
        static public void ECH_HeroTraining(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            if (!(target is MOM.BaseUnit)) return;

            var u = target as MOM.BaseUnit;
            if (u.GetEnchantments().Find(o => o.source == (Enchantment)ENCH.HEROISM) == null)
            {
                u.levelOverride = 3;
                u.GetAttributes().SetDirty();
            }
        }
        static public void ECH_MagicBullet(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            if (es.fIntData == null) return;

            FInt extraAmmo = es.fIntData;

            if (target is BattleUnit)
            {
                var unit = target as BattleUnit;

                if (unit.race == (Race)RACE.SOULTRAPPED || ret.GetFinal((Tag)TAG.MECHANICAL_UNIT) > 0)
                    extraAmmo = extraAmmo + FInt.ONE;

                var orginalAmmo = unit.dbSource.Get().GetTag(TAG.AMMUNITION).ToInt();
                unit.GetCurentFigure().rangedAmmo = orginalAmmo + extraAmmo.ToInt();
                ret.AddFinal((Tag)TAG.ENCHANTED_WEAPON, 1);
                unit.GetAttributes().GetDirty();
            }

            if (target is MOM.Unit)
            {
                var unit = target as MOM.Unit;

                if (unit.race == (Race)RACE.SOULTRAPPED || ret.GetFinal((Tag)TAG.MECHANICAL_UNIT) > 0)
                    extraAmmo = extraAmmo + FInt.ONE;

                ret.AddFinal((Tag)TAG.AMMUNITION, extraAmmo);
                ret.AddFinal((Tag)TAG.ENCHANTED_WEAPON, 1);
                unit.GetAttributes().GetDirty();
            }
        }

        static public void ECH_MaxXP(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            if (target is BattleUnit)
            {
                var unit = target as BattleUnit;

                //LevelOverride cannot be +=, ench is triggered more then one time
                unit.levelOverride = 4;
                unit.GetAttributes().GetDirty();
            }
        }

        static public void ECH_ImprovedWarlord(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            if (!(target is PlayerWizard)) return;

            var playerWizard = target as PlayerWizard;
            var wizardGroups = GameManager.GetGroupsOfWizard(playerWizard.ID);

            foreach (var g in wizardGroups)
            {
                var units = g.GetUnits();
                for (int i = 0; i < units.Count; i++)
                {
                    if (units[i].Get().canGainXP)
                        units[i].Get().xp += 1;
                }
            }
        }
        static public void ECH_ConstantStorm(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            var random = new MHRandom();
            int numberOfAttacks = 1; //random.GetInt(3, 6);
            int[] dmg = new int[1] { e.fIntData.ToInt() };
            var b = Battle.GetBattle();
            List<BattleUnit> enemyUnits = new List<BattleUnit>();

            if (target is Battle)
            {
                if (b.attacker.wizard != null)
                {
                    enemyUnits = b.attackerUnits.FindAll(o => o.IsAlive());
                }
            }


            if (enemyUnits.Count > 0)
            {
                BattleUnit unit;
                var effect = "Effect_ConstantStorm";               

                for (int i = 0; i < numberOfAttacks; i++)
                {
                    unit = enemyUnits[random.GetInt(0, enemyUnits.Count)];
                    if (!unit.IsAlive())
                    {
                        unit = enemyUnits.Find(o => o.IsAlive());
                        if (unit == null || !unit.IsAlive()) break;
                    }
                    if (!unit.simulated && b != null)
                    {
                        AudioLibrary.RequestSFX("EnchantmentConstantStorm");
                        FSMBattleTurn.instance?.CastEffect(unit.GetPosition(), effect);
                    }

                    dmg[0] = random.GetSuccesses(0.3f, dmg[0]);
                    if (unit.attributes.Contains(TAG.LIGHTNING_WEAKNESS))
                    {
                        dmg[0] = dmg[0] * 2;
                    }

                    if (unit.attributes.Contains(TAG.MAGIC_IMMUNITY))
                    {
                        unit.ApplyDamage(dmg, random, null, 50, true, null, null, e);
                    }
                    else if (unit.attributes.Contains(TAG.ELEMENTAL_ARMOR))
                    {
                        unit.ApplyDamage(dmg, random, null, 10, false, null, null, e);
                    }
                    else if (unit.attributes.Contains(TAG.RESIST_ELEMENTS))
                    {
                        unit.ApplyDamage(dmg, random, null, 3, false, null, null, e);
                    }
                    else if (unit.attributes.Contains(TAG.LARGE_SHIELD))
                    {
                        unit.ApplyDamage(dmg, random, null, 2, false, null, null, e);
                    }
                    else
                    {
                        unit.ApplyDamage(dmg, random, null, 0, false, null, null, e);
                    }

                    if (enemyUnits.Count <= 0) return;
                }
            }
        }
        static public void ECH_ShallowWater(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            ret.AddFinal((Tag)TAG.CAN_SWIM, 1);
        }
        static public void ECH_SeaHunter(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            if(ret.GetFinal((Tag)TAG.CAN_SWIM) <= 0) ret.AddFinal((Tag)TAG.CAN_SWIM, 1);
            ret.AddFinal((Tag)TAG.MELEE_ATTACK, 2);
            ret.AddFinal((Tag)TAG.RANGE_ATTACK, 2);
            ret.AddFinal((Tag)TAG.DEFENCE, 2);
            ret.AddFinal((Tag)TAG.RESIST, 2);
        }
        static public void ECH_WindCaveAttack(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            if(e.fIntData == null) return;

            var random = new MHRandom();
            int maxNumberOfHalvedUnits = e.fIntData.ToInt();
            var b = Battle.GetBattle();
            List<BattleUnit> enemyUnits = new List<BattleUnit>();

            if (target is Battle)
            {
                if (b.attacker.wizard != null)
                {
                    enemyUnits = b.attackerUnits.FindAll(o => o.IsAlive());
                }
            }


            if (enemyUnits.Count > 0)
            {
                BattleUnit unit;
                enemyUnits.RandomSort();
                var effect = "Effect_WindCave";
                float attackChance = 0.05f;

                for (int i = 0; i < enemyUnits.Count; i++)
                {
                    unit = enemyUnits[i];
                    if (!unit.IsAlive() ||
                        unit.attributes.Contains(TAG.NON_CORPOREAL)) continue;

                    if (random.GetSuccesses(attackChance, 1) > 0)
                    {
                        float figures = (float)unit.FigureCount() / 2;
                        float figureHP = (float)unit.currentFigureHP / 2;

                        unit.figureCount = (int)Math.Ceiling(figures);
                        unit.currentFigureHP = (int)Math.Ceiling(figureHP);

                        if (!unit.simulated && b != null)
                        {
                            AudioLibrary.RequestSFX("EnchantmentWindCave");
                            FSMBattleTurn.instance?.CastEffect(unit.GetPosition(), effect);
                        }
                    }

                    if (maxNumberOfHalvedUnits <= 0) return;
                }
            }
        }
        static public void ECH_WindCaveAttributeChange(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            ret.AddFinal((Tag)TAG.RANGE_ATTACK_CHANCE, (FInt)(- 0.1f));
            if (ret.ContainsKey((Tag)TAG.CAN_FLY))
            {
                ret.SetFinal((Tag)TAG.CAN_FLY, FInt.ZERO);
                if (!ret.ContainsKey((Tag)TAG.CAN_SWIM))
                {
                    ret.SetFinal((Tag)TAG.CAN_SWIM, FInt.ONE);
                }
            }
        }
        static public void ECH_PirateCurse(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            ret.AddFinal((Tag)TAG.RESIST, -1);
            ret.AddFinal((Tag)TAG.MOVEMENT_POINTS, -1);
        }
        static public void ECH_Admiral(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            if (!(target is MOM.BaseUnit)) return;

            var u = target as MOM.BaseUnit;
            if (u.GetEnchantments().Find(o => o.source == (Enchantment)ENCH.HEROISM) == null)
            {
                u.levelOverride = 4;
            }
        }
        static public void ECH_Mud(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            if (es.fIntData == null) return;
            if (!(target is BattleUnit)) return;

            ret.AddFinal((Tag)TAG.MOVEMENT_POINTS, -es.fIntData.ToInt());
        }

        #endregion
        #region Wizzard Special Enchancements
        static public void ESP_NatureAwareness(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, object data)
        {
            var fow = data as FOW;
            if (fow == null) return;

            if(target != GameManager.GetHumanWizard())
            {
                Debug.LogError("Fow data is used only by human player ");
            }

            var area = World.GetArcanus().area.GetAreaHex();
            foreach (var v in area)
            {
                fow.MarkVisible(v, true);
            }
            area = World.GetMyrror().area.GetAreaHex();
            foreach (var v in area)
            {
                fow.MarkVisible(v, false);
            }
        }
        static public void ESP_Armageddon(IEnchantable target, EnchantmentScript es, EnchantmentInstance ei, object data)
        {
            var gm = target as GameManager;
            if (gm == null)
            {
                Debug.LogError("Enchantment designed to work for GameManager!");
                return;
            }

            MHRandom random = new MHRandom();
            int volcanos = random.GetInt(3, 7); //pick from 3 to 6
            WorldCode.Plane p;
            Hex h;
            MOM.Location hexLocation = null;
            List<Vector3i> l = new List<Vector3i>();

            for (int i = 0; i < volcanos; i++)
            {
                //pick a plane
                if (random.GetInt(0, 2) == 0)
                {
                    p = World.GetArcanus();
                }
                else
                {
                    p = World.GetMyrror();
                }

                HashSet<Vector3i> hexes = p.GetLandHexes();
                int hashStartIndex = random.GetInt(0, hexes.Count);
                int index;
                Hex validHex = null;

                while (true)
                {
                    index = 0;
                    foreach (var item in hexes)
                    {
                        if (index >= hashStartIndex)
                        {
                            h = p.GetHexAt(item);
                            var terrain = h.GetTerrain();
                            var terrainType = terrain.terrainType;

                            // cannot raise volcano on certain terrain type or on the other volcano 
                            if (terrainType != DBDef.ETerrainType.Sea &&
                                terrainType != DBDef.ETerrainType.Coast &&
                                terrainType != DBDef.ETerrainType.RiverBank &&
                                terrain != (DBDef.Terrain)TERRAIN.VOLCANO &&
                                terrain != (DBDef.Terrain)TERRAIN.MYR_VOLCANO)
                            {
                                // cannot raise volcano on the magic node
                                List<MOM.Location> planeLocs = GameManager.GetLocationsOfThePlane(p);
                                hexLocation = null;
                                hexLocation = planeLocs.Find(o => o.Position == h.Position);
                                if (hexLocation != null)
                                {
                                    if (hexLocation.source.Get() is MagicNode)
                                    {
                                        index++;
                                        continue;
                                    }
                                }

                                // cannot raise volcano on the range of caster's towns
                                List<Vector3i> townRange = HexNeighbors.GetRange(h.Position, 2); // town range is 2, need to check if hex is on range of any town
                                List<MOM.Location> locInRange = new List<MOM.Location>();
                                foreach (var position in townRange)
                                {
                                    locInRange.AddRange(planeLocs.FindAll(o => o.GetPosition() == position));
                                }
                                if (locInRange.Count > 0)
                                {
                                    bool valid = true;
                                    foreach (var loc in locInRange)
                                    {
                                        if (loc is TownLocation)
                                        {
                                            TownLocation t = loc as TownLocation;
                                            if (t.GetWizardOwner() == ei.owner.GetEntity())
                                            {
                                                valid = false;
                                                break;
                                            }
                                        }
                                    }
                                    if (valid)
                                    {
                                        validHex = h;
                                        break;
                                    }
                                }
                            }
                        }
                        index++;
                    }
                    if(validHex != null)
                    {
                        break;
                    }
                    else
                    {
                        hashStartIndex = 0;
                    }
                }

                // spawn volcano
                DBDef.Terrain changeTo = p.arcanusType ? (DBDef.Terrain)TERRAIN.VOLCANO : (DBDef.Terrain)TERRAIN.MYR_VOLCANO;
                validHex.SetTerrain(changeTo, p);
                validHex.Resource = null;
                if (validHex.resourceInstance != null)
                {
                    GameObject.Destroy(validHex.resourceInstance);
                    validHex.resourceInstance = null;
                }

                HashSet<Vector3i> rebuildRequired = new HashSet<Vector3i>();
                rebuildRequired.Add(validHex.Position);
                p.RebuildUpdatedTerrains(rebuildRequired);
                p.UpdateHeightsAfterTerrainChange(validHex.Position);

                // update location terrain mesh alignment if volcano spawned under location
                if (hexLocation != null)
                {
                    // 15% chance to destroy building in town
                    if (hexLocation is TownLocation)
                    {
                        var t = hexLocation as TownLocation;
                        List<DBReference<Building>> buildings = new List<DBReference<Building>>(t.buildings);
                        for (int j = buildings.Count - 1; j >= 0; j--)
                        {
                            if (random.GetFloat(0f, 1f) <= 0.15f)
                            {
                                t.RemoveBuildingSpecial(buildings[j]);
                            }
                        }
                    }
                }
                if (ei.owner.GetEntity() is PlayerWizard)
                {
                    var w = ei.owner.GetEntity() as PlayerWizard;
                    w.AddVolcano(validHex.Position, p.arcanusType);
                }
                else
                {
                    Debug.LogError("ESP_Armageddon: enchantment instance owner is no PlayerWizard type");
                }
            }
        }
        static public void ESP_Awareness(IEnchantable target, EnchantmentScript es, EnchantmentInstance instance, object data)
        {
            var fow = data as FOW;
            if (fow == null) return;

            if (target != GameManager.GetHumanWizard())
            {
                Debug.LogError("Fow data is used only by human player ");
            }

            List<MOM.Location> locations = new List<MOM.Location>(GameManager.Get().registeredLocations);
            locations = locations.FindAll(o => o is TownLocation);

            var arcanus = World.GetArcanus();

            foreach (var l in locations)
            {
                 foreach (var v in HexNeighbors.GetRange(l.GetPosition(), 1))
                 {
                     fow.MarkVisible(v, l.plane == arcanus);
                 }
            }
        }
        static public void ESP_Ambush(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            var b = Battle.GetBattle();
            if (b == null || b.defenderUnits == null || b.defenderUnits.Count <= 0) return;

            List<BattleUnit> enemyUnits = new List<BattleUnit>();
            enemyUnits = b.defenderUnits.FindAll(o => o.IsAlive() && o.GetWizardOwner() != ei.owner.GetEntity());

            foreach (var u in enemyUnits)
            {
                if (!(u is BattleUnit)) continue;
                u.AddEnchantment((Enchantment)ENCH.AMBUSH_UNIT, null, 1);
            }

            if (b.defender != null && b.defender.GetWizardOwner() != ei.owner.GetEntity())
            {
                b.defender.AddEnchantment((Enchantment)ENCH.AMBUSH_WIZARD, null, 1);
            }
        }
        static public void ESP_SeaHunter(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            var b = Battle.GetBattle();
            var owner = ei.owner;
            if (owner == null || b == null || b.landBattle) return;

            if (owner.GetEntity() == b.attacker.GetWizardOwner())
                b.attacker.AddEnchantment((Enchantment)ENCH.SEA_HUNTER_WIZARD, owner.GetEntity(), ei.countDown);
            else
                b.defender.AddEnchantment((Enchantment)ENCH.SEA_HUNTER_WIZARD, owner.GetEntity(), ei.countDown);
        }
        static public void ESP_PirateCurse(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            var b = Battle.GetBattle();
            if (b != null && e.fIntData != null) b.lastTurn = e.fIntData.ToInt();
            BattleHUD.Get()?.UpdateGeneralInfo();
        }
        static public void ESP_KlackonSupremacy(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            var castingBonus = 5;

            var b = Battle.GetBattle();
            var owner = ei.owner;
            if (owner == null || b == null || 
                b.attackerUnits == null || b.attackerUnits.Count == 0 ||
                b.defenderUnits == null || b.defenderUnits.Count == 0) return;

            if (owner.GetEntity() == b.attacker.GetWizardOwner())
                b.attacker.castingSkill += b.attackerUnits.FindAll(o => o.race == (Race)RACE.KLACKONS || o.GetDBName() == "HERO-PHYM").Count * castingBonus;
            else
                b.defender.castingSkill += b.defenderUnits.FindAll(o => o.race == (Race)RACE.KLACKONS || o.GetDBName() == "HERO-PHYM").Count * castingBonus;
        }

        #endregion

        #region Join / Disconnect Triggers
        static public void EJOIN_Example(IEnchantable source, IEnchantable otherUnit, EnchantmentInstance ei, object data)
        {
            MOM.Unit sUnit = source as MOM.Unit;
            MOM.Unit oUnit = otherUnit as MOM.Unit;
            if (sUnit == null || oUnit == null) return;

            Debug.Log("Unit " + sUnit.dbSource.dbName + " triggered join enchantment script event due to sharing now groupw with " + oUnit.dbSource.dbName);
        }
        static public void ELEAVE_Example(IEnchantable source, IEnchantable otherUnit, EnchantmentInstance ei, object data)
        {
            MOM.Unit sUnit = source as MOM.Unit;
            MOM.Unit oUnit = otherUnit as MOM.Unit;
            if (sUnit == null || oUnit == null) return;

            Debug.Log("Unit " + sUnit.dbSource.dbName + " triggered join enchantment script event due to disconnected from " + oUnit.dbSource.dbName);
        }
        static public void SJOIN_WindWalking(IEnchantable source, IEnchantable otherUnit, EnchantmentInstance ei, object data)
        {
            BaseUnit owner = source as BaseUnit;
            BaseUnit target = otherUnit as BaseUnit;
            if (owner == null || target == null) return;

            foreach (var script in ei.source.Get().scripts)
            {
                if (script.triggerType == EEnchantmentType.GroupChange)
                {
                    var enchToAdd = (Enchantment)DataBase.Get(script.stringData, false);

                    if (enchToAdd == null)
                        Debug.LogError("SJOIN_WindWalking StringData is not a ench.");
                    else
                    {
                        var ownerMove = owner.GetAttFinal(TAG.MOVEMENT_POINTS).ToInt();

                        var oEnchs = target.GetEnchantments();
                        var unitEnch = oEnchs.Find(o => o.source == enchToAdd);
                        if ((Enchantment)ENCH.WIND_WALKING_UNIT == enchToAdd &&
                            unitEnch == null)
                        {
                            if (owner == target) continue;
                            target.AddEnchantment(enchToAdd, owner, -1, ownerMove.ToString());
                        }
                        else
                        {
                            try
                            {
                                var orginalMove = Convert.ToInt32(unitEnch.parameters);
                                unitEnch.parameters = Math.Max(orginalMove, ownerMove).ToString();
                                target.GetAttributes().SetDirty();
                                //u.Get().group?.Get().UpdateMovementFlags();
                            }
                            catch
                            {
                            }
                        }
                    }
                }
            }
        }

        static public void SLEAVE_WindWalking(IEnchantable source, IEnchantable otherUnit, EnchantmentInstance ei, object data)
        {
            BaseUnit owner = source as BaseUnit;
            BaseUnit target = otherUnit as MOM.BaseUnit;
            List<BaseUnit> unitsInGroup = data as List<BaseUnit>;
            if (owner == null || target == null) return;

            foreach (var script in ei.source.Get().scripts)
            {
                if (script.triggerType == EEnchantmentType.GroupChange)
                {
                    var enchToRemove = (Enchantment)DataBase.Get(script.stringData, false);
                    if (enchToRemove == null)
                        Debug.LogError("SLEAVE_WindWalking StringData is not a ench.");
                    else
                    {
                        List<BaseUnit> otherBonusSource = null;
                        var bestMoveBonus = 0;
                        if (unitsInGroup != null && unitsInGroup.Count > 0)
                        {
                            foreach (var u in unitsInGroup)
                            {
                                if (owner == u) continue;
                                if (u.GetSkills().Contains((Skill)SKILL.WIND_WALKING) ||
                                    u.GetEnchantments().Find(o => o.source == (Enchantment)ENCH.WIND_WALKING_OWNER) != null )
                                {
                                    if (otherBonusSource == null)
                                    {
                                        otherBonusSource = new List<BaseUnit>();
                                    }
                                    otherBonusSource.Add(u);
                                }
                            }
                        }

                        if (otherBonusSource == null)
                        {
                            if (target.GetEnchantments().Find(o => o.source == enchToRemove) != null)
                            {
                                target.RemoveEnchantment(enchToRemove);
                            }
                        }
                        else
                        {
                            if (otherBonusSource.Count == 1)
                            {
                                if (otherBonusSource[0].GetEnchantments().Find(o => o.source == enchToRemove) != null)
                                {
                                    otherBonusSource[0].RemoveEnchantment(enchToRemove);
                                }
                            }

                            foreach (var u in otherBonusSource)
                            {
                                if (bestMoveBonus < u.GetAttFinal((Tag)TAG.MOVEMENT_POINTS) )
                                {
                                    bestMoveBonus = u.GetAttFinal((Tag)TAG.MOVEMENT_POINTS).ToInt();
                                }
                            }
                            var ench = target.GetEnchantments().Find(o => o.source == enchToRemove);
                            if(ench != null)
                            {
                                ench.parameters = bestMoveBonus.ToString();
                                target.GetAttributes().SetDirty();
                            }
                        }
                    }
                }
            }
        }static public void SJOIN_MassWaterWalking(IEnchantable source, IEnchantable otherUnit, EnchantmentInstance ei, object data)
        {
            BaseUnit owner = source as BaseUnit;
            BaseUnit target = otherUnit as BaseUnit;
            if (owner == null || target == null) return;

            foreach (var script in ei.source.Get().scripts)
            {
                if (script.triggerType == EEnchantmentType.GroupChange)
                {
                    var enchToAdd = (Enchantment)DataBase.Get(script.stringData, false);

                    if (enchToAdd == null)
                        Debug.LogError("SJOIN_MassWaterWalking StringData is not a ench.");
                    else
                    {
                        var oEnchs = target.GetEnchantments();
                        var unitEnch = oEnchs.Find(o => o.source == enchToAdd || o.source == ei.source);
                        if ((Enchantment)ENCH.MASS_WATER_WALKING_UNIT == enchToAdd &&
                            unitEnch == null)
                        {
                            if (owner == target) continue;
                            target.AddEnchantment(enchToAdd, owner, -1);
                        }
                    }
                }
            }
        }

        static public void SLEAVE_MassWaterWalking(IEnchantable source, IEnchantable otherUnit, EnchantmentInstance ei, object data)
        {
            BaseUnit owner = source as BaseUnit;
            BaseUnit target = otherUnit as MOM.BaseUnit;
            List<BaseUnit> unitsInGroup = data as List<BaseUnit>;
            if (owner == null || target == null) return;

            foreach (var script in ei.source.Get().scripts)
            {
                if (script.triggerType == EEnchantmentType.GroupChange)
                {
                    var enchToRemove = (Enchantment)DataBase.Get(script.stringData, false);
                    if (enchToRemove == null)
                        Debug.LogError("SLEAVE_WindWalking StringData is not a ench.");
                    else
                    {
                        List<BaseUnit> otherBonusSource = null;
                        if (unitsInGroup != null && unitsInGroup.Count > 0)
                        {
                            foreach (var u in unitsInGroup)
                            {
                                if (owner == u) continue;
                                if (u.GetSkills().Contains((Skill)SKILL.MASS_WATER_WALKING) ||
                                    u.GetEnchantments().Find(o => o.source == (Enchantment)ENCH.MASS_WATER_WALKING_OWNER) != null)
                                {
                                    if (otherBonusSource == null)
                                    {
                                        otherBonusSource = new List<BaseUnit>();
                                    }
                                    otherBonusSource.Add(u);
                                }
                            }
                        }

                        if (otherBonusSource == null)
                        {
                            if (target.GetEnchantments().Find(o => o.source == enchToRemove) != null)
                            {
                                target.RemoveEnchantment(enchToRemove);
                            }
                        }
                    }
                }
            }
        }
        #endregion
        #region Remote Trigger Filter
        /// <summary>
        /// Remote triggers cannot use attributes.GetFinal of the units
        /// </summary>
        /// <param name="source"></param>
        /// <param name="otherUnit"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        static public bool EFILTER_Race(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            var targetUnit = target as BaseUnit;
            if (targetUnit != null)
            {
                var battle = Battle.GetBattle();
                if (battle != null && ei != null && ei.owner != null)
                {
                    if (IsTownProtected(GameManager.GetWizard(ei.owner.ID), ei.source, battle)) return false;
                }

                if (e != null && targetUnit.race.dbName == e.stringData)
                {
                    return true;
                }
            }

            return false;
        }

        static public bool EFILTER_NotSettler(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            var targetUnit = target as BaseUnit;
            if (targetUnit != null)
            {
                var battle = Battle.GetBattle();
                if (battle != null && ei != null && ei.owner != null)
                {
                    if (IsTownProtected(GameManager.GetWizard(ei.owner.ID), ei.source, battle)) return false;
                }

                if (e != null && targetUnit.GetAttributes().GetBase((Tag)TAG.SETTLER_UNIT ) == 0)
                {
                    return true;
                }
            }

            return false;
        }

        static public bool EFILTER_NotFlyNotNonCorporeal(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            var targetUnit = target as BattleUnit;
            if (targetUnit != null)
            {
                var battle = Battle.GetBattle();
                if (battle != null && ei != null && ei.owner != null)
                {
                    if (IsTownProtected(EntityManager.GetEntity(ei.owner.ID) as ISpellCaster, ei.source, battle)) return false;
                }

                if (e != null && !targetUnit.nonCorporealMovement)
                {
                    return true;
                }
            }

            return false;
        }

        static public bool EFILTER_NotFullChaosDef(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            var targetUnit = target as BaseUnit;
            if (targetUnit != null)
            {
                var battle = Battle.GetBattle();
                if (battle != null && ei != null && ei.owner != null)
                {
                    if (IsTownProtected(GameManager.GetWizard(ei.owner.ID), ei.source, battle)) return false;
                }

                if (e != null && targetUnit.GetAttributes().GetBase((Tag)TAG.MAGIC_IMMUNITY) == 0 &&
                    targetUnit.GetAttributes().GetBase((Tag)TAG.RIGHTEOUSNESS) == 0)
                {
                    return true;
                }
            }

            return false;
        }

        static public bool EFILTER_NotFullDeathDef(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            var targetUnit = target as BaseUnit;
            if (targetUnit != null)
            {
                var battle = Battle.GetBattle();
                if (battle != null && ei != null && ei.owner != null)
                {
                    MOM.Reference r = ei.owner;
                    if(r != null)
                    {
                        if (r.Get<BattleUnit>() != null)
                        {
                            var bu = r.Get<BattleUnit>();
                            var w = bu.GetWizardOwner();
                            if (w != null && IsTownProtected(w, ei.source, battle)) return false;
                        }
                        else if (r.Get<PlayerWizard>() != null)
                        {
                            var w = r.Get<PlayerWizard>();                            
                            if (w != null && IsTownProtected(w, ei.source, battle)) return false;
                        }
                    }                    
                }

                if (e != null && targetUnit.GetAttributes().GetBase((Tag)TAG.MAGIC_IMMUNITY) == 0 &&
                    targetUnit.GetAttributes().GetBase((Tag)TAG.RIGHTEOUSNESS) == 0  &&
                    targetUnit.GetAttributes().GetBase((Tag)TAG.DEATH_IMMUNITY) == 0)
                {
                    return true;
                }
            }

            return false;
        }
        static public bool EFILTER_NotMagicImmunity(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            var targetUnit = target as BaseUnit;
            if (targetUnit != null)
            {
                var battle = Battle.GetBattle();
                if (battle != null && ei != null && ei.owner != null)
                {
                    if (IsTownProtected(GameManager.GetWizard(ei.owner.ID), ei.source, battle)) return false;
                }

                if (e != null && targetUnit.GetAttributes().GetBase((Tag)TAG.MAGIC_IMMUNITY) == 0)
                {
                    return true;
                }
            }

            return false;
        }
        static public bool EFILTER_RealmDeath(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            var targetUnit = target as BaseUnit;
            if (targetUnit != null)
            {
                var battle = Battle.GetBattle();
                if (battle != null && ei != null && ei.owner != null)
                {
                    if (IsTownProtected(GameManager.GetWizard(ei.owner.ID), ei.source, battle)) return false;
                }

                if (e != null && targetUnit.race == (Race)RACE.REALM_DEATH)
                {
                    return true;
                }
            }

            return false;
        }
        static public bool EFILTER_RealmChaos(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            var targetUnit = target as BaseUnit;
            if (targetUnit != null)
            {
                var battle = Battle.GetBattle();
                if (battle != null && ei != null && ei.owner != null)
                {
                    if (IsTownProtected(GameManager.GetWizard(ei.owner.ID), ei.source, battle)) return false;
                }

                if (e != null && targetUnit.race == (Race)RACE.REALM_CHAOS)
                {
                    return true;
                }
            }

            return false;
        }
        static public bool EFILTER_RealmNature(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            var targetUnit = target as BaseUnit;
            if (targetUnit != null)
            {
                var battle = Battle.GetBattle();
                if (battle != null && ei != null && ei.owner != null)
                {
                    if (IsTownProtected(GameManager.GetWizard(ei.owner.ID), ei.source, battle)) return false;
                }

                if (e != null && targetUnit.race == (Race)RACE.REALM_NATURE)
                {
                    return true;
                }
            }

            return false;
        }
        static public bool EFILTER_RealmSorcery(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            var targetUnit = target as BaseUnit;
            if (targetUnit != null)
            {
                var battle = Battle.GetBattle();
                if (battle != null && ei != null && ei.owner != null)
                {
                    if (IsTownProtected(GameManager.GetWizard(ei.owner.ID), ei.source, battle)) return false;
                }

                if (e != null && targetUnit.race == (Race)RACE.REALM_SORCERY)
                {
                    return true;
                }
            }

            return false;
        }
        static public bool EFILTER_RealmDeathNotMagicImmunity(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            var targetUnit = target as BaseUnit;
            if (targetUnit != null)
            {
                var battle = Battle.GetBattle();
                if (battle != null && ei != null && ei.owner != null)
                {
                    if (IsTownProtected(GameManager.GetWizard(ei.owner.ID), ei.source, battle)) return false;
                }

                if (e != null && targetUnit.GetAttributes().GetBase((Tag)TAG.MAGIC_IMMUNITY) == 0 &&
                    targetUnit.race == (Race)RACE.REALM_DEATH)
                {
                    return true;
                }
            }

            return false;
        }
        static public bool EFILTER_RealmLifeNotMagicImmunity(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            var targetUnit = target as BaseUnit;
            if (targetUnit != null)
            {
                var battle = Battle.GetBattle();
                if (battle != null && ei != null && ei.owner != null)
                {
                    if (IsTownProtected(GameManager.GetWizard(ei.owner.ID), ei.source, battle)) return false;
                }

                if (e != null && targetUnit.GetAttributes().GetBase((Tag)TAG.MAGIC_IMMUNITY) == 0 &&
                    targetUnit.race == (Race)RACE.REALM_LIFE)
                {
                    return true;
                }
            }

            return false;
        }
        static public bool EFILTER_OwnUnits(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            var targetUnit = target as BaseUnit;
            PlayerWizard caster = GetSpellCasterOwner(ei);

            if (targetUnit.GetWizardOwner() == caster)
            {
                var battle = Battle.GetBattle();
                if (battle != null && ei != null && ei.owner != null)
                {
                    if (IsTownProtected(GameManager.GetWizard(ei.owner.ID), ei.source, battle)) return false;
                }

                return true;
            }

            return false;
        }
        static public bool EFILTER_OwnUnitsNotInvisible(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            var targetUnit = target as BaseUnit;
            PlayerWizard caster = GetSpellCasterOwner(ei);

            if (targetUnit.GetWizardOwner() == caster)
            {
                var battle = Battle.GetBattle();
                if (battle != null && ei != null && ei.owner != null)
                {
                    if (IsTownProtected(GameManager.GetWizard(ei.owner.ID), ei.source, battle)) return false;
                    if (targetUnit.invisibilityProtection > 0) return false;
                }

                return true;
            }

            return false;
        }

        static public bool EFILTER_OwnUnitsNotFantastic(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            var targetUnit = target as BaseUnit;
            PlayerWizard caster = GetSpellCasterOwner(ei);

            if (targetUnit.GetWizardOwner() == caster && 
                targetUnit.GetAttributes().GetBase((Tag)TAG.FANTASTIC_CLASS) == 0)
            {
                var battle = Battle.GetBattle();
                if (battle != null && ei != null && ei.owner != null)
                {
                    if (IsTownProtected(GameManager.GetWizard(ei.owner.ID), ei.source, battle)) return false;
                }

                return true;
            }

            return false;
        }
        static public bool EFILTER_OwnFantasticUnit(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            var targetUnit = target as BaseUnit;
            PlayerWizard caster = GetSpellCasterOwner(ei);

            if (targetUnit != null && targetUnit.GetWizardOwner() == caster)
            {
                var battle = Battle.GetBattle();
                if (battle != null && ei != null && ei.owner != null)
                {
                    if (IsTownProtected(GameManager.GetWizard(ei.owner.ID), ei.source, battle)) return false;
                }

                if (e != null && targetUnit.GetAttributes().GetBase((Tag)TAG.FANTASTIC_CLASS) > 0)
                {
                    return true;
                }
            }

            return false;
        }
        static public bool EFILTER_OwnShipUnits(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            var targetUnit = target as BaseUnit;
            PlayerWizard caster = GetSpellCasterOwner(ei);

            if (targetUnit.GetWizardOwner() == caster &&
                targetUnit.attributes.GetBase(TAG.SHIP) > 0)
            {
                var battle = Battle.GetBattle();
                if (battle != null && ei != null && ei.owner != null)
                {
                    if (IsTownProtected(GameManager.GetWizard(ei.owner.ID), ei.source, battle)) return false;
                }

                return true;
            }

            return false;
        }
        static public bool EFILTER_OwnCanSwimUnits(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            var targetUnit = target as BaseUnit;
            PlayerWizard caster = GetSpellCasterOwner(ei);
            if (data is NetDictionary<DBReference<Tag>, FInt>)
            {
                NetDictionary<DBReference<Tag>, FInt> att = data as NetDictionary<DBReference<Tag>, FInt>;

                if (targetUnit.GetWizardOwner() == caster &&
                    att.GetFinal((Tag)TAG.CAN_SWIM) > FInt.ZERO || att.GetFinal((Tag)TAG.NON_CORPOREAL) > FInt.ZERO)
                {
                    var battle = Battle.GetBattle();
                    if (battle != null && ei != null && ei.owner != null)
                    {
                        if (IsTownProtected(GameManager.GetWizard(ei.owner.ID), ei.source, battle)) return false;
                    }

                    return true;
                }
            }

            return false;
        }
        static public bool EFILTER_OwnCanSwimUnitsWorld(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            var targetUnit = target as MOM.Unit;
            PlayerWizard caster = GetSpellCasterOwner(ei);
            if (data is NetDictionary<DBReference<Tag>, FInt> && targetUnit != null)
            {
                NetDictionary<DBReference<Tag>, FInt> att = data as NetDictionary<DBReference<Tag>, FInt>;

                if (targetUnit.GetWizardOwner() == caster &&
                    att.GetFinal((Tag)TAG.CAN_SWIM) > FInt.ZERO || att.GetFinal((Tag)TAG.NON_CORPOREAL) > FInt.ZERO)
                {
                    var battle = Battle.GetBattle();
                    if (battle != null && ei != null && ei.owner != null)
                    {
                        if (IsTownProtected(GameManager.GetWizard(ei.owner.ID), ei.source, battle)) return false;
                    }

                    return true;
                }
            }

            return false;
        }
        static public bool EFILTER_EnemyShipUnits(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            var targetUnit = target as BaseUnit;
            PlayerWizard caster = GetSpellCasterOwner(ei);

            if (targetUnit.GetWizardOwner() != caster &&
                targetUnit.attributes.GetBase(TAG.SHIP) > 0)
            {
                var battle = Battle.GetBattle();
                if (battle != null && ei != null && ei.owner != null)
                {
                    if (IsTownProtected(GameManager.GetWizard(ei.owner.ID), ei.source, battle)) return false;
                }

                return true;
            }

            return false;
        }
        static public bool EFILTER_EnemyUnits(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            var targetUnit = target as BaseUnit;
            PlayerWizard caster = GetSpellCasterOwner(ei);

            if (targetUnit.GetWizardOwner() != caster)
            {
                var battle = Battle.GetBattle();
                if (battle != null && ei != null && ei.owner != null)
                {
                    if (IsTownProtected(GameManager.GetWizard(ei.owner.ID), ei.source, battle)) return false;
                }

                return true;
            }

            return false;
        }
        static public bool EFILTER_IsFantasticUnit(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            var targetUnit = target as BaseUnit;
            if (targetUnit != null)
            {
                var battle = Battle.GetBattle();
                if (battle != null && ei != null && ei.owner != null)
                {
                    if (IsTownProtected(GameManager.GetWizard(ei.owner.ID), ei.source, battle)) return false;
                }

                if (e != null && targetUnit.GetAttributes().GetBase((Tag)TAG.FANTASTIC_CLASS) > 0)
                {
                    return true;
                }
            }

            return false;
        }
        static public bool EFILTER_IsNormalUnit(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            var targetUnit = target as BaseUnit;
            if (targetUnit != null)
            {
                var battle = Battle.GetBattle();
                if (battle != null && ei != null && ei.owner != null)
                {
                    if (IsTownProtected(GameManager.GetWizard(ei.owner.ID), ei.source, battle)) return false;
                }

                if (e != null && targetUnit.GetAttributes().GetBase((Tag)TAG.NORMAL_CLASS) > 0)
                {
                    return true;
                }
            }

            return false;
        }
        static public bool EFILTER_IsChaosFantasticUnit(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            var targetUnit = target as BaseUnit;
            if (targetUnit != null)
            {
                var battle = Battle.GetBattle();
                if (battle != null && ei != null && ei.owner != null)
                {
                    if (IsTownProtected(GameManager.GetWizard(ei.owner.ID), ei.source, battle)) return false;
                }

                if (e != null && targetUnit.GetAttributes().GetBase((Tag)TAG.FANTASTIC_CLASS) > 0
                    && targetUnit.race == (Race)RACE.REALM_CHAOS)
                {
                    return true;
                }
            }

            return false;
        }
        static public bool EFILTER_RealmLifeOrDeath(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            var targetUnit = target as BaseUnit;
            var battle = Battle.GetBattle();
            if (battle != null && ei != null && ei.owner != null)
            {
                if (IsTownProtected(GameManager.GetWizard(ei.owner.ID), ei.source, battle)) return false;
            }

            if (targetUnit.race == (Race)RACE.REALM_LIFE)
            {
                return true;
            }
            if (targetUnit.race == (Race)RACE.REALM_DEATH)
            {
                return true;
            }

            return false;
        }
        static public bool EFILTER_SpellWard(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            var targetUnit = target as BaseUnit;
            var uPosition = targetUnit.GetPosition();
            var loc = GameManager.Get().GetLocationAt(uPosition);
            List<EnchantmentInstance> ench = new List<EnchantmentInstance>();

            var battle = Battle.GetBattle();
            if (battle != null && ei != null && ei.owner != null)
            {
                if (IsTownProtected(GameManager.GetWizard(ei.owner.ID), ei.source, battle)) return false;
            }

            if (loc != null)
            {
                ench = loc.GetEnchantments();
            }
            else
            {
                var b = Battle.GetBattle();
                if (b == null) return false;
                ench = b.GetEnchantments();
            }
            if(targetUnit.GetWizardOwner() != null && ei.owner != null)
                if (targetUnit.GetWizardOwner().ID == ei.owner.ID) return false;

            if (e != null 
                && ench.Find(o => o.source.Get() == (Enchantment)ENCH.SPELL_WARD_NATURE) != null
                && targetUnit.GetAttributes().GetBase((Tag)TAG.FANTASTIC_CLASS) > 0
                && targetUnit.race == (Race)RACE.REALM_NATURE)
            {
                return true;
            }
            else if (e != null
                && ench.Find(o => o.source.Get() == (Enchantment)ENCH.SPELL_WARD_LIFE) != null
                && targetUnit.GetAttributes().GetBase((Tag)TAG.FANTASTIC_CLASS) > 0
                && targetUnit.race == (Race)RACE.REALM_LIFE)
            {
                return true;
            }
            else if (e != null
                && ench.Find(o => o.source.Get() == (Enchantment)ENCH.SPELL_WARD_CHAOS) != null
                && targetUnit.GetAttributes().GetBase((Tag)TAG.FANTASTIC_CLASS) > 0
                && targetUnit.race == (Race)RACE.REALM_CHAOS)
            {
                return true;
            }
            else if (e != null
                && ench.Find(o => o.source.Get() == (Enchantment)ENCH.SPELL_WARD_DEATH) != null
                && targetUnit.GetAttributes().GetBase((Tag)TAG.FANTASTIC_CLASS) > 0
                && targetUnit.race == (Race)RACE.REALM_DEATH)
            {
                return true;
            }
            else if (e != null
                && ench.Find(o => o.source.Get() == (Enchantment)ENCH.SPELL_WARD_SORCERY) != null
                && targetUnit.GetAttributes().GetBase((Tag)TAG.FANTASTIC_CLASS) > 0
                && targetUnit.race == (Race)RACE.REALM_SORCERY)
            {
                return true;
            }

            return false;
        }
        static public bool EFILTER_OwnHeroUnit(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            var targetUnit = target as MOM.Unit;
            if (targetUnit != null)
            {
                var battle = Battle.GetBattle();
                if (battle != null && ei != null && ei.owner != null)
                {
                    if (IsTownProtected(GameManager.GetWizard(ei.owner.ID), ei.source, battle)) return false;
                }

                if (e != null && targetUnit.GetWizardOwner() == ei.owner?.GetEntity() &&
                targetUnit.IsHero())
                {
                    return true;
                }
            }

            return false;
        }
        static public bool EFILTER_OwnHeroNonFantasyUnit(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            var targetUnit = target as MOM.Unit;
            if (targetUnit != null)
            {
                var battle = Battle.GetBattle();
                if (battle != null && ei != null && ei.owner != null)
                {
                    if (IsTownProtected(GameManager.GetWizard(ei.owner.ID), ei.source, battle)) return false;
                }

                if (e != null && targetUnit.GetWizardOwner() == ei.owner?.GetEntity() &&
                targetUnit.IsHero() && targetUnit.GetAttributes().GetBase((Tag)TAG.FANTASTIC_CLASS) <= 0)
                {
                    return true;
                }
            }

            return false;
        }
        static public bool EFILTER_NeutralRealmTechAndSoultrapped(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            var targetUnit = target as BaseUnit;
            if (targetUnit != null)
            {
                var battle = Battle.GetBattle();
                if (battle != null && ei != null && ei.owner != null)
                {
                    if (IsTownProtected(GameManager.GetWizard(ei.owner.ID), ei.source, battle)) return false;
                }

                if (e != null && targetUnit.GetWizardOwner() == null && (targetUnit.race == (Race)RACE.REALM_TECH || targetUnit.race == (Race)RACE.SOULTRAPPED))
                {
                    return true;
                }
            }

            return false;
        }
        static public bool EFILTER_OwnUnitsMainRace(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            var targetUnit = target as BaseUnit;
            var owner = ei.owner;
            if (owner == null || !(owner.GetEntity() is PlayerWizard)) return false;
            var wiz = owner.GetEntity() as PlayerWizard;

            if (targetUnit.GetWizardOwner() == wiz && wiz.mainRace == targetUnit.race)
            {
                return true;
            }

            return false;
        }
        static public bool EFILTER_AnyUnitWithNoSwimFly(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            var targetUnit = target as BattleUnit;

            if (targetUnit != null && targetUnit.GetAttributes().GetBase((Tag)TAG.CAN_SWIM) <= 0 && targetUnit.GetAttributes().GetBase((Tag)TAG.CAN_FLY) <= 0)
            {
                return true;
            }

            return false;
        }
        static public bool EFILTER_Attacker(IEnchantable target, EnchantmentScript e, EnchantmentInstance ei, object data)
        {
            var targetUnit = target as BattleUnit;
            var battle = Battle.GetBattle();

            if (battle != null && ei != null && targetUnit != null)
            {
                return targetUnit.attackingSide;
            }

            return false;
        }
        #endregion

        #region Requirement Scripts
        static public bool REQ_NoFantasticUnit(IEnchantable owner)
        {
            if(owner is IAttributable)
            {
                var att = owner as IAttributable;
                return att.GetAttFinal(TAG.FANTASTIC_CLASS) < FInt.ONE;
            }

            return false;
        }
        #endregion

        #region Utility
        private static FInt ResitModFromEnch(BattleUnit attackerBu, BattleUnit targetBu, Spell spell)
        {
            return GameScript.SpellScripts.ResistModFromEnch(attackerBu, targetBu, spell);
        }
        private static FInt ResitModFromEnch(MOM.Unit attackerU, MOM.Unit targetU, Spell spell)
        {
            return GameScript.SpellScripts.ResistModFromEnch(attackerU, targetU, spell);
        }

        private static Dictionary<ERealm, TAG> realmToTag = new Dictionary<ERealm, TAG>
                                                         {
                                                             { ERealm.Life, TAG.LIFE_MAGIC_BOOK },
                                                             { ERealm.Death, TAG.DEATH_MAGIC_BOOK },
                                                             { ERealm.Nature, TAG.NATURE_MAGIC_BOOK },
                                                             { ERealm.Chaos, TAG.CHAOS_MAGIC_BOOK },
                                                             { ERealm.Sorcery, TAG.SORCERY_MAGIC_BOOK }
                                                         };

        static bool IsTownProtected(PlayerWizard spellcaster, Enchantment ench, TownLocation town)
        {
            if (spellcaster != null && ench != null && town != null )
            {
                if (spellcaster.ID != town.GetOwnerID() &&
                    (ench.realm == ERealm.Nature && town.isNatureProtected > 0 ||
                    ench.realm == ERealm.Sorcery && town.isSorceryProtected > 0 ||
                    ench.realm == ERealm.Chaos && town.isChaosProtected > 0 ||
                    ench.realm == ERealm.Life && town.isLifeProtected > 0 ||
                    ench.realm == ERealm.Death && town.isDeathProtected > 0))
                {
                    return true;
                }
            }

            return false;
        }
        static bool IsTownProtected(ISpellCaster spellcaster, Enchantment ench, Battle battle)
        {
            if (battle != null && spellcaster != null && ench != null)
            {
                BattlePlayer targetOwner;
                if (battle.attacker.wizard == spellcaster)
                    targetOwner = battle.defender;
                else
                    targetOwner = battle.attacker;

                if (spellcaster.GetWizardOwnerID() != targetOwner.GetID() &&
                    (ench.realm == ERealm.Nature && targetOwner.isNatureProtected > 0 ||
                    ench.realm == ERealm.Sorcery && targetOwner.isSorceryProtected > 0 ||
                    ench.realm == ERealm.Chaos && targetOwner.isChaosProtected > 0 ||
                    ench.realm == ERealm.Life && targetOwner.isLifeProtected > 0 ||
                    ench.realm == ERealm.Death && targetOwner.isDeathProtected > 0))
                {
                    return true;
                }
            }

            return false;
        }
        private static void RemoveSpellWardEnfeeblingHex(ref MOM.TownLocation town)
        {
            var townEnch = town.GetEnchantments();
            var find = townEnch.Find(o => o.source == (Enchantment)ENCH.SPELL_WARD_DEATH ||
            o.source == (Enchantment)ENCH.SPELL_WARD_LIFE || o.source == (Enchantment)ENCH.SPELL_WARD_CHAOS ||
            o.source == (Enchantment)ENCH.SPELL_WARD_SORCERY || o.source == (Enchantment)ENCH.SPELL_WARD_NATURE);
            if (find == null)
            {
                town.RemoveEnchantment((Enchantment)ENCH.SPELL_WARD_ENFEEBLING_HEX);
            }
        }
        static PlayerWizard GetSpellCasterOwner(EnchantmentInstance ei)
        {
            PlayerWizard caster = null;
            if (ei != null)
            {
                Entity owner = ei.owner?.GetEntity();
                if (owner is PlayerWizard)
                {
                    caster = owner as PlayerWizard;
                }
                else if (owner is BattleUnit)
                {
                    caster = (owner as BattleUnit).GetWizardOwner();
                }
                else if ((owner as IEnchantable) is BattlePlayer)
                {
                    caster = ((owner as IEnchantable) as BattlePlayer).GetWizardOwner();
                }
                else
                {
                    Debug.LogWarning("GetSpellCasterOwner cannot recognize enchantment owner");
                }
            }
            else
            {
                Debug.LogWarning("GetSpellCasterOwner enchantment instance is null");
            }
            return caster;
        }
        public static int GetSpellCasterOwnerID(EnchantmentInstance ei)
        {
            int casterID = -1;
            if (ei != null)
            {
                Entity owner = ei.owner?.GetEntity();
                if (owner is PlayerWizard)
                {
                    casterID = (owner as PlayerWizard).GetID();
                }
                else if (owner is BattleUnit)
                {
                    casterID = (owner as BattleUnit).GetWizardOwnerID();
                }
                else if ((owner as IEnchantable) is BattlePlayer)
                {
                    casterID = ((owner as IEnchantable) as BattlePlayer).GetID();
                }
                else
                {
                    Debug.LogWarning("GetSpellCasterOwnerID cannot recognize enchantment owner");
                }
            }
            else
            {
                Debug.LogWarning("GetSpellCasterOwnerID enchantment instance is null");
            }
            return casterID;
        }
        #endregion
    }
}
#endif