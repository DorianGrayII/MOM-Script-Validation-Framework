#if !USE_DEBUG_SCRIPT || !UNITY_EDITOR
﻿using UnityEngine;
using System.Collections.Generic;
using DBDef;
using MHUtils;
using MOM;
using DBEnum;

namespace MOMScripts
{
    public class GameplayScripts : ScriptBase
    {
        #region Attribute Calculation
        static public NetDictionary<DBReference<Tag>, FInt> UpdateAttributes(IAttributable a)
        {
            ///NOTE! modification of the final dictionary (named finalDict)
            ///by adding or setting new value should happen using 
            ///(UtilExtension) TagExtension 
            ///                     AddFinal (to modify in relation to previous value)
            ///                     SetFinal (to force specific final value)

            BaseUnit bu = null;
            BattleUnit ba = null;
            if (a is BaseUnit)
            {
                bu = a as BaseUnit;
                bu.invulnerabilityProtection = 0;
                bu.invisibilityProtection = 0;
                bu.blurProtection = 0f;
                bu.targetDefMod = 0f;
                bu.isSpellLock = false;
                bu.canMove = true;
                bu.windMasteryNegative = false;
                bu.chaosSurgeEffect = false;
            }
            if (a is BattleUnit)
            {
                ba = a as BattleUnit;
                ba.haste = false;
                ba.canAttack = true;
                ba.canCastSpells = true;
                ba.canContrAttack = true;
                ba.canDefend = true;
                ba.darknessEffect = false;
            }
            BattleUnit bbu = bu as BattleUnit;
            Battle b = null;
            if(bbu != null)
            {
                b = Battle.GetBattle();
            }

            var baseAtt = a.GetAttributes().baseAttributes;
            var finalDict = new NetDictionary<DBReference<Tag>, FInt>();
            foreach(var v in baseAtt)
            {
                finalDict.AddFinal(v.Key.Get(), v.Value);
            }

            if(a is ISkillable)
            {
                var aIS = a as ISkillable;
                if (aIS.GetSkills() != null)
                {
                    foreach (var v in aIS.GetSkills())
                    {
                        if(v.Get().script != null)
                        {
                            foreach(var s in v.Get().script)
                            {
                                if( s.triggerType == ESkillType.AttributeChange)
                                {
                                    if (!string.IsNullOrEmpty(s.trigger))
                                    {
                                        bool execute = (bool)ScriptLibrary.Call(s.trigger, aIS, null, v.Get(), s, finalDict);
                                        if (!execute) continue;
                                    }

                                    var change = ScriptLibrary.Call(s.activatorMain, aIS, v.Get(), s, finalDict);
                                }
                            }
                        }                      
                    }                    
                }
            }
            if(a is IEnchantable)
            {
                var aIS = a as IEnchantable;
                aIS.TriggerScripts(EEnchantmentType.AttributeChange, finalDict);
                if (bbu != null)
                {
                    b?.GetPlayer(bbu.attackingSide)?.TriggerScripts(EEnchantmentType.RemoteUnitAttributeChange, finalDict, aIS);
                    b?.TriggerScripts(EEnchantmentType.RemoteUnitAttributeChange, finalDict, aIS);
                }
                else
                {
                    MOM.Unit u = bu as MOM.Unit;
                    if(u != null && u.group != null)
                    {
                        u.group.Get().GetLocationHostSmart()?.TriggerScripts(EEnchantmentType.RemoteUnitAttributeChange, finalDict, aIS);
                    }

                    bu?.GetWizardOwner()?.TriggerScripts(EEnchantmentType.RemoteUnitAttributeChange, finalDict, aIS);
                    GameManager.Get().TriggerScripts(EEnchantmentType.RemoteUnitAttributeChange, finalDict, aIS);
                }             

                aIS.TriggerScripts(EEnchantmentType.AttributeChangeMP, finalDict);
                if (bbu != null)
                {
                    b?.GetPlayer(bbu.attackingSide)?.TriggerScripts(EEnchantmentType.RemoteUnitAttributeChangeMP, finalDict, aIS);
                    b?.TriggerScripts(EEnchantmentType.RemoteUnitAttributeChangeMP, finalDict, aIS);
                }
                else
                {
                    MOM.Unit u = bu as MOM.Unit;
                    if (u != null && u.group != null)
                    {
                        u.group.Get().GetLocationHostSmart()?.TriggerScripts(EEnchantmentType.RemoteUnitAttributeChangeMP, finalDict, aIS);
                    }

                    bu?.GetWizardOwner()?.TriggerScripts(EEnchantmentType.RemoteUnitAttributeChangeMP, finalDict, aIS);
                    GameManager.Get().TriggerScripts(EEnchantmentType.RemoteUnitAttributeChangeMP, finalDict, aIS);
                }
            }

            List<Tag> tagsToZero = null;
            foreach(var v in finalDict)
            {
                if(v.Value < FInt.ZERO && !v.Key.Get().canGoNegative)
                {
                    if(tagsToZero == null) tagsToZero = new List<Tag>();
                    tagsToZero.Add(v.Key.Get());
                }
            }

            if (tagsToZero != null)
            {
                foreach(var v in tagsToZero)
                {
                    finalDict.SetFinal(v, FInt.ZERO);
                }
            }

            if(bbu != null)
            {
                BattleHUD.Get()?.SetUnitDirty(bbu);
            }

            return finalDict;
        }
        /// <summary>
        /// Produce estimated value of the modified unit in relation to her own base value
        /// This allows for a unit to use its simulated value which uses skills and other details to be called by 
        /// some most obvious modifiers (attack, defence and being damaged)
        /// 
        /// This method is called each time attributes become dirty
        /// </summary>
        /// <param name="bu">unit in question</param>
        /// <returns></returns>
        static public int GetBattleUnitValue(BattleUnit bu)
        {
            return GetModifiedBattleUnitValue(bu, null, FInt.ZERO);
        }
        static public int GetModifiedBattleUnitValue(BattleUnit bu, Tag t0, FInt modifier)
        {
            int value = BaseUnit.GetUnitStrength(bu.dbSource.Get());
            var a = bu.GetAttributes();
            var sA = bu.GetUnitSourceSampleAttributes();

            return GetModifiedValue(value, a, sA, t0, modifier, bu.GetMaxFigureCount(), bu.figureCount, bu.currentFigureHP);
        }
        static public int GetWorldUnitValue(MOM.Unit u)
        {
            return GetModifiedWorldUnitValue(u, null, FInt.ZERO);
        }
        static public int GetModifiedWorldUnitValue(MOM.Unit u, Tag t0, FInt modifier)
        {
            int value = BaseUnit.GetUnitStrength(u.dbSource.Get());
            var a = u.GetAttributes();
            var sA = u.GetUnitSourceSampleAttributes();

            return GetModifiedValue(value, a, sA, t0, modifier, u.MaxCount(), u.figureCount, u.currentFigureHP);
        }
        static public int GetModifiedValueFixedHP(BattleUnit bu, float curentTotalHP)
        {
            int value = BaseUnit.GetUnitStrength(bu.dbSource.Get());
            var a = bu.GetAttributes();
            var sA = bu.GetUnitSourceSampleAttributes();
            value = GetModifiedValueFixedHP(value, a, sA, null, FInt.ZERO, bu.GetMaxFigureCount(), curentTotalHP);
            return value;
        }
        static float GetBaseModifiedValue(int origValue, Attributes a, Attributes sA, Tag t0, FInt modifier, int maxCount)
        {
            Tag t = (Tag)TAG.MELEE_ATTACK;
            Tag t2 = (Tag)TAG.MELEE_ATTACK_CHANCE;

            FInt c1 = a.GetFinal(t) + (t0 == t ? modifier : FInt.ZERO);
            FInt c2 = a.GetFinal(t2) + (t0 == t2 ? modifier : FInt.ZERO);

            float figureScalar = (maxCount + 5) / 10.0f;

            float NSIV = 250f;
            //NSVI - Normalized stat increase value
            //Unit gains NSVI strategic points per 1 full attack point gained
            //1 full attack means that unit gained about 3 attack at 0.3 chance to hit etc
            //this is further scaled by number of figures where
            //single figure gains only 60% of the bonus
            //5 figures gain 100% of the bonus
            //9 figures gain 140% of the bonus

            FInt delta = c1 * c2 - sA.GetFinal(t) * sA.GetFinal(t2);
            float valueChange = NSIV * delta.ToFloat() * figureScalar;

            t = (Tag)TAG.RANGE_ATTACK;
            t2 = (Tag)TAG.RANGE_ATTACK_CHANCE;
            c1 = a.GetFinal(t) + (t0 == t ? modifier : FInt.ZERO);
            c2 = a.GetFinal(t2) + (t0 == t2 ? modifier : FInt.ZERO);

            FInt ammo = a.GetFinal(TAG.AMMUNITION);
            if (t0 == (Tag)TAG.AMMUNITION && modifier + ammo <= FInt.ZERO) ammo = FInt.ZERO;

            delta = c1 * c2 * (ammo > 0 ? 1 : 0) - sA.GetFinal(t) * sA.GetFinal(t2);
            valueChange += NSIV * delta.ToFloat() * figureScalar;

            t = (Tag)TAG.DEFENCE;
            t2 = (Tag)TAG.DEFENCE_CHANCE;
            c1 = a.GetFinal(t) + (t0 == t ? modifier : FInt.ZERO);
            c2 = a.GetFinal(t2) + (t0 == t2 ? modifier : FInt.ZERO);

            delta = c1 * c2 - sA.GetFinal(t) * sA.GetFinal(t2);
            valueChange += NSIV * delta.ToFloat() * figureScalar;

            t = (Tag)TAG.RESIST;
            c1 = a.GetFinal(t) + (t0 == t ? modifier : FInt.ZERO);

            delta = c1 - sA.GetFinal(t);
            valueChange += NSIV * .5f * delta.ToFloat() * figureScalar;
            return valueChange;
        }        
        static int GetModifiedValueFixedHP(int origValue, Attributes a, Attributes sA, Tag t0, FInt modifier, int maxCount, float curentTotalHP)
        {
            var valueChange = GetBaseModifiedValue(origValue, a, sA, t0, modifier, maxCount);

            return (int)((curentTotalHP * 0.8f + 0.2f) * (origValue + valueChange));
        }
        static int GetModifiedValue(int origValue, Attributes a, Attributes sA, Tag t0, FInt modifier, int maxCount, int curentCount, int curentHP)
        {
            //calculate unit value due to its current figure status            
            if (curentCount == 0) return 0;
            float oneFigure = 1f / maxCount;
            
            int maxHP = a.GetFinal((Tag)TAG.HIT_POINTS).ToInt();

            float allButLast = (curentCount - 1);
            //alive figure is worth minimum of 1/2 of its value
            float lastFigure = 0.5f * (curentHP / (float)maxHP) + 0.5f;
            //build value between 0-1 based on the current figure count and status
            float curentSquadHP = allButLast * oneFigure + lastFigure * oneFigure;

            return GetModifiedValueFixedHP(origValue, a, sA, t0, modifier, maxCount, curentSquadHP);            
        }
        #endregion

