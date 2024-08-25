#if !USE_DEBUG_SCRIPT || !UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using DBDef;
using MHUtils;
using MOM;
using MOM.Adventures;
using System;
using WorldCode;
using Plane = DBDef.Plane;
using Terrain = DBDef.Terrain;
using Unit = DBDef.Unit;
using Group = MOM.Group;
using System.Linq;
using DBEnum;

namespace MOMScripts
{
    public class EditorScripts : ScriptBase
    {
        public enum ETerrainCategory
        {
            Highland,
            Lowland,
            NonGrassland,
            Seashore,
            Water,
        }
        public enum EMinedResources
        {
            SilverOre,
            GoldOre,
            IronOre,
            Coal,
            Gems,
            MithrilOre,
            AdamantineOre,
            QuorkCrystals,
            CrysxCrystals,
            Orichalcum,
            AnyMinedResource,
        }

        #region Filter with Tag
        // Wizard
        [ScriptType(ScriptType.Type.EditorWizardFilter)]
        [ScriptParameters(null, typeof(int))]
        [ScriptParameters(null, null)]
        [ScriptRetType(LogicEntry.LEntry.LEEntryWizard)]
        static public object FwT_AnyWizard(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                           Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            List<PlayerWizard> wiz = GameManager.GetWizards();
            LogicEntry le = advLogic as LogicEntry;
            int parameterValue = 0;
            List<object> l = new List<object>();

            if (!String.IsNullOrEmpty(le.scriptStringParameter))
            {
                float value = UTIL_StringParameterProcessor(le.scriptStringParameter).t1;
                parameterValue = Mathf.RoundToInt(value);
            }

            foreach (var w in wiz)
            {
                if (!UTIL_IsOwnerValid(w.ID, le.playerOwner, advData)) continue;
                l.Add(w);
            }

            if (parameterValue > 0)
            {
                l.RandomSortThreadSafe();
                l = l.GetRange(0, Mathf.Min(parameterValue, l.Count));
            }

            return AdvList.MakeList(l, advLogic, advData, localLists);
        }

        [ScriptType(ScriptType.Type.EditorWizardFilter)]
        [ScriptParameters(typeof(Race))]
        [ScriptRetType(LogicEntry.LEntry.LEEntryWizard)]
        static public object FwT_WizardByRace(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                              Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            List<PlayerWizard> wizards = GameManager.GetWizards();
            LogicEntry le = advLogic as LogicEntry;
            DBClass t = DataBase.Get(le.scriptTypeParameter, false);
            List<object> l = new List<object>();

            if (t != null)
            {
                foreach (var w in wizards)
                {
                    if (!UTIL_IsOwnerValid(w.ID, le.playerOwner, advData)) continue;

                    if (w.mainRace.Get() == t)
                    {
                        l.Add(w);
                    }
                }
            }
            return AdvList.MakeList(l, advLogic, advData, localLists);
        }

        [ScriptType(ScriptType.Type.EditorWizardFilter)]
        [ScriptParameters(typeof(Trait))]
        [ScriptRetType(LogicEntry.LEntry.LEEntryWizard)]
        static public object FwT_WizardByTrait(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                               Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            List<PlayerWizard> wizards = GameManager.GetWizards();
            LogicEntry le = advLogic as LogicEntry;
            DBClass t = DataBase.Get(le.scriptTypeParameter, false);
            List<object> l = new List<object>();

            if (t != null)
            {
                foreach (var w in wizards)
                {
                    if (!UTIL_IsOwnerValid(w.ID, le.playerOwner, advData)) continue;

                    if (w.HasTrait((Trait)t))
                    {
                        l.Add(w);
                    }
                }
            }
            return AdvList.MakeList(l, advLogic, advData, localLists);
        }

        [ScriptType(ScriptType.Type.EditorWizardFilter)]
        [ScriptParameters(typeof(Tag), typeof(FInt))]
        [ScriptParameters(typeof(Tag), null)]
        [ScriptRetType(LogicEntry.LEntry.LEEntryWizard)]
        static public object FwT_WizardByMagic(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                               Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            List<PlayerWizard> wizards = GameManager.GetWizards();
            LogicEntry le = advLogic as LogicEntry;
            DBClass t = DataBase.Get(le.scriptTypeParameter, false);
            FInt parameterValue = FInt.ONE;
            List<object> l = new List<object>();

            if (!String.IsNullOrEmpty(le.scriptStringParameter))
            {
                float value = UTIL_StringParameterProcessor(le.scriptStringParameter).t1;
                parameterValue = new FInt(value);
            }

            if (t != null)
            {
                Tag tag = (Tag)t;
                foreach (var w in wizards)
                {
                    if (!UTIL_IsOwnerValid(w.ID, le.playerOwner, advData)) continue;

                    if (w.GetAttributes().Contains(tag, parameterValue))
                    {
                        l.Add(w);
                    }
                }
            }
            return AdvList.MakeList(l, advLogic, advData, localLists);
        }

        [ScriptType(ScriptType.Type.EditorWizardFilter)]
        [ScriptParameters(typeof(LogicUtils.Comparison), typeof(FInt))]
        [ScriptParameters(typeof(LogicUtils.Comparison), null)]
        [ScriptRetType(LogicEntry.LEntry.LEEntryWizard)]
        static public object FwT_WizardByFame(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                              Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            List<PlayerWizard> wizards = GameManager.GetWizards();
            LogicEntry le = advLogic as LogicEntry;
            LogicUtils.Comparison lComp = LogicUtils.Comparison.MoreOrEqualThan;
            if (!string.IsNullOrEmpty(le.scriptTypeParameter))
            {
                lComp = (LogicUtils.Comparison)Enum.Parse(typeof(LogicUtils.Comparison), le.scriptTypeParameter);
            }
            int parameterValue = 1;
            List<object> l = new List<object>();

            if (!String.IsNullOrEmpty(le.scriptStringParameter))
            {
                float value = UTIL_StringParameterProcessor(le.scriptStringParameter).t1;
                parameterValue = Mathf.RoundToInt(value);
            }

            foreach (var w in wizards)
            {
                if (!UTIL_IsOwnerValid(w.ID, le.playerOwner, advData)) continue;

                if (UTIL_IsValueValid(w.GetFame(), parameterValue, lComp))
                {
                    l.Add(w);
                }
            }

            return AdvList.MakeList(l, advLogic, advData, localLists);
        }

        [ScriptType(ScriptType.Type.EditorWizardFilter)]
        [ScriptParameters(typeof(PlayerWizard.Familiar))]
        [ScriptRetType(LogicEntry.LEntry.LEEntryWizard)]
        static public object FwT_WizardByFamiliar(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                  Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            List<PlayerWizard> wizards = GameManager.GetWizards();
            LogicEntry le = advLogic as LogicEntry;
            if (string.IsNullOrEmpty(le.scriptTypeParameter))
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; parameter is null or empty");
            }
            PlayerWizard.Familiar f = (PlayerWizard.Familiar)Enum.Parse(typeof(PlayerWizard.Familiar), le.scriptTypeParameter);

            List<object> l = new List<object>();

            foreach (var w in wizards)
            {
                if (!UTIL_IsOwnerValid(w.ID, le.playerOwner, advData)) continue;

                if (w.familiar == f)
                {
                    l.Add(w);
                }
            }

            return AdvList.MakeList(l, advLogic, advData, localLists);
        }

        [ScriptType(ScriptType.Type.EditorWizardFilter)]
        [ScriptParameters(typeof(LogicUtils.Comparison), typeof(FInt))]
        [ScriptParameters(typeof(LogicUtils.Comparison), null)]
        [ScriptRetType(LogicEntry.LEntry.LEEntryWizard)]
        static public object FwT_WizardByNumberOfCity(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                      Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            List<PlayerWizard> wizards = GameManager.GetWizards();
            LogicEntry le = advLogic as LogicEntry;
            LogicUtils.Comparison lComp = LogicUtils.Comparison.MoreOrEqualThan;
            if (!string.IsNullOrEmpty(le.scriptTypeParameter))
            {
                lComp = (LogicUtils.Comparison)Enum.Parse(typeof(LogicUtils.Comparison), le.scriptTypeParameter);
            }
            int parameterValue = 1;
            List<object> l = new List<object>();
            var loc = GameManager.Get().registeredLocations;

            if (!String.IsNullOrEmpty(le.scriptStringParameter))
            {
                float value = UTIL_StringParameterProcessor(le.scriptStringParameter).t1;
                parameterValue = Mathf.RoundToInt(value);
            }

            foreach (var w in wizards)
            {
                if (!UTIL_IsOwnerValid(w.ID, le.playerOwner, advData)) continue;
                int cities = 0;
                if (loc != null)
                {
                    foreach (var v in loc)
                    {
                        if (v is TownLocation &&
                            v.owner == w.ID)
                        {
                            cities++;
                        }
                    }
                }
                if (UTIL_IsValueValid(cities, parameterValue, lComp))
                {
                    l.Add(w);
                }
            }

            return AdvList.MakeList(l, advLogic, advData, localLists);
        }

        [ScriptType(ScriptType.Type.EditorWizardFilter)]
        [ScriptParameters(typeof(LogicUtils.Comparison), typeof(FInt))]
        [ScriptParameters(typeof(LogicUtils.Comparison), null)]
        [ScriptRetType(LogicEntry.LEntry.LEEntryWizard)]
        static public object FwT_WizardByPopulationOverall(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                           Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            List<PlayerWizard> wizards = GameManager.GetWizards();
            LogicEntry le = advLogic as LogicEntry;
            LogicUtils.Comparison lComp = LogicUtils.Comparison.MoreOrEqualThan;
            if (!string.IsNullOrEmpty(le.scriptTypeParameter))
            {
                lComp = (LogicUtils.Comparison)Enum.Parse(typeof(LogicUtils.Comparison), le.scriptTypeParameter);
            }
            int parameterValue = 1;
            List<object> l = new List<object>();
            var loc = GameManager.Get().registeredLocations;

            if (!String.IsNullOrEmpty(le.scriptStringParameter))
            {
                float value = UTIL_StringParameterProcessor(le.scriptStringParameter).t1;
                parameterValue = Mathf.RoundToInt(value);
            }

            foreach (var w in wizards)
            {
                if (!UTIL_IsOwnerValid(w.ID, le.playerOwner, advData)) continue;
                int population = 0;
                if (loc != null)
                {
                    foreach (var v in loc)
                    {
                        if (v is TownLocation &&
                            v.owner == w.ID)
                        {
                            population += (v as TownLocation).GetPopUnits();
                        }
                    }
                }
                if (UTIL_IsValueValid(population, parameterValue, lComp))
                {
                    l.Add(w);
                }
            }

            return AdvList.MakeList(l, advLogic, advData, localLists);
        }

        [ScriptType(ScriptType.Type.EditorWizardFilter)]
        [ScriptParameters(typeof(LogicUtils.Comparison), typeof(int))]
        [ScriptRetType(LogicEntry.LEntry.LEEntryWizard)]
        static public object FwT_WizardByMana(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                              Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            List<PlayerWizard> wizards = GameManager.GetWizards();
            LogicEntry le = advLogic as LogicEntry;
            float requiredValue = UTIL_StringParameterProcessor(le.scriptStringParameter).t1;
            int iRequiredValue = Mathf.RoundToInt(requiredValue);
            LogicUtils.Comparison lComp = LogicUtils.Comparison.MoreOrEqualThan;
            if (!string.IsNullOrEmpty(le.scriptTypeParameter))
            {
                lComp = (LogicUtils.Comparison)Enum.Parse(typeof(LogicUtils.Comparison), le.scriptTypeParameter);
            }
            List<object> l = new List<object>();

            foreach (var w in wizards)
            {
                if (!UTIL_IsOwnerValid(w.ID, le.playerOwner, advData)) continue;

                if (UTIL_IsValueValid(w.GetMana(), iRequiredValue, lComp))
                {
                    l.Add(w);
                }
            }

            return AdvList.MakeList(l, advLogic, advData, localLists);
        }

        [ScriptType(ScriptType.Type.EditorWizardFilter)]
        [ScriptParameters(typeof(LogicUtils.Comparison), typeof(int))]
        [ScriptRetType(LogicEntry.LEntry.LEEntryWizard)]
        static public object FwT_WizardByManaPerTurn(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                     Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            List<PlayerWizard> wizards = GameManager.GetWizards();
            LogicEntry le = advLogic as LogicEntry;
            float requiredValue = UTIL_StringParameterProcessor(le.scriptStringParameter).t1;
            int iRequiredValue = Mathf.RoundToInt(requiredValue);
            LogicUtils.Comparison lComp = LogicUtils.Comparison.MoreOrEqualThan;
            if (!string.IsNullOrEmpty(le.scriptTypeParameter))
            {
                lComp = (LogicUtils.Comparison)Enum.Parse(typeof(LogicUtils.Comparison), le.scriptTypeParameter);
            }
            List<object> l = new List<object>();

            foreach (var w in wizards)
            {
                if (!UTIL_IsOwnerValid(w.ID, le.playerOwner, advData)) continue;

                if (UTIL_IsValueValid(w.CalculateManaIncome(true), iRequiredValue, lComp))
                {
                    l.Add(w);
                }
            }

            return AdvList.MakeList(l, advLogic, advData, localLists);
        }

        [ScriptType(ScriptType.Type.EditorWizardFilter)]
        [ScriptParameters(typeof(LogicUtils.Comparison), typeof(int))]
        [ScriptRetType(LogicEntry.LEntry.LEEntryWizard)]
        static public object FwT_WizardByGold(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                              Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            List<PlayerWizard> wizards = GameManager.GetWizards();
            LogicEntry le = advLogic as LogicEntry;
            float requiredValue = UTIL_StringParameterProcessor(le.scriptStringParameter).t1;
            int iRequiredValue = Mathf.RoundToInt(requiredValue);
            LogicUtils.Comparison lComp = LogicUtils.Comparison.MoreOrEqualThan;
            if (!string.IsNullOrEmpty(le.scriptTypeParameter))
            {
                lComp = (LogicUtils.Comparison)Enum.Parse(typeof(LogicUtils.Comparison), le.scriptTypeParameter);
            }
            List<object> l = new List<object>();

            foreach (var w in wizards)
            {
                if (!UTIL_IsOwnerValid(w.ID, le.playerOwner, advData)) continue;

                if (UTIL_IsValueValid(w.money, iRequiredValue, lComp))
                {
                    l.Add(w);
                }
            }

            return AdvList.MakeList(l, advLogic, advData, localLists);
        }

        [ScriptType(ScriptType.Type.EditorWizardFilter)]
        [ScriptParameters(typeof(LogicUtils.Comparison), typeof(int))]
        [ScriptRetType(LogicEntry.LEntry.LEEntryWizard)]
        static public object FwT_WizardByGoldPerTurn(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                     Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            List<PlayerWizard> wizards = GameManager.GetWizards();
            LogicEntry le = advLogic as LogicEntry;
            float requiredValue = UTIL_StringParameterProcessor(le.scriptStringParameter).t1;
            int iRequiredValue = Mathf.RoundToInt(requiredValue);
            LogicUtils.Comparison lComp = LogicUtils.Comparison.MoreOrEqualThan;
            if (!string.IsNullOrEmpty(le.scriptTypeParameter))
            {
                lComp = (LogicUtils.Comparison)Enum.Parse(typeof(LogicUtils.Comparison), le.scriptTypeParameter);
            }
            List<object> l = new List<object>();

            foreach (var w in wizards)
            {
                if (!UTIL_IsOwnerValid(w.ID, le.playerOwner, advData)) continue;

                if (UTIL_IsValueValid(w.CalculateMoneyIncome(true), iRequiredValue, lComp))
                {
                    l.Add(w);
                }
            }

            return AdvList.MakeList(l, advLogic, advData, localLists);
        }

        [ScriptType(ScriptType.Type.EditorWizardFilter)]
        [ScriptParameters(typeof(LogicUtils.Comparison), typeof(int))]
        [ScriptRetType(LogicEntry.LEntry.LEEntryWizard)]
        static public object FwT_WizardByFoodPerTurn(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                     Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            List<PlayerWizard> wizards = GameManager.GetWizards();
            LogicEntry le = advLogic as LogicEntry;
            float requiredValue = UTIL_StringParameterProcessor(le.scriptStringParameter).t1;
            int iRequiredValue = Mathf.RoundToInt(requiredValue);
            LogicUtils.Comparison lComp = LogicUtils.Comparison.MoreOrEqualThan;
            if (!string.IsNullOrEmpty(le.scriptTypeParameter))
            {
                lComp = (LogicUtils.Comparison)Enum.Parse(typeof(LogicUtils.Comparison), le.scriptTypeParameter);
            }
            List<object> l = new List<object>();

            foreach (var w in wizards)
            {
                if (!UTIL_IsOwnerValid(w.ID, le.playerOwner, advData)) continue;

                if (UTIL_IsValueValid(w.CalculateFoodIncome(true), iRequiredValue, lComp))
                {
                    l.Add(w);
                }
            }

            return AdvList.MakeList(l, advLogic, advData, localLists);
        }

        [ScriptType(ScriptType.Type.EditorWizardFilter)]
        [ScriptParameters(typeof(Wizard))]
        [ScriptRetType(LogicEntry.LEntry.LEEntryWizard)]
        static public object FwT_WizardByName(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                              Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            List<PlayerWizard> wizards = GameManager.GetWizards();
            LogicEntry le = advLogic as LogicEntry;
            DBClass t = DataBase.Get(le.scriptTypeParameter, false);
            List<object> l = new List<object>();

            if (t != null)
            {
                foreach (var w in wizards)
                {
                    if (!UTIL_IsOwnerValid(w.ID, le.playerOwner, advData)) continue;

                    if (w.IsWizard(t as Wizard) && !w.isCustom)
                    {
                        l.Add(w);
                    }
                }
            }
            return AdvList.MakeList(l, advLogic, advData, localLists);
        }

        [ScriptType(ScriptType.Type.EditorWizardFilter)]
        [ScriptParameters(typeof(Enchantment))]
        [ScriptRetType(LogicEntry.LEntry.LEEntryWizard)]
        static public object FwT_WizardByEnchantment(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                     Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            List<PlayerWizard> wizards = GameManager.GetWizards();
            LogicEntry le = advLogic as LogicEntry;
            DBClass t = DataBase.Get(le.scriptTypeParameter, false);
            List<object> l = new List<object>();

            if (t != null)
            {
                foreach (var w in wizards)
                {
                    if (!UTIL_IsOwnerValid(w.ID, le.playerOwner, advData)) continue;

                    Enchantment ench = (Enchantment)t;
                    foreach (var e in w.GetEnchantments())
                    {
                        if (e.source == ench)
                        {
                            l.Add(w);
                        }
                    }
                }
            }
            return AdvList.MakeList(l, advLogic, advData, localLists);
        }

        [ScriptType(ScriptType.Type.EditorWizardFilter)]
        [ScriptParameters(typeof(Spell))]
        [ScriptRetType(LogicEntry.LEntry.LEEntryWizard)]
        static public object FwT_WizardByLearnedSpell(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                    Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            List<PlayerWizard> wizards = GameManager.GetWizards();
            LogicEntry le = advLogic as LogicEntry;
            DBClass t = DataBase.Get(le.scriptTypeParameter, false);
            List<object> l = new List<object>();

            if (t != null)
            {
                foreach (var w in wizards)
                {
                    if (!UTIL_IsOwnerValid(w.ID, le.playerOwner, advData)) continue;

                    Spell spell = (Spell)t;
                    foreach (var s in w.GetSpells())
                    {
                        if (s == spell)
                        {
                            l.Add(w);
                        }
                    }
                }
            }
            return AdvList.MakeList(l, advLogic, advData, localLists);
        }
        [ScriptType(ScriptType.Type.EditorWizardFilter)]
        [ScriptRetType(LogicEntry.LEntry.LEEntryWizard)]
        static public object FwT_WizardByHealingSpell(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                    Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            List<PlayerWizard> wizards = GameManager.GetWizards();
            LogicEntry le = advLogic as LogicEntry;
            List<object> l = new List<object>();

            foreach (var w in wizards)
            {
                if (!UTIL_IsOwnerValid(w.ID, le.playerOwner, advData)) continue;

                var spell = w.GetSpells().Find(o => o.Get().healingSpell);
                if (spell != null)
                {
                    l.Add(w);
                }
            }

            return AdvList.MakeList(l, advLogic, advData, localLists);
        }


        // Heroes
        [ScriptType(ScriptType.Type.EditorHeroFilter)]
        [ScriptParameters(null, typeof(int))]
        [ScriptParameters(null, null)]
        [ScriptRetType(LogicEntry.LEntry.LEEntryHero)]
        static public object FwT_AnyHero(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                         Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            var groups = GameManager.Get().registeredGroups;
            LogicEntry le = advLogic as LogicEntry;
            int parameterValue = 0;
            List<object> l = new List<object>();

            if (!String.IsNullOrEmpty(le.scriptStringParameter))
            {
                float value = UTIL_StringParameterProcessor(le.scriptStringParameter).t1;
                parameterValue = Mathf.RoundToInt(value);
            }

            foreach (var g in groups)
            {
                if (!UTIL_IsOwnerValid(g.GetOwnerID(), le.playerOwner, advData)) continue;
                if (g.GetLocationHost()?.otherPlaneLocation?.Get() != null && g.plane.arcanusType) continue;

                foreach (var u in g.GetUnits())
                {
                    if (u.Get().dbSource.Get() is Hero)
                    {
                        l.Add(u.Get());
                    }
                }
            }

            if (parameterValue > 0)
            {
                l.RandomSortThreadSafe();
                l = l.GetRange(0, Mathf.Min(parameterValue, l.Count));
            }

            return AdvList.MakeList(l, advLogic, advData, localLists);
        }

        [ScriptType(ScriptType.Type.EditorHeroFilter)]
        [ScriptParameters(typeof(Tag), typeof(FInt))]
        [ScriptRetType(LogicEntry.LEntry.LEEntryHero)]
        static public object FwT_HeroByTag(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                           Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            var groups = GameManager.Get().registeredGroups;
            LogicEntry le = advLogic as LogicEntry;
            DBClass t = DataBase.Get(le.scriptTypeParameter, false);
            FInt parameterValue = FInt.ONE;
            List<object> l = new List<object>();

            if (!String.IsNullOrEmpty(le.scriptStringParameter))
            {
                float value = UTIL_StringParameterProcessor(le.scriptStringParameter).t1;
                parameterValue = new FInt(value);
            }

            if (t != null)
            {
                Tag tag = (Tag)t;
                foreach (var g in groups)
                {
                    if (!UTIL_IsOwnerValid(g.GetOwnerID(), le.playerOwner, advData)) continue;
                    if (g.GetLocationHost()?.otherPlaneLocation?.Get() != null && g.plane.arcanusType) continue;

                    foreach (var u in g.GetUnits())
                    {
                        if (u.Get().dbSource.Get() is Hero &&
                            u.Get().GetAttributes().Contains(tag, parameterValue))
                        {
                            l.Add(u.Get());
                        }
                    }
                }
            }
            return AdvList.MakeList(l, advLogic, advData, localLists);
        }

        [ScriptType(ScriptType.Type.EditorHeroFilter)]
        [ScriptParameters(typeof(LogicUtils.Comparison), typeof(FInt))]
        static public object FwT_HeroByLevel(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            var groups = GameManager.Get().registeredGroups;
            LogicEntry le = advLogic as LogicEntry;
            LogicUtils.Comparison lComp = (LogicUtils.Comparison)Enum.Parse(typeof(LogicUtils.Comparison), le.scriptTypeParameter);
            List<object> l = new List<object>();

            int parameterValue = 1;
            if (!String.IsNullOrEmpty(le.scriptStringParameter))
            {
                float value = UTIL_StringParameterProcessor(le.scriptStringParameter).t1;
                parameterValue = Mathf.RoundToInt(value);
            }

            foreach (var g in groups)
            {
                if (!UTIL_IsOwnerValid(g.GetOwnerID(), le.playerOwner, advData)) continue;
                if (g.GetLocationHost()?.otherPlaneLocation?.Get() != null && g.plane.arcanusType) continue;

                foreach (var u in g.GetUnits())
                {
                    if (u.Get().dbSource.Get() is Hero &&
                        UTIL_IsValueValid(u.Get().GetLevel(), parameterValue, lComp))
                    {
                        l.Add(u.Get());
                    }
                }
            }

            return AdvList.MakeList(l, advLogic, advData, localLists);
        }

        [ScriptType(ScriptType.Type.EditorHeroFilter)]
        [ScriptParameters(typeof(Skill))]
        [ScriptRetType(LogicEntry.LEntry.LEEntryHero)]
        static public object FwT_HeroBySkill(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                             Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            var groups = GameManager.Get().registeredGroups;
            LogicEntry le = advLogic as LogicEntry;
            DBClass t = DataBase.Get(le.scriptTypeParameter, false);
            List<object> l = new List<object>();

            if (t != null)
            {
                foreach (var g in groups)
                {
                    if (!UTIL_IsOwnerValid(g.GetOwnerID(), le.playerOwner, advData)) continue;
                    if (g.GetLocationHost()?.otherPlaneLocation?.Get() != null && g.plane.arcanusType) continue;

                    foreach (var u in g.GetUnits())
                    {
                        if (u.Get().dbSource.Get() is Hero &&
                            u.Get().GetSkills().Contains(t as Skill))
                        {
                            l.Add(u.Get());
                        }
                    }
                }
            }
            return AdvList.MakeList(l, advLogic, advData, localLists);
        }

        [ScriptType(ScriptType.Type.EditorHeroFilter)]
        [ScriptRetType(LogicEntry.LEntry.LEEntryHero)]
        static public object FwT_HeroByAnyEquipment(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                    Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            var groups = GameManager.Get().registeredGroups;
            LogicEntry le = advLogic as LogicEntry;
            List<object> l = new List<object>();

            foreach (var g in groups)
            {
                if (!UTIL_IsOwnerValid(g.GetOwnerID(), le.playerOwner, advData)) continue;
                if (g.GetLocationHost()?.otherPlaneLocation?.Get() != null && g.plane.arcanusType) continue;

                foreach (var u in g.GetUnits())
                {
                    if (u.Get().dbSource.Get() is Hero)
                    {
                        if (u.Get().artefactManager != null && u.Get().artefactManager.equipmentSlots.Find(o => o.item != null) != null)
                        {
                            l.Add(u.Get());
                        }
                    }
                }
            }
            return AdvList.MakeList(l, advLogic, advData, localLists);
        }

        [ScriptType(ScriptType.Type.EditorHeroFilter)]
        [ScriptParameters(typeof(Hero))]
        [ScriptRetType(LogicEntry.LEntry.LEEntryHero)]
        static public object FwT_HeroByName(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                            Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            var groups = GameManager.Get().registeredGroups;
            LogicEntry le = advLogic as LogicEntry;
            DBClass t = DataBase.Get(le.scriptTypeParameter, false);
            List<object> l = new List<object>();

            if (t != null)
            {
                foreach (var g in groups)
                {
                    if (!UTIL_IsOwnerValid(g.GetOwnerID(), le.playerOwner, advData)) continue;
                    if (g.GetLocationHost()?.otherPlaneLocation?.Get() != null && g.plane.arcanusType) continue;

                    foreach (var u in g.GetUnits())
                    {
                        if (u.Get().dbSource.Get().Equals(t))
                        {
                            l.Add(u.Get());
                        }
                    }
                }

                if (advData.mainPlayerWizard > 0)
                {
                    var w = GameManager.GetWizard(advData.mainPlayerWizard);
                    foreach (var h in w.GetDeadHeroes())
                    {
                        if (h.dbSource.Get().Equals(t))
                        {
                            l.Add(h);
                        }
                    }
                }
            }
            return AdvList.MakeList(l, advLogic, advData, localLists);
        }

        [ScriptType(ScriptType.Type.EditorHeroFilter)]
        [ScriptParameters(typeof(Hero))]
        [ScriptRetType(LogicEntry.LEntry.LEEntryHero)]
        static public object FwT_LivingHeroByName(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                            Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            var groups = GameManager.Get().registeredGroups;
            LogicEntry le = advLogic as LogicEntry;
            DBClass t = DataBase.Get(le.scriptTypeParameter, false);
            List<object> l = new List<object>();

            if (t != null)
            {
                foreach (var g in groups)
                {
                    if (!UTIL_IsOwnerValid(g.GetOwnerID(), le.playerOwner, advData)) continue;
                    if (g.GetLocationHost()?.otherPlaneLocation?.Get() != null && g.plane.arcanusType) continue;

                    foreach (var u in g.GetUnits())
                    {
                        if (u.Get().dbSource.Get().Equals(t))
                        {
                            l.Add(u.Get());
                        }
                    }
                }
            }
            return AdvList.MakeList(l, advLogic, advData, localLists);
        }

        // City
        [ScriptType(ScriptType.Type.EditorCityFilter)]
        [ScriptParameters(null, typeof(int))]
        [ScriptParameters(null, null)]
        [ScriptRetType(LogicEntry.LEntry.LEEntryCity)]
        static public object FwT_AnyCity(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                         Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            var locs = GameManager.Get().registeredLocations;
            LogicEntry le = advLogic as LogicEntry;
            int parameterValue = 0;

            if (!String.IsNullOrEmpty(le.scriptStringParameter))
            {
                float value = UTIL_StringParameterProcessor(le.scriptStringParameter).t1;
                parameterValue = Mathf.RoundToInt(value);
            }

            List<object> l = new List<object>();
            foreach (var loc in locs)
            {
                if (!UTIL_IsOwnerValid(loc.owner, le.playerOwner, advData)) continue;
                if (loc is TownLocation)
                {
                    if (!UTIL_IsCityTypeValid(le.cityType, loc as TownLocation, advData)) continue;

                    l.Add(loc);
                }
            }

            if (parameterValue > 1)
            {
                l.RandomSortThreadSafe();
                l = l.GetRange(0, Mathf.Min(parameterValue, l.Count));
            }

            return AdvList.MakeList(l, advLogic, advData, localLists);
        }

        [ScriptType(ScriptType.Type.EditorCityFilter)]
        [ScriptParameters(typeof(Town))]
        [ScriptRetType(LogicEntry.LEntry.LEEntryCity)]
        static public object FwT_CityByRace(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                             Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            var locs = GameManager.Get().registeredLocations;
            LogicEntry le = advLogic as LogicEntry;
            DBClass t = DataBase.Get(le.scriptTypeParameter, false);
            List<object> l = new List<object>();

            if (t != null)
            {
                foreach (var loc in locs)
                {
                    if (!UTIL_IsOwnerValid(loc.owner, le.playerOwner, advData)) continue;
                    if (loc is TownLocation)
                    {
                        if (!UTIL_IsCityTypeValid(le.cityType, loc as TownLocation, advData)) continue;
                        if ((loc as TownLocation).race.Get() == (t as Town).race)
                        {
                            l.Add(loc);
                        }
                    }
                }
            }

            return AdvList.MakeList(l, advLogic, advData, localLists);
        }

        [ScriptType(ScriptType.Type.EditorCityFilter)]
        [ScriptParameters(typeof(Plane))]
        [ScriptRetType(LogicEntry.LEntry.LEEntryCity)]
        static public object FwT_CityByPlane(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                             Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            var locs = GameManager.Get().registeredLocations;
            LogicEntry le = advLogic as LogicEntry;
            DBClass t = DataBase.Get(le.scriptTypeParameter, false);
            List<object> l = new List<object>();

            if (t != null)
            {
                foreach (var loc in locs)
                {
                    if (!UTIL_IsOwnerValid(loc.owner, le.playerOwner, advData)) continue;
                    if (loc is TownLocation)
                    {
                        if (!UTIL_IsCityTypeValid(le.cityType, loc as TownLocation, advData)) continue;
                        if ((loc as TownLocation).GetPlane().planeSource.Get() == t)
                        {
                            l.Add(loc);
                        }
                    }
                }
            }

            return AdvList.MakeList(l, advLogic, advData, localLists);
        }


