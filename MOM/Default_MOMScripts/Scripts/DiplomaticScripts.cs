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
    public class DiplomaticScripts : ScriptBase
    {

        static public void UpdateWillOfWar(DiplomaticStatus status)
        {
            //slow tendency toward neutrality
            if (status.willOfWar > 0) status.willOfWar--;
            if (status.willOfWar < 0) status.willOfWar++;

            var aiDifficulty = DifficultySettingsData.GetSettingAsInt("UI_DIFF_AI_SKILL");            
            int ownHostility = status.GetOwnPersonality().hostility;
            //hostility may grow based on the personality
            if (UnityEngine.Random.Range(0, 100) < ownHostility)
            {
                status.willOfWar ++;
            }

            //will of war purely based on the relationship accelerating toward more polarized war will
            if (status.GetRelationship() < -30 && status.willOfWar < 50) status.willOfWar++;
            if (status.GetRelationship() > 30 && status.willOfWar > 50) status.willOfWar--;

            //war status based on the relationship with fear
            if (status.fear < 0)
            {
                status.willOfWar ++;
                //enemy is stronger and due to the goal of the game they are more willing to start a war
                //this increases based on AI
                if (aiDifficulty > 1) status.willOfWar ++;

                if (status.GetRelationship() < -60) status.willOfWar += 2;
                if (status.GetRelationship() < -30) status.willOfWar += 1;
            }

            if (status.openWar)
            {
                //update this state, based on the war events
                if (status.warDelta < -10000 && status.fear > 10000)
                {
                    //enemy is stronger and I'm losing!
                    status.willOfWar -= 3;
                }
                else if (status.warDelta < -5000 && status.fear > 5000)
                {
                    //this war does not seem beneficial
                    status.willOfWar--;
                }
                else if (status.warDelta < 1000 && status.willOfWar > 10)
                {
                    //there is no gain in prolonged wars...
                    //either there is not enough battles and this war is dry or they do not end well for me
                    status.willOfWar--;
                }
                else if (status.warDelta > 5000 && status.fear < -5000)
                {
                    //that might be a way to win
                    status.willOfWar++;
                }
                else
                {
                    //its going well enough
                }
            }

            var targetMagic = status.target.Get().GetMagicAndResearch();

            if (targetMagic.curentlyCastSpell != null && targetMagic.curentlyCastSpell == (Spell)SPELL.SPELL_OF_MASTERY)
            {
                status.willOfWar = 100;

                //relationship degrade fast when one declare they are the master of magic
                status.GetReverseStatusFromTarget().ChangeRelationshipBy(-25, true);
            }

            var w = status.owner.Get() as PlayerWizardAI;
            if (w != null)
            {
                var ownLocations = GameManager.GetLocationsOfWizard(status.owner.Get().GetID());
                
                int targetID = status.target.Get().ID;
                float pressure = 0f;

                if (w.arcanusVisibility.sensedGroups != null)
                {
                    ProcessPlaneDanger(w.arcanusVisibility, targetID, status, ownLocations, aiDifficulty, ref pressure);
                }
                if (w.myrrorVisibility.sensedGroups != null)
                {
                    ProcessPlaneDanger(w.myrrorVisibility, targetID, status, ownLocations, aiDifficulty, ref pressure);
                }
                //increase will of war if there is border/army pressure between players.
                //there will be few steps with increased "difficulty" of will of war attempts
                if (UnityEngine.Random.Range(0, 2f) < pressure)
                {
                    status.willOfWar++;

                    if (UnityEngine.Random.Range(0, 4f) < pressure)
                    {
                        status.willOfWar++;

                        if (UnityEngine.Random.Range(0, 10f) < pressure)
                        {
                            status.willOfWar++;
                        }
                    }
                }

                if(status.target.Get() != GameManager.GetHumanWizard())
                {
                    //if this is relationship between two AI, AI difficulty is used to push them away from war between each other
                    //AI fighting each other results in limited strength to be used against player
                    status.willOfWar -= aiDifficulty * aiDifficulty;
                }
            }

            status.willOfWar = Mathf.Clamp(status.willOfWar, -100, 100);

            //slowly degraded toward 0
            status.warDelta = (int)(status.warDelta * 0.9f);

            if (status.openWar) status.ChangeRelationshipBy(-200, false);

            //If Ai have v.high relationship with someone and v.high will of war, (or other way around) they both will counter each other as your relationship influence your will of war 
            //ie giving presents may result in cutting down war
            int diff = Mathf.Abs(status.willOfWar - status.GetRelationship());
            if (diff < 40)
            {
                float strength = (40 - diff) / 40f;

                status.willOfWar -= (int)(status.willOfWar * .1f * strength);
                var rel = -(int)(status.GetRelationship() * .1f * strength);
                status.ChangeRelationshipBy(rel, true);
            }
        }
        static void ProcessPlaneDanger(AIPlaneVisibility planeVisibility, int targetID, DiplomaticStatus status, List<MOM.Location> ownLocations, int aiDifficulty, ref float pressure)
        {
            int maxDanger = 0;
            foreach (var v in planeVisibility.sensedGroups)
            {
                if (v.GetOwnerID() == targetID && v.GetUnits().Count > 0)
                {
                    if (v.IsGroupInvisible() && planeVisibility.ownGroups.FindAll(o => o.GetDistanceTo(v) < 2) == null)
                    {
                        continue;
                    }
                    bool isAllied = status.IsAllied();
                    var val = v.GetValue();
                    pressure += 0.1f + val * 0.0001f;

                    if (!isAllied && v.GetLocationHostSmart() == null)
                    {
                        foreach (var l in ownLocations)
                        {
                            if (!(l is TownLocation)) continue;
                            int dist = l.GetDistanceTo(v);
                            if (aiDifficulty >2 && dist < 5 || dist < 4)
                            {
                                //provide full danger value of the army only at the 2000+ army strength
                                float scale = Mathf.Clamp01(val / 2000f);
                                var h = status.owner.Get().personality.Get().hostility;
                                if(h > 0)
                                {
                                    //maximum hostile ai will add +40% to the perceived trespassing violation, and that is further scaled by ai difficulty (x0 easy, x1 normal,...)
                                    int diffScalar = (aiDifficulty - 1);
                                    scale = scale * (1 + diffScalar * h * 0.01f);
                                }

                                int danger = 0;
                                if (aiDifficulty <= 2) danger = (int)((4 - dist) * aiDifficulty * scale);
                                else danger = (int)((5 - dist) * 2 * scale);

                                if (targetID == PlayerWizard.HumanID())
                                {
                                    maxDanger += danger;
                                }
                                else if (danger > maxDanger)
                                {
                                    maxDanger = danger;
                                }
                            }
                        }
                    }
                }
            }
            if (maxDanger > 0)
            {
                status.willOfWar += maxDanger;

                if (targetID == PlayerWizard.HumanID())
                    status.ConsiderWarningForWalking();
            }
        }
        static public void UpdateWillOfTreaty(DiplomaticStatus status)
        {
            if (status.willTreaty < 100) status.willTreaty += 1;
            if (status.willTreaty < 0) status.willTreaty += UnityEngine.Random.Range(0, 4);
            if (status.willTreaty < 50) status.willTreaty += UnityEngine.Random.Range(0, 4);

            status.willTreaty = Mathf.Clamp(status.willTreaty, -100, 100);
        }
        static public void UpdateWillOfTrade(DiplomaticStatus status)
        {
            int changeScale = 2;
            if (status.GetRelationship() < -40)
            {
                //will grow after trading about 7p/turn on average
                changeScale = 3;
            }
            else if (status.GetRelationship() < 40)
            {
                //will grow after trading about 11p/turn on average
                changeScale = 5;
            }
            else
            {
                //will grow after trading about 17p/turn on average
                changeScale = 8;
            }

            if (status.willToTrade < 100) status.willToTrade += changeScale;
            if (status.willToTrade < 0) status.willToTrade += UnityEngine.Random.Range(0, changeScale) + 1;
            if (status.willToTrade < 50) status.willToTrade += UnityEngine.Random.Range(0, changeScale) + 1;

            status.willToTrade = Mathf.Clamp(status.willToTrade, -100, 100);
        }

        static public void TreatyBreakOthersReaction(DiplomaticStatus status, int strength)
        {
            HashSet<int> knownWizards = new HashSet<int>();
            foreach (var v in status.GetDiplomacyManager().statusses)
            {
                knownWizards.Add(v.Key);
            }
            foreach (var v in status.GetTargetDiplomacyManager().statusses)
            {
                knownWizards.Add(v.Key);
            }

            var victim = status.target.Get();

            foreach (var v in knownWizards)
            {
                if (v != status.owner.ID)
                {
                    var w = GameManager.GetWizard(v);
                    FInt pScale = w.GetPersonality().reactionTooNegativeDiplomacy;
                    var s = w.GetDiplomacy().GetStatusToward(status.owner.ID);
                    var vs = w.GetDiplomacy().GetStatusToward(victim.GetID());

                    //if status does not exists, then its likely someone the other side knows, but they do not know treaty breaker.
                    if (s == null) continue;
                    float victimInfluence = 1f;

                    if (vs != null && vs.GetRelationship() < 0)
                    {
                        //if victim is not liked, then fallout from break of contract of other parties with them is limited. And is 0 at -30 relationship
                        victimInfluence = Mathf.Clamp01(1f + vs.GetRelationship() * 0.03f);
                    }

                    s.ChangeRelationshipBy(-(strength * pScale * victimInfluence).ToInt(), true);
                }
            }
        }

        /// <returns>100 if wizard is willing to start war. -100 if the war is disastrous for them </returns>
        static public int TRE_WarEvaluation(DiplomaticStatus status)
        {
            //AI script is not designed to work with human, who evaluates at their own discretion
            if (!(status.owner.Get() is PlayerWizardAI)) return -100;

            //Will is based on
            // - fear : based on army force difference
            // - army pressure : created by expeditions(groups) and locations controlled by other player that have units defending them. 
            //ie. Player may show their peaceful intentions having undefended locations near AI towns(ie planar gates).
            //On the other hand AI will not see capturing unprotected non-towns as act of war. 
            //this way one can for example use planar gates to travel between planes, and avoid political issue of the tower standing to close to the border.
            //it changes with time and during war, it changes to reflect gains and losses
            var willOfWar = Mathf.Clamp(status.willOfWar, -100, 100);


            //Additionally will of war is lowered by number of other treaties sides have
            if (status.treaties != null)
            {
                foreach(var t in status.treaties)
                {
                    //we would break alliance before considering war
                    if (t.source.Get() == (Treaty)TREATY.ALLIANCE) willOfWar -= 200;

                    //treaties that require agreement from two sides are treaties that breaking it should bring diplomatic repercussions
                    if(t.source.Get().agreementRequired)
                    {
                        willOfWar -= 10;
                    }
                }
            }

            return willOfWar;
        }
        /// <summary>
        /// Update status of the relationship between wizards when this treaty starts
        /// </summary>
        static public void TRE_WarStart(DiplomaticStatus status)
        {
            status.openWar = true;
            status.willOfWar += 100;
            status.ChangeRelationshipBy(-200, true);

            var ownerDiplomacy = status.owner.Get().GetDiplomacy();

            var treaties = status.GetTreaties();
            if(treaties != null)
            {
                for(int i=0; i< treaties.Count; i++)
                {
                    if(treaties[i].source != (Treaty)TREATY.WAR)
                    {
                        status.BreakTreaty(treaties[i]);
                        i--;
                    }
                }
            }

            foreach (var v in ownerDiplomacy.statusses)
            {
                if (v.Value.treaties != null)
                {
                    var t = v.Value.treaties.Find(o => o.source.Get() == (Treaty)TREATY.ALLIANCE);
                    if(t != null)
                    {
                        //we are allied and attempting to drag someone into our war. All allies are automatically reconsidering their friendships

                        var allyDiplomacy = v.Value.target.Get().GetDiplomacy();
                        var allyTowardUs = allyDiplomacy.GetStatusToward(status.owner.Get());
                        var allyTowardOthers = allyDiplomacy.GetStatusToward(status.owner.Get());

                        if (allyTowardOthers.openWar) continue;

                        //do not make decisions for player.
                        if (!(v.Value.target.Get() is PlayerWizardAI)) continue;

                        var allyPersonality = v.Value.target.Get().GetPersonality();

                        //Do ally like us more than the other party?
                        //or at least do they want to have war with the other party more than with us?
                        if (allyTowardUs.GetRelationship() > allyTowardOthers.GetRelationship() ||
                            allyTowardUs.willOfWar < allyTowardOthers.willOfWar)
                        {                            
                            DiplomaticMessage m = new DiplomaticMessage();
                            m.messageType = DiplomaticMessage.MessageType.WarDeclaration;
                            m.domination = DiplomaticMessage.Domination.ClearSameAndBelow;
                            allyTowardOthers.AddMessage(m);
                        }
                        else
                        {                            
                            DiplomaticMessage m = new DiplomaticMessage();
                            m.messageType = DiplomaticMessage.MessageType.BreakTreaty;
                            m.domination = DiplomaticMessage.Domination.ClearSameAndBelow;
                            m.keys = new string[1] { TREATY.ALLIANCE.ToString() };

                            allyTowardUs.AddMessage(m);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Update status of the relationship between wizards when this treaty ends
        /// </summary>
        static public void TRE_WarEnd(DiplomaticStatus status)
        {
            status.openWar = false;
            status.willOfWar -= 40;

            var ownerDiplomacy = status.owner.Get().GetDiplomacy();

            foreach (var v in ownerDiplomacy.statusses)
            {
                if (v.Value.treaties != null)
                {
                    var t = v.Value.treaties.Find(o => o.source.Get() == (Treaty)TREATY.ALLIANCE);
                    if (t != null)
                    {
                        var allyDiplomacy = v.Value.target.Get().GetDiplomacy();
                        var allyTowardUs = allyDiplomacy.GetStatusToward(status.owner.Get());
                        var allyTowardOthers = allyDiplomacy.GetStatusToward(status.owner.Get());

                        if (allyTowardOthers.openWar) continue;

                        //do not make decisions for player.
                        if (!(v.Value.target.Get() is PlayerWizardAI)) continue;

                        //Do ally like us more than the other party?
                        //or at least do they want to have war with the other party more than with us?
                        if (allyTowardOthers.willOfWar < 50 || allyTowardUs.GetRelationship() > allyTowardOthers.willOfWar)
                        {
                            DiplomaticMessage m = new DiplomaticMessage();
                            m.messageType = DiplomaticMessage.MessageType.BreakTreaty;
                            m.domination = DiplomaticMessage.Domination.ClearSameAndBelow;
                            m.keys = new string[1] { TREATY.WAR.ToString() };

                            allyTowardOthers.AddMessage(m);
                        }                        
                    }
                }
            }
        }


        /// <returns>100 if wizard is willing to start alliance. -100 if the treaty is disastrous for them </returns>
        static public int TRE_AllianceEvaluation(DiplomaticStatus status)
        {
            var willOfAlliance = status.GetRelationship();
            if(status.willOfWar < 0)
            {
                //maybe alliance is good due to fear?
                willOfAlliance = Math.Max(willOfAlliance, -status.willOfWar / 2);
            }
            willOfAlliance -= 10;

            //we need to end alliance before considering war
            if (status.openWar) return -100;            

            return willOfAlliance;
        }
        static public void TRE_AllianceUpdate(DiplomaticStatus status)
        {
            var ownerDiplomacy = status.owner.Get().GetDiplomacy();
            var allyDiplomacy = status.target.Get().GetDiplomacy();
            var allyTowardUs = allyDiplomacy.GetStatusToward(status.owner.Get());

            foreach (var v in ownerDiplomacy.statusses)
            {
                if (v.Value.openWar)
                {                            
                    var allyTowardEnemy = allyDiplomacy.GetStatusToward(v.Key);

                    if (allyTowardEnemy.openWar)
                    {
                        //improve relationship with ally in joined war
                        status.ChangeRelationshipBy(+1, true);
                        continue;
                    }
                    else
                    {
                        //ally is not in joined war!
                        status.ChangeRelationshipBy(-1, true);
                    }

                    //do not make decisions for player.
                    if (!(v.Value.target.Get() is PlayerWizardAI)) continue;

                    var ownerTowardTheirEnemy = allyDiplomacy.GetStatusToward(v.Key);
                    //increase ally will of war toward those harassing our ally

                    if(UnityEngine.Random.Range(0f,1f )< 0.5f) ownerTowardTheirEnemy.willOfWar++;
                    
                }
            }
        }
        static public void TRE_AllianceBreak(DiplomaticStatus status)
        {            
            var theirStatus = status.GetReverseStatusFromTarget();

            //their relationship changes extra stronger due to being directly affected
            theirStatus.willOfWar += 20;
            theirStatus.ChangeRelationshipBy(-40, true);

            TreatyBreakOthersReaction(status, 60);
        }

        static public int TRE_ResearchEvaluation(DiplomaticStatus status)
        {
            //research is always beneficial enough to want it.            
            var r = status.GetRelationship();
            var value = (int)(10 + r*1.5f);

            if (status.openWar) value -= 200;
            //single division only removes one overflow created by multiplying of the two values
            //            return MathF.Sign(value) * value * value / 100;
            // fixed locally
            return (int)Mathf.Sign(value) * value * value / 100;
        }
        static public void TRE_ResearchStart(DiplomaticStatus status)
        {
            var wizard = status.owner.Get();
            var ench = (Enchantment)DataBase.Get("ENCH-RESEARCH_ALLIANCE", false);
            wizard.AddEnchantment(ench, wizard as IEnchantable);
            status.ChangeRelationshipBy(8, true);
        }
        static public void TRE_ResearchBreak(DiplomaticStatus status)
        {
            var theirStatus = status.GetReverseStatusFromTarget();

            //their relationship changes extra stronger due to being directly affected            
            theirStatus.ChangeRelationshipBy(-20, true);

            TreatyBreakOthersReaction(status, 30);
        }
        static public void TRE_ResearchEnd(DiplomaticStatus status)
        {
            var wizard = status.owner.Get();
            var ench = (Enchantment)DataBase.Get("ENCH-RESEARCH_ALLIANCE", false);
            wizard.RemoveEnchantment(ench);
        }


        static public int TRE_TradeEvaluation(DiplomaticStatus status)
        {
            //trade is always beneficial enough to want it. 
            var r = status.GetRelationship();
            var value = (int)( 30 + r);

            if (status.openWar) value -= 200;
            //single division only removes one overflow created by multiplying of the two values 
            //            return MathF.Sign(value) * value * value / 100;
            // fixed locally
            return (int)Mathf.Sign(value) * value * value / 100;
        }
        static public void TRE_TradeStart(DiplomaticStatus status)
        {
            var wizard = status.owner.Get();
            var ench = (Enchantment)DataBase.Get("ENCH-TRADE_ALLIANCE", false);
            wizard.AddEnchantment(ench, wizard as IEnchantable);
            status.ChangeRelationshipBy(8, true);
        }
        static public void TRE_TradeBreak(DiplomaticStatus status)
        {
            var theirStatus = status.GetReverseStatusFromTarget();

            //their relationship changes extra stronger due to being directly affected            
            theirStatus.ChangeRelationshipBy(-20, true);

            TreatyBreakOthersReaction(status, 30);
        }
        static public void TRE_TradeEnd(DiplomaticStatus status)
        {
            var wizard = status.owner.Get();
            var ench = (Enchantment)DataBase.Get("ENCH-TRADE_ALLIANCE", false);
            wizard.RemoveEnchantment(ench);
        }
    }

}
#endif