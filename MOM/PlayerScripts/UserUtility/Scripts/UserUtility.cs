/**********************************
 *
 * Author:  Dorian Gray
 * Date:    May 19, 2024
 * Version: 1.0.16
 *
 **********************************/


using System.Collections.Generic;
using DBDef;
using MHUtils;
using MOM;

/// <summary>
/// Common helper methods, mostly for generating formatted debug messages, for use by players in their callback scripts
/// </summary>
/// <remarks>
/// Some thought was made into the method naming convention, rather it should be generic and opaque or informative.
/// Ultimately, I decided to be somewhat more informative.  As a result the following patters were adopted.
/// 
/// GetNameID - formatted Name and ID
/// GetOwnerNameID - (formatted Name and ID) both belonging to the Wizard Owner
/// GetNameOwnerID - formatted Name and Wizard Owner ID
///                  more common than you think as many object don't have a 
///                  relevant ID and is instead associated with the owning Wizard
/// GetOwnerAsWizardNameID - special case where the object used is polymorphic
/// GetOwnerAsBattleUnitNameID
/// 
/// Why all the null checks?
///    The purpose of these helper function arose from logging arguments, which could be null and a normal argument value.
///    I wanted a set of functions I could quickly utilize where I did not have to null-check upfront and it would handle for me.
///    At the same time, I wanted to see in the log when null values were passed, so went with the explicit "{null} string.
/// 
/// </remarks>

namespace UserUtility
{
    public static class Utility
    {
        static readonly int iHumanID = global::MOM.PlayerWizard.HumanID();
        static readonly int iInvalidWizardID = global::MOM.PlayerWizard.InvalidWizardID();

        #region PlayerWizard helpers

        /// <summary>
        /// Determines if target is Human
        /// </summary>
        /// <param name="wizard">target</param>
        /// <returns>true if target is a Human Player.</returns>
        public static bool IsHuman(PlayerWizard wizard)
        {
            // Note - PlayerWizard.GetID and PlayerWizard.GetOwnerID returns same value
            return (wizard != null) ? (wizard.GetID() == iHumanID) : false;
        }

        /// <summary>
        /// Generates a formatted string containing the target's name and ID
        /// </summary>
        /// <param name="wizard">target</param>
        /// <returns>formatted string or {null} on error.</returns>
        public static string GetNameID(PlayerWizard wizard)
        {
            string str = "{null}";

            if (wizard != null)
            {
                // Note - PlayerWizard.GetID and PlayerWizard.GetOwnerID returns same value
                str = string.Format("{0}({1})", wizard.GetName(), wizard.GetID());
            }

            return str;
        }

        /// <summary>
        /// Checks the wizard for the trait attribute.
        /// </summary>
        /// <param name="wizard">target</param>
        /// <param name="trait">the trait to locate</param>
        /// <returns>true if trait if found in wizard's List<Trait> container.</Trait></returns>
        public static bool HasTrait(PlayerWizard wizard, DBEnum.TRAIT trait)
        {
            bool bRetVal = false;

            if (wizard != null)
            {
                List<Trait> traitList = wizard.GetTraits();

                bRetVal = HasTrait(traitList, (Trait)trait);
            }

            return bRetVal;
        }

        #endregion

        #region IEnchantable helpers
        /// <summary>
        /// Generates a formatted string containing the target's name and Wizard Owner ID
        /// </summary>
        /// <param name="ie">target</param>
        /// <returns>formatted string or {null} on error.</returns>
        public static string GetNameOwnerID(IEnchantable ie)
        {
            string str = "{null}";

            if (ie != null)
            {
                // initially unowned
                int iID = 0;

                PlayerWizard owner = ie.GetWizardOwner();

                if (owner != null)
                {
                    // Note - PlayerWizard.GetID and PlayerWizard.GetOwnerID returns same value
                    iID = owner.GetID();
                }

                str = string.Format("{0}({1})", ie.GetName(), iID);
            }

            return str;
        }

