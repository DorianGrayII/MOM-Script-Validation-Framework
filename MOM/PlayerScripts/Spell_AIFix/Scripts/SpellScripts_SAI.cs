/**********************************
 *
 * Author:  Dorian Gray
 * Date:    May 9, 2024
 * Version: 1.0.1
 *
 **********************************/

#if !USE_DEBUG_SCRIPT || !UNITY_EDITOR

using System.Collections.Generic;
using DBDef;
using DBEnum;
using MHUtils;
using MOM;
using UnityEngine;
using WorldCode;


namespace GameScript_SAI
{
    using static GameScript.SpellScripts;
    using static UserUtility.Utility;

    public class SpellScripts : ScriptBase
    {
        /// <summary>
        /// enables verbose counter magic logging
        /// </summary>
        private const bool bLoggingEnabled = true;

        #region EarthToMud
        public static int SBAI_EarthToMud(SpellCastData data, object target, Spell spell)
        {
            int iRetVal = 0;
            Vector3i pos = (Vector3i)target;
            if (pos == null)
            {
                Debug.LogError("  [SBAI_EarthToMud] target == null");
                return iRetVal;
            }

            int iDistance = spell.fIntData[0].ToInt();

            Battle battle = data.battle;

            List<BattleUnit> buList = battle.GetAllUnits();
            foreach (BattleUnit bu in buList)
            {
                if (HexCoordinates.HexDistance(bu.GetPosition(), pos) < iDistance &&
                    bu.attributes.DoesNotContains((Tag)TAG.TELEPORTING) &&
                    bu.attributes.DoesNotContains((Tag)TAG.CAN_FLY) &&
                    bu.attributes.DoesNotContains((Tag)TAG.NON_CORPOREAL) &&
                    bu.attributes.DoesNotContains((Tag)TAG.EARTH_WALKER) &&
                    bu.GetEnchantments().Find(o => o.source == (Enchantment)ENCH.EARTH_TO_MUD) == null)
                {
                    if (bu.ownerID == data.GetWizardID())
                    {
                        iRetVal -= bu.GetBattleUnitValue();
                    }
                    else
                    {
                        iRetVal += bu.GetBattleUnitValue();
                    }
                }
            }

            if (bLoggingEnabled)
            {
#pragma warning disable CS0162 // Unreachable code detected

                Debug.Log(spell.dbName + " with script " +
                          spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + iRetVal);
#pragma warning restore CS0162 // Unreachable code detected
            }

            return iRetVal;
        }

        public static bool SBH_MassSlow(SpellCastData data, object target, Spell spell)
        {
            if (target == null)
            {
                Debug.LogError("  [SBH_MassSlow] - target == null");
                return false;
            }

            Vector3i pos = Vector3i.invalid;
            BattleUnit bu = target as BattleUnit;
            HexCoordinates hex = target as HexCoordinates;
            if (bu != null)
            {
                Debug.LogWarningFormat("  [SBH_MassSlow] - target is BattleUnit:{0}", GetNameOwnerID(bu));
                pos = bu.battlePosition;
            }
            if (hex != null)
            {
                Debug.LogWarningFormat("  [SBH_MassSlow] - target is HexCoordinates:{0}", hex.ToString());
                pos = (Vector3i)target;
            }
            if (target is Vector3i)
            {
                pos = (Vector3i)target;
            }

            int iDistance = spell.fIntData[0].ToInt();

            if ((data.battle != null) && (pos != Vector3i.invalid))
            {
                foreach (BattleUnit battleUnit in data.battle.GetAllUnits())
                {
                    if (HexCoordinates.HexDistance(battleUnit.GetPosition(), pos) <= iDistance &&
                        battleUnit.attributes.DoesNotContains((Tag)TAG.TELEPORTING) &&
                        battleUnit.attributes.DoesNotContains((Tag)TAG.CAN_FLY) &&
                        battleUnit.attributes.DoesNotContains((Tag)TAG.NON_CORPOREAL) &&
                        battleUnit.GetSkills().Find(o => o == (Skill)SKILL.EARTH_WALKER) == null &&
                        battleUnit.GetEnchantments().Find(o => o.source == (Enchantment)ENCH.EARTH_TO_MUD) == null)
                    {
                        foreach (Enchantment en in spell.enchantmentData)
                        {
                            battleUnit.AddEnchantment(en, data.caster as Entity, en.lifeTime, null, spell.worldCost);
                        }
                    }
                }
                return true;
            }
            return false;
        }
        #endregion
        public static int SBAI_WarpWood(SpellCastData data, object target, Spell spell)
        {
            int iRetVal = 0;
            BattleUnit bu = target as BattleUnit;
            if (bu == null)
            {
                return iRetVal;
            }

            iRetVal = bu.GetBattleUnitValue();

            if (bLoggingEnabled)
            {
                Debug.Log(spell.dbName + " with script " +
                            spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + iRetVal +
                            " on unit " + bu.GetDBName().ToString());
            }

            return iRetVal;
        }

