#if !USE_DEBUG_SCRIPT || !UNITY_EDITOR

using DBDef;
using DBEnum;
using MHUtils;
using MHUtils.UI;
using MOM;
using System;
using System.Collections.Generic;
using UnityEngine;
using WorldCode;

namespace GameScript
{
    public class SpellScripts : ScriptBase
    {
        static MHRandom random = MHRandom.Get();

        #region Spell Targetting (STAR)       

        static public bool STAR_Friendly_Unit(SpellCastData data, object target, Spell spell)
        {
            if (target is MOM.Unit)
            {
                var u = target as MOM.Unit;

                if (data.GetWizardID() == u.group.Get().GetOwnerID())
                {
                    if (EnchAlreadyOnObject(data.caster, spell, u)) return false;

                    var location = u.group.Get().GetLocationHostSmart();
                    if (location != null && location is TownLocation)
                    {
                        if (!IsTownProtected(data.GetWizardID(), spell, location as TownLocation)) return true;
                    }
                    else
                    {
                        return true;
                    }

                }
            }
            else if (target is BattleUnit)
            {
                var bu = target as BattleUnit;

                if (data.GetWizardID() == bu.ownerID)
                {
                    if (EnchAlreadyOnObject(data.caster, spell, bu)) return false;

                    if (!IsTownProtected(data.GetWizardID(), spell, data.battle)) return true;
                }
            }
            return false;
        }
        static public bool STAR_CloakOfFear(SpellCastData data, object target, Spell spell)
        {
            if (STAR_Friendly_Unit(data, target, spell))
            {
                var u = target as MOM.Unit;
                var b = target as MOM.BattleUnit;
                if (u != null && u.GetAttFinal((Tag)TAG.CAUSE_FEAR) < 1)
                {
                    return true;
                }
                else if (b != null && b.GetAttFinal((Tag)TAG.CAUSE_FEAR) < 1)
                {
                    return true;
                }
            }
            
            return false;
        }

        static public bool STAR_FriendlyNormal_Unit(SpellCastData data, object target, Spell spell)
        {
            //var w = data.GetPlayerWizard();
            if (target is MOM.Unit)
            {
                var u = target as MOM.Unit;

                if (data.GetWizardID() == u.group.Get().GetOwnerID() && u.attributes.Contains(TAG.NORMAL_CLASS))
                {
                    if (EnchAlreadyOnObject(data.caster, spell, u)) return false;

                    var location = u.group.Get().GetLocationHostSmart();
                    if (location != null && location is TownLocation)
                    {
                        if (!IsTownProtected(data.GetWizardID(), spell, location as TownLocation)) return true;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            else if (target is BattleUnit)
            {
                var bu = target as BattleUnit;

                if (data.GetWizardID() == bu.ownerID && bu.attributes.Contains(TAG.NORMAL_CLASS))
                {
                    if (EnchAlreadyOnObject(data.caster, spell, bu)) return false;

                    if (!IsTownProtected(data.GetWizardID(), spell, data.battle)) return true;
                }
            }
            return false;
        }
        static public bool STAR_FriendlyNormalUnitNormalRange(SpellCastData data, object target, Spell spell)
        {
            //var w = data.GetPlayerWizard();
            if (target is MOM.Unit)
            {
                var u = target as MOM.Unit;

                if (data.GetWizardID() == u.group.Get().GetOwnerID() && u.attributes.Contains(TAG.NORMAL_CLASS)
                    && u.attributes.DoesNotContains((Tag)TAG.MAGIC_RANGE) && u.attributes.Contains(TAG.RANGED_UNIT))
                {
                    if (EnchAlreadyOnObject(data.caster, spell, u)) return false;

                    var location = u.group.Get().GetLocationHostSmart();
                    if (location != null && location is TownLocation)
                    {
                        if (!IsTownProtected(data.GetWizardID(), spell, location as TownLocation)) return true;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            else if (target is BattleUnit)
            {
                var bu = target as BattleUnit;

                if (data.GetWizardID() == bu.ownerID && bu.attributes.Contains(TAG.NORMAL_CLASS)
                    && bu.attributes.DoesNotContains((Tag)TAG.MAGIC_RANGE) && bu.attributes.Contains(TAG.RANGED_UNIT))
                {
                    if (EnchAlreadyOnObject(data.caster, spell, bu)) return false;

                    if (!IsTownProtected(data.GetWizardID(), spell, data.battle)) return true;
                }
            }
            return false;
        }
        static public bool STAR_FriendlyNormalUnitOrHeroNormalRange(SpellCastData data, object target, Spell spell)
        {
            //var w = data.GetPlayerWizard();
            if (target is MOM.Unit)
            {
                var u = target as MOM.Unit;

                if (data.GetWizardID() == u.group.Get().GetOwnerID() && (u.attributes.Contains(TAG.NORMAL_CLASS) 
                    || u.attributes.Contains(TAG.HERO_CLASS))
                    && u.attributes.DoesNotContains((Tag)TAG.MAGIC_RANGE) 
                    && (u.attributes.Contains(TAG.RANGED_UNIT) || u.attributes.Contains(TAG.NORMAL_RANGE)))
                {
                    if (EnchAlreadyOnObject(data.caster, spell, u)) return false;

                    var location = u.group.Get().GetLocationHostSmart();
                    if (location != null && location is TownLocation)
                    {
                        if (!IsTownProtected(data.GetWizardID(), spell, location as TownLocation)) return true;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            else if (target is BattleUnit)
            {
                var bu = target as BattleUnit;

                if (data.GetWizardID() == bu.ownerID && (bu.attributes.Contains(TAG.NORMAL_CLASS) || bu.attributes.Contains(TAG.HERO_CLASS))
                    && bu.attributes.DoesNotContains((Tag)TAG.MAGIC_RANGE) 
                    && (bu.attributes.Contains(TAG.RANGED_UNIT) || bu.attributes.Contains(TAG.NORMAL_RANGE)))
                {
                    if (EnchAlreadyOnObject(data.caster, spell, bu)) return false;

                    if (!IsTownProtected(data.GetWizardID(), spell, data.battle)) return true;
                }
            }
            return false;
        }
        static public bool STAR_FriendlyNonFantastic_Unit(SpellCastData data, object target, Spell spell)
        {
            //var w = data.GetPlayerWizard();
            if (target is MOM.Unit)
            {
                var u = target as MOM.Unit;

                if (data.GetWizardID() == u.group.Get().GetOwnerID() && u.attributes.DoesNotContains((Tag)TAG.FANTASTIC_CLASS))
                {
                    if (EnchAlreadyOnObject(data.caster, spell, u)) return false;

                    var location = u.group.Get().GetLocationHostSmart();
                    if (location != null && location is TownLocation)
                    {
                        if (!IsTownProtected(data.GetWizardID(), spell, location as TownLocation)) return true;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            else if (target is BattleUnit)
            {
                var bu = target as BattleUnit;

                if (data.GetWizardID() == bu.ownerID && (bu.attributes.DoesNotContains((Tag)TAG.FANTASTIC_CLASS)))
                {
                    if (EnchAlreadyOnObject(data.caster, spell, bu)) return false;

                    if (!IsTownProtected(data.GetWizardID(), spell, data.battle)) return true;
                }
            }
            return false;
        }
        static public bool STAR_HolyWeapon(SpellCastData data, object target, Spell spell)
        {
            var frienldyNonFantasticUnit = STAR_FriendlyNonFantastic_Unit(data, target, spell);

            var t = target as MOM.Unit;
            if (t != null )
            {
                var owner = t.GetWizardOwner();
                if (owner != null &&
                    owner.GetEnchantments().Find(o => o.source == (Enchantment)ENCH.HOLY_ARMS) != null) return false;
            }
            var b = target as MOM.BattleUnit;
            if (b != null)
            {
                var owner = b.GetWizardOwner();
                if (owner != null &&
                    owner.GetEnchantments().Find(o => o.source == (Enchantment)ENCH.HOLY_ARMS) != null) return false;
            }

            if (frienldyNonFantasticUnit) 
                return true;
            else
                return false;
        }
        static public bool STAR_FriendlyNormalOrUndead_Unit(SpellCastData data, object target, Spell spell)
        {
            //var w = data.GetPlayerWizard();
            if (target is MOM.Unit)
            {
                var u = target as MOM.Unit;

                if (data.GetWizardID() == u.group.Get().GetOwnerID() && (u.attributes.Contains(TAG.NORMAL_CLASS) ||
                    u.attributes.Contains(TAG.HERO_CLASS) || u.race == (Race)RACE.REALM_DEATH))
                {
                    if (EnchAlreadyOnObject(data.caster, spell, u)) return false;

                    var location = u.group.Get().GetLocationHostSmart();
                    if (location != null && location is TownLocation)
                    {
                        if (!IsTownProtected(data.GetWizardID(), spell, location as TownLocation)) return true;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            else if (target is BattleUnit)
            {
                var bu = target as BattleUnit;

                if (data.GetWizardID() == bu.ownerID && (bu.attributes.Contains(TAG.NORMAL_CLASS) ||
                    bu.attributes.Contains(TAG.HERO_CLASS) || bu.race == RACE.REALM_DEATH))
                {
                    if (EnchAlreadyOnObject(data.caster, spell, bu)) return false;

                    if (!IsTownProtected(data.GetWizardID(), spell, data.battle)) return true;
                }
            }
            return false;
        }
        static public bool STAR_Inviolability(SpellCastData data, object target, Spell spell)
        {
            if (STAR_Friendly_Unit(data, target, spell))
            {
                if(data.caster is BattleUnit)
                {
                    var bu = data.caster as BattleUnit;
                    var hp = bu.GetTotalHealth();
                    var sacrifice = spell.fIntData[0];

                    if (hp > sacrifice) 
                    {
                        return true;
                    }
                    else
                    {
                        if (data.GetWizardID() == PlayerWizard.HumanID())
                            PopupGeneral.OpenPopup(null, "UI_TARGETING_FAILED", "UI_NOT_ENOUGHT_HP", "UI_OK");
                        return false;
                    }
                }
            }
            return false;
        }
        static public bool STAR_MassPiercing(SpellCastData data, object target, Spell spell)
        {
            if (STAR_OwnBattlePlayer(data, target, spell))
            {
                if (data.caster is BattleUnit)
                {
                    var bu = data.caster as BattleUnit;
                    var hp = bu.GetTotalHealth();
                    var sacrifice = spell.fIntData[0];

                    if (hp > sacrifice)
                    {
                        return true;
                    }
                    else
                    {
                        if (data.GetWizardID() == PlayerWizard.HumanID())
                            PopupGeneral.OpenPopup(null, "UI_TARGETING_FAILED", "UI_NOT_ENOUGHT_HP", "UI_OK");
                        return false;
                    }
                }
            }
            return false;
        }

        static public bool STAR_BlackChannels(SpellCastData data, object target, Spell spell)
        {
            if (target is MOM.Unit)
            {
                var u = target as MOM.Unit;

                var skillList = u.GetSkills();

                if (EnchAlreadyOnObject(data.caster, spell, u)) return false;
                if (skillList.Contains((Skill)SKILL.BLACK_CHANNELS) ||
                    skillList.Contains((Skill)SKILL.CHAOS_CHANNELS1) ||
                    skillList.Contains((Skill)SKILL.CHAOS_CHANNELS2) ||
                    skillList.Contains((Skill)SKILL.CHAOS_CHANNELS3)) return false;

                if (data.GetWizardID() == u.group.Get().GetOwnerID() && (u.attributes.Contains(TAG.NORMAL_CLASS) ||
                    u.attributes.Contains(TAG.HERO_CLASS) || u.attributes.Contains(TAG.REANIMATED)))
                {
                    var location = u.group.Get().GetLocationHostSmart();
                    if (location != null && location is TownLocation)
                    {
                        if (!IsTownProtected(data.GetWizardID(), spell, location as TownLocation)) return true;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        static public bool STAR_FriendlyNoUndead_Unit(SpellCastData data, object target, Spell spell)
        {
            //var w = data.GetPlayerWizard();
            if (target is MOM.Unit)
            {
                var u = target as MOM.Unit;

                var location = u.group.Get().GetLocationHostSmart();
                if (data.GetWizardID() == u.group.Get().GetOwnerID() && u.race != (Race)RACE.REALM_DEATH)
                {
                    if (EnchAlreadyOnObject(data.caster, spell, u)) return false;

                    if (location != null && location is TownLocation)
                    {
                        if (!IsTownProtected(data.GetWizardID(), spell, location as TownLocation)) return true;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            if (target is BattleUnit)
            {
                var bu = target as BattleUnit;

                if (data.GetWizardID() == bu.ownerID && bu.race != (Race)RACE.REALM_DEATH)
                {
                    if (EnchAlreadyOnObject(data.caster, spell, bu)) return false;

                    if (!IsTownProtected(data.GetWizardID(), spell, data.battle)) return true;
                }
            }
            return false;
        }

        static public bool STAR_WaterWalking(SpellCastData data, object target, Spell spell)
        {
            if (target is MOM.Unit)
            {
                var u = target as MOM.Unit;

                if (data.GetWizardID() == u.group.Get().GetOwnerID())
                {
                    if (EnchAlreadyOnObject(data.caster, spell, u)) return false;

                    var location = u.group.Get().GetLocationHostSmart();
                    if (location != null && location is TownLocation)
                    {
                        if (!IsTownProtected(data.GetWizardID(), spell, location as TownLocation))
                        {
                            if (!u.GetAttributes().Contains(TAG.CAN_SWIM) && !u.GetAttributes().Contains(TAG.CAN_FLY))
                            {
                                return true;
                            }
                        }
                    }
                    else
                    {
                        if (!u.GetAttributes().Contains(TAG.CAN_SWIM) && !u.GetAttributes().Contains(TAG.CAN_FLY))
                        {
                            return true;
                        }
                    }

                }
            }
            return false;
        }
        static public bool STAR_Endurance(SpellCastData data, object target, Spell spell)
        {
            if (target is MOM.Unit)
            {
                var u = target as MOM.Unit;

                if (data.GetWizardID() == u.group.Get().GetOwnerID())
                {
                    if (EnchAlreadyOnObject(data.caster, spell, u)) return false;
                    if (u.GetSkills().Contains((Skill)SKILL.ITEM_ENDURANCE)) return false;

                    var location = u.group.Get().GetLocationHostSmart();
                    if (location != null && location is TownLocation)
                    {
                        if (!IsTownProtected(data.GetWizardID(), spell, location as TownLocation)) return true;
                    }
                    else
                    {
                        return true;
                    }

                }
            }

            return false;
        }
        static public bool STAR_Enemy_Unit(SpellCastData data, object target, Spell spell)
        {
            //var w = data.GetPlayerWizard();
            if (target is MOM.Unit)
            {
                var u = target as MOM.Unit;

                if (data.GetWizardID() != u.group.Get().GetOwnerID())
                {
                    if (EnchAlreadyOnObject(data.caster, spell, u)) return false;

                    var location = u.group.Get().GetLocationHostSmart();
                    if (location != null && location is TownLocation)
                    {
                        if (!IsTownProtected(data.GetWizardID(), spell, location as TownLocation)) return true;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            else if (target is BattleUnit)
            {
                var g = target as BattleUnit;

                if (data.GetWizardID() != g.ownerID)
                {
                    if (!g.currentlyVisible) return false;
                    if (EnchAlreadyOnObject(data.caster, spell, g)) return false;

                    if (!IsTownProtected(data.GetWizardID(), spell, data.battle)) return true;
                }
            }
            return false;
        }

        static public bool STAR_EnemyNoDeathImmus_Unit(SpellCastData data, object target, Spell spell)
        {
            //var w = data.GetPlayerWizard();
            if (target is MOM.Unit)
            {
                var u = target as MOM.Unit;

                if (data.GetWizardID() != u.group.Get().GetOwnerID()
                    && u.attributes.DoesNotContains((Tag)TAG.MAGIC_IMMUNITY)
                    && u.attributes.DoesNotContains((Tag)TAG.DEATH_IMMUNITY)
                    && u.attributes.DoesNotContains((Tag)TAG.RIGHTEOUSNESS))
                {
                    if (EnchAlreadyOnObject(data.caster, spell, u)) return false;

                    var location = u.group.Get().GetLocationHostSmart();
                    if (location != null && location is TownLocation)
                    {
                        if (!IsTownProtected(data.GetWizardID(), spell, location as TownLocation)) return true;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            else if (target is BattleUnit)
            {
                var bu = target as BattleUnit;

                if (data.GetWizardID() != bu.ownerID
                    && bu.attributes.DoesNotContains((Tag)TAG.MAGIC_IMMUNITY)
                    && bu.attributes.DoesNotContains((Tag)TAG.DEATH_IMMUNITY)
                    && bu.attributes.DoesNotContains((Tag)TAG.RIGHTEOUSNESS))
                {
                    if (!bu.currentlyVisible) return false;
                    if (EnchAlreadyOnObject(data.caster, spell, bu)) return false;

                    if (!IsTownProtected(data.GetWizardID(), spell, data.battle)) return true;
                }
            }
            return false;
        }

        static public bool STAR_EnemyNoMagicImmu_Unit(SpellCastData data, object target, Spell spell)
        {
            //var w = data.GetPlayerWizard();
            if (target is MOM.Unit)
            {
                var u = target as MOM.Unit;

                if (data.GetWizardID() != u.group.Get().GetOwnerID()
                    && u.attributes.DoesNotContains((Tag)TAG.MAGIC_IMMUNITY))
                {
                    if (EnchAlreadyOnObject(data.caster, spell, u)) return false;

                    var location = u.group.Get().GetLocationHostSmart();
                    if (location != null && location is TownLocation)
                    {
                        if (!IsTownProtected(data.GetWizardID(), spell, location as TownLocation)) return true;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            else if (target is BattleUnit)
            {
                var bu = target as BattleUnit;

                if (data.GetWizardID() != bu.ownerID
                    && bu.attributes.DoesNotContains((Tag)TAG.MAGIC_IMMUNITY))
                {
                    if (!bu.currentlyVisible) return false;
                    if (EnchAlreadyOnObject(data.caster, spell, bu)) return false;

                    if (!IsTownProtected(data.GetWizardID(), spell, data.battle)) return true;
                }
            }
            return false;
        }

        static public bool STAR_EnemyNoNonCorporeal_Unit(SpellCastData data, object target, Spell spell)
        {
            //var w = data.GetPlayerWizard();
            if (target is MOM.Unit)
            {
                var u = target as MOM.Unit;

                if (data.GetWizardID() != u.group.Get().GetOwnerID() &&
                    u.attributes.DoesNotContains((Tag)TAG.NON_CORPOREAL))
                {
                    if (EnchAlreadyOnObject(data.caster, spell, u)) return false;

                    var location = u.group.Get().GetLocationHostSmart();
                    if (location != null && location is TownLocation)
                    {
                        if (!IsTownProtected(data.GetWizardID(), spell, location as TownLocation)) return true;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            else if (target is BattleUnit)
            {
                var bu = target as BattleUnit;

                if (data.GetWizardID() != bu.ownerID &&
                    bu.attributes.DoesNotContains((Tag)TAG.NON_CORPOREAL))
                {
                    if (!bu.currentlyVisible) return false;
                    if (EnchAlreadyOnObject(data.caster, spell, bu)) return false;

                    if (!IsTownProtected(data.GetWizardID(), spell, data.battle)) return true;
                }
            }
            return false;
        }

        static public bool STAR_EnemyNoMagicImmuNoNonCorporeal_Unit(SpellCastData data, object target, Spell spell)
        {
            //var w = data.GetPlayerWizard();
            if (target is MOM.Unit)
            {
                var u = target as MOM.Unit;

                if (data.GetWizardID() != u.group.Get().GetOwnerID()
                    && (u.attributes.DoesNotContains((Tag)TAG.MAGIC_IMMUNITY) &&
                    u.attributes.DoesNotContains((Tag)TAG.NON_CORPOREAL)))
                {
                    if (EnchAlreadyOnObject(data.caster, spell, u)) return false;

                    var location = u.group.Get().GetLocationHostSmart();
                    if (location != null && location is TownLocation)
                    {
                        if (!IsTownProtected(data.GetWizardID(), spell, location as TownLocation)) return true;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            else if (target is BattleUnit)
            {
                var bu = target as BattleUnit;

                if (data.GetWizardID() != bu.ownerID
                    && (bu.attributes.DoesNotContains((Tag)TAG.MAGIC_IMMUNITY) &&
                    bu.attributes.DoesNotContains((Tag)TAG.NON_CORPOREAL)))
                {
                    if (!bu.currentlyVisible) return false;
                    if (EnchAlreadyOnObject(data.caster, spell, bu)) return false;

                    if (!IsTownProtected(data.GetWizardID(), spell, data.battle)) return true;
                }
            }
            return false;
        }

        static public bool STAR_EnemyNoStoneImmu_Unit(SpellCastData data, object target, Spell spell)
        {
            //var w = data.GetPlayerWizard();
            if (target is MOM.Unit)
            {
                var u = target as MOM.Unit;

                if (data.GetWizardID() != u.group.Get().GetOwnerID()
                    && u.attributes.DoesNotContains((Tag)TAG.MAGIC_IMMUNITY)
                    && u.attributes.DoesNotContains((Tag)TAG.STONING_IMMUNITY))
                {
                    if (EnchAlreadyOnObject(data.caster, spell, u)) return false;

                    var location = u.group.Get().GetLocationHostSmart();
                    if (location != null && location is TownLocation)
                    {
                        if (!IsTownProtected(data.GetWizardID(), spell, location as TownLocation)) return true;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            else if (target is BattleUnit)
            {
                var g = target as BattleUnit;

                if (data.GetWizardID() != g.ownerID
                    && g.attributes.DoesNotContains((Tag)TAG.MAGIC_IMMUNITY)
                    && g.attributes.DoesNotContains((Tag)TAG.STONING_IMMUNITY))
                {
                    if (!g.currentlyVisible) return false;
                    if (EnchAlreadyOnObject(data.caster, spell, g)) return false;

                    if (!IsTownProtected(data.GetWizardID(), spell, data.battle)) return true;
                }
            }
            return false;
        }

        static public bool STAR_EnemyNoColdImmu_Unit(SpellCastData data, object target, Spell spell)
        {
            //var w = data.GetPlayerWizard();
            if (target is MOM.Unit)
            {
                var u = target as MOM.Unit;

                if (data.GetWizardID() != u.group.Get().GetOwnerID()
                    && u.attributes.DoesNotContains((Tag)TAG.MAGIC_IMMUNITY)
                    && u.attributes.DoesNotContains((Tag)TAG.COLD_IMMUNITY))
                {
                    if (EnchAlreadyOnObject(data.caster, spell, u)) return false;

                    var location = u.group.Get().GetLocationHostSmart();
                    if (location != null && location is TownLocation)
                    {
                        if (!IsTownProtected(data.GetWizardID(), spell, location as TownLocation)) return true;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            else if (target is BattleUnit)
            {
                var g = target as BattleUnit;

                if (data.GetWizardID() != g.ownerID
                    && g.attributes.DoesNotContains((Tag)TAG.MAGIC_IMMUNITY)
                    && g.attributes.DoesNotContains((Tag)TAG.COLD_IMMUNITY))
                {
                    if (!g.currentlyVisible) return false;
                    if (EnchAlreadyOnObject(data.caster, spell, g)) return false;

                    if (!IsTownProtected(data.GetWizardID(), spell, data.battle)) return true;
                }
            }
            return false;
        }
        static public bool STAR_EnemyNoFireImmu_Unit(SpellCastData data, object target, Spell spell)
        {
            //var w = data.GetPlayerWizard();
            if (target is MOM.Unit)
            {
                var u = target as MOM.Unit;

                if (data.GetWizardID() != u.group.Get().GetOwnerID()
                    && u.attributes.DoesNotContains((Tag)TAG.MAGIC_IMMUNITY)
                    && u.attributes.DoesNotContains((Tag)TAG.FIRE_IMMUNITY)
                    && u.attributes.DoesNotContains((Tag)TAG.RIGHTEOUSNESS))
                {
                    if (EnchAlreadyOnObject(data.caster, spell, u)) return false;

                    var location = u.group.Get().GetLocationHostSmart();
                    if (location != null && location is TownLocation)
                    {
                        if (!IsTownProtected(data.GetWizardID(), spell, location as TownLocation)) return true;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            else if (target is BattleUnit)
            {
                var g = target as BattleUnit;

                if (data.GetWizardID() != g.ownerID
                    && g.attributes.DoesNotContains((Tag)TAG.MAGIC_IMMUNITY)
                    && g.attributes.DoesNotContains((Tag)TAG.FIRE_IMMUNITY)
                    && g.attributes.DoesNotContains((Tag)TAG.RIGHTEOUSNESS))
                {
                    if (!g.currentlyVisible) return false;
                    if (EnchAlreadyOnObject(data.caster, spell, g)) return false;

                    if (!IsTownProtected(data.GetWizardID(), spell, data.battle)) return true;
                }
            }
            return false;
        }

        static public bool STAR_EnemyNoRighteousnessImmu_Unit(SpellCastData data, object target, Spell spell)
        {
            //var w = data.GetPlayerWizard();
            if (target is MOM.Unit)
            {
                var u = target as MOM.Unit;

                if (data.GetWizardID() != u.group.Get().GetOwnerID()
                    && u.attributes.DoesNotContains((Tag)TAG.MAGIC_IMMUNITY)
                    && u.attributes.DoesNotContains((Tag)TAG.RIGHTEOUSNESS))
                {
                    if (EnchAlreadyOnObject(data.caster, spell, u)) return false;

                    var location = u.group.Get().GetLocationHostSmart();
                    if (location != null && location is TownLocation)
                    {
                        if (!IsTownProtected(data.GetWizardID(), spell, location as TownLocation)) return true;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            else if (target is BattleUnit)
            {
                var g = target as BattleUnit;

                if (data.GetWizardID() != g.ownerID
                    && g.attributes.DoesNotContains((Tag)TAG.MAGIC_IMMUNITY)
                    && g.attributes.DoesNotContains((Tag)TAG.RIGHTEOUSNESS))
                {
                    if (!g.currentlyVisible) return false;
                    if (EnchAlreadyOnObject(data.caster, spell, g)) return false;

                    if (!IsTownProtected(data.GetWizardID(), spell, data.battle)) return true;
                }
            }
            return false;
        }
        static public bool STAR_EnemyNoFantastic_Unit(SpellCastData data, object target, Spell spell)
        {
            //var w = data.GetPlayerWizard();
            if (target is MOM.Unit)
            {
                var u = target as MOM.Unit;

                if (data.GetWizardID() != u.group.Get().GetOwnerID()
                    && u.attributes.DoesNotContains((Tag)TAG.FANTASTIC_CLASS))
                {
                    if (EnchAlreadyOnObject(data.caster, spell, u)) return false;

                    var location = u.group.Get().GetLocationHostSmart();
                    if (location != null && location is TownLocation)
                    {
                        if (!IsTownProtected(data.GetWizardID(), spell, location as TownLocation)) return true;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            else if (target is BattleUnit)
            {
                var g = target as BattleUnit;

                if (data.GetWizardID() != g.ownerID
                    && g.attributes.DoesNotContains((Tag)TAG.FANTASTIC_CLASS))
                {
                    if (!g.currentlyVisible) return false;
                    if (EnchAlreadyOnObject(data.caster, spell, g)) return false;

                    if (!IsTownProtected(data.GetWizardID(), spell, data.battle)) return true;
                }
            }
            return false;
        }
        static public bool STAR_EnemyNoFantasticNoDeathImmus_Unit(SpellCastData data, object target, Spell spell)
        {
            //var w = data.GetPlayerWizard();
            if (target is MOM.Unit)
            {
                var u = target as MOM.Unit;

                if (data.GetWizardID() != u.group.Get().GetOwnerID()
                    && u.attributes.DoesNotContains((Tag)TAG.FANTASTIC_CLASS)
                    && u.attributes.DoesNotContains((Tag)TAG.MAGIC_IMMUNITY)
                    && u.attributes.DoesNotContains((Tag)TAG.DEATH_IMMUNITY)
                    && u.attributes.DoesNotContains((Tag)TAG.RIGHTEOUSNESS))
                {
                    if (EnchAlreadyOnObject(data.caster, spell, u)) return false;

                    var location = u.group.Get().GetLocationHostSmart();
                    if (location != null && location is TownLocation)
                    {
                        if (!IsTownProtected(data.GetWizardID(), spell, location as TownLocation)) return true;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            else if (target is BattleUnit)
            {
                var g = target as BattleUnit;

                if (data.GetWizardID() != g.ownerID
                    && g.attributes.DoesNotContains((Tag)TAG.FANTASTIC_CLASS)
                    && g.attributes.DoesNotContains((Tag)TAG.MAGIC_IMMUNITY)
                    && g.attributes.DoesNotContains((Tag)TAG.DEATH_IMMUNITY)
                    && g.attributes.DoesNotContains((Tag)TAG.RIGHTEOUSNESS))
                {
                    if (!g.currentlyVisible) return false;
                    if (EnchAlreadyOnObject(data.caster, spell, g)) return false;

                    if (!IsTownProtected(data.GetWizardID(), spell, data.battle)) return true;
                }
            }
            return false;
        }
        static public bool STAR_EnemyFantastic_Unit(SpellCastData data, object target, Spell spell)
        {
            //var w = data.GetPlayerWizard();
            if (target is MOM.Unit)
            {
                var u = target as MOM.Unit;

                if (data.GetWizardID() != u.group.Get().GetOwnerID()
                    && !u.isSpellLock
                    && u.attributes.Contains(TAG.FANTASTIC_CLASS))
                {
                    if (EnchAlreadyOnObject(data.caster, spell, u)) return false;

                    var location = u.group.Get().GetLocationHostSmart();
                    if (location != null && location is TownLocation)
                    {
                        if (!IsTownProtected(data.GetWizardID(), spell, location as TownLocation)) return true;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            if (target is BattleUnit)
            {
                var g = target as BattleUnit;

                if (data.GetWizardID() != g.ownerID
                    && !g.isSpellLock
                    && g.attributes.Contains(TAG.FANTASTIC_CLASS))
                {
                    if (!g.currentlyVisible) return false;
                    if (EnchAlreadyOnObject(data.caster, spell, g)) return false;

                    if (!IsTownProtected(data.GetWizardID(), spell, data.battle)) return true;
                }
            }
            return false;
        }

        static public bool STAR_NonFantasticEnemyWithAmmoNoRighteousnessImmu_Unit(SpellCastData data, object target, Spell spell)
        {
            //var w = data.GetPlayerWizard();
            if (target is MOM.Unit)
            {
                var u = target as MOM.Unit;

                if (data.GetWizardID() != u.group.Get().GetOwnerID()
                    && u.GetAttFinal((Tag)TAG.MAGIC_IMMUNITY) == 0
                    && u.GetAttFinal((Tag)TAG.FANTASTIC_CLASS) == 0
                    && u.GetAttFinal((Tag)TAG.RIGHTEOUSNESS) == 0
                    && u.GetAttFinal(TAG.AMMUNITION) > 0
                    && u.GetAttFinal((Tag)TAG.BOULDER_RANGE) == 0
                    && u.GetAttFinal((Tag)TAG.MAGIC_RANGE) == 0)
                {
                    if (EnchAlreadyOnObject(data.caster, spell, u)) return false;

                    var location = u.group.Get().GetLocationHostSmart();
                    if (location != null && location is TownLocation)
                    {
                        if (!IsTownProtected(data.GetWizardID(), spell, location as TownLocation)) return true;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            else if (target is BattleUnit)
            {
                var g = target as BattleUnit;

                //var x = g.GetAttFinal((Tag)TAG.BOULDER_RANGE);

                if (data.GetWizardID() != g.ownerID
                    && g.GetAttFinal((Tag)TAG.MAGIC_IMMUNITY) == 0
                    && g.GetAttFinal((Tag)TAG.FANTASTIC_CLASS) == 0
                    && g.GetAttFinal((Tag)TAG.RIGHTEOUSNESS) == 0
                    && g.GetCurentFigure().rangedAmmo > 0
                    && g.GetAttFinal((Tag)TAG.BOULDER_RANGE) == 0
                    && g.GetAttFinal((Tag)TAG.MAGIC_RANGE) == 0)
                {
                    if (!g.currentlyVisible) return false;
                    if (EnchAlreadyOnObject(data.caster, spell, g)) return false;

                    if (!IsTownProtected(data.GetWizardID(), spell, data.battle)) return true;
                }
            }
            return false;
        }

        static public bool STAR_EnemyNoIlusionImmu_Unit(SpellCastData data, object target, Spell spell)
        {
            //var w = data.GetPlayerWizard();
            if (target is MOM.Unit)
            {
                var u = target as MOM.Unit;

                if (data.GetWizardID() != u.group.Get().GetOwnerID() &&
                    (u.attributes.DoesNotContains((Tag)TAG.ILLUSIONS_IMMUNITY)
                    && u.attributes.DoesNotContains((Tag)TAG.MAGIC_IMMUNITY)))
                {
                    if (EnchAlreadyOnObject(data.caster, spell, u)) return false;

                    var location = u.group.Get().GetLocationHostSmart();
                    if (location != null && location is TownLocation)
                    {
                        if (!IsTownProtected(data.GetWizardID(), spell, location as TownLocation)) return true;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            if (target is BattleUnit)
            {
                var g = target as BattleUnit;

                if (data.GetWizardID() != g.ownerID &&
                    (g.attributes.DoesNotContains((Tag)TAG.ILLUSIONS_IMMUNITY)
                    && g.attributes.DoesNotContains((Tag)TAG.MAGIC_IMMUNITY)))
                {
                    if (!g.currentlyVisible) return false;
                    if (EnchAlreadyOnObject(data.caster, spell, g)) return false;

                    if (!IsTownProtected(data.GetWizardID(), spell, data.battle)) return true;
                }
            }
            return false;
        }

        static public bool STAR_EnemyChaosDeath_Unit(SpellCastData data, object target, Spell spell)
        {
            //var w = data.GetPlayerWizard();
            if (target is MOM.Unit)
            {
                var u = target as MOM.Unit;

                if (data.GetWizardID() != u.group.Get().GetOwnerID()
                    && !u.isSpellLock
                    && (u.race == (Race)RACE.REALM_CHAOS
                    || u.race == (Race)RACE.REALM_DEATH))
                {
                    if (EnchAlreadyOnObject(data.caster, spell, u)) return false;

                    var location = u.group.Get().GetLocationHostSmart();
                    if (location != null && location is TownLocation)
                    {
                        if (!IsTownProtected(data.GetWizardID(), spell, location as TownLocation)) return true;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            if (target is BattleUnit)
            {
                var g = target as BattleUnit;

                if (data.GetWizardID() != g.ownerID
                    && !g.isSpellLock
                    && (g.race == (Race)RACE.REALM_CHAOS
                   || g.race == (Race)RACE.REALM_DEATH))
                {
                    if (!g.currentlyVisible) return false;
                    if (EnchAlreadyOnObject(data.caster, spell, g)) return false;

                    if (!IsTownProtected(data.GetWizardID(), spell, data.battle)) return true;
                }
            }
            return false;
        }
        static public bool STAR_EnemyNonFlyNonCorporeal(SpellCastData data, object target, Spell spell)
        {
            //var w = data.GetPlayerWizard();
            if (target is MOM.Unit)
            {
                var u = target as MOM.Unit;

                if (data.GetWizardID() != u.group.Get().GetOwnerID()
                    && u.attributes.DoesNotContains((Tag)TAG.CAN_FLY)
                    && u.attributes.DoesNotContains((Tag)TAG.NON_CORPOREAL))
                {
                    if (EnchAlreadyOnObject(data.caster, spell, u)) return false;

                    var location = u.group.Get().GetLocationHostSmart();
                    if (location != null && location is TownLocation)
                    {
                        if (!IsTownProtected(data.GetWizardID(), spell, location as TownLocation)) return true;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            else if (target is BattleUnit)
            {
                var g = target as BattleUnit;

                if (data.GetWizardID() != g.ownerID
                    && g.attributes.DoesNotContains((Tag)TAG.CAN_FLY)
                    && g.attributes.DoesNotContains((Tag)TAG.NON_CORPOREAL))
                {
                    if (!g.currentlyVisible) return false;
                    if (EnchAlreadyOnObject(data.caster, spell, g)) return false;

                    if (!IsTownProtected(data.GetWizardID(), spell, data.battle)) return true;
                }
            }

            return false;
        }
        static public bool STAR_CracksCall(SpellCastData data, object target, Spell spell)
        {
            if (data.battle == null)
                return false;
            else
            {
                if (data.battle.landBattle && target is BattleUnit)
                {
                    var g = target as BattleUnit;

                    if (data.GetWizardID() != g.ownerID
                        && g.attributes.DoesNotContains((Tag)TAG.CAN_FLY)
                        && g.attributes.DoesNotContains((Tag)TAG.NON_CORPOREAL))
                    {
                        if (!g.currentlyVisible) return false;

                        if (!IsTownProtected(data.GetWizardID(), spell, data.battle)) return true;
                    }
                }
            }


            return false;
        }
        static public bool STAR_Confusion(SpellCastData data, object target, Spell spell)
        {
            if (!STAR_EnemyNoIlusionImmu_Unit(data, target, spell)) return false;

            if (target is BattleUnit)
            {
                var bu = target as BattleUnit;
                if (!bu.currentlyVisible) return false;

                if (bu.GetEnchantments().Find(o => o.source == (Enchantment)ENCH.CONFUSION ||
                                                  o.source == (Enchantment)ENCH.POSSESSION ||
                                                  o.source == (Enchantment)ENCH.BLACK_SLEEP) == null)
                {
                    return true;
                }
            }

            return false;
        }
        static public bool STAR_Possession(SpellCastData data, object target, Spell spell)
        {
            if (!STAR_EnemyNoFantasticNoDeathImmus_Unit(data, target, spell)) return false;

            if (target is BattleUnit)
            {
                var bu = target as BattleUnit;
                if (!bu.currentlyVisible) return false;

                if (bu.GetEnchantments().Find(o => o.source == (Enchantment)ENCH.CONFUSION ||
                                                   o.source == (Enchantment)ENCH.POSSESSION ||
                                                   o.source == (Enchantment)ENCH.BLACK_SLEEP) == null)
                {
                    return true;
                }
            }

            return false;
        }
        static public bool STAR_FriendlyBomber(SpellCastData data, object target, Spell spell)
        {
            //var w = data.GetPlayerWizard();
            if (target is BattleUnit)
            {
                var g = target as BattleUnit;

                if (data.GetWizardID() == g.ownerID
                    && g.attributes.Contains(TAG.BOMBER))
                {
                    return true;
                }
            }
            return false;
        }

        static public bool STAR_Dispel_Any_Unit(SpellCastData data, object target, Spell spell)
        {

            if (target is MOM.Unit)
            {
                var unit = target as MOM.Unit;
                var spellcaster = data.caster;
                var unitOwner = unit.GetWizardOwner();
                var enchList = unit.GetEnchantments();

                for (int i = 0; i < enchList.Count; i++)
                {
                    //Dispel only negative ench on own ba. Dispel only positive ench on enemy ba.
                    if (enchList[i].source.Get().allowDispel != false &&
                        (unitOwner == spellcaster && enchList[i].source.Get().enchCategory == EEnchantmentCategory.Negative ||
                        unitOwner != spellcaster && (enchList[i].source.Get().enchCategory != EEnchantmentCategory.Negative)))
                    {
                        var location = unit.group.Get().GetLocationHostSmart();
                        if (location != null && location is TownLocation)
                        {
                            if (!IsTownProtected(data.GetWizardID(), spell, location as TownLocation)) return true;
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
            }
            if (target is BattleUnit)
            {
                var bu = target as BattleUnit;
                var spellcaster = data.caster;
                var unitOwner = bu.GetWizardOwner();
                var enchList = bu.GetEnchantments();
                for (int i = 0; i < enchList.Count; i++)
                {
                    //Dispel only negative ench on own ba. Dispel only positive ench on enemy ba.
                    if (enchList[i].source.Get().allowDispel != false &&
                        (unitOwner == spellcaster && enchList[i].source.Get().enchCategory == EEnchantmentCategory.Negative ||
                        unitOwner != spellcaster && enchList[i].source.Get().enchCategory != EEnchantmentCategory.Negative))
                    {
                        if (!IsTownProtected(data.GetWizardID(), spell, data.battle)) return true;
                    }
                    else if ((enchList[i].source == (Enchantment)ENCH.CONFUSION || enchList[i].source == (Enchantment)ENCH.CONFUSION_POSSESSION)
                        && unitOwner == spellcaster)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        static public bool STAR_TargetNormalRoad(SpellCastData data, object target, Spell spell)
        {
            //var spellcaster = data.GetPlayerWizard();

            if (data.battle != null) return false;

            if (data.battle == null)
            {
                var h = target as Hex;
                if (h == null) return false;

                if (World.GetArcanus().GetHexAt(h.Position) == h)
                {
                    var t = World.GetArcanus().GetRoadManagers().GetRoadTypeAt(h.Position);
                    return t == RoadManager.RoadType.Normal;
                }
                else
                {
                    var t = World.GetMyrror().GetRoadManagers().GetRoadTypeAt(h.Position);
                    return t == RoadManager.RoadType.Normal;
                }
            }
            return false;
        }

        static public bool STAR_TargetLandHex(SpellCastData data, object target, Spell spell)
        {
            //var spellcaster = data.GetPlayerWizard();

            if (data.battle != null)
            {
                if (!(target is Vector3i)) return false;
                var t = (Vector3i)target;

                var h = data.battle.plane.GetHexAt(t);
                if (h == null) return false;

                if (!IsTownProtected(data.GetWizardID(), spell, data.battle)
                    && h.IsLand()) return true;

            }

            if (data.battle == null)
            {
                var h = target as Hex;
                if (h != null)
                {
                    WorldCode.Plane p = World.GetArcanus();
                    if (p.GetHexAt(h.Position) != h)
                    {
                        p = World.GetMyrror();
                        if (p.GetHexAt(h.Position) != h)
                        {
                            Debug.LogError("STAR_TargetLandHex: cannot find hex");
                            return false;
                        }
                    }

                    foreach (var l in GameManager.GetLocationsOfThePlane(p))
                    {
                        if (l.Position == h.Position)
                        {
                            if (!IsTownProtected(data.GetWizardID(), spell, l as TownLocation))
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                    }
                    return true;
                }
                else
                {
                    Debug.Log("Spell is designed to target Hex");
                }

            }

            return false;
        }


        static public bool STAR_TargetEmptyLandHex(SpellCastData data, object target, Spell spell)
        {
            //var spellcaster = data.GetPlayerWizard();

            if (data.battle != null)
            {
                if (!(target is Vector3i)) return false;
                var t = (Vector3i)target;

                var h = data.battle.plane.GetHexAt(t);
                if (h == null) return false;

                if (!IsTownProtected(data.GetWizardID(), spell, data.battle)
                    && h.IsLand() && data.battle.GetUnitAt(t) == null) return true;

            }

            if (data.battle == null)
            {
                var h = target as Hex;
                if (h != null)
                {
                    WorldCode.Plane p = World.GetArcanus();
                    if (p.GetHexAt(h.Position) != h)
                    {
                        p = World.GetMyrror();
                        if (p.GetHexAt(h.Position) != h)
                        {
                            Debug.LogError("STAR_TargetLandHex: cannot find hex");
                            return false;
                        }
                    }

                    foreach (var l in GameManager.GetLocationsOfThePlane(p))
                    {
                        if (l.Position != h.Position)
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    Debug.Log("Spell is designed to target Hex");
                }

            }

            return false;
        }
        static public bool STAR_TargetEmptyHex(SpellCastData data, object target, Spell spell)
        {
            //var spellcaster = data.GetPlayerWizard();

            if (data.battle != null)
            {
                if (!(target is Vector3i)) return false;
                var t = (Vector3i)target;

                var h = data.battle.plane.GetHexAt(t);
                if (h == null) return false;

                if (!IsTownProtected(data.GetWizardID(), spell, data.battle)
                    && data.battle.GetUnitAt(t) == null) return true;

            }

            if (data.battle == null)
            {
                var h = target as Hex;
                if (h != null)
                {
                    WorldCode.Plane p = World.GetArcanus();
                    if (p.GetHexAt(h.Position) != h)
                    {
                        p = World.GetMyrror();
                        if (p.GetHexAt(h.Position) != h)
                        {
                            Debug.LogError("STAR_TargetLandHex: cannot find hex");
                            return false;
                        }
                    }

                    foreach (var l in GameManager.GetLocationsOfThePlane(p))
                    {
                        if (l.Position != h.Position)
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    Debug.Log("Spell is designed to target Hex");
                }

            }

            return false;
        }
        static public bool STAR_BattleWall(SpellCastData data, object target, Spell spell)
        {
            //var spellcaster = data.GetPlayerWizard();

            if (data.battle != null)
            {
                if (!(target is Vector3i)) return false;
                var pos = (Vector3i)target;

                var h = data.battle.plane.GetHexAt(pos);
                if (h == null) return false;

                if (!IsTownProtected(data.GetWizardID(), spell, data.battle)
                    && h.IsLand())
                {
                    foreach (var wallPart in data.battle.battleWalls)
                    {
                        if (wallPart.position == pos)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
        static public bool STAR_TargetEmptyBattleHex(SpellCastData data, object target, Spell spell)
        {
            //var wizard = data.GetPlayerWizard();

            if (data.battle != null)
            {
                if (!(target is Vector3i)) return false;
                var t = (Vector3i)target;

                if (t.y > 0 && data.battle.attacker.GetID() == data.GetWizardID() ||
                    t.y < 0 && data.battle.defender.GetID() == data.GetWizardID())
                {
                    return false;
                }

                var h = data.battle.plane.GetHexAt(t);
                if (h == null) return false;

                if (!data.battle.IsLocationEmpty(t))
                {
                    return false;
                }

                if (!IsTownProtected(data.GetWizardID(), spell, data.battle)) return true;

            }
            else
            {
                //in virtual fights without battle, targeting empty hex always succeeds
                return true;
            }

            return false;
        }
        static public bool STAR_TargetEmptyLandBattleHex(SpellCastData data, object target, Spell spell)
        {
            //var wizard = data.GetPlayerWizard();

            if (data.battle != null)
            {
                if (!(target is Vector3i)) return false;
                var t = (Vector3i)target;

                if (t.y > 0 && data.battle.attacker.GetID() == data.GetWizardID() ||
                    t.y < 0 && data.battle.defender.GetID() == data.GetWizardID())
                {
                    return false;
                }

                var h = data.battle.plane.GetHexAt(t);
                if (h == null) return false;

                if (!data.battle.IsLocationEmpty(t))
                {
                    return false;
                }
                if (data.battle.battleWalls != null)
                {
                    var w = data.battle.battleWalls.Find(o => o.position == t);
                    if (w != null && w.standing) return false;
                }

                if (!IsTownProtected(data.GetWizardID(), spell, data.battle)
                    && h.IsLand()) return true;

            }
            else
            {
                //in virtual fights without battle, targeting empty hex always succeeds
                return true;
            }

            return false;
        }

        static public bool STAR_FriendlyGroup(SpellCastData data, object target, Spell spell)
        {
            //var w = data.GetPlayerWizard();
            if (target is MOM.Group)
            {
                var g = target as MOM.Group;
                if (data.GetWizardID() == g.GetOwnerID())
                {
                    var location = g.GetLocationHostSmart();
                    if (location != null && location is TownLocation)
                    {
                        if (!IsTownProtected(data.GetWizardID(), spell, location as TownLocation)) return true;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        static public bool STAR_EnemyGroup(SpellCastData data, object target, Spell spell)
        {
            //var spellcaster = data.GetPlayerWizard();
            if (target is MOM.Group)
            {
                var group = target as MOM.Group;

                // Check if group do not stand on hex with protected town.
                if (data.GetWizardID() != group.GetOwnerID())
                {
                    var location = group.GetLocationHostSmart();
                    if (location != null && location is TownLocation)
                    {
                        if (!IsTownProtected(data.GetWizardID(), spell, location as TownLocation)) return true;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        static public bool STAR_FriendlyTown(SpellCastData data, object target, Spell spell)
        {
            if (target is TownLocation)
            {
                var g = target as TownLocation;
                if (data.GetWizardID() == g.GetOwnerID())
                {
                    if (EnchAlreadyOnObject(data.caster, spell, g)) return false;
                    return true;
                }
            }
            return false;
        }
        static public bool STAR_FriendlyTownNoOutpost(SpellCastData data, object target, Spell spell)
        {
            if (STAR_FriendlyTown(data, target, spell))
            {
                var g = target as TownLocation;
                if (g.Population < 1000)
                    return false;
                else
                    return true;
            }

            return false;
        }
        static public bool STAR_FriendlyTownWithFortress(SpellCastData data, object target, Spell spell)
        {
            if (STAR_FriendlyTown(data, target, spell))
            {
                var g = target as TownLocation;
                if (data.GetPlayerWizard().wizardTower.Get() == g) return true;
            }

            return false;
        }
        static public bool STAR_AddBuildingWallOfStone(SpellCastData data, object target, Spell spell)
        {
            if (STAR_FriendlyTown(data, target, spell))
            {
                var g = target as TownLocation;
                foreach (var b in g.buildings)
                {
                    if (b == (Building)BUILDING.CITY_WALLS) return false;
                }
                return true;
            }

            return false;
        }

        static public bool STAR_EnemyTown(SpellCastData data, object target, Spell spell)
        {
            var wizard = data.GetPlayerWizard();
            if (target is TownLocation)
            {
                var town = target as TownLocation;
                if (data.GetWizardID() != town.GetOwnerID())
                {
                    if (EnchAlreadyOnObject(data.caster, spell, town)) return false;
                    //Check if someone has casted counter magic on the town
                    if ((bool)ScriptLibrary.Call("CounterMagicNightShade", town, spell, wizard))
                    {
                        wizard.GetMagicAndResearch().ResetCasting();
                        return false;
                    }
                    if (!IsTownProtected(data.GetWizardID(), spell, town)) return true;
                }
            }
            return false;
        }
        static public bool STAR_EnemyLocation(SpellCastData data, object target, Spell spell)
        {
            var spellcaster = data.GetPlayerWizard();
            if (target is MOM.Location)
            {
                var location = target as MOM.Location;
                if (spellcaster.ID != location.GetOwnerID())
                {
                    if (EnchAlreadyOnObject(data.caster, spell, location)) return false;
                    return true;
                }
            }
            return false;
        }
        static public bool STAR_EnemyNode(SpellCastData data, object target, Spell spell)
        {
            var spellcaster = data.GetPlayerWizard();
            if (target is MOM.Location)
            {
                var location = target as MOM.Location;
                if (location.locationType != ELocationType.Node) return false;
                if (location.melding == null || location.melding.meldOwner < 1) return false;

                if (spellcaster.ID != location.GetOwnerID())
                {
                    if (EnchAlreadyOnObject(data.caster, spell, location)) return false;
                    return true;
                }
            }
            return false;
        }
        static public bool STAR_OwnWizard(SpellCastData data, object target, Spell spell)
        {
            var w = target as PlayerWizard;

            if (w != null)
            {
                if (EnchAlreadyOnObject(data.caster, spell, w)) return false;
                int owner = data.GetWizardID();
                return w.ID == owner;
            }

            Debug.Log("Spell is designed to target wizard");
            return false;
        }
        static public bool STAR_EnemyWizard(SpellCastData data, object target, Spell spell)
        {
            var w = target as PlayerWizard;
            if (w != null)
            {
                if (!w.isAlive) return false;
                if (EnchAlreadyOnObject(data.caster, spell, w)) return false;
                int owner = data.GetWizardID();
                var spallCaster = GameManager.GetWizard(owner);

                if (spallCaster.discoveredWizards != null)
                {
                    if (spallCaster.discoveredWizards.Contains(w))
                    {
                        return w.ID != owner;
                    }
                    return false;
                }
            }

            return false;
        }
        static public bool STAR_OwnBattlePlayer(SpellCastData data, object target, Spell spell)
        {
            var bp = target as BattlePlayer;

            if (bp != null)
            {
                var owner = data.GetPlayerWizard();
                if (bp.wizard == owner)
                {
                    if (EnchAlreadyOnObject(data.caster, spell, bp)) return false;

                    return true;
                }
            }
            else
            {
                Debug.Log("Spell is designed to target BattlePlayer");
            }

            return false;
        }
        static public bool STAR_EnemyBattlePlayer(SpellCastData data, object target, Spell spell)
        {
            var bp = target as BattlePlayer;
            if (bp != null)
            {
                var owner = data.GetPlayerWizard();
                if (bp.wizard != owner)
                {
                    if (EnchAlreadyOnObject(data.caster, spell, bp)) return false;

                    if (!IsTownProtected(data.GetWizardID(), spell, data.battle)) return true;
                }
            }
            else
            {
                Debug.Log("Spell is designed to target BattlePlayer");
            }

            return false;
        }
        static public bool STAR_CastingBlock(SpellCastData data, object target, Spell spell)
        {
            if(STAR_EnemyBattlePlayer(data, target, spell))
            {
                if((target as BattlePlayer).wizard == null)
                {
                    var battle = data.battle;
                    if (data.GetWizardID() == PlayerWizard.HumanID() && battle != null &&
                        !battle.GetHumanPlayer().autoPlayByAI)
                        PopupGeneral.OpenPopup(null, "UI_TARGETING_FAILED", "UI_SPELL_NO_VALID_TARGET_WIZARD", "UI_OK");
                    return false;
                }
                if (data.caster is BattleUnit)
                {
                    var bu = data.caster as BattleUnit;
                    var hp = bu.GetTotalHealth();
                    var sacrifice = spell.fIntData[0];

                    if (hp > sacrifice)
                    {
                        return true;
                    }
                    else
                    {
                        if (data.GetWizardID() == PlayerWizard.HumanID())
                            PopupGeneral.OpenPopup(null, "UI_TARGETING_FAILED", "UI_NOT_ENOUGHT_HP", "UI_OK");
                        return false;
                    }
                }
            }

            return false;
        }
        static public bool STAR_TransmuteResource(SpellCastData data, object target, Spell spell)
        {
            var h = target as Hex;
            var w = data.GetPlayerWizard();
            if (h != null)
            {
                if (h.Resource == null) return false;

                var transmuteTo = h.Resource.Get().transmuteTo;
                if (transmuteTo == null) return false;

                WorldCode.Plane p = World.GetArcanus();
                if (p.GetHexAt(h.Position) != h)
                {
                    p = World.GetMyrror();
                    if (p.GetHexAt(h.Position) != h)
                    {
                        Debug.LogError("STAR_TransmuteResource: cannot find hex");
                        return false;
                    }
                }

                List<Vector3i> poss = HexNeighbors.GetRange(h.Position, 2); // town range is 2, need to check if any town protects this hex
                List<MOM.Location> locs = GameManager.GetLocationsOfThePlane(p);
                List<MOM.Location> locInRange = new List<MOM.Location>();
                foreach (var pos in poss)
                {
                    locInRange.AddRange(locs.FindAll(o => o.GetPosition() == pos));
                }
                if (locInRange.Count > 0)
                {
                    foreach (var loc in locInRange)
                    {
                        if (loc is TownLocation)
                        {
                            TownLocation t = loc as TownLocation;
                            if (IsTownProtected(data.GetWizardID(), spell, t)) return false;
                        }
                    }
                }
                return true;
            }
            else
            {
                Debug.Log("Spell is designed to target Hex");
            }

            return false;
        }
        static public bool STAR_ChangeTerrain(SpellCastData data, object target, Spell spell)
        {
            var h = target as Hex;
            var w = data.GetPlayerWizard();
            if (h != null)
            {
                var changeTo = h.GetTerrain().transmuteTo;
                if (changeTo == null) return false;

                WorldCode.Plane p = World.GetArcanus();
                if (p.GetHexAt(h.Position) != h)
                {
                    p = World.GetMyrror();
                    if (p.GetHexAt(h.Position) != h)
                    {
                        Debug.LogError("STAR_TransmuteResource: cannot find hex");
                        return false;
                    }
                }

                List<MOM.Location> locs = GameManager.GetLocationsOfThePlane(p);

                var local = locs.Find(o => o.GetPosition() == h.Position);
                if (local != null && !(local is TownLocation)
                    && local.locationType != ELocationType.Lair
                    && local.locationType != ELocationType.StrongLair
                    && local.locationType != ELocationType.WeakLair
                    && local.locationType != ELocationType.Ruins)
                {
                    return false;
                }

                local = locs.Find(o => HexCoordinates.HexDistance(o.GetPosition(), h.Position) <= 2);
                if (local != null && local is TownLocation)
                {
                    TownLocation t = local as TownLocation;
                    if (IsTownProtected(data.GetWizardID(), spell, t)) return false;
                }
                return true;
            }
            else
            {
                Debug.Log("Spell is designed to target Hex");
            }

            return false;
        }
        static public bool STAR_DetectMinerals(SpellCastData data, object target, Spell spell)
        {
            var hex = target as Hex;
            var w = data.GetPlayerWizard();
            if (hex != null && hex.resourceInstance == null && hex.IsLand())
            {
                WorldCode.Plane p = World.GetArcanus();
                if (p.GetHexAt(hex.Position) != hex)
                {
                    p = World.GetMyrror();
                    if (p.GetHexAt(hex.Position) != hex)
                    {
                        Debug.LogError("STAR_DetectMinerals: cannot find hex");
                        return false;
                    }
                }
                List<MOM.Location> locs = GameManager.GetLocationsOfThePlane(p);
                var local = locs.Find(o => o.GetPosition() == hex.Position);
                if (local != null && !(local is TownLocation)
                    && local.locationType != ELocationType.Lair
                    && local.locationType != ELocationType.StrongLair
                    && local.locationType != ELocationType.WeakLair
                    && local.locationType != ELocationType.Ruins
                    && local.locationType != ELocationType.MidGameLair
                    && local.locationType != ELocationType.WaterArcanusUnique
                    && local.locationType != ELocationType.WaterMyrrorUnique
                    && local.locationType != ELocationType.BossLair)
                {
                    return false;
                }
                var hexsAround = HexNeighbors.GetRange(hex.Position, 1);
                foreach (var h in hexsAround)
                {
                    if (p.GetHexAt(h).resourceInstance != null) return false;
                }
                return true;
            }
            else
            {
                Debug.Log("Spell is designed to target Hex");
            }

            return false;
        }
        static public bool STAR_SeaHarvest(SpellCastData data, object target, Spell spell)
        {
            var hex = target as Hex;
            var w = data.GetPlayerWizard();
            if (hex != null && hex.resourceInstance != null && !hex.IsLand())
            {
                WorldCode.Plane p = World.GetArcanus();
                if (p.GetHexAt(hex.Position) != hex)
                {
                    p = World.GetMyrror();
                    if (p.GetHexAt(hex.Position) != hex)
                    {
                        Debug.LogError("STAR_SeaHarvest: cannot find hex");
                        return false;
                    }
                }
                List<MOM.Location> locs = GameManager.GetLocationsOfThePlane(p);
                var local = locs.Find(o => o.GetPosition() == hex.Position);
                if (local != null && !(local is TownLocation)
                    && local.locationType != ELocationType.Lair
                    && local.locationType != ELocationType.StrongLair
                    && local.locationType != ELocationType.WeakLair
                    && local.locationType != ELocationType.Ruins
                    && local.locationType != ELocationType.MidGameLair
                    && local.locationType != ELocationType.WaterArcanusUnique
                    && local.locationType != ELocationType.WaterMyrrorUnique)
                {
                    return false;
                }

                if (FOW.Get().IsVisible(hex.Position, p))
                {
                    return true;
                }
            }
            else
            {
                Debug.Log("Spell is designed to target Hex");
            }

            return false;
        }
        static public bool STAR_RaiseVolcano(SpellCastData data, object target, Spell spell)
        {
            var h = target as Hex;
            var w = data.GetPlayerWizard();
            if (h != null)
            {
                if (!h.IsLand()) return false;

                WorldCode.Plane p = World.GetArcanus();
                if (p.GetHexAt(h.Position) != h)
                {
                    p = World.GetMyrror();
                    if (p.GetHexAt(h.Position) != h)
                    {
                        Debug.LogError("STAR_TransmuteResource: cannot find hex");
                        return false;
                    }
                }

                // cannot raise volcano on the magic node
                MOM.Location hexLocation = GameManager.GetLocationsOfThePlane(p).Find(o => o.Position == h.Position);
                if (hexLocation != null)
                {
                    if (hexLocation.source.Get() is MagicNode)
                    {
                        return false;
                    }
                }

                List<Vector3i> poss = HexNeighbors.GetRange(h.Position, 2); // town range is 2, need to check if any town protects this hex
                List<MOM.Location> locs = GameManager.GetLocationsOfThePlane(p);
                List<MOM.Location> locInRange = new List<MOM.Location>();
                foreach (var pos in poss)
                {
                    locInRange.AddRange(locs.FindAll(o => o.GetPosition() == pos));
                }
                if (locInRange.Count > 0)
                {
                    foreach (var loc in locInRange)
                    {
                        if (loc is TownLocation)
                        {
                            TownLocation t = loc as TownLocation;
                            if (IsTownProtected(data.GetWizardID(), spell, t)) return false;
                        }
                    }
                }
                return true;
            }
            else
            {
                Debug.Log("Spell is designed to target Hex");
            }

            return false;
        }
        static public bool STAR_Battle(SpellCastData data, object target, Spell spell)
        {
            if (EnchAlreadyOnObject(data.caster, spell, data.battle)) return false;

            if (data.battle != null)
            {

                if (!IsTownProtected(data.GetWizardID(), spell, data.battle)) return true;
            }
            return false;
        }

        static public bool STAR_BattlePrayerHighPrayer(SpellCastData data, object target, Spell spell)
        {
            if (!STAR_Battle(data, target, spell)) return false;

            foreach (var e in spell.enchantmentData)
            {
                //if high prayer is already on battle, do not allow to cast prayer
                if (e == (Enchantment)ENCH.PRAYER_GLOBAL)
                {
                    var battle = target as Battle;
                    var enchsOnBattle = battle.GetEnchantments();
                    if (enchsOnBattle != null)
                    {
                        var hpg = enchsOnBattle.Find(o => o.source == (Enchantment)ENCH.HIGH_PRAYER_GLOBAL &&
                                                    o.owner.ID == data.GetWizardID());
                        if (hpg != null) return false;
                    }
                }
            }

            return true;
        }

        static public bool STAR_WallFriendlyTownOrWallTownBattle(SpellCastData data, object target, Spell spell)
        {
            if (target is Hex)
            {
                var hex = target as Hex;
                foreach (var l in GameManager.Get().registeredLocations)
                {
                    if (l is TownLocation && hex.Position == l.GetPosition())
                    {
                        if (data.GetWizardID() == l.GetOwnerID())
                        {
                            if (EnchAlreadyOnObject(data.caster, spell, l as TownLocation)) return false;
                            return true;
                        }
                    }
                }
            }

            if (data.battle != null)
            {
                if (data.battle.gDefender == null) return false;
                var loc = data.battle.gDefender.GetLocationHostSmart();

                if (data.GetWizardID() == data.battle.defender.GetID() &&
                    loc is TownLocation)
                {
                    if (EnchAlreadyOnObject(data.caster, spell, data.battle)) return false;

                    return true;
                }
            }
            return false;
        }
        static public bool STAR_WorldHex(SpellCastData data, object target, Spell spell)
        {
            var h = target as Hex;
            if (h != null)
            {
                WorldCode.Plane p = World.GetArcanus();
                if (p.GetHexAt(h.Position) != h)
                {
                    p = World.GetMyrror();
                    if (p.GetHexAt(h.Position) != h)
                    {
                        Debug.LogError("STAR_WorldHex: cannot find hex");
                        return false;
                    }
                }
                return true;
            }
            else
            {
                Debug.Log("Spell is designed to target World Hex");
            }

            return false;
        }
        static public bool STAR_AnimateDeath(SpellCastData data, object target, Spell spell)
        {
            if (data.battle != null)
            {
                /*conditions 
                * 1) unit is dead; 
                * 2) unit isn't Hero; 
                * 3) unit isn't from death realm; 
                * 4) unit isn't battle summon; 
                * 5) unit wasn't slain mostly by irreversible damages; 
                * 6) if enemy - unit hasn't magic immunity*/
                int totalHp;
                foreach (var u in ListUtils.MultiEnumerable( data.GetFriendlyUnits(), data.GetEnemyUnits() ))
                {
                    totalHp = u.GetBaseFigure().maxHitPoints * u.maxCount;

                    if (!u.IsAlive() && !(u.dbSource.Get() is Hero) &&
                        u.race != (Race)RACE.REALM_DEATH &&
                        !u.summon &&
                        u.irreversibleDamages < totalHp / 2)
                    {
                        if (data.GetPlayerWizard() != u.GetWizardOwner() &&
                            u.GetAttributes().Contains(TAG.MAGIC_IMMUNITY))
                        {
                            continue;
                        }
                        if (!data.battle.simulation)
                        {
                            //skip if something stands on this unit place
                            bool skip = false;
                            foreach (var s in data.battle.buToSource)
                            {
                                if (s.Key.IsAlive() && s.Key.GetPosition() == u.GetPosition())
                                {
                                    skip = true;
                                    break;
                                }
                            }
                            if (skip) continue;
                        }
                        return true;
                    }
                }               
            }
            else
            {
                Debug.Log("Spell is designed to work in the battle");
            }
            return false;
        }
        static public bool STAR_Reconstruct(SpellCastData data, object target, Spell spell)
        {
            if (data.battle != null)
            {
                /*conditions 
                * 1) unit is dead; 
                * 2) unit isn't Hero;
                * 4) unit isn't battle summon; 
                * 5) unit wasn't slain mostly by irreversible damages; 
                * 6) unit was Realm Tech*/

                int ownetID = data.GetWizardID();
                List<BattleUnit> ownerUnits;

                if (data.battle.attacker.GetID() == ownetID)
                {
                    ownerUnits = data.battle.attackerUnits;
                }
                else
                {
                    ownerUnits = data.battle.defenderUnits;
                }

                foreach (var u in ownerUnits)
                {
                    int totalHp = u.GetBaseFigure().maxHitPoints * u.maxCount;

                    if (!u.IsAlive() && !(u.dbSource.Get() is Hero) &&
                        u.GetAttFinal(TAG.MECHANICAL_UNIT) > FInt.ZERO &&
                        data.battle.buToSource[u].group != null &&
                        u.irreversibleDamages < totalHp / 2)
                    {
                        return true;
                    }
                }
            }
            else
            {
                Debug.Log("Spell is designed to work in the battle");
            }
            return false;
        }

        static public bool STAR_RaiseDeath(SpellCastData data, object target, Spell spell)
        {
            List<BattleUnit> units = data.GetFriendlyUnits();
            foreach (var u in units)
            {
                if (!u.IsAlive() && 
                    !u.dbSource.Get().unresurrectable && 
                    u.GetAttributes().DoesNotContains((Tag)TAG.FANTASTIC_CLASS))
                {
                    if (data.battle != null && !data.battle.simulation)
                    {
                        //skip if something stands on this unit place
                        bool skip = false;
                        foreach (var v in data.battle.buToSource)
                        {
                            if (v.Key.IsAlive() && v.Key.GetPosition() == u.GetPosition())
                            {
                                skip = true;
                                break;
                            }
                        }
                        if (skip) continue;
                    }
                    return true;
                }
            }
            
            return false;
        }
        static public bool STAR_MassInvisibility(SpellCastData data, object target, Spell spell)
        {
            if (data.battle != null)
            {
                List<BattleUnit> units = data.GetFriendlyUnits();
                foreach (var u in units)
                {
                    if (u.GetEnchantments().Find(o => o.source == (Enchantment)ENCH.INVISIBILITY_SKILL ||
                    o.source == (Enchantment)ENCH.INVISIBILITY_SPELL) == null)
                    {
                        return true;
                    }
                }
            }
            else
            {
                Debug.Log("Spell is designed to work in the battle");
            }
            return false;
        }
        static public bool STAR_GameMenager(SpellCastData data, object target, Spell spell)
        {
            if (GameManager.Get() == target && spell != null)
            {
                var gameManager = GameManager.Get();
                foreach (var e in spell.enchantmentData)
                {
                    if (EnchAlreadyOnObject(data.caster, spell, gameManager)) return false;
                }
                return true;
            }
            else
            {
                Debug.Log("Spell is designed to work in the world");
            }
            return false;
        }
        static public bool STAR_Resurrection(SpellCastData data, object target, Spell spell)
        {
            if (GameManager.Get() != null)
            {
                var owner = data.GetPlayerWizard();
                var deadHeros = owner.GetDeadHeroes();
                if (owner != null && deadHeros.Count > 0 &&
                    owner.heroes.Count < 6)
                {
                    if (deadHeros.Find(o => o.dbSource.Get().unresurrectable == false) == null)
                    {
                        return false;
                    }
                    return true;
                }
            }
            else
            {
                Debug.Log("Spell is designed to work in the world");
            }
            return false;
        }
        static public bool STAR_AstralGate(SpellCastData data, object target, Spell spell)
        {
            var w = data.GetPlayerWizard();
            if (target is TownLocation)
            {
                var tl = target as TownLocation;
                if (w.ID == tl.GetOwnerID())
                {
                    if (EnchAlreadyOnObject(data.caster, spell, tl)) return false;

                    var plane = World.GetOtherPlane(tl.GetPlane());
                    if (GameManager.Get().GetLocationAt(tl.GetPosition(), plane) != null)
                    {
                        return false;
                    }
                    if (GameManager.Get().GetGroupAt(tl.GetPosition(), plane) != null)
                    {
                        return false;
                    }
                    return true;
                }
            }
            return false;
        }
        static public bool STAR_WordOfRecall(SpellCastData data, object target, Spell spell)
        {
            var w = data.GetPlayerWizard();
            if (w == null) return false;

            if (target is MOM.Unit && w.summoningCircle != null)
            {
                var u = target as MOM.Unit;
                if (u.GetAttributes().Contains(TAG.SHIP) &&
                    u.GetAttributes().DoesNotContains((Tag)TAG.CAN_FLY)) return false;

                if (w.ID == u.group.Get().GetOwnerID())
                {
                    return true;
                }
            }
            else if (target is BattleUnit &&
                     w.summoningCircle != null &&
                     data.battle != null &&
                     data.battle.gDefender != null &&
                     data.battle.gDefender.GetLocationHostSmart() != w.summoningCircle.Get())
            {
                var bu = target as BattleUnit;

                if (bu.GetAttributes().Contains(TAG.SHIP) &&
                   bu.GetAttributes().DoesNotContains((Tag)TAG.CAN_FLY)) return false;

                if (w.ID == bu.ownerID && data.battle != null)
                {
                    if (data.battle.buToSource[bu].group != null &&
                        data.battle.buToSource[bu].group.Get().GetOwnerID() == w.GetID()) return true;
                }
            }
            return false;
        }
        static public bool STAR_RecallHero(SpellCastData data, object target, Spell spell)
        {
            var w = data.GetPlayerWizard();
            if (w == null) return false;
            if (target is MOM.Unit && (target as MOM.Unit).GetAttFinal(TAG.HERO_CLASS) > 0
                && w.summoningCircle != null)
            {
                var u = target as MOM.Unit;
                if (data.GetWizardID() == u.group.Get().GetOwnerID())
                {
                    return true;
                }
            }
            else if (target is BattleUnit && (target as BattleUnit).GetAttFinal(TAG.HERO_CLASS) > 0
                && w.summoningCircle != null)
            {
                var bu = target as BattleUnit;

                if (data.GetWizardID() == bu.ownerID && data.battle != null)
                {
                    if (data.battle.buToSource[bu].group != null) return true;
                }
            }
            return false;
        }
        static public bool STAR_SummonHero(SpellCastData data, object target, Spell spell)
        {
            var w = data.GetPlayerWizard();
            if (w == null) return false;

            if (w.heroes.Count < w.GetMaxHeroCount())
            {
                List<Hero> heroesList = new List<Hero>(DataBase.GetType<Hero>());
                heroesList = heroesList.FindAll(o => !o.champion && !MOM.Unit.HeroInUseByWizard(o, w.GetID()));
                if (heroesList.Count > 0)
                {
                    return true;
                }
                else
                {
                    if (w.IsHuman)
                        PopupGeneral.OpenPopup(null, "UI_HERO_SUMMONING_FAILED", "UI_HERO_NOT_AVAILABLE", "UI_OK");

                    return false;
                }
            }
            else
            {
                if (w.IsHuman)
                    PopupGeneral.OpenPopup(null, "UI_HERO_SUMMONING_FAILED", "UI_HERO_LIST_FULL", "UI_OK");

                return false;
            }

            return false;
        }
        static public bool STAR_SummonChampion(SpellCastData data, object target, Spell spell)
        {
            var w = data.GetPlayerWizard();
            if (w == null) return false;

            if (w.heroes.Count < w.GetMaxHeroCount())
            {
                List<Hero> heroesList = new List<Hero>(DataBase.GetType<Hero>());
                heroesList = heroesList.FindAll(o => o.champion && !o.unresurrectable && !MOM.Unit.HeroInUseByWizard(o, w.GetID()));
                if (w.GetAttFinal((Tag)TAG.LIFE_MAGIC_BOOK) <= 0)
                {
                    if (heroesList.Find(o => o == (Hero)HERO.ELANA) != null)
                        heroesList.Remove((Hero)HERO.ELANA);
                    if (heroesList.Find(o => o == (Hero)HERO.ROLAND) != null)
                        heroesList.Remove((Hero)HERO.ROLAND);
                }
                if (w.GetAttFinal((Tag)TAG.DEATH_MAGIC_BOOK) <= 0)
                {
                    if (heroesList.Find(o => o == (Hero)HERO.MORTU) != null)
                        heroesList.Remove((Hero)HERO.MORTU);
                    if (heroesList.Find(o => o == (Hero)HERO.RAVASHACK) != null)
                        heroesList.Remove((Hero)HERO.RAVASHACK);
                }


                if (heroesList.Count > 0)
                {
                    return true;
                }
                else
                {
                    if (w.IsHuman)
                        PopupGeneral.OpenPopup(null, "UI_HERO_SUMMONING_FAILED", "UI_HERO_NOT_AVAILABLE", "UI_OK");

                    return false;
                }
            }
            else
            {
                if (w.IsHuman)
                    PopupGeneral.OpenPopup(null, "UI_HERO_SUMMONING_FAILED", "UI_HERO_LIST_FULL", "UI_OK");

                return false;
            }

            return false;
        }
        static public bool STAR_Heroism(SpellCastData data, object target, Spell spell)
        {
            if (STAR_FriendlyNonFantastic_Unit(data, target, spell))
            {
                var u = target as MOM.BaseUnit;
                return u.xp < 120;
            }
            else
            {
                return false;
            }
        }
        static public bool STAR_SpellBlast(SpellCastData data, object target, Spell spell)
        {
            if (STAR_EnemyWizard(data, target, spell))
            {
                var w = target as PlayerWizard;
                if (w.GetMagicAndResearch().curentlyCastSpell == null) return false;
                if (w.GetMagicAndResearch().curentlyCastSpell == (Spell)SPELL.SPELL_OF_RETURN) return false;

                bool sufficentMana = w.GetMagicAndResearch().castingProgress <= data.GetPlayerWizard().mana;

                return sufficentMana;
            }
            else
            {
                return false;
            }
        }
        static public bool STAR_Incarnation(SpellCastData data, object target, Spell spell)
        {
            var w = data.GetPlayerWizard();
            var unit = DataBase.Get<DBDef.Hero>(spell.stringData[0], true);
            if (unit == null)
            {
                Debug.LogError("Unit " + spell.stringData + " not found in database");
                return false;
            }

            if (w.heroes.Find(o => o.Get().dbSource == unit) == null)
            {
                if (w.heroes.Count < w.GetMaxHeroCount())
                {
                    return true;
                }
                else
                {
                    if (w.IsHuman)
                        PopupGeneral.OpenPopup(null, "UI_HERO_SUMMONING_FAILED", "UI_HERO_LIST_FULL", "UI_OK");

                    return false;
                }
            }
            else
            {
                if (w.IsHuman)
                {
                    PopupGeneral.OpenPopup(null, "UI_INFO", DBUtils.Localization.Get("UI_TORIN_ALREADY_IN_SERVICE "), "UI_OK");
                }
                return false;
            }

        }
        static public bool STAR_UndeadHero(SpellCastData data, object target, Spell spell)
        {
            if (GameManager.Get() != null)
            {
                var owner = data.GetPlayerWizard();
                var deadHeros = owner.GetDeadHeroes();
                if (owner != null && deadHeros.Count > 0 &&
                    owner.heroes.Count < 6)
                {
                    if (deadHeros.Find(o => o.dbSource.Get().unresurrectable == false && o.attributes.GetFinal((Tag)TAG.FANTASTIC_CLASS) <= 0) != null)
                    {
                        return true;
                    }
                    else 
                        return false;
                }
            }
            else
            {
                Debug.Log("Spell is designed to work in the world");
            }
            return false;
        }

        #endregion
        #region Spell Battle Hex (SBH)
        static public bool SBH_MassSlow(SpellCastData data, object target, Spell spell)
        {
            if (!(target is Vector3i)) return false;
            var distance = spell.fIntData[0];

            var pos = (Vector3i)target;
            if (data.battle != null)
            {
                foreach (var v in data.battle.GetAllUnits())
                {
                    if (HexCoordinates.HexDistance(v.GetPosition(), pos) <= distance &&
                        v.attributes.DoesNotContains((Tag)TAG.TELEPORTING) &&
                        v.attributes.DoesNotContains((Tag)TAG.CAN_FLY) &&
                        v.attributes.DoesNotContains((Tag)TAG.NON_CORPOREAL) &&
                        v.GetSkills().Find(o => o == (Skill)SKILL.EARTH_WALKER) == null &&
                        v.GetEnchantments().Find(o => o.source == (Enchantment)ENCH.EARTH_TO_MUD) == null)
                    {
                        foreach (var en in spell.enchantmentData)
                        {
                            v.AddEnchantment(en, data.caster as Entity, en.lifeTime, null, spell.worldCost);
                        }
                    }
                }
            }
            return true;
        }

        static public bool SBH_BattleSummon(SpellCastData data, object target, Spell spell)
        {
            //summon chosen creature on v3i position
            if (spell.stringData == null || spell.stringData.Length < 1)
            {
                Debug.LogError("Spell " + spell.dbName + " miss summon info to work with");
                return false;
            }
            var unit = DataBase.Get<DBDef.Unit>(spell.stringData[0], true);
            if (unit == null)
            {
                Debug.LogError("Unit " + spell.stringData[0] + " not found in database");
                return false;
            }
            //             if (!(target is Vector3i))
            //             {
            //                 Debug.LogError("Target is not a location");
            //                 return false;
            //             }
            var pos = (Vector3i)target;
            //             if (!data.battle.IsLocationEmpty(pos))
            //             {
            //                 Debug.LogError("Target location occupied");
            //                 return false;
            //             }

            if (data.battle != null)
                data.battle.CreateSummon(data.GetWizardID(), unit, pos);
            else
                data.CreateSummon(data.GetWizardID(), unit);

            return true;
        }
        static public bool SBH_SummonVortex(SpellCastData data, object target, Spell spell)
        {
            if (data.battle == null) return false;

            var pos = (Vector3i)target;
            if (!data.battle.IsLocationEmpty(pos))
            {
                Debug.Log("Vortex: Target location occupied");
                return false;
            }

            bool attacker = false;
            if (data.caster is BattlePlayer)
            {
                attacker = data.caster == data.battle.attacker;
            }
            else if (data.battle.attackerUnits != null)
            {
                var bu = data.caster as BattleUnit;
                if (bu != null)
                {
                    attacker = data.battle.attackerUnits.Contains(bu);
                }
            }

            Vortex.CreateVortex(data.battle, pos, attacker);
            return true;
        }
        static public bool SBH_Disrupt(SpellCastData data, object target, Spell spell)
        {
            if (!(target is Vector3i) || data.battle == null) return false;

            var pos = (Vector3i)target;
            foreach (var wallPart in data.battle.battleWalls)
            {
                if (wallPart.position == pos)
                {
                    wallPart.AnimateDestroy();
                }
            }

            return true;
        }

        #endregion
        #region Spell Battle Group (SBG)

        /// Spells targeting battleUnit 
        /// Do not apply any changes to other variables by target
        /// Target is BattleUnit


        static public bool SBG_ApplyEnchantment(SpellCastData data, object target, Spell spell)
        {
            if (spell.enchantmentData == null)
            {
                Debug.LogWarning("SBG_EnchantBattleGroup requires " + spell.dbName + " to have enchantments data ");
                return false;
            }
            if (!(target is BattleUnit))
            {
                Debug.LogWarning("SBG_EnchantBattleGroup is not targeting unit in battle");
                return false;
            }
            var g = target as BattleUnit;

            foreach (var v in spell.enchantmentData)
            {
                g.AddEnchantment(v, data.caster as IEnchantable, v.lifeTime, null, spell.battleCost);
            }
            return true;
        }
        static public bool SBG_Inviolability(SpellCastData data, object target, Spell spell)
        {
            if(data.caster is BattleUnit)
            {
                var caster = data.caster as BattleUnit;
                int dmg = spell.fIntData[0].ToInt();
                int[] dmgBuffer = { dmg };
                bool def = caster.canDefend;
                caster.canDefend = false;
                caster.ApplyDamage(dmgBuffer, new MHRandom(), null, 0);
                caster.canDefend = def;
                SBG_ApplyEnchantment(data, target, spell);

                return true;
            }

            return false;
        }
        static public bool SBG_ApplyEnchantmentUpdateInvisibility(SpellCastData data, object target, Spell spell)
        {
            if (spell.enchantmentData == null)
            {
                Debug.LogWarning("SBG_EnchantBattleGroup requires " + spell.dbName + " to have enchantments data ");
                return false;
            }
            if (!(target is BattleUnit))
            {
                Debug.LogWarning("SBG_EnchantBattleGroup is not targeting unit in battle");
                return false;
            }
            var g = target as BattleUnit;

            foreach (var v in spell.enchantmentData)
            {
                g.AddEnchantment(v, data.caster as IEnchantable, v.lifeTime, null, spell.battleCost);
            }

            data.battle?.UpdateInvisibility();

            return true;
        }
        //That script on apply ench use target owner as a ench owner
        static public bool SBG_ApplyEnchantmentWithReversOwner(SpellCastData data, object target, Spell spell)
        {
            if (spell.enchantmentData == null)
            {
                Debug.LogWarning("SBG_ApplyEnchantmentWithReversOwner requires " + spell.dbName + " to have enchantments data ");
                return false;
            }
            if (!(target is BattleUnit))
            {
                Debug.LogWarning("SBG_ApplyEnchantmentWithReversOwner is not targeting unit in battle");
                return false;
            }
            var g = target as BattleUnit;

            foreach (var v in spell.enchantmentData)
            {
                g.AddEnchantment(v, g as IEnchantable, v.lifeTime, null, spell.battleCost);
            }
            return true;
        }
        static public bool SBG_ApplyMagicWallEnchantment(SpellCastData data, object target, Spell spell)
        {
            if (spell.enchantmentData == null)
            {
                Debug.LogWarning("SBG_ApplyMagicWallEnchantment requires " + spell.dbName + " to have enchantments data ");
                return false;
            }
            if (data.battle == null)
            {
                Debug.LogWarning("SBG_ApplyMagicWallEnchantment is not targeting battle");
                return false;
            }

            foreach (var v in spell.enchantmentData)
            {
                data.battle.AddEnchantment(v, data.caster as IEnchantable, v.lifeTime, null, spell.battleCost);
            }
            return true;
        }
        static public bool SBG_ResistCheckAgainstEnchantment(SpellCastData data, object target, Spell spell)
        {

            if (spell.enchantmentData == null)
            {
                Debug.LogWarning("SBG_EnchantBattleGroup requires " + spell.dbName + " to have enchantments data ");
                return false;
            }
            if (!(target is BattleUnit))
            {
                Debug.LogWarning("SBG_EnchantBattleGroup is not targeting unit in battle");
                return false;
            }

            // if caster is hero
            BattleUnit hero = data.GetCasterAsBattleUnit();

            FInt resRed = FInt.ZERO;
            if (spell.fIntData[0] != null)
            {
                resRed = spell.fIntData[0];
            }

            var targetBu = target as BattleUnit;
            var resistMod = -1 * resRed + ResistModFromEnch(hero, targetBu, spell);

            if (random.GetInt(1, 11) > targetBu.attributes.GetFinal(TAG.RESIST) + resistMod)
            {
                foreach (var v in spell.enchantmentData)
                {
                    targetBu.AddEnchantment(v, data.caster as IEnchantable, v.lifeTime, null, spell.battleCost);
                }
            }
            return true;
        }
        static public bool SBG_WarpCreature(SpellCastData data, object target, Spell spell)
        {

            if (spell.enchantmentData == null)
            {
                Debug.LogWarning("SBG_WarpCreature requires " + spell.dbName + " to have enchantments data ");
                return false;
            }
            if (!(target is BattleUnit))
            {
                Debug.LogWarning("SBG_WarpCreature is not targeting unit in battle");
                return false;
            }

            // if caster is hero
            BattleUnit hero = data.GetCasterAsBattleUnit();

            FInt resRed = FInt.ZERO;
            if (spell.fIntData != null && spell.fIntData.Length > 0)
            {
                resRed = spell.fIntData[0];
            }

            var targetBu = target as BattleUnit;

            var unitEnch = targetBu.GetEnchantments();
            var potentialEnch = new List<Enchantment>();

            foreach (var i in spell.enchantmentData)
                potentialEnch.Add(i);

            foreach (var e in unitEnch)
            {
                if (e == null) return false;
                var temp = e.source;

                if (potentialEnch.Contains(temp))
                    potentialEnch.Remove(temp);
            }

            if (potentialEnch.Count == 0) return false;

            var resistMod = -1 * resRed + ResistModFromEnch(hero, targetBu, spell);

            if (random.GetInt(0, 10) > targetBu.attributes.GetFinal(TAG.RESIST) + resistMod)
            {
                potentialEnch.RandomSortThreadSafe();
                targetBu.AddEnchantment(potentialEnch[0], data.caster as IEnchantable, potentialEnch[0].lifeTime, null, spell.battleCost);
            }

            return true;
        }

        static public bool SBG_IceBolt(SpellCastData data, object target, Spell spell)
        {
            if (spell.fIntData[0] == null || spell.fIntData.Length == 0)
            {
                Debug.LogWarning("SBG_IceBolt have miss FIntData");
                return false;
            }
            if (!(target is BattleUnit))
            {
                Debug.LogWarning("SBG_IceBolt is not targeting unit in battle");
                return false;
            }

            int[] dmg = new int[1] { spell.fIntData[0].ToInt() };

            //Add extra dmg if player used extra mana
            if (data.GetPlayerWizard() != null &&
                data.GetPlayerWizard().GetMagicAndResearch().extensionItemSpellBattle != null)
            {
                dmg[0] += data.GetPlayerWizard().magicAndResearch.extensionItemSpellBattle.extraPower;
            }

            dmg[0] = ProduceSpellDamage(dmg[0]);

            var unit = target as BattleUnit;
            if (unit.attributes.Contains(TAG.COLD_IMMUNITY) ||
                unit.attributes.Contains(TAG.MAGIC_IMMUNITY))
                unit.ApplyDamage(dmg, random, null, 50, true);
            else if (unit.attributes.Contains(TAG.ELEMENTAL_ARMOR))
                unit.ApplyDamage(dmg, random, null, 10);
            else if (unit.attributes.Contains(TAG.RESIST_ELEMENTS))
                unit.ApplyDamage(dmg, random, null, 3);
            else if (unit.attributes.Contains(TAG.LARGE_SHIELD))
                unit.ApplyDamage(dmg, random, null, 2);
            else
                unit.ApplyDamage(dmg, random, null, 0);

            return true;
        }
        static public bool SBG_Petrify(SpellCastData data, object target, Spell spell)
        {
            if (!(target is BattleUnit))
            {
                Debug.LogWarning("SBG_Petrify is not targeting unit in battle");
                return false;
            }
            // if caster is hero             
            int heroResistMod = 0;
            if (data.caster is BattleUnit)
            {
                BattleUnit hero = data.GetCasterAsBattleUnit();
                if (hero != null)
                {
                    heroResistMod = hero.attributes.GetFinal((Tag)TAG.SPELL_SAVE).ToInt();
                }
            }

            var unit = target as BattleUnit;

            if (unit.attributes.Contains(TAG.STONING_IMMUNITY) ||
                unit.attributes.Contains(TAG.MAGIC_IMMUNITY))
                unit.ApplyResistFigureDeath(random, heroResistMod, 50, true, null, null, spell);
            else if (unit.attributes.Contains(TAG.ELEMENTAL_ARMOR))
                unit.ApplyResistFigureDeath(random, heroResistMod, 10, false, null, null, spell);
            else if (unit.attributes.Contains(TAG.RESIST_ELEMENTS))
                unit.ApplyResistFigureDeath(random, heroResistMod, 3, false, null, null, spell);
            else
                unit.ApplyResistFigureDeath(random, heroResistMod, 0, false, null, null, spell);

            unit.GetOrCreateFormation(null, false)?.UpdateFigureCount();

            return true;
        }
        static public bool SBG_DispelEvil(SpellCastData data, object target, Spell spell)
        {
            if (!(target is BattleUnit))
            {
                Debug.LogWarning("SBG_DispelEvil is not targeting unit in battle");
                return false;
            }

            // if caster is hero
            int heroResistMod = 0;
            if (data.caster is BattleUnit)
            {
                BattleUnit hero = data.GetCasterAsBattleUnit();
                if (hero != null)
                {
                    heroResistMod = hero.attributes.GetFinal((Tag)TAG.SPELL_SAVE).ToInt();
                }
            }

            var unit = target as BattleUnit;

            if (unit.attributes.Contains(TAG.REANIMATED))
                unit.ApplyResistFigureDeath(random, 9 + heroResistMod, 0, false, null, null, spell);
            else
                unit.ApplyResistFigureDeath(random, 4 + heroResistMod, 0, false, null, null, spell);

            unit.GetOrCreateFormation(null, false)?.UpdateFigureCount();

            return true;
        }
        static public bool SBG_Banish(SpellCastData data, object target, Spell spell)
        {
            if (!(target is BattleUnit))
            {
                Debug.LogWarning("SBG_Banish is not targeting unit in battle");
                return false;
            }

            int resMod = spell.fIntData[0].ToInt();

            //Add extra resMod if caster used extra mana
            if (data.GetPlayerWizard() != null &&
                data.GetPlayerWizard().GetMagicAndResearch().extensionItemSpellBattle != null)
            {
                resMod += data.GetPlayerWizard().magicAndResearch.extensionItemSpellBattle.extraPower;
            }

            // if caster is hero
            int heroResistMod = 0;
            if (data.caster is BattleUnit)
            {
                BattleUnit hero = data.GetCasterAsBattleUnit();
                if (hero != null)
                {
                    heroResistMod = hero.attributes.GetFinal((Tag)TAG.SPELL_SAVE).ToInt();
                }
            }


            var unit = target as BattleUnit;

            unit.ApplyResistFigureDeath(random, resMod + heroResistMod, 0, false, null, null, spell);
            unit.GetOrCreateFormation(null, false)?.UpdateFigureCount();

            return true;
        }

        static public bool SBG_AddSkill(SpellCastData data, object target, Spell spell)
        {
            if (!(target is BattleUnit))
            {
                Debug.LogWarning("SBG_AddSkill is not targeting unit in battle");
                return false;
            }

            var unit = target as BattleUnit;
            DBDef.Skill skill = null;

            var skillName = spell.stringData;
            DBClass s = DataBase.Get(skillName[1], false);
            if (s is Skill)
                skill = s as Skill;
            else
                return false;

            unit.AddSkill(skill);

            return true;
        }

        static public bool SBG_WarpWood(SpellCastData data, object target, Spell spell)
        {
            if (!(target is BattleUnit))
            {
                Debug.LogWarning("SBG_WarpWood is not targeting unit in battle");
                return false;
            }

            var unit = target as BattleUnit;

            unit.GetCurentFigure().rangedAmmo = 0;
            unit.GetAttributes().GetDirty();

            return true;
        }

        static public bool SBG_FireBolt(SpellCastData data, object target, Spell spell)
        {
            int[] dmg = new int[1];

            //That setup is made for Call Chaos spell.
            if (spell.fIntData != null && spell.fIntData[0] != null)
                dmg[0] = spell.fIntData[0].ToInt();
            if (dmg[0] == 0) dmg[0] = 15;

            if (!(target is BattleUnit))
            {
                Debug.LogWarning("SBG_FireBolt is not targeting unit in battle");
                return false;
            }

            //Add extra dmg if player used extra mana
            if (data.GetPlayerWizard() != null &&
                data.GetPlayerWizard().GetMagicAndResearch().extensionItemSpellBattle != null)
            {
                dmg[0] += data.GetPlayerWizard().magicAndResearch.extensionItemSpellBattle.extraPower;
            }

            dmg[0] = ProduceSpellDamage(dmg[0]);

            var unit = target as BattleUnit;
            if (unit.attributes.Contains(TAG.FIRE_IMMUNITY) ||
                unit.attributes.Contains(TAG.MAGIC_IMMUNITY) ||
                unit.attributes.Contains(TAG.RIGHTEOUSNESS))
                unit.ApplyDamage(dmg, random, null, 50, true, null, null, spell);
            else if (unit.attributes.Contains(TAG.ELEMENTAL_ARMOR))
                unit.ApplyDamage(dmg, random, null, 10, false, null, null, spell);
            else if (unit.attributes.Contains(TAG.RESIST_ELEMENTS) ||
                unit.attributes.Contains(TAG.BLESS))
                unit.ApplyDamage(dmg, random, null, 3, false, null, null, spell);
            else if (unit.attributes.Contains(TAG.LARGE_SHIELD))
                unit.ApplyDamage(dmg, random, null, 2, false, null, null, spell);
            else
                unit.ApplyDamage(dmg, random, null, 0, false, null, null, spell);

            return true;
        }

        static public bool SBG_LightningBolt(SpellCastData data, object target, Spell spell)
        {
            if (spell.fIntData[0] == null || spell.fIntData.Length == 0)
            {
                Debug.LogWarning("SBG_LightningBolt have miss FIntData");
                return false;
            }
            if (!(target is BattleUnit))
            {
                Debug.LogWarning("SBG_LighteningBolt is not targeting unit in battle");
                return false;
            }

            int[] dmg = new int[1] { spell.fIntData[0].ToInt() };

            //Add extra dmg if player used extra mana
            if (data.GetPlayerWizard() != null &&
                data.GetPlayerWizard().GetMagicAndResearch().extensionItemSpellBattle != null)
            {
                dmg[0] += data.GetPlayerWizard().magicAndResearch.extensionItemSpellBattle.extraPower;
            }

            dmg[0] = ProduceSpellDamage(dmg[0]);

            var unit = target as BattleUnit;
            if (unit.attributes.Contains(TAG.LIGHTNING_WEAKNESS))
            {
                dmg[0] = dmg[0] * 2;
            }

            if (unit.attributes.Contains(TAG.MAGIC_IMMUNITY) ||
                unit.attributes.Contains(TAG.RIGHTEOUSNESS))
                unit.ApplyDamage(dmg, random, null, 50, true, null, null, spell);
            else if (unit.attributes.Contains(TAG.ELEMENTAL_ARMOR))
                unit.ApplyDamage(dmg, random, null, 10, false, null, null, spell);
            else if (unit.attributes.Contains(TAG.RESIST_ELEMENTS) ||
                unit.attributes.Contains(TAG.BLESS))
                unit.ApplyDamage(dmg, random, null, 3, false, null, null, spell);
            else if (unit.attributes.Contains(TAG.LARGE_SHIELD))
                unit.ApplyDamage(dmg, random, null, 2, false, null, null, spell);
            else
                unit.ApplyDamage(dmg, random, null, 0, false, null, null, spell);

            return true;
        }

        static public bool SBG_FireBall(SpellCastData data, object target, Spell spell)
        {
            if (spell.fIntData[0] == null && spell.fIntData.Length == 0)
            {
                Debug.LogWarning("SBG_FireBall have miss FIntData");
                return false;
            }
            if (!(target is BattleUnit))
            {
                Debug.LogWarning("SBG_FireBall is not targeting unit in battle");
                return false;
            }

            int[] dmg = new int[9];
            var baseDmg = spell.fIntData[0].ToInt();

            //Add extra dmg if player used extra mana
            if (data.GetPlayerWizard() != null &&
                data.GetPlayerWizard().GetMagicAndResearch().extensionItemSpellBattle != null)
            {
                dmg[0] += data.GetPlayerWizard().magicAndResearch.extensionItemSpellBattle.extraPower;
            }

            var unit = target as BattleUnit;
            dmg = ProduceAreaSpellDamage(baseDmg, dmg, unit.figureCount);

            if (unit.attributes.Contains(TAG.MAGIC_IMMUNITY) ||
                unit.attributes.Contains(TAG.FIRE_IMMUNITY) ||
                unit.attributes.Contains(TAG.RIGHTEOUSNESS))
                unit.ApplyImmolationDamage(dmg, random, null, 50, true, null, null, spell);
            else if (unit.attributes.Contains(TAG.ELEMENTAL_ARMOR))
                unit.ApplyImmolationDamage(dmg, random, null, 10, false, null, null, spell);
            else if (unit.attributes.Contains(TAG.RESIST_ELEMENTS) ||
                unit.attributes.Contains(TAG.BLESS))
                unit.ApplyImmolationDamage(dmg, random, null, 3, false, null, null, spell);
            else if (unit.attributes.Contains(TAG.LARGE_SHIELD))
                unit.ApplyImmolationDamage(dmg, random, null, 2, false, null, null, spell);
            else
                unit.ApplyImmolationDamage(dmg, random, null, 0, false, null, null, spell);

            return true;
        }

        static public bool SBG_WarpLightning(SpellCastData data, object target, Spell spell)
        {
            if (!(target is BattleUnit))
            {
                Debug.LogWarning("SBG_WarpLightning is not targeting unit in battle");
                return false;
            }

            var unit = target as BattleUnit;

            for (int i = 10; i > 0; i--)
            {
                int[] dmg = new int[1];

                for (int dmgCheck = 0; dmgCheck < i; dmgCheck++)
                {
                    if (random.GetFloat(0.0f, 1.0f) <= 0.3f)
                    {
                        dmg[0]++;
                    }
                }
                if (unit.attributes.Contains(TAG.LIGHTNING_WEAKNESS))
                {
                    dmg[0] = dmg[0] * 2;
                }


                if (unit.figureCount <= 0) break;

                if (unit.attributes.Contains(TAG.MAGIC_IMMUNITY) ||
                    unit.attributes.Contains(TAG.RIGHTEOUSNESS))
                    unit.ApplyDamage(dmg, random, null, 50, true, null, null, spell);
                else if (unit.attributes.Contains(TAG.ELEMENTAL_ARMOR))
                    unit.ApplyDamage(dmg, random, null, 10, false, null, null, spell);
                else if (unit.attributes.Contains(TAG.RESIST_ELEMENTS) ||
                    unit.attributes.Contains(TAG.BLESS))
                    unit.ApplyDamage(dmg, random, null, 3, false, null, null, spell);
                else if (unit.attributes.Contains(TAG.LARGE_SHIELD))
                    unit.ApplyDamage(dmg, random, null, 2, false, null, null, spell);
                else
                    unit.ApplyDamage(dmg, random, null, 0, false, null, null, spell);

            }
            return true;
        }

        static public bool SBG_DoomBolt(SpellCastData data, object target, Spell spell)
        {
            if (!(target is BattleUnit))
            {
                Debug.LogWarning("SBG_DoomBolt is not targeting unit in battle");
                return false;
            }
            int doomBoltDmg = 10;
            int[] dmg = new int[1] { doomBoltDmg };

            var unit = target as BattleUnit;
            unit.canDefend = false;
            unit.ApplyDamage(dmg, random, null, 0, false, null, null, spell);
            unit.canDefend = true;

            return true;
        }

        static public bool SBG_Disintegrate(SpellCastData data, object target, Spell spell)
        {


            if (!(target is BattleUnit))
            {
                Debug.LogWarning("SBG_Disintegrate is not targeting unit in battle");
                return false;
            }

            // if caster is hero
            int heroResistMod = 0;
            if (data.caster is BattleUnit)
            {
                BattleUnit hero = data.GetCasterAsBattleUnit();
                if (hero != null)
                {
                    heroResistMod = hero.attributes.GetFinal((Tag)TAG.SPELL_SAVE).ToInt();
                }
            }

            var unit = target as BattleUnit;
            if (unit.attributes.Contains(TAG.ELEMENTAL_ARMOR))
                unit.ApplyResistDisintegrate(random, heroResistMod, 10, false, null, null, spell);
            else if (unit.attributes.Contains(TAG.RESIST_ELEMENTS) ||
                    unit.attributes.Contains(TAG.BLESS))
                unit.ApplyResistDisintegrate(random, heroResistMod, 3, false, null, null, spell);
            else
                unit.ApplyResistDisintegrate(random, heroResistMod, 0, false, null, null, spell);

            return true;
        }

        static public bool SBG_StarFires(SpellCastData data, object target, Spell spell)
        {
            if (spell.fIntData[0] == null || spell.fIntData.Length == 0)
            {
                Debug.LogWarning("SBG_StarFires have miss FIntData");
                return false;
            }
            if (!(target is BattleUnit))
            {
                Debug.LogWarning("SBG_StarFires is not targeting unit in battle");
                return false;
            }

            int[] dmg = new int[1] { spell.fIntData[0].ToInt() };
            dmg[0] = ProduceSpellDamage(dmg[0]);



            var unit = target as BattleUnit;
            if (unit.attributes.Contains(TAG.MAGIC_IMMUNITY))
                unit.ApplyDamage(dmg, random, null, 50, true, null, null, spell);
            else if (unit.attributes.Contains(TAG.LARGE_SHIELD))
                unit.ApplyDamage(dmg, random, null, 2, false, null, null, spell);
            else
                unit.ApplyDamage(dmg, random, null, 0, false, null, null, spell);

            return true;
        }

        static public bool SBG_Heal(SpellCastData data, object target, Spell spell)
        {
            if (spell.fIntData[0] == null || spell.fIntData.Length == 0)
            {
                Debug.LogWarning("SBG_Heal have miss FIntData");
                return false;
            }
            int heal = spell.fIntData[0].ToInt();

            if (!(target is BattleUnit))
            {
                Debug.LogWarning("SBG_Heal is not targeting unit in battle");
                return false;
            }

            var unit = target as BattleUnit;
            unit.HealUnit(heal);

            return true;
        }

        static public bool SBG_PsionicBlast(SpellCastData data, object target, Spell spell)
        {
            if (spell.fIntData[0] == null || spell.fIntData.Length == 0)
            {
                Debug.LogWarning("SBG_PsionicBlast have miss FIntData");
                return false;
            }

            if (!(target is BattleUnit))
            {
                Debug.LogWarning("SBG_PsionicBlast is not targeting unit in battle");
                return false;
            }


            int[] dmg = new int[1] { spell.fIntData[0].ToInt() };

            //Add extra dmg if player used extra mana
            if (data.GetPlayerWizard() != null &&
                data.GetPlayerWizard().GetMagicAndResearch().extensionItemSpellBattle != null)
            {
                dmg[0] += data.GetPlayerWizard().magicAndResearch.extensionItemSpellBattle.extraPower;
            }

            dmg[0] = ProduceSpellDamage(dmg[0]);

            var unit = target as BattleUnit;
            if (unit.attributes.Contains(TAG.MAGIC_IMMUNITY))
                unit.ApplyDamage(dmg, random, null, 50, true, null, null, spell);
            else if (unit.attributes.Contains(TAG.ILLUSIONS_IMMUNITY))
                unit.ApplyDamage(dmg, random, null, 0, false, null, null, spell);
            else
            {
                unit.canDefend = false;
                unit.ApplyDamage(dmg, random, null, 0, false, null, null, spell);
                unit.canDefend = true;
            }

            return true;

        }
        /*static public bool SBG_Haste(SpellCastData data, object target, Spell spell)
        {
            if (spell.enchantmentData == null)
            {
                Debug.LogWarning("SBG_Haste requires " + spell.dbName + " to have enchantments data ");
                return false;
            }
            if (!(target is BattleUnit))
            {
                Debug.LogWarning("SBG_Haste is not targeting unit in battle");
                return false;
            }
            var bu = target as BattleUnit;

            foreach (var v in spell.enchantmentData)
            {
                bu.AddEnchantment(v, data.caster as IEnchantable, v.lifeTime, null, spell.battleCost);
            }
//             int unitMove = bu.GetAttFinal((Tag)TAG.MOVEMENT_POINTS).ToInt();
//             bu.Mp += unitMove;

//             if (BattleHUD.GetSelectedUnit() == bu)
//             {
//                 MHEventSystem.TriggerEvent<BattleHUD>(BattleHUD.Get(), null);
//             }
            return true; 
        }*/


        static public bool SBG_DispelMagic(SpellCastData data, object target, Spell spell)
        {
            if (!(target is BattleUnit))
            {
                Debug.LogWarning("SBG_DispelMagic is not targeting unit in battle");
                return false;
            }

            FInt dispelStr = FInt.ZERO;
            ISpellCaster spellCaster = data.caster;

            if (spell.fIntData != null)
            {
                dispelStr = spell.fIntData[0];
            }

            var wiz = data.GetPlayerWizard();
            //Add extra dmg if caster used extra mana
            if (wiz != null &&
                wiz.GetMagicAndResearch().extensionItemSpellBattle != null)
            {
                dispelStr += wiz.magicAndResearch.extensionItemSpellBattle.extraPower;
            }

            var wizAsCaster = wiz == spellCaster ? wiz : null;

            var bu = target as BattleUnit;
            var enchList = bu.GetEnchantments();

            var unitOwner = bu.GetWizardOwner();


            if (bu.isSpellLock && unitOwner != wiz)
            {
                for (int i = enchList.Count - 1; i >= 0; i--)
                {
                    var ench = enchList[i].source.Get();

                    if (ench.scripts == null) continue;

                    var spellLockEnch = Array.Find(ench.scripts, o => o.tag == (Tag)TAG.SPELL_LOCK);

                    if (spellLockEnch != null)
                    {
                        if (GetDispelSuccess(wizAsCaster, enchList[i], dispelStr, spellLockEnch.fIntData))
                        {
                            if (!bu.simulated)
                            {
                                if (spellCaster == GameManager.GetHumanWizard())
                                {
                                    var message = DBUtils.Localization.Get("UI_ENCHANTMENT_REMOVED_SUCCESSFULLY", true, enchList[i].source.Get().GetDILocalizedName());
                                    PopupGeneral.OpenPopup(null, "UI_SUCCESS", message, "UI_OK");
                                }
                                else
                                {
                                    var message = DBUtils.Localization.Get("UI_AI_ENCHANTMENT_REMOVED_SUCCESSFULLY_FROM", true, enchList[i].source.Get().GetDILocalizedName(), bu.GetDescriptionInfo().GetLocalizedName());
                                    PopupGeneral.OpenPopup(null, "UI_SUCCESS", message, "UI_OK");
                                }

                            }

                            bu.RemoveEnchantment(enchList[i]);
                            //If unit have spelllock and other ench on it, you need to dispel
                            // spelllock first and in another dispel use rest.

                            break;
                        }
                    }
                }
            }
            else
            {
                string removedEnch = "";
                for (int i = enchList.Count - 1; i >= 0; i--)
                {
                    //Dispel only ench that allow to dispel.
                    if (enchList[i].source.Get().allowDispel == false) continue;
                    if (unitOwner == wiz && enchList[i].source.Get().mindControl) continue;

                    //Dispel only negative ench on own ba. Dispel only positive ench on enemy ba.
                    if (unitOwner == wiz && enchList[i].source.Get().enchCategory == EEnchantmentCategory.Negative ||
                        unitOwner != wiz && enchList[i].source.Get().enchCategory != EEnchantmentCategory.Negative)
                    {
                        if (GetDispelSuccess(wizAsCaster, enchList[i], dispelStr))
                        {
                            if (!bu.simulated)
                            {
                                var newRemEch = enchList[i].source.Get().GetDILocalizedName();
                                if (removedEnch.Length > 0)
                                {
                                    removedEnch = removedEnch + ", " + newRemEch;
                                }
                                else
                                {
                                    removedEnch = newRemEch;
                                }
                            }

                            bu.RemoveEnchantment(enchList[i]);
                        }
                    }
                }
                if (!bu.simulated)
                {
                    if (spellCaster.GetWizardOwner() == GameManager.GetHumanWizard()) //player is casting dispel
                    {
                        if (removedEnch != "")
                        {
                            if (removedEnch.Contains(", ")) //different text for 1 and more than 1 enchantments removed
                            {
                                var message = DBUtils.Localization.Get("UI_ENCHANTMENTS_REMOVED_SUCCESSFULLY", true, removedEnch);
                                PopupGeneral.OpenPopup(null, "UI_SUCCESS", message, "UI_OK");
                            }
                            else
                            {
                                var message = DBUtils.Localization.Get("UI_ENCHANTMENT_REMOVED_SUCCESSFULLY", true, removedEnch);
                                PopupGeneral.OpenPopup(null, "UI_SUCCESS", message, "UI_OK");
                            }
                        }
                        else
                        {
                            PopupGeneral.OpenPopup(null, "UI_DISPEL_FAILED", "UI_NO_ENCHANTMENTS_REMOVED", "UI_OK");
                        }
                    }
                    else     //enemy wizard is casting dispel during battle with player
                    {
                        if (removedEnch != "")
                        {
                            var message = DBUtils.Localization.Get("UI_AI_ENCHANTMENT_REMOVED_SUCCESSFULLY_FROM", true, removedEnch, bu.GetName());
                            PopupGeneral.OpenPopup(null, "UI_ENCHANTMENT_DISPELLED", message, "UI_OK");
                        }
                        else
                        {
                            PopupGeneral.OpenPopup(null, "UI_DISPEL_FAILED", "UI_AI_NO_ENCHANTMENTS_REMOVED", "UI_OK");
                        }
                    }
                }
            }
            return true;
        }

        static public bool SBG_DisenchantArea(SpellCastData data, object target, Spell spell)
        {

            FInt dispelStr = FInt.ZERO;
            PlayerWizard casterOwner = data.GetPlayerWizard();

            if (spell.fIntData != null)
            {
                dispelStr = spell.fIntData[0];
            }
            else
            {
                //In oMom dispel power was 100 for nightshade.
                //But our dispel is more powerfully (dispel work on units as well )
                //and each building that use nightshade add extra try.
                dispelStr = (FInt)50f;
            }

            ISpellCaster spellCaster = data.caster;
            var wiz = data.GetPlayerWizard();
            //Add extra dmg if player used extra mana
            if (wiz != null &&
                wiz.GetMagicAndResearch().extensionItemSpellBattle != null)
            {
                dispelStr += wiz.magicAndResearch.extensionItemSpellBattle.extraPower;
            }

            var wizAsCaster = wiz == spellCaster ? wiz : null;

            //remove enchantment from units
            List<BattleUnit> ownerUnits;
            List<BattleUnit> enemyUnits;

            ownerUnits = data.GetFriendlyUnits();
            enemyUnits = data.GetEnemyUnits();

            List<EnchantmentInstance> enchList;
            string removedEnch = "";

            for (int i = ownerUnits.Count - 1; i >= 0; i--)
            {
                if (ownerUnits.Count <= i) continue;

                enchList = ownerUnits[i].GetEnchantments();

                for (int j = enchList.Count - 1; j >= 0; j--)
                {
                    if (ownerUnits.Count <= i) continue;
                    if (enchList.Count <= j) continue;

                    //Dispel only ench that allow to dispel.
                    if (enchList[j].source.Get().allowDispel == false) continue;
                    if (enchList[j].source.Get().mindControl == true) continue;

                    if (enchList[j].source.Get().enchCategory == EEnchantmentCategory.Negative)
                    {
                        if (GetDispelSuccess(wizAsCaster, enchList[j], dispelStr))
                        {
                            if (!ownerUnits[i].simulated)
                            {
                                var newRemEch = enchList[j].source.Get().GetDILocalizedName();
                                if (removedEnch.Length > 0)
                                {
                                    removedEnch = removedEnch + ", " + newRemEch;
                                }
                                else
                                {
                                    removedEnch = newRemEch;
                                }
                            }

                            ownerUnits[i].RemoveEnchantment(enchList[j].source.Get());
                        }
                    }
                }
            }

            for (int i = enemyUnits.Count - 1; i >= 0; i--)
            {
                if (enemyUnits.Count <= i) continue;
                enchList = enemyUnits[i].GetEnchantments();

                if (enemyUnits[i].isSpellLock)
                {
                    for (int j = 0; j < enchList.Count; j++)
                    {
                        if (enemyUnits.Count <= i) continue;
                        if (enchList.Count <= j) continue;

                        //Dispel only ench that allow to dispel.
                        if (enchList[j].source.Get().allowDispel == false) continue;

                        var ench = enchList[j].source.Get();

                        if (ench.scripts == null) continue;
                        var s = Array.Find(enchList[j].source.Get().scripts, o => o.tag == (Tag)TAG.SPELL_LOCK);
                        if (s != null)
                        {
                            if (GetDispelSuccess(wizAsCaster, enchList[j], dispelStr, s.fIntData))
                            {
                                if (!enemyUnits[i].simulated)
                                {
                                    var newRemEch = enchList[j].source.Get().GetDILocalizedName();
                                    if (removedEnch.Length > 0)
                                    {
                                        removedEnch = removedEnch + ", " + newRemEch;
                                    }
                                    else
                                    {
                                        removedEnch = newRemEch;
                                    }
                                }

                                enemyUnits[i].RemoveEnchantment(enchList[j].source);
                            }

                            break;
                        }
                    }
                }
                else
                {
                    for (int j = enchList.Count - 1; j >= 0; j--)
                    {
                        if (enemyUnits.Count <= i) continue;
                        if (enchList.Count <= j) continue;

                        //Dispel only ench that allow to dispel.
                        if (enchList[j].source.Get().allowDispel == false) continue;

                        if (enchList[j].owner != wiz)
                        {
                            if (GetDispelSuccess(wizAsCaster, enchList[j], dispelStr))
                            {
                                if (!enemyUnits[i].simulated)
                                {
                                    var newRemEch = enchList[j].source.Get().GetDILocalizedName();
                                    if (removedEnch.Length > 0)
                                    {
                                        removedEnch = removedEnch + ", " + newRemEch;
                                    }
                                    else
                                    {
                                        removedEnch = newRemEch;
                                    }
                                }

                                enemyUnits[i].RemoveEnchantment(enchList[j].source.Get());
                            }
                        }
                    }
                }
            }
            if (data.battle != null)
            {
                //remove enchantment from battlefield 
                if (IsTownProtected(data.GetWizardID(), spell, data.battle))
                {
                    return true;
                }


                enchList = data.battle.GetEnchantments();

                for (int i = enchList.Count - 1; i >= 0; i--)
                {
                    if (enchList.Count <= i) continue;

                    if (enchList[i].owner != wiz)
                    {
                        //check if caster is a unit
                        BattleUnit bu = enchList[i].owner?.GetEntity() as BattleUnit;
                        if (bu != null)
                        {
                            if (bu.GetWizardOwner() == wiz) continue;
                        }

                        //Dispel only ench that allow to dispel.
                        if (enchList[i].source.Get().allowDispel == false) continue;

                        if (GetDispelSuccess(wizAsCaster, enchList[i], dispelStr))
                        {

                            if (!data.battle.simulation)
                            {
                                var newRemEch = enchList[i].source.Get().GetDILocalizedName();
                                if (removedEnch.Length > 0)
                                {
                                    removedEnch = removedEnch + ", " + newRemEch;
                                }
                                else
                                {
                                    removedEnch = newRemEch;
                                }
                            }

                            data.battle.RemoveEnchantment(enchList[i].source.Get());
                        }
                    }
                }
            }

            if (!data.battle.simulation)
            {
                if (data.battle.attacker.GetWizardOwner() == GameManager.GetHumanWizard() ||
                    data.battle.defender.GetWizardOwner() == GameManager.GetHumanWizard())
                {
                    if (spellCaster.GetWizardOwner() == GameManager.GetHumanWizard())
                    {
                        if (!string.IsNullOrEmpty(removedEnch))
                        {
                            if (removedEnch.Contains(", ")) //different text for 1 and more than 1 enchantments removed
                            {
                                var message = DBUtils.Localization.Get("UI_ENCHANTMENTS_REMOVED_SUCCESSFULLY", true, removedEnch);
                                PopupGeneral.OpenPopup(null, "UI_SUCCESS", message, "UI_OK");
                            }
                            else
                            {
                                var message = DBUtils.Localization.Get("UI_ENCHANTMENT_REMOVED_SUCCESSFULLY", true, removedEnch);
                                PopupGeneral.OpenPopup(null, "UI_SUCCESS", message, "UI_OK");
                            }
                        }
                        else
                        {
                            PopupGeneral.OpenPopup(null, "UI_DISPEL_FAILED", "UI_NO_ENCHANTMENTS_REMOVED", "UI_OK");
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(removedEnch))
                        {
                            var message = DBUtils.Localization.Get("UI_AI_ENCHANTMENTS_REMOVED_SUCCESSFULLY_FROM_GLOBAL", true, removedEnch);
                            PopupGeneral.OpenPopup(null, "UI_ENCHANTMENT_DISPELLED", message, "UI_OK");
                        }
                        else
                        {
                            PopupGeneral.OpenPopup(null, "UI_DISPEL_FAILED", "UI_AI_NO_ENCHANTMENTS_REMOVED", "UI_OK");
                        }
                    }
                }
            }
            return true;
        }

        static public bool SBG_LifeDrain(SpellCastData data, object target, Spell spell)
        {
            if (!(target is BattleUnit))
            {
                Debug.LogWarning("SBG_LifeDrain is not targeting unit in battle");
                return false;
            }

            // if caster is hero
            BattleUnit hero = data.GetCasterAsBattleUnit();

            var buTrg = target as BattleUnit;
            var resMod = ResistModFromEnch(hero, buTrg, spell).ToInt();

            //Add extra resMod if caster used extra mana
            if (data.GetPlayerWizard() != null &&
                data.GetPlayerWizard().GetMagicAndResearch().extensionItemSpellBattle != null)
            {
                resMod -= data.GetPlayerWizard().GetMagicAndResearch().extensionItemSpellBattle.extraPower;
            }

            int[] roll = new int[] { (random.GetInt(1, 11) - buTrg.GetAttFinal(TAG.RESIST) - resMod).ToInt() };

            if (roll[0] > 0)
            {
                var hpOld = buTrg.currentFigureHP + (buTrg.FigureCount() - 1) * buTrg.GetBaseFigure().maxHitPoints;

                buTrg.ApplyDamage(roll, random, null, 0, true, null, null, spell);

                var hpNew = buTrg.currentFigureHP + (buTrg.FigureCount() - 1) * buTrg.GetBaseFigure().maxHitPoints;
                var casterUnit = data.GetCasterAsBattleUnit();
                int resultDmg = hpOld - hpNew;

                if (resultDmg < 1) return true;
                if (casterUnit != null)
                {
                    casterUnit.HealUnit(resultDmg, true);
                }
                else if (data.battle != null && data.GetPlayerWizard() != null)
                {
                    if (data.GetPlayerWizard().GetTraits().Contains((Trait)TRAIT.ARCHMAGE))
                        data.GetPlayerWizard().castingSkillDevelopment += (int)(resultDmg * 4.5f);
                    else
                        data.GetPlayerWizard().castingSkillDevelopment += resultDmg * 3;
                }
            }
            return true;
        }

        static public bool SBG_WordOfDeath(SpellCastData data, object target, Spell spell)
        {
            // if caster is hero
            int heroResistMod = 0;
            if (data.caster is BattleUnit)
            {
                BattleUnit hero = data.GetCasterAsBattleUnit();
                if (hero != null)
                {
                    heroResistMod = hero.attributes.GetFinal((Tag)TAG.SPELL_SAVE).ToInt();
                }
            }


            int resisMod = (spell.fIntData[0] * FInt.N_ONE).ToInt();

            if (!(target is BattleUnit))
            {
                Debug.LogWarning("SBG_WordOfDeath is not targeting unit in battle");
                return false;
            }

            var buTrg = target as BattleUnit;

            if (buTrg.attributes.Contains(TAG.DEATH_IMMUNITY) ||
                buTrg.attributes.Contains(TAG.RIGHTEOUSNESS) ||
                buTrg.attributes.Contains(TAG.MAGIC_IMMUNITY))
                buTrg.ApplyResistFigureDeath(random, 0, 50 - resisMod - heroResistMod, true, null, null, spell);
            else if (buTrg.attributes.Contains(TAG.BLESS))
                buTrg.ApplyResistFigureDeath(random, 0, 3 - resisMod - heroResistMod, false, null, null, spell);
            else
                buTrg.ApplyResistFigureDeath(random, 0, resisMod - heroResistMod, false, null, null, spell);

            buTrg.GetOrCreateFormation(null, false)?.UpdateFigureCount();

            return true;
        }

        static public bool SBG_BlackSleep(SpellCastData data, object target, Spell spell)
        {

            if (!(target is BattleUnit))
            {
                Debug.LogWarning("SBG_BlackSleep is not targeting unit in battle");
                return false;
            }

            // if caster is hero
            BattleUnit hero = data.GetCasterAsBattleUnit();

            var resistmod = FInt.ZERO;
            if (spell.fIntData != null && spell.fIntData.Length > 0)
            {
                resistmod = spell.fIntData[0];
            }

            var targetBu = target as BattleUnit;
            var naturalResist = targetBu.attributes.GetFinal(TAG.RESIST);
            int roll = random.GetInt(1, 11) - (naturalResist + ResistModFromEnch(hero, targetBu, spell) - resistmod).ToInt();
            if (roll > 0)
            {
                foreach (var v in spell.enchantmentData)
                {
                    targetBu.AddEnchantment(v, data.caster as IEnchantable, v.lifeTime, null, spell.battleCost);
                }
            }
            return true;
        }

        static public bool SBG_Web(SpellCastData data, object target, Spell spell)
        {

            if (spell.enchantmentData == null)
            {
                Debug.LogWarning("SBG_EnchantBattleGroup requires " + spell.dbName + " to have enchantments data ");
                return false;
            }
            if (!(target is BattleUnit))
            {
                Debug.LogWarning("SBG_EnchantBattleGroup is not targeting unit in battle");
                return false;
            }

            // if caster is hero
            BattleUnit hero = data.GetCasterAsBattleUnit();

            FInt resistRed = FInt.ZERO;
            if (spell.fIntData[0] != null)
            {
                resistRed = spell.fIntData[0];
            }
            var targetBu = target as BattleUnit;
            int resistMod = (targetBu.attributes.GetFinal(TAG.RESIST) - resistRed + ResistModFromEnch(hero, targetBu, spell)).ToInt();
            int roll = random.GetInt(1, 11);
            int turnsInWeb = Math.Max(1, roll - resistMod) /*+ 1*/;

            targetBu.AddEnchantment(spell.enchantmentData[0], data.caster as IEnchantable, turnsInWeb, null, spell.battleCost);
            if (targetBu.GetEnchantments().Find(o => o.source.Get() == spell.enchantmentData[1]) == null)
            {
                targetBu.AddEnchantment(spell.enchantmentData[1], data.caster as IEnchantable, -1, null, spell.battleCost);
            }

            return true;
        }

        static public bool SBG_CrackCall(SpellCastData data, object target, Spell spell)
        {

            var ba = target as BattleUnit;

            if (ba != null && random.GetFloat(0, 1) < 0.25)
            {
                ba.figureCount = 0;

                //                 int[] dmg = new int[] { ba.figureCount * ba.GetCurentFigure().maxHitPoints };
                //                 ba.canDefend = false;
                //                 ba.ApplyDamage(dmg, random, null, 0);
                //                 ba.canDefend = true;

                if (data.battle != null)
                {
                    var position = ba.GetPosition();

                    foreach (var wallPart in data.battle.battleWalls)
                    {
                        if (HexCoordinates.HexDistance(position, wallPart.position) <= 1)
                        {
                            wallPart.AnimateDestroy();
                            return true;
                        }
                    }
                }
            }
            else
            {
                return false;
            }

            return true;
        }
        static public bool SBG_Disrupt(SpellCastData data, object target, Spell spell)
        {
            if (data.battle != null)
            {
                var wall = data.battle.battleWalls;
                foreach (var wallPart in wall)
                {
                    wallPart.AnimateDestroy();
                }
            }
            return true;
        }
        static public bool SBG_RaiseDeath(SpellCastData data, object target, Spell spell)
        {
            BattleUnit bu = target as BattleUnit;
            if(bu == null)
            {
                //no target chosen, this might happen ie by AI not having popup to select unit
                int value = 0;
                List<BattleUnit> units = data.GetFriendlyUnits();
                foreach (var u in units)
                {
                    if (!u.IsAlive() &&
                        !u.dbSource.Get().unresurrectable &&
                        u.GetAttributes().DoesNotContains((Tag)TAG.FANTASTIC_CLASS))
                    {
                        if (data.battle != null && !data.battle.simulation)
                        {
                            //skip if something stands on this unit place
                            bool skip = false;
                            foreach (var v in data.battle.buToSource)
                            {
                                if (v.Key.IsAlive() && v.Key.GetPosition() == u.GetPosition())
                                {
                                    skip = true;
                                    break;
                                }
                            }
                            if (skip) continue;
                        }

                        var value2 = u.GetBattleUnitValueFixedHP(0.5f);
                        if(value2 > value)
                        {
                            bu = u;
                            value = value2;
                        }
                    }
                }
                if (value == 0) return false;
            }

            if (bu == null || data.battle == null) return false;

            bu.HealUnit(bu.GetMaxFigureCount() * bu.GetCurentFigure().maxHitPoints / 2, true);
            bu.irreversibleDamages = 0;
            bu.undeadDamages = 0;
            bu.normalDamages = 0;

            if (!data.battle.simulation)
            {
                data.battle.TriggerJoinScripts(bu);
                data.battle.plane.ClearSearcherData();
            
                var formation = bu.GetOrCreateFormation(null, true);
                if(formation != null)
                {
                    formation.InstantMove();
                    formation.UpdateFigureCount();
                }
            
                BattleHUD.Get().BaseUpdate();
                VerticalMarkerManager.Get().Addmarker(bu);
            }
            return true;
        }
        static public bool SBG_AnimateDeath(SpellCastData data, object target, Spell spell)
        {
            BattleUnit bu = target as BattleUnit;
            if(bu == null)
            {
                //no target chosen, this might happen ie by AI not having popup to select unit
                /*conditions 
                * 1) unit is dead; 
                * 2) unit isn't Hero; 
                * 3) unit isn't from death realm; 
                * 4) unit isn't battle summon; 
                * 5) unit wasn't slain mostly by irreversible damages; 
                * 6) if enemy - unit hasn't magic immunity*/
                int totalHp;
                int value = 0;
                foreach (var bUnit in ListUtils.MultiEnumerable(data.GetFriendlyUnits(), data.GetEnemyUnits()))
                {
                    totalHp = bUnit.GetBaseFigure().maxHitPoints * bUnit.maxCount;

                    if (!bUnit.IsAlive() && !(bUnit.dbSource.Get() is Hero) &&
                        bUnit.race != (Race)RACE.REALM_DEATH &&
                        !bUnit.summon &&
                        bUnit.irreversibleDamages < totalHp / 2)
                    {
                        if (data.GetPlayerWizard() != bUnit.GetWizardOwner() &&
                            bUnit.GetAttributes().Contains(TAG.MAGIC_IMMUNITY))
                        {
                            continue;
                        }
                        if (!data.battle.simulation)
                        {
                            //skip if something stands on this unit place
                            bool skip = false;
                            foreach (var s in data.battle.buToSource)
                            {
                                if (s.Key.IsAlive() && s.Key.GetPosition() == bUnit.GetPosition())
                                {
                                    skip = true;
                                    break;
                                }
                            }
                            if (skip) continue;
                        }

                        var value2 = bUnit.GetBattleUnitValueFixedHP(1f);
                        if (value2 > value)
                        {
                            value = value2;
                            bu = bUnit;
                        }
                    }
                }
            }

            if (bu == null || data.battle == null) return false;

            if (data.battle.simulation)
            {
                //during simulation AnimateDeath is not modifying unit, just healing it and taking over control if needed. 
                if(data.GetEnemyUnits().Contains(bu))
                {
                    data.GetEnemyUnits().Remove(bu);
                    data.GetFriendlyUnits().Add(bu);
                    bu.ownerID = data.GetWizardID();
                    bu.attackingSide = data.IsCasterAttackingSide();
                }

                bu.HealUnit(bu.GetMaxFigureCount() * bu.GetCurentFigure().maxHitPoints, true);
                bu.normalDamages = 0;
                bu.irreversibleDamages = 0;
                bu.undeadDamages = 0;
                //special marker to ensure this can be corrected after battle ends if result is applied
                bu.isAnimatedInSimulation = true;
            }
            else
            {

                MOM.Unit u = data.battle.buToSource[bu];
                if(bu.GetWizardOwnerID() != data.GetWizardID())
                {
                    //this parameter prevents returning to original owner if the ownership changes
                    bu.isHopingToJoin = true;
                }
                AnimateDead(u, bu, data);

                if (bu.GetWizardOwner() != null)
                {
                    bu.GetWizardOwner().ModifyUnitSkillsByTraits(bu);
                }

                data.battle.UnitListsDirty();
                data.battle.UpdateInvisibility();            
            }
            return true;
        }
        static public bool SBG_Reconstruct(SpellCastData data, object target, Spell spell)
        {
            BattleUnit bu = target as BattleUnit;
            if (bu == null || data.battle == null) return false;

            //check if position is available, if not find one
            if (data.battle.plane.GetSearcherData().IsUnitAt(bu.GetPosition()))
            {
                bool valid = false;
                int i = 1;
                while (!valid)
                {
                    var pos = bu.GetSurroundingArea(i);
                    foreach (var v in pos)
                    {
                        if (!data.battle.plane.GetSearcherData().IsUnitAt(v))
                        {
                            bu.battlePosition = v;
                            valid = true;
                            break;
                        }
                    }
                    i++;
                }
            }


            var unit = data.battle.CreateSummon(data.GetWizardID(), bu.dbSource, bu.battlePosition);
            unit.xp = bu.xp;
            unit.CopyEnchantmentManagerFrom(bu);

            if (data.battle.attackerUnits.Contains(bu))
                data.battle.attackerUnits.Remove(bu);
            else
                data.battle.defenderUnits.Remove(bu);

            unit.customName = DBUtils.Localization.Get("DES_RECONSTRUCT_NAME_DES");

            data.battle.plane.ClearSearcherData();
            data.battle.UnitListsDirty();
            if (!data.battle.simulation)
            {
                BattleHUD.Get().BaseUpdate();
                VerticalMarkerManager.Get().Addmarker(unit);
            }
            return true;
        }
        static public bool SBG_WordOfRecall(SpellCastData data, object target, Spell spell)
        {
            if (!(target is BattleUnit))
            {
                Debug.LogWarning("SBG_WordOfRecall is not targeting Battle Unit.");
                return false;
            }

            if (data.battle == null || data.GetPlayerWizard() == null) return false;

            var bu = target as MOM.BattleUnit;
            var u = data.battle.buToSource[bu];

            if (data.GetPlayerWizard().summoningCircle != null)
            {
                if (data.battle.attackerUnits.Contains(bu))
                {
                    data.battle.AttackerRemoveUnit(bu);
                    var attackers = data.battle.attackerUnits;

                    if (attackers.Count > 0 && data.GetPlayerWizard().GetID() == PlayerWizard.HumanID())
                        BattleHUD.Get().SelectUnit(attackers[0], true, true);
                }
                if (data.battle.defenderUnits.Contains(bu))
                {
                    data.battle.DefenderRemoveUnit(bu);
                    var defenders = data.battle.defenderUnits;

                    if (defenders.Count > 0 && data.GetPlayerWizard().GetID() == PlayerWizard.HumanID())
                        BattleHUD.Get().SelectUnit(defenders[0], true, true);
                }

                data.battle.buToSource.Remove(bu);
                data.battle.UpdateInvisibility();
                bu.GetOrCreateFormation(null, false)?.Destroy();

                var summoningCircle = data.GetPlayerWizard().summoningCircle.Get();
                var gr = summoningCircle.GetLocalGroup();
                u.figureCount = bu.FigureCount();
                u.currentFigureHP = bu.currentFigureHP;

                u.group.Get().TransferUnit(gr, u);

            }

            return true;
        }
        #endregion
        #region Spell World Group Or Town (SWG)
        static public bool SWG_ApplyEnchantment(ISpellCaster source, object target, Spell spell)
        {
            if (spell.enchantmentData == null)
            {
                Debug.LogWarning("SWG_ApplyEnchantment requires " + spell.dbName + " to have enchantments data ");
                return false;
            }

            if (!(target is MOM.Unit) && !(target is MOM.Location))
            {
                Debug.LogWarning("SWG_ApplyEnchantment is not targeting unit or Location");
                return false;
            }

            if (target is MOM.Unit)
            {
                foreach (var v in spell.enchantmentData)
                {
                    var u = target as MOM.Unit;
                    u.AddEnchantment(v, source as IEnchantable, v.lifeTime, null, spell.worldCost);
                }
            }
            else if (target is MOM.TownLocation)
            {
                foreach (var v in spell.enchantmentData)
                {
                    var t = target as MOM.TownLocation;
                    t.AddEnchantment(v, source as IEnchantable, v.lifeTime, null, spell.worldCost);
                }
            }
            else
            {
                foreach (var v in spell.enchantmentData)
                {
                    var l = target as MOM.Location;
                    l.AddEnchantment(v, source as IEnchantable, v.lifeTime, null, spell.worldCost);
                }
            }

            return true;
        }
        static public bool SWG_Invisibility(ISpellCaster source, object target, Spell spell)
        {
            if (spell.enchantmentData == null)
            {
                Debug.LogWarning("SWG_ApplyEnchantment requires " + spell.dbName + " to have enchantments data ");
                return false;
            }

            if (!(target is MOM.Unit))
            {
                Debug.LogWarning("SWG_ApplyEnchantment is not targeting unit or Town Location");
                return false;
            }

            if (target is MOM.Unit)
            {
                var u = target as MOM.Unit;
                foreach (var v in spell.enchantmentData)
                {
                    u.AddEnchantment(v, source as IEnchantable, v.lifeTime, null, spell.worldCost);
                }
                u.group.Get().UpdateMarkers();
                u.group.Get().UpdateMapFormation(false);
            }

            return true;
        }
        //That script on apply ench use target owner as a ench owner
        static public bool SWG_ApplyEnchantmentWithReversOwner(ISpellCaster source, object target, Spell spell)
        {
            if (spell.enchantmentData == null)
            {
                Debug.LogWarning("SWG_ApplyEnchantmentWithReversOwner requires " + spell.dbName + " to have enchantments data ");
                return false;
            }

            if (!(target is MOM.Unit) && !(target is MOM.TownLocation))
            {
                Debug.LogWarning("SWG_ApplyEnchantmentWithReversOwner is not targeting unit or Town Location");
                return false;
            }

            if (target is MOM.Unit)
            {
                foreach (var v in spell.enchantmentData)
                {
                    var u = target as MOM.Unit;
                    u.AddEnchantment(v, u as IEnchantable, v.lifeTime, null, spell.worldCost);
                }
            }
            else
            {
                foreach (var v in spell.enchantmentData)
                {
                    var t = target as MOM.TownLocation;
                    t.AddEnchantment(v, t as IEnchantable, v.lifeTime, null, spell.worldCost);
                }
            }

            return true;
        }
        static public bool SWG_ApplyMagicWallEnchantment(ISpellCaster source, object target, WorldCode.Plane plane, Spell spell)
        {
            if (spell.enchantmentData == null)
            {
                Debug.LogWarning("SWG_ApplyMagicWallEnchantment requires " + spell.dbName + " to have enchantments data ");
                return false;
            }

            if (!(target is Vector3i))
            {
                Debug.LogWarning("SWG_ApplyMagicWallEnchantment is not targeting position.");
                return false;
            }

            if (target is Vector3i)
            {
                var position = (Vector3i)target;

                foreach (var l in GameManager.Get().registeredLocations)
                {
                    if (l is TownLocation && l.IsDistanceTo_Zero(position, plane))
                    {
                        foreach (var v in spell.enchantmentData)
                        {
                            l.AddEnchantment(v, source as IEnchantable, v.lifeTime, null, spell.worldCost);
                        }
                    }
                }
            }

            return true;
        }
        static public bool SWG_AddSkills(ISpellCaster source, object target, Spell spell)
        {

            if (!(target is MOM.Unit))
            {
                Debug.LogWarning("SWG_AddSkills is not targeting unit in battle");
                return false;
            }

            var unit = target as MOM.Unit;
            DBDef.Skill skill = null;

            for (int i = 0; i < spell.stringData.Length; i++)
            {
                var skillName = spell.stringData;
                DBClass s = DataBase.Get(skillName[i], false);
                if (s is Skill)
                {
                    skill = s as Skill;
                    if (!unit.GetSkills().Contains(skill))
                    {
                        unit.AddSkill(skill);
                    }
                }
            }

            return true;
        }
        static public bool SWG_BlackChannels(ISpellCaster source, object target, Spell spell)
        {

            if (!(target is MOM.Unit))
            {
                Debug.LogWarning("SWG_AddSkills is not targeting unit in battle");
                return false;
            }

            var unit = target as MOM.Unit;
            unit.canNaturalHeal = false;
            unit.canGainXP = false;
            DBDef.Skill skill = null;

            for (int i = 0; i < spell.stringData.Length; i++)
            {
                var skillName = spell.stringData;
                DBClass s = DataBase.Get(skillName[i], false);
                if (s is Skill)
                {
                    skill = s as Skill;
                    if (!unit.GetSkills().Contains(skill))
                    {
                        unit.AddSkill(skill);
                    }
                }
            }

            return true;
        }
        static public bool SWG_NaturesCures(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is MOM.Group))
            {
                Debug.LogWarning("SWG_NaturesCures is not targeting units in battle");
                return false;
            }

            MOM.Group group = target as MOM.Group;
            var units = group.GetUnits();

            for (int i = 0; i < units.Count; i++)
            {
                units[i].Get().Heal(100.0f);
            }

            return true;
        }
        static public bool SWG_FireStorm(ISpellCaster source, object target, Spell spell)
        {
            if (spell.fIntData[0] == null || spell.fIntData.Length == 0)
            {
                Debug.LogWarning("SWG_FireStorm have miss FIntData");
                return false;
            }
            if (!(target is MOM.Group))
            {
                Debug.LogWarning("SWG_FireStorm is not targeting units in battle");
                return false;
            }

            int[] dmg = new int[9];
            var baseDmg = spell.fIntData[0].ToInt();

            MOM.Group group = target as MOM.Group;
            var units = group.GetUnits();


            for (int i = units.Count - 1; i >= 0; i--)
            {
                dmg = ProduceAreaSpellDamage(baseDmg, dmg, units[i].Get().figureCount);

                if (units[i].Get().attributes.Contains(TAG.MAGIC_IMMUNITY) ||
                    units[i].Get().attributes.Contains(TAG.FIRE_IMMUNITY) ||
                    units[i].Get().attributes.Contains(TAG.RIGHTEOUSNESS))
                {
                    units[i].Get().ApplyImmolationDamage(dmg, random, 50, spell, true);
                }
                else if (units[i].Get().attributes.Contains(TAG.ELEMENTAL_ARMOR))
                {
                    units[i].Get().ApplyImmolationDamage(dmg, random, 10, spell);
                }
                else if (units[i].Get().attributes.Contains(TAG.RESIST_ELEMENTS) ||
                    units[i].Get().attributes.Contains(TAG.BLESS))
                {
                    units[i].Get().ApplyImmolationDamage(dmg, random, 3, spell);
                }
                else if (units[i].Get().attributes.Contains(TAG.LARGE_SHIELD))
                {
                    units[i].Get().ApplyImmolationDamage(dmg, random, 2, spell);
                }
                else
                {
                    units[i].Get().ApplyImmolationDamage(dmg, random, 0, spell);
                }
            }

            return true;
        }
        static public bool SWG_IceStorm(ISpellCaster source, object target, Spell spell)
        {
            if (spell.fIntData[0] == null || spell.fIntData.Length == 0)
            {
                Debug.LogWarning("SWG_IceStorm have miss FIntData");
                return false;
            }
            if (!(target is MOM.Group))
            {
                Debug.LogWarning("SWG_IceStorm is not targeting units in battle");
                return false;
            }

            MOM.Group group = target as MOM.Group;
            var units = group.GetUnits();

            int[] dmg = new int[9];
            var baseDmg = spell.fIntData[0].ToInt();

            for (int i = units.Count - 1; i >= 0; i--)
            {
                dmg = ProduceAreaSpellDamage(baseDmg, dmg, units[i].Get().figureCount);
                if (units[i].Get().attributes.Contains(TAG.MAGIC_IMMUNITY) ||
                    units[i].Get().attributes.Contains(TAG.COLD_IMMUNITY))
                {
                    units[i].Get().ApplyImmolationDamage(dmg, random, 50, spell, true);
                }
                else if (units[i].Get().attributes.Contains(TAG.ELEMENTAL_ARMOR))
                {
                    units[i].Get().ApplyImmolationDamage(dmg, random, 10, spell);
                }
                else if (units[i].Get().attributes.Contains(TAG.RESIST_ELEMENTS))
                {
                    units[i].Get().ApplyImmolationDamage(dmg, random, 3, spell);
                }
                else if (units[i].Get().attributes.Contains(TAG.LARGE_SHIELD))
                {
                    units[i].Get().ApplyImmolationDamage(dmg, random, 2, spell);
                }
                else
                {
                    units[i].Get().ApplyImmolationDamage(dmg, random, 0, spell);
                }
            }

            return true;
        }
        static public bool SWG_Stasis(ISpellCaster source, object target, Spell spell)
        {
            if (spell.enchantmentData == null)
            {
                Debug.LogWarning("SWG_Stasis requires " + spell.dbName + " to have enchantments data ");
                return false;
            }
            if (!(target is MOM.Group))
            {
                Debug.LogWarning("SWG_Stasis is not targeting group");
                return false;
            }

            FInt resRed = FInt.ZERO;
            if (spell.fIntData != null && spell.fIntData.Length > 0)
            {
                resRed = spell.fIntData[0];
            }

            MOM.Group group = target as MOM.Group;
            foreach (var targetU in group.GetUnits())
            {
                var resistMod = -1 * resRed + ResistModFromEnch(null, targetU, spell);
                int finalRes = (targetU.Get().attributes.GetFinal(TAG.RESIST) + resistMod).ToInt();
                var roll = random.GetInt(1, 11);

                if (roll > finalRes)
                {
                    foreach (var v in spell.enchantmentData)
                    {
                        targetU.Get().AddEnchantment(v, source as IEnchantable, roll - finalRes, null, spell.worldCost);
                        targetU.Get().UpdateMP();
                    }
                }
            }

            return true;
        }
        static public bool SWG_ChaosChannels(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is MOM.Unit))
            {
                Debug.LogWarning("SWG_ChaosChannels is not targeting unit on world map.");
                return false;
            }

            var unit = target as MOM.Unit;

            var unitSkills = unit.GetSkills();
            var potentialSkills = new List<DBReference<Skill>>();

            foreach (var i in spell.stringData)
                potentialSkills.Add(DataBase.Get(i, false) as Skill);

            foreach (var skill in unitSkills)
            {
                if (potentialSkills.Contains(skill))
                    potentialSkills.Remove(skill);
            }

            if (potentialSkills.Count > 0)
            {
                potentialSkills.RandomSort();

                for (int i = potentialSkills.Count - 1; i >= 0; i--)
                {
                    if (potentialSkills[i].Get() == (Skill)SKILL.CHAOS_CHANNELS3)
                    {
                        unit.AddSkill(potentialSkills[i].Get());
                        var unitSkillScripts = unit.GetSkillManager().GetSkillScripts();
                        var unitOwnFireBreath = false;
                        foreach (var uss in unitSkillScripts)
                        {
                            foreach (var e in uss.Value)
                            {
                                if (e.activatorSecondary == null) continue;
                                if (e.activatorSecondary.Contains("ACT_ApplyFireBreathAttack"))
                                    unitOwnFireBreath = true;
                            }
                        }
                        if (unitOwnFireBreath == false)
                        {
                            unit.AddSkill((Skill)SKILL.FIRE_BREATH);
                        }
                        if (unit.GetAttributes().DoesNotContains((Tag)TAG.FANTASTIC_CLASS))
                        {
                            unit.GetAttributes().AddToBase((Tag)TAG.FANTASTIC_CLASS, FInt.ONE);
                            unit.EnsureEnchantments();
                        }
                        return true;
                    }
                    else if (potentialSkills[i].Get() == (Skill)SKILL.CHAOS_CHANNELS2)
                    {
                        if (unit.GetAttFinal(TAG.CAN_FLY) <= 0)
                        {
                            unit.AddSkill(potentialSkills[i].Get());
                            unit.GetAttributes().AddToBase((Tag)TAG.FANTASTIC_CLASS, FInt.ONE);
                            unit.EnsureEnchantments();
                            return true;
                        }
                        else
                        {
                            potentialSkills.Remove(potentialSkills[i]);
                        }
                    }
                    else
                    {
                        unit.AddSkill(potentialSkills[i].Get());
                        unit.GetAttributes().AddToBase((Tag)TAG.FANTASTIC_CLASS, FInt.ONE);
                        unit.EnsureEnchantments();
                        return true;
                    }
                }
            }
            else
                return false;



            return true;
        }

        static public bool SWG_PlanarTravel(ISpellCaster source, object target, Spell spell)
        {
            MOM.Group group = target as MOM.Group;
            if (group == null) Debug.LogError("TRG on invalid type: expected Group in SWG_PlaneShift");

            if (group.IsSwitchPlaneDestinationValid())
            {
                group.PlaneSwitch();
            }
            return true;
        }
        static public bool SWG_Lycanthropy(ISpellCaster source, object target, Spell spell)
        {
            var targetUnit = target as MOM.Unit;
            if (targetUnit == null) Debug.LogError("Target is not MOM.Unit");

            var unit = DataBase.Get<DBDef.Unit>(spell.stringData[0], true);
            if (unit == null) Debug.LogError("Unit " + spell.stringData + " not found in database");

            var u = MOM.Unit.CreateFrom(unit);
            var group = targetUnit.group?.Get();

            u.CopyEnchantmentManagerFrom(targetUnit);
            u.EnsureEnchantments();
            var move = group.CurentMP().ToFloat();

            if (group.GetUnits().Count < 9)
            {
                group.AddUnit(u);
                targetUnit.Destroy();
            }
            else
            {
                targetUnit.Destroy();
                group.AddUnit(u);
            }

            u.Mp = (FInt)Mathf.Min(u.GetMaxMP().ToFloat(), move);
            u.EnsureEnchantments();

            return true;
        }
        static public bool SWG_SpellWard(ISpellCaster source, object target, Spell spell)
        {
            if (spell.enchantmentData == null)
            {
                Debug.LogWarning("SWG_SpellWard requires " + spell.dbName + " to have enchantments data ");
                return false;
            }

            if (!(target is MOM.TownLocation))
            {
                Debug.LogWarning("SWG_SpellWard is not targeting Town Location");
                return false;
            }

            // That spell need to add only selected event from Popup
            foreach (var v in spell.enchantmentData)
            {
                var g = target as MOM.TownLocation;
                g.AddEnchantment(v, source as IEnchantable, v.lifeTime, null, spell.worldCost);
            }


            return true;
        }
        static public bool SWG_Consecration(ISpellCaster source, object target, Spell spell)
        {
            if (spell.enchantmentData == null)
            {
                Debug.LogWarning("SWG_Consecration requires " + spell.dbName + " to have enchantments data ");
                return false;
            }

            if (!(target is MOM.TownLocation))
            {
                Debug.LogWarning("SWG_Consecration is not targeting Town Location");
                return false;
            }

            var town = target as TownLocation;
            var enchList = town.GetEnchantments();

            for (int i = enchList.Count - 1; i >= 0; i--)
            {
                var en = enchList[i].source.Get();
                if (en == (Enchantment)ENCH.EVIL_PRESENCE ||
                   en == (Enchantment)ENCH.FAMINE ||
                   en == (Enchantment)ENCH.CURSED_LANDS ||
                   en == (Enchantment)ENCH.PESTILENCE ||
                   en == (Enchantment)ENCH.CHAOS_RIFT)
                    town.RemoveEnchantment(en);
            }

            //Add enchantments
            foreach (var v in spell.enchantmentData)
            {
                var g = target as MOM.TownLocation;
                g.AddEnchantment(v, source as IEnchantable, v.lifeTime, null, spell.worldCost);
            }

            return true;
        }
        static public bool SWG_CallTheVoid(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is MOM.TownLocation))
            {
                Debug.LogWarning("SWG_CallTheVoid is not targeting Town Location");
                return false;
            }

            TownLocation t = target as MOM.TownLocation;

            //Every garrisoned unit is struck with a strength  10 Doom Damage attack;
            List<Reference<MOM.Unit>> units = t.GetLocalGroup().GetUnits();
            for (int i = units.Count - 1; i >= 0; i--)
            {
                units[i].Get().ApplyDoomDmg(10, null);
            }
            
            //If the target is an Outpost, it is removed from the map.
            if(t.IsAnOutpost())
            {
                t.Raze(source.GetWizardOwnerID());
                return true;
            }

            var buildingList = new List<DBReference<Building>>(t.buildings);
            foreach (var b in buildingList)
            {
                if (!t.IsRegularBuilding(b)) continue;

                if (random.GetSuccesses(0.5f, 1) > 0)
                {
                    t.RemoveBuilding(b);
                }
            }

            int slainPop = random.GetSuccesses(0.5f, t.Population / 1000);
            t.Population = Mathf.Max(1000, t.Population - slainPop * 1000);

            foreach (var hexPosition in t.GetSurroundingArea(t.GetTownRange()))
            {
                var hex = World.GetActivePlane().GetHexAt(hexPosition);
                if (hex.ActiveHex && random.GetSuccesses(0.5f, 1) > 0)
                {
                    hex.ActiveHex = false;
                }
            }

            return true;
        }
        static public bool SWG_BlackWind(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is MOM.Group))
            {
                Debug.LogWarning("SWG_BlackWind is not targeting units in battle");
                return false;
            }


            MOM.Group group = target as MOM.Group;
            var units = group.GetUnits();

            int resistReduction = 0;
            if (spell.fIntData != null && spell.fIntData.Length > 0)
            {
                resistReduction = spell.fIntData[0].ToInt();
            }

            for (int i = units.Count - 1; i >= 0; i--)
            {
                if (units[i].Get().attributes.Contains(TAG.MAGIC_IMMUNITY) ||
                    units[i].Get().attributes.Contains(TAG.DEATH_IMMUNITY) ||
                    units[i].Get().attributes.Contains(TAG.RIGHTEOUSNESS))
                    continue;
                else if (units[i].Get().attributes.Contains(TAG.BLESS))
                    units[i].Get().ApplyResistFigureDeath(random, resistReduction, 3, false);
                else
                    units[i].Get().ApplyResistFigureDeath(random, resistReduction, 0, false);
            }

            return true;
        }
        static public bool SWG_Earthquake(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is TownLocation))
            {
                Debug.LogWarning("SWG_Earthquake is not targeting units in battle");
                return false;
            }


            var town = target as TownLocation;
            var group = town.GetLocalGroup();
            var units = group.GetUnits();

            for (int i = town.buildings.Count - 1; i >= 0; i--)
            {
                if (!town.IsRegularBuilding(town.buildings[i])) continue;

                if (random.GetFloat(0f, 1f) <= 0.15f)
                    town.RemoveBuilding(town.buildings[i]);
            }

            if (group == null || units.Count < 1)
                return false;

            for (int i = units.Count - 1; i >= 0; i--)
            {
                TAG[] prohibitedTags = new TAG[] { TAG.CAN_FLY, TAG.NON_CORPOREAL };
                if (units[i].Get().GetAttributes().ContainsAny(prohibitedTags)) continue;
                if (random.GetFloat(0f, 1f) < 0.25f)
                {
                    units[i].Get().Destroy();
                }
            }

            return true;
        }
        static public bool SWG_DisenchantArea(ISpellCaster source, object target, WorldCode.Plane plane, Spell spell)
        {
            if (!(target is Vector3i))
            {
                return false;
            }

            FInt dispelStr;
            if (spell != null)
            {
                dispelStr = spell.fIntData[0];
            }
            else
            {
                //In oMom dispel power was 100 for nightshade.
                //But our dispel is more powerfully (dispel work on units as well )
                //and each building that use nightshade add extra try.
                dispelStr = (FInt)50f;
            }

            var spellCaster = source as PlayerWizard;

            //Add extra dmg if player used extra mana
            if (spellCaster != null &&
                spellCaster.GetMagicAndResearch().extensionItemSpellWorld != null)
            {
                dispelStr += spellCaster.magicAndResearch.extensionItemSpellWorld.extraPower;
            }


            var position = (Vector3i)target;
            var spellCasterOwner = source.GetWizardOwner();

            //remove enchantment from units
            var groups = GameManager.Get().registeredGroups;
            //groups = groups.FindAll(o => o.GetPosition() == position);
            List<EnchantmentInstance> enchList;
            string removedEnch = "";

            foreach (var g in groups)
            {
                if (!g.IsDistanceTo_Zero(position, plane)) continue;

                if (g.GetLocationHost()?.otherPlaneLocation?.Get() != null && g.plane.arcanusType) continue;
                foreach (var u in g.GetUnits())
                {
                    MOM.Unit unit = u.Get();
                    var unitOwner = unit.GetWizardOwner();

                    enchList = unit.GetEnchantments();

                    if (unit.isSpellLock && unitOwner != spellCasterOwner)
                    {
                        for (int i = enchList.Count - 1; i >= 0; i--)
                        {
                            if (enchList.Count <= i) continue;

                            var ench = enchList[i].source.Get();

                            if (ench.scripts == null) continue;
                            var s = Array.Find(enchList[i].source.Get().scripts, o => o.tag == (Tag)TAG.SPELL_LOCK);
                            if (s != null)
                            {
                                if (GetDispelSuccess(spellCasterOwner, enchList[i], dispelStr, s.fIntData))
                                {
                                    var newRemEch = enchList[i].source.Get().GetDILocalizedName();
                                    if (removedEnch.Length > 0)
                                    {
                                        removedEnch = removedEnch + ", " + newRemEch;
                                    }
                                    else
                                    {
                                        removedEnch = newRemEch;
                                    }

                                    unit.RemoveEnchantment(enchList[i].source);
                                }

                                break;
                            }
                        }
                    }
                    else
                    {
                        for (int i = enchList.Count - 1; i >= 0; i--)
                        {
                            if (enchList.Count <= i) continue;

                            //Dispel only ench that allow to dispel.
                            if (enchList[i].source.Get().allowDispel == false) continue;

                            //Disenchant only negative ench on own unit. Disenchant only positive ench on enemy unit.
                            if (!(unitOwner == spellCasterOwner && enchList[i].source.Get().enchCategory == EEnchantmentCategory.Negative ||
                                unitOwner != spellCasterOwner && enchList[i].source.Get().enchCategory == EEnchantmentCategory.Positive))
                                continue;

                            if (GetDispelSuccess(spellCasterOwner, enchList[i], dispelStr))
                            {
                                var newRemEch = enchList[i].source.Get().GetDILocalizedName();
                                if (removedEnch.Length > 0)
                                {
                                    removedEnch = removedEnch + ", " + newRemEch;
                                }
                                else
                                {
                                    removedEnch = newRemEch;
                                }

                                unit.RemoveEnchantment(enchList[i].source);
                            }
                        }
                    }
                }
            }

            //remove enchantment from location
            var location = GameManager.Get().registeredLocations.Find(o => o.IsDistanceTo_Zero(position, plane));
            if (location == null) return true;

            int locationOwner = location.owner;
            enchList = location.GetEnchantments();

            for (int i = enchList.Count - 1; i >= 0; i--)
            {
                //Dispel only ench that allow to dispel.
                if (enchList[i].source.Get().allowDispel == false) continue;

                //Disenchant only negative ench on own unit. Disenchant only positive ench on enemy unit.
                if (!(locationOwner == spellCasterOwner.ID && enchList[i].source.Get().enchCategory == EEnchantmentCategory.Negative ||
                    locationOwner != spellCasterOwner.ID && enchList[i].source.Get().enchCategory == EEnchantmentCategory.Positive))
                    continue;

                if (GetDispelSuccess(spellCasterOwner, enchList[i], dispelStr))
                {
                    var newRemEch = enchList[i].source.Get().GetDILocalizedName();
                    if (removedEnch.Length > 0)
                    {
                        removedEnch = removedEnch + ", " + newRemEch;
                    }
                    else
                    {
                        removedEnch = newRemEch;
                    }

                    location.RemoveEnchantment(enchList[i].source);
                }
            }

            if (spellCaster == GameManager.GetHumanWizard()) //player is casting dispel
            {
                if (removedEnch != "")
                {
                    if (removedEnch.Contains(", ")) //different text for 1 and more than 1 enchantments removed
                    {
                        var message = DBUtils.Localization.Get("UI_ENCHANTMENTS_REMOVED_SUCCESSFULLY", true, removedEnch);
                        PopupGeneral.OpenPopup(null, "UI_SUCCESS", message, "UI_OK");
                    }
                    else
                    {
                        var message = DBUtils.Localization.Get("UI_ENCHANTMENT_REMOVED_SUCCESSFULLY", true, removedEnch);
                        PopupGeneral.OpenPopup(null, "UI_SUCCESS", message, "UI_OK");
                    }
                }
                else
                {
                    PopupGeneral.OpenPopup(null, "UI_DISPEL_FAILED", "UI_NO_ENCHANTMENTS_REMOVED", "UI_OK");
                }
            }

            return true;
        }
        static public bool SWG_DisenchantAreaNightshade(ISpellCaster source, object target, WorldCode.Plane plane, Spell spell)
        {
            if (!(target is Vector3i))
            {
                return false;
            }

            FInt dispelStr;
            if (spell != null)
            {
                dispelStr = spell.fIntData[0];
            }
            else
            {
                //In oMom dispel power was 100 for nightshade.
                //But our dispel is more powerfully (dispel work on units as well )
                //and each building that use nightshade add extra try.
                dispelStr = (FInt)50f;
            }

            var spellCaster = source as PlayerWizard;

            //Add extra dmg if player used extra mana
            if (spellCaster != null &&
                spellCaster.GetMagicAndResearch().extensionItemSpellWorld != null)
            {
                dispelStr += spellCaster.magicAndResearch.extensionItemSpellWorld.extraPower;
            }


            var position = (Vector3i)target;
            var spellCasterOwner = source.GetWizardOwner();

            //remove enchantment from units
            var groups = GameManager.Get().registeredGroups;
            //groups = groups.FindAll(o => o.GetPosition() == position);
            List<EnchantmentInstance> enchList;
            string removedEnch = "";

            //If there were any negative ench tested show message
            bool negativesEnchOnUnit = false;
            bool negativesEnchOnTown = false;

            foreach (var g in groups)
            {
                if (!g.IsDistanceTo_Zero(position, plane)) continue;

                if (g.GetLocationHost()?.otherPlaneLocation?.Get() != null && g.plane.arcanusType) continue;
                foreach (var u in g.GetUnits())
                {
                    MOM.Unit unit = u.Get();
                    var unitOwner = unit.GetWizardOwner();

                    enchList = unit.GetEnchantments();

                    if (unit.isSpellLock && unitOwner != spellCasterOwner)
                    {
                        for (int i = enchList.Count - 1; i >= 0; i--)
                        {
                            if (enchList.Count <= i) continue;

                            var ench = enchList[i].source.Get();

                            if (ench.scripts == null) continue;
                            var s = Array.Find(enchList[i].source.Get().scripts, o => o.tag == (Tag)TAG.SPELL_LOCK);
                            if (s != null)
                            {
                                negativesEnchOnUnit = true;
                                if (GetDispelSuccess(spellCasterOwner, enchList[i], dispelStr, s.fIntData))
                                {
                                    var newRemEch = enchList[i].source.Get().GetDILocalizedName();
                                    if (removedEnch.Length > 0)
                                    {
                                        removedEnch = removedEnch + ", " + newRemEch;
                                    }
                                    else
                                    {
                                        removedEnch = newRemEch;
                                    }

                                    unit.RemoveEnchantment(enchList[i].source);
                                }

                                break;
                            }
                        }
                    }
                    else
                    {
                        for (int i = enchList.Count - 1; i >= 0; i--)
                        {
                            if (enchList.Count <= i) continue;

                            //Dispel only ench that allow to dispel.
                            if (enchList[i].source.Get().allowDispel == false) continue;

                            //Disenchant only negative ench on own unit. Disenchant only positive ench on enemy unit.
                            if (!(unitOwner == spellCasterOwner && enchList[i].source.Get().enchCategory == EEnchantmentCategory.Negative ||
                                unitOwner != spellCasterOwner && enchList[i].source.Get().enchCategory == EEnchantmentCategory.Positive))
                                continue;

                            negativesEnchOnUnit = true;
                            if (GetDispelSuccess(spellCasterOwner, enchList[i], dispelStr))
                            {
                                var newRemEch = enchList[i].source.Get().GetDILocalizedName();
                                if (removedEnch.Length > 0)
                                {
                                    removedEnch = removedEnch + ", " + newRemEch;
                                }
                                else
                                {
                                    removedEnch = newRemEch;
                                }

                                unit.RemoveEnchantment(enchList[i].source);
                            }
                        }
                    }
                }
            }

            //remove enchantment from location
            var location = GameManager.Get().registeredLocations.Find(o => o.IsDistanceTo_Zero(position, plane));
            if (location == null) return true;

            int locationOwner = location.owner;
            enchList = location.GetEnchantments();

            for (int i = enchList.Count - 1; i >= 0; i--)
            {
                //Dispel only ench that allow to dispel.
                if (enchList[i].source.Get().allowDispel == false) continue;

                //Disenchant only negative ench on own unit. Disenchant only positive ench on enemy unit.
                if (!(locationOwner == spellCasterOwner.ID && enchList[i].source.Get().enchCategory == EEnchantmentCategory.Negative ||
                    locationOwner != spellCasterOwner.ID && enchList[i].source.Get().enchCategory == EEnchantmentCategory.Positive))
                    continue;

                negativesEnchOnTown = true;
                if (GetDispelSuccess(spellCasterOwner, enchList[i], dispelStr))
                {
                    var newRemEch = enchList[i].source.Get().GetDILocalizedName();
                    if (removedEnch.Length > 0)
                    {
                        removedEnch = removedEnch + ", " + newRemEch;
                    }
                    else
                    {
                        removedEnch = newRemEch;
                    }

                    location.RemoveEnchantment(enchList[i].source);
                }
            }

            //player is casting dispel
            if (spellCaster == GameManager.GetHumanWizard() && (negativesEnchOnUnit || negativesEnchOnTown))
            {
                if (removedEnch != "")
                {
                    if (removedEnch.Contains(", ")) //different text for 1 and more than 1 enchantments removed
                    {
                        var message = DBUtils.Localization.Get("UI_NIGHTSHADE_DISPEL_SUCCESS", true, location.name, removedEnch);
                        PopupGeneral.OpenPopup(null, "UI_SUCCESS", message, "UI_OK");
                    }
                    else
                    {
                        var message = DBUtils.Localization.Get("UI_NIGHTSHADE_DISPEL_SUCCESS", true, location.name, removedEnch);
                        PopupGeneral.OpenPopup(null, "UI_SUCCESS", message, "UI_OK");
                    }
                }
                else
                {
                    var message = DBUtils.Localization.Get("UI_NIGHTSHADE_DISPEL_FAILURE", true, location.name);
                    PopupGeneral.OpenPopup(null, "UI_DISPEL_FAILED", message, "UI_OK");
                }
            }

            return true;
        }
        static public bool SWG_SummoningCircle(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is TownLocation))
            {
                Debug.LogWarning("SWG_SummoningCircle is not targeting town.");
                return false;
            }
            var town = target as TownLocation;
            var caster = source.GetWizardOwner();
            if (town.GetOwnerID() != caster.ID) return false;
            //             if (caster.summoningCircle != null)
            //             {
            //                 var lastPlace = caster.summoningCircle.Get();
            //                 if (caster == GameManager.GetHumanWizard())
            //                 {
            //                     VerticalMarkerManager.Get().UpdateInfoOnMarker(lastPlace);
            //                 }
            //             }
            caster.SetSummoningLocation(town);

            //             if (caster == GameManager.GetHumanWizard())
            //             {
            //                 VerticalMarkerManager.Get().UpdateInfoOnMarker(town);
            //             }

            return true;
        }
        static public bool SWG_SpellOfReturn(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is TownLocation))
            {
                Debug.LogWarning("SWG_SpellOfReturn is not targeting town.");
                return false;
            }

            var town = target as TownLocation;
            var caster = source.GetWizardOwner();

            caster.banishedTurn = 0;

            if (caster.wizardTower != null)
            {
                Debug.LogWarning("SWG_SpellOfReturn will not work! wizard already have tower.");
                return false;
            }

            caster.SetTowerLocation(town);

            return true;
        }
        static public bool SWG_MoveFortress(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is TownLocation))
            {
                Debug.LogWarning("SWG_MoveFortress is not targeting town.");
                return false;
            }
            var town = target as TownLocation;
            var spellCaster = source.GetWizardOwner();
            var oldFortress = spellCaster.wizardTower;
            oldFortress.Get().RemoveWizardTowerBonus();

            spellCaster.SetTowerLocation(town);
            if (oldFortress == spellCaster.summoningCircle)
            {
                spellCaster.SetSummoningLocation(town);
                spellCaster.mainRace = town.race;
            }
            oldFortress.Get().UpdateOwnerModels();

            return true;
        }
        static public bool SWG_AddBuilding(ISpellCaster source, object target, Spell spell)
        {
            if (spell.buildingData == null)
            {
                Debug.LogWarning("SWG_AddBuilding requires " + spell.dbName + " to have enchantments data ");
                return false;
            }

            if (!(target is MOM.TownLocation))
            {
                Debug.LogWarning("SWG_AddBuilding is not targeting Town Location");
                return false;
            }

            foreach (var v in spell.buildingData)
            {
                var g = target as MOM.TownLocation;
                g.AddBuilding(v);
                g.craftingQueue.CleanBuildingFromQueue(v);
            }

            return true;
        }
        static public bool SWG_WordOfRecall(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is MOM.Unit))
            {
                Debug.LogWarning("SWG_WordOfRecall is not targeting unit.");
                return false;
            }

            var u = target as MOM.Unit;
            var sourceGroup = u.group.Get();
            var summoningCircle = source.GetWizardOwner().summoningCircle;
            if (summoningCircle != null)
            {
                var gr = summoningCircle.Get().GetLocalGroup();
                sourceGroup.TransferUnit(gr, u);
                var formation = sourceGroup.GetMapFormation();
                if (formation != null && formation.source == u)
                {
                    sourceGroup.UpdateMapFormation();
                }
            }

            return true;
        }
        static public bool SWG_ApplyEnchantmentWarpNode(ISpellCaster source, object target, Spell spell)
        {
            if (spell.enchantmentData == null)
            {
                Debug.LogWarning("SWG_ApplyEnchantmentWarpNode requires " + spell.dbName + " to have enchantments data ");
                return false;
            }

            if (!(target is MOM.Location))
            {
                Debug.LogWarning("SWG_ApplyEnchantmentWarpNode is not targeting unit or Location");
                return false;
            }

            if (target is MOM.Location)
            {
                foreach (var v in spell.enchantmentData)
                {
                    var t = target as MOM.Location;
                    t.AddEnchantment(v, source as IEnchantable, v.lifeTime, null, spell.worldCost);
                    VerticalMarkerManager.Get().UpdateInfoOnMarker(t);
                }
            }

            return true;
        }
        #endregion

        #region Spell World Global/Wizard
        static public bool SWW_UnitTest(ISpellCaster source, object target, Spell spell)
        {
            var w = target as MOM.Unit;
            if (w == null)
            {
                Debug.Log("Spell is designed to target unit");
                return false;
            }

            var baseValue = w.GetWorldUnitValue();
            var modValue = w.GetModifiedWorldUnitValue(TAG.DEFENCE, (FInt)10.0) +
                w.GetModifiedWorldUnitValue(TAG.RESIST, (FInt)10.0);
            var diff = modValue - (baseValue * 2);


            Debug.Log("Test unit " + w.GetName() +
                " give diff value " + diff +
                " from " + baseValue + " and " + modValue);

            return true;

        }
        static public bool SWW_ApplyWizardEnchantment(ISpellCaster source, object target, Spell spell)
        {
            var w = target as PlayerWizard;
            if (w == null)
            {
                Debug.Log("Spell is designed to target wizard");
                return false;
            }

            foreach (var v in spell.enchantmentData)
            {
                w.AddEnchantment(v, source as IEnchantable, v.lifeTime, null, spell.worldCost);
            }

            return true;

        }
        static public bool SWW_ApplyGameManagerEnchantment(ISpellCaster source, object target, Spell spell)
        {
            if (GameManager.Get() == null)
            {
                Debug.Log("Spell is designed to target gamemanager.");
                return false;
            }

            foreach (var v in spell.enchantmentData)
            {
                GameManager.Get().AddEnchantment(v, source as IEnchantable, v.lifeTime, null, spell.worldCost);
            }

            return true;
        }
        static public bool SWW_DeathWish(ISpellCaster source, object target, Spell spell)
        {
            var wizards = GameManager.Get().wizards;
            var owner = source as PlayerWizard;
            if (wizards.Count < 1 && owner != null)
            {
                Debug.Log("Spell is designed to target all wizard's units.");
                return false;
            }

            var registerGroups = GameManager.Get().registeredGroups;

            List<Reference<MOM.Unit>> unitsRef = new List<Reference<MOM.Unit>>();
            int unitsDestroyed = 0;

            foreach (var w in wizards)
            {
                if (w.GetID() == owner.GetID()) continue;
                var wizardGroups = registerGroups.FindAll(o => o.GetOwnerID() == w.ID);

                foreach (var g in wizardGroups)
                {
                    if (g.GetLocationHost()?.otherPlaneLocation?.Get() != null && g.plane.arcanusType) continue;
                    unitsRef.AddRange(g.GetUnits());
                }
            }

            int resistMod = 0;
            if (spell.fIntData != null && spell.fIntData.Length > 0)
            {
                resistMod = spell.fIntData[0].ToInt();
            }

            foreach (var uRef in unitsRef)
            {
                var u = uRef.Get();

                //Check if unit is in protected location
                var position = u.GetPosition();
                var locations = GameManager.GetLocationsOfThePlane(u.GetPlane());
                var potencialLocation = locations.Find(o => o.GetPosition() == position);
                if (potencialLocation != null)
                {
                    var potencialTown = potencialLocation as TownLocation;
                    int wizID = source.GetWizardOwner() != null ? source.GetWizardOwner().GetID() : 0;
                    if (potencialTown != null && IsTownProtected(wizID, spell, potencialTown)) continue;
                }

                if ((u.attributes.DoesNotContains((Tag)TAG.MAGIC_IMMUNITY) ||
                    u.attributes.DoesNotContains((Tag)TAG.DEATH_IMMUNITY)) &&
                    (u.attributes.Contains(TAG.HERO_CLASS) ||
                    u.attributes.Contains(TAG.NORMAL_CLASS)))
                {
                    u.ApplyResistUnitDeath(random, resistMod);
                    if (u.group == null)
                    {
                        unitsDestroyed++;
                    }
                }
            }
            PopupGlobalCast.additionalData = new Multitype<Spell, int>(spell, unitsDestroyed);

            return true;
        }
        static public bool SWW_GreatUnsummoning(ISpellCaster source, object target, Spell spell)
        {

            int resistMod = spell.fIntData[0].ToInt();

            var wizards = GameManager.Get().wizards;
            if (wizards.Count < 1)
            {
                Debug.Log("Spell is designed to target ale wizards units.");
                return false;
            }

            var registerGroups = GameManager.Get().registeredGroups;

            List<Reference<MOM.Unit>> unitsRef = new List<Reference<MOM.Unit>>();
            int unitsDestroyed = 0;

            foreach (var w in wizards)
            {
                var wizardGroups = registerGroups.FindAll(o => o.GetOwnerID() == w.ID);

                foreach (var g in wizardGroups)
                {
                    if (g.GetLocationHost()?.otherPlaneLocation?.Get() != null && g.plane.arcanusType) continue;
                    unitsRef.AddRange(g.GetUnits());
                }
            }

            foreach (var uRef in unitsRef)
            {
                var u = uRef.Get();

                //Check if unit is in protected location
                var position = u.GetPosition();
                var locations = GameManager.GetLocationsOfThePlane(u.GetPlane());
                var potencialLocation = locations.Find(o => o.GetPosition() == position);
                if (potencialLocation != null)
                {
                    var potencialTown = potencialLocation as MOM.TownLocation;
                    int wizID = source.GetWizardOwner() != null ? source.GetWizardOwner().GetID() : 0;
                    if (potencialTown != null && IsTownProtected(wizID, spell, potencialTown)) continue;
                }

                if ((u.attributes.DoesNotContains((Tag)TAG.MAGIC_IMMUNITY) ||
                    u.attributes.DoesNotContains((Tag)TAG.SPELL_LOCK)) &&
                    (u.attributes.Contains(TAG.FANTASTIC_CLASS)))
                {
                    u.ApplyResistUnitDeath(random, resistMod);
                    if (u.group == null)
                    {
                        unitsDestroyed++;
                    }
                }
            }

            PopupGlobalCast.additionalData = new Multitype<Spell, int>(spell, unitsDestroyed);

            return true;
        }
        static public bool SWW_DrainPower(ISpellCaster source, object target, Spell spell)
        {
            var owner = source as PlayerWizard;
            var w = target as PlayerWizard;
            if (w == null || owner == null)
            {
                Debug.Log("Spell is designed to target wizard");
                return false;
            }

            var drainPower = random.GetInt(50, 151);

            if (w.mana > drainPower)
            {
                w.mana -= drainPower;
                owner.mana += drainPower;
            }
            else
            {
                owner.mana += w.mana;
                w.mana = 0;
            }

            if (!(source is PlayerWizardAI) || !(target is PlayerWizardAI))
            {
                HUD.Get().UpdateHUD();
            }

            if (owner == GameManager.GetHumanWizard())
            {
                PopupTargetWizard.ShowDrainEffect(spell, w, drainPower, true);
            }

            return true;
        }
        static public bool SWW_CruelUnminding(ISpellCaster source, object target, Spell spell)
        {
            var owner = source as PlayerWizard;
            var w = target as PlayerWizard;
            if (w == null || owner == null)
            {
                Debug.Log("Spell is designed to target wizard");
                return false;
            }

            float drainPower = random.GetFloat(0.01f, 0.11f);
            int drainValue = (int)((float)w.ModifiableCastingSkill * drainPower);

            if (drainValue < 1)
            {
                w.ModifiableCastingSkill -= 1;
                if (owner == GameManager.GetHumanWizard())
                {
                    PopupTargetWizard.ShowDrainEffect(spell, w, 1, false);
                }
            }
            else
            {
                w.ModifiableCastingSkill = w.ModifiableCastingSkill - drainValue;
                if (owner == GameManager.GetHumanWizard())
                {
                    PopupTargetWizard.ShowDrainEffect(spell, w, drainValue, false);
                }
            }

            if (!(source is PlayerWizardAI) || !(target is PlayerWizardAI))
            {
                HUD.Get().UpdateHUD();
            }

            return true;
        }
        static public bool SWW_SpellBlast(ISpellCaster source, object target, Spell spell)
        {
            var targetWizard = target as PlayerWizard;
            var spellCaster = source as PlayerWizard;
            if (targetWizard == null)
            {
                Debug.Log("Spell is designed to target wizard");
                return false;
            }

            var curentlyCastedSpell = targetWizard.GetMagicAndResearch().curentlyCastSpell;

            if (curentlyCastedSpell != null)
            {
                var castingProgress = targetWizard.GetMagicAndResearch().castingProgress;

                if (castingProgress <= spellCaster.mana)
                {
                    spellCaster.mana -= castingProgress;
                    targetWizard.GetMagicAndResearch().ResetCasting();
                }

                if (spellCaster.IsHuman)
                {
                    string message = DBUtils.Localization.Get("UI_SPELLBLAST_SUCCESSFUL", true, curentlyCastedSpell.GetDescriptionInfo().GetLocalizedName());
                    PopupGeneral.OpenPopup(null, "UI_INFO", message, "UI_OK");
                }
            }

            return true;

        }

        static public bool SWW_Disjunction(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is EnchantmentInstance))
            {
                return false;
            }

            FInt dispelStr = spell.fIntData[0];
            var spellCaster = source as PlayerWizard;

            //Add extra dispel power if player used extra mana
            if (spellCaster != null &&
                spellCaster.GetMagicAndResearch().extensionItemSpellWorld != null)
            {
                //Multiplier used for additional mana.
                //Base strength have this multiplier baked into its default value
                FInt multiplier = spell.fIntData[1];
                dispelStr += spellCaster.magicAndResearch.extensionItemSpellWorld.extraPower * multiplier;
            }

            var spellCasterOwner = source.GetWizardOwner();
            var ench = target as EnchantmentInstance;
            var enchManager = ench.manager;

            if (GetDispelSuccess(spellCasterOwner, ench, dispelStr))
            {
                //success
                if (spellCaster == GameManager.GetHumanWizard())
                {
                    var message = DBUtils.Localization.Get("UI_ENCHANTMENT_REMOVED_SUCCESSFULLY", true, ench.source.Get().GetDILocalizedName());
                    PopupGeneral.OpenPopup(null, "UI_SUCCESS", message, "UI_OK");
                }
                //                 else
                //                 {
                //                     var message = DBUtils.Localization.Get("UI_AI_ENCHANTMENT_REMOVED_SUCCESSFULLY", true, ench.source.Get().GetDILocalizedName());
                //                     PopupGeneral.OpenPopup(null, "UI_ENCHANTMENT_DISPELLED", message, "UI_OK");
                //                 }

                enchManager.owner.RemoveEnchantment(ench);
            }
            else
            {
                //failure
                if (spellCaster == GameManager.GetHumanWizard())
                {
                    var message = DBUtils.Localization.Get("UI_FAILED_TO_REMOVE_ENCHANTMENT", true, ench.source.Get().GetDILocalizedName());
                    PopupGeneral.OpenPopup(null, "UI_DISPEL_FAILED", message, "UI_OK");
                }
                else
                {
                    var message = DBUtils.Localization.Get("UI_AI_FAILED_TO_REMOVE_ENCHANTMENT", true, ench.source.Get().GetDILocalizedName());
                    PopupGeneral.OpenPopup(null, "UI_DISPEL_FAILED", message, "UI_OK");
                }
            }

            return true;

        }

        static public bool SWW_SpellBinding(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is EnchantmentInstance))
            {
                return false;
            }

            var spellCaster = source.GetWizardOwner();
            var ench = target as EnchantmentInstance;
            var enchOwner = GameManager.GetWizard(ench.owner.ID);

            var enchOnGameMenager = GameManager.Get().GetEnchantments().Find(o => o == ench);

            if (enchOnGameMenager != null)
            {
                //check if spellcaster already own this enchantment
                if (GameManager.Get().GetEnchantments().Find(o => o.source == ench.source && o.owner.ID == spellCaster.GetID()) != null)
                {
                    return false;
                }

                GameManager.Get().RemoveEnchantment(enchOnGameMenager);
                GameManager.Get().AddEnchantment(enchOnGameMenager.source, spellCaster, enchOnGameMenager.countDown, enchOnGameMenager.parameters, enchOnGameMenager.dispelCost);
                return true;
            }
            else if (ench != null)
            {
                if (spellCaster.GetEnchantments().Find(o => o.source == ench.source) != null)
                {
                    return false;
                }
                enchOwner.RemoveEnchantment(ench);
                spellCaster.AddEnchantment(ench.source, spellCaster, ench.countDown, ench.parameters, ench.dispelCost);
                return true;
            }

            return false;
        }
        static public bool SWW_Subversion(ISpellCaster source, object target, Spell spell)
        {
            var spellCaster = source as PlayerWizard;
            var targetWizard = target as PlayerWizard;
            if (targetWizard == null || spellCaster == null)
            {
                Debug.LogError("Spell is designed to target wizard");
                return false;
            }

            if (targetWizard.discoveredWizards == null) return false;

            foreach (var wizard in targetWizard.discoveredWizards)
            {
                if (spellCaster == GameManager.GetHumanWizard()) continue;
                var diplomacyStatus = targetWizard.GetDiplomacy().GetStatusToward(wizard.ID);
                diplomacyStatus.ChangeRelationshipBy(-25, true);
            }


            return true;
        }
        static public bool SWW_SpellOfMastery(ISpellCaster source, object target, Spell spell)
        {
            var w = source.GetWizardOwner();

            if (w.ID == PlayerWizard.HumanID())
            {
                w.SetGameScore(true, true);

                var gameMenager = GameManager.Get();
                if (gameMenager != null)
                {
                    var wizards = gameMenager.wizards;
                    var room = TheRoom.Open(w, TheRoom.RoomEvents.EnemiesDefeated, wizards);
                    if (room != null)
                    {
                        room.StartCoroutine(room.WaitWhileOpenThenEndGame());
                    }
                }
            }
            else
            {
                GameManager.GetHumanWizard().SetGameScore(false, false);

                var vic = UIManager.Open<Victory>(UIManager.Layer.Popup);
                vic.SetMessage("UI_YOU_ARE_OBLITERATED");
                vic.btKeepPlaying.gameObject.SetActive(false);
            }
            return true;
        }
        #endregion
        #region Spell Battle Global/Wizard
        static public bool SBW_ApplyWizardEnchantment(SpellCastData data, object target, Spell spell)
        {
            var w = target as BattlePlayer;
            if (w == null)
            {
                Debug.Log("Spell is designed to target BattlePlayer");
                return false;
            }

            foreach (var v in spell.enchantmentData)
            {
                w.AddEnchantment(v, data.caster as IEnchantable, v.lifeTime, null, spell.battleCost);
            }

            return true;
        }
        static public bool SBW_ApplyBattleEnchantment(SpellCastData data, object target, Spell spell)
        {
            if (!(target is Battle))
            {
                Debug.LogError("Spell " + spell.dbName + " is not targeting battle, while using script to do so");
                return false;
            }
            var b = target as Battle;
            var wizard = data.GetPlayerWizard();

            foreach (var v in spell.enchantmentData)
            {
                var dispellCost = spell.battleCost;
                if (wizard != null && wizard.GetMagicAndResearch().extensionItemSpellBattle != null)
                {
                    dispellCost += wizard.GetMagicAndResearch().extensionItemSpellBattle.extraMana;
                }
                b.AddEnchantment(v, data.caster as IEnchantable, v.lifeTime, null, dispellCost);
            }

            return true;
        }
        static public bool SBW_ApplyBattleEnchantmentUpdateVisibility(SpellCastData data, object target, Spell spell)
        { 
            if(SBW_ApplyBattleEnchantment(data, target, spell))
            {
                (target as Battle).UpdateInvisibility();
                return true;
            }
            return false;
        }
        static public bool SBW_ApplyBattleEnchantmentPrayerHighPrayer(SpellCastData data, object target, Spell spell)
        {
            if (SBW_ApplyBattleEnchantment(data, target, spell))
            {
                var battle = target as Battle;
                if (battle == null || spell == null || spell.enchantmentData == null) return false;

                EnchantmentInstance enchToRemove = null;
                foreach (var ench in battle.GetEnchantments())
                {
                    if (ench.source == (Enchantment)ENCH.PRAYER_GLOBAL &&
                        Array.Find(spell.enchantmentData, o => o == (Enchantment)ENCH.HIGH_PRAYER_GLOBAL) != null &&
                        ench.owner != null &&
                        data.GetWizardID() == MOMScripts.EnchantmentScripts.GetSpellCasterOwnerID(ench))
                    {
                        enchToRemove = ench;
                        break;
                    }
                }

                if (enchToRemove != null)
                {
                    if (battle != null)
                    {
                        battle.RemoveEnchantment(enchToRemove);
                    }

                }
            }
            return false;
        }

        static public bool SBW_ApplyWizardEnchantmentWithSacrifice(SpellCastData data, object target, Spell spell)
        {
            if (data.caster is BattleUnit)
            {
                var caster = data.caster as BattleUnit;
                int dmg = spell.fIntData[0].ToInt();
                int[] dmgBuffer = { dmg };
                bool def = caster.canDefend;
                caster.canDefend = false;
                caster.ApplyDamage(dmgBuffer, new MHRandom(), null, 0);
                caster.canDefend = def;

                SBW_ApplyWizardEnchantment(data, target, spell);
                
                return true;
            }

            return false;
        }
        static public bool SBW_FlameStrike(SpellCastData data, object target, Spell spell)
        {
            if (spell.fIntData[0] == null || spell.fIntData.Length == 0)
            {
                Debug.LogWarning("SBW_FlameStrike have miss FIntData");
                return false;
            }
            if (!(target is BattlePlayer))
            {
                Debug.LogWarning("SBW_FlameStrike is not targeting unit in battle");
                return false;
            }

            int[] dmg = new int[9];
            var baseDmg = spell.fIntData[0].ToInt();

            var b = Battle.GetBattle();

            foreach (var u in data.GetEnemyUnits())
            {
                if (!u.IsAlive()) continue;

                //reset base dmg strength
                dmg = ProduceAreaSpellDamage(baseDmg ,dmg, u.figureCount);

                if (u.attributes.Contains(TAG.MAGIC_IMMUNITY) ||
                    u.attributes.Contains(TAG.FIRE_IMMUNITY) ||
                    u.attributes.Contains(TAG.RIGHTEOUSNESS))
                    u.ApplyImmolationDamage(dmg, random, null, 50, true, null, null, spell);
                else if (u.attributes.Contains(TAG.ELEMENTAL_ARMOR))
                    u.ApplyImmolationDamage(dmg, random, null, 10, false, null, null, spell);
                else if (u.attributes.Contains(TAG.RESIST_ELEMENTS) ||
                    u.attributes.Contains(TAG.BLESS))
                    u.ApplyImmolationDamage(dmg, random, null, 3, false, null, null, spell);
                else if (u.attributes.Contains(TAG.LARGE_SHIELD))
                    u.ApplyImmolationDamage(dmg, random, null, 2, false, null, null, spell);
                else
                    u.ApplyImmolationDamage(dmg, random, null, 0, false, null, null, spell);

                if (!u.simulated && b != null)
                {
                    var effect = ((Spell)SPELL.FLAME_STRIKE).castEffect;
                    FSMBattleTurn.instance?.CastEffect(u.GetPosition(), effect);
                    u.GetOrCreateFormation(null, false)?.UpdateFigureCount();
                }
            }

            return true;
        }

        static public bool SBW_MassHealing(SpellCastData data, object target, Spell spell)
        {
            if (!(target is BattlePlayer))
            {
                Debug.LogWarning("SBW_FlameStrike is not targeting unit in battle");
                return false;
            }

            int heal = spell.fIntData[0].ToInt();
            var b = Battle.GetBattle();

            foreach (var u in data.GetFriendlyUnits())
            {
                if (!u.IsAlive()) continue;
                u.HealUnit(heal);
                if (!u.simulated && b != null && !u.canNaturalHeal)
                {
                    var effect = ((Spell)SPELL.MASS_HEALING).castEffect;
                    FSMBattleTurn.instance?.CastEffect(u.GetPosition(), effect);
                }
            }

            return true;
        }
        static public bool SBW_MassInvisibility(SpellCastData data, object target, Spell spell)
        {
            if (!(target is Battle))
            {
                Debug.LogError("Spell " + spell.dbName + " is not targeting battle, while using script to do so");
                return false;
            }

            foreach (var u in data.GetFriendlyUnits())
            {
                foreach (var v in spell.enchantmentData)
                {
                    if (u.GetEnchantments().Find(o => o.source == v) == null)
                    {
                        u.AddEnchantment(v, data.caster as IEnchantable, v.lifeTime, null, spell.battleCost);
                    }
                }
            }
            data.battle?.UpdateInvisibility();

            return true;
        }
        static public bool SBW_HolyWord(SpellCastData data, object target, Spell spell)
        {
            var b = Battle.GetBattle();

            if (!(target is BattlePlayer))
            {
                Debug.LogWarning("SBW_HolyWord is not targeting unit in battle");
                return false;
            }

            foreach (var u in data.GetEnemyUnits())
            {
                if (u.attributes.Contains(TAG.FANTASTIC_CLASS) &&
                    u.attributes.DoesNotContains((Tag)TAG.SPELL_LOCK))
                {
                    if (u.attributes.Contains(TAG.MAGIC_IMMUNITY))
                        u.ApplyResistFigureDeath(random, 0, 50, true, null, null, spell);
                    else if (u.attributes.Contains(TAG.REANIMATED) ||
                        u.race == (Race)RACE.REALM_DEATH)
                        u.ApplyResistFigureDeath(random, 7, 0, false, null, null, spell);
                    else if (u.GetSkills().Contains((Skill)SKILL.CHAOS_CHANNELS1) ||
                        u.GetSkills().Contains((Skill)SKILL.CHAOS_CHANNELS2) ||
                        u.GetSkills().Contains((Skill)SKILL.CHAOS_CHANNELS3))
                        u.ApplyResistFigureDeath(random, 2, 0, false, null, null, spell);
                    else
                        u.ApplyResistFigureDeath(random, 2, 0, false, null, null, spell);

                    if (!u.simulated && b != null)
                    {
                        var effect = ((Spell)SPELL.HOLY_WORD).castEffect;
                        FSMBattleTurn.instance?.CastEffect(u.GetPosition(), effect);
                    }
                }
                u.GetOrCreateFormation(null, false)?.UpdateFigureCount();
            }

            return true;
        }
        static public bool SBW_DeathSpell(SpellCastData data, object target, Spell spell)
        {
            int resisMod = (spell.fIntData[0] * FInt.N_ONE).ToInt();

            if (!(target is BattlePlayer))
            {
                Debug.LogWarning("SBW_DeathSpell is not targeting unit in battle");
                return false;
            }

            foreach (var u in data.GetEnemyUnits())
            {
                if (u.attributes.Contains(TAG.DEATH_IMMUNITY) ||
                    u.attributes.Contains(TAG.RIGHTEOUSNESS) ||
                    u.attributes.Contains(TAG.MAGIC_IMMUNITY))
                    u.ApplyResistFigureDeath(random, 0, 50 - resisMod, true, null, null, spell);
                else if (u.attributes.Contains(TAG.BLESS))
                    u.ApplyResistFigureDeath(random, 0, 3 - resisMod, false, null, null, spell);
                else
                    u.ApplyResistFigureDeath(random, 0, 2, false, null, null, spell);

                u.GetOrCreateFormation(null, false)?.UpdateFigureCount();
            }

            return true;
        }
        static public bool SBW_CallChaos(SpellCastData data, object target, Spell spell)
        {
            if (!(target is Battle))
            {
                Debug.LogError("Spell " + spell.dbName + " is not targeting battle, while using script to do so");
                return false;
            }
#if UNITY_EDITOR
            if (data.battle != null)
            {
                foreach (var v in data.battle.attackerUnits)
                {
                    if (v.IsAlive() && !data.battle.plane.GetSearcherData().IsUnitAt(v.GetPosition()))
                    {
                        Debug.LogError("unit missing marker on searcher data! before test");
                    }
                }
                foreach (var v in data.battle.defenderUnits)
                {
                    if (v.IsAlive() && !data.battle.plane.GetSearcherData().IsUnitAt(v.GetPosition()))
                    {
                        Debug.LogError("unit missing marker on searcher data! before test");
                    }
                }
            }
#endif



            foreach (var targetBu in data.GetEnemyUnits())
            {
                if (!targetBu.IsAlive()) continue;

                int vCase = random.GetInt(1, 9);

                string effect = ""; //visual effect, used for each component of this spell
                string sfx = ""; //audio effect for the above
                switch (vCase)
                {
                    case (2):
                        targetBu.HealUnit(5);
                        if (!targetBu.simulated && data.battle != null)
                        {
                            effect = ((Spell)SPELL.HEALING).castEffect;
                            sfx = ((Spell)SPELL.HEALING).audioEffect;
                            FSMBattleTurn.instance?.CastEffect(targetBu.GetPosition(), effect, sfx);
                        }
                        break;
                    case (3):

                        var uSkills = targetBu.GetSkills();
                        var potentialSkills = new List<DBReference<Skill>>();

                        for (int i = 0; i < 3; i++)
                        {
                            potentialSkills.Add(DataBase.Get(spell.stringData[i], false) as Skill);
                        }


                        foreach (var skill in uSkills)
                        {
                            if (potentialSkills.Contains(skill))
                                potentialSkills.Remove(skill);
                        }

                        if (potentialSkills.Count == 0) return false;

                        potentialSkills.RandomSort();
                        targetBu.AddSkill(potentialSkills[0].Get());
                        if (!targetBu.simulated && data.battle != null)
                        {
                            FSMBattleTurn.instance?.CastEffect(targetBu.GetPosition(), "Effect_ChaosSpell", "SpellChaos");
                            VerticalMarkerManager.Get().UpdateInfoOnMarker(targetBu);
                        }

                        if (potentialSkills[0].Get() == (Skill)SKILL.CHAOS_CHANNELS3)
                        {
                            var unitSkillScripts = targetBu.GetSkillManager().GetSkillScripts();
                            bool addFireBreath = false;
                            foreach (var skills in unitSkillScripts)
                            {
                                if (skills.Value == null) continue;
                                foreach (var e in skills.Value)
                                {
                                    if (e.activatorSecondary == null) continue;
                                    if (e.activatorSecondary.Contains("ACT_ApplyFireBreathAttack") != false)
                                    {
                                        addFireBreath = true;
                                        break;
                                    }
                                }
                            }

                            if (addFireBreath)
                            {
                                targetBu.AddSkill((Skill)SKILL.FIRE_BREATH);
                            }
                        }
                        break;
                    case (4):
                        var unitEnch = targetBu.GetEnchantments();
                        var potentialEnch = new List<Enchantment>();


                        for (int i = 3; i < 6; i++)
                            potentialEnch.Add(DataBase.Get(spell.stringData[i], false) as Enchantment);

                        foreach (var e in unitEnch)
                        {
                            if (e == null) return false;
                            var temp = e.source;

                            if (potentialEnch.Contains(temp))
                                potentialEnch.Remove(temp);
                        }

                        if (potentialEnch.Count == 0) return false;

                        if (random.GetInt(1, 11) > targetBu.attributes.GetFinal(TAG.RESIST) + ResistModFromEnch(null, targetBu, spell))
                        {
                            if (potentialEnch.Count > 0)
                            {
                                potentialEnch.RandomSort();
                                targetBu.AddEnchantment(potentialEnch[0], data.caster as IEnchantable, potentialEnch[0].lifeTime, null, spell.battleCost);
                                if (!targetBu.simulated && data.battle != null)
                                {
                                    effect = ((Spell)SPELL.WARP_CREATURE).castEffect;
                                    sfx = ((Spell)SPELL.WARP_CREATURE).audioEffect;
                                    FSMBattleTurn.instance?.CastEffect(targetBu.GetPosition(), effect, sfx);
                                }

                            }
                        }
                        break;
                    case (5):
                        SBG_FireBolt(data, targetBu, spell);
                        if (!targetBu.simulated && data.battle != null)
                        {
                            effect = ((Spell)SPELL.FIRE_BOLT).castEffect;
                            sfx = ((Spell)SPELL.FIRE_BOLT).audioEffect;
                            FSMBattleTurn.instance?.CastEffect(targetBu.GetPosition(), effect, sfx);
                        }
                        break;
                    case (6):
                        SBG_WarpLightning(data, targetBu, spell);
                        if (!targetBu.simulated && data.battle != null)
                        {
                            effect = ((Spell)SPELL.WARP_LIGHTNING).castEffect;
                            sfx = ((Spell)SPELL.WARP_LIGHTNING).audioEffect;
                            FSMBattleTurn.instance?.CastEffect(targetBu.GetPosition(), effect, sfx);
                        }
                        break;
                    case (7):
                        if (targetBu.attributes.DoesNotContains((Tag)TAG.MAGIC_IMMUNITY)
                        && targetBu.attributes.DoesNotContains((Tag)TAG.RIGHTEOUSNESS))
                        {
                            SBG_DoomBolt(data, targetBu, spell);
                            if (!targetBu.simulated && data.battle != null)
                            {
                                effect = ((Spell)SPELL.DOOM_BOLT).castEffect;
                                sfx = ((Spell)SPELL.DOOM_BOLT).audioEffect;
                                FSMBattleTurn.instance?.CastEffect(targetBu.GetPosition(), effect, sfx);
                            }
                        }
                        break;
                    case (8):
                        if (targetBu.attributes.DoesNotContains((Tag)TAG.MAGIC_IMMUNITY)
                        && targetBu.attributes.DoesNotContains((Tag)TAG.RIGHTEOUSNESS))
                        {
                            SBG_Disintegrate(data, targetBu, spell);
                            if (!targetBu.simulated && data.battle != null)
                            {
                                effect = ((Spell)SPELL.DISINTEGRATE).castEffect;
                                sfx = ((Spell)SPELL.DISINTEGRATE).audioEffect;
                                FSMBattleTurn.instance?.CastEffect(targetBu.GetPosition(), effect, sfx);
                            }
                        }
                        break;

                    default:
                        //On 1 there is no effect;
                        break;
                }
#if UNITY_EDITOR
                if (data.battle != null)
                {
                    foreach (var v in data.battle.attackerUnits)
                    {
                        if (v.IsAlive() && !data.battle.plane.GetSearcherData().IsUnitAt(v.GetPosition()))
                        {
                            Debug.LogError("unit missing marker on searcher data! after " + vCase);
                        }
                    }
                    foreach (var v in data.battle.defenderUnits)
                    {
                        if (v.IsAlive() && !data.battle.plane.GetSearcherData().IsUnitAt(v.GetPosition()))
                        {
                            Debug.LogError("unit missing marker on searcher data! after " + vCase);
                        }
                    }
                }
#endif
            }

            return true;
        }


        #endregion
        #region Spell World Summon (SWS)

        static public bool SWS_CraftArtefact(ISpellCaster source, Spell spell, object target, object data)
        {
            var magic = source.GetWizardOwner().GetMagicAndResearch();
            source.GetWizardOwner().artefacts.Add(magic.craftItemSpell.artefact);

            return true;

        }
        static public bool SWS_Summon(ISpellCaster source, Spell spell, object target, object data)
        {
            var w = source.GetWizardOwner();

            //find summoning circle
            //summon chosen creature

            if (target is MOM.Location)
            {
                var unit = DataBase.Get<DBDef.Unit>(spell.stringData[0], true);
                if (unit == null)
                {
                    Debug.LogError("Unit " + spell.stringData + " not found in database");
                    return false;
                }

                var u = MOM.Unit.CreateFrom(unit);

                var loc = target as MOM.Location;
                loc.GetLocalGroup().AddUnit(u);
                u.UpdateMP();

                if (w.IsHuman) TheRoom.Open(w, TheRoom.RoomEvents.UnitSummoned, u);
                return true;
            }
            else
            {
                //missing target, ie no summoning circle
            }
            return false;
        }

        static public bool SWS_SummonSpecificHero(ISpellCaster source, Spell spell, object target, object data)
        {
            var w = source.GetWizardOwner();

            //find summoning circle
            //summon chosen creature

            if (target is MOM.Location && w != null)
            {
                var unit = DataBase.Get<DBDef.Hero>(spell.stringData[0], true);
                if (unit == null)
                {
                    Debug.LogError("Unit " + spell.stringData + " not found in database");
                    return false;
                }

                var deadHero = w.GetDeadHeroes().Find(o => o.dbSource.Get() == unit);

                if (deadHero != null)
                {
                    MOM.Unit hero = DeadHero.ConvertDeadHeroToUnit(deadHero);
                    w.RemoveFromDeadHeroesList(deadHero);
                    var loc = target as MOM.Location;
                    loc.GetLocalGroup().AddUnit(hero);
                    hero.UpdateMP();
                    if (w.IsHuman) TheRoom.Open(w, TheRoom.RoomEvents.UnitSummoned, hero);
                }
                else
                {
                    var u = MOM.Unit.CreateFrom(unit);

                    var loc = target as MOM.Location;
                    loc.GetLocalGroup().AddUnit(u);
                    u.UpdateMP();
                    if (w.IsHuman) TheRoom.Open(w, TheRoom.RoomEvents.UnitSummoned, u);
                }

                return true;
            }
            else
            {
                //missing target, ie no summoning circle
            }
            return false;
        }
        static public bool SWS_SummonHero(ISpellCaster source, Spell spell, object target, object data)
        {
            var w = source.GetWizardOwner();

            //find summoning circle
            //summon random hero

            if (target is MOM.Location)
            {
//                 if (w.heroes.Count >= w.GetMaxHeroCount())
//                 {
//                     if (w.IsHuman)
//                         PopupGeneral.OpenPopup(null, "UI_HERO_SUMMONING_FAILED", "UI_HERO_LIST_FULL", "UI_OK");
//                     return false;
//                 }

                MOM.Unit u = data as MOM.Unit;
                if(u != null)
                {
                    var loc = target as MOM.Location;
                    loc.GetLocalGroup().AddUnit(u);
                    u.UpdateMP(); 
                    if (w.IsHuman) TheRoom.Open(w, TheRoom.RoomEvents.UnitSummoned, u);
                    return true;
                }
            }
            else
            {
                //missing target, ie no summoning circle
            }
            return false;
        }
        static public bool SWS_SummonChampion(ISpellCaster source, Spell spell, object target, object data)
        {
            var w = source.GetWizardOwner();

            //find summoning circle
            //summon random champion

            if (target is MOM.Location)
            {
                //                 if (w.heroes.Count >= w.GetMaxHeroCount())
                //                 {
                //                     if (w.IsHuman)
                //                         PopupGeneral.OpenPopup(null, "UI_HERO_SUMMONING_FAILED", "UI_HERO_LIST_FULL", "UI_OK");
                //                     return false;
                //                 }

                MOM.Unit u = data as MOM.Unit;
                if (u != null)
                {
                    var loc = target as MOM.Location;
                    loc.GetLocalGroup().AddUnit(u);
                    u.UpdateMP();
                    if (w.IsHuman) TheRoom.Open(w, TheRoom.RoomEvents.UnitSummoned, u);
                    return true;
                }
            }
            else
            {
                //missing target, ie no summoning circle
            }
            return false;
        }
        static public bool SWS_Resurrection(ISpellCaster source, Spell spell, object target, object data)
        {
            var w = source.GetWizardOwner();

            if (target is MOM.Location)
            {
                var unit = data as MOM.Unit; ;
                if (unit == null)
                {
                    Debug.LogError("Unit is null, or provided data is not a Unit");
                    return false;
                }

                var loc = target as MOM.Location;
                loc.GetLocalGroup().AddUnit(unit);
                unit.UpdateMP();
                return true;
            }
            else
            {
                Debug.LogError("SWS_Resurrection script: Missing target, ie no summoning circle");
            }
            return false;
        }
        static public bool SWS_UndeadHero(ISpellCaster source, Spell spell, object target, object data)
        {
            if (target is MOM.Location)
            {
                var unit = data as MOM.Unit; ;
                if (unit == null)
                {
                    Debug.LogError("Unit is null, or provided data is not a Unit");
                    return false;
                }

                var loc = target as MOM.Location;

                unit.canNaturalHeal = false;
                unit.canGainXP = false;
                DBDef.Skill skill = null;

                for (int i = 0; i < spell.stringData.Length; i++)
                {
                    var skillName = spell.stringData;
                    DBClass s = DataBase.Get(skillName[i], false);
                    if (s is Skill)
                    {
                        skill = s as Skill;
                        if (!unit.GetSkills().Contains(skill))
                        {
                            unit.AddSkill(skill);
                        }
                    }
                }

                loc.GetLocalGroup().AddUnit(unit);
                unit.UpdateMP();
                return true;
            }
            else
            {
                Debug.LogError("SWS_Resurrection script: Missing target, ie no summoning circle");
            }
            return false;
        }

        #endregion
        #region Spell World Other

        static public bool SWO_TransmuteResource(ISpellCaster source, Spell spell, object target, object data)
        {
            if (target is Vector3i)
            {
                WorldCode.Plane p = data as WorldCode.Plane;
                if (p == null)
                {
                    Debug.LogError("SWO_Transmute: cannot find proper Plane");
                    return false;
                }

                var h = p.GetHexAt((Vector3i)target);
                var transmuteTo = h.Resource.Get().transmuteTo;
                var g = AssetManager.Get<GameObject>(transmuteTo.GetModel3dName());
                if (g == null)
                {
                    Debug.Log("Spawn resource :" + transmuteTo.dbName + " model " + transmuteTo.descriptionInfo.graphic + " at " + h.Position + " graphic " + g + " failed");
                    return false;
                }

                GameObject.Destroy(h.resourceInstance);
                h.resourceInstance = null;
                h.Resource = transmuteTo;

                var pos = HexCoordinates.HexToWorld3D(h.Position);
                Chunk c = p.GetChunkFor(h.Position);
                MHRandom random = new MHRandom();
                var instance = GameObjectUtils.Instantiate(g, c.go.transform);
                instance.transform.localRotation = Quaternion.Euler(Vector3.up * random.GetFloat(0, 360));
                instance.transform.position = pos;
                h.resourceInstance = instance;

                List<GameObject> toDestroy = null;

                foreach (Transform ins in instance.transform)
                {
                    var itemPos = ins.position;
                    itemPos.y = p.GetHeightAt(itemPos, true);

                    var gOffset = ins.gameObject.GetComponent<GroundOffset>();

                    if (gOffset != null)
                    {
                        if (itemPos.y < 0)
                        {
                            if (toDestroy == null) toDestroy = new List<GameObject>();
                            toDestroy.Add(ins.gameObject);
                            continue;
                        }
                        itemPos.y += gOffset.heightOffset;
                    }
                    ins.position = itemPos;
                }
                if (toDestroy != null)
                {
                    foreach (var td in toDestroy)
                    {
                        GameObject.Destroy(td);
                    }
                }
                return true;
            }
            else
            {
                Debug.LogError("SWO_Transmute script: Missing target");
            }
            return false;
        }
        static public bool SWO_DetectMinerals(ISpellCaster source, Spell spell, object target, object data)
        {
            if (target is Vector3i)
            {
                WorldCode.Plane p = data as WorldCode.Plane;
                if (p == null)
                {
                    Debug.LogError("SWO_DetectMinerals: cannot find proper Plane");
                    return false;
                }

                var h = p.GetHexAt((Vector3i)target);
                var resources = DataBase.GetType<DBDef.Resource>();

                if (resources == null)
                    Debug.LogError("SWO_DetectMinerals: No resources to spawn.");

                resources.RandomSort();

                for (int i = 0; i < resources.Count; i++)
                {
                    if (resources[i].mineral)
                    {
                        h.Resource = resources[i];
                        break;
                    }
                }

                if (h.Resource == null)
                    Debug.LogError("SWO_DetectMinerals: No mineral resource.");

                var g = AssetManager.Get<GameObject>(h.Resource.Get().GetModel3dName());
                if (g == null)
                {
                    Debug.Log("Spawn resource :" + h.Resource.Get().dbName + " model " + h.Resource.Get().descriptionInfo.graphic + " at " + h.Position + " graphic " + g + " failed");
                    return false;
                }

                var pos = HexCoordinates.HexToWorld3D(h.Position);
                Chunk c = p.GetChunkFor(h.Position);
                MHRandom random = new MHRandom();
                var instance = GameObjectUtils.Instantiate(g, c.go.transform);
                instance.transform.localRotation = Quaternion.Euler(Vector3.up * random.GetFloat(0, 360));
                instance.transform.position = pos;
                h.resourceInstance = instance;

                List<GameObject> toDestroy = null;

                foreach (Transform ins in instance.transform)
                {
                    var itemPos = ins.position;
                    itemPos.y = p.GetHeightAt(itemPos, true);

                    var gOffset = ins.gameObject.GetComponent<GroundOffset>();

                    if (gOffset != null)
                    {
                        if (itemPos.y < 0)
                        {
                            if (toDestroy == null) toDestroy = new List<GameObject>();
                            toDestroy.Add(ins.gameObject);
                            continue;
                        }
                        itemPos.y += gOffset.heightOffset;
                    }
                    ins.position = itemPos;
                }
                if (toDestroy != null)
                {
                    foreach (var td in toDestroy)
                    {
                        GameObject.Destroy(td);
                    }
                }
                return true;
            }
            else
            {
                Debug.LogError("SWO_DetectMinerals script: Missing target");
            }
            return false;
        }
        static public bool SWO_SeaHarvest(ISpellCaster source, Spell spell, object target, object data)
        {
            if (target is Vector3i)
            {
                var pos = (Vector3i)target;
                var townRange = TownLocation.GetGeneralTownRange();

                WorldCode.Plane p = data as WorldCode.Plane;
                if (p == null)
                {
                    Debug.LogError("SWO_SeaHarvest: cannot find proper Plane");
                    return false;
                }

                List<MOM.Location> locs = GameManager.GetLocationsOfThePlane(p);
                var local = locs.Find(o => o.GetDistanceTo(pos) <= 2);

                if (local != null && local.owner != source.GetWizardOwnerID() && 
                    local.owner > 0)
                {
                    if(random.GetFloat(0f, 1.0f) > 0.2f)
                    {
                        PopupGeneral.OpenPopup(null, "UI_SEA_HARVEST_FAILED", "UI_SEA_HARVEST_FAILED_DES", "UI_OK");    
                        return false;
                    }   
                }

                var wiz = source.GetWizardOwner();
                if (wiz != null)
                {
                    var castingSkillDevelopment = 10;
                    var mana = random.GetInt(1, 51);
                    var money = random.GetInt(50, 101);

                    wiz.castingSkillDevelopment += castingSkillDevelopment;
                    wiz.mana += mana;
                    wiz.money += money;
                    if (GameManager.GetHumanWizard() == wiz)
                    {
                        HUD.Get().UpdateHUD();
                        var s = DBUtils.Localization.Get("UI_SEA_HARVEST_SUCCESS_DES", true, castingSkillDevelopment, mana, money);
                        PopupGeneral.OpenPopup(null, "UI_SEA_HARVEST_SUCCESS", s, "UI_OK");
                    }
                }

                var hex = p.GetHexAt(pos);
                hex.Resource = null;
                GameObject.Destroy(hex.resourceInstance);

                return true;
            }
            else
            {
                Debug.LogError("SWO_SeaHarvest script: Missing target");
            }
            return false;
        }
        static public bool SWO_ChangeTerrain(ISpellCaster source, Spell spell, object target, object data)
        {
            if (target is Vector3i)
            {
                WorldCode.Plane p = data as WorldCode.Plane;
                if (p == null)
                {
                    Debug.LogError("SWO_ChangeTerrain: cannot find proper Plane");
                    return false;
                }

                var h = p.GetHexAt((Vector3i)target);
                var changeTo = h.GetTerrain().transmuteTo;

                if(h.GetTerrain() == (DBDef.Terrain)TERRAIN.VOLCANO ||
                   h.GetTerrain() == (DBDef.Terrain)TERRAIN.MYR_VOLCANO)
                {
                    foreach (var w in GameManager.GetWizards())
                    {
                        var volcanos = w.GetVolcanoList();
                        int index = volcanos.FindIndex(o => o.t0 == (Vector3i)target && o.t1 == p.arcanusType);
                        if (index > -1)
                        {
                            w.RemoveVolcanoFromList(volcanos[index]);
                        }
                    }
                }

                h.SetTerrain(changeTo, p);
                h.UpdateHexProduction();

                HashSet<Vector3i> rebuildRequired = new HashSet<Vector3i>();
                rebuildRequired.Add((Vector3i)target);
                p.RebuildUpdatedTerrains(rebuildRequired);
                p.UpdateHeightsAfterTerrainChange((Vector3i)target);
                return true;
            }
            else
            {
                Debug.LogError("SWO_ChangeTerrain script: Missing target");
            }
            return false;
        }
        static public bool SWO_EnchantRoad(ISpellCaster source, Spell spell, object target, object data)
        {
            var plane = data as WorldCode.Plane;
            var v = (Vector3i)target;
            var h = plane.GetHexAt(v);

            if (h == null) return false;

            var t = plane.GetRoadManagers().GetRoadTypeAt(v);
            if (t == RoadManager.RoadType.Normal)
            {
                var area = HexNeighbors.GetRange(v, 2);
                foreach (var hPos in area)
                {
                    t = plane.GetRoadManagers().GetRoadTypeAt(hPos);
                    if (t == RoadManager.RoadType.Normal)
                    {
                        plane.GetRoadManagers().SetRoadMode(hPos, RoadManager.RoadType.Enchanted);
                    }
                }

                return true;
            }

            return false;
        }
        static public bool SWO_Corruption(ISpellCaster source, Spell spell, object target, object data)
        {
            if (target is Vector3i)
            {
                WorldCode.Plane p = data as WorldCode.Plane;
                if (p == null)
                {
                    Debug.LogError("SWO_Corruption: cannot find proper Plane");
                    return false;
                }

                var h = p.GetHexAt((Vector3i)target);
                h.ActiveHex = false;

                return true;
            }
            else
            {
                Debug.LogError("SWO_Corruption script: Missing target");
            }
            return false;
        }
        static public bool SWO_RaiseVolcano(ISpellCaster source, Spell spell, object target, object data)
        {
            if (target is Vector3i)
            {
                WorldCode.Plane p = data as WorldCode.Plane;
                if (p == null)
                {
                    Debug.LogError("SWO_RaiseVolcano: cannot find proper Plane");
                    return false;
                }

                DBDef.Terrain changeTo = p.arcanusType ? (DBDef.Terrain)TERRAIN.VOLCANO : (DBDef.Terrain)TERRAIN.MYR_VOLCANO;
                var h = p.GetHexAt((Vector3i)target);
                Debug.Log("index (" + h.index + ") hex changes terrain from " + h.GetTerrain() + " to " + changeTo);
                h.SetTerrain(changeTo, p);
                h.Resource = null;
                if (h.resourceInstance != null)
                {
                    GameObject.Destroy(h.resourceInstance);
                    h.resourceInstance = null;
                }

                HashSet<Vector3i> rebuildRequired = new HashSet<Vector3i>();
                rebuildRequired.Add((Vector3i)target);
                p.RebuildUpdatedTerrains(rebuildRequired);
                p.UpdateHeightsAfterTerrainChange((Vector3i)target);

                MOM.Location hexLocation = GameManager.GetLocationsOfThePlane(p).Find(o => o.Position == h.Position);
                if (hexLocation != null)
                {
                    // 15% chance to destroy building in town
                    if (hexLocation is TownLocation)
                    {
                        MHRandom random = new MHRandom();
                        var t = hexLocation as TownLocation;
                        var buildings = new List<DBReference<Building>>(t.buildings);
                        for (int i = buildings.Count - 1; i >= 0; i--)
                        {
                            if (!t.IsRegularBuilding(buildings[i])) continue;

                            if (random.GetFloat(0f, 1f) <= 0.15f)
                            {
                                t.RemoveBuilding(buildings[i]);
                            }
                        }
                    }
                }
                if (source is PlayerWizard)
                {
                    var w = source as PlayerWizard;
                    w.AddVolcano(h.Position, p.arcanusType);
                }
                else
                {
                    Debug.LogError("SWO_RaiseVolcano: source is no PlayerWizard type");
                }
                return true;
            }
            else
            {
                Debug.LogError("SWO_RaiseVolcano script: Missing target");
            }
            return false;
        }
        static public bool SWO_EarthLore(ISpellCaster source, Spell spell, object target, object data)
        {
            if (target is Vector3i)
            {
                WorldCode.Plane p = data as WorldCode.Plane;
                Vector3i v3i = (Vector3i)target;
                if (p == null)
                {
                    Debug.LogError("SWO_EarthLore: cannot find proper Plane");
                    return false;
                }

                var area = p.GetPositionWrapping(HexNeighbors.GetRange(v3i, 7));
                var locs = GameManager.GetLocationsOfThePlane(p.arcanusType);
                foreach (var v in area)
                {
                    FOW.Get().MarkVisible(v, p.arcanusType);

                    var l = locs.Find(o => o.GetPosition() == v);
                    if (l != null && !l.discovered)
                    {
                        l.MakeDiscovered();
                    }
                }
                FOW.Get().UpdateFogForPlane(p);

                return true;
            }
            else
            {
                Debug.LogError("SWO_EarthLore script: Missing target");
            }
            return false;
        }
        static public bool SWO_PowerSeeker(ISpellCaster source, Spell spell, object target, object data)
        {
            if (target is Vector3i)
            {
                WorldCode.Plane p = data as WorldCode.Plane;
                Vector3i v3i = (Vector3i)target;
                if (p == null)
                {
                    Debug.LogError("SWO_PowerSeeker: cannot find proper Plane");
                    return false;
                }

                var locs = GameManager.GetLocationsOfThePlane(p.arcanusType);
                foreach (var v in locs)
                {
                    if (v.locationType == ELocationType.Node)
                    {
                        if(!v.discovered)
                            v.MakeDiscovered();
                        
                        v.explored = true;
                        FOW.Get().MarkVisible(v.GetPosition(), p.arcanusType);
                    }
                }
                FOW.Get().UpdateFogForPlane(p);

                return true;
            }
            else
            {
                Debug.LogError("SWO_PowerSeeker script: Missing target");
            }
            return false;
        }

        #endregion
        #region Spell Battle casting value estimator for AI
        //provide integer value of the gain by casting spell on the target.
        //There is not need for AI script if spell modify only:
        //TAG.MELEE_ATTACK, TAG.MELEE_ATTACK_CHANCE
        //TAG.RANGE_ATTACK, TAG.RANGE_ATTACK_CHANCE
        //TAG.DEFENCE, TAG.DEFENCE_CHANCE
        //TAG.RESIST, Figures, HP

        static public int SBAI_Placeholder(SpellCastData data, object target, Spell spell)
        {

            return 0;
        }

        static public int SBAI_MeleeSummon(SpellCastData data, object target, Spell spell)
        {
            //summons 
            //this unit value is full only if summoned next to an enemy unit (melee summon behaviour)
            //otherwise its value is lowered by 10%
            if (spell.stringData == null || spell.stringData.Length < 1)
            {
                Debug.LogError("Spell " + spell.dbName + " miss summon info to work with");
                return 0;
            }
            var unit = DataBase.Get<DBDef.Unit>(spell.stringData[0], true);
            if (unit == null)
            {
                Debug.LogError("Unit " + spell.stringData + " not found in database");
                return 0;
            }
            if (!(target is Vector3i))
            {
                Debug.LogError("Target is not a location");
                return 0;
            }
            var battle = data.battle;
            if (battle == null) return BaseUnit.GetUnitStrength(unit);



            var pos = (Vector3i)target;
            if (!battle.IsLocationEmpty(pos))
            {
                Debug.LogError("Target location occupied");
                return 0;
            }
            var value = BaseUnit.GetUnitStrength(unit);

            var around = HexNeighbors.GetRange(pos, 1, 1);
            foreach (var v in around)
            {
                var bu = battle.GetUnitAt(v);
                if (bu != null)
                {
                    if (bu.ownerID != data.GetWizardID())
                    {
#if (UNITY_EDITOR && DEBUG_SPELLS)
                        Debug.Log(spell.dbName + " with script " +
                            spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + (int)value);
#endif
                        return value;
                    }
                }
            }
#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + (int)value);
#endif

            return (int)(value * 0.9f);
        }

        static public int SBAI_EarthToMud(SpellCastData data, object target, Spell spell)
        {

            if (!(target is HexCoordinates)) return 0;
            var hex = target as HexCoordinates;

            var distance = spell.fIntData[0];
            float value = 0f;

            var pos = (Vector3i)target;
            var battle = data.battle;
            foreach (var unit in battle.GetAllUnits())
            {
                if (HexCoordinates.HexDistance(unit.GetPosition(), pos) < distance &&
                    unit.attributes.DoesNotContains((Tag)TAG.TELEPORTING) &&
                    unit.attributes.DoesNotContains((Tag)TAG.CAN_FLY) &&
                    unit.attributes.DoesNotContains((Tag)TAG.NON_CORPOREAL) &&
                    unit.attributes.DoesNotContains((Tag)TAG.ELEMENTAL_ARMOR) &&
                    unit.attributes.DoesNotContains((Tag)TAG.RESIST_ELEMENTS) &&
                    unit.GetEnchantments().Find(o => o.source == (Enchantment)ENCH.EARTH_TO_MUD) == null)
                {
                    if (unit.ownerID == data.GetWizardID())
                        value -= unit.GetBattleUnitValue();
                    else
                        value += unit.GetBattleUnitValue();
                }
            }
#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " SBAI_ script value " + (int)value);
#endif

            return (int)value;
        }

        static public int SBAI_ResistElementals(SpellCastData data, object target, Spell spell)
        {
            if (!(target is BattleUnit)) return 0;
            var bu = target as BattleUnit;

            if (EnchAlreadyOnObject(data.caster, spell, bu)) return 0;
            //How much value that spell can give based on enemy units and enemy wizard spell books and target unit.
            int evaluationValue = 0;

            List<BattleUnit> enemyUnits = data.GetEnemyUnits();
            float value = 0f;

            if(enemyUnits.Count == 0)
            {
                return 0;
            }

            var enemyWizard = enemyUnits[0].GetWizardOwner();
            if (enemyWizard != null)
            {
                //For each book give 1 point of evaluation value
                evaluationValue += enemyWizard.GetAttributes().GetFinal((Tag)TAG.CHAOS_MAGIC_BOOK).ToInt();
                evaluationValue += enemyWizard.GetAttributes().GetFinal((Tag)TAG.NATURE_MAGIC_BOOK).ToInt();
            }

            //If enemy unit have skill from searched domains add 1 point to evaluation value. Max one point per unit.
            foreach (var u in enemyUnits)
            {
                if (!u.IsAlive()) continue;
                foreach (var s in u.GetSkills())
                {
                    if (s.Get().domain == ESkillDomain.Chaos)
                    {
                        evaluationValue++;
                        break;
                    }
                    if (s.Get().domain == ESkillDomain.Nature)
                    {
                        evaluationValue++;
                        break;
                    }
                }
            }
            if (evaluationValue == 0) return 0;

            //Average spell value based on target
            value = bu.GetModifiedBattleUnitValue(TAG.DEFENCE, (FInt)3.0) +
                             bu.GetModifiedBattleUnitValue(TAG.RESIST, (FInt)3.0) -
                             bu.GetBattleUnitValue() * 2;

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " + 
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + (int)value +
                " on unit " + bu.GetDBName().ToString());
#endif
            return (int)value;
        }

        static public int SBAI_ElementalArmor(SpellCastData data, object target, Spell spell)
        {
            if (!(target is BattleUnit)) return 0;
            var bu = target as BattleUnit;

            //How much value that spell can give bested on enemy units and enemy wizard spell books and target unit.
            int books = 0;

            List<BattleUnit> enemyUnits = data.GetEnemyUnits();
            float value = 0f;
            if(enemyUnits.Count == 0)
            {
                return 0;
            }

            var enemyWizard = enemyUnits[0].GetWizardOwner();
            if (enemyWizard != null)
            {
                //For each book give 1 point of evaluation value
                books += enemyWizard.GetAttributes().GetFinal((Tag)TAG.CHAOS_MAGIC_BOOK).ToInt();
                books += enemyWizard.GetAttributes().GetFinal((Tag)TAG.NATURE_MAGIC_BOOK).ToInt();
            }

            int elementalUnits = 0;
            int totalAlive = 0;
            //If enemy unit have skill from searched domains add 1 point to evaluation value. Max one point per unit.
            foreach (var u in enemyUnits)
            {
                if (!u.IsAlive()) continue;
                totalAlive++;
                foreach (var s in u.GetSkills())
                {
                    if (s.Get().domain == ESkillDomain.Chaos)
                    {
                        elementalUnits++;
                        break;
                    }
                    if (s.Get().domain == ESkillDomain.Nature)
                    {
                        elementalUnits++;
                        break;
                    }
                }
            }

            //value reaches maximum if all units are elemental or wizard have 8 relevant books or some of each
            float valueScalar = Mathf.Clamp01(books / 8f + elementalUnits / (float)totalAlive);
            if (valueScalar <= 0)
                value = 0f;
            else
            {
                //Average spell value based on target
                var spellValue = bu.GetModifiedBattleUnitValue(TAG.DEFENCE, (FInt)10.0) +
                                 bu.GetModifiedBattleUnitValue(TAG.RESIST, (FInt)10.0) -
                                 bu.GetBattleUnitValue() * 2;

                value = spellValue * valueScalar;
            }
#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + (int)value +
                " on unit " + bu.GetDBName().ToString());
#endif

            return (int)value;
        }
        static public int SBAI_Web(SpellCastData data, object target, Spell spell)
        {

            if (!(target is BattleUnit)) return 0;

            var targetBu = target as BattleUnit;
            if (EnchAlreadyOnObject(data.caster, spell, targetBu)) return 0;

            // if caster is hero
            BattleUnit hero = data.GetCasterAsBattleUnit();

            FInt value = (FInt)targetBu.GetBattleUnitValue();

            if (targetBu.GetAttributes().Contains(TAG.CAN_FLY))
            {
                value = value * (FInt)1.2;
            }

            var resistmod = FInt.ZERO;
            if (spell.fIntData != null && spell.fIntData.Length > 0)
            {
                resistmod = spell.fIntData[0];
            }

            var resistValue = (targetBu.GetAttributes().GetFinal((Tag)TAG.RESIST) + ResistModFromEnch(hero, targetBu, spell) - resistmod).ToInt();

            var valuePercent = (Mathf.Min((12 - resistValue) * 0.1f, 0.1f));

            value *= valuePercent;

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + value.ToInt() +
                " on unit " + targetBu.GetDBName().ToString());
#endif
            return value.ToInt();
        }

        static public int SBAI_CracksCall(SpellCastData data, object target, Spell spell)
        {

            if (!(target is BattleUnit)) return 0;
            var bu = target as BattleUnit;
            int value = 0;

            if (bu.GetAttFinal(TAG.CAN_FLY) == null && 
                bu.GetAttFinal(TAG.NON_CORPOREAL) == null)
            {
                value = bu.GetBattleUnitValue() / 3;
            }

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + value +
                " on unit " + bu.GetDBName().ToString());
#endif

            return value;
        }
        static public int SBAI_CallLightning(SpellCastData data, object target, Spell spell)
        {
            if (!(target is PlayerWizard)) return 0;
            var wizard = target as PlayerWizard;
            int value = 0;

            //this spell does 8 dmg * 0.3 chance
            float avDmg = 8 * 0.3f;
            //use multiplier to show increased value of this spell due to the fact it lasts many turns and hit multiple times each round
            //therefore gaining chance favor
            float multiplier = 3;

            // unit value as a target is estimated based on half of their resistance times their chance to defend.

            foreach (var unit in data.GetEnemyUnits())
            {
                if (!unit.IsAlive()) continue;

                int defBonus = 0;
                if (unit.attributes.Contains(TAG.MAGIC_IMMUNITY) ||
                        unit.attributes.Contains(TAG.RIGHTEOUSNESS))
                {
                    defBonus = 50;
                }
                else if (unit.attributes.Contains(TAG.ELEMENTAL_ARMOR))
                {
                    defBonus = 10;
                }
                else if (unit.attributes.Contains(TAG.RESIST_ELEMENTS) ||
                         unit.attributes.Contains(TAG.BLESS))
                {
                    defBonus = 3;
                }
                else if (unit.attributes.Contains(TAG.LARGE_SHIELD))
                {
                    defBonus = 2;
                }
                int defValue = (unit.GetAttFinal(TAG.DEFENCE) + defBonus).ToInt() / 2;
                FInt defChance = unit.GetAttFinal(TAG.DEFENCE_CHANCE);
                int unitFullValue = unit.GetBattleUnitValue();
                value += (int)(Mathf.Max(0, unitFullValue * multiplier * avDmg - defValue * defChance.ToFloat()));
            }

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + value +
                " on units.");
#endif

            return value;
        }
        static public int SBAI_Regeneration(SpellCastData data, object target, Spell spell)
        {
            if (!(target is BattleUnit)) return 0;
            var bu = target as BattleUnit;

            int value = bu.GetBattleUnitValue() / 2;

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + value +
                " on unit " + bu.GetDBName().ToString());
#endif

            return value;
        }

        static public int SBAI_Petrify(SpellCastData data, object target, Spell spell)
        {

            if (!(target is BattleUnit)) return 0;
            var targetBu = target as BattleUnit;

            FInt value = (FInt)targetBu.GetBattleUnitValue();

            // if caster is hero
            BattleUnit hero = data.GetCasterAsBattleUnit();

            var resistValue = targetBu.GetAttributes().GetFinal((Tag)TAG.RESIST).ToInt() + ResistModFromEnch(hero, targetBu, spell).ToInt();

            FInt spellValueMod = (FInt)0.8;
            var valuePercent = (Mathf.Clamp(10 - resistValue, 0, 10) * 0.1f);

            value = value * spellValueMod * valuePercent;

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + value.ToInt() +
                " on unit " + targetBu.GetDBName().ToString());
#endif

            return value.ToInt();
        }

        static public int SBAI_WarpWood(SpellCastData data, object target, Spell spell)
        {
            if (!(target is BattleUnit)) return 0;
            var bu = target as BattleUnit;

            var oldValue = bu.GetBattleUnitValue();
            var actualAmmo = bu.GetBaseFigure().rangedAmmo;
            bu.GetBaseFigure().rangedAmmo = 0;
            bu.GetAttributes().SetDirty();
            var newValue = bu.GetBattleUnitValue();
            bu.GetBaseFigure().rangedAmmo = actualAmmo;
            bu.GetAttributes().SetDirty();

            float value = oldValue - newValue;

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + (int)value +
                " on unit " + bu.GetDBName().ToString());
#endif

            return (int)value;
        }

        static public int SBAI_EldritchWeapon(SpellCastData data, object target, Spell spell)
        {
            if (!(target is BattleUnit)) return 0;
            var bu = target as BattleUnit;
            var buValue = bu.GetBattleUnitValue();
            var value = 0;

            List<BattleUnit> enemyUnits = data.GetEnemyUnits();

            //Check if units have proper skills
            foreach (var u in enemyUnits)
            {
                if (!u.IsAlive()) continue;
                if (u.attributes.Contains(TAG.WEAPON_IMMUNITY))
                    value += (int)(buValue * 0.1f);
            }

            //Average spell value based on target value change
            value = value +
                        bu.GetModifiedBattleUnitValue(TAG.MELEE_ATTACK_CHANCE, (FInt)0.1) +
                        bu.GetModifiedBattleUnitValue(TAG.RANGE_ATTACK_CHANCE, (FInt)0.1) -
                        buValue * 2;

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + value +
                " on unit " + bu.GetDBName().ToString());
#endif

            return value;
        }
        static public int SBAI_Shatter(SpellCastData data, object target, Spell spell)
        {
            if (!(target is BattleUnit)) return 0;
            var targetBu = target as BattleUnit;
            FInt buValue = (FInt)targetBu.GetBattleUnitValue();

            // if caster is hero
            BattleUnit hero = data.GetCasterAsBattleUnit();

            FInt value = (buValue * 4) -
                         targetBu.GetModifiedBattleUnitValue(TAG.DEFENCE, FInt.N_ONE) -
                         targetBu.GetModifiedBattleUnitValue(TAG.RANGE_ATTACK, FInt.N_ONE) -
                         targetBu.GetModifiedBattleUnitValue(TAG.THROW_BONUS, FInt.N_ONE) -
                         targetBu.GetModifiedBattleUnitValue(TAG.FIRE_BREATH_BONUS, FInt.N_ONE);


            FInt resRed = FInt.ZERO;
            if (spell.fIntData != null && spell.fIntData.Length > 0)
            {
                resRed = spell.fIntData[0];
            }

            var resistValue = (targetBu.GetAttributes().GetFinal((Tag)TAG.RESIST) + ResistModFromEnch(hero, targetBu, spell) - resRed).ToInt();

            var valuePercent = (Mathf.Clamp(10 - resistValue, 0, 10) * 0.1f);

            value = value * valuePercent;

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + value +
                " on unit " + targetBu.GetDBName().ToString());
#endif

            return value.ToInt();
        }
        static public int SBAI_WarpCreature(SpellCastData data, object target, Spell spell)
        {

            if (!(target is BattleUnit)) return 0;
            var targetBu = target as BattleUnit;
            var buMeleeAttackHalved = targetBu.attributes.GetFinal(TAG.MELEE_ATTACK) / 2;

            // if caster is hero
            BattleUnit hero = data.GetCasterAsBattleUnit();

            FInt resRed = FInt.ZERO;
            if (spell.fIntData != null && spell.fIntData.Length > 0)
            {
                resRed = spell.fIntData[0];
            }

            //There is check only for meleeAttack we do not know what spell will apply. 
            float value = targetBu.GetBattleUnitValue() - targetBu.GetModifiedBattleUnitValue
                (TAG.MELEE_ATTACK, (FInt)(buMeleeAttackHalved * -1));

            var resistValue = (targetBu.GetAttributes().GetFinal((Tag)TAG.RESIST) + ResistModFromEnch(hero, targetBu, spell) - resRed).ToInt();

            var valuePercent = (Mathf.Clamp(10 - resistValue, 0, 10) * 0.1f);

            value = value * valuePercent;

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + (int)value +
                " on unit " + targetBu.GetDBName().ToString());
#endif

            return (int)value;
        }

        static public int SBAI_FlameBlade(SpellCastData data, object target, Spell spell)
        {
            if (!(target is BattleUnit)) return 0;
            var bu = target as BattleUnit;
            var buValue = bu.GetBattleUnitValue();
            int value = 0;

            List<BattleUnit> enemyUnits = data.GetEnemyUnits();

            //Check if units have proper skills
            foreach (var u in enemyUnits)
            {
                if (!u.IsAlive()) continue;
                if (u.attributes.Contains(TAG.WEAPON_IMMUNITY))
                    value += (int)(buValue * 0.1f);
            }

            //Average spell value based on target
            value = bu.GetModifiedBattleUnitValue(TAG.MELEE_ATTACK, (FInt)2.0) +
                         bu.GetModifiedBattleUnitValue(TAG.RANGE_ATTACK, (FInt)2.0) +
                         bu.GetModifiedBattleUnitValue(TAG.THROW_BONUS, (FInt)2.0) -
                         (buValue * 3);

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + value +
                " on unit " + bu.GetDBName().ToString());
#endif

            return value;
        }

        static public int SBAI_MetalFires(SpellCastData data, object target, Spell spell)
        {
            int value = 0;

            List<BattleUnit> enemyUnits = data.GetEnemyUnits();
            List<BattleUnit> friendyUnits = data.GetFriendlyUnits();

            foreach (var u in friendyUnits)
            {
                if (!u.IsAlive()) continue;
                value += u.GetModifiedBattleUnitValue(TAG.MELEE_ATTACK, (FInt)1.0) +
                         u.GetModifiedBattleUnitValue(TAG.RANGE_ATTACK, (FInt)1.0) -
                         (u.GetBattleUnitValue() * 2);
            }

            //Check if units have proper skills
            foreach (var u in enemyUnits)
            {
                if (!u.IsAlive()) continue;
                if (u.attributes.Contains(TAG.WEAPON_IMMUNITY))
                    value += (int)(value * 0.1f);
            }

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + value +
                " on units.");
#endif

            return value;
        }

        static public int SBAI_Immolation(SpellCastData data, object target, Spell spell)
        {
            if (!(target is BattleUnit)) return 0;

            var bu = target as BattleUnit;
            var buValue = bu.GetBattleUnitValue();

            //Average spell value based on target value change
            var value = (int)(buValue * 0.8f);

            if (data != null)
            {
                var enemies = data.GetEnemyUnits();
                var ourUnits = data.GetFriendlyUnits();

                var targetAttack = bu.GetAttFinal(TAG.MELEE_ATTACK);
                foreach (var u in ourUnits)
                {
                    if (u.GetAttFinal(TAG.MELEE_ATTACK) > targetAttack)
                        return 0;
                }
                foreach (var u in enemies)
                {
                    if (u.GetAttFinal(TAG.FIRE_IMMUNITY) > 0)
                    {
                        value -= value / enemies.Count;
                        continue;
                    }
                }
            }

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + value +
                " on units.");
#endif

            return value;
        }

        static public int SBAI_Bless(SpellCastData data, object target, Spell spell)
        {
            if (!(target is BattleUnit)) return 0;

            var bu = target as BattleUnit;

            //How much value that spell can give bested on enemy units and enemy wizard spell books and target unit.
            int evaluationValue = 0;

            List<BattleUnit> enemyUnits = data.GetEnemyUnits();
            float value = 0f;
            if(enemyUnits.Count == 0)
            {
                return 0;
            }

            var enemyWizard = enemyUnits[0].GetWizardOwner();
            if (enemyWizard != null)
            {
                //For each book give 1 point of evaluation value
                evaluationValue += enemyWizard.GetAttributes().GetFinal((Tag)TAG.CHAOS_MAGIC_BOOK).ToInt();
                evaluationValue += enemyWizard.GetAttributes().GetFinal((Tag)TAG.DEATH_MAGIC_BOOK).ToInt();
            }

            //If enemy unit have skill from searched domains add 1 point to evaluation value. Max one point per unit.
            if (evaluationValue == 0)
            {
                foreach (var u in enemyUnits)
                {
                    if (!u.IsAlive()) continue;
                    foreach (var s in u.GetSkills())
                    {
                        if (s.Get().domain == ESkillDomain.Chaos)
                        {
                            evaluationValue++;
                            break;
                        }
                        if (s.Get().domain == ESkillDomain.Death)
                        {
                            evaluationValue++;
                            break;
                        }
                    }
                }
            }

            //Average spell value based on target
            var spellValue = bu.GetModifiedBattleUnitValue(TAG.DEFENCE, (FInt)3.0) +
                             bu.GetModifiedBattleUnitValue(TAG.RESIST, (FInt)3.0) -
                             bu.GetBattleUnitValue() * 2;

            if (evaluationValue <= 0)
                value = 0f;
            else
                value = spellValue;

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + (int)value +
                " on unit " + bu.GetDBName().ToString());
#endif

            return (int)value;
        }

        static public int SBAI_HolyWeapon(SpellCastData data, object target, Spell spell)
        {
            if (!(target is BattleUnit)) return 0;
            var bu = target as BattleUnit;
            var buValue = bu.GetBattleUnitValue();
            var value = 0;

            List<BattleUnit> enemyUnits = data.GetEnemyUnits();

            //Check if units have proper skills
            foreach (var u in enemyUnits)
            {
                if (!u.IsAlive()) continue;
                if (u.attributes.Contains(TAG.WEAPON_IMMUNITY))
                    value += (int)(buValue * 0.1f);
            }

            //Average spell value based on target value change
            value = bu.GetModifiedBattleUnitValue(TAG.MELEE_ATTACK_CHANCE, (FInt)0.1) +
                        bu.GetModifiedBattleUnitValue(TAG.RANGE_ATTACK_CHANCE, (FInt)0.1) -
                        buValue * 2;

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + value +
                " on unit " + bu.GetDBName().ToString());
#endif

            return value;
        }

        static public int SBAI_TrueSight(SpellCastData data, object target, Spell spell)
        {
            if (!(target is BattleUnit)) return 0;
            var bu = target as BattleUnit;
            if (bu.GetAttFinal(TAG.ILLUSIONS_IMMUNITY) > 0) return 0;

            //How much value that spell can give bested on enemy units and enemy wizard spell books and target unit.
            int evaluationValue = 0;

            List<BattleUnit> enemyUnits = data.GetEnemyUnits();
            float value = 0f;

            if(enemyUnits.Count == 0)
            {
                return 0;
            }

            //Check if wizard have proper magic books
            var enemyWizard = enemyUnits[0].GetWizardOwner();
            if (enemyWizard != null)
            {
                evaluationValue += enemyWizard.GetAttributes().GetFinal((Tag)TAG.SORCERY_MAGIC_BOOK).ToInt();
            }

            //Check if units have proper skills
            foreach (var u in enemyUnits)
            {
                if (!u.IsAlive()) continue;
                if (u.GetAttFinal((Tag)TAG.INVISIBILITY) > 0)
                    evaluationValue++;
            }

            if (data.battle != null)
            {
                if (data.battle.attacker.GetID() == bu.ownerID &&
                    bu.GetAttFinal((Tag)TAG.AMMUNITION) > 0 && data.battle.darknessWall)
                    evaluationValue++;
            }

            if (evaluationValue > 2)
            {
                foreach (var u in data.GetFriendlyUnits())
                {
                    if (!u.IsAlive()) continue;
                    if (u.GetAttFinal((Tag)TAG.ILLUSIONS_IMMUNITY) < 1)
                        evaluationValue++;
                }
            }

            //Average spell value based on target
            var spellValue = bu.GetBattleUnitValue() * 2;

            if (evaluationValue <= 0)
                value = 0f;
            else
            {
                if (evaluationValue >= 2) value = spellValue * 2;
                else value = spellValue;
            }

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + (int)value +
                " on unit " + bu.GetDBName().ToString());
#endif


            return (int)value;
        }

        static public int SBAI_Invulnerability(SpellCastData data, object target, Spell spell)
        {
            if (!(target is BattleUnit)) return 0;
            var bu = target as BattleUnit;
            var buValue = bu.GetBattleUnitValue();

            //Spell gives 2 points of invulnerability what is something like ~6 defence points.
            var value = bu.GetModifiedBattleUnitValue(TAG.DEFENCE, (FInt)6.0) - buValue;

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + value +
                " on unit " + bu.GetDBName().ToString());
#endif

            return value;
        }

        static public int SBAI_Righteousness(SpellCastData data, object target, Spell spell)
        {

            if (!(target is BattleUnit)) return 0;
            var bu = target as BattleUnit;

            //How much value that spell can give bested on enemy units and enemy wizard spell books and target unit.
            int evaluationValue = 0;

            List<BattleUnit> enemyUnits = data.GetEnemyUnits();
            float value = 0f;
            if(enemyUnits.Count == 0)
            {
                return 0;
            }

            var enemyWizard = enemyUnits[0].GetWizardOwner();
            if (enemyWizard != null)
            {
                //For each book give 1 point of evaluation value
                evaluationValue += enemyWizard.GetAttributes().GetFinal((Tag)TAG.CHAOS_MAGIC_BOOK).ToInt();
                evaluationValue += enemyWizard.GetAttributes().GetFinal((Tag)TAG.DEATH_MAGIC_BOOK).ToInt();
            }

            //If enemy unit have skill from searched domains add 1 point to evaluation value. Max one point per unit.
            foreach (var u in enemyUnits)
            {
                if (!u.IsAlive()) continue;
                foreach (var s in u.GetSkills())
                {
                    if (s.Get().domain == ESkillDomain.Chaos)
                    {
                        evaluationValue++;
                        break;
                    }
                    if (s.Get().domain == ESkillDomain.Death)
                    {
                        evaluationValue++;
                        break;
                    }
                }
            }

            //Average spell value based on target
            var spellValue = bu.GetModifiedBattleUnitValue(TAG.DEFENCE, (FInt)20.0) +
                             bu.GetModifiedBattleUnitValue(TAG.RESIST, (FInt)20.0) -
                             bu.GetBattleUnitValue() * 2;

            if (evaluationValue <= 0)
                value = 0f;
            else
                value = spellValue;

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + (int)value +
                " on unit " + bu.GetDBName().ToString());
#endif

            return (int)value;
        }

        static public int SBAI_CloakOfFear(SpellCastData data, object target, Spell spell)
        {
            if (!(target is BattleUnit)) return 0;

            var bu = target as BattleUnit;
            var buValue = bu.GetBattleUnitValue();

            var value = 0;

            //Average spell value based on target value change
            if (buValue > 1500)
                value = (int)(buValue * 0.4f);
            else if (buValue > 3000)
                value = (int)(buValue * 0.2f);
            else
                value = (int)(buValue * 0.5f);

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + value +
                " on units.");
#endif

            return value;
        }

        static public int SBAI_BlackSleep(SpellCastData data, object target, Spell spell)
        {

            if (!(target is BattleUnit)) return 0;
            var targetBu = target as BattleUnit;

            FInt value = (FInt)targetBu.GetBattleUnitValue();

            // if caster is hero
            BattleUnit hero = data.GetCasterAsBattleUnit();

            var resistmod = FInt.ZERO;
            if (spell.fIntData != null && spell.fIntData.Length > 0)
            {
                resistmod = spell.fIntData[0];
            }

            var resistValue = (targetBu.GetAttributes().GetFinal((Tag)TAG.RESIST) + ResistModFromEnch(hero, targetBu, spell) - resistmod).ToInt();

            var valuePercent = (Mathf.Clamp(10 - resistValue, 0, 10) * 0.1f);

            value = value * valuePercent;

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + value.ToInt() +
                " on unit " + targetBu.GetDBName().ToString());
#endif

            return value.ToInt();
        }
        static public int SBAI_Weakness(SpellCastData data, object target, Spell spell)
        {

            if (!(target is BattleUnit)) return 0;
            var targetBu = target as BattleUnit;

            FInt value = (FInt)targetBu.GetBattleUnitValue();

            // if caster is hero
            BattleUnit hero = data.GetCasterAsBattleUnit();

            var resistmod = FInt.ZERO;
            if (spell.fIntData != null && spell.fIntData.Length > 0)
            {
                resistmod = spell.fIntData[0];
            }

            var resistValue = (targetBu.GetAttributes().GetFinal((Tag)TAG.RESIST) + ResistModFromEnch(hero, targetBu, spell) - resistmod).ToInt();

            var valuePercent = (Mathf.Clamp(10 - resistValue, 0, 10) * 0.1f);

            value = value * valuePercent;

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + value.ToInt() +
                " on unit " + targetBu.GetDBName().ToString());
#endif

            return value.ToInt();
        }

        static public int SBAI_LifeDrain(SpellCastData data, object target, Spell spell)
        {

            if (!(target is BattleUnit)) return 0;
            var targetBu = target as BattleUnit;

            // if caster is hero
            BattleUnit hero = data.GetCasterAsBattleUnit();

            FInt value = (FInt)targetBu.GetBattleUnitValue();

            var resistValue = targetBu.GetAttributes().GetFinal((Tag)TAG.RESIST).ToInt() + ResistModFromEnch(hero, targetBu, spell).ToInt();

            var valuePercent = (Mathf.Clamp(10 - resistValue, 0, 10) * 0.1f);

            value = value * valuePercent * (FInt)1.2;

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + value.ToInt() +
                " on unit " + targetBu.GetDBName().ToString());
#endif

            return value.ToInt();
        }

        static public int SBAI_WraithForm(SpellCastData data, object target, Spell spell)
        {
            if (!(target is BattleUnit)) return 0;

            var targetBu = target as BattleUnit;
            var buValue = targetBu.GetBattleUnitValue();
            var buDef = (targetBu.attributes.GetFinal(TAG.DEFENCE)).ToInt();

            var value = 0;

            switch (buDef)
            {
                case 1:
                    value = (buValue * (FInt)0.7).ToInt();
                    break;
                case 2:
                    value = (buValue * (FInt)0.6).ToInt();
                    break;
                case 3:
                    value = (buValue * (FInt)0.5).ToInt();
                    break;
                case 4:
                    value = (buValue * (FInt)0.4).ToInt();
                    break;
                case 5:
                    value = (buValue * (FInt)0.3).ToInt();
                    break;
                case 6:
                    value = (buValue * (FInt)0.3).ToInt();
                    break;
                case 7:
                    value = (buValue * (FInt)0.2).ToInt();
                    break;
                case 8:
                    value = (buValue * (FInt)0.2).ToInt();
                    break;
                case 9:
                    value = (buValue * (FInt)0.1).ToInt();
                    break;

                default:
                    value = 0;
                    break;
            }

            if (targetBu.attributes.Contains(TAG.WEAPON_IMMUNITY)) value = 0;
            if (targetBu.attributes.Contains(TAG.NON_CORPOREAL)) value = value / 2;

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + value +
                " on unit " + targetBu.GetDBName().ToString());
#endif


            return value;
        }
        static public int SBAI_GuardianWind(SpellCastData data, object target, Spell spell)
        {
            if (!(target is BattleUnit)) return 0;

            var bu = target as BattleUnit;
            var buValue = bu.GetBattleUnitValue();

            List<BattleUnit> enemyUnits = data.GetEnemyUnits();
            var evaluationValue = 0;

            evaluationValue = enemyUnits.FindAll(o => o.IsAlive() && 
                                                      o.currentlyVisible && 
                                                      o.GetCurentFigure().rangedAmmo > 0 && 
                                                      o.attributes.GetFinal(TAG.NORMAL_RANGE) > 0).Count;

            //Average spell value based on target value change
            var value = (int)(buValue * 0.2f) * evaluationValue;

            if (bu.attributes.Contains(TAG.MISSILE_IMMUNITY)) value = 0;

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + value +
                " on unit " + bu.GetDBName().ToString());
#endif


            return value;
        }
        static public int SBAI_DispelMagic(SpellCastData data, object target, Spell spell)
        {
            if (!(target is BattleUnit)) return 0;
            var bu = target as BattleUnit;
            var buValue = bu.GetBattleUnitValue();

            var value = 0;
            var evaluationValue = 0;

            foreach (var e in bu.GetEnchantments())
            {
                var ench = e.source;
                if (ench.Get().enchCategory == EEnchantmentCategory.Negative &&
                    bu.ownerID == data.GetWizardID() ||
                    ench.Get().enchCategory == EEnchantmentCategory.Positive &&
                    bu.ownerID != data.GetWizardID())
                {
                    evaluationValue++;
                }
            }
            if (evaluationValue == 0)
            {
#if (UNITY_EDITOR && DEBUG_SPELLS)
                Debug.Log(spell.dbName + " with script " +
                    spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + 0 +
                    " on unit " + bu.GetDBName().ToString());
#endif
                return 0;
            }

            switch (evaluationValue)
            {
                case 1:
                    value = (buValue * (FInt)0.5).ToInt();
                    break;
                case 2:
                    value = (buValue * (FInt)0.7).ToInt();
                    break;
                case 3:
                    value = (buValue * (FInt)0.9).ToInt();
                    break;
                case 4:
                    value = (buValue * (FInt)1.1).ToInt();
                    break;

                default:
                    value = (buValue * (FInt)1.3).ToInt();
                    break;
            }

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + value +
                " on unit " + bu.GetDBName().ToString());
#endif

            return value;
        }

        static public int SBAI_Blur(SpellCastData data, object target, Spell spell)
        {
            var value = 0;

             List <BattleUnit> enemyUnits = data.GetEnemyUnits().FindAll(o => o.IsAlive());
            if (enemyUnits.Count == 0) return 0;

            //Find unit with biggest value of melee or range attack in enemy units 
            enemyUnits.Sort(delegate (BattleUnit a, BattleUnit b)
            {
                return a.GetAttFinal(TAG.MELEE_ATTACK).CompareTo(b.GetAttFinal(TAG.MELEE_ATTACK));
            });
            BattleUnit higestMeleeAttacker = enemyUnits[enemyUnits.Count - 1];

            enemyUnits.Sort(delegate (BattleUnit a, BattleUnit b)
            {
                return a.GetAttFinal(TAG.RANGE_ATTACK).CompareTo(b.GetAttFinal(TAG.RANGE_ATTACK));
            });
            BattleUnit higestRangeAttacker = enemyUnits[enemyUnits.Count - 1];


            //Spell gives blur what is something like ~3 defence points.
            if (higestMeleeAttacker.GetAttFinal(TAG.MELEE_ATTACK) > higestRangeAttacker.GetAttFinal(TAG.RANGE_ATTACK))
                value = higestMeleeAttacker.GetBattleUnitValue() / 3;
            else
                value = higestRangeAttacker.GetBattleUnitValue() / 3;

            //Until correction in core code there cannot be subtract smaller FInts then 1
            /*if (higestMeleeAttacker.GetAttFinal(TAG.MELEE_ATTACK) > higestRangeAttacker.GetAttFinal(TAG.RANGE_ATTACK))
                value = higestMeleeAttacker.GetBattleUnitValue() - 
                    higestMeleeAttacker.GetModifiedBattleUnitValue(TAG.MELEE_ATTACK_CHANCE, (FInt)(-0.1));
            else
                value = higestRangeAttacker.GetBattleUnitValue() - 
                    higestRangeAttacker.GetModifiedBattleUnitValue(TAG.RANGE_ATTACK_CHANCE, (FInt)(-0.1));*/

            value = value * (enemyUnits.Count / 2 + 1);

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + value +
                " on unit" + bu.GetDBName().ToString());
#endif

            return value;
        }
        static public int SBAI_Vertigo(SpellCastData data, object target, Spell spell)
        {

            if (!(target is BattleUnit)) return 0;
            var targetBu = target as BattleUnit;
            var buValue = (FInt)targetBu.GetBattleUnitValue();

            // if caster is hero
            BattleUnit hero = data.GetCasterAsBattleUnit();

            //Until correction in core code there cannot be subtract smaller FInts then 1
            /*FInt value = buValue * 3 -
                (FInt)targetBu.GetModifiedBattleUnitValue(TAG.MELEE_ATTACK, (FInt)(-0.2)) -
                (FInt)targetBu.GetModifiedBattleUnitValue(TAG.RANGE_ATTACK_CHANCE, (FInt)(-0.2)) -
                (FInt)targetBu.GetModifiedBattleUnitValue(TAG.DEFENCE, FInt.ONE);*/

            var resRed = FInt.ZERO;
            if (spell.fIntData != null && spell.fIntData.Length > 0)
            {
                resRed = spell.fIntData[0];
            }
            var resistValue = (targetBu.GetAttributes().GetFinal((Tag)TAG.RESIST) + ResistModFromEnch(hero, targetBu, spell) - resRed).ToInt();

            var valuePercent = (Mathf.Clamp(10 - resistValue, 0, 10) * 0.1f);

            FInt value = buValue * valuePercent;

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + value.ToInt() +
                " on unit " + targetBu.GetDBName().ToString());
#endif

            return value.ToInt();
        }

        static public int SBAI_SpellLock(SpellCastData data, object target, Spell spell)
        {
            if (!(target is BattleUnit)) return 0;
            var bu = target as BattleUnit;
            var buValue = bu.GetBattleUnitValue();

            if (data.GetEnemyUnits() != null && data.GetEnemyUnits().Count > 0 &&
                data.GetEnemyUnits()[0].GetWizardOwnerID() <= 0)
            {
                return 0;
            }

            var value = 0;
            var evaluationValue = 0;

            foreach (var e in bu.GetEnchantments())
            {
                var ench = e.source;
                if (ench.Get().enchCategory == EEnchantmentCategory.Positive &&
                    bu.ownerID == data.GetWizardID())
                {
                    evaluationValue++;
                }
            }

            if (evaluationValue == 0) return 0;
            switch (evaluationValue)
            {
                case 1:
                    value = (buValue * (FInt)0.5).ToInt();
                    break;
                case 2:
                    value = (buValue * (FInt)0.6).ToInt();
                    break;
                case 3:
                    value = (buValue * (FInt)0.7).ToInt();
                    break;
                case 4:
                    value = (buValue * (FInt)0.8).ToInt();
                    break;

                default:
                    value = (buValue * (FInt)0.8).ToInt();
                    break;
            }



#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + value +
                " on unit.");
#endif

            return value;
        }

        static public int SBAI_Flight(SpellCastData data, object target, Spell spell)
        {
            if (!(target is BattleUnit)) return 0;

            var bu = target as BattleUnit;
            var buValue = bu.GetBattleUnitValue();

            //Average spell value based on target value change
            var value = (int)(buValue * 0.3f);

            if (bu.attributes.Contains(TAG.CAN_FLY)) value = 0;

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + value +
                " on unit " + bu.GetDBName().ToString());
#endif


            return value;
        }

        static public int SBAI_MagicImmunity(SpellCastData data, object target, Spell spell)
        {
            if (!(target is BattleUnit)) return 0;

            var bu = target as BattleUnit;

            var buValue = bu.GetBattleUnitValue();

            //Average spell value based on target value change
            var value = (int)(buValue * 0.7f);

            if (bu.attributes.Contains(TAG.MAGIC_IMMUNITY)) value = 0;

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + value +
                " on unit " + bu.GetDBName().ToString());
#endif


            return value;
        }

        static public int SBAI_Haste(SpellCastData data, object target, Spell spell)
        {
            if (!(target is BattleUnit)) return 0;

            var bu = target as BattleUnit;
            var buValue = bu.GetBattleUnitValue();
            var value = 0;

            //Average spell value based on target value change
            if (buValue > 1500)
                value = (int)(buValue * 1.0f);
            else if (buValue > 3000)
                value = (int)(buValue * 1.5f);
            else
                value = (int)(buValue * 2f);

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + value +
                " on unit " + bu.GetDBName().ToString());
#endif

            return value;
        }
        static public int SBAI_Entangle(SpellCastData data, object target, Spell spell)
        {
            if (!(target is PlayerWizard)) return 0;
            var wizard = target as PlayerWizard;
            int value = 0;
            var unitValue = 500;

            var enemyUnits = data.GetEnemyUnits();

            var units = enemyUnits.FindAll(
            o => o.IsAlive() && 
                 o.GetAttFinal(TAG.CAN_FLY) == 0 && 
                 o.GetAttFinal(TAG.NON_CORPOREAL) == 0);

            value = units.Count * unitValue;

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + value +
                " on units.");
#endif

            return value;
        }
        static public int SBAI_Disrupt(SpellCastData data, object target, Spell spell)
        {
            //ToDo: AI do not know how to cast that Spell
            return 0;
            var battle = data.battle;
            if (!(target is Vector3i) || battle == null) return 0;

            float value = 0f;

            var pos = (Vector3i)target;

            foreach (var unit in battle.attackerUnits)
            {
                if (HexCoordinates.HexDistance(unit.battlePosition, pos) < 4)
                {
                    foreach (var dUnit in battle.defenderUnits)
                    {
                        if (HexCoordinates.HexDistance(dUnit.battlePosition, pos) < 3)
                        {
                            value = (unit.GetBattleUnitValue() + dUnit.GetBattleUnitValue()) / 2;
                        }
                    }
                }
            }


#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " SBAI_ script value " + (int)value);
#endif

            return (int)value;
        }
        static public int SBAI_WallOfFire(SpellCastData data, object target, Spell spell)
        {
            var battle = data.battle;

            if (!(battle != null))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }
            int value = 0;
            var location = battle.gDefender.GetLocationHostSmart();
            var town = location as TownLocation;
            if (town == null) return value;

            var townValue = town.GetStrategicValue();

            //Average spell value based on target
            value = (int)(townValue * 0.5f);

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + value +
                " on town " + town.name);
#endif

            return value;
        }
        static public int SBAI_WarpReality(SpellCastData data, object target, Spell spell)
        {
            int value = 0;

            foreach (var u in data.GetFriendlyUnits())
            {
                if (!u.IsAlive()) continue;
                if (u.dbSource.Get().race != (Race)RACE.REALM_CHAOS)
                {
                    FInt skill = FInt.Max(u.GetAttFinal(TAG.MELEE_ATTACK_CHANCE),
                        u.GetAttFinal(TAG.RANGE_ATTACK_CHANCE));
                    if (skill < 0.2f)
                    {
                        value -= (int)u.GetBattleUnitValue();
                    }
                    else
                    {
                        value -= (int)(u.GetBattleUnitValue() * (0.2f / skill.ToFloat()));
                    }
                }
                else
                {
                    FInt skill = FInt.Max(u.GetAttFinal(TAG.MELEE_ATTACK_CHANCE),
                        u.GetAttFinal(TAG.RANGE_ATTACK_CHANCE));
                    if (skill < 0.2f)
                    {
                        value += (int)u.GetBattleUnitValue();
                    }
                    else
                    {
                        value += (int)(u.GetBattleUnitValue() * (0.2f / skill.ToFloat()));
                    }
                }
            }
            foreach (var u in data.GetEnemyUnits())
            {
                if (!u.IsAlive()) continue;
                if (u.dbSource.Get().race == (Race)RACE.REALM_CHAOS)
                {
                    FInt skill = FInt.Max(u.GetAttFinal(TAG.MELEE_ATTACK_CHANCE),
                        u.GetAttFinal(TAG.RANGE_ATTACK_CHANCE));
                    if (skill < 0.2f)
                    {
                        value -= (int)u.GetBattleUnitValue();
                    }
                    else
                    {
                        value -= (int)(u.GetBattleUnitValue() * (0.2f / skill.ToFloat()));
                    }
                }
                else
                {
                    FInt skill = FInt.Max(u.GetAttFinal(TAG.MELEE_ATTACK_CHANCE),
                        u.GetAttFinal(TAG.RANGE_ATTACK_CHANCE));
                    if (skill < 0.2f)
                    {
                        value += (int)u.GetBattleUnitValue();
                    }
                    else
                    {
                        value += (int)(u.GetBattleUnitValue() * (0.2f / skill.ToFloat()));
                    }
                }
            }

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript .ToString() + " give SpellAI value " + value +
                " on battlefield.");
#endif

            return value;
        }
        static public int SBAI_FlameStrike(SpellCastData data, object target, Spell spell)
        {
            int value = 0;

            var targetWizard = target as PlayerWizard;

            foreach (var unit in data.GetEnemyUnits())
            {
                if (!unit.IsAlive()) continue;
                if (unit.attributes.Contains(TAG.FIRE_IMMUNITY) ||
                        unit.attributes.Contains(TAG.MAGIC_IMMUNITY) ||
                        unit.attributes.Contains(TAG.RIGHTEOUSNESS))
                {
                    continue;
                }

                //note, each figure is attacked independently, therefore spell strength is valued as if targetting figures not units.
                float hp = unit.GetAttFinal(TAG.HIT_POINTS).ToFloat();
                float def = unit.GetAttFinal(TAG.DEFENCE).ToFloat();
                float spellStrength = 15;

                //apply attack and def efficiency 30%
                def *= .3f;
                spellStrength *= .3f;

                float expectedDmg = spellStrength - def;
                if (expectedDmg <= 0) continue;
                expectedDmg = Mathf.Min(expectedDmg, hp);

                value += (int)(unit.GetBattleUnitValue() * expectedDmg / hp);
            }

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + value +
                " on battlefield.");
#endif

            return value;
        }
        static public int SBAI_CallChaos(SpellCastData data, object target, Spell spell)
        {
            int value = 0;
            //max effect value is limited due to one option not affecting enemy, and overall chance to neutralize unit being limited as well
            var maxEffectValue = 0.5f;
            var death = (Race)RACE.REALM_DEATH;

            foreach (var u in data.GetEnemyUnits())
            {
                if (!u.IsAlive()) continue;
                //this random spell strength is partially universal plus modifiers:
                float strength = 0.4f;

                //- target units resisting some effects
                var resistMod = ResistModFromEnch(null, u, spell);
                FInt resist = u.GetAttFinal(TAG.RESIST) + resistMod;
                strength += 0.4f * (10 - FInt.Min(10, resist).ToFloat()) / 10f;

                //- target unit not being unable to benefit from heal
                strength += u.GetTotalHpPercent() < 1f && u.race.Get() != death ? 0.1f : 0f;

                //- target unit not being able to benefit from chaos channels
                strength += u.GetAttFinal(TAG.FANTASTIC_CLASS) > 0 ? 0.1f : 0f;

                value += (int)(u.GetBattleUnitValue() * maxEffectValue * strength);
            }

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + value +
                " on battlefield.");
#endif

            return value;
        }
        static public int SBAI_TrueLight(SpellCastData data, object target, Spell spell)
        {
            int value = 0;
            var unitFlatChange = 150;

            foreach (var u in data.GetFriendlyUnits())
            {
                if (!u.IsAlive()) continue;
                if (u.dbSource.Get().race == (Race)RACE.REALM_LIFE)
                {
                    value += unitFlatChange;
                }
                if (u.dbSource.Get().race == (Race)RACE.REALM_DEATH)
                {
                    value -= unitFlatChange;
                }
            }
            foreach (var u in data.GetEnemyUnits())
            {
                if (!u.IsAlive()) continue;
                if (u.dbSource.Get().race == (Race)RACE.REALM_LIFE)
                {
                    value -= unitFlatChange;
                }
                if (u.dbSource.Get().race == (Race)RACE.REALM_DEATH)
                {
                    value += unitFlatChange;
                }
            }


#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + value +
                " on battlefield.");
#endif

            return value;
        }
        static public int SBAI_Heroism(SpellCastData data, object target, Spell spell)
        {
            if (!(target is BattleUnit))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            var unit = target as BattleUnit;
            var unitValue = unit.GetBattleUnitValue();
            int orginalLevelOverride = unit.levelOverride;
            unit.levelOverride = 4;
            unit.GetAttributes().SetDirty();
            var unitMaxLevelValue = unit.GetBattleUnitValue();
            unit.levelOverride = orginalLevelOverride;
            unit.GetAttributes().SetDirty();


            int value = 0;

            //Average spell value based on target
            value = unitMaxLevelValue - unitValue;

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + value +
                " on unit " + unit.GetDBName().ToString());
#endif

            return value;
        }
        static public int SBAI_Prayer(SpellCastData data, object target, Spell spell)
        {
            int value = 0;
            var unitPercentValue = 0.2f;

            foreach (var u in data.GetFriendlyUnits())
            {
                if (!u.IsAlive()) continue;
                value += (int)(u.GetBattleUnitValue() * unitPercentValue);
            }

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + value +
                " on battlefield.");
#endif

            return value;
        }
        static public int SBAI_MassHealing(SpellCastData data, object target, Spell spell)
        {
            int value = 0;

            foreach (var u in data.GetFriendlyUnits())
            {
                if (!u.IsAlive()) continue;
                if (u.normalDamages > 0)
                {
                    var hp = u.GetAttFinal(TAG.HIT_POINTS);
                    int maxHP = u.maxCount * hp.ToInt();

                    float dmg = u.GetTotalHpPercent();
                    int maxHeal = Mathf.Min(u.normalDamages, 5);
                    float healValue = maxHeal / (float)maxHP;

                    float uValue = u.GetBattleUnitValue();
                    // unit value prorate healed value
                    float newUValue = uValue * healValue;

                    value += (int)(newUValue);
                }
            }


#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + value +
                " on battlefield.");
#endif

            return value;
        }
        static public int SBAI_HolyWord(SpellCastData data, object target, Spell spell)
        {
            int value = 0;

            var undead = (Race)RACE.REALM_DEATH;
            //var chaos = (Race)RACE.REALM_CHAOS;

            foreach (var u in data.GetEnemyUnits())
            {
                if (!u.IsAlive()) continue;
                if (u.GetAttFinal(TAG.FANTASTIC_CLASS) == 0 || u.GetAttFinal(TAG.HERO_CLASS) > 0) continue;

                var resistMod = ResistModFromEnch(null, u, spell);
                FInt def = u.GetAttFinal(TAG.RESIST) + resistMod;
                def -= 2;

                if (u.race.Get() == undead)
                {
                    def -= 5;
                }

                def = FInt.Min(FInt.Max(0, def), 10);
                float valueScale = def.ToFloat() * 0.1f;
                value += (int)(u.GetBattleUnitValue() * (1f - valueScale));
            }


#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + value +
                " on battlefield.");
#endif

            return value;
        }
        static public int SBAI_HighPrayer(SpellCastData data, object target, Spell spell)
        {
            int value = 0;

            foreach (var u in data.GetFriendlyUnits())
            {
                if (!u.IsAlive()) continue;
                value += Mathf.Max(200, (int)(u.GetBattleUnitValue() * 0.4f));
            }

            foreach (var u in data.GetEnemyUnits())
            {
                if (!u.IsAlive()) continue;
                value += (int)(u.GetBattleUnitValue() * 0.2f);
            }

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + value +
                " on battlefield.");
#endif

            return value;
        }
        static public int SBAI_Terror(SpellCastData data, object target, Spell spell)
        {
            int value = 0;

            foreach (var u in data.GetEnemyUnits())
            {
                if (!u.IsAlive()) continue;
                var resistMod = ResistModFromEnch(null, u, spell);
                var resist = u.GetAttFinal(TAG.RESIST) + 1 + resistMod;
                float share = 1 - resist.ToFloat() / 10f;

                if (share < 0) continue;

                value += (int)(u.GetBattleUnitValue() * share * 0.5f);

            }

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + value +
                " on battlefield.");
#endif

            return value;
        }
        static public int SBAI_ManaLeak(SpellCastData data, object target, Spell spell)
        {
            int value = 0;
            var unitPercentValue = 0.5f;

            foreach (var u in data.GetEnemyUnits())
            {
                if (!u.IsAlive()) continue;
                if (u.GetAttFinal((Tag)TAG.MAGIC_RANGE) == 0) continue;
                var baseMana = u.GetAttFinal(TAG.MANA_POINTS) / 4;
                if (u.mana > baseMana)
                {
                    float mra = u.GetAttFinal((Tag)TAG.RANGE_ATTACK).ToFloat() * 0.1f;
                    if (mra > 1)
                        value += (int)(u.GetBattleUnitValue() * unitPercentValue);
                    else
                        value += (int)(u.GetBattleUnitValue() * unitPercentValue * mra);
                }
            }
            return value;
        }
        static public int SBAI_Darkness(SpellCastData data, object target, Spell spell)
        {

            int value = 0;
            var unitFlatChange = 175;

            foreach (var u in data.GetFriendlyUnits())
            {
                if (!u.IsAlive()) continue;
                if (u.GetAttFinal(TAG.FANTASTIC_CLASS) <= 0) continue;
                if (u.dbSource.Get().race == (Race)RACE.REALM_DEATH)
                {
                    value += unitFlatChange;
                }
                if (u.dbSource.Get().race == (Race)RACE.REALM_LIFE)
                {
                    value -= unitFlatChange;
                }
            }
            foreach (var u in data.GetEnemyUnits())
            {
                if (!u.IsAlive()) continue;
                if (u.GetAttFinal(TAG.FANTASTIC_CLASS) <= 0) continue;
                if (u.dbSource.Get().race == (Race)RACE.REALM_LIFE)
                {
                    value += unitFlatChange;
                }
                if (u.dbSource.Get().race == (Race)RACE.REALM_DEATH)
                {
                    value -= unitFlatChange;
                }
            }

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + value +
                " on battlefield.");
#endif

            return value;
        }
        static public int SBAI_Possession(SpellCastData data, object target, Spell spell)
        {
            if (!(target is BattleUnit)) return 0;

            var targetBu = target as BattleUnit;
            var buValue = targetBu.GetBattleUnitValue();

            FInt resRed = FInt.ZERO;
            if (spell.fIntData != null && spell.fIntData.Length > 0)
            {
                resRed = spell.fIntData[0];
            }

            BattleUnit hero = data.GetCasterAsBattleUnit();

            var resistMod = -1 * resRed + ResistModFromEnch(hero, targetBu, spell);
            var buDef = (targetBu.attributes.GetFinal(TAG.RESIST) + resistMod).ToInt();

            float resistChance = 1 - Mathf.Clamp01(buDef / 10f);

            int value = Mathf.RoundToInt(buValue * resistChance);

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + value +
                " on unit " + targetBu.GetDBName().ToString());
#endif


            return value;
        }
        static public int SBAI_BlackPrayer(SpellCastData data, object target, Spell spell)
        {
            int value = 0;
            int unitFlatChange = 200;

            var targetWizard = target as PlayerWizard;

            foreach (var u in data.GetEnemyUnits())
            {
                if (!u.IsAlive()) continue;
                value += unitFlatChange;
            }


#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + value +
                " on battlefield.");
#endif

            return value;
        }
        static public int SBAI_WallOfDarkness(SpellCastData data, object target, Spell spell)
        {
            if (data.battle == null ||
                data.battle.gDefender == null ||
                data.battle.gDefender.GetOwnerID() != data.GetWizardID() ||
                data.battle.gDefender.GetLocationHostSmart() == null) return 0;

            int value = 0;
            var location = data.battle.gDefender.GetLocationHostSmart();
            var town = location as TownLocation;
            if (town == null) return value;

            Tag lr = (Tag)TAG.LONG_RANGE;
            Tag mr = (Tag)TAG.MAGIC_RANGE;

            foreach (var u in data.GetFriendlyUnits())
            {
                if (!u.IsAlive()) continue;
                if (u.GetBaseFigure().rangedAmmo > 0)
                {
                    value += 100;
                    if (u.GetAttFinal(lr) > 0)
                    {
                        value += 100;
                    }
                    else if (u.GetAttFinal(mr) > 0)
                    {
                        value += 100;
                    }
                }
            }
            foreach (var u in data.GetEnemyUnits())
            {
                if (!u.IsAlive()) continue;
                if (u.GetBaseFigure().rangedAmmo > 0)
                {
                    value += 100;
                    if (u.GetAttFinal(lr) > 0)
                    {
                        value += 100;
                    }
                    else if (u.GetAttFinal(mr) > 0)
                    {
                        value += 100;
                    }
                }
            }

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + value +
                " on town " + town.name);
#endif

            return value;
        }
        static public int SBAI_Wrack(SpellCastData data, object target, Spell spell)
        {
            int value = 0;

            FInt resRed = FInt.ZERO;
            if (spell.fIntData != null && spell.fIntData.Length > 0)
            {
                resRed = spell.fIntData[0];
            }
            foreach (var u in data.GetEnemyUnits())
            {
                if (!u.IsAlive()) continue;

                FInt resist = -1 * resRed + ResistModFromEnch(data.GetCasterAsBattleUnit(), u, spell);
                resist += u.GetAttFinal(TAG.RESIST) + 1;
                var resistMod = Mathf.Clamp(resist.ToInt(), 0, 10);
                var hp = u.currentFigureHP + (u.figureCount - 1) * u.GetAttFinal(TAG.HIT_POINTS);
                //assume loss across 3 turns
                float hpLoss = Mathf.Clamp01(3 / hp.ToFloat());
                float hpLossResisted = hpLoss * (1 - resistMod / 10f);
                value += (int)(u.GetBattleUnitValue() * hpLossResisted);
            }
            return value;
        }
        static public int SBAI_Confusion(SpellCastData data, object target, Spell spell)
        {
            if (!(target is BattleUnit)) return 0;

            var targetBu = target as BattleUnit;
            var buValue = targetBu.GetBattleUnitValue();

            var resRed = FInt.ZERO;
            if (spell.fIntData != null && spell.fIntData.Length > 0)
            {
                resRed = spell.fIntData[0];
            }

            BattleUnit hero = data.GetCasterAsBattleUnit();

            var resistMod = -1 * resRed + ResistModFromEnch(hero, targetBu, spell);
            var buDef = (targetBu.attributes.GetFinal(TAG.RESIST) + resistMod).ToInt();

            var value = (int)(buValue * 0.7f * (1 - Mathf.Clamp01(buDef / 10f)));

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + value +
                " on unit " + targetBu.GetDBName().ToString());
#endif


            return value;
        }
        static public int SBAI_CounterMagic(SpellCastData data, object target, Spell spell)
        {
            int value = 0;
            var bookValueMulti = 200;

            var b = target as Battle;
            if (b == null) return 0;

            foreach (var v in data.GetEnemyUnits())
            {
                if (v.IsAlive() && v.canCastSpells) value += (int)(v.GetBattleUnitValue() * 0.2f);
            }

            var w = data.GetPlayerWizard();
            if (w != null)
            {
                value += w.GetAttFinal(TAG.MAGIC_BOOK).ToInt() * bookValueMulti;
            }

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + value +
                " on battlefield.");
#endif

            return value;
        }
        static public int SBAI_Invisibility(SpellCastData data, object target, Spell spell)
        {
            if (!(target is BattleUnit)) return 0;

            var targetBu = target as BattleUnit;
            if (targetBu.GetAttFinal(TAG.INVISIBILITY) > 0) return 0;

#if (UNITY_EDITOR && DEBUG_SPELLS)
                Debug.Log(spell.dbName + " with script " +
                    spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + value +
                    " on unit " + targetBu.GetDBName().ToString());
#endif

            var buValue = targetBu.GetBattleUnitValue();

            //Average spell value based on target
            var value = (int)(buValue * 0.4f);


#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + value +
                " on unit " + targetBu.GetDBName().ToString());
#endif


            return value;
        }
        static public int SBAI_CreatureBinding(SpellCastData data, object target, Spell spell)
        {
            if (!(target is BattleUnit)) return 0;

            var targetBu = target as BattleUnit;
            var buValue = targetBu.GetBattleUnitValue();
            var buDef = (targetBu.attributes.GetFinal(TAG.RESIST)).ToInt();

            BattleUnit hero = data.GetCasterAsBattleUnit();

            FInt resRed = FInt.ZERO;
            if (spell.fIntData != null && spell.fIntData.Length > 0)
            {
                resRed = spell.fIntData[0];
            }

            var resistMod = -1 * resRed + ResistModFromEnch(hero, targetBu, spell);
            buDef += resistMod.ToInt() - 2;

            int value = (int)(buValue * 2 * (1 - Mathf.Clamp01(buDef / 10f)));


#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + value +
                " on unit " + targetBu.GetDBName().ToString());
#endif


            return value;
        }
        static public int SBAI_MassInvisibility(SpellCastData data, object target, Spell spell)
        {
            int value = 0;
            var unitPercentValue = 0.4f;

            var targetWizard = target as PlayerWizard;

            foreach (var u in data.GetFriendlyUnits())
            {
                if (!u.IsAlive()) continue;
                value += (int)(u.GetBattleUnitValue() * unitPercentValue);
            }

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + value +
                " on battlefield.");
#endif
            return value;
        }
        static public int SBAI_RaiseDead(SpellCastData data, object target, Spell spell)
        {
            int value = 0;
            
            List<BattleUnit> units = data.GetFriendlyUnits();
            foreach (var u in units)
            {
                if (!u.IsAlive() && 
                    !u.dbSource.Get().unresurrectable && 
                    u.GetAttributes().DoesNotContains((Tag)TAG.FANTASTIC_CLASS))
                {
                    if(data.battle != null && !data.battle.simulation)
                    {
                        //skip if something stands on this unit place
                        bool skip = false;
                        foreach(var v in data.battle.buToSource)
                        {
                            if(v.Key.IsAlive() && v.Key.GetPosition() == u.GetPosition())
                            {
                                skip = true;
                                break;
                            }
                        }
                        if (skip) continue;
                    }
                    var value2 = u.GetBattleUnitValueFixedHP(0.5f);
                    if (value2 > value)
                    {
                        value = value2;
                    }
                }
            }

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + value +
                " on battlefield.");
#endif            
            return value;
        }
        static public int SBAI_AnimatedDead(SpellCastData data, object target, Spell spell)
        {
            int value = 0;

            //no target chosen, this might happen ie by AI not having popup to select unit
            /*conditions 
            * 1) unit is dead; 
            * 2) unit isn't Hero; 
            * 3) unit isn't from death realm; 
            * 4) unit isn't battle summon; 
            * 5) unit wasn't slain mostly by irreversible damages; 
            * 6) if enemy - unit hasn't magic immunity*/
            int totalHp;            
            foreach (var bUnit in ListUtils.MultiEnumerable(data.GetFriendlyUnits(), data.GetEnemyUnits()))
            {
                totalHp = bUnit.GetBaseFigure().maxHitPoints * bUnit.maxCount;

                if (!bUnit.IsAlive() && !(bUnit.dbSource.Get() is Hero) &&
                    bUnit.race != (Race)RACE.REALM_DEATH &&
                    !bUnit.summon &&
                    bUnit.irreversibleDamages < totalHp / 2)
                {
                    if (data.GetPlayerWizard() != bUnit.GetWizardOwner() &&
                        bUnit.GetAttributes().Contains(TAG.MAGIC_IMMUNITY))
                    {
                        continue;
                    }
                    if (!data.battle.simulation)
                    {
                        //skip if something stands on this unit place
                        bool skip = false;
                        foreach (var s in data.battle.buToSource)
                        {
                            if (s.Key.IsAlive() && s.Key.GetPosition() == bUnit.GetPosition())
                            {
                                skip = true;
                                break;
                            }
                        }
                        if (skip) continue;
                    }

                    var value2 = bUnit.GetBattleUnitValueFixedHP(1f);
                    if (value2 > value)
                    {
                        value = value2;
                    }
                }
            }

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + value +
                " on battlefield.");
#endif

            return value;
        }
        static public int SBAI_WordOfRecal(SpellCastData data, object target, Spell spell)
        {
            //if enemy is 5x stronger, spell is worth 50% of the un-summoned unit
            //if enemy is 3x stronger, spell is worth 25% of the un-summoned unit
            //is enemy is 1.5x stronger spell is worth 0
            if (data == null || data.battle == null || !(target is BattleUnit)) return 0;

            int ownValue = data.GetFriendlyValue();
            int enemyValue = data.GetEnemyValue();

            if (ownValue >= enemyValue || enemyValue <= 0) return 0;
            float scale = ownValue / (float)enemyValue;
            var bu = target as BattleUnit;

            if (bu.GetAttributes().Contains(TAG.SHIP) &&
                bu.GetAttributes().DoesNotContains((Tag)TAG.CAN_FLY)) return 0;

            var value = bu.GetBattleUnitValue();

            if (scale < 3f)
            {
                return (int)(value * Mathf.Lerp(0f, 0.25f, (scale - 1f) / (3f - 1f)));
            }
            else if (scale < 5f)
            {
                return (int)(value * Mathf.Lerp(0.25f, 0.5f, (scale - 3f) / (5f - 3f)));
            }
            else
            {
                return (int)(value * 0.5f);
            }

        }
        static public int SBAI_MagicVortex(SpellCastData data, object target, Spell spell)
        {
            //ToDo: AI do not know how to use it
            //ToDo: Check with coder.
            Debug.Log("Spell " + spell.dbName + " is not check for AI use yet.");
            return 0;

            float value = 0.0f;

            var battle = data.battle;
            if (battle == null) return (int)value;

            var pos = (Vector3i)target;
            if (!battle.IsLocationEmpty(pos))
            {
                Debug.LogError("Target location occupied");
                return 0;
            }

            var around = HexNeighbors.GetRange(pos, 1, 1);
            foreach (var v in around)
            {
                var bu = battle.GetUnitAt(v);
                if (bu != null)
                {
                    if (bu.ownerID != data.GetWizardID())
                    {
                        value += bu.GetBattleUnitValue() * 0.3f;
                    }
                }
            }

            foreach (var u in data.GetFriendlyUnits())
            {
                if (!u.IsAlive()) continue;
                value -= u.GetBattleUnitValue() * 0.1f;
            }

            foreach (var u in data.GetEnemyUnits())
            {
                if (!u.IsAlive()) continue;
                value += u.GetBattleUnitValue() * 0.2f;
            }


#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + (int)value);
#endif

            return (int)value;
        }
        static public int SBAI_DisenchantTrue(SpellCastData data, object target, Spell spell)
        {
            //ToDo: AI do not know how to use it
            //ToDo: Check with coder.
            Debug.Log("Spell " + spell.dbName + " is not check for AI use yet.");
            return 0;

            if (!(target is Battle))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            if (data.caster == null)
            {
                Debug.LogError("Spell " + spell.dbName + " source is invalid");
                return 0;
            }

            int value = 0;
            var unitEnchValue = 0.1f;
            var locEnchValue = 200;

            var battle = target as Battle;
            List<EnchantmentInstance> enchList;
            List<BattleUnit> enemyUnits;
            List<BattleUnit> friendlyUnits;
            BattlePlayer enemyWizard;
            BattlePlayer friendlyWizard;
            if (data.caster.GetWizardOwner().GetID() == battle.attacker.GetID())
            {
                enemyUnits = battle.defenderUnits;
                friendlyUnits = battle.attackerUnits;
                enemyWizard = battle.defender;
                friendlyWizard = battle.attacker;
            }
            else
            {
                enemyUnits = battle.attackerUnits;
                friendlyUnits = battle.defenderUnits;
                enemyWizard = battle.attacker;
                friendlyWizard = battle.defender;
            }

            foreach (var u in enemyUnits)
            {
                enchList = u.GetEnchantments();

                for (int i = enchList.Count - 1; i >= 0; i--)
                {
                    if (enchList.Count <= i) continue;

                    //Dispel only ench that allow to dispel.
                    if (enchList[i].source.Get().allowDispel == false) continue;

                    //Disenchant only positive ench on enemy unit.
                    if (enchList[i].source.Get().enchCategory == EEnchantmentCategory.Positive)
                        value += (int)(u.GetFakePower() * unitEnchValue);
                }
            }
            foreach (var u in friendlyUnits)
            {
                enchList = u.GetEnchantments();

                for (int i = enchList.Count - 1; i >= 0; i--)
                {
                    if (enchList.Count <= i) continue;

                    //Dispel only ench that allow to dispel.
                    if (enchList[i].source.Get().allowDispel == false) continue;

                    //Disenchant only negative ench on own unit
                    if (enchList[i].source.Get().enchCategory == EEnchantmentCategory.Negative)
                        value += (int)(u.GetFakePower() * unitEnchValue);
                }
            }

            //remove enchantment from location
            enchList = battle.GetEnchantments();

            for (int i = enchList.Count - 1; i >= 0; i--)
            {
                if (enchList.Count <= i) continue;

                //Dispel only ench that allow to dispel.
                if (enchList[i].source.Get().allowDispel == false) continue;

                //Disenchant only negative ench on own battle if own by enemy. Disenchant only positive ench on battle if own by caster.
                if (enchList[i].owner.GetEntity() == data.caster.GetWizardOwner() && enchList[i].source.Get().enchCategory == EEnchantmentCategory.Negative ||
                    enchList[i].owner.GetEntity() != data.caster.GetWizardOwner() && enchList[i].source.Get().enchCategory == EEnchantmentCategory.Positive)
                    value += locEnchValue;
            }
            //remove enchantment from enemy wizard
            enchList = enemyWizard.GetEnchantments();

            for (int i = enchList.Count - 1; i >= 0; i--)
            {
                if (enchList.Count <= i) continue;

                //Dispel only ench that allow to dispel.
                if (enchList[i].source.Get().allowDispel == false) continue;

                //Disenchant only negative ench on own battle if own by enemy. Disenchant only positive ench on battle if own by caster.
                if (enchList[i].owner.GetEntity() == data.caster.GetWizardOwner() && enchList[i].source.Get().enchCategory == EEnchantmentCategory.Negative ||
                    enchList[i].owner.GetEntity() != data.caster.GetWizardOwner() && enchList[i].source.Get().enchCategory == EEnchantmentCategory.Positive)
                    value += locEnchValue;
            }

            //remove enchantment from friendly wizard
            enchList = friendlyWizard.GetEnchantments();

            for (int i = enchList.Count - 1; i >= 0; i--)
            {
                if (enchList.Count <= i) continue;

                //Dispel only ench that allow to dispel.
                if (enchList[i].source.Get().allowDispel == false) continue;

                //Disenchant only negative ench on own battle if own by enemy. Disenchant only positive ench on battle if own by caster.
                if (enchList[i].owner.GetEntity() == data.caster.GetWizardOwner() && enchList[i].source.Get().enchCategory == EEnchantmentCategory.Negative ||
                    enchList[i].owner.GetEntity() != data.caster.GetWizardOwner() && enchList[i].source.Get().enchCategory == EEnchantmentCategory.Positive)
                    value += locEnchValue;
            }

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + (int)value);
#endif

            return (int)value;
        }
        static public int SBAI_MassShield(SpellCastData data, object target, Spell spell)
        {

            int value = 0;
            var unitFlatChange = 100;

            foreach (var u in data.GetFriendlyUnits())
            {
                if (!u.IsAlive()) continue;
                if (u.GetAttFinal(TAG.LARGE_SHIELD) <= 0) continue;
                    value += unitFlatChange;
            }
            foreach (var u in data.GetEnemyUnits())
            {
                if (!u.IsAlive()) continue;
                if (u.GetAttFinal(TAG.RANGE_ATTACK) <= 0) continue;
                if (u.GetAttFinal(TAG.RANGE_ATTACK) > 0)
                {
                    value += unitFlatChange * u.GetAttFinal(TAG.RANGE_ATTACK).ToInt() / 5;
                }
            }

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + value +
                " on battlefield.");
#endif

            return value;
        }
        static public int SBAI_Reconstruct(SpellCastData data, object target, Spell spell)
        {
            int value = 0;

            foreach (var u in data.GetFriendlyUnits())
            {
                if (!u.IsAlive() && (u.race == RACE.REALM_TECH || u.race == RACE.NON_RACIAL))
                {
                    var val = BaseUnit.GetUnitStrength(u.dbSource.Get());
                    if (value < val)
                    {
                        value = val/2;
                    }
                }
            }

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + value +
                " on battlefield.");
#endif

            return value;
        }
        static public int SBAI_BomberBoom(SpellCastData data, object target, Spell spell)
        {
            if (!(target is BattleUnit)) return 0;
            var pos = (target as BattleUnit).GetPosition();

            var distance = 1;
            float value = 0f;

            var battle = data.battle;
            foreach (var unit in battle.GetAllUnits())
            {
                if (HexCoordinates.HexDistance(unit.GetPosition(), pos) <= distance)
                {
                    if (unit.ownerID == data.GetWizardID())
                        value -= unit.GetBattleUnitValue();
                    else
                        value += unit.GetBattleUnitValue();
                }
            }
#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " SBAI_ script value " + (int)value);
#endif

            return (int)value;
        }
        static public int SBAI_AddAmmo(SpellCastData data, object target, Spell spell)
        {
            if (!(target is BattleUnit)) return 0;
            var bu = target as BattleUnit;

            var oldValue = bu.GetBattleUnitValue();
            var actualAmmo = bu.GetBaseFigure().rangedAmmo;
            bu.GetBaseFigure().rangedAmmo += 2;
            bu.GetAttributes().SetDirty();
            var newValue = bu.GetBattleUnitValue();
            bu.GetBaseFigure().rangedAmmo = actualAmmo;
            bu.GetAttributes().SetDirty();

            float value = oldValue - newValue;

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + (int)value +
                " on unit " + bu.GetDBName().ToString());
#endif

            return (int)value;
        }
        static public int SBAI_MassPiercing(SpellCastData data, object target, Spell spell)
        {

            int value = 0;
            var unitValue = 100;

            foreach (var u in data.GetFriendlyUnits())
            {
                if (!u.IsAlive()) continue;
                if (u.GetSkills().Find(o => o == (Skill)SKILL.ARMOR_PIERCING 
                    || o == (Skill)SKILL.MAGIC_SORCERY_PIERCING_RANGE_ATTACK
                    || o == (Skill)SKILL.ITEM_LIGHTNING) == null) continue;
                value += unitValue /2;
            }
            foreach (var u in data.GetEnemyUnits())
            {
                if (!u.IsAlive()) continue;
                value += unitValue * u.GetAttFinal(TAG.DEFENCE).ToInt() / 10;
            }

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + value +
                " on battlefield.");
#endif

            return value;
        }
        static public int SBAI_CastingBlock(SpellCastData data, object target, Spell spell)
        {

            int value = 0;
            var unitValue = 100;
            var caster = data.caster as BattleUnit;

            if (target is BattlePlayer && caster != null 
                && caster.GetTotalHealth() > caster.GetAttFinal(TAG.HIT_POINTS) /2 )
            {
                var bp = target as BattlePlayer;
                var books = bp.GetWizardOwner().GetAttFinal(TAG.MAGIC_BOOK).ToInt();
                var skill = bp.castingSkill;
                var mana = bp.mana;

                if (books < 2 || skill < 2 || mana < 2) return 0;

                value = books * unitValue;
                value += skill * unitValue / 10;
                value += mana * unitValue / 100;

            }


#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + value +
                " on battlefield.");
#endif

            return value;
        }
        static public int SBAI_Healing(SpellCastData data, object target, Spell spell)
        {
            int value = 0;
            var t = target as BattleUnit;

            if (t == null && !t.IsAlive()) return value;
            if (t.normalDamages > 0)
            {
                var hp = t.GetAttFinal(TAG.HIT_POINTS);
                int maxHP = t.maxCount * hp.ToInt();

                float dmg = t.GetTotalHpPercent();
                int maxHeal = Mathf.Min(t.normalDamages, 5);
                float healValue = maxHeal / (float)maxHP;

                float uValue = t.GetBattleUnitValue();
                // unit value prorate healed value
                float newUValue = uValue * healValue;

                value += (int)(newUValue);
            }


#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + value +
                " on battlefield.");
#endif

            return value;
        }
        #endregion

        #region Spell World casting value estimator for AI
        static public int SWAI_IrrelevantForAI(ISpellCaster source, object target, Spell spell)
        {
#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + 0);
#endif
            return 0;
        }
        static public int SWAI_Placeholder(ISpellCaster source, object target, Spell spell)
        {
#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + 500);
#endif
            //placeholder estimation is not smart enough to provide sensible guidance
            return 0;
        }
        public static int SWAI_Summon(ISpellCaster source, object target, Spell spell)
        {
            if (spell.stringData == null || spell.stringData.Length < 1)
            {
                Debug.LogError("Spell " + spell.dbName + " miss summon info to work with");
                return 0;
            }
            var unit = DataBase.Get(spell.stringData[0], true);

            int value = 0;
            if (unit is DBDef.Hero)
            {
                value = BaseUnit.GetUnitStrength(unit as DBDef.Hero);
            }
            else if (unit is DBDef.Unit)
            {
                value = BaseUnit.GetUnitStrength(unit as DBDef.Unit);
            }
            else
            {
                Debug.LogError("Unit " + spell.stringData[0] + " not found in database");
                return 0;
            }

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + (int)value);
#endif
            return value;
        }
        public static int SWAI_SummonHero(ISpellCaster source, object target, Spell spell)
        {
            var caster = source as PlayerWizard;
            var value = 0;

            if (caster.heroes != null)
            {
                value = 4000 / (caster.heroes.Count + 1);
            }

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + (int)value);
#endif
            return value;
        }
        public static int SWAI_SummonChampion(ISpellCaster source, object target, Spell spell)
        {
            var caster = source as PlayerWizard;
            var value = 0;

            if (caster.heroes != null)
            {
                value = 8000 / (caster.heroes.Count + 1);
            }

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + (int)value);
#endif
            return value;
        }

        static public int SWAI_ResistElementals(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is MOM.Unit))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            var unit = target as MOM.Unit;

            //How much value that spell can give based on enemy units and enemy wizard spell books and target unit.
            float multi = 0;
            float value = 0f;

            var caster = source as PlayerWizard;
            var enemyWizards = GameManager.GetWizards();
            foreach (var w in enemyWizards)
            {
                TAG[] spellBooks = new TAG[] { TAG.CHAOS_MAGIC_BOOK, TAG.NATURE_MAGIC_BOOK };

                if (w.ID == caster?.ID) continue;
                if (w.GetAttributes().ContainsAny(spellBooks))
                {
                    multi += 0.1f;
                }
            }

            //Average spell value based on target
            var spellValue = unit.GetModifiedWorldUnitValue(TAG.DEFENCE, (FInt)3.0) +
                             unit.GetModifiedWorldUnitValue(TAG.RESIST, (FInt)3.0) -
                             unit.GetWorldUnitValue() * 2;

            value = spellValue * (0.3f + multi);


#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + (int)value +
                " on unit " + unit.GetDBName().ToString());
#endif
            return (int)value;
        }
        static public int SWAI_WallOfStone(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is TownLocation))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            var town = target as TownLocation;
            var townValue = town.GetStrategicValue();
            int value = 0;

            //Average spell value based on target
            value = (int)(townValue * 0.3f);

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on town " + town.name);
#endif

            return value;
        }
        static public int SWAI_GiantStrength(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is MOM.Unit))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            var unit = target as MOM.Unit;
            var buValue = unit.GetWorldUnitValue();
            int value = 0;

            //Average spell value based on target
            value = unit.GetModifiedWorldUnitValue(TAG.MELEE_ATTACK, (FInt)1.0) +
                         unit.GetModifiedWorldUnitValue(TAG.THROW_BONUS, (FInt)1.0) -
                         (buValue * 2);

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on unit " + unit.GetDBName().ToString());
#endif

            return value;
        }
        static public int SWAI_StoneSkin(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is MOM.Unit))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            var unit = target as MOM.Unit;
            var buValue = unit.GetWorldUnitValue();
            int value = 0;

            //Average spell value based on target
            value = unit.GetModifiedWorldUnitValue(TAG.DEFENCE, (FInt)1.0) -
                         buValue;

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on unit " + unit.GetDBName().ToString());
#endif

            return value;
        }
        static public int SWAI_WaterWalk(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is MOM.Unit))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            var unit = target as MOM.Unit;
            var buValue = unit.GetWorldUnitValue();
            int value = 0;

            if (unit.group == null || unit.group.Get() == null ||
                unit.group.Get().GetDesignation() == null ||
                unit.GetAttFinal(TAG.CAN_FLY) > 0 || unit.GetAttFinal(TAG.CAN_SWIM) > 0)
            {
                return value = 0;
            }
            var destination = unit.group.Get().GetDesignation();

            //If unit need water transport
            if (destination != null &&
                destination.inNeedOfWaterTransport &&
                !unit.CanTravelOverWater())
            {                
                value += 500;
            }

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on unit " + unit.GetDBName().ToString());
#endif

            return value;
        }
        static public int SWAI_PathFinding(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is MOM.Unit))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            var unit = target as MOM.Unit;
            var buValue = unit.GetWorldUnitValue();
            int value = 0;

            if (unit.group == null || unit.group.Get() == null ||
                unit.group.Get().GetDesignation() == null)
            {
                return value = 0;
            }

            var destination = unit.group.Get().GetDesignation();

            var unitMaxMp = unit.GetMaxMP();

            //If unit have destination set
            if (destination != null && unitMaxMp < 4)
            {
                //Average spell value based on target
                value = buValue / 2;
            }
            else if (destination != null)
            {
                //Average spell value based on target
                value = buValue / 3;
            }

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on unit " + unit.GetDBName().ToString());
#endif

            return value;
        }
        static public int SWAI_NatureCurs(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is MOM.Group))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            var group = target as MOM.Group;
            int value = 0;


            foreach (var u in group.GetUnits())
            {
                if (u.Get().race == RACE.REALM_DEATH) continue;

                //      Full health unit strength                   - actual unit Strength
                value += BaseUnit.GetUnitStrength(u.Get().dbSource) - u.Get().GetWorldUnitValue();
            }

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on group with " + group.GetUnits().Count + " units");
#endif

            return value;
        }
        static public int SWAI_ElementalArmor(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is MOM.Unit))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            var unit = target as MOM.Unit;

            //How much value that spell can give based on enemy units and enemy wizard spell books and target unit.
            float multi = 0;
            float value = 0f;

            var caster = source as PlayerWizard;
            var enemyWizards = GameManager.GetWizards();
            foreach (var w in enemyWizards)
            {
                TAG[] spellBooks = new TAG[] { TAG.CHAOS_MAGIC_BOOK, TAG.NATURE_MAGIC_BOOK };

                if (w.ID == caster?.ID) continue;
                if (w.GetAttributes().ContainsAny(spellBooks))
                {
                    multi += 0.1f;
                }
            }

            //Average spell value based on target
            var spellValue = unit.GetModifiedWorldUnitValue(TAG.DEFENCE, (FInt)10.0) +
                             unit.GetModifiedWorldUnitValue(TAG.RESIST, (FInt)10.0) -
                             unit.GetWorldUnitValue() * 2;

            value = spellValue * (0.1f + multi);


#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + (int)value +
                " on unit " + unit.GetDBName().ToString());
#endif
            return (int)value;
        }
        static public int SWAI_IronSkin(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is MOM.Unit))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            var unit = target as MOM.Unit;
            var buValue = unit.GetWorldUnitValue();
            int value = 0;

            //Average spell value based on target
            value = unit.GetModifiedWorldUnitValue(TAG.DEFENCE, (FInt)5.0) -
                         buValue;

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on unit " + unit.GetDBName().ToString());
#endif

            return value;
        }
        static public int SWAI_Regeneration(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is MOM.Unit))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            var unit = target as MOM.Unit;
            var buValue = unit.GetWorldUnitValue();
            int value = 0;

            if (unit.attributes.Contains(TAG.REGENERATION))
            {
#if (UNITY_EDITOR && DEBUG_SPELLS)
                Debug.Log(spell.dbName + " with script " +
                    spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                    " on unit " + unit.GetDBName().ToString());
#endif
                return value;
            }

            //Average spell value based on target
            value = (int)(buValue * 0.7f);

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on unit " + unit.GetDBName().ToString());
#endif

            return value;
        }
        static public int SWAI_IceStorm(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is MOM.Group))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            var group = target as MOM.Group;
            int value = 0;

            foreach (var u in group.GetUnits())
            {
                value += u.Get().GetWorldUnitValue() / 4;
            }

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on group with " + group.GetUnits().Count + " units");
#endif
            //less valuable if spellcaster isn't at war with target
            var targetOwner = group.GetOwnerID();
            var spellcasterOwner = source.GetWizardOwner();
            if (spellcasterOwner != null)
            {
                if (targetOwner == 0 || !spellcasterOwner.GetDiplomacy().IsAtWarWith(targetOwner))
                {
                    value = Mathf.RoundToInt(value * 0.6f);
                }
            }
            return value;
        }
        static public int SWAI_Earthquake(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is TownLocation))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            var town = target as TownLocation;
            var townValue = town.GetStrategicValue();
            int value = 0;
            var units = town.GetLocalGroup().GetUnits().FindAll(
                o => o.Get().GetAttFinal(TAG.CAN_FLY) == 0 && o.Get().GetAttFinal(TAG.NON_CORPOREAL) == 0);


            //Average spell value based on target
            value = (int)(townValue * Mathf.Max(1, units.Count) * 0.4f);

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on town " + town.name);
#endif

            return value;
        }
        static public int SWAI_GaiasBlessing(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is TownLocation))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            var town = target as TownLocation;
            var townValue = town.GetStrategicValue();
            int value = 0;

            //Average spell value based on target
            value = (int)(townValue * 3.5f);

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on town " + town.name);
#endif

            return value;
        }

        static public int SWAI_HerbMastery(ISpellCaster source, object target, Spell spell)
        {
            if (!(source is PlayerWizardAI))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            int value = 0;
            var valueMultiplier = 500;

            var wizard = source as PlayerWizardAI;
            var groups = wizard.arcanusVisibility.ownGroups;

            foreach (var u in groups)
            {
                var unitsInGroup = u.GetUnits().Count;
                value += unitsInGroup * valueMultiplier;
            }

            groups = wizard.myrrorVisibility.ownGroups;
            foreach (var u in groups)
            {
                var unitsInGroup = u.GetUnits().Count;
                value += unitsInGroup * valueMultiplier;
            }


#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on global target.");
#endif

            return value;
        }

        static public int SWAI_EldritchWeapon(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is MOM.Unit))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            var unit = target as MOM.Unit;
            var buValue = unit.GetWorldUnitValue();
            int value = 0;

            //Average spell value based on target
            value = unit.GetModifiedWorldUnitValue(TAG.MELEE_ATTACK_CHANCE, (FInt)0.1) +
                    unit.GetModifiedWorldUnitValue(TAG.RANGE_ATTACK_CHANCE, (FInt)0.1) -
                         buValue * 2;

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on unit " + unit.GetDBName().ToString());
#endif

            return value;
        }

        static public int SWAI_ChaosChannels(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is MOM.Unit))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            var unit = target as MOM.Unit;
            var buValue = unit.GetWorldUnitValue();
            int value = 0;

            //Average spell value based on target
            value = unit.GetModifiedWorldUnitValue(TAG.MELEE_ATTACK, (FInt)2.0) -
                         buValue;

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on unit " + unit.GetDBName().ToString());
#endif

            return value;
        }

        static public int SWAI_FlameBlade(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is MOM.Unit))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            var unit = target as MOM.Unit;
            var buValue = unit.GetWorldUnitValue();
            int value = 0;

            //Average spell value based on target
            value = unit.GetModifiedWorldUnitValue(TAG.MELEE_ATTACK, (FInt)2.0) +
                    unit.GetModifiedWorldUnitValue(TAG.RANGE_ATTACK, (FInt)2.0) +
                    unit.GetModifiedWorldUnitValue(TAG.THROW_BONUS, (FInt)2.0) -
                         buValue * 3;

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on unit " + unit.GetDBName().ToString());
#endif

            return value;
        }

        static public int SWAI_Immolation(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is MOM.Unit))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            var unit = target as MOM.Unit;
            var buValue = unit.GetWorldUnitValue();
            int value = 0;

            //Average spell value based on target
            value = (int)(buValue * 0.8f);

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on unit " + unit.GetDBName().ToString());
#endif

            return value;
        }

        static public int SWAI_FireStorm(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is MOM.Group))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            var group = target as MOM.Group;
            int value = 0;

            foreach (var u in group.GetUnits())
            {
                value += (int)(u.Get().GetWorldUnitValue() / 3);
            }

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on group with " + group.GetUnits().Count + " units");
#endif

            return value;
        }

        static public int SWAI_ChaosRift(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is TownLocation))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }
            var valueMultiplier = 250;

            var town = target as TownLocation;
            var townValue = town.GetStrategicValue();
            int value = 0;
            var units = town.GetLocalGroup();

            //Average spell value based on target
            value = (int)(townValue * 0.4f + units.GetUnits().Count * valueMultiplier);

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on town " + town.name);
#endif

            return value;
        }

        static public int SWAI_MeteorStorm(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is GameManager))
            {
                if (!(target is PlayerWizard))
                {
                    Debug.LogWarning("Spell " + spell.dbName + " target is invalid");
                }
                return 0;
            }
            if (!(source is PlayerWizard))
            {
                return 0;
            }

            var wizards = GameManager.Get().wizards;
            if (wizards == null || wizards.Count < 2)
            {
                Debug.Log(spell.aiWorldEvaluationScript + " is designed to target wizards units and towns.");
                return 0;
            }

            var townChangeValue = 250;
            var unitChangeValue = 125;
            var owner = source as PlayerWizard;
            var townsNum = GameManager.Get().registeredLocations.FindAll(
                o => o.GetOwnerID() != owner.ID && o is TownLocation && o.GetOwnerID() > 0).Count;
            var unitsNum = GameManager.Get().registeredGroups.FindAll(
                o => o.GetOwnerID() != owner.ID && o.GetOwnerID() > 0 && (o.GetLocationHost()?.otherPlaneLocation?.Get() == null || o.plane.arcanusType)).Count;

            int value = 0;

            //Average spell value based on target
            value += unitsNum * unitChangeValue;
            value += townsNum * townChangeValue;

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on towns ");
#endif

            return value;
        }

        static public int SWAI_ChaosSurge(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is GameManager))
            {
                if (!(target is PlayerWizard))
                {
                    Debug.LogWarning("Spell " + spell.dbName + " target is invalid");
                }
                return 0;
            }
            if (!(source is PlayerWizardAI))
            {
                return 0;
            }

            int value = 0;
            var valueMultiplier = 250;

            if (GameManager.Get().GetEnchantments().Find(o => o.source == ENCH.CHAOS_SURGE_GLOBAL) != null)
            {
                return value;
            }

            var wizard = source as PlayerWizardAI;
            var groups = wizard.arcanusVisibility.ownGroups;
            foreach (var u in groups)
            {
                var unitsInGroup = u.GetUnits().Count;
                value += unitsInGroup * valueMultiplier;
            }

            groups = wizard.myrrorVisibility.ownGroups;
            foreach (var u in groups)
            {
                var unitsInGroup = u.GetUnits().Count;
                value += unitsInGroup * valueMultiplier;
            }

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on towns ");
#endif

            return value;
        }

        static public int SWAI_DoomMastery(ISpellCaster source, object target, Spell spell)
        {
            if (!(source is PlayerWizardAI))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }
            int value = 0;
            var valueUnitMultiplier = 250;
            var valueTown = 1000;

            var wizard = source as PlayerWizardAI;

            var groups = wizard.arcanusVisibility.ownGroups;
            groups.AddRange(wizard.myrrorVisibility.ownGroups);

            foreach (var u in groups)
            {
                var unitsInGroup = u.GetUnits().Count;
                value += unitsInGroup * valueUnitMultiplier;
            }

            var locations = wizard.arcanusVisibility.knownLocations;
            locations.AddRange(wizard.myrrorVisibility.knownLocations);

            foreach (var l in locations)
            {
                if (!(l is TownLocation)) continue;
                value += valueTown;
            }

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on towns ");
#endif

            return value;
        }

        static public int SWAI_CallTheVoid(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is TownLocation))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }
            var valueUnitMultiplier = 250;

            var town = target as TownLocation;
            var townValue = town.GetStrategicValue();
            int value = 0;
            var units = town.GetLocalGroup();

            //Average spell value based on target
            value = (int)(townValue * 3 + units.GetUnits().Count * valueUnitMultiplier);

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on town " + town.name);
#endif

            return value;
        }

        static public int SWAI_Bless(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is MOM.Unit))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            var unit = target as MOM.Unit;

            //How much value that spell can give based on enemy units and enemy wizard spell books and target unit.
            float multi = 0;
            float value = 0f;

            var caster = source as PlayerWizard;
            var enemyWizards = GameManager.GetWizards();
            foreach (var w in enemyWizards)
            {
                TAG[] spellBooks = new TAG[] { TAG.CHAOS_MAGIC_BOOK, TAG.DEATH_MAGIC_BOOK };

                if (w.ID == caster?.ID) continue;
                if (w.GetAttributes().ContainsAny(spellBooks))
                {
                    multi += 0.1f;
                }
            }

            //Average spell value based on target
            var spellValue = unit.GetModifiedWorldUnitValue(TAG.DEFENCE, (FInt)3.0) +
                             unit.GetModifiedWorldUnitValue(TAG.RESIST, (FInt)3.0) -
                             unit.GetWorldUnitValue() * 2;

            value = spellValue * (0.3f + multi);


#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + (int)value +
                " on unit " + unit.GetDBName().ToString());
#endif
            return (int)value;
        }

        static public int SWAI_Endurance(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is MOM.Unit))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            var unit = target as MOM.Unit;
            var buValue = unit.GetWorldUnitValue();
            int value = 0;

            //Average spell value based on target
            value = unit.GetWorldUnitValue() / 3;

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on unit " + unit.GetDBName().ToString());
#endif

            return value;
        }

        static public int SWAI_HolyWeapon(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is MOM.Unit))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            var unit = target as MOM.Unit;
            var buValue = unit.GetWorldUnitValue();
            int value = 0;

            //Average spell value based on target
            value = unit.GetModifiedWorldUnitValue(TAG.MELEE_ATTACK_CHANCE, (FInt)0.1) +
                    unit.GetModifiedWorldUnitValue(TAG.RANGE_ATTACK_CHANCE, (FInt)0.1) -
                         buValue * 2;

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on unit " + unit.GetDBName().ToString());
#endif

            return value;
        }

        static public int SWAI_HolyArmor(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is MOM.Unit))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            var unit = target as MOM.Unit;
            var buValue = unit.GetWorldUnitValue();
            int value = 0;

            //Average spell value based on target
            value = unit.GetModifiedWorldUnitValue(TAG.DEFENCE, (FInt)2.0) -
                         buValue;

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on unit " + unit.GetDBName().ToString());
#endif

            return value;
        }

        static public int SWAI_TrueSight(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is MOM.Unit))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            var unit = target as MOM.Unit;

            //How much value that spell can give based on enemy units and enemy wizard spell books and target unit.
            float multi = 0;
            float value = 0f;

            var caster = source as PlayerWizard;
            var enemyWizards = GameManager.GetWizards();
            foreach (var w in enemyWizards)
            {
                TAG[] spellBooks = new TAG[] { TAG.SORCERY_MAGIC_BOOK };

                if (w.ID == caster?.ID) continue;
                if (w.GetAttributes().ContainsAny(spellBooks))
                {
                    multi += 0.2f;
                }
            }

            //Average spell value based on target
            var spellValue = unit.GetModifiedWorldUnitValue(TAG.DEFENCE, (FInt)3.0)
                             - unit.GetWorldUnitValue();

            value = spellValue * (0.2f + multi);


#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + (int)value +
                " on unit " + unit.GetDBName().ToString());
#endif
            return (int)value;
        }

        static public int SWAI_HeavenlyLight(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is TownLocation))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            var town = target as TownLocation;
            int value = 0;
            var units = town.GetLocalGroup();

            //Average spell value based on target
            value = town.GetStrategicValue();

            if (source is PlayerWizard)
            {
                var lifeBooks = (source as PlayerWizard).GetAttFinal(TAG.LIFE_MAGIC_BOOK) * 0.25f;
                if (lifeBooks > 0)
                {
                    value = value * lifeBooks.ToInt();
                }
            }

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on town " + town.name);
#endif

            return value;
        }

        static public int SWAI_Lionheart(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is MOM.Unit))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            var unit = target as MOM.Unit;
            var buValue = unit.GetWorldUnitValue();
            int value = 0;

            //Average spell value based on target
            value = unit.GetModifiedWorldUnitValue(TAG.MELEE_ATTACK, (FInt)3.0) +
                unit.GetModifiedWorldUnitValue(TAG.RESIST, (FInt)3.0) +
                unit.GetModifiedWorldUnitValue(TAG.HIT_POINTS, (FInt)3.0) +
                unit.GetModifiedWorldUnitValue(TAG.RANGE_ATTACK, (FInt)3.0) +
                unit.GetModifiedWorldUnitValue(TAG.THROW_BONUS, (FInt)3.0) + -
                         buValue * 5;

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on unit " + unit.GetDBName().ToString());
#endif

            return value;
        }

        static public int SWAI_Invulnerability(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is MOM.Unit))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            var unit = target as MOM.Unit;
            var buValue = unit.GetWorldUnitValue();
            int value = 0;

            //Average spell value based on target
            value = unit.GetModifiedWorldUnitValue(TAG.DEFENCE, (FInt)6.0) -
                         buValue;

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on unit " + unit.GetDBName().ToString());
#endif

            return value;
        }

        static public int SWAI_Righteousness(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is MOM.Unit))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            var unit = target as MOM.Unit;

            //How much value that spell can give based on enemy units and enemy wizard spell books and target unit.
            float multi = 0;
            float value = 0f;

            var caster = source as PlayerWizard;
            var enemyWizards = GameManager.GetWizards();
            foreach (var w in enemyWizards)
            {
                TAG[] spellBooks = new TAG[] { TAG.CHAOS_MAGIC_BOOK, TAG.DEATH_MAGIC_BOOK };

                if (w.ID == caster?.ID) continue;
                if (w.GetAttributes().ContainsAny(spellBooks))
                {
                    multi += 0.1f;
                }
            }

            //Average spell value based on target
            var spellValue = unit.GetModifiedWorldUnitValue(TAG.DEFENCE, (FInt)20.0) +
                             unit.GetModifiedWorldUnitValue(TAG.RESIST, (FInt)20.0) -
                             unit.GetWorldUnitValue() * 2;

            value = spellValue * (0.1f + multi);

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + (int)value +
                " on unit " + unit.GetDBName().ToString());
#endif
            return (int)value;
        }

        static public int SWAI_Prosperity(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is TownLocation))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            var town = target as TownLocation;
            var townValue = town.GetStrategicValue();
            int value = 0;

            //Average spell value based on target
            value = (int)(townValue * 2.25f);

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on town " + town.name);
#endif

            return value;
        }

        static public int SWAI_AltarOfBattle(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is TownLocation))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            var town = target as TownLocation;
            var townValue = town.GetStrategicValue();
            int value = 0;

            //Average spell value based on target
            value = (int)(townValue * 2.0f);

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on town " + town.name);
#endif

            return value;
        }

        static public int SWAI_StreamOfLife(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is TownLocation))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            var town = target as TownLocation;
            var townValue = town.GetStrategicValue();
            int value = 0;

            //Average spell value based on target
            value = (int)(townValue * 2.5f);

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on town " + town.name);
#endif

            return value;
        }

        static public int SWAI_Inspirations(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is TownLocation))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            var town = target as TownLocation;
            var townValue = town.GetStrategicValue();
            int value = 0;

            //Average spell value based on target
            value = (int)(townValue * 3.5f);

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on town " + town.name);
#endif

            return value;
        }

        static public int SWAI_HolyArms(ISpellCaster source, object target, Spell spell)
        {
            if (!(source is PlayerWizardAI))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            int value = 0;
            var valueMultiplier = 350;

            var wizard = source as PlayerWizardAI;
            var groupsArcanus = wizard.arcanusVisibility.ownGroups;
            var groupsMyrror = wizard.myrrorVisibility.ownGroups;

            foreach (var u in groupsArcanus)
            {
                value += u.GetUnits().Count * valueMultiplier;
            }
            foreach (var u in groupsMyrror)
            {
                value += u.GetUnits().Count * valueMultiplier;
            }

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on towns ");
#endif

            return value;
        }

        static public int SWAI_Consecration(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is TownLocation))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            var town = target as TownLocation;
            var townValue = town.GetStrategicValue();
            float valueMultiplier = 0;
            int value = 0;

            var caster = source as PlayerWizard;
            var enemyWizards = GameManager.GetWizards();

            foreach (var w in enemyWizards)
            {
                TAG[] spellBooks = new TAG[] { TAG.DEATH_MAGIC_BOOK, TAG.CHAOS_MAGIC_BOOK };

                if (w.ID == caster?.ID) continue;
                if (w.GetAttributes().ContainsAny(spellBooks))
                {
                    valueMultiplier += 0.2f;
                }
            }

            //Average spell value based on target
            value = (int)(townValue * valueMultiplier);

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on town " + town.name);
#endif

            return value;
        }

        static public int SWAI_Crusade(ISpellCaster source, object target, Spell spell)
        {
            if (!(source is PlayerWizardAI))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            int value = 0;
            var valueMultiplier = 350;

            var wizard = source as PlayerWizardAI;
            var groupsArcanus = wizard.arcanusVisibility.ownGroups;
            var groupsMyrror = wizard.myrrorVisibility.ownGroups;

            foreach (var u in groupsArcanus)
            {
                value += u.GetUnits().Count * valueMultiplier;
            }
            foreach (var u in groupsMyrror)
            {
                value += u.GetUnits().Count * valueMultiplier;
            }

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on towns ");
#endif

            return value;
        }

        static public int SWAI_CharmOfLife(ISpellCaster source, object target, Spell spell)
        {
            if (!(source is PlayerWizardAI))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }
            int value = 0;
            var valueMultiplier = 450;

            var wizard = source as PlayerWizardAI;
            var groupsArcanus = wizard.arcanusVisibility.ownGroups;
            var groupsMyrror = wizard.myrrorVisibility.ownGroups;

            foreach (var u in groupsArcanus)
            {
                value += u.GetUnits().Count * valueMultiplier;
            }
            foreach (var u in groupsMyrror)
            {
                value += u.GetUnits().Count * valueMultiplier;
            }

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on towns.");
#endif

            return value;
        }

        static public int SWAI_DarkRituals(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is TownLocation))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            var town = target as TownLocation;
            var townValue = town.GetStrategicValue();
            int value = 0;

            foreach (var b in town.buildings)
            {
                if (b == (Building)BUILDING.SHRINE ||
                    b == (Building)BUILDING.PARTHENON ||
                    b == (Building)BUILDING.TEMPLE ||
                    b == (Building)BUILDING.CATHEDRAL)
                {
                    value += (int)(townValue * 0.1f);
                }
            }

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on town " + town.name);
#endif

            return value;
        }

        static public int SWAI_CloakOfFear(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is MOM.Unit))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            var unit = target as MOM.Unit;
            var buValue = unit.GetWorldUnitValue();
            int value = 0;

            //Average spell value based on target
            if (buValue > 1500)
                value = (int)(buValue * 0.8f);
            else if (buValue > 3000)
                value = (int)(buValue * 0.6f);
            else
                value = (int)(buValue * 0.9f);

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on unit " + unit.GetDBName().ToString());
#endif

            return value;
        }

        static public int SWAI_Lycanthropy(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is MOM.Unit))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            var unit = target as MOM.Unit;
            var buValue = unit.GetWorldUnitValue();
            var werValue = BaseUnit.GetUnitStrength((Subrace)UNIT.DTH_WEREWOLVES);
            int value = 0;

            //Average spell value based on target
            value = (int)(werValue - buValue);

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on unit " + unit.GetDBName().ToString());
#endif

            return value;
        }

        static public int SWAI_BlackChannels(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is MOM.Unit))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            var unit = target as MOM.Unit;
            var buValue = unit.GetWorldUnitValue();
            int value = 0;

            //Average spell value based on target
            value = unit.GetModifiedWorldUnitValue(TAG.MELEE_ATTACK, (FInt)2.0) +
                    unit.GetModifiedWorldUnitValue(TAG.RANGE_ATTACK, (FInt)1.0) +
                    unit.GetModifiedWorldUnitValue(TAG.DEFENCE, (FInt)1.0) +
                    unit.GetModifiedWorldUnitValue(TAG.RESIST, (FInt)1.0) +
                    unit.GetModifiedWorldUnitValue(TAG.HIT_POINTS, (FInt)1.0) +
                    -buValue * 5;

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on unit " + unit.GetDBName().ToString());
#endif

            return value;
        }
        static public int SWAI_WallOfDarkness(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is TownLocation))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            var town = target as TownLocation;
            var townValue = town.GetStrategicValue();
            int value = 0;

            //Average spell value based on target
            value = (int)(townValue * 1.0f);

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on town " + town.name);
#endif

            return value;
        }

        static public int SWAI_WraithForm(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is MOM.Unit))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            var unit = target as MOM.Unit;
            var unitValue = unit.GetWorldUnitValue();
            int value = 0;

            if (unit.attributes.Contains(TAG.WEAPON_IMMUNITY))
            {
#if (UNITY_EDITOR && DEBUG_SPELLS)
                Debug.Log(spell.dbName + " with script " +
                    spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                    " on unit " + unit.GetDBName().ToString());
#endif
                return value;
            }

            var unitDef = (unit.attributes.GetFinal(TAG.DEFENCE)).ToInt();

            switch (unitDef)
            {
                case 1:
                    value = (unitValue * (FInt)0.7).ToInt();
                    break;
                case 2:
                    value = (unitValue * (FInt)0.6).ToInt();
                    break;
                case 3:
                    value = (unitValue * (FInt)0.5).ToInt();
                    break;
                case 4:
                    value = (unitValue * (FInt)0.4).ToInt();
                    break;
                case 5:
                    value = (unitValue * (FInt)0.3).ToInt();
                    break;
                case 6:
                    value = (unitValue * (FInt)0.3).ToInt();
                    break;
                case 7:
                    value = (unitValue * (FInt)0.2).ToInt();
                    break;
                case 8:
                    value = (unitValue * (FInt)0.2).ToInt();
                    break;
                case 9:
                    value = (unitValue * (FInt)0.1).ToInt();
                    break;

                default:
                    value = 0;
                    break;
            }

            if (unit.attributes.Contains(TAG.NON_CORPOREAL)) value = value / 2;
           
            var destination = unit.group.Get().GetDesignation();

            //If unit need water transport
            if (destination != null &&
                destination.inNeedOfWaterTransport &&
                !unit.CanTravelOverWater())
            {
                value += 750;
            }

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on unit " + unit.GetDBName().ToString());
#endif

            return value;
        }

        static public int SWAI_EvilPresence(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is TownLocation))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            var town = target as TownLocation;
            var townValue = town.GetStrategicValue();
            int value = 0;

            foreach (var b in town.buildings)
            {
                if (b == (Building)BUILDING.SHRINE ||
                    b == (Building)BUILDING.PARTHENON ||
                    b == (Building)BUILDING.TEMPLE ||
                    b == (Building)BUILDING.CATHEDRAL)
                {
                    value += (int)(townValue * 0.25f);
                }
            }

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on town " + town.name);
#endif

            return value;
        }

        static public int SWAI_BlackWind(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is MOM.Group))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            var group = target as MOM.Group;
            int value = 0;

            int resistReduction = 0;
            if (spell.fIntData != null && spell.fIntData.Length > 0)
            {
                resistReduction = spell.fIntData[0].ToInt();
            }
            foreach (var u in group.GetUnits())
            {
                var unitValue = u.Get().GetWorldUnitValue();

                var unitResist = (u.Get().attributes.GetFinal(TAG.RESIST)).ToInt() - resistReduction;

                switch (unitResist)
                {
                    case 1:
                        value = (unitValue * (FInt)0.8).ToInt();
                        break;
                    case 2:
                        value = (unitValue * (FInt)0.7).ToInt();
                        break;
                    case 3:
                        value = (unitValue * (FInt)0.6).ToInt();
                        break;
                    case 4:
                        value = (unitValue * (FInt)0.5).ToInt();
                        break;
                    case 5:
                        value = (unitValue * (FInt)0.4).ToInt();
                        break;
                    case 6:
                        value = (unitValue * (FInt)0.4).ToInt();
                        break;
                    case 7:
                        value = (unitValue * (FInt)0.3).ToInt();
                        break;
                    case 8:
                        value = (unitValue * (FInt)0.2).ToInt();
                        break;
                    case 9:
                        value = (unitValue * (FInt)0.1).ToInt();
                        break;

                    default:
                        value = 0;
                        break;
                }
            }


#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on group with position " + group.Position);
#endif

            return value;
        }

        static public int SWAI_Famine(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is TownLocation))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            var town = target as TownLocation;
            var townValue = town.GetStrategicValue();
            int value = 0;

            //Average spell value based on target
            value = (int)(townValue * 2.0f);

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on town " + town.name);
#endif

            return value;
        }

        static public int SWAI_CruelUnminding(ISpellCaster source, object target, Spell spell)
        {
            if (!(source is PlayerWizard))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            var wizard = source as PlayerWizard;
            int value = wizard.GetTotalCastingSkill() * 50;

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on wizard. ");
#endif

            return value;
        }

        static public int SWAI_Pestilence(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is TownLocation))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            var town = target as TownLocation;
            var townValue = town.GetStrategicValue();
            int value = 0;

            //Average spell value based on target
            value = (int)(townValue * 2.5f);

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on town " + town.name);
#endif

            return value;
        }

        static public int SWAI_DeathWish(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is GameManager))
            {
                if (!(target is PlayerWizard))
                {
                    Debug.LogWarning("Spell " + spell.dbName + " target is invalid");
                }
                return 0;
            }
            if (!(source is PlayerWizard))
            {
                return 0;
            }

            int unitChangeValue = 250;
            var owner = source as PlayerWizard;
            int value = 0;
            var wizards = (target as GameManager).wizards;
            if (wizards == null || wizards.Count < 2)
            {
                Debug.Log("SWAI_DeathWish is designed to target ale wizards units.");
                return 0;
            }

            var registerGroups = GameManager.Get().registeredGroups;

            int unitsNum = 0;

            foreach (var w in wizards)
            {
                var wizardGroups = registerGroups.FindAll(o => o.GetOwnerID() == w.ID &&
                o.GetOwnerID() != owner.GetID());

                foreach (var g in wizardGroups)
                {
                    if (g.GetLocationHost()?.otherPlaneLocation?.Get() != null && g.plane.arcanusType) continue;
                    var fantasyUnits = g.GetUnits().FindAll(o => o.Get().GetAttFinal(TAG.FANTASTIC_CLASS) == 0);

                    unitsNum += fantasyUnits.Count;
                }
            }

            //Average spell value based on target
            value = unitsNum * unitChangeValue;

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on all units in game.");
#endif

            return value;
        }

        static public int SWAI_ResistMagic(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is MOM.Unit))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            var unit = target as MOM.Unit;
            var buValue = unit.GetWorldUnitValue();
            int value = 0;

            if (unit.GetAttributes().Contains(TAG.MAGIC_IMMUNITY))
            {
                return value;
            }

            //Average spell value based on target
            value = (unit.GetModifiedWorldUnitValue(TAG.RESIST, (FInt)5.0) -
                         buValue) / 2;

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on unit " + unit.GetDBName().ToString());
#endif

            return value;
        }

        static public int SWAI_GuardianWind(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is MOM.Unit))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            var unit = target as MOM.Unit;
            int value = 0;

            if (unit.attributes.Contains(TAG.MAGIC_IMMUNITY))
            {
#if (UNITY_EDITOR && DEBUG_SPELLS)
                Debug.Log(spell.dbName + " with script " +
                    spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                    " on unit " + unit.GetDBName().ToString());
#endif
                return value;
            }

            //Average spell value based on target
            value = unit.GetWorldUnitValue() / 2;

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on unit " + unit.GetDBName().ToString());
#endif

            return value;
        }

        static public int SWAI_SpellLock(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is MOM.Unit))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            var unit = target as MOM.Unit;
            var unitValue = unit.GetWorldUnitValue();
            int value = 0;

            var evaluationValue = 0;

            if (unit.attributes.Contains(TAG.MAGIC_IMMUNITY))
            {
#if (UNITY_EDITOR && DEBUG_SPELLS)
                Debug.Log(spell.dbName + " with script " +
                    spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                    " on unit " + unit.GetDBName().ToString());
#endif
                return value;
            }

            foreach (var e in unit.GetEnchantments())
            {
                var ench = e.source;
                if (ench.Get().enchCategory == EEnchantmentCategory.Positive &&
                    unit.GetWizardOwner().ID == source.GetWizardOwner()?.ID)
                {
                    evaluationValue++;
                }

                if (evaluationValue == 0)
                {
#if (UNITY_EDITOR && DEBUG_SPELLS)
                    Debug.Log(spell.dbName + " with script " +
                        spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                        " on unit " + unit.GetDBName().ToString());
#endif
                    return value;
                }

                switch (evaluationValue)
                {
                    case 1:
                        value = (unitValue * (FInt)0.5).ToInt();
                        break;
                    case 2:
                        value = (unitValue * (FInt)0.6).ToInt();
                        break;
                    case 3:
                        value = (unitValue * (FInt)0.7).ToInt();
                        break;
                    case 4:
                        value = (unitValue * (FInt)0.8).ToInt();
                        break;

                    default:
                        value = (unitValue * (FInt)0.8).ToInt();
                        break;
                }

            }


#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on unit " + unit.GetDBName().ToString());
#endif

            return value;
        }

        static public int SWAI_Flight(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is MOM.Unit))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }


            var unit = target as MOM.Unit;
            int value = 0;

            if (unit.attributes.Contains(TAG.CAN_FLY))
            {
#if (UNITY_EDITOR && DEBUG_SPELLS)
                Debug.Log(spell.dbName + " with script " +
                    spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                    " on unit " + unit.GetDBName().ToString());
#endif
                return value;
            }

            //Average spell value based on target
            value = (int)(unit.GetWorldUnitValue() * 0.25f);

            var destination = unit.group.Get().GetDesignation();

            //If unit need water transport
            if (destination != null &&
                destination.inNeedOfWaterTransport &&
                !unit.CanTravelOverWater())
            {
                value += 750;
            }

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on unit " + unit.GetDBName().ToString());
#endif

            return value;
        }

        static public int SWAI_WindMastery(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is GameManager))
            {
                if (!(target is PlayerWizard))
                {
                    Debug.LogWarning("Spell " + spell.dbName + " target is invalid");
                }

                return 0;
            }


            int ownUnitChangeValue = 75;
            int enemyUnitChangeValue = 50;
            var wizardID = source.GetWizardOwnerID();
            var registeredGroups = (target as GameManager).registeredGroups;

            int value = 0;

            foreach (var gr in registeredGroups)
            {
                if (gr.GetLocationHost()?.otherPlaneLocation?.Get() != null && gr.plane.arcanusType) continue;
                if (gr.GetUnits().Find(o => o.Get().attributes.GetFinal(TAG.SHIP) > 0) != null)
                {
                    if (gr.GetUnits().Find(o => o.Get().GetWizardOwner()?.GetID() == wizardID) != null)
                    {
                        value += ownUnitChangeValue;
                    }
                    else
                    {
                        value += enemyUnitChangeValue;
                    }
                }
            }

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on own ships.");
#endif

            return value;
        }

        static public int SWAI_WindWalking(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is MOM.Unit))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            var unit = target as MOM.Unit;
            int value = 0;

            var group = unit.group.Get();

            if (group != null && group.aiDesignation != null &&
                group.aiDesignation.inNeedOfWaterTransport)
            {
                if (group.GetUnits().Count < 3)
                    value = 500 + (int)(unit.GetWorldUnitValue());
                else if (group.GetUnits().Count < 6)
                    value = 1000 + (int)(unit.GetWorldUnitValue());
                else
                    value = 1500 + (int)(unit.GetWorldUnitValue());
            }


#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on unit " + unit.GetDBName().ToString());
#endif

            return value;
        }

        static public int SWAI_Stasis(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is MOM.Group))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            var group = target as MOM.Group;
            int value = 0;

            foreach (var u in group.GetUnits())
            {
                value += (int)(u.Get().GetWorldUnitValue() * 0.5f);
            }

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on group with " + group.GetUnits().Count + " units");
#endif

            return value;
        }

        static public int SWAI_MagicImmunity(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is MOM.Unit))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            var unit = target as MOM.Unit;
            int value = 0;

            if (unit.attributes.Contains(TAG.MAGIC_IMMUNITY))
            {
#if (UNITY_EDITOR && DEBUG_SPELLS)
                Debug.Log(spell.dbName + " with script " +
                    spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                    " on unit " + unit.GetDBName().ToString());
#endif
                return value;
            }

            //Average spell value based on target
            value = (int)(unit.GetWorldUnitValue() * 0.8f);

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on unit " + unit.GetDBName().ToString());
#endif

            return value;
        }

        static public int SWAI_SpellWard(ISpellCaster source, object target, Spell spell)
        {
            //ToDo: AI do not know how to use it            
            //ToDo: Check with coder
            Debug.Log("Spell " + spell.dbName + " is not check for AI use yet.");
            return 0;

            if (!(target is TownLocation))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            var town = target as TownLocation;
            var townValue = town.GetStrategicValue();
            int value = 0;

            //Average spell value based on target
            value = (int)(townValue * 2.5f);

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on town " + town.name);
#endif
            //TODO: Temporary 0 is set so AI do not cast it.
            return value = 0;
        }

        static public int SWAI_GreatUnsummoning(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is GameManager))
            {
                if (!(target is PlayerWizard))
                {
                    Debug.LogWarning("Spell " + spell.dbName + " target is invalid");
                }
                return 0;
            }
            if (!(source is PlayerWizard))
            {
                return 0;
            }

            int unitChangeValue = 350;
            int value = 0;
            var wizards = (target as GameManager).wizards;
            if (wizards == null || wizards.Count < 2)
            {
                Debug.Log("SWAI_GreatUnsummoning is designed to target ale wizards units.");
                return 0;
            }

            var registerGroups = GameManager.Get().registeredGroups;

            int unitsNum = 0;

            foreach (var w in wizards)
            {
                if (w.GetID() == (source as PlayerWizard).GetID()) continue;
                var wizardGroups = registerGroups.FindAll(o => o.GetOwnerID() == w.ID);

                foreach (var g in wizardGroups)
                {
                    if (g.GetLocationHost()?.otherPlaneLocation?.Get() != null && g.plane.arcanusType) continue;
                    var fantasyUnits = g.GetUnits().FindAll(o => o.Get().GetAttFinal(TAG.FANTASTIC_CLASS) > 0);
                    unitsNum += fantasyUnits.Count;
                }
            }

            //Average spell value based on target
            value = unitsNum * unitChangeValue;

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on all units in game.");
#endif

            return value;
        }

        static public int SWAI_SummoningCircle(ISpellCaster source, object target, Spell spell)
        {

            if (!(target is TownLocation))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            var town = target as TownLocation;
            var wiz = town.GetWizardOwner();
            if (wiz == null || town.IsAnOutpost()) return 0;

            //this spell is used in verity of custom strategies and should not be considered
            //for cast on their own unless wizard have no circle at all at that time            
            if (wiz.summoningCircle != null)
            {
                return 0;
            }

            var townValue = town.GetStrategicValue();
            int value = 0;

            //Average spell value based on target
            value = (int)(townValue * 0.2f * town.locationTactic.needToBuildArmy) +
                    (int)(townValue * 0.1f);



#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on town " + town.name);
#endif
            return value;
        }


        static public int SWAI_FlyingFortress(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is TownLocation))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            var town = target as TownLocation;
            var townValue = town.GetStrategicValue();
            var defenders = town.GetUnits();
            int value = 0;

            value = (int)(townValue * 0.1f);

            foreach (var u in defenders)
            {
                if (u.Get().GetAttFinal(TAG.CAN_FLY) == 0)
                {
                    value += (int)(value * 0.1f);
                }
            }

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on town " + town.name);
#endif
            return value;
        }

        static public int SWAI_CreateEnchantItem(ISpellCaster source, object target, Spell spell)
        {
            //ToDo: AI do not know how to use it
            //ToDo: check with coder
            Debug.Log("Spell " + spell.dbName + " is not check for AI use yet.");
            return 0;

            if (!(source is PlayerWizardAI))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            int value = 0;
            int unitChangeValue = 500;

            var wizard = source as PlayerWizardAI;
            List<Reference<MOM.Unit>> wizardHeros = new List<Reference<MOM.Unit>>();
            var groups = wizard.arcanusVisibility.ownGroups;
            groups.AddRange(wizard.myrrorVisibility.ownGroups);

            foreach (var u in groups)
            {
                var heroesInGroup = u.GetUnits().FindAll(o => o.Get().attributes.Contains(TAG.HERO_CLASS)).Count;
                value += heroesInGroup * unitChangeValue;
            }

            value = value / Math.Max(wizard.artefacts.Count, 1);


#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on all units in game.");
#endif

            return value;
        }

        static public int SWAI_CreateArtefactItem(ISpellCaster source, object target, Spell spell)
        {
            //ToDo: AI do not know how to use it
            //ToDo: check with coder
            Debug.Log("Spell " + spell.dbName + " is not check for AI use yet.");
            return 0;

            if (!(source is PlayerWizardAI))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            int value = 0;
            int unitChangeValue = 1000;

            var wizard = source as PlayerWizardAI;
            var groups = wizard.arcanusVisibility.ownGroups;
            groups.AddRange(wizard.myrrorVisibility.ownGroups);

            foreach (var u in groups)
            {
                var heroesInGroup = u.GetUnits().FindAll(o => o.Get().attributes.Contains(TAG.HERO_CLASS)).Count;
                value += heroesInGroup * unitChangeValue;
            }

            value = value / Math.Max(wizard.artefacts.Count, 1);


#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on all units in game.");
#endif

            return value;
        }
        static public int SWAI_SpellOfReturn(ISpellCaster source, object target, Spell spell)
        {
            if (!(source is PlayerWizardAI))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            if ((source as PlayerWizardAI).banishedTurn > 0) return 10000;
            return 0;
        }
        static public int SWAI_ChangeTerrain(ISpellCaster source, object target, Spell spell)
        {
            //ToDo: Check with coder
            //ToDo: AI do not know how to use it
            Debug.Log("Spell " + spell.dbName + " is not check for AI use yet.");
            return 0;

            if (!(target is Hex))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }
            if (!(source is PlayerWizardAI))
            {
                Debug.LogError("Spell " + spell.dbName + " caster is invalid");
                return 0;
            }

            var hex = target as Hex;
            var caster = source as PlayerWizardAI;

            if (hex.GetTerrain().terrainType == ETerrainType.Swamp)
            {
                var locations = GameManager.Get().registeredLocations;

                foreach (var l in locations)
                {
                    if (!(l is TownLocation) || caster.GetID() != (l as TownLocation).GetID()) continue;

                    if (WorldCode.Plane.Get().GetDistanceWrapping(hex.Position, l.GetPosition()) < 3)
                    {
                        var townValue = (l as TownLocation).GetStrategicValue();
                        return (int)(townValue * 0.7f);
                    }
                }
            }

            return 0;
        }
        static public int SWAI_Transmute(ISpellCaster source, object target, Spell spell)
        {
            //ToDo: Check with coder
            //ToDo: AI do not know how to use it
            Debug.Log("Spell " + spell.dbName + " is not check for AI use yet.");
            return 0;

            if (!(target is Hex))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }
            if (!(source is PlayerWizardAI))
            {
                Debug.LogError("Spell " + spell.dbName + " caster is invalid");
                return 0;
            }

            var hex = target as Hex;
            var caster = source as PlayerWizardAI;

            if (hex.GetTerrain().terrainType == ETerrainType.Swamp)
            {
                var locations = GameManager.Get().registeredLocations;

                foreach (var l in locations)
                {
                    if (!(l is TownLocation) || caster.GetID() != (l as TownLocation).GetID()) continue;

                    if (WorldCode.Plane.Get().GetDistanceWrapping(hex.Position, l.GetPosition()) < 3)
                    {
                        var townValue = (l as TownLocation).GetStrategicValue();
                        return (int)(townValue * 1.0f);
                    }
                }
            }

            return 0;
        }

        static public int SWAI_MoveFortress(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is TownLocation))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            var town = target as TownLocation;
            var townValue = town.GetStrategicValue();
            int value = 0;

            if (town.locationTactic.needToBuildArmy > 1)
            {
                //Average spell value based on target
                value = (int)(townValue * 0.5f);
            }

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on town " + town.name);
#endif
            return value;
        }

        static public int SWAI_NaturesWrath(ISpellCaster source, object target, Spell spell)
        {
            //this spell targets game manager, not specific player
            if (!(target is GameManager))
            {
                if (!(target is PlayerWizard))
                {
                    Debug.LogWarning("Spell " + spell.dbName + " target is invalid");
                }
                return 0;
            }
            if (!(source is PlayerWizard))
            {
                return 0;
            }

            var wizards = GameManager.Get().wizards;
            var spellOwner = source as PlayerWizard;
            if (wizards == null || wizards.Count < 2)
            {
                Debug.Log(spell.aiWorldEvaluationScript + " is designed to target wizards units and towns.");
                return 0;
            }

            int unitChangeValue = 250;
            int townChangeValue = 500;
            int value = 0;

            var registerlocations = GameManager.Get().registeredLocations;

            int unitsNum = 0;
            int townsNum = 0;

            var wizardTowns = registerlocations.FindAll(o => o.GetOwnerID() != spellOwner.ID &&
            o.GetWizardOwner() != null &&
            (o.GetWizardOwner().GetAttFinal(TAG.DEATH_MAGIC_BOOK) > 0 ||
            o.GetWizardOwner().GetAttFinal(TAG.CHAOS_MAGIC_BOOK) > 0));

            foreach (var l in wizardTowns)
            {
                if (!(l is TownLocation)) continue;
                townsNum++;
                unitsNum += l.GetUnits().Count;
            }

            //Average spell value based on target
            value += unitsNum * unitChangeValue;
            value += townsNum * townChangeValue;

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on all units in game.");
#endif

            return value;
        }

        static public int SWAI_Corruption(ISpellCaster source, object target, Spell spell)
        {
            //ToDo: Check with coder
            //ToDo: AI do not know how to use it
            Debug.Log("Spell " + spell.dbName + " is not check for AI use yet.");
            return 0;

            if (!(target is Hex))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }
            if (!(source is PlayerWizardAI))
            {
                Debug.LogError("Spell " + spell.dbName + " caster is invalid");
                return 0;
            }

            var hex = target as Hex;
            var caster = source as PlayerWizardAI;

            var locations = GameManager.Get().registeredLocations;

            foreach (var l in locations)
            {
                if (!(l is TownLocation) || caster.GetID() == (l as TownLocation).GetID()) continue;

                if (WorldCode.Plane.Get().GetDistanceWrapping(hex.Position, l.GetPosition()) < 3)
                {
                    var townValue = (l as TownLocation).GetStrategicValue();
                    return (int)(townValue * 0.5f);
                }
            }

            return 0;
        }
        static public int SWAI_RaiseVolcano(ISpellCaster source, object target, Spell spell)
        {
            //ToDo: Check with coder
            //ToDo: AI do not know how to use it
            Debug.Log("Spell " + spell.dbName + " is not check for AI use yet.");
            return 0;

            if (!(target is Hex))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }
            if (!(source is PlayerWizardAI))
            {
                Debug.LogError("Spell " + spell.dbName + " caster is invalid");
                return 0;
            }

            var hex = target as Hex;
            var caster = source as PlayerWizardAI;

            var locations = GameManager.Get().registeredLocations;

            foreach (var l in locations)
            {
                if (!(l is TownLocation) || caster.GetID() == (l as TownLocation).GetID()) continue;

                if (WorldCode.Plane.Get().GetDistanceWrapping(hex.Position, l.GetPosition()) < 3)
                {
                    var townValue = (l as TownLocation).GetStrategicValue();
                    return (int)(townValue * 0.9f);
                }
            }

            return 0;
        }
        static public int SWAI_GreatWasting(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is GameManager))
            {
                if (!(target is PlayerWizard))
                {
                    Debug.LogWarning("Spell " + spell.dbName + " target is invalid");
                }
                return 0;
            }
            if (!(source is PlayerWizard))
            {
                return 0;
            }

            var wizards = GameManager.Get().wizards;
            if (wizards == null || (wizards != null && wizards.Count < 2))
            {
                Debug.Log(spell.aiWorldEvaluationScript + " is designed to target wizards units and towns.");
                return 0;
            }

            int townChangeValue = 500;
            int value = 0;

            var registerlocations = GameManager.Get().registeredLocations;
            int townsNum = 0;

            foreach (var w in wizards)
            {
                var wizardTowns = registerlocations.FindAll(o => o.GetOwnerID() == w.ID);
                foreach (var l in wizardTowns)
                {
                    if (!(l is TownLocation)) continue;
                    townsNum++;
                }

            }

            //Average spell value based on target
            townChangeValue = townsNum * townChangeValue;

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on all units in game.");
#endif

            return townChangeValue;
        }
        static public int SWAI_Armageddon(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is GameManager))
            {
                if (!(target is PlayerWizard))
                {
                    Debug.LogWarning("Spell " + spell.dbName + " target is invalid");
                }
                return 0;
            }
            if (!(source is PlayerWizard))
            {
                return 0;
            }

            int townChangeValue = 1000;
            int value = 0;
            var wizards = (target as GameManager).wizards;
            var owner = source as PlayerWizard;
            if (wizards == null || wizards.Count < 2)
            {
                Debug.Log(spell.aiWorldEvaluationScript + " is designed to target ale wizards units and towns.");
                return 0;
            }

            var registerlocations = GameManager.Get().registeredLocations;
            int townsNum = 0;

            foreach (var w in wizards)
            {
                var wizardTowns = registerlocations.FindAll(o => o.GetOwnerID() == w.ID &&
                o.GetOwnerID() != owner.GetID());
                foreach (var l in wizardTowns)
                {
                    if (!(l is TownLocation)) continue;
                    townsNum++;
                }

            }

            //Average spell value based on target
            townChangeValue = townsNum * townChangeValue;

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on all units in game.");
#endif

            return townChangeValue;
        }
        static public int SWAI_JustCause(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is PlayerWizard))
            {
                Debug.LogError("Spell " + spell.dbName + " caster is invalid");
                return 0;
            }

            var caster = target as PlayerWizardAI;
            var locations = GameManager.Get().registeredLocations;
            var value = 0;
            var famePoint = 20;

            foreach (var l in locations)
            {
                if (!(l is TownLocation) || caster.GetID() == (l as TownLocation).GetID()) continue;

                var town = l as TownLocation;
                if (town.GetUnrest() > 0.1f)
                {
                    var townValue = town.GetStrategicValue();
                    value = value + (int)(townValue * 0.5f);
                }
            }

            return value + famePoint * 10;
        }
        static public int SWAI_Heroism(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is MOM.Unit))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            var unit = target as MOM.Unit;
            var unitValue = unit.GetWorldUnitValue();
            int orginalLevelOverride = unit.levelOverride;
            unit.levelOverride = 4;
            unit.GetAttributes().SetDirty();
            var unitMaxLevelValue = unit.GetWorldUnitValue();
            unit.levelOverride = orginalLevelOverride;
            unit.GetAttributes().SetDirty();


            int value = 0;

            //Average spell value based on target
            value = unitMaxLevelValue - unitValue;

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on unit " + unit.GetDBName().ToString());
#endif

            return value;
        }
        static public int SWAI_PlanarTravel(ISpellCaster source, object target, Spell spell)
        {
            //ToDo: AI do not know how to use it
            //ToDo: Check with coder, that script is placeholder
            Debug.Log("Spell " + spell.dbName + " is not check for AI use yet.");
            return 0;

            if (!(target is MOM.Unit))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            var unit = target as MOM.Unit;
            var buValue = unit.GetWorldUnitValue();
            int value = 0;

            if (unit.group == null || unit.group.Get() == null ||
                unit.group.Get().GetDesignation() == null ||
                unit.GetAttFinal(TAG.CAN_FLY) > 0 || unit.GetAttFinal(TAG.CAN_SWIM) > 0)
            {
                return value = 0;
            }
            var destination = unit.group.Get().GetDesignation();

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on unit " + unit.GetDBName().ToString());
#endif

            return value;
        }
        static public int SWAI_Resurrection(ISpellCaster source, object target, Spell spell)
        {
            //ToDo: AI do not know how to use it
            Debug.Log("Spell " + spell.dbName + " is not check for AI use yet.");
            return 0;

            if (!(target is GameManager))
            {
                if (!(target is PlayerWizard))
                {
                    Debug.LogWarning("Spell " + spell.dbName + " target is invalid");
                }
                return 0;
            }

            int value = 0;
            var caster = source as PlayerWizardAI;

            //Average spell value based on target
            foreach (var h in caster.deadHeroes)
            {
                var unit = DataBase.GetType<DBDef.Hero>().Find(o => o.dbName == h.name);
                if (unit == null) continue;
                value = unit.recruitmentCost * h.xp / 50;
                break;
            }

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on all units in game.");
#endif

            return value;
        }
        static public int SWAI_PlanarSeal(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is GameManager))
            {
                if (!(target is PlayerWizard))
                {
                    Debug.LogWarning("Spell " + spell.dbName + " target is invalid");
                }
                return 0;
            }
            if (!(source is PlayerWizard))
            {
                return 0;
            }

            var caster = source as PlayerWizard;
            var groups = GameManager.Get().registeredGroups;
            var value = 0;

            int[] wizardsArmyStrArcanus = new int[6];
            int[] wizardsArmyStrMyrror = new int[6];

            //if some other wizard is casting spell of master, there is no gain from blocking the plan
            var som = (Spell)SPELL.SPELL_OF_MASTERY;
            foreach (var v in GameManager.GetWizards())
            {
                if (v == caster) continue;
                var magicAndResearch = (v as PlayerWizard).GetMagicAndResearch();
                if (magicAndResearch.curentlyCastSpell == som)
                {
                    return 0;
                }
            }

            foreach (var g in groups)
            {
                if (g.GetOwnerID() == 0) continue;
                //if (g.GetLocationHost()?.otherPlaneLocation?.Get() != null && g.plane.arcanusType) continue;

                if (g.GetPlane().arcanusType)
                {
                    foreach (var u in g.GetUnits())
                    {
                        wizardsArmyStrArcanus[g.GetOwnerID()] += u.Get().GetWorldUnitValue();
                    }
                }
                else
                {
                    foreach (var u in g.GetUnits())
                    {
                        wizardsArmyStrMyrror[g.GetOwnerID()] += u.Get().GetWorldUnitValue();
                    }
                }
            }

            if (caster.wizardTower.Get().plane.arcanusType)
            {
                for (int i = 0; i < wizardsArmyStrMyrror.Length; i++)
                {
                    if (caster.GetID() == i) continue;
                    value += wizardsArmyStrMyrror[i];
                }
            }
            else
            {
                for (int i = 0; i < wizardsArmyStrArcanus.Length; i++)
                {
                    if (caster.GetID() == i) continue;
                    value += wizardsArmyStrArcanus[i];
                }
            }

            return value / 3;
        }
        static public int SWAI_PlaneShift(ISpellCaster source, object target, Spell spell)
        {
            //ToDo: AI do not know how to use it
            //ToDo: Check with coder, that script is placeholder
            Debug.Log("Spell " + spell.dbName + " is not check for AI use yet.");
            return 0;

            if (!(target is MOM.Unit))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            var unit = target as MOM.Unit;
            var buValue = unit.GetWorldUnitValue();
            int value = 0;

            if (unit.group == null || unit.group.Get() == null ||
                unit.group.Get().GetDesignation() == null ||
                unit.GetAttFinal(TAG.CAN_FLY) > 0 || unit.GetAttFinal(TAG.CAN_SWIM) > 0)
            {
                return value = 0;
            }
            var destination = unit.group.Get().GetDesignation();

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on unit " + unit.GetDBName().ToString());
#endif

            return value;
        }
        static public int SWAI_LifeForce(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is GameManager))
            {
                if (!(target is PlayerWizard))
                {
                    Debug.LogWarning("Spell " + spell.dbName + " target is invalid");
                }
                return 0;
            }
            if (!(source is PlayerWizard))
            {
                return 0;
            }

            int bookValue = 500;
            int value = 0;

            var caster = source as PlayerWizard;
            var wizards = (target as GameManager).wizards;
            foreach (var w in wizards)
            {
                if (w.ID == caster?.ID) continue;
                for (int i = 0; i < w.GetAttFinal(TAG.DEATH_MAGIC_BOOK); i++)
                {
                    value += bookValue;
                }
            }



#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on from foe wizards.");
#endif
            return value;
        }
        static public int SWAI_Tranqulity(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is GameManager))
            {
                if (!(target is PlayerWizard))
                {
                    Debug.LogWarning("Spell " + spell.dbName + " target is invalid");
                }
                return 0;
            }
            if (!(source is PlayerWizard))
            {
                return 0;
            }

            int bookValue = 650;
            int value = 0;

            var caster = source as PlayerWizard;
            var wizards = (target as GameManager).wizards;
            foreach (var w in wizards)
            {
                if (w.ID == caster?.ID) continue;
                for (int i = 0; i < w.GetAttFinal(TAG.CHAOS_MAGIC_BOOK); i++)
                {
                    value += bookValue;
                }
            }

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on from foe wizards.");
#endif
            return value;
        }
        static public int SWAI_DrainPower(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is PlayerWizard))
            {
                Debug.LogError("Spell " + spell.dbName + " caster is invalid");
                return 0;
            }

            var t = target as PlayerWizard;
            var value = 0;

            if (t.mana > 150)
            {
                value = t.mana * 2;
            }
            else if (t.mana > 50)
            {
                value = (int)(t.mana * 1.5f);
            }

            return Mathf.Clamp(value, 0, 2000);
        }
        static public int SWAI_Subversion(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is PlayerWizard))
            {
                Debug.LogError("Spell " + spell.dbName + " caster is invalid");
                return 0;
            }
            var wizardValue = 225;

            var value = GameManager.GetWizards().Count * wizardValue;


            return value;
        }
        static public int SWAI_CloudOfShadow(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is TownLocation))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }
            if (!(source is PlayerWizard))
            {
                Debug.LogError("Spell " + spell.dbName + " source is invalid");
                return 0;
            }

            var town = target as TownLocation;
            var townValue = town.GetStrategicValue();
            int value = 0;

            if (GameManager.Get().GetEnchantments().Find(o => o.source == ENCH.ETERNAL_NIGHT) != null)
            {
                return value;
            }

            float deathBooks = (source as PlayerWizard).GetAttFinal(TAG.DEATH_MAGIC_BOOK).ToFloat();

            //Average spell value based on target
            value = (int)(townValue * deathBooks * 0.25f);

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on town " + town.name);
#endif

            return value;
        }
        static public int SWAI_WarpNode(ISpellCaster source, object target, Spell spell)
        {
            //ToDo: AI do not know how to use it
            Debug.Log("Spell " + spell.dbName + " is not check for AI use yet.");
            return 0;

            if (!(target is MOM.Location))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            var location = target as MOM.Location;
            var powerValue = 100;
            int value = 0;
            if (location.GetOwnerID() == 0) return value;
            //Average spell value based on target
            value = (int)(location.power * powerValue);

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on town " + location.name);
#endif

            return value;
        }
        static public int SWAI_CursedLands(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is TownLocation))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            var town = target as TownLocation;
            var townValue = town.GetStrategicValue();
            int value = 0;

            //Average spell value based on target
            value = (int)(townValue * 1.5f);

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on town " + town.name);
#endif

            return value;
        }
        static public int SWAI_EternalNight(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is GameManager))
            {
                if (!(target is PlayerWizard))
                {
                    Debug.LogWarning("Spell " + spell.dbName + " target is invalid");
                }
                return 0;
            }
            if (!(source is PlayerWizard))
            {
                return 0;
            }

            int deathBookValue = 500;
            int lifeBookValue = 500;
            int value = 0;

            if (GameManager.Get().GetEnchantments().Find(o => o.source == ENCH.ETERNAL_NIGHT) != null)
            {
                return value;
            }

            var caster = source as PlayerWizard;
            int deathBooks = caster.GetAttFinal(TAG.DEATH_MAGIC_BOOK).ToInt();

            foreach (var w in (target as GameManager).wizards)
            {
                if (w.ID == caster.GetID()) continue;
                value += w.GetAttFinal(TAG.LIFE_MAGIC_BOOK).ToInt() * lifeBookValue;
                value -= w.GetAttFinal(TAG.DEATH_MAGIC_BOOK).ToInt() * deathBookValue / 4;
            }

            //Average spell value based on target
            value += deathBooks * deathBookValue;


#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on from foe wizards.");
#endif
            return value;
        }
        static public int SWAI_EvilOmens(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is GameManager))
            {
                if (!(target is PlayerWizard))
                {
                    Debug.LogWarning("Spell " + spell.dbName + " target is invalid");
                }
                return 0;
            }
            if (!(source is PlayerWizard))
            {
                return 0;
            }

            //How much value that spell can give based on enemy units and enemy wizard spell books and target unit.
            int bookValue = 650;
            int value = 0;

            var caster = source as PlayerWizard;
            foreach (var w in (target as GameManager).wizards)
            {
                if (w.ID == caster?.ID) continue;
                for (int i = 0; i < w.GetAttFinal(TAG.LIFE_MAGIC_BOOK) ||
                    i < w.GetAttFinal(TAG.NATURE_MAGIC_BOOK); i++)
                {
                    value += bookValue;
                }
            }

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on from foe wizards.");
#endif
            return value;
        }
        static public int SWAI_Disenchant(ISpellCaster source, object target, Spell spell)
        {
            //ToDo: AI do not know how to use it
            //ToDo: Check with coder.
            Debug.Log("Spell " + spell.dbName + " is not check for AI use yet.");
            return 0;

            if (!(target is Vector3i))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            if (!(source is PlayerWizard))
            {
                Debug.LogError("Spell " + spell.dbName + " source is invalid");
                return 0;
            }

            int value = 0;
            var unitEnchValue = 0.05f;
            var locEnchValue = 250;

            var position = (Vector3i)target;
            var spellCaster = source.GetWizardOwner();

            //remove enchantment from units
            var groups = GameManager.Get().registeredGroups;
            var group = groups.FindAll(o => o.GetPosition() == position);
            List<EnchantmentInstance> enchList;

            foreach (var g in groups)
            {
                if (g.GetLocationHost()?.otherPlaneLocation?.Get() != null && g.plane.arcanusType) continue;
                foreach (var u in g.GetUnits())
                {
                    MOM.Unit unit = u.Get();
                    var unitOwner = unit.GetWizardOwner();

                    enchList = unit.GetEnchantments();

                    for (int i = enchList.Count - 1; i >= 0; i--)
                    {
                        if (enchList.Count <= i) continue;

                        //Dispel only ench that allow to dispel.
                        if (unit.GetEnchantments()[i].source.Get().allowDispel == false) continue;

                        //Disenchant only negative ench on own unit. Disenchant only positive ench on enemy unit.
                        if (!(unitOwner == spellCaster && enchList[i].source.Get().enchCategory == EEnchantmentCategory.Negative ||
                            unitOwner != spellCaster && enchList[i].source.Get().enchCategory == EEnchantmentCategory.Positive))
                            continue;

                        value += (int)(unit.GetWorldUnitValue() * unitEnchValue);
                    }
                }
            }

            //remove enchantment from location
            var location = GameManager.Get().registeredLocations.Find(o => o.GetPosition() == position);
            if (location == null) return value;

            int locationOwner = location.owner;
            enchList = location.GetEnchantments();
            value += locEnchValue * enchList.Count;

            for (int i = enchList.Count - 1; i >= 0; i--)
            {
                if (enchList.Count <= i) continue;

                //Dispel only ench that allow to dispel.
                if (enchList[i].source.Get().allowDispel == false) continue;

                //Disenchant only negative ench on own unit. Disenchant only positive ench on enemy unit.
                if (!(locationOwner == spellCaster.ID && enchList[i].source.Get().enchCategory == EEnchantmentCategory.Negative ||
                    locationOwner != spellCaster.ID && enchList[i].source.Get().enchCategory == EEnchantmentCategory.Positive))
                    continue;

                value += locEnchValue;
            }


#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on from foe wizards.");
#endif
            return value;
        }
        static public int SWAI_DisenchantTrue(ISpellCaster source, object target, Spell spell)
        {
            //ToDo: AI do not know how to use it
            //ToDo: Check with coder.
            Debug.Log("Spell " + spell.dbName + " is not check for AI use yet.");
            return 0;

            if (!(target is Vector3i))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            if (!(source is PlayerWizard))
            {
                Debug.LogError("Spell " + spell.dbName + " source is invalid");
                return 0;
            }

            int value = 0;
            var unitEnchValue = 0.1f;
            var locEnchValue = 300;

            var position = (Vector3i)target;
            var spellCaster = source.GetWizardOwner();

            //remove enchantment from units
            var groups = GameManager.Get().registeredGroups;
            var group = groups.FindAll(o => o.GetPosition() == position);
            List<EnchantmentInstance> enchList;

            foreach (var g in groups)
            {
                if (g.GetLocationHost()?.otherPlaneLocation?.Get() != null && g.plane.arcanusType) continue;
                foreach (var u in g.GetUnits())
                {
                    MOM.Unit unit = u.Get();
                    var unitOwner = unit.GetWizardOwner();

                    enchList = unit.GetEnchantments();

                    for (int i = enchList.Count - 1; i >= 0; i--)
                    {
                        if (enchList.Count <= i) continue;

                        //Dispel only ench that allow to dispel.
                        if (unit.GetEnchantments()[i].source.Get().allowDispel == false) continue;

                        //Disenchant only negative ench on own unit. Disenchant only positive ench on enemy unit.
                        if (!(unitOwner == spellCaster && enchList[i].source.Get().enchCategory == EEnchantmentCategory.Negative ||
                            unitOwner != spellCaster && enchList[i].source.Get().enchCategory == EEnchantmentCategory.Positive))
                            continue;

                        value += (int)(unit.GetWorldUnitValue() * unitEnchValue);
                    }
                }
            }

            //remove enchantment from location
            var location = GameManager.Get().registeredLocations.Find(o => o.GetPosition() == position);
            if (location == null) return value;

            int locationOwner = location.owner;
            enchList = location.GetEnchantments();

            for (int i = enchList.Count - 1; i >= 0; i--)
            {
                if (enchList.Count <= i) continue;

                //Dispel only ench that allow to dispel.
                if (enchList[i].source.Get().allowDispel == false) continue;

                //Disenchant only negative ench on own unit. Disenchant only positive ench on enemy unit.
                if (!(locationOwner == spellCaster.ID && enchList[i].source.Get().enchCategory == EEnchantmentCategory.Negative ||
                    locationOwner != spellCaster.ID && enchList[i].source.Get().enchCategory == EEnchantmentCategory.Positive))
                    continue;

                value += locEnchValue;
            }


#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on from foe wizards.");
#endif
            return value;
        }
        static public int SWAI_SpellBlast(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is PlayerWizard))
            {
                Debug.LogError("Spell " + spell.dbName + " caster is invalid");
                return 0;
            }

            var caster = (source as PlayerWizard);
            if (caster == null) return 0;

            int value = 0;
            var manaValue = 2;


            var curentlyCastedSpell = (target as PlayerWizard).GetMagicAndResearch().curentlyCastSpell;

            if (curentlyCastedSpell != null)
            {
                var magicAndResearch = (target as PlayerWizard).GetMagicAndResearch();

                if (magicAndResearch.castingProgress > caster.mana) return 0;


                if (magicAndResearch.curentlyCastSpell == (Spell)SPELL.SPELL_OF_MASTERY)
                {
                    return 9000;
                }
                else
                {
                    var castingProgress = magicAndResearch.castingProgress;
                    value = castingProgress * manaValue;

                    if (magicAndResearch.castingProgress > caster.mana / 2) return value / 3;
                    if (magicAndResearch.castingProgress > caster.mana / 3) return value / 2;
                    if (magicAndResearch.castingProgress > caster.mana / 5) return value;
                    return (int)(value * 1.5f);
                }
            }
            return 0;
        }
        static public int SWAI_AuraOfMajesty(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is PlayerWizard))
            {
                Debug.LogError("Spell " + spell.dbName + " source is invalid");
                return 0;
            }

            var wizardTarget = target as PlayerWizard;
            if (wizardTarget.discoveredWizards == null) return 0;

            var wizardValue = 225;

            var value = wizardTarget.discoveredWizards.Count * wizardValue;

            return value;
        }
        static public int SWAI_DisjunctionTrue(ISpellCaster source, object target, Spell spell)
        {
            var ei = target as EnchantmentInstance;
            if (ei == null)
            {
                return 0;
            }

            var pw = ei.owner?.Get<PlayerWizard>();
            if (source == pw) return 0;

            if (ei.manager.owner == source)
            {
                //enemy effect on self
                return 1000 + ei.upkeepMana * 100;
            }
            else
            {
                return 700 + ei.upkeepMana * 100;
            }
        }
        static public int SWAI_Disjunction(ISpellCaster source, object target, Spell spell)
        {
            var ei = target as EnchantmentInstance;
            if (ei == null)
            {
                return 0;
            }

            var pw = ei.owner?.Get<PlayerWizard>();
            if (source == pw) return 0;

            if (ei.manager.owner == source)
            {
                //enemy effect on self
                return 750 + ei.upkeepMana * 50;
            }
            else
            {
                return 350 + ei.upkeepMana * 50;
            }
        }

        static public int SWAI_Invisibility(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is MOM.Unit))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            var unit = target as MOM.Unit;
            int value = 0;

            if (unit.attributes.Contains(TAG.INVISIBILITY))
            {
#if (UNITY_EDITOR && DEBUG_SPELLS)
                Debug.Log(spell.dbName + " with script " +
                    spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                    " on unit " + unit.GetDBName().ToString());
#endif
                return value;
            }

            //Average spell value based on target
            value = (int)(unit.GetWorldUnitValue() * 0.3f);


#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on unit " + unit.GetDBName().ToString());
#endif

            return value;
        }
        static public int SWAI_SpellBinding(ISpellCaster source, object target, Spell spell)
        {
            //ToDo: AI do not know how to use it
            //ToDo: Spell do not have target for AI / there is no Targeting Script
            Debug.Log("Spell " + spell.dbName + " is not check for AI use yet.");
            return 0;

            if (!(source is PlayerWizard))
            {
                return 0;
            }

            int value = 0;
            // if target is spell researchcost * 2 as a value.


#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on from foe wizards.");
#endif
            return value;
        }
        static public int SWAI_SuppressMagic(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is GameManager))
            {
                if (!(target is PlayerWizard))
                {
                    Debug.LogWarning("Spell " + spell.dbName + " target is invalid");
                }
                return 0;
            }
            if (!(source is PlayerWizard))
            {
                return 0;
            }

            int bookValue = 250;
            int value = 0;

            var caster = source as PlayerWizard;
            var wizards = GameManager.GetWizards();
            foreach (var w in wizards)
            {
                if (w.ID == caster?.ID) continue;
                FInt book = w.GetAttFinal(TAG.MAGIC_BOOK);
                value += bookValue * book.ToInt();
            }

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on from foe wizards.");
#endif
            return value;
        }
        static public int SWAI_ZombieMastery(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is PlayerWizard))
            {
                Debug.LogError("Spell " + spell.dbName + " caster is invalid");
                return 0;
            }
            var value = 0;
            var valueMulti = 500;

            var caster = target as PlayerWizard;
            value = valueMulti + valueMulti * caster.GetAttFinal(TAG.DEATH_MAGIC_BOOK).ToInt();

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on from foe wizards.");
#endif

            return value;
        }
        static public int SWAI_WallOfFire(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is TownLocation))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            var town = target as TownLocation;
            var townValue = town.GetStrategicValue();
            int value = 0;

            //Average spell value based on target
            value = (int)(townValue * 1.0f);

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on town " + town.name);
#endif

            return value;
        }
        static public int SWAI_WordOfRecal(ISpellCaster source, object target, Spell spell)
        {
            if (source == null || target == null
                || !(source is PlayerWizard) || !(target is MOM.Unit)) return 0;

            var wizard = source as PlayerWizard;

            if (wizard.GetTowerLocation() == null) return 0;
            var tower = wizard.GetTowerLocation();

            if (tower.GetUnits().Count > 0) return 0;

            var menager = GameManager.Get();
            foreach (var hex in tower.GetSurroundingArea(2))
            {
                if (menager.GetGroupAt(hex, tower.GetPlane()) != null)
                {
                    var u = target as MOM.Unit;
                    if (u.GetAttributes().Contains(TAG.SHIP) &&
                        u.GetAttributes().DoesNotContains((Tag)TAG.CAN_FLY)) return 0;

                    var targetValue = u.GetWorldUnitValue();

                    return targetValue;
                }
            }
            return 0;
        }

        static public int SWAI_TimeStop(ISpellCaster source, object target, Spell spell)
        {
            var w = source as PlayerWizard;
            if (w == null)
            {
                Debug.LogError("incorrect input data, missing wizard as caster");
                return 0;
            }

            //see how much mana would be left after this spell is cast.
            var rem = w.mana - spell.worldCost;
            if (rem < 0) return 0;

            //see how much mana from reserve need to be add each turn to sustain time stop
            int cost = spell.upkeepCost - w.CalculateManaIncome(true);

            //if cost could be sustained from mana produced, this wizard could never leave time stop
            if (cost <= 0) return 4000;

            //each turn that spell could be sustained provides 100 strategic value to the caster
            //main reason this value is so low, is very high initial cost.
            return Mathf.Min(4000, 100 * rem / cost);
        }
        static public int SWAI_SpellOfMastery(ISpellCaster source, object target, Spell spell)
        {
            var w = source as PlayerWizard;
            if (w == null)
            {
                Debug.LogError("incorrect input data, missing wizard as caster");
                return 0;
            }

            int skill = w.GetTotalCastingSkill();
            int cost = spell.worldCost - w.mana;

            if (skill == 0) return 0;
            int turns = cost / skill;

            //spell value is:
            //- 2000 if casting would take 200 turns
            //- 10000 if casting would take 100 turns
            //- 25000 if casting would take 40 turns
            //- 100000 if casting would take 10 turns

            return 400000 / turns;

        }
        static public int SWAI_HeroTraining(ISpellCaster source, object target, Spell spell)
        {
            if (!(source is PlayerWizardAI))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            int value = 0;
            var valueAdd = 350;

            var wizard = source as PlayerWizardAI;

            foreach (var h in wizard.heroes)
            {
                if (h.Get().GetLevel() > 3)
                {
                    value += valueAdd;
                }
            }

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on towns ");
#endif

            return value;
        }
        static public int SWAI_DetectMinerals(ISpellCaster source, object target, Spell spell)
        {
            //ToDo: Check with coder
            //ToDo: AI do not know how to use it
            Debug.Log("Spell " + spell.dbName + " is not check for AI use yet.");
            return 0;

            if (!(target is Hex))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            var hex = target as Hex;
            var caster = source as PlayerWizardAI;


            var locations = GameManager.Get().registeredLocations;

            foreach (var l in locations)
            {
                if (!(l is TownLocation) || caster.GetID() != (l as TownLocation).GetID()) continue;

                if (WorldCode.Plane.Get().GetDistanceWrapping(hex.Position, l.GetPosition()) < 3)
                {
                    var townValue = (l as TownLocation).GetStrategicValue();
                    return (int)(townValue * 0.7f);
                }
            }

            return 0;
        }
        static public int SWAI_AddAmmo(ISpellCaster source, object target, Spell spell)
        {
            if (!(target is MOM.Unit))
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return 0;
            }

            var unit = target as MOM.Unit;
            var buValue = unit.GetWorldUnitValue();
            int value = 0;

            //Average spell value based on target
            value = unit.GetModifiedWorldUnitValue(TAG.AMMUNITION, (FInt)2.0) + buValue;

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + value +
                " on unit " + unit.GetDBName().ToString());
#endif

            return value;
        }
        #endregion
        #region Utility
        public static MOM.Unit AnimateDead(MOM.Unit source, BattleUnit activeBU, SpellCastData data)
        {
            MOM.Unit newUnit = source;
            
            if (source.dbSource == (Subrace)UNIT.LIF_ARCH_ANGEL)
            {
                newUnit = MOM.Unit.CreateFrom((Subrace)UNIT.DTH_ARCH_ANGEL);
            }
            else if (source.dbSource == (Subrace)UNIT.SOR_DJINN)
            {
                newUnit = MOM.Unit.CreateFrom((Subrace)UNIT.DTH_DJINN);
            }
            else if (source.dbSource == (Subrace)UNIT.CHA_EFREET)
            {
                newUnit = MOM.Unit.CreateFrom((Subrace)UNIT.DTH_EFREET);
            }

            List<Skill> except = new List<Skill>() { (Skill)SKILL.CHAOS_CHANNELS1,
                                                     (Skill)SKILL.CHAOS_CHANNELS2,
                                                     (Skill)SKILL.CHAOS_CHANNELS3};
            if (source != newUnit)
            {
                newUnit.xp = source.xp;
                newUnit.CopySkillManagerFrom(source, except);                
            }
            else
            {
                foreach (var skill in except)
                {
                    newUnit.GetSkillManager().Remove(skill);
                }                
            }
            UpdateRaiseDeadAttributes(newUnit);

            #region update or create formation if this happens during not simulated battle
            if (data != null && data.battle != null && !data.battle.simulation)
            {
                if (source != newUnit && activeBU != null && data != null)
                {
                    BattleUnit animateUnit = BattleUnit.Create(newUnit, false, data.GetWizardID(), data.IsCasterAttackingSide());
                    animateUnit.Mp = new FInt(animateUnit.GetCurentFigure().movementSpeed);
                    animateUnit.GetCurentFigure().rangedAmmo = activeBU.GetCurentFigure().rangedAmmo;
                    animateUnit.mana = activeBU.mana;
                    animateUnit.battlePosition = activeBU.GetPosition();                    
                    animateUnit.summon = activeBU.summon; //if any effect would allow to work on summons, this marker should persist

                    data.battle.buToSource.Remove(activeBU);
                    if (data.battle.defenderUnits.Contains(activeBU))
                        data.battle.defenderUnits.Remove(activeBU);
                    else if (data.battle.attackerUnits.Contains(activeBU))
                        data.battle.attackerUnits.Remove(activeBU);

                    data.battle.buToSource[animateUnit] = newUnit;                    
                    if (activeBU.battleFormation != null)
                        activeBU.battleFormation.Destroy();

                    activeBU = animateUnit;
                }
                else
                {
                    activeBU.HealUnit(activeBU.GetMaxFigureCount() * activeBU.GetCurentFigure().maxHitPoints, true);

                    if (data.battle.defenderUnits.Contains(activeBU))
                        data.battle.defenderUnits.Remove(activeBU);
                    else if (data.battle.attackerUnits.Contains(activeBU))
                        data.battle.attackerUnits.Remove(activeBU);
                }

                activeBU.attackingSide = data.IsCasterAttackingSide();
                activeBU.irreversibleDamages = 0;
                activeBU.undeadDamages = 0;
                activeBU.normalDamages = 0;                
                UpdateRaiseDeadAttributes(activeBU);

                activeBU.ownerID = data.GetWizardID();
                if (data.IsCasterAttackingSide())
                {                    
                    data.battle.AttackerAddUnit(activeBU);
                }
                else
                    data.battle.DefenderAddUnit(activeBU);                
                
                data.battle.plane.ClearSearcherData();

                var formation = activeBU.GetOrCreateFormation(null, true);
                if (formation != null)
                {
                    formation.InstantMove();
                    formation.UpdateFigureCount();
                }

                BattleHUD.Get().BaseUpdate();
                VerticalMarkerManager.Get().Addmarker(activeBU);                
            }
            #endregion
            return newUnit;
        }
        private static void UpdateRaiseDeadAttributes(BaseUnit b)
        {
            b.AddEnchantment((Enchantment)ENCH.REANIMATE_UNDEAD, null);
            b.race = (Race)RACE.REALM_DEATH;
            b.canNaturalHeal = false;
            b.canGainXP = false;

            List<Skill> except = new List<Skill>() { (Skill)SKILL.CHAOS_CHANNELS1,
                                                     (Skill)SKILL.CHAOS_CHANNELS2,
                                                     (Skill)SKILL.CHAOS_CHANNELS3};
            List<Skill> include = new List<Skill>() {(Skill)SKILL.COLD_IMMUNITY,
                                                     (Skill)SKILL.POISON_IMMUNITY,
                                                     (Skill)SKILL.ILLUSIONS_IMMUNITY,
                                                     (Skill)SKILL.DEATH_IMMUNITY};
            var skillManager = b.GetSkillManager();
            var attributes = b.GetAttributes();
            foreach (var s in except)
            {
                if (b.GetSkills().Find(o => o == s) != null)
                {
                    b.RemoveSkill(s);
                }
            }
            foreach (var s in include)
            {
                if (b.GetSkills().Find(o => o == s) == null)
                {
                    b.AddSkill(s);
                }
            }

            if (attributes.Contains(TAG.SETTLER_UNIT))
            {
                b.AddSkill((Skill)SKILL.REANIMATED_SETTLER);
            }
            if(attributes.Contains(TAG.NORMAL_CLASS))
            {
                attributes.SetBaseTo(TAG.NORMAL_CLASS, FInt.ZERO);
                attributes.SetBaseTo(TAG.FANTASTIC_CLASS, FInt.ONE);
                b.EnsureEnchantments();
            }
        }
        public static FInt ResistModFromEnch(BattleUnit attackerBu, BattleUnit targetBu, Spell spell)
        {
            FInt resist = FInt.ZERO;
            FInt resistMod = FInt.ZERO;
            if (attackerBu != null)
                resistMod = attackerBu.attributes.GetFinal((Tag)TAG.SPELL_SAVE);

            if (spell.realm == ERealm.Chaos && targetBu.attributes.Contains(TAG.RIGHTEOUSNESS) ||
                spell.realm == ERealm.Death && targetBu.attributes.Contains(TAG.RIGHTEOUSNESS))
                resist = new FInt(30);

            else if (spell.realm == ERealm.Chaos && targetBu.attributes.Contains(TAG.ELEMENTAL_ARMOR) ||
                     spell.realm == ERealm.Nature && targetBu.attributes.Contains(TAG.ELEMENTAL_ARMOR))
                resist = new FInt(10);

            else if (spell.realm == ERealm.Chaos && targetBu.attributes.Contains(TAG.RESIST_ELEMENTS) ||
                     spell.realm == ERealm.Nature && targetBu.attributes.Contains(TAG.RESIST_ELEMENTS))
                resist = new FInt(3);

            else if (spell.realm == ERealm.Chaos && targetBu.attributes.Contains(TAG.BLESS) ||
                     spell.realm == ERealm.Death && targetBu.attributes.Contains(TAG.BLESS))
                resist = new FInt(3);

            return resist - resistMod;
        }
        public static FInt ResistModFromEnch(MOM.Unit attackerU, MOM.Unit targetUnit, Spell spell)
        {
            FInt resist = FInt.ZERO;
            FInt resistMod = FInt.ZERO;
            if (attackerU != null)
                resistMod = attackerU.attributes.GetFinal((Tag)TAG.SPELL_SAVE);


            if (spell.realm == ERealm.Chaos && targetUnit.attributes.Contains(TAG.RIGHTEOUSNESS) ||
                spell.realm == ERealm.Death && targetUnit.attributes.Contains(TAG.RIGHTEOUSNESS))
                resist = new FInt(30);

            else if (spell.realm == ERealm.Chaos && targetUnit.attributes.Contains(TAG.ELEMENTAL_ARMOR) ||
                spell.realm == ERealm.Nature && targetUnit.attributes.Contains(TAG.ELEMENTAL_ARMOR))
                resist = new FInt(10);

            else if (spell.realm == ERealm.Chaos && targetUnit.attributes.Contains(TAG.RESIST_ELEMENTS) ||
                spell.realm == ERealm.Nature && targetUnit.attributes.Contains(TAG.RESIST_ELEMENTS))
                resist = new FInt(3);

            else if (spell.realm == ERealm.Chaos && targetUnit.attributes.Contains(TAG.BLESS) ||
                spell.realm == ERealm.Death && targetUnit.attributes.Contains(TAG.BLESS))
                resist = new FInt(3);

            return resist - resistMod;
        }
        public static bool IsTownProtected(int spellcasterSideWizardID, Spell spell, TownLocation town)
        {
            if (town != null)
            {
                if (spellcasterSideWizardID != town.GetOwnerID() &&
                    (spell.realm == ERealm.Nature && town.isNatureProtected > 0 ||
                    spell.realm == ERealm.Sorcery && town.isSorceryProtected > 0 ||
                    spell.realm == ERealm.Chaos && town.isChaosProtected > 0 ||
                    spell.realm == ERealm.Life && town.isLifeProtected > 0 ||
                    spell.realm == ERealm.Death && town.isDeathProtected > 0))
                {
                    return true;
                }
            }

            return false;
        }
        public static bool IsTownProtected(int spellcasterSideWizardID, Spell spell, Battle battle)
        {
            if (battle != null)
            {
                BattlePlayer defender = battle.defender;

                if (spellcasterSideWizardID != defender.GetID() &&
                    (spell.realm == ERealm.Nature && defender.isNatureProtected > 0 ||
                    spell.realm == ERealm.Sorcery && defender.isSorceryProtected > 0 ||
                    spell.realm == ERealm.Chaos && defender.isChaosProtected > 0 ||
                    spell.realm == ERealm.Life && defender.isLifeProtected > 0 ||
                    spell.realm == ERealm.Death && defender.isDeathProtected > 0))
                {
                    return true;
                }
            }

            return false;
        }

        static public bool GetDispelSuccess(
            PlayerWizard spellCasterAsWizard,
            EnchantmentInstance enchantment,
            FInt dispelStrength)
        {
            // Dispell SpellLock first then other ench
            return GetDispelSuccess(spellCasterAsWizard, enchantment, dispelStrength, (FInt)enchantment.dispelCost);
        }

        static public bool GetDispelSuccess(
            PlayerWizard spellCasterAsWizard,
            EnchantmentInstance enchantment,
            FInt dispelStrength,
            FInt spellCost)
        {
            PlayerWizard enchantmentOwner = enchantment.owner != null ? enchantment.owner.GetEntity() as PlayerWizard : null;
            var i = FInt.ONE;
            if (enchantmentOwner != null)
            {
                i = enchantmentOwner.GetDispelDificulty(enchantment);
            }

            var dispelChance = FInt.ZERO;
            if (spellCasterAsWizard == null)
            {
                dispelChance = dispelStrength / (dispelStrength + i * spellCost);
            }
            else
            {
                dispelChance = spellCasterAsWizard.easierDispelling * dispelStrength / (dispelStrength + i * spellCost);
            }

            return random.GetSuccesses(dispelChance.ToFloat(), 1) > 0;
        }
        static bool EnchAlreadyOnObject(ISpellCaster spellCaster, Spell spell, object testedObject)
        {
            var enchNum = 0;

            //Search for ench that target have already on it
            if (spell != null && spell.enchantmentData != null)
            {
                if (testedObject is IEnchantable)
                {
                    var enchantments = (testedObject as IEnchantable).GetEnchantments();
                    if (enchantments != null && enchantments.Count > 0)
                    {
                        foreach (var e in spell.enchantmentData)
                        {
                            foreach (var eInst in enchantments)
                            {
                                if (eInst.source.Get() != e) continue;

                                //if enchantment have no owner
                                if (eInst.owner == null)
                                {
                                    //Count up if caster is null 
                                    if (spellCaster == null) enchNum++;

                                    //either way Exit
                                    continue;
                                }

                                //if caster is null, but enchantment have owner Exit
                                if (spellCaster == null) continue;

                                //if caster is not null, and neither is owner
                                if ((eInst.owner.GetEntity() is ISpellCaster) &&
                                    (eInst.owner.GetEntity() as ISpellCaster).GetWizardOwnerID() == spellCaster.GetWizardOwnerID())
                                {
                                    enchNum++;
                                }

                                if (enchNum == spell.enchantmentData.Length)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }

                enchNum = 0;

                if (testedObject is ISkillable)
                {
                    var skills = (testedObject as ISkillable)?.GetSkills();
                    if (skills != null && skills.Count > 0)
                    {
                        foreach (var e in spell.enchantmentData)
                        {
                            //check if item skill is already on unit
                            if (skills != null && skills.Count > 0)
                            {
                                foreach (var s in skills)
                                {
                                    var enchs = s.Get().relatedEnchantment;
                                    if (enchs != null && enchs.Length > 0)
                                    {
                                        foreach (var en in enchs)
                                        {
                                            if (en == e) enchNum++;

                                            if (enchNum == spell.enchantmentData.Length)
                                            {
                                                return true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        public static int ProduceSpellDamage(int dmg)
        {
            var random = new MHRandom();
            var hitChance = 0.3f;
            dmg = random.GetSuccesses(hitChance, dmg);
            return dmg;
        }
        public static int[] ProduceAreaSpellDamage(int baseDmg, int[] dmg, int figures)
        {
            for (int i = 0; i < figures; i++)
            {
                var random = new MHRandom();
                var hitChance = 0.3f;
                dmg[i] = random.GetSuccesses(hitChance, baseDmg);
            }
            return dmg;
        }
        #endregion
    }
}
#endif