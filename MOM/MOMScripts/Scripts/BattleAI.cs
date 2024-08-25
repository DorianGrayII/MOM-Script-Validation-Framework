#if !USE_DEBUG_SCRIPT || !UNITY_EDITOR

using DBDef;
using DBEnum;
using MHUtils;
using MOM;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace GameScript
{
    public class BattleAI : ScriptBase
    {
        static public void AITurn(Battle battle, bool manageAttacker)
        {
            List<BattleUnit> allyUnits;
            List<BattleUnit> enemyUnits;

            if (manageAttacker)
            {
                allyUnits = battle.attackerUnits;
                enemyUnits = battle.defenderUnits;
            }
            else
            {
                allyUnits = battle.defenderUnits;
                enemyUnits = battle.attackerUnits;
            }

            foreach(var v in allyUnits)
            {
                if (v.FigureCount() > 0)
                {
                    UnitTurn(v, allyUnits, enemyUnits, battle);
                }
            }
        }

        static public void UnitTurn(BattleUnit u, 
                                    List<BattleUnit> allyUnits, 
                                    List<BattleUnit> enemyUnits,
                                    Battle battle)
        {

        }

    }
}
#endif