        [ScriptType(ScriptType.Type.EditorCityFilter)]
        [ScriptParameters(typeof(Building))]
        [ScriptRetType(LogicEntry.LEntry.LEEntryCity)]
        static public object FwT_CityByBuilding(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            var locs = GameManager.Get().registeredLocations;
            LogicEntry le = advLogic as LogicEntry;
            DBClass t = DataBase.Get(le.scriptTypeParameter, false);

            List<object> l = new List<object>();
            foreach (var loc in locs)
            {
                if (!UTIL_IsOwnerValid(loc.owner, le.playerOwner, advData)) continue;
                if (loc is TownLocation)
                {
                    if (!UTIL_IsCityTypeValid(le.cityType, loc as TownLocation, advData)) continue;
                    if ((loc as TownLocation).HaveBuilding(t as DBDef.Building))
                    {
                        l.Add(loc);
                    }
                }
            }

            return AdvList.MakeList(l, advLogic, advData, localLists);
        }

        [ScriptType(ScriptType.Type.EditorCityFilter)]
        [ScriptParameters(typeof(ETerrainCategory))]
        [ScriptRetType(LogicEntry.LEntry.LEEntryCity)]
        static public object FwT_CityByTerrainType(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                   Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            var locs = GameManager.Get().registeredLocations;
            LogicEntry le = advLogic as LogicEntry;
            ETerrainCategory terrainCat = (ETerrainCategory)Enum.Parse(typeof(ETerrainCategory), le.scriptTypeParameter);
            List<object> l = new List<object>();

            foreach (var loc in locs)
            {
                if (!UTIL_IsOwnerValid(loc.owner, le.playerOwner, advData)) continue;
                if (loc is TownLocation)
                {
                    if (!UTIL_IsCityTypeValid(le.cityType, loc as TownLocation, advData)) continue;

                    var hex = loc.GetPlane().GetHexAt(loc.GetPosition());
                    if (UTIL_IsTerrainTypeValid(terrainCat, hex, loc))
                    {
                        l.Add(loc);
                    }
                }
            }
            return AdvList.MakeList(l, advLogic, advData, localLists);
        }

        [ScriptType(ScriptType.Type.EditorCityFilter)]
        [ScriptParameters(typeof(LogicUtils.Comparison), typeof(Multitype<float, float, float>))]
        [ScriptParameters(typeof(LogicUtils.Comparison), typeof(int))]
        [ScriptRetType(LogicEntry.LEntry.LEEntryCity)]
        static public object FwT_CityByUnrest(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                              Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            var locs = GameManager.Get().registeredLocations;
            LogicEntry le = advLogic as LogicEntry;
            LogicUtils.Comparison lComp = LogicUtils.Comparison.MoreOrEqualThan;
            if (!string.IsNullOrEmpty(le.scriptTypeParameter))
            {
                lComp = (LogicUtils.Comparison)Enum.Parse(typeof(LogicUtils.Comparison), le.scriptTypeParameter);
            }
            FInt requiredValue = FInt.ONE;
            List<object> l = new List<object>();

            if (!String.IsNullOrEmpty(le.scriptStringParameter))
            {
                var value = UTIL_StringParameterProcessor(le.scriptStringParameter).t1;
                requiredValue = new FInt(value);
            }

            foreach (var loc in locs)
            {
                if (!UTIL_IsOwnerValid(loc.owner, le.playerOwner, advData)) continue;

                if (loc is TownLocation)
                {
                    TownLocation t = loc as TownLocation;
                    if (!UTIL_IsCityTypeValid(le.cityType, t, advData)) continue;

                    FInt unrestPercent = (FInt)t.GetUnrest();
                    if (unrestPercent > 0)
                    {
                        if (UTIL_IsValueValid(unrestPercent, requiredValue * 0.01f, lComp))
                        {
                            l.Add(loc);
                        }
                    }
                }
            }

            return AdvList.MakeList(l, advLogic, advData, localLists);
        }
        [ScriptType(ScriptType.Type.EditorCityFilter)]
        [ScriptParameters(typeof(LogicUtils.Comparison), typeof(Multitype<float, float, float>))]
        [ScriptParameters(typeof(LogicUtils.Comparison), typeof(int))]
        [ScriptRetType(LogicEntry.LEntry.LEEntryCity)]
        static public object FwT_CityByRebelsToPopulation(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                              Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            var locs = GameManager.Get().registeredLocations;
            LogicEntry le = advLogic as LogicEntry;
            LogicUtils.Comparison lComp = LogicUtils.Comparison.MoreOrEqualThan;
            if (!string.IsNullOrEmpty(le.scriptTypeParameter))
            {
                lComp = (LogicUtils.Comparison)Enum.Parse(typeof(LogicUtils.Comparison), le.scriptTypeParameter);
            }
            FInt requiredValue = FInt.ONE;
            List<object> l = new List<object>();

            if (!String.IsNullOrEmpty(le.scriptStringParameter))
            {
                var value = UTIL_StringParameterProcessor(le.scriptStringParameter).t1;
                requiredValue = new FInt(value);
            }

            foreach (var loc in locs)
            {
                if (!UTIL_IsOwnerValid(loc.owner, le.playerOwner, advData)) continue;

                if (loc is TownLocation)
                {
                    TownLocation t = loc as TownLocation;
                    if (!UTIL_IsCityTypeValid(le.cityType, t, advData)) continue;

                    var rebels = t.GetRebels();
                    FInt rebelsToPopulation = FInt.ZERO;
                    if (rebels > 0)
                        rebelsToPopulation = (FInt)rebels / t.GetPopUnits();

                    if (UTIL_IsValueValid(rebelsToPopulation, requiredValue * 0.01f, lComp))
                    {
                        l.Add(loc);
                    }
                }
            }

            return AdvList.MakeList(l, advLogic, advData, localLists);
        }

        [ScriptType(ScriptType.Type.EditorCityFilter)]
        [ScriptParameters(typeof(Enchantment))]
        [ScriptRetType(LogicEntry.LEntry.LEEntryCity)]
        static public object FwT_CityByEnchantment(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                   Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            var locs = GameManager.Get().registeredLocations;
            LogicEntry le = advLogic as LogicEntry;
            DBClass t = DataBase.Get(le.scriptTypeParameter, false);
            List<object> l = new List<object>();

            if (t != null)
            {
                Enchantment ench = (Enchantment)t;
                foreach (var loc in locs)
                {
                    if (!UTIL_IsOwnerValid(loc.owner, le.playerOwner, advData)) continue;
                    if (loc is TownLocation)
                    {
                        TownLocation town = loc as TownLocation;
                        if (!UTIL_IsCityTypeValid(le.cityType, town, advData)) continue;
                        foreach (var e in town.GetEnchantments())
                        {
                            if (e.source == ench)
                            {
                                l.Add(loc);
                            }
                        }
                    }
                }
            }

            return AdvList.MakeList(l, advLogic, advData, localLists);
        }

        [ScriptType(ScriptType.Type.EditorCityFilter)]
        [ScriptParameters(typeof(LogicUtils.Comparison), typeof(FInt))]
        [ScriptRetType(LogicEntry.LEntry.LEEntryCity)]
        static public object FwT_CityByPopulation(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                  Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            var locs = GameManager.Get().registeredLocations;
            LogicEntry le = advLogic as LogicEntry;
            float requiredValue = UTIL_StringParameterProcessor(le.scriptStringParameter).t1;
            int iRequiredValue = Mathf.RoundToInt(requiredValue);
            LogicUtils.Comparison lComp = LogicUtils.Comparison.MoreOrEqualThan;
            if (!string.IsNullOrEmpty(le.scriptTypeParameter))
            {
                lComp = (LogicUtils.Comparison)Enum.Parse(typeof(LogicUtils.Comparison), le.scriptTypeParameter);
            }
            List<object> l = new List<object>();

            foreach (var loc in locs)
            {
                if (!UTIL_IsOwnerValid(loc.owner, le.playerOwner, advData)) continue;
                if (loc is TownLocation)
                {
                    TownLocation town = loc as TownLocation;
                    if (!UTIL_IsCityTypeValid(le.cityType, town, advData)) continue;
                    if (UTIL_IsValueValid(town.GetPopUnits(), iRequiredValue, lComp))
                    {
                        l.Add(loc);
                    }
                }
            }

            return AdvList.MakeList(l, advLogic, advData, localLists);
        }

        // Units
        [ScriptType(ScriptType.Type.EditorUnitFilter)]
        [ScriptParameters(null, typeof(int))]
        [ScriptParameters(null, null)]
        [ScriptRetType(LogicEntry.LEntry.LEEntryUnit)]
        static public object FwT_AnyUnit(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                         Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            var groups = GameManager.Get().registeredGroups;
            LogicEntry le = advLogic as LogicEntry;
            int parameterValue = 0;
            List<object> l = new List<object>();

            if (!String.IsNullOrEmpty(le.scriptStringParameter))
            {
                float value = UTIL_StringParameterProcessor(le.scriptStringParameter).t1;
                parameterValue = Mathf.RoundToInt(value);
            }

            foreach (var g in groups)
            {
                if (!UTIL_IsOwnerValid(g.GetOwnerID(), le.playerOwner, advData)) continue;
                if (g.GetLocationHost()?.otherPlaneLocation?.Get() != null && g.plane.arcanusType) continue;

                foreach (var u in g.GetUnits())
                {
                    l.Add(u.Get());
                }
            }

            if (parameterValue > 0)
            {
                l.RandomSortThreadSafe();
                l = l.GetRange(0, Mathf.Min(parameterValue, l.Count));
            }

            return AdvList.MakeList(l, advLogic, advData, localLists);
        }

        [ScriptType(ScriptType.Type.EditorUnitFilter)]
        [ScriptParameters(typeof(Race))]
        [ScriptRetType(LogicEntry.LEntry.LEEntryUnit)]
        static public object FwT_UnitByRace(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                            Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            var groups = GameManager.Get().registeredGroups;
            LogicEntry le = advLogic as LogicEntry;
            DBClass t = DataBase.Get(le.scriptTypeParameter, false);
            List<object> l = new List<object>();

            if (t != null)
            {
                foreach (var g in groups)
                {
                    if (!UTIL_IsOwnerValid(g.GetOwnerID(), le.playerOwner, advData)) continue;
                    if (g.GetLocationHost()?.otherPlaneLocation?.Get() != null && g.plane.arcanusType) continue;

                    foreach (var u in g.GetUnits())
                    {
                        if (u.Get().dbSource.Get() is Unit)
                        {
                            if (u.Get().race == t)
                            {
                                l.Add(u.Get());
                            }
                        }
                    }
                }
            }

            return AdvList.MakeList(l, advLogic, advData, localLists);
        }

        [ScriptType(ScriptType.Type.EditorUnitFilter)]
        [ScriptParameters(typeof(Unit))]
        [ScriptParameters(typeof(Hero))]
        [ScriptRetType(LogicEntry.LEntry.LEEntryUnit)]
        static public object FwT_UnitBySubrace(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                               Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            var groups = GameManager.Get().registeredGroups;
            LogicEntry le = advLogic as LogicEntry;
            DBClass t = DataBase.Get(le.scriptTypeParameter, false);
            List<object> l = new List<object>();

            if (t != null)
            {
                foreach (var g in groups)
                {
                    if (!UTIL_IsOwnerValid(g.GetOwnerID(), le.playerOwner, advData)) continue;
                    if (g.GetLocationHost()?.otherPlaneLocation?.Get() != null && g.plane.arcanusType) continue;

                    foreach (var u in g.GetUnits())
                    {
                        if (u.Get().dbSource.Get() == t)
                        {
                            l.Add(u.Get());
                        }
                    }
                }
            }
            return AdvList.MakeList(l, advLogic, advData, localLists);
        }

        [ScriptType(ScriptType.Type.EditorUnitFilter)]
        [ScriptParameters(typeof(Tag), typeof(FInt))]
        [ScriptParameters(typeof(Tag), null)]
        [ScriptRetType(LogicEntry.LEntry.LEEntryUnit)]
        static public object FwT_UnitByTag(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                           Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            var groups = GameManager.Get().registeredGroups;
            LogicEntry le = advLogic as LogicEntry;
            DBClass t = DataBase.Get(le.scriptTypeParameter, false);
            FInt parameterValue = FInt.ONE;
            List<object> l = new List<object>();

            if (!String.IsNullOrEmpty(le.scriptStringParameter))
            {
                float value = UTIL_StringParameterProcessor(le.scriptStringParameter).t1;
                parameterValue = new FInt(value);
            }

            if (t != null)
            {
                Tag tag = (Tag)t;
                foreach (var g in groups)
                {
                    if (!UTIL_IsOwnerValid(g.GetOwnerID(), le.playerOwner, advData)) continue;
                    if (g.GetLocationHost()?.otherPlaneLocation?.Get() != null && g.plane.arcanusType) continue;

                    foreach (var u in g.GetUnits())
                    {
                        if (u.Get().GetAttributes().Contains(tag, parameterValue))
                        {
                            l.Add(u.Get());
                        }
                    }
                }
            }
            return AdvList.MakeList(l, advLogic, advData, localLists);
        }

        [ScriptType(ScriptType.Type.EditorUnitFilter)]
        [ScriptParameters(typeof(LogicUtils.Comparison), typeof(int))]
        [ScriptRetType(LogicEntry.LEntry.LEEntryUnit)]
        static public object FwT_UnitByLevel(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                             Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            var groups = GameManager.Get().registeredGroups;
            LogicEntry le = advLogic as LogicEntry;
            float requiredLvl = UTIL_StringParameterProcessor(le.scriptStringParameter).t1;
            int iRequiredLevel = Mathf.RoundToInt(requiredLvl);
            LogicUtils.Comparison lComp = LogicUtils.Comparison.MoreOrEqualThan;
            if (!string.IsNullOrEmpty(le.scriptTypeParameter))
            {
                lComp = (LogicUtils.Comparison)Enum.Parse(typeof(LogicUtils.Comparison), le.scriptTypeParameter);
            }
            List<object> l = new List<object>();

            foreach (var g in groups)
            {
                if (!UTIL_IsOwnerValid(g.GetOwnerID(), le.playerOwner, advData)) continue;
                if (g.GetLocationHost()?.otherPlaneLocation?.Get() != null && g.plane.arcanusType) continue;

                foreach (var u in g.GetUnits())
                {
                    if (UTIL_IsValueValid(u.Get().GetLevel(), iRequiredLevel, lComp))
                    {
                        l.Add(u.Get());
                    }
                }
            }
            return AdvList.MakeList(l, advLogic, advData, localLists);
        }

        [ScriptType(ScriptType.Type.EditorUnitFilter)]
        [ScriptParameters(typeof(ETerrainCategory))]
        [ScriptRetType(LogicEntry.LEntry.LEEntryUnit)]
        static public object FwT_UnitByTerrain(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                               Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            var groups = GameManager.Get().registeredGroups;
            LogicEntry le = advLogic as LogicEntry;
            ETerrainCategory terrainCat = (ETerrainCategory)Enum.Parse(typeof(ETerrainCategory), le.scriptTypeParameter);
            List<object> l = new List<object>();

            foreach (var g in groups)
            {
                if (!UTIL_IsOwnerValid(g.GetOwnerID(), le.playerOwner, advData)) continue;
                if (g.GetLocationHost()?.otherPlaneLocation?.Get() != null && g.plane.arcanusType) continue;

                var hex = g.GetPlane().GetHexAt(g.GetPosition());
                if (UTIL_IsTerrainTypeValid(terrainCat, hex, g))
                {
                    foreach (var u in g.GetUnits())
                    {
                        l.Add(u.Get());
                    }
                }
            }
            return AdvList.MakeList(l, advLogic, advData, localLists);
        }

        [ScriptType(ScriptType.Type.EditorUnitFilter)]
        [ScriptParameters(typeof(Enchantment))]
        [ScriptRetType(LogicEntry.LEntry.LEEntryUnit)]
        static public object FwT_UnitByEnchantment(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                   Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            var groups = GameManager.Get().registeredGroups;
            LogicEntry le = advLogic as LogicEntry;
            DBClass t = DataBase.Get(le.scriptTypeParameter, false);
            List<object> l = new List<object>();

            if (t != null)
            {
                Enchantment ench = (Enchantment)t;
                foreach (var g in groups)
                {
                    var group = g;
                    if (!UTIL_IsOwnerValid(group.GetOwnerID(), le.playerOwner, advData)) continue;
                    if (g.GetLocationHost()?.otherPlaneLocation?.Get() != null && g.plane.arcanusType) continue;

                    foreach (var u in group.GetUnits())
                    {
                        foreach (var e in u.Get().GetEnchantments())
                        {
                            if (e.source == ench)
                            {
                                l.Add(u.Get());
                            }
                        }
                    }
                }
            }

            return AdvList.MakeList(l, advLogic, advData, localLists);
        }

        [ScriptType(ScriptType.Type.EditorUnitFilter)]
        [ScriptParameters(typeof(Spell))]
        [ScriptRetType(LogicEntry.LEntry.LEEntryUnit)]
        static public object FwT_UnitByLearnedSpell(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                    Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            var groups = GameManager.Get().registeredGroups;
            LogicEntry le = advLogic as LogicEntry;
            DBClass t = DataBase.Get(le.scriptTypeParameter, false);
            List<object> l = new List<object>();

            if (t != null)
            {
                Spell s = (Spell)t;
                foreach (var g in groups)
                {
                    var group = g;
                    if (!UTIL_IsOwnerValid(group.GetOwnerID(), le.playerOwner, advData)) continue;
                    if (g.GetLocationHost()?.otherPlaneLocation?.Get() != null && g.plane.arcanusType) continue;

                    foreach (var u in group.GetUnits())
                    {
                        foreach (var e in u.Get().GetSpells())
                        {
                            if (e.Get() == s)
                            {
                                l.Add(u.Get());
                            }
                        }
                    }
                }
            }

            return AdvList.MakeList(l, advLogic, advData, localLists);
        }

        [ScriptType(ScriptType.Type.EditorUnitFilter)]
        [ScriptRetType(LogicEntry.LEntry.LEEntryUnit)]
        static public object FwT_UnitByHealingSpell(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                    Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            var groups = GameManager.Get().registeredGroups;
            LogicEntry le = advLogic as LogicEntry;
            List<object> l = new List<object>();

            foreach (var g in groups)
            {
                var group = g;
                if (!UTIL_IsOwnerValid(group.GetOwnerID(), le.playerOwner, advData)) continue;
                if (g.GetLocationHost()?.otherPlaneLocation?.Get() != null && g.plane.arcanusType) continue;

                foreach (var u in group.GetUnits())
                {
                    var spell = u.Get().GetSpells().Find(o => o.Get().healingSpell);
                    if (spell != null)
                    {
                        l.Add(u.Get());
                    }
                }
            }

            return AdvList.MakeList(l, advLogic, advData, localLists);
        }

        [ScriptType(ScriptType.Type.EditorUnitFilter)]
        [ScriptRetType(LogicEntry.LEntry.LEEntryUnit)]
        static public object FwT_FantasticUnits(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            var groups = GameManager.Get().registeredGroups;
            LogicEntry le = advLogic as LogicEntry;
            List<object> l = new List<object>();

            foreach (var g in groups)
            {
                if (!UTIL_IsOwnerValid(g.GetOwnerID(), le.playerOwner, advData)) continue;
                if (g.GetLocationHost()?.otherPlaneLocation?.Get() != null && g.plane.arcanusType) continue;

                foreach (var u in g.GetUnits())
                {
                    if (u.Get().attributes.Contains(TAG.FANTASTIC_CLASS))
                    {
                        l.Add(u.Get());
                    }
                }
            }
            return AdvList.MakeList(l, advLogic, advData, localLists);
        }

        [ScriptType(ScriptType.Type.EditorUnitFilter)]
        [ScriptRetType(LogicEntry.LEntry.LEEntryUnit)]
        static public object FwT_NormalUnits(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                             Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            var groups = GameManager.Get().registeredGroups;
            LogicEntry le = advLogic as LogicEntry;
            List<object> l = new List<object>();

            foreach (var g in groups)
            {
                if (!UTIL_IsOwnerValid(g.GetOwnerID(), le.playerOwner, advData)) continue;
                if (g.GetLocationHost()?.otherPlaneLocation?.Get() != null && g.plane.arcanusType) continue;

                foreach (var u in g.GetUnits())
                {
                    if (u.Get().attributes.Contains(TAG.NORMAL_CLASS))
                    {
                        l.Add(u.Get());
                    }
                }
            }
            return AdvList.MakeList(l, advLogic, advData, localLists);
        }

        [ScriptType(ScriptType.Type.EditorUnitFilter)]
        [ScriptParameters(typeof(Skill))]
        [ScriptRetType(LogicEntry.LEntry.LEEntryUnit)]
        static public object FwT_UnitBySkill(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                             Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            var groups = GameManager.Get().registeredGroups;
            LogicEntry le = advLogic as LogicEntry;
            DBClass t = DataBase.Get(le.scriptTypeParameter, false);
            List<object> l = new List<object>();

            if (t != null)
            {
                foreach (var g in groups)
                {
                    if (!UTIL_IsOwnerValid(g.GetOwnerID(), le.playerOwner, advData)) continue;
                    if (g.GetLocationHost()?.otherPlaneLocation?.Get() != null && g.plane.arcanusType) continue;

                    foreach (var u in g.GetUnits())
                    {
                        if (u.Get().GetSkills().Contains(t as Skill))
                            l.Add(u.Get());
                    }
                }
            }
            return AdvList.MakeList(l, advLogic, advData, localLists);
        }

        //Enchantments
        [ScriptType(ScriptType.Type.EditorEnchantmentFilter)]
        [ScriptParameters(null, null)]
        [ScriptRetType(LogicEntry.LEntry.LEEntryEnchantment)]
        static public object FwT_AnyEnchantment(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            List<PlayerWizard> wizards = GameManager.GetWizards();
            LogicEntry le = advLogic as LogicEntry;
            List<object> l = new List<object>();
            List<Group> groups;
            List<MOM.Location> locations;

            foreach (var w in wizards)
            {
                if (!UTIL_IsOwnerValid(w.ID, le.playerOwner, advData)) continue;

                groups = GameManager.GetGroupsOfWizard(w.ID);
                locations = GameManager.GetLocationsOfWizard(w.ID);

                l.AddRange(w.GetEnchantments());
                foreach (var g in groups)
                {
                    foreach (var u in g.GetUnits())
                    {
                        l.AddRange(u.Get().GetEnchantments());
                    }
                }
                foreach (var loc in locations)
                {
                    l.AddRange(loc.GetEnchantments());
                }
            }

            return AdvList.MakeList(l, advLogic, advData, localLists);
        }

        [ScriptType(ScriptType.Type.EditorEnchantmentFilter)]
        [ScriptParameters(null, null)]
        [ScriptRetType(LogicEntry.LEntry.LEEntryEnchantment)]
        static public object FwT_AllEnchantments(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            List<PlayerWizard> wizards = GameManager.GetWizards();
            LogicEntry le = advLogic as LogicEntry;
            List<object> l = new List<object>();
            List<Group> groups;
            List<MOM.Location> locations;

            foreach (var w in wizards)
            {
                if (!UTIL_IsOwnerValid(w.ID, le.playerOwner, advData)) continue;

                groups = GameManager.GetGroupsOfWizard(w.ID);
                locations = GameManager.GetLocationsOfWizard(w.ID);

                l.AddRange(w.GetEnchantments());
                foreach (var g in groups)
                {
                    foreach (var u in g.GetUnits())
                    {
                        l.AddRange(u.Get().GetEnchantments());
                    }
                }
                foreach (var loc in locations)
                {
                    l.AddRange(loc.GetEnchantments());
                }
            }
            return AdvList.MakeList(l, advLogic, advData, localLists);
        }
        [ScriptType(ScriptType.Type.EditorEnchantmentFilter)]
        [ScriptParameters(typeof(Enchantment))]
        [ScriptRetType(LogicEntry.LEntry.LEEntryEnchantment)]
        static public object FwT_EnchantmentByName(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                   Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            List<PlayerWizard> wizards = GameManager.GetWizards();
            LogicEntry le = advLogic as LogicEntry;
            DBClass t = DataBase.Get(le.scriptTypeParameter, false);
            List<object> l = new List<object>();
            List<Group> groups;
            List<MOM.Location> locations;

            if (t != null)
            {
                foreach (var w in wizards)
                {
                    if (!UTIL_IsOwnerValid(w.ID, le.playerOwner, advData)) continue;

                    groups = GameManager.GetGroupsOfWizard(w.ID);
                    locations = GameManager.GetLocationsOfWizard(w.ID);

                    Enchantment ench = (Enchantment)t;

                    l.AddRange(w.GetEnchantments().FindAll(o => o.source == ench));
                    foreach (var g in groups)
                    {
                        foreach (var u in g.GetUnits())
                        {
                            l.AddRange(u.Get().GetEnchantments().FindAll(o => o.source == ench));
                        }
                    }
                    foreach (var loc in locations)
                    {
                        l.AddRange(loc.GetEnchantments().FindAll(o => o.source == ench));
                    }
                }
            }
            return AdvList.MakeList(l, advLogic, advData, localLists);
        }

        // Equipment
        [ScriptType(ScriptType.Type.EditorEquipmentFilter)]
        [ScriptRetType(LogicEntry.LEntry.LEEntryEnchantment)]
        static public object FwT_AnyEquipment(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                              Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            List<PlayerWizard> wizards = GameManager.GetWizards();
            var groups = GameManager.Get().registeredGroups;
            LogicEntry le = advLogic as LogicEntry;
            List<object> l = new List<object>();

            foreach (var w in wizards)
            {
                if (!UTIL_IsOwnerValid(w.ID, le.playerOwner, advData)) continue;


                if (w.artefacts != null && w.artefacts.Count > 0)
                {
                    l.AddRange(w.artefacts);
                }
            }

            foreach (var g in groups)
            {
                if (!UTIL_IsOwnerValid(g.GetOwnerID(), le.playerOwner, advData)) continue;
                if (g.GetLocationHost()?.otherPlaneLocation?.Get() != null && g.plane.arcanusType) continue;

                foreach (var u in g.GetUnits())
                {
                    if (u.Get().dbSource.Get() is Hero)
                    {
                        var hero = u.Get();
                        foreach (var v in hero.artefactManager.equipmentSlots)
                        {
                            if (v.item != null)
                            {
                                l.Add(v.item);
                            }
                        }
                    }
                }
            }
            return AdvList.MakeList(l, advLogic, advData, localLists);
        }

        [ScriptType(ScriptType.Type.EditorEquipmentFilter)]
        [ScriptRetType(LogicEntry.LEntry.LEEntryEnchantment)]
        static public object FwT_EquipmentByUnEquipped(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                              Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            List<PlayerWizard> wizards = GameManager.GetWizards();
            LogicEntry le = advLogic as LogicEntry;
            List<object> l = new List<object>();

            foreach (var w in wizards)
            {
                if (!UTIL_IsOwnerValid(w.ID, le.playerOwner, advData)) continue;


                if (w.artefacts != null && w.artefacts.Count > 0)
                {
                    l.AddRange(w.artefacts);
                }
            }
            return AdvList.MakeList(l, advLogic, advData, localLists);
        }

        [ScriptType(ScriptType.Type.EditorEquipmentFilter)]
        [ScriptRetType(LogicEntry.LEntry.LEEntryEnchantment)]
        static public object FwT_EquipmentByEquiped(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                    Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            var groups = GameManager.Get().registeredGroups;
            LogicEntry le = advLogic as LogicEntry;
            List<object> l = new List<object>();

            foreach (var g in groups)
            {
                if (!UTIL_IsOwnerValid(g.GetOwnerID(), le.playerOwner, advData)) continue;
                if (g.GetLocationHost()?.otherPlaneLocation?.Get() != null && g.plane.arcanusType) continue;

                foreach (var u in g.GetUnits())
                {
                    if (u.Get().dbSource.Get() is Hero)
                    {
                        var hero = u.Get();
                        foreach (var v in hero.artefactManager.equipmentSlots)
                        {
                            if (v.item != null)
                            {
                                l.Add(v.item);
                            }
                        }
                    }
                }
            }
            return AdvList.MakeList(l, advLogic, advData, localLists);
        }

        [ScriptType(ScriptType.Type.EditorEquipmentFilter)]
        [ScriptParameters(typeof(EEquipmentType))]
        [ScriptRetType(LogicEntry.LEntry.LEEntryEnchantment)]
        static public object FwT_EquipmentByType(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            List<PlayerWizard> wizards = GameManager.GetWizards();
            var groups = GameManager.Get().registeredGroups;
            LogicEntry le = advLogic as LogicEntry;
            EEquipmentType equipmentType = EEquipmentType.None;
            List<object> l = new List<object>();
            if (!string.IsNullOrEmpty(le.scriptTypeParameter))
            {
                equipmentType = (EEquipmentType)Enum.Parse(typeof(EEquipmentType), le.scriptTypeParameter);
            }

            foreach (var w in wizards)
            {
                if (!UTIL_IsOwnerValid(w.ID, le.playerOwner, advData)) continue;


                if (w.artefacts != null && w.artefacts.Count > 0)
                {
                    foreach (var a in w.artefacts)
                    {
                        if (a.equipmentType == equipmentType)
                        {
                            l.Add(a);
                        }
                    }
                }
            }

            foreach (var g in groups)
            {
                if (!UTIL_IsOwnerValid(g.GetOwnerID(), le.playerOwner, advData)) continue;
                if (g.GetLocationHost()?.otherPlaneLocation?.Get() != null && g.plane.arcanusType) continue;

                foreach (var u in g.GetUnits())
                {
                    if (u.Get().dbSource.Get() is Hero)
                    {
                        var hero = u.Get();
                        foreach (var v in hero.artefactManager.equipmentSlots)
                        {
                            if (v.item != null && v.item.equipmentType == equipmentType)
                            {
                                l.Add(v.item);
                            }
                        }
                    }
                }
            }
            return AdvList.MakeList(l, advLogic, advData, localLists);
        }

        //Group
        [ScriptType(ScriptType.Type.EditorGroupFilter)]
        [ScriptRetType(LogicEntry.LEntry.LEEntryGroup)]
        static public object FwT_AllGroups(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                           Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            var groups = GameManager.Get().registeredGroups;
            LogicEntry le = advLogic as LogicEntry;
            List<object> l = new List<object>();

            foreach (var g in groups)
            {
                if (!UTIL_IsOwnerValid(g.GetOwnerID(), le.playerOwner, advData)) continue;
                if (g.GetLocationHost()?.otherPlaneLocation?.Get() != null && g.plane.arcanusType) continue;
                l.Add(g);
            }

            return AdvList.MakeList(l, advLogic, advData, localLists);
        }

