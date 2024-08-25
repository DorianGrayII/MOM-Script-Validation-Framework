#if !USE_DEBUG_SCRIPT || !UNITY_EDITOR

using DBDef;
using DBEnum;
using MHUtils;
using MOM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Group = MOM.Group;

namespace MOMScripts
{
    public class TraitScripts : ScriptBase
    {
        #region Prerequisit
        /// <param name="wizard"> The wizard wishing to aquire the trait </param>
        /// <returns> bool - true if wizard passes prerequisits. </returns>
        static public bool TPRE_Archmage(Attributes attributes, List<Trait> selectedTraits)
        {
            //4 Spellbooks in any one Realm.
            return RequireMinRealms(attributes, 4, 1);
        }

        static public bool TPRE_SageMaster(Attributes attributes, List<Trait> selectedTraits)
        {
            //1 Spellbook in each of 2 different Realms.
            return RequireMinRealms(attributes, 1, 2);
        }

        static public bool TPRE_DivinePower(Attributes attributes, List<Trait> selectedTraits)
        {
            return RequireMinBooks(attributes, TAG.LIFE_MAGIC_BOOK, 4);
        }

        static public bool TPRE_InfernalPower(Attributes attributes, List<Trait> selectedTraits)
        {
            return RequireMinBooks(attributes, TAG.DEATH_MAGIC_BOOK, 4);
        }
        static public bool TPRE_Runemaster(Attributes attributes, List<Trait> selectedTraits)
        {
            return RequireMinRealms(attributes, 2, 3);
        }
        static public bool TPRE_ChaosMastery(Attributes attributes, List<Trait> selectedTraits)
        {
            return RequireMinBooks(attributes, TAG.CHAOS_MAGIC_BOOK, 4);
        }
        static public bool TPRE_NatureMastery(Attributes attributes, List<Trait> selectedTraits)
        {
            return RequireMinBooks(attributes, TAG.NATURE_MAGIC_BOOK, 4);
        }
        static public bool TPRE_SorceryMastery(Attributes attributes, List<Trait> selectedTraits)
        {
            return RequireMinBooks(attributes, TAG.SORCERY_MAGIC_BOOK, 4);
        }
        static public bool TPRE_ManaFocusing(Attributes attributes, List<Trait> selectedTraits)
        {
            return RequireMinRealms(attributes, 4, 1);
        }
        static public bool TPRE_NodeMastery(Attributes attributes, List<Trait> selectedTraits)
        {
            int realms = 3;
            var nature = (Tag)TAG.NATURE_MAGIC_BOOK;
            var chaos  = (Tag)TAG.CHAOS_MAGIC_BOOK;
            var sorcery = (Tag)TAG.SORCERY_MAGIC_BOOK;

            foreach (var kvp in attributes.GetFinalDictionary())
            {
                Tag t = (Tag)kvp.Key;
                if (t == nature || t == chaos || t == sorcery)
                {
                    if (kvp.Value > 0)
                    {
                        realms--;
                        if (realms == 0) return true;
                    }
                }
            }
            return false;
        }
        static public bool TPRE_Lifebringer(Attributes attributes, List<Trait> traits)
        {
            return RequireMinBooks(attributes, TAG.LIFE_MAGIC_BOOK, 4);
        }
        static public bool TPRE_NatureSummoner(Attributes attributes, List<Trait> traits)
        {
            return RequireMinBooks(attributes, TAG.NATURE_MAGIC_BOOK, 4) &&
                ExclusionTraits(traits, (Trait)TRAIT.CONJURER) &&
                ExclusionTraits(traits, (Trait)TRAIT.CHANNELER);
        }
        static public bool TPRE_Conjunrer(Attributes attributes, List<Trait> traits)
        {
            return ExclusionTraits(traits, (Trait)TRAIT.NATURE_SUMMONER);
        }
        static public bool TPRE_Channeler(Attributes attributes, List<Trait> traits)
        {
            return ExclusionTraits(traits, (Trait)TRAIT.NATURE_SUMMONER);
        }
        static public bool TPRE_TechMaster(Attributes attributes, List<Trait> traits)
        {
            return RequireMinRealms(attributes, 3, 2);
        }
        static public bool TPRE_Demonologist(Attributes attributes, List<Trait> traits)
        {
            return RequiredTraits(traits, (Trait)TRAIT.CONJURER) &&
                RequireMinBooks(attributes, TAG.DEATH_MAGIC_BOOK,1) && 
                RequireMinBooks(attributes, TAG.CHAOS_MAGIC_BOOK, 1);
        }
        static public bool TPRE_Orcomancer(Attributes attributes, List<Trait> traits)
        {
            return ExclusionTraits(traits, (Trait)TRAIT.KLACKON_SUPREMACY) &&
                RequireMinBooks(attributes, TAG.DEATH_MAGIC_BOOK, 1);

        }
        static public bool TPRE_MyrranRefugee(Attributes attributes, List<Trait> traits)
        {
            //only 1 refugee allowed
            if (GameManager.GetWizards()?.Find(o => o.HasTrait((Trait)TRAIT.MYRRAN_REFUGEE)) != null) return false;
            //cannot take if already myrran
            return ExclusionTraits(traits, (Trait)TRAIT.MYRRAN);
        }
        static public bool TPRE_Myrran(Attributes attributes, List<Trait> traits)
        {
            //only 1 myrran allowed
            if (GameManager.GetWizards()?.Find(o => o.HasTrait((Trait)TRAIT.MYRRAN)) != null) return false;
            //cannot take if already refugee
            return ExclusionTraits(traits, (Trait)TRAIT.MYRRAN_REFUGEE);
        }
        static public bool TPRE_ImprovedWarlord(Attributes attributes, List<Trait> traits)
        {
            return RequiredTraits(traits, (Trait)TRAIT.WARLORD);
        }
        static public bool TPRE_AntiMagic(Attributes attributes, List<Trait> traits)
        {
            return RequireMaxBooks(attributes, TAG.MAGIC_BOOK, 2);
        }
        static public bool TPRE_Tactitian(Attributes attributes, List<Trait> traits)
        {
            return ExclusionTraits(traits, (Trait)TRAIT.SWIFT_SEALEGS);
        }
        static public bool TPRE_SwiftSealegs(Attributes attributes, List<Trait> traits)
        {
            return ExclusionTraits(traits, (Trait)TRAIT.TACTITIAN);
        }
        static public bool TPRE_Necromancer(Attributes attributes, List<Trait> traits)
        {
            return RequireMinBooks(attributes, TAG.DEATH_MAGIC_BOOK, 3);
        }
        static public bool TPRE_DeathEater(Attributes attributes, List<Trait> traits)
        {
            return RequireMinBooks(attributes, TAG.DEATH_MAGIC_BOOK, 3);
        }
        static public bool TPRE_KlackonSupremacy(Attributes attributes, List<Trait> traits)
        {
            return ExclusionTraits(traits, (Trait)TRAIT.ORCOMANCER);

        }
        #endregion
        #region Race Filter
        static public List<DBDef.Race> TRAC_Orcs(List<DBDef.Race> race)
        {
            //leaves list only with orcs, assuming they were present in the incoming list
           return race.FindAll(o => o.dbName == "RACE-ORCS");
        }
        static public List<DBDef.Race> TRAC_Myrran(List<DBDef.Race> race)
        {
            //leaves list only with myrran races
            return race.FindAll(o => !o.arcanusRace);
        }
        static public List<DBDef.Race> TRAC_Klackon(List<DBDef.Race> race)
        {
            return race.FindAll(o => o.dbName == "RACE-KLACKONS");
        }