        #region Group Methods

        public static FInt CalculateGroupMaxMp(List<Reference<MOM.Unit>> units, Reference<MOM.Unit> transporter)
        {
            FInt max = FInt.MAX ;
            if (transporter != null)
            {
                var transportMaxMp = transporter.Get().GetMaxMP();

                // Wind Mastery spell work only on ships and they all have transport ability.
//                 if (transporter.Get().GetAttFinal(TAG.WIND_MASTERY_POSITIVE) > 0)
//                     return transportMaxMp * 1.5f;
//                 else if (transporter.Get().GetAttFinal(TAG.WIND_MASTERY_NEGATIVE) > 0)
//                     return transportMaxMp / 2;
//                 else
                    return transportMaxMp;
            }

            foreach (var v in units)
            {
                max = FInt.Min(max, v.Get().GetMaxMP());
            }
            
            if (max == int.MaxValue) return FInt.ZERO;
            return max;
        }

        #endregion

        #region Counter Magic
        public static bool CounterMagicNightShade(TownLocation town, Spell spell, PlayerWizard spellCaster)
        {
            var enchShrine= DataBase.Get<DBDef.Enchantment>(ENCH.SHRINE, false);
            var enchSagesGuild = DataBase.Get<DBDef.Enchantment>(ENCH.SAGES_GUILD, false);

            foreach (var res in town.GetResources())
            {
                if (res == (DBDef.Resource)DBEnum.RESOURCE.NIGHTSHADE)
                {
                    foreach (var cm in town.GetEnchantments())
                    {
                        if (enchShrine == cm.source)
                        {
                            if (DispellWorldSpell(enchShrine, spell, spellCaster))
                                return true;
                        }
                        else if (enchSagesGuild == cm.source)
                        {
                            if (DispellWorldSpell(enchSagesGuild, spell, spellCaster))
                                return true;
                        }
                    }
                }
            }


            return false;

        }
        public static bool CounterMagicWorld(Spell spell, PlayerWizard spellCaster)
        {
            var gameMenager = GameManager.Get();
            if (gameMenager.worldCounterMagic > 0)
            {

                var enchSuppressMagic = DataBase.Get<DBDef.Enchantment>(ENCH.SUPPRESS_MAGIC, false);
                var enchLifeForce = DataBase.Get<DBDef.Enchantment>(ENCH.LIFE_FORCE, false);
                var enchTranqulity = DataBase.Get<DBDef.Enchantment>(ENCH.TRANQUILITY, false);

                foreach (var cm in gameMenager.GetEnchantments())
                {
                    if (enchSuppressMagic == cm.source)
                    {
                        if (DispellWorldSpell(enchSuppressMagic, spell, spellCaster))
                            return true;
                    }
                    else if (enchLifeForce == cm.source && spell.realm == ERealm.Death)
                    {
                        if (DispellWorldSpell(enchLifeForce, spell, spellCaster))
                            return true;
                    }
                    else if (enchTranqulity == cm.source && spell.realm == ERealm.Chaos)
                    {
                        if (DispellWorldSpell(enchTranqulity, spell, spellCaster))
                            return true;
                    }
                }
            }
            
            return false;

        }
        public static bool CounterMagicBattle(Battle battle, Spell spell, ISpellCaster spellCaster)
        {
            var playerWizard = spellCaster is BattleUnit ? (spellCaster as BattleUnit).GetWizardOwner() :  spellCaster as PlayerWizard;
            
            if (battle != null && battle.gDefender != null &&
                battle.gDefender.GetLocationHost() != null &&
                battle.gDefender.GetLocationHost().locationType == ELocationType.Node )
            {
                if (playerWizard != null && playerWizard.GetTraits().Find(o => o == (Trait)TRAIT.NODE_MASTERY) != null)
                {
                    return false;
                }

            }
            if (battle != null && battle.battleCounterMagic > 0)
            {
                var enchCounterMagic = DataBase.Get<DBDef.Enchantment>(ENCH.COUNTER_MAGIC, false);
                var enchCounterMagicNodeChaos = DataBase.Get<DBDef.Enchantment>(ENCH.MAGIC_NODE_CHAOS_COUNTER_MAGIC, false);
                var enchCounterMagicNodeNature = DataBase.Get<DBDef.Enchantment>(ENCH.MAGIC_NODE_NATURE_COUNTER_MAGIC, false);
                var enchCounterMagicNodeSorcery = DataBase.Get<DBDef.Enchantment>(ENCH.MAGIC_NODE_SORCERY_COUNTER_MAGIC, false);

                foreach (var cm in battle.GetEnchantments())
                {
                    if (enchCounterMagic == cm.source)
                    {
                        if (DispellBattleSpell(battle, enchCounterMagic, spell, spellCaster))
                            return true;
                    }
                    //node will not dispel magic from same realm
                    else if (spell.realm != cm.source.Get().realm)
                    {
                        if (enchCounterMagicNodeChaos == cm.source)
                        {
                            if (DispellBattleSpell(battle, enchCounterMagicNodeChaos, spell, spellCaster))
                                return true;
                        }
                        if (enchCounterMagicNodeNature == cm.source)
                        {
                            if (DispellBattleSpell(battle, enchCounterMagicNodeNature, spell, spellCaster))
                                return true;
                        }
                        if (enchCounterMagicNodeSorcery == cm.source)
                        {
                            if (DispellBattleSpell(battle, enchCounterMagicNodeSorcery, spell, spellCaster))
                                return true;
                        
                        }
                    }
                }
            }
            return false;
        }