        [ScriptType(ScriptType.Type.EditorGroupFilter)]
        [ScriptParameters(typeof(Hero))]
        [ScriptRetType(LogicEntry.LEntry.LEEntryGroup)]
        static public object FwT_GroupByHero(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                             Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            var groups = GameManager.Get().registeredGroups;
            LogicEntry le = advLogic as LogicEntry;
            DBClass t = DataBase.Get(le.scriptTypeParameter, false);
            List<object> l = new List<object>();

            if (t != null)
            {
                foreach (var g in groups)
                {
                    if (!UTIL_IsOwnerValid(g.GetOwnerID(), le.playerOwner, advData)) continue;
                    if (g.GetLocationHost()?.otherPlaneLocation?.Get() != null && g.plane.arcanusType) continue;

                    var h = g.GetUnits().FindAll(o => o.Get().dbSource.Get().Equals(t));
                    if (h.Count > 0)
                    {
                        l.Add(g);
                    }
                }
            }
            return AdvList.MakeList(l, advLogic, advData, localLists);
        }
        [ScriptType(ScriptType.Type.EditorGroupFilter)]
        [ScriptRetType(LogicEntry.LEntry.LEEntryGroup)]
        static public object FwT_GroupStrongest(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                             Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            var groups = GameManager.Get().registeredGroups;
            LogicEntry le = advLogic as LogicEntry;
            List<object> l = new List<object>();
            Group strongest = null;
            int power = 0;

            foreach (var g in groups)
            {
                if (!UTIL_IsOwnerValid(g.GetOwnerID(), le.playerOwner, advData)) continue;
                if (g.GetLocationHost()?.otherPlaneLocation?.Get() != null && g.plane.arcanusType) continue;

                int localPower = 0;
                foreach (var u in g.GetUnits())
                {
                    localPower += u.Get().GetSimpleUnitStrength();
                }

                if (localPower > power)
                {
                    power = localPower;
                    strongest = g;
                }
            }
            if (strongest != null)
            {
                l.Add(strongest);
            }

            return AdvList.MakeList(l, advLogic, advData, localLists);
        }
        [ScriptType(ScriptType.Type.EditorGroupFilter)]
        [ScriptParameters(typeof(LogicUtils.Comparison), typeof(int))]
        [ScriptRetType(LogicEntry.LEntry.LEEntryGroup)]
        static public object FwT_GroupBySize(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                             Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            var groups = GameManager.Get().registeredGroups;
            LogicEntry le = advLogic as LogicEntry;
            float size = UTIL_StringParameterProcessor(le.scriptStringParameter).t1;
            int iSize = Mathf.RoundToInt(size);
            LogicUtils.Comparison lComp = LogicUtils.Comparison.MoreOrEqualThan;
            if (!string.IsNullOrEmpty(le.scriptTypeParameter))
            {
                lComp = (LogicUtils.Comparison)Enum.Parse(typeof(LogicUtils.Comparison), le.scriptTypeParameter);
            }
            List<object> l = new List<object>();

            foreach (var g in groups)
            {
                if (!UTIL_IsOwnerValid(g.GetOwnerID(), le.playerOwner, advData)) continue;
                if (g.GetLocationHost()?.otherPlaneLocation?.Get() != null && g.plane.arcanusType) continue;

                if (UTIL_IsValueValid(g.GetUnits().Count, iSize, lComp))
                {
                    l.Add(g);
                }
            }

            return AdvList.MakeList(l, advLogic, advData, localLists);
        }

        #endregion

        #region FilterListProcessing
        static public object FLP_AandB(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                    Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicProcessing lp = advLogic as LogicProcessing;
            AdvList listA = advData.GetListByName(lp.listA, localLists);
            if (listA == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List A :" + lp.listA + ", doesn't exist");
                return null;
            }

            AdvList listB = advData.GetListByName(lp.listB, localLists);
            if (listB == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List B :" + lp.listB + ", doesn't exist");
                return null;
            }

            List<object> list = new List<object>();
            list.AddRange(listA.list.Intersect(listB.list));

            return AdvList.MakeList(list, advLogic, advData, localLists);
        }

        static public object FLP_AorB(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                    Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicProcessing lp = advLogic as LogicProcessing;
            AdvList listA = advData.GetListByName(lp.listA, localLists);
            if (listA == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List A :" + lp.listA + ", doesn't exist");
                return null;
            }

            AdvList listB = advData.GetListByName(lp.listB, localLists);
            if (listB == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List B :" + lp.listB + ", doesn't exist");
                return null;
            }

            List<object> list = new List<object>();
            list.AddRange(listA.list.Union(listB.list));

            return AdvList.MakeList(list, advLogic, advData, localLists);
        }

        static public object FLP_AminusB(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                    Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {

            LogicProcessing lp = advLogic as LogicProcessing;
            AdvList listA = advData.GetListByName(lp.listA, localLists);
            if (listA == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List A :" + lp.listA + ", doesn't exist");
                return null;
            }

            AdvList listB = advData.GetListByName(lp.listB, localLists);
            if (listB == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List B :" + lp.listB + ", doesn't exist");
                return null;
            }

            List<object> list = new List<object>();
            list.AddRange(listA.list.Except(listB.list));

            return AdvList.MakeList(list, advLogic, advData, localLists);
        }

        [ScriptType(ScriptType.Type.EditorFilterListProcessing)]
        [ScriptParameters(null, typeof(int))]
        static public object FLP_GetRandomXelements(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                    Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicProcessing lp = advLogic as LogicProcessing;
            AdvList al = advData.GetListByName(lp.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lp.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lp.listA + ", doesn't exist");
                return null;
            }

            int parameterValue = 1;
            List<object> l = new List<object>(al.list);

            if (!String.IsNullOrEmpty(lp.scriptStringParameter))
            {
                float value = UTIL_StringParameterProcessor(lp.scriptStringParameter).t1;
                parameterValue = Mathf.RoundToInt(value);
            }

            l.RandomSortThreadSafe();
            l = l.GetRange(0, Mathf.Min(parameterValue, l.Count));

            return AdvList.MakeList(l, advLogic, advData, localLists);
        }

        [ScriptType(ScriptType.Type.EditorFilterListProcessing)]
        [ScriptParameters(null, typeof(int))]
        static public object FLP_GetRandomXPercentElements(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                           Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicProcessing lp = advLogic as LogicProcessing;
            AdvList al = advData.GetListByName(lp.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lp.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lp.listA + ", doesn't exist");
                return null;
            }

            float parameterValuePercentage;
            int xElements = 1;
            List<object> l = new List<object>(al.list);

            if (!String.IsNullOrEmpty(lp.scriptStringParameter))
            {
                float value = UTIL_StringParameterProcessor(lp.scriptStringParameter).t1;
                parameterValuePercentage = value * 0.01f;
                xElements = Mathf.RoundToInt(l.Count() * parameterValuePercentage);
            }

            l.RandomSortThreadSafe();
            l = l.GetRange(0, Mathf.Min(xElements, l.Count));

