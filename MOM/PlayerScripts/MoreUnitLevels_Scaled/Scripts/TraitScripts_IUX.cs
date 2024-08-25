/**********************************
 *
 * Author:  Dorian Gray
 * Date:    May 1, 2024
 * Version: 1.0.2
 *
 **********************************/

#if !USE_DEBUG_SCRIPT || !UNITY_EDITOR

using DBDef;
using DBEnum;
using MHUtils;
using MOM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Group = MOM.Group;

namespace MOMScripts_IUX
{
    public class TraitScripts : ScriptBase
    {
        public static void TINIT_Warlord(PlayerWizard w)
        {
            w.unitLevelIncrease += 2;
            w.AddEnchantment((Enchantment)ENCH.WARLORD, w as IEnchantable);
        }
    }

}
#endif