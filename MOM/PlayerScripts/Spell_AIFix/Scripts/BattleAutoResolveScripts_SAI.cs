/**********************************
 *
 * Author:  Dorian Gray
 * Date:    May 18, 2024
 * Version: 1.0.0
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
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using WorldCode;

namespace MOMScripts_SAI
{
    public class BattleAutoResolveScripts : ScriptBase
    {
        public static bool IsSpellValidForAutoresolves(Spell spell)
        {
            if (string.IsNullOrEmpty(spell.battleScript))
            {
                return false;
            }

            return spell.targetType.enumType == ETargetType.TargetUnit ||
                spell.summonFantasticUnitSpell
                ? true
                : false;
        }
        public static void TestSpells(ISpellCaster caster, List<BattleUnit> ownBattleUnitList, List<BattleUnit> enemyBattleUnitList)
        {
            List<Spell> spellList = DataBase.GetType<Spell>();
            int failedIndex = 0;
            SpellCastData data = new SpellCastData(caster, ownBattleUnitList, enemyBattleUnitList);

            foreach (Spell spell in spellList)
            {    
                data.DebugUnitRefresh();
                if (!IsSpellValidForAutoresolves(spell))
                {
                    continue;
                }

                string strLastScriptName = "";
                try
                {
                    if (spell.targetType.enumType == ETargetType.TargetUnit)
                    {
                        bool bSpellEval = false;
                        bool bTargetSucceeds = false;
                        bool bSpellCastSucceeds = false;
                        List<BattleUnit> buList;
                        if (spell.targetType == (TargetType)TARGET_TYPE.UNIT_ENEMY)
                        {
                            buList = enemyBattleUnitList;
                        }
                        else
                        {
                            buList = ownBattleUnitList;
                        }

                        foreach (BattleUnit bu in buList)
                        {
                            bSpellEval      = false;
                            bTargetSucceeds = false;
                            bSpellCastSucceeds = false;
                            int iAiSpellEvalValue = 0;
                            bool bCanCastAtTarget = false;

                            if (!string.IsNullOrEmpty(spell.targetingScript))
                            {
                                strLastScriptName = spell.targetingScript;
                                bCanCastAtTarget = (bool)ScriptLibrary.CallNoCatch(spell.targetingScript, data, bu, spell);
                            }
                            if (bCanCastAtTarget)
                            {
                                bTargetSucceeds = true;

                                if (!string.IsNullOrEmpty(spell.aiBattleEvaluationScript))
                                {
                                    iAiSpellEvalValue = 0;
                                    strLastScriptName = spell.aiBattleEvaluationScript;
                                    iAiSpellEvalValue = (int)ScriptLibrary.CallNoCatch(spell.aiBattleEvaluationScript, data, bu, spell);
                                }
                                else
                                {
                                    //simulate cast to get actual value if needed. not recommended during test casts
                                    iAiSpellEvalValue = 10;
                                }
                                if (iAiSpellEvalValue > 0)
                                {
                                    bSpellEval = true;
                                    strLastScriptName = spell.battleScript;
                                    bSpellCastSucceeds = (bool)ScriptLibrary.CallNoCatch(spell.battleScript, data, bu, spell);
                                    if (bSpellCastSucceeds)
                                    {
                                        break;
                                    }
                                }
                            }
                        }

                        if (!bTargetSucceeds)
                        {
                            Debug.LogWarning((++failedIndex) + " Failed targeting script " + spell.targetingScript + 
                                " in spell " + spell.dbName + " making it not possible to test evaluation and execution of the spell ");
                        }
                        else if (!bSpellEval)
                        {
                            Debug.LogWarning((++failedIndex) + " Failed evaluation script " + spell.aiBattleEvaluationScript + 
                                " in spell " + spell.dbName + " making it not possible to test execution of the spell ");
                        }                        
                        else if(! bSpellCastSucceeds)
                        {
                            Debug.LogWarning((++failedIndex) + " Failed execution script " + spell.battleScript + 
                                " in spell " + spell.dbName);
                        }
                    }   
                    else if (spell.summonFantasticUnitSpell)
                    {
                        bool bSpellEval = false;
                        bool bTargetSucceeds = false;
                        bool bSpellCastSucceeds = false;
                        int iAISpellEvalValue = 0;
                        bool bCanCastAtTarget = false;

                        if (!string.IsNullOrEmpty(spell.targetingScript))
                        {
                            strLastScriptName = spell.targetingScript;
                            bCanCastAtTarget = (bool)ScriptLibrary.CallNoCatch(spell.targetingScript, data, Vector3i.invalid, spell);
                        }
                        if (bCanCastAtTarget)
                        {
                            bTargetSucceeds = true;
                            if (!string.IsNullOrEmpty(spell.aiBattleEvaluationScript))
                            {
                                strLastScriptName = spell.aiBattleEvaluationScript;
                                iAISpellEvalValue = (int)ScriptLibrary.CallNoCatch(spell.aiBattleEvaluationScript, data, Vector3i.invalid, spell);
                                
                            }
                            else
                            {
                                //simulate cast to get actual value if needed. not recommended during test casts
                                iAISpellEvalValue = 10;
                            }
                            if (iAISpellEvalValue != 0)
                            {
                                bSpellEval = true;
                                 
                                strLastScriptName = spell.battleScript;
                                bSpellCastSucceeds = (bool)ScriptLibrary.CallNoCatch(spell.battleScript, data, Vector3i.invalid, spell);
                                if (bSpellCastSucceeds)
                                {
                                    break;
                                }
                            }
                        }

                        if (!bTargetSucceeds)
                        {
                            Debug.LogWarning((++failedIndex) + " Failed targeting script " + spell.targetingScript + 
                                " in spell " + spell.dbName + " making it not possible to test evaluation and execution of the spell ");
                        }
                        else if (!bSpellEval)
                        {
                            Debug.LogWarning((++failedIndex) + " Failed evaluation script " + spell.aiBattleEvaluationScript + 
                                " in spell " + spell.dbName + " making it not possible to test execution of the spell ");
                        }
                        else if (!bSpellCastSucceeds)
                        {
                            Debug.LogWarning((++failedIndex) + " Failed execution script " + spell.battleScript + 
                                " in spell " + spell.dbName);
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Spell " + spell.dbName + " had exception when calling script "+ strLastScriptName + " \n" + e);
                }
            }
        }
    }
}
#endif