            return AdvList.MakeList(l, advLogic, advData, localLists);
        }

        [ScriptType(ScriptType.Type.EditorFilterListProcessing)]
        [ScriptParameters(typeof(Tag), typeof(FInt))]
        [ScriptParameters(typeof(Tag), null)]
        static public object FLP_FilterByTag(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                             Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicProcessing lp = advLogic as LogicProcessing;
            AdvList al = advData.GetListByName(lp.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lp.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lp.listA + ", doesn't exist");
                return null;
            }

            FInt parameterValue = FInt.ONE;
            List<object> l = new List<object>(al.list);
            DBClass t = DataBase.Get(lp.scriptTypeParameter, false);

            if (!String.IsNullOrEmpty(lp.scriptStringParameter))
            {
                float value = UTIL_StringParameterProcessor(lp.scriptStringParameter).t1;
                parameterValue = new FInt(value);
            }

            List<object> l2 = new List<object>();

            if (t != null && l != null)
            {
                Tag tag = (Tag)t;

                if (al.listType == LogicEntry.LEntry.LEEntryUnit || al.listType == LogicEntry.LEntry.LEEntryHero)
                {
                    foreach (var v in l)
                    {
                        if (v is IAttributable)
                        {
                            if ((v as IAttributable).GetAttributes().Contains(tag, parameterValue))
                            {
                                l2.Add(v);
                            }
                        }
                    }
                }
                else if (al.listType == LogicEntry.LEntry.LEEntryGroup)
                {
                    foreach (var g in l)
                    {
                        Group gr = g as Group;
                        if (gr == null) continue;

                        foreach (var v in gr.GetUnits())
                        {
                            if (v.Get().GetAttributes().Contains(tag, parameterValue))
                            {
                                l2.Add(v);
                            }
                        }
                    }
                }
                else if (al.listType == LogicEntry.LEntry.LEEntryCity)
                {
                    foreach (var c in l)
                    {
                        TownLocation tl = c as TownLocation;
                        if (tl == null || tl.localGroup == null) continue;

                        foreach (var v in tl.localGroup.Get().GetUnits())
                        {
                            if (v.Get().GetAttributes().Contains(tag, parameterValue))
                            {
                                l2.Add(v);
                            }
                        }
                    }
                }
                else if (al.listType == LogicEntry.LEntry.LEEntryWizard)
                {
                    foreach (var w in l)
                    {
                        PlayerWizard wiz = w as PlayerWizard;
                        if (wiz == null) continue;

                        if (wiz.GetAttributes().Contains(tag, parameterValue))
                        {
                            l2.Add(wiz);
                        }
                    }
                }
            }
            return AdvList.MakeList(l2, advLogic, advData, localLists);
        }

        [ScriptType(ScriptType.Type.EditorFilterListProcessing)]
        [ScriptParameters(typeof(Race))]
        static public object FLP_FilterByRace(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                              Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicProcessing lp = advLogic as LogicProcessing;
            AdvList al = advData.GetListByName(lp.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lp.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lp.listA + ", doesn't exist");
                return null;
            }

            List<object> l = new List<object>(al.list);
            DBClass t = DataBase.Get(lp.scriptTypeParameter, false);

            List<object> l2 = new List<object>();
            if (t != null)
            {
                foreach (var v in l)
                {
                    if (v is MOM.Unit)
                    {
                        var u = v as MOM.Unit;
                        if (u.race == t)
                            l2.Add(v);
                    }
                    else if (v is PlayerWizard)
                    {
                        var w = v as PlayerWizard;
                        if (w.mainRace.Get() == t)
                            l2.Add(v);
                    }
                    else if (v is TownLocation)
                    {
                        var c = v as TownLocation;
                        if (c.race.Get() == t)
                            l2.Add(v);
                    }
                }
            }
            return AdvList.MakeList(l2, advLogic, advData, localLists);
        }

        [ScriptType(ScriptType.Type.EditorFilterListProcessing)]
        [ScriptParameters(typeof(Unit))]
        [ScriptParameters(typeof(Hero))]
        static public object FLP_FilterBySubrace(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicProcessing lp = advLogic as LogicProcessing;
            AdvList al = advData.GetListByName(lp.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lp.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lp.listA + ", doesn't exist");
                return null;
            }

            List<object> l = new List<object>(al.list);
            DBClass t = DataBase.Get(lp.scriptTypeParameter, false);

            List<object> l2 = new List<object>();
            if (t != null)
            {
                foreach (var v in l)
                {
                    if (v is MOM.Unit)
                    {
                        MOM.Unit u = v as MOM.Unit;
                        if (u.dbSource.Get() == t)
                            l2.Add(v);
                    }
                }
            }
            return AdvList.MakeList(l2, advLogic, advData, localLists);
        }
        [ScriptType(ScriptType.Type.EditorFilterListProcessing)]
        [ScriptParameters(typeof(LogicUtils.Comparison), typeof(int))]
        static public object FLP_FilterByLevel(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicProcessing lp = advLogic as LogicProcessing;
            LogicUtils.Comparison lComp = (LogicUtils.Comparison)Enum.Parse(typeof(LogicUtils.Comparison), lp.scriptTypeParameter);
            AdvList al = advData.GetListByName(lp.listA, localLists);

            if (al == null)
            {
                al = advData.GetListByName(lp.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lp.listA + ", doesn't exist");
                return null;
            }

            int parameterValue = 1;
            if (!String.IsNullOrEmpty(lp.scriptStringParameter))
            {
                float value = UTIL_StringParameterProcessor(lp.scriptStringParameter).t1;
                parameterValue = Mathf.RoundToInt(value);
            }

            List<object> l = new List<object>(al.list);

            List<object> l2 = new List<object>();
            foreach (var v in l)
            {
                if (v is MOM.Unit)
                {
                    MOM.Unit u = v as MOM.Unit;
                    if (UTIL_IsValueValid(u.GetLevel(), parameterValue, lComp))
                    {
                        l2.Add(v);
                    }
                }
            }
            return AdvList.MakeList(l2, advLogic, advData, localLists);
        }


        [ScriptType(ScriptType.Type.EditorFilterListProcessing)]
        [ScriptParameters(typeof(ETerrainCategory))]
        static public object FLP_FilterByTerrain(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicProcessing lp = advLogic as LogicProcessing;
            AdvList al = advData.GetListByName(lp.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lp.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lp.listA + ", doesn't exist");
                return null;
            }

            List<object> l = new List<object>(al.list);
            ETerrainCategory terrainCat = (ETerrainCategory)Enum.Parse(typeof(ETerrainCategory), lp.scriptTypeParameter);

            List<object> l2 = new List<object>();
            foreach (var v in l)
            {
                if (v is IPlanePosition)
                {
                    IPlanePosition u = v as IPlanePosition;
                    var hex = u.GetPlane().GetHexAt(u.GetPosition());

                    if (UTIL_IsTerrainTypeValid(terrainCat, hex, u))
                    {
                        l2.Add(v);
                    }
                }
            }
            return AdvList.MakeList(l2, advLogic, advData, localLists);
        }

        [ScriptType(ScriptType.Type.EditorFilterListProcessing)]
        [ScriptParameters(typeof(Plane))]
        static public object FLP_FilterByPlane(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicProcessing lp = advLogic as LogicProcessing;
            AdvList al = advData.GetListByName(lp.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lp.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lp.listA + ", doesn't exist");
                return null;
            }

            List<object> l = new List<object>(al.list);
            DBClass t = DataBase.Get(lp.scriptTypeParameter, false);

            List<object> l2 = new List<object>();
            if (t != null)
            {
                foreach (var v in l)
                {
                    if (v is IPlanePosition)
                    {
                        IPlanePosition u = v as IPlanePosition;
                        if (u.GetPlane().planeSource.Get() == t)
                            l2.Add(v);
                    }
                }
            }
            return AdvList.MakeList(l2, advLogic, advData, localLists);
        }

        [ScriptType(ScriptType.Type.EditorFilterListProcessing)]
        [ScriptParameters(typeof(Skill))]
        static public object FLP_FilterBySkill(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicProcessing lp = advLogic as LogicProcessing;
            AdvList al = advData.GetListByName(lp.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lp.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lp.listA + ", doesn't exist");
                return null;
            }

            List<object> l = new List<object>(al.list);
            DBClass t = DataBase.Get(lp.scriptTypeParameter, false);

            List<object> l2 = new List<object>();
            if (t != null)
            {
                foreach (var v in l)
                {
                    if (v is ISkillable)
                    {
                        ISkillable u = v as ISkillable;
                        if (u.GetSkills().Contains(t as Skill))
                            l2.Add(v);
                    }
                }
            }
            return AdvList.MakeList(l2, advLogic, advData, localLists);
        }

        [ScriptType(ScriptType.Type.EditorFilterListProcessing)]
        [ScriptParameters(typeof(Spell))]
        static public object FLP_FilterByLearnedSpell(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicProcessing lp = advLogic as LogicProcessing;
            AdvList al = advData.GetListByName(lp.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lp.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lp.listA + ", doesn't exist");
                return null;
            }

            List<object> l = new List<object>(al.list);
            DBClass t = DataBase.Get(lp.scriptTypeParameter, false);

            List<object> l2 = new List<object>();
            if (t != null)
            {
                foreach (var v in l)
                {
                    if (v is ISpellCaster)
                    {
                        ISpellCaster u = v as ISpellCaster;
                        if (u.GetSpells().Contains(t as Spell))
                            l2.Add(v);
                    }
                }
            }
            return AdvList.MakeList(l2, advLogic, advData, localLists);
        }
        [ScriptType(ScriptType.Type.EditorFilterListProcessing)]
        static public object FLP_FilterByHealingSpell(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicProcessing lp = advLogic as LogicProcessing;
            AdvList al = advData.GetListByName(lp.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lp.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lp.listA + ", doesn't exist");
                return null;
            }

            List<object> l = new List<object>(al.list);

            List<object> l2 = new List<object>();
            foreach (var v in l)
            {
                if (v is ISpellCaster)
                {
                    ISpellCaster u = v as ISpellCaster;
                    var spell = u.GetSpells().Find(o => o.Get().healingSpell);
                    if (spell != null)
                        l2.Add(v);
                }
            }
            return AdvList.MakeList(l2, advLogic, advData, localLists);
        }

        [ScriptType(ScriptType.Type.EditorFilterListProcessing)]
        [ScriptParameters(typeof(Enchantment))]
        static public object FLP_FilterByEnchantment(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicProcessing lp = advLogic as LogicProcessing;
            AdvList al = advData.GetListByName(lp.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lp.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lp.listA + ", doesn't exist");
                return null;
            }

            List<object> l = new List<object>(al.list);
            DBClass t = DataBase.Get(lp.scriptTypeParameter, false);

            List<object> l2 = new List<object>();
            if (t != null)
            {
                Enchantment ench = (Enchantment)t;
                foreach (var v in l)
                {
                    if (v is IEnchantable)
                    {
                        IEnchantable ie = v as IEnchantable;
                        foreach (var e in ie.GetEnchantments())
                        {
                            if (e.source == ench)
                            {
                                l2.Add(v);
                                break;
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element is not  IEnchantable");
                    }
                }
            }
            return AdvList.MakeList(l2, advLogic, advData, localLists);
        }

        [ScriptType(ScriptType.Type.EditorFilterListProcessing)]
        [ScriptParameters(typeof(Resource))]
        static public object FLP_FilterByResource(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicProcessing lp = advLogic as LogicProcessing;
            AdvList al = advData.GetListByName(lp.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lp.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lp.listA + ", doesn't exist");
                return null;
            }

            List<object> l = new List<object>(al.list);
            DBClass t = DataBase.Get(lp.scriptTypeParameter, false);

            List<object> l2 = new List<object>();
            if (t != null)
            {
                foreach (var v in l)
                {
                    if (v is TownLocation)
                    {
                        TownLocation tl = v as TownLocation;
                        if (tl.GetPotentialResources().Contains(t))
                        {
                            l2.Add(v);
                        }
                    }
                    else
                    {
                        Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element is not  IEnchantable");
                    }
                }
            }
            return AdvList.MakeList(l2, advLogic, advData, localLists);
        }

        [ScriptType(ScriptType.Type.EditorFilterListProcessing)]
        static public object FLP_FilterByAnyResource(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicProcessing lp = advLogic as LogicProcessing;
            AdvList al = advData.GetListByName(lp.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lp.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lp.listA + ", doesn't exist");
                return null;
            }

            List<object> l = new List<object>(al.list);

            List<object> l2 = new List<object>();
            foreach (var v in l)
            {
                if (v is TownLocation)
                {
                    TownLocation tl = v as TownLocation;
                    var pr = tl.GetPotentialResources();
                    if (pr != null && pr.Count > 0)
                    {
                        l2.Add(v);
                    }
                }
                else
                {
                    Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element is not TownLocation");
                }
            }
            return AdvList.MakeList(l2, advLogic, advData, localLists);
        }

        [ScriptType(ScriptType.Type.EditorFilterListProcessing)]
        [ScriptParameters(typeof(EMinedResources))]
        static public object FLP_FilterByMinedResource(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicProcessing lp = advLogic as LogicProcessing;
            EMinedResources minedRes = (EMinedResources)Enum.Parse(typeof(EMinedResources), lp.scriptTypeParameter.ToString());
            AdvList al = advData.GetListByName(lp.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lp.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lp.listA + ", doesn't exist");
                return null;
            }

            List<object> l = new List<object>(al.list);

            List<object> l2 = new List<object>();
            foreach (var v in l)
            {
                if (v is TownLocation)
                {
                    TownLocation tl = v as TownLocation;
                    foreach (var res in tl.GetPotentialResources())
                    {
                        if (UTIL_IsResourceValid(minedRes, res))
                        {
                            l2.Add(v);
                            break;
                        }
                    }
                }
                else
                {
                    Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element is not TownLocation");
                }
            }
            return AdvList.MakeList(l2, advLogic, advData, localLists);
        }
        [ScriptType(ScriptType.Type.EditorFilterListProcessing)]
        static public object FLP_FilterByAnyMinedResource(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicProcessing lp = advLogic as LogicProcessing;
            AdvList al = advData.GetListByName(lp.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lp.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lp.listA + ", doesn't exist");
                return null;
            }

            List<object> l = new List<object>(al.list);

            List<object> l2 = new List<object>();
            foreach (var v in l)
            {
                if (v is TownLocation)
                {
                    TownLocation tl = v as TownLocation;
                    foreach (var res in tl.GetPotentialResources())
                    {
                        if (UTIL_IsMinedResource(res))
                        {
                            l2.Add(v);
                        }
                    }
                }
                else
                {
                    Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element is not TownLocation");
                }
            }
            return AdvList.MakeList(l2, advLogic, advData, localLists);
        }

        [ScriptType(ScriptType.Type.EditorFilterListProcessing)]
        [ScriptParameters(typeof(Building))]
        [ScriptRetType(LogicEntry.LEntry.LEEntryCity)]
        static public object FLP_FilterByBuilding(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicProcessing lp = advLogic as LogicProcessing;
            AdvList al = advData.GetListByName(lp.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lp.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lp.listA + ", doesn't exist");
                return null;
            }

            List<object> l = new List<object>(al.list);
            DBClass t = DataBase.Get(lp.scriptTypeParameter, false);

            List<object> l2 = new List<object>();
            if (t != null)
            {
                foreach (var v in l)
                {
                    if (v is TownLocation)
                    {
                        TownLocation town = v as TownLocation;
                        if (town.HaveBuilding(t as DBDef.Building))
                            l2.Add(v);
                    }
                }
            }
            return AdvList.MakeList(l2, advLogic, advData, localLists);
        }

        [ScriptType(ScriptType.Type.EditorFilterListProcessing)]
        [ScriptParameters(null, typeof(int))]
        [ScriptParameters(null, null)]
        [ScriptRetType(LogicEntry.LEntry.LEEntryCity)]
        static public object FLP_OpponentCityInRangeOf(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicProcessing lp = advLogic as LogicProcessing;
            AdvList al = advData.GetListByName(lp.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lp.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lp.listA + ", doesn't exist");
                return null;
            }

            int parameterValue = 1;
            if (!String.IsNullOrEmpty(lp.scriptStringParameter))
            {
                float value = UTIL_StringParameterProcessor(lp.scriptStringParameter).t1;
                parameterValue = Mathf.RoundToInt(value);
            }

            List<object> l2 = new List<object>();
            foreach (var v in al.list)
            {
                IPlanePosition ipp = v as IPlanePosition;
                if (ipp == null)
                {
                    Debug.Log("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element:" + v.ToString() + ", doesn't implement IPlanePosition");
                    continue;
                }

                var locationsOfPlane = GameManager.GetLocationsOfThePlane(ipp.GetPlane());
                List<Vector3i> positions = ipp.GetSurroundingArea(parameterValue);

                foreach (var loc in locationsOfPlane)
                {
                    if (positions.Contains(loc.GetPosition()))
                    {
                        if (loc is TownLocation)
                        {
                            var ownerID = (loc as TownLocation).GetOwnerID();
                            if (ownerID != 0 && ownerID != advData.mainPlayerWizard)
                            {
                                l2.Add(loc);
                            }
                        }
                    }
                }
            }
            return AdvList.MakeList(l2, advLogic, advData, localLists);
        }

        [ScriptType(ScriptType.Type.EditorFilterListProcessing)]
        [ScriptParameters(null, typeof(int))]
        [ScriptParameters(null, null)]
        [ScriptRetType(LogicEntry.LEntry.LEEntryCity)]
        static public object FLP_NeutralCityInRangeOf(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicProcessing lp = advLogic as LogicProcessing;
            AdvList al = advData.GetListByName(lp.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lp.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lp.listA + ", doesn't exist");
                return null;
            }

            int parameterValue = 1;

            if (!String.IsNullOrEmpty(lp.scriptStringParameter))
            {
                float value = UTIL_StringParameterProcessor(lp.scriptStringParameter).t1;
                parameterValue = Mathf.RoundToInt(value);
            }

            List<object> l2 = new List<object>();
            foreach (var v in al.list)
            {
                IPlanePosition ipp = v as IPlanePosition;
                if (ipp == null)
                {
                    Debug.Log("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element:" + v.ToString() + ", doesn't implement IPlanePosition");
                    continue;
                }

                var locationsOfPlane = GameManager.GetLocationsOfThePlane(ipp.GetPlane());
                List<Vector3i> positions = ipp.GetSurroundingArea(parameterValue);

                foreach (var loc in locationsOfPlane)
                {
                    if (positions.Contains(loc.GetPosition()))
                    {
                        if (loc is TownLocation)
                        {
                            var ownerID = (loc as TownLocation).GetOwnerID();
                            if (ownerID == 0)
                            {
                                l2.Add(loc);
                            }
                        }
                    }
                }
            }
            return AdvList.MakeList(l2, advLogic, advData, localLists);
        }

        [ScriptType(ScriptType.Type.EditorFilterListProcessing)]
        [ScriptParameters(null, typeof(int))]
        [ScriptParameters(null, null)]
        [ScriptRetType(LogicEntry.LEntry.LEEntryCity)]
        static public object FLP_ActivePlayerCityInRangeOf(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicProcessing lp = advLogic as LogicProcessing;
            AdvList al = advData.GetListByName(lp.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lp.listA, publicLists);
            }
            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lp.listA + ", doesn't exist");
                return null;
            }

            int parameterValue = 1;

            if (!String.IsNullOrEmpty(lp.scriptStringParameter))
            {
                float value = UTIL_StringParameterProcessor(lp.scriptStringParameter).t1;
                parameterValue = Mathf.RoundToInt(value);
            }

            List<object> l2 = new List<object>();
            foreach (var v in al.list)
            {
                IPlanePosition ipp = v as IPlanePosition;
                if (ipp == null)
                {
                    Debug.Log("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element:" + v.ToString() + ", doesn't implement IPlanePosition");
                    continue;
                }

                var locationsOfPlane = GameManager.GetLocationsOfThePlane(ipp.GetPlane());
                List<Vector3i> positions = ipp.GetSurroundingArea(parameterValue);

                foreach (var loc in locationsOfPlane)
                {
                    if (positions.Contains(loc.GetPosition()))
                    {
                        if (loc is TownLocation)
                        {
                            var ownerID = (loc as TownLocation).GetOwnerID();
                            if (advData.mainPlayerWizard > 0 && ownerID == advData.mainPlayerWizard)
                            {
                                l2.Add(loc);
                            }
                        }
                    }
                }
            }
            return AdvList.MakeList(l2, advLogic, advData, localLists);
        }

        [ScriptType(ScriptType.Type.EditorFilterListProcessing)]
        [ScriptParameters(typeof(Resource), typeof(int))]
        [ScriptParameters(typeof(Resource), null)]
        static public object FLP_FilterByResourceInRangeOf(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicProcessing lp = advLogic as LogicProcessing;
            AdvList al = advData.GetListByName(lp.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lp.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lp.listA + ", doesn't exist");
                return false;
            }

            DBClass t = DataBase.Get(lp.scriptTypeParameter, false);
            int parameterValue = 1;

            if (!String.IsNullOrEmpty(lp.scriptStringParameter))
            {
                float value = UTIL_StringParameterProcessor(lp.scriptStringParameter).t1;
                parameterValue = Mathf.RoundToInt(value);
            }

            List<object> l2 = new List<object>();
            List<Vector3i> positions;
            Hex hex;

            foreach (var v in al.list)
            {
                IPlanePosition ipp = v as IPlanePosition;
                if (ipp == null)
                {
                    Debug.Log("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element:" + v.ToString() + ", doesn't implement IPlanePosition");
                    continue;
                }
                WorldCode.Plane plane = ipp.GetPlane();
                positions = ipp.GetSurroundingArea(parameterValue);

                foreach (var pos in positions)
                {
                    hex = plane.GetHexAt(pos);
                    if (hex == null || hex.Resource == null) continue;

                    if (hex.Resource.Get() == t as Resource)
                        l2.Add(pos);
                }
            }
            return AdvList.MakeList(l2, advLogic, advData, localLists);
        }

        [ScriptType(ScriptType.Type.EditorFilterListProcessing)]
        [ScriptRetType(LogicEntry.LEntry.LEEntryUnit)]
        static public object FLP_UnitsFromLocation(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicProcessing lp = advLogic as LogicProcessing;
            AdvList al = advData.GetListByName(lp.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lp.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogWarning("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lp.listA + ", doesn't exist");
                return null;
            }

            List<object> l = new List<object>(al.list);

            List<object> l2 = new List<object>();
            foreach (var v in l)
            {
                if (v is MOM.Location)
                {
                    MOM.Location loc = v as MOM.Location;
                    var g = loc.GetLocalGroup();
                    if (g != null && g.GetUnits().Count > 0)
                    {
                        g.GetUnits().ForEach(o => l2.Add(o.Get()));
                    }
                }
                else
                {
                    Debug.LogWarning("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lp.listA + ", element is not Location");
                }
            }
            return AdvList.MakeList(l2, advLogic, advData, localLists);
        }
        [ScriptType(ScriptType.Type.EditorFilterListProcessing)]
        [ScriptRetType(LogicEntry.LEntry.LEEntryWizard)]
        static public object FLP_WizardDefeated(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicProcessing lp = advLogic as LogicProcessing;
            AdvList al = advData.GetListByName(lp.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lp.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogWarning("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lp.listA + ", doesn't exist");
                return null;
            }

            int parameterValue = 1;

            if (!String.IsNullOrEmpty(lp.scriptStringParameter))
            {
                float value = UTIL_StringParameterProcessor(lp.scriptStringParameter).t1;
                parameterValue = Mathf.RoundToInt(value);
            }

            List<object> l2 = new List<object>();
            foreach (var v in al.list)
            {
                if (v is PlayerWizard)
                {
                    PlayerWizard w = v as PlayerWizard;
                    if (w.GetID() == parameterValue && !w.isAlive)
                    {
                        if (w.banishedBy == advData.mainPlayerWizard)
                        {
                            l2.Add(w);
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lp.listA + ", element is not PlayerWizard");
                }
            }
            return AdvList.MakeList(l2, advLogic, advData, localLists);
        }
        [ScriptType(ScriptType.Type.EditorFilterListProcessing)]
        [ScriptRetType(LogicEntry.LEntry.LEEntryWizard)]
        [ScriptParameters(typeof(LogicEntry.PlayerOwner), null)]
        static public object FLP_WizardBelongsTo(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicProcessing lp = advLogic as LogicProcessing;
            AdvList al = advData.GetListByName(lp.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lp.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogWarning("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lp.listA + ", doesn't exist");
                return null;
            }
            LogicEntry.PlayerOwner playerOwner = (LogicEntry.PlayerOwner)Enum.Parse(typeof(LogicEntry.PlayerOwner), lp.scriptTypeParameter);

            List<object> l2 = new List<object>();
            foreach (var v in al.list)
            {
                if (v is PlayerWizard)
                {
                    PlayerWizard w = v as PlayerWizard;
                    if (UTIL_IsOwnerValid(w.GetID(), playerOwner, advData))
                    {
                        l2.Add(w);
                    }
                }
                else
                {
                    Debug.LogWarning("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lp.listA + ", element is not PlayerWizard");
                }
            }
            return AdvList.MakeList(l2, advLogic, advData, localLists);
        }

        #endregion


        #region Result Action

        [ScriptType(ScriptType.Type.EditorListResult)]
        static public bool FRA_ListSizeEqual(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                             Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicRequirement lr = advLogic as LogicRequirement;
            int iRequiredValue = 1;
            string listName = lr.listA;
            AdvList al = advData.GetListByName(listName, localLists);
            if (al == null)
            {
                al = advData.GetListByName(listName, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lr.listA + ", doesn't exist");
                return false;
            }

            return UTIL_IsValueValid(al.Count(), iRequiredValue, LogicUtils.Comparison.MoreOrEqualThan);
        }

        [ScriptType(ScriptType.Type.EditorListResult)]
        [ScriptParameters(typeof(LogicUtils.Comparison), typeof(FInt))]
        static public bool FRA_ListSizeComparison(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                  Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicRequirement lr = advLogic as LogicRequirement;
            float requiredValue = UTIL_StringParameterProcessor(lr.scriptStringParameter).t1;
            int iRequiredValue = Mathf.RoundToInt(requiredValue);
            LogicUtils.Comparison lComp = (LogicUtils.Comparison)Enum.Parse(typeof(LogicUtils.Comparison), lr.logicComparison.ToString());
            if (lr.listA == null)
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; ListA doesn't exist");
            string listName = lr.listA;
            AdvList al = advData.GetListByName(listName, localLists);
            if (al == null)
            {
                al = advData.GetListByName(listName, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lr.listA + ", doesn't exist");
                return false;
            }

            return UTIL_IsValueValid(al.Count(), iRequiredValue, lComp);
        }

        [ScriptParameters(null, typeof(int))]
        static public bool FRA_Chance(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicRequirement lr = advLogic as LogicRequirement;
            var parameterValue = 1f;

            if (!String.IsNullOrEmpty(lr.scriptStringValue))
            {
                parameterValue = UTIL_StringParameterProcessor(lr.scriptStringValue).t1;
            }
            else
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; string parameter is null or empty");
            }

            float value = MHRandom.Get().GetFloat(0f, 1f);
            return value <= parameterValue * 0.01f;
        }

        [ScriptParameters(null, typeof(int))]
        [ScriptParameters(null, null)]
        static public bool FRA_Fame(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            List<PlayerWizard> wiz = GameManager.GetWizards();
            LogicRequirement lr = advLogic as LogicRequirement;
            int parameterValue = 1;

            if (!String.IsNullOrEmpty(lr.scriptStringParameter))
            {
                float value = UTIL_StringParameterProcessor(lr.scriptStringParameter).t1;
                parameterValue = Mathf.RoundToInt(value);
            }

            foreach (var w in wiz)
            {
                if (!UTIL_IsOwnerValid(w.ID, lr.playerOwner, advData)) continue;
                if (lr.equal)
                {
                    return w.GetFame() == parameterValue;
                }
                else if (lr.lessThan)
                {
                    return w.GetFame() < parameterValue;
                }
                else if (lr.moreThan)
                {
                    return w.GetFame() > parameterValue;
                }
            }
            return false;
        }

        [ScriptParameters(null, typeof(int))]
        static public bool FRA_Turn(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicRequirement lr = advLogic as LogicRequirement;
            int parameterValue = 1;
            if (!String.IsNullOrEmpty(lr.scriptStringParameter))
            {
                float value = UTIL_StringParameterProcessor(lr.scriptStringParameter).t1;
                parameterValue = Mathf.RoundToInt(value);
            }

            var turn = TurnManager.GetTurnNumber();
            if (lr.equal)
            {
                return turn == parameterValue;
            }
            else if (lr.lessThan)
            {
                return turn < parameterValue;
            }
            else if (lr.moreThan)
            {
                return turn > parameterValue;
            }
            return false;
        }

        [ScriptParameters(null, typeof(int))]
        static public bool FRA_Food(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            List<PlayerWizard> wiz = GameManager.GetWizards();
            LogicRequirement lr = advLogic as LogicRequirement;
            int parameterValue = 1;

            if (!String.IsNullOrEmpty(lr.scriptStringParameter))
            {
                float value = UTIL_StringParameterProcessor(lr.scriptStringParameter).t1;
                parameterValue = Mathf.RoundToInt(value);
            }

            foreach (var w in wiz)
            {
                if (!UTIL_IsOwnerValid(w.ID, lr.playerOwner, advData)) continue;

                var food = w.CalculateFoodIncome(true);
                return UTIL_IsValueValid(food, parameterValue, lr.logicComparison);
            }
            return false;
        }

        [ScriptParameters(null, typeof(int))]
        static public bool FRA_Gold(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            List<PlayerWizard> wiz = GameManager.GetWizards();
            LogicRequirement lr = advLogic as LogicRequirement;
            int parameterValue = 1;

            if (!String.IsNullOrEmpty(lr.scriptStringParameter))
            {
                float value = UTIL_StringParameterProcessor(lr.scriptStringParameter).t1;
                parameterValue = Mathf.RoundToInt(value);
            }

            foreach (var w in wiz)
            {
                if (!UTIL_IsOwnerValid(w.ID, lr.playerOwner, advData)) continue;

                return UTIL_IsValueValid(w.money, parameterValue, lr.logicComparison);
            }
            return false;
        }

        [ScriptParameters(typeof(Tag), typeof(FInt))]
        [ScriptParameters(typeof(Tag), null)]
        static public bool FRA_Magic(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            List<PlayerWizard> wiz = GameManager.GetWizards();
            LogicRequirement lr = advLogic as LogicRequirement;
            DBClass t = DataBase.Get(lr.scriptTypeParameter, false);
            Tag tag = (Tag)t;
            FInt parameterValue = FInt.ONE;

            if (!String.IsNullOrEmpty(lr.scriptStringParameter))
            {
                float value = UTIL_StringParameterProcessor(lr.scriptStringParameter).t1;
                parameterValue = new FInt(value);
            }

            foreach (var w in wiz)
            {
                if (!UTIL_IsOwnerValid(w.ID, lr.playerOwner, advData)) continue;

                return w.GetAttributes().Contains(tag, parameterValue);
            }
            return false;
        }

        [ScriptParameters(null, typeof(int))]
        static public bool FRA_Mana(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            List<PlayerWizard> wiz = GameManager.GetWizards();
            LogicRequirement lr = advLogic as LogicRequirement;
            int parameterValue = 1;

            if (!String.IsNullOrEmpty(lr.scriptStringParameter))
            {
                float value = UTIL_StringParameterProcessor(lr.scriptStringParameter).t1;
                parameterValue = Mathf.RoundToInt(value);
            }

            foreach (var w in wiz)
            {
                if (!UTIL_IsOwnerValid(w.ID, lr.playerOwner, advData)) continue;

                return UTIL_IsValueValid(w.GetMana(), parameterValue, lr.logicComparison);
            }
            return false;
        }

        [ScriptType(ScriptType.Type.EditorWizardResult)]
        [ScriptParameters(typeof(Tag), typeof(FInt))]
        [ScriptParameters(typeof(Tag), null)]
        static public bool FRA_WizardHaveTag(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            List<PlayerWizard> wiz = GameManager.GetWizards();
            LogicRequirement lr = advLogic as LogicRequirement;
            DBClass t = DataBase.Get(lr.scriptTypeParameter, false);
            Tag tag = (Tag)t;
            FInt parameterValue = FInt.ONE;

            if (!String.IsNullOrEmpty(lr.scriptStringParameter))
            {
                float value = UTIL_StringParameterProcessor(lr.scriptStringParameter).t1;
                parameterValue = new FInt(value);
            }

            foreach (var w in wiz)
            {
                if (!UTIL_IsOwnerValid(w.ID, lr.playerOwner, advData)) continue;

                return w.GetAttributes().Contains(tag, parameterValue);
            }
            return false;
        }

        [ScriptType(ScriptType.Type.EditorWizardResult)]
        [ScriptParameters(typeof(Tag), typeof(FInt))]
        [ScriptParameters(typeof(Tag), null)]
        static public bool FRA_WizardDoesNotHaveTag(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            return !FRA_WizardHaveTag(advData, baseNode, advLogic, publicLists, localLists);
        }

        [ScriptType(ScriptType.Type.EditorWizardResult)]
        [ScriptParameters(typeof(Spell))]
        static public bool FRA_WizardHaveLearnedSpell(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            List<PlayerWizard> wiz = GameManager.GetWizards();
            LogicRequirement lr = advLogic as LogicRequirement;
            DBClass t = DataBase.Get(lr.scriptTypeParameter, false);
            Spell spell = (Spell)t;

            foreach (var w in wiz)
            {
                if (!UTIL_IsOwnerValid(w.ID, lr.playerOwner, advData)) continue;

                return w.GetSpells().Contains(spell);
            }
            return false;
        }

        [ScriptType(ScriptType.Type.EditorWizardResult)]
        [ScriptParameters(typeof(Enchantment))]
        static public bool FRA_WizardHaveEnchantment(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            List<PlayerWizard> wiz = GameManager.GetWizards();
            LogicRequirement lr = advLogic as LogicRequirement;
            DBClass t = DataBase.Get(lr.scriptTypeParameter, false);
            Enchantment ench = (Enchantment)t;

            foreach (var w in wiz)
            {
                if (!UTIL_IsOwnerValid(w.ID, lr.playerOwner, advData)) continue;

                foreach (var e in w.GetEnchantments())
                {
                    if (e.source == ench)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        [ScriptType(ScriptType.Type.EditorWizardResult)]
        static public bool FRA_WizardHaveWorldEnchantments(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            List<PlayerWizard> wiz = GameManager.GetWizards();
            LogicRequirement lr = advLogic as LogicRequirement;

            foreach (var w in wiz)
            {
                if (!UTIL_IsOwnerValid(w.ID, lr.playerOwner, advData)) continue;

                foreach (var e in w.GetEnchantments())
                {
                    if (e.source.Get().worldEnchantment)
                    {
                        return true;
                    }
                }
                foreach (var e in GameManager.Get().GetEnchantments())
                {
                    if (e.source.Get().worldEnchantment && e.owner == w)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        [ScriptType(ScriptType.Type.EditorWizardResult)]
        static public bool FRA_WizardDoesNotHaveWorldEnchantments(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            return !FRA_WizardHaveWorldEnchantments(advData, baseNode, advLogic, publicLists, localLists);
        }

        [ScriptType(ScriptType.Type.EditorWizardResult)]
        [ScriptParameters(typeof(Spell))]
        static public bool FRA_WizardDoesNotHaveLearnedSpell(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            return !FRA_WizardHaveLearnedSpell(advData, baseNode, advLogic, publicLists, localLists);
        }

        [ScriptType(ScriptType.Type.EditorWizardResult)]
        [ScriptParameters(typeof(Enchantment))]
        static public bool FRA_WizardDoesNotHaveEnchantment(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            return !FRA_WizardHaveEnchantment(advData, baseNode, advLogic, publicLists, localLists);
        }
        [ScriptType(ScriptType.Type.EditorWizardResult)]
        static public bool FRA_WizardHaveMagicNode(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicRequirement lr = advLogic as LogicRequirement;
            //LogicUtils.Comparison lComp = (LogicUtils.Comparison)Enum.Parse(typeof(LogicUtils.Comparison), lr.scriptTypeParameter);
            AdvList al = advData.GetListByName(lr.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lr.listA, publicLists);
            }
            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lr.listA + ", doesn't exist");
                return false;
            }

            foreach (var v in al.list)
            {
                if (v is PlayerWizard)
                {
                    PlayerWizard wizard = v as PlayerWizard;
                    var locs = GameManager.GetLocationsOfWizard(wizard.ID);

                    return (locs != null && locs.Find(o => o.locationType == ELocationType.Node) != null);
                }
                else
                {
                    Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element:" + v.ToString() + ", isn't PlayerWizard type");
                }
            }

            return false;
        }

        [ScriptType(ScriptType.Type.EditorWizardResult)]
        [ScriptParameters(typeof(Spell))]
        static public bool FRA_WizardIsResearchnigSpell(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            List<PlayerWizard> wiz = GameManager.GetWizards();
            LogicRequirement lr = advLogic as LogicRequirement;
            DBClass t = DataBase.Get(lr.scriptTypeParameter, false);

            if (t != null)
            {
                DBReference<Spell> s = (DBReference<Spell>)t;
                foreach (var w in wiz)
                {
                    if (!UTIL_IsOwnerValid(w.ID, lr.playerOwner, advData)) continue;

                    return w.magicAndResearch.curentlyResearched == s;
                }
            }
            return false;
        }
        [ScriptType(ScriptType.Type.EditorWizardResult)]
        static public bool FRA_WizardIsResearchnigAnySpell(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            List<PlayerWizard> wiz = GameManager.GetWizards();
            LogicRequirement lr = advLogic as LogicRequirement;

            foreach (var w in wiz)
            {
                if (!UTIL_IsOwnerValid(w.ID, lr.playerOwner, advData)) continue;

                return w.magicAndResearch.curentlyResearched != null;
            }
            return false;
        }

        [ScriptType(ScriptType.Type.EditorHeroResult)]
        [ScriptParameters(typeof(Tag), typeof(FInt))]
        [ScriptParameters(typeof(Tag), null)]
        static public bool FRA_HeroHaveTag(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicRequirement lr = advLogic as LogicRequirement;
            DBClass t = DataBase.Get(lr.scriptTypeParameter, false);
            string listName = lr.listA;
            AdvList al = advData.GetListByName(listName, localLists);
            if (al == null)
            {
                al = advData.GetListByName(listName, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lr.listA + ", doesn't exist");
                return false;
            }

            Tag tag = (Tag)t;
            FInt parameterValue = FInt.ONE;
            if (!String.IsNullOrEmpty(lr.scriptStringParameter))
            {
                float value = UTIL_StringParameterProcessor(lr.scriptStringParameter).t1;
                parameterValue = new FInt(value);
            }

            foreach (var v in al.list)
            {
                MOM.Unit hero = v as MOM.Unit;
                if (hero == null || !(hero.dbSource.Get() is Hero))
                {
                    Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; Object: " + v.ToString() + ", is not a Hero type");
                    continue;
                }

                if (hero.GetAttributes().Contains(tag, parameterValue)) return true;
            }
            return false;
        }

        [ScriptType(ScriptType.Type.EditorHeroResult)]
        [ScriptParameters(typeof(Tag), typeof(FInt))]
        [ScriptParameters(typeof(Tag), null)]
        static public bool FRA_HeroDoesNotHaveTag(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            return !FRA_HeroHaveTag(advData, baseNode, advLogic, publicLists, localLists);
        }

        [ScriptType(ScriptType.Type.EditorHeroResult)]
        [ScriptParameters(typeof(Skill), null)]
        static public bool FRA_HeroHaveSkill(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicRequirement lr = advLogic as LogicRequirement;
            DBClass t = DataBase.Get(lr.scriptTypeParameter, false);
            string listName = lr.listA;
            AdvList al = advData.GetListByName(listName, localLists);
            if (al == null)
            {
                al = advData.GetListByName(listName, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lr.listA + ", doesn't exist");
                return false;
            }

            foreach (var v in al.list)
            {
                MOM.Unit hero = v as MOM.Unit;
                if (hero == null || !(hero.dbSource.Get() is Hero))
                {
                    Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; Object: " + v.ToString() + ", is not a Hero type");
                    continue;
                }

                if (hero.GetSkills().Contains(t as Skill)) return true;
            }
            return false;
        }

        [ScriptType(ScriptType.Type.EditorHeroResult)]
        [ScriptParameters(typeof(Skill), null)]
        static public bool FRA_HeroDoesNotHaveSkill(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            return !FRA_HeroHaveSkill(advData, baseNode, advLogic, publicLists, localLists);
        }

        [ScriptType(ScriptType.Type.EditorHeroResult)]
        [ScriptParameters(typeof(ETerrainCategory))]
        static public bool FRA_HeroOnTerrain(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicRequirement lr = advLogic as LogicRequirement;
            ETerrainCategory terrainCat = (ETerrainCategory)Enum.Parse(typeof(ETerrainCategory), lr.scriptTypeParameter);
            string listName = lr.listA;
            AdvList al = advData.GetListByName(listName, localLists);
            if (al == null)
            {
                al = advData.GetListByName(listName, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lr.listA + ", doesn't exist");
                return false;
            }

            foreach (var v in al.list)
            {
                MOM.Unit unit = v as MOM.Unit;
                if (unit == null || !(unit.dbSource.Get() is Hero))
                {
                    Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; Object: " + v.ToString() + ", is not a Hero type");
                    continue;
                }

                WorldCode.Plane plane = unit.GetPlane();
                Hex hex = plane.GetHexAt(unit.GetPosition());
                if (UTIL_IsTerrainTypeValid(terrainCat, hex, unit))
                {
                    return true;
                }
            }
            return false;
        }


        [ScriptType(ScriptType.Type.EditorCityResult)]
        [ScriptParameters(typeof(ETerrainCategory))]
        static public bool FRA_CityOnTerrain(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicRequirement lr = advLogic as LogicRequirement;
            ETerrainCategory terrainCat = (ETerrainCategory)Enum.Parse(typeof(ETerrainCategory), lr.scriptTypeParameter);
            string listName = lr.listA;
            AdvList al = advData.GetListByName(listName, localLists);
            if (al == null)
            {
                al = advData.GetListByName(listName, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lr.listA + ", doesn't exist");
                return false;
            }

            foreach (var v in al.list)
            {
                TownLocation town = v as TownLocation;
                if (town == null)
                {
                    Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; element on list :" + lr.listA + ", is not a City type");
                    continue;
                }

                WorldCode.Plane plane = town.GetPlane();
                Hex hex = plane.GetHexAt(town.GetPosition());
                if (UTIL_IsTerrainTypeValid(terrainCat, hex, town))
                {
                    return true;
                }
            }
            return false;
        }

        [ScriptType(ScriptType.Type.EditorCityResult)]
        [ScriptParameters(typeof(LogicUtils.Comparison), typeof(FInt))]
        static public bool FRA_Unrest(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicRequirement lr = advLogic as LogicRequirement;
            LogicUtils.Comparison lComp = (LogicUtils.Comparison)Enum.Parse(typeof(LogicUtils.Comparison), lr.scriptTypeParameter);
            AdvList al = advData.GetListByName(lr.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lr.listA, publicLists);
            }
            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lr.listA + ", doesn't exist");
                return false;
            }

            FInt parameterValue = FInt.ONE;
            if (!String.IsNullOrEmpty(lr.scriptStringParameter))
            {
                float value = UTIL_StringParameterProcessor(lr.scriptStringParameter).t1;
                parameterValue = new FInt(value);
            }

            foreach (var v in al.list)
            {
                if (v is TownLocation)
                {
                    TownLocation town = v as TownLocation;
                    FInt unrest = (FInt)town.GetUnrest();
                    return UTIL_IsValueValid(unrest, parameterValue * 0.01f, lComp);
                }
                else
                {
                    Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element:" + v.ToString() + ", isn't TownLocation type");
                }
            }

            return false;
        }
        [ScriptType(ScriptType.Type.EditorCityResult)]
        [ScriptParameters(typeof(LogicUtils.Comparison), typeof(FInt))]
        static public bool FRA_RebelsToPopulation(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicRequirement lr = advLogic as LogicRequirement;
            LogicUtils.Comparison lComp = (LogicUtils.Comparison)Enum.Parse(typeof(LogicUtils.Comparison), lr.scriptTypeParameter);
            AdvList al = advData.GetListByName(lr.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lr.listA, publicLists);
            }
            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lr.listA + ", doesn't exist");
                return false;
            }

            FInt parameterValue = FInt.ONE;
            if (!String.IsNullOrEmpty(lr.scriptStringParameter))
            {
                float value = UTIL_StringParameterProcessor(lr.scriptStringParameter).t1;
                parameterValue = new FInt(value);
            }

            foreach (var v in al.list)
            {
                if (v is TownLocation)
                {
                    TownLocation town = v as TownLocation;
                    var rebels = town.GetRebels();
                    FInt rebelsToPop = FInt.ZERO;
                    if (rebels > 0)
                    {
                        rebelsToPop = (FInt)rebels / town.GetPopUnits();
                    }
                    return UTIL_IsValueValid(rebelsToPop, parameterValue * 0.01f, lComp);
                }
                else
                {
                    Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element:" + v.ToString() + ", isn't TownLocation type");
                }
            }

            return false;
        }

        [ScriptType(ScriptType.Type.EditorCityResult)]
        [ScriptParameters(typeof(LogicUtils.Comparison), typeof(FInt))]
        static public bool FRA_Population(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicRequirement lr = advLogic as LogicRequirement;
            LogicUtils.Comparison lComp = (LogicUtils.Comparison)Enum.Parse(typeof(LogicUtils.Comparison), lr.scriptTypeParameter);
            AdvList al = advData.GetListByName(lr.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lr.listA, publicLists);
            }
            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lr.listA + ", doesn't exist");
                return false;
            }

            int parameterValue = 1;
            if (!String.IsNullOrEmpty(lr.scriptStringParameter))
            {
                float value = UTIL_StringParameterProcessor(lr.scriptStringParameter).t1;
                parameterValue = Mathf.RoundToInt(value);
            }

            foreach (var v in al.list)
            {
                if (v is TownLocation)
                {
                    TownLocation town = v as TownLocation;
                    return UTIL_IsValueValid(town.GetPopUnits(), parameterValue, lComp);
                }
                else
                {
                    Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element:" + v.ToString() + ", isn't TownLocation type");
                }
            }

            return false;
        }

        [ScriptType(ScriptType.Type.EditorCityResult)]
        [ScriptParameters(typeof(LogicUtils.Comparison), typeof(FInt))]
        static public bool FRA_GoldProduction(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicRequirement lr = advLogic as LogicRequirement;
            LogicUtils.Comparison lComp = (LogicUtils.Comparison)Enum.Parse(typeof(LogicUtils.Comparison), lr.scriptTypeParameter);
            AdvList al = advData.GetListByName(lr.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lr.listA, publicLists);
            }
            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lr.listA + ", doesn't exist");
                return false;
            }

            int parameterValue = 1;
            if (!String.IsNullOrEmpty(lr.scriptStringParameter))
            {
                float value = UTIL_StringParameterProcessor(lr.scriptStringParameter).t1;
                parameterValue = Mathf.RoundToInt(value);
            }

            foreach (var v in al.list)
            {
                if (v is TownLocation)
                {
                    TownLocation town = v as TownLocation;
                    return UTIL_IsValueValid(town.CalculateMoneyIncome(), parameterValue, lComp);
                }
                else
                {
                    Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element:" + v.ToString() + ", isn't TownLocation type");
                }
            }

            return false;
        }

        [ScriptType(ScriptType.Type.EditorCityResult)]
        static public bool FRA_GarrisonInCity(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicRequirement lr = advLogic as LogicRequirement;
            AdvList al = advData.GetListByName(lr.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lr.listA, publicLists);
            }
            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lr.listA + ", doesn't exist");
                return false;
            }

            foreach (var v in al.list)
            {
                if (v is TownLocation)
                {
                    TownLocation town = v as TownLocation;
                    return (town.GetUnits() != null && town.GetUnits().Count > 0);
                }
                else
                {
                    Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element:" + v.ToString() + ", isn't TownLocation type");
                }
            }

            return false;
        }
        [ScriptType(ScriptType.Type.EditorCityResult)]
        static public bool FRA_HeroInCity(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicRequirement lr = advLogic as LogicRequirement;
            AdvList al = advData.GetListByName(lr.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lr.listA, publicLists);
            }
            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lr.listA + ", doesn't exist");
                return false;
            }

            foreach (var v in al.list)
            {
                if (v is TownLocation)
                {
                    TownLocation town = v as TownLocation;
                    var units = town.GetUnits();
                    return (units != null && units.Find(o => o.Get().dbSource.Get() is Hero) != null);
                }
                else
                {
                    Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element:" + v.ToString() + ", isn't TownLocation type");
                }
            }

            return false;
        }

        [ScriptType(ScriptType.Type.EditorCityResult)]
        [ScriptParameters(typeof(Resource), null)]
        static public bool FRA_ResourceInCityRange(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicRequirement lr = advLogic as LogicRequirement;
            AdvList al = advData.GetListByName(lr.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lr.listA, publicLists);
            }
            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lr.listA + ", doesn't exist");
                return false;
            }

            DBClass t = DataBase.Get(lr.scriptTypeParameter, false);
            List<object> l2 = new List<object>();

            foreach (var v in al.list)
            {
                TownLocation tl = v as TownLocation;
                if (tl == null)
                {
                    Debug.Log("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element:" + v.ToString() + ", doesn't contain TownLocation");
                    continue;
                }

                foreach (var res in tl.GetPotentialResources())
                {
                    if (res == (t as Resource))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        [ScriptType(ScriptType.Type.EditorCityResult)]
        [ScriptParameters(typeof(Resource), typeof(FInt))]
        static public bool FRA_ResourcelInRangeOf(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicRequirement lr = advLogic as LogicRequirement;
            AdvList al = advData.GetListByName(lr.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lr.listA, publicLists);
            }
            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lr.listA + ", doesn't exist");
                return false;
            }

            DBClass t = DataBase.Get(lr.scriptTypeParameter, false);
            int parameterValue = 1;

            if (!String.IsNullOrEmpty(lr.scriptStringParameter))
            {
                float value = UTIL_StringParameterProcessor(lr.scriptStringParameter).t1;
                parameterValue = Mathf.RoundToInt(value);
            }

            List<object> l2 = new List<object>();
            List<Vector3i> positions;
            Hex hex;

            foreach (var v in al.list)
            {
                IPlanePosition ipp = v as IPlanePosition;
                if (ipp == null)
                {
                    Debug.Log("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element:" + v.ToString() + ", doesn't implement IPlanePosition");
                    continue;
                }
                WorldCode.Plane plane = ipp.GetPlane();
                positions = ipp.GetSurroundingArea(parameterValue);

                foreach (var pos in positions)
                {
                    hex = plane.GetHexAt(pos);
                    if (hex == null || hex.Resource == null) continue;

                    if (hex.Resource.Get() == (t as Resource))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        [ScriptType(ScriptType.Type.EditorSharedTagResult)]
        [ScriptParameters(typeof(Tag), typeof(FInt))]
        [ScriptParameters(typeof(Tag), null)]
        static public bool FRA_HaveSharedTag(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicRequirement lr = advLogic as LogicRequirement;
            List<PlayerWizard> wizards = GameManager.GetWizards();
            //             AdvList al = advData.GetListByName(lr.listA, localLists);
            //             if (al == null)
            //             {
            //                 al = advData.GetListByName(lr.listA, publicLists);
            //             }
            //             if (al == null)
            //             {
            //                 Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lr.listA + ", doesn't exist");
            //                 return false;
            //             }

            DBClass t = DataBase.Get(lr.scriptTypeParameter, false);
            Tag tag = (Tag)t;
            FInt parameterValue = FInt.ONE;

            if (!String.IsNullOrEmpty(lr.scriptStringParameter))
            {
                float value = UTIL_StringParameterProcessor(lr.scriptStringParameter).t1;
                parameterValue = new FInt(value);
            }

            if (tag != null)
            {
                foreach (var w in wizards)
                {
                    if (!UTIL_IsOwnerValid(w.ID, lr.playerOwner, advData)) continue;

                    return w.GetAttributes().Contains(tag, parameterValue);
                }
            }
            else
            {
                Debug.Log("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; Tag parameter is missing");
            }

            //             foreach (var v in al.list)
            //             {
            //                 IAttributable iatt = v as IAttributable;
            //                 if (iatt == null)
            //                 {
            //                     Debug.Log("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element:" + v.ToString() + ", doesn't implement IAttributable");
            //                     continue;
            //                 }
            // 
            //                 return iatt.GetAttributes().Contains(tag, parameterValue);
            //             }

            return false;
        }

        [ScriptType(ScriptType.Type.EditorSharedTagResult)]
        [ScriptParameters(typeof(Tag), typeof(FInt))]
        [ScriptParameters(typeof(Tag), null)]
        static public bool FRA_DoesNotHaveSharedTag(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            return !FRA_HaveSharedTag(advData, baseNode, advLogic, publicLists, localLists);
        }

        [ScriptType(ScriptType.Type.EditorFamiliarResult)]
        [ScriptParameters(typeof(PlayerWizard.Familiar))]
        static public bool FRA_FamiliarOfMagic(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicRequirement lr = advLogic as LogicRequirement;
            var wiz = GameManager.GetWizards();
            PlayerWizard.Familiar f = (PlayerWizard.Familiar)Enum.Parse(typeof(PlayerWizard.Familiar), lr.scriptTypeParameter);

            foreach (var w in wiz)
            {
                if (!UTIL_IsOwnerValid(w.ID, lr.playerOwner, advData)) continue;
                if (w.familiar == f)
                {
                    return true;
                }
            }

            return false;
        }
        [ScriptType(ScriptType.Type.OtherCriteria)]
        static public bool FRA_IsGarrisonInLocation(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            var adventureSource = advData.advSource;
            if (adventureSource == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; advSource in AdventureData doesn't exist");
                return false;
            }

            if (adventureSource is MOM.Location)
            {
                if (adventureSource.GetUnits() != null && adventureSource.GetUnits().Count > 0) return true;
            }

            return false;
        }
        [ScriptType(ScriptType.Type.OtherCriteria)]
        static public bool FRA_ActiveWizardIsAI(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            return advData.mainPlayerWizard > 1;
        }

        #endregion

        #region Modifier
        [ScriptType(ScriptType.Type.EditorModifier)]
        [ScriptParameters(typeof(Tag), typeof(Multitype<float, float, float>))]
        [ScriptParameters(typeof(Tag), null)]
        static public void FMO_AddTag(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicModifier lm = advLogic as LogicModifier;
            DBClass t = DataBase.Get(lm.scriptTypeParameter, false);
            AdvList al = advData.GetListByName(lm.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lm.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", doesn't exist");
                return;
            }

            FInt parameterValue = FInt.ONE;
            if (!String.IsNullOrEmpty(lm.scriptStringParameter))
            {
                parameterValue = (FInt)UTIL_GetStringParameterValue(lm.scriptStringParameter);
            }

            Tag tag = t as Tag;

            foreach (var v in al.list)
            {
                if (v is IAttributable)
                {
                    IAttributable iatr = v as IAttributable;
                    iatr.GetAttributes().AddToBase(tag, parameterValue);
                }
                else if (v is Group)
                {
                    Group g = v as Group;
                    foreach (var u in g.GetUnits())
                    {
                        u.Get().GetAttributes().AddToBase(tag, parameterValue);
                    }
                }
                else
                {
                    Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element:" + v.ToString() + ", isn't IAttributable type");
                }
            }
        }

        [ScriptType(ScriptType.Type.EditorModifier)]
        [ScriptParameters(typeof(Tag), typeof(Multitype<float, float, float>))]
        static public void FMO_SetTag(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicModifier lm = advLogic as LogicModifier;
            DBClass t = DataBase.Get(lm.scriptTypeParameter, false);
            AdvList al = advData.GetListByName(lm.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lm.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", doesn't exist");
                return;
            }

            FInt parameterValue = FInt.ONE;
            if (!String.IsNullOrEmpty(lm.scriptStringParameter))
            {
                parameterValue = (FInt)UTIL_GetStringParameterValue(lm.scriptStringParameter);
            }

            Tag tag = t as Tag;

            foreach (var v in al.list)
            {
                if (v is IAttributable)
                {
                    IAttributable iatr = v as IAttributable;
                    iatr.GetAttributes().SetBaseTo(tag, parameterValue);
                }
                else
                {
                    Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element:" + v.ToString() + ", isn't IAttributable type");
                }
            }
        }

        [ScriptType(ScriptType.Type.EditorModifier)]
        [ScriptParameters(null, typeof(Multitype<float, float, float>))]
        static public void FMO_AddPopulation(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicModifier lm = advLogic as LogicModifier;
            AdvList al = advData.GetListByName(lm.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lm.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", doesn't exist");
            }

            int parameterValue = 0;
            if (!String.IsNullOrEmpty(lm.scriptStringParameter))
            {
                parameterValue = UTIL_GetStringParameterValue(lm.scriptStringParameter);
            }

            foreach (var v in al.list)
            {
                if (v is TownLocation)
                {
                    TownLocation town = v as TownLocation;
                    town.Population += parameterValue * 1000;
                }
                else
                {
                    Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element:" + v.ToString() + ", isn't TownLocation type");
                }
            }
        }

        [ScriptType(ScriptType.Type.EditorModifier)]
        [ScriptParameters(null, typeof(Multitype<float, float, float>))]
        static public void FMO_TakePopulation(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicModifier lm = advLogic as LogicModifier;
            AdvList al = advData.GetListByName(lm.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lm.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", doesn't exist");
            }

            int parameterValue = 0;
            if (!String.IsNullOrEmpty(lm.scriptStringParameter))
            {
                parameterValue = UTIL_GetStringParameterValue(lm.scriptStringParameter);
            }

            foreach (var v in al.list)
            {
                if (v is TownLocation)
                {
                    TownLocation town = v as TownLocation;
                    town.Population -= parameterValue * 1000; // Set accessor will limit to 1000
                }
                else
                {
                    Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element:" + v.ToString() + ", isn't TownLocation type");
                }
            }
        }

        [ScriptType(ScriptType.Type.EditorModifier)]
        [ScriptParameters(null, typeof(Multitype<float, float, float>))]
        static public void FMO_AddPopulationPercentage(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicModifier lm = advLogic as LogicModifier;
            AdvList al = advData.GetListByName(lm.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lm.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", doesn't exist");
            }

            float parameterValue = 0f;
            if (!String.IsNullOrEmpty(lm.scriptStringParameter))
            {
                parameterValue = UTIL_GetStringParameterValue(lm.scriptStringParameter);
                parameterValue = parameterValue * 0.01f;
            }

            foreach (var v in al.list)
            {
                if (v is TownLocation)
                {
                    TownLocation town = v as TownLocation;

                    int percentageValue = Mathf.RoundToInt(town.Population * parameterValue);
                    town.Population += percentageValue;
                }
                else
                {
                    Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element:" + v.ToString() + ", isn't TownLocation type");
                }
            }
        }

        [ScriptType(ScriptType.Type.EditorModifier)]
        [ScriptParameters(null, typeof(Multitype<float, float, float>))]
        static public void FMO_TakePopulationPercentage(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            //TODO check population units
            LogicModifier lm = advLogic as LogicModifier;
            AdvList al = advData.GetListByName(lm.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lm.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", doesn't exist");
            }

            float parameterValue = 0f;
            if (!String.IsNullOrEmpty(lm.scriptStringParameter))
            {
                parameterValue = UTIL_GetStringParameterValue(lm.scriptStringParameter);
                parameterValue = parameterValue * 0.01f;
            }

            foreach (var v in al.list)
            {
                if (v is TownLocation)
                {
                    TownLocation town = v as TownLocation;
                    int percentageValue = Mathf.RoundToInt(town.Population * parameterValue);
                    town.Population -= percentageValue; // set accessor will not allow below 10000 if currently above.
                }
                else
                {
                    Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element:" + v.ToString() + ", isn't TownLocation type");
                }
            }
        }

        [ScriptType(ScriptType.Type.EditorModifier)]
        [ScriptParameters(null, typeof(Multitype<float, float, float>))]
        static public void FMO_AddGold(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicModifier lm = advLogic as LogicModifier;
            AdvList al = advData.GetListByName(lm.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lm.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", doesn't exist");
            }

            int parameterValue = 0;
            if (!String.IsNullOrEmpty(lm.scriptStringParameter))
            {
                parameterValue = UTIL_GetStringParameterValue(lm.scriptStringParameter);
            }

            foreach (var v in al.list)
            {
                if (v is PlayerWizard)
                {
                    PlayerWizard wizard = v as PlayerWizard;
                    wizard.money += parameterValue;
                }
                else
                {
                    Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element:" + v.ToString() + ", isn't PlayerWizard type");
                }
            }
        }

        [ScriptType(ScriptType.Type.EditorModifier)]
        [ScriptParameters(null, typeof(Multitype<float, float, float>))]
        static public void FMO_TakeGold(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicModifier lm = advLogic as LogicModifier;
            AdvList al = advData.GetListByName(lm.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lm.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", doesn't exist");
            }

            int parameterValue = 0;
            if (!String.IsNullOrEmpty(lm.scriptStringParameter))
            {
                parameterValue = UTIL_GetStringParameterValue(lm.scriptStringParameter);
            }

            foreach (var v in al.list)
            {
                if (v is PlayerWizard)
                {
                    PlayerWizard wizard = v as PlayerWizard;
                    if (wizard.money - parameterValue >= 0)
                    {
                        wizard.money -= parameterValue;
                    }
                    else
                    {
                        wizard.money = 0;
                    }
                }
                else
                {
                    Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element:" + v.ToString() + ", isn't PlayerWizard type");
                }
            }
        }

        [ScriptType(ScriptType.Type.EditorModifier)]
        [ScriptParameters(null, typeof(Multitype<float, float, float>))]
        static public void FMO_AddGoldPercentage(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicModifier lm = advLogic as LogicModifier;
            AdvList al = advData.GetListByName(lm.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lm.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", doesn't exist");
            }

            float parameterValue = 0f;
            if (!String.IsNullOrEmpty(lm.scriptStringParameter))
            {
                parameterValue = UTIL_GetStringParameterValue(lm.scriptStringParameter);
                parameterValue = parameterValue * 0.01f;
            }

            foreach (var v in al.list)
            {
                if (v is PlayerWizard)
                {
                    PlayerWizard wizard = v as PlayerWizard;
                    int percentageValue = Mathf.RoundToInt(wizard.money * parameterValue);
                    wizard.money += percentageValue;
                }
                else
                {
                    Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element:" + v.ToString() + ", isn't PlayerWizard type");
                }
            }
        }


        [ScriptType(ScriptType.Type.EditorModifier)]
        [ScriptParameters(null, typeof(Multitype<float, float, float>))]
        static public void FMO_TakeGoldPercentage(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicModifier lm = advLogic as LogicModifier;
            AdvList al = advData.GetListByName(lm.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lm.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", doesn't exist");
            }

            float parameterValue = 0f;
            if (!String.IsNullOrEmpty(lm.scriptStringParameter))
            {
                parameterValue = UTIL_GetStringParameterValue(lm.scriptStringParameter);
                parameterValue = parameterValue * 0.01f;
            }

            foreach (var v in al.list)
            {
                if (v is PlayerWizard)
                {
                    PlayerWizard wizard = v as PlayerWizard;
                    int percentageValue = Mathf.RoundToInt(wizard.money * parameterValue);
                    if (wizard.money - percentageValue >= 0)
                    {
                        wizard.money -= percentageValue;
                    }
                    else
                    {
                        wizard.money = 0;
                    }
                }
                else
                {
                    Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element:" + v.ToString() + ", isn't PlayerWizard type");
                }
            }
        }

        [ScriptType(ScriptType.Type.EditorModifier)]
        [ScriptParameters(null, typeof(Multitype<float, float, float>))]
        static public void FMO_AddGoldPerTurn(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicModifier lm = advLogic as LogicModifier;
            AdvList al = advData.GetListByName(lm.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lm.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", doesn't exist");
            }

            string parameter = UTIL_GetStringParameterValue(lm.scriptStringParameter).ToString();
            foreach (var v in al.list)
            {
                if (v is IEnchantable)
                {
                    IEnchantable ie = v as IEnchantable;
                    ie.AddEnchantment((Enchantment)ENCH.ADD_GOLD_PER_TURN, null, -1, parameter);
                }
                else
                {
                    Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element:" + v.ToString() + ", isn't IEnchantable type");
                }
            }
        }

        [ScriptType(ScriptType.Type.EditorModifier)]
        [ScriptParameters(null, typeof(Multitype<float, float, float>))]
        static public void FMO_TakeGoldPerTurn(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicModifier lm = advLogic as LogicModifier;
            AdvList al = advData.GetListByName(lm.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lm.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", doesn't exist");
            }

            string parameter = UTIL_GetStringParameterValue(lm.scriptStringParameter).ToString();
            foreach (var v in al.list)
            {
                if (v is IEnchantable)
                {
                    IEnchantable ie = v as IEnchantable;
                    ie.AddEnchantment((Enchantment)ENCH.TAKE_GOLD_PER_TURN, null, -1, parameter);
                }
                else
                {
                    Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element:" + v.ToString() + ", isn't IEnchantable type");
                }
            }
        }

        [ScriptType(ScriptType.Type.EditorModifier)]
        [ScriptParameters(null, typeof(Multitype<float, float, float>))]
        static public void FMO_AddFoodPerTurn(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicModifier lm = advLogic as LogicModifier;
            AdvList al = advData.GetListByName(lm.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lm.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", doesn't exist");
            }

            string parameter = UTIL_GetStringParameterValue(lm.scriptStringParameter).ToString();
            foreach (var v in al.list)
            {
                if (v is IEnchantable)
                {
                    IEnchantable ie = v as IEnchantable;
                    ie.AddEnchantment((Enchantment)ENCH.ADD_FOOD_PER_TURN, null, -1, parameter);
                }
                else
                {
                    Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element:" + v.ToString() + ", isn't IEnchantable type");
                }
            }
        }

        [ScriptType(ScriptType.Type.EditorModifier)]
        [ScriptParameters(null, typeof(Multitype<float, float, float>))]
        static public void FMO_TakeFoodPerTurn(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicModifier lm = advLogic as LogicModifier;
            AdvList al = advData.GetListByName(lm.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lm.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", doesn't exist");
            }

            string parameter = UTIL_GetStringParameterValue(lm.scriptStringParameter).ToString();
            foreach (var v in al.list)
            {
                if (v is IEnchantable)
                {
                    IEnchantable ie = v as IEnchantable;
                    ie.AddEnchantment((Enchantment)ENCH.TAKE_FOOD_PER_TURN, null, -1, parameter);
                }
                else
                {
                    Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element:" + v.ToString() + ", isn't IEnchantable type");
                }
            }
        }

        [ScriptType(ScriptType.Type.EditorModifier)]
        [ScriptParameters(null, typeof(Multitype<float, float, float>))]
        static public void FMO_AddMana(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicModifier lm = advLogic as LogicModifier;
            AdvList al = advData.GetListByName(lm.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lm.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", doesn't exist");
            }

            int parameterValue = 0;
            if (!String.IsNullOrEmpty(lm.scriptStringParameter))
            {
                parameterValue = UTIL_GetStringParameterValue(lm.scriptStringParameter);
            }

            foreach (var v in al.list)
            {
                if (v is PlayerWizard)
                {
                    PlayerWizard wizard = v as PlayerWizard;
                    wizard.mana += parameterValue;
                }
                else
                {
                    Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element:" + v.ToString() + ", isn't PlayerWizard type");
                }
            }
        }

        [ScriptType(ScriptType.Type.EditorModifier)]
        [ScriptParameters(null, typeof(Multitype<float, float, float>))]
        static public void FMO_TakeMana(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicModifier lm = advLogic as LogicModifier;
            AdvList al = advData.GetListByName(lm.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lm.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", doesn't exist");
            }

            int parameterValue = 0;
            if (!String.IsNullOrEmpty(lm.scriptStringParameter))
            {
                parameterValue = UTIL_GetStringParameterValue(lm.scriptStringParameter);
            }

            foreach (var v in al.list)
            {
                if (v is PlayerWizard)
                {
                    PlayerWizard wizard = v as PlayerWizard;
                    if (wizard.mana - parameterValue >= 0)
                    {
                        wizard.mana -= parameterValue;
                    }
                    else
                    {
                        wizard.mana = 0;
                    }
                }
                else
                {
                    Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element:" + v.ToString() + ", isn't PlayerWizard type");
                }
            }
        }

        [ScriptType(ScriptType.Type.EditorModifier)]
        [ScriptParameters(null, typeof(Multitype<float, float, float>))]
        static public void FMO_AddManaPerTurn(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicModifier lm = advLogic as LogicModifier;
            AdvList al = advData.GetListByName(lm.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lm.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", doesn't exist");
            }

            string parameter = UTIL_GetStringParameterValue(lm.scriptStringParameter).ToString();
            foreach (var v in al.list)
            {
                if (v is IEnchantable)
                {
                    IEnchantable ie = v as IEnchantable;
                    ie.AddEnchantment((Enchantment)ENCH.ADD_MANA_PER_TURN, null, -1, parameter);
                }
                else
                {
                    Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element:" + v.ToString() + ", isn't IEnchantable type");
                }
            }
        }

        [ScriptType(ScriptType.Type.EditorModifier)]
        [ScriptParameters(null, typeof(Multitype<float, float, float>))]
        static public void FMO_TakeManaPerTurn(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicModifier lm = advLogic as LogicModifier;
            AdvList al = advData.GetListByName(lm.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lm.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", doesn't exist");
            }

            string parameter = UTIL_GetStringParameterValue(lm.scriptStringParameter).ToString();
            foreach (var v in al.list)
            {
                if (v is IEnchantable)
                {
                    IEnchantable ie = v as IEnchantable;
                    ie.AddEnchantment((Enchantment)ENCH.TAKE_MANA_PER_TURN, null, -1, parameter);
                }
                else
                {
                    Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element:" + v.ToString() + ", isn't IEnchantable type");
                }
            }
        }

        [ScriptType(ScriptType.Type.EditorModifier)]
        [ScriptParameters(null, typeof(Multitype<float, float, float>))]
        static public void FMO_AddManaPercentage(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicModifier lm = advLogic as LogicModifier;
            AdvList al = advData.GetListByName(lm.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lm.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", doesn't exist");
            }

            float parameterValue = 0f;
            if (!String.IsNullOrEmpty(lm.scriptStringParameter))
            {
                parameterValue = UTIL_GetStringParameterValue(lm.scriptStringParameter);
                parameterValue = parameterValue * 0.01f;
            }

            foreach (var v in al.list)
            {
                if (v is PlayerWizard)
                {
                    PlayerWizard wizard = v as PlayerWizard;
                    int percentageValue = Mathf.RoundToInt(wizard.GetMana() * parameterValue);
                    wizard.mana += percentageValue;
                }
                else
                {
                    Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element:" + v.ToString() + ", isn't PlayerWizard type");
                }
            }
        }

        [ScriptType(ScriptType.Type.EditorModifier)]
        [ScriptParameters(null, typeof(Multitype<float, float, float>))]
        static public void FMO_TakeManaPercentage(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicModifier lm = advLogic as LogicModifier;
            AdvList al = advData.GetListByName(lm.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lm.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", doesn't exist");
            }

            float parameterValue = 0f;
            if (!String.IsNullOrEmpty(lm.scriptStringParameter))
            {
                parameterValue = UTIL_GetStringParameterValue(lm.scriptStringParameter);
                parameterValue = parameterValue * 0.01f;
            }

            foreach (var v in al.list)
            {
                if (v is PlayerWizard)
                {
                    PlayerWizard wizard = v as PlayerWizard;
                    int percentageValue = Mathf.RoundToInt(wizard.GetMana() * parameterValue);
                    if (wizard.mana - percentageValue >= 0)
                    {
                        wizard.mana -= percentageValue;
                    }
                    else
                    {
                        wizard.mana = 0;
                    }
                }
                else
                {
                    Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element:" + v.ToString() + ", isn't PlayerWizard type");
                }
            }
        }

        [ScriptParameters(typeof(Hero), null)]
        static public void FMO_AddHero(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicModifier lm = advLogic as LogicModifier;
            List<Hero> heroesList = new List<Hero>(DataBase.GetType<Hero>());

            if (lm.requirementGroups != null)
            {
                foreach (var v in lm.requirementGroups)
                {
                    for (int i = 0; i < heroesList.Count; i++)
                    {
                        bool fulfilled = false;
                        if (v.options != null)
                        {
                            foreach (var o in v.options)
                            {
                                var dbc = DataBase.Get(o.typeData, true);
                                if (dbc is Hero)
                                {
                                    if (heroesList[i] == dbc)
                                    {
                                        fulfilled = true;
                                        break;
                                    }
                                }
                                else if (dbc is Tag)
                                {
                                    var cTag = Array.Find(heroesList[i].tags, ct => ct.tag == dbc);
                                    if (cTag != null)
                                    {
                                        switch (o.sign)
                                        {
                                            case ">=":
                                                fulfilled = cTag.amount >= o.value;
                                                break;
                                            case ">":
                                                fulfilled = cTag.amount > o.value;
                                                break;
                                            case "=":
                                                fulfilled = cTag.amount == o.value;
                                                break;
                                            case "<":
                                                fulfilled = cTag.amount < o.value;
                                                break;
                                            case "<=":
                                                fulfilled = cTag.amount <= o.value;
                                                break;
                                            default:
                                                Debug.Log("Sign is: " + o.sign);
                                                break;
                                        }
                                    }
                                }
                            }
                        }
                        if (!fulfilled)
                        {
                            heroesList.RemoveAt(i);
                            i--;
                        }
                    }
                }

                if (heroesList.Count == 0)
                {
                    Debug.LogWarning("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; no element fulfilled requirements");
                    return;
                }

                heroesList.RandomSortThreadSafe();

                AdvList al = advData.GetListByName(lm.listA, localLists);
                if (al == null)
                    al = advData.GetListByName(lm.listA, publicLists);

                if (al == null)
                {
                    Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", doesn't exist");
                }

                foreach (var v in al.list)
                {
                    if (v is Group)
                    {
                        MOM.Unit u = MOM.Unit.CreateFrom(heroesList[0]);
                        if (heroesList[0].gainsXP && lm.powerScalable)
                        {
                            int exp = BudgetScaling() / OneExpCost();
                            u.xp += exp;
                        }
                        advData.heroes.Add(new KeyValuePair<MOM.Unit, IGroup>(u, v as Group));
                    }
                    else if (v is TownLocation)
                    {
                        MOM.Unit u = MOM.Unit.CreateFrom(heroesList[0]);
                        if (heroesList[0].gainsXP && lm.powerScalable)
                        {
                            int exp = BudgetScaling() / OneExpCost();
                            u.xp += exp;
                        }
                        advData.heroes.Add(new KeyValuePair<MOM.Unit, IGroup>(u, (v as TownLocation).localGroup.Get()));

                    }
                    else
                    {
                        Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element :" + v.ToString() + ", isn't Group type");
                    }
                }
            }
        }

        [ScriptParameters(typeof(Unit), typeof(FInt))]
        [ScriptParameters(typeof(Unit), null)]
        [ScriptParameters(typeof(Hero), typeof(FInt))]
        [ScriptParameters(typeof(Hero), null)]
        static public void FMO_AddUnit(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicModifier lm = advLogic as LogicModifier;
            List<Unit> potencialUnitList = new List<Unit>();
            List<Unit> readyUnitList = new List<Unit>();
            int amount = 1;
            float minPower = 0.0f;
            float maxPower = 0.0f; //max value for all units 
            float maxPowerPerUnit = 0.0f; //max value for one units 
            var l = PowerEstimate.GetList();
            l = l.FindAll(o => DataBase.GetType<DBDef.Unit>().Contains(o.t0.dbSource.Get() as DBDef.Unit));

            var nodePowerRequirement = String.IsNullOrEmpty(lm.powerValue);


            if (!String.IsNullOrEmpty(lm.numberOfUnits))
            {
                amount = Mathf.RoundToInt(UTIL_GetRandomFromStringParameter(lm.numberOfUnits));
            }

            //Add power to units based on turns
            if (lm.powerScalable)
            {
                var scalablebudget = BudgetScaling();
                maxPower = scalablebudget;
                maxPowerPerUnit = scalablebudget / amount;
            }

            if (!nodePowerRequirement)
            {
                potencialUnitList = new List<Unit>();

                minPower += UTIL_StringParameterProcessor(lm.powerValue).t1;
                maxPower += UTIL_StringParameterProcessor(lm.powerValue).t2;

                foreach (var v in l)
                {
                    if (v.t1 >= minPower && v.t1 <= maxPower)
                    {
                        if (v.t0.dbSource.Get() is Unit )
                        {
                            potencialUnitList.Add(v.t0.dbSource.Get() as Unit);
                        }
                    }
                }
            }
            else
            {
                potencialUnitList = new List<Unit>(DataBase.GetType<Unit>());
            }

            if (lm.requirementGroups != null)
            {
                foreach (var v in lm.requirementGroups)
                {
                    for (int i = 0; i < potencialUnitList.Count; i++)
                    {
                        bool fulfilled = false;
                        if (v.options != null)
                        {
                            foreach (var o in v.options)
                            {
                                var dbc = DataBase.Get(o.typeData, true);
                                if (dbc is Unit)
                                {
                                    if (potencialUnitList[i] == dbc)
                                    {
                                        fulfilled = true;
                                        break;
                                    }
                                }
                                if (dbc is Race)
                                {
                                    if (potencialUnitList[i].race == dbc)
                                    {
                                        fulfilled = true;
                                        break;
                                    }
                                }
                                else if (dbc is Tag)
                                {
                                    var cTag = Array.Find(potencialUnitList[i].tags, ct => ct.tag == dbc);
                                    if (cTag != null)
                                    {
                                        switch (o.sign)
                                        {
                                            case ">=":
                                                fulfilled = cTag.amount >= o.value;
                                                break;
                                            case ">":
                                                fulfilled = cTag.amount > o.value;
                                                break;
                                            case "=":
                                                fulfilled = cTag.amount == o.value;
                                                break;
                                            case "<":
                                                fulfilled = cTag.amount < o.value;
                                                break;
                                            case "<=":
                                                fulfilled = cTag.amount <= o.value;
                                                break;
                                            default:
                                                Debug.Log("Sign is: " + o.sign);
                                                break;
                                        }
                                    }
                                }
                            }
                        }
                        if (!fulfilled)
                        {
                            potencialUnitList.RemoveAt(i);
                            i--;
                        }
                    }
                }
            }

            if (potencialUnitList.Count == 0)
            {
                Debug.LogWarning("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; no element fulfilled requirements");
                return;
            }

            AdvList al = advData.GetListByName(lm.listA, localLists);
            if (al == null)
                al = advData.GetListByName(lm.listA, publicLists);

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", doesn't exist");
            }


            //check how much units can by buy from budget
            for (int i = 0; i < amount; i++)
            {
                var random = new MHRandom().GetInt(0, potencialUnitList.Count);

                var powerValue = l.Find(o => o.t0.dbSource == potencialUnitList[random]).t1;
                if (maxPower >= powerValue || nodePowerRequirement)
                {
                    readyUnitList.Add(potencialUnitList[random]);
                    if (!nodePowerRequirement)
                        maxPower -= powerValue;
                }
            }

            foreach (var v in al.list)
            {
                int readyXpGainers = readyUnitList.FindAll(o => o.gainsXP).Count;

                if (readyUnitList.Count == 0)
                    Debug.LogWarning("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List readyUnitList is empty");

                if (v is Group)
                {
                    int random = new MHRandom().GetInt(1, readyXpGainers + 1);
                    int exp = (int)maxPower / OneExpCost();

                    foreach (var unit in readyUnitList)
                    {
                        MOM.Unit u = MOM.Unit.CreateFrom(unit);
                        GameManager.GetWizard(advData.mainPlayerWizard).ModifyUnitSkillsByTraits(u);
                        if (unit.gainsXP)
                        {
                            u.xp += exp / random;
                        }
                        (v as Group).AddUnit(u);
                        u.UpdateMP();

                    }
                }
                else if (v is TownLocation)
                {
                    int random = new MHRandom().GetInt(1, readyXpGainers + 1);
                    int exp = (int)maxPower / OneExpCost();

                    foreach (var unit in readyUnitList)
                    {
                        MOM.Unit u = MOM.Unit.CreateFrom(unit);
                        GameManager.GetWizard(advData.mainPlayerWizard).ModifyUnitSkillsByTraits(u);
                        if (unit.gainsXP)
                        {
                            u.xp += exp / random;
                        }
                        (v as TownLocation).AddUnit(u);
                        u.UpdateMP();

                    }
                }
                else
                {
                    Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element :" + v.ToString() + ", isn't Group type");
                }
            }
        }

        static public void FMO_AddEquipment(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicModifier lm = advLogic as LogicModifier;
            List<DBDef.Artefact> artefactsList = new List<DBDef.Artefact>(DataBase.GetType<DBDef.Artefact>());

            int min = 0;
            int max = 0;

            if (!String.IsNullOrEmpty(lm.powerValue))
            {
                min = Mathf.RoundToInt(UTIL_StringParameterProcessor(lm.powerValue).t1);
                max = Mathf.RoundToInt(UTIL_StringParameterProcessor(lm.powerValue).t2);

                if (lm.powerScalable)
                {
                    max += BudgetScaling();
                }

                for (var i = artefactsList.Count - 1; i >= 0; i--)
                {
                    var cost = DataBase.GetType<ArtefactPrefab>().Find(o => o.eType == artefactsList[i].eType).cost; ;
                    foreach (var p in artefactsList[i].power)
                    {
                        cost += p.cost;
                    }
                    if (cost < min || cost > max)
                    {
                        artefactsList.RemoveAt(i);
                    }
                }
            }

            if (lm.requirementGroups != null && lm.requirementGroups.Count > 0)
            {
                foreach (var v in lm.requirementGroups)
                {
                    for (int i = 0; i < artefactsList.Count; i++)
                    {
                        bool fulfilled = false;
                        if (v.options != null)
                        {
                            foreach (var o in v.options)
                            {
                                EEquipmentType equipType;
                                Enum.TryParse<EEquipmentType>(o.typeData, out equipType);
                                if (equipType != EEquipmentType.None)
                                {
                                    if (artefactsList[i].eType == equipType)
                                    {
                                        fulfilled = true;
                                        break;
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }

                                var dbc = DataBase.Get(o.typeData, true);
                                if (dbc is ArtefactPower)
                                {
                                    foreach (var p in artefactsList[i].power)
                                    {
                                        if (p == dbc)
                                        {
                                            fulfilled = true;
                                            break;
                                        }
                                    }
                                }
                                if (dbc is DBDef.Artefact)
                                {
                                    if (artefactsList[i] == dbc)
                                    {
                                        fulfilled = true;
                                        break;
                                    }
                                }
                            }
                        }
                        if (!fulfilled)
                        {
                            artefactsList.RemoveAt(i);
                            i--;
                        }
                    }
                }

                MOM.Artefact art = null;
                var wiz = GameManager.GetWizard(advData.mainPlayerWizard);

                if (artefactsList.Count > 0)
                {
                    artefactsList.RandomSortThreadSafe();
                    art = MOM.Artefact.Craft(artefactsList[0]);
                }
                else
                {
                    art = MOM.Artefact.FactoryByRequirements(lm.requirementGroups, min, max);
                }

                if (art != null)
                {
                    wiz.artefacts.Add(art);
                }
                else
                {
                    Debug.LogWarning("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; no element fulfilled requirements");
                }
            }
            else
            {
                //add random predefined artefact based only on budget
                MOM.Artefact art = null;
                var wiz = GameManager.GetWizard(advData.mainPlayerWizard);

                if (artefactsList.Count > 0)
                {
                    artefactsList.RandomSort();
                    art = MOM.Artefact.Craft(artefactsList[0]);
                }

                if (art != null)
                {
                    wiz.artefacts.Add(art);
                }
                else
                {
                    Debug.LogWarning("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; no element fulfilled power requirements");
                }
            }
        }

        [ScriptType(ScriptType.Type.EditorModifier)]
        static public void FMO_TakeHero(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicModifier lm = advLogic as LogicModifier;
            AdvList al = advData.GetListByName(lm.listA, localLists);
            if (al == null)
                al = advData.GetListByName(lm.listA, publicLists);

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", doesn't exist");
            }

            if (al.listType == LogicEntry.LEntry.LEEntryHero)
            {
                for (int i = al.list.Count - 1; i >= 0; i--)
                {
                    MOM.Unit u = al.list[i] as MOM.Unit;
                    if (u == null || !(u.dbSource.Get() is Hero)) continue;

                    u.Destroy();
                }
            }
            else if (al.listType == LogicEntry.LEntry.LEEntryGroup)
            {
                foreach (var g in al.list)
                {
                    Group gr = g as Group;
                    if (gr == null) continue;
                    var units = gr.GetUnits();

                    for (int i = units.Count - 1; i >= 0; i--)
                    {
                        MOM.Unit u = units[i];
                        if (u == null || !(u.dbSource.Get() is Hero)) continue;

                        u.Destroy();
                    }
                }
            }
            else if (al.listType == LogicEntry.LEntry.LEEntryCity)
            {
                foreach (var o in al.list)
                {
                    TownLocation tl = o as TownLocation;
                    if (tl == null || tl.localGroup == null) continue;

                    var units = tl.localGroup.Get().GetUnits();

                    for (int i = units.Count - 1; i >= 0; i--)
                    {
                        MOM.Unit u = units[i].Get();
                        if (u == null || !(u.dbSource.Get() is Hero)) continue;

                        u.Destroy();
                    }
                }
            }
        }

        [ScriptType(ScriptType.Type.EditorModifier)]
        [ScriptParameters(typeof(LogicUtils.UnitType), typeof(Multitype<float, float, float>))]
        [ScriptParameters(typeof(LogicUtils.UnitType), null)]
        static public void FMO_TakeUnit(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicModifier lm = advLogic as LogicModifier;
            AdvList al = advData.GetListByName(lm.listA, localLists);
            if (al == null)
                al = advData.GetListByName(lm.listA, publicLists);

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", doesn't exist");
            }

            int parameterValue = 1;

            if (!String.IsNullOrEmpty(lm.scriptStringParameter))
            {
                parameterValue = UTIL_GetStringParameterValue(lm.scriptStringParameter);
            }

            Func<object, bool> process = delegate (object o)
            {
                MOM.Unit u = o as MOM.Unit;
                if (u == null) return false;

                if (lm.scriptTypeParameter == LogicUtils.UnitType.Normal.ToString())
                {
                    if (u.attributes.Contains(TAG.FANTASTIC_CLASS)) return false;
                }
                if (lm.scriptTypeParameter == LogicUtils.UnitType.Fantastic.ToString())
                {
                    if (u.attributes.Contains(TAG.NORMAL_CLASS)) return false;
                }

                u.Destroy();
                return true;
            };


            if (al.listType == LogicEntry.LEntry.LEEntryUnit || al.listType == LogicEntry.LEntry.LEEntryHero)
            {
                int count = 0;
                for (int i = al.list.Count - 1; i >= 0; i--)
                {
                    if (parameterValue > 0 && count >= parameterValue)
                    {
                        break;
                    }

                    MOM.Unit u = al.list[i] as MOM.Unit;
                    if (u == null)
                    {
                        u = (al.list[i] as Reference<MOM.Unit>).Get();
                    }

                    if (process(u))
                    {
                        count++;
                    }
                }
            }
            else if (al.listType == LogicEntry.LEntry.LEEntryGroup)
            {
                foreach (var g in al.list)
                {
                    Group gr = g as Group;
                    if (gr == null) continue;
                    int count = 0;

                    var units = gr.GetUnits();
                    for (int i = units.Count - 1; i >= 0; i--)
                    {
                        if (parameterValue > 0 && count >= parameterValue)
                        {
                            break;
                        }
                        if (process(units[i].Get()))
                        {
                            count++;
                        }
                    }
                }
            }
            else if (al.listType == LogicEntry.LEntry.LEEntryCity)
            {
                foreach (var t in al.list)
                {
                    TownLocation tl = t as TownLocation;
                    if (tl == null || tl.localGroup == null) continue;
                    int count = 0;

                    var units = tl.localGroup.Get().GetUnits();
                    for (int i = units.Count - 1; i >= 0; i--)
                    {
                        if (parameterValue > 0 && count >= parameterValue)
                        {
                            break;
                        }
                        if (process(units[i].Get()))
                        {
                            count++;
                        }
                    }
                }
            }
        }

        [ScriptType(ScriptType.Type.EditorModifier)]
        static public void FMO_TakeEquipment(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicModifier lm = advLogic as LogicModifier;
            AdvList al = advData.GetListByName(lm.listA, localLists);
            if (al == null)
                al = advData.GetListByName(lm.listA, publicLists);

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", doesn't exist");
            }

            int parameterValue = 1;
            if (!String.IsNullOrEmpty(lm.scriptStringParameter))
            {
                parameterValue = UTIL_GetStringParameterValue(lm.scriptStringParameter);
            }

            if (al.listType == LogicEntry.LEntry.LEEntryEquipment)
            {
                int count = 0;
                al.list.RandomSortThreadSafe();
                for (int i = al.list.Count - 1; i >= 0; i--)
                {
                    if (parameterValue > 0 && count >= parameterValue)
                    {
                        break;
                    }

                    if (al.list[i] is MOM.Artefact)
                    {
                        MOM.Artefact a = al.list[i] as MOM.Artefact;
                        var artefacts = GameManager.GetWizard(advData.mainPlayerWizard).artefacts;
                        if (artefacts.Contains(a))
                        {
                            count++;
                            artefacts.Remove(a);
                        }
                        else
                        {
                            var groups = GameManager.GetGroupsOfWizard(advData.mainPlayerWizard);
                            foreach (var g in groups)
                            {
                                foreach (var u in g.GetUnits())
                                {
                                    if (u.Get().dbSource.Get() is Hero)
                                    {
                                        var hero = u.Get();
                                        var slot = hero.artefactManager.equipmentSlots.Find(o => o.item == a);
                                        if (slot != null)
                                        {
                                            count++;
                                            slot.item = null;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        [ScriptType(ScriptType.Type.EditorModifier)]
        [ScriptParameters(typeof(Building), null)]
        static public void FMO_AddBuilding(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicModifier lm = advLogic as LogicModifier;
            DBClass t = DataBase.Get(lm.scriptTypeParameter, false);
            AdvList al = advData.GetListByName(lm.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lm.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", doesn't exist");
            }

            foreach (var v in al.list)
            {
                if (v is TownLocation)
                {
                    Building b = t as Building;
                    TownLocation town = v as TownLocation;
                    if (!town.buildings.Contains(b) && town.PossibleBuildings(true).Contains(b))
                    {
                        town.AddBuilding(b);
                    }
                    else
                    {
                        Debug.LogWarning("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element:" + v.ToString() + ", can't add building");
                    }
                }
                else
                {
                    Debug.LogWarning("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element:" + v.ToString() + ", isn't TownLocation type");
                }
            }
        }

        [ScriptType(ScriptType.Type.EditorModifier)]
        [ScriptParameters(typeof(Building), null)]
        static public void FMO_TakeBuilding(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicModifier lm = advLogic as LogicModifier;
            DBClass t = DataBase.Get(lm.scriptTypeParameter, false);
            AdvList al = advData.GetListByName(lm.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lm.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", doesn't exist");
            }

            foreach (var v in al.list)
            {
                if (v is TownLocation)
                {
                    Building b = t as Building;
                    TownLocation town = v as TownLocation;
                    if (town.buildings.Contains(b))
                    {
                        town.RemoveBuilding(b, false);
                    }
                    else
                    {
                        Debug.LogWarning("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element:" + v.ToString() + ", can't remove building");
                    }
                }
                else
                {
                    Debug.LogWarning("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element:" + v.ToString() + ", isn't TownLocation type");
                }
            }
        }

        [ScriptType(ScriptType.Type.EditorModifier)]
        [ScriptParameters(null, null)]
        static public void FMO_TakeAnyBuilding(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicModifier lm = advLogic as LogicModifier;
            AdvList al = advData.GetListByName(lm.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lm.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", doesn't exist");
            }

            int parameterValue = 1;

            if (!String.IsNullOrEmpty(lm.scriptStringParameter))
            {
                parameterValue = UTIL_GetStringParameterValue(lm.scriptStringParameter);
            }

            foreach (var v in al.list)
            {
                if (v is TownLocation)
                {
                    TownLocation town = v as TownLocation;
                    List<DBReference<Building>> buildings = new List<DBReference<Building>>(town.buildings);
                    buildings.RandomSortThreadSafe();
                    int index = 0;

                    foreach (var b in buildings)
                    {
                        if (index < parameterValue)
                        {
                            if (!town.IsRegularBuilding(b)) continue; //skip enchantment buildings

                            if (town.RemoveBuilding(b))
                            {
                                index++;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element:" + v.ToString() + ", isn't TownLocation type");
                }
            }
        }

        [ScriptType(ScriptType.Type.EditorModifier)]
        [ScriptParameters(typeof(Resource), typeof(FInt))]
        static public void FMO_TakeResource(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicModifier lm = advLogic as LogicModifier;
            DBClass t = DataBase.Get(lm.scriptTypeParameter, false);
            AdvList al = advData.GetListByName(lm.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lm.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", doesn't exist");
            }

            foreach (var v in al.list)
            {
                if (v is TownLocation)
                {
                    TownLocation town = v as TownLocation;
                    var p = town.GetPlane();
                    var area = town.GetSurroundingArea(town.GetTownRange());
                    Hex h;
                    foreach (var a in area)
                    {
                        h = p.GetHexAt(a);
                        if (h.Resource != null && h.Resource == t)
                        {
                            h.Resource = null;
                            GameObject.Destroy(h.resourceInstance);
                        }
                    }
                }
                else
                {
                    Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element:" + v.ToString() + ", isn't TownLocation type");
                }
            }
        }

        [ScriptType(ScriptType.Type.EditorModifier)]
        [ScriptParameters(typeof(EMinedResources), typeof(FInt))]
        static public void FMO_TakeMinedResource(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicModifier lm = advLogic as LogicModifier;
            DBClass t = DataBase.Get(lm.scriptTypeParameter, false);
            EMinedResources res = (EMinedResources)Enum.Parse(typeof(EMinedResources), lm.scriptTypeParameter);
            AdvList al = advData.GetListByName(lm.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lm.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", doesn't exist");
            }

            foreach (var v in al.list)
            {
                if (v is TownLocation)
                {
                    TownLocation town = v as TownLocation;
                    var p = town.GetPlane();
                    var area = town.GetSurroundingArea(town.GetTownRange());
                    Hex h;
                    foreach (var a in area)
                    {
                        h = p.GetHexAt(a);
                        if (h != null && h.Resource != null && UTIL_IsResourceValid(res, h.Resource))
                        {
                            h.Resource = null;
                            GameObject.Destroy(h.resourceInstance);
                        }
                    }
                }
                else
                {
                    Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element:" + v.ToString() + ", isn't TownLocation type");
                }
            }
        }
        [ScriptType(ScriptType.Type.EditorModifier)]
        [ScriptParameters(null, typeof(FInt))]
        static public void FMO_TakeAnyResource(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicModifier lm = advLogic as LogicModifier;
            AdvList al = advData.GetListByName(lm.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lm.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", doesn't exist");
            }

            int parameterValue = 1;

            if (!String.IsNullOrEmpty(lm.scriptStringParameter))
            {
                parameterValue = UTIL_GetStringParameterValue(lm.scriptStringParameter);
            }

            foreach (var v in al.list)
            {
                if (v is TownLocation)
                {
                    TownLocation town = v as TownLocation;
                    var p = town.GetPlane();
                    var area = town.GetSurroundingArea(town.GetTownRange());
                    Hex h;
                    List<Hex> hexToChange = new List<Hex>();
                    foreach (var a in area)
                    {
                        if (parameterValue > 0 && parameterValue > hexToChange.Count)
                        {
                            h = p.GetHexAt(a);
                            if (h != null && h.Resource != null)
                            {
                                h.Resource = null;
                                hexToChange.Add(h);
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (hexToChange.Count > 0)
                    {
                        foreach (var hex in hexToChange)
                        {
                            GameObject.Destroy(hex.resourceInstance);
                        }
                    }
                }
                else
                {
                    Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element:" + v.ToString() + ", isn't TownLocation type");
                }
            }
        }

        [ScriptType(ScriptType.Type.EditorModifier)]
        [ScriptParameters(null, typeof(FInt))]
        static public void FMO_TakeAnyMinedResource(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicModifier lm = advLogic as LogicModifier;
            AdvList al = advData.GetListByName(lm.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lm.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", doesn't exist");
            }

            int parameterValue = 1;

            if (!String.IsNullOrEmpty(lm.scriptStringParameter))
            {
                parameterValue = UTIL_GetStringParameterValue(lm.scriptStringParameter);
            }

            foreach (var v in al.list)
            {
                if (v is TownLocation)
                {
                    TownLocation town = v as TownLocation;
                    var p = town.GetPlane();
                    var area = town.GetSurroundingArea(town.GetTownRange());
                    Hex h;
                    List<Hex> hexToChange = new List<Hex>();
                    foreach (var a in area)
                    {
                        if (parameterValue > 0 && parameterValue > hexToChange.Count)
                        {
                            h = p.GetHexAt(a);
                            if (h.Resource != null && UTIL_IsMinedResource(h.Resource))
                            {
                                h.Resource = null;
                                hexToChange.Add(h);
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (hexToChange.Count > 0)
                    {
                        foreach (var hex in hexToChange)
                        {
                            GameObject.Destroy(hex.resourceInstance);
                        }
                    }
                }
                else
                {
                    Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element:" + v.ToString() + ", isn't TownLocation type");
                }
            }
        }

        [ScriptType(ScriptType.Type.EditorModifier)]
        static public void FMO_ConvertCity(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicModifier lm = advLogic as LogicModifier;
            AdvList al = advData.GetListByName(lm.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lm.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", doesn't exist");
            }

            foreach (var v in al.list)
            {
                if (v is TownLocation)
                {
                    TownLocation town = v as TownLocation;

                    town.SetOwnerID(advData.mainPlayerWizard);
                    town.MakeDiscovered();
                    town.UpdateMarkers();
                    if (advData.mainPlayerWizard == GameManager.GetHumanWizard().ID)
                    {
                        FOW.Get().UpdateFogForPlane(town.GetPlane());
                    }
                }
                else
                {
                    Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element:" + v.ToString() + ", isn't TownLocation type");
                }
            }
        }

        /*[ScriptType(ScriptType.Type.EditorModifier)]
        static public void FMO_SpawnCity(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {

        }*/

        [ScriptType(ScriptType.Type.EditorModifier)]
        static public void FMO_LoseCity(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicModifier lm = advLogic as LogicModifier;
            AdvList al = advData.GetListByName(lm.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lm.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", doesn't exist");
            }

            foreach (var v in al.list)
            {
                if (v is TownLocation)
                {
                    TownLocation town = v as TownLocation;
                    town.SetOwnerID(0);
                }
                else
                {
                    Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element:" + v.ToString() + ", isn't TownLocation type");
                }
            }
        }

        [ScriptType(ScriptType.Type.EditorModifier)]
        static public void FMO_DestroyCity(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicModifier lm = advLogic as LogicModifier;
            AdvList al = advData.GetListByName(lm.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lm.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", doesn't exist");
            }

            foreach (var v in al.list)
            {
                if (v is TownLocation)
                {
                    TownLocation town = v as TownLocation;
                    town.Destroy();
                }
                else
                {
                    Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element:" + v.ToString() + ", isn't TownLocation type");
                }
            }
        }

        [ScriptType(ScriptType.Type.EditorModifier)]
        [ScriptParameters(typeof(Resource), null)]
        static public void FMO_AddResourceInLocationPlace(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicModifier lm = advLogic as LogicModifier;
            DBClass t = DataBase.Get(lm.scriptTypeParameter, false);
            MOM.Location loc = advData.advSource as MOM.Location;

            if (loc == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; adventure source is not a Location");
            }

            if (t != null)
            {
                var res = t as Resource;
                var p = loc.GetPlane();
                Hex h;

                h = p.GetHexAt(loc.Position);
                //if (h == null || h.additionalDecorInstance != null) continue;

                if (h.Resource == null)
                {
                    if (res != null)
                    {
                        h.Resource = res;
                        var g = AssetManager.Get<GameObject>(res.GetModel3dName());

                        if (g == null)
                        {
                            Debug.Log("Spawn resource :" + res.dbName + " model " + res.descriptionInfo.graphic + " at " + h.Position + " graphic " + g + " failed");
                            return;
                        }

                        var pos = HexCoordinates.HexToWorld3D(h.Position);
                        Chunk c = p.GetChunkFor(h.Position);
                        MHRandom random = new MHRandom();
                        var instance = GameObjectUtils.Instantiate(g, c.go.transform);
                        instance.transform.localRotation = Quaternion.Euler(Vector3.up * random.GetFloat(0, 360));
                        instance.transform.position = pos;
                        h.resourceInstance = instance;

                        List<GameObject> toDestroy = null;

                        foreach (Transform ins in instance.transform)
                        {
                            var itemPos = ins.position;
                            itemPos.y = p.GetHeightAt(itemPos, true);

                            var gOffset = ins.gameObject.GetComponent<GroundOffset>();

                            if (gOffset != null)
                            {
                                if (itemPos.y < 0)
                                {
                                    if (toDestroy == null) toDestroy = new List<GameObject>();
                                    toDestroy.Add(ins.gameObject);
                                    continue;
                                }
                                itemPos.y += gOffset.heightOffset;
                            }
                            ins.position = itemPos;
                        }
                        if (toDestroy != null)
                        {
                            foreach (var td in toDestroy)
                            {
                                GameObject.Destroy(td);
                            }
                        }

                        if (PlayerWizard.HumanID() == advData.mainPlayerWizard)
                        {
                            var w = GameManager.GetWizard(advData.mainPlayerWizard);
                            var si = new SummaryInfo()
                            {
                                summaryType = SummaryInfo.SummaryType.eResourceDiscovered,
                                isArcanus = p.arcanusType,
                                position = h.Position,
                                graphic = res.GetDescriptionInfo().graphic,
                            };
                            w.AddNotification(si);

                            var fow = FOW.Get();
                            if (!fow.IsVisible(h.Position, p))
                            {
                                fow.MarkVisible(h.Position, p.arcanusType);
                                FOW.Get().UpdateFogForPlane(p);
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; hex already occupied by other resource");
                }
            }
        }
        [ScriptType(ScriptType.Type.EditorModifier)]
        [ScriptParameters(typeof(Resource), null)]
        static public void FMO_AddResourceWithinRangeOf(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicModifier lm = advLogic as LogicModifier;
            DBClass t = DataBase.Get(lm.scriptTypeParameter, false);
            AdvList al = advData.GetListByName(lm.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lm.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", doesn't exist");
            }

            int parameterValue = 1;

            if (!String.IsNullOrEmpty(lm.scriptStringParameter))
            {
                parameterValue = UTIL_GetStringParameterValue(lm.scriptStringParameter);
            }
            if (t != null)
            {
                var res = t as Resource;
                foreach (var v in al.list)
                {
                    if (v is TownLocation)
                    {
                        TownLocation town = v as TownLocation;
                        var p = town.GetPlane();
                        var area = town.GetSurroundingArea(parameterValue);
                        area.RandomSortThreadSafe();
                        Hex h;

                        foreach (var a in area)
                        {
                            MOM.Location hexLocation = GameManager.GetLocationsOfThePlane(p).Find(o => o.Position == a);
                            if (hexLocation != null) continue;

                            h = p.GetHexAt(a);
                            if (h == null || h.additionalDecorInstance != null) continue;

                            if (h.Resource == null && h.IsLand())
                            {
                                if (res != null)
                                {
                                    h.Resource = res;
                                    var g = AssetManager.Get<GameObject>(res.GetModel3dName());

                                    if (g == null)
                                    {
                                        Debug.Log("Spawn resource :" + res.dbName + " model " + res.descriptionInfo.graphic + " at " + h.Position + " graphic " + g + " failed");
                                        return;
                                    }

                                    var pos = HexCoordinates.HexToWorld3D(h.Position);
                                    Chunk c = p.GetChunkFor(h.Position);
                                    MHRandom random = new MHRandom();
                                    var instance = GameObjectUtils.Instantiate(g, c.go.transform);
                                    instance.transform.localRotation = Quaternion.Euler(Vector3.up * random.GetFloat(0, 360));
                                    instance.transform.position = pos;
                                    h.resourceInstance = instance;

                                    List<GameObject> toDestroy = null;

                                    foreach (Transform ins in instance.transform)
                                    {
                                        var itemPos = ins.position;
                                        itemPos.y = p.GetHeightAt(itemPos, true);

                                        var gOffset = ins.gameObject.GetComponent<GroundOffset>();

                                        if (gOffset != null)
                                        {
                                            if (itemPos.y < 0)
                                            {
                                                if (toDestroy == null) toDestroy = new List<GameObject>();
                                                toDestroy.Add(ins.gameObject);
                                                continue;
                                            }
                                            itemPos.y += gOffset.heightOffset;
                                        }
                                        ins.position = itemPos;
                                    }
                                    if (toDestroy != null)
                                    {
                                        foreach (var td in toDestroy)
                                        {
                                            GameObject.Destroy(td);
                                        }
                                    }

                                    if (PlayerWizard.HumanID() == advData.mainPlayerWizard)
                                    {
                                        var w = GameManager.GetWizard(advData.mainPlayerWizard);
                                        var si = new SummaryInfo()
                                        {
                                            summaryType = SummaryInfo.SummaryType.eResourceDiscovered,
                                            isArcanus = p.arcanusType,
                                            position = h.Position,
                                            graphic = res.GetDescriptionInfo().graphic,
                                        };
                                        w.AddNotification(si);

                                        var fow = FOW.Get();
                                        if (!fow.IsVisible(h.Position, p))
                                        {
                                            fow.MarkVisible(h.Position, p.arcanusType);
                                            FOW.Get().UpdateFogForPlane(p);
                                        }
                                    }

                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element:" + v.ToString() + ", isn't TownLocation type");
                    }
                }
            }
        }
        [ScriptType(ScriptType.Type.EditorModifier)]
        [ScriptParameters(typeof(EMinedResources), null)]
        static public void FMO_AddMinedResourceInRangeOf(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicModifier lm = advLogic as LogicModifier;
            EMinedResources minedRes = (EMinedResources)Enum.Parse(typeof(EMinedResources), lm.scriptTypeParameter);
            AdvList al = advData.GetListByName(lm.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lm.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", doesn't exist");
            }

            int parameterValue = 1;

            if (!String.IsNullOrEmpty(lm.scriptStringParameter))
            {
                parameterValue = UTIL_GetStringParameterValue(lm.scriptStringParameter);
            }
            if (minedRes != null)
            {
                Resource res = null;
                switch (minedRes)
                {
                    case EMinedResources.AdamantineOre:
                        res = (DBDef.Resource)RESOURCE.ADAMANTINE_ORE;
                        break;
                    case EMinedResources.Coal:
                        res = (DBDef.Resource)RESOURCE.COAL;
                        break;
                    case EMinedResources.CrysxCrystals:
                        res = (DBDef.Resource)RESOURCE.CRYSX_CRYSTALS;
                        break;
                    case EMinedResources.Gems:
                        res = (DBDef.Resource)RESOURCE.GEMS;
                        break;
                    case EMinedResources.GoldOre:
                        res = (DBDef.Resource)RESOURCE.GOLD_ORE;
                        break;
                    case EMinedResources.IronOre:
                        res = (DBDef.Resource)RESOURCE.IRON_ORE;
                        break;
                    case EMinedResources.MithrilOre:
                        res = (DBDef.Resource)RESOURCE.MITHRIL_ORE;
                        break;
                    case EMinedResources.QuorkCrystals:
                        res = (DBDef.Resource)RESOURCE.QUORK_CRYSTALS;
                        break;
                    case EMinedResources.SilverOre:
                        res = (DBDef.Resource)RESOURCE.SILVER_ORE;
                        break;
                    case EMinedResources.Orichalcum:
                        res = (DBDef.Resource)RESOURCE.ORICHALCUM;
                        break;
                    case EMinedResources.AnyMinedResource:
                        List<RESOURCE> l = new List<RESOURCE>() { RESOURCE.ADAMANTINE_ORE, RESOURCE.COAL, RESOURCE.CRYSX_CRYSTALS, RESOURCE.GEMS, RESOURCE.GOLD_ORE, RESOURCE.IRON_ORE, RESOURCE.MITHRIL_ORE, RESOURCE.QUORK_CRYSTALS, RESOURCE.SILVER_ORE };
                        l.RandomSortThreadSafe();
                        res = (DBDef.Resource)l[0];
                        break;
                }
                foreach (var v in al.list)
                {
                    if (v is TownLocation)
                    {
                        TownLocation town = v as TownLocation;
                        var p = town.GetPlane();
                        var area = town.GetSurroundingArea(parameterValue);
                        area.RandomSortThreadSafe();
                        Hex h;

                        foreach (var a in area)
                        {
                            MOM.Location hexLocation = GameManager.GetLocationsOfThePlane(p).Find(o => o.Position == a);
                            if (hexLocation != null) continue;

                            h = p.GetHexAt(a);
                            if (h == null || h.additionalDecorInstance != null) continue;

                            if (h.Resource == null && h.IsLand())
                            {
                                if (res != null)
                                {
                                    h.Resource = res;
                                    var g = AssetManager.Get<GameObject>(res.GetModel3dName());

                                    if (g == null)
                                    {
                                        Debug.Log("Spawn resource :" + res.dbName + " model " + res.descriptionInfo.graphic + " at " + h.Position + " graphic " + g + " failed");
                                        return;
                                    }

                                    var pos = HexCoordinates.HexToWorld3D(h.Position);
                                    Chunk c = p.GetChunkFor(h.Position);
                                    MHRandom random = new MHRandom();
                                    var instance = GameObjectUtils.Instantiate(g, c.go.transform);
                                    instance.transform.localRotation = Quaternion.Euler(Vector3.up * random.GetFloat(0, 360));
                                    instance.transform.position = pos;
                                    h.resourceInstance = instance;

                                    List<GameObject> toDestroy = null;

                                    foreach (Transform ins in instance.transform)
                                    {
                                        var itemPos = ins.position;
                                        itemPos.y = p.GetHeightAt(itemPos, true);

                                        var gOffset = ins.gameObject.GetComponent<GroundOffset>();

                                        if (gOffset != null)
                                        {
                                            if (itemPos.y < 0)
                                            {
                                                if (toDestroy == null) toDestroy = new List<GameObject>();
                                                toDestroy.Add(ins.gameObject);
                                                continue;
                                            }
                                            itemPos.y += gOffset.heightOffset;
                                        }
                                        ins.position = itemPos;
                                    }
                                    if (toDestroy != null)
                                    {
                                        foreach (var td in toDestroy)
                                        {
                                            GameObject.Destroy(td);
                                        }
                                    }

                                    if (PlayerWizard.HumanID() == advData.mainPlayerWizard)
                                    {
                                        var w = GameManager.GetWizard(advData.mainPlayerWizard);
                                        var si = new SummaryInfo()
                                        {
                                            summaryType = SummaryInfo.SummaryType.eResourceDiscovered,
                                            isArcanus = p.arcanusType,
                                            position = h.Position,
                                            graphic = res.GetDescriptionInfo().graphic,
                                        };
                                        w.AddNotification(si);

                                        var fow = FOW.Get();
                                        if (!fow.IsVisible(h.Position, p))
                                        {
                                            fow.MarkVisible(h.Position, p.arcanusType);
                                            FOW.Get().UpdateFogForPlane(p);
                                        }
                                    }

                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element:" + v.ToString() + ", isn't TownLocation type");
                    }
                }
            }
        }

        [ScriptType(ScriptType.Type.EditorModifier)]
        [ScriptParameters(null, typeof(Multitype<FInt, FInt>))]
        static public void FMO_AddUnrestPercentage(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicModifier lm = advLogic as LogicModifier;
            AdvList al = advData.GetListByName(lm.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lm.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", doesn't exist");
            }

            Enchantment e = (Enchantment)ENCH.ADD_UNREST_PERCENTAGE;
            string parameter = UTIL_GetStringParameterValue(lm.scriptStringParameter).ToString();
            foreach (var v in al.list)
            {
                if (v is TownLocation)
                {
                    TownLocation town = v as TownLocation;
                    town.AddEnchantment(e, null, -1, parameter);
                }
                else
                {
                    Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element:" + v.ToString() + ", isn't TownLocation type");
                }
            }
        }
        [ScriptType(ScriptType.Type.EditorModifier)]
        [ScriptParameters(null, typeof(Multitype<FInt, FInt>))]
        static public void FMO_AddRebels(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicModifier lm = advLogic as LogicModifier;
            AdvList al = advData.GetListByName(lm.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lm.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", doesn't exist");
            }

            Enchantment e = (Enchantment)ENCH.ADD_REBELS;
            string parameter = "1";
            if (!String.IsNullOrEmpty(lm.scriptStringParameter))
            {
                parameter = UTIL_GetStringParameterValue(lm.scriptStringParameter).ToString();
            }

            foreach (var v in al.list)
            {
                if (v is TownLocation)
                {
                    TownLocation town = v as TownLocation;
                    town.AddEnchantment(e, null, -1, parameter);
                }
                else
                {
                    Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element:" + v.ToString() + ", isn't TownLocation type");
                }
            }
        }
        [ScriptType(ScriptType.Type.EditorModifier)]
        [ScriptParameters(null, typeof(Multitype<FInt, FInt>))]
        static public void FMO_TakeRebels(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicModifier lm = advLogic as LogicModifier;
            AdvList al = advData.GetListByName(lm.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lm.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", doesn't exist");
            }

            Enchantment e = (Enchantment)ENCH.TAKE_REBELS;
            string parameter = "1";
            if (!String.IsNullOrEmpty(lm.scriptStringParameter))
            {
                parameter = UTIL_GetStringParameterValue(lm.scriptStringParameter).ToString();
            }
            foreach (var v in al.list)
            {
                if (v is TownLocation)
                {
                    TownLocation town = v as TownLocation;
                    town.AddEnchantment(e, null, -1, parameter);
                }
                else
                {
                    Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element:" + v.ToString() + ", isn't TownLocation type");
                }
            }
        }

        [ScriptType(ScriptType.Type.EditorModifier)]
        [ScriptParameters(null, typeof(Multitype<FInt, FInt, FInt>))]
        static public void FMO_LowerUnrestPercent(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicModifier lm = advLogic as LogicModifier;
            AdvList al = advData.GetListByName(lm.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lm.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", doesn't exist");
            }

            Enchantment e = (Enchantment)ENCH.TAKE_UNREST_PERCENTAGE;
            string parameter = UTIL_GetStringParameterValue(lm.scriptStringParameter).ToString();
            foreach (var v in al.list)
            {
                if (v is TownLocation)
                {
                    TownLocation town = v as TownLocation;
                    town.AddEnchantment(e, null, -1, parameter);
                }
                else
                {
                    Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element:" + v.ToString() + ", isn't TownLocation type");
                }
            }
        }

        [ScriptType(ScriptType.Type.EditorModifier)]
        [ScriptParameters(null, typeof(Multitype<FInt, FInt>))]
        static public void FMO_AddResearchPoints(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicModifier lm = advLogic as LogicModifier;
            AdvList al = advData.GetListByName(lm.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lm.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", doesn't exist");
            }

            int parameterValue = 1;

            if (!String.IsNullOrEmpty(lm.scriptStringParameter))
            {
                parameterValue = UTIL_GetStringParameterValue(lm.scriptStringParameter);
            }

            foreach (var v in al.list)
            {
                if (v is PlayerWizard)
                {
                    PlayerWizard w = v as PlayerWizard;
                    w.magicAndResearch.ChangeResearchPointsAndUpdate(parameterValue, true);
                    if (w == GameManager.GetHumanWizard())
                    {
                        HUD.Get().UpdateResearchButton();
                    }
                }
                else
                {
                    Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element:" + v.ToString() + ", isn't TownLocation type");
                }
            }
        }

        [ScriptType(ScriptType.Type.EditorModifier)]
        [ScriptParameters(null, typeof(Multitype<FInt, FInt>))]
        static public void FMO_TakeResearchPointsPerc(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicModifier lm = advLogic as LogicModifier;
            AdvList al = advData.GetListByName(lm.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lm.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", doesn't exist");
            }

            float parameterValue = 0f;
            if (!String.IsNullOrEmpty(lm.scriptStringParameter))
            {
                parameterValue = UTIL_GetStringParameterValue(lm.scriptStringParameter);
                parameterValue = parameterValue * 0.01f;
            }

            foreach (var v in al.list)
            {
                if (v is PlayerWizard)
                {
                    PlayerWizard w = v as PlayerWizard;
                    int percentageValue = Mathf.RoundToInt(w.magicAndResearch.researchProgress * parameterValue);
                    w.magicAndResearch.ChangeResearchPointsAndUpdate(percentageValue, false);
                    if (w == GameManager.GetHumanWizard())
                    {
                        HUD.Get().UpdateResearchButton();
                    }
                }
                else
                {
                    Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element:" + v.ToString() + ", isn't TownLocation type");
                }
            }
        }

        [ScriptType(ScriptType.Type.EditorModifier)]
        [ScriptParameters(null, typeof(Multitype<FInt, FInt>))]
        static public void FMO_AddCityProduction(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicModifier lm = advLogic as LogicModifier;
            AdvList al = advData.GetListByName(lm.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lm.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", doesn't exist");
            }

            Enchantment e = (Enchantment)ENCH.ADD_PRODUCTION_PER_TURN;
            string parameter = UTIL_GetStringParameterValue(lm.scriptStringParameter).ToString();
            foreach (var v in al.list)
            {
                if (v is TownLocation)
                {
                    TownLocation town = v as TownLocation;
                    town.AddEnchantment(e, null, -1, parameter);
                }
                else
                {
                    Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element:" + v.ToString() + ", isn't TownLocation type");
                }
            }
        }

        [ScriptType(ScriptType.Type.EditorModifier)]
        [ScriptParameters(null, typeof(Multitype<FInt, FInt>))]
        static public void FMO_TakeCityProductionn(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicModifier lm = advLogic as LogicModifier;
            AdvList al = advData.GetListByName(lm.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lm.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", doesn't exist");
            }

            Enchantment e = (Enchantment)ENCH.TAKE_PRODUCTION_PER_TURN;
            string parameter = UTIL_GetStringParameterValue(lm.scriptStringParameter).ToString();
            foreach (var v in al.list)
            {
                if (v is TownLocation)
                {
                    TownLocation town = v as TownLocation;
                    town.AddEnchantment(e, null, -1, parameter);
                }
                else
                {
                    Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element:" + v.ToString() + ", isn't TownLocation type");
                }
            }
        }

        [ScriptType(ScriptType.Type.EditorModifier)]
        [ScriptParameters(typeof(Spell))]
        static public void FMO_AddLearnedSpell(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicModifier lm = advLogic as LogicModifier;
            DBClass t = DataBase.Get(lm.scriptTypeParameter, false);
            AdvList al = advData.GetListByName(lm.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lm.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", doesn't exist");
                return;
            }

            FInt parameterValue = FInt.ONE;
            if (!String.IsNullOrEmpty(lm.scriptStringParameter))
            {
                float value = UTIL_StringParameterProcessor(lm.scriptStringParameter).t1;
                parameterValue = new FInt(value);
            }

            Spell spell = t as Spell;

            foreach (var v in al.list)
            {
                if (v is ISpellCaster)
                {
                    ISpellCaster isc = v as ISpellCaster;
                    isc.AddSpell(spell);
                }
                else
                {
                    Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element:" + v.ToString() + ", isn't ISpellCaster type");
                }
            }
        }

        [ScriptType(ScriptType.Type.EditorModifier)]
        [ScriptParameters(typeof(Trait))]
        static public void FMO_AddTrait(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicModifier lm = advLogic as LogicModifier;
            DBClass t = DataBase.Get(lm.scriptTypeParameter, false);
            AdvList al = advData.GetListByName(lm.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lm.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", doesn't exist");
                return;
            }

            Trait trait = t as Trait;

            foreach (var v in al.list)
            {
                if (v is PlayerWizard)
                {
                    PlayerWizard w = v as PlayerWizard;
                    w.AddTrait(trait);
                }
                else
                {
                    Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element:" + v.ToString() + ", isn't PlayerWizard type");
                }
            }
        }

        [ScriptType(ScriptType.Type.EditorModifier)]
        [ScriptParameters(typeof(Skill))]
        [ScriptParameters(typeof(SkillPack))]
        static public void FMO_AddSkill(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicModifier lm = advLogic as LogicModifier;
            DBClass t = DataBase.Get(lm.scriptTypeParameter, false);
            AdvList al = advData.GetListByName(lm.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lm.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", doesn't exist");
                return;
            }

            Skill skill = t as Skill;
            var skills = (t as SkillPack)?.skills;
            MHRandom random = MHRandom.Get();
            foreach (var v in al.list)
            {
                if (v is ISkillable)
                {
                    ISkillable iSkill = v as ISkillable;
                    if(skill != null)
                    {
                        iSkill.AddSkill(skill);
                    }
                    else if (skills != null && skills.Length > 0)
                    {
                        var targetSkills = iSkill.GetSkills();
                        var s = Array.FindAll(skills, o => !targetSkills.Contains(o) && 
                            !(o.applicationScript?.triggerType == ESkillType.Caster && targetSkills.Find(skill2 => skill2.Get().applicationScript?.triggerType == ESkillType.Caster) != null));
                        
                        if (s != null && s.Length > 0)
                        {
                            int index = random.GetInt(0, s.Length);
                            iSkill.AddSkill(s[index]);
                        }
                    }
                }
                else if (v is Group)
                {
                    Group g = v as Group;
                    foreach (var u in g.GetUnits())
                    {
                        if(skill != null)
                        {
                            u.Get().AddSkill(skill);
                        }
                        else if (skills != null && skills.Length > 0)
                        {
                            var targetSkills = u.Get().GetSkills();
                            var s = Array.FindAll(skills, o => !targetSkills.Contains(o) &&
                                !(o.applicationScript?.triggerType == ESkillType.Caster && targetSkills.Find(skill2 => skill2.Get().applicationScript?.triggerType == ESkillType.Caster) != null));
                            if (s != null && s.Length > 0)
                            {
                                int index = random.GetInt(0, s.Length);
                                u.Get().AddSkill(s[index]);
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element:" + v.ToString() + ", isn't ISkillable type");
                }
            }
        }
        [ScriptType(ScriptType.Type.EditorModifier)]
        static public void FMO_DefeatedWizardReward(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicModifier lm = advLogic as LogicModifier;
            AdvList al = advData.GetListByName(lm.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lm.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogWarning("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", doesn't exist");
                return;
            }

            PlayerWizard wizard = GameManager.GetWizard(advData.mainPlayerWizard);
            if (wizard != null)
            {
                foreach (var v in al.list)
                {
                    if (v is PlayerWizard)
                    {
                        int MANA_CONSOLATION_PRIZE = 500;

                        PlayerWizard w = v as PlayerWizard;
                        int spellbooks = wizard.GetWizardSpellbooksCount();

                        //Add spellbook if possible
                        if (spellbooks < 13)
                        {
                            List<Tag> books = new List<Tag>(w.GetWizardSpellbooks());
                            books.RandomSort();
                            var att = wizard.GetAttributes();
                            foreach (var b in books)
                            {
                                if (b == (Tag)TAG.LIFE_MAGIC_BOOK)
                                {
                                    if (att.Contains(TAG.DEATH_MAGIC_BOOK))
                                    {
                                        continue;
                                    }
                                }
                                else if (b == (Tag)TAG.DEATH_MAGIC_BOOK)
                                {
                                    if (att.Contains(TAG.LIFE_MAGIC_BOOK))
                                    {
                                        continue;
                                    }
                                }
                                wizard.AddBook(b, FInt.ONE);
                                return;
                            }
                        }

                        //Add trait if possible
                        var traits = DataBase.GetType<Trait>();
                        traits = traits.FindAll(o => o.cost == 1 && !wizard.HasTrait(o) && !o.rewardExclusion);

                        if (traits.Count > 0 && wizard.GetTraitsCount() < 6)
                        {
                            for (int i = traits.Count - 1; i >= 0; i--)
                            {
                                if (!string.IsNullOrEmpty(traits[i].prerequisiteScript))
                                {
                                    if (!(bool)ScriptLibrary.Call(traits[i].prerequisiteScript, wizard.GetAttributes(), wizard.GetTraits()))
                                    {
                                        traits.RemoveAt(i);
                                    }
                                }
                            }
                            if (traits.Count > 0)
                            {
                                traits.RandomSort();
                                wizard.AddTrait(traits[0]);
                                return;
                            }
                        }

                        //add a Spell if possible
                        List<Spell> spellList = new List<Spell>(DataBase.GetType<Spell>());
                        List<Spell> spells = new List<Spell>();
                        ERarity rarity = ERarity.VeryRare;

                        MagicAndResearch mar = wizard.GetMagicAndResearch();
                        List<MagicUnlocks> limits = mar.GetUnlockLimits();

                        while (true)
                        {
                            spells = spellList.FindAll(
                                o => o.rarity == rarity
                                 && (o.realm == ERealm.Arcane || o.realm == ERealm.Tech && wizard.traitTechMagic || limits.FindIndex(k => k.realm == o.realm) > -1)
                                 && o.treasureExclude == false
                                 && o.researchExclusion == false);

                            foreach (var s in wizard.GetSpells())
                            {
                                var spell = s.Get();
                                if (spells.Contains(spell))
                                {
                                    spells.Remove(spell);
                                }
                            }

                            if (mar.curentlyResearched != null && spells.Contains(mar.curentlyResearched.Get()))
                            {
                                spells.Remove(mar.curentlyResearched.Get());
                            }

                            if (spells.Count > 0)
                            {
                                var index = UnityEngine.Random.Range(0, spells.Count);
                                wizard.AddSpell(spells[index]);
                                return;
                            }
                            else
                            {
                                if (rarity == ERarity.VeryRare)
                                    rarity = ERarity.Rare;
                                else if (rarity == ERarity.Rare)
                                    rarity = ERarity.Uncommon;
                                else if (rarity == ERarity.Uncommon)
                                    rarity = ERarity.Common;
                                else if (rarity == ERarity.Common)
                                    break;
                            }
                        }
                        wizard.mana += MANA_CONSOLATION_PRIZE;
                    }
                    else
                    {
                        Debug.LogWarning("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element:" + v.ToString() + ", isn't PlayerWizard type");
                    }
                }
            }

        }
        [ScriptType(ScriptType.Type.EditorModifier)]
        static public void FMO_WaterLairReward(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            Group playerGroup = advData.mainPlayerGroup as Group;
            PlayerWizard wizard = GameManager.GetWizard(advData.mainPlayerWizard);
            if (advData.advSource is MOM.Location && wizard != null)
            {
                var castingSkill = 25; // obligatory bonus
                var csPrice = 50;
                var csPool = 10;
                var gold = 0;
                var mana = 0;
                var goldManaPrice = 10;
                var locationBudget = (advData.advSource as MOM.Location).budget;

                while (locationBudget > 50)
                {
                    var chance = UnityEngine.Random.Range(0f, 1f);
                    if (chance <= 0.3f)
                    {
                        //casting skill
                        float castingSkillChance = UnityEngine.Random.Range(0f, 1f);

                        if (castingSkillChance <= 0.2 && locationBudget >= csPrice * 20)
                        {
                            locationBudget -= csPrice * 20;
                            castingSkill += csPool * 20;
                        }
                        else if (castingSkillChance <= 0.4 && locationBudget >= csPrice * 10)
                        {
                            locationBudget -= csPrice * 10;
                            castingSkill += csPool * 10;
                        }
                        else if (castingSkillChance <= 0.6 && locationBudget >= csPrice * 6)
                        {
                            locationBudget -= csPrice * 6;
                            castingSkill += csPool * 6;
                        }
                        else if (castingSkillChance <= 0.8 && locationBudget >= csPrice * 3)
                        {
                            locationBudget -= csPrice * 3;
                            castingSkill += csPool * 3;
                        }
                        else if (castingSkillChance <= 1.0)
                        {
                            locationBudget -= csPrice;
                            castingSkill += csPool;
                        }
                    }
                    else if (chance <= 0.6f)
                    {
                        //gold/mana
                        float goldManaChance = UnityEngine.Random.Range(0f, 1f);
                        var bonus = 0;

                        if (goldManaChance <= 0.2 && locationBudget >= goldManaPrice * 20)
                        {
                            locationBudget -= goldManaPrice * 20;
                            bonus += goldManaPrice * 20;
                        }
                        else if (goldManaChance <= 0.4 && locationBudget >= goldManaPrice * 10)
                        {
                            locationBudget -= goldManaPrice * 10;
                            bonus += goldManaPrice * 10;
                        }
                        else if (goldManaChance <= 0.6 && locationBudget >= goldManaPrice * 6)
                        {
                            locationBudget -= goldManaPrice * 6;
                            bonus += goldManaPrice * 6;
                        }
                        else if (goldManaChance <= 0.8 && locationBudget >= goldManaPrice * 3)
                        {
                            locationBudget -= goldManaPrice * 3;
                            bonus += goldManaPrice * 3;
                        }
                        else
                        {
                            locationBudget -= goldManaPrice;
                            bonus += goldManaPrice;
                        }

                        if (chance <= 0.45)
                        {
                            gold += bonus;
                        }
                        else
                        {
                            mana += bonus;
                        }
                    }
                    else if (chance <= 0.9f)
                    {
                        //unit
                        if (playerGroup == null)
                        {
                            Debug.LogWarning("FMO_WaterLairReward, unit part: playerGroup is null");
                            continue;
                        }

                        DBDef.Group g = (DBDef.Group)GROUP.WATER_LAIR_REWARD;
                        List<DBDef.Unit> units = new List<DBDef.Unit>();
                        
                        foreach (var u in g.Units)
                        {
                            if(BaseUnit.GetUnitStrength(u) / 2 <= locationBudget)
                            {
                                units.Add(u);
                            }
                        }
                        if (units.Count > 0)
                        {
                            units.RandomSort();
                            var unit = MOM.Unit.CreateFrom(units[0]);
                            wizard.ModifyUnitSkillsByTraits(unit);
                            playerGroup.AddUnit(unit);
                            unit.UpdateMP();
                            locationBudget -= BaseUnit.GetUnitStrength(units[0]) / 2;
                        }
                    }
                    else if (chance <= 0.95f)
                    {
                        //hero
                        if (wizard.heroes.Count >= wizard.GetMaxHeroCount()) continue;
                        
                        if (playerGroup == null)
                        {
                            Debug.LogWarning("FMO_WaterLairReward, hero part: playerGroup is null");
                            continue;
                        }

                        DBDef.Group g = (DBDef.Group)GROUP.WATER_LAIR_REWARD;
                        List<DBDef.Hero> units = new List<DBDef.Hero>();

                        foreach (var u in g.heroes)
                        {
                            if (!MOM.Unit.HeroInUseByWizard(u, wizard.GetID()) &&
                                BaseUnit.GetUnitStrength(u) / 2 <= locationBudget)
                            {
                                units.Add(u);
                            }
                        }
                        if (units.Count > 0)
                        {
                            units.RandomSort();
                            var unit = MOM.Unit.CreateFrom(units[0]);
                            wizard.ModifyUnitSkillsByTraits(unit);
                            playerGroup.AddUnit(unit);
                            unit.UpdateMP();
                            locationBudget -= BaseUnit.GetUnitStrength(units[0]) / 2;
                        }
                    }
                    else
                    {
                        //spell
                        int spellCommonPrize = 150;
                        int spellUncommonPrize = 300;
                        int spellRarePrize = 550;
                        int spellVRarePrize = 800;

                        List<Spell> spellList = new List<Spell>(DataBase.GetType<Spell>());
                        List<Spell> spells = new List<Spell>();
                        ERarity rarity = ERarity.None;

                        if (locationBudget >= spellVRarePrize)
                        {
                            rarity = ERarity.VeryRare;
                        }
                        else if (locationBudget >= spellRarePrize)
                        {
                            rarity = ERarity.Rare;
                        }
                        else if (locationBudget >= spellUncommonPrize)
                        {
                            rarity = ERarity.Uncommon;
                        }
                        else if (locationBudget >= spellCommonPrize)
                        {
                            rarity = ERarity.Common;
                        }
                        else
                        {
                            continue;
                        }

                        MagicAndResearch mar = wizard.GetMagicAndResearch();
                        List<MagicUnlocks> limits = mar.GetUnlockLimits();

                        while (true)
                        {
                            spells = spellList.FindAll(
                                o => o.rarity == rarity
                                    && (o.realm == ERealm.Arcane || o.realm == ERealm.Tech && wizard.traitTechMagic || limits.FindIndex(k => k.realm == o.realm) > -1)
                                    && o.treasureExclude == false
                                    && o.researchExclusion == false);

                            foreach (var s in wizard.GetSpells())
                            {
                                var spell = s.Get();
                                if (spells.Contains(spell))
                                {
                                    spells.Remove(spell);
                                }
                            }

                            if (mar.curentlyResearched != null && spells.Contains(mar.curentlyResearched.Get()))
                            {
                                spells.Remove(mar.curentlyResearched.Get());
                            }

                            if (spells.Count > 0)
                            {
                                var index = UnityEngine.Random.Range(0, spells.Count);
                                wizard.AddSpell(spells[index]);
                                return;
                            }
                            else
                            {
                                if (rarity == ERarity.VeryRare)
                                    rarity = ERarity.Rare;
                                else if (rarity == ERarity.Rare)
                                    rarity = ERarity.Uncommon;
                                else if (rarity == ERarity.Uncommon)
                                    rarity = ERarity.Common;
                                else if (rarity == ERarity.Common)
                                    break;
                            }
                        }
                    }
                }

                if(locationBudget > 0)
                {
                    gold += locationBudget;
                }
                wizard.money += gold;
                wizard.mana += mana;
                wizard.castingSkillDevelopment += castingSkill;
            }
        }

        [ScriptType(ScriptType.Type.EditorModifier)]
        static public void FMO_RewardTraitFirst(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicModifier lm = advLogic as LogicModifier;
            AdvList al = advData.GetListByName(lm.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lm.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogWarning("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", doesn't exist");
                return;
            }

            PlayerWizard wizard = GameManager.GetWizard(advData.mainPlayerWizard);
            if (wizard != null)
            {
                foreach (var v in al.list)
                {
                    if (v is PlayerWizard)
                    {
                        int MANA_CONSOLATION_PRIZE = 500;

                        PlayerWizard w = v as PlayerWizard;

                        //Add trait if possible
                        var traits = DataBase.GetType<Trait>();
                        traits = traits.FindAll(o => o.cost == 1 && !wizard.HasTrait(o) && !o.rewardExclusion);

                        if (traits.Count > 0 && wizard.GetTraitsCount() < 6)
                        {
                            for (int i = traits.Count - 1; i >= 0; i--)
                            {
                                if (!string.IsNullOrEmpty(traits[i].prerequisiteScript))
                                {
                                    if (!(bool)ScriptLibrary.Call(traits[i].prerequisiteScript, wizard.GetAttributes(), wizard.GetTraits()))
                                    {
                                        traits.RemoveAt(i);
                                    }
                                }
                            }
                            if (traits.Count > 0)
                            {
                                traits.RandomSort();
                                wizard.AddTrait(traits[0]);
                                return;
                            }
                        }

                        int spellbooks = wizard.GetWizardSpellbooksCount();
                        //Add spellbook if possible
                        if (spellbooks < 13)
                        {
                            List<Tag> books = new List<Tag>(w.GetWizardSpellbooks());
                            books.RandomSort();
                            var att = wizard.GetAttributes();
                            foreach (var b in books)
                            {
                                if (b == (Tag)TAG.LIFE_MAGIC_BOOK)
                                {
                                    if (att.Contains(TAG.DEATH_MAGIC_BOOK))
                                    {
                                        continue;
                                    }
                                }
                                else if (b == (Tag)TAG.DEATH_MAGIC_BOOK)
                                {
                                    if (att.Contains(TAG.LIFE_MAGIC_BOOK))
                                    {
                                        continue;
                                    }
                                }
                                wizard.AddBook(b, FInt.ONE);
                                return;
                            }
                        }

                        //add a Spell if possible
                        List<Spell> spellList = new List<Spell>(DataBase.GetType<Spell>());
                        List<Spell> spells = new List<Spell>();
                        ERarity rarity = ERarity.VeryRare;

                        MagicAndResearch mar = wizard.GetMagicAndResearch();
                        List<MagicUnlocks> limits = mar.GetUnlockLimits();

                        while (true)
                        {
                            spells = spellList.FindAll(
                                o => o.rarity == rarity
                                 && (o.realm == ERealm.Arcane || o.realm == ERealm.Tech && wizard.traitTechMagic || limits.FindIndex(k => k.realm == o.realm) > -1)
                                 && o.treasureExclude == false
                                 && o.researchExclusion == false);

                            foreach (var s in wizard.GetSpells())
                            {
                                var spell = s.Get();
                                if (spells.Contains(spell))
                                {
                                    spells.Remove(spell);
                                }
                            }

                            if (mar.curentlyResearched != null && spells.Contains(mar.curentlyResearched.Get()))
                            {
                                spells.Remove(mar.curentlyResearched.Get());
                            }

                            if (spells.Count > 0)
                            {
                                var index = UnityEngine.Random.Range(0, spells.Count);
                                wizard.AddSpell(spells[index]);
                                return;
                            }
                            else
                            {
                                if (rarity == ERarity.VeryRare)
                                    rarity = ERarity.Rare;
                                else if (rarity == ERarity.Rare)
                                    rarity = ERarity.Uncommon;
                                else if (rarity == ERarity.Uncommon)
                                    rarity = ERarity.Common;
                                else if (rarity == ERarity.Common)
                                    break;
                            }
                        }
                        wizard.mana += MANA_CONSOLATION_PRIZE;
                    }
                    else
                    {
                        Debug.LogWarning("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element:" + v.ToString() + ", isn't PlayerWizard type");
                    }
                }
            }

        }

        [ScriptType(ScriptType.Type.EditorModifier)]
        [ScriptParameters(null, typeof(Multitype<float, float, float>))]
        [ScriptParameters(null, null)]
        static public void FMO_Heal(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicModifier lm = advLogic as LogicModifier;
            AdvList al = advData.GetListByName(lm.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lm.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", doesn't exist");
            }

            float parameterValue = 1f;
            if (!String.IsNullOrEmpty(lm.scriptStringParameter))
            {
                parameterValue = UTIL_StringParameterProcessor(lm.scriptStringParameter).t1;
                parameterValue = parameterValue * 0.01f;
            }

            foreach (var v in al.list)
            {
                if (v is MOM.Unit)
                {
                    MOM.Unit u = v as MOM.Unit;
                    u.Heal(parameterValue);
                }
                else if (v is Group)
                {
                    Group g = v as Group;
                    foreach (var u in g.GetUnits())
                    {
                        u.Get().Heal(parameterValue);
                    }
                }
                else
                {
                    Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element:" + v.ToString() + ", isn't Unit type");
                }
            }
        }

        [ScriptType(ScriptType.Type.EditorModifier)]
        static public void FMO_AddFly(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicModifier lm = advLogic as LogicModifier;
            AdvList al = advData.GetListByName(lm.listA, localLists);
            if (al == null)
                al = advData.GetListByName(lm.listA, publicLists);

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", doesn't exist");
            }

            foreach (var v in al.list)
            {
                if (v is MOM.Unit)
                {
                    MOM.Unit u = v as MOM.Unit;
                    if (!u.GetAttributes().Contains(TAG.CAN_FLY))
                    {
                        u.GetAttributes().AddToBase((Tag)TAG.CAN_FLY, 1);
                        u.group.Get().UpdateMovementFlags();
                    }
                }
                else if (v is Group)
                {
                    Group g = v as Group;
                    foreach (var u in g.GetUnits())
                    {
                        if (!u.Get().GetAttributes().Contains(TAG.CAN_FLY))
                        {
                            u.Get().GetAttributes().AddToBase((Tag)TAG.CAN_FLY, 1);
                            g.UpdateMovementFlags();
                        }
                    }
                }
            }
        }

        [ScriptType(ScriptType.Type.EditorModifier)]
        static public void FMO_AddSwim(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicModifier lm = advLogic as LogicModifier;
            AdvList al = advData.GetListByName(lm.listA, localLists);
            if (al == null)
                al = advData.GetListByName(lm.listA, publicLists);

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", doesn't exist");
            }

            foreach (var v in al.list)
            {
                if (v is MOM.Unit)
                {
                    MOM.Unit u = v as MOM.Unit;
                    if (!u.GetAttributes().Contains(TAG.CAN_SWIM))
                    {
                        u.GetAttributes().AddToBase((Tag)TAG.CAN_SWIM, 1);
                        u.group.Get().UpdateMovementFlags();
                    }
                }
                else if (v is Group)
                {
                    Group g = v as Group;
                    foreach (var u in g.GetUnits())
                    {
                        if (!u.Get().GetAttributes().Contains(TAG.CAN_SWIM))
                        {
                            u.Get().GetAttributes().AddToBase((Tag)TAG.CAN_SWIM, 1);
                            g.UpdateMovementFlags();
                        }
                    }
                }
            }
        }

        [ScriptType(ScriptType.Type.EditorModifier)]
        [ScriptParameters(null, typeof(Multitype<FInt, FInt>))]
        static public void FMO_AddXP(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicModifier lm = advLogic as LogicModifier;
            AdvList al = advData.GetListByName(lm.listA, localLists);
            if (al == null)
                al = advData.GetListByName(lm.listA, publicLists);

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", doesn't exist");
            }

            int parameterValue = 1;
            if (!String.IsNullOrEmpty(lm.scriptStringParameter))
            {
                parameterValue = UTIL_GetStringParameterValue(lm.scriptStringParameter);
            }

            foreach (var v in al.list)
            {
                if (v is BaseUnit)
                {
                    BaseUnit bu = v as BaseUnit;
                    if (bu.canGainXP)
                    {
                        bu.xp += parameterValue;
                    }
                }
                else if (v is Reference<BaseUnit>)
                {
                    BaseUnit bu = (v as Reference<BaseUnit>).Get();
                    if (bu.canGainXP)
                    {
                        bu.xp += parameterValue;
                    }
                }
                else if (v is Group)
                {
                    Group g = v as Group;
                    int unitsCount = g.GetUnits().FindAll(o => o.Get().canGainXP).Count;
                    if (unitsCount == 0) continue;
                    int bonus = parameterValue / unitsCount;

                    foreach (var u in g.GetUnits())
                    {
                        if (u.Get().canGainXP)
                            u.Get().xp += bonus;
                    }
                }
                else
                {
                    Debug.LogWarning("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", contains at least one non BaseUnit object: " + v.ToString());
                }
            }
        }

        [ScriptType(ScriptType.Type.EditorModifier)]
        [ScriptParameters(typeof(Tag), null)]
        static public void FMO_AddSpelbook(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicModifier lm = advLogic as LogicModifier;
            AdvList al = advData.GetListByName(lm.listA, localLists);
            if (al == null)
                al = advData.GetListByName(lm.listA, publicLists);

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", doesn't exist");
            }

            int parameterValue = 1;
            DBClass t = DataBase.Get(lm.scriptTypeParameter, false);

            if (!String.IsNullOrEmpty(lm.scriptStringParameter))
            {
                float value = UTIL_GetRandomFromStringParameter(lm.scriptStringParameter);
                parameterValue = Mathf.RoundToInt(value);
            }
            if (t != null)
            {
                Tag tag = t as Tag;
                foreach (var v in al.list)
                {
                    if (v is PlayerWizard)
                    {
                        PlayerWizard w = v as PlayerWizard;
                        w.AddBook(tag, (FInt)parameterValue);
                    }
                }
            }
        }

        [ScriptType(ScriptType.Type.EditorModifier)]
        [ScriptParameters(null, typeof(Multitype<float, float, float>))]
        static public void FMO_AddFame(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicModifier lm = advLogic as LogicModifier;
            AdvList al = advData.GetListByName(lm.listA, localLists);
            if (al == null)
                al = advData.GetListByName(lm.listA, publicLists);

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", doesn't exist");
            }
            int parameterValue = 1;

            if (!String.IsNullOrEmpty(lm.scriptStringParameter))
            {
                float value = UTIL_GetRandomFromStringParameter(lm.scriptStringParameter);
                parameterValue = Mathf.RoundToInt(value);
            }

            foreach (var v in al.list)
            {
                if (v is PlayerWizard)
                {
                    (v as PlayerWizard).AddFame(parameterValue);
                }
            }
        }

        [ScriptType(ScriptType.Type.EditorModifier)]
        [ScriptParameters(null, typeof(Multitype<float, float, float>))]
        static public void FMO_TakeFame(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicModifier lm = advLogic as LogicModifier;
            AdvList al = advData.GetListByName(lm.listA, localLists);
            if (al == null)
                al = advData.GetListByName(lm.listA, publicLists);

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", doesn't exist");
            }
            int parameterValue = 1;

            if (!String.IsNullOrEmpty(lm.scriptStringParameter))
            {
                float value = UTIL_StringParameterProcessor(lm.scriptStringParameter).t1;
                parameterValue = Mathf.RoundToInt(value);
            }

            foreach (var v in al.list)
            {
                if (v is PlayerWizard)
                {
                    (v as PlayerWizard).TakeFame(parameterValue);
                }
            }
        }

        [ScriptType(ScriptType.Type.EditorModifier)]
        static public void FMO_TurnToRuin(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicModifier lm = advLogic as LogicModifier;
            AdvList al = advData.GetListByName(lm.listA, localLists);
            if (al == null)
            {
                al = advData.GetListByName(lm.listA, publicLists);
            }

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", doesn't exist");
            }


            foreach (var v in al.list)
            {
                if (v is TownLocation)
                {
                    TownLocation town = v as TownLocation;
                    town.TurnToRuin();
                }
                else
                {
                    Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List element:" + v.ToString() + ", isn't TownLocation type");
                }
            }
        }

        [ScriptType(ScriptType.Type.EditorModifier)]
        static public void FMO_RemoveGlobalEnchantments(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicModifier lm = advLogic as LogicModifier;
            AdvList al = advData.GetListByName(lm.listA, localLists);
            if (al == null)
                al = advData.GetListByName(lm.listA, publicLists);

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", doesn't exist");
            }

            var enchs = GameManager.Get().GetEnchantments();
            foreach (var v in al.list)
            {
                if (v is PlayerWizard)
                {
                    PlayerWizard w = v as PlayerWizard;
                    for (int i = enchs.Count - 1; i >= 0; i--)
                    {
                        if (enchs[i].owner == w)
                        {
                            GameManager.Get().RemoveEnchantment(enchs[i]);
                        }
                    }
                }
                else
                {
                    Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", doesn't contains wizard");
                }
            }
        }
        [ScriptType(ScriptType.Type.EditorModifier)]
        static public void FMO_DisjunctionEvent(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicModifier lm = advLogic as LogicModifier;
            AdvList al = advData.GetListByName(lm.listA, localLists);
            if (al == null)
                al = advData.GetListByName(lm.listA, publicLists);

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", doesn't exist");
            }

            var enchs = GameManager.Get().GetEnchantments();
            foreach (var v in al.list)
            {
                if (v is PlayerWizard)
                {
                    PlayerWizard w = v as PlayerWizard;
                    for (int i = enchs.Count - 1; i >= 0; i--)
                    {
                        if (enchs[i].owner == w && enchs[i].source.Get().allowDispel)
                        {
                            GameManager.Get().RemoveEnchantment(enchs[i].source);
                        }
                    }

                    var wizardEnchs = w.GetEnchantments();
                    for (int i = wizardEnchs.Count - 1; i >= 0; i--)
                    {
                        if (wizardEnchs[i].source.Get().allowDispel)
                        {
                            w.RemoveEnchantment(wizardEnchs[i].source);
                        }
                    }
                }
                else
                {
                    Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", doesn't contains wizard");
                }
            }
        }

        [ScriptType(ScriptType.Type.EditorModifier)]
        [ScriptParameters(typeof(Enchantment))]
        static public void FMO_RemoveEnchantment(AdventureData advData, BaseNode baseNode, AdvLogic advLogic,
                                                 Dictionary<string, AdvList> publicLists, Dictionary<string, AdvList> localLists)
        {
            LogicModifier lm = advLogic as LogicModifier;
            AdvList al = advData.GetListByName(lm.listA, localLists);
            if (al == null)
                al = advData.GetListByName(lm.listA, publicLists);

            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", doesn't exist");
            }

            DBClass t = DataBase.Get(lm.scriptTypeParameter, false);
            if (t != null)
            {
                foreach (var v in al.list)
                {
                    if (v is IEnchantable)
                    {
                        IEnchantable ie = v as IEnchantable;
                        Enchantment e = t as Enchantment;
                        var enchs = ie.GetEnchantments();
                        for (int i = enchs.Count - 1; i >= 0; i--)
                        {
                            if (enchs[i].source == e)
                            {
                                GameManager.Get().RemoveEnchantment(enchs[i].source);
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + lm.listA + ", doesn't contains IEnchantable objects");
                    }
                }
            }
        }


        #endregion

        #region Spawn Node Scripts
        [ScriptType(ScriptType.Type.EditorSpawnLocation)]
        static public void FSA_SpawnLocationAtRandomPlane(AdventureData advData, BaseNode baseNode)
        {
            NodeSpawnLocation nsl = baseNode as NodeSpawnLocation;
            DBClass t = DataBase.Get(nsl.spawnName, false);
            DBDef.Location location = t as DBDef.Location;
            DBDef.Group group = t as DBDef.Group;
            int distance = 1;
            if (!string.IsNullOrEmpty(nsl.distance))
            {
                distance = Mathf.RoundToInt(UTIL_GetRandomFromStringParameter(nsl.distance));
            }
            string moduleName = baseNode.parentEvent.module.name;
            int eventID = nsl.navigateToEvent;

            AdvList al = advData.GetListByName(nsl.anchorName, null);
            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + nsl.anchorName + ", doesn't exist");
                return;
            }

            IPlanePosition ipp = al.list.Count == 0 ? null : al.list[0] as IPlanePosition;
            if (ipp == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List:" + al.list + " element aren't IPlanePosition type");
                return;
            }

            WorldCode.Plane plane;
            if (MHRandom.Get().GetInt(0, 2) == 0)
                plane = World.GetArcanus();
            else
                plane = World.GetMyrror();

            var positions = ipp.GetSurroundingArea(distance);
            positions.RandomSortThreadSafe();

            for (int i = 0; i < positions.Count; i++)
            {
                Hex hex = plane.GetHexAt(positions[i]);
                if (hex != null && hex.IsLand() && hex.Resource == null &&
                    GameManager.Get().GetLocationAt(positions[i], plane) == null &&
                    GameManager.Get().GetGroupAt(positions[i], plane) == null)
                {
                    MOM.Location newLoc = MOM.Location.CreateLocation(positions[i], plane, location, 0);
                    //newLoc.GuaranteeDefenders();

                    if (eventID != 0)
                    {
                        newLoc.advTrigger = new AdventureTrigger();
                        newLoc.advTrigger.module = moduleName;
                        newLoc.advTrigger.adventure = eventID;
                    }
                    if (group != null)
                    {
                        Group g = ScriptLibrary.Call(group.creationScript, group, 1000, plane, 0) as Group;

                        g.Position = positions[i];
                        g.GetMapFormation();
                        g.UpdateMarkers();
                    }
                    return;
                }
            }
            Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; Unable to spawn location");
        }

        [ScriptType(ScriptType.Type.EditorSpawnLocation)]
        static public void FSA_SpawnLocationAtCurrentPlane(AdventureData advData, BaseNode baseNode)
        {
            NodeSpawnLocation nsl = baseNode as NodeSpawnLocation;
            DBClass t = DataBase.Get(nsl.spawnName, false);
            DBDef.Location location = t as DBDef.Location;
            DBDef.Group group = t as DBDef.Group;
            int distance = 1;
            if (!string.IsNullOrEmpty(nsl.distance))
            {
                distance = Mathf.RoundToInt(UTIL_GetRandomFromStringParameter(nsl.distance));
            }
            string moduleName = baseNode.parentEvent.module.name;
            int eventID = nsl.navigateToEvent;

            AdvList al = advData.GetListByName(nsl.anchorName, null);
            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + nsl.anchorName + ", doesn't exist");
                return;
            }

            IPlanePosition ipp = al.list.Count == 0 ? null : al.list[0] as IPlanePosition;
            if (ipp == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List:" + al.list + " element aren't IPlanePosition type");
                return;
            }

            WorldCode.Plane plane = ipp.GetPlane();
            var positions = ipp.GetSurroundingArea(distance);
            positions.RandomSortThreadSafe();

            for (int i = 0; i < positions.Count; i++)
            {
                Hex hex = plane.GetHexAt(plane.GetPositionWrapping(positions[i]));
                if (hex != null && hex.IsLand() && hex.Resource == null &&
                    GameManager.Get().GetLocationAt(positions[i], plane) == null &&
                    GameManager.Get().GetGroupAt(positions[i], plane) == null)
                {
                    if (location != null)
                    {
                        MOM.Location newLoc = MOM.Location.CreateLocation(positions[i], plane, location, 0);
                        //newLoc.GuaranteeDefenders();

                        if (eventID != 0)
                        {
                            newLoc.advTrigger = new AdventureTrigger();
                            newLoc.advTrigger.module = moduleName;
                            newLoc.advTrigger.adventure = eventID;
                        }
                    }
                    if (group != null)
                    {
                        Group g = ScriptLibrary.Call(group.creationScript, group, 1000, plane, 0) as Group;

                        g.Position = positions[i];
                        g.GetMapFormation();
                        g.UpdateMarkers();
                    }
                    return;
                }
            }
            Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; Unable to spawn location");
        }

        [ScriptType(ScriptType.Type.EditorSpawnLocation)]
        static public void FSA_SpawnLocationAtOtherPlane(AdventureData advData, BaseNode baseNode)
        {
            NodeSpawnLocation nsl = baseNode as NodeSpawnLocation;
            DBClass t = DataBase.Get(nsl.spawnName, false);
            DBDef.Location location = t as DBDef.Location;
            DBDef.Group group = t as DBDef.Group;
            int distance = 1;
            if (!string.IsNullOrEmpty(nsl.distance))
            {
                distance = Mathf.RoundToInt(UTIL_GetRandomFromStringParameter(nsl.distance));
            }
            string moduleName = baseNode.parentEvent.module.name;
            int eventID = nsl.navigateToEvent;

            AdvList al = advData.GetListByName(nsl.anchorName, null);
            if (al == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List :" + nsl.anchorName + ", doesn't exist");
                return;
            }

            IPlanePosition ipp = al.list.Count == 0 ? null : al.list[0] as IPlanePosition;
            if (ipp == null)
            {
                Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; List:" + al.list + " element aren't IPlanePosition type");
                return;
            }

            WorldCode.Plane plane = World.GetOtherPlane(ipp.GetPlane());
            var positions = ipp.GetSurroundingArea(distance);
            positions.RandomSortThreadSafe();

            for (int i = 0; i < positions.Count; i++)
            {
                Hex hex = plane.GetHexAt(plane.GetPositionWrapping(positions[i]));
                if (hex != null && hex.IsLand() && hex.Resource == null &&
                    GameManager.Get().GetLocationAt(positions[i], plane) == null &&
                    GameManager.Get().GetGroupAt(positions[i], plane) == null)
                {
                    MOM.Location newLoc = MOM.Location.CreateLocation(positions[i], plane, location, 0);
                    //newLoc.GuaranteeDefenders();

                    if (eventID != 0)
                    {
                        newLoc.advTrigger = new AdventureTrigger();
                        newLoc.advTrigger.module = moduleName;
                        newLoc.advTrigger.adventure = eventID;
                    }
                    if (group != null)
                    {
                        Group g = ScriptLibrary.Call(group.creationScript, group, 1000, plane, 0) as Group;

                        g.Position = positions[i];
                        g.GetMapFormation();
                        g.UpdateMarkers();
                    }
                    return;
                }
            }
            Debug.LogError("Event: " + baseNode.parentEvent.name + ", node: " + baseNode.ID + "; Unable to spawn location");
        }

        [ScriptType(ScriptType.Type.EditorSpawnLocation)]
        static public void FSA_StrongLocationsBothPlanes(AdventureData advData, BaseNode baseNode)
        {
            FSA_SpawnStrongLocations(World.GetArcanus());
            FSA_SpawnStrongLocations(World.GetMyrror());
        }
        static public void FSA_WeakLocationsBothPlanes(AdventureData advData, BaseNode baseNode)
        {
            FSA_SpawnWeakLocations(World.GetArcanus());
            FSA_SpawnWeakLocations(World.GetMyrror());
        }
        [ScriptType(ScriptType.Type.EditorSpawnLocation)]
        static public void FSA_StrongTechDungeonsBothPlanes(AdventureData advData, BaseNode baseNode)
        {
            FSA_SpawnStrongTechDungeons(World.GetArcanus());
            FSA_SpawnStrongTechDungeons(World.GetMyrror());
        }
        [ScriptType(ScriptType.Type.EditorSpawnLocation)]
        static public void FSA_StrongLocationsArcanus(AdventureData advData, BaseNode baseNode)
        {
            FSA_SpawnStrongLocations(World.GetArcanus());
        }
        [ScriptType(ScriptType.Type.EditorSpawnLocation)]
        static public void FSA_StrongLocationsMyrror(AdventureData advData, BaseNode baseNode)
        {
            FSA_SpawnStrongLocations(World.GetMyrror());
        }
        static public void FSA_SpawnStrongLocations(WorldCode.Plane p)
        {
            List<List<Vector3i>> islands = p.GetIslands().FindAll(o => o.Count >= 10);
            var locations = GameManager.GetLocationsOfThePlane(p);
            var groups = GameManager.GetGroupsOfPlane(p);
            List<Vector3i> availablePos = new List<Vector3i>();
            List<Vector3i> strongLocations = new List<Vector3i>();
            var lairsMultiplier = DifficultySettingsData.GetSettingAsFloat("UI_LAIR_NUMBER_MULTIPLIER");

            foreach (var list in islands)
            {
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    var pos = list[i];
                    if (p.GetHexAt(pos).HaveFlag(ETerrainType.Mountain))
                    {
                        continue;
                    }
                    else if (p.GetHexAt(pos).additionalDecorInstance != null)
                    {
                        continue;
                    }
                    else if (groups.FindIndex(o => pos == o.GetPosition()) >= 0)
                    {
                        continue;
                    }
                    //note that all locations are filtered out by the check HexCoordinates.HexDistance(pos, o.GetPosition()) <= 2
                    else if (locations.FindIndex(o => o is TownLocation && (o as TownLocation).GetOwnerID() > 0 && HexCoordinates.HexDistance(pos, o.GetPosition()) <= 8 ||
                                                      HexCoordinates.HexDistance(pos, o.GetPosition()) <= 2) >= 0)
                    {
                        continue;
                    }
                    availablePos.Add(pos);
                }
            }

            int minLoc = availablePos.Count / 7;
            availablePos.RandomSort();
            if (lairsMultiplier != 0 && lairsMultiplier != 1)
            {
                int lairStartingCount = availablePos.Count;
                //Magic number *0.75 allow to lower numer of lairs after "lair space" become free from preview lair placing cycles (like FSA_SpawnWeakLocations)
                int lairsCount = (int)((float)lairStartingCount * lairsMultiplier * 0.75);
                if (lairsCount < 0) lairsCount = 1;
                availablePos.RemoveRange(0, availablePos.Count - lairsCount );
            }

            for (int i = 0; i < availablePos.Count; i++)
            {
                var index = strongLocations.FindIndex(o => HexCoordinates.HexDistance(availablePos[i], o) <= 1);
                if (index == -1)
                {
                    strongLocations.Add(availablePos[i]);
                }

                if (strongLocations.Count == minLoc)
                {
                    break;
                }
            }

            int idx;
            var rdn = MHRandom.Get();
            var dbLocations = DataBase.GetType<DBDef.Location>();
            dbLocations = dbLocations.FindAll(o => o.locationType == ELocationType.StrongLair);

            foreach (var v in strongLocations)
            {
                idx = rdn.GetInt(0, dbLocations.Count);
                MOM.Location.CreateLocation(v, p, dbLocations[idx], 0);
            }
        }
        //used to spawn initial weak lairs to guarantee some in the vicinity of each player.
        static public void FSA_SpawnWeakLocations(WorldCode.Plane p)
        {
            var locations = GameManager.GetLocationsOfThePlane(p);
            var groups = GameManager.GetGroupsOfPlane(p);
            List<Vector3i> availbleHex = new List<Vector3i>();
            List<Vector3i> emptyHex = new List<Vector3i>();
            List<Vector3i> weakLocations = new List<Vector3i>();
            var lairsMultiplier = DifficultySettingsData.GetSettingAsFloat("UI_LAIR_NUMBER_MULTIPLIER");

            var playerTowns = locations.FindAll(o => o is TownLocation && (o as TownLocation).GetOwnerID() > 0);
            foreach (var t in playerTowns)
            {
                availbleHex = HexNeighbors.GetRange(t.GetPosition(), 5, 2);
                int spawn = 0;

                availbleHex.RandomSort();

                if (lairsMultiplier != 0 && lairsMultiplier != 1)
                {
                    int lairStartingCount = availbleHex.Count;
                    int lairsCount = Mathf.RoundToInt((float)lairStartingCount * lairsMultiplier);
                    if (lairsCount < 0) lairsCount = 1;
                    availbleHex.RemoveRange(0, availbleHex.Count - lairsCount);
                }

                foreach (var h in availbleHex)
                {
                    var pos = p.GetPositionWrapping(h);
                    var hex = p.GetHexAt(pos);
                    if (hex == null) continue;

                    if (locations.FindIndex(o => o.GetPosition() == pos) >= 0) continue;
                    else if (groups.FindIndex(o => o.GetPosition() == pos) >= 0) continue;
                    else if (hex.HaveFlag(ETerrainType.Mountain)) continue;
                    else if (hex.additionalDecorInstance != null) continue;
                    else if (hex.resourceInstance != null) continue;
                    else if (hex.HaveFlag(ETerrainType.Sea)) continue;

                    weakLocations.Add(pos);
                    spawn++;
                    if (spawn == 2) break;
                }
            }

            int idx;
            var rdn = MHRandom.Get();
            var dbLocations = DataBase.GetType<DBDef.Location>();
            dbLocations = dbLocations.FindAll(o => o.locationType == ELocationType.WeakLair);

            foreach (var v in weakLocations)
            {
                idx = rdn.GetInt(0, dbLocations.Count);
                MOM.Location.CreateLocation(v, p, dbLocations[idx], 0);
            }
        }
        static public void FSA_SpawnStrongTechDungeons(WorldCode.Plane p)
        {
            List<List<Vector3i>> islands = p.GetIslands().FindAll(o => o.Count >= 10);
            var locations = GameManager.GetLocationsOfThePlane(p);
            var groups = GameManager.GetGroupsOfPlane(p);
            List<Vector3i> availablePos = new List<Vector3i>();
            List<Vector3i> spawnLocations = new List<Vector3i>();

            foreach (var list in islands)
            {
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    var pos = list[i];
                    if (p.GetHexAt(pos).Resource != null ||
                        p.GetHexAt(pos).HaveFlag(ETerrainType.Mountain))
                    {
                        continue;
                    }
                    else if (p.GetHexAt(pos).additionalDecorInstance != null)
                    {
                        continue;
                    }
                    else if (groups.FindIndex(o => pos == o.GetPosition()) >= 0)
                    {
                        continue;
                    }
                    else if (locations.FindIndex(o => pos == o.GetPosition()) >= 0)
                    {
                        continue;
                    }
                    availablePos.Add(pos);
                }
            }

            //find strongest player and human player
            int player = PlayerWizard.HumanID();
            int strongest = player;
            int townCount = 0;
            foreach (var v in GameManager.GetWizards())
            {
                var townCount2 = GameManager.GetWizardTownCount(v.GetID());
                if (townCount2 > townCount)
                {
                    strongest = v.GetID();
                    townCount = townCount2;
                }
            }

            List<MOM.Location> locs = GameManager.GetLocationsOfThePlane(p).FindAll(o => o.GetOwnerID() == strongest || o.GetOwnerID() == player);

            //sort locations by distance to strongest player and human player towns
            availablePos.Sort(delegate (Vector3i a, Vector3i b)
            {
                int distA = int.MaxValue;
                foreach (var k in locs)
                {
                    var d = k.GetDistanceTo(a);
                    if (d < distA)
                    {
                        distA = d;
                    }
                }
                int distB = int.MaxValue;
                foreach (var k in locs)
                {
                    var d = k.GetDistanceTo(b);
                    if (d < distB)
                    {
                        distB = d;
                    }
                }
                return distA.CompareTo(distB);
            });

            //attempt to create locations (based on difficulty) spread out over the map, but focused around strongest AI and a player towns
            var midGame = DifficultySettingsData.GetSetting("UI_MID_GAME_AWAKE");
            int creationCount = 5;
            switch (midGame.value)
            {
                case "1":
                    creationCount = 7;
                    break;
                case "2":
                    creationCount = 7;
                    break;
                case "3":
                    creationCount = 12;
                    break;
                case "4":
                    creationCount = 14;
                    break;
            }
            //region where we don't want to spawn locations that is to close to other spawns
            int exlusionZone = 4;

            //now pick only some best locations, ensuring that they are close-ish to those towns.
            //On average map with size in thousands should still provide us with region where those players have their presence.
            //additionally distance to other spawns would be a limited problem due to the fact
            //that many hexes were excluded from the list due to them being not land, being occupied etc.
            availablePos = availablePos.GetRange(0, 400);

            //sort locations to ensure variety
            availablePos.RandomSort();


            for (int i = 0; i < availablePos.Count; i++)
            {
                var index = spawnLocations.FindIndex(o => HexCoordinates.HexDistance(availablePos[i], o) <= exlusionZone);
                if (index == -1)
                {
                    spawnLocations.Add(availablePos[i]);
                }

                if (spawnLocations.Count == creationCount)
                {
                    break;
                }
            }

            //spawn
            var techLocation = DataBase.GetType<DBDef.Location>().Find(o => o.dbName == "LOCATION-TECH_DUNGEON");
            foreach (var v in spawnLocations)
            {
                MOM.Location.CreateLocation(v, p, techLocation, 0);
            }
        }

        [ScriptType(ScriptType.Type.EditorSpawnLocation)]
        static public void FSA_SpawnKraken1(AdventureData advData, BaseNode baseNode)
        {
            FSA_SpawnKrakenLair(World.GetArcanus());
        }
        [ScriptType(ScriptType.Type.EditorSpawnLocation)]
        static public void FSA_SpawnKraken2(AdventureData advData, BaseNode baseNode)
        {
            FSA_SpawnKrakenLair(World.GetMyrror());
        }

        static public void FSA_SpawnKrakenLair(WorldCode.Plane p)
        {
            List<Vector3i> seas = new List<Vector3i>(p.waterBodies.Keys);
            seas.RandomSort();
            var groups = GameManager.GetGroupsOfPlane(p);
            var locations = GameManager.GetLocationsOfThePlane(p);
            for (int x = 0; x < seas.Count; x++)
            {
                var pos = seas[x];
                if (p.area.DistanceToBorder(pos) < 3) continue;
                if (p.GetWaterBodyFor(pos) == null || p.GetWaterBodyFor(pos).Count < 10) continue;
                bool valid = true;
                foreach (var h in HexNeighbors.neighbours)
                {
                    if (p.IsLand(pos + h))
                    {
                        valid = false;
                        continue;
                    }
                    if (p.GetHexAt(pos).Resource != null)
                    {
                        valid = false;
                        continue;
                    }
                    if (p.GetHexAt(pos).additionalDecorInstance != null)
                    {
                        valid = false;
                        continue;
                    }
                    if (groups.FindIndex(o => pos == o.GetPosition()) >= 0)
                    {
                        valid = false;
                        continue;
                    }
                    if (locations.FindIndex(o => pos == o.GetPosition()) >= 0)
                    {
                        valid = false;
                        continue;
                    }
                }
                if (!valid) continue;

                //no location might be located next to it
                if (GameManager.GetLocationsOfThePlane(p).Find(l => l.GetDistanceTo(pos) <= 1) != null) continue;
                DBDef.Location inst;
                if (p.arcanusType)
                {
                    inst = (DBDef.Location)LOCATION.WATER_BOSS_LAIR_1;
                }
                else
                {
                    inst = (DBDef.Location)LOCATION.WATER_BOSS_LAIR_2;
                }
                if (inst == null) continue;
                var loc = MOM.Location.CreateLocation(pos, p, inst, 0, true);
                FOW.Get().MarkVisible(loc.GetPosition(), p.arcanusType);
                foreach(var h in HexNeighbors.neighbours)
                {
                    FOW.Get().MarkVisible(pos + h, p.arcanusType);
                }

                FOW.Get().UpdateFogForPlane(p);
                break;
            }
        }
        #endregion

        #region Battle Node Scripts
        [ScriptType(ScriptType.Type.EditorBattleNode)]
        static public object DefaultLocationGroup(AdventureData advData, BaseNode baseNode)
        {
            if (advData.advSource != null && advData.advSource is MOM.Location)
            {
                return (advData.advSource as MOM.Location).GetLocalGroup();
            }
            return null;
        }
        static object CreateGroup(AdventureData advData, BaseNode baseNode, int budget)
        {
            NodeBattle nb = baseNode as NodeBattle;
            if (nb == null) return null;

            if (nb.isScalable)
            {
                budget += BudgetScaling();
            }

            var dbg = DataBase.Get(nb.opponentGroupName, true) as DBDef.Group;
            if (dbg != null)
            {
                WorldCode.Plane p = advData.adventurePlane;
                if (p == null)
                {
                    AdvList al = advData.GetListByName(nb.listA, null);
                    if (al != null)
                    {
                        if (al.list[0] is MOM.Unit)
                        {
                            p = (al.list[0] as MOM.Unit).GetPlane();
                        }
                        else if (al.list[0] is MOM.Group)
                        {
                            p = (al.list[0] as MOM.Group).GetPlane();
                        }
                    }
                }
                var g = ScriptLibrary.Call(dbg.creationScript, dbg, budget, p, nb.level);
                //consider if there is a location involved in an event, if it is, transfer units to a location
                if (advData.advSource != null && advData.advSource is MOM.Location
                    && g != null && g is MOM.Group)
                {
                    var group = g as MOM.Group;
                    var loc = advData.advSource as MOM.Location;
                    if (loc.GetLocalGroup() != null && loc.GetLocalGroup().alive)
                    {
                        if (loc.GetLocalGroup().GetUnits().Count > 0)
                        {
                            //location contained previous units. Destroy them to avoid unit overflow or danger stacking
                            var oldUnits = loc.GetLocalGroup().GetUnits().ToArray();
                            foreach (var oldUnit in oldUnits)
                            {
                                oldUnit.Get().Destroy();
                            }
                        }
                        //transfer new units to a location
                        //transfer of all units would destroy newly created group (cleanup)
                        group.TransferUnits(loc.GetLocalGroup());
                        return loc.GetLocalGroup();
                    }
                }

                return g;
            }
            return null;
        }
        [ScriptType(ScriptType.Type.EditorBattleNode)]
        static public object WeakGroup(AdventureData advData, BaseNode baseNode)
        {
            int budget = 600;
            var dbBudgetGroupValue = DataBase.Get<BudgetValue>(BUDGET_VALUE.WEAK_GROUP);
            if (dbBudgetGroupValue != null)
            {
                budget = dbBudgetGroupValue.value;
            }
            return CreateGroup(advData, baseNode, budget);
        }

        [ScriptType(ScriptType.Type.EditorBattleNode)]
        static public object MediumGroup(AdventureData advData, BaseNode baseNode)
        {
            NodeBattle nb = baseNode as NodeBattle;

            int budget = 600;
            var dbBudgetGroupValue = DataBase.Get<BudgetValue>(BUDGET_VALUE.NORMAL_GROUP);
            if (dbBudgetGroupValue != null)
            {
                budget = dbBudgetGroupValue.value;
            }
            return CreateGroup(advData, baseNode, budget);
        }
        [ScriptType(ScriptType.Type.EditorBattleNode)]
        static public object StrongGroup(AdventureData advData, BaseNode baseNode)
        {
            NodeBattle nb = baseNode as NodeBattle;

            int budget = 600;
            var dbBudgetGroupValue = DataBase.Get<BudgetValue>(BUDGET_VALUE.STRONG_GROUP);
            if (dbBudgetGroupValue != null)
            {
                budget = dbBudgetGroupValue.value;
            }
            return CreateGroup(advData, baseNode, budget);
        }
        [ScriptType(ScriptType.Type.EditorBattleNode)]
        static public object VStrongGroup(AdventureData advData, BaseNode baseNode)
        {
            NodeBattle nb = baseNode as NodeBattle;

            int budget = 600;
            var dbBudgetGroupValue = DataBase.Get<BudgetValue>(BUDGET_VALUE.VSTRONG_GROUP);
            if (dbBudgetGroupValue != null)
            {
                budget = dbBudgetGroupValue.value;
            }
            return CreateGroup(advData, baseNode, budget);
        }

        [ScriptType(ScriptType.Type.EditorBattleNode)]
        [ScriptParameters(null, typeof(int))]
        static public object HeroGroup(AdventureData advData, BaseNode baseNode)
        {
            NodeBattle nb = baseNode as NodeBattle;

            if (nb == null) return null;
            //int quantity = Mathf.RoundToInt(UTIL_GetRandomFromStringParameter(nb.scriptStringParameter));

            var dbg = DataBase.Get(nb.opponentGroupName, true) as DBDef.Group;
            if (dbg != null)
            {
                WorldCode.Plane p = advData.adventurePlane;
                if (p == null)
                {
                    AdvList al = advData.GetListByName(nb.listA, null);
                    if (al != null)
                    {
                        if (al.list[0] is MOM.Unit)
                        {
                            p = (al.list[0] as MOM.Unit).GetPlane();
                        }
                        else if (al.list[0] is MOM.Group)
                        {
                            p = (al.list[0] as MOM.Group).GetPlane();
                        }
                    }
                }
                var g = ScriptLibrary.Call(dbg.creationScript, dbg, 0, p, nb.level);
                return g;
            }
            return null;
        }

        #endregion


        #region UTILS

        /// <summary>
        /// Convert string into numeric values in format: 
        /// t0: chance, 
        /// t1: min, 
        /// t2: max
        /// </summary>
        /// <param name="param"></param>
        /// <returns>chance, min, max</returns>
        static public Multitype<float, float, float> UTIL_StringParameterProcessor(string param)
        {
            try
            {
                string[] parameters = param.Split(';');

                float chance = 1f;
                string min = null;
                string max = null;
                for (int i = 0; i < parameters.Length; i++)
                {
                    int percent = parameters[i].IndexOf("%");
                    if (percent > -1)
                    {
                        string sChance = parameters[i].Substring(0, percent);

                        sChance = sChance.Replace(" ", "");
                        chance = Convert.ToSingle(sChance, Globals.GetCultureInfo()) * 0.01f;
                    }
                    else if (min == null)
                    {
                        min = parameters[i].Replace(" ", "");
                    }
                    else
                    {
                        max = parameters[i].Replace(" ", "");
                    }
                }

                float fMin = min != null ? Convert.ToSingle(min, Globals.GetCultureInfo()) : 0f;
                float fMax = max != null ? Convert.ToSingle(max, Globals.GetCultureInfo()) : fMin;

                return new Multitype<float, float, float>(chance, fMin, fMax);
            }
            catch
            {
                Debug.LogError("UTIL_StringParameterProcessor error");
                return null;
            }
        }

        static public float UTIL_GetRandomFromStringParameter(string value)
        {
            var val = UTIL_StringParameterProcessor(value);

            return MHRandom.Get().GetFloat(val.t1, val.t2);
        }

        static public int UTIL_GetStringParameterValue(string value)
        {
            var change = UTIL_StringParameterProcessor(value);
            if (change.t0 < 1f && MHRandom.Get().GetFloat(0f, 1f) > change.t0)
            {
                return 0;
            }
            if (change.t1 != change.t2)
            {
                return Mathf.RoundToInt(MHRandom.Get().GetFloat(change.t1, change.t2));
            }
            else
            {
                return Mathf.RoundToInt(change.t1);
            }
        }

        static public bool UTIL_IsValueValid(int value, int requiredValue, LogicUtils.Comparison lComp)
        {
            switch (lComp)
            {
                case LogicUtils.Comparison.Equal:
                    return value == requiredValue;
                case LogicUtils.Comparison.LessOrEqualThan:
                    return value <= requiredValue;
                case LogicUtils.Comparison.LessThan:
                    return value < requiredValue;
                case LogicUtils.Comparison.MoreOrEqualThan:
                    return value >= requiredValue;
                case LogicUtils.Comparison.MoreThan:
                    return value > requiredValue;
                default:
                    return false;
            }
        }
        static public bool UTIL_IsValueValid(FInt value, FInt requiredValue, LogicUtils.Comparison lComp)
        {
            switch (lComp)
            {
                case LogicUtils.Comparison.Equal:
                    return value == requiredValue;
                case LogicUtils.Comparison.LessOrEqualThan:
                    return value <= requiredValue;
                case LogicUtils.Comparison.LessThan:
                    return value < requiredValue;
                case LogicUtils.Comparison.MoreOrEqualThan:
                    return value >= requiredValue;
                case LogicUtils.Comparison.MoreThan:
                    return value > requiredValue;
                default:
                    return false;
            }
        }

        static public bool UTIL_IsOwnerValid(int ownerID, LogicEntry.PlayerOwner playerOwner, AdventureData advData)
        {
            switch (playerOwner)
            {
                case LogicEntry.PlayerOwner.ActivePlayer:
                    if (advData.mainPlayerWizard == 0)
                    {
                        Debug.LogError("Using opponent players mode when no player is event owner");
                    }
                    return advData.mainPlayerWizard > 0 && ownerID == advData.mainPlayerWizard;
                case LogicEntry.PlayerOwner.AnyPlayer:
                    return true;
                case LogicEntry.PlayerOwner.NeutralPlayers:
                    return ownerID == 0;
                case LogicEntry.PlayerOwner.NonNeutralPlayer:
                    return ownerID != 0;
                case LogicEntry.PlayerOwner.OpponentPlayers:
                    if (advData.mainPlayerWizard == 0)
                    {
                        Debug.LogError("Using opponent players mode when no player is event owner");
                    }
                    return ownerID != 0 && ownerID != advData.mainPlayerWizard;
                default:
                    return false;
            }
        }

        static public bool UTIL_IsCityTypeValid(LogicEntry.CityType cityType, TownLocation city, AdventureData advData)
        {
            switch (cityType)
            {
                case LogicEntry.CityType.All:
                case LogicEntry.CityType.None:
                    return !city.IsAnOutpost();
                case LogicEntry.CityType.Capitol:
                    if (city.GetOwnerID() == 0) return false;
                    return city == GameManager.GetWizard(city.GetOwnerID()).GetTowerLocation();
                case LogicEntry.CityType.Outpost:
                    return city.IsAnOutpost();
                case LogicEntry.CityType.Hamlet:
                    return city.GetTownSize() == TownSize.Hamlet;
                case LogicEntry.CityType.Vilage:
                    return city.GetTownSize() == TownSize.Village;
                case LogicEntry.CityType.Town:
                    return city.GetTownSize() == TownSize.Town;
                case LogicEntry.CityType.Citi:
                    return city.GetTownSize() == TownSize.City;
                default:
                    return false;
            }
        }
        static public bool UTIL_IsTerrainTypeValid(ETerrainCategory terrain, Hex hex, IPlanePosition ipp)
        {
            switch (terrain)
            {
                case ETerrainCategory.Highland:
                    return hex.HaveAnyFlag(Hex.HEIGHT);
                case ETerrainCategory.Lowland:
                    return hex.HaveAnyFlag(Hex.LOWLAND);
                case ETerrainCategory.NonGrassland:
                    return hex.HaveAnyFlag(Hex.NON_GRASSLAND);
                case ETerrainCategory.Seashore:
                    return hex.IsLand() && hex.SeaAround(ipp.GetPlane()) > 0;
                case ETerrainCategory.Water:
                    return !hex.IsLand();
                default:
                    return false;
            }
        }
        static public bool UTIL_IsResourceValid(EMinedResources minedRes, Resource res)
        {
            switch (minedRes)
            {
                case EMinedResources.AdamantineOre:
                    return res == (DBDef.Resource)RESOURCE.ADAMANTINE_ORE;
                case EMinedResources.Coal:
                    return res == (DBDef.Resource)RESOURCE.COAL;
                case EMinedResources.CrysxCrystals:
                    return res == (DBDef.Resource)RESOURCE.CRYSX_CRYSTALS;
                case EMinedResources.Gems:
                    return res == (DBDef.Resource)RESOURCE.GEMS;
                case EMinedResources.GoldOre:
                    return res == (DBDef.Resource)RESOURCE.GOLD_ORE;
                case EMinedResources.IronOre:
                    return res == (DBDef.Resource)RESOURCE.IRON_ORE;
                case EMinedResources.MithrilOre:
                    return res == (DBDef.Resource)RESOURCE.MITHRIL_ORE;
                case EMinedResources.QuorkCrystals:
                    return res == (DBDef.Resource)RESOURCE.QUORK_CRYSTALS;
                case EMinedResources.SilverOre:
                    return res == (DBDef.Resource)RESOURCE.SILVER_ORE;
                case EMinedResources.Orichalcum:
                    return res == (DBDef.Resource)RESOURCE.ORICHALCUM;
                case EMinedResources.AnyMinedResource:
                    if (res.mineral)
                        return false;
                    else
                        return true;
                default:
                    return false;
            }
        }
        static public bool UTIL_IsMinedResource(Resource res)
        {
            return res.mineral;
        }


        public static int BudgetScaling()
        {
            var turn = TurnManager.GetTurnNumber();
            var dbBudget = DataBase.Get<BudgetScaling>(BUDGET_SCALING.BUDGET);
            float scalablebudget = 1.0f;
            if (dbBudget != null)
            {
                scalablebudget = BudgetSample.Sample(dbBudget.turnToBudget, turn);
            }
            return (int)scalablebudget;
        }
        public static int OneExpCost()
        {
            int oneExpCost = 1;
            var dbBudgetExp = DataBase.Get<BudgetValue>(BUDGET_VALUE.EXP_COST);
            if (dbBudgetExp != null)
            {
                oneExpCost = dbBudgetExp.value;
            }
            return oneExpCost;
        }
        #endregion

    }
}
#endif