        /// <summary>
        /// Checks the polymorphic target as being flagged as 'simulated' by game engine.
        /// </summary>
        /// <param name="ie">target, expects a MOM.Unit, BattleUnit or Battle</param>
        /// <returns>true if simulation flag is set.</returns>
        public static bool IsSimulated(IEnchantable ie)
        {
            /*
             * 	IEnchantable can be one of (BaseUnit),
	         *    (BattlePlayer),
	         *    (GameManager),
	         *    (Location),
	         *    (PlayerWizard)
             *    
             *    But only the following, below have a simulation data member
             */
            bool bRetVal = false;

            if (ie != null)
            {
                MOM.Unit unit = ie as MOM.Unit;
                BattleUnit bu = ie as BattleUnit;
                Battle battle = ie as Battle;

                if (unit != null)
                {
                    bRetVal = unit.simulationUnit;
                }
                else if (bu != null)
                {
                    bRetVal = bu.simulated;
                }
                else if (battle != null)
                {
                    bRetVal = battle.simulation;
                }
            }

            return bRetVal;
        }

        #endregion

        #region ISpellCaster helpers

        /// <summary>
        /// Tests if target is owned by Human player
        /// </summary>
        /// <param name="target"></param>
        /// <returns>true if Owner of target is Human Player.</returns>
        public static bool IsOwnerHuman(ISpellCaster target)
        {
            return (target != null) ? (target.GetWizardOwnerID() == iHumanID) : false;
        }

        /// <summary>
        /// Generates a formatted string containing the target's name and owning Wizard's ID
        /// </summary>
        /// <param name="caster">target</param>
        /// <returns>formatted string or {null} on error.</returns>
        public static string GetNameOwnerID(ISpellCaster caster)
        {
            string str = "{null}";

            if (caster != null)
            {
                str = string.Format("{0}({1})", caster.GetName(), caster.GetWizardOwnerID());
            }

            return str;
        }

        #endregion

        #region IDescriptionInfoType helpers

        /// <summary>
        /// Generates a formatted string containing the target's localized name
        /// Can be used on the following: Artefact, Building, Enchantment, Location, Plane, Race, Resource, Skill, Spell, Subrace, Tag, Terrain, Town, Trait, Wizard
        /// </summary>
        /// <param name="descriptionInfo">target</param>
        /// <returns>formatted string or {null} on error.</returns>
        public static string GetName(IDescriptionInfoType descriptionInfo)
        {
            string str = "{null}";

            if (descriptionInfo != null)
            {
                // Get Description Info Localized Name
                str = descriptionInfo.GetDILocalizedName();
            }

            return str;
        }

        /// <summary>
        /// Generates a formatted string containing the target's localized name and an int value such as Spell Cost
        /// </summary>
        /// <param name="descriptionInfo">target</param>
        /// <param name="iValue">user provided variable to be included in formatted string</param>
        /// <returns>formatted string or {null} on error.</returns>
        public static string GetEnchantmentName(IDescriptionInfoType descriptionInfo, int iValue)
        {
            string str = "{null}";

            if (descriptionInfo != null)
            {
                str = string.Format("{0}({1})", GetName(descriptionInfo), iValue);
            }

            return str;
        }

        #endregion

        #region EnchantmentInstance helpers
        /// <summary>
        /// Generates a formatted string containing the target owner's name and owner Wizard's ID from an
        /// internal polymorphic entity.  This entity is cast to a PlayerWizard and used for the
        /// name and owner ID data.
        /// </summary>
        /// <param name="ei">target</param>
        /// <returns>formatted string or {null} on error.</returns>
        public static string GetOwnerAsWizardNameID(EnchantmentInstance ei)
        {
            string str = "{null}";

            if (ei != null)
            {
                PlayerWizard enchOwner = GetOwnerAsPlayerWizard(ei);

                if (enchOwner != null)
                {
                    str = string.Format("{0}({1})", enchOwner.GetName(), enchOwner.GetWizardOwnerID());
                }
            }

            return str;
        }

        /// <summary>
        /// Generates a formated string containing the target's name and Owner's ID
        /// </summary>
        /// <param name="ei">target</param>
        /// <returns>formatted string or {null} on error.</returns>
        public static string GetNameOwnerID(EnchantmentInstance ei)
        {
            string str = "{null}";

            if (ei != null)
            {
                if (ei.owner != null)
                {
                    str = string.Format("{0}({1})", ei.nameID, ei.owner.ID);
                }
                else
                {
                    str = string.Format("{0}({1})", ei.nameID, 0);
                }
            }

            return str;
        }

        /// <summary>
        /// Generates a formatted string containing the target's localized name
        /// </summary>
        /// <param name="ei">target</param>
        /// <returns>formatted string or {null} on error.</returns>
        public static string GetEnchantmentName(EnchantmentInstance ei)
        {
            string str = "{null}";

            if (ei != null)
            {
                str = ei.source.Get().GetDILocalizedName();
            }

            return str;
        }

