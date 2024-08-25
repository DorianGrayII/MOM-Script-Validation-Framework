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

namespace MOMScripts
{
    public class BattleAutoResolveScripts : ScriptBase
    {
        static public bool IsSpellValidForAutoresolves(Spell s)
        {
            if (string.IsNullOrEmpty(s.battleScript)) return false;
            if (s.targetType.enumType == ETargetType.TargetUnit || 
                s.summonFantasticUnitSpell )
            {
                return true;
            }

            return false;
        }
        static public void TestSpells(ISpellCaster caster, List<BattleUnit> ownUnits, List<BattleUnit> enemyUnits)
        {
            var spells = DataBase.GetType<Spell>();
            int failedIndex = 0;
            SpellCastData data = new SpellCastData(caster, ownUnits, enemyUnits);

            foreach (var s in spells)
            {    
                data.DebugUnitRefresh();
                if (!IsSpellValidForAutoresolves(s)) continue;
                string scriptName = "";
                try
                {
                    if (s.targetType.enumType == ETargetType.TargetUnit)
                    {
                        bool evaluation = false;
                        bool targetsSucceed = false;
                        bool executionSucceed = false;
                        List<BattleUnit> list;
                        if (s.targetType == (TargetType)TARGET_TYPE.UNIT_ENEMY)
                        {
                            list = enemyUnits;
                        }
                        else
                        {
                            list = ownUnits;
                        }

                        foreach (var u in list)
                        {
                            int value = 0;
                            bool valid = true;
                            

                            if (!string.IsNullOrEmpty(s.targetingScript))
                            {
                                scriptName = s.targetingScript;
                                valid = (bool)ScriptLibrary.CallNoCatch(s.targetingScript, data, u, s);
                            }
                            if (!valid) continue;
                            targetsSucceed = true;

                            if (!string.IsNullOrEmpty(s.aiBattleEvaluationScript))
                            {
                                value = 0;
                                scriptName = s.aiBattleEvaluationScript;
                                value = (int)ScriptLibrary.CallNoCatch(s.aiBattleEvaluationScript, data, u, s);
                                
                            }
                            else
                            {
                                evaluation = true;
                                //simulate cast to get actual value if needed. not recommended during test casts
                                value = 10;
                            }
                            if (value == 0) continue;
                            evaluation = true;

                            if (valid)
                            {
                                targetsSucceed = true;
                                scriptName = s.battleScript;
                                valid = (bool)ScriptLibrary.CallNoCatch(s.battleScript, data, u, s);
                                if (valid)
                                {
                                    executionSucceed = true;
                                    break;
                                }
                            }
                        }

                        if (!targetsSucceed)
                        {
                            Debug.LogWarning((++failedIndex) + " Failed targeting script " + s.targetingScript + " in spell " + s.dbName + " making it not possible to test evaluation and execution of the spell ");
                        }
                        else if (!evaluation)
                        {
                            Debug.LogWarning((++failedIndex) + " Failed evaluation script " + s.aiBattleEvaluationScript + " in spell " + s.dbName + " making it not possible to test execution of the spell ");
                        }                        
                        else if(! executionSucceed)
                        {
                            Debug.LogWarning((++failedIndex) + " Failed execution script " + s.battleScript + " in spell " + s.dbName);
                        }
                    }   
                    else if(s.summonFantasticUnitSpell)
                    {
                        bool evaluation = false;
                        bool targetsSucceed = false;
                        bool executionSucceed = false;
                        int value = 0;
                        bool valid = true;

                        if (!string.IsNullOrEmpty(s.targetingScript))
                        {
                            scriptName = s.targetingScript;
                            valid = (bool)ScriptLibrary.CallNoCatch(s.targetingScript, data, Vector3i.invalid, s);
                        }
                        if (valid)
                        {
                            targetsSucceed = true;
                            if (!string.IsNullOrEmpty(s.aiBattleEvaluationScript))
                            {
                                scriptName = s.aiBattleEvaluationScript;
                                value = (int)ScriptLibrary.CallNoCatch(s.aiBattleEvaluationScript, data, Vector3i.invalid, s);
                                
                            }
                            else
                            {
                                evaluation = true;
                                //simulate cast to get actual value if needed. not recommended during test casts
                                value = 10;
                            }
                            if (value != 0)
                            {
                                evaluation = true;
                                 
                                scriptName = s.battleScript;
                                valid = (bool)ScriptLibrary.CallNoCatch(s.battleScript, data, Vector3i.invalid, s);
                                if (valid)
                                {
                                    executionSucceed = true;
                                    break;
                                }
                            }
                        }

                        if (!targetsSucceed)
                        {
                            Debug.LogWarning((++failedIndex) + " Failed targeting script " + s.targetingScript + " in spell " + s.dbName + " making it not possible to test evaluation and execution of the spell ");
                        }
                        else if (!evaluation)
                        {
                            Debug.LogWarning((++failedIndex) + " Failed evaluation script " + s.aiBattleEvaluationScript + " in spell " + s.dbName + " making it not possible to test execution of the spell ");
                        }
                        else if (!executionSucceed)
                        {
                            Debug.LogWarning((++failedIndex) + " Failed execution script " + s.battleScript + " in spell " + s.dbName);
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("Spell " + s.dbName + " had exception when calling script "+ scriptName + " \n" + e);
                }
            }
        }
    }
}
#endif