        private static bool DispellWorldSpell(Enchantment dispellEnch, Spell spell, PlayerWizard spellCaster)
        {
            if (spell == null || dispellEnch == null || spellCaster == null) return false;

            float suppressStr = dispellEnch.scripts[0].fIntData.ToFloat();
            float chance = suppressStr / (suppressStr + (float)spell.GetWorldCastingCost(spellCaster, true));
            var spellCounteredSucceses = new MHRandom().GetSuccesses(chance, 1);
            var countereMagicEnchs = GameManager.Get().GetEnchantments().FindAll(o => o.source == dispellEnch);

            if (spellCounteredSucceses > 0 && 
                (countereMagicEnchs.Find(o => o.owner == null && spellCaster != null) != null ||
                countereMagicEnchs.Find(o => o.owner.ID != spellCaster.ID) != null))
            {
                if (PlayerWizard.HumanID() == spellCaster.ID)
                {
                    PopupGeneral.OpenPopup(null, "UI_COUNTER_MAGIC", "UI_SPELL_COUNTERED", "UI_UHH");
                }
                return true;
            }
            return false;
        }
        private static bool DispellBattleSpell(Battle battle, Enchantment dispellingEnch, Spell spell, ISpellCaster spellCaster)
        {
            if (battle == null || dispellingEnch == null || spell == null || spellCaster == null) return false;

            float suppressStr = dispellingEnch.scripts[0].fIntData.ToFloat();
            float chance = suppressStr / (suppressStr + (float)spell.GetBattleCastingCostByDistance(spellCaster, true));
            var spellCounteredSucceses = new MHRandom().GetSuccesses(chance, 1);
            var suppressMagicEnchs = battle.GetEnchantments().FindAll(o => o.source == dispellingEnch);

            if (spellCounteredSucceses > 0)
            {
                foreach (var ench in suppressMagicEnchs)
                {
                    //That code block search for situations where spell/ench is allow to cast
                    if (ench.owner == null && spellCaster == null) return false;
                    if(ench.owner != null && spellCaster != null)
                    {
                        if (ench.owner.GetEntity() is PlayerWizard && 
                           (ench.owner.GetEntity() as PlayerWizard).GetID() == spellCaster.GetWizardOwnerID())
                        {
                            return false;
                        }
                        else if (ench.owner.GetEntity() is BattleUnit &&
                            (ench.owner.GetEntity() as BattleUnit).GetWizardOwnerID() == spellCaster.GetWizardOwnerID())
                        {
                            return false;
                        }

                    }
                }
                if (PlayerWizard.HumanID() == spellCaster.GetWizardOwnerID())
                {
                    PopupGeneral.OpenPopup(null, "UI_COUNTER_MAGIC", "UI_SPELL_COUNTERED", "UI_UHH");
                }
                return true;
            } 
            return false;
        }

