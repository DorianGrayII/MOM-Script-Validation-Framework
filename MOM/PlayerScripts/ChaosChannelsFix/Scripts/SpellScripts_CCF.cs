/**********************************
 *
 * Author:  Dorian Gray
 * Date:    Feb 23, 2024
 * Version: 1.0.7
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

namespace MOMScripts_CCF
{
    public class SpellScripts : ScriptBase
    {

        public static bool SWG_ChaosChannels(ISpellCaster source, object target, Spell spell)
        {
            // Debug.Log("Updated SWG_ChaosChannels method invoked");
            MOM.Unit unit = target as MOM.Unit;

            if (unit == null)
            {
                Debug.LogWarning("SWG_ChaosChannels is not targeting unit on world map.");
                return false;
            }

            List<DBReference<Skill>> unitSkills = unit.GetSkills();
            List<DBReference<Skill>> potentialSkills = new List<DBReference<Skill>>();

            foreach (string str in spell.stringData)
            {
                potentialSkills.Add(DataBase.Get(str, false) as Skill);
            }

            foreach (DBReference<Skill> skill in unitSkills)
            {
                if (potentialSkills.Contains(skill))
                {
                    potentialSkills.Remove(skill);
                }
            }

            if (potentialSkills.Count > 0)
            {
                potentialSkills.RandomSort();

                for (int i = potentialSkills.Count - 1; i >= 0; i--)
                {
                    if (potentialSkills[i].Get() == (Skill)SKILL.CHAOS_CHANNELS3)
                    {
//                      Debug.Log("Considering CHAOS_CHANNELS3(Fire Breath)");

                        // do not apply to ranged units
                        if (unit.GetAttributes().DoesNotContains((Tag)TAG.RANGED_UNIT))
                        {
                            Dictionary<ESkillType, List<SkillScript>> unitSkillScripts = unit.GetSkillManager().GetSkillScripts();
                            bool unitOwnFireBreath = false;
                            foreach (KeyValuePair<ESkillType, List<SkillScript>> uss in unitSkillScripts)
                            {
                                foreach (SkillScript e in uss.Value)
                                {
                                    if (e.activatorSecondary == null)
                                    {
                                        continue;
                                    }

                                    if (e.activatorSecondary.Contains("ACT_ApplyFireBreathAttack"))
                                    {
                                        unitOwnFireBreath = true;
                                    }
                                }
                            }

                            // do not apply to units who already have Fire Breath
                            if ((unitOwnFireBreath == false) &&
                                (unit.GetSkills().Contains((Skill)SKILL.FIRE_BREATH) == false))
                            {
                                // Add CHAOS_CHANNELS3 to the unit
                                unit.AddSkill(potentialSkills[i].Get());
                                unit.AddSkill((Skill)SKILL.FIRE_BREATH);
                                if (unit.GetAttributes().DoesNotContains((Tag)TAG.FANTASTIC_CLASS))
                                {
                                    unit.GetAttributes().AddToBase((Tag)TAG.FANTASTIC_CLASS, FInt.ONE);
                                    unit.EnsureEnchantments();
                                }

//                              Debug.Log("CHAOS_CHANNELS3(Fire Breath) is applied");
                                return true;
                            }
                        }
                    }
                    else if (potentialSkills[i].Get() == (Skill)SKILL.CHAOS_CHANNELS2)
                    {
//                      Debug.Log("Considering CHAOS_CHANNELS2(Flying)");

                        // do not apply to units that can already fly or to ship units
                        if ((unit.GetAttFinal(TAG.CAN_FLY) <= 0) && (unit.GetAttFinal(TAG.SHIP) <= 0))
                        {
                            // Add CHAOS_CHANNELS2 to the unit
                            unit.AddSkill(potentialSkills[i].Get());
                            if (unit.GetAttributes().DoesNotContains((Tag)TAG.FANTASTIC_CLASS))
                            {
                                unit.GetAttributes().AddToBase((Tag)TAG.FANTASTIC_CLASS, FInt.ONE);
                                unit.EnsureEnchantments();
                            }

//                          Debug.Log("CHAOS_CHANNELS2(Flying) is applied");
                            return true;
                        }
                    }
                    else if (potentialSkills[i].Get() == (Skill)SKILL.CHAOS_CHANNELS1)
                    {
//                      Debug.Log("Defaulted to CHAOS_CHANNELS1(DemonSkin)");

                        // Add CHAOS_CHANNELS1 to the unit
                        unit.AddSkill(potentialSkills[i].Get());
                        if (unit.GetAttributes().DoesNotContains((Tag)TAG.FANTASTIC_CLASS))
                        {
                            unit.GetAttributes().AddToBase((Tag)TAG.FANTASTIC_CLASS, FInt.ONE);
                            unit.EnsureEnchantments();
                        }

//                      Debug.Log("CHAOS_CHANNELS1(DemonSkin) is applied");
                        return true;
                    }
                    else
                    {
                        DescriptionInfo descriptionInfo = potentialSkills[i].Get().GetDescriptionInfo();
                        Debug.LogError("SWG_ChaosChannels encountered unexpected potential Skill: " + descriptionInfo.GetName());
                        return false;
                    }
                }
            }  // if (potentialSkills.Count > 0)

            return false;
        }
    }
}

#endif