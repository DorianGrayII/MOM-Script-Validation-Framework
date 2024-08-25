/**********************************
 *
 * Author:  Dorian Gray
 * Date:    May 15, 2024
 * Version: 1.0.1
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

namespace GameScript_IUX
{
    using static GameScript.SpellScripts;
    public class SpellScripts : ScriptBase
    {
        public static bool STAR_Heroism(SpellCastData data, object target, Spell spell)
        {
            if (STAR_FriendlyNonFantastic_Unit(data, target, spell))
            {
                BaseUnit bu = target as MOM.BaseUnit;
                // use XP for Elite(Lvl 10)
                return bu.xp < 600;
            }
            else
            {
                return false;
            }
        }

        public static int SBAI_Heroism(SpellCastData data, object target, Spell spell)
        {
            int iRetVal = 0;
            BattleUnit bu = target as BattleUnit;
            if (bu == null)
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return iRetVal;
            }

            int iUnitValue = bu.GetBattleUnitValue();
            int iOrgLevelOverride = bu.levelOverride;
            bu.levelOverride = 10;
            bu.GetAttributes().SetDirty();
            int iUnitMaxLevelValue = bu.GetBattleUnitValue();

            // restore previous Level Override
            bu.levelOverride = iOrgLevelOverride;
            bu.GetAttributes().SetDirty();

            //Average spell value based on target
            iRetVal = iUnitMaxLevelValue - iUnitValue;

            return iRetVal;
        }

        public static int SWAI_Heroism(ISpellCaster source, object target, Spell spell)
        {
            int iRetVal = 0;
            MOM.Unit unit = target as MOM.Unit;
            if (unit == null)
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return iRetVal;
            }

            int iUnitValue = unit.GetWorldUnitValue();
            int iOrgLevelOverride = unit.levelOverride;
            unit.levelOverride = 10;
            unit.GetAttributes().SetDirty();
            int iUnitValueWithHeroism = unit.GetWorldUnitValue();

            // restore previous Level Override
            unit.levelOverride = iOrgLevelOverride;
            unit.GetAttributes().SetDirty();

            //Average spell value based on target
            iRetVal = iUnitValueWithHeroism - iUnitValue;

            return iRetVal;
        }
    }
}

#endif