/**********************************
 *
 * Author:  Dorian Gray
 * Date:    May 10, 2024
 * Version: 1.0.3
 *
 **********************************/

#if !USE_DEBUG_SCRIPT || !UNITY_EDITOR

using DBDef;
using MHUtils;
using MOM;
using System;
using System.Collections.Generic;
using UnityEngine;
using WorldCode;

namespace MOMScripts_COTW
{
    public class SpellScripts : ScriptBase
    {

        public static bool SBH_RandomBattleSummon(SpellCastData data, object target, Spell spell)
        {
            //summon chosen creature on v3i position
            if (spell == null || spell.stringData == null || spell.stringData.Length < 1)
            {
                Debug.LogError("Spell " + spell.dbName + " missing spell or script stringData");
                return false;
            }

            List<DBReference<DBDef.Unit>> units = new List<DBReference<DBDef.Unit>>();

            foreach (string unitName in spell.stringData)
            {
                units.Add(DataBase.Get<DBDef.Unit>(unitName, true));
            }

            if (units.Count <= 0)
            {
                Debug.LogError("Unit " + spell.stringData[0] + " not found in database");
                return false;
            }

            units.RandomSort();
            // just grab the 1st one
            DBDef.Unit unit = units[0].Get();

            //             if (!(target is Vector3i))
            //             {
            //                 Debug.LogError("Target is not a location");
            //                 return false;
            //             }
            Vector3i pos = (Vector3i)target;
            //             if (!data.battle.IsLocationEmpty(pos))
            //             {
            //                 Debug.LogError("Target location occupied");
            //                 return false;
            //             }

            if (data.battle != null)
            {
                data.battle.CreateSummon(data.GetWizardID(), unit, pos);
            }
            else
            {
                data.CreateSummon(data.GetWizardID(), unit);
            }

            return true;
        }

    }
}

#endif