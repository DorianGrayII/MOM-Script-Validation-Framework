#if !USE_DEBUG_SCRIPT || !UNITY_EDITOR

using DBDef;
using DBEnum;
using MHUtils;
using MOM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using WorldCode;

namespace MOMScripts
{
    public class BattleAIScripts : ScriptBase
    {
        #region ver 2
        static public IEnumerator AITurnV02(Battle battle, bool manageAttacker)
        {
            var w = manageAttacker ? battle.attacker : battle.defender;
            //NOTE: v1 spell casting in use!
            if (GameManager.Get() == null || 
                GameManager.Get().useManaInAutoresolves ||
                w.GetWizardOwner() != GameManager.GetHumanWizard())
            {
                yield return CastSpells(w, battle);
            }            

            List<BattleUnit> allyUnits;
            List<BattleUnit> enemyUnits;

            if (manageAttacker)
            {
                allyUnits = battle.attackerUnits.FindAll(o => o.IsAlive());
                enemyUnits = battle.defenderUnits.FindAll(o => o.IsAlive());
            }
            else
            {
                allyUnits = battle.defenderUnits.FindAll(o => o.IsAlive());
                enemyUnits = battle.attackerUnits.FindAll(o => o.IsAlive());
            }

            yield return AIFramesV02(w, allyUnits, enemyUnits, battle);

            if (w != battle.GetHumanPlayer() ||
               battle.GetHumanPlayer().autoPlayByAI)
            {
                battle.activeTurn.AITurnEnd();
            }
        }
        static IEnumerator AIFramesV02(BattlePlayer w,
                                    List<BattleUnit> allyUnits,
                                    List<BattleUnit> enemyUnits,
                                    Battle battle)
        {
            while (battle.activeTurn == null) yield return null;

            foreach (var v in allyUnits)
            {
                while (v.Mp > 0 && v.IsAlive())
                {
                    yield return battle.WaitForAttention();
                    if(w == battle.GetHumanPlayer() && 
                        !battle.GetHumanPlayer().autoPlayByAI) yield break;
                    var mp = v.Mp;
                    yield return UnitTurnV02(w, v, allyUnits, enemyUnits, battle, false, false);
                    //if unit did not use any MP, it might be grounded with no actions to carry.
                    //to avoid soft locks, skip this unit.
                    if (mp == v.Mp) break;
                }
            }
        }

