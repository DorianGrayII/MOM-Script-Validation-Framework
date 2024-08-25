/**********************************
 *
 * Author:  Dorian Gray
 * Date:    May 18, 2024
 * Version: 1.0.0
 *
 **********************************/

#if !USE_DEBUG_SCRIPT || !UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using DBDef;
using DBEnum;
using MHUtils;
using MOM;
using UnityEngine;
using WorldCode;

namespace MOMScripts_SAI
{
    public class BattleAIScripts : ScriptBase
    {
        #region ver 2
        public static IEnumerator AITurnV02(Battle battle, bool bAttacker)
        {
            BattlePlayer battlePlayer = bAttacker ? battle.attacker : battle.defender;
            //NOTE: v1 spell casting in use!
            if (GameManager.Get() == null ||
                GameManager.Get().useManaInAutoresolves ||
                battlePlayer.GetWizardOwner() != GameManager.GetHumanWizard())
            {
                yield return CastSpells(battlePlayer, battle);
            }

            List<BattleUnit> allyBattleUnitList;
            List<BattleUnit> enemyBattleUnitList;

            if (bAttacker)
            {
                allyBattleUnitList = battle.attackerUnits.FindAll(o => o.IsAlive());
                enemyBattleUnitList = battle.defenderUnits.FindAll(o => o.IsAlive());
            }
            else
            {
                allyBattleUnitList = battle.defenderUnits.FindAll(o => o.IsAlive());
                enemyBattleUnitList = battle.attackerUnits.FindAll(o => o.IsAlive());
            }

            yield return AIFramesV02(battlePlayer, allyBattleUnitList, enemyBattleUnitList, battle);

            if (battlePlayer != battle.GetHumanPlayer() ||
               battle.GetHumanPlayer().autoPlayByAI)
            {
                battle.activeTurn.AITurnEnd();
            }
        }

        private static IEnumerator AIFramesV02(BattlePlayer battlePlayer,
                                    List<BattleUnit> allyBattleUnitList,
                                    List<BattleUnit> enemyBattleUnitList,
                                    Battle battle)
        {
            while (battle.activeTurn == null)
            {
                yield return null;
            }

            foreach (BattleUnit buAlly in allyBattleUnitList)
            {
                while (buAlly.Mp > 0 && buAlly.IsAlive())
                {
                    yield return battle.WaitForAttention();
                    if (battlePlayer == battle.GetHumanPlayer() &&
                        !battle.GetHumanPlayer().autoPlayByAI)
                    {
                        yield break;
                    }

                    FInt mpAlly = buAlly.Mp;
                    yield return UnitTurnV02(battlePlayer, buAlly, allyBattleUnitList, enemyBattleUnitList, battle, false, false);
                    //if unit did not use any MP, it might be grounded with no actions to carry.
                    //to avoid soft locks, skip this unit.
                    if (mpAlly == buAlly.Mp)
                    {
                        break;
                    }
                }
            }
        }

