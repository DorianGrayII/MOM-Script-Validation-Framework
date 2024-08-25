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
    public class SkillScripts : ScriptBase
    {
        #region Active Triggers (ATRI)
        /// <param name="source"> unit producing damage/attack </param>
        /// <param name="skill"> skill used to attack </param>
        /// <param name="skillScript"> skillScript used </param>
        /// <param name="battle"> battle information </param>
        /// <param name="target"> target requiring validation </param>
        /// <returns> bool - true if target is valid for this form of attack </returns>
        static public bool ATRI_NormalAttack(ISkillable source, object destination, Skill skill, SkillScript skillScript, Battle battle, BattleAttackStack battleStack)
        {
            BattleUnit bu = source as BattleUnit;
            if (bu == null) Debug.LogError("TRG on invalid type: expected Battle unit in TrgNormalAttack");

            BattleUnit buTrg = destination as BattleUnit;
            if (buTrg == null)
            {
                if (!(destination is Vector3i))
                {
                    Debug.LogError("Unhandled type in variable Target for TrgNormalAttack");
                }
                else
                {
                    Vector3i position = (Vector3i)destination;
                    var oponents = bu.attackingSide ? battle.defenderUnits : battle.attackerUnits;

                    if (oponents != null)
                    {
                        foreach (var v in oponents)
                        {
                            if (v.GetPosition() == position)
                            {
                                buTrg = v;
                                break;
                            }
                        }
                    }
                    //handle terrain attack??
                }
            }
            if (buTrg == null) return false;

            if (buTrg.attackingSide != bu.attackingSide &&
               HexCoordinates.HexDistance(buTrg.GetPosition(), bu.GetPosition()) == 1)
            {
                BattleAttack ba = new BattleAttack();
                ba.dmg = battleStack.GetRollCache();
                ba.source = source as BattleUnit;
                ba.destination = destination as BattleUnit;
                //initiative of the attachment is in relation to the source
                //fire breath happens before normal attack
                ba.initiative = FInt.ONE + bu.initiativeModifier;
                ba.attackStack = battleStack;
                ba.type = skillScript.triggerType;
                ba.skill = skill;
                ba.skillScript = skillScript;
                ba.ConsiderWalls(battle);

                battleStack.attackQueue.Add(ba);

                return true;
            }

            return false;
        }
        static public bool ATRI_NormalRangedAttack(ISkillable source, object destination, Skill skill, SkillScript skillScript, Battle battle, BattleAttackStack battleStack)
        {
            BattleUnit bu = source as BattleUnit;
            if (bu == null) Debug.LogError("TRG on invalid type: expected Battle unit in TrgNormalAttack");

            BattleUnit buTrg = destination as BattleUnit;
            if (buTrg == null)
            {
                if (!(destination is Vector3i))
                {
                    Debug.LogError("Unhandled type in variable Target for TrgNormalAttack");
                }
                else
                {
                    Vector3i position = (Vector3i)destination;
                    var oponents = bu.attackingSide ? battle.defenderUnits : battle.attackerUnits;

                    if (oponents != null)
                    {
                        foreach (var v in oponents)
                        {
                            if (v.GetPosition() == position)
                            {
                                buTrg = v;
                                break;
                            }
                        }
                    }
                    //handle terrain attack??
                }
            }
            if (buTrg == null) return false;

            if (buTrg.attackingSide != bu.attackingSide &&
               HexCoordinates.HexDistance(buTrg.GetPosition(), bu.GetPosition()) > 1)
            {
                BattleAttack ba = new BattleAttack();
                ba.dmg = battleStack.GetRollCache();
                ba.source = source as BattleUnit;
                ba.destination = destination as BattleUnit;
                //initiative of the attachment is in relation to the source
                //fire breath happens before normal attack
                ba.initiative = FInt.ONE + bu.initiativeModifier;
                ba.attackStack = battleStack;
                ba.type = skillScript.triggerType;
                ba.skill = skill;
                ba.skillScript = skillScript;
                ba.ConsiderWalls(battle);

                battleStack.attackQueue.Add(ba);

                //Destroy Wall if bu contain WallCrusher and bu is attacker side
                if(battle != null && battle.battleWalls != null &&
                    bu.GetAttFinal((Tag)TAG.WALL_CRUSHER) >0 &&
                    battle.attackerUnits.Contains(bu) &&
                    bu.attackingSide)
                {
                    var position = buTrg.GetPosition();
                    BattleWall wall = null;
                    int dist = -1;
                    foreach (var wallPart in battle.battleWalls)
                    {
                        if (HexCoordinates.HexDistance(position, wallPart.position) <= 1)
                        {
                            wall = wallPart;
                        }
                    }
                    if(wall != null)
                    {
                        dist = HexCoordinates.HexDistance(bu.GetPosition(), wall.position);
                        if (dist <= 1 && UnityEngine.Random.Range(0f, 1f) < 0.5f ||
                            dist > 1 && UnityEngine.Random.Range(0f, 1f) < 0.25f)
                        {
                            wall.AnimateDestroy();
                            return true;
                        }
                    }
                }
                
                return true;
            }

            return false;
        }

        #endregion
        #region Passive Triggers (TRI)

        /// <param name="source"> unit producing damage/attack </param>
        /// <param name="skill"> skill used to attack </param>
        /// <param name="skillScript"> skillScript used </param>
        /// <param name="battle"> battle information </param>
        /// <param name="skillToEnhance"> preceeding skill which tries to trigger this one </param>
        /// <returns> bool - true if triggering is successful </returns>
        static public bool TRI_AddonAttack(ISkillable source, ISkillable destination, Skill skill, SkillScript skillScript, Battle battle, BattleAttackStack battleStack)
        {
            //breath, throw will attach to attacker melee basic attacks 

            if (battleStack == null || battleStack.attackQueue == null || battleStack.defender == source) return false;

            var existingAddons = battleStack.attackQueue.FindAll(o => o.source == source &&
                    o.type == ESkillType.BattleAttackAddon &&
                    o.skill == skill &&
                    o.skillScript == skillScript);

            //loop from the end as we will add new elements at the end of the list,
            //which we do not want to process
            for (int i = battleStack.attackQueue.Count - 1; i >= 0; i--)
            {
                if (battleStack.attackQueue[i].source == source &&
                    battleStack.attackQueue[i].type == ESkillType.BattleAttack)
                {
                    if (existingAddons.Exists(o => o.addonToIndex == i)) continue;

                    var trigSkill = battleStack.attackQueue[i];

                    BattleAttack ba = new BattleAttack();
                    ba.dmg = battleStack.GetRollCache();
                    ba.source = source as BattleUnit;
                    ba.destination = destination as BattleUnit;
                    //initiative of the attachment is in relation to the source
                    //fire breath happens before normal attack
                    if (skillScript != null)
                    {
                        ba.initiative = trigSkill.initiative - skillScript.priority;
                    }
                    else
                    {
                        ba.initiative = trigSkill.initiative - FInt.ONE;
                    }
                    ba.attackStack = battleStack;
                    ba.type = ESkillType.BattleAttackAddon;
                    ba.skill = skill;
                    ba.skillScript = skillScript;
                    ba.addonToIndex = i;
                    ba.ConsiderWalls(battle);

                    battleStack.attackQueue.Add(ba);
                }
            }
            return true;
        }
        static public bool TRI_AddonAttack2(ISkillable source, ISkillable destination, Skill skill, SkillScript skillScript, Battle battle, BattleAttackStack battleStack)
        {
            //addon2 will attach to all attacks

            if (battleStack == null || battleStack.attackQueue == null) return false;

            var existingAddons = battleStack.attackQueue.FindAll(o => o.source == source &&
                    o.type == ESkillType.BattleAttackAddon2 &&
                    o.skill == skill &&
                    o.skillScript == skillScript);

            //loop from the end as we will add new elements at the end of the list,
            //which we do not want to process
            for (int i = battleStack.attackQueue.Count - 1; i >= 0; i--)
            {
                if (battleStack.attackQueue[i].source == source &&
                    (battleStack.attackQueue[i].type == ESkillType.BattleAttack ||
                    battleStack.attackQueue[i].type == ESkillType.BattleAttackAddon ||
                    battleStack.attackQueue[i].type == ESkillType.BattleRangedAttack))
                {
                    if (existingAddons.Exists(o => o.addonToIndex == i)) continue;

                    var trigSkill = battleStack.attackQueue[i];

                    BattleAttack ba = new BattleAttack();
                    ba.dmg = battleStack.GetRollCache();
                    ba.source = source as BattleUnit;
                    ba.destination = destination as BattleUnit;
                    //initiative of the attachment is in relation to the source
                    if (skillScript != null)
                    {
                        ba.initiative = trigSkill.initiative - skillScript.priority;
                    }
                    else
                    {
                        ba.initiative = trigSkill.initiative;
                    }
                    ba.attackStack = battleStack;
                    ba.type = ESkillType.BattleAttackAddon2;
                    ba.skill = skill;
                    ba.skillScript = skillScript;
                    ba.addonToIndex = i;
                    ba.ConsiderWalls(battle);

                    battleStack.attackQueue.Add(ba);
                }
            }
            return true;
        }
        static public bool TRI_AddonAttack2ToAllNonAttackAddonsAttacks(ISkillable source, ISkillable destination, Skill skill, SkillScript skillScript, Battle battle, BattleAttackStack battleStack)
        {
            //addon2 will attach to all non battle Attack Addons attacks

            if (battleStack == null || battleStack.attackQueue == null) return false;

            var existingAddons = battleStack.attackQueue.FindAll(o => o.source == source &&
                    o.type == ESkillType.BattleAttackAddon2 &&
                    o.skill == skill &&
                    o.skillScript == skillScript);

            //loop from the end as we will add new elements at the end of the list,
            //which we do not want to process
            for (int i = battleStack.attackQueue.Count - 1; i >= 0; i--)
            {
                if (battleStack.attackQueue[i].source == source &&
                    (battleStack.attackQueue[i].type == ESkillType.BattleAttack ||
                    battleStack.attackQueue[i].type == ESkillType.BattleRangedAttack))
                {
                    if (existingAddons.Exists(o => o.addonToIndex == i)) continue;

                    var trigSkill = battleStack.attackQueue[i];

                    BattleAttack ba = new BattleAttack();
                    ba.dmg = battleStack.GetRollCache();
                    ba.source = source as BattleUnit;
                    ba.destination = destination as BattleUnit;
                    //initiative of the attachment is in relation to the source
                    if (skillScript != null)
                    {
                        ba.initiative = trigSkill.initiative - skillScript.priority;
                    }
                    else
                    {
                        ba.initiative = trigSkill.initiative;
                    }
                    ba.attackStack = battleStack;
                    ba.type = ESkillType.BattleAttackAddon2;
                    ba.skill = skill;
                    ba.skillScript = skillScript;
                    ba.addonToIndex = i;
                    ba.ConsiderWalls(battle);

                    battleStack.attackQueue.Add(ba);
                }
            }
            return true;
        }

        static public bool TRI_RangeAttackAddon2(ISkillable source, ISkillable destination, Skill skill, SkillScript skillScript, Battle battle, BattleAttackStack battleStack)
        {
            //addon2 will attach to all non battle Attack Addons attacks

            if (battleStack == null || battleStack.attackQueue == null) return false;

            var existingAddons = battleStack.attackQueue.FindAll(o => o.source == source &&
                    o.type == ESkillType.BattleAttackAddon2 &&
                    o.skill == skill &&
                    o.skillScript == skillScript);

            //loop from the end as we will add new elements at the end of the list,
            //which we do not want to process
            for (int i = battleStack.attackQueue.Count - 1; i >= 0; i--)
            {
                if (battleStack.attackQueue[i].source == source &&
                    (battleStack.attackQueue[i].type == ESkillType.BattleRangedAttack))
                {
                    if (existingAddons.Exists(o => o.addonToIndex == i)) continue;

                    var trigSkill = battleStack.attackQueue[i];

                    BattleAttack ba = new BattleAttack();
                    ba.dmg = battleStack.GetRollCache();
                    ba.source = source as BattleUnit;
                    ba.destination = destination as BattleUnit;
                    //initiative of the attachment is in relation to the source
                    if (skillScript != null)
                    {
                        ba.initiative = trigSkill.initiative - skillScript.priority;
                    }
                    else
                    {
                        ba.initiative = trigSkill.initiative;
                    }
                    ba.attackStack = battleStack;
                    ba.type = ESkillType.BattleAttackAddon2;
                    ba.skill = skill;
                    ba.skillScript = skillScript;
                    ba.addonToIndex = i;
                    ba.ConsiderWalls(battle);

                    battleStack.attackQueue.Add(ba);
                }
            }
            return true;
        }

        static public bool TRI_MeleeAttackAddon2(ISkillable source, ISkillable destination, Skill skill, SkillScript skillScript, Battle battle, BattleAttackStack battleStack)
        {
            //addon2 will attach to all non battle Attack Addons attacks

            if (battleStack == null || battleStack.attackQueue == null) return false;

            var existingAddons = battleStack.attackQueue.FindAll(o => o.source == source &&
                    o.type == ESkillType.BattleAttackAddon2 &&
                    o.skill == skill &&
                    o.skillScript == skillScript);

            //loop from the end as we will add new elements at the end of the list,
            //which we do not want to process
            for (int i = battleStack.attackQueue.Count - 1; i >= 0; i--)
            {
                if (battleStack.attackQueue[i].source == source &&
                    (battleStack.attackQueue[i].type == ESkillType.BattleAttack))
                {
                    if (existingAddons.Exists(o => o.addonToIndex == i)) continue;

                    var trigSkill = battleStack.attackQueue[i];

                    BattleAttack ba = new BattleAttack();
                    ba.dmg = battleStack.GetRollCache();
                    ba.source = source as BattleUnit;
                    ba.destination = destination as BattleUnit;
                    //initiative of the attachment is in relation to the source
                    if (skillScript != null)
                    {
                        ba.initiative = trigSkill.initiative - skillScript.priority;
                    }
                    else
                    {
                        ba.initiative = trigSkill.initiative;
                    }
                    ba.attackStack = battleStack;
                    ba.type = ESkillType.BattleAttackAddon2;
                    ba.skill = skill;
                    ba.skillScript = skillScript;
                    ba.addonToIndex = i;
                    ba.ConsiderWalls(battle);

                    battleStack.attackQueue.Add(ba);
                }
            }
            return true;
        }
        static public bool TRI_AddonAttackEveryAttack(ISkillable source, ISkillable destination, Skill skill, SkillScript skillScript, Battle battle, BattleAttackStack battleStack)
        {
            //addon will attach to all melee basic attacks

            if (battleStack == null || battleStack.attackQueue == null) return false;

            var existingAddons = battleStack.attackQueue.FindAll(o => o.source == source &&
                    o.type == ESkillType.BattleAttackAddon &&
                    o.skill == skill &&
                    o.skillScript == skillScript);

            //loop from the end as we will add new elements at the end of the list,
            //which we do not want to process
            for (int i = battleStack.attackQueue.Count - 1; i >= 0; i--)
            {
                if (battleStack.attackQueue[i].source == source &&
                    battleStack.attackQueue[i].type == ESkillType.BattleAttack)
                {
                    if (existingAddons.Exists(o => o.addonToIndex == i)) continue;

                    var trigSkill = battleStack.attackQueue[i];

                    BattleAttack ba = new BattleAttack();
                    ba.dmg = battleStack.GetRollCache();
                    ba.source = source as BattleUnit;
                    ba.destination = destination as BattleUnit;
                    //initiative of the attachment is in relation to the source
                    //addon happens before normal attack
                    if(skillScript != null)
                    {
                        ba.initiative = trigSkill.initiative - skillScript.priority;
                    }
                    else
                    {
                        ba.initiative = trigSkill.initiative - FInt.ONE;
                    }
                    ba.attackStack = battleStack;
                    ba.type = ESkillType.BattleAttackAddon;
                    ba.skill = skill;
                    ba.skillScript = skillScript;
                    ba.addonToIndex = i;
                    ba.ConsiderWalls(battle);

                    battleStack.attackQueue.Add(ba);
                }
            }
            return true;
        }
        static public bool TRI_BattleAttackEffect(ISkillable source, ISkillable destination, Skill skill, SkillScript skillScript, Battle battle, BattleAttackStack battleStack)
        {
            //piercing will attach to all melee basic attacks 

            if (battleStack == null || battleStack.attackQueue == null) return false;

            for (int i = battleStack.attackQueue.Count - 1; i >= 0; i--)
            {
                BattleAttack ba = battleStack.attackQueue[i];
                if (ba.source == source)
                {
                    if (skillScript.battleAttackEffect == ESkillBattleAttackEffect.Piercing)
                        ba.isPiercing = true;
                    else if (skillScript.battleAttackEffect == ESkillBattleAttackEffect.Illusion)
                        ba.isIllusion = true;
                    else if (skillScript.battleAttackEffect == ESkillBattleAttackEffect.FirstStrike)
                    {
                        //First strike only work when unit attack
                        if(battleStack.attacker == ba.source)
                        {
                            var antiFirstStrike = false;
                            foreach (var s in destination.GetSkills())
                            {
                                if (s.Get().script == null) continue;

                                if (Array.Find(s.Get().script, o => o.battleAttackEffect == ESkillBattleAttackEffect.AntiFirstStrike) != null)
                                {
                                    antiFirstStrike = true;
                                    break;
                                }
                            }
                            if (!antiFirstStrike)
                            {
                                ba.isFirstStrike = true;
                                ba.initiative = ba.initiative - 0.5f;
                            }
                        }
                    }
                }
            }
            return true;
        }

#endregion
        #region Attack Producing Activators        
        /// <param name="source"> unit producing damage/attack </param>
        /// <param name="skill"> skill used to attack </param>
        /// <param name="skillScript"> skillScript used </param>
        /// <param name="battle"> battle information </param>
        /// <param name="dmgBuffer"> base buffer, if provided then it may be reused if fitting </param>
        /// <param name="random"> random class for rolls </param>
        /// <returns> array of the damages produced by the unit attack form </returns>
        static public object ACT_ProduceNormalDamage(ISkillable source, ISkillable destination, Skill skill, SkillScript skillScript, Battle battle, int[] dmgBuffer, MHRandom random, object data)
        {
            BattleUnit bu = source as BattleUnit;
            BattleUnit buTrg = destination as BattleUnit;

            if (bu == null) Debug.LogError("TRG on invalid type: expected Battle unit in TrgNormalAttack");

            if (buTrg.attributes.Contains(TAG.CAUSE_FEAR))
            {
                if (bu.attributes.Contains(TAG.RIGHTEOUSNESS) ||
                    bu.attributes.Contains(TAG.MAGIC_IMMUNITY) ||
                    bu.attributes.Contains(TAG.DEATH_IMMUNITY))
                    dmgBuffer = bu.ProduceFeardDamage(dmgBuffer, random, buTrg, 30);
                else if (bu.attributes.Contains(TAG.BLESS))
                    dmgBuffer = bu.ProduceFeardDamage(dmgBuffer, random, buTrg, 3);
                else
                    dmgBuffer = bu.ProduceFeardDamage(dmgBuffer, random, buTrg);
            }
            else
            {
                dmgBuffer = bu.ProduceDamage(dmgBuffer, random, buTrg);
            }


            return dmgBuffer;
        }
        static public object ACT_ProduceNoDamage(ISkillable source, ISkillable destination, Skill skill, SkillScript skillScript, Battle battle, int[] dmgBuffer, MHRandom random, object data)
        {
            for(int i=0;i< dmgBuffer.Length; i++)
            {
                dmgBuffer[i] = 0;
            }
            return dmgBuffer;
        }
        static public object ACT_BreathAttack(ISkillable source, ISkillable destination, Skill skill, SkillScript skillScript, Battle battle, int[] dmgBuffer, MHRandom random, object data)
        {
            BattleUnit bu = source as BattleUnit;
            BattleUnit buTrg = destination as BattleUnit;
            if (bu == null) Debug.LogError("TRG on invalid type: expected Battle unit in TrgNormalAttack");

            int attack = skillScript.fIntParam.ToInt();
            attack += bu.attributes.GetFinal(TAG.FIRE_BREATH_BONUS).ToInt();

            dmgBuffer = bu.ProduceDamage(dmgBuffer, random, bu.GetCurentFigure().attackChance, attack, buTrg);

            return dmgBuffer;
        }
        static public object ACT_LighteningBreathAttack(ISkillable source, ISkillable destination, Skill skill, SkillScript skillScript, Battle battle, int[] dmgBuffer, MHRandom random, object data)
        {
            dmgBuffer = (int[])ACT_BreathAttack(source, destination, skill, skillScript, battle, dmgBuffer, random, data);

            BattleUnit buTrg = destination as BattleUnit;
            if (buTrg != null && buTrg.attributes.Contains(TAG.LIGHTNING_WEAKNESS))
            {
                for (int i = 0; i < dmgBuffer.Length; i++)
                {
                    if (dmgBuffer[i] > 0)
                        dmgBuffer[i] = dmgBuffer[i] * 2;
                }
            }

            return dmgBuffer;
        }
        static public object ACT_Throw(ISkillable source, ISkillable destination, Skill skill, SkillScript skillScript, Battle battle, int[] dmgBuffer, MHRandom random, object data)
        {
            BattleUnit bu = source as BattleUnit;
            BattleUnit buTrg = destination as BattleUnit;
            if (bu == null) Debug.LogError("TRG on invalid type: expected Battle unit in TrgNormalAttack");

            int attack = skillScript.fIntParam.ToInt();
            attack += bu.attributes.GetFinal(TAG.THROW_BONUS).ToInt();

            dmgBuffer = bu.ProduceDamage(dmgBuffer, random, bu.GetCurentFigure().attackChance, attack, buTrg);

            return dmgBuffer;
        }
        static public object ACT_Bite(ISkillable source, ISkillable destination, Skill skill, SkillScript skillScript, Battle battle, int[] dmgBuffer, MHRandom random, object data)
        {
            BattleUnit bu = source as BattleUnit;
            BattleUnit buTrg = destination as BattleUnit;
            if (bu == null) Debug.LogError("TRG on invalid type: expected Battle unit in TrgNormalAttack");

            int attack = skillScript.fIntParam.ToInt();

            dmgBuffer = bu.ProduceDamage(dmgBuffer, random, bu.GetCurentFigure().attackChance, attack, buTrg);

            return dmgBuffer;
        }
        static public object ACT_ProducePoisonAttack(ISkillable source, ISkillable destination, Skill skill, SkillScript skillScript, Battle battle, int[] dmgBuffer, MHRandom random, object data)
        {
            BattleUnit bu = source as BattleUnit;
            if (bu == null) Debug.LogError("TRG on invalid type: expected Battle unit in TrgNormalAttack");

            //Prepare number of poison hits.
            int attack = skillScript.fIntParam.ToInt();
            dmgBuffer[0] = attack * bu.figureCount;

            return dmgBuffer;
        }
        static public object ACT_ProduceImmolationAttack(ISkillable source, ISkillable destination, Skill skill, SkillScript skillScript, Battle battle, int[] dmgBuffer, MHRandom random, object data)
        {
            BattleUnit bu = source as BattleUnit;
            BattleUnit buTrg = destination as BattleUnit;
            if (bu == null) Debug.LogError("TRG on invalid type: expected Battle unit in TrgNormalAttack");

            var baseDmg = skillScript.fIntParam.ToInt();
            //Prepare number of poison hits.
            for (int i = 0; i < buTrg.figureCount; i++)
            {
                var hitChance = 0.3f;
                dmgBuffer[i] = random.GetSuccesses(hitChance, baseDmg);
            }

            return dmgBuffer;
        }
        static public object ACT_ProduceLifeStealAttack(ISkillable source, ISkillable destination, Skill skill, SkillScript skillScript, Battle battle, int[] dmgBuffer, MHRandom random, object data)
        {
            BattleUnit bu = source as BattleUnit;
            if (bu == null) Debug.LogError("TRG on invalid type: expected Battle unit in TrgNormalAttack");

            int resistReduction = skillScript.fIntParam.ToInt();

            // if caster is hero
            BattleUnit hero = null;
            int resModFromHero = 0;
            if (source is BattleUnit)
            {
                hero = source as BattleUnit;
                resModFromHero = hero.attributes.GetFinal((Tag)TAG.SPELL_SAVE).ToInt();
            }

            dmgBuffer[0] = resistReduction + resModFromHero;

            return dmgBuffer;
        }
        static public object ACT_ProducePowerDrainAttack(ISkillable source, ISkillable destination, Skill skill, SkillScript skillScript, Battle battle, int[] dmgBuffer, MHRandom random, object data)
        {
            BattleUnit bu = source as BattleUnit;
            if (bu == null)
            {
                Debug.LogError("TRG on invalid type: expected Battle unit in TrgNormalAttack");
            }

            dmgBuffer[0] = random.GetInt(2, 21);

            return dmgBuffer;
        }
        static public object ACT_ProduceRangeNormalDamage(ISkillable source, ISkillable destination, Skill skill, SkillScript skillScript, Battle battle, int[] dmgBuffer, MHRandom random, object data)
        {
            BattleUnit bu = source as BattleUnit;
            BattleUnit buTrg = destination as BattleUnit;

            if (bu == null)
            {
                Debug.LogError("TRG on invalid type: expected Battle unit in TrgNormalAttack");
            }
            else
            {               
                dmgBuffer = bu.ProduceRangedDamage(dmgBuffer, random, buTrg);
            }

            return dmgBuffer;
        }
        
        static public object ACT_ProduceRangedAreaAttack(ISkillable source, ISkillable destination, Skill skill, SkillScript skillScript, Battle battle, int[] dmgBuffer, MHRandom random, object data)
        {
            BattleUnit bu = source as BattleUnit;
            BattleUnit buTrg = destination as BattleUnit;

            if (bu == null) Debug.LogError("TRG on invalid type: expected Battle unit in TrgNormalAttack");

            dmgBuffer = bu.ProduceRangedAreaDamage(dmgBuffer, buTrg.figureCount, random, buTrg);

            return dmgBuffer;
        }
        static public object ACT_PlaneShift(ISkillable source, ISkillable destination, Skill skill, SkillScript skillScript, Battle battle, int[] dmgBuffer, MHRandom random, object data)
        {
            MOM.Unit u = source as MOM.Unit;
            if (u == null) Debug.LogError("TRG on invalid type: expected Unit in ACT_PlaneShift");
            
            MOM.Group g = u.group?.Get();
            if (!g.IsSwitchPlaneDestinationValid()) return null;

            List<MOM.Unit> list = new List<MOM.Unit> { u };
            g.PlaneSwitch(list);

            if(g != null)
            {
                g.UpdateMapFormation();
            }
                        
            return dmgBuffer;
        }
        #endregion
        #region Attack Applying Activators      
        /// <param name="source"> unit producing damage/attack </param>
        /// <param name="skill"> skill used to attack </param>
        /// <param name="skillScript"> skillScript used </param>
        /// <param name="battle"> battle information </param>
        /// <param name="dmgBuffer"> base buffer, if provided then it may be reused if fitting </param>
        /// <param name="random"> random class for rolls </param>
        /// <param name="target"> unit targetted by the attack </param>
        /// <returns> bool - true if unit was successfully attacked </returns>
        static public bool ACT_ApplyAttack(ISkillable source, ISkillable destination, Skill skill, SkillScript skillScript, Battle battle, int[] dmgBuffer, MHRandom random, object data)
        {
            Vector3i position = Vector3i.zero;
            BattleUnit buTrg = destination as BattleUnit;
            BattleUnit bu = source as BattleUnit;

            if (bu == null) Debug.LogError("ACT on invalid type: expected Battle unit in ACT_ApplyAttack");
            
            if (!(data is BattleAttack))
            {
                Debug.LogError("Data is not BattleAttack type");
                return false;
            }
            BattleAttack ba = data as BattleAttack;

            if (buTrg == null) return false;

            if (bu.attributes.Contains(TAG.CHAOS_WEAPON) != false)
            {
                for (int i = bu.figureCount - 1; i >= 0; i--)
                {
                    dmgBuffer[i] = bu.attributes.GetFinal(TAG.MELEE_ATTACK).ToInt() / 2;
                }

                buTrg.canDefend = false;
                buTrg.ApplyDamage(dmgBuffer, random, ba, 0);
                buTrg.canDefend = true;
            }
            else if (buTrg.attributes.Contains(TAG.WEAPON_IMMUNITY) &&
                bu.attributes.DoesNotContains((Tag)TAG.ENCHANTED_WEAPON))
            {
                buTrg.ApplyDamage(dmgBuffer, random, ba, 10, true);
            }
            else if (buTrg.attributes.Contains(TAG.ELEMENTAL_ARMOR) &&
                (bu.race == (Race)RACE.REALM_NATURE || bu.race == (Race)RACE.REALM_CHAOS))
            {
                buTrg.ApplyDamage(dmgBuffer, random, ba, 10);
            }
            else if (buTrg.attributes.Contains(TAG.RESIST_ELEMENTS) &&
                (bu.race == (Race)RACE.REALM_NATURE || bu.race == (Race)RACE.REALM_CHAOS))
            {
                buTrg.ApplyDamage(dmgBuffer, random, ba, 3);
            }
            else if (buTrg.attributes.Contains(TAG.BLESS) &&
                (bu.race == (Race)RACE.REALM_DEATH || bu.race == (Race)RACE.REALM_CHAOS))
            {
                buTrg.ApplyDamage(dmgBuffer, random, ba, 3);
            }
            else
                buTrg.ApplyDamage(dmgBuffer, random, ba, 0);

            return true;
        }
        static public bool ACT_ApplyRangeAttack(ISkillable source, ISkillable destination, Skill skill, SkillScript skillScript, Battle battle, int[] dmgBuffer, MHRandom random, object data)
        {
            Vector3i position = Vector3i.zero;
            BattleUnit buTrg = destination as BattleUnit;
            BattleUnit bu = source as BattleUnit;

            if (bu == null) Debug.LogError("ACT on invalid type: expected Battle unit in ACT_ApplyRangeAttack");
            
            if (!(data is BattleAttack))
            {
                Debug.LogError("Data is not BattleAttack type");
                return false;
            }
            BattleAttack ba = data as BattleAttack;

            if (buTrg == null) return false;

            if (bu.attributes.Contains(TAG.CHAOS_WEAPON))
            {
                for (int i = bu.figureCount - 1; i >= 0; i--)
                {
                    dmgBuffer[i] = bu.attributes.GetFinal(TAG.RANGE_ATTACK).ToInt() / 2;
                }

                buTrg.canDefend = false;
                buTrg.ApplyDamage(dmgBuffer, random, ba, 0);
                buTrg.canDefend = true;
            }
            else if (buTrg.attributes.Contains(TAG.MISSILE_IMMUNITY))
            {
                buTrg.ApplyDamage(dmgBuffer, random, ba, 50, true);
            }
            else if (buTrg.attributes.Contains(TAG.WEAPON_IMMUNITY) &&
                bu.attributes.DoesNotContains((Tag)TAG.ENCHANTED_WEAPON))
            {
                buTrg.ApplyDamage(dmgBuffer, random, ba, 10, true);
            }
            else if (buTrg.attributes.Contains(TAG.LARGE_SHIELD))
            {
                buTrg.ApplyDamage(dmgBuffer, random, ba, 2);
            }
            else
            {
                buTrg.ApplyDamage(dmgBuffer, random, ba, 0);
            }

            return true;
        }
        static public bool ACT_ApplyMagicRangeAttack(ISkillable source, ISkillable destination, Skill skill, SkillScript skillScript, Battle battle, int[] dmgBuffer, MHRandom random, object data)
        {
            Vector3i position = Vector3i.zero;
            BattleUnit buTrg = destination as BattleUnit;
            BattleUnit bu = source as BattleUnit;

            if (bu == null) Debug.LogError("ACT on invalid type: expected Battle unit in ACT_ApplyMagicRangeAttack");
            
            if (!(data is BattleAttack))
            {
                Debug.LogError("Data is not BattleAttack type");
                return false;
            }
            BattleAttack ba = data as BattleAttack;

            if (skillScript.battleAttackEffect == ESkillBattleAttackEffect.Piercing)
                ba.isPiercing = true;
            if (skillScript.battleAttackEffect == ESkillBattleAttackEffect.Illusion)
                ba.isIllusion = true;

            if (buTrg == null) return false;

            if (bu.attributes.Contains(TAG.CHAOS_WEAPON))
            {
                for (int i = bu.figureCount - 1; i >= 0; i--)
                {
                    dmgBuffer[i] = bu.attributes.GetFinal(TAG.RANGE_ATTACK).ToInt() / 2;
                }

                buTrg.canDefend = false;
                buTrg.ApplyDamage(dmgBuffer, random, ba, 0);
                buTrg.canDefend = true;
            }

            else if (buTrg.attributes.Contains(TAG.MAGIC_IMMUNITY) &&
                !bu.GetSkills().Contains((Skill)SKILL.MAGIC_TECH_RANGE_ATTACK))
            {
                buTrg.ApplyDamage(dmgBuffer, random, ba, 50, true);
            }

            else if (buTrg.attributes.Contains(TAG.RIGHTEOUSNESS) &&
                bu.GetSkills().Contains((Skill)SKILL.MAGIC_CHAOS_RANGE_ATTACK))
            {
                buTrg.ApplyDamage(dmgBuffer, random, ba, 50, true);
            }

            else if (buTrg.attributes.Contains(TAG.ELEMENTAL_ARMOR) &&
                bu.GetSkills().Contains((Skill)SKILL.MAGIC_NATURE_RANGE_ATTACK))
            {
                buTrg.ApplyDamage(dmgBuffer, random, ba, 10, false);
            }

            else if (buTrg.attributes.Contains(TAG.ELEMENTAL_ARMOR) &&
                bu.GetSkills().Contains((Skill)SKILL.MAGIC_CHAOS_RANGE_ATTACK))
            {
                buTrg.ApplyDamage(dmgBuffer, random, ba, 10, false);
            }

            else if (buTrg.attributes.Contains(TAG.RESIST_ELEMENTS) &&
                bu.GetSkills().Contains((Skill)SKILL.MAGIC_NATURE_RANGE_ATTACK))
            {
                buTrg.ApplyDamage(dmgBuffer, random, ba, 3, false);
            }

            else if (buTrg.attributes.Contains(TAG.RESIST_ELEMENTS) &&
                bu.GetSkills().Contains((Skill)SKILL.MAGIC_CHAOS_RANGE_ATTACK))
            {
                buTrg.ApplyDamage(dmgBuffer, random, ba, 3, false);
            }

            else if (buTrg.attributes.Contains(TAG.BLESS) &&
                bu.GetSkills().Contains((Skill)SKILL.MAGIC_CHAOS_RANGE_ATTACK))
            {
                buTrg.ApplyDamage(dmgBuffer, random, ba, 3, false);
            }

            else if (buTrg.attributes.Contains(TAG.LARGE_SHIELD))
            {
                buTrg.ApplyDamage(dmgBuffer, random, ba, 2, false);
            }
            else
            {
                buTrg.ApplyDamage(dmgBuffer, random, ba, 0);
            }

            return true;
        }
        static public bool ACT_ApplyBoulderAreaAttack(ISkillable source, ISkillable destination, Skill skill, SkillScript skillScript, Battle battle, int[] dmgBuffer, MHRandom random, object data)
        {
            Vector3i position = Vector3i.zero;
            BattleUnit buTrg = destination as BattleUnit;
            BattleUnit bu = source as BattleUnit;

            if (bu == null) Debug.LogError("ACT on invalid type: expected Battle unit in ACT_ApplyBoulderAreaAttack");
            
            if (!(data is BattleAttack))
            {
                Debug.LogError("Data is not BattleAttack type");
                return false;
            }
            BattleAttack ba = data as BattleAttack;

            if (buTrg == null) return false;

//             if (buTrg.attributes.Contains(TAG.MISSILE_IMMUNITY))
//             {
//                 //immunity replace figure's defence with value 50
//                 buTrg.ApplyAreaDamage(dmgBuffer, random, ba, 50, true);
//             }
//             else
//             if (buTrg.attributes.Contains(TAG.WEAPON_IMMUNITY))
//             {
//                 buTrg.ApplyAreaDamage(dmgBuffer, random, ba, 10);
//             }
//             else
//             {
                buTrg.ApplyAreaDamage(dmgBuffer, random, ba, 0);
//            }

            return true;
        }
        static public bool ACT_ApplyFireBreathAttack(ISkillable source, ISkillable destination, Skill skill, SkillScript skillScript, Battle battle, int[] dmgBuffer, MHRandom random, object data)
        {
            Vector3i position = Vector3i.zero;
            BattleUnit buTrg = destination as BattleUnit;
            BattleUnit bu = source as BattleUnit;

            if (bu == null) Debug.LogError("ACT on invalid type: expected Battle unit in ACT_ApplyFireBreathAttack");

            if (!(data is BattleAttack))
            {
                Debug.LogError("Data is not BattleAttack type");
                return false;
            }
            BattleAttack ba = data as BattleAttack;

            if (buTrg == null) return false;

            if (buTrg.attributes.Contains(TAG.FIRE_IMMUNITY) ||
                buTrg.attributes.Contains(TAG.MAGIC_IMMUNITY) ||
                buTrg.attributes.Contains(TAG.RIGHTEOUSNESS))
            {
                //immunity replace figure's defence with value 50
                buTrg.ApplyDamage(dmgBuffer, random, ba, 50, true);
            }
            else if (buTrg.attributes.Contains(TAG.ELEMENTAL_ARMOR))
            {
                buTrg.ApplyDamage(dmgBuffer, random, ba, 10);
            }
            else if (buTrg.attributes.Contains(TAG.RESIST_ELEMENTS) ||
                     buTrg.attributes.Contains(TAG.BLESS))
            {
                buTrg.ApplyDamage(dmgBuffer, random, ba, 3);
            }
            else if (buTrg.attributes.Contains(TAG.LARGE_SHIELD))
            {
                buTrg.ApplyDamage(dmgBuffer, random, ba, 2);
            }
            else
            {
                buTrg.ApplyDamage(dmgBuffer, random, ba, 0);
            }

            return true;
        }
        static public bool ACT_ApplyColdBreathAttack(ISkillable source, ISkillable destination, Skill skill, SkillScript skillScript, Battle battle, int[] dmgBuffer, MHRandom random, object data)
        {
            Vector3i position = Vector3i.zero;
            BattleUnit buTrg = destination as BattleUnit;
            BattleUnit bu = source as BattleUnit;

            if (bu == null) Debug.LogError("ACT on invalid type: expected Battle unit in ACT_ApplyColdBreathAttack");

            if (!(data is BattleAttack))
            {
                Debug.LogError("Data is not BattleAttack type");
                return false;
            }
            BattleAttack ba = data as BattleAttack;

            if (buTrg == null) return false;

            if (buTrg.attributes.Contains(TAG.COLD_IMMUNITY) ||
                buTrg.attributes.Contains(TAG.MAGIC_IMMUNITY))
            {
                //immunity replace figure's defence with value 50
                buTrg.ApplyDamage(dmgBuffer, random, ba, 50, true);
            }
            else if (buTrg.attributes.Contains(TAG.ELEMENTAL_ARMOR))
            {
                buTrg.ApplyDamage(dmgBuffer, random, ba, 10);

                Enchantment ench = (Enchantment)ENCH.COLDNESS;
                if(ench != null)
                    buTrg.AddEnchantment((Enchantment)ENCH.COLDNESS, bu, ench.lifeTime, null, 0);
            }
            else if (buTrg.attributes.Contains(TAG.RESIST_ELEMENTS))
            {
                buTrg.ApplyDamage(dmgBuffer, random, ba, 3);

                Enchantment ench = (Enchantment)ENCH.COLDNESS;
                if (ench != null)
                    buTrg.AddEnchantment((Enchantment)ENCH.COLDNESS, bu, ench.lifeTime, null, 0);
            }
            else if (buTrg.attributes.Contains(TAG.LARGE_SHIELD))
            {
                buTrg.ApplyDamage(dmgBuffer, random, ba, 2);

                Enchantment ench = (Enchantment)ENCH.COLDNESS;
                if (ench != null)
                    buTrg.AddEnchantment((Enchantment)ENCH.COLDNESS, bu, ench.lifeTime, null, 0);
            }
            else
            {
                buTrg.ApplyDamage(dmgBuffer, random, ba, 0);

                Enchantment ench = (Enchantment)ENCH.COLDNESS;
                if (ench != null)
                    buTrg.AddEnchantment((Enchantment)ENCH.COLDNESS, bu, ench.lifeTime, null, 0);
            }

            return true;
        }
        static public bool ACT_ApplyImmolationAttack(ISkillable source, ISkillable destination, Skill skill, SkillScript skillScript, Battle battle, int[] dmgBuffer, MHRandom random, object data)
        {
            Vector3i position = Vector3i.zero;
            BattleUnit buTrg = destination as BattleUnit;
            BattleUnit bu = source as BattleUnit;

            if (bu == null) Debug.LogError("ACT on invalid type: expected Battle unit in ACT_ApplyImmolationAttack");

            if (!(data is BattleAttack))
            {
                Debug.LogError("Data is not BattleAttack type");
                return false;
            }
            BattleAttack ba = data as BattleAttack;

            if (buTrg == null) return false;

            if (buTrg.attributes.Contains(TAG.FIRE_IMMUNITY) ||
                buTrg.attributes.Contains(TAG.MAGIC_IMMUNITY) ||
                buTrg.attributes.Contains(TAG.RIGHTEOUSNESS))
            {
                //immunity replace figure's defence with value 50
                buTrg.ApplyImmolationDamage(dmgBuffer, random, ba, 50, true);
            }
            else if (buTrg.attributes.Contains(TAG.ELEMENTAL_ARMOR))
            {
                buTrg.ApplyImmolationDamage(dmgBuffer, random, ba, 10);
            }
            else if (buTrg.attributes.Contains(TAG.RESIST_ELEMENTS) ||
                     buTrg.attributes.Contains(TAG.BLESS))
            {
                buTrg.ApplyImmolationDamage(dmgBuffer, random, ba, 3);
            }
            else if (buTrg.attributes.Contains(TAG.LARGE_SHIELD))
            {
                buTrg.ApplyImmolationDamage(dmgBuffer, random, ba, 2);
            }
            else
            {
                buTrg.ApplyImmolationDamage(dmgBuffer, random, ba, 0);
            }

            return true;
        }
        static public bool ACT_ApplyLightningBreathAttack(ISkillable source, ISkillable destination, Skill skill, SkillScript skillScript, Battle battle, int[] dmgBuffer, MHRandom random, object data)
        {
            Vector3i position = Vector3i.zero;
            BattleUnit buTrg = destination as BattleUnit;
            BattleUnit bu = source as BattleUnit;

            if (bu == null) Debug.LogError("ACT on invalid type: expected Battle unit in ACT_ApplyLightningBreathAttack");

            if (!(data is BattleAttack))
            {
                Debug.LogError("Data is not BattleAttack type");
                return false;
            }
            BattleAttack ba = data as BattleAttack;

            if (buTrg == null) return false;

            //add piercing damage
            ba.isPiercing = true;

            //checking immunities and defense modifiers
            if (buTrg.attributes.Contains(TAG.MAGIC_IMMUNITY) ||
                buTrg.attributes.Contains(TAG.RIGHTEOUSNESS))
            {
                //immunity replace figure's defence with value 50
                buTrg.ApplyDamage(dmgBuffer, random, ba, 50, true);
            }
            else if (buTrg.attributes.Contains(TAG.ELEMENTAL_ARMOR))
            {
                buTrg.ApplyDamage(dmgBuffer, random, ba, 10);
            }
            else if (buTrg.attributes.Contains(TAG.RESIST_ELEMENTS) ||
                     buTrg.attributes.Contains(TAG.BLESS))
            {
                buTrg.ApplyDamage(dmgBuffer, random, ba, 3);
            }
            else if (buTrg.attributes.Contains(TAG.LARGE_SHIELD))
            {
                buTrg.ApplyDamage(dmgBuffer, random, ba, 2);
            }
            else
            {
                buTrg.ApplyDamage(dmgBuffer, random, ba, 0);
            }

            return true;
        }
        static public bool ACT_ApplyThrowAttack(ISkillable source, ISkillable destination, Skill skill, SkillScript skillScript, Battle battle, int[] dmgBuffer, MHRandom random, object data)
        {
            Vector3i position = Vector3i.zero;
            BattleUnit buTrg = destination as BattleUnit;
            BattleUnit bu = source as BattleUnit;

            if (bu == null) Debug.LogError("ACT on invalid type: expected Battle unit in ACT_ApplyThrowAttack");

            if (!(data is BattleAttack))
            {
                Debug.LogError("Data is not BattleAttack type");
                return false;
            }
            BattleAttack ba = data as BattleAttack;

            if (buTrg == null) return false;

            //if (buTrg.attributes.Contains(TAG.MISSILE_IMMUNITY))
            //    buTrg.ApplyDamage(dmgBuffer, random, ba, 50, true);

            if (bu.attributes.Contains(TAG.CHAOS_WEAPON) != false)
            {
                for (int i = bu.figureCount - 1; i >= 0; i--)
                {
                    dmgBuffer[i] = dmgBuffer[i] / 2;
                }

                buTrg.canDefend = false;
                buTrg.ApplyDamage(dmgBuffer, random, ba, 0);
                buTrg.canDefend = true;
            }
            else if (buTrg.attributes.Contains(TAG.LARGE_SHIELD))
                buTrg.ApplyDamage(dmgBuffer, random, ba, 2);
            else
                buTrg.ApplyDamage(dmgBuffer, random, ba, 0);

            return true;
        }
        static public bool ACT_ApplyDeathAttack(ISkillable source, ISkillable destination, Skill skill, SkillScript skillScript, Battle battle, int[] dmgBuffer, MHRandom random, object data)
        {
            Vector3i position = Vector3i.zero;
            BattleUnit buTrg = destination as BattleUnit;
            BattleUnit bu = source as BattleUnit;
            var resistReduction = skillScript.fIntParam.ToInt();

            if (bu == null) Debug.LogError("ACT on invalid type: expected Battle unit in ACT_ApplyDeathAttack");

            if (!(data is BattleAttack))
            {
                Debug.LogError("Data is not BattleAttack type");
                return false;
            }

            if (buTrg == null) return false;

            if (buTrg.attributes.Contains(TAG.MAGIC_IMMUNITY) ||
                buTrg.attributes.Contains(TAG.DEATH_IMMUNITY) ||
                buTrg.attributes.Contains(TAG.RIGHTEOUSNESS))
                buTrg.ApplyResistFigureDeath(random, resistReduction, 50, true);
            else if (buTrg.attributes.Contains(TAG.BLESS))
                buTrg.ApplyResistFigureDeath(random, resistReduction, 3);
            else
                buTrg.ApplyResistFigureDeath(random, resistReduction);

            return true;
        }
        static public bool ACT_ApplyStoningAttack(ISkillable source, ISkillable destination, Skill skill, SkillScript skillScript, Battle battle, int[] dmgBuffer, MHRandom random, object data)
        {
            Vector3i position = Vector3i.zero;
            BattleUnit buTrg = destination as BattleUnit;
            BattleUnit bu = source as BattleUnit;
            BattleAttack ba = data as BattleAttack;
            var resistReduction = skillScript.fIntParam.ToInt();

            if (bu == null) Debug.LogError("ACT on invalid type: expected Battle unit in ACT_ApplyStoningAttack");

            if (!(data is BattleAttack))
            {
                Debug.LogError("Data is not BattleAttack type");
                return false;
            }

            if (buTrg == null) return false;

            if (buTrg.attributes.Contains(TAG.MAGIC_IMMUNITY) ||
                buTrg.attributes.Contains(TAG.STONING_IMMUNITY))
                buTrg.ApplyResistFigureDeath(random, resistReduction, 50, true, null, null, null, ba);
            else if (buTrg.attributes.Contains(TAG.ELEMENTAL_ARMOR))
                buTrg.ApplyResistFigureDeath(random, resistReduction, 10, false, null, null, null, ba);
            else if (buTrg.attributes.Contains(TAG.RESIST_ELEMENTS))
                buTrg.ApplyResistFigureDeath(random, resistReduction, 3, false, null, null, null, ba);
            else
                buTrg.ApplyResistFigureDeath(random, resistReduction, 0, false, null, null, null, ba);

            return true;
        }
        static public bool ACT_ApplyStoningAttackPerAttackFigure(ISkillable source, ISkillable destination, Skill skill, SkillScript skillScript, Battle battle, int[] dmgBuffer, MHRandom random, object data)
        {
            Vector3i position = Vector3i.zero;
            BattleUnit buTrg = destination as BattleUnit;
            BattleUnit bu = source as BattleUnit;
            BattleAttack ba = data as BattleAttack;
            var resistReduction = skillScript.fIntParam.ToInt();

            if (bu == null) Debug.LogError("ACT on invalid type: expected Battle unit in ACT_ApplyStoningAttackPerAttackFigure");

            if (!(data is BattleAttack))
            {
                Debug.LogError("Data is not BattleAttack type");
                return false;
            }

            if (buTrg == null) return false;

            if (buTrg.attributes.Contains(TAG.MAGIC_IMMUNITY) ||
                buTrg.attributes.Contains(TAG.STONING_IMMUNITY))
            {
                for (int i = 0; i < bu.FigureCount(); i++)
                    buTrg.ApplyResistOneFigureDeath(random, resistReduction, 50, true, null, null, null, ba);
            }
            else if (buTrg.attributes.Contains(TAG.ELEMENTAL_ARMOR))
            {
                for (int i = 0; i < bu.FigureCount(); i++)
                    buTrg.ApplyResistOneFigureDeath(random, resistReduction, 10, false, null, null, null, ba);
            }
            else if (buTrg.attributes.Contains(TAG.RESIST_ELEMENTS))
            {
                for (int i = 0; i < bu.FigureCount(); i++)
                    buTrg.ApplyResistOneFigureDeath(random, resistReduction, 3, false, null, null, null, ba);
            }
            else
            {
                for (int i = 0; i < bu.FigureCount(); i++)
                    buTrg.ApplyResistOneFigureDeath(random, resistReduction, 0, false, null, null, null, ba);
            }

            return true;
        }
        static public bool ACT_ApplyItemDestrucionAttack(ISkillable source, ISkillable destination, Skill skill, SkillScript skillScript, Battle battle, int[] dmgBuffer, MHRandom random, object data)
        {
            Vector3i position = Vector3i.zero;
            BattleUnit buTrg = destination as BattleUnit;
            BattleAttack ba = data as BattleAttack;
            BattleUnit bu = source as BattleUnit;


            if (bu == null) Debug.LogError("ACT on invalid type: expected Battle unit in ACT_ApplyItemDestrucionAttack");

            if (!(data is BattleAttack))
            {
                Debug.LogError("Data is not BattleAttack type");
                return false;
            }

            FInt resModFromParam = skill.script[0].fIntParam;
            if (resModFromParam == null)
                resModFromParam = FInt.ZERO;

            // if caster is hero
            BattleUnit hero = null;
            int resModFromHero = 0;
            if (source is BattleUnit)
            {
                hero = source as BattleUnit;
                resModFromHero = hero.attributes.GetFinal((Tag)TAG.SPELL_SAVE).ToInt();
            }

            if (buTrg == null) return false;

            if (buTrg.attributes.Contains(TAG.MAGIC_IMMUNITY) || buTrg.attributes.Contains(TAG.RIGHTEOUSNESS))
                buTrg.ApplyResistOneFigureDeath(random, resModFromHero + resModFromParam.ToInt(), 50, true, null, null, null, ba);
            else if(buTrg.attributes.Contains(TAG.ELEMENTAL_ARMOR))
                buTrg.ApplyResistOneFigureDeath(random, resModFromHero + resModFromParam.ToInt(), 10, false, null, null, null, ba);
            else if (buTrg.attributes.Contains(TAG.RESIST_ELEMENTS) || buTrg.attributes.Contains(TAG.BLESS))
                buTrg.ApplyResistOneFigureDeath(random, resModFromHero + resModFromParam.ToInt(), 3, false, null, null, null, ba);
            else
                buTrg.ApplyResistOneFigureDeath(random, resModFromHero + resModFromParam.ToInt(), 0, false, null, null, null, ba);

            return true;
        }
        static public bool ACT_ApplyItemDeathAttack(ISkillable source, ISkillable destination, Skill skill, SkillScript skillScript, Battle battle, int[] dmgBuffer, MHRandom random, object data)
        {
            Vector3i position = Vector3i.zero;
            BattleUnit buTrg = destination as BattleUnit;
            BattleAttack ba = data as BattleAttack;
            BattleUnit bu = source as BattleUnit;


            if (bu == null) Debug.LogError("ACT on invalid type: expected Battle unit in ACT_ApplyItemDeathAttack");

            if (!(data is BattleAttack))
            {
                Debug.LogError("Data is not BattleAttack type");
                return false;
            }

            FInt resModFromParam = FInt.ZERO;
            if (skill.script[0] != null && skill.script[0].fIntParam != null)
                resModFromParam = skill.script[0].fIntParam;

            // if caster is hero
            BattleUnit hero = null;
            int resModFromHero = 0;
            if (source is BattleUnit)
            {
                hero = source as BattleUnit;
                resModFromHero = hero.attributes.GetFinal((Tag)TAG.SPELL_SAVE).ToInt();
            }

            if (buTrg == null) return false;

            if (buTrg.attributes.Contains(TAG.MAGIC_IMMUNITY) || 
                buTrg.attributes.Contains(TAG.RIGHTEOUSNESS) ||
                buTrg.attributes.Contains(TAG.DEATH_IMMUNITY))
                buTrg.ApplyResistOneFigureDeath(random, resModFromHero + resModFromParam.ToInt(), 50, true, null, null, null, ba);
            else if (buTrg.attributes.Contains(TAG.BLESS))
                buTrg.ApplyResistOneFigureDeath(random, resModFromHero + resModFromParam.ToInt(), 3, false, null, null, null, ba);
            else
                buTrg.ApplyResistOneFigureDeath(random, resModFromHero + resModFromParam.ToInt(), 0, false, null, null, null, ba);

            return true;
        }
        static public bool ACT_ApplyDoomAttack(ISkillable source, ISkillable destination, Skill skill, SkillScript skillScript, Battle battle, int[] dmgBuffer, MHRandom random, object data)
        {
            Vector3i position = Vector3i.zero;
            BattleUnit buTrg = destination as BattleUnit;
            BattleUnit bu = source as BattleUnit;
            int[] dam = new int[] { skillScript.fIntParam.ToInt() };

            if (bu == null) Debug.LogError("ACT on invalid type: expected Battle unit in ACT_ApplyDoomAttack");

            if (!(data is BattleAttack))
            {
                Debug.LogError("Data is not BattleAttack type");
                return false;
            }
            BattleAttack ba = data as BattleAttack;

            if (buTrg == null) return false;
            if (buTrg.attributes.Contains(TAG.MAGIC_IMMUNITY) ||
                buTrg.attributes.Contains(TAG.RIGHTEOUSNESS)) return false;

            dam[0] += bu.attributes.GetFinal(TAG.DOOM_GAZE_BONUS).ToInt();
            buTrg.canDefend = false;
            buTrg.ApplyDamage(dam, random, ba, 0);
            buTrg.canDefend = true;

            return true;
        }
		
        static public bool ACT_ApplyPoisonAttack(ISkillable source, ISkillable destination, Skill skill, SkillScript skillScript, Battle battle, int[] dmgBuffer, MHRandom random, object data)
        {
            Vector3i position = Vector3i.zero;
            BattleUnit buTrg = destination as BattleUnit;
            BattleUnit bu = source as BattleUnit;

            if (bu == null) Debug.LogError("ACT on invalid type: expected Battle unit in ACT_ApplyPoisonAttack");

            if (!(data is BattleAttack))
            {
                Debug.LogError("Data is not BattleAttack type");
                return false;
            }
            BattleAttack ba = data as BattleAttack;

            if (buTrg == null) return false;

            if (buTrg.attributes.Contains(TAG.POISON_IMMUNITY))
                buTrg.ApplyPoisonDmg(dmgBuffer, random, ba, 0, 50, true);
            else
                buTrg.ApplyPoisonDmg(dmgBuffer, random, ba, 0);

            return true;
        }
        static public bool ACT_ApplyDispelEvilAttack(ISkillable source, ISkillable destination, Skill skill, SkillScript skillScript, Battle battle, int[] dmgBuffer, MHRandom random, object data)
        {
            Vector3i position = Vector3i.zero;
            BattleUnit buTrg = destination as BattleUnit;
            
            var race = buTrg.race;
            BattleUnit bu = source as BattleUnit;
            var resistReductionChaos = 4;
            var resistReductionDeath = 9;

            if (bu == null) Debug.LogError("ACT on invalid type: expected Battle unit in ACT_ApplyDispelEvilAttack");

            if (!(data is BattleAttack))
            {
                Debug.LogError("Data is not BattleAttack type");
                return false;
            }

            if (buTrg == null) return false;

            BattleAttack ba = data as BattleAttack;

            if (buTrg.attributes.Contains(TAG.MAGIC_IMMUNITY) &&
                race == (Race)RACE.REALM_DEATH)
                buTrg.ApplyResistFigureDeath(random, resistReductionDeath, 50, true, null, null, null, ba);
            else if (buTrg.attributes.Contains(TAG.MAGIC_IMMUNITY) &&
                race == (Race)RACE.REALM_CHAOS)
                buTrg.ApplyResistFigureDeath(random, resistReductionChaos, 50, true, null, null, null, ba);
            else if (race == (Race)RACE.REALM_DEATH)
                buTrg.ApplyResistFigureDeath(random, resistReductionDeath, 0, false, null, null, null, ba);
            else if (race == (Race)RACE.REALM_CHAOS)
                buTrg.ApplyResistFigureDeath(random, resistReductionChaos, 0, false, null, null, null, ba);

            return true;
        }
        static public bool ACT_ApplyLifeStealingAttack(ISkillable source, ISkillable destination, Skill skill, SkillScript skillScript, Battle battle, int[] dmgBuffer, MHRandom random, object data)
        {
            Vector3i position = Vector3i.zero;
            BattleUnit buTrg = destination as BattleUnit;
            BattleUnit bu = source as BattleUnit;
            var resistReduction = dmgBuffer[0];

            if (bu == null) Debug.LogError("ACT on invalid type: expected Battle unit in ACT_ApplyLifeStealingAttack");

            if (!(data is BattleAttack))
            {
                Debug.LogError("Data is not BattleAttack type");
                return false;
            }
            BattleAttack ba = data as BattleAttack;

            if (buTrg == null) return false;

            if (buTrg.attributes.Contains(TAG.MAGIC_IMMUNITY) ||
                buTrg.attributes.Contains(TAG.DEATH_IMMUNITY) ||
                buTrg.attributes.Contains(TAG.RIGHTEOUSNESS))
                //immunity replace figure's defence with value 50
                buTrg.ApplyLifeStealDmg(bu, random, ba, resistReduction, 50, true);
            else if (buTrg.attributes.Contains(TAG.BLESS))
                buTrg.ApplyLifeStealDmg(bu, random, ba, resistReduction, 3);
            else
                buTrg.ApplyLifeStealDmg(bu, random, ba, resistReduction);

            return true;
        }
        static public bool ACT_ApplyPowerDrainAttack(ISkillable source, ISkillable destination, Skill skill, SkillScript skillScript, Battle battle, int[] dmgBuffer, MHRandom random, object data)
        {
            Vector3i position = Vector3i.zero;
            BattleUnit buTrg = destination as BattleUnit;
            BattleUnit bu = source as BattleUnit;
            var powerDrain = dmgBuffer[0];

            if (bu == null) Debug.LogError("ACT on invalid type: expected Battle unit in ACT_ApplyPowerDrainAttack");

            if (!(data is BattleAttack))
            {
                Debug.LogError("Data is not BattleAttack type");
                return false;
            }
            BattleAttack ba = data as BattleAttack;

            if (buTrg == null || buTrg.GetAttributes().GetFinal(TAG.MANA_POINTS) == 0) return false;

            if (buTrg.attributes.Contains(TAG.MAGIC_IMMUNITY) ||
                buTrg.attributes.Contains(TAG.DEATH_IMMUNITY) ||
                buTrg.attributes.Contains(TAG.RIGHTEOUSNESS)) 
                return true;
            else if (buTrg.attributes.Contains(TAG.BLESS))
                buTrg.ApplyPowerDraintDmg(bu, random, ba, powerDrain, 1);
            else
                buTrg.ApplyPowerDraintDmg(bu, random, ba, powerDrain);

            return true;
        }
        static public int ACT_Noble(ISkillable source, ISkillable destination, Skill skill, SkillScript skillScript, Battle battle, int[] dmgBuffer, MHRandom random, object data)
        {
            BaseUnit unit = source as BaseUnit;
            int lvl = unit.GetLevel();

            switch (lvl)
            {
                case 1:
                    return 10;
                case 2:
                    return 20;
                case 3:
                    return 30;
                case 4:
                    return 40;
                case 5:
                    return 50;
                case 6:
                    return 60;
                case 7:
                    return 70;
                case 8:
                    return 80;
                case 9:
                    return 90;
                default:
                    return 10;
            }

        }
        static public int ACT_Peasant(ISkillable source, ISkillable destination, Skill skill, SkillScript skillScript, Battle battle, int[] dmgBuffer, MHRandom random, object data)
        {
            BaseUnit unit = source as BaseUnit;
            int lvl = unit.GetLevel();

            switch (lvl)
            {
                case 1:
                case 2:
                    return 2;
                case 3:
                case 4:
                    return 4;
                case 5:
                case 6:
                    return 6;
                case 7:
                case 8:
                    return 8;
                case 9:
                    return 10;
                default:
                    return 2;
            }

        }
        static public int ACT_Fame(ISkillable source, ISkillable destination, Skill skill, SkillScript skillScript, Battle battle, int[] dmgBuffer, MHRandom random, object data)
        {
            BaseUnit unit = source as BaseUnit;
            var wizardOwner = unit.GetWizardOwner();
            if (wizardOwner == null) return 0;

            int lvl = unit.GetLevel();

            int fameBonus = (int)(lvl * 3f);
            
            return fameBonus;

        }
        static public int ACT_FameSuper(ISkillable source, ISkillable destination, Skill skill, SkillScript skillScript, Battle battle, int[] dmgBuffer, MHRandom random, object data)
        {
            BaseUnit unit = source as BaseUnit;
            int lvl = unit.GetLevel();

            int fameBonus = (int)(unit.GetLevel() * 4.5f);

            var wizardOwner = unit.GetWizardOwner();
            if (wizardOwner == null) return 0;

            return fameBonus;

        }
        static public object ACT_HolyBonus(ISkillable source, ISkillable destination, Skill skill, SkillScript skillScript, Battle battle, int[] dmgBuffer, MHRandom random, object data)
        {
            MOM.Unit unit = source as MOM.Unit;
            if (unit == null || unit.group == null) return null;
            var group = unit.group;


            return null;
        }
        static public bool ACT_ApplyItemHolyAvengerAttack(ISkillable source, ISkillable destination, Skill skill, SkillScript skillScript, Battle battle, int[] dmgBuffer, MHRandom random, object data)
        {
            Vector3i position = Vector3i.zero;
            BattleUnit buTrg = destination as BattleUnit;

            var race = buTrg.race;
            BattleUnit bu = source as BattleUnit;
            var resistReductionChaos = 4;
            var resistReductionDeath = 9;

            if (bu == null) Debug.LogError("ACT on invalid type: expected Battle unit in ACT_ApplyItemHolyAvengerAttack");

            if (!(data is BattleAttack))
            {
                Debug.LogError("Data is not BattleAttack type");
                return false;
            }

            if (buTrg == null) return false;

            BattleAttack ba = data as BattleAttack;

            if (buTrg.attributes.Contains(TAG.MAGIC_IMMUNITY) &&
                race == (Race)RACE.REALM_DEATH)
                buTrg.ApplyResistOneFigureDeath(random, resistReductionDeath, 50, true, null, null, null, ba);
            else if (buTrg.attributes.Contains(TAG.MAGIC_IMMUNITY) &&
                race == (Race)RACE.REALM_CHAOS)
                buTrg.ApplyResistOneFigureDeath(random, resistReductionChaos, 50, true, null, null, null, ba);
            else if (race == (Race)RACE.REALM_DEATH)
                buTrg.ApplyResistOneFigureDeath(random, resistReductionDeath, 0, false, null, null, null, ba);
            else if (race == (Race)RACE.REALM_CHAOS)
                buTrg.ApplyResistOneFigureDeath(random, resistReductionChaos, 0, false, null, null, null, ba);

            return true;
        }
        static public bool ACT_ApplyBleeding(ISkillable source, ISkillable destination, Skill skill, SkillScript skillScript, Battle battle, int[] dmgBuffer, MHRandom random, object data)
        {
            Vector3i position = Vector3i.zero;
            BattleUnit buTrg = destination as BattleUnit;
            BattleUnit bu = source as BattleUnit;
            BattleAttack ba = data as BattleAttack;

            if (bu == null) Debug.LogError("ACT on invalid type: expected Battle unit in ACT_ApplyBleeding");

            if (!(data is BattleAttack))
            {
                Debug.LogError("Data is not BattleAttack type");
                return false;
            }

            if (!buTrg.attributes.Contains(TAG.DEATH_IMMUNITY) &&
                !buTrg.attributes.Contains(TAG.MECHANICAL_UNIT))
            {
                buTrg.ApplyBleeding(bu, null, null, ba);
            }

            return true;
        }
        static public bool ACT_ApplyStun(ISkillable source, ISkillable destination, Skill skill, SkillScript skillScript, Battle battle, int[] dmgBuffer, MHRandom random, object data)
        {
            Vector3i position = Vector3i.zero;
            BattleUnit buTrg = destination as BattleUnit;
            BattleUnit bu = source as BattleUnit;
            BattleAttack ba = data as BattleAttack;

            if (bu == null) Debug.LogError("ACT on invalid type: expected Battle unit in ACT_ApplyStun");

            if (!(data is BattleAttack))
            {
                Debug.LogError("Data is not BattleAttack type");
                return false;
            }

            FInt resModFromParam = FInt.ZERO;
            if (skill.script[0] != null && skill.script[0].fIntParam != null)
                resModFromParam = skill.script[0].fIntParam;
                
            if (resModFromParam == null)
                resModFromParam = FInt.ZERO;

            // if caster is hero
            BattleUnit hero = null;
            int resModFromHero = 0;
            if (source is BattleUnit)
            {
                hero = source as BattleUnit;
                resModFromHero = hero.attributes.GetFinal((Tag)TAG.SPELL_SAVE).ToInt();
            }

            buTrg.ApplyResistStun(random, resModFromHero + resModFromParam.ToInt(), 0, false, null, null, null, ba);

            return true;
        }
        static public bool ACT_ApplyBiteAttack(ISkillable source, ISkillable destination, Skill skill, SkillScript skillScript, Battle battle, int[] dmgBuffer, MHRandom random, object data)
        {
            Vector3i position = Vector3i.zero;
            BattleUnit buTrg = destination as BattleUnit;
            BattleUnit bu = source as BattleUnit;

            if (bu == null) Debug.LogError("ACT on invalid type: expected Battle unit in ACT_ApplyBiteAttack");

            if (!(data is BattleAttack))
            {
                Debug.LogError("Data is not BattleAttack type");
                return false;
            }
            BattleAttack ba = data as BattleAttack;

            if (buTrg == null) return false;

            if (buTrg.attributes.Contains(TAG.LARGE_SHIELD))
                buTrg.ApplyDamage(dmgBuffer, random, ba, 2);
            else
                buTrg.ApplyDamage(dmgBuffer, random, ba, 0);

            return true;
        }
        static public bool ACT_ApplyBombAttack(ISkillable source, ISkillable destination, Skill skill, SkillScript skillScript, Battle battle, int[] dmgBuffer, MHRandom random, object data)
        {
            Vector3i position = Vector3i.zero;
            BattleUnit buTrg = destination as BattleUnit;
            BattleUnit bu = source as BattleUnit;

            if (bu == null) Debug.LogError("ACT on invalid type: expected Battle unit in ACT_ApplyBombAttack");

            if (!(data is BattleAttack))
            {
                Debug.LogError("Data is not BattleAttack type");
                return false;
            }
            BattleAttack ba = data as BattleAttack;

            if (buTrg == null) return false;

            if (buTrg.attributes.Contains(TAG.MAGIC_IMMUNITY) ||
                buTrg.attributes.Contains(TAG.FIRE_IMMUNITY) ||
                buTrg.attributes.Contains(TAG.RIGHTEOUSNESS))
            {
                buTrg.ApplyImmolationDamage(dmgBuffer, random, ba, 50, true);
            }
            else if (buTrg.attributes.Contains(TAG.ELEMENTAL_ARMOR))
            {
                buTrg.ApplyImmolationDamage(dmgBuffer, random, ba, 10);
            }
            else if (buTrg.attributes.Contains(TAG.RESIST_ELEMENTS) ||
                    buTrg.attributes.Contains(TAG.BLESS))
            {
                buTrg.ApplyImmolationDamage(dmgBuffer, random, ba, 3);
            }
            else if (buTrg.attributes.Contains(TAG.LARGE_SHIELD))
            {
                buTrg.ApplyImmolationDamage(dmgBuffer, random, ba, 2);
            }
            else
                buTrg.ApplyImmolationDamage(dmgBuffer, random, ba, 0);

            //Bomber Killself
            bu.canDefend = false;
            var unitHp = bu.GetMaxFigureCount() * bu.GetBaseFigure().maxHitPoints;
            bu.ApplyDamage(new int[] { unitHp }, random, ba, 0);
            BattleHUD.Get()?.UnselectUnit();

            return true;
        }
        static public bool ACT_ApplyMudAttack(ISkillable source, ISkillable destination, Skill skill, SkillScript skillScript, Battle battle, int[] dmgBuffer, MHRandom random, object data)
        {
            Vector3i position = Vector3i.zero;
            BattleUnit buTrg = destination as BattleUnit;
            BattleUnit bu = source as BattleUnit;
            BattleAttack ba = data as BattleAttack;
            var resistReduction = skillScript.fIntParam.ToInt();

            if (bu == null) Debug.LogError("ACT on invalid type: expected Battle unit in ACT_ApplyMudAttack");

            if (!(data is BattleAttack))
            {
                Debug.LogError("Data is not BattleAttack type");
                return false;
            }

            if (buTrg == null) return false;

            if (buTrg.attributes.Contains(TAG.MAGIC_IMMUNITY))
                buTrg.ApplyResistMud(random, resistReduction, 50, true, null, null, null, ba);
            else if (buTrg.attributes.Contains(TAG.ELEMENTAL_ARMOR))
                buTrg.ApplyResistMud(random, resistReduction, 10, false, null, null, null, ba);
            else if (buTrg.attributes.Contains(TAG.RESIST_ELEMENTS))
                buTrg.ApplyResistMud(random, resistReduction, 3, false, null, null, null, ba);
            else
                buTrg.ApplyResistMud(random, resistReduction, 0, false, null, null, null, ba);

            return true;
        }

        #endregion
        #region Passive activators (to be used in attribute processing)
        /// <param name="source"> unit producing damage/attack </param>
        /// <param name="skill"> skill used to attack </param>
        /// <param name="skillScript"> skillScript used </param>
        /// <param name="retAttribute"> dictionary of the curent attribute library </param>
        /// <returns> change data </returns> 
        static public object ACTPass_Empty(ISkillable source, Skill skill, SkillScript skillScript, NetDictionary<DBReference<Tag>, FInt> retAttribute)
        {
            return null;
        }
        static public object ACTPass_ToHit(ISkillable source, Skill skill, SkillScript skillScript, NetDictionary<DBReference<Tag>, FInt> retAttribute)
        {
            var t1 = (Tag)TAG.MELEE_ATTACK_CHANCE;
            var t2 = (Tag)TAG.RANGE_ATTACK_CHANCE;

            if (retAttribute.ContainsKey(t1))
            {
                retAttribute.AddFinal(t1, skillScript.fIntParam);
            }
            if (retAttribute.ContainsKey(t2))
            {
                retAttribute.AddFinal(t2, skillScript.fIntParam);
            }


            //TODo change data implementation

            return null;
        }
        static public object ACTPass_ToHitMelee(ISkillable source, Skill skill, SkillScript skillScript, NetDictionary<DBReference<Tag>, FInt> retAttribute)
        {
            var t1 = (Tag)TAG.MELEE_ATTACK_CHANCE;

            retAttribute.AddFinal(t1, skillScript.fIntParam);

            //TODo change data implementation

            return null;
        }
        static public object ACTPass_ToHitRange(ISkillable source, Skill skill, SkillScript skillScript, NetDictionary<DBReference<Tag>, FInt> retAttribute)
        {
            var t1 = (Tag)TAG.RANGE_ATTACK_CHANCE;

            retAttribute.AddFinal(t1, skillScript.fIntParam);

            //TODo change data implementation

            return null;
        }
        static public object ACTPass_LevelBonus(ISkillable source, Skill skill, SkillScript skillScript, NetDictionary<DBReference<Tag>, FInt> retAttribute)
        {
            BattleUnit bu = source as BattleUnit;

            var level = bu.GetLevel();

            if (level < 2) return null;


            if (bu.dbSource.Get() is Hero)
            {
                var increse = new FInt(level - 1);
                retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK, increse);
                retAttribute.AddFinal((Tag)TAG.THROW_BONUS, increse);
                retAttribute.AddFinal((Tag)TAG.FIRE_BREATH_BONUS, increse);
                retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK, increse);
                retAttribute.AddFinal((Tag)TAG.RESIST, increse);
                retAttribute.AddFinal((Tag)TAG.HIT_POINTS, increse);

                increse = new FInt(level / 2);
                retAttribute.AddFinal((Tag)TAG.DEFENCE, increse);

                increse = new FInt(level / 3);
                retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK_CHANCE, increse);
                retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK_CHANCE, increse);                
            }
            else
            {
                var increse = new FInt(level - 1);
                retAttribute.AddFinal((Tag)TAG.RESIST, increse);

                increse = new FInt(level / 2);
                retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK, increse);
                retAttribute.AddFinal((Tag)TAG.THROW_BONUS, increse);
                retAttribute.AddFinal((Tag)TAG.FIRE_BREATH_BONUS, increse);
                retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK, increse);
                retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK_CHANCE, increse);
                retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK_CHANCE, increse);

                increse = new FInt(level / 3);
                retAttribute.AddFinal((Tag)TAG.DEFENCE, increse);                
                retAttribute.AddFinal((Tag)TAG.HIT_POINTS, increse);
            }
            return null;
        }
        static public object ACTPass_LuckUnit(ISkillable source, Skill skill, SkillScript skillScript, NetDictionary<DBReference<Tag>, FInt> retAttribute)
        {
            var t1 = (Tag)TAG.MELEE_ATTACK_CHANCE;
            var t2 = (Tag)TAG.RANGE_ATTACK_CHANCE;
            var t3 = (Tag)TAG.DEFENCE_CHANCE;            

            FInt val = new FInt(0.1f);            
            retAttribute.AddFinal(t1, val);
            retAttribute.AddFinal(t2, val);
            retAttribute.AddFinal(t3, val);

            var t4 = (Tag)TAG.RESIST;            
            retAttribute.AddFinal(t4, FInt.ONE);

            return null;
        }


        static public object ACTPass_ArcanePower(ISkillable source, Skill skill, SkillScript skillScript, NetDictionary<DBReference<Tag>, FInt> retAttribute)
        {
            BaseUnit unit = source as BaseUnit;
            FInt lvl = new FInt(unit.GetLevel());
            retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK, lvl);            

            return null;
        }
        static public object ACTPass_ArcanePowerSuper(ISkillable source, Skill skill, SkillScript skillScript, NetDictionary<DBReference<Tag>, FInt> retAttribute)
        {
            BaseUnit unit = source as BaseUnit;
            FInt lvl = new FInt((int)( unit.GetLevel() * 1.5f));

            retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK, lvl);

            return null;
        }
        static public object ACTPass_Agility(ISkillable source, Skill skill, SkillScript skillScript, NetDictionary<DBReference<Tag>, FInt> retAttribute)
        {
            BaseUnit unit = source as BaseUnit;
            FInt lvl = new FInt(unit.GetLevel());
            retAttribute.AddFinal((Tag)TAG.DEFENCE, lvl);            

            return null;
        }
        static public object ACTPass_AgilitySuper(ISkillable source, Skill skill, SkillScript skillScript, NetDictionary<DBReference<Tag>, FInt> retAttribute)
        {
            BaseUnit unit = source as BaseUnit;
            FInt lvl = new FInt((int)(unit.GetLevel() * 1.5f));

            retAttribute.AddFinal((Tag)TAG.DEFENCE, lvl);

            return null;
        }
        static public object ACTPass_Noble(ISkillable source, Skill skill, SkillScript skillScript, NetDictionary<DBReference<Tag>, FInt> retAttribute)
        {
            retAttribute.SetFinal((Tag)TAG.UPKEEP_GOLD, FInt.ZERO);

            return null;
        }
        static public object ACTPass_Blademaster(ISkillable source, Skill skill, SkillScript skillScript, NetDictionary<DBReference<Tag>, FInt> retAttribute)
        {
            BaseUnit unit = source as BaseUnit;
            var level = unit.GetLevel();
            if (level < 2) return null;
            FInt lvl = new FInt((int)(level / 2));
            lvl *= 0.1f;

            retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK_CHANCE, lvl);
            retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK_CHANCE, lvl);

            return null;
        }
        static public object ACTPass_BlademasterSuper(ISkillable source, Skill skill, SkillScript skillScript, NetDictionary<DBReference<Tag>, FInt> retAttribute)
        {
            BaseUnit unit = source as BaseUnit;
            var level = unit.GetLevel();
            if (level < 2) return null;
            FInt lvl = new FInt((int)(level * 1.5f / 2));
            lvl *= 0.1f;

            retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK_CHANCE, lvl);
            retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK_CHANCE, lvl);

            return null;
        }
        static public object ACTPass_Constitution(ISkillable source, Skill skill, SkillScript skillScript, NetDictionary<DBReference<Tag>, FInt> retAttribute)
        {
            BaseUnit unit = source as BaseUnit;
            FInt lvl = new FInt(unit.GetLevel());
            retAttribute.AddFinal((Tag)TAG.HIT_POINTS, lvl);

            return null;
        }
        static public object ACTPass_ConstitutionSuper(ISkillable source, Skill skill, SkillScript skillScript, NetDictionary<DBReference<Tag>, FInt> retAttribute)
        {
            BaseUnit unit = source as BaseUnit;
            FInt lvl = new FInt((int)(unit.GetLevel() * 1.5f));

            retAttribute.AddFinal((Tag)TAG.HIT_POINTS, lvl);

            return null;
        }
        static public object ACTPass_Might(ISkillable source, Skill skill, SkillScript skillScript, NetDictionary<DBReference<Tag>, FInt> retAttribute)
        {
            BaseUnit unit = source as BaseUnit;
            FInt lvl = new FInt(unit.GetLevel());
            retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK, lvl);

            return null;
        }
        static public object ACTPass_MightSuper(ISkillable source, Skill skill, SkillScript skillScript, NetDictionary<DBReference<Tag>, FInt> retAttribute)
        {
            BaseUnit unit = source as BaseUnit;
            FInt lvl = new FInt((int)(unit.GetLevel() * 1.5f));

            retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK, lvl);

            return null;
        }
