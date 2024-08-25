#if !USE_DEBUG_SCRIPT || !UNITY_EDITOR

using DBDef;
using DBEnum;
using MHUtils;
using MOM;
using MOM.Adventures;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WorldCode;

namespace MOMScripts
{
    public class TradeAIScripts : ScriptBase
    {
        static public List<object> GetTotalWaresList(PlayerWizard w)
        {
            List<object> list = new List<object>();

            if (w is PlayerWizardAI)
            {
                //add gold
                var ware = new Multitype<NodeTrade.TradeCurrency, string, int>(NodeTrade.TradeCurrency.Gold, "IconGold", w.money);
                int count = GetWareSaleEvaluation(w, ware);
                if (count > 0)
                {
                    ware.t2 = count;
                    list.Add(ware);
                }


                //add mana
                ware = new Multitype<NodeTrade.TradeCurrency, string, int>(NodeTrade.TradeCurrency.Mana, "IconMana", w.mana);
                count = GetWareSaleEvaluation(w, ware);
                if (count > 0)
                {
                    ware.t2 = count;
                    list.Add(ware);
                }
            }
            else
            {
                var ware = new Multitype<NodeTrade.TradeCurrency, string, int>(NodeTrade.TradeCurrency.Gold, "IconGold", w.money);                
                list.Add(ware);
                ware = new Multitype<NodeTrade.TradeCurrency, string, int>(NodeTrade.TradeCurrency.Mana, "IconMana", w.mana);                
                list.Add(ware);                
            }

            //add Spells
            var sm = w.GetSpellManager();
            var spells = sm.GetSpells();
            if (spells != null)
            {
                var filteredOut = spells.FindAll(o => GetWareSaleEvaluation(w, o) > 0);
                list.AddRange(filteredOut);
            }

            //add artefacts
            if(w.artefacts != null)
            {
                var filteredOut = w.artefacts.FindAll(o => GetWareSaleEvaluation(w, o) > 0);
                list.AddRange(filteredOut);
            }

            return list;
        }
        static public int GetWareSaleEvaluation(PlayerWizard w, object o)
        {
            //return 1+: will sell that many
            //return 0: will not sell
            
            if (o is Multitype<NodeTrade.TradeCurrency, string, int>)
            {
                var k = o as Multitype<NodeTrade.TradeCurrency, string, int>;

                if (k.t0 == NodeTrade.TradeCurrency.Gold)
                {
                    int income = w.CalculateMoneyIncome(true);
                    int money = w.money;

                    if(money > 300 && income > -0.1f * money)
                    {
                        float incomeShare = income / (float)money;
                        //use incomeShare to boost part of the money AI is willing to part with.
                        //-0.1 is increase of 0,
                        //while +0.4 (income equal 40% of current cash) would boost sale offer by 0.25 of owned savings
                        float boost = (Mathf.Min(0.4f, incomeShare) + 0.1f) * 0.5f;

                        var tax = w.GetTaxRank();
                        if (tax.rebelion > 0.5f)
                        {
                            //I really need this money....
                            return 0;
                        }
                        if (tax.rebelion > 0.2f)
                        {
                            //I do not think I will offer much in this field...
                            return (int)(money * (0.25f+ boost));
                        }

                        //I will trade half of what I have
                        return (int)(money * (0.5f+ boost));
                    }
                    else
                    {
                        return 0;
                    }
                }
                else if (k.t0 == NodeTrade.TradeCurrency.Mana)
                {
                    int income = w.CalculateManaIncome(true);
                    int mana = w.mana;

                    float incomeShare = income / (float)mana;                    

                    //mana is sold only in case it have minimum of positive income or slightly negative in case of larger supplies
                    if (mana > 300 && income > 0 || 
                        mana > 800 && income > -0.05f)
                    {
                        //use incomeShare to boost part of the mana AI is willing to part with.                    
                        //income equal 50% of current mana would boost sale offer by 0.25 of owned savings
                        //it may produce negative boost in case income is negative and it past requirement test
                        float boost = Mathf.Min(0.5f, incomeShare) * 0.5f;

                        //allow to sell part of the savings
                        return (int)(mana * (0.3f+ boost));
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
            else if(o is DBReference<Spell>)
            {
                var k = o as DBReference<Spell>;                

                //use character to ensure AI will not sell items they are opposing use of.
                //ie: Lawful character may not be willing to trade overland damage spells.
                //on the other hand this will limit already limited trade options.
                //someone may sell items, but still be angry if the others use it. "Its just for display mate!"
                //need to think it over if any action is needed.
                //also... should we filter out spells that are too powerful based on relationship, and put a bar of "aboveMaximum"               
                //relationship required for spell of mastery?

                return 1;
            }
            else if(o is MOM.Artefact)
            {
                //limit which artefacts are for sale based on relationship? 
                //there is little-t-none incentive to limit artefact sales, as the only artefacts for sale are those unused.
                //so it does not seem like any action is needed in this aspect

                return 1;
            }

            Debug.LogWarning("Unknown item, no sale offer for it! " + o);
            return 0;
        }
        static public bool WareCanBeAcquired(PlayerWizard recipient, object item)
        {
            if(item is DBReference<Spell>)
            {
                var k = item as DBReference<Spell>;
                if (k.Get().researchExclusion) return false;
                var dict = MagicAndResearch.RealmTagDictionary();

                if (k.Get().researchCost <= 0) return false;

                if(k.Get().realm == ERealm.Tech)
                {
                    return recipient.traitTechMagic == true;
                }
                else if (dict.ContainsKey(k.Get().realm))
                {
                    return recipient.GetAttributes().Contains(dict[k.Get().realm], FInt.ONE);
                }
                else if(k.Get().realm == ERealm.Arcane)
                {
                    return true;
                }

                return false;
            }

            return true;
        }
        /// <returns>1 very beneficial, 0 normal, -1 lightly beneficial, -2 cannot acquire</returns>
        static public int AdvantageIfAcquired(PlayerWizard recipient, object o)
        {
            if (!WareCanBeAcquired(recipient, o)) return -2;

            if (o is Multitype<NodeTrade.TradeCurrency, string, int>)
            {
                var k = o as Multitype<NodeTrade.TradeCurrency, string, int>;
                if (k.t0 == NodeTrade.TradeCurrency.Gold)
                {
                    var tax = recipient.GetTaxRank();
                    if (tax.rebelion > 0.5f)
                    {
                        //I really need some money....
                        return 1;
                    }
                    if (tax.rebelion > 0.2f)
                    {
                        //I could use some extra gold...
                        return 0;
                    }
                }
                if (k.t0 == NodeTrade.TradeCurrency.Mana)
                {                    
                    int skill = recipient.GetTotalCastingSkill() * 10;
                    int manaIncome = recipient.CalculateManaIncome(true);
                    int want = -1;
                    if(recipient.mana < skill * 10 && manaIncome < skill / 3)
                    {
                        //low mana with medium income
                        want++;
                    }
                    if(recipient.mana < skill * 20 && manaIncome < skill / 5)
                    {
                        //medium mana with low income 
                        want++;
                    }
                    if(recipient.mana < skill * 20 && manaIncome < 0)
                    {
                        //medium mana with negative income
                        want++;
                    }
                    return Mathf.Min(1, want);
                }

                return -1;
            }

            else if (o is DBReference<Spell>)
            {
                var s = o as DBReference<Spell>;
                return recipient.GetMagicAndResearch().SpellInterestLevel(s.Get());
            }
            else if (o is MOM.Artefact)
            {
                var s = o as MOM.Artefact;
                if (recipient.heroes == null || recipient.heroes.Count < 1) return -1;

                if (recipient.artefacts != null)
                {
                    var aOwned = recipient.artefacts.Find(k =>
                                    s.equipmentType == k.equipmentType &&
                                    s.GetValue() <= k.GetValue());
                    if (aOwned != null)
                    {
                        //AI owns artefact of this type unequipped and of greater value. 
                        return -1;
                    }
                }
                int value = -1;
                foreach (var v in recipient.heroes)
                {
                    if (v.Get().artefactManager.equipmentSlots == null) continue;
                    foreach (var slot in v.Get().artefactManager.equipmentSlots)
                    {
                        var index = Array.FindIndex(slot.slotType.Get().eTypes, k => k == s.equipmentType);
                        if (index > -1)
                        {
                            if (slot.item != null)
                            {
                                if (slot.item.GetValue() < s.GetValue()) value = 0;
                            }
                            else
                            {
                                //empty slot worth filling
                                value = 1;
                            }
                        }
                    }
                }

                return value;
            }

            Debug.LogError("item type not processed!" + o);
            return -2;
        }
        static public float ValueScale(object o, int advantageIfAcquired)
        {
            // TODO
            // Warning	CA2233	Correct the potential overflow in the operation 'advantageIfAcquired--1' in 'TradeAIScripts.ValueScale(object, int)'.

            switch (advantageIfAcquired)
            {
                case 1:
                    if (o is Multitype<NodeTrade.TradeCurrency, string, int>)
                    {
                        return 1f;
                    }
                    else if (o is DBReference<Spell>)
                    {
                        return 0.85f;
                    }
                    else if (o is MOM.Artefact)
                    {
                        return 0.70f;
                    }
                    break;
                case 0:
                    if (o is Multitype<NodeTrade.TradeCurrency, string, int>)
                    {
                        return 0.8f;
                    }
                    else if (o is DBReference<Spell>)
                    {
                        return 0.7f;
                    }
                    else if (o is MOM.Artefact)
                    {
                        return 0.5f;
                    }
                    break;
                case -1:
                    if (o is Multitype<NodeTrade.TradeCurrency, string, int>)
                    {
                        return 0.6f;
                    }
                    else if (o is DBReference<Spell>)
                    {
                        return 0.5f;
                    }
                    else if (o is MOM.Artefact)
                    {
                        return 0.20f;
                    }
                    break;
                default:
                    return 0;
            }

            return 0f;
        }


    }
}
#endif