        public static bool STAR_ChangeTerrain(SpellCastData data, object target, Spell spell)
        {
            Hex hex = target as Hex;

            if (hex != null)
            {
                DBDef.Terrain changeTo = hex.GetTerrain().transmuteTo;
                if (changeTo == null)
                {
                    return false;
                }


                WorldCode.Plane plane = World.GetArcanus();

                if (plane.GetHexAt(hex.Position) != hex)
                {
                    plane = World.GetMyrror();
                    if (plane.GetHexAt(hex.Position) != hex)
                    {
                        Debug.LogError("STAR_ChangeTerrain: cannot find hex");
                        return false;
                    }
                }

                List<MOM.Location> locList = GameManager.GetLocationsOfThePlane(plane);

                MOM.Location local = locList.Find(o => o.GetPosition() == hex.Position);
                if (local != null && !(local is TownLocation)
                    && local.locationType != ELocationType.Lair
                    && local.locationType != ELocationType.StrongLair
                    && local.locationType != ELocationType.WeakLair
                    && local.locationType != ELocationType.Ruins)
                {
                    return false;
                }

                local = locList.Find(o => HexCoordinates.HexDistance(o.GetPosition(), hex.Position) <= 2);
                if (local != null && local is TownLocation)
                {
                    TownLocation townLoc = local as TownLocation;
                    if (IsTownProtected(data.GetWizardID(), spell, townLoc))
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                Debug.Log("Spell is designed to target Hex");
            }

            return false;
        }

        public static int SWAI_ChangeTerrain(ISpellCaster source, object target, Spell spell)
        {
            int iRetVal = 0;
            Hex hex = target as Hex;
            PlayerWizardAI caster = source as PlayerWizardAI;
            if (hex == null)
            {
                Debug.LogError("Spell " + spell.dbName + " target is invalid");
                return iRetVal;
            }
            if (caster == null)
            {
                Debug.LogError("Spell " + spell.dbName + " caster is invalid");
                return iRetVal;
            }

            ETerrainType terrainType = hex.GetTerrain().terrainType;

            if ((terrainType == ETerrainType.Swamp) ||
                (terrainType == ETerrainType.Desert) ||
                (terrainType == ETerrainType.GrassLand))
            {
                List<MOM.Location> locList = GameManager.GetTownsOfWizard(caster.GetID());

                foreach (MOM.Location loc in locList)
                {
                    TownLocation townLoc = loc as TownLocation;

                    if (townLoc != null)
                    {
                        if (WorldCode.Plane.Get().GetDistanceWrapping(hex.Position, loc.GetPosition()) < 3)
                        {
                            int iTownValue = townLoc.GetStrategicValue();
                            // we are looking for a town that has no forest
                            if ((townLoc.GetForestCountInArea() == 0) &&
                                (terrainType == ETerrainType.GrassLand))
                            {  // return a value such that the AI will
                               // convert a plains to forest so that the town
                               // can now build sawmills, etc ...
                                iRetVal = iTownValue;
                            }
                            else
                            { // else then just return a value
                              // for Swamp & Desert hexes such that the AI will convert to plains
                                iRetVal = (int)(iTownValue * 0.7f);
                            }
                            break;
                        }
                    }
                }
            }

            if (bLoggingEnabled)
            {
                Debug.Log(spell.dbName + " with script " +
                          spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + iRetVal +
                          " on hex " + hex.Position.ToString() + " terrain " + terrainType.ToString());
            }
            return iRetVal;
        }

        public static int SBAI_DispelMagic(SpellCastData data, object target, Spell spell)
        {
            int iRetVal = 0;
            BattleUnit bu = target as BattleUnit;
            if (bu == null)
            {
                return iRetVal;
            }

            int iBuValue = bu.GetBattleUnitValue();
            int iEnchCount = 0;

            foreach (EnchantmentInstance ei in bu.GetEnchantments())
            {
                DBReference<Enchantment> ench = ei.source;
                if (ench.Get().allowDispel == true)
                {
                    if ((ench.Get().enchCategory == EEnchantmentCategory.Negative &&
                        bu.ownerID == data.GetWizardID()) ||
                        (ench.Get().enchCategory == EEnchantmentCategory.Positive &&
                        bu.ownerID != data.GetWizardID()))
                    {
                        iEnchCount++;
                    }
                }
            }

            switch (iEnchCount)
            {
                case 1:
                    iRetVal = (int)(iBuValue * 0.3);
                    break;
                case 2:
                    iRetVal = (int)(iBuValue * 0.5);
                    break;
                case 3:
                    iRetVal = (int)(iBuValue * 0.7);
                    break;
                case 4:
                    iRetVal = (int)(iBuValue * 0.9);
                    break;

                default:
                    iRetVal = (int)(iBuValue * 1.1);
                    break;
            }

//#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + iRetVal + 
                " on unit " + bu.GetDBName().ToString());
//#endif

            return iRetVal;
        }


