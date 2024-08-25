/**********************************
 *
 * Author:  Dorian Gray
 * Date:    Feb 23 2024
 * Version: 1.0.13
 *
 **********************************/

#if !USE_DEBUG_SCRIPT || !UNITY_EDITOR

using System.Collections.Generic;
using DBDef;
using MHUtils;
using MOM;

/// <summary>
/// Common helper methods, mostly for generating formatted debug messages, for use by players in their callback scripts
/// </summary>
/// <remarks>
/// Some thought was made into the method naming convention, rather it should be generic and opaque or informative.
/// Ultimately, I decided to be somewhat more informative.  As a result the followning patters were adopted.
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
///    I wanted a set of functions I coulc quickly utilize where I did not have to null-check upfront and it would handle for me.
///    At the same time, I wanted to see in the log when null values were passed, so went with the explicit "{null} string.
/// 
/// </remarks>
/// <warning>
/// You will have to change the following namespace name
/// </warning>

namespace UserUtility_END
{
    public static class Utility
    {
        static readonly int iHumanID = global::MOM.PlayerWizard.HumanID();

        #region PlayerWizard helpers

        /// <summary>
        /// Determines if target is Human
        /// </summary>
        /// <param name="wizard">target</param>
        /// <returns>True if target is a Human Player</returns>
        public static bool IsHuman(PlayerWizard wizard)
        {
            // Note - PlayerWizard.GetID and PlayerWizard.GetOwnerID returns same value
            return (wizard != null) ? (wizard.GetID() == iHumanID) : false;
        }