        #endregion
        #region Initial Scripts Called when adding a trait to a wizard

        static public void TINIT_Alchemy(PlayerWizard w)
        {
            w.alchemyRatio = 1;
            if (w.unitModificationSkills == null) w.unitModificationSkills = new NetDictionary<string, string>();
            w.unitModificationSkills["SKILL-ENCHANTED_WEAPON2"] = "CSKI_NormalOurNonFantasticCon";
        }

        static public void TINIT_Archmage(PlayerWizard w)
        {
            w.castingSkillBonus += 10;
            w.skillIncomBonus += 50;
            w.globalDispelDificultyIncrease += (FInt)1;
        }
        static public void TINIT_Artificer(PlayerWizard w)
        {
            w.castCostPercentDiscountSpells[(Spell)SPELL.ENCHANT_ITEM] += 0.50f;
            w.castCostPercentDiscountSpells[(Spell)SPELL.CREATE_ARTEFACT] += 0.50f;
        }
        static public void TINIT_Runemaster(PlayerWizard w)
        {
            w.researchDiscontPercent[ERealm.Arcane] += 0.25f;
            w.castCostPercentDiscountRealms[ERealm.Arcane] += 0.25f;
            w.easierDispelling = (FInt)2;
        }

        static public void TINIT_ChaosMastery(PlayerWizard w)
        {
            w.realmDispelDificultyIncrease[ERealm.Chaos] += FInt.ONE;
            w.researchDiscontPercent[ERealm.Chaos] += 0.15f;
            w.castCostPercentDiscountRealms[ERealm.Chaos] += 0.15f;
            if(w.nodesMasteryPercentBonus == null) w.nodesMasteryPercentBonus = new NetDictionary<ERealm, FInt>();
            w.nodesMasteryPercentBonus[ERealm.Chaos] = new FInt(1);
        }

