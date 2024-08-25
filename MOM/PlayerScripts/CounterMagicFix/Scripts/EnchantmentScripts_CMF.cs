/**********************************
 *
 * Author:  Dorian Gray
 * Date:    Feb 23, 2024
 * Version: 1.0.4
 *
 **********************************/

#if !USE_DEBUG_SCRIPT || !UNITY_EDITOR

using System;
using System.Collections;
using DBDef;
using DBEnum;
using MHUtils;
using MOM;
using UnityEngine;
using WorldCode;
using System.Collections.Generic;



namespace MOMScripts_CMF
{
    using static UserUtility_CMF.Utility;

    public class EnchantmentScripts : ScriptBase
    {
        #region Counter Magic Pool
        public enum CM_TYPE { CM_HUMAN, CM_AI };

        private static int[] rgCounterMagicPool = new int[2];

        /// <summary>
        /// enables verbose counter magic logging
        /// </summary>
        private const bool bLoggingEnabled = true;

        public static int DecrCounterMagicPool(int iVal, CM_TYPE cmWhich)
        {
            rgCounterMagicPool[(int)cmWhich] -= iVal;
            if (rgCounterMagicPool[(int)cmWhich] < 0)
            {
                rgCounterMagicPool[(int)cmWhich] = 0;
            }

            return rgCounterMagicPool[(int)cmWhich];
        }

        public static int IncrCounterMagicPool(int iVal, CM_TYPE cmWhich)
        {
            rgCounterMagicPool[(int)cmWhich] += iVal;

            return rgCounterMagicPool[(int)cmWhich];
        }

        public static int GetCounterMagicPool(CM_TYPE cmWhich)
        {
            return rgCounterMagicPool[(int)cmWhich];
        }

        public static int SetCounterMagicPool(int iVal, CM_TYPE cmWhich)
        {
            rgCounterMagicPool[(int)cmWhich] = iVal;

            return rgCounterMagicPool[(int)cmWhich];
        }

        #endregion

        /// <summary>
        /// Is called when Counter Magic is being added to target
        /// </summary>
        /// <param name="target"></param>
        /// <param name="e"></param>
        /// <param name="ei"></param>
        public static void EAPP_AddCounterMagic(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            PlayerWizard wizard = GetOwnerAsPlayerWizard(ei);

            bool bSimulated = IsSimulated(target);

            if (bLoggingEnabled && !bSimulated)
            {
                string strCaster   = string.Empty;
                strCaster = wizard != null ? GetNameID(wizard) : GetOwnerAsBattleUnitNameID(ei);

                string strTarget   = GetNameOwnerID(target);
                string strEnchName = GetEnchantmentName(e, ei.dispelCost);
                
                Debug.LogFormat("  invoking [EAPP_AddCounterMagic] e:{0} castBy:{1} Target:{2} ...",
                                   strEnchName, strCaster, strTarget);
            }

            if (target is GameManager)
            {
                GameManager gameManager = GameManager.Get();

                if (gameManager == null)
                {
                    Debug.LogError("    [EAPP_AddCounterMagic] - gameManager == null");
                    return;
                }

                gameManager.worldCounterMagic++;
            }

            Battle battle = target as Battle;
            if (battle != null)
            {
                battle.battleCounterMagic++;
            }
            else
            {
                Debug.LogError("    [EAPP_AddCounterMagic] - target as Battle == null");
                return;
            }

            string strCombatLog = string.Empty;

            if (wizard != null)
            {
                if (IsHuman(wizard))
                {
                    int iOldVal = GetCounterMagicPool(CM_TYPE.CM_HUMAN);
                    int iNewVal = IncrCounterMagicPool(ei.dispelCost, CM_TYPE.CM_HUMAN);
                    strCombatLog = string.Format("Counter Magic Pool:{0} -> {1}", iOldVal, iNewVal);
                }
                else
                {
                    int iOldVal = GetCounterMagicPool(CM_TYPE.CM_AI);
                    int iNewVal = IncrCounterMagicPool(ei.dispelCost, CM_TYPE.CM_AI);
                    strCombatLog = string.Format("AI Counter Magic Pool:{0} -> {1}", iOldVal, iNewVal);
                }
            }
            else 
            {
                // treat it as Neutral mobs....
                int iOldVal = GetCounterMagicPool(CM_TYPE.CM_AI);
                int iNewVal = IncrCounterMagicPool(ei.dispelCost, CM_TYPE.CM_AI);
                strCombatLog = string.Format("AI Counter Magic Pool:{0} -> {1}", iOldVal, iNewVal);
            }

            if (!string.IsNullOrEmpty(strCombatLog))
            {
                BattleHUD.CombatLogAdd(strCombatLog);

                if (bLoggingEnabled && !bSimulated)
                {
                    Debug.Log(strCombatLog);
                }
            }
        }

        public static void ECH_EndBattleCounterMagic(IEnchantable target, EnchantmentScript es, EnchantmentInstance ei, object data)
        {
            bool bSimulated = IsSimulated(target);

            if (bLoggingEnabled && !bSimulated)
            {
                PlayerWizard wizard = GetOwnerAsPlayerWizard(ei);
                string strCaster = string.Empty;
                strCaster = wizard != null ? GetNameID(wizard) : GetOwnerAsBattleUnitNameID(ei);

                string strTarget    = GetNameOwnerID(target);
                string strEnchName  = GetEnchantmentName(ei);

                Debug.LogFormat("  invoking [ECH_EndBattleCounterMagic] Caster:{0} Target:{1} ei:{2} ...",
                                 strCaster, strTarget, strEnchName);
            }
            // Battle has ended, so reset both Counter Magic pools.

            SetCounterMagicPool(0, CM_TYPE.CM_AI);
            SetCounterMagicPool(0, CM_TYPE.CM_HUMAN);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target">target as GameManager or Battle</param>
        /// <param name="e"></param>
        /// <param name="ei"></param>
        public static void EREM_RemoveCounterMagic2(IEnchantable target, Enchantment e, EnchantmentInstance ei)
        {
            bool bSimulated = IsSimulated(target);

            if (bLoggingEnabled)// && !bSimulated)
            {
                PlayerWizard wizard = GetOwnerAsPlayerWizard(ei);
                string strCaster    = wizard != null ? GetNameID(wizard) : GetOwnerAsBattleUnitNameID(ei);

                string strTarget   = GetNameOwnerID(target);
                string strEnchName = GetName(e);

                Debug.LogFormat("  invoking [EREM_RemoveCounterMagic2] Caster:{0} Target:{1} Enchantment:{2} Simulated:{3} ...",
                                 strCaster, strTarget, strEnchName, bSimulated);
            }

            if (target is GameManager)
            {
                GameManager gameManager = GameManager.Get();

                if (gameManager.GetEnchantments().Find(
                    o => o.source == ei.source) == null)
                {
                    gameManager.worldCounterMagic--;
                }
            }

            Battle battle = target as Battle;
            if (battle != null)
            {
                if (battle.GetEnchantments().Find(
                    o => o.source == ei.source) == null)
                {
                    battle.battleCounterMagic--;

                    if (GetWizardOwnerID(ei) == PlayerWizard.HumanID())
                    {
                        SetCounterMagicPool(0, CM_TYPE.CM_HUMAN);
                    }
                    else
                    {
                        SetCounterMagicPool(0, CM_TYPE.CM_AI);
                    }
                }
            }
            else
            {
                Debug.LogError("    [EREM_RemoveCounterMagic] - target as Battle == null");
                return;
            }
        }
    }
}
#endif