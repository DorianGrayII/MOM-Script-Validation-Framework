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
    public class SkillScripts : ScriptBase
    {
        public static void GetUnitLevelBonus(int Level, NetDictionary<DBReference<Tag>, FInt> retAttribute)
        {
            switch (Level)
            {
                case 2:
                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK_CHANCE, (FInt)0.04);
                    retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK_CHANCE, (FInt)0.04);
                    break;
                case 3:
                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK_CHANCE, (FInt)0.08);
                    retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK_CHANCE, (FInt)0.08);

                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK, FInt.ONE);
                    retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK, FInt.ONE);
                    retAttribute.AddFinal((Tag)TAG.FIRE_BREATH_BONUS, FInt.ONE);
                    retAttribute.AddFinal((Tag)TAG.THROW_BONUS, FInt.ONE);
                    break;
                case 4:
                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK_CHANCE, (FInt)0.12);
                    retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK_CHANCE, (FInt)0.12);

                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK, FInt.ONE);
                    retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK, FInt.ONE);
                    retAttribute.AddFinal((Tag)TAG.FIRE_BREATH_BONUS, FInt.ONE);
                    retAttribute.AddFinal((Tag)TAG.THROW_BONUS, FInt.ONE);
                    retAttribute.AddFinal((Tag)TAG.RESIST, FInt.ONE);

                    retAttribute.AddFinal((Tag)TAG.DEFENCE, FInt.ONE);
                    break;
                case 5:
                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK_CHANCE, (FInt)0.14);
                    retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK_CHANCE, (FInt)0.14);

                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK, FInt.ONE);
                    retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK, FInt.ONE);
                    retAttribute.AddFinal((Tag)TAG.FIRE_BREATH_BONUS, FInt.ONE);
                    retAttribute.AddFinal((Tag)TAG.THROW_BONUS, FInt.ONE);
                    retAttribute.AddFinal((Tag)TAG.RESIST, FInt.ONE);

                    retAttribute.AddFinal((Tag)TAG.DEFENCE, FInt.ONE);
                    retAttribute.AddFinal((Tag)TAG.HIT_POINTS, FInt.ONE);
                    break;
                case 6:
                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK_CHANCE, (FInt)0.16);
                    retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK_CHANCE, (FInt)0.16);

                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK, (FInt)2.0);
                    retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK, (FInt)2.0);
                    retAttribute.AddFinal((Tag)TAG.FIRE_BREATH_BONUS, (FInt)2.0);
                    retAttribute.AddFinal((Tag)TAG.THROW_BONUS, (FInt)2.0);
                    retAttribute.AddFinal((Tag)TAG.RESIST, FInt.ONE);

                    retAttribute.AddFinal((Tag)TAG.DEFENCE, FInt.ONE);
                    retAttribute.AddFinal((Tag)TAG.HIT_POINTS, FInt.ONE);
                    break;
                case 7:
                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK_CHANCE, (FInt)0.18);
                    retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK_CHANCE, (FInt)0.18);

                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK, (FInt)2.0);
                    retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK, (FInt)2.0);
                    retAttribute.AddFinal((Tag)TAG.FIRE_BREATH_BONUS, (FInt)2.0);
                    retAttribute.AddFinal((Tag)TAG.THROW_BONUS, (FInt)2.0);
                    retAttribute.AddFinal((Tag)TAG.RESIST, (FInt)2.0);

                    retAttribute.AddFinal((Tag)TAG.DEFENCE, FInt.ONE);
                    retAttribute.AddFinal((Tag)TAG.HIT_POINTS, FInt.ONE);
                    break;
                case 8:
                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK_CHANCE, (FInt)0.2);
                    retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK_CHANCE, (FInt)0.2);

                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK, (FInt)2.0);
                    retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK, (FInt)2.0);
                    retAttribute.AddFinal((Tag)TAG.FIRE_BREATH_BONUS, (FInt)2.0);
                    retAttribute.AddFinal((Tag)TAG.THROW_BONUS, (FInt)2.0);
                    retAttribute.AddFinal((Tag)TAG.RESIST, (FInt)2.0);

                    retAttribute.AddFinal((Tag)TAG.DEFENCE, FInt.ONE);
                    retAttribute.AddFinal((Tag)TAG.HIT_POINTS, FInt.ONE);
                    break;
                case 9:
                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK_CHANCE, (FInt)0.22);
                    retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK_CHANCE, (FInt)0.22);

                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK, (FInt)3.0);
                    retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK, (FInt)3.0);
                    retAttribute.AddFinal((Tag)TAG.FIRE_BREATH_BONUS, (FInt)3.0);
                    retAttribute.AddFinal((Tag)TAG.THROW_BONUS, (FInt)3.0);
                    retAttribute.AddFinal((Tag)TAG.RESIST, (FInt)2.0);

                    retAttribute.AddFinal((Tag)TAG.DEFENCE, (FInt)2.0);
                    retAttribute.AddFinal((Tag)TAG.HIT_POINTS, FInt.ONE);
                    break;
                case 10:
                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK_CHANCE, (FInt)0.24);
                    retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK_CHANCE, (FInt)0.24);

                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK, (FInt)3.0);
                    retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK, (FInt)3.0);
                    retAttribute.AddFinal((Tag)TAG.FIRE_BREATH_BONUS, (FInt)3.0);
                    retAttribute.AddFinal((Tag)TAG.THROW_BONUS, (FInt)3.0);
                    retAttribute.AddFinal((Tag)TAG.RESIST, (FInt)3.0);

                    retAttribute.AddFinal((Tag)TAG.DEFENCE, (FInt)2.0);
                    retAttribute.AddFinal((Tag)TAG.HIT_POINTS, (FInt)2.0);
                    break;
                case 11:
                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK_CHANCE, (FInt)0.26);
                    retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK_CHANCE, (FInt)0.26);

                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK, (FInt)3.0);
                    retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK, (FInt)3.0);
                    retAttribute.AddFinal((Tag)TAG.FIRE_BREATH_BONUS, (FInt)3.0);
                    retAttribute.AddFinal((Tag)TAG.THROW_BONUS, (FInt)3.0);
                    retAttribute.AddFinal((Tag)TAG.RESIST, (FInt)3.0);

                    retAttribute.AddFinal((Tag)TAG.DEFENCE, (FInt)2.0);
                    retAttribute.AddFinal((Tag)TAG.HIT_POINTS, (FInt)2.0);
                    break;
                case 12:
                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK_CHANCE, (FInt)0.28);
                    retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK_CHANCE, (FInt)0.28);

                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK, (FInt)4.0);
                    retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK, (FInt)4.0);
                    retAttribute.AddFinal((Tag)TAG.FIRE_BREATH_BONUS, (FInt)4.0);
                    retAttribute.AddFinal((Tag)TAG.THROW_BONUS, (FInt)4.0);
                    retAttribute.AddFinal((Tag)TAG.RESIST, (FInt)3.0);

                    retAttribute.AddFinal((Tag)TAG.DEFENCE, (FInt)2.0);
                    retAttribute.AddFinal((Tag)TAG.HIT_POINTS, (FInt)2.0);
                    break;
                case 13:
                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK_CHANCE, (FInt)0.3);
                    retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK_CHANCE, (FInt)0.3);

                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK, (FInt)4.0);
                    retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK, (FInt)4.0);
                    retAttribute.AddFinal((Tag)TAG.FIRE_BREATH_BONUS, (FInt)4.0);
                    retAttribute.AddFinal((Tag)TAG.THROW_BONUS, (FInt)4.0);
                    retAttribute.AddFinal((Tag)TAG.RESIST, (FInt)4.0);

                    retAttribute.AddFinal((Tag)TAG.DEFENCE, (FInt)2.0);
                    retAttribute.AddFinal((Tag)TAG.HIT_POINTS, (FInt)2.0);
                    break;
                case 14:
                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK_CHANCE, (FInt)0.32);
                    retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK_CHANCE, (FInt)0.32);

                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK, (FInt)4.0);
                    retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK, (FInt)4.0);
                    retAttribute.AddFinal((Tag)TAG.FIRE_BREATH_BONUS, (FInt)4.0);
                    retAttribute.AddFinal((Tag)TAG.THROW_BONUS, (FInt)4.0);
                    retAttribute.AddFinal((Tag)TAG.RESIST, (FInt)4.0);

                    retAttribute.AddFinal((Tag)TAG.DEFENCE, (FInt)3.0);
                    retAttribute.AddFinal((Tag)TAG.HIT_POINTS, (FInt)2.0);
                    break;
                case 15:
                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK_CHANCE, (FInt)0.34);
                    retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK_CHANCE, (FInt)0.34);

                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK, (FInt)5.0);
                    retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK, (FInt)5.0);
                    retAttribute.AddFinal((Tag)TAG.FIRE_BREATH_BONUS, (FInt)5.0);
                    retAttribute.AddFinal((Tag)TAG.THROW_BONUS, (FInt)5.0);
                    retAttribute.AddFinal((Tag)TAG.RESIST, (FInt)4.0);

                    retAttribute.AddFinal((Tag)TAG.DEFENCE, (FInt)3.0);
                    retAttribute.AddFinal((Tag)TAG.HIT_POINTS, (FInt)3.0);
                    break;
                case 16:
                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK_CHANCE, (FInt)0.36);
                    retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK_CHANCE, (FInt)0.36);

                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK, (FInt)5.0);
                    retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK, (FInt)5.0);
                    retAttribute.AddFinal((Tag)TAG.FIRE_BREATH_BONUS, (FInt)5.0);
                    retAttribute.AddFinal((Tag)TAG.THROW_BONUS, (FInt)5.0);
                    retAttribute.AddFinal((Tag)TAG.RESIST, (FInt)5.0);

                    retAttribute.AddFinal((Tag)TAG.DEFENCE, (FInt)3.0);
                    retAttribute.AddFinal((Tag)TAG.HIT_POINTS, (FInt)3.0);
                    break;
                case 17:
                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK_CHANCE, (FInt)0.38);
                    retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK_CHANCE, (FInt)0.38);

                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK, (FInt)5.0);
                    retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK, (FInt)5.0);
                    retAttribute.AddFinal((Tag)TAG.FIRE_BREATH_BONUS, (FInt)5.0);
                    retAttribute.AddFinal((Tag)TAG.THROW_BONUS, (FInt)5.0);
                    retAttribute.AddFinal((Tag)TAG.RESIST, (FInt)5.0);

                    retAttribute.AddFinal((Tag)TAG.DEFENCE, (FInt)3.0);
                    retAttribute.AddFinal((Tag)TAG.HIT_POINTS, (FInt)3.0);
                    break;
                case 18:
                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK_CHANCE, (FInt)0.4);
                    retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK_CHANCE, (FInt)0.4);

                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK, (FInt)6.0);
                    retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK, (FInt)6.0);
                    retAttribute.AddFinal((Tag)TAG.FIRE_BREATH_BONUS, (FInt)6.0);
                    retAttribute.AddFinal((Tag)TAG.THROW_BONUS, (FInt)6.0);
                    retAttribute.AddFinal((Tag)TAG.RESIST, (FInt)5.0);

                    retAttribute.AddFinal((Tag)TAG.DEFENCE, (FInt)3.0);
                    retAttribute.AddFinal((Tag)TAG.HIT_POINTS, (FInt)3.0);
                    break;
                case 19:
                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK_CHANCE, (FInt)0.4);
                    retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK_CHANCE, (FInt)0.4);

                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK, (FInt)6.0);
                    retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK, (FInt)6.0);
                    retAttribute.AddFinal((Tag)TAG.FIRE_BREATH_BONUS, (FInt)6.0);
                    retAttribute.AddFinal((Tag)TAG.THROW_BONUS, (FInt)6.0);
                    retAttribute.AddFinal((Tag)TAG.RESIST, (FInt)6.0);

                    retAttribute.AddFinal((Tag)TAG.DEFENCE, (FInt)4.0);
                    retAttribute.AddFinal((Tag)TAG.HIT_POINTS, (FInt)3.0);
                    break;
                case 20:
                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK_CHANCE, (FInt)0.4);
                    retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK_CHANCE, (FInt)0.4);

                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK, (FInt)6.0);
                    retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK, (FInt)6.0);
                    retAttribute.AddFinal((Tag)TAG.FIRE_BREATH_BONUS, (FInt)6.0);
                    retAttribute.AddFinal((Tag)TAG.THROW_BONUS, (FInt)6.0);
                    retAttribute.AddFinal((Tag)TAG.RESIST, (FInt)6.0);

                    retAttribute.AddFinal((Tag)TAG.DEFENCE, (FInt)4.0);
                    retAttribute.AddFinal((Tag)TAG.HIT_POINTS, (FInt)4.0);
                    break;

                default:
                    break;
            }
        }

        public static object ACTPass_LevelBonus(ISkillable source, Skill skill, SkillScript skillScript, NetDictionary<DBReference<Tag>, FInt> retAttribute)
        {
            BattleUnit bu = source as BattleUnit;

            int level = bu.GetLevel();

            if (level < 2)
            {
                return null;
            }

            if (bu.dbSource.Get() is Hero)
            {
                FInt increse = new FInt(level - 1);
                retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK, increse);
                retAttribute.AddFinal((Tag)TAG.THROW_BONUS, increse);
                retAttribute.AddFinal((Tag)TAG.FIRE_BREATH_BONUS, increse);
                retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK, increse);
                retAttribute.AddFinal((Tag)TAG.RESIST, increse);
                retAttribute.AddFinal((Tag)TAG.HIT_POINTS, increse);

                increse = new FInt(level / 2);
                retAttribute.AddFinal((Tag)TAG.DEFENCE, increse);

                increse = new FInt(level / 3);
                retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK_CHANCE, increse);
                retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK_CHANCE, increse);
            }
            else
            {
                GetUnitLevelBonus(level, retAttribute);
            }
            return null;
        }

        public static object ACTPass_UnitLevelUp(ISkillable source, Skill _1, SkillScript _2, NetDictionary<DBReference<Tag>, FInt> retAttribute)
        {
            if (!(source is MOM.BaseUnit))
            {
                return null;
            }

            BaseUnit unit = source as MOM.BaseUnit;

            GetUnitLevelBonus(unit.GetLevel(), retAttribute);
            return null;
        }
    }
}

#endif