        #endregion

        #region Budget, Treasury

        public static int LocationPower(WorldCode.Plane p, DBDef.Location source)
        {
            int power = 0;
            if (source.locationType != ELocationType.Node) { return power; }

            MHRandom r = new MHRandom();
            var magicNode = source as MagicNode;

            if (p.planeSource.Get() == (DBDef.Plane)DBEnum.PLANE.ARCANUS)
                power = r.GetInt(magicNode.powerRange.minimumCount, magicNode.powerRange.maximumCount);
            else
                power = r.GetInt(magicNode.powerRange.minimumCount * 2, magicNode.powerRange.maximumCount * 2);

            return power;
        }

        public static int LocationBudget(WorldCode.Plane p, DBDef.Location source, int power)
        {
            int budget = 0;
            var lType = source.locationType;
            int random;
            MHRandom r = new MHRandom();

            switch (lType)
            {
                //Budget Range Arcanus 125-1500, Myrror 500-5600
                //Power on Arcanus is 5 - 10, Power on Myrror is 10 - 20
                case ELocationType.Node:
                    random = r.GetInt(6, 15);
                    budget = random * (int)Mathf.Pow(power, 2);
                    break;
                //Budget Range Arcanus 70-220, Myrror 70-350
                case ELocationType.WeakWaterLair:
                case ELocationType.WeakLair:
                    if (p.planeSource.Get() == (DBDef.Plane)DBEnum.PLANE.ARCANUS)
                    {
                        random = r.GetInt(7, 22);
                    }
                    else
                    {
                        random = r.GetInt(7, 35);
                    }
                    budget = random * 10;
                    break;
                //Budget Range Arcanus 550-1150, Myrror 650-1750
                case ELocationType.WaterLair:
                case ELocationType.Lair:
                    if (p.planeSource.Get() == (DBDef.Plane)DBEnum.PLANE.ARCANUS)
                    {
                        random = r.GetInt(5, 12);
                        budget = random * 100 + 50;
                    }
                    else
                    {
                        random = r.GetInt(6, 17);
                        budget = random * 100 + 50;
                    }
                    break;
                //Budget Range Arcanus 900-2050, Myrror 1550-3250
                case ELocationType.StrongWaterLair:
                case ELocationType.StrongLair:
                    if (p.planeSource.Get() == (DBDef.Plane)DBEnum.PLANE.ARCANUS)
                    {
                        random = r.GetInt(17, 40);
                        budget = random * 50 + 50;
                    }
                    else
                    {
                        random = r.GetInt(15, 32);
                        budget = random * 100 + 50;
                    }
                    break;
                //Budget Range Arcanus 750-4250, Myrror 750-4750
                case ELocationType.Ruins:
                    if (p.planeSource.Get() == (DBDef.Plane)DBEnum.PLANE.ARCANUS)
                    {
                        random = r.GetInt(10, 80);
                    }
                    else
                    {
                        random = r.GetInt(10, 90);
                    }
                    budget = random * 50 + 250;
                    break;
                //Budget Range 850-1650
                case ELocationType.PlaneTower:
                    random = r.GetInt(2, 11);
                    budget = random * 100 + 650;
                    break;
                case ELocationType.MidGameLair:
                    budget = 2000;
                    break;
                default:
                    Debug.LogWarning("None Type Location created.");
                    budget = 2000;
                    break;
            }

            return budget;
        }
        public static Treasure LocationTreasure(MOM.Location location, int treasureBudget, Race guardianRealm, bool allowTown = false)
        {
            if (location is TownLocation && !allowTown) return null;

            //Prize cost
            int goldPrize = 10;
            int manaPrize = 10;
            int artifactCommonPrize = 325;
            int artifactUncommonPrize = 1000;
            int artifactVUncommonPrize = 2000;
            int artifactRarePrize = 3000;
            int artifactVRarePrize = 8000;
            int artifactVVRarePrize = 12000;
            //Hero Prize is mid range.
            int heroPrize = 950;
            int spellCommonPrize = 150;
            int spellUncommonPrize = 300;
            int spellRarePrize = 550;
            int spellVRarePrize = 800;
            int specialPrize = 950;

            Treasure treasure = new Treasure();
            var tb = treasureBudget;

            int gold = 0;
            int mana = 0;
            bool hero = false;
            bool commonSpell = false;
            bool uncommonSpell = false;
            bool rareSpell = false;
            bool veryRareSpell = false;
            bool special = false;
            bool doubleSpecial = false;
            int[] artifactBudgets = new int[3];
            int artifactFullSlots = 0;


            bool planeTowerSpellReward = false;

            while (tb >= 50)
            {
                float chance = Random.Range(0f, 1f);

                //If treasury is counted for PlaneTower make sure it got spell
                if (location.locationType == ELocationType.PlaneTower
                    && planeTowerSpellReward == false)
                {
                    chance = 0.93f;
                    planeTowerSpellReward = true;
                }

                if (chance <= 0.2f
                    && tb >= goldPrize) // chance 3/15 
                {
                    //Gold
                    float goldChance = Random.Range(0f, 1f);

                    if (goldChance <= 0.2 && tb >= goldPrize * 20)
                    {
                        tb -= goldPrize * 20;
                        gold += goldPrize * 20;
                    }
                    else if (goldChance <= 0.4 && tb >= goldPrize * 10)
                    {
                        tb -= goldPrize * 10;
                        gold += goldPrize * 10;
                    }
                    else if (goldChance <= 0.6 && tb >= goldPrize * 6)
                    {
                        tb -= goldPrize * 6;
                        gold += goldPrize * 6;
                    }
                    else if (goldChance <= 0.8 && tb >= goldPrize * 3)
                    {
                        tb -= goldPrize * 3;
                        gold += goldPrize * 3;
                    }
                    else if (goldChance <= 1.0)
                    {
                        tb -= goldPrize;
                        gold += goldPrize;
                    }
                }
                else if (chance <= 0.4 && tb >= manaPrize) // chance 3/15 
                {
                    //Mana
                    float manaChance = Random.Range(0f, 1f);

                    if (manaChance <= 0.2 && tb >= manaPrize * 20)
                    {
                        tb -= manaPrize * 20;
                        mana += manaPrize * 20;
                    }
                    else if (manaChance <= 0.4 && tb >= manaPrize * 10)
                    {
                        tb -= manaPrize * 10;
                        mana += manaPrize * 10;
                    }
                    else if (manaChance <= 0.6 && tb >= manaPrize * 6)
                    {
                        tb -= manaPrize * 6;
                        mana += manaPrize * 6;
                    }
                    else if (manaChance <= 0.8 && tb >= manaPrize * 3)
                    {
                        tb -= manaPrize * 3;
                        mana += manaPrize * 3;
                    }
                    else if (manaChance <= 1.0)
                    {
                        tb -= manaPrize;
                        mana += manaPrize;
                    }
                }
                else if (chance <= 0.6 && artifactFullSlots < 3 && tb >= artifactCommonPrize) // chance 4/15 
                {
                    //Artefact
                    for (int i = 0; i < artifactBudgets.Length; i++)
                    {
                        if (artifactBudgets[i] != 0) continue;

                        float artefactChance = Random.Range(0f, 1f);
                        //If artifactFullSlots == 3, slots are full to not enter artefact budget creation.
                        artifactFullSlots++;

                        if (artefactChance <= 0.1 && tb >= artifactVVRarePrize)
                        {
                            // Very Rare Artefact
                            tb -= artifactVVRarePrize;
                            artifactBudgets[i] = artifactVVRarePrize;
                            if (artifactBudgets[i] != 0) break;
                        }
                        else if (artefactChance <= 0.2 && tb >= artifactVRarePrize)
                        {
                            // Rare Artefact
                            tb -= artifactVRarePrize;
                            artifactBudgets[i] = artifactVRarePrize;
                            if (artifactBudgets[i] != 0) break;
                        }
                        else if (artefactChance <= 0.3 && tb >= artifactRarePrize)
                        {
                            // Rare Artefact
                            tb -= artifactRarePrize;
                            artifactBudgets[i] = artifactRarePrize;
                            if (artifactBudgets[i] != 0) break;
                        }
                        else if (artefactChance <= 0.5 && tb >= artifactVUncommonPrize)
                        {
                            // Rare Artefact
                            tb -= artifactVUncommonPrize;
                            artifactBudgets[i] = artifactVUncommonPrize;
                            if (artifactBudgets[i] != 0) break;
                        }
                        else if (artefactChance <= 0.7 && tb >= artifactUncommonPrize)
                        {
                            // Uncommon Artefact
                            tb -= artifactUncommonPrize;
                            artifactBudgets[i] = artifactUncommonPrize;
                            if (artifactBudgets[i] != 0) break;
                        }
                        else if (artefactChance <= 1.0)
                        {
                            tb -= artifactCommonPrize;
                            artifactBudgets[i] = artifactCommonPrize;
                            if (artifactBudgets[i] != 0) break;
                        }
                    }

                }
                else if (chance <= 0.70 && tb >= heroPrize) 
                {
                    //Hero
                    if (hero) continue;

                    tb -= heroPrize;
                    hero = true;
                }
                else if (chance <= 0.9 && tb >= spellCommonPrize)  
                {
                    //Spell
                    if (special || commonSpell || uncommonSpell || rareSpell || veryRareSpell || doubleSpecial) continue;

                    float spellChance = Random.Range(0f, 1f);
                    if (spellChance <= 0.1 && tb >= spellVRarePrize)
                    {
                        // Very Rare Spell
                        tb -= spellVRarePrize;
                        veryRareSpell = true;
                    }
                    else if (spellChance <= 0.3 && tb >= spellRarePrize)
                    {
                        // Rare spell
                        tb -= spellRarePrize;
                        rareSpell = true;
                    }
                    else if (spellChance <= 0.6 && tb >= spellUncommonPrize)
                    {
                        // Uncommon Spell
                        tb -= spellUncommonPrize;
                        uncommonSpell = true;
                    }
                    else if (spellChance <= 1.0)
                    {
                        tb -= spellCommonPrize;
                        commonSpell = true;
                    }
                }
                else if (chance <= 1.0f && tb >= specialPrize) 
                {
                    //Special
                    //if (commonSpell || uncommonSpell || rareSpell || veryRareSpell || doubleSpecial) continue;

                    tb -= specialPrize;
                    if (special)
                        doubleSpecial = true;
                    else
                        special = true;
                }
            }

            treasure.goldMana = new int[] { gold, mana };
            treasure.artefacts = new int[] { artifactBudgets[0], artifactBudgets[1], artifactBudgets[2] };
            treasure.heroSpellSpecial = new bool[] { hero, commonSpell, uncommonSpell, rareSpell, veryRareSpell, special, doubleSpecial };
            treasure.guardianRealm = guardianRealm;

            return treasure;
        }


