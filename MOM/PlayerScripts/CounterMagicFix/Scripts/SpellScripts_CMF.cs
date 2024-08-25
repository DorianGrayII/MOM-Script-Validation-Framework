/**********************************
 *
 * Author:  Dorian Gray
 * Date:    May 19, 2024
 * Version: 1.0.11
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
using WorldCode;
using GameScript;


namespace GameScript_CMF
{
    using static UserUtility.Utility;

    public class SpellScripts : ScriptBase
    {
        static MHRandom random = MHRandom.Get();

        /// <summary>
        /// enables verbose logging
        /// </summary>
        private const bool bLoggingEnabled = true;
        /// <summary>
        /// controls rather spell battle costs will be range-adjusted or not
        /// </summary>
        private const bool bCMBattleCostByDistance = false;

        /// <summary>
        /// Modified version of SBG_DispelMagic
        /// used by SPELL-DISPEL_MAGIC_TRUE
        ///         SPELL-DISPEL_MAGIC
        /// </summary>
        /// <param name="data"></param>
        /// <param name="target">target as BattleUnit</param>
        /// <param name="spell"></param>
        /// <returns></returns>
        public static bool SBG_DispelMagic(SpellCastData data, object target, Spell spell)
        {
            /*
             *  Note:
             *  if (spellCaster.GetWizardOwner() == GameManager.GetHumanWizard()) //player is casting dispel
             */
            BattleUnit bu = target as BattleUnit;
            if (bu == null)
            {
                Debug.LogError("[SBG_DispelMagic] Spell " + spell.dbName + " target object is not a BattleUnit type");
                return false;
            }

            ISpellCaster spellCaster = data.caster;
            string strCaster         = GetCasterNameOwnerID(data);

            bool bSimulated = bu.simulated;

            if (bLoggingEnabled && !bSimulated)
            {
                bool bPlayerDispeller = false;
                if (spellCaster != null)
                {
                    PlayerWizard owner = spellCaster.GetWizardOwner();
                    if (IsHuman(owner))
                    {
                        bPlayerDispeller = true;
                    }
                }

                if (bPlayerDispeller)
                {
                    Debug.LogFormat("  invoking [SBG_DispelMagic]    Spell:{0} castBy:{1} ...", GetSpellNameBattleCost(spell), strCaster);
                }
                else
                {
                    Debug.LogFormat("  AI invoking [SBG_DispelMagic] Spell:{0} castBy:{1} ...", GetSpellNameBattleCost(spell), strCaster);
                }
            }

            FInt fDispelCost = FInt.ZERO;
            int iExtraPower = 0;

            if (spell.fIntData != null)
            {
                fDispelCost = spell.fIntData[0];
            }

            PlayerWizard wizDispeller = data.GetPlayerWizard();

            // Add extra dispelCost if caster used extra mana
            if (wizDispeller != null &&
                wizDispeller.GetMagicAndResearch().extensionItemSpellBattle != null)
            {
                iExtraPower = wizDispeller.magicAndResearch.extensionItemSpellBattle.extraPower;
                fDispelCost += iExtraPower;
            }

            PlayerWizard wizDispellerIsCaster = wizDispeller == spellCaster ? wizDispeller : null;

            List<EnchantmentInstance> unitEnchList = bu.GetEnchantments();
            PlayerWizard buOwner                   = bu.GetWizardOwner();

            bool bIsUnitOwnerHuman = (buOwner != null) ? buOwner.IsHuman : false;

            if (bLoggingEnabled && !bSimulated)
            {
                string strSpell    = GetSpellName(spell);
                string strUnitName = GetNameOwnerID(bu);
                if (bIsUnitOwnerHuman)
                {
                    Debug.LogFormat("  AI Casting {0}({1}) ExtraMana:{2} On {3}", strSpell, fDispelCost.ToInt(), iExtraPower, strUnitName);
                }
                else
                {
                    Debug.LogFormat("     Casting {0}({1}) ExtraMana:{2} On {3}", strSpell, fDispelCost.ToInt(), iExtraPower, strUnitName);
                }
            }

            if (bu.isSpellLock && buOwner != wizDispeller)
            {
                for (int i = unitEnchList.Count - 1; i >= 0; i--)
                {
                    Enchantment ench = unitEnchList[i].source.Get();

                    if (ench.scripts == null)
                    {
                        continue;
                    }

                    // returns the first occurrence in the array
                    EnchantmentScript esSpellLock = Array.Find(ench.scripts, o => o.tag == (Tag)TAG.SPELL_LOCK);

                    if (esSpellLock != null)
                    {
                        if (GetDispelSuccess(wizDispellerIsCaster, unitEnchList[i], fDispelCost, esSpellLock.fIntData, bSimulated))
                        {
                            if (!bSimulated)
                            {
                            //    if (spellCaster == GameManager.GetHumanWizard())
                            if (spellCaster.GetWizardOwner() == GameManager.GetHumanWizard()) // player is casting dispel
                                {
                                    string message = DBUtils.Localization.Get("UI_ENCHANTMENT_REMOVED_SUCCESSFULLY", true, unitEnchList[i].source.Get().GetDILocalizedName());
                                    PopupGeneral.OpenPopup(null, "UI_SUCCESS", message, "UI_OK");
                                }
                                else
                                {
                                    string message = DBUtils.Localization.Get("UI_AI_ENCHANTMENT_REMOVED_SUCCESSFULLY_FROM", true, unitEnchList[i].source.Get().GetDILocalizedName(), bu.GetDescriptionInfo().GetLocalizedName());
                                    PopupGeneral.OpenPopup(null, "UI_SUCCESS", message, "UI_OK");
                                }

                            }

                            bu.RemoveEnchantment(unitEnchList[i]);
                            //If unit have spell lock and other enchantments on it, you need to dispel
                            // spelllock first and in another dispel use rest.

                            break;
                        }
                    }
                }
            }
            else // if (bu.isSpellLock && buOwner == wizDispeller)
            {
                string removedEnch = string.Empty;
                for (int i = unitEnchList.Count - 1; i >= 0; i--)
                {
                    // Dispel only enchantments that allow to dispel.
                    if (unitEnchList[i].source.Get().allowDispel == false)
                    {
                        continue;
                    }

                    if (buOwner == wizDispeller && unitEnchList[i].source.Get().mindControl)
                    {
                        continue;
                    }

                    // Dispel only negative enchantments on own ba. Dispel only positive enchantments on enemy ba.
                    if ((buOwner == wizDispeller && unitEnchList[i].source.Get().enchCategory == EEnchantmentCategory.Negative) ||
                        (buOwner != wizDispeller && unitEnchList[i].source.Get().enchCategory != EEnchantmentCategory.Negative))
                    {
                        if (GetDispelSuccess(wizDispellerIsCaster, unitEnchList[i], fDispelCost, bSimulated))
                        {
                            if (!bSimulated)
                            {
                                string newRemEch = unitEnchList[i].source.Get().GetDILocalizedName();
                                removedEnch = removedEnch.Length > 0 ? removedEnch + ", " + newRemEch : newRemEch;
                            }
                            bu.RemoveEnchantment(unitEnchList[i]);
                        }
                    }
                }
                if (!bSimulated)
                {
                    if (spellCaster.GetWizardOwner() == GameManager.GetHumanWizard()) // player is casting dispel
                    {
                        if (!string.IsNullOrEmpty(removedEnch))
                        {
                            if (removedEnch.Contains(", ")) // different text for 1 and more than 1 enchantments removed
                            {
                                string message = DBUtils.Localization.Get("UI_ENCHANTMENTS_REMOVED_SUCCESSFULLY", true, removedEnch);
                                PopupGeneral.OpenPopup(null, "UI_SUCCESS", message, "UI_OK");
                            }
                            else
                            {
                                string message = DBUtils.Localization.Get("UI_ENCHANTMENT_REMOVED_SUCCESSFULLY", true, removedEnch);
                                PopupGeneral.OpenPopup(null, "UI_SUCCESS", message, "UI_OK");
                            }
                        }
                        else
                        {
                            PopupGeneral.OpenPopup(null, "UI_DISPEL_FAILED", "UI_NO_ENCHANTMENTS_REMOVED", "UI_OK");
                        }
                    }
                    else     // enemy wizard is casting dispel during battle with player
                    {
                        if (!string.IsNullOrEmpty(removedEnch))
                        {
                            string message = DBUtils.Localization.Get("UI_AI_ENCHANTMENT_REMOVED_SUCCESSFULLY_FROM", true, removedEnch, bu.GetName());
                            PopupGeneral.OpenPopup(null, "UI_ENCHANTMENT_DISPELLED", message, "UI_OK");
                        }
                        else
                        {
                            PopupGeneral.OpenPopup(null, "UI_DISPEL_FAILED", "UI_AI_NO_ENCHANTMENTS_REMOVED", "UI_OK");
                        }
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="target"></param>
        /// <param name="spell"></param>
        /// <returns></returns>
        public static bool SBG_DisenchantArea(SpellCastData data, object target, Spell spell)
        {
            FInt dispelCost = FInt.ZERO;

            if (spell.fIntData != null)
            {
                dispelCost = spell.fIntData[0];
            }
            else
            {
                //In oMom dispel power was 100 for nightshade.
                //But our dispel is more powerfully (dispel work on units as well )
                //and each building that use nightshade add extra try.
                dispelCost = (FInt)50f;
            }

            ISpellCaster spellCaster  = data.caster;
            PlayerWizard playerWizard = data.GetPlayerWizard();
            // Add extra dmg if player used extra mana
            if (playerWizard != null &&
                playerWizard.GetMagicAndResearch().extensionItemSpellBattle != null)
            {
                dispelCost += playerWizard.magicAndResearch.extensionItemSpellBattle.extraPower;
            }

            PlayerWizard playerWizardAsCaster = playerWizard == spellCaster ? playerWizard : null;

            // remove enchantment from units
            List<BattleUnit> ownerUnitList = data.GetFriendlyUnits();
            List<BattleUnit> enemyUnitList = data.GetEnemyUnits();

//            List<EnchantmentInstance> eiList;
            string strRemovedEnchs = string.Empty;

            strRemovedEnchs = DispelEnchantsFromFriendlyUnits(ownerUnitList, playerWizardAsCaster, dispelCost);

            #region replacedBy_DispelEnchantsFromFriendlyUnits

            /*
            for (int i = ownerUnitList.Count - 1; i >= 0; i--)
            {
                if (ownerUnitList.Count <= i)
                {
                    continue;
                }

                eiList = ownerUnitList[i].GetEnchantments();

                for (int j = eiList.Count - 1; j >= 0; j--)
                {
                    if (ownerUnitList.Count <= i)
                    {
                        continue;
                    }

                    if (eiList.Count <= j)
                    {
                        continue;
                    }

                    // Dispel only enchantments that allow to dispel.
                    if (eiList[j].source.Get().allowDispel == false)
                    {
                        continue;
                    }

                    if (eiList[j].source.Get().mindControl == true)
                    {
                        continue;
                    }

                    bool bSimulated = ownerUnitList[i].simulated;

                    if (eiList[j].source.Get().enchCategory == EEnchantmentCategory.Negative)
                    {
                        if (GetDispelSuccess(wizAsCaster, eiList[j], dispelCost, bSimulated))
                        {
                            if (!bSimulated)
                            {
                                string newRemEch = eiList[j].source.Get().GetDILocalizedName();
                                removedEnch = removedEnch.Length > 0 ? removedEnch + ", " + newRemEch : newRemEch;
                            }

                            ownerUnitList[i].RemoveEnchantment(eiList[j].source.Get());
                        }
                    }
                }
            }
            */
            #endregion

            strRemovedEnchs = DispelEnchantsFromEnemyUnits(enemyUnitList, playerWizardAsCaster, playerWizard, dispelCost, strRemovedEnchs);

            #region replacedBy_DispelEnchantsFromEnemyUnits

            /*
            for (int i = enemyUnitList.Count - 1; i >= 0; i--)
            {
                if (enemyUnitList.Count <= i)
                {
                    continue;
                }

                eiList = enemyUnitList[i].GetEnchantments();

                if (enemyUnitList[i].isSpellLock)
                {
                    for (int j = 0; j < eiList.Count; j++)
                    {
                        if (enemyUnitList.Count <= i)
                        {
                            continue;
                        }

                        if (eiList.Count <= j)
                        {
                            continue;
                        }

                        // Dispel only enchantments that allow to dispel.
                        if (eiList[j].source.Get().allowDispel == false)
                        {
                            continue;
                        }

                        Enchantment ench = eiList[j].source.Get();

                        if (ench.scripts == null)
                        {
                            continue;
                        }

                        EnchantmentScript esSpellLock = Array.Find(eiList[j].source.Get().scripts, o => o.tag == (Tag)TAG.SPELL_LOCK);
                        if (esSpellLock != null)
                        {
                            bool bSimulated = enemyUnitList[i].simulated;
                            if (GetDispelSuccess(wizAsCaster, eiList[j], dispelCost, esSpellLock.fIntData, bSimulated))
                            {
                                if (!bSimulated)
                                {
                                    string newRemEch = eiList[j].source.Get().GetDILocalizedName();
                                    removedEnch = removedEnch.Length > 0 ? removedEnch + ", " + newRemEch : newRemEch;
                                }

                                enemyUnitList[i].RemoveEnchantment(eiList[j].source);
                            }

                            break;
                        }
                    }
                }  
                else // isSpellLock == false
                {
                    for (int j = eiList.Count - 1; j >= 0; j--)
                    {
                        if (enemyUnitList.Count <= i)
                        {
                            continue;
                        }

                        if (eiList.Count <= j)
                        {
                            continue;
                        }

                        //Dispel only enchantments that allow to dispel.
                        if (eiList[j].source.Get().allowDispel == false)
                        {
                            continue;
                        }

                        if (eiList[j].owner != wiz)
                        {
                            bool bSimulated = enemyUnitList[i].simulated;

                            if (GetDispelSuccess(wizAsCaster, eiList[j], dispelCost, bSimulated))
                            {
                                if (!bSimulated)
                                {
                                    string newRemEch = eiList[j].source.Get().GetDILocalizedName();
                                    removedEnch = removedEnch.Length > 0 ? removedEnch + ", " + newRemEch : newRemEch;
                                }

                                enemyUnitList[i].RemoveEnchantment(eiList[j].source.Get());
                            }
                        }
                    }
                }
            }
            */
            #endregion

            if (data.battle != null)
            {
                bool bSimulated = data.battle.simulation;
                //remove enchantment from battlefield 
                if (GameScript.SpellScripts.IsTownProtected(data.GetWizardID(), spell, data.battle))
                {
                    return true;
                }

                strRemovedEnchs = DispelEnchantsFromBattle(data.battle, playerWizardAsCaster, data.GetPlayerWizard(), dispelCost, strRemovedEnchs);

                #region replacedByPreviousFunction
                /*
                eiList = data.battle.GetEnchantments();

                for (int i = eiList.Count - 1; i >= 0; i--)
                {
                    if (eiList.Count <= i)
                    {
                        continue;
                    }

                    if (eiList[i].owner != wiz)
                    {
                        //check if caster is a unit
                        BattleUnit bu = eiList[i].owner?.GetEntity() as BattleUnit;
                        if (bu != null)
                        {
                            if (bu.GetWizardOwner() == wiz)
                            {
                                continue;
                            }
                        }

                        //Dispel only enchantments that allow to dispel.
                        if (eiList[i].source.Get().allowDispel == false)
                        {
                            continue;
                        }

                        if (GetDispelSuccess(wizAsCaster, eiList[i], dispelCost, bSimulated))
                        {
                            if (!bSimulated)
                            {
                                string newRemEch = eiList[i].source.Get().GetDILocalizedName();
                                removedEnch = removedEnch.Length > 0 ? removedEnch + ", " + newRemEch : newRemEch;
                            }

                            data.battle.RemoveEnchantment(eiList[i].source.Get());
                        }
                    }
                }
                */
                #endregion

                if (!bSimulated)
                {
                    if (data.battle.attacker.GetWizardOwner() == GameManager.GetHumanWizard() ||
                        data.battle.defender.GetWizardOwner() == GameManager.GetHumanWizard())
                    {
                        if (spellCaster.GetWizardOwner() == GameManager.GetHumanWizard())
                        {
                            if (!string.IsNullOrEmpty(strRemovedEnchs))
                            {
                                if (strRemovedEnchs.Contains(", ")) //different text for 1 and more than 1 enchantments removed
                                {
                                    string message = DBUtils.Localization.Get("UI_ENCHANTMENTS_REMOVED_SUCCESSFULLY", true, strRemovedEnchs);
                                    PopupGeneral.OpenPopup(null, "UI_SUCCESS", message, "UI_OK");
                                }
                                else
                                {
                                    string message = DBUtils.Localization.Get("UI_ENCHANTMENT_REMOVED_SUCCESSFULLY", true, strRemovedEnchs);
                                    PopupGeneral.OpenPopup(null, "UI_SUCCESS", message, "UI_OK");
                                }
                            }
                            else
                            {
                                PopupGeneral.OpenPopup(null, "UI_DISPEL_FAILED", "UI_NO_ENCHANTMENTS_REMOVED", "UI_OK");
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(strRemovedEnchs))
                            {
                                string message = DBUtils.Localization.Get("UI_AI_ENCHANTMENTS_REMOVED_SUCCESSFULLY_FROM_GLOBAL", true, strRemovedEnchs);
                                PopupGeneral.OpenPopup(null, "UI_ENCHANTMENT_DISPELLED", message, "UI_OK");
                            }
                            else
                            {
                                PopupGeneral.OpenPopup(null, "UI_DISPEL_FAILED", "UI_AI_NO_ENCHANTMENTS_REMOVED", "UI_OK");
                            }
                        }
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Specialized version of SBW_ApplyBattleEnchantment
        /// corrects battle costs used
        /// </summary>
        /// <param name="data"></param>
        /// <param name="target">target as Battle</param>
        /// <param name="spell"></param>
        /// <returns></returns>
        public static bool SBW_ApplyCounterMagic(SpellCastData data, object target, Spell spell)
        {
            Battle b = target as Battle;
            if (b == null)
            {
                Debug.LogError("Spell " + spell.dbName + " is not targeting Battle type");
                return false;
            }

            bool bSimulated = b.simulation;
            
            if (bLoggingEnabled && !bSimulated)
            {
                string strCasterName = GetWizardNameID(data);
                string strSpell      = GetSpellNameBattleCost(spell);
                Debug.LogFormat("invoking [SBW_ApplyCounterMagic] Spell:{0} castBy:{1} ... ", strSpell, strCasterName);
            }

            PlayerWizard wizard = data.GetPlayerWizard();

            foreach (Enchantment ench in spell.enchantmentData)
            {
                int iDispelCost = 1;

                if (bCMBattleCostByDistance)
                {

#pragma warning disable CS0162 // Unreachable code detected
                    bool bChanneler = false;

                    // turn off channelling benefit for purpose of calculating dispel cost
                    if (wizard != null && HasTrait(wizard, TRAIT.CHANNELER))
                    {
                        wizard.ignorSpellcastingRange = false;
                        bChanneler = true;
                    }
                    // int dispelCost = spell.battleCost;
                    iDispelCost = spell.GetBattleCastingCostByDistance(data.caster);

                    if (bChanneler)
                    {
                        // true it back on
                        wizard.ignorSpellcastingRange = true;
                    }
 #pragma warning restore CS0162 // Unreachable code detected
                }
                else
                {
                    iDispelCost = spell.battleCost;
                }

                int iExtraMana  = 0;
                if (wizard != null && wizard.GetMagicAndResearch().extensionItemSpellBattle != null)
                {
                    iExtraMana = wizard.GetMagicAndResearch().extensionItemSpellBattle.extraMana;
                    iDispelCost += iExtraMana;
                }
                if (bLoggingEnabled && !bSimulated)
                {
                //    string strCasterName = GetWizardNameID(data);
                    string strCasterName = GetCasterNameOwnerID(data);
                    string strEnchName   = GetEnchantmentName(ench, iDispelCost);
                    Debug.LogFormat("  AddEnchantment({0}) castBy:{1} battleCost:{2} extraMana:{3}", 
                                       strEnchName, strCasterName, spell.battleCost, iExtraMana);
                }
                b.AddEnchantment(ench, data.caster as IEnchantable, ench.lifeTime, null, iDispelCost);
            }

            return true;
        }

 
        public static bool SWW_Disjunction(ISpellCaster source, object target, Spell spell)
        {
            EnchantmentInstance ei = target as EnchantmentInstance;
            if (ei == null)
            {
                Debug.LogError("Spell " + spell.dbName + " is not targeting EnchantmentInstance object type");
                return false;
            }

            FInt dispelStr = spell.fIntData[0];
            PlayerWizard spellCaster = source as PlayerWizard;

            //Add extra dispel power if player used extra mana
            if (spellCaster != null &&
                spellCaster.GetMagicAndResearch().extensionItemSpellWorld != null)
            {
                //Multiplier used for additional mana.
                //Base strength have this multiplier baked into its default value
                FInt multiplier = spell.fIntData[1];
                dispelStr += spellCaster.magicAndResearch.extensionItemSpellWorld.extraPower * multiplier;
            }

            PlayerWizard spellCasterOwner = source.GetWizardOwner();
            EnchantmentManager enchManager = ei.manager;

            if (GetDispelSuccess(spellCasterOwner, ei, dispelStr))
            {
                //success
                // if (spellCaster == GameManager.GetHumanWizard())
                if (spellCaster.GetWizardOwner() == GameManager.GetHumanWizard())
                {
                    string message = DBUtils.Localization.Get("UI_ENCHANTMENT_REMOVED_SUCCESSFULLY", true, ei.source.Get().GetDILocalizedName());
                    PopupGeneral.OpenPopup(null, "UI_SUCCESS", message, "UI_OK");
                }
                //                 else
                //                 {
                //                     var message = DBUtils.Localization.Get("UI_AI_ENCHANTMENT_REMOVED_SUCCESSFULLY", true, ench.source.Get().GetDILocalizedName());
                //                     PopupGeneral.OpenPopup(null, "UI_ENCHANTMENT_DISPELLED", message, "UI_OK");
                //                 }

                enchManager.owner.RemoveEnchantment(ei);
            }
            else
            {
                //failure
                //if (spellCaster == GameManager.GetHumanWizard())
                if (spellCaster.GetWizardOwner() == GameManager.GetHumanWizard())
                {
                    string message = DBUtils.Localization.Get("UI_FAILED_TO_REMOVE_ENCHANTMENT", true, ei.source.Get().GetDILocalizedName());
                    PopupGeneral.OpenPopup(null, "UI_DISPEL_FAILED", message, "UI_OK");
                }
                else
                {
                    string message = DBUtils.Localization.Get("UI_AI_FAILED_TO_REMOVE_ENCHANTMENT", true, ei.source.Get().GetDILocalizedName());
                    PopupGeneral.OpenPopup(null, "UI_DISPEL_FAILED", message, "UI_OK");
                }
            }

            return true;
        }
        public static bool SWG_DisenchantArea(ISpellCaster source, object target, WorldCode.Plane plane, Spell spell)
        {
            if (!(target is Vector3i))
            {
                Debug.LogError("Spell " + spell.dbName + " is not targeting Vector3i object type");
                return false;
            }

            FInt fDispelCost;
            if ((spell != null) && (spell.fIntData != null) && (spell.fIntData.Length > 0))
            {
                fDispelCost = spell.fIntData[0];
            }
            else
            {
                //In oMom dispel power was 100 for nightshade.
                //But our dispel is more powerfully (dispel work on units as well )
                //and each building that use nightshade add extra try.
                fDispelCost = (FInt)50f;
            }

            PlayerWizard spellCaster = source as PlayerWizard;

            //Add extra dmg if player used extra mana
            if (spellCaster != null &&
                spellCaster.GetMagicAndResearch().extensionItemSpellWorld != null)
            {
                fDispelCost += spellCaster.magicAndResearch.extensionItemSpellWorld.extraPower;
            }


            Vector3i targetPosn = (Vector3i)target;
            PlayerWizard spellCasterOwner = source.GetWizardOwner();

            //remove enchantment from units
            List<MOM.Group> groupList = GameManager.Get().registeredGroups;
            //groups = groups.FindAll(o => o.GetPosition() == position);
            List<EnchantmentInstance> eiList;
            string removedEnch = string.Empty;

            foreach (MOM.Group group in groupList)
            {
                if (!group.IsDistanceTo_Zero(targetPosn, plane))
                {
                    continue;
                }

                if (group.GetLocationHost()?.otherPlaneLocation?.Get() != null && group.plane.arcanusType)
                {
                    continue;
                }

                foreach (Reference<MOM.Unit> refUnit in group.GetUnits())
                {
                    MOM.Unit unit = refUnit.Get();
                    PlayerWizard unitOwner = unit.GetWizardOwner();

                    eiList = unit.GetEnchantments();

                    bool bSimulated = unit.simulationUnit;
                    if (unit.isSpellLock && unitOwner != spellCasterOwner)
                    {
                        for (int i = eiList.Count - 1; i >= 0; i--)
                        {
                            if (eiList.Count <= i)
                            {
                                continue;
                            }

                            Enchantment ench = eiList[i].source.Get();

                            if (ench.scripts == null)
                            {
                                continue;
                            }

                            EnchantmentScript es = Array.Find(eiList[i].source.Get().scripts, o => o.tag == (Tag)TAG.SPELL_LOCK);
                            if (es != null)
                            {
                                if (GetDispelSuccess(spellCasterOwner, eiList[i], fDispelCost, es.fIntData, bSimulated))
                                {
                                    string newRemEch = eiList[i].source.Get().GetDILocalizedName();
                                    removedEnch = removedEnch.Length > 0 ? removedEnch + ", " + newRemEch : newRemEch;

                                    unit.RemoveEnchantment(eiList[i].source);
                                }

                                break;
                            }
                        }
                    }
                    else
                    {
                        for (int i = eiList.Count - 1; i >= 0; i--)
                        {
                            if (eiList.Count <= i)
                            {
                                continue;
                            }

                            //Dispel only ench that allow to dispel.
                            if (eiList[i].source.Get().allowDispel)
                            {

                                //Disenchant only negative ench on own unit. Disenchant only positive ench on enemy unit.
                                if ((unitOwner == spellCasterOwner && eiList[i].source.Get().enchCategory == EEnchantmentCategory.Negative) ||
                                    (unitOwner != spellCasterOwner && eiList[i].source.Get().enchCategory == EEnchantmentCategory.Positive))
                                {

                                    if (GetDispelSuccess(spellCasterOwner, eiList[i], fDispelCost, bSimulated))
                                    {
                                        string newRemEch = eiList[i].source.Get().GetDILocalizedName();
                                        removedEnch = removedEnch.Length > 0 ? removedEnch + ", " + newRemEch : newRemEch;

                                        unit.RemoveEnchantment(eiList[i].source);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            //remove enchantment from location
            MOM.Location location = GameManager.Get().registeredLocations.Find(o => o.IsDistanceTo_Zero(targetPosn, plane));
            if (location == null)
            {
                return true;
            }

            int iLocationOwnerID = location.owner;
            eiList = location.GetEnchantments();

            for (int i = eiList.Count - 1; i >= 0; i--)
            {
                //Dispel only ench that allow to dispel.
                if (eiList[i].source.Get().allowDispel)
                {
                    //Disenchant only negative enchantments on own unit. Disenchant only positive enchantments on enemy unit.
                    if ((iLocationOwnerID == spellCasterOwner.ID && eiList[i].source.Get().enchCategory == EEnchantmentCategory.Negative) ||
                        (iLocationOwnerID != spellCasterOwner.ID && eiList[i].source.Get().enchCategory == EEnchantmentCategory.Positive))
                    {

                        if (GetDispelSuccess(spellCasterOwner, eiList[i], fDispelCost))
                        {
                            string newRemEch = eiList[i].source.Get().GetDILocalizedName();
                            removedEnch = removedEnch.Length > 0 ? removedEnch + ", " + newRemEch : newRemEch;

                            location.RemoveEnchantment(eiList[i].source);
                        }
                    }
                }
            }

//            if (spellCaster == GameManager.GetHumanWizard()) //player is casting dispel
            if (spellCaster.GetWizardOwner() == GameManager.GetHumanWizard())
            {
                if (!string.IsNullOrEmpty(removedEnch))
                {
                    if (removedEnch.Contains(", ")) //different text for 1 and more than 1 enchantments removed
                    {
                        string message = DBUtils.Localization.Get("UI_ENCHANTMENTS_REMOVED_SUCCESSFULLY", true, removedEnch);
                        PopupGeneral.OpenPopup(null, "UI_SUCCESS", message, "UI_OK");
                    }
                    else
                    {
                        string message = DBUtils.Localization.Get("UI_ENCHANTMENT_REMOVED_SUCCESSFULLY", true, removedEnch);
                        PopupGeneral.OpenPopup(null, "UI_SUCCESS", message, "UI_OK");
                    }
                }
                else
                {
                    PopupGeneral.OpenPopup(null, "UI_DISPEL_FAILED", "UI_NO_ENCHANTMENTS_REMOVED", "UI_OK");
                }
            }

            return true;
        }

        public static bool GetDispelSuccess(PlayerWizard spellCasterAsWizard, EnchantmentInstance ei, FInt fDispelCost)
        {
            // Dispel SpellLock first then other enchantments
            return GetDispelSuccess(spellCasterAsWizard, ei, fDispelCost, (FInt)ei.dispelCost, false);
        }

        /// <summary>
        /// Overloaded GetDispelSuccess method
        /// </summary>
        /// <param name="spellCasterAsWizard"></param>
        /// <param name="ei"></param>
        /// <param name="fDispelCost"></param>
        /// <param name="bSimulated"></param>
        /// <returns></returns>
        public static bool GetDispelSuccess(PlayerWizard spellCasterAsWizard, EnchantmentInstance ei, FInt fDispelCost, bool bSimulated)
        {
            // Dispel SpellLock first then other enchantments
            return GetDispelSuccess(spellCasterAsWizard, ei, fDispelCost, (FInt)ei.dispelCost, bSimulated);
        }

        /// <summary>
        /// Overladed GetDispelSuccess method
        /// </summary>
        /// <param name="spellCasterAsWizard">can be null</param>
        /// <param name="ei"></param>
        /// <param name="dispelCost"></param>
        /// <param name="spellCost"></param>
        /// <param name="bSimulated"></param>
        /// <returns></returns>
        public static bool GetDispelSuccess(PlayerWizard spellCasterAsWizard, EnchantmentInstance ei, FInt dispelCost, FInt spellCost, bool bSimulated = false)
        {
            // this is the owner of the target
            PlayerWizard eiOwner = ei.owner != null ? ei.owner.GetEntity() as PlayerWizard : null;

            if (bLoggingEnabled && !bSimulated)
            {
                // spellCasterAsWizard could be null in case of neutral mob unit (i.e. Djinn) caster
                string strCaster    = GetNameID(spellCasterAsWizard);
                string strEnchOwner = GetNameID(eiOwner);
                string strEnchName  = string.Format("{0}({1})", GetEnchantmentName(ei), spellCost.ToInt());

                Debug.LogFormat("  invoking [GetDispelSuccess]({0}) castBy:{1} vs {2} castBy:{3} ... ",
                                       dispelCost.ToInt(), strCaster, strEnchName, strEnchOwner);
            }

            FInt RetortMod = FInt.ONE;
            bool bIsEnchOwnerHuman = false;
            if (eiOwner != null)
            {
                bIsEnchOwnerHuman = eiOwner.IsHuman;
                RetortMod = eiOwner.GetDispelDificulty(ei);
            }

            FInt dispelChance = FInt.ZERO;
            FInt spellStr = RetortMod * spellCost;

            int iEasyMod = (spellCasterAsWizard != null) ? spellCasterAsWizard.easierDispelling.ToInt() : 1;

            dispelChance = iEasyMod * dispelCost / (dispelCost + (RetortMod * spellCost));

            bool bReturn = random.GetSuccesses(dispelChance.ToFloat(), 1) > 0;

            if (bLoggingEnabled && !bSimulated)
            {
                //       Debug.LogFormat(" VAR TEST bIsEnchOwnerHuman:{0} ei.nameID:{1} ei.owner.ID:{2} ei.GetEventDisplayName:{3} spellCasterAsWizard:{4}", 
                //                              bIsEnchOwnerHuman, ei.nameID, ei.owner != null ? ei.owner.ID : -1, ei.GetEventDisplayName(), GetNameID(spellCasterAsWizard));

                string strDispelSuccess = string.Format("GetDispelSuccess({0})", dispelCost.ToInt());
                string strEnchName      = string.Format("{0}({1})", GetEnchantmentName(ei), spellStr.ToInt());

                if (bIsEnchOwnerHuman == false)
                {
                    Debug.LogFormat("    {0,-22} vs    {1,-22} DispelCost:{2,-4} Chance:{3,-5:P1} Dispelled?:{4,-5} retortMod:{5,2} easyMod:{6}",
                                    strDispelSuccess, strEnchName, spellCost.ToInt(), dispelChance.ToFloat(), bReturn, RetortMod.ToInt(), iEasyMod);
                }
                else
                {
                    Debug.LogFormat("    AI {0,-22} vs {1,-22} DispelCost:{2,-4} Chance:{3,-5:P1} Dispelled?:{4,-5} retortMod:{5,2} easyMod:{6}",
                                    strDispelSuccess, strEnchName, spellCost.ToInt(), dispelChance.ToFloat(), bReturn, RetortMod.ToInt(), iEasyMod);
                }
            }

            return bReturn;
        }

        #region EnchAlreadyOnObject Fix
        /*
         * Following may need to be uncommented to ensure the optimized 'EnchAlreadyOnObject' method is called
         * 
         */
         /*
        public static bool STAR_Friendly_Unit(SpellCastData data, object target, Spell spell)
        {
            MOM.Unit   u  = target as MOM.Unit;
            BattleUnit bu = target as BattleUnit;
            if (u != null)
            {
                if (data.GetWizardID() == u.group.Get().GetOwnerID())
                {
                    if (EnchAlreadyOnObject(data.caster, spell, u))
                    {
                        return false;
                    }

                    MOM.Location location = u.group.Get().GetLocationHostSmart();
                    TownLocation townLocation = location as TownLocation;
                    
                    if (townLocation != null)
                    {
                        if (!GameScript.SpellScripts.IsTownProtected(data.GetWizardID(), spell, townLocation))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        return true;
                    }

                }
            }
            else if (bu != null)
            {
                if (data.GetWizardID() == bu.ownerID)
                {
                    if (EnchAlreadyOnObject(data.caster, spell, bu))
                    {
                        return false;
                    }

                    if (!GameScript.SpellScripts.IsTownProtected(data.GetWizardID(), spell, data.battle))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool STAR_FriendlyNormal_Unit(SpellCastData data, object target, Spell spell)
        {
            //var w = data.GetPlayerWizard();
            MOM.Unit   u  = target as MOM.Unit;
            BattleUnit bu = target as BattleUnit;
            if (u != null)
            {
                if (data.GetWizardID() == u.group.Get().GetOwnerID() && u.attributes.Contains(TAG.NORMAL_CLASS))
                {
                    if (EnchAlreadyOnObject(data.caster, spell, u))
                    {
                        return false;
                    }

                    MOM.Location location = u.group.Get().GetLocationHostSmart();
                    TownLocation townLocation = location as TownLocation;
                    if (townLocation != null)
                    {
                        if (!GameScript.SpellScripts.IsTownProtected(data.GetWizardID(), spell, townLocation))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            else if (bu != null)
            {
                if (data.GetWizardID() == bu.ownerID && bu.attributes.Contains(TAG.NORMAL_CLASS))
                {
                    if (EnchAlreadyOnObject(data.caster, spell, bu))
                    {
                        return false;
                    }

                    if (!GameScript.SpellScripts.IsTownProtected(data.GetWizardID(), spell, data.battle))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool STAR_FriendlyNormalUnitNormalRange(SpellCastData data, object target, Spell spell)
        {
            //var w = data.GetPlayerWizard();
            MOM.Unit   u  = target as MOM.Unit;
            BattleUnit bu = target as BattleUnit;
            if (u != null)
            {
                if (data.GetWizardID() == u.group.Get().GetOwnerID() 
                    && u.attributes.Contains(TAG.NORMAL_CLASS)
                    && u.attributes.DoesNotContains((Tag)TAG.MAGIC_RANGE) 
                    && u.attributes.Contains(TAG.RANGED_UNIT))
                {
                    if (EnchAlreadyOnObject(data.caster, spell, u))
                    {
                        return false;
                    }

                    MOM.Location location = u.group.Get().GetLocationHostSmart();
                    TownLocation townLocation = location as TownLocation;
                    if (townLocation != null)
                    {
                        if (!GameScript.SpellScripts.IsTownProtected(data.GetWizardID(), spell, townLocation))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            else if (bu != null)
            {
                if (data.GetWizardID() == bu.ownerID && bu.attributes.Contains(TAG.NORMAL_CLASS)
                    && bu.attributes.DoesNotContains((Tag)TAG.MAGIC_RANGE) 
                    && bu.attributes.Contains(TAG.RANGED_UNIT))
                {
                    if (EnchAlreadyOnObject(data.caster, spell, bu))
                    {
                        return false;
                    }

                    if (!GameScript.SpellScripts.IsTownProtected(data.GetWizardID(), spell, data.battle))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool STAR_FriendlyNormalUnitOrHeroNormalRange(SpellCastData data, object target, Spell spell)
        {
            //var w = data.GetPlayerWizard();
            MOM.Unit   u  = target as MOM.Unit;
            BattleUnit bu = target as BattleUnit;
            if (u != null)
            {
                if (data.GetWizardID() == u.group.Get().GetOwnerID() 
                    && (u.attributes.Contains(TAG.NORMAL_CLASS) || u.attributes.Contains(TAG.HERO_CLASS))
                    && u.attributes.DoesNotContains((Tag)TAG.MAGIC_RANGE) 
                    && (u.attributes.Contains(TAG.RANGED_UNIT) || u.attributes.Contains(TAG.NORMAL_RANGE)))
                {
                    if (EnchAlreadyOnObject(data.caster, spell, u))
                    {
                        return false;
                    }

                    MOM.Location location = u.group.Get().GetLocationHostSmart();
                    TownLocation townLocation = location as TownLocation;
                    if (townLocation != null)
                    {
                        if (!GameScript.SpellScripts.IsTownProtected(data.GetWizardID(), spell, townLocation))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            else if (bu != null)
            {
                if (data.GetWizardID() == bu.ownerID 
                    && (bu.attributes.Contains(TAG.NORMAL_CLASS) || bu.attributes.Contains(TAG.HERO_CLASS))
                    && bu.attributes.DoesNotContains((Tag)TAG.MAGIC_RANGE) 
                    && (bu.attributes.Contains(TAG.RANGED_UNIT) || bu.attributes.Contains(TAG.NORMAL_RANGE)))
                {
                    if (EnchAlreadyOnObject(data.caster, spell, bu))
                    {
                        return false;
                    }

                    if (!GameScript.SpellScripts.IsTownProtected(data.GetWizardID(), spell, data.battle))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public static bool STAR_FriendlyNonFantastic_Unit(SpellCastData data, object target, Spell spell)
        {
            //var w = data.GetPlayerWizard();
            MOM.Unit   u  = target as MOM.Unit;
            BattleUnit bu = target as BattleUnit;
            if (u != null)
            {
                if (data.GetWizardID() == u.group.Get().GetOwnerID() 
                    && u.attributes.DoesNotContains((Tag)TAG.FANTASTIC_CLASS))
                {
                    if (EnchAlreadyOnObject(data.caster, spell, u))
                    {
                        return false;
                    }

                    MOM.Location location = u.group.Get().GetLocationHostSmart();
                    TownLocation townLocation = location as TownLocation;
                    if (townLocation != null)
                    {
                        if (!GameScript.SpellScripts.IsTownProtected(data.GetWizardID(), spell, townLocation))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            else if (bu != null)
            {
                if (data.GetWizardID() == bu.ownerID 
                    && bu.attributes.DoesNotContains((Tag)TAG.FANTASTIC_CLASS))
                {
                    if (EnchAlreadyOnObject(data.caster, spell, bu))
                    {
                        return false;
                    }

                    if (!GameScript.SpellScripts.IsTownProtected(data.GetWizardID(), spell, data.battle))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static int SBAI_Web(SpellCastData data, object target, Spell spell)
        {
            BattleUnit targetBu = target as BattleUnit;
            if (targetBu == null) 
            {
                Debug.LogWarning("SBAI_Web target is BattleUnit == null");
                return 0;
            }

            if (EnchAlreadyOnObject(data.caster, spell, targetBu))
            {
                return 0;
            }

            // if caster is hero
            BattleUnit hero = data.GetCasterAsBattleUnit();

            FInt value = (FInt)targetBu.GetBattleUnitValue();

            if (targetBu.GetAttributes().Contains(TAG.CAN_FLY))
            {
                value = value * (FInt)1.2;
            }

            FInt resistmod = FInt.ZERO;
            if (spell.fIntData != null && spell.fIntData.Length > 0)
            {
                resistmod = spell.fIntData[0];
            }

            int resistValue = (targetBu.GetAttributes().GetFinal((Tag)TAG.RESIST) + GameScript.SpellScripts.ResistModFromEnch(hero, targetBu, spell) - resistmod).ToInt();

            float valuePercent = Mathf.Min((12 - resistValue) * 0.1f, 0.1f);

            value *= valuePercent;

#if UNITY_EDITOR && DEBUG_SPELLS
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + value.ToInt() +
                " on unit " + targetBu.GetDBName().ToString());
#endif
            return value.ToInt();
        }

        public static bool STAR_EnemyBattlePlayer(SpellCastData data, object target, Spell spell)
        {
            BattlePlayer bp = target as BattlePlayer;
            if (bp != null)
            {
                PlayerWizard owner = data.GetPlayerWizard();
                if (bp.wizard != owner)
                {
                    if (EnchAlreadyOnObject(data.caster, spell, bp))
                    {
                        return false;
                    }

                    if (!GameScript.SpellScripts.IsTownProtected(data.GetWizardID(), spell, data.battle))
                    {
                        return true;
                    }
                }
            }
            else
            {
                Debug.Log("Spell is designed to target BattlePlayer");
            }

            return false;
        }
       */
        #endregion

        #region cyclomatic complexity helpers

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ownerUnitList"></param>
        /// <param name="playerWizardAsCaster">can be null</param>
        /// <param name="fDispelCost"></param>
        /// <returns>string containing dispelled enchantments</returns>
        private static string DispelEnchantsFromFriendlyUnits(List<BattleUnit> ownerUnitList, PlayerWizard playerWizardAsCaster, FInt fDispelCost)
        {
            string strRetVal = string.Empty;

            for (int i = ownerUnitList.Count - 1; i >= 0; i--)
            {
                if (ownerUnitList.Count <= i)
                {
                    continue;
                }

                List<EnchantmentInstance> eiList = ownerUnitList[i].GetEnchantments();

                for (int j = eiList.Count - 1; j >= 0; j--)
                {
                    if (ownerUnitList.Count <= i)
                    {
                        continue;
                    }

                    if (eiList.Count <= j)
                    {
                        continue;
                    }

                    // Dispel only enchantments that allows dispel.
                    if (eiList[j].source.Get().allowDispel)
                    {
                        if (eiList[j].source.Get().enchCategory == EEnchantmentCategory.Negative)
                        {
                            bool bSimulated = ownerUnitList[i].simulated;
                            if (GetDispelSuccess(playerWizardAsCaster, eiList[j], fDispelCost, bSimulated))
                            {
                                if (!bSimulated)
                                {
                                    string newRemEch = eiList[j].source.Get().GetDILocalizedName();
                                    strRetVal = strRetVal.Length > 0 ? strRetVal + ", " + newRemEch : newRemEch;
                                }

                                ownerUnitList[i].RemoveEnchantment(eiList[j].source.Get());
                            }
                        }
                    }
                }
            }

            return strRetVal;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="enemyUnitList"></param>
        /// <param name="playerWizardAsCaster">can be null</param>
        /// <param name="playerWizardDispeller"></param>
        /// <param name="fDispelCost"></param>
        /// <param name="strEnchants"></param>
        /// <returns>updated string containing dispelled enchantments</returns>
        private static string DispelEnchantsFromEnemyUnits(List<BattleUnit> enemyUnitList, PlayerWizard playerWizardAsCaster, PlayerWizard playerWizardDispeller, FInt fDispelCost, string strEnchants)
        {
            string strRetVal = strEnchants;

            for (int i = enemyUnitList.Count - 1; i >= 0; i--)
            {
                if (enemyUnitList.Count <= i)
                {
                    continue;
                }

                List<EnchantmentInstance> eiList = enemyUnitList[i].GetEnchantments();

                if (enemyUnitList[i].isSpellLock)
                {
                    for (int j = 0; j < eiList.Count; j++)
                    {
                        if (enemyUnitList.Count <= i)
                        {
                            continue;
                        }

                        if (eiList.Count <= j)
                        {
                            continue;
                        }

                        Enchantment ench = eiList[j].source.Get();

                        // Dispel only enchantments that allows dispel.
                        if (ench.allowDispel)
                        {
                            if (ench.scripts == null)
                            {
                                continue;
                            }

                            EnchantmentScript esSpellLock = Array.Find(ench.scripts, o => o.tag == (Tag)TAG.SPELL_LOCK);
                            if (esSpellLock != null)
                            {
                                bool bSimulated = enemyUnitList[i].simulated;
                                if (GetDispelSuccess(playerWizardAsCaster, eiList[j], fDispelCost, esSpellLock.fIntData, bSimulated))
                                {
                                    if (!bSimulated)
                                    {
                                        string newRemEch = ench.GetDILocalizedName();
                                        strRetVal = strRetVal.Length > 0 ? strRetVal + ", " + newRemEch : newRemEch;
                                    }

                                    enemyUnitList[i].RemoveEnchantment(ench);
                                }

                                break;
                            }
                        }
                    }
                }
                else // isSpellLock == false
                {
                    for (int j = eiList.Count - 1; j >= 0; j--)
                    {
                        if (enemyUnitList.Count <= i)
                        {
                            continue;
                        }

                        if (eiList.Count <= j)
                        {
                            continue;
                        }

                        Enchantment ench = eiList[j].source.Get();
                        //Dispel only enchantments that allows dispel.
                        if (ench.allowDispel)
                        {
                            if (eiList[j].owner != playerWizardDispeller)
                            {
                                bool bSimulated = enemyUnitList[i].simulated;

                                if (GetDispelSuccess(playerWizardAsCaster, eiList[j], fDispelCost, bSimulated))
                                {
                                    if (!bSimulated)
                                    {
                                        string newRemEch = ench.GetDILocalizedName();
                                        strRetVal = strRetVal.Length > 0 ? strRetVal + ", " + newRemEch : newRemEch;
                                    }

                                    enemyUnitList[i].RemoveEnchantment(ench);
                                }
                            }
                        }
                    }
                }
            }
            return strRetVal;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="playerWizardAsCaster"></param>
        /// <param name="fDispelCost"></param>
        /// <param name="strEnchants"></param>
        /// <returns></returns>
        private static string DispelEnchantsFromBattle(Battle battle, PlayerWizard playerWizardAsCaster, PlayerWizard playerWizardDispeller, FInt fDispelCost, string strEnchants)
        {
            string strRetVal = strEnchants;
            bool bSimulated  = battle.simulation;

            List<EnchantmentInstance> eiList = battle.GetEnchantments();

            for (int i = eiList.Count - 1; i >= 0; i--)
            {
                if (eiList.Count <= i)
                {
                    continue;
                }

                if (eiList[i].owner != playerWizardDispeller)
                {
                    //check if caster is a unit
                    BattleUnit bu = eiList[i].owner?.GetEntity() as BattleUnit;
                    if (bu != null)
                    {
                        if (bu.GetWizardOwner() == playerWizardDispeller)
                        {
                            continue;
                        }
                    }

                    Enchantment ench = eiList[i].source.Get();
                    //Dispel only enchantments that allow dispel.
                    if (ench.allowDispel)
                    {
                        if (GetDispelSuccess(playerWizardAsCaster, eiList[i], fDispelCost, bSimulated))
                        {
                            if (!bSimulated)
                            {
                                string strRemovedEnchs = ench.GetDILocalizedName();
                                strRetVal = strRetVal.Length > 0 ? strRetVal + ", " + strRemovedEnchs : strRemovedEnchs;
                            }

                            battle.RemoveEnchantment(ench);
                        }
                    }
                }
            }

            return strRetVal;
        }

        #endregion
    }
}

#endif