/**********************************
 *
 * Author:  Dorian Gray
 * Date:    Feb 23, 2024
 * Version: 1.0.0
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


namespace GameScript_ETM
{
    using static UserUtility_ETM.Utility;

    public class SpellScripts : ScriptBase
    {
        /// <summary>
        /// enables verbose counter magic logging
        /// </summary>
        const bool bLoggingEnabled = false;

        #region EarthToMud
        public static int SBAI_EarthToMud(SpellCastData data, object target, Spell spell)
        {
            if (target == null)
            {
                Debug.LogError("  [SBAI_EarthToMud] - target == null");
                return 0;
            }

            BattleUnit     bu   = target as BattleUnit;

            if (bu != null)
            {
                Debug.LogWarningFormat("  [SBAI_EarthToMud] - target is BattleUnit:{0}", GetNameOwnerID(bu));
            }

            FInt distance = spell.fIntData[0];
            int value = 0;

            Vector3i pos = (Vector3i)target;
            Battle battle = data.battle;

            foreach (BattleUnit unit in battle.GetAllUnits())
            {
                if (HexCoordinates.HexDistance(unit.GetPosition(), pos) < distance &&
                    unit.attributes.DoesNotContains((Tag)TAG.TELEPORTING) &&
                    unit.attributes.DoesNotContains((Tag)TAG.CAN_FLY) &&
                    unit.attributes.DoesNotContains((Tag)TAG.NON_CORPOREAL) &&
                    unit.attributes.DoesNotContains((Tag)TAG.EARTH_WALKER) &&
                    unit.GetEnchantments().Find(o => o.source == (Enchantment)ENCH.EARTH_TO_MUD) == null)
                {
                    if (unit.ownerID == data.GetWizardID())
                    {
                        value -= unit.GetBattleUnitValue();
                    }
                    else
                    {
                        value += unit.GetBattleUnitValue();
                    }
                }
            }
#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " SBAI_ script value " + (int)value);
#endif
            if (bLoggingEnabled)
            {
#pragma warning disable CS0162 // Unreachable code detected
                Debug.LogFormat("  [SBAI_EarthToMud]  Returning:{0}", value);
#pragma warning restore CS0162 // Unreachable code detected
            }

            return value;
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

            FInt distance = spell.fIntData[0];

            if ((data.battle != null) && (pos != Vector3i.invalid))
            {
                foreach (BattleUnit v in data.battle.GetAllUnits())
                {
                    if (HexCoordinates.HexDistance(v.GetPosition(), pos) <= distance &&
                        v.attributes.DoesNotContains((Tag)TAG.TELEPORTING) &&
                        v.attributes.DoesNotContains((Tag)TAG.CAN_FLY) &&
                        v.attributes.DoesNotContains((Tag)TAG.NON_CORPOREAL) &&
                        v.GetSkills().Find(o => o == (Skill)SKILL.EARTH_WALKER) == null &&
                        v.GetEnchantments().Find(o => o.source == (Enchantment)ENCH.EARTH_TO_MUD) == null)
                    {
                        foreach (Enchantment en in spell.enchantmentData)
                        {
                            v.AddEnchantment(en, data.caster as Entity, en.lifeTime, null, spell.worldCost);
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