        static public void ClaimAward(PlayerWizard wizard, MOM.Location location, IGroup iGroup, List<KeyValuePair<MOM.Unit, IGroup>> heroes, Treasure treasure = null)
        {
            Treasure award;
            if(treasure != null)
            {
                award = treasure;
            }
            else
            {
                award = LocationTreasure(location, location.budget, location.guardianRace);
            }

            if (award != null && award.goldMana[0] == 0 && award.goldMana[1] == 0 &&
                award.artefacts[0] == 0 && award.artefacts[1] == 0
                && award.artefacts[2] == 0 && award.heroSpellSpecial[0] == false &&
                award.heroSpellSpecial[1] == false)
            {
                float chance = Random.Range(0f, 1f);
                if (chance <= 0.5f)
                {
                    int value = Random.Range(10, 20);
                    award.goldMana[0] += value;
                }
                else
                {
                    int value = Random.Range(10, 20);
                    award.goldMana[1] += value;
                }

            }

            if (award.goldMana[0] > 0)
            {
                wizard.money += award.goldMana[0];
            }
            if (award.goldMana[1] > 0)
            {
                wizard.mana += award.goldMana[1];
            }
            if (award.artefacts [0] > 0)
            {
                //add an Artefact
                var artefact = MOM.Artefact.CraftRandomByBudget(award.artefacts[0]);
                if (artefact != null)
                {
                    wizard.artefacts.Add(artefact);
                }
            }
            if (award.artefacts[1] > 0)
            {
                //add an Artefact
                var artefact = MOM.Artefact.CraftRandomByBudget(award.artefacts[1]);
                if (artefact != null)
                {
                    wizard.artefacts.Add(artefact);
                }
            }
            if (award.artefacts[2] > 0)
            {
                //add an Artefact
                var artefact = MOM.Artefact.CraftRandomByBudget(award.artefacts[2]);
                if (artefact != null)
                {
                    wizard.artefacts.Add(artefact);
                }
            }
            if (award.heroSpellSpecial[0])
            {
                if (wizard.heroes != null && wizard.heroes.Count >= wizard.GetMaxHeroCount())
                {
                    if (award.heroSpellSpecial[5] == false)
                    {
                        award.heroSpellSpecial[5] = true;
                    }
                    else if(award.heroSpellSpecial[4] == false)
                    {
                        award.heroSpellSpecial[4] = true;
                    }
                    else if (award.heroSpellSpecial[3] == false)
                    {
                        award.heroSpellSpecial[3] = true;
                    }
                }
                else
                {
                    // add a Hero
                    List<Hero> heroesList = new List<Hero>(DataBase.GetType<Hero>());
                    heroesList = heroesList.FindAll(o => !o.champion && !MOM.Unit.HeroInUseByWizard(o, wizard.GetID()) && o.GetTag(TAG.EVENT_ONLY_UNIT) == 0);
                    heroesList.RandomSort();

                    for (int i = 0; i < heroesList.Count; i++)
                    {
                        MOM.Unit u = MOM.Unit.CreateFrom(heroesList[i]);
                        if(heroes != null)
                        {
                            heroes.Add(new KeyValuePair<MOM.Unit, IGroup>(u, iGroup));
                        }
                        else
                        {
                            iGroup.AddUnit(u);
                        }
                        wizard.ModifyUnitSkillsByTraits(u);
                        break;
                    }
                }

            }
            if (award.heroSpellSpecial[1] || award.heroSpellSpecial[2] || award.heroSpellSpecial[3] || award.heroSpellSpecial[4])
            {
                //add a Spell
                List<Spell> spellList = new List<Spell>(DataBase.GetType<Spell>());
                ERarity rarity = ERarity.Common;
                if (award.heroSpellSpecial[1]) rarity = ERarity.Common;
                if (award.heroSpellSpecial[2]) rarity = ERarity.Uncommon;
                if (award.heroSpellSpecial[3]) rarity = ERarity.Rare;
                if (award.heroSpellSpecial[4]) rarity = ERarity.VeryRare;

                MagicAndResearch mar = wizard.GetMagicAndResearch();
                List<MagicUnlocks> limits = mar.GetUnlockLimits();
                bool spellGranted = false;
                bool spellOneRarityUp = false;

                List<Spell> spells = new List<Spell>();
                
                while (true)
                {
                    spells = UpdateSpellList(spells, spellList, rarity, wizard, limits, mar);

                    if (spells.Count > 0 )
                    {
                        var index = Random.Range(0, spells.Count);
                        wizard.AddSpell(spells[index]);
                        spellGranted = true;
                        break;
                    }
                    else if (spellOneRarityUp)
                    {
                        break;
                    }
                    else if(!spellOneRarityUp)
                    {
                        if (rarity == ERarity.Common)
                            rarity = ERarity.Uncommon;
                        else if (rarity == ERarity.Uncommon)
                            rarity = ERarity.Rare;
                        else if (rarity == ERarity.Rare)
                            rarity = ERarity.VeryRare;

                        spells = UpdateSpellList(spells, spellList, rarity, wizard, limits, mar);

                        if (spells.Count > 0)
                            spellOneRarityUp = true;
                        else
                            break;
                    }
                }                    
                //If player receive spell but there is no more spells to hive him as a reward.
                if (!spellGranted)
                {
                    if (award.heroSpellSpecial[1])
                    {
                        wizard.money += Random.Range(1, 50);
                        wizard.mana += Random.Range(1, 50);
                    }
                    else if (award.heroSpellSpecial[2])
                    {
                        wizard.money += Random.Range(1, 100);
                        wizard.mana += Random.Range(1, 100);
                    }
                    else if (award.heroSpellSpecial[3])
                    {
                        wizard.money += Random.Range(1, 150);
                        wizard.mana += Random.Range(1, 150);
                    }
                    else if (award.heroSpellSpecial[4])
                    {
                        wizard.money += Random.Range(1, 200);
                        wizard.mana += Random.Range(1, 200);
                    }
                }
            }
            if (award.heroSpellSpecial[5])
            {
                //add a Special
                if (!award.heroSpellSpecial[6]) //single Special
                {
                    float chance = Random.Range(0f, 1f);
                    if (chance < 0.74f)
                    {
                        //Spellbook selected
                        var att = wizard.GetAttributes();

                        int spellbooksCount = wizard.GetWizardSpellbooksCount();

                        if (spellbooksCount < 13)
                        {
                            // player don't have max amount of Spellbooks
                            var books = DataBase.GetType<Tag>();
                            books = books.FindAll(o => o.parent == (Tag)TAG.MAGIC_BOOK);

                            if (att.Contains(TAG.LIFE_MAGIC_BOOK))
                            {
                                books.Remove((Tag)TAG.DEATH_MAGIC_BOOK);
                            }
                            else if (att.Contains(TAG.DEATH_MAGIC_BOOK))
                            {
                                books.Remove((Tag)TAG.LIFE_MAGIC_BOOK);
                            }

                            books.Remove((Tag)TAG.ARCANE_BOOK);
                            books.Remove((Tag)TAG.TECH_BOOK);

                            books.RandomSort();
                            if (award.guardianRealm != null)
                            {
                                Tag MagicBookTag;
                                raceToMagicBookTag.TryGetValue(award.guardianRealm, out MagicBookTag);
                                var chosenBook = books.Find(o => o == MagicBookTag);

                                if (chosenBook != null)
                                {
                                    wizard.AddBook(chosenBook, FInt.ONE);

                                }
                                else
                                {
                                    wizard.AddBook(books[0], FInt.ONE);
                                }
                            }
                            else
                            {
                                wizard.AddBook(books[0], FInt.ONE);
                            }
                        }
                        else
                        {
                            // Artefact instead of Spellbook
                            wizard.artefacts.Add(MOM.Artefact.RandomFactory(2000));
                        }
                    }
                    else
                    {
                        //Trait selected
                        var traits = DataBase.GetType<Trait>();
                        traits = traits.FindAll(o => o.cost == 1 && !wizard.HasTrait(o) && !o.rewardExclusion);

                        if (traits.Count > 0 && wizard.GetTraitsCount() < 6)
                        {
                            AddTrait(traits, wizard);

                        }
                        // Artefact instead of Trait
                        else
                        {
                            wizard.artefacts.Add(MOM.Artefact.RandomFactory(2000));
                        }
                    }
                }
                else // double Special
                {
                    var traits = DataBase.GetType<Trait>();
                    traits = traits.FindAll(o => o.cost == 2 && !wizard.HasTrait(o) && !o.rewardExclusion);

                    if (traits.Count > 0 && wizard.GetTraitsCount() < 6)
                    {
                        AddTrait(traits, wizard);
                    }
                    // Artefact instead of Trait
                    else
                    {
                        wizard.artefacts.Add(MOM.Artefact.RandomFactory(4000));
                    }
                }
            }
            if (!(wizard is PlayerWizardAI))
            {
                HUD.Get()?.UpdateHUD();
            }
        }