        static public void TINIT_NatureMastery(PlayerWizard w)
        {
            w.realmDispelDificultyIncrease[ERealm.Nature] += FInt.ONE;
            w.researchDiscontPercent[ERealm.Nature] += 0.15f;
            w.castCostPercentDiscountRealms[ERealm.Nature] += 0.15f;
            if (w.nodesMasteryPercentBonus == null) w.nodesMasteryPercentBonus = new NetDictionary<ERealm, FInt>();
            w.nodesMasteryPercentBonus[ERealm.Nature] = new FInt(1);
        }
        static public void TINIT_SorceryMastery(PlayerWizard w)
        {
            w.realmDispelDificultyIncrease[ERealm.Sorcery] += FInt.ONE;
            w.researchDiscontPercent[ERealm.Sorcery] += 0.15f;
            w.castCostPercentDiscountRealms[ERealm.Sorcery] += 0.15f;
            if (w.nodesMasteryPercentBonus == null) w.nodesMasteryPercentBonus = new NetDictionary<ERealm, FInt>();
            w.nodesMasteryPercentBonus[ERealm.Sorcery] = new FInt(1);
        }
        static public void TINIT_Channeler(PlayerWizard w)
        {
            w.ignorSpellcastingRange = true;
            w.lowerEnchantmentPercentUpkeepCost = new FInt(0.5f);
            w.channelerFantasticUnitsUpkeepDiscount = new FInt(0.5f);
        }
        static public void TINIT_Conjunrer(PlayerWizard w)
        {
            w.lowerFantasticUnitsPercentSummonCost = new FInt(0.25f);
            w.conjuerFantasticUnitsUpkeepDiscount = new FInt(0.25f);
            w.lowerResearchFantasticUnitsPercentCost = new FInt(0.25f);
        }
        static public void TINIT_DivineInfernalPower(PlayerWizard w)
        {
            w.townsPowerPercentBonus = new FInt(0.50f);
        }
        static public void TINIT_SageMaster(PlayerWizard w)
        {
            w.buildingsResearchPercentBonus = new FInt(0.25f);
        }
        static public void TINIT_Charismatic(PlayerWizard w)
        {
            w.lowerMercenaryAndHeroCost = 0.50f;
            w.lowerArtefactCost = 0.50f;
        }
        static public void TINIT_Famus(PlayerWizard w)
        {
            w.AddFame(25);
            w.famous = true;
        }
        static public void TINIT_ManaFocusing(PlayerWizard w)
        {
            w.manaPercentBonus = new FInt(0.25f);
        }
        static public void TINIT_Myrran(PlayerWizard w)
        {
            w.myrranRaces = true;
        }
        static public void TINIT_NodeMaster(PlayerWizard w)
        {
            w.nodesPowerPercentBonus = new FInt(1f);
        }
        static public void TINIT_Warlord(PlayerWizard w)
        {
            w.unitLevelIncrease += 1;
            w.AddEnchantment((Enchantment)ENCH.WARLORD, w as IEnchantable);
        }
        static public void TINIT_FantasticWorlord(PlayerWizard w)
        {
            w.AddEnchantment((Enchantment)ENCH.FANTASTIC_WARLORD, w as IEnchantable);
        }
        static public void TINIT_Stonemason(PlayerWizard w)
        {
            if (w.newBuildedTownsModificationEnchs == null) w.newBuildedTownsModificationEnchs = new NetDictionary<string, string>();
            w.newBuildedTownsModificationEnchs["ENCH-STONEMASON"] = "CEKI_CapitolCon";

            if (w.unitModificationSkills == null) w.unitModificationSkills = new NetDictionary<string, string>();
            w.unitModificationSkills["SKILL-ENGINEER"] = "CSKI_MainRaceCon";

            if(w.townExtraBuilding == null) w.townExtraBuilding = new NetDictionary<string, string>();
            w.townExtraBuilding["BUILDING-CITY_WALLS"] = "CBKI_Any";
        }
        static public void TINIT_Lifebringer(PlayerWizard w)
        {
            if (w.unitModificationSkills == null) w.unitModificationSkills = new NetDictionary<string, string>();
            w.unitModificationSkills["SKILL-HEALER"] = "CSKI_MainRaceCon";
            w.castCostPercentDiscountSpells[(Spell)SPELL.RESURRECTION] += 0.25f;
            w.castCostPercentDiscountSpells[(Spell)SPELL.HEALING] += 0.25f;
            w.castCostPercentDiscountSpells[(Spell)SPELL.MASS_HEALING] += 0.25f;

        }
        static public void TINIT_HeroMagnet(PlayerWizard w)
        {
            w.heroHireBonus += 1;
            w.castCostPercentDiscountSpells[(Spell)SPELL.SUMMON_HERO] += 0.25f;
            w.castCostPercentDiscountSpells[(Spell)SPELL.SUMMON_CHAMPION] += 0.25f;

        }
        static public void TINIT_NatureSummoner(PlayerWizard w)
        {
            if(String.IsNullOrEmpty(w.startingUnit)) 
                w.startingUnit = "UNIT-NTR_WAR_BEARS";
            else
                w.startingUnit += ",UNIT-NTR_WAR_BEARS";

            w.fantasticNatureUnitsUpkeepDiscount += 0.5f;
        }
        static public void TINIT_TechMaster(PlayerWizard w)
        {
            if (String.IsNullOrEmpty(w.startingUnit))
                w.startingUnit = "UNIT-TECH_SCOUTING_PROBE";
            else 
                w.startingUnit += ",UNIT-TECH_SCOUTING_PROBE";

            w.researchDiscontPercent[ERealm.Arcane] += 0.05f;
            w.researchDiscontPercent[ERealm.Tech] += 0.05f;
            w.researchDiscontPercent[ERealm.Death] += 0.05f;
            w.researchDiscontPercent[ERealm.Chaos] += 0.05f;
            w.researchDiscontPercent[ERealm.Life] += 0.05f;
            w.researchDiscontPercent[ERealm.Sorcery] += 0.05f;
            w.researchDiscontPercent[ERealm.Nature] += 0.05f;

            w.traitTechMagic = true;

            //its add unlock limits only if we get trait later in game (no from start)
            //On start we add them in magic and research GetUnlockLimits()
            if (w.magicAndResearch != null 
                && w.magicAndResearch.GetUnlockLimits().Find(o => o.realm == ERealm.Tech) == null)
            {
                w.magicAndResearch.UpdateUnlockLimits((Tag)TAG.TECH_BOOK, FInt.ONE * 11);
            }
            
        }
        static public void TINIT_Demonologist(PlayerWizard w)
        {
            if (String.IsNullOrEmpty(w.startingUnit))
                w.startingUnit = "UNIT-DTH_LESSER_SHADOW_DEMONS";
            else
                w.startingUnit += ",UNIT-DTH_LESSER_SHADOW_DEMONS";
        }
        static public void TINIT_Orcomancer(PlayerWizard w)
        {
            if (w.unitModificationSkills == null) w.unitModificationSkills = new NetDictionary<string, string>();
            w.unitModificationSkills["SKILL-LIFE_STEALING_1_ADDON2"] = "CSKI_OnlyOrcsAndBghtruCon";
            w.unitModificationSkills["SKILL-DEATH_IMMUNITY"] = "CSKI_OnlyOrcsAndBghtruCon";
            w.dedicatedRace = "RACE-ORCS";
            w.deathFigureCastingSkillFilter = "FCS_OrcsAndBahgtru";
        }
        static public void TINIT_ImprovedWarlord(PlayerWizard w)
        {
            w.AddEnchantment((Enchantment)ENCH.IMPROVED_WARLORD, w as IEnchantable);
            w.AddEnchantment((Enchantment)ENCH.IMPROVED_WARLORD_UNIT, w as IEnchantable);

            if (String.IsNullOrEmpty(w.startingUnit))
                w.startingUnit = "SUS_MainRaceSwordsmen";
            else
                w.startingUnit += ",SUS_MainRaceSwordsmen";
        }
        static public void TINIT_AntiMagic(PlayerWizard w)
        {
            w.AddEnchantment((Enchantment)ENCH.ANTI_MAGIC, w as IEnchantable);

            if (w.unitModificationSkills == null) w.unitModificationSkills = new NetDictionary<string, string>();
            w.unitModificationSkills["SKILL-MINOR_REGENERATION"] = "CSKI_AnyOurNonFantasticCon";
        }
        static public void TINIT_Necromancer(PlayerWizard w)
        {
            w.AddEnchantment((Enchantment)ENCH.ETERNAL_NIGHT_NECROMANCER, w as IEnchantable);
        }
        static public void TINIT_KlackonSupremacy(PlayerWizard w)
        {
            w.startingUnit = "SUS_Phym";
            w.dedicatedRace = "RACE-KLACKONS";
            w.percentUnrestModifier -= 0.2f;
            w.AddEnchantment((Enchantment)ENCH.KLACKON_SUPREMACY, w as IEnchantable);
        }
        static public void TINIT_PeoplePower(PlayerWizard w)
        {
            if (w.newBuildedTownsModificationEnchs == null) w.newBuildedTownsModificationEnchs = new NetDictionary<string, string>();
            w.newBuildedTownsModificationEnchs["ENCH-POWER_PEOPLE_CITY"] = "CEKI_CapitolCon";

            w.AddEnchantment((Enchantment)ENCH.WIZARD_POWER_PEOPLE, w as IEnchantable);
        }
        static public void TINIT_ThePirate(PlayerWizard w)
        {
            w.traitThePirat = 5;
        }
        static public void TINIT_DeathEater(PlayerWizard w)
        {
            if (w.unitModificationSkills == null) w.unitModificationSkills = new NetDictionary<string, string>();
            w.unitModificationSkills["SKILL-DEATH_EATER"] = "CSKI_AnyOurUndead";
        }
        static public void TINIT_Tactitan(PlayerWizard w)
        {
            w.AddEnchantment((Enchantment)ENCH.TACTITIAN, w as IEnchantable);
            w.AddEnchantment((Enchantment)ENCH.AMBUSH, w as IEnchantable);
        }
        static public void TINIT_SwiftSealegs(PlayerWizard w)
        {
            w.AddEnchantment((Enchantment)ENCH.WATER_SPEED, w as IEnchantable);
            w.AddEnchantment((Enchantment)ENCH.ADMIRAL, w as IEnchantable);
            w.AddEnchantment((Enchantment)ENCH.SEA_MASTER, w as IEnchantable);
            w.seaMasterTrait = true;
        }
        static public void TINIT_SeaHunter(PlayerWizard w)
        {
            w.AddEnchantment((Enchantment)ENCH.SEA_HUNTER, w as IEnchantable);
        }
        static public void TINIT_WaterBoss1(PlayerWizard w)
        {
            w.researchDiscontPercent[ERealm.Arcane] += 0.25f;
        }
        static public void TINIT_WaterBoss2(PlayerWizard w)
        {
            w.castCostPercentDiscountSpells[(Spell)SPELL.MAGIC_SPIRIT] += 0.25f;
            w.castCostPercentDiscountSpells[(Spell)SPELL.DISPEL_MAGIC] += 0.25f;
            w.castCostPercentDiscountSpells[(Spell)SPELL.SPELL_OF_RETURN] += 0.25f;
            w.castCostPercentDiscountSpells[(Spell)SPELL.SUMMONING_CIRCLE] += 0.25f;
            w.castCostPercentDiscountSpells[(Spell)SPELL.DETECT_MAGIC] += 0.25f;
            w.castCostPercentDiscountSpells[(Spell)SPELL.SUMMON_HERO] += 0.25f;
            w.castCostPercentDiscountSpells[(Spell)SPELL.RECALL_HERO] += 0.25f;
            w.castCostPercentDiscountSpells[(Spell)SPELL.DISENCHANT_AREA] += 0.25f;
            w.castCostPercentDiscountSpells[(Spell)SPELL.AWARENESS] += 0.25f;
            w.castCostPercentDiscountSpells[(Spell)SPELL.DISJUNCTION] += 0.25f;
            w.castCostPercentDiscountSpells[(Spell)SPELL.SUMMON_CHAMPION] += 0.25f;
            w.castCostPercentDiscountSpells[(Spell)SPELL.SPELL_OF_MASTERY] += 0.25f;
        }