        static IEnumerator UnitTurnV02(BattlePlayer w, 
                                    BattleUnit u,
                                    List<BattleUnit> allyUnits,
                                    List<BattleUnit> enemyUnits,
                                    Battle battle,
                                    bool seekThroughWalls,
                                    bool forceSeekThroughGates )
        {
            // A) no MP: end unit turn
            // B) pick preferred target 
            // simulate or load from cache to know how much gain you can gain on specific attack
            // C) pathfind to attack position. 
            //Failure: find next target (B)
            //Success: attack if in range
            // D) goto (A)

            foreach(var v in allyUnits)
            {
                if(v.IsAlive() && !battle.plane.GetSearcherData().IsUnitAt(v.GetPosition()))
                {
                    Debug.LogError("AI unit missing marker on searcher data!");
                }
            }
            foreach (var v in enemyUnits )
            {
                if (v.IsAlive() && !battle.plane.GetSearcherData().IsUnitAt(v.GetPosition()))
                {
                    Debug.LogError("AI's enemy unit missing marker on searcher data!");
                }
            }

            //consider spell casting if possible
            yield return CastSpells(u, w, battle);
            if (u.spellCasted) yield break;

            List<AIAttackOption> options = battle.aIBattleTactics.GetOptions(u, enemyUnits, battle);

            options.Sort((a, b)=>
            {
                //sorting criteria
                //by negating target change, high value represents large gain.
                //Note: sort might be adjusted for different tactics to consider only 50% or none of own change when considering plans
                int gainA = a.ownValueChange - a.targetValueChange;
                int gainB = b.ownValueChange - b.targetValueChange;

                //large first order
                return -gainA.CompareTo(gainB);
            });

            //find area attacker may reach.
            SearcherDataV2 sd = battle.plane.GetSearcherData();
            RequestDataV2 rd = RequestDataV2.CreateRequest(battle.plane, u.GetPosition(), u.Mp, u, true);
            rd.ignoreWalls = seekThroughWalls;
            rd.stoppedByGate = forceSeekThroughGates ? false : rd.stoppedByGate;

            PathfinderV2.FindArea(rd);
            var area = rd.GetArea();
            var areaNoObstacle = rd.GetNoObstackleArea();

            //for teleporting units add 1 hex area around enemies
            if (u.teleporting)
            {
                foreach (var v in enemyUnits)
                {
                    var pos = v.GetPosition();                    
                    bool possible = false;
                    foreach (var k in HexNeighbors.neighbours )
                    {
                        var kPos = pos + k;
                        if (battle.IsLocationFreeForUnit(kPos, !u.attackingSide))
                        {
                            //add to area only if neighbor is not in area and is not blocked by obstacle
                            //ie: it is possible to teleport next to that location
                            if(!area.Contains(kPos)) area.Add(kPos);
                            possible = true;
                        }
                    }
                    if(possible)
                    {
                        //add enemy position to area if at least 1 neighboring location was possible
                        if (!area.Contains(pos)) area.Add(pos);
                    }
                }
            }

            if (allyUnits != null)
            {
                //exclude ally locations from area
                foreach (var v in allyUnits)
                {
                    if (v == u) continue;
                    var pos = v.GetPosition();
                    if (area.Contains(pos)) area.Remove(pos);
                }
            }

            bool longRange = u.GetAttFinal((Tag)TAG.LONG_RANGE) > 0 || 
                             u.GetAttFinal((Tag)TAG.MAGIC_RANGE) > 0;

            //consider all options in order from most valuable to the least valuable
            for (int i = 0; i < options.Count; i++)
            {
                var option = options[i];
                if (!option.attackValid) continue;

                var targets = battle.aIBattleTactics.FilterByOption(enemyUnits, option);
                if (targets == null || targets.Count < 1) continue;

                targets.SortInPlace(delegate (BattleUnit a, BattleUnit b)
                {
                    var distA = HexCoordinates.HexDistance(a.GetPosition(), u.GetPosition());
                    var distB = HexCoordinates.HexDistance(b.GetPosition(), u.GetPosition());
                    return distA.CompareTo(distB);
                });

                if (option.melee)
                {
                    if (area != null)
                    {
                        BattleUnit preferredTarget = null;
                        foreach (var t in targets)
                        {
                            if (area.Contains(t.GetPosition()))
                            {
                                preferredTarget = t;
                                break;
                            }
                        }

                        if (preferredTarget != null)
                        {
                            //execute moveTo and attack
                            List<Vector3i> path = null;
                            if(u.teleporting)
                            {
                                path = new List<Vector3i>{ u.GetPosition() };

                                var targetPos = preferredTarget.GetPosition();
                                foreach (var k in HexNeighbors.neighbours)
                                {
                                    var kPos = targetPos + k;
                                    if (battle.IsLocationFreeForUnit(kPos, !u.attackingSide))
                                    {
                                        path.Add(kPos);
                                        break;
                                    }
                                }
                                path.Add(targetPos);
                            }
                            else
                            {                                
                                path = rd.GetPathTo(preferredTarget.GetPosition());
                            }
                            
                            if (path != null && path.Count > 2)
                            {
                                yield return battle.WaitForAttention();

                                path.RemoveLast();
                                Vector3i destination = path[path.Count - 1];
                                CutByEarlierObstacle(path, sd, battle, u);

                                path = u.CutPathToMP(battle, u.Mp, path, true, false);
                                if (path.Count > 1)
                                {
                                    DetectInvalidLocation(path[path.Count - 1], sd, "A " + seekThroughWalls);
                                    battle.GainAttention(u.GetOrCreateFormation());
                                    u.MoveViaPath(path);

                                    UpdateAIUnitInfo(u);
                                }

                                yield return battle.WaitForAttention();
                                //if movement was stopped abruptly, reconsider actions
                                if (path.Count < 2 || path[path.Count - 1] != destination) yield break;

                                battle.plane.ClearSearcherData();
                            }

                            yield return battle.WaitForAttention();
                            if (u.Mp == 0) yield break;

                            yield return battle.AttackUnit(u, preferredTarget);                            

                            yield return battle.WaitForAttention();
                            if (preferredTarget.IsAlive() && u.IsAlive() && u.Mp > 0)
                            {
                                yield return battle.AttackUnit(u, preferredTarget);
                            }

                            yield break;
                        }
                    }
                }
                else //Ranged
                {
                    BattleUnit preferredTarget = null;
                    Vector3i target = Vector3i.zero;
                    for (int k=0; k<targets.Count; k++)
                    {
                        var potentialTarget = targets[k];
                        target = potentialTarget.GetPosition();
                        if (preferredTarget != null &&
                            HexCoordinates.HexDistance(u.GetPosition(), preferredTarget.GetPosition()) <=
                            HexCoordinates.HexDistance(u.GetPosition(), target))
                        {
                            //we have target and we it is closer than current target, no need to check further
                            continue;
                        }

                        if (battle.darknessWall && battle.AttactThroughWall(u.GetPosition(),target))
                        {
                            if (u.GetAttFinal(TAG.ILLUSIONS_IMMUNITY) <= 0)
                            {
                                //unit is not immune to illusions, it cannot attack through darkness wall
                                continue;
                            }
                        }

                        if (!longRange && potentialTarget.GetAttFinal(TAG.MISSILE_IMMUNITY) > 0)
                        {
                            //target is immune to missiles, it cannot be attacked by normal ranged units
                            continue;
                        }

                        preferredTarget = potentialTarget;
                    }
                    
                    //if none of the target fulfill the requirements not being hidden by wall, continue
                    if (preferredTarget == null) continue;
                    
                    target = preferredTarget.GetPosition();
                    int dist = HexCoordinates.HexDistance(u.GetPosition(), target);
                    FInt mpToShoot = u.Mp - 1;

                    List<Multitype<Vector3i, int>> shootLocations = null;

                    if (longRange)
                    {
                        //consider optimal shooting range
                        shootLocations = ReachShootOptions(areaNoObstacle, target, rd, enemyUnits, 2, 30, false);

                        //consider optimal shooting range with enemy in contact
                        if (shootLocations == null) shootLocations = ReachShootOptions(areaNoObstacle, target, rd, enemyUnits, 2, 30, true);

                        if (shootLocations == null) continue;

                        //1. pick location closest to distance 8 to the enemy. Even if not reachable it may be better positioning for next turn shoot                    
                        //2. second criteria is shortest number of hexes passed to reach destination

                        shootLocations.SortInPlace(delegate (Multitype<Vector3i, int> a, Multitype<Vector3i, int> b)
                        {
                            int targetDist = 8;
                            // compare distance to closest enemy, and pick places closest to distance "targetDist"
                            //this way ranged units would keep distance, but wont run to the other side of the map
                            int distA = Mathf.Abs(targetDist - a.t1);
                            int distB = Mathf.Abs(targetDist - b.t1);

                            int ret = distA.CompareTo(distB);

                            //if equal then pick place that is closest to current position.
                            //This way unit wont move if this does not provide strategic advantage 
                            if (ret == 0)
                            {
                                distA = Mathf.Abs(HexCoordinates.HexDistance(u.GetPosition(), a.t0));
                                distB = Mathf.Abs(HexCoordinates.HexDistance(u.GetPosition(), b.t0));
                                ret = distA.CompareTo(distB);
                            }

                            return ret;
                        });
                    }
                    else
                    {
                        int maxRange = dist + u.Mp.ToInt() - 1;
                        if (dist - mpToShoot > maxRange)
                        {
                            //even on most favorable terrain, this attack cannot reach favorable range.
                            continue;
                        }

                        //consider optimal shooting range
                        shootLocations = ReachShootOptions(areaNoObstacle, target, rd, enemyUnits, 2, 3, false);

                        //consider suboptimal shooting range
                        if (shootLocations == null) shootLocations = ReachShootOptions(areaNoObstacle, target, rd, enemyUnits, 4, maxRange, false);

                        //consider optimal shooting range with enemy in contact
                        if (shootLocations == null) shootLocations = ReachShootOptions(areaNoObstacle, target, rd, enemyUnits, 2, 3, true);

                        //consider suboptimal shooting range with enemy in contact
                        if (shootLocations == null) shootLocations = ReachShootOptions(areaNoObstacle, target, rd, enemyUnits, 4, maxRange, true);


                        if (shootLocations == null) continue;

                        shootLocations.SortInPlace(delegate (Multitype<Vector3i, int> a, Multitype<Vector3i, int> b)
                        {
                            //as close to distance 3 as possible
                            int targetDist = 3;
                            int distA = Mathf.Abs(HexCoordinates.HexDistance(a.t0, target) - targetDist);
                            int distB = Mathf.Abs(HexCoordinates.HexDistance(b.t0, target) - targetDist);

                            int ret = distA.CompareTo(distB);

                            //if equal, then compare distance to closest enemy, and pick further away place
                            if(ret == 0)
                            {
                                ret = -a.t1.CompareTo(b.t1);
                            }

                            //if still equal then pick place that is closest to current position.
                            //This way unit wont move if this does not provide strategic advantage 
                            if (ret == 0)
                            {
                                distA = Mathf.Abs(HexCoordinates.HexDistance(u.GetPosition(), a.t0));
                                distB = Mathf.Abs(HexCoordinates.HexDistance(u.GetPosition(), b.t0));
                                ret = distA.CompareTo(distB);
                            }

                            return ret;
                        });
                    }

                    var shootLocation = shootLocations[0].t0;

                    //execute moveTo and attack
                    var path = rd.GetPathTo(shootLocation);
                    if (path != null && path.Count >= 2)
                    {
                        yield return battle.WaitForAttention();

                        Vector3i destination = path[path.Count - 1];

                        CutByEarlierObstacle(path, sd, battle, u);
                        path = u.CutPathToMP(battle, u.Mp, path, true, false);
                        if (path.Count > 1)
                        {
                            DetectInvalidLocation(path[path.Count - 1], sd, "B " + seekThroughWalls);
                            battle.GainAttention(u.GetOrCreateFormation());
                            u.MoveViaPath(path);

                            UpdateAIUnitInfo(u);
                        }

                        yield return battle.WaitForAttention();
                        //if movement was stopped abruptly, reconsider actions
                        if (path.Count < 2 || path[path.Count - 1] != destination) yield break;

                        battle.plane.ClearSearcherData();
                    }

                    yield return battle.WaitForAttention();
                    if (u.Mp == 0) yield break;

                    yield return battle.AttackUnit(u, preferredTarget);
                    yield break;
                }
            }
            if (u.Mp == 0) yield break;

            //no attack was sensible, just move toward best target
            //maybe it should be closest among all targets? 
            //but this is easy to exploit by manipulation what is closer what is not.
            //therefore sorting happens only within chosen target group

            for (int i = 0; i < options.Count; i++)
            {
                var option = options[i];
                if (!option.attackValid) continue;

                var targets = battle.aIBattleTactics.FilterByOption(enemyUnits, option);
                if (targets == null || targets.Count < 1) continue;

                targets.SortInPlace(delegate (BattleUnit a, BattleUnit b)
                {
                    var distA = HexCoordinates.HexDistance(a.GetPosition(), u.GetPosition());
                    var distB = HexCoordinates.HexDistance(b.GetPosition(), u.GetPosition());
                    return distA.CompareTo(distB);
                });
                
                BattleUnit preferredTarget = null;
                List<Vector3i> path = null;
                for (int k = 0; k < targets.Count; k++)
                {
                    //target first enemy in the list,
                    //if we cannot pathfind to it, try next one
                    preferredTarget = targets[k];

                    RequestDataV2 rd2 = RequestDataV2.CreateRequest(battle.plane, u.GetPosition(), preferredTarget.GetPosition(), u);
                    rd2.ignoreWalls = seekThroughWalls;
                    rd2.stoppedByGate = forceSeekThroughGates ? false : rd2.stoppedByGate;
                    PathfinderV2.FindPath(rd2);

                    path = rd2.GetPath();
                    if (path != null && path.Count > 2) break;
                }

                if (path != null && path.Count > 2)
                {
                    yield return battle.WaitForAttention();

                    path.RemoveLast();
                    CutByEarlierObstacle(path, sd, battle, u);

                    path = u.CutPathToMP(battle, u.Mp, path, true, false);
                    if (path.Count > 1)
                    {
                        DetectInvalidLocation(path[path.Count - 1], sd, "C " + seekThroughWalls);
                    }
                    else
                    {
                        continue;
                    }
                    battle.GainAttention(u.GetOrCreateFormation());
                    u.MoveViaPath(path);

                    UpdateAIUnitInfo(u);

                    battle.plane.ClearSearcherData();
                    yield break;
                }
            }

            //unit did not choose to act, 
            //if map contains walls and unit is not flying, it may seek the action again with the walls "turned off"
            //if this was already tried or criteria are not fulfilled there is no point in attempting the same for the second time

            if (!seekThroughWalls && battle.battleWalls.Count > 0 && !rd.nonCorporeal && !forceSeekThroughGates)
            {
                yield return UnitTurnV02(w, u, allyUnits, enemyUnits, battle, false, true);
            }

            if (u.attackingSide && u.Mp > 0 && battle.battleWalls.Count > 0)
            {
                //attacker may check if it is standing next to the wall, as it did not use all MP, it may attack the wall.
                var wallCrusher = u.GetAttFinal((Tag)TAG.WALL_CRUSHER) > FInt.ZERO;
                BattleWall wall = null;
                int distance = int.MaxValue;

                foreach (var v in battle.battleWalls)
                {
                    if (!v.standing) continue;

                    int dist = HexCoordinates.HexDistance(v.position, u.battlePosition);
                    if (distance > dist && 
                        ( wallCrusher && (dist < 2 || u.GetCurentFigure().rangedAmmo > 0) || v.gate && dist < 2))
                    {
                        wall = v;
                    }
                }

                if (wall != null)
                {                    
                    while (wall != null && u.Mp > 0)
                    {
                        yield return battle.AttackWall(u, wall);
                    }
                }
            }

            u.Mp = FInt.ZERO;
        }
        
