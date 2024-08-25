#if !USE_DEBUG_SCRIPT || !UNITY_EDITOR

using DBDef;
using DBEnum;
using MHUtils;
using MOM;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MOMScripts
{
    public class UIScripts : ScriptBase
    {
        #region Sorting scripts
        [ScriptType(ScriptType.Type.UISortScript)]
        [ScriptParameters("UI_SORT_BY_NAME")]
        static public void AZSort(List<MOM.Location> list)
        {
            list.Sort(delegate (MOM.Location a, MOM.Location b)
            {
                return a.name.CompareTo(b.name);
            });
        }

        [ScriptType(ScriptType.Type.UISortScript)]
        [ScriptParameters("UI_SORT_BY_RACE")]
        static public void TownByRaceSort(List<MOM.Location> list)
        {
            list.Sort(delegate (MOM.Location a, MOM.Location b)
            {
                string nameA = (a as TownLocation).race.dbName;
                string nameB = (b as TownLocation).race.dbName;
                return nameA.CompareTo(nameB);
            });
        }

        [ScriptType(ScriptType.Type.UISortScript)]
        [ScriptParameters("UI_SORT_BY_POPULATION")]
        static public void TownByPopulationSort(List<MOM.Location> list)
        {
            list.Sort(delegate (MOM.Location a, MOM.Location b)
            {
                return -(a as TownLocation).Population.CompareTo((b as TownLocation).Population);
            });
        }

        [ScriptType(ScriptType.Type.UISortScript)]
        [ScriptParameters("UI_SORT_BY_FOOD_INCOME")]
        static public void TownByFoodSort(List<MOM.Location> list)
        {
            list.Sort(delegate (MOM.Location a, MOM.Location b)
            {
                return -(a as TownLocation).CalculateFoodFinalIncome().CompareTo((b as TownLocation).CalculateFoodFinalIncome());
            });
        }

        [ScriptType(ScriptType.Type.UISortScript)]
        [ScriptParameters("UI_SORT_BY_GOLD_INCOME")]
        static public void TownByGoldSort(List<MOM.Location> list)
        {
            list.Sort(delegate (MOM.Location a, MOM.Location b)
            {
                return -(a as TownLocation).CalculateMoneyIncome(true).CompareTo((b as TownLocation).CalculateMoneyIncome(true));
            });
        }

        [ScriptType(ScriptType.Type.UISortScript)]
        [ScriptParameters("UI_SORT_BY_PRODUCTION_INCOME")]
        static public void TownByProductionSort(List<MOM.Location> list)
        {
            list.Sort(delegate (MOM.Location a, MOM.Location b)
            {
                return -(a as TownLocation).CalculateProductionIncome().CompareTo((b as TownLocation).CalculateProductionIncome());
            });
        }

        [ScriptType(ScriptType.Type.UISortScript)]
        [ScriptParameters("UI_SORT_BY_REBELS")]
        static public void TownByRebelsSort(List<MOM.Location> list)
        {
            list.Sort(delegate (MOM.Location a, MOM.Location b)
            {
                return -(a as TownLocation).GetRebels().CompareTo((b as TownLocation).GetRebels());
            });
        }

        [ScriptType(ScriptType.Type.UISortScript)]
        [ScriptParameters("UI_SORT_BY_DATE")]
        static public void TownByConqueredTurnSort(List<MOM.Location> list)
        {
            list.Sort(delegate (MOM.Location a, MOM.Location b)
            {
                return (a as TownLocation).conqueredTurn.CompareTo((b as TownLocation).conqueredTurn);
            });
        }
        #endregion
    }
}
#endif