        /// <summary>
        /// Generates a formatted string containing the target's owner name and Wizard's ID from an
        /// internal polymorphic entity.  This entity is cast to a BattleUnit and used for the
        /// name and owner ID data.
        /// </summary>
        /// <param name="ei">target</param>
        /// <returns>formatted string or {null} on error.</returns>
        public static string GetOwnerAsBattleUnitNameID(EnchantmentInstance ei)
        {
            string str = "{null}";

            if (ei != null)
            {
                BattleUnit bu = GetOwnerAsBattleUnit(ei);

                str = string.Format("{0}({1})", GetName(bu), (bu != null) ? bu.GetWizardOwnerID() : 0);
            }

            return str;
        }

        /// <summary>
        /// Determines if owner exists as a BattleUnit
        /// </summary>
        /// <param name="ei">target</param>
        /// <returns>BattleUnit containing target's owner or null if not found.</returns>
        public static BattleUnit GetOwnerAsBattleUnit(EnchantmentInstance ei)
        {
            BattleUnit bu = null;

            if (ei != null)
            {
                bu = (ei.owner != null) ? (ei.owner.GetEntity() as BattleUnit) : null;
            }

            return bu;
        }

        /// <summary>
        /// Determines if owner exists as an ISpellCaster
        /// </summary>
        /// <param name="ei">target</param>
        /// <returns>ISpellCaster containing the target's owner or null if not found.</returns>
        public static ISpellCaster GetOwnerAsISpellCaster(EnchantmentInstance ei)
        {
            ISpellCaster spellCaster = null;

            if (ei != null)
            {
                spellCaster = (ei.owner != null) ? (ei.owner.GetEntity() as ISpellCaster) : null;
            }

            return spellCaster;
        }

        /// <summary>
        /// Obtains the owner PlayerWizard object from a polymorphic target
        /// </summary>
        /// <param name="ei">target as PlayerWizard, BattleUnit or IEnchantable->BattlePlayer</param>
        /// <returns>PlayerWizard containing target's owner or null if not found.</returns>
        public static PlayerWizard GetOwnerAsPlayerWizard(EnchantmentInstance ei)
        {
            PlayerWizard playerWizard = null;
            if (ei != null)
            {
                Entity owner = ei.owner?.GetEntity();
                if (owner is PlayerWizard)
                {
                    playerWizard = owner as PlayerWizard;
                }
                else if (owner is BattleUnit)
                {
                    playerWizard = (owner as BattleUnit).GetWizardOwner();
                }
                else if ((owner as IEnchantable) is BattlePlayer)
                {
                    playerWizard = ((owner as IEnchantable) as BattlePlayer).GetWizardOwner();
                }
            }
            return playerWizard;
        }

        /// <summary>
        /// Obtains the Owner's Wizard ID from a polymorphic type
        /// </summary>
        /// <param name="ei">target as a PlayWizard, BattleUnit or IEnchantable->BattlePlayer</param>
        /// <returns>int containing the Owner's Wizard ID or iInvalidWizardID if not found.</returns>
        public static int GetWizardOwnerID(EnchantmentInstance ei)
        {
            int iID = iInvalidWizardID;
            if (ei != null)
            {
                Entity owner = ei.owner?.GetEntity();
                if (owner is PlayerWizard)
                {
                    // Note - PlayerWizard.GetID and PlayerWizard.GetOwnerID returns same value
                    iID = (owner as PlayerWizard).GetID();
                }
                else if (owner is BattleUnit)
                {
                    iID = (owner as BattleUnit).GetWizardOwnerID();
                }
                else if ((owner as IEnchantable) is BattlePlayer)
                {
                    iID = ((owner as IEnchantable) as BattlePlayer).GetID();
                }
            }

            return iID;
        }

        #endregion

        #region Spell helpers
        /// <summary>
        /// Generates a formatted string containing the Spell's localized name
        /// </summary>
        /// <param name="spell">target</param>
        /// <returns>the formatted string or {null} on error.</returns>
        public static string GetSpellName(Spell spell)
        {
            string str = "{null}";

            if (spell != null)
            {
                str = spell.GetDescriptionInfo().GetLocalizedName();
            }

            return str;
        }

