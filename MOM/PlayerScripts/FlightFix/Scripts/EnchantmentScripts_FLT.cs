/**********************************
 *
 * Author:  Dorian Gray
 * Date:    May 15, 2024
 * Version: 1.0.0
 *
 **********************************/

#if !USE_DEBUG_SCRIPT || !UNITY_EDITOR

using System;
using DBDef;
using DBEnum;
using MHUtils;
using MOM;
using UnityEngine;


namespace MOMScripts_FLT
{
    using static UserUtility.Utility;

    public class EnchantmentScripts : ScriptBase
    {
        /// <summary>
        /// enables verbose logging
        /// </summary>
        private const bool bLoggingEnabled = false;


        public static void ECH_AddTag(IEnchantable target, EnchantmentScript es, EnchantmentInstance ei, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            bool bSimulated = IsSimulated(target);
            if (bLoggingEnabled && !bSimulated)
            {
                Debug.LogFormat("  invoking MOMScripts_FLT::ECH_AddTag:{0} es:{1} ei:{2} ... ",
                                 GetNameOwnerID(target), (es != null) ? es.script : "{null}", (ei != null) ? ei.nameID : "{null}");
      // Following is for detailed debugging ...
      //          Debug.LogFormat("StackTrace: '{0}'", Environment.StackTrace);
            }

            Tag tag = (Tag)DataBase.Get(es.stringData, false);
            if (tag != null)
            {
                int defMod = es.fIntData.ToInt();
                ret.AddFinal(tag, defMod);
            }
            else
            {
                Debug.LogWarning(es.stringData + " is not a Tag. You have a typo?");
            }
        }

        static public void ECH_Flight(IEnchantable target, EnchantmentScript es, EnchantmentInstance ei, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            bool bSimulated = IsSimulated(target);
            if (bLoggingEnabled && !bSimulated)
            {
                Debug.LogFormat("  invoking ECH_Flight:{0} es:{1} ei:{2} ... ", 
                                 GetNameOwnerID(target), es != null ? es.script : "{null}", (ei != null) ? ei.nameID : "{null}");
            }
            ret.AddFinal((Tag)TAG.CAN_FLY, 1);
            ret.AddFinal((Tag)TAG.SIGHT_RANGE_BONUS, 1);

            /*
            if (ret.GetFinal((Tag)TAG.MOVEMENT_POINTS) <= 2)
            {
                ret.AddFinal((Tag)TAG.MOVEMENT_POINTS, (FInt)(1.0));
            }
            */

            if (ret.GetFinal((Tag)TAG.MOVEMENT_POINTS) <= 3)
            {
                ret.SetFinal((Tag)TAG.MOVEMENT_POINTS, (FInt)4);
            }
        }
    }
}
#endif