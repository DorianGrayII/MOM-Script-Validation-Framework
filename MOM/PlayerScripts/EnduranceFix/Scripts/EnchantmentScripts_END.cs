/**********************************
 *
 * Author:  Dorian Gray
 * Date:    Feb 19, 2024
 * Version: 1.0.4
 *
 **********************************/

#if !USE_DEBUG_SCRIPT || !UNITY_EDITOR

using System;
using DBDef;
using DBEnum;
using MHUtils;
using MOM;
using UnityEngine;


namespace MOMScripts_END
{
    using static UserUtility_END.Utility;

    public class EnchantmentScripts : ScriptBase
    {
        /// <summary>
        /// enables verbose counter magic logging
        /// </summary>
        const bool bLoggingEnabled = true;


        public static void ECH_AddTag(IEnchantable target, EnchantmentScript es, EnchantmentInstance ei, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            if (bLoggingEnabled && !IsSimulated(target))
            {
                Debug.LogFormat("  invoking ECH_AddTag:{0} es:{1} ei:{2} ... ",
                                 GetNameOwnerID(target), (es != null) ? es.script : "{null}", (ei != null) ? ei.nameID : "{null}");

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


        /// <summary>
        /// EnchantmentScript TriggerType="AttributeChange" Script="ECH_AddEndurance" FIntData="1" StringData="TAG-MOVEMENT_POINTS"
        /// </summary>
        /// <param name="target"></param>
        /// <param name="es"></param>
        /// <param name="ei"></param>
        /// <param name="ret">Changes to the target are made here</param>
        public static void ECH_AddEndurance(IEnchantable target, EnchantmentScript es, EnchantmentInstance ei, NetDictionary<DBReference<Tag>, FInt> ret)
        {
            bool bSimulated = IsSimulated(target);
            if (bLoggingEnabled && !bSimulated)
            {
                Debug.LogFormat("  invoking ECH_AddEndurance:{0} es:{1} ei:{2} ... ", 
                                 GetNameOwnerID(target), es != null ? es.script : "{null}", (ei != null) ? ei.nameID : "{null}");
            }

            if (es != null)
            {
                // following should be TAG-MOVEMENT_POINTS
                Tag tag = (Tag)DataBase.Get(es.stringData, false);
                if (tag != null)
                {
                    // following should be '1'
                    int defMod = es.fIntData.ToInt();
                    ret.AddFinal(tag, defMod);

                    // Get current value for TAG.ENGINEER_UNIT
                    FInt fEngVal = ret.GetFinal((Tag)TAG.ENGINEER_UNIT);

                    // if Engineer, this value will be >= 1
                    if (fEngVal >= 1)
                    {
                        // double the engineering construction rate
                        ret.AddFinal((Tag)TAG.ENGINEER_UNIT, fEngVal);

                        if (bLoggingEnabled && !bSimulated)
                        {
                            // verify the value is being set
                            FInt fNewEngVal = ret.GetFinal((Tag)TAG.ENGINEER_UNIT);
                            Debug.LogFormat("    eng val:{0}->{1}", fEngVal.ToInt(), fNewEngVal.ToInt());
                        }
                    }
                }
                else
                {
                    Debug.LogWarning(es.stringData + " DB file missing valid Tag. You have a typo?");
                }
            }
        }

        /// <summary>
        /// EnchantmentApplicationScript TriggerType="None" Script="EAPP_Endurance"
        /// </summary>
        /// <param name="target"></param>
        /// <param name="e"></param>
        /// <param name="ei"></param>
        public static void EAPP_Endurance(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            MOM.Unit unit = target as MOM.Unit;
            if (unit != null)
            {
                if (bLoggingEnabled && !unit.simulationUnit)
                {
                    Debug.LogFormat("  invoking EAPP_Endurance:{0} e:{1} ei:{2} ... ", 
                                    GetNameOwnerID(target), GetName(e), GetNameOwnerID(ei) );
                }

                unit.GetAttributes().SetDirty();
                unit.group?.Get().UpdateMovementFlags();
            }

            BattleUnit bu = target as BattleUnit;
            if (bu != null)
            {
                if (bLoggingEnabled && !bu.simulated)
                {
                    Debug.LogFormat("  invoking EAPP_Endurance:{0} e:{1} ei:{2} ... ", 
                                    GetNameOwnerID(target), GetName(e), GetNameOwnerID(ei) );
                }

                bu.GetAttributes().SetDirty();
                if (bu.canMove == false)
                {
                    bu.Mp = FInt.ZERO;
                }

                if (BattleHUD.GetSelectedUnit() == bu &&
                    IsOwnerHuman(bu))
                {
                    MHEventSystem.TriggerEvent<BattleHUD>(BattleHUD.Get(), null);
                }
            }
        }

        /// <summary>
        /// EnchantmentRemovalScript TriggerType="None" Script="EREM_Endurance"
        /// </summary>
        /// <param name="target"></param>
        /// <param name="e"></param>
        /// <param name="ei"></param>
        public static void EREM_Endurance(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            _ = e;
            _ = ei;

            MOM.Unit u = target as MOM.Unit;
            if (u != null)
            {
                bool bSimulated = u.simulationUnit;
                if (bLoggingEnabled && !bSimulated)
                {
                    Debug.LogFormat("  invoking EREM_Endurance Target:{0} ... ", 
                                        GetNameOwnerID(target));
                }

                u.GetAttributes().SetDirty();
                u.group?.Get().UpdateMovementFlags();
                if (FSMSelectionManager.Get().GetSelectedGroup() == u.group?.Get() &&
                    IsOwnerHuman(u))
                {
                    MHEventSystem.TriggerEvent<HUD>(HUD.Get(), null);
                }
            }

            BattleUnit bu = target as BattleUnit;
            if (bu != null)
            {
                bool bSimulated = bu.simulated;
                if (bLoggingEnabled && !bSimulated)
                {
                    Debug.LogFormat("  invoking EREM_Endurance Target:{0} ... ", 
                                     GetNameOwnerID(target));
                }

                bu.GetAttributes().SetDirty();

                if (BattleHUD.GetSelectedUnit() == bu && 
                    IsOwnerHuman(bu))
                {
                    MHEventSystem.TriggerEvent<BattleHUD>(BattleHUD.Get(), null);
                }
            }
        }
    }
}
#endif