        /// <summary>
        /// Generates a formatted string containing the target's name and ID
        /// </summary>
        /// <param name="wizard">target</param>
        /// <returns>Returns the formatted string or {null} on error.</returns
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
        /// Checks the target for the trait attribute.
        /// </summary>
        /// <param name="wizard">target</param>
        /// <param name="trait"></param>
        /// <returns></returns>
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
        /// <returns>Returns the formatted string or {null} on error.</returns
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
        /// <returns>True if simulation flag is set.</returns>
        public static bool IsSimulated(IEnchantable ie)
        {
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
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <returns>True if Owner of target is Human Player</returns>
        public static bool IsOwnerHuman(ISpellCaster target)
        {
            return (target != null) ? (target.GetWizardOwnerID() == iHumanID) : false;
        }

        /// <summary>
        /// Generates a formatted string containing the target's name and owning Wizard's ID
        /// </summary>
        /// <param name="caster">target</param>
        /// <returns>Returns the formatted string or {null} on error.</returns
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
        /// <returns>Returns the formatted string or {null} on error.</returns
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
        /// <param name="iValue"></param>
        /// <returns>Returns the formatted string or {null} on error.</returns>
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
        /// <returns>Returns the formatted string or {null} on error.</returns
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
        /// <returns>Returns the formatted string or {null} on error.</returns
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
        /// <returns>Returns the formatted string or {null} on error.</returns
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
        /// <returns>Returns the formatted string or {null} on error.</returns
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
        /// <returns>target's owner as BattleUnit or null</returns>
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
        /// <returns>target's owner as ISpellCaster or null</returns>
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
        /// Obtains polymorphic owner as a PlayerWizard object for the target
        /// </summary>
        /// <param name="ei">target</param>
        /// <returns>target's owner as PlayerWizard or null</returns>
        public static PlayerWizard GetOwnerAsPlayerWizard(EnchantmentInstance ei)
        {
            PlayerWizard caster = null;
            if (ei != null)
            {
                Entity owner = ei.owner?.GetEntity();
                if (owner is PlayerWizard)
                {
                    caster = owner as PlayerWizard;
                }
                else if (owner is BattleUnit)
                {
                    caster = (owner as BattleUnit).GetWizardOwner();
                }
                else if ((owner as IEnchantable) is BattlePlayer)
                {
                    caster = ((owner as IEnchantable) as BattlePlayer).GetWizardOwner();
                }
            }
            return caster;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ei">target</param>
        /// <returns>Wizard Owner ID</returns>
        public static int GetWizardOwnerID(EnchantmentInstance ei)
        {
            int iID = 0;
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
        /// Generates a formatted string containing the target's localized name
        /// </summary>
        /// <param name="spell">target</param>
        /// <returns>Returns the formatted string or {null} on error.</returns
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
        /// 
        /// </summary>
        /// <param name="spell"></param>
        /// <param name="iSpellCost"></param>
        /// <returns></returns>
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
        /// Generates a formatted string containing the target's localized name and battle cost
        /// </summary>
        /// <param name="spell"></param>
        /// <returns>Returns the formatted string or {null} on error.</return
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
        /// 
        /// </summary>
        /// <param name="bu">target</param>
        /// <returns>True if target's owner is a Human Player</returns>
        public static bool IsOwnerHuman(BaseUnit bu)
        {
            return (bu.GetWizardOwnerID() == iHumanID);
        }

        /// <summary>
        /// Generates a formatted string containing the target's localized name
        /// </summary>
        /// <param name="bu">target</param>
        /// <returns>Returns the formatted string or {null} on error.</returns>
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
        /// <returns>Returns the formatted string or {null} on error.</returns
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
        /// <returns>Returns the formatted string or {null} on error.</returns
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
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <returns>Returns the formatted string or {null} on error.</returns>
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
        /// <returns>True if trait is found</returns>
        public static bool HasTrait(List<Trait> traitList, Trait trait)
        {
            return traitList != null ? (traitList.Find(o => o == trait) != null) : false;
        }

        /// <summary>
        /// Local version of IsEngineer that uses GetBase instead of GetFinal 
        /// </summary>
        /// <param name="baseUnit">target</param>
        /// <returns>True if target contains the ENGINEER_UNIT tag</returns
        public static bool IsEngineer(BaseUnit baseUnit)
        {
            return GetBaseValue(baseUnit, DBEnum.TAG.ENGINEER_UNIT) > 0;
        }

        /// <summary>
        /// Obtains target's attribute value via GetBase
        /// </summary>
        /// <param name="baseUnit">target</param>
        /// <param name="targetTag">attribute</param>
        /// <returns>FInt containing attribute value</returns>
        public static FInt GetBaseValue(BaseUnit baseUnit, DBEnum.TAG targetTag)
        {
            return baseUnit != null ? baseUnit.GetAttributes().GetBase(targetTag) : FInt.ZERO;
        }

        /// <summary>
        /// Operates the same as string.IsNullOrEmpty, with exception that that type is an array
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array">target</param>
        /// <returns>True if the array parameter is null or has a length of zero; otherwise, False.</returns>
        public static bool IsNullOrEmpty<T>(this T[] array)
        {
            return array == null || array.Length == 0;
        }

        /// <summary>
        /// Operates the same as string.IsNullOrEmpty, with exception that that type is ICollection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection">target</param>
        /// <returns>True if the ICollection parameter is null or has a length of zero; otherwise, False.</returns>
        public static bool IsNullOrEmpty<T>(this ICollection<T> collection)
        {
            return collection == null || collection.Count == 0;
        }

        #endregion


        #region EnchAlreadyOnObject

        /// <summary>
        /// A complete rewrite of the original version supplied by Muha that was O(n^3).
        /// </summary>
        /// <param name="spellCaster"></param>
        /// <param name="spell"></param>
        /// <param name="target">Expects ISkillable and/or IEnchantable</param>
        /// <returns></returns>
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
        /// <returns></returns>
        public static bool EnchantAlreadyOnIEnchantable(Spell spell, IEnchantable target)
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
        /// <returns></returns>
        public static bool EnchantAlreadyOnISkillable(Spell spell, ISkillable target)
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
#endif