        /// <summary>
        /// Generates a formatted string containing the spell's localized name and user provided iSpellCost
        /// </summary>
        /// <param name="spell"></param>
        /// <param name="iSpellCost"></param>
        /// <returns>the formatted string or {null} on error.</returns>
        public static string GetSpellName(Spell spell, int iSpellCost)
        {
            string str = "{null}";

            if (spell != null)
            {
                str = string.Format("{0}({1})", spell.GetDescriptionInfo().GetLocalizedName(), iSpellCost);
            }

            return str;
        }

        /// <summary>
        /// Generates a formatted string containing the spell's localized name and battle cost
        /// </summary>
        /// <param name="spell"></param>
        /// <returns>the formatted string or {null} on error.</returns>
        public static string GetSpellNameBattleCost(Spell spell)
        {
            string str = "{null}";

            if (spell != null)
            {
                str = string.Format("{0}({1})", spell.GetDescriptionInfo().GetLocalizedName(), spell.battleCost);
            }

            return str;
        }
        #endregion

        #region BaseUnit helpers

        /// <summary>
        /// Test if target belongs to a Human player
        /// </summary>
        /// <param name="bu">target</param>
        /// <returns>true if target's owner is a Human Player</returns>
        public static bool IsOwnerHuman(BaseUnit bu)
        {
            bool bRetVal = false;

            if (bu != null)
            {
               bRetVal = (bu.GetWizardOwnerID() == iHumanID);
            }

            return bRetVal;
        }

        /// <summary>
        /// Generates a formatted string containing the target's localized name
        /// </summary>
        /// <param name="bu">target</param>
        /// <returns>formatted string or {null} on error.</returns>
        public static string GetName(BaseUnit bu)
        {
            string str = "{null}";

            if (bu != null)
            {
                str = bu.GetDescriptionInfo().GetLocalizedName();
            }

            return str;
        }

        /// <summary>
        /// Generates a formatted string containing the target's name and owning Wizard's ID
        /// </summary>
        /// <param name="bu">target</param>
        /// <returns>formatted string or {null} on error.</returns>
        public static string GetNameOwnerID(BaseUnit bu)
        {
            string str = "{null}";

            if (bu != null)
            {
                str = string.Format("{0}({1})", GetName(bu), bu.GetWizardOwnerID());
            }

            return str;
        }

        #endregion

        #region SpellCastData helpers

        /// <summary>
        /// Generates a formatted string containing the Wizard name and ID
        /// </summary>
        /// <param name="data">target</param>
        /// <returns>formatted string or {null} on error.</returns>
        public static string GetWizardNameID(SpellCastData data)
        {
            string str = "{null}";

            if (data != null)
            {
                int iID = data.GetWizardID();

                PlayerWizard wizard = GameManager.GetWizard(iID);

                if (wizard != null)
                {
                    str = string.Format("{0}({1})", wizard.GetName(), iID);
                }
            }

            return str;
        }

        /// <summary>
        /// Generates a formatted string containing the caster name and the owning Wizard's ID
        /// </summary>
        /// <param name="data"></param>
        /// <returns>the formatted string or {null} on error.</returns>
        public static string GetCasterNameOwnerID(SpellCastData data)
        {
            string str = "{null}";

            if (data != null)
            {
                ISpellCaster caster = data.caster;

                if (caster != null)
                {
                    str = string.Format("{0}({1})", caster.GetName(), caster.GetWizardOwnerID());
                }
            }

            return str;
        }

        #endregion


        #region utility
        /// <summary>
        /// Search target list for value
        /// </summary>
        /// <param name="traitList">target</param>
        /// <param name="trait">value</param>
        /// <returns>true if trait is found</returns>
        public static bool HasTrait(List<Trait> traitList, Trait trait)
        {
            return traitList != null ? (traitList.Find(o => o == trait) != null) : false;
        }

        /// <summary>
        /// Local version of IsEngineer that uses GetBase instead of GetFinal 
        /// </summary>
        /// <param name="baseUnit">target</param>
        /// <returns>true if target contains the ENGINEER_UNIT tag</returns>
        public static bool IsEngineer(BaseUnit baseUnit)
        {
            return GetBaseValue(baseUnit, DBEnum.TAG.ENGINEER_UNIT) > 0;
        }

        /// <summary>
        /// Obtains target's attribute value via GetBase
        /// </summary>
        /// <param name="baseUnit">target</param>
        /// <param name="targetTag">attribute</param>
        /// <returns>FInt containing the attribute value or FInt.ZERO if not found</returns>
        public static FInt GetBaseValue(BaseUnit baseUnit, DBEnum.TAG targetTag)
        {
            return baseUnit != null ? baseUnit.GetAttributes().GetBase(targetTag) : FInt.ZERO;
        }