        public static void GenerateRazeReward(TownLocation town, PlayerWizard wizard, IGroup attackerGroup)
        {
            int baseValue = 175;
            int perPopIncrease = 5;
            int popValue = town.GetPopUnits();
            int budget = 0;

            budget = baseValue + (((popValue * (popValue + 1)) / 2) -1) * perPopIncrease;

            Treasure award = LocationTreasure(town, budget, town.race, true);

            ClaimAward(wizard, null, attackerGroup, null, award);

            MOM.Unit unit = MOM.Unit.CreateFrom((Subrace)UNIT.B_ORC_LEASHED);
            wizard.ModifyUnitSkillsByTraits(unit);
            attackerGroup.AddUnit(unit);
            unit.UpdateMP();
        }

        static private Dictionary<Race, Tag> raceToMagicBookTag = new Dictionary<Race, Tag>
                                                         {
                                                             { (Race)RACE.REALM_CHAOS, (Tag)TAG.CHAOS_MAGIC_BOOK },
                                                             { (Race)RACE.REALM_DEATH, (Tag)TAG.DEATH_MAGIC_BOOK },
                                                             { (Race)RACE.REALM_LIFE, (Tag)TAG.LIFE_MAGIC_BOOK },
                                                             { (Race)RACE.REALM_NATURE, (Tag)TAG.NATURE_MAGIC_BOOK },
                                                             { (Race)RACE.REALM_SORCERY, (Tag)TAG.SORCERY_MAGIC_BOOK }
                                                         };


