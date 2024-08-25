/**********************************
 *
 * Author:  Dorian Gray
 * Date:    May 19, 2024
 * Version: 1.0.2
 *
 **********************************/

#if !USE_DEBUG_SCRIPT || !UNITY_EDITOR

using System;
using System.Collections.Generic;
using DBDef;
using DBEnum;
using MHUtils;
using MOM;
using UnityEngine;
using WorldCode;


namespace GameScript_ETM
{
    using static UserUtility.Utility;

    public class SpellScripts : ScriptBase
    {
        /// <summary>
        /// enables verbose counter magic logging
        /// </summary>
        private const bool bLoggingEnabled = true;

        #region EarthToMud
        public static int SBAI_EarthToMud(SpellCastData data, object target, Spell spell)
        {
            int iRetVal = 0;
            Vector3i pos = (Vector3i)target;
            if (pos == null)
            {
                Debug.LogError("  [SBAI_EarthToMud] target == null");
                return iRetVal;
            }

            int iDistance = spell.fIntData[0].ToInt();

            Battle battle = data.battle;

            List<BattleUnit> buList = battle.GetAllUnits();
            foreach (BattleUnit bu in buList)
            {
                if (HexCoordinates.HexDistance(bu.GetPosition(), pos) < iDistance &&
                    bu.attributes.DoesNotContains((Tag)TAG.TELEPORTING) &&
                    bu.attributes.DoesNotContains((Tag)TAG.CAN_FLY) &&
                    bu.attributes.DoesNotContains((Tag)TAG.NON_CORPOREAL) &&
                    bu.attributes.DoesNotContains((Tag)TAG.EARTH_WALKER) &&
                    bu.GetEnchantments().Find(o => o.source == (Enchantment)ENCH.EARTH_TO_MUD) == null)
                {
                    if (bu.ownerID == data.GetWizardID())
                    {
                        iRetVal -= bu.GetBattleUnitValue();
                    }
                    else
                    {
                        iRetVal += bu.GetBattleUnitValue();
                    }
                }
            }

            if (bLoggingEnabled)
            {
#pragma warning disable CS0162 // Unreachable code detected

                Debug.Log(spell.dbName + " with script " +
                          spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + iRetVal);
#pragma warning restore CS0162 // Unreachable code detected
            }

            return iRetVal;
        }

        public static bool SBH_MassSlow(SpellCastData data, object target, Spell spell)
        {
            if (target == null)
            {
                Debug.LogError("  [SBH_MassSlow] - target == null");
                return false;
            }

            Vector3i       pos = Vector3i.invalid;
            BattleUnit     bu  = target as BattleUnit;
            HexCoordinates hex = target as HexCoordinates;
            if (bu != null)
            {
                Debug.LogWarningFormat("  [SBH_MassSlow] - target is BattleUnit:{0}", GetNameOwnerID(bu));
                pos = bu.battlePosition;
            }
            if (hex != null)
            {
                Debug.LogWarningFormat("  [SBH_MassSlow] - target is HexCoordinates:{0}", hex.ToString());
                pos = (Vector3i)target;
            }
            if (target is Vector3i)
            {
                pos = (Vector3i)target;
            }

            int iDistance = spell.fIntData[0].ToInt();

            if ((data.battle != null) && (pos != Vector3i.invalid))
            {
                foreach (BattleUnit battleUnit in data.battle.GetAllUnits())
                {
                    if (HexCoordinates.HexDistance(battleUnit.GetPosition(), pos) <= iDistance &&
                        battleUnit.attributes.DoesNotContains((Tag)TAG.TELEPORTING) &&
                        battleUnit.attributes.DoesNotContains((Tag)TAG.CAN_FLY) &&
                        battleUnit.attributes.DoesNotContains((Tag)TAG.NON_CORPOREAL) &&
                        battleUnit.GetSkills().Find(o => o == (Skill)SKILL.EARTH_WALKER) == null &&
                        battleUnit.GetEnchantments().Find(o => o.source == (Enchantment)ENCH.EARTH_TO_MUD) == null)
                    {
                        foreach (Enchantment en in spell.enchantmentData)
                        {
                            battleUnit.AddEnchantment(en, data.caster as Entity, en.lifeTime, null, spell.worldCost);
                        }
                    }
                }
                return true;
            }
            return false;
        }
        #endregion
    }
}

#endif