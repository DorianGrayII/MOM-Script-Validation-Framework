/**********************************
 *
 * Author:  Dorian Gray
 * Date:    May 15, 2024
 * Version: 1.0.5
 *
 **********************************/

using DBDef;
using DBEnum;
using MHUtils;
using MOM;
using System;
using System.Collections;
using System.Collections.Generic;

namespace MOMScripts_IUX
{
    public class EnchantmentScripts : ScriptBase
    {
        #region Crusade
        public static void EAPP_Crusade(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            MOM.PlayerWizard playerWizard = target as MOM.PlayerWizard;
            if (playerWizard != null)
            {
                playerWizard.unitLevelIncrease += 2;

                List<MOM.Group> groupList = GameManager.GetGroupsOfWizard(playerWizard.GetID());
                foreach (MOM.Group group in groupList)
                {
                    foreach (Reference<MOM.Unit> refUnit in group.GetUnits())
                    {
                        refUnit.Get().GetAttributes().SetDirty();
                        refUnit.Get().group?.Get()?.TriggerOnJoinScripts(refUnit);
                    }
                }
            }
        }

        public static void EREM_Crusade(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            MOM.PlayerWizard playerWizard = target as MOM.PlayerWizard;
            if (playerWizard != null)
            {
                playerWizard.unitLevelIncrease -= 2;

                List<MOM.Group> groupList = GameManager.GetGroupsOfWizard(playerWizard.GetID());
                foreach (MOM.Group group in groupList)
                {
                    foreach (Reference<MOM.Unit> refUnit in group.GetUnits())
                    {
                        refUnit.Get().GetAttributes().SetDirty();
                        refUnit.Get().group?.Get()?.TriggerOnJoinScripts(refUnit);
                    }
                }
            }
        }

        #endregion

        #region Heroism

        public static void ECH_Heroism(IEnchantable target, EnchantmentScript es, EnchantmentInstance ei, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            MOM.Unit unit = target as MOM.Unit;
            if (unit != null)
            {
                // use XP for Elite(Lvl 10)

                if (unit.xp >= 600)
                {
                    ei.countDown = 0;
                }
            }
        }

        public static void EAPP_Heroism(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            BaseUnit bu = target as BaseUnit;
            if (bu != null)
            {
                bu.levelOverride = 10;
                bu.GetAttributes().SetDirty();
            }
        }

        #endregion

        public static void ECH_MaxXP(IEnchantable target, EnchantmentScript es, EnchantmentInstance ei, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            BattleUnit bu = target as BattleUnit;
            if (bu != null)
            {
                //LevelOverride cannot be +=, ench is triggered more then one time
                bu.levelOverride = 10;

                // following appears to be a bug and is changed
                // bu.GetAttributes().GetDirty();
                bu.GetAttributes().SetDirty();
            }
        }
    }
}