        private static IEnumerator UnitTurnV02(BattlePlayer battlePlayer,
                                       BattleUnit buAlly,
                                       List<BattleUnit> allyBattleUnitList,
                                       List<BattleUnit> enemyBattleUnitList,
                                       Battle battle,
                                       bool bSeekThroughWalls,
                                       bool bForceSeekThroughGates)
        {
            // A) no MP: end unit turn
            // B) pick preferred target 
            // simulate or load from cache to know how much gain you can gain on specific attack
            // C) pathfind to attack position. 
            //Failure: find next target (B)
            //Success: attack if in range
            // D) goto (A)

            foreach (BattleUnit abu in allyBattleUnitList)
            {
                if (abu.IsAlive() && !battle.plane.GetSearcherData().IsUnitAt(abu.GetPosition()))
                {
                    Debug.LogError("AI unit missing marker on searcher data!");
                }
            }
            foreach (BattleUnit ebu in enemyBattleUnitList)
            {
                if (ebu.IsAlive() && !battle.plane.GetSearcherData().IsUnitAt(ebu.GetPosition()))
                {
                    Debug.LogError("AI's enemy unit missing marker on searcher data!");
                }
            }

            //consider spell casting if possible
            yield return CastSpells(buAlly, battlePlayer, battle);
            if (buAlly.spellCasted)
            {
                yield break;
            }

            List<AIAttackOption> aiAttackOptionList = battle.aIBattleTactics.GetOptions(buAlly, enemyBattleUnitList, battle);

            aiAttackOptionList.Sort((a, b) =>
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
            SearcherDataV2 searchData = battle.plane.GetSearcherData();
            RequestDataV2 request = RequestDataV2.CreateRequest(battle.plane, buAlly.GetPosition(), buAlly.Mp, buAlly, true);
            request.ignoreWalls = bSeekThroughWalls;
            request.stoppedByGate = bForceSeekThroughGates ? false : request.stoppedByGate;

            PathfinderV2.FindArea(request);
            List<Vector3i> area = request.GetArea();
            List<Vector3i> areaNoObstacle = request.GetNoObstackleArea();

            //for teleporting units add 1 hex area around enemies
            if (buAlly.teleporting)
            {
                foreach (BattleUnit buEnemy in enemyBattleUnitList)
                {
                    Vector3i vecEnemyPosn = buEnemy.GetPosition();
                    bool bPossible = false;
                    foreach (Vector3i vecNeighbor in HexNeighbors.neighbours)
                    {
                        Vector3i kPos = vecEnemyPosn + vecNeighbor;
                        if (battle.IsLocationFreeForUnit(kPos, !buAlly.attackingSide))
                        {
                            //add to area only if neighbor is not in area and is not blocked by obstacle
                            //ie: it is possible to teleport next to that location
                            if (!area.Contains(kPos))
                            {
                                area.Add(kPos);
                            }

                            bPossible = true;
                        }
                    }
                    if (bPossible)
                    {
                        //add enemy position to area if at least 1 neighboring location was possible
                        if (!area.Contains(vecEnemyPosn))
                        {
                            area.Add(vecEnemyPosn);
                        }
                    }
                }
            }

            if (allyBattleUnitList != null)
            {
                //exclude ally locations from area
                foreach (BattleUnit bu in allyBattleUnitList)
                {
                    if (bu == buAlly)
                    {
                        continue;
                    }

                    Vector3i pos = bu.GetPosition();
                    if (area.Contains(pos))
                    {
                        area.Remove(pos);
                    }
                }
            }

            bool bIsAllyBoulderRange = buAlly.GetAttFinal((Tag)TAG.BOULDER_RANGE) > 0;
            bool bIsAllyMagicRange   = buAlly.GetAttFinal((Tag)TAG.MAGIC_RANGE) > 0;

            bool bIsAllyLongRange = buAlly.GetAttFinal((Tag)TAG.LONG_RANGE) > 0 ||
                                    buAlly.GetAttFinal((Tag)TAG.MAGIC_RANGE) > 0;

            //consider all options in order from most valuable to the least valuable
            for (int i = 0; i < aiAttackOptionList.Count; i++)
            {
                AIAttackOption attOption = aiAttackOptionList[i];
                if (!attOption.attackValid)
                {
                    continue;
                }

                List<BattleUnit> buTargetList = battle.aIBattleTactics.FilterByOption(enemyBattleUnitList, attOption);
                if (buTargetList == null || buTargetList.Count < 1)
                {
                    continue;
                }

                buTargetList.SortInPlace(delegate (BattleUnit a, BattleUnit b)
                {
                    int distA = HexCoordinates.HexDistance(a.GetPosition(), buAlly.GetPosition());
                    int distB = HexCoordinates.HexDistance(b.GetPosition(), buAlly.GetPosition());
                    return distA.CompareTo(distB);
                });

                if (attOption.melee)
                {
                    if (area != null)
                    {
                        BattleUnit buPreferredTarget = null;
                        foreach (BattleUnit buTarget in buTargetList)
                        {
                            if (area.Contains(buTarget.GetPosition()))
                            {
                                buPreferredTarget = buTarget;
                                break;
                            }
                        }

                        if (buPreferredTarget != null)
                        {
                            //execute moveTo and attack
                            List<Vector3i> path = null;
                            if (buAlly.teleporting)
                            {
                                path = new List<Vector3i> { buAlly.GetPosition() };

                                Vector3i vecPreferredTarget = buPreferredTarget.GetPosition();
                                foreach (Vector3i vecNeighbor in HexNeighbors.neighbours)
                                {
                                    Vector3i vecPosn = vecPreferredTarget + vecNeighbor;
                                    if (battle.IsLocationFreeForUnit(vecPosn, !buAlly.attackingSide))
                                    {
                                        path.Add(vecPosn);
                                        break;
                                    }
                                }
                                path.Add(vecPreferredTarget);
                            }
                            else
                            {
                                path = request.GetPathTo(buPreferredTarget.GetPosition());
                            }

                            if (path != null && path.Count > 2)
                            {
                                yield return battle.WaitForAttention();

                                path.RemoveLast();
                                Vector3i destination = path[path.Count - 1];
                                CutByEarlierObstacle(path, searchData, battle, buAlly);

                                path = buAlly.CutPathToMP(battle, buAlly.Mp, path, true, false);
                                if (path.Count > 1)
                                {
                                    DetectInvalidLocation(path[path.Count - 1], searchData, "A " + bSeekThroughWalls);
                                    battle.GainAttention(buAlly.GetOrCreateFormation());
                                    buAlly.MoveViaPath(path);

                                    UpdateAIUnitInfo(buAlly);
                                }

                                yield return battle.WaitForAttention();
                                //if movement was stopped abruptly, reconsider actions
                                if (path.Count < 2 || path[path.Count - 1] != destination)
                                {
                                    yield break;
                                }

                                battle.plane.ClearSearcherData();
                            }

                            yield return battle.WaitForAttention();
                            if (buAlly.Mp == 0)
                            {
                                yield break;
                            }

                            yield return battle.AttackUnit(buAlly, buPreferredTarget);

                            yield return battle.WaitForAttention();
                            if (buPreferredTarget.IsAlive() && buAlly.IsAlive() && buAlly.Mp > 0)
                            {
                                yield return battle.AttackUnit(buAlly, buPreferredTarget);
                            }

                            yield break;
                        }
                    }
                }
                else //Ranged
                {
                    BattleUnit buPreferredTarget = null;
                    Vector3i vecPotentialTargetPosn = Vector3i.zero;
                    for (int k = 0; k < buTargetList.Count; k++)
                    {
                        BattleUnit buPotentialTarget = buTargetList[k];
                        vecPotentialTargetPosn = buPotentialTarget.GetPosition();
                        if (buPreferredTarget != null &&
                            HexCoordinates.HexDistance(buAlly.GetPosition(), buPreferredTarget.GetPosition()) <=
                            HexCoordinates.HexDistance(buAlly.GetPosition(), vecPotentialTargetPosn))
                        {
                            //we have target and we it is closer than current target, no need to check further
                            continue;
                        }

                        if (battle.darknessWall && battle.AttactThroughWall(buAlly.GetPosition(), vecPotentialTargetPosn))
                        {
                            if (buAlly.GetAttFinal(TAG.ILLUSIONS_IMMUNITY) <= 0)
                            {
                                //unit is not immune to illusions, it cannot attack through darkness wall
                                continue;
                            }
                        }

                        if (buPotentialTarget.GetAttFinal(TAG.MISSILE_IMMUNITY) > 0)
                        {
                            if (!bIsAllyMagicRange && !bIsAllyBoulderRange)
                            {
                                //target is immune to missiles, it cannot be attacked by normal ranged units
                                continue;
                            }
                        }

                        buPreferredTarget = buPotentialTarget;
                    }

                    //if none of the target fulfill the requirements not being hidden by wall, continue
                    if (buPreferredTarget == null)
                    {
                        continue;
                    }

                    vecPotentialTargetPosn = buPreferredTarget.GetPosition();
                    int iDistToTarget = HexCoordinates.HexDistance(buAlly.GetPosition(), vecPotentialTargetPosn);
                    FInt fIntMpToShoot = buAlly.Mp - 1;

                    List<Multitype<Vector3i, int>> shootLocationList = null;

                    if (bIsAllyLongRange)
                    {
                        //consider optimal shooting range
                        shootLocationList = ReachShootOptions(areaNoObstacle, vecPotentialTargetPosn, request, enemyBattleUnitList, 2, 30, false);

                        //consider optimal shooting range with enemy in contact
                        if (shootLocationList == null)
                        {
                            shootLocationList = ReachShootOptions(areaNoObstacle, vecPotentialTargetPosn, request, enemyBattleUnitList, 2, 30, true);
                        }

                        if (shootLocationList == null)
                        {
                            continue;
                        }

                        //1. pick location closest to distance 8 to the enemy. Even if not reachable it may be better positioning for next turn shoot                    
                        //2. second criteria is shortest number of hexes passed to reach destination

                        shootLocationList.SortInPlace(delegate (Multitype<Vector3i, int> a, Multitype<Vector3i, int> b)
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
                                distA = Mathf.Abs(HexCoordinates.HexDistance(buAlly.GetPosition(), a.t0));
                                distB = Mathf.Abs(HexCoordinates.HexDistance(buAlly.GetPosition(), b.t0));
                                ret = distA.CompareTo(distB);
                            }

                            return ret;
                        });
                    }
                    else
                    {
                        int iMaxRange = iDistToTarget + buAlly.Mp.ToInt() - 1;
                        if (iDistToTarget - fIntMpToShoot > iMaxRange)
                        {
                            //even on most favorable terrain, this attack cannot reach favorable range.
                            continue;
                        }

                        //consider optimal shooting range
                        shootLocationList = ReachShootOptions(areaNoObstacle, vecPotentialTargetPosn, request, enemyBattleUnitList, 2, 3, false);

                        //consider suboptimal shooting range
                        if (shootLocationList == null)
                        {
                            shootLocationList = ReachShootOptions(areaNoObstacle, vecPotentialTargetPosn, request, enemyBattleUnitList, 4, iMaxRange, false);
                        }

                        //consider optimal shooting range with enemy in contact
                        if (shootLocationList == null)
                        {
                            shootLocationList = ReachShootOptions(areaNoObstacle, vecPotentialTargetPosn, request, enemyBattleUnitList, 2, 3, true);
                        }

                        //consider suboptimal shooting range with enemy in contact
                        if (shootLocationList == null)
                        {
                            shootLocationList = ReachShootOptions(areaNoObstacle, vecPotentialTargetPosn, request, enemyBattleUnitList, 4, iMaxRange, true);
                        }

                        if (shootLocationList == null)
                        {
                            continue;
                        }

                        shootLocationList.SortInPlace(delegate (Multitype<Vector3i, int> a, Multitype<Vector3i, int> b)
                        {
                            //as close to distance 3 as possible
                            int targetDist = 3;
                            int distA = Mathf.Abs(HexCoordinates.HexDistance(a.t0, vecPotentialTargetPosn) - targetDist);
                            int distB = Mathf.Abs(HexCoordinates.HexDistance(b.t0, vecPotentialTargetPosn) - targetDist);

                            int ret = distA.CompareTo(distB);

                            //if equal, then compare distance to closest enemy, and pick further away place
                            if (ret == 0)
                            {
                                ret = -a.t1.CompareTo(b.t1);
                            }

                            //if still equal then pick place that is closest to current position.
                            //This way unit wont move if this does not provide strategic advantage 
                            if (ret == 0)
                            {
                                distA = Mathf.Abs(HexCoordinates.HexDistance(buAlly.GetPosition(), a.t0));
                                distB = Mathf.Abs(HexCoordinates.HexDistance(buAlly.GetPosition(), b.t0));
                                ret = distA.CompareTo(distB);
                            }

                            return ret;
                        });
                    }

                    Vector3i shootLocation = shootLocationList[0].t0;

                    //execute moveTo and attack
                    List<Vector3i> path = request.GetPathTo(shootLocation);
                    if (path != null && path.Count >= 2)
                    {
                        yield return battle.WaitForAttention();

                        Vector3i destination = path[path.Count - 1];

                        CutByEarlierObstacle(path, searchData, battle, buAlly);
                        path = buAlly.CutPathToMP(battle, buAlly.Mp, path, true, false);
                        if (path.Count > 1)
                        {
                            DetectInvalidLocation(path[path.Count - 1], searchData, "B " + bSeekThroughWalls);
                            battle.GainAttention(buAlly.GetOrCreateFormation());
                            buAlly.MoveViaPath(path);

                            UpdateAIUnitInfo(buAlly);
                        }

                        yield return battle.WaitForAttention();
                        //if movement was stopped abruptly, reconsider actions
                        if (path.Count < 2 || path[path.Count - 1] != destination)
                        {
                            yield break;
                        }

                        battle.plane.ClearSearcherData();
                    }

                    yield return battle.WaitForAttention();
                    if (buAlly.Mp == 0)
                    {
                        yield break;
                    }

                    yield return battle.AttackUnit(buAlly, buPreferredTarget);
                    yield break;
                }
            }
            if (buAlly.Mp == 0)
            {
                yield break;
            }