        #endregion

        #region  Starting Units Scripts
        static public BattleUnit SUS_MainRaceSwordsmen(Race race, PlayerWizard w)
        {
            List<Multitype<BattleUnit, int>> list = PowerEstimate.GetList();
            list = list.FindAll(o => DataBase.GetType<DBDef.Unit>().Contains(o.t0.dbSource.Get() as DBDef.Unit));

            Multitype<BattleUnit, int> bunit = list.Find(o => o.t0.race.Get() == race && o.t0.GetAttFinal(TAG.TOWN_DEFENDER_2) > 0);
            return bunit.t0;
        }
        static public BattleUnit SUS_Phym(Race race, PlayerWizard w)
        {
            List<Multitype<BattleUnit, int>> list = PowerEstimate.GetList();

            Multitype<BattleUnit, int> bunit = list.Find(o => o.t0.GetDBName() == "HERO-PHYM");
            return bunit.t0;
        }
        #endregion

        #region  Conditions for Adding skills to normal units or hero

        static public bool CSKI_MainRaceCon(object u, Skill s, PlayerWizard w)
        {
            if (u is MOM.Unit)
            {
                var unit = u as MOM.Unit;
                if (unit.race != w.mainRace) return false;
                if (unit.GetSkills().Find(o => o == s) != null)
                {
                    return false;
                }
                return true;
            }
            else if (u is DBDef.Unit)
            {
                var unit = u as DBDef.Unit;
                if (unit.race != w.mainRace.Get()) return false;
                foreach (var skill in unit.skills)
                {
                    if (skill == s)
                    {
                        return false;
                    }
                }
                return true;
            }

            return false;
        }
        static public bool CSKI_OnlyOrcsAndBghtruCon(object u, Skill s, PlayerWizard w)
        {
            if (u is MOM.Unit)
            {
                var unit = u as MOM.Unit;
                if (!(unit.race.Get() == (Race)RACE.ORCS || unit.dbSource.Get() == (Subrace)HERO.BAHGTRU)) return false;
                if (unit.GetSkills().Find(o => o == s) != null)
                {
                    return false;
                }
                return true;
            }
            else if (u is DBDef.Unit)
            {
                var unit = u as DBDef.Unit;
                if (!(unit.race == (Race)RACE.ORCS || unit.dbName == "HERO-BAHGTRU")) return false;
                foreach (var skill in unit.skills)
                {
                    if (skill == s)
                    {
                        return false;
                    }
                }
                return true;
            }

            return false;
        }
        static public bool CSKI_AnyOurNonFantasticCon(object u, Skill s, PlayerWizard w)
        {
            if (u is MOM.Unit)
            {
                var unit = u as MOM.Unit;
                if (unit.GetSkills().Find(o => o == s) != null)
                {
                    return false;
                }
                if (unit.GetAttFinal((Tag)TAG.FANTASTIC_CLASS) > 0 )
                {
                    return false;
                }
                return true;
            }
            else if (u is DBDef.Unit)
            {
                var unit = u as DBDef.Unit;
                foreach (var skill in unit.skills)
                {
                    if (skill == s)
                    {
                        return false;
                    }
                }
                if (unit.GetTag((Tag)TAG.FANTASTIC_CLASS) > 0)
                {
                    return false;
                }
                return true;
            }

            return false;
        }
        static public bool CSKI_AnyOurUndead(object u, Skill s, PlayerWizard w)
        {
            if (u is MOM.Unit)
            {
                var unit = u as MOM.Unit;
                if (unit.GetSkills().Find(o => o == s) != null)
                {
                    return false;
                }
                if (unit.race == (Race)RACE.REALM_DEATH || unit.GetAttFinal((Tag)TAG.REANIMATED) > 0)
                {
                    return true;
                }
            }
            else if (u is BattleUnit)
            {
                var unit = u as BattleUnit;
                foreach (var skill in unit.GetSkills())
                {
                    if (skill == s)
                    {
                        return false;
                    }
                }
                if (unit.race == (Race)RACE.REALM_DEATH || unit.GetAttFinal((Tag)TAG.REANIMATED) > 0)
                {
                    return true;
                }
            }
            else if (u is DBDef.Unit)
            {
                var unit = u as DBDef.Unit;
                foreach (var skill in unit.skills)
                {
                    if (skill == s)
                    {
                        return false;
                    }
                }
                if (unit.race == (Race)RACE.REALM_DEATH || unit.GetTag((Tag)TAG.REANIMATED) > 0)
                {
                    return true;
                }
            }

            return false;
        }
        static public bool CSKI_NormalOurNonFantasticCon(object u, Skill s, PlayerWizard w)
        {
            if (u is MOM.Unit)
            {
                var unit = u as MOM.Unit;
                if (unit.GetSkills().Find(o => o == s) != null)
                {
                    return false;
                }
                if (unit.GetAttFinal((Tag)TAG.FANTASTIC_CLASS) > 0 || unit.GetAttFinal((Tag)TAG.HERO_CLASS) > 0)
                {
                    return false;
                }
                return true;
            }
            else if (u is DBDef.Unit)
            {
                var unit = u as DBDef.Unit;
                foreach (var skill in unit.skills)
                {
                    if (skill == s)
                    {
                        return false;
                    }
                }
                if (unit.GetTag((Tag)TAG.FANTASTIC_CLASS) > 0 || unit.GetTag((Tag)TAG.HERO_CLASS) > 0)
                {
                    return false;
                }
                return true;
            }

            return false;
        }
        #endregion
        #region  Conditions for Adding buildings to towns
        static public bool CBKI_Any(TownLocation tl, Building b, PlayerWizard w)
        {
            return true;
        }
        #endregion
        #region  Conditions for Adding enchantments to towns
        static public bool CEKI_CapitolCon(TownLocation tl, Enchantment e, PlayerWizard w)
        {
            if(w.wizardTower != null && w.wizardTower.Get() == tl)
                return true;
            else
                return false;
        }
        static public bool CEKI_MainRaceCon(TownLocation tl, Enchantment e, PlayerWizard w)
        {
            if (w.mainRace.Get() == tl.race.Get())
                return true;
            else
                return false;
        }
        #endregion