        private static void AddTrait(List<Trait> traits, PlayerWizard wizard)
        {
            traits.RandomSort();

            for (int i = traits.Count - 1; i >= 0; i--)
            {
                if (!string.IsNullOrWhiteSpace(traits[i].prerequisiteScript))
                {
                    if(!(bool)ScriptLibrary.Call(traits[i].prerequisiteScript, wizard.GetAttributes(), wizard.GetTraits()))
                    {
                        traits.RemoveAt(i);
                    }
                }
            }
            if (traits.Count > 0)
            {
                wizard.AddTrait(traits[0]);
            }
            else
            {
                wizard.artefacts.Add(MOM.Artefact.RandomFactory(2000));
            }
        }

        private static List<Spell> UpdateSpellList(List<Spell> spells, List<Spell> spellList, ERarity rarity, PlayerWizard wizard, List<MagicUnlocks> limits, MagicAndResearch mar)
        {
            spells = spellList.FindAll(o => o.rarity == rarity &&
              (o.realm == ERealm.Arcane ||
              o.realm == ERealm.Tech && wizard.traitTechMagic ||
              limits.FindIndex(k => k.realm == o.realm &&
                            k.booksAdvantages != null &&
                            (int)k.booksAdvantages.Get().rewardLimit >= (int)o.rarity) > -1) &&
              o.treasureExclude == false &&
              o.researchExclusion == false);

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

            return spells;
        }

        #endregion
    }
}
#endif