//         static public object ACTPass_Praymaster(ISkillable source, Skill skill, SkillScript skillScript, NetDictionary<DBReference<Tag>, FInt> retAttribute)
//         {
//             BaseUnit unit = source as BaseUnit;
// 
//             //do not add bonus if unit have better one
//             var unitEnch = unit.GetEnchantments().Find(o => o.source == (Enchantment)ENCH.PRAYMASTER_UNIT);
//             if (unitEnch != null)
//                 return null;
// 
//             FInt lvl = new FInt(unit.GetLevel());
//             retAttribute.AddFinal((Tag)TAG.RESIST, lvl);
// 
//             return null;
//         }
//         static public object ACTPass_PraymasterSuper(ISkillable source, Skill skill, SkillScript skillScript, NetDictionary<DBReference<Tag>, FInt> retAttribute)
//         {
//             BaseUnit unit = source as BaseUnit;
// 
//             //do not add bonus if unit have better one
//             var unitEnch = unit.GetEnchantments().Find(o => o.source == (Enchantment)ENCH.PRAYMASTER_UNIT);
//             if (unitEnch != null)
//                 return null;
// 
//             FInt lvl = new FInt(unit.GetLevel() * 1.5f);
//             retAttribute.AddFinal((Tag)TAG.RESIST, lvl);
// 
//             return null;
//         }
//         static public object ACTPass_Leadership(ISkillable source, Skill skill, SkillScript skillScript, NetDictionary<DBReference<Tag>, FInt> retAttribute)
//         {
//             BaseUnit unit = source as BaseUnit;            
//             
//             //do not add bonus if unit have better one
//             var unitEnch = unit.GetEnchantments().Find(o => o.source == (Enchantment)ENCH.LEADERSHIP_UNIT);
//             if (unitEnch != null)
//                 return null;
// 
//             FInt lvl = new FInt((int)(unit.GetLevel() / 3));
//             retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK, lvl);
//             retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK, lvl);
//             retAttribute.AddFinal((Tag)TAG.THROW_BONUS, lvl);
//             retAttribute.AddFinal((Tag)TAG.FIRE_BREATH_BONUS, lvl);
// 
//             return null;
//         }
//         static public object ACTPass_LeadershipSuper(ISkillable source, Skill skill, SkillScript skillScript, NetDictionary<DBReference<Tag>, FInt> retAttribute)
//         {
//             BaseUnit unit = source as BaseUnit;
// 
//             //do not add bonus if unit have better one
//             var unitEnch = unit.GetEnchantments().Find(o => o.source == (Enchantment)ENCH.LEADERSHIP_UNIT);
//             if (unitEnch != null)
//                 return null;
// 
//             FInt lvl = new FInt((int)(unit.GetLevel() / 2));
//             retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK, lvl);
//             retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK, lvl);
//             retAttribute.AddFinal((Tag)TAG.THROW_BONUS, lvl);
//             retAttribute.AddFinal((Tag)TAG.FIRE_BREATH_BONUS, lvl);
// 
//             return null;
//         }

        static public object ACTPass_Scouting(ISkillable source, Skill skill, SkillScript skillScript, NetDictionary<DBReference<Tag>, FInt> retAttribute)
        {
            retAttribute.AddFinal((Tag)TAG.SIGHT_RANGE_BONUS, skillScript.fIntParam);
            
            return null;
        }
        static public object ACTPass_Sage(ISkillable source, Skill skill, SkillScript skillScript, NetDictionary<DBReference<Tag>, FInt> retAttribute)
        {
            BaseUnit unit = source as BaseUnit;
            FInt lvl = new FInt((int)(unit.GetLevel() * 3f));
            retAttribute.AddFinal((Tag)TAG.RESEARCH_PRODUCTION, lvl);

            return null;
        }
        static public object ACTPass_SageSuper(ISkillable source, Skill skill, SkillScript skillScript, NetDictionary<DBReference<Tag>, FInt> retAttribute)
        {
            BaseUnit unit = source as BaseUnit;
            FInt lvl = new FInt((int)(unit.GetLevel() * 4.5f));
            retAttribute.AddFinal((Tag)TAG.RESEARCH_PRODUCTION, lvl);

            return null;
        }
        static public object ACTPass_Foodie(ISkillable source, Skill skill, SkillScript skillScript, NetDictionary<DBReference<Tag>, FInt> retAttribute)
        {
            retAttribute.AddFinal((Tag)TAG.UPKEEP_FOOD, (FInt)3.0f);
            retAttribute.SetFinal((Tag)TAG.UPKEEP_GOLD, (FInt)0.0f);

            BaseUnit unit = source as BaseUnit;
            int lvl = unit.GetLevel();

            switch (lvl)
            {
                case 1:
                case 2:
                case 3:
                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK, FInt.ONE);
                    retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK, FInt.ONE);
                    retAttribute.AddFinal((Tag)TAG.DEFENCE, FInt.ONE);
                    retAttribute.AddFinal((Tag)TAG.RESIST, FInt.ONE);
                    retAttribute.AddFinal((Tag)TAG.HIT_POINTS, FInt.ONE);
                    break;
                case 4:
                case 5:
                case 6:
                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK, (FInt)2.0f);
                    retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK, (FInt)2.0f);
                    retAttribute.AddFinal((Tag)TAG.DEFENCE, (FInt)2.0f);
                    retAttribute.AddFinal((Tag)TAG.RESIST, (FInt)2.0f);
                    retAttribute.AddFinal((Tag)TAG.HIT_POINTS, (FInt)2.0f);
                    break;
                case 7:
                case 8:
                case 9:
                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK, (FInt)3.0f);
                    retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK, (FInt)3.0f);
                    retAttribute.AddFinal((Tag)TAG.DEFENCE, (FInt)3.0f);
                    retAttribute.AddFinal((Tag)TAG.RESIST, (FInt)3.0f);
                    retAttribute.AddFinal((Tag)TAG.HIT_POINTS, (FInt)3.0f);
                    break;


                default:
                    break;
            }


            return null;
        }
        static public object ACTPass_Peasant(ISkillable source, Skill skill, SkillScript skillScript, NetDictionary<DBReference<Tag>, FInt> retAttribute)
        {
            retAttribute.SetFinal((Tag)TAG.UPKEEP_GOLD, FInt.ZERO);

            BaseUnit unit = source as BaseUnit;
            int lvl = unit.GetLevel();
            switch (lvl)
            {
                case 1:
                case 2:
                case 3:
                    retAttribute.AddFinal((Tag)TAG.HIT_POINTS, FInt.ONE);
                    break;
                case 4:
                case 5:
                case 6:
                    retAttribute.AddFinal((Tag)TAG.HIT_POINTS, (FInt)2.0f);
                    break;
                case 7:
                case 8:
                case 9:
                    retAttribute.AddFinal((Tag)TAG.HIT_POINTS, (FInt)3.0f);
                    break;


                default:
                    break;
            }

            return null;
        }
        static public object ACTPass_AddManaIfNoCaster(ISkillable source, Skill skill, SkillScript skillScript, NetDictionary<DBReference<Tag>, FInt> retAttribute)
        {
            var skillCaster = source.GetSkills().Find(o =>
                           o.Get()?.applicationScript?.triggerType == ESkillType.Caster);
            if (skillCaster == null)
            {
                retAttribute.AddFinal((Tag)TAG.MANA_POINTS, (FInt)10);
            }

            return null;
        }
        static public object ACTPass_BlackChannels(ISkillable source, Skill skill, SkillScript skillScript, NetDictionary<DBReference<Tag>, FInt> retAttribute)
        {
            /*BaseUnit unit = source as BaseUnit;
            unit.race = (Race)RACE.REALM_DEATH;
            FInt addedHP = FInt.ONE;

            retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK, 2);
            retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK, FInt.ONE);
            retAttribute.AddFinal((Tag)TAG.THROW_BONUS, FInt.ONE);
            retAttribute.AddFinal((Tag)TAG.DOOM_GAZE_BONUS, FInt.ONE);
            retAttribute.AddFinal((Tag)TAG.FIRE_BREATH_BONUS, FInt.ONE);
            retAttribute.AddFinal((Tag)TAG.DEFENCE, FInt.ONE);
            retAttribute.AddFinal((Tag)TAG.RESIST, FInt.ONE);
            retAttribute.AddFinal((Tag)TAG.HIT_POINTS, addedHP);
            retAttribute.AddFinal((Tag)TAG.REANIMATED, FInt.ONE);

            if (retAttribute.ContainsKey((Tag)TAG.NORMAL_CLASS))
            {
                retAttribute.SetFinal((Tag)TAG.NORMAL_CLASS, FInt.ZERO);
                retAttribute.SetFinal((Tag)TAG.FANTASTIC_CLASS, FInt.ONE);
            }

            if (retAttribute.ContainsKey((Tag)TAG.HERO_CLASS))
            {
                retAttribute.SetFinal((Tag)TAG.HERO_CLASS, FInt.ZERO);
                retAttribute.SetFinal((Tag)TAG.FANTASTIC_CLASS, FInt.ONE);
            }

//             var curentHp = unit.currentFigureHP;
//             curentHp = curentHp + addedHP.ToInt();
//             //updating marker at this point would attempt to take final attributes while they are recalculated
//             unit.UpdateCurentFigureHpWithoutMarkers(curentHp);

            var upkeep = retAttribute.GetFinal((Tag)TAG.UPKEEP_FOOD);
            retAttribute.AddFinal((Tag)TAG.UPKEEP_FOOD, -1 * upkeep);
            upkeep = retAttribute.GetFinal((Tag)TAG.UPKEEP_GOLD);
            retAttribute.AddFinal((Tag)TAG.UPKEEP_GOLD, -1 * upkeep);

            //unit.EnsureEnchantments();*/

            return null;
        }
        static public object ACTPass_ChaosChannels1(ISkillable source, Skill skill, SkillScript skillScript, NetDictionary<DBReference<Tag>, FInt> retAttribute)
        {
            BaseUnit unit = source as BaseUnit;
            unit.race = (Race)RACE.REALM_CHAOS;

            retAttribute.AddFinal((Tag)TAG.DEFENCE, 3);

            return null;

        }
        static public object ACTPass_ChaosChannels2(ISkillable source, Skill skill, SkillScript skillScript, NetDictionary<DBReference<Tag>, FInt> retAttribute)
        {
            BaseUnit unit = source as BaseUnit;
            unit.race = (Race)RACE.REALM_CHAOS;

            retAttribute.AddFinal((Tag)TAG.CAN_FLY, FInt.ONE);
            retAttribute.AddFinal((Tag)TAG.SIGHT_RANGE_BONUS, FInt.ONE);

            return null;

        }
        static public object ACTPass_ChaosChannels3(ISkillable source, Skill skill, SkillScript skillScript, NetDictionary<DBReference<Tag>, FInt> retAttribute)
        {
            BaseUnit unit = source as BaseUnit;
            unit.race = (Race)RACE.REALM_CHAOS;
            var fireBreath = retAttribute.GetFinal((Tag)TAG.FIRE_BREATH_BONUS).ToInt();

            switch (fireBreath)
            {
                case 0 :
                    retAttribute.AddFinal((Tag)TAG.FIRE_BREATH_BONUS, 2);
                    break;
                case 1:
                    retAttribute.AddFinal((Tag)TAG.FIRE_BREATH_BONUS, 1);
                    break;

                default:
                    break;
            }

            return null;
        }
        static public object ACTPass_EnchantedWeapon(ISkillable source, Skill skill, SkillScript skillScript, NetDictionary<DBReference<Tag>, FInt> retAttribute)
        {
            var t = (Tag)TAG.ENCHANTED_WEAPON;
            retAttribute.AddFinal(t, FInt.ONE);

            return null;
        }
        static public object ACTPass_EnchantedWeapon2(ISkillable source, Skill skill, SkillScript skillScript, NetDictionary<DBReference<Tag>, FInt> retAttribute)
        {
            var t = (Tag)TAG.ENCHANTED_WEAPON;
            retAttribute.AddFinal(t, FInt.ONE);
            retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK_CHANCE, (FInt)0.1);
            if (!retAttribute.ContainsKey((Tag)TAG.MAGIC_RANGE))
            {
                retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK_CHANCE, (FInt)0.1);
            }

            return null;
        }
        static public object ACTPass_MithrilWeapon(ISkillable source, Skill skill, SkillScript skillScript, NetDictionary<DBReference<Tag>, FInt> retAttribute)
        {
            retAttribute.AddFinal((Tag)TAG.ENCHANTED_WEAPON, FInt.ONE);
            retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK, FInt.ONE);
            retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK_CHANCE, (FInt)0.1);
            retAttribute.AddFinal((Tag)TAG.DEFENCE, FInt.ONE);
            retAttribute.AddFinal((Tag)TAG.THROW_BONUS, FInt.ONE);
            if (!retAttribute.ContainsKey((Tag)TAG.MAGIC_RANGE))
            {
                retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK_CHANCE, (FInt)0.1);
                retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK, FInt.ONE);
            }

            return null;
        }
        static public object ACTPass_AdamantineWeapon(ISkillable source, Skill skill, SkillScript skillScript, NetDictionary<DBReference<Tag>, FInt> retAttribute)
        {
            retAttribute.AddFinal((Tag)TAG.ENCHANTED_WEAPON, FInt.ONE);
            retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK, (FInt)2.0);
            retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK_CHANCE, (FInt)0.1);
            retAttribute.AddFinal((Tag)TAG.DEFENCE, (FInt)2.0);
            retAttribute.AddFinal((Tag)TAG.THROW_BONUS, (FInt)2.0);
            if (!retAttribute.ContainsKey((Tag)TAG.MAGIC_RANGE))
            {
                retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK_CHANCE, (FInt)0.1);
                retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK, (FInt)2.0);
            }

            return null;
        }
        static public object ACTPass_AddTag(ISkillable source, Skill skill, SkillScript skillScript, NetDictionary<DBReference<Tag>, FInt> retAttribute)
        {
            Tag tag = (Tag)DataBase.Get(skillScript.stringData, false);
            if (tag != null)
            {
                int tagMod = skillScript.fIntParam.ToInt();
                retAttribute.AddFinal(tag, tagMod);
            }
            else Debug.LogWarning(skillScript.stringData + " is not a Tag. You have a typo?");

            return null;
        }
        static public object ACTPass_Caster(ISkillable source, Skill skill, SkillScript skillScript, NetDictionary<DBReference<Tag>, FInt> retAttribute)
        {
            if (!(source is MOM.BaseUnit))
                return null;

            var unit = source as MOM.BaseUnit;
            var level = unit.GetLevel();

            var tagManaValue = retAttribute.GetFinal((Tag)TAG.MANA_POINTS);

            tagManaValue *= Mathf.Max(1, level);

            retAttribute.SetFinal((Tag)TAG.CASTER, FInt.ONE);

            //If unit have magic range attack it will have less mana but it will have more ammo with each 
            var unitSkills = unit.GetSkills();
            foreach (var s in unitSkills)
            {
                retAttribute.SetFinal((Tag)TAG.MANA_POINTS, (tagManaValue).ReturnRoundedFloor());

                DBDef.SkillScript magicRangeAttackScript = null;
                if (s.Get() != null && s.Get().script != null)
                {
                    magicRangeAttackScript = Array.Find(s.Get().script, o => o.activatorSecondary == "ACT_ApplyMagicRangeAttack");
                }

                if (magicRangeAttackScript != null)
                {
                    switch (level)
                    {
                        case 4:
                            tagManaValue *= 0.94f;
                            break;
                        case 5:
                            tagManaValue *= 0.88f;
                            break;
                        case 6:
                            tagManaValue *= 0.82f;
                            break;
                        case 7:
                            tagManaValue *= 0.76f;
                            break;
                        case 8:
                            tagManaValue *= 0.70f;
                            break;
                        case 9:
                            tagManaValue *= 0.64f;
                            break;

                        default:
                            break;
                    }
                    retAttribute.SetFinal((Tag)TAG.AMMUNITION, (tagManaValue/3).ReturnRoundedFloor());
                }
            }

            return null;
        }

        static public object ACTPass_UnitLevelUp(ISkillable source, Skill skill, SkillScript skillScript, NetDictionary<DBReference<Tag>, FInt> retAttribute)
        {
            if (!(source is MOM.BaseUnit)) return null;
            var unit = source as MOM.BaseUnit;

            switch (unit.GetLevel())
            {
                case 2:
                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK, FInt.ONE);
                    retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK, FInt.ONE);
                    retAttribute.AddFinal((Tag)TAG.FIRE_BREATH_BONUS, FInt.ONE);
                    retAttribute.AddFinal((Tag)TAG.THROW_BONUS, FInt.ONE);
                    retAttribute.AddFinal((Tag)TAG.RESIST, FInt.ONE);
                    return null;
                case 3:
                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK, FInt.ONE);
                    retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK, FInt.ONE);
                    retAttribute.AddFinal((Tag)TAG.FIRE_BREATH_BONUS, FInt.ONE);
                    retAttribute.AddFinal((Tag)TAG.THROW_BONUS, FInt.ONE);
                    retAttribute.AddFinal((Tag)TAG.DEFENCE, FInt.ONE);
                    retAttribute.AddFinal((Tag)TAG.RESIST, (FInt)2.0);
                    return null;
                case 4:
                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK, (FInt)2.0);
                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK_CHANCE, (FInt)0.1);
                    retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK, (FInt)2.0);
                    retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK_CHANCE, (FInt)0.1);
                    retAttribute.AddFinal((Tag)TAG.FIRE_BREATH_BONUS, (FInt)2.0);
                    retAttribute.AddFinal((Tag)TAG.THROW_BONUS, (FInt)2.0);
                    retAttribute.AddFinal((Tag)TAG.DEFENCE, FInt.ONE);
                    retAttribute.AddFinal((Tag)TAG.RESIST, (FInt)3.0);
                    retAttribute.AddFinal((Tag)TAG.HIT_POINTS, FInt.ONE);
                    return null;
                case 5:
                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK, (FInt)2.0);
                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK_CHANCE, (FInt)0.2);
                    retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK, (FInt)2.0);
                    retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK_CHANCE, (FInt)0.2);
                    retAttribute.AddFinal((Tag)TAG.FIRE_BREATH_BONUS, (FInt)2.0);
                    retAttribute.AddFinal((Tag)TAG.THROW_BONUS, (FInt)2.0);
                    retAttribute.AddFinal((Tag)TAG.DEFENCE, (FInt)2.0);
                    retAttribute.AddFinal((Tag)TAG.RESIST, (FInt)4.0);
                    retAttribute.AddFinal((Tag)TAG.HIT_POINTS, FInt.ONE);
                    return null;
                case 6:
                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK, (FInt)3.0);
                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK_CHANCE, (FInt)0.3);
                    retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK, (FInt)3.0);
                    retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK_CHANCE, (FInt)0.3);
                    retAttribute.AddFinal((Tag)TAG.FIRE_BREATH_BONUS, (FInt)3.0);
                    retAttribute.AddFinal((Tag)TAG.THROW_BONUS, (FInt)3.0);
                    retAttribute.AddFinal((Tag)TAG.DEFENCE, (FInt)2.0);
                    retAttribute.AddFinal((Tag)TAG.RESIST, (FInt)5.0);
                    retAttribute.AddFinal((Tag)TAG.HIT_POINTS, (FInt)2.0);
                    return null;

                default:
                    return null;
            }

        }

        static public object ACTPass_HeroLevelUp(ISkillable source, Skill skill, SkillScript skillScript, NetDictionary<DBReference<Tag>, FInt> retAttribute)
        {
            if (!(source is MOM.BaseUnit)) return null;
            var unit = source as MOM.BaseUnit;

            switch (unit.GetLevel())
            {
                case 2:
                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK, FInt.ONE);
                    retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK, FInt.ONE);
                    retAttribute.AddFinal((Tag)TAG.FIRE_BREATH_BONUS, FInt.ONE);
                    retAttribute.AddFinal((Tag)TAG.THROW_BONUS, FInt.ONE);
                    retAttribute.AddFinal((Tag)TAG.DEFENCE, FInt.ONE);
                    retAttribute.AddFinal((Tag)TAG.RESIST, FInt.ONE);
                    retAttribute.AddFinal((Tag)TAG.HIT_POINTS, FInt.ONE);
                    return null;
                case 3:
                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK, (FInt)2.0);
                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK_CHANCE, (FInt)0.1);
                    retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK, (FInt)2.0);
                    retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK_CHANCE, (FInt)0.1);
                    retAttribute.AddFinal((Tag)TAG.FIRE_BREATH_BONUS, (FInt)2.0);
                    retAttribute.AddFinal((Tag)TAG.THROW_BONUS, (FInt)2.0);
                    retAttribute.AddFinal((Tag)TAG.DEFENCE, FInt.ONE);
                    retAttribute.AddFinal((Tag)TAG.RESIST, (FInt)2.0);
                    retAttribute.AddFinal((Tag)TAG.HIT_POINTS, (FInt)2.0);
                    return null;
                case 4:
                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK, (FInt)3.0);
                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK_CHANCE, (FInt)0.1);
                    retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK, (FInt)3.0);
                    retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK_CHANCE, (FInt)0.1);
                    retAttribute.AddFinal((Tag)TAG.FIRE_BREATH_BONUS, (FInt)3.0);
                    retAttribute.AddFinal((Tag)TAG.THROW_BONUS, (FInt)3.0);
                    retAttribute.AddFinal((Tag)TAG.DEFENCE, (FInt)2.0);
                    retAttribute.AddFinal((Tag)TAG.RESIST, (FInt)3.0);
                    retAttribute.AddFinal((Tag)TAG.HIT_POINTS, (FInt)3.0);
                    return null;
                case 5:
                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK, (FInt)4.0);
                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK_CHANCE, (FInt)0.1);
                    retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK, (FInt)4.0);
                    retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK_CHANCE, (FInt)0.1);
                    retAttribute.AddFinal((Tag)TAG.FIRE_BREATH_BONUS, (FInt)4.0);
                    retAttribute.AddFinal((Tag)TAG.THROW_BONUS, (FInt)4.0);
                    retAttribute.AddFinal((Tag)TAG.DEFENCE, (FInt)2.0);
                    retAttribute.AddFinal((Tag)TAG.RESIST, (FInt)4.0);
                    retAttribute.AddFinal((Tag)TAG.HIT_POINTS, (FInt)4.0);
                    return null;
                case 6:
                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK, (FInt)5.0);
                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK_CHANCE, (FInt)0.2);
                    retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK, (FInt)5.0);
                    retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK_CHANCE, (FInt)0.2);
                    retAttribute.AddFinal((Tag)TAG.FIRE_BREATH_BONUS, (FInt)5.0);
                    retAttribute.AddFinal((Tag)TAG.THROW_BONUS, (FInt)5.0);
                    retAttribute.AddFinal((Tag)TAG.DEFENCE, (FInt)3.0);
                    retAttribute.AddFinal((Tag)TAG.RESIST, (FInt)5.0);
                    retAttribute.AddFinal((Tag)TAG.HIT_POINTS, (FInt)5.0);
                    return null;
                case 7:
                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK, (FInt)6.0);
                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK_CHANCE, (FInt)0.2);
                    retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK, (FInt)6.0);
                    retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK_CHANCE, (FInt)0.2);
                    retAttribute.AddFinal((Tag)TAG.FIRE_BREATH_BONUS, (FInt)6.0);
                    retAttribute.AddFinal((Tag)TAG.THROW_BONUS, (FInt)6.0);
                    retAttribute.AddFinal((Tag)TAG.DEFENCE, (FInt)3.0);
                    retAttribute.AddFinal((Tag)TAG.RESIST, (FInt)6.0);
                    retAttribute.AddFinal((Tag)TAG.HIT_POINTS, (FInt)6.0);
                    return null;
                case 8:
                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK, (FInt)7.0);
                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK_CHANCE, (FInt)0.2);
                    retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK, (FInt)7.0);
                    retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK_CHANCE, (FInt)0.2);
                    retAttribute.AddFinal((Tag)TAG.FIRE_BREATH_BONUS, (FInt)7.0);
                    retAttribute.AddFinal((Tag)TAG.THROW_BONUS, (FInt)7.0);
                    retAttribute.AddFinal((Tag)TAG.DEFENCE, (FInt)4.0);
                    retAttribute.AddFinal((Tag)TAG.RESIST, (FInt)7.0);
                    retAttribute.AddFinal((Tag)TAG.HIT_POINTS, (FInt)7.0);
                    return null;
                case 9:
                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK, (FInt)8.0);
                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK_CHANCE, (FInt)0.3);
                    retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK, (FInt)8.0);
                    retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK_CHANCE, (FInt)0.3);
                    retAttribute.AddFinal((Tag)TAG.FIRE_BREATH_BONUS, (FInt)8.0);
                    retAttribute.AddFinal((Tag)TAG.THROW_BONUS, (FInt)8.0);
                    retAttribute.AddFinal((Tag)TAG.DEFENCE, (FInt)4.0);
                    retAttribute.AddFinal((Tag)TAG.RESIST, (FInt)8.0);
                    retAttribute.AddFinal((Tag)TAG.HIT_POINTS, (FInt)8.0);
                    return null;

                default:
                    return null;
            }

        }
        static public object ACTPass_Invulnerability(ISkillable source, Skill skill, SkillScript skillScript, NetDictionary<DBReference<Tag>, FInt> retAttribute)
        {
            var bu = source as BaseUnit;
            if(bu != null) 
                bu.invulnerabilityProtection += skill.script[0].fIntParam.ToInt();

            return null;
        }
        static public object ACTPass_Haste(ISkillable source, Skill skill, SkillScript skillScript, NetDictionary<DBReference<Tag>, FInt> retAttribute)
        {
            if (source is BattleUnit)
            {
                var bu = source as BattleUnit;                
                bu.haste = true;
            }

            return null;
        }
        static public object ACTPass_Hivemind(ISkillable source, Skill skill, SkillScript skillScript, NetDictionary<DBReference<Tag>, FInt> retAttribute)
        {
            int bonus = 0;
            Race race = (Race)DataBase.Get(skillScript.stringData, false);
            FInt value = skillScript.fIntParam;
            var baseUnit = source as BaseUnit;
            if (source is MOM.Unit)
            {
                var u = source as MOM.Unit;
                if(u.group != null && race != null)
                {
                    foreach (var item in u.group.Get().GetUnits())
                    {
                        if (item.Get() == u) continue;
                        if (item.Get().race == race) bonus++;
                    }
                    if(bonus > 0)
                    {
                        bonus = bonus / 2;
                    }
                }
            }
            else if(source is BattleUnit)
            {
                var battle = Battle.Get();
                if(battle == null)
                {
                    //Debug.LogWarning("ACTPass_Hivemind: battle instance doesn't exist");
                    return null;
                }
                var bu = source as BattleUnit;
                var units = bu.attackingSide ? battle.attackerUnits : battle.defenderUnits;
                
                foreach (var item in units)
                {
                    if (item == bu || !item.IsAlive()) continue;
                    if (item.race == race) bonus++;
                }
                if (bonus > 0)
                {
                    bonus = bonus / 2;
                }
            }
            else
            {
                Debug.LogWarning("ACTPass_Hivemind: skill source invalid" + source.ToString());
                return null;
            }

            switch (bonus)
            {
                case 1:
                    retAttribute.AddFinal((Tag)TAG.RESIST, value);
                    return null;
                case 2:
                    retAttribute.AddFinal((Tag)TAG.RESIST, value);
                    retAttribute.AddFinal((Tag)TAG.DEFENCE, value);
                    return null;
                case 3:
                    retAttribute.AddFinal((Tag)TAG.RESIST, value);
                    retAttribute.AddFinal((Tag)TAG.DEFENCE, value);
                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK, value);
                    if (baseUnit.rangeAttack)
                        retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK, value);
                    return null;
                case 4:
                    retAttribute.AddFinal((Tag)TAG.RESIST, value);
                    retAttribute.AddFinal((Tag)TAG.DEFENCE, value);
                    retAttribute.AddFinal((Tag)TAG.HIT_POINTS, value);
                    retAttribute.AddFinal((Tag)TAG.MELEE_ATTACK, value);
                    if (baseUnit.rangeAttack)
                        retAttribute.AddFinal((Tag)TAG.RANGE_ATTACK, value);
                        
                    return null;
            }

            return null;
        }
        static public object ACTPass_GnollGeneral(ISkillable source, Skill skill, SkillScript skillScript, NetDictionary<DBReference<Tag>, FInt> retAttribute)
        {
            int bonus = 0;
            DBDef.Unit subrace = (DBDef.Unit)DataBase.Get(skillScript.stringData, false);
            FInt value = skillScript.fIntParam;
            if (source is MOM.Unit)
            {
                var u = source as MOM.Unit;
                if(u.group != null && subrace != null)
                {
                    foreach (var item in u.group.Get().GetUnits())
                    {
                        if (item.Get() == u) continue;
                        if (item.Get().dbSource == subrace) bonus++;
                    }
                    if(bonus > 0)
                    {
                        bonus = bonus / 2;
                        retAttribute.AddFinal((Tag)TAG.DEFENCE, bonus * value);
                    }
                }
            }
            else if(source is BattleUnit)
            {
                var battle = Battle.Get();
                if(battle == null)
                {
                    //Debug.LogWarning("ACTPass_GnollGeneral: instance of battle doesn't exist");
                    return null;
                }
                var bu = source as BattleUnit;
                var units = bu.attackingSide ? battle.attackerUnits : battle.defenderUnits;
                
                foreach (var item in units)
                {
                    if (item == bu || !item.IsAlive()) continue;
                    if (item.dbSource == subrace) bonus++;
                }
                if (bonus > 0)
                {
                    bonus = bonus / 2;
                    retAttribute.AddFinal((Tag)TAG.DEFENCE, bonus * value);
                }
            }
            else
            {
                Debug.LogWarning("ACTPass_GnollGeneral: skill source invalid" + source.ToString());
                return null;
            }

            return null;
        }

        #endregion
        #region Skill Application & Removal Scripts
        static public void SAPP_Empty(ISkillable source, Skill skill, SkillScript skillScript)
        {

        }
        static public void SAPP_AddSpell(ISkillable source, Skill skill, SkillScript skillScript)
        {
            if(source is ISpellCaster)
            {
                var isc = source as ISpellCaster;
                var spell = (Spell)DataBase.Get(skillScript.stringData, false);
                if (spell != null)
                {
                    isc.AddSpell(spell);
                }
                else Debug.LogWarning(skillScript.stringData + " is not a Tag. You have a typo?");
            }
            else
            {
                Debug.LogError("Source should by ISpellCaster type");
            }
        }
        static public void SAPP_AddEnchantment(ISkillable source, Skill skill, SkillScript skillScript)
        {
            if (source is MOM.Unit)
            {
                var unit = source as MOM.Unit;
                var enchName = skill.applicationScript.stringData;

                if (String.IsNullOrEmpty(enchName))
                {
                    Debug.LogError(skill.dbName + " stringData in SkillApplicationScript is empty.");
                }
                var ench = (Enchantment)DataBase.Get(enchName, false);

                unit.AddEnchantment(ench, unit as IEnchantable, ench.lifeTime, null, 0);
            }
        }
        static public void SAPP_Invisibility(ISkillable source, Skill skill, SkillScript skillScript)
        {
            if (source is MOM.Unit)
            {
                var unit = source as MOM.Unit;
                var enchName = skill.applicationScript.stringData;

                if (String.IsNullOrEmpty(enchName))
                {
                    Debug.LogError(skill.dbName + " stringData in SkillApplicationScript is empty.");
                }
                var ench = (Enchantment)DataBase.Get(enchName, false);

                unit.AddEnchantment(ench, unit as IEnchantable, ench.lifeTime, null, 0);
                if(unit.group != null)
                {
                    unit.group.Get().UpdateMarkers();
                    unit.group.Get().UpdateMapFormation(false);
                }
            }
        }
        static public void SAPP_UnitUpdateMove(ISkillable source, Skill skill, SkillScript skillScript)
        {
            if (source is MOM.Unit)
            {
                (source as MOM.Unit).GetAttributes().SetDirty();
                (source as MOM.Unit).group?.Get().UpdateMovementFlags();
            }
        }
        static public void SAPP_BlackChannels(ISkillable source, Skill skill, SkillScript skillScript)
        {
            BaseUnit unit = source as BaseUnit;
            unit.race = (Race)RACE.REALM_DEATH;
            var att = unit.GetAttributes();
            att.AddToBase((Tag)TAG.MELEE_ATTACK, 2);
            att.AddToBase((Tag)TAG.RANGE_ATTACK, FInt.ONE);
            att.AddToBase((Tag)TAG.THROW_BONUS, FInt.ONE);
            att.AddToBase((Tag)TAG.DOOM_GAZE_BONUS, FInt.ONE);
            att.AddToBase((Tag)TAG.FIRE_BREATH_BONUS, FInt.ONE);
            att.AddToBase((Tag)TAG.DEFENCE, FInt.ONE);
            att.AddToBase((Tag)TAG.RESIST, FInt.ONE);
            att.AddToBase((Tag)TAG.HIT_POINTS, FInt.ONE);
            att.AddToBase((Tag)TAG.REANIMATED, FInt.ONE);

            if(att.Contains(TAG.NORMAL_CLASS))
            {
                att.SetBaseTo((Tag)TAG.NORMAL_CLASS, FInt.ZERO);
                att.SetBaseTo((Tag)TAG.FANTASTIC_CLASS, FInt.ONE);
            }
            if (att.Contains(TAG.HERO_CLASS))
            {
                att.SetBaseTo((Tag)TAG.HERO_CLASS, FInt.ZERO);
                att.SetBaseTo((Tag)TAG.FANTASTIC_CLASS, FInt.ONE);
            }

            att.SetBaseTo((Tag)TAG.UPKEEP_FOOD, FInt.ZERO);
            att.SetBaseTo((Tag)TAG.UPKEEP_GOLD, FInt.ZERO);

            if(unit.GetWizardOwner() != null)
            {
                MOM.Unit u = source as MOM.Unit;
                unit.GetWizardOwner().ModifyUnitSkillsByTraits(u);
            }

            unit.EnsureEnchantments();
        }
        static public void SAPP_AddDoubleShot(ISkillable source, Skill skill, SkillScript skillScript)
        {
            if (source is BaseUnit)
            {
                var bu = source as BaseUnit;
                bu.doubleShot = true;
            }
            else Debug.LogWarning(skill.dbName + " skill owner is not base unit.");
        }
        static public void SAPP_AddTag(ISkillable source, Skill skill, SkillScript skillScript)
        {
            if (source as BaseUnit != null &&
                skill.applicationScript != null &&
                skill.applicationScript.fIntParam != null &&
                skill.applicationScript.stringData != "")
            {

                Tag tag = (Tag)DataBase.Get(skill.applicationScript.stringData, false);
                if (tag != null)
                {
                    int tagMod = skill.applicationScript.fIntParam.ToInt();
                    //(source as MOM.Unit).GetAttributes().AddToBase(tag, tagMod);
                    (source as BaseUnit).GetAttributes().AddToBase(tag, tagMod);
                }
                else Debug.LogWarning(skill.applicationScript.stringData + " is not a Tag. You have a typo?");
            }
            else Debug.LogWarning(skill.dbName + "there is problem with enchantment unit as a target, script, fintdata or stringdata");
        }
        static public object SAPP_NonCorporeal(ISkillable source, Skill skill, SkillScript skillScript)
        {
            if (source as BaseUnit != null &&
                skill.applicationScript != null &&
                skill.applicationScript.fIntParam != null &&
                skill.applicationScript.stringData != "")
            {
                int tagMod = skill.applicationScript.fIntParam.ToInt();
                var t = (Tag)TAG.NON_CORPOREAL;
                var s = (Tag)TAG.CAN_SWIM;

                (source as MOM.Unit).GetAttributes().AddToBase(t, tagMod);
                (source as MOM.Unit).GetAttributes().AddToBase(s, tagMod);
            }
            else Debug.LogWarning(skill.dbName + "there is problem with enchantment unit as a target, script, fintdata or stringdata");


            return null;
        }
        static public void SAPP_EnchantedWeapon(ISkillable source, Skill skill, SkillScript skillScript)
        {
            if (source is BaseUnit)
            {
                var unit = source as MOM.Unit;
                if (skill == (Skill)SKILL.ENCHANTED_WEAPON3)
                {
                    if (unit.GetSkills().Find(o => o.Get().dbName == "SKILL-ENCHANTED_WEAPON4" ) != null)
                    {
                        unit.RemoveSkill((Skill)SKILL.ENCHANTED_WEAPON3);
                        unit.RemoveSkill((Skill)SKILL.ENCHANTED_WEAPON2);
                        unit.RemoveSkill((Skill)SKILL.ENCHANTED_WEAPON1);
                    }
                }
                if (skill == (Skill)SKILL.ENCHANTED_WEAPON2)
                {
                    if (unit.GetSkills().Find(o => o.Get().dbName == "SKILL-ENCHANTED_WEAPON4" ||
                    o.Get().dbName == "SKILL-ENCHANTED_WEAPON3") != null)
                    {
                        unit.RemoveSkill((Skill)SKILL.ENCHANTED_WEAPON2);
                        unit.RemoveSkill((Skill)SKILL.ENCHANTED_WEAPON1);
                    }
                }
                if (skill == (Skill)SKILL.ENCHANTED_WEAPON1)
                {
                    if (unit.GetSkills().Find(o => o.Get().dbName == "SKILL-ENCHANTED_WEAPON4"||
                    o.Get().dbName == "SKILL-ENCHANTED_WEAPON3" ||
                    o.Get().dbName == "SKILL-ENCHANTED_WEAPON2") != null)
                    {
                        unit.RemoveSkill((Skill)SKILL.ENCHANTED_WEAPON1);
                    }
                }

                return;
            }
            else Debug.LogWarning(skill.dbName + " skill owner is not base unit.");
        }
        static public void SREM_RemoveDoubleShot(ISkillable source, Skill skill, SkillScript skillScript)
        {
            if (source is BaseUnit)
            {
                var bu = source as BaseUnit;
                bu.doubleShot = false;
            }
            else Debug.LogWarning(skill.dbName + " skill owner is not base unit.");
        }
        static public void SREM_RemoveTag(ISkillable source, Skill skill, SkillScript skillScript)
        {
            if (source as BaseUnit != null &&
                skill.applicationScript != null &&
                skill.applicationScript.fIntParam != null &&
                skill.applicationScript.stringData != "")
            {

                Tag tag = (Tag)DataBase.Get(skill.applicationScript.stringData, false);
                if (tag != null)
                {
                    int tagMod = -1 * skill.applicationScript.fIntParam.ToInt();
                    //(source as MOM.Unit).GetAttributes().AddToBase(tag, tagMod);
                    (source as BaseUnit).GetAttributes().AddToBase(tag, tagMod);
                }
                else Debug.LogWarning(skill.applicationScript.stringData + " is not a Tag. You have a typo?");
            }
            else Debug.LogWarning(skill.dbName + "there is problem with enchantment unit as a target, script, fintdata or stringdata");
        }
        static public object SREM_NonCorporeal(ISkillable source, Skill skill, SkillScript skillScript)
        {
            if (source as BaseUnit != null &&
                skill.applicationScript != null &&
                skill.applicationScript.fIntParam != null &&
                skill.applicationScript.stringData != "")
            {
                int tagMod = -1 * skill.applicationScript.fIntParam.ToInt();
                var t = (Tag)TAG.NON_CORPOREAL;
                var s = (Tag)TAG.CAN_SWIM;

                (source as MOM.Unit).GetAttributes().AddToBase(t, tagMod);
                (source as MOM.Unit).GetAttributes().AddToBase(s, tagMod);
            }
            else Debug.LogWarning(skill.dbName + "there is problem with enchantment unit as a target, script, fintdata or stringdata");


            return null;
        }

        static public void SREM_RemoveSpell(ISkillable source, Skill skill, SkillScript skillScript)
        {
            if (source is ISpellCaster)
            {
                var isc = source as ISpellCaster;
                var spell = (Spell)DataBase.Get(skillScript.stringData, false);
                if (spell != null)
                {
                    isc.RemoveSpell(spell);
                }
                else Debug.LogWarning(skillScript.stringData + " is not a Tag. You have a typo?");
            }
            else
            {
                Debug.LogError("Source should by ISpellCaster type");
            }
        }
        static public void SREM_RemoveEnchantment(ISkillable source, Skill skill, SkillScript skillScript)
        {
            if (source is MOM.Unit)
            {
                var unit = source as MOM.Unit;
                var enchName = skill.applicationScript.stringData;

                if (String.IsNullOrEmpty(enchName))
                {
                    Debug.LogError(skill.dbName + " stringData in SkillApplicationScript is empty.");
                }
                var ench = (Enchantment)DataBase.Get(enchName, false);
                unit.RemoveEnchantment(ench);
            }
        }
        static public void SREM_RemoveInvisibility(ISkillable source, Skill skill, SkillScript skillScript)
        {
            if (source is MOM.Unit)
            {
                var unit = source as MOM.Unit;
                var enchName = skill.applicationScript.stringData;

                if (String.IsNullOrEmpty(enchName))
                {
                    Debug.LogError(skill.dbName + " stringData in SkillApplicationScript is empty.");
                }
                var ench = (Enchantment)DataBase.Get(enchName, false);
                unit.RemoveEnchantment(ench);
                if (unit.group != null)
                {
                    unit.group.Get().UpdateMarkers();
                    unit.group.Get().UpdateMapFormation(false);
                }
            }
        }
        static public void SREM_UnitUpdateMove(ISkillable source, Skill skill, SkillScript skillScript)
        {
            if (source is MOM.Unit)
            {
                (source as MOM.Unit).GetAttributes().SetDirty();
                (source as MOM.Unit).group?.Get().UpdateMovementFlags();
            }
        }

        #endregion
        #region Join / Disconnect Triggers
        static public void SJOIN_MakeDirty(ISkillable source, ISkillable otherUnit, Skill skill, object data)
        {
            if(source is BaseUnit)
            {
                var bu = source as BaseUnit;
                bu.GetAttributes().SetDirty();
            }
        }
        static public void SLEAVE_MakeDirty(ISkillable source, ISkillable otherUnit, Skill skill, object data)
        {
            if (source is BaseUnit)
            {
                var bu = source as BaseUnit;
                bu.GetAttributes().SetDirty();
            }
        }
        static public void SJOIN_GnollGeneral(ISkillable source, ISkillable otherUnit, Skill skill, object data)
        {
            BaseUnit owner = source as BaseUnit;
            BaseUnit target = otherUnit as BaseUnit;
            List<BaseUnit> unitsInGroup = data as List<BaseUnit>;

            if (owner == null || target == null || owner == target) return;
            if (target.dbSource != (DBDef.Unit)UNIT.GNO_WOLF_RIDERS) return;
            
            //set owner's attributes as dirty to trigger skill activator script in next attributes update
            owner.GetAttributes().SetDirty();

            foreach (var s in skill.script)
            {
                if (s.triggerType == ESkillType.GroupChange)
                {
                    var enchToAdd = (Enchantment)DataBase.Get(s.stringData, false);

                    if (enchToAdd == null)
                        Debug.LogError("Enchantment " + skill.dbName + " StringData is not a enchantment.");
                    else
                    {
                        var ei = target.GetEnchantments().Find(o => o.source == enchToAdd);
                        if (ei == null)
                        {
                            target.AddEnchantment(enchToAdd, owner as IEnchantable, enchToAdd.lifeTime, null, 0);
                            target.GetAttributes().SetDirty();
                        }
                    }
                }
            }
        }
        static public void SLEAVE_GnollGeneral(ISkillable source, ISkillable otherUnit, Skill skill, object data)
        {
            BaseUnit owner = source as BaseUnit;
            BaseUnit target = otherUnit as BaseUnit;
            List<BaseUnit> unitsInGroup = data as List<BaseUnit>;
            
            if (owner == null || target == null || owner == target) return;
            if (target.dbSource != (DBDef.Unit)UNIT.GNO_WOLF_RIDERS) return;


            //set owner's attributes as dirty to trigger skill activator script in next attributes update
            owner.GetAttributes().SetDirty();

            foreach (var s in skill.script)
            {
                if (s.triggerType == ESkillType.GroupChange)
                {
                    var enchToRemove = (Enchantment)DataBase.Get(s.stringData, false);

                    if (enchToRemove == null)
                        Debug.LogError("Enchantment " + skill.dbName + " StringData is not a enchantment.");
                    else
                    {
                        if (unitsInGroup != null && unitsInGroup.Count > 0)
                        {
                            foreach (var u in unitsInGroup)
                            {
                                if (u.GetSkills().Contains((Skill)SKILL.GNOLL_GENERAL) &&
                                    u != owner)
                                {
                                    return;
                                }
                                
                            }
                        }

                        var ei = target.GetEnchantments().Find(o => o.source == enchToRemove);
                        if (ei != null)
                        {
                            target.RemoveEnchantment(ei);
                        }
                    }
                }
            }
        }static public void SJOIN_Hivemind(ISkillable source, ISkillable otherUnit, Skill skill, object data)
        {
            BaseUnit owner = source as BaseUnit;
            BaseUnit target = otherUnit as BaseUnit;
            Race race = (Race)RACE.KLACKONS;

            if (owner == null) return;
            //set owner's attributes as dirty to trigger skill activator script in next attributes update
            owner.GetAttributes().SetDirty();

            if(target == null || owner == target) return;
            if (target.race != (Race)RACE.KLACKONS) return;

            foreach (var s in skill.script)
            {
                if (s.triggerType == ESkillType.GroupChange)
                {
                    var enchToAdd = (Enchantment)DataBase.Get(s.stringData, false);

                    if (enchToAdd == null)
                        Debug.LogError("Enchantment " + skill.dbName + " StringData is not a enchantment.");
                    else
                    {
                        var ei = target.GetEnchantments().Find(o => o.source == enchToAdd);
                        if (ei == null)
                        {
                            target.AddEnchantment(enchToAdd, owner as IEnchantable, enchToAdd.lifeTime, null, 0);
                            target.GetAttributes().SetDirty();
                        }
                    }
                }
            }
        }
        static public void SLEAVE_Hivemind(ISkillable source, ISkillable otherUnit, Skill skill, object data)
        {
            BaseUnit owner = source as BaseUnit;
            BaseUnit target = otherUnit as BaseUnit;
            List<BaseUnit> unitsInGroup = data as List<BaseUnit>;

            if (owner == null) return;
            //set owner's attributes as dirty to trigger skill activator script in next attributes update
            owner.GetAttributes().SetDirty();

            if (target == null || owner == target) return;
            if (target.race != (Race)RACE.KLACKONS) return;

            foreach (var s in skill.script)
            {
                if (s.triggerType == ESkillType.GroupChange)
                {
                    var enchToRemove = (Enchantment)DataBase.Get(s.stringData, false);

                    if (enchToRemove == null)
                        Debug.LogError("Enchantment " + skill.dbName + " StringData is not a enchantment.");
                    else
                    {
                        if (unitsInGroup != null && unitsInGroup.Count > 0)
                        {
                            foreach (var u in unitsInGroup)
                            {
                                if (u.GetSkills().Contains((Skill)SKILL.HIVEMIND) &&
                                    u != owner)
                                {
                                    return;
                                }
                                
                            }
                        }

                        var ei = target.GetEnchantments().Find(o => o.source == enchToRemove);
                        if (ei != null)
                        {
                            target.RemoveEnchantment(ei);
                        }
                    }
                }
            }
        }
        static public void SJOIN_HolyBonus(ISkillable source, ISkillable otherUnit, Skill skill, object data)
        {
            BaseUnit owner = source as BaseUnit;
            BaseUnit target = otherUnit as BaseUnit;
            if (owner == null || target == null) return;

            foreach (var s in skill.script)
            {
                if (s.triggerType == ESkillType.GroupChange)
                {
                    var enchToAdd = (Enchantment)DataBase.Get(s.stringData, false);

                    if (enchToAdd == null)
                        Debug.LogError("Skill " + skill.dbName + " StringData is not a skill.");
                    else
                    {
                        var targetEnchs = target.GetEnchantments();

                        if ((Enchantment)ENCH.HOLY_BONUS_UNIT_1 == enchToAdd &&
                            (targetEnchs.Find(o => o.source == enchToAdd ||
                                                   o.source == (Enchantment)ENCH.HOLY_BONUS_UNIT_2) == null))
                        {
                            target.AddEnchantment(enchToAdd, owner as IEnchantable);
                            target.GetAttributes().SetDirty();
                        }

                        if ((Enchantment)ENCH.HOLY_BONUS_UNIT_2 == enchToAdd &&
                            (targetEnchs.Find(o => o.source == enchToAdd) == null))
                        {
                            if (targetEnchs.Find(o => o.source == (Enchantment)ENCH.HOLY_BONUS_UNIT_1) != null)
                            {
                                target.RemoveEnchantment((Enchantment)ENCH.HOLY_BONUS_UNIT_1);
                            }

                            target.AddEnchantment(enchToAdd, owner as IEnchantable);
                            target.GetAttributes().SetDirty();
                        }
                    }
                }
            }
        }
        static public void SLEAVE_HolyBonus(ISkillable source, ISkillable otherUnit, Skill skill, object data)
        {
            BaseUnit owner = source as BaseUnit;
            BaseUnit target = otherUnit as BaseUnit;
            List<BaseUnit> unitsInGroup = data as List<BaseUnit>;

            if (owner == null || target == null) return;

            foreach (var s in skill.script)
            {
                if (s.triggerType == ESkillType.GroupChange)
                {
                    var enchToRemove = (Enchantment)DataBase.Get(s.stringData, false);

                    if (enchToRemove == null)
                        Debug.LogError("Skill " + skill.dbName + " StringData is not a skill.");
                    else
                    {
                        var targetEnchs = target.GetEnchantments();

                        if ((Enchantment)ENCH.HOLY_BONUS_UNIT_1 == enchToRemove &&
                            targetEnchs.Find(o => o.source == enchToRemove) != null)
                        {
                            var holyUnit = unitsInGroup?.Find(o => o.GetSkills().Contains((Skill)SKILL.HOLY_BONUS_GROUP_1));
                            if (holyUnit == null)
                            {
                                target.RemoveEnchantment(enchToRemove);
                            }
                        }

                        if ((Enchantment)ENCH.HOLY_BONUS_UNIT_2 == enchToRemove &&
                            targetEnchs.Find(o => o.source == enchToRemove) != null)
                        {
                            var holyUnit = unitsInGroup?.Find(o => o.GetSkills().Contains((Skill)SKILL.HOLY_BONUS_GROUP_2));
                            if (holyUnit == null)
                            {
                                target.RemoveEnchantment(enchToRemove);

                                holyUnit = unitsInGroup?.Find(o => o.GetSkills().Contains((Skill)SKILL.HOLY_BONUS_GROUP_1));
                                if (holyUnit != null)
                                    target.AddEnchantment((Enchantment)ENCH.HOLY_BONUS_UNIT_1, holyUnit as IEnchantable);
                            }
                        }
                    }
                }
            }
        }
        static public void SJOIN_ResistToAllBonus(ISkillable source, ISkillable otherUnit, Skill skill, object data)
        {
            BaseUnit owner = source as BaseUnit;
            BaseUnit target = otherUnit as BaseUnit;

            if (owner == null || target == null) return;

            foreach (var s in skill.script)
            {
                if (s.triggerType == ESkillType.GroupChange)
                {
                    var enchToAdd = (Enchantment)DataBase.Get(s.stringData, false);

                    if (enchToAdd == null)
                        Debug.LogError("Skill " + skill.dbName + " StringData is not a skill.");
                    else
                    {
                        //try to find an exceptive skill and compare bonus values
                        var prayermasterBonus = 0;
                        var prayermasterEnch = target.GetEnchantments().Find(o => o.source == (Enchantment)ENCH.PRAYMASTER_UNIT);
                        if (prayermasterEnch != null)
                        {
                            prayermasterBonus = prayermasterEnch.intParametr;
                        }

                        var targetEnchs = target.GetEnchantments();

                        if ((Enchantment)ENCH.RESIST_TO_ALL_UNIT_1 == enchToAdd &&
                            (targetEnchs.Find(o => o.source == enchToAdd || 
                                                   o.source == (Enchantment)ENCH.RESIST_TO_ALL_UNIT_2) == null))
                        {
                            if (prayermasterBonus > 0) continue; //RESIST_TO_ALL_UNIT_1 gives bonus 1 and any prayermasterBonus gives at least bonus 1, so no point to give new enchantment

                            target.AddEnchantment(enchToAdd, owner as IEnchantable);
                        }

                        if ((Enchantment)ENCH.RESIST_TO_ALL_UNIT_2 == enchToAdd &&
                            (targetEnchs.Find(o => o.source == enchToAdd) == null))
                        {
                            if(prayermasterEnch != null)
                            {
                                if (prayermasterBonus > 1) 
                                    continue; //RESIST_TO_ALL_UNIT_2 gives bonus 2 and prayermasterBonus above 1 gives at least bonus 2, so no point to give new enchantment
                                else
                                    target.RemoveEnchantment(prayermasterEnch); //if RESIST_TO_ALL_UNIT_2 gives higher bonus Prayermaster should be removed
                            }

                            if (targetEnchs.Find(o => o.source == (Enchantment)ENCH.RESIST_TO_ALL_UNIT_1) != null)
                            {
                                target.RemoveEnchantment((Enchantment)ENCH.RESIST_TO_ALL_UNIT_1);
                            }

                            target.AddEnchantment(enchToAdd, owner as IEnchantable);
                        }
                    }
                }
            }
        }
        static public void SLEAVE_ResistToAllBonus(ISkillable source, ISkillable otherUnit, Skill skill, object data)
        {
            BaseUnit owner = source as BaseUnit;
            BaseUnit target = otherUnit as BaseUnit;
            List<BaseUnit> unitsInGroup = data as List<BaseUnit>;

            if (owner == null || target == null) return;

            foreach (var s in skill.script)
            {
                if (s.triggerType == ESkillType.GroupChange)
                {
                    var enchToRemove = (Enchantment)DataBase.Get(s.stringData, false);

                    if (enchToRemove == null)
                        Debug.LogError("Skill " + skill.dbName + " StringData is not a skill.");
                    else
                    {
                        //try to find an exceptive skill and the highest bonus vale to compare later
                        float prayermasterBonus = 0f;
                        BaseUnit prayermasterUnit = null;
                        if (unitsInGroup != null && unitsInGroup.Count > 0)
                        {
                            float localBonus;
                            foreach (var u in unitsInGroup)
                            {
                                localBonus = 0f;
                                if (u.GetSkills().Contains((Skill)SKILL.PRAYMASTER_GROUP_SUPER))
                                {
                                    localBonus = u.GetLevel() * 1.5f;
                                    if (localBonus > prayermasterBonus)
                                    {
                                        prayermasterBonus = localBonus;
                                        prayermasterUnit = u;
                                    }
                                }
                                else if (u.GetSkills().Contains((Skill)SKILL.PRAYMASTER_GROUP))
                                {
                                    localBonus = u.GetLevel();
                                    if (localBonus > prayermasterBonus)
                                    {
                                        prayermasterBonus = localBonus;
                                        prayermasterUnit = u;
                                    }
                                }
                            }
                        }


                        var targetEnchs = target.GetEnchantments();
                        BaseUnit holyUnit = null;

                        if ((Enchantment)ENCH.RESIST_TO_ALL_UNIT_1 == enchToRemove &&
                            targetEnchs.Find(o => o.source == enchToRemove) != null)
                        {
                            holyUnit = unitsInGroup?.Find(o => o.GetSkills().Contains((Skill)SKILL.RESIST_TO_ALL_GROUP_1));
                            if (holyUnit == null)
                            {
                                target.RemoveEnchantment(enchToRemove);

                                if((int)prayermasterBonus > 0)
                                {
                                    if (target.GetEnchantments().Find(o => o.source == (Enchantment)ENCH.PRAYMASTER_UNIT) == null)
                                    {
                                        var ei = target.AddEnchantment((Enchantment)ENCH.PRAYMASTER_UNIT, prayermasterUnit as IEnchantable);
                                        ei.intParametr = (int)prayermasterBonus;
                                        target.GetAttributes().SetDirty();
                                    }
                                }
                            }
                        }
                        else if ((Enchantment)ENCH.RESIST_TO_ALL_UNIT_2 == enchToRemove &&
                            targetEnchs.Find(o => o.source == enchToRemove) != null)
                        {
                            holyUnit = unitsInGroup?.Find(o => o.GetSkills().Contains((Skill)SKILL.RESIST_TO_ALL_GROUP_2));
                            if (holyUnit == null)
                            {
                                target.RemoveEnchantment(enchToRemove);

                                if ((int)prayermasterBonus > 1)
                                {
                                    if (target.GetEnchantments().Find(o => o.source == (Enchantment)ENCH.PRAYMASTER_UNIT) == null)
                                    {
                                        var ei = target.AddEnchantment((Enchantment)ENCH.PRAYMASTER_UNIT, prayermasterUnit as IEnchantable);
                                        ei.intParametr = (int)prayermasterBonus;
                                        target.GetAttributes().SetDirty();
                                        continue;
                                    }
                                }

                                holyUnit = unitsInGroup?.Find(o => o.GetSkills().Contains((Skill)SKILL.RESIST_TO_ALL_GROUP_1));
                                if (holyUnit != null)
                                { 
                                    target.AddEnchantment((Enchantment)ENCH.RESIST_TO_ALL_UNIT_1, holyUnit as IEnchantable);
                                }
                                else
                                {
                                    if ((int)prayermasterBonus > 0)
                                    {
                                        if (target.GetEnchantments().Find(o => o.source == (Enchantment)ENCH.PRAYMASTER_UNIT) == null)
                                        {
                                            var ei = target.AddEnchantment((Enchantment)ENCH.PRAYMASTER_UNIT, prayermasterUnit as IEnchantable);
                                            ei.intParametr = (int)prayermasterBonus;
                                            target.GetAttributes().SetDirty();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        static public void SJOIN_Praymaster(ISkillable source, ISkillable otherUnit, Skill skill, object data)
        {
            BaseUnit owner = source as BaseUnit;
            BaseUnit target = otherUnit as BaseUnit;
            List<BaseUnit> unitsInGroup = data as List<BaseUnit>;

            if (owner == null || target == null) return;

            foreach (var s in skill.script)
            {
                if (s.triggerType == ESkillType.GroupChange)
                {
                    var enchToAdd = (Enchantment)DataBase.Get(s.stringData, false);

                    if (enchToAdd == null)
                        Debug.LogError("Enchantment " + skill.dbName + " StringData is not a enchantment.");
                    else
                    {
                        float skillBonus = 0f;

                        //find the highest value of bonus in group units
                        if (unitsInGroup != null && unitsInGroup.Count > 0)
                        {
                            float localBonus;
                            foreach (var u in unitsInGroup)
                            {
                                localBonus = 0f;
                                if (u.GetSkills().Contains((Skill)SKILL.PRAYMASTER_GROUP_SUPER))
                                {
                                    localBonus = u.GetLevel() * 1.5f;
                                    if (localBonus > skillBonus)
                                    {
                                        skillBonus = localBonus;
                                    }
                                }
                                else if (u.GetSkills().Contains((Skill)SKILL.PRAYMASTER_GROUP))
                                {
                                    localBonus = u.GetLevel();
                                    if (localBonus > skillBonus)
                                    {
                                        skillBonus = localBonus;
                                    }
                                }
                            }
                        }
                        
                        var ei = target.GetEnchantments().Find(o => o.source == enchToAdd);

                        //try to find an exceptive skill and compare bonus values
                        FInt alterBonus = FInt.ZERO;
                        var resist = target.GetEnchantments().Find(o => o.source == (Enchantment)ENCH.RESIST_TO_ALL_UNIT_1 ||
                                                                    o.source == (Enchantment)ENCH.RESIST_TO_ALL_UNIT_2);
                        if(resist != null) 
                        {
                            alterBonus = resist.source.Get().scripts[0].fIntData;
                        }
                        if( (int)skillBonus > alterBonus )
                        {
                            if(resist != null)
                                target.RemoveEnchantment(resist);

                            if (ei != null)
                            {
                                ei.intParametr = (int)skillBonus;
                                target.GetAttributes().SetDirty();
                            }
                            else
                            {
                                ei = target.AddEnchantment(enchToAdd, owner as IEnchantable, enchToAdd.lifeTime, null, 0);
                                ei.intParametr = (int)skillBonus;
                                target.GetAttributes().SetDirty();
                            }
                        }
                    }
                }
            }
        }
        static public void SLEAVE_Praymaster(ISkillable source, ISkillable otherUnit, Skill skill, object data)
        {
            BaseUnit owner = source as BaseUnit;
            BaseUnit target = otherUnit as BaseUnit;
            List<BaseUnit> unitsInGroup = data as List<BaseUnit>;
            if (owner == null || target == null) return;

            foreach (var s in skill.script)
            {
                if (s.triggerType == ESkillType.GroupChange)
                {
                    var enchToRemove = (Enchantment)DataBase.Get(s.stringData, false);

                    if (enchToRemove == null)
                        Debug.LogError("Enchantment " + skill.dbName + " StringData is not a enchantment.");
                    else
                    {
                        float skillBonus = 0f;
                        if (unitsInGroup != null && unitsInGroup.Count > 0)
                        {
                            float localBonus;
                            foreach (var u in unitsInGroup)
                            {
                                localBonus = 0f;
                                if (u.GetSkills().Contains((Skill)SKILL.PRAYMASTER_GROUP_SUPER))
                                {
                                    if (u == owner) continue;

                                    localBonus = (u.GetLevel() * 1.5f);
                                    if(localBonus > skillBonus)
                                    {
                                        skillBonus = localBonus;
                                    }
                                }
                                else if (u.GetSkills().Contains((Skill)SKILL.PRAYMASTER_GROUP))
                                {
                                    if (u == owner) continue;

                                    localBonus = u.GetLevel();
                                    if(localBonus > skillBonus)
                                    {
                                        skillBonus = localBonus;
                                    }
                                }
                            }
                        }

                        //try to find an exceptive skill and compare bonus values
                        var resistUnit = unitsInGroup?.Find(o => o.GetSkills().Contains((Skill)SKILL.RESIST_TO_ALL_GROUP_2));
                        bool resist2 = resistUnit != null;
                        bool resist1 = false;
                        
                        if (!resist2)
                        {
                            resistUnit = unitsInGroup?.Find(o => o.GetSkills().Contains((Skill)SKILL.RESIST_TO_ALL_GROUP_1));
                            resist1 = resistUnit != null;
                        }

                        var ei = target.GetEnchantments().Find(o => o.source == enchToRemove);
                        if ((int)skillBonus > 0)
                        {
                            if (resist2 && (int)skillBonus < 2)
                            {
                                if (ei != null)
                                {
                                    target.RemoveEnchantment(ei);
                                }
                                if(target.GetEnchantments().Find(o => o.source == (Enchantment)ENCH.RESIST_TO_ALL_UNIT_2) == null)
                                {
                                    target.AddEnchantment((Enchantment)ENCH.RESIST_TO_ALL_UNIT_2, resistUnit as IEnchantable);
                                }
                            }
                            else
                            {
                                if (ei != null)
                                {
                                    ei.intParametr = (int)skillBonus;
                                    target.GetAttributes().SetDirty();
                                }
                            }
                        }
                        else
                        {
                            if(ei != null)
                            {
                                target.RemoveEnchantment(ei);
                            }

                            if (resist2)
                            {
                                if(target.GetEnchantments().Find(o => o.source == (Enchantment)ENCH.RESIST_TO_ALL_UNIT_2) == null)                                
                                {
                                    target.AddEnchantment((Enchantment)ENCH.RESIST_TO_ALL_UNIT_2, resistUnit as IEnchantable);
                                }
                            }
                            else if (resist1)
                            {
                                if (target.GetEnchantments().Find(o => o.source == (Enchantment)ENCH.RESIST_TO_ALL_UNIT_1) == null)
                                {
                                    target.AddEnchantment((Enchantment)ENCH.RESIST_TO_ALL_UNIT_1, resistUnit as IEnchantable);
                                }
                            }
                        }
                    }
                }
            }
        }
        static public void SJOIN_Leadership(ISkillable source, ISkillable otherUnit, Skill skill, object data)
        {
            BaseUnit owner = source as BaseUnit;
            BaseUnit target = otherUnit as BaseUnit;
            List<BaseUnit> unitsInGroup = data as List<BaseUnit>;

            if (owner == null || target == null) return;

            // wont' apply if target is Fantastic Unit or Undead, or Chaos Channeled
            if (target.GetAttributes().Contains(TAG.FANTASTIC_CLASS) || target.race == (Race)RACE.REALM_DEATH) return;

            foreach (var s in skill.script)
            {
                if (s.triggerType == ESkillType.GroupChange)
                {
                    var enchToAdd = (Enchantment)DataBase.Get(s.stringData, false);

                    if (enchToAdd == null)
                        Debug.LogError("Enchantment " + skill.dbName + " StringData is not a enchantment.");
                    else
                    {
                        float skillBonus = 0f;
                        
                        //find the highest value of bonus in group units
                        if (unitsInGroup != null && unitsInGroup.Count > 0)
                        {
                            float localBonus;
                            foreach (var u in unitsInGroup)
                            {
                                localBonus = 0f;
                                if (u.GetSkills().Contains((Skill)SKILL.LEADERSHIP_GROUP_SUPER))
                                {
                                    localBonus = u.GetLevel() / 2;
                                    if (localBonus > skillBonus)
                                    {
                                        skillBonus = localBonus;
                                    }
                                }
                                else if (u.GetSkills().Contains((Skill)SKILL.LEADERSHIP_GROUP))
                                {
                                    localBonus = u.GetLevel() / 3;
                                    if (localBonus > skillBonus)
                                    {
                                        skillBonus = localBonus;
                                    }
                                }
                            }
                        }

                        if ((int)skillBonus > 0)
                        {
                            var ei = target.GetEnchantments().Find(o => o.source == enchToAdd);
                            if (ei != null)
                            {
                                ei.intParametr = (int)skillBonus;
                                target.GetAttributes().SetDirty();
                            }
                            else
                            {
                                ei = target.AddEnchantment(enchToAdd, owner as IEnchantable, enchToAdd.lifeTime, null, 0);
                                ei.intParametr = (int)skillBonus;
                                target.GetAttributes().SetDirty();
                            }
                        }
                    }
                }
            }
        }
        static public void SLEAVE_Leadership(ISkillable source, ISkillable otherUnit, Skill skill, object data)
        {
            BaseUnit owner = source as BaseUnit;
            BaseUnit target = otherUnit as BaseUnit;
            List<BaseUnit> unitsInGroup = data as List<BaseUnit>;
            if (owner == null || target == null) return;

            // wont' apply if target is Fantastic Unit or Undead, or Chaos Channeled
            if (target.GetAttributes().Contains(TAG.FANTASTIC_CLASS) || target.race == (Race)RACE.REALM_DEATH) return;

            foreach (var s in skill.script)
            {
                if (s.triggerType == ESkillType.GroupChange)
                {
                    var enchToRemove = (Enchantment)DataBase.Get(s.stringData, false);

                    if (enchToRemove == null)
                        Debug.LogError("Enchantment " + skill.dbName + " StringData is not a enchantment.");
                    else
                    {
                        float skillBonus = 0f;
                        if (unitsInGroup != null && unitsInGroup.Count > 0)
                        {
                            float localBonus;
                            foreach (var u in unitsInGroup)
                            {
                                localBonus = 0f;
                                if (u.GetSkills().Contains((Skill)SKILL.LEADERSHIP_GROUP_SUPER))
                                {
                                    if (u == owner) continue;

                                    localBonus = u.GetLevel() / 2;
                                    if (localBonus > skillBonus)
                                    {
                                        skillBonus = localBonus;
                                    }
                                }
                                else if (u.GetSkills().Contains((Skill)SKILL.LEADERSHIP_GROUP))
                                {
                                    if (u == owner) continue;

                                    localBonus = u.GetLevel() / 3;
                                    if (localBonus > skillBonus)
                                    {
                                        skillBonus = localBonus;
                                    }
                                }
                            }
                        }

                        var ei = target.GetEnchantments().Find(o => o.source == enchToRemove);
                        if ((int)skillBonus > 0)
                        {
                            if (ei != null)
                            {
                                ei.intParametr = (int)skillBonus;
                                target.GetAttributes().SetDirty();
                            }
                        }
                        else
                        {
                            if (ei != null)
                            {
                                target.RemoveEnchantment(ei);
                            }
                        }
                    }
                }
            }
        }static public void SJOIN_ShackleMaster(ISkillable source, ISkillable otherUnit, Skill skill, object data)
        {
            BaseUnit owner = source as BaseUnit;
            BaseUnit target = otherUnit as BaseUnit;
            List<BaseUnit> unitsInGroup = data as List<BaseUnit>;

            if (owner == null || target == null) return;

            if (target.dbSource != (Subrace)UNIT.B_ORC_RUSALKA &&
                target.dbSource != (Subrace)UNIT.B_ORC_SEA_CREATURE &&
                target.dbSource != (Subrace)UNIT.B_ORC_LEASHED) return;

            foreach (var s in skill.script)
            {
                if (s.triggerType == ESkillType.GroupChange)
                {
                    var enchToAdd = (Enchantment)DataBase.Get(s.stringData, false);

                    if (enchToAdd == null)
                        Debug.LogError("Enchantment " + skill.dbName + " StringData is not a enchantment.");
                    else
                    {
                        var ei = target.GetEnchantments().Find(o => o.source == enchToAdd);
                        if (ei == null)
                        {
                            ei = target.AddEnchantment(enchToAdd, owner as IEnchantable);
                            target.GetAttributes().SetDirty();
                        }
                    }
                }
            }
        }
        static public void SLEAVE_ShackleMaster(ISkillable source, ISkillable otherUnit, Skill skill, object data)
        {
            BaseUnit owner = source as BaseUnit;
            BaseUnit target = otherUnit as BaseUnit;
            List<BaseUnit> unitsInGroup = data as List<BaseUnit>;
            if (owner == null || target == null) return;

            if (target.dbSource != (Subrace)UNIT.B_ORC_RUSALKA &&
                target.dbSource != (Subrace)UNIT.B_ORC_SEA_CREATURE &&
                target.dbSource != (Subrace)UNIT.B_ORC_LEASHED) return;

            foreach (var s in skill.script)
            {
                if (s.triggerType == ESkillType.GroupChange)
                {
                    var enchToRemove = (Enchantment)DataBase.Get(s.stringData, false);

                    if (enchToRemove == null)
                        Debug.LogError("Enchantment " + skill.dbName + " StringData is not a enchantment.");
                    else
                    {
                        bool remove = true;
                        if (unitsInGroup != null && unitsInGroup.Count > 0)
                        {
                            foreach (var u in unitsInGroup)
                            {
                                if (u.GetSkills().Contains((Skill)SKILL.SHACKLE_MASTER))
                                {
                                    if (u == owner) continue;

                                    remove = false;
                                }
                                
                            }
                        }

                        if(remove)
                        {
                            var ei = target.GetEnchantments().Find(o => o.source == enchToRemove);
                            {
                                if (ei != null)
                                {
                                    target.RemoveEnchantment(ei);
                                }
                            }
                        }
                    }
                }
            }
        }
        static public void SJOIN_Armsmaster(ISkillable source, ISkillable otherUnit, Skill skill, object data)
        {
            BaseUnit owner = source as BaseUnit;
            BaseUnit target = otherUnit as BaseUnit;
            List<BaseUnit> unitsInGroup = data as List<BaseUnit>;

            if (owner == null || target == null) return;

            // wont' apply if target is Fantastic Unit or Hero class
            if (target.GetAttributes().Contains(TAG.FANTASTIC_CLASS) || target.dbSource.Get() is Hero) return;

            foreach (var s in skill.script)
            {
                if (s.triggerType == ESkillType.GroupChange)
                {
                    var enchToAdd = (Enchantment)DataBase.Get(s.stringData, false);

                    if (enchToAdd == null)
                        Debug.LogError("Enchantment " + skill.dbName + " StringData is not a enchantment.");
                    else
                    {
                        //find the highest value of bonus in group units
                        int skillBonus = 0;
                        if (unitsInGroup != null && unitsInGroup.Count > 0)
                        {
                            int localBonus;
                            foreach (var u in unitsInGroup)
                            {
                                localBonus = 0;
                                if (u.GetSkills().Contains((Skill)SKILL.ARMSMASTER_GROUP_SUPER))
                                {
                                    localBonus = u.GetLevel() * 3;
                                    if (localBonus > skillBonus)
                                    {
                                        skillBonus = localBonus;
                                    }
                                }
                                else if (u.GetSkills().Contains((Skill)SKILL.ARMSMASTER_GROUP))
                                {
                                    localBonus = u.GetLevel() * 2;
                                    if (localBonus > skillBonus)
                                    {
                                        skillBonus = localBonus;
                                    }
                                }
                            }
                        }

                        if (skillBonus > 0)
                        {
                            var ei = target.GetEnchantments().Find(o => o.source == enchToAdd);
                            if (ei != null)
                            {
                                ei.intParametr = skillBonus;
                                target.GetAttributes().SetDirty();
                            }
                            else
                            {
                                ei = target.AddEnchantment(enchToAdd, owner as IEnchantable, enchToAdd.lifeTime, null, 0);
                                ei.intParametr = skillBonus;
                                target.GetAttributes().SetDirty();
                            }
                        }
                    }
                }
            }
        }
        static public void SLEAVE_Armsmaster(ISkillable source, ISkillable otherUnit, Skill skill, object data)
        {
            BaseUnit owner = source as BaseUnit;
            BaseUnit target = otherUnit as BaseUnit;
            List<BaseUnit> unitsInGroup = data as List<BaseUnit>;
            if (owner == null || target == null) return;

            // wont' apply if target is Fantastic Unit or Hero class
            if (target.GetAttributes().Contains(TAG.FANTASTIC_CLASS) || target.dbSource.Get() is Hero) return;

            foreach (var s in skill.script)
            {
                if (s.triggerType == ESkillType.GroupChange)
                {
                    var enchToRemove = (Enchantment)DataBase.Get(s.stringData, false);

                    if (enchToRemove == null)
                        Debug.LogError("Enchantment " + skill.dbName + " StringData is not a enchantment.");
                    else
                    {
                        int skillBonus = 0;
                        if (unitsInGroup != null && unitsInGroup.Count > 0)
                        {
                            int localBonus;
                            foreach (var u in unitsInGroup)
                            {
                                localBonus = 0;
                                if (u.GetSkills().Contains((Skill)SKILL.ARMSMASTER_GROUP_SUPER))
                                {
                                    if (u == owner) continue;

                                    localBonus = u.GetLevel() * 3;
                                    if (localBonus > skillBonus)
                                    {
                                        skillBonus = localBonus;
                                    }
                                }
                                else if (u.GetSkills().Contains((Skill)SKILL.ARMSMASTER_GROUP))
                                {
                                    if (u == owner) continue;

                                    localBonus = u.GetLevel() * 2;
                                    if (localBonus > skillBonus)
                                    {
                                        skillBonus = localBonus;
                                    }
                                }
                            }
                        }
                        
                        var ei = target.GetEnchantments().Find(o => o.source == enchToRemove);
                        if (skillBonus > 0)
                        {
                            if (ei != null)
                            {
                                ei.intParametr = (int)skillBonus;
                                target.GetAttributes().SetDirty();
                            }
                        }
                        else
                        {
                            if (ei != null)
                            {
                                target.RemoveEnchantment(ei);
                            }
                        }
                    }
                }
            }
        }


        #endregion
        #region Turn/Battle Event Skills (Turn start, Battle End etc)        
        static public bool SKTurnTri_Heal(ISkillable source, Skill skill, SkillScript skillScript)
        {
            if(source is BaseUnit)
            {
                var bu = source as BaseUnit;
                if (bu.canNaturalHeal) return true;
            }
            return false;
        }
        static public object SKTurnAct_Heal(ISkillable source, Skill skill, SkillScript skillScript)
        {
            if (source is BaseUnit)
            {
                var bu = source as BaseUnit;
                bu.currentFigureHP = bu.GetAttributes().GetFinal(DBEnum.TAG.HIT_POINTS).ToInt();
                if(bu.dbSource.Get() is DBDef.Unit)
                {
                    var u = bu.dbSource.Get() as DBDef.Unit;
                    bu.figureCount = u.figures;
                }
                
            }
            return false;
        }
        static public object SKTurnTri_Haste(ISkillable source, Skill skill, SkillScript skillScript)
        {
            if (source is BattleUnit)
            {
                var bu = source as BattleUnit;
                if (bu.canMove)
                {
                    int unitMove = bu.GetAttFinal((Tag)TAG.MOVEMENT_POINTS).ToInt();
                    bu.Mp += unitMove;

                    if (BattleHUD.GetSelectedUnit() == bu && bu.GetWizardOwnerID() == 1)
                    {
                        MHEventSystem.TriggerEvent<BattleHUD>(BattleHUD.Get(), null);
                    }
                }
            }

            return false;
        }
        static public object SKTurnTri_Cursed(ISkillable source, Skill skill, SkillScript skillScript)
        {
            if (source is BattleUnit)
            {
                var bu = source as BattleUnit;
                var b = Battle.GetBattle();
                if (b != null)
                {
                    var enemy = b.GetAllUnits().FindAll(o => o.GetWizardOwnerID() != bu.GetWizardOwnerID());
                    if(skill.script[0].stringData != null)
                    foreach (var e in enemy)
                    {
                            if (e.GetEnchantments().Find(o => o.source.Get() == (Enchantment)ENCH.CURSED_UNIT) != null) continue;

                            var ench = (Enchantment)DataBase.Get(skill.script[0].stringData, false);
                            if (ench == null&& skill.script[0] != null && skill.script[0].fIntParam != null) break;

                            var ei = e.AddEnchantment(ench, bu as Entity);
                            ei.intParametr = skill.script[0].fIntParam.ToInt();
                            e.GetAttributes().SetDirty();
                    }
                }
            }

            return false;
        }
        static public object SKTurnTri_GhostshipTerror(ISkillable source, Skill skill, SkillScript skillScript)
        {
            if (source is BattleUnit)
            {
                var bu = source as BattleUnit;
                var b = Battle.GetBattle();
                if (b != null)
                {
                    BattlePlayer enemyWizard;
                    if (b.attackerUnits.Find(o => o == bu) != null)
                    {
                        enemyWizard = b.defender;
                    }
                    else
                    {
                        enemyWizard = b.attacker;
                    }
                    var ench = (Enchantment)DataBase.Get(skill.script[0].stringData, false);
                    if(ench == null || enemyWizard.GetEnchantments().Find(o => o.source.Get() == ench) != null) return false;

                    enemyWizard.AddEnchantment(ench, bu as Entity);
                }
            }

            return false;
        }
        #endregion
        #region Description scripts
        static public string DSCR_Throw(ISkillable source, Skill skill, DBDef.Unit dbUnit = null )
        {
            var value = 0;
            if (source is MOM.BaseUnit && skill != null)
            {
                var unit = source as MOM.BaseUnit;
                value = (skill.script[0].fIntParam + unit.GetAttFinal(TAG.THROW_BONUS)).ToInt();
            }
            else if (dbUnit != null && skill != null)
            {
                value = dbUnit.GetTag(TAG.THROW_BONUS).ToInt();
            }
            else if (skill != null)
            {
                value = skill.script[0].fIntParam.ToInt();
            }

            return DBUtils.Localization.Get("DES_THROWN", true, value);
        }
        static public string DSCR_FireBreath(ISkillable source, Skill skill, DBDef.Unit dbUnit = null)
        {
            var value = 0;
            if (source is MOM.BaseUnit && skill != null)
            {
                var unit = source as MOM.BaseUnit;
                value = (skill.script[0].fIntParam + unit.GetAttFinal(TAG.FIRE_BREATH_BONUS)).ToInt();
            }
            else if (dbUnit != null && skill != null)
            {
                value = dbUnit.GetTag(TAG.FIRE_BREATH_BONUS).ToInt();
            }
            else if(skill != null)
            {
                value = skill.script[0].fIntParam.ToInt();
            }

            return DBUtils.Localization.Get("DES_FIRE_BREATH", true, value);
        }
        static public string DSCR_ColdBreath(ISkillable source, Skill skill, DBDef.Unit dbUnit = null)
        {
            var value = 0;
            if (source is MOM.BaseUnit && skill != null)
            {
                var unit = source as MOM.BaseUnit;
                value = (skill.script[0].fIntParam + unit.GetAttFinal(TAG.FIRE_BREATH_BONUS)).ToInt();
            }
            else if (dbUnit != null && skill != null)
            {
                value = dbUnit.GetTag(TAG.FIRE_BREATH_BONUS).ToInt();
            }
            else if (skill != null)
            {
                value = skill.script[0].fIntParam.ToInt();
            }

            return DBUtils.Localization.Get("DES_COLD_BREATH", true, value);
        }
        static public string DSCR_LighteningBreath(ISkillable source, Skill skill, DBDef.Unit dbUnit = null)
        {
            var value = 0;
            if (source is MOM.BaseUnit && skill != null)
            {
                var unit = source as MOM.BaseUnit;
                value = (skill.script[0].fIntParam + unit.GetAttFinal(TAG.FIRE_BREATH_BONUS)).ToInt();
            }
            else if (dbUnit != null && skill != null)
            {
                value = dbUnit.GetTag(TAG.FIRE_BREATH_BONUS).ToInt();
            }
            else if (skill != null)
            {
                value = skill.script[0].fIntParam.ToInt();
            }

            return DBUtils.Localization.Get("DES_LIGHTNING_BREATH", true, value);
        }
        static public string DSCR_DoomGaze(ISkillable source, Skill skill, DBDef.Unit dbUnit = null)
        {
            var value = 0;
            if (source is MOM.BaseUnit && skill != null)
            {
                var unit = source as MOM.BaseUnit;
                value = (skill.script[0].fIntParam + unit.GetAttFinal(TAG.DOOM_GAZE_BONUS)).ToInt();
            }
            else if (dbUnit != null && skill != null)
            {
                value = dbUnit.GetTag(TAG.DOOM_GAZE_BONUS).ToInt();
            }
            else if (skill != null)
            {
                value = skill.script[0].fIntParam.ToInt();
            }

            return DBUtils.Localization.Get("DES_DOOM_GAZE", true, value);
        }
        static public string DSCR_Caster(ISkillable source, Skill skill, DBDef.Unit dbUnit = null)
        {
            var value = 0;
            if (source is MOM.BaseUnit && skill != null)
            {
                var unit = source as MOM.BaseUnit;
                value = unit.GetAttFinal(TAG.MANA_POINTS).ToInt();
            }
            else if (dbUnit != null && skill != null)
            {
                value = dbUnit.GetTag(TAG.MANA_POINTS).ToInt();
            }
            else if (skill != null)
            {
                value = skill.script[0].fIntParam.ToInt();
            }

            return DBUtils.Localization.Get("DES_CASTER", true, value);
        }
        static public string DSCR_Scouting(ISkillable source, Skill skill, DBDef.Unit dbUnit = null)
        {
            var value = 0;
            if (source is MOM.BaseUnit && skill != null)
            {
                var unit = source as MOM.BaseUnit;
                value = unit.GetAttFinal(TAG.SIGHT_RANGE_BONUS).ToInt();
            }
            else if (dbUnit != null && skill != null)
            {
                value = dbUnit.GetTag(TAG.SIGHT_RANGE_BONUS).ToInt();
            }
            else if (skill != null)
            {
                value = skill.script[0].fIntParam.ToInt();
            }

            return DBUtils.Localization.Get("DES_SCOUTING", true, value);
        }
        static public string DSCR_Bleeding(ISkillable source, Skill skill, DBDef.Unit dbUnit = null)
        {
            var value = 0;
            if (source is MOM.BaseUnit && skill != null)
            {
                var unit = source as MOM.BaseUnit;
                value = unit.GetAttFinal(TAG.BLEEDING).ToInt();
            }
            else if (dbUnit != null && skill != null)
            {
                value = dbUnit.GetTag(TAG.BLEEDING).ToInt();
            }

            return DBUtils.Localization.Get("DES_BLEEDING_UNIT", true, value);
        }
        static public string DSCR_Bite(ISkillable source, Skill skill, DBDef.Unit dbUnit = null)
        {
            var value = 0;
            if (source is MOM.BaseUnit && skill != null)
            {
                var unit = source as MOM.BaseUnit;
                value = (skill.script[0].fIntParam).ToInt();
            }
            else
            {
                value = skill.script[0].fIntParam.ToInt();
            }

            return DBUtils.Localization.Get("DES_PIERCING_BITE", true, value);
        }
        #endregion
    }
}
#endif