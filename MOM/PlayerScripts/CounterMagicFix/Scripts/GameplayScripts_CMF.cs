/**********************************
 *
 * Author:  Dorian Gray
 * Date:    May 19, 2024
 * Version: 1.0.4
 *
 **********************************/

#if !USE_DEBUG_SCRIPT || !UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine;
using DBDef;
using MHUtils;
using MOM;
using DBEnum;
using GameScript;
using MOMScripts_CMF;

namespace GameScript_CMF
{
    using static UserUtility.Utility;
    public class GameplayScripts : ScriptBase
    {
        /// <summary>
        /// enables verbose logging
        /// </summary>
        private const bool bLoggingEnabled = true;
        /// <summary>
        /// controls rather spell battle costs will be range-adjusted or not
        /// </summary>
        private const bool bCMBattleCostByDistance = false;

        private static bool DispellWorldSpell(Enchantment eDispellingEnch, Spell spell, PlayerWizard spellCaster)
        {
            if (bLoggingEnabled)
            {
                Debug.Log("invoking [DispellWorldSpell] ...");
            }

            if (spell == null || eDispellingEnch == null || spellCaster == null)
            {
                Debug.LogWarning("  [DispellWorldSpell] - one or more invalid parameters passed");
                return false;
            }

            float fDispelCost = eDispellingEnch.scripts[0].fIntData.ToFloat();
            float fSpellCost  = spell.GetWorldCastingCost(spellCaster, true);
            float chance      = fDispelCost / (fDispelCost + fSpellCost);
            int spellCounteredSucceses = new MHRandom().GetSuccesses(chance, 1);
            List<EnchantmentInstance> eiList = GameManager.Get().GetEnchantments().FindAll(o => o.source.Get() == eDispellingEnch);
            bool bDispelled = spellCounteredSucceses > 0;

            if (bLoggingEnabled)
            {
                bool bHumanTarget = false;

                string strSpellName      = string.Format("{0}({1})", GetSpellName(spell), fSpellCost);
                string strDispellingEnch = string.Format("{0}({1})", GetName(eDispellingEnch), fDispelCost);

                if (IsOwnerHuman(spellCaster))
                {
                    bHumanTarget = true;
                }

                if (bHumanTarget)
                {
                    Debug.LogFormat("  AI DispellWorldSpell {0,-24} vs  {1,-24} Chance:{2,-5:P1} Countered?:{3}",
                                    strDispellingEnch, strSpellName, chance, bDispelled);
                }
                else
                {
                    Debug.LogFormat("  DispellWorldSpell    {0,-24} vs  {1,-24} Chance:{2,-5:P1} Countered?:{3}",
                                    strDispellingEnch, strSpellName, chance, bDispelled);
                }
            }

            if (bDispelled)
            {
                EnchantmentInstance eiCM1 = eiList.Find(o => o.owner == null);
                EnchantmentInstance eiCM2 = eiList.Find(o => o.owner.ID != spellCaster.ID);

                if (eiCM1 != null || eiCM2 != null)
                {
                    if (bLoggingEnabled)
                    {
                        if (eiCM1 != null)
                        {
                            string strEnch    = GetNameOwnerID(eiCM1);
                            string strCaster2 = GetNameOwnerID((ISpellCaster)spellCaster);
                            Debug.LogFormat("    Ench:{0} Caster:{1}", strEnch, strCaster2);
                        }
                        if (eiCM2 != null)
                        {
                            string strEnch    = GetNameOwnerID(eiCM2);
                            string strCaster2 = GetNameOwnerID((ISpellCaster)spellCaster);
                            Debug.LogFormat("    Ench:{0} Caster:{1}", strEnch, strCaster2);
                        }
                    }

                    if (PlayerWizard.HumanID() == spellCaster.ID)
                    {
                        PopupGeneral.OpenPopup(null, "UI_COUNTER_MAGIC", "UI_SPELL_COUNTERED", "UI_UHH");
                    }
                    return true;
                }
            }

            if (bLoggingEnabled)
            {
                Debug.Log("   dispel attempt aborted - invalid spellCaster");
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="battle"></param>
        /// <param name="eDispellingEnch"></param>
        /// <param name="spell"></param>
        /// <param name="spellCaster"></param>
        /// <returns></returns>
        private static bool NodeDispelBattleSpell(Battle battle, Enchantment eDispellingEnch, Spell spell, ISpellCaster spellCaster)
        {
            if (battle == null || eDispellingEnch == null || spell == null || spellCaster == null)
            {
                Debug.LogWarning("  [NodeDispelBattleSpell] - one or more invalid parameters passed");
                return false;
            }

            bool bSimulated = battle.simulation;
            bool bHumanTarget = false;

            if (IsOwnerHuman(spellCaster))
            {
                bHumanTarget = true;
            }

            if (bLoggingEnabled && !bSimulated)
            {
                string strCaster = GetNameOwnerID(spellCaster);
                string strSpell  = GetSpellNameBattleCost(spell);
                if (bHumanTarget)
                {
                    Debug.LogFormat("  AI invoking [NodeDispelBattleSpell] vs Spell:{0} castBy:{1} ...", 
                                     strSpell, strCaster);
                }
                else
                {
                    Debug.LogFormat("  invoking [NodeDispelBattleSpell] vs   Spell:{0} castBy:{1} ...",
                                     strSpell, strCaster);
                }
            }

            // Obtain the Nodes dispelCost value from the XML data
            float dispelCost = eDispellingEnch.scripts[0].fIntData.ToFloat();
            float spellCost = 1;
 
            if (bCMBattleCostByDistance)
            {
            }
            else
            {
                spellCost = spell.battleCost;
            }

            float chance        = dispelCost / (dispelCost + spellCost);
            int iSpellCountered = new MHRandom().GetSuccesses(chance, 1);
            List<EnchantmentInstance> eiList  = battle.GetEnchantments().FindAll(o => o.source.Get() == eDispellingEnch);
            bool bDispelled = (iSpellCountered > 0);

            if (bLoggingEnabled && !bSimulated)
            {
                string strSpellName      = string.Format("{0}({1})", GetSpellName(spell), spellCost);
                string strDispellingEnch = string.Format("{0}({1})", GetName(eDispellingEnch), dispelCost);
                string strCasterName     = GetNameOwnerID(spellCaster);

                if (bHumanTarget)
                {
                    Debug.LogFormat("  AI {0,-24} vs {1,-24} Chance:{2,-5:P1} Caster:{3,-15} Dispel?:{4}",
                                    strDispellingEnch, strSpellName, chance, strCasterName, bDispelled);

                }
                else
                {
                    Debug.LogFormat("     {0,-24} vs {1,-24} Chance:{2,-5:P1} Caster:{3,-15} Dispel?:{4}",
                                    strDispellingEnch, strSpellName, chance, strCasterName, bDispelled);
                }

            }

            if (bDispelled && !bSimulated)
            {
                if (bLoggingEnabled)
                {
                    Debug.LogFormat("    searching battle enchantments Count({0})", eiList.Count);
                }

                foreach (EnchantmentInstance ei in eiList)
                {
                    //That code block search for situations where spell/ench is allow to cast
                    if ((ei.owner == null) && (spellCaster == null))
                    {
                        if (bLoggingEnabled)
                        {
                            Debug.Log("   dispel attempt aborted - ei.owner && spellCaster == null");
                        }

                        return false;
                    }
                    else if ((ei.owner != null) && (spellCaster != null))
                    {
                        if (ei.owner.GetEntity() is PlayerWizard &&
                           (ei.owner.GetEntity() as PlayerWizard).GetID() == spellCaster.GetWizardOwnerID())
                        {
                            if (bLoggingEnabled)
                            {
                                Debug.Log("   dispel attempt aborted - will not dispel own spell");
                            }

                            return false;
                        }
                        else if (ei.owner.GetEntity() is BattleUnit &&
                                (ei.owner.GetEntity() as BattleUnit).GetWizardOwnerID() == spellCaster.GetWizardOwnerID())
                        {
                            if (bLoggingEnabled)
                            {
                                Debug.Log("    dispel attempt aborted - will not dispel on own unit");
                            }

                            return false;
                        }
                    }
                }

                if (IsOwnerHuman(spellCaster))
                {
                    PopupGeneral.OpenPopup(null, "UI_COUNTER_MAGIC", "UI_SPELL_COUNTERED", "UI_UHH");
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Modified version of DispellBattleSpell
        /// </summary>
        /// <param name="battle"></param>
        /// <param name="ei">Dispelling Enchantment</param>
        /// <param name="spell">Target Spell</param>
        /// <param name="spellCaster"></param>
        /// <returns></returns>
        private static bool DispelBattleSpell2(Battle battle, EnchantmentInstance ei, Spell spell, ISpellCaster spellCaster)
        {
            if (battle == null || ei == null || spell == null || spellCaster == null)
            {
                Debug.LogWarning("  [DispelBattleSpell2] - one or more invalid parameters passed");
                return false;
            }

            bool bSimulated = battle.simulation;

            if (bLoggingEnabled && !bSimulated)
            {
                string strCaster = GetNameOwnerID(spellCaster);
                string strSpell  = GetSpellNameBattleCost(spell);
                Debug.LogFormat("  invoking [DispelBattleSpell2] Spell:{0} castBy:{1}", strSpell, strCaster);
            }

            bool bDispelled        = false;
            bool bValidTargetFound = false;
            int iWizardOwnerID     = spellCaster.GetWizardOwnerID();

            //////////////////////////////////////////////
            //
            //   Search for valid spell targets first
            //

            if (ei.owner != null)
            {
                if (ei.owner.GetEntity() is PlayerWizard &&
                   (ei.owner.GetEntity() as PlayerWizard).GetID() != iWizardOwnerID)
                {
                    if (bLoggingEnabled && !bSimulated)
                    {
                        string strCaster    = GetNameOwnerID(spellCaster);
                        string strSpellName = GetSpellNameBattleCost(spell);
                        Debug.LogFormat("    Target spell found:[{0}] castBy:{1}", strSpellName, strCaster);
                    }
                    bValidTargetFound = true;
                }
                else if (ei.owner.GetEntity() is BattleUnit &&
                        (ei.owner.GetEntity() as BattleUnit).GetWizardOwnerID() != iWizardOwnerID)
                {
                    if (bLoggingEnabled && !bSimulated)
                    {
                        string strCaster    = GetNameOwnerID(spellCaster);
                        string strSpellName = GetSpellNameBattleCost(spell);
                        Debug.LogFormat("    Target spell found:[{0}] castBy:{1}", strSpellName, strCaster);
                    }
                    bValidTargetFound = true;
                }
            }


            //////////////////////////////////////////////
            //
            //   Target Spell Acquired
            //

            if (bValidTargetFound)
            {
                // float dispelStr = dispellingEnch.scripts[0].fIntData.ToFloat(); // looks wrong

                int iCMPool = 0;
                bool bIsCasterOwnerHuman = IsOwnerHuman(spellCaster);
                if (bIsCasterOwnerHuman)
                {
                    iCMPool = EnchantmentScripts.GetCounterMagicPool(EnchantmentScripts.CM_TYPE.CM_AI);
                }
                else
                {
                    iCMPool = EnchantmentScripts.GetCounterMagicPool(EnchantmentScripts.CM_TYPE.CM_HUMAN);
                }

                 float fDispelCost = iCMPool;
              //  float fDispelCost   = (float)ei.dispelCost;

                float fSpellCost = 1;

                if (bCMBattleCostByDistance)
                {
                }
                else
                {
                    fSpellCost = spell.GetBattleCastingCost(spellCaster, true);
                }

                //////////////////////////////////////////////
                //
                //   Attempt to Dispel the Target Spell
                //
                //   Dispel Chance (%) = (Dispel Casting Cost / (Dispel Casting Cost + Spell Casting Cost)) × 100
                //
                float fChance       = fDispelCost / (fDispelCost + fSpellCost);
                int iSpellCountered = new MHRandom().GetSuccesses(fChance, 1);
                bDispelled          = (iSpellCountered > 0);

                string strCombatLog = "";
                if (bIsCasterOwnerHuman)
                {
                    int iNewValue = EnchantmentScripts.DecrCounterMagicPool(5, EnchantmentScripts.CM_TYPE.CM_AI);
                    strCombatLog = string.Format("AI Counter Magic Pool:{0} -> {1}", iCMPool, iNewValue);

                    if (iNewValue == 0)
                    {
                        if (bLoggingEnabled && !bSimulated)
                        {
                            Debug.LogFormat("  Removing Enchantment:{0}", GetEnchantmentName(ei));
                        }
                        // need to end CM enchant
                        battle.RemoveEnchantment(ei);
                    }
                }
                else
                {
                    int iNewValue = EnchantmentScripts.DecrCounterMagicPool(5, EnchantmentScripts.CM_TYPE.CM_HUMAN);
                    strCombatLog = string.Format("Counter Magic Pool:{0} -> {1}", iCMPool, iNewValue);

                    if (iNewValue == 0)
                    {
                        if (bLoggingEnabled && !bSimulated)
                        {
                            Debug.LogFormat("  Removing Enchantment:{0}", GetEnchantmentName(ei));
                        }
                        // need to end CM enchant 
                        battle.RemoveEnchantment(ei);
                    }
                }

                BattleHUD.CombatLogAdd(strCombatLog);

                if (bLoggingEnabled && !bSimulated)
                {
                    Debug.Log(strCombatLog);
                    bool bHumanTarget = false;

                    if (IsOwnerHuman(spellCaster))
                    {
                        bHumanTarget = true;
                    }

                    string strSpellName      = string.Format("[{0}]({1})", GetSpellName(spell), fSpellCost);
                    string strDispellingEnch = string.Format("[{0}]({1})", GetEnchantmentName(ei), fDispelCost);
                    string strCasterName     = GetNameOwnerID(spellCaster);

                    if (bHumanTarget)
                    {
                        Debug.LogFormat("    AI {0,-24} vs {1,-24} Chance:{2,-5:P1} Caster:{3,-15} Dispel?:{4}",
                                        strDispellingEnch, strSpellName, fChance, strCasterName, bDispelled);

                    }
                    else
                    {
                        Debug.LogFormat("       {0,-24} vs {1,-24} Chance:{2,-5:P1} Caster:{3,-15} Dispel?:{4}",
                                        strDispellingEnch, strSpellName, fChance, strCasterName, bDispelled);
                    }
                }
            } // if (bValidTargetFound)

            if (bDispelled)
            {
                // is human player target?
                if (IsOwnerHuman(spellCaster))
                {
                    PopupGeneral.OpenPopup(null, "UI_COUNTER_MAGIC", "UI_SPELL_COUNTERED", "UI_UHH");
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="battle"></param>
        /// <param name="spell"></param>
        /// <param name="spellCaster"></param>
        /// <returns>true if spell is countered</returns>
        public static bool CounterMagicBattle(Battle battle, Spell spell, ISpellCaster spellCaster)
        {
            bool bCounterMagicActive = (battle != null) ? (battle.battleCounterMagic > 0) : false;
            bool bSimulated = false;
            if (battle != null)
            {
                bSimulated = battle.simulation;
            }
            else
            {
                Debug.LogWarning("[CounterMagicBattle] battle == null");
                return false;
            }

            PlayerWizard playerWizard = spellCaster is BattleUnit ? (spellCaster as BattleUnit).GetWizardOwner() : spellCaster as PlayerWizard;

            // are we fighting on a Node and does playerWizard have Node Mastery?

            if (battle.gDefender != null &&
                battle.gDefender.GetLocationHost() != null &&
                battle.gDefender.GetLocationHost().locationType == ELocationType.Node)
            {
                if (playerWizard != null && HasTrait(playerWizard, TRAIT.NODE_MASTERY))
                {
                    return false;
                }
            }

            if (bCounterMagicActive)
            {
                if (bLoggingEnabled && !bSimulated)
                {
                    string strSpellName = GetSpellNameBattleCost(spell);
                    string strCaster    = GetNameOwnerID(spellCaster);
                    Debug.LogFormat("invoking [CounterMagicBattle] vs Spell:{0} castBy:{1} ...", strSpellName, strCaster);
                }
                Enchantment enchCounterMagic = DataBase.Get<DBDef.Enchantment>(ENCH.COUNTER_MAGIC, false);
                Enchantment enchCounterMagicNodeChaos = DataBase.Get<DBDef.Enchantment>(ENCH.MAGIC_NODE_CHAOS_COUNTER_MAGIC, false);
                Enchantment enchCounterMagicNodeNature = DataBase.Get<DBDef.Enchantment>(ENCH.MAGIC_NODE_NATURE_COUNTER_MAGIC, false);
                Enchantment enchCounterMagicNodeSorcery = DataBase.Get<DBDef.Enchantment>(ENCH.MAGIC_NODE_SORCERY_COUNTER_MAGIC, false);

                // make a local copy of the ieList to iterate over
                List<EnchantmentInstance> ieList = new List<EnchantmentInstance>(battle.GetEnchantments());
                foreach (EnchantmentInstance cmEnch in ieList)
                {
                    if (enchCounterMagic == cmEnch.source.Get())
                    {
                    //    if (DispellBattleSpell(battle, enchCounterMagic, spell, spellCaster))
                          if (DispelBattleSpell2(battle, cmEnch, spell, spellCaster))
                        {
                            return true;
                        }
                    }
                    //node will not dispel magic from same realm
                    else if (spell.realm != cmEnch.source.Get().realm)
                    {
                        if (enchCounterMagicNodeChaos == cmEnch.source.Get())
                        {
                            if (NodeDispelBattleSpell(battle, enchCounterMagicNodeChaos, spell, spellCaster))
                            {
                                return true;
                            }
                        }
                        if (enchCounterMagicNodeNature == cmEnch.source.Get())
                        {
                            if (NodeDispelBattleSpell(battle, enchCounterMagicNodeNature, spell, spellCaster))
                            {
                                return true;
                            }
                        }
                        if (enchCounterMagicNodeSorcery == cmEnch.source.Get())
                        {
                            if (NodeDispelBattleSpell(battle, enchCounterMagicNodeSorcery, spell, spellCaster))
                            {
                                return true;
                            }
                        }
                    }
                }
            }  // if (bCounterMagicActive)

            return false;
        }

    } // public class GameplayScripts
} // namespace MOMScripts_CMF

#endif