            //no attack was sensible, just move toward best target
            //maybe it should be closest among all targets? 
            //but this is easy to exploit by manipulation what is closer what is not.
            //therefore sorting happens only within chosen target group

            for (int i = 0; i < aiAttackOptionList.Count; i++)
            {
                AIAttackOption option = aiAttackOptionList[i];
                if (!option.attackValid)
                {
                    continue;
                }

                List<BattleUnit> targets = battle.aIBattleTactics.FilterByOption(enemyBattleUnitList, option);
                if (targets == null || targets.Count < 1)
                {
                    continue;
                }

                targets.SortInPlace(delegate (BattleUnit a, BattleUnit b)
                {
                    int distA = HexCoordinates.HexDistance(a.GetPosition(), buAlly.GetPosition());
                    int distB = HexCoordinates.HexDistance(b.GetPosition(), buAlly.GetPosition());
                    return distA.CompareTo(distB);
                });

                BattleUnit preferredTarget = null;
                List<Vector3i> path = null;
                for (int k = 0; k < targets.Count; k++)
                {
                    //target first enemy in the list,
                    //if we cannot pathfind to it, try next one
                    preferredTarget = targets[k];

                    RequestDataV2 rd2 = RequestDataV2.CreateRequest(battle.plane, buAlly.GetPosition(), preferredTarget.GetPosition(), buAlly);
                    rd2.ignoreWalls = bSeekThroughWalls;
                    rd2.stoppedByGate = bForceSeekThroughGates ? false : rd2.stoppedByGate;
                    PathfinderV2.FindPath(rd2);

                    path = rd2.GetPath();
                    if (path != null && path.Count > 2)
                    {
                        break;
                    }
                }

                if (path != null && path.Count > 2)
                {
                    yield return battle.WaitForAttention();

                    path.RemoveLast();
                    CutByEarlierObstacle(path, searchData, battle, buAlly);

                    path = buAlly.CutPathToMP(battle, buAlly.Mp, path, true, false);
                    if (path.Count > 1)
                    {
                        DetectInvalidLocation(path[path.Count - 1], searchData, "C " + bSeekThroughWalls);
                    }
                    else
                    {
                        continue;
                    }
                    battle.GainAttention(buAlly.GetOrCreateFormation());
                    buAlly.MoveViaPath(path);

                    UpdateAIUnitInfo(buAlly);

                    battle.plane.ClearSearcherData();
                    yield break;
                }
            }