        #region  Death Figures Casting Skill Modyfication
        static public bool FCS_OrcsAndBahgtru(BattleUnit bt)
        {
            if (bt.race == (Race)RACE.ORCS || bt.GetDBName() == "HERO-BAHGTRU")
            {
                return true;
            }
            return false;
        }
        #endregion

        #region Helpers
        static public bool RequireMinBooks(Attributes attributes, TAG bookType, int minimum)
        {
            foreach (var kvp in attributes.GetFinalDictionary())
            {
                var tag = (Tag)kvp.Key;
                if (tag != (Tag)bookType) continue;
                if (kvp.Value >= minimum)
                {
                    return true;
                }
            }
            return false;
        }
        static public bool RequireMaxBooks(Attributes attributes, TAG bookType, int maximum)
        {
            if (bookType == TAG.MAGIC_BOOK)
            {
                var books = 0;
                foreach (var kvp in attributes.GetFinalDictionary())
                {
                    books += kvp.Value.ToInt();
                }
                if (books <= maximum)
                    return true;
            }
            else
            {
                foreach (var kvp in attributes.GetFinalDictionary())
                {
                    var tag = (Tag)kvp.Key;
                    if (tag != (Tag)bookType) continue;
                    if (kvp.Value <= maximum)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        static public bool RequireMinRealms(Attributes attributes, int spellBooks, int realms)
        {
            foreach (var kvp in attributes.GetFinalDictionary())
            {
                var tag = (Tag)kvp.Key;
                if (tag.parent != (Tag)TAG.MAGIC_BOOK) continue;
                if (kvp.Value >= spellBooks)
                {
                    realms--;
                    if (realms == 0)
                        return true;
                }
            }
            return false;
        }
        static public bool ExclusionTraits(List<Trait> traits, Trait trait1)
        {
            if (traits.Find(o => o == trait1) != null)
            {
                return false;
            }
            return true;
        }
        static public bool RequiredTraits(List<Trait> traits, Trait trait1)
        {
            if (traits.Find(o => o == trait1) != null)
            {
                return true;
            }
            return false;
        }
        #endregion
    }
}
#endif