        public static int SBAI_DispelMagicTrue(SpellCastData data, object target, Spell spell)
        {
            int iRetVal = 0;
            BattleUnit bu = target as BattleUnit;
            if (bu == null)
            {
                return iRetVal;
            }

            int iBuValue = bu.GetBattleUnitValue();
            int iEnchCount = 0;

            foreach (EnchantmentInstance ei in bu.GetEnchantments())
            {
                DBReference<Enchantment> ench = ei.source;
                if (ench.Get().allowDispel == true)
                {
                    if ((ench.Get().enchCategory == EEnchantmentCategory.Negative &&
                        bu.ownerID == data.GetWizardID()) ||
                        (ench.Get().enchCategory == EEnchantmentCategory.Positive &&
                        bu.ownerID != data.GetWizardID()))
                    {
                        iEnchCount++;
                    }
                }
            }

            switch (iEnchCount)
            {
                case 1:
                    iRetVal = (int)(iBuValue * 0.3);
                    break;
                case 2:
                    iRetVal = (int)(iBuValue * 0.5);
                    break;
                case 3:
                    iRetVal = (int)(iBuValue * 0.7);
                    break;
                case 4:
                    iRetVal = (int)(iBuValue * 0.9);
                    break;

                default:
                    iRetVal = (int)(iBuValue * 1.1);
                    break;
            }

            iRetVal = (int)(iRetVal * 1.1);

//#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + iRetVal + 
                " on unit " + bu.GetDBName().ToString());
//#endif

            return iRetVal;
        }

        public static int SBAI_MassInvisibility(SpellCastData data, object target, Spell spell)
        {
            int iRetVal = 0;
            float unitPercentValue = 0.6f;

            foreach (BattleUnit bu in data.GetFriendlyUnits())
            {
                if (!bu.IsAlive())
                {
                    continue;
                }

                iRetVal += bu.GetBattleUnitValue();
            }

            iRetVal = (int)(iRetVal * unitPercentValue);

//#if (UNITY_EDITOR && DEBUG_SPELLS)
            Debug.Log(spell.dbName + " with script " +
                spell.aiBattleEvaluationScript.ToString() + " give SpellAI value " + iRetVal +
                " on battlefield.");
//#endif
            return iRetVal;
        }
    }
}

#endif