            //unit did not choose to act, 
            //if map contains walls and unit is not flying, it may seek the action again with the walls "turned off"
            //if this was already tried or criteria are not fulfilled there is no point in attempting the same for the second time

            if (!bSeekThroughWalls && battle.battleWalls.Count > 0 && !request.nonCorporeal && !bForceSeekThroughGates)
            {
                yield return UnitTurnV02(battlePlayer, buAlly, allyBattleUnitList, enemyBattleUnitList, battle, false, true);
            }

            if (buAlly.attackingSide && buAlly.Mp > 0 && battle.battleWalls.Count > 0)
            {
                //attacker may check if it is standing next to the wall, as it did not use all MP, it may attack the wall.
                bool bWallCrusher = buAlly.GetAttFinal((Tag)TAG.WALL_CRUSHER) > FInt.ZERO;
                BattleWall wallTarget = null;
                int iDistance = int.MaxValue;

                foreach (BattleWall battleWall in battle.battleWalls)
                {
                    if (!battleWall.standing)
                    {
                        continue;
                    }

                    int iDistToWall = HexCoordinates.HexDistance(battleWall.position, buAlly.battlePosition);
                    if (iDistance > iDistToWall &&
                        ((bWallCrusher && (iDistToWall < 2 || buAlly.GetCurentFigure().rangedAmmo > 0)) || (battleWall.gate && iDistToWall < 2)))
                    {
                        wallTarget = battleWall;
                    }
                }

                if (wallTarget != null)
                {
                    while (wallTarget != null && buAlly.Mp > 0)
                    {
                        yield return battle.AttackWall(buAlly, wallTarget);
                    }
                }
            }

            buAlly.Mp = FInt.ZERO;
        }

        private static List<Multitype<Vector3i, int>> ReachShootOptions(List<Vector3i> area,
                                          Vector3i target,
                                          RequestDataV2 request,
                                          List<BattleUnit> enemyBattleUnitList,
                                          int minRange, int maxRange, bool bAllowOtherContact)
        {
            List<Multitype<Vector3i, int>> vecList = null;
            foreach (Vector3i vecPosn in area)
            {
                int dist = HexCoordinates.HexDistance(target, vecPosn);
                if (dist >= minRange && dist <= maxRange)
                {
                    //does unit have chance to shoot from that location? Only if not all MP are used out
                    if (request.GetCostTo(vecPosn) >= request.mpRange)
                    {
                        continue;
                    }

                    bool bValid = true;
                    int iMinDist = int.MaxValue;

                    foreach (BattleUnit buEnemy in enemyBattleUnitList)
                    {
                        if (buEnemy.GetPosition() == vecPosn)
                        {
                            continue;
                        }

                        int iEnemyDistToPosn = HexCoordinates.HexDistance(buEnemy.GetPosition(), vecPosn);
                        if (!bAllowOtherContact)
                        {
                            if (iEnemyDistToPosn < 2)
                            {
                                bValid = false;
                                break;
                            }
                        }
                        iMinDist = Mathf.Min(iMinDist, iEnemyDistToPosn);
                    }
                    if (bValid)
                    {
                        if (vecList == null)
                        {
                            vecList = new List<Multitype<Vector3i, int>>();
                        }

                        vecList.Add(new Multitype<Vector3i, int>(vecPosn, iMinDist));
                    }
                }
            }

            return vecList;
        }

