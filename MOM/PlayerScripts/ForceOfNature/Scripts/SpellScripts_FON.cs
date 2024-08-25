/**********************************
 *
 * Author:  Dorian Gray
 * Date:    May 19, 2024
 * Version: 1.0.3
 *
 **********************************/

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

namespace MOMScripts_FON
{
    using static UserUtility.Utility;

    public class SpellScripts : ScriptBase
    {

        const bool bIsLoggingEnabled = false;

        public static int SWAI_ForceOfNature(ISpellCaster source, object target, Spell spell)
        {
            int iRetVal = 0;
            MOM.Unit unit = target as MOM.Unit;

            if (unit == null)
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return iRetVal;
            }

            int buValue = unit.GetWorldUnitValue();

            //Average spell value based on target
            iRetVal = unit.GetModifiedWorldUnitValue(TAG.DEFENCE, (FInt)3.0) -
                         buValue;

#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiWorldEvaluationScript.ToString() + " give SpellAI value " + iRetVal +
                " on unit " + unit.GetDBName().ToString());
#endif

            return iRetVal;
        }


        public static bool SWG_ForceOfNature(ISpellCaster source, object target, Spell spell)
        {
            MOM.Unit unit = target as MOM.Unit;

            if (unit == null)
            {
                Debug.LogWarning("SWG_ForceOfNature is not targeting unit on world map.");
                return false;
            }

            List<DBReference<Skill>> unitSkills      = unit.GetSkills();
            List<DBReference<Skill>> potentialSkills = new List<DBReference<Skill>>();

            // This would be SKILL-FORCE_OF_NATURE1, SKILL-FORCE_OF_NATURE2, SKILL-FORCE_OF_NATURE3
            foreach (string str in spell.stringData)
            {
                potentialSkills.Add(DataBase.Get(str, true) as Skill);
            }

            // Can only have one of the three, so return if unit has any already
            foreach (DBReference<Skill> skill in unitSkills)
            {
                if (potentialSkills.Contains(skill))
                {
                    return true;
                }
            }

            if (potentialSkills.Count > 0)
            {
                potentialSkills.RandomSort();

                foreach (DBReference<Skill> skill in potentialSkills)
                {
                    if (skill == (Skill)"SKILL-FORCE_OF_NATURE3")
                    {
                        // Debug.Log("Considering FORCE_OF_NATURE3");

                        // only apply following to non-ranged units
                        if (unit.GetAttributes().DoesNotContains((Tag)TAG.RANGED_UNIT))
                        {
                            Dictionary<ESkillType, List<SkillScript>> unitSkillScripts = unit.GetSkillManager().GetSkillScripts();
                            bool unitOwnPoison = false;
                            foreach (KeyValuePair<ESkillType, List<SkillScript>> uss in unitSkillScripts)
                            {
                                foreach (SkillScript e in uss.Value)
                                {
                                    if (e.activatorSecondary == null)
                                    {
                                        continue;
                                    }

                                    if (e.activatorSecondary.Contains("ACT_ApplyPoisonAttack"))
                                    {
                                        unitOwnPoison = true;
                                    }
                                }
                            }

                            // do not apply to units who already have POISON_4_ADDON2
                            if ((unitOwnPoison == false) &&
                                (unit.GetSkills().Contains((Skill)SKILL.POISON_4_ADDON2) == false))
                            {
                                // Add FORCE_OF_NATURE3 to the unit
                                unit.AddSkill(skill);
                                unit.AddSkill((Skill)SKILL.POISON_4_ADDON2);
                                if (unit.GetAttributes().DoesNotContains((Tag)TAG.FANTASTIC_CLASS))
                                {
                                    unit.GetAttributes().AddToBase((Tag)TAG.FANTASTIC_CLASS, FInt.ONE);
                                    unit.EnsureEnchantments();
                                }

//                              Debug.Log("FORCE_OF_NATURE3(Poison Touch) is applied");
                                return true;
                            }
                        }
                    }
                    else if (skill == (Skill)"SKILL-FORCE_OF_NATURE2")
                    {
                        // Debug.Log("Considering FORCE_OF_NATURE2");

                        // only apply following to non-pathfinding units
                        if (unit.GetAttFinal(TAG.PATHFINDING) <= 0)
                        {
                            unit.AddSkill(skill);
                            unit.GetAttributes().AddToBase((Tag)"TAG-PATHFINDING", FInt.ONE);

                            if (unit.GetAttributes().DoesNotContains((Tag)TAG.FANTASTIC_CLASS))
                                {
                                unit.GetAttributes().AddToBase((Tag)TAG.FANTASTIC_CLASS, FInt.ONE);
                                unit.EnsureEnchantments();
                                }
                            // Debug.Log("FORCE_OF_NATURE2(Pathfinding) is applied");
                            return true;
                        }
                    }
                    else
                    {
                        // Debug.Log("defaulted to FORCE_OF_NATURE1");

                        unit.AddSkill(skill);
                        if (unit.GetAttributes().DoesNotContains((Tag)TAG.FANTASTIC_CLASS))
                            {
                            unit.GetAttributes().AddToBase((Tag)TAG.FANTASTIC_CLASS, FInt.ONE);
                            unit.EnsureEnchantments();
                            }

                        // Debug.Log("FORCE_OF_NATURE1(IronSkin) is applied");
                        return true;
                    }
                }
            }
            return false;
        }

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
                                                     (Skill)SKILL.CHAOS_CHANNELS3,
                                                     (Skill)"SKILL-FORCE_OF_NATURE1",
                                                     (Skill)"SKILL-FORCE_OF_NATURE2",
                                                     (Skill)"SKILL-FORCE_OF_NATURE3" };
            if (source != newUnit)
            {
                newUnit.xp = source.xp;
                newUnit.CopySkillManagerFrom(source, except);
            }
            else
            {
                foreach (Skill skill in except)
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
                    {
                        data.battle.defenderUnits.Remove(activeBU);
                    }
                    else if (data.battle.attackerUnits.Contains(activeBU))
                    {
                        data.battle.attackerUnits.Remove(activeBU);
                    }

                    data.battle.buToSource[animateUnit] = newUnit;
                    if (activeBU.battleFormation != null)
                    {
                        activeBU.battleFormation.Destroy();
                    }

                    activeBU = animateUnit;
                }
                else
                {
                    activeBU.HealUnit(activeBU.GetMaxFigureCount() * activeBU.GetCurentFigure().maxHitPoints, true);

                    if (data.battle.defenderUnits.Contains(activeBU))
                    {
                        data.battle.defenderUnits.Remove(activeBU);
                    }
                    else if (data.battle.attackerUnits.Contains(activeBU))
                    {
                        data.battle.attackerUnits.Remove(activeBU);
                    }
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
                {
                    data.battle.DefenderAddUnit(activeBU);
                }

                data.battle.plane.ClearSearcherData();

                Formation formation = activeBU.GetOrCreateFormation(null, true);
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
                                                     (Skill)SKILL.CHAOS_CHANNELS3,
                                                     (Skill)"SKILL-FORCE_OF_NATURE1",
                                                     (Skill)"SKILL-FORCE_OF_NATURE2",
                                                     (Skill)"SKILL-FORCE_OF_NATURE3" };
            List<Skill> include = new List<Skill>() {(Skill)SKILL.COLD_IMMUNITY,
                                                     (Skill)SKILL.POISON_IMMUNITY,
                                                     (Skill)SKILL.ILLUSIONS_IMMUNITY,
                                                     (Skill)SKILL.DEATH_IMMUNITY};
            SkillManager skillManager = b.GetSkillManager();
            Attributes attributes = b.GetAttributes();
            foreach (Skill s in except)
            {
                if (skillManager.GetSkills().Find(o => o == s) != null)
                {
                    skillManager.Remove(s);
                }
            }
            foreach (Skill s in include)
            {
                if (skillManager.GetSkills().Find(o => o == s) == null)
                {
                    skillManager.Add(s);
                }
            }

            if (attributes.Contains(TAG.SETTLER_UNIT))
            {
                skillManager.Add((Skill)SKILL.REANIMATED_SETTLER);
            }
            if(attributes.Contains(TAG.NORMAL_CLASS))
            {
                attributes.SetBaseTo(TAG.NORMAL_CLASS, FInt.ZERO);
                attributes.SetBaseTo(TAG.FANTASTIC_CLASS, FInt.ONE);
                b.EnsureEnchantments();
            }
        }
        #endregion

    }
}

#endif