        /// <summary>
        /// Operates the same as string.IsNullOrEmpty, with exception that the input type is an array
        /// </summary>
        /// <typeparam name="T">The type of the elements in the array</typeparam>
        /// <param name="array">target</param>
        /// <returns>true if the array parameter is null or has a length of zero; otherwise, false.</returns>
        public static bool IsNullOrEmpty<T>(this T[] array)
        {
            return array == null || array.Length == 0;
        }

        /// <summary>
        /// Operates the same as string.IsNullOrEmpty, with exception that the input type is a ICollection
        /// </summary>
        /// <typeparam name="T">The type of the elements in the collection</typeparam>
        /// <param name="collection">target</param>
        /// <returns>true if the ICollection parameter is null or has a length of zero; otherwise, false.</returns>
        public static bool IsNullOrEmpty<T>(this ICollection<T> collection)
        {
            return collection == null || collection.Count == 0;
        }

        #endregion


        #region EnchAlreadyOnObject replacement

        /// <summary>
        /// A complete rewrite of the original version supplied by Muha that was O(n^3).
        /// While this rewrite is O(n + m)
        /// </summary>
        /// <param name="spellCaster">ignored, but is required to match the original function signature</param>
        /// <param name="spell"></param>
        /// <param name="target">Expects object as ISkillable or IEnchantable</param>
        /// <returns>true if target already enchanted with the spell</returns>
        public static bool EnchAlreadyOnObject(ISpellCaster spellCaster, Spell spell, object target)
        {
            bool bResult = false;

            ISkillable   skillable   = target as ISkillable;
            IEnchantable enchantable = target as IEnchantable;

            if (skillable != null)
            {
                if (EnchantAlreadyOnISkillable(spell, skillable))
                {
                    bResult = true;
                }
            }
            if ((enchantable != null) && (bResult == false))
            {
                if (EnchantAlreadyOnIEnchantable(spell, enchantable))
                {
                    bResult = true;
                }
            }

            return bResult;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spell"></param>
        /// <param name="target"></param>
        /// <returns>true if target is already enchanted with the spell</returns>
        private static bool EnchantAlreadyOnIEnchantable(Spell spell, IEnchantable target)
        {
            bool bRetVal = false;

            // Validate parameters
            if (target != null && spell != null && spell.enchantmentData != null)
            {
                List<EnchantmentInstance> eiTargetList = target.GetEnchantments();

                if (!IsNullOrEmpty(eiTargetList) && !IsNullOrEmpty(spell.enchantmentData))
                {
                    HashSet<Enchantment> spellEnchSet  = new HashSet<Enchantment>(spell.enchantmentData);
                    HashSet<Enchantment> targetEnchSet = new HashSet<Enchantment>();

                    foreach (EnchantmentInstance eiTarget in eiTargetList)
                    {
                        targetEnchSet.Add(eiTarget.source.Get());
                    }

                    // note - the following is purported to be O(n + m)
                    if (targetEnchSet.IsSupersetOf(spellEnchSet))
                    {
                        bRetVal = true;
                    }
                }
            }
            return bRetVal;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spell"></param>
        /// <param name="target"></param>
        /// <returns>true if target is already enchanted with the spell</returns>
        private static bool EnchantAlreadyOnISkillable(Spell spell, ISkillable target)
        {
            bool bRetVal = false;

            // Validate parameters
            if (target != null && spell != null && spell.enchantmentData != null)
            {
                List<DBReference<Skill>> refTargetSkillList = target.GetSkills();

                if (!IsNullOrEmpty(refTargetSkillList) && !IsNullOrEmpty(spell.enchantmentData))
                {
                    HashSet<Enchantment> spellEnchSet  = new HashSet<Enchantment>(spell.enchantmentData);
                    HashSet<Enchantment> targetEnchSet = new HashSet<Enchantment>();

                    foreach (DBReference<Skill> refTargetSkill in refTargetSkillList)
                    {
                        Enchantment[] rgTargetEnch = refTargetSkill.Get().relatedEnchantment;
                        if (!IsNullOrEmpty(rgTargetEnch))
                        {
                            targetEnchSet.UnionWith(rgTargetEnch);
                        }
                    }

                    if (targetEnchSet.IsSupersetOf(spellEnchSet))
                    {
                        bRetVal = true;
                    }
                }
            }
            return bRetVal;
        }

        #endregion

    }
}