        private static IEnumerator CastSpells(BattlePlayer battlePlayer, Battle battle)
        {
            if (battlePlayer == null || battlePlayer.wizard == null || battlePlayer.spellCasted)
            {
                yield break;
            }

            if (battlePlayer.castingBlock)
            {
                yield break;
            }

            List<DBReference<Spell>> refSpellList = battlePlayer.wizard.GetSpells();
            if (refSpellList == null || battlePlayer.wizard.wizardTower?.Get() == null)
            {
                yield break;
            }

            int iAttStrategicValue = battle.GetStrategicValue(battle.attacker == battlePlayer);
            int iDefStrategicValue = battle.GetStrategicValue(battle.attacker != battlePlayer);

            //current situation, compare both sides:
            //advantage of 5x or more. use maximum of 5% mana per spell if possible (use spells only if mana supplies are very large)
            //advantage of 2x or more. use maximum of 20% mana per spell if possible (use spells only if mana supplies are fair)
            //advantage of 1x or more. use maximum of 50% mana per spell if possible (use spells only if mana supplies are fair)
            //disadvantage use spell only if % of advantage gained is large enough in comparison to share or mana invested.
            //          ie: 0.3 advantage -> 0.5 advantage is 20% increase.  for which use maximum of 40% mana.
            //              0.5 advantage -> 0.9 advantage is 40% increase. for which use maximum of 80% mana.

            //use largest gain spell that fits mana limit profile. 
            //there is an option to do spell value sort based on mana/value proportion, but it does not fit current casting design.

            int maxMana = battlePlayer.mana;

            float fAdvantage = iDefStrategicValue == 0 ? 10f : iAttStrategicValue / (float)iDefStrategicValue;
            int manaToUse = 0;
            if (fAdvantage >= 5f)
            {
                manaToUse = (int)Mathf.Min(maxMana, battlePlayer.mana * 0.05f);
            }
            else if (fAdvantage >= 2f)
            {
                manaToUse = (int)Mathf.Min(maxMana, battlePlayer.mana * 0.2f);
            }
            else if (fAdvantage > 1f)
            {
                manaToUse = (int)Mathf.Min(maxMana, battlePlayer.mana * 0.5f);
            }
            else
            {
                manaToUse = maxMana;
            }

            refSpellList = refSpellList.FindAll(o => !string.IsNullOrEmpty(o.Get().battleScript) &&
                                         o.Get().GetBattleCastingCostByDistance(battlePlayer.GetWizardOwner()) <= manaToUse &&
                                         o.Get().GetBattleCastingCost(battlePlayer.GetWizardOwner()) <= battlePlayer.castingSkill);

            if (refSpellList.Count > 0)
            {
                DifficultyOption aiDifficulty = DifficultySettingsData.GetSetting("UI_DIFF_AI_SKILL");
                if (aiDifficulty == null)
                {
                    Debug.LogError("Cannot resolve casting, no AI settings!");
                    yield break;
                }
                List<Multitype<Vector3i, int, Spell>> optionList = new List<Multitype<Vector3i, int, Spell>>();
                float fCastingShare = 0;
                int iAiSkill = 0;
                switch (aiDifficulty.value)
                {
                    case "1":
                        //beginner
                        //consider only 1 spell or 20% of spells whichever is higher                        
                        fCastingShare = 0.2f;
                        iAiSkill = 1;
                        break;
                    case "2":
                        //advanced
                        //consider only 1 spell or 50% of spells whichever is higher
                        fCastingShare = 0.5f;
                        iAiSkill = 2;
                        break;
                    case "3":
                        //skilled
                        //consider only 1 spell or 80% of spells whichever is higher
                        fCastingShare = 0.8f;
                        iAiSkill = 3;
                        break;
                    case "4":
                        //master
                        //consider all spells
                        fCastingShare = 1f;
                        iAiSkill = 4;
                        break;
                    default:
                        Debug.LogWarning("AI setting not implemented!");

                        break;
                }

                if (battlePlayer.playerOwner)
                {
                    fCastingShare = 1f;
                    iAiSkill = 4;
                }

                if (refSpellList.Count > 1)
                {
                    refSpellList.RandomSort();
                }

                int iMaxNumSpells = Mathf.Max((int)(refSpellList.Count * fCastingShare), 1);

                //if list would be build it would be cached for all cast considerations that are relevant to the spell.
                HashSet<Vector3i> hexesCloseToUnits = new HashSet<Vector3i>();

                for (int i = 0; i < iMaxNumSpells; i++)
                {
                    Multitype<Vector3i, int, Spell> data = CastingSimulation(refSpellList[i].Get(), iAiSkill, battle, battlePlayer.wizard, battlePlayer, hexesCloseToUnits);
                    if (data != null)
                    {
                        optionList.Add(data);
                    }
                }

                if (optionList.Count < 1)
                {
                    yield break;
                }

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

                if (optionList.Count > 0)
                {
                    Multitype<Vector3i, int, Spell> choice = null;
                    foreach (Multitype<Vector3i, int, Spell> mtOption in optionList)
                    {
                        int iBattleCastingCostByDistance = mtOption.t2.GetBattleCastingCostByDistance(battlePlayer.GetWizardOwner());

                        mtOption.t1 = mtOption.t2.GetSpellTacticalValue(iBattleCastingCostByDistance, mtOption.t1);

                        if (choice == null || choice.t1 < mtOption.t1)
                        {
                            choice = mtOption;
                        }
                    }

                    if (choice != null)
                    {
                        Spell spell = choice.t2;
                        PlayerWizard spellCaster = battlePlayer.wizard;

                        battlePlayer.spellCasted = true;

                        //Check if someone is has casted counter magic battle
                        //and, if true, is the spell is countered?
                        if ((bool)ScriptLibrary.Call("CounterMagicBattle", battle, spell, spellCaster))
                        {
                            battlePlayer.UseResourcesFor(spell);
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
                        SpellCastData spellData = new SpellCastData(spellCaster, battle);
                        if (spell.targetType.enumType == ETargetType.TargetWizard)
                        {
                            bool bTargetEnemy = spell.targetType == (TargetType)TARGET_TYPE.WIZARD_ENEMY;
                            BattlePlayer targetBattlePlayer = bTargetEnemy ? battle.GetOtherPlayer(battlePlayer) : battlePlayer;

                            BattleHUD.CombatLogSpell(spellCaster, spell, targetBattlePlayer);
                            Battle.CastBattleSpell(spell, spellData, targetBattlePlayer);
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
                            BattleUnit targetUnit = battle.GetUnitAt(choice.t0);
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
                        battlePlayer.UseResourcesFor(spell);

                        battle.activeTurn.CastEffect(choice.t0, spell, battlePlayer);

                        if (Settings.GetData().GetBattleCameraFollow() == true)
                        {
                            CameraController.CenterAt(choice.t0);
                        }

                        yield return new WaitForSeconds(1.5f);

                    }
                }
            }
        }

        private static IEnumerator CastSpells(BattleUnit buCaster, BattlePlayer owner, Battle battle)
        {
            if (buCaster == null || buCaster.mana <= 0 || buCaster.spellCasted)
            {
                yield break;
            }

            List<DBReference<Spell>> refSpellList = buCaster.GetSpells();
            if (refSpellList == null || refSpellList.Count == 0)
            {
                yield break;
            }

            int iAttStrategicValue = battle.GetStrategicValue(battle.attacker == owner);
            int iDefStrategicValue = battle.GetStrategicValue(battle.attacker != owner);

            //current situation, compare both sides:
            //advantage of 5x or more. use maximum of 5% mana per spell if possible (use spells only if mana supplies are very large)
            //advantage of 2x or more. use maximum of 20% mana per spell if possible (use spells only if mana supplies are fair)
            //advantage of 1x or more. use maximum of 50% mana per spell if possible (use spells only if mana supplies are fair)
            //disadvantage use spell only if % of advantage gained is large enough in comparison to share or mana invested.
            //          ie: 0.3 advantage -> 0.5 advantage is 20% increase.  for which use maximum of 40% mana.
            //              0.5 advantage -> 0.9 advantage is 40% increase. for which use maximum of 80% mana.

            //use largest gain spell that fits mana limit profile. 
            //there is an option to do spell value sort based on mana/value proportion, but it does not fit current casting design.

            int iMaxAvailMana = buCaster.mana;

            float fAdvantage = iDefStrategicValue == 0 ? 10f : iAttStrategicValue / (float)iDefStrategicValue;
            int manaToUse = buCaster.mana;

            refSpellList = refSpellList.FindAll(o => !string.IsNullOrEmpty(o.Get().battleScript) &&
                                         o.Get().GetBattleCastingCost(buCaster) <= manaToUse);
            if (owner.wizard != null && buCaster.isHero && owner.wizard.banishedTurn <= 0)
            {
                refSpellList.AddRange(owner.wizard.GetSpells().FindAll(o =>
                                         !string.IsNullOrEmpty(o.Get().battleScript) &&
                                         o.Get().GetBattleCastingCost(buCaster) <= manaToUse));
            }

            if (refSpellList.Count > 0)
            {
                DifficultyOption ai = DifficultySettingsData.GetSetting("UI_DIFF_AI_SKILL");
                if (ai == null)
                {
                    Debug.LogError("Cannot resolve casting, no AI settings!");
                }
                List<Multitype<Vector3i, int, Spell>> optionList = new List<Multitype<Vector3i, int, Spell>>();
                float fCastingShare = 0;
                int aiSkill = 0;
                switch (ai.value)
                {
                    case "1":
                        //beginner
                        //consider only 1 spell or 20% of spells whichever is higher                        
                        fCastingShare = 0.2f;
                        aiSkill = 1;
                        break;
                    case "2":
                        //advanced
                        //consider only 1 spell or 50% of spells whichever is higher
                        fCastingShare = 0.5f;
                        aiSkill = 2;
                        break;
                    case "3":
                        //skilled
                        //consider only 1 spell or 80% of spells whichever is higher
                        fCastingShare = 0.8f;
                        aiSkill = 3;
                        break;
                    case "4":
                        //master
                        //consider all spells
                        fCastingShare = 1f;
                        aiSkill = 4;
                        break;
                    default:
                        Debug.LogWarning("AI setting not implemented!");

                        break;
                }

                if (owner.playerOwner)
                {
                    fCastingShare = 1f;
                    aiSkill = 4;
                }

                if (refSpellList.Count > 1)
                {
                    refSpellList.RandomSort();
                }

                int iMaxNumSpells = Mathf.Max((int)(refSpellList.Count * fCastingShare), 1);

                //if list would be build it would be cached for all cast considerations that are relevant to the spell.
                HashSet<Vector3i> hexesCloseToUnits = new HashSet<Vector3i>();

                for (int i = 0; i < iMaxNumSpells; i++)
                {
                    Multitype<Vector3i, int, Spell> data = CastingSimulation(refSpellList[i].Get(), aiSkill, battle, buCaster, owner, hexesCloseToUnits);
                    if (data != null)
                    {
                        optionList.Add(data);
                    }
                }

                if (optionList.Count < 1)
                {
                    yield break;
                }

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

                if (optionList.Count > 0)
                {
                    Multitype<Vector3i, int, Spell> choice = null;
                    foreach (Multitype<Vector3i, int, Spell> option in optionList)
                    {
                        int cost = option.t2.GetBattleCastingCost(buCaster);

                        option.t1 = option.t2.GetSpellTacticalValue(cost, option.t1);

                        if (choice == null || choice.t1 < option.t1)
                        {
                            if (option.t1 > 0)
                            {
                                choice = option;
                            }
                        }
                    }

                    if (choice != null)
                    {
                        Spell spell = choice.t2;

                        buCaster.spellCasted = true;
                        buCaster.Mp = FInt.ZERO;
                        if (BattleHUD.GetSelectedUnit() == buCaster)
                        {
                            BattleHUD.RefreshSelection();
                        }


                        //Check if someone is casted counter magic battle
                        if ((bool)ScriptLibrary.Call("CounterMagicBattle", battle, spell, buCaster))
                        {
                            buCaster.mana -= spell.GetBattleCastingCost(buCaster);
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
                        SpellCastData spellData = new SpellCastData(buCaster, battle);
                        if (spell.targetType.enumType == ETargetType.TargetWizard)
                        {
                            bool bTargetEnemy = spell.targetType == (TargetType)TARGET_TYPE.WIZARD_ENEMY;
                            BattlePlayer targetW = bTargetEnemy ? battle.GetOtherPlayer(owner) : owner;

                            BattleHUD.CombatLogSpell(buCaster, spell, targetW);
                            Battle.CastBattleSpell(spell, spellData, targetW);
                            BattleHUD.CombatLogSpellAddEffect();
                        }
                        else if (spell.targetType.enumType == ETargetType.TargetGlobal)
                        {
                            BattleHUD.CombatLogSpell(buCaster, spell, battle);
                            Battle.CastBattleSpell(spell, spellData, battle);
                            BattleHUD.CombatLogSpellAddEffect();
                        }
                        else
                        {
                            BattleUnit buTarget = battle.GetUnitAt(choice.t0);
                            if (buTarget != null)
                            {
                                BattleHUD.CombatLogSpell(buCaster, spell, buTarget);
                                Battle.CastBattleSpell(spell, spellData, buTarget);
                                BattleHUD.CombatLogSpellAddEffect();
                            }
                            else
                            {
                                BattleHUD.CombatLogSpell(buCaster, spell, choice.t0);
                                Battle.CastBattleSpell(spell, spellData, choice.t0);
                                BattleHUD.CombatLogSpellAddEffect();
                            }
                            buTarget?.GetOrCreateFormation().UpdateFigureCount();
                        }
                        buCaster.mana -= spell.GetBattleCastingCost(buCaster);

                        battle.activeTurn.CastEffect(choice.t0, spell, buCaster);

                        if (Settings.GetData().GetBattleCameraFollow() == true)
                        {
                            CameraController.CenterAt(choice.t0);
                        }

                        yield return new WaitForSeconds(1.5f);
                    }
                }
            }
        }

        private static Multitype<Vector3i, int, Spell> CastingSimulation(Spell spell, int aiSkill, Battle battle, ISpellCaster caster, BattlePlayer w, HashSet<Vector3i> hexesCloseToUnits)
        {
            ETargetType spellTargetType = spell.targetType.enumType;
            if (spellTargetType == ETargetType.TargetUnit)
            {
                List<BattleUnit> buTargetList = new List<BattleUnit>();
                foreach (KeyValuePair<BattleUnit, MOM.Unit> v in battle.buToSource)
                {
                    if (v.Key.IsAlive() &&
                        (v.Key.GetWizardOwnerID() == caster.GetWizardOwnerID() ||
                        v.Key.currentlyVisible) &&
                        (bool)ScriptLibrary.Call(spell.targetingScript, new SpellCastData(caster, battle), v.Key, spell))
                    {
                        buTargetList.Add(v.Key);
                    }
                }
                if (buTargetList.Count < 1)
                {
                    return null;
                }

                int start = UnityEngine.Random.Range(0, buTargetList.Count);

                BattleUnit buBestTarget = null;
                int iBestStrategicValueDelta = 0;
                for (int i = 0; i < buTargetList.Count; i++)
                {
                    int index = (i + start) % buTargetList.Count;

                    BattleUnit curBuTarget = buTargetList[index];
                    int iStrategicValueDelta = 0;

                    if (!string.IsNullOrEmpty(spell.aiBattleEvaluationScript))
                    {
                        iStrategicValueDelta = (int)ScriptLibrary.Call(spell.aiBattleEvaluationScript, new SpellCastData(caster, battle), curBuTarget, spell);
                    }
                    else
                    {
                        int prev = curBuTarget.GetBattleUnitValue();
                        int post = curBuTarget.GetStrategicValueForSpell(caster, battle, spell);

                        iStrategicValueDelta = post - prev;
                        if (curBuTarget.ownerID != w.GetID())
                        {
                            iStrategicValueDelta = -iStrategicValueDelta;
                        }
                    }

                    if (iBestStrategicValueDelta < iStrategicValueDelta)
                    {
                        iBestStrategicValueDelta = iStrategicValueDelta;
                        buBestTarget = buTargetList[index];
                    }
                }

                #if (UNITY_EDITOR)
                if (spell != null && buBestTarget != null && buBestTarget.dbSource != null &&
                   buBestTarget.dbSource.dbName != null)
                {
                    Debug.Log(spell.dbName + " with script " + spell.aiBattleEvaluationScript.ToString() + 
                        " give SpellAI value " + iBestStrategicValueDelta + 
                        " on unit " + buBestTarget.GetDBName().ToString());
                }
                #endif
                if (iBestStrategicValueDelta <= 0)
                {
                    return null;
                }

                return new Multitype<Vector3i, int, Spell>(buBestTarget.GetPosition(), iBestStrategicValueDelta, spell);
            }
            else if (spellTargetType == ETargetType.TargetHex)
            {
                //make list of locations around allied and enemy units if one is not prepared. 
                if (hexesCloseToUnits.Count == 0)
                {
                    foreach (KeyValuePair<BattleUnit, MOM.Unit> v in battle.buToSource)
                    {
                        if (!v.Key.IsAlive())
                        {
                            continue;
                        }

                        List<Vector3i> nearNeighbors = HexNeighbors.GetRange(v.Key.GetPosition(), 1, 1);
                        foreach (Vector3i posn in nearNeighbors)
                        {
                            if (!hexesCloseToUnits.Contains(posn) && battle.plane.area.IsInside(posn))
                            {
                                hexesCloseToUnits.Add(posn);
                            }
                        }
                    }
                }

                Multitype<Vector3i, int, Spell> target = null;
                foreach (Vector3i posn in hexesCloseToUnits)
                {
                    bool bCanCastAtTarget = true;
                    if (!string.IsNullOrEmpty(spell.targetingScript))
                    {
                        bCanCastAtTarget = (bool)ScriptLibrary.Call(spell.targetingScript, new SpellCastData(caster, battle), posn, spell);
                    }

                    if (bCanCastAtTarget)
                    {
                        if (string.IsNullOrEmpty(spell.aiBattleEvaluationScript))
                        {
                            Debug.LogWarning("Spell " + spell.dbName + " does not have ai evaluation script, required for this category of spells");
                            return null;
                        }

                        int iAiSpellEvalValue = (int)ScriptLibrary.Call(spell.aiBattleEvaluationScript, new SpellCastData(caster, battle), posn, spell);
                        if (iAiSpellEvalValue <= 0)
                        {
                            continue;
                        }

                        if (target == null || target.t1 < iAiSpellEvalValue)
                        {
                            target = new Multitype<Vector3i, int, Spell>(posn, iAiSpellEvalValue, spell);
                        }
                    }
                }

                if (target != null && target.t1 > 0)
                {
                    return target;
                }
            }
            else if (spellTargetType == ETargetType.TargetGlobal ||
                     spellTargetType == ETargetType.WorldHexBattleGlobal)
            {
                bool bCanCastAtTarget = true;
                if (!string.IsNullOrEmpty(spell.targetingScript))
                {
                    bCanCastAtTarget = (bool)ScriptLibrary.Call(spell.targetingScript, new SpellCastData(caster, battle), battle, spell);
                }

                if (bCanCastAtTarget)
                {
                    if (string.IsNullOrEmpty(spell.aiBattleEvaluationScript))
                    {
                        Debug.LogWarning("Spell " + spell.dbName + " does not have ai evaluation script, required for this category of spells");
                        return null;
                    }

                    int iAiSpellEvalValue = (int)ScriptLibrary.Call(spell.aiBattleEvaluationScript, new SpellCastData(caster, battle), battle, spell);
                    if (iAiSpellEvalValue <= 0)
                    {
                        return null;
                    }

                    return new Multitype<Vector3i, int, Spell>(Vector3i.invalid, iAiSpellEvalValue, spell);
                }
            }
            else if (spellTargetType == ETargetType.TargetWizard)
            {
                bool bTargetEnemy = spell.targetType == (TargetType)TARGET_TYPE.WIZARD_ENEMY;
                BattlePlayer targetW = bTargetEnemy ? battle.GetOtherPlayer(w) : w;

                bool bCanCastAtTarget = true;
                if (!string.IsNullOrEmpty(spell.targetingScript))
                {
                    bCanCastAtTarget = (bool)ScriptLibrary.Call(spell.targetingScript, new SpellCastData(caster, battle), targetW, spell);
                }

                if (bCanCastAtTarget)
                {
                    if (string.IsNullOrEmpty(spell.aiBattleEvaluationScript))
                    {
                        Debug.LogWarning("Spell " + spell.dbName + " does not have ai evaluation script, required for this category of spells");
                        return null;
                    }

                    int iAiSpellEvalValue = (int)ScriptLibrary.Call(spell.aiBattleEvaluationScript, new SpellCastData(caster, battle), targetW, spell);
                    if (iAiSpellEvalValue <= 0)
                    {
                        return null;
                    }

                    return new Multitype<Vector3i, int, Spell>(Vector3i.invalid, iAiSpellEvalValue, spell);
                }
            }

            return null;
        }

        private static void DetectInvalidLocation(Vector3i pos, SearcherDataV2 sd, string identifier)
        {
            if (sd.IsUnitAt(pos))
            {
                Debug.LogWarning("(" + identifier + ") Tracking potential issue at " + pos);
            }
        }

        private static void CutByEarlierObstacle(List<Vector3i> poss, SearcherDataV2 sd, Battle battle, BattleUnit walker)
        {
            int iStartIndex = poss.Count - 1;

            bool bStoppedByGate = walker.attackingSide && sd.walls != null && sd.walls[sd.gateIndex];

            for (int i = iStartIndex; i > 0; i--)
            {
                if (sd.IsUnitAt(poss[i]))
                {
                    BattleUnit obstacle = battle.GetUnitAt(poss[i]);
                    //if first free expected location is occupied by an enemy or
                    //occupied by anyone when it was not location that we could pass through
                    if (walker.attackingSide != obstacle.attackingSide || iStartIndex == i)
                    {
                        iStartIndex = i - 1;
                        continue;
                    }
                }
                if (bStoppedByGate)
                {
                    //if gate for an attacker is detected on the path, cut path to step before
                    if (sd.GetIndex(poss[i]) == sd.gateIndex)
                    {
                        iStartIndex = i - 1;
                        continue;
                    }
                }
            }

            if (iStartIndex < poss.Count - 1)
            {
                poss.RemoveRange(iStartIndex + 1, poss.Count - iStartIndex - 1);
            }
        }
        private static void UpdateAIUnitInfo(BattleUnit bu)
        {
            if (bu.attackingSide)
            {
                BattleHUD.Get().attackerInfo.UpdateUnitInfoDisplay(bu, false);
                BattleHUD.Get().attackerInfo.HighlightSelectedUnit(bu);

            }
            else
            {
                BattleHUD.Get().defenderInfo.UpdateUnitInfoDisplay(bu, false);
                BattleHUD.Get().defenderInfo.HighlightSelectedUnit(bu);
            }
        }

        #endregion
    }
}
#endif