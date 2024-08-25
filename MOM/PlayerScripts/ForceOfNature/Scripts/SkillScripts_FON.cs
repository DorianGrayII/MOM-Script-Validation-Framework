/**********************************
 *
 * Author:  Dorian Gray
 * Date:    Feb 23, 2024
 * Version: 1.0.1
 *
 **********************************/

#if !USE_DEBUG_SCRIPT || !UNITY_EDITOR

using DBDef;
using DBEnum;
using MHUtils;
using MOM;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MOMScripts_FON
{
    public class SkillScripts : ScriptBase
    {
        #region Passive activators (to be used in attribute processing)

        /// <param name="source"> unit producing damage/attack </param>
        /// <param name="skill"> skill used to attack </param>
        /// <param name="skillScript"> skillScript used </param>
        /// <param name="retAttribute"> dictionary of the curent attribute library </param>
        /// <returns> change data </returns>
        public static object ACTPass_ForceOfNature1(ISkillable source, Skill skill, SkillScript skillScript, NetDictionary<DBReference<Tag>, FInt> retAttribute)
        {
            // Debug.Log("ACTPass_ForceOfNature1");
            BaseUnit unit = source as BaseUnit;
            unit.race = (Race)RACE.REALM_NATURE;

            retAttribute.AddFinal((Tag)TAG.DEFENCE, 3);

            return null;
        }

        public static object ACTPass_ForceOfNature2(ISkillable source, Skill skill, SkillScript skillScript, NetDictionary<DBReference<Tag>, FInt> retAttribute)
        {
//            Debug.Log("ACTPass_ForceOfNature2");
            BaseUnit unit = source as BaseUnit;
            unit.race = (Race)RACE.REALM_NATURE;
            retAttribute.AddFinal((Tag)TAG.PATHFINDING, FInt.ONE);

            return null;
        }

        public static object ACTPass_ForceOfNature3(ISkillable source, Skill skill, SkillScript skillScript, NetDictionary<DBReference<Tag>, FInt> retAttribute)
        {
            // Debug.Log("ACTPass_ForceOfNature3");
            BaseUnit unit = source as BaseUnit;
            unit.race = (Race)RACE.REALM_NATURE;

            Tag tag = (Tag)"TAG-POISON_TOUCH";
            int Poison = retAttribute.GetFinal(tag).ToInt();

            switch (Poison)
            {
                case 0 :
                    retAttribute.AddFinal(tag, 4);
                    break;
                case 1:
                    retAttribute.AddFinal(tag, 3);
                    break;
                case 2 :
                    retAttribute.AddFinal(tag, 2);
                    break;
                case 3:
                    retAttribute.AddFinal(tag, 1);
                    break;

                default:
                    break;
            }
            return null;
        }

        #endregion

    }
}
#endif