        static List<Multitype<Vector3i, int>> ReachShootOptions(List<Vector3i> area,
                                          Vector3i target,
                                          RequestDataV2 rd,
                                          List<BattleUnit> enemyUnits,
                                          int minRange, int maxRange, bool allowOtherContact)
        {
            List<Multitype<Vector3i, int>> newArea = null;            
            foreach (var v in area)
            {
                var dist = HexCoordinates.HexDistance(target, v);
                if (dist >= minRange && dist <= maxRange)
                {
                    //does unit have chance to shoot from that location? Only if not all MP are used out
                    if (rd.GetCostTo(v) >= rd.mpRange) continue;
                    
                    bool valid = true;
                    int minDIst = int.MaxValue;

                    foreach (var e in enemyUnits)
                    {
                        if (e.GetPosition() == v) continue;

                        var distE = HexCoordinates.HexDistance(e.GetPosition(), v);
                        if (!allowOtherContact)
                        {                            
                            if (distE < 2)
                            {
                                valid = false;
                                break;
                            }
                        }
                        minDIst = Mathf.Min(minDIst, distE);
                    }
                    if(valid)
                    {
                        if (newArea == null)
                        {
                            newArea = new List<Multitype<Vector3i, int>>();
                        }

                        newArea.Add(new Multitype<Vector3i, int>(v, minDIst));                        
                    }
                }
            }

            return newArea;
        }
        static IEnumerator CastSpells(BattlePlayer w, Battle battle)
        {
            if (w == null || w.wizard == null || w.spellCasted) yield break;
            if (w.castingBlock) yield break;

            var spells = w.wizard.GetSpells();
            if (spells == null || w.wizard.wizardTower?.Get() == null) yield break;


            var ownPower = battle.GetStrategicValue(battle.attacker == w);
            var otherPower = battle.GetStrategicValue(battle.attacker != w);

            //current situation, compare both sides:
            //advantage of 5x or more. use maximum of 5% mana per spell if possible (use spells only if mana supplies are very large)
            //advantage of 2x or more. use maximum of 20% mana per spell if possible (use spells only if mana supplies are fair)
            //advantage of 1x or more. use maximum of 50% mana per spell if possible (use spells only if mana supplies are fair)
            //disadvantage use spell only if % of advantage gained is large enough in comparison to share or mana invested.
            //          ie: 0.3 advantage -> 0.5 advantage is 20% increase.  for which use maximum of 40% mana.
            //              0.5 advantage -> 0.9 advantage is 40% increase. for which use maximum of 80% mana.

            //use largest gain spell that fits mana limit profile. 
            //there is an option to do spell value sort based on mana/value proportion, but it does not fit current casting design.

            int maxMana = w.mana;

            float advantage = otherPower == 0 ? 10f : ownPower / (float)otherPower;
            int manaToUse = 0;
            if (advantage >= 5f)
            {
                manaToUse = (int)Mathf.Min(maxMana, w.mana * 0.05f);
            }
            else if (advantage >= 2f)
            {
                manaToUse = (int)Mathf.Min(maxMana, w.mana * 0.2f);
            }
            else if (advantage > 1f)
            {
                manaToUse = (int)Mathf.Min(maxMana, w.mana * 0.5f);
            }
            else
            {
                manaToUse = maxMana;
            }
            
            spells = spells.FindAll(o => !string.IsNullOrEmpty(o.Get().battleScript) &&
                                         o.Get().GetBattleCastingCostByDistance(w.GetWizardOwner()) <= manaToUse &&
                                         o.Get().GetBattleCastingCost(w.GetWizardOwner()) <= w.castingSkill);

            if (spells.Count > 0)
            {
                var ai = DifficultySettingsData.GetSetting("UI_DIFF_AI_SKILL");
                if (ai == null)
                {
                    Debug.LogError("Cannot resolve casting, no AI settings!");
                }
                var options = new List<Multitype<Vector3i, int, Spell>>();
                float castingShare = 0;
                int aiSkill = 0;
                switch (ai.value)
                {
                    case "1":
                        //beginner
                        //consider only 1 spell or 20% of spells whichever is higher                        
                        castingShare = 0.2f;
                        aiSkill = 1;
                        break;
                    case "2":
                        //advanced
                        //consider only 1 spell or 50% of spells whichever is higher
                        castingShare = 0.5f;
                        aiSkill = 2;
                        break;
                    case "3":
                        //skilled
                        //consider only 1 spell or 80% of spells whichever is higher
                        castingShare = 0.8f;
                        aiSkill = 3;
                        break;
                    case "4":
                        //master
                        //consider all spells
                        castingShare = 1f;
                        aiSkill = 4;
                        break;
                    default:
                        Debug.LogWarning("AI setting not implemented!");

                        break;
                }

                if(w.playerOwner)
                {
                    castingShare = 1f;
                    aiSkill = 4;
                }

                if (spells.Count > 1) spells.RandomSort();
                int maxCastConsiderede = (int)(spells.Count * castingShare);
                
                //if list would be build it would be cached for all cast considerations that are relevant to the spell.
                HashSet<Vector3i> hexesCloseToUnits = new HashSet<Vector3i>();

                for (int i = 0; i < 1 || i < maxCastConsiderede; i++)
                {
                    var data = CastingSimulation(spells[i].Get(), aiSkill, battle, w.wizard, w, hexesCloseToUnits);
                    if (data != null) options.Add(data);
                }

                if (options.Count < 1) yield break;

//                 
// 
//                 if (advantage >= 1f)
//                 {
//                     //mana limits what was chosen to consider, therefore now just pick best
//                     var powerExpected = otherPower > 0 ? 1f / otherPower : 0;
//                     var manaAllowed = w.mana > 0 ? 2f / w.mana : 0;
// 
//                     options = options.FindAll(o => o.t1 * powerExpected >= o.t2.battleCost * manaAllowed);
//                 }

                if (options.Count > 0)
                {
                    Multitype<Vector3i, int, Spell> choice = null;
                    foreach(var v in options)
                    {
                        int cost = v.t2.GetBattleCastingCostByDistance(w.GetWizardOwner());
                        
                        v.t1 = v.t2.GetSpellTacticalValue(cost, v.t1);

                        if(choice == null || choice.t1 < v.t1)
                        {
                            choice = v;
                        }
                    }

                    if (choice != null)
                    {
                        var spell = choice.t2;
                        var spellCaster = w.wizard;

                        w.spellCasted = true;

                        //Check if someone is casted counter magic battle
                        if ((bool) ScriptLibrary.Call("CounterMagicBattle", battle, spell, spellCaster))
                        {
                            w.UseResourcesFor(spell);
                            if (spell.targetType.enumType != ETargetType.TargetWizard &&
                                spell.targetType.enumType != ETargetType.TargetGlobal)
                            {
                                Battle.GetBattle()?.ResistedSpell(choice.t0, true);   
                            }
                            else
                            {
                                Battle.GetBattle()?.ResistedSpell(Vector3i.invalid, true);
                            }

                            BattleHUD.CombatLogAdd(DBUtils.Localization.Get("UI_COMBAT_LOG_SPELL_COUNTERED",true,spell.GetDescriptionInfo().GetLocalizedName()));
                            yield break;
                        }
                        var spellData = new SpellCastData(spellCaster, battle);
                        if (spell.targetType.enumType == ETargetType.TargetWizard)
                        {
                            bool targetEnemy = spell.targetType == (TargetType)TARGET_TYPE.WIZARD_ENEMY;
                            var targetW = targetEnemy ? battle.GetOtherPlayer(w) : w;

                            BattleHUD.CombatLogSpell(spellCaster, spell, targetW);                            
                            Battle.CastBattleSpell(spell, spellData, targetW);
                            BattleHUD.CombatLogSpellAddEffect();
                        }
                        else if (spell.targetType.enumType == ETargetType.TargetGlobal)
                        {
                            BattleHUD.CombatLogSpell(spellCaster, spell, battle);                            
                            Battle.CastBattleSpell(spell, spellData, battle);
                            BattleHUD.CombatLogSpellAddEffect();
                        }
                        else
                        {
                            var targetUnit = battle.GetUnitAt(choice.t0);
                            if (targetUnit != null)
                            {
                                BattleHUD.CombatLogSpell(spellCaster, spell, targetUnit);                                
                                Battle.CastBattleSpell(spell, spellData, targetUnit);
                                BattleHUD.CombatLogSpellAddEffect();
                            }
                            else
                            {
                                BattleHUD.CombatLogSpell(spellCaster, spell, choice.t0);                                
                                Battle.CastBattleSpell(spell, spellData, choice.t0);
                                BattleHUD.CombatLogSpellAddEffect();
                            }
                            targetUnit?.GetOrCreateFormation().UpdateFigureCount();
                        }
                        w.UseResourcesFor(spell);

                        battle.activeTurn.CastEffect(choice.t0, spell, w);

                        if (Settings.GetData().GetBattleCameraFollow() == true) CameraController.CenterAt(choice.t0);
                        yield return new WaitForSeconds(1.5f);

                    }
                }
            }
        }
        static IEnumerator CastSpells(BattleUnit caster, BattlePlayer owner, Battle battle)
        {
            if (caster == null || caster.mana <= 0 || caster.spellCasted) yield break;

            var spells = caster.GetSpells();
            if (spells == null || spells.Count == 0) yield break;


            var ownPower = battle.GetStrategicValue(battle.attacker == owner);
            var otherPower = battle.GetStrategicValue(battle.attacker != owner);

            //current situation, compare both sides:
            //advantage of 5x or more. use maximum of 5% mana per spell if possible (use spells only if mana supplies are very large)
            //advantage of 2x or more. use maximum of 20% mana per spell if possible (use spells only if mana supplies are fair)
            //advantage of 1x or more. use maximum of 50% mana per spell if possible (use spells only if mana supplies are fair)
            //disadvantage use spell only if % of advantage gained is large enough in comparison to share or mana invested.
            //          ie: 0.3 advantage -> 0.5 advantage is 20% increase.  for which use maximum of 40% mana.
            //              0.5 advantage -> 0.9 advantage is 40% increase. for which use maximum of 80% mana.

            //use largest gain spell that fits mana limit profile. 
            //there is an option to do spell value sort based on mana/value proportion, but it does not fit current casting design.

            int maxMana = caster.mana;

            float advantage = otherPower == 0 ? 10f : ownPower / (float)otherPower;
            int manaToUse = caster.mana;
            
            spells = spells.FindAll(o => !string.IsNullOrEmpty(o.Get().battleScript) &&
                                         o.Get().GetBattleCastingCost(caster) <= manaToUse);
            if(owner.wizard != null && caster.isHero && owner.wizard.banishedTurn <= 0)
            {
                spells.AddRange(owner.wizard.GetSpells().FindAll(o => 
                                         !string.IsNullOrEmpty(o.Get().battleScript) &&
                                         o.Get().GetBattleCastingCost(caster) <= manaToUse));
            }
            
            if (spells.Count > 0)
            {
                var ai = DifficultySettingsData.GetSetting("UI_DIFF_AI_SKILL");
                if (ai == null)
                {
                    Debug.LogError("Cannot resolve casting, no AI settings!");
                }
                var options = new List<Multitype<Vector3i, int, Spell>>();
                float castingShare = 0;
                int aiSkill = 0;
                switch (ai.value)
                {
                    case "1":
                        //beginner
                        //consider only 1 spell or 20% of spells whichever is higher                        
                        castingShare = 0.2f;
                        aiSkill = 1;
                        break;
                    case "2":
                        //advanced
                        //consider only 1 spell or 50% of spells whichever is higher
                        castingShare = 0.5f;
                        aiSkill = 2;
                        break;
                    case "3":
                        //skilled
                        //consider only 1 spell or 80% of spells whichever is higher
                        castingShare = 0.8f;
                        aiSkill = 3;
                        break;
                    case "4":
                        //master
                        //consider all spells
                        castingShare = 1f;
                        aiSkill = 4;
                        break;
                    default:
                        Debug.LogWarning("AI setting not implemented!");

                        break;
                }

                if (owner.playerOwner)
                {
                    castingShare = 1f;
                    aiSkill = 4;
                }

                if (spells.Count > 1) spells.RandomSort();
                int maxCastConsiderede = (int)(spells.Count * castingShare);

                //if list would be build it would be cached for all cast considerations that are relevant to the spell.
                HashSet<Vector3i> hexesCloseToUnits = new HashSet<Vector3i>();

                for (int i = 0; i < 1 || i < maxCastConsiderede; i++)
                {
                    var data = CastingSimulation(spells[i].Get(), aiSkill, battle, caster, owner, hexesCloseToUnits);
                    if (data != null) options.Add(data);
                }

                if (options.Count < 1) yield break;

                //                 
                // 
                //                 if (advantage >= 1f)
                //                 {
                //                     //mana limits what was chosen to consider, therefore now just pick best
                //                     var powerExpected = otherPower > 0 ? 1f / otherPower : 0;
                //                     var manaAllowed = w.mana > 0 ? 2f / w.mana : 0;
                // 
                //                     options = options.FindAll(o => o.t1 * powerExpected >= o.t2.battleCost * manaAllowed);
                //                 }

                if (options.Count > 0)
                {
                    Multitype<Vector3i, int, Spell> choice = null;
                    foreach (var v in options)
                    {
                        int cost = v.t2.GetBattleCastingCost(caster);

                        v.t1 = v.t2.GetSpellTacticalValue(cost, v.t1);

                        if (choice == null || choice.t1 < v.t1)
                        {
                            if(v.t1 > 0) choice = v;
                        }
                    }

                    if (choice != null)
                    {
                        var spell = choice.t2;

                        caster.spellCasted = true;
                        caster.Mp = FInt.ZERO;
                        if (BattleHUD.GetSelectedUnit() == caster) BattleHUD.RefreshSelection();


                        //Check if someone is casted counter magic battle
                        if ((bool)ScriptLibrary.Call("CounterMagicBattle", battle, spell, caster))
                        {
                            caster.mana -= spell.GetBattleCastingCost(caster);
                            if (spell.targetType.enumType != ETargetType.TargetWizard &&
                                spell.targetType.enumType != ETargetType.TargetGlobal)
                            {
                                Battle.GetBattle()?.ResistedSpell(choice.t0, true);
                            }
                            else
                            {
                                Battle.GetBattle()?.ResistedSpell(Vector3i.invalid, true);
                            }

                            BattleHUD.CombatLogAdd(DBUtils.Localization.Get("UI_COMBAT_LOG_SPELL_COUNTERED", true, spell.GetDescriptionInfo().GetLocalizedName()));
                            yield break;
                        }
                        var spellData = new SpellCastData(caster, battle);
                        if (spell.targetType.enumType == ETargetType.TargetWizard)
                        {
                            bool targetEnemy = spell.targetType == (TargetType)TARGET_TYPE.WIZARD_ENEMY;
                            var targetW = targetEnemy ? battle.GetOtherPlayer(owner) : owner;

                            BattleHUD.CombatLogSpell(caster, spell, targetW);
                            Battle.CastBattleSpell(spell, spellData, targetW);
                            BattleHUD.CombatLogSpellAddEffect();
                        }
                        else if (spell.targetType.enumType == ETargetType.TargetGlobal)
                        {
                            BattleHUD.CombatLogSpell(caster, spell, battle);
                            Battle.CastBattleSpell(spell, spellData, battle);
                            BattleHUD.CombatLogSpellAddEffect();
                        }
                        else
                        {
                            var targetUnit = battle.GetUnitAt(choice.t0);
                            if (targetUnit != null)
                            {
                                BattleHUD.CombatLogSpell(caster, spell, targetUnit);
                                Battle.CastBattleSpell(spell, spellData, targetUnit);
                                BattleHUD.CombatLogSpellAddEffect();
                            }
                            else
                            {
                                BattleHUD.CombatLogSpell(caster, spell, choice.t0);
                                Battle.CastBattleSpell(spell, spellData, choice.t0);
                                BattleHUD.CombatLogSpellAddEffect();
                            }
                            targetUnit?.GetOrCreateFormation().UpdateFigureCount();
                        }
                        caster.mana -= spell.GetBattleCastingCost(caster);

                        battle.activeTurn.CastEffect(choice.t0, spell, caster);

                        if (Settings.GetData().GetBattleCameraFollow() == true) CameraController.CenterAt(choice.t0);
                        yield return new WaitForSeconds(1.5f);
                    }
                }
            }
        }
        static Multitype<Vector3i, int, Spell> CastingSimulation(Spell spell, int aiSkill, Battle b, ISpellCaster caster, BattlePlayer w, HashSet<Vector3i> hexesCloseToUnits)
        {
            var tType = spell.targetType.enumType;
            if (tType == ETargetType.TargetUnit)
            {
                List<BattleUnit> targets = new List<BattleUnit>();
                foreach (var v in b.buToSource)
                {
                    if (v.Key.IsAlive() && 
                        (v.Key.GetWizardOwnerID() == caster.GetWizardOwnerID() ||  
                        v.Key.currentlyVisible) &&
                        (bool)ScriptLibrary.Call(spell.targetingScript, new SpellCastData( caster, b), v.Key, spell))
                    {
                        targets.Add(v.Key);
                    }
                }
                if (targets.Count < 1) return null;
                int start = UnityEngine.Random.Range(0, targets.Count);

                BattleUnit target = null;
                int gain = 0;
                for (int i = 0; i < targets.Count; i++)
                {
                    int index = (i + start) % targets.Count;

                    var t = targets[index];
                    int offset = 0;

                    if (!string.IsNullOrEmpty(spell.aiBattleEvaluationScript))
                    {
                        offset = (int)ScriptLibrary.Call(spell.aiBattleEvaluationScript, new SpellCastData(caster, b), t, spell);
                    }
                    else
                    {
                        var prev = t.GetBattleUnitValue();
                        var post = t.GetStrategicValueForSpell(caster, b, spell);

                        offset = post - prev;
                        if (t.ownerID != w.GetID())
                        {
                            offset = -offset;
                        }
                    }

                    if (gain < offset)
                    {
                        gain = offset;
                        target = targets[index];
                    }
                }

#if (UNITY_EDITOR)
                if(spell != null && target != null && target.dbSource != null &&
                    target.dbSource.dbName != null)
                {
                    Debug.Log(spell.dbName + " give SpellAI value " + gain +
                        " on unit " + target.dbSource.dbName.ToString());
                }
#endif
                if (gain <= 0) return null;

                return new Multitype<Vector3i, int, Spell>(target.GetPosition(), gain, spell);
            }
            else if (tType == ETargetType.TargetHex)
            {
                //make list of locations around allied and enemy units if one is not prepared. 
                if (hexesCloseToUnits.Count == 0)
                {
                    foreach (var v in b.buToSource)
                    {
                        if (!v.Key.IsAlive()) continue;
                        var area = HexNeighbors.GetRange(v.Key.GetPosition(), 1, 1);
                        foreach(var k in area)
                        {
                            if(!hexesCloseToUnits.Contains(k) && b.plane.area.IsInside(k))
                            {
                                hexesCloseToUnits.Add(k);
                            }
                        }
                    }
                }

                Multitype<Vector3i, int, Spell> target = null;
                foreach (var v in hexesCloseToUnits)
                {
                    bool canCast = true;
                    if (!string.IsNullOrEmpty(spell.targetingScript))
                    {
                        canCast = (bool)ScriptLibrary.Call(spell.targetingScript, new SpellCastData( caster, b), v, spell);
                    }

                    if (canCast)
                    {
                        if (string.IsNullOrEmpty(spell.aiBattleEvaluationScript))
                        {
                            Debug.LogWarning("Spell " + spell.dbName + " does not have ai evaluation script, required for this category of spells");
                            return null;
                        }

                        int value = (int)ScriptLibrary.Call(spell.aiBattleEvaluationScript, new SpellCastData(caster, b), v, spell);
                        if (value <= 0) continue;
                        if (target == null || target.t1 < value)
                        {
                            target = new Multitype<Vector3i, int, Spell>(v, value, spell);
                        }
                    }
                }

                if (target!= null && target.t1 > 0)
                {
                    return target;
                }
            }
            else if (tType == ETargetType.TargetGlobal ||
                     tType == ETargetType.WorldHexBattleGlobal)
            {
                Multitype<Vector3i, int, Spell> target = null;
                bool canCast = true;
                if (!string.IsNullOrEmpty(spell.targetingScript))
                {
                    canCast = (bool)ScriptLibrary.Call(spell.targetingScript, new SpellCastData(caster, b), b, spell);
                }

                if (canCast)
                {
                    if (string.IsNullOrEmpty(spell.aiBattleEvaluationScript))
                    {
                        Debug.LogWarning("Spell " + spell.dbName + " does not have ai evaluation script, required for this category of spells");
                        return null;
                    }

                    int value = (int)ScriptLibrary.Call(spell.aiBattleEvaluationScript, new SpellCastData(caster, b), b, spell);
                    if (value <= 0) return null;
                    
                    return new Multitype<Vector3i, int, Spell>(Vector3i.invalid, value, spell);                    
                }
            }
            else if (tType == ETargetType.TargetWizard)
            {
                bool targetEnemy = spell.targetType == (TargetType)TARGET_TYPE.WIZARD_ENEMY;
                var targetW = targetEnemy ? b.GetOtherPlayer(w) : w;

                Multitype<Vector3i, int, Spell> target = null;
                bool canCast = true;
                if (!string.IsNullOrEmpty(spell.targetingScript))
                {
                    canCast = (bool)ScriptLibrary.Call(spell.targetingScript, new SpellCastData(caster, b), targetW, spell);
                }

                if (canCast)
                {
                    if (string.IsNullOrEmpty(spell.aiBattleEvaluationScript))
                    {
                        Debug.LogWarning("Spell " + spell.dbName + " does not have ai evaluation script, required for this category of spells");
                        return null;
                    }

                    int value = (int)ScriptLibrary.Call(spell.aiBattleEvaluationScript, new SpellCastData(caster, b), targetW, spell);
                    if (value <= 0) return null;
                    
                    return new Multitype<Vector3i, int, Spell>(Vector3i.invalid, value, spell);                    
                }
            }

            return null;
        }
        static void DetectInvalidLocation(Vector3i pos, SearcherDataV2 sd, string identifier)
        {
            if (sd.IsUnitAt(pos))
            {
                Debug.LogWarning("("+identifier + ") Tracking potential issue at " + pos);
            }
        }
        static void CutByEarlierObstacle(List<Vector3i> poss, SearcherDataV2 sd, Battle battle, BattleUnit walker)
        {
            int goodIndex = poss.Count - 1;

            bool stopByGate = walker.attackingSide && sd.walls != null && sd.walls[sd.gateIndex];            

            for (int i = goodIndex; i > 0; i--)
            {
                if (sd.IsUnitAt(poss[i]))
                {
                    var obstacle = battle.GetUnitAt(poss[i]);
                    //if first free expected location is occupied by an enemy or
                    //occupied by anyone when it was not location that we could pass through
                    if(walker.attackingSide != obstacle.attackingSide || goodIndex == i)
                    {
                        goodIndex = i-1;
                        continue;
                    }                    
                }
                if (stopByGate)
                {
                    //if gate for an attacker is detected on the path, cut path to step before
                    if (sd.GetIndex(poss[i]) == sd.gateIndex)
                    {
                        goodIndex = i-1;
                        continue;
                    }
                }
            }            

            if (goodIndex < poss.Count - 1)
            {
                poss.RemoveRange(goodIndex+1, poss.Count - goodIndex-1);
            }
        }
        private static void UpdateAIUnitInfo(BattleUnit u)
        {
            if (u.attackingSide) 
            {
                BattleHUD.Get().attackerInfo.UpdateUnitInfoDisplay(u, false);
                BattleHUD.Get().attackerInfo.HighlightSelectedUnit(u);

            }
            else 
            {
                BattleHUD.Get().defenderInfo.UpdateUnitInfoDisplay(u, false);
                BattleHUD.Get().defenderInfo.HighlightSelectedUnit(u);
            }
        }

        #endregion
    }
}
#endif