#if !USE_DEBUG_SCRIPT || !UNITY_EDITOR

using DBDef;
using DBEnum;
using MHUtils;
using MOM;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Group = MOM.Group;

namespace MOMScripts
{
    public class GroupScripts : ScriptBase
    {
        static List<Multitype<BattleUnit, int>> OptimizedNoSettlers(int points, int optimalization)
        {
            List<Multitype<BattleUnit, int>> list = PowerEstimate.GetList();
            list = list.FindAll(o => DataBase.GetType<DBDef.Unit>().Contains(o.t0.dbSource.Get() as DBDef.Unit));

            //first unit minimal cost is either 1/3 of remaining points or 900 assuming optimization starts at 4
            int half = Mathf.Min(points / 2, 500+ optimalization*100);

            //we are looking only for units with the cost between 50-100% of the remaining points
            //if pool of available points is larger than 1000, we want to clamp options to 500-1000
            var l2 = list.FindAll(o => 
                o.t1 >= half && 
                o.t1 <= points && 
                !o.t0.GetAttributes().Contains(TAG.CONSTRUCTION_UNIT));

            if(l2.Count < 1)
            {
                //if some unit criteria eliminate to many units, we want to go for more allowing option
                l2 = list.FindAll(o => 
                o.t1 <= points && 
                !o.t0.GetAttributes().Contains(TAG.CONSTRUCTION_UNIT));
            }
            return l2;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mainTag"> Required Tag for a Unit </param>
        /// <param name="secondaryTag"> Alternate for a mainTag </param>
        /// <param name="optionalTags"> Unit required one of a Tags from array </param>
        /// <param name="forbiddenTags"> Unit has to have none of Tags in array </param>
        /// <param name="maxValue"> </param>
        /// <param name="oneOfOptionalTags"> Use </param>
        /// <returns></returns>
        static List<Multitype<BattleUnit, int>> OptimizedWithCriteria(DBDef.Group g)
        {
            List<Multitype<BattleUnit, int>> list = PowerEstimate.GetList();
            list = list.FindAll(o => DataBase.GetType<DBDef.Unit>().Contains(o.t0.dbSource.Get() as DBDef.Unit));

            if(g.race != null && g.race.Length > 0)
            {
                List<Multitype<BattleUnit, int>> tempList = new List<Multitype<BattleUnit, int>>();

                foreach (var r in g.race)
                {
                    tempList.AddRange(list.FindAll(o => o.t0.race == r));
                }
                list = tempList;
            }

            if(g.requiredTags != null && g.requiredTags.Length > 0)
            {
                list = list.FindAll(o => o.t0.GetAttributes().ContainsAll(g.requiredTags));
            }

            if(g.optionalTags != null && g.optionalTags.Length > 0)
            {
                list = list.FindAll(o => o.t0.GetAttributes().ContainsAny(g.optionalTags));
            }

            if(g.forbiddenTags != null && g.forbiddenTags.Length > 0)
            {
                list = list.FindAll(o => o.t0.GetAttributes().ContainsNone(g.forbiddenTags));
            }

            return list;

        }

        static List<Multitype<BattleUnit, int>> OptimizedWithCriteria(Dictionary<Race, float> races, Tag[] optionalTags, Tag[] forbiddenTags)
        {
            List<Multitype<BattleUnit, int>> list = PowerEstimate.GetList();
            list = list.FindAll(o => DataBase.GetType<DBDef.Unit>().Contains(o.t0.dbSource.Get() as DBDef.Unit));

            if (races != null && races.Count > 0)
            {
                List<Multitype<BattleUnit, int>> tempList = new List<Multitype<BattleUnit, int>>();
                MHRandom r = new MHRandom();
                float value = r.GetFloat(0, 1);
                float chance = 0f;

                foreach (var race in races)
                {
                    if (race.Value == 1)
                    {
                        tempList.AddRange(list.FindAll(o => (o.t0.race == race.Key)));
                    }
                    else
                    {
                        chance += race.Value;
                        if (chance >= value)
                        {
                            tempList.AddRange(list.FindAll(o => (o.t0.race == race.Key)));
                            break;
                        }
                    }
                }
                list = tempList;
            }
            
            if (optionalTags != null && optionalTags.Length > 0)
            {
                list = list.FindAll(o => o.t0.GetAttributes().ContainsAny(optionalTags));
            }
            
            //use forbiddenTags if valid
            if (forbiddenTags != null && forbiddenTags.Length > 0)
            {
                list = list.FindAll(o => o.t0.GetAttributes().ContainsNone(forbiddenTags));
            }

            return list;
        }

        static List<Multitype<BattleUnit, int>> ReturnByPointsOptimalization(int points, int optimalization, List<Multitype<BattleUnit, int>> list)
        {
            //first unit minimal cost is either 1/3 of remaining points or 900 assuming optimization starts at 4
            int half = Mathf.Min(points / 3, 500 + optimalization * 100);

            //we are looking only for units with the cost between 50-100% of the remaining points
            //if pool of available points is larger than 1000, we want to clamp options to 500-1000
            var l2 = list.FindAll(o =>
                o.t1 >= half &&
                o.t1 <= points);

            if (l2.Count < 1)
            {
                //if some unit criteria eliminate to many units, we want to go for more allowing option
                l2 = list.FindAll(o =>
                o.t1 <= points);
            }
            return l2;
        }

        static List<Multitype<BattleUnit, int>> ReturnByPoints(int points, int optimalization, List<Multitype<BattleUnit, int>> list)
        {
            var l2 = list.FindAll(o =>
                o.t1 <= points);

            return l2;
        }
        #region Defenders Scripts
        static public object DEF_GeneralDefenders(MOM.Location l, int points, bool rampagingGroup)
        {
            Group g;
            Tag[] forbidenTag = new Tag[] {
                (Tag)TAG.CONSTRUCTION_UNIT, (Tag)TAG.SHIP,
                (Tag)TAG.DEBUG_TAG, (Tag)TAG.EVENT_ONLY_UNIT,
                (Tag)TAG.LAIR_EXCLUSION};

            if (rampagingGroup == false)
            {
                g = GroupCreation(l, points, null, null, forbidenTag, false);
            }
            else
            {
                if(l is TownLocation && (l as TownLocation).race != null)
                {
                    //if possible find race of the source town
                    Dictionary<Race, float> race = new Dictionary<Race, float>()
                    {
                        { (l as TownLocation).race, 1f }
                    };

                    g = GroupCreation(null, l.GetPlane(), l.GetOwnerID(),
                                  points, race, null, forbidenTag, false);
                }
                else
                {
                    g = GroupCreation(null, l.GetPlane(), l.GetOwnerID(),
                                  points, null, null, forbidenTag, false);
                }

                
                //g = GroupCreation(null, points, null, null, forbidenTag, false);
            }

            return g;
        }

        static public object DEF_ChaosDefenders(MOM.Location l, int points, bool rampagingGroup)
        {
            Group g;
            Tag[] forbidenTag = new Tag[] {
                (Tag)TAG.CONSTRUCTION_UNIT, (Tag)TAG.SHIP,
                (Tag)TAG.DEBUG_TAG, (Tag)TAG.EVENT_ONLY_UNIT,
                (Tag)TAG.LAIR_EXCLUSION};
            Dictionary<Race, float> race = new Dictionary<Race, float>()
            {
                { (Race)RACE.REALM_CHAOS, 1f }
            };
            
            if (rampagingGroup == false)
            {
                g = GroupCreation(l, points, race, null, forbidenTag, false);
            }
            else
            {
                g = GroupCreation(null, l.GetPlane(), l.GetOwnerID(),
                                  points, race, null, forbidenTag, false);
            }

            return g;
        }

        static public object DEF_NatureDefenders(MOM.Location l, int points, bool rampagingGroup)
        {
            Group g;
            Tag[] forbidenTag = new Tag[] {
                (Tag)TAG.CONSTRUCTION_UNIT, (Tag)TAG.SHIP,
                (Tag)TAG.DEBUG_TAG, (Tag)TAG.EVENT_ONLY_UNIT,
                (Tag)TAG.LAIR_EXCLUSION};
            Dictionary<Race, float> race = new Dictionary<Race, float>()
            {
                { (Race)RACE.REALM_NATURE, 1f }
            };

            if (rampagingGroup == false)
            {
                g = GroupCreation(l, points, race, null, forbidenTag, false);
            }
            else
            {
                g = GroupCreation(null, l.GetPlane(), l.GetOwnerID(),
                                  points, race, null, forbidenTag, false);
            }

            return g;
        }
        static public object DEF_SorceryDefenders(MOM.Location l, int points, bool rampagingGroup)
        {
            Group g;
            Tag[] forbidenTag = new Tag[] {
                (Tag)TAG.CONSTRUCTION_UNIT, (Tag)TAG.SHIP,
                (Tag)TAG.DEBUG_TAG, (Tag)TAG.EVENT_ONLY_UNIT,
                (Tag)TAG.LAIR_EXCLUSION};
            Dictionary<Race, float> race = new Dictionary<Race, float>()
            {
                { (Race)RACE.REALM_SORCERY, 1f }
            };
            
            if (rampagingGroup == false)
            {
                g = GroupCreation(l, points, race, null, forbidenTag, false);
            }
            else
            {
                g = GroupCreation(null, l.GetPlane(), l.GetOwnerID(),
                                  points, race, null, forbidenTag, false);
            }

            return g;
        }
        static public object DEF_DeathDefenders(MOM.Location l, int points, bool rampagingGroup)
        {
            Group g;
            Tag[] forbidenTag = new Tag[] {
                (Tag)TAG.CONSTRUCTION_UNIT, (Tag)TAG.SHIP,
                (Tag)TAG.DEBUG_TAG, (Tag)TAG.EVENT_ONLY_UNIT,
                (Tag)TAG.LAIR_EXCLUSION};
            Dictionary<Race, float> race = new Dictionary<Race, float>()
            {
                { (Race)RACE.REALM_DEATH, 1f }
            };

            if (rampagingGroup == false)
            {
                g = GroupCreation(l, points, race, null, forbidenTag, false);
            }
            else
            {
                g = GroupCreation(null, l.GetPlane(), l.GetOwnerID(),
                                  points, race, null, forbidenTag, false);
            }

            return g;
        }
        static public object DEF_LifeDefenders(MOM.Location l, int points, bool rampagingGroup)
        {
            Group g;
            Tag[] forbidenTag = new Tag[] {
                (Tag)TAG.CONSTRUCTION_UNIT, (Tag)TAG.SHIP,
                (Tag)TAG.DEBUG_TAG, (Tag)TAG.EVENT_ONLY_UNIT,
                (Tag)TAG.LAIR_EXCLUSION};
            Dictionary<Race, float> race = new Dictionary<Race, float>()
            {
                { (Race)RACE.REALM_LIFE, 1f }
            };

            if (rampagingGroup == false)
            {
                g = GroupCreation(l, points, race, null, forbidenTag, false);
            }
            else
            {
                g = GroupCreation(null, l.GetPlane(), l.GetOwnerID(),
                                  points, race, null, forbidenTag, false);
            }

            return g;
        }
        static public object DEF_SynergyDefenders(MOM.Location l, int points, bool rampagingGroup)
        {
            Group g;
            Tag[] forbidenTag = new Tag[] {
                (Tag)TAG.CONSTRUCTION_UNIT, (Tag)TAG.SHIP,
                (Tag)TAG.DEBUG_TAG, (Tag)TAG.EVENT_ONLY_UNIT,
                (Tag)TAG.LAIR_EXCLUSION};
            Dictionary<Race, float> race = new Dictionary<Race, float>()
            {
                { (Race)RACE.REALM_CHAOS, 0.2f },
                { (Race)RACE.REALM_NATURE, 0.2f },
                { (Race)RACE.REALM_DEATH, 0.2f },
                { (Race)RACE.REALM_LIFE, 0.2f },
                { (Race)RACE.REALM_SORCERY, 0.2f }
            };

            if (rampagingGroup == false)
            {
                g = GroupCreation(l, points, race, null, forbidenTag, false);
            }
            else
            {
                g = GroupCreation(null, l.GetPlane(), l.GetOwnerID(),
                                  points, race, null, forbidenTag, false);
            }

            return g;
        }
        static public object DEF_LairDefenders(MOM.Location l, int points, bool rampagingGroup)
        {
            Group g;
            Tag[] forbidenTag = new Tag[] { 
                (Tag)TAG.CONSTRUCTION_UNIT, (Tag)TAG.SHIP, 
                (Tag)TAG.DEBUG_TAG, (Tag)TAG.EVENT_ONLY_UNIT,
                (Tag)TAG.LAIR_EXCLUSION};
            Dictionary<Race, float> race = new Dictionary<Race, float>()
            {
                { (Race)RACE.REALM_CHAOS, 0.5f },
                { (Race)RACE.REALM_NATURE, 0.5f },
            };
            
            if (rampagingGroup == false)
            {
                g = GroupCreation(l, points, race, null, forbidenTag, false);
            }
            else
            {
                g = GroupCreation(null, l.GetPlane(), l.GetOwnerID(),
                                  points, race, null, forbidenTag, false);
            }
                        
            return g;
        }
        static public object DEF_TempleDefenders(MOM.Location l, int points, bool rampagingGroup)
        {
            Group g;
            Tag[] forbidenTag = new Tag[] { 
                (Tag)TAG.CONSTRUCTION_UNIT, (Tag)TAG.SHIP,
                (Tag)TAG.DEBUG_TAG, (Tag)TAG.EVENT_ONLY_UNIT ,
                (Tag)TAG.LAIR_EXCLUSION};

            Dictionary<Race, float> race = new Dictionary<Race, float>()
            {
                { (Race)RACE.REALM_LIFE, 0.25f },
                { (Race)RACE.REALM_DEATH, 0.75f },
            };

            if (rampagingGroup == false)
            {
                g = GroupCreation(l, points, race, null, forbidenTag, false);
            }
            else
            {
                g = GroupCreation(null, l.GetPlane(), l.GetOwnerID(),
                                  points, race, null, forbidenTag, false);
            }

            return g;
        }
        static public object DEF_Ancient_Temple(MOM.Location l, int points, bool rampagingGroup)
        {
            Group g;
            Tag[] forbidenTag = new Tag[] {
                (Tag)TAG.CONSTRUCTION_UNIT, (Tag)TAG.SHIP,
                (Tag)TAG.DEBUG_TAG, (Tag)TAG.EVENT_ONLY_UNIT ,
                (Tag)TAG.LAIR_EXCLUSION};

            Dictionary<Race, float> race = new Dictionary<Race, float>()
            {
                { (Race)RACE.REALM_LIFE, 0.25f },
                { (Race)RACE.REALM_DEATH, 0.50f },
                { (Race)RACE.REALM_SORCERY, 0.25f },
            };

            if (rampagingGroup == false)
            {
                g = GroupCreation(l, points, race, null, forbidenTag, false);
            }
            else
            {
                g = GroupCreation(null, l.GetPlane(), l.GetOwnerID(),
                                  points, race, null, forbidenTag, false);
            }

            return g;
        }
        static public object DEF_AbandonKeep(MOM.Location l, int points, bool rampagingGroup)
        {
            Group g;
            Tag[] forbidenTag = new Tag[] {
                (Tag)TAG.CONSTRUCTION_UNIT, (Tag)TAG.SHIP,
                (Tag)TAG.DEBUG_TAG, (Tag)TAG.EVENT_ONLY_UNIT ,
                (Tag)TAG.LAIR_EXCLUSION};

            Dictionary<Race, float> race = new Dictionary<Race, float>()
            {
                { (Race)RACE.REALM_CHAOS, 0.40f },
                { (Race)RACE.REALM_NATURE, 0.20f },
                { (Race)RACE.REALM_SORCERY, 0.40f },
            };

            if (rampagingGroup == false)
            {
                g = GroupCreation(l, points, race, null, forbidenTag, false);
            }
            else
            {
                g = GroupCreation(null, l.GetPlane(), l.GetOwnerID(),
                                  points, race, null, forbidenTag, false);
            }

            return g;
        }
        static public object DEF_TechDefenders(MOM.Location l, int points, bool rampagingGroup)
        {
            Group g;

            if (rampagingGroup == false)
            {
                List<Subrace> subraces = new List<Subrace>() 
                { 
                    (DBDef.Unit)UNIT.SOUL_GOLEM_DUNG,
                    (DBDef.Unit)UNIT.SOUL_CANNON_DUNG,
                    (DBDef.Unit)UNIT.SOUL_BOMBER_DUNG,
                    (DBDef.Unit)UNIT.SOUL_PRIESTS_DUNG,
                    (DBDef.Unit)UNIT.SOUL_CAVALRY_DUNG,
                    (DBDef.Unit)UNIT.SOUL_HALBERDIERS_DUNG,                    
                };
                g = GroupCreation(l, l.GetPlane(), l.GetOwnerID(),points, subraces, false);
            }
            else
            {
                Tag[] forbidenTag = new Tag[] {
                (Tag)TAG.CONSTRUCTION_UNIT, (Tag)TAG.SHIP,
                (Tag)TAG.DEBUG_TAG, (Tag)TAG.EVENT_ONLY_UNIT ,
                (Tag)TAG.LAIR_EXCLUSION};

                Dictionary<Race, float> race = new Dictionary<Race, float>()
                {
                    { (Race)RACE.REALM_TECH, 1.0f },
                };

                g = GroupCreation(null, l.GetPlane(), l.GetOwnerID(),
                                  points, race, null, forbidenTag, false);
            }

            return g;
        }
        static public object DEF_WaterLairs(MOM.Location l, int points, bool rampagingGroup)
        {
            Group g;
            Tag[] forbidenTag = new Tag[] { (Tag)TAG.CONSTRUCTION_UNIT, (Tag)TAG.DEBUG_TAG,
            (Tag)TAG.EVENT_ONLY_UNIT, (Tag)TAG.LAIR_EXCLUSION};

            Dictionary<Race, float> race = new Dictionary<Race, float>()
            {
                { (Race)RACE.REALM_REEF_DUNG, 0.5f },
                { (Race)RACE.REALM_CAVE_DUNG, 0.5f },
            };

            g = GroupCreation(l, points, race, null, forbidenTag, false);

            return g;
        }
        static public object DEF_ReefWaterLairs(MOM.Location l, int points, bool rampagingGroup)
        {
            Group g;
            Tag[] forbidenTag = new Tag[] { (Tag)TAG.CONSTRUCTION_UNIT, (Tag)TAG.DEBUG_TAG,
            (Tag)TAG.EVENT_ONLY_UNIT, (Tag)TAG.LAIR_EXCLUSION};

            Dictionary<Race, float> race = new Dictionary<Race, float>()
            {
                { (Race)RACE.REALM_REEF_DUNG, 1.0f },
            };

            g = GroupCreation(l, points, race, null, forbidenTag, false);

            return g;
        }
        static public object DEF_CaveWaterLairs(MOM.Location l, int points, bool rampagingGroup)
        {
            Group g;
            Tag[] forbidenTag = new Tag[] { (Tag)TAG.CONSTRUCTION_UNIT, (Tag)TAG.DEBUG_TAG,
            (Tag)TAG.EVENT_ONLY_UNIT, (Tag)TAG.LAIR_EXCLUSION};

            Dictionary<Race, float> race = new Dictionary<Race, float>()
            {
                { (Race)RACE.REALM_CAVE_DUNG, 1.0f },
            };

            g = GroupCreation(l, points, race, null, forbidenTag, false);

            return g;
        }
        #endregion
        #region Rampaging Scripts
        static public object RAM_Sorcery(Vector3i pos, WorldCode.Plane p, int points)
        {
            Group g;
            Tag[] forbidenTag = new Tag[] { (Tag)TAG.CONSTRUCTION_UNIT, 
                (Tag)TAG.SHIP, (Tag)TAG.DEBUG_TAG, 
                (Tag)TAG.EVENT_ONLY_UNIT, (Tag)TAG.LAIR_EXCLUSION};
            Dictionary<Race, float> race = new Dictionary<Race, float>()
            {
                { (Race)RACE.REALM_SORCERY, 1f }
            };

            g = GroupCreation(null, p, 0, points, race, null, forbidenTag, false);

            return g;
        }
        static public object RAM_Nature(Vector3i pos, WorldCode.Plane p, int points)
        {
            Group g;
            Tag[] forbidenTag = new Tag[] { (Tag)TAG.CONSTRUCTION_UNIT,
                (Tag)TAG.SHIP, (Tag)TAG.DEBUG_TAG,
                (Tag)TAG.EVENT_ONLY_UNIT, (Tag)TAG.LAIR_EXCLUSION}; 
            Dictionary<Race, float> race = new Dictionary<Race, float>()
            {
                { (Race)RACE.REALM_NATURE, 1f }
            };

            g = GroupCreation(null, p, 0, points, race, null, forbidenTag, false);

            return g;
        }
        static public object RAM_Chaos(Vector3i pos, WorldCode.Plane p, int points)
        {
            Group g;
            Tag[] forbidenTag = new Tag[] { (Tag)TAG.CONSTRUCTION_UNIT,
                (Tag)TAG.SHIP, (Tag)TAG.DEBUG_TAG,
                (Tag)TAG.EVENT_ONLY_UNIT, (Tag)TAG.LAIR_EXCLUSION};
            Dictionary<Race, float> race = new Dictionary<Race, float>()
            {
                { (Race)RACE.REALM_CHAOS, 1f }
            };

            g = GroupCreation(null, p, 0, points, race, null, forbidenTag, false);

            return g;
        }
        static public object RAM_Life(Vector3i pos, WorldCode.Plane p, int points)
        {
            Group g;
            Tag[] forbidenTag = new Tag[] { (Tag)TAG.CONSTRUCTION_UNIT,
                (Tag)TAG.SHIP, (Tag)TAG.DEBUG_TAG,
                (Tag)TAG.EVENT_ONLY_UNIT, (Tag)TAG.LAIR_EXCLUSION};
            Dictionary<Race, float> race = new Dictionary<Race, float>()
            {
                { (Race)RACE.REALM_LIFE, 1f }
            };

            g = GroupCreation(null, p, 0, points, race, null, forbidenTag, false);

            return g;
        }
        static public object RAM_Death(Vector3i pos, WorldCode.Plane p, int points)
        {
            Group g;
            Tag[] forbidenTag = new Tag[] { (Tag)TAG.CONSTRUCTION_UNIT,
                (Tag)TAG.SHIP, (Tag)TAG.DEBUG_TAG,
                (Tag)TAG.EVENT_ONLY_UNIT, (Tag)TAG.LAIR_EXCLUSION};
            Dictionary<Race, float> race = new Dictionary<Race, float>()
            {
                { (Race)RACE.REALM_DEATH, 1f }
            };

            g = GroupCreation(null, p, 0, points, race, null, forbidenTag, false);

            return g;
        }
        static public object RAM_Synergy(Vector3i pos, WorldCode.Plane p, int points)
        {
            Group g;
            Tag[] forbidenTag = new Tag[] { (Tag)TAG.CONSTRUCTION_UNIT,
                (Tag)TAG.SHIP, (Tag)TAG.DEBUG_TAG,
                (Tag)TAG.EVENT_ONLY_UNIT, (Tag)TAG.LAIR_EXCLUSION};
            Dictionary<Race, float> race = new Dictionary<Race, float>()
            {
                { (Race)RACE.REALM_CHAOS, 0.2f },
                { (Race)RACE.REALM_NATURE, 0.2f },
                { (Race)RACE.REALM_DEATH, 0.2f },
                { (Race)RACE.REALM_LIFE, 0.2f },
                { (Race)RACE.REALM_SORCERY, 0.2f }
            };

            g = GroupCreation(null, p, 0, points, race, null, forbidenTag, false);

            return g;
        }
        static public object RAM_Lair(Vector3i pos, WorldCode.Plane p, int points)
        {
            Group g;
            Tag[] forbidenTag = new Tag[] { (Tag)TAG.CONSTRUCTION_UNIT,
                (Tag)TAG.SHIP, (Tag)TAG.DEBUG_TAG,
                (Tag)TAG.EVENT_ONLY_UNIT, (Tag)TAG.LAIR_EXCLUSION};
            Dictionary<Race, float> race = new Dictionary<Race, float>()
            {
                { (Race)RACE.REALM_CHAOS, 0.5f },
                { (Race)RACE.REALM_NATURE, 0.5f },
            };

            g = GroupCreation(null, p, 0, points, race, null, forbidenTag, false);

            return g;
        }
        static public object RAM_AbandonKeep(Vector3i pos, WorldCode.Plane p, int points)
        {
            Group g;
            Tag[] forbidenTag = new Tag[] { (Tag)TAG.CONSTRUCTION_UNIT,
                (Tag)TAG.SHIP, (Tag)TAG.DEBUG_TAG,
                (Tag)TAG.EVENT_ONLY_UNIT, (Tag)TAG.LAIR_EXCLUSION};
            Dictionary<Race, float> race = new Dictionary<Race, float>()
            {
                { (Race)RACE.REALM_CHAOS, 0.40f },
                { (Race)RACE.REALM_NATURE, 0.20f },
                { (Race)RACE.REALM_SORCERY, 0.40f },
            };

            g = GroupCreation(null, p, 0, points, race, null, forbidenTag, false);

            return g;
        }
        static public object RAM_AncientTemple(Vector3i pos, WorldCode.Plane p, int points)
        {
            Group g;
            Tag[] forbidenTag = new Tag[] { (Tag)TAG.CONSTRUCTION_UNIT,
                (Tag)TAG.SHIP, (Tag)TAG.DEBUG_TAG,
                (Tag)TAG.EVENT_ONLY_UNIT, (Tag)TAG.LAIR_EXCLUSION};
            Dictionary<Race, float> race = new Dictionary<Race, float>()
            {
                { (Race)RACE.REALM_LIFE, 0.25f },
                { (Race)RACE.REALM_DEATH, 0.50f },
                { (Race)RACE.REALM_SORCERY, 0.25f },
            };

            g = GroupCreation(null, p, 0, points, race, null, forbidenTag, false);

            return g;
        }
        static public object RAM_Temple(Vector3i pos, WorldCode.Plane p, int points)
        {
            Group g;
            Tag[] forbidenTag = new Tag[] { (Tag)TAG.CONSTRUCTION_UNIT,
                (Tag)TAG.SHIP, (Tag)TAG.DEBUG_TAG,
                (Tag)TAG.EVENT_ONLY_UNIT, (Tag)TAG.LAIR_EXCLUSION};
            Dictionary<Race, float> race = new Dictionary<Race, float>()
            {
                { (Race)RACE.REALM_LIFE, 0.25f },
                { (Race)RACE.REALM_DEATH, 0.75f },
            };

            g = GroupCreation(null, p, 0, points, race, null, forbidenTag, false);

            return g;
        }
        static public object RAM_Tech(Vector3i pos, WorldCode.Plane p, int points)
        {
            Group g;
            
            List<Subrace> subraces = new List<Subrace>()
                {
                    (DBDef.Unit)UNIT.SOUL_GOLEM_DUNG,
                    (DBDef.Unit)UNIT.SOUL_CANNON_DUNG,
                    (DBDef.Unit)UNIT.SOUL_BOMBER_DUNG,
                    (DBDef.Unit)UNIT.SOUL_PRIESTS_DUNG,
                    (DBDef.Unit)UNIT.SOUL_CAVALRY_DUNG,
                    (DBDef.Unit)UNIT.SOUL_HALBERDIERS_DUNG,
                };
            g = GroupCreation(null, p, 0, points, subraces, false);

            return g;
        }
        static public object RAM_WaterLairs(Vector3i pos, WorldCode.Plane p, int points)
        {
            Group g;
            Tag[] optionalTag = new Tag[] { (Tag)TAG.SHIP };
            Tag[] forbidenTag = new Tag[] { (Tag)TAG.CONSTRUCTION_UNIT, (Tag)TAG.DEBUG_TAG,
                (Tag)TAG.EVENT_ONLY_UNIT, (Tag)TAG.LAIR_EXCLUSION};
            Dictionary<Race, float> race = new Dictionary<Race, float>()
            {
                { (Race)RACE.REALM_REEF_DUNG, 0.50f },
                { (Race)RACE.REALM_CAVE_DUNG, 0.50f },
            };

            g = GroupCreation(null, p, 0, points, race, optionalTag, forbidenTag, false);
            return g;
        }
        static public object RAM_ReefWaterLairs(Vector3i pos, WorldCode.Plane p, int points)
        {
            Group g;
            Tag[] forbidenTag = new Tag[] { (Tag)TAG.CONSTRUCTION_UNIT, (Tag)TAG.DEBUG_TAG,
            (Tag)TAG.EVENT_ONLY_UNIT, (Tag)TAG.LAIR_EXCLUSION};

            Dictionary<Race, float> race = new Dictionary<Race, float>()
            {
                { (Race)RACE.REALM_REEF_DUNG, 1.0f },                
            };

            g = GroupCreation(null, p, 0, points, race, null, forbidenTag, false);
            return g;
        }
        static public object RAM_CaveWaterLairs(Vector3i pos, WorldCode.Plane p, int points)
        {
            Group g;
            Tag[] forbidenTag = new Tag[] { (Tag)TAG.CONSTRUCTION_UNIT, (Tag)TAG.DEBUG_TAG,
            (Tag)TAG.EVENT_ONLY_UNIT, (Tag)TAG.LAIR_EXCLUSION};

            Dictionary<Race, float> race = new Dictionary<Race, float>()
            {                
                { (Race)RACE.REALM_CAVE_DUNG, 1.0f },
            };

            g = GroupCreation(null, p, 0, points, race, null, forbidenTag, false);
            return g;
        }
        #endregion


        static public Group EventGroup(DBDef.Group g, int points, WorldCode.Plane plane, int level)
        {
            if(g == null) return null;

            List<DBDef.Subrace> potentialUnits = new List<DBDef.Subrace>();
            List<MOM.Unit> readyUnits = new List<MOM.Unit>();
            var l = PowerEstimate.GetList();
            l = l.FindAll(o => DataBase.GetType<DBDef.Unit>().Contains(o.t0.dbSource.Get() as DBDef.Unit));

            int optimalization = 6;
            List<Multitype<BattleUnit, int>> sourceList = OptimizedWithCriteria(g);

            if (g.Units != null && g.Units.Length > 0)
            {
                foreach (var unit in g.Units)
                {
                    points -= l.Find(o => o.t0.dbSource.Get() == unit).t1;
                    potentialUnits.Add(unit);
                }
            }
            if (g.heroes != null && g.heroes.Length > 0)
            {
                foreach (var hero in g.heroes)
                {
                    points -= l.Find(o => o.t0.dbSource.Get() == hero).t1;
                    potentialUnits.Add(hero);
                }
            }
            if (points > 0 && g.maxHeroesCount > 0 &&
                potentialUnits.Count < 9)
            {
                var heroList = sourceList.FindAll(o => o.t0.GetAttFinal((Tag)TAG.HERO_CLASS) > 0 &&
                o.t0.GetAttFinal((Tag)TAG.FANTASTIC_CLASS) == 0);
                if (heroList == null || heroList.Count == 0)
                {
                    Debug.LogError("EventGroup - heroList list is empty. Need to add TAG.HERO_CLASS or TAG.CHAMPION_CLASS to group?");
                }

                heroList.RandomSort();
                for (int i = 0; potentialUnits.Count <= 9 ; i++)
                {
                    var groupHeroes = g.heroes != null ? g.heroes.Length : 0;
                    var actualHeroCount = groupHeroes + i;
                    if (potentialUnits.Count == 9 || points <= 0 || g.maxHeroesCount == actualHeroCount) break;
                    potentialUnits.Add(heroList[i].t0.dbSource.Get());
                    int cost = heroList[i].t1;
                    points -= cost;
                }
            }

            while (points > 0 && (g.race != null || g.requiredTags != null ||
                g.optionalTags != null || g.forbiddenTags != null) &&
                potentialUnits.Count < 9)
            {
                var unitList = sourceList.FindAll(o => o.t0.GetAttFinal((Tag)TAG.HERO_CLASS) == 0);
                if (unitList.Count == 0) break;
                int share = Random.Range(1, optimalization);
                var list = ReturnByPointsOptimalization(points / share, optimalization, unitList);
                if (list.Count < 1)
                {
                    if (share == 1) break;
                    optimalization = Mathf.Max(1, optimalization - 1);
                    continue;
                }

                int option = Random.Range(0, list.Count);
                int cost = list[option].t1;

                potentialUnits.Add(list[option].t0.dbSource.Get());
                points -= cost;
                if (potentialUnits.Count == 9) break;

                optimalization = Mathf.Max(1, optimalization - 1);
            }

            int readyXpGainers = potentialUnits.FindAll(o => o.gainsXP).Count;
            //Random unit num that recive exp
            int expUnitsNum = new MHRandom().GetInt(1, readyXpGainers + 1);
            //Number of exp for random number of units
            int exp = points > 0 ? points / EditorScripts.OneExpCost() / expUnitsNum : 0;
            potentialUnits.RandomSort();

            foreach (var dbUnit in potentialUnits)
            {
                MOM.Unit unit = MOM.Unit.CreateFrom(dbUnit);
                if (readyXpGainers > 0 || expUnitsNum == 0)
                {
                    LevelUpUnit(exp, ref expUnitsNum, dbUnit, level, ref unit);
                }

                readyUnits.Add(unit);
            }

            Group ng = new Group(plane, 0, true);
            ng.aiNeturalExpedition = new AINeutralExpedition(ng);
            readyUnits.ForEach(o => ng.AddUnit(o));

            return ng;
        }
        static public Group PredefinedGroup(DBDef.Group g, int points, WorldCode.Plane plane, int level)
        {
            if (g == null) return null;
            List<DBDef.Subrace> potentialUnits = new List<DBDef.Subrace>();
            List<MOM.Unit> readyUnits = new List<MOM.Unit>();

            if (g.Units != null && g.Units.Length > 0)
            {
                foreach (var unit in g.Units)
                {
                    potentialUnits.Add(unit);
                }
            }
            if (g.heroes != null && g.heroes.Length > 0)
            {
                foreach (var hero in g.heroes)
                {
                    potentialUnits.Add(hero);
                }
            }

            MOM.Unit u = null;
            foreach(var dbUnit in potentialUnits)
            {
                if(readyUnits.Count < 9)
                {
                    u = MOM.Unit.CreateFrom(dbUnit);
                    if(level > 0)
                    {
                        SimpleLevelUpUnit(dbUnit, level, ref u);
                    }
                    readyUnits.Add(u);
                }
            }

            Group ng = new Group(plane, 0, true);
            readyUnits.ForEach(o => ng.AddUnit(o));
            return ng;
        }
        static public Group GroupCreation(MOM.Location l, int points,
                                          Dictionary<Race, float> race,
                                          Tag[] optionalTags, Tag[] forbiddenTags,
                                          bool powerScalable)
        {
            return GroupCreation(l, l.GetPlane(), l.GetOwnerID(), points, race, optionalTags, forbiddenTags, powerScalable);
        }
        static public Group GroupCreation(MOM.Location host, //optional
                                          WorldCode.Plane plane,
                                          int wizardOwner,
                                          int points,
                                          Dictionary<Race, float> race,
                                          Tag[] optionalTags, Tag[] forbiddenTags,
                                          bool powerScalable)
        {

            List<MOM.Unit> units = new List<MOM.Unit>();
            int optimalization = 6;
            List<Multitype<BattleUnit, int>> sourceList = OptimizedWithCriteria(race , optionalTags, forbiddenTags);
            List<Multitype<BattleUnit, int>> list;

            //First 1-3 units price for 1 unit is max of half the budget
            list = ReturnByPoints(points / 2, optimalization, sourceList);
            if (list.Count > 1)
            {
                int option = Random.Range(0, list.Count);
                int unitCount = Random.Range(1, 4);
                int cost = list[option].t1;
                for (int i = 1; i <= unitCount && points > cost; i++)
                {
                    var unit = list[option].t0.dbSource.Get();
                    units.Add(MOM.Unit.CreateFrom(unit));
                    points -= cost;
                }
            }

            while (true)
            {
                list = ReturnByPoints(points, optimalization, sourceList);
                if (list.Count >= 1)
                {
                    int option = Random.Range(0, list.Count);
                    int unitCount = Random.Range(1, 4);
                    int cost = list[option].t1;
                    if (list.Count == 1 && points == cost) Debug.Log("FAILED SOFTLOCK :P");
                    for (int i = 1; i <= unitCount && points >= cost; i++)
                    {
                        var unit = list[option].t0.dbSource.Get();
                        units.Add(MOM.Unit.CreateFrom(unit));
                        points -= cost;
                    }
                }
                else
                {
                    break;
                }
            }

            while(units.Count > 9)
            {
                //drop cheapest units until unit count is 9
                int value = int.MaxValue;
                MOM.Unit unit = null;
                foreach(var v in units)
                {
                    var k = v.GetWorldUnitValue();
                    if(k < value)
                    {
                        value = k;
                        unit = v;
                    }
                }
#if UNITY_EDITOR
                Debug.Log("Dropped " + unit.dbSource.Get().dbName);
#endif
                units.Remove(unit);
                unit.Destroy();
            }

            MOM.Group ng;
            if (host != null)
            {
                ng = host.GetLocalGroup();
            }
            else
            {
                ng = new Group(plane, wizardOwner);
            }
            units.ForEach(o => ng.AddUnit(o));            

            return ng;
        }
        static public Group GroupCreation(MOM.Location host, //optional
                                          WorldCode.Plane plane,
                                          int wizardOwner,
                                          int points,
                                          List<Subrace> options,
                                          bool powerScalable)
        {

            List<MOM.Unit> units = new List<MOM.Unit>();
            int optimalization = 6;
            List<Multitype<BattleUnit, int>> sourceList = new List<Multitype<BattleUnit, int>>();
            List<Multitype<BattleUnit, int>> powerList = PowerEstimate.GetList();
            powerList = powerList.FindAll(o => DataBase.GetType<DBDef.Unit>().Contains(o.t0.dbSource.Get() as DBDef.Unit));
            foreach (var v in options)
            {
                var u = powerList.Find(o => o.t0.dbSource.dbName == v.dbName);
                if(u != null)
                {
                    sourceList.Add(u);
                }
            }
            if(sourceList.Count == 0)
            {
                return null;
            }

            List<Multitype<BattleUnit, int>> list;

            //First 1-3 units price for 1 unit is max of half the budget
            list = ReturnByPoints(points / 2, optimalization, sourceList);
            if (list.Count > 1)
            {
                int option = Random.Range(0, list.Count);
                int unitCount = Random.Range(1, 4);
                int cost = list[option].t1;
                for (int i = 1; i <= unitCount && points > cost; i++)
                {
                    var unit = list[option].t0.dbSource.Get();
                    units.Add(MOM.Unit.CreateFrom(unit));
                    points -= cost;
                }
            }

            while (true)
            {
                list = ReturnByPoints(points, optimalization, sourceList);
                if (list.Count > 1)
                {
                    int option = Random.Range(0, list.Count);
                    int unitCount = Random.Range(1, 4);
                    int cost = list[option].t1;
                    for (int i = 1; i <= unitCount && points > cost; i++)
                    {
                        var unit = list[option].t0.dbSource.Get();
                        units.Add(MOM.Unit.CreateFrom(unit));
                        points -= cost;
                    }
                }
                else
                {
                    break;
                }
            }

            while (units.Count > 9)
            {
                //drop cheapest units until unit count is 9
                int value = int.MaxValue;
                MOM.Unit unit = null;
                foreach (var v in units)
                {
                    var k = v.GetWorldUnitValue();
                    if (k < value)
                    {
                        value = k;
                        unit = v;
                    }
                }
#if UNITY_EDITOR
                Debug.Log("Dropped " + unit.dbSource.Get().dbName);
#endif
                units.Remove(unit);
                unit.Destroy();
            }

            MOM.Group ng;
            if (host != null)
            {
                ng = host.GetLocalGroup();
            }
            else
            {
                ng = new Group(plane, wizardOwner);
            }
            units.ForEach(o => ng.AddUnit(o));

            return ng;
        }
        static public List<Subrace> GeneralGroup(int points)
        {
            List<DBDef.Subrace> picks = new List<DBDef.Subrace>();
            int optimalization = 6;
            StringBuilder sb = new StringBuilder();
            while (true)
            {
                int share = Random.Range(1, optimalization);
                List<Multitype<BattleUnit, int>> list = OptimizedNoSettlers(points / share, optimalization);
                if (list.Count < 1) break;

                int option = Random.Range(0, list.Count);
                int cost = list[option].t1;

                picks.Add(list[option].t0.dbSource.Get());
                points -= cost;
                if (picks.Count == 9) break;

                optimalization = Mathf.Max(1, optimalization - 1);
                sb.AppendLine(cost + " " + list[option].t0.dbSource);
            }
            //Debug.Log("Created group units " + sb.ToString());
            return picks;
        }
        static public List<Subrace> SuicideGroup(int points)
        {
            List<DBDef.Subrace> picks = new List<DBDef.Subrace>();
            picks.Add((Subrace)DBEnum.UNIT.SOR_PHANTOM_WARRIORS);
            return picks;
        }


        static public List<Subrace> StartingGroup(Race race, PlayerWizard owner = null)
        {
            List<Multitype<BattleUnit, int>> list = PowerEstimate.GetList();
            list = list.FindAll(o => DataBase.GetType<DBDef.Unit>().Contains(o.t0.dbSource.Get() as DBDef.Unit) ||
            DataBase.GetType<DBDef.Hero>().Contains(o.t0.dbSource.Get() as DBDef.Hero));

            List<DBDef.Subrace> picks = new List<DBDef.Subrace>();

            //Adding hero from settings to player
            var hero = SettingsHero(owner, list);
            if (hero != null)
            {
                picks.Add(hero);
            }
            


            if (race != null)
            {
                list = list.FindAll(o => o.t0.race.Get() == race);
            }

            //for development only - since we don't have all units of all races
            if(list.Count < 2)
            {
                return GeneralGroup(1000);
            }

            picks.Add(list.Find(o => o.t0.GetAttFinal(TAG.TOWN_STARTING_DEFENDER) > 0 ).t0.dbSource.Get());

            var InitialEconomy = DifficultySettingsData.GetSetting("UI_INITIAL_ECONOMY");
            if (InitialEconomy.value == "1" || InitialEconomy.value == "2")
            {
                picks.Add(list.Find(o => o.t0.GetAttributes().Contains(TAG.SETTLER_UNIT)).t0.dbSource.Get());
            }


            return picks;
        }
        static public List<Subrace> StartingGroupWithExtraUnit(Race race, PlayerWizard owner, string extraUnits)
        {
            if (owner == null || string.IsNullOrEmpty(extraUnits) ) Debug.LogError("StartingGroupWithExtraUnit owner or extra unit null.");

            List<Multitype<BattleUnit, int>> list = PowerEstimate.GetList();
            list = list.FindAll(o => DataBase.GetType<DBDef.Unit>().Contains(o.t0.dbSource.Get() as DBDef.Unit) || 
            DataBase.GetType<DBDef.Hero>().Contains(o.t0.dbSource.Get() as DBDef.Hero));

            List<DBDef.Subrace> picks = new List<DBDef.Subrace>();

            List<Multitype<BattleUnit, int>> bunits = new List<Multitype<BattleUnit, int>>();
            string[] aExtraUnits = extraUnits.Split(',');
            foreach (var eu in aExtraUnits)
            {
                var b = list.Find(o => o.t0.GetDBName() == eu);
                if (b != null)
                {
                    picks.Add(b.t0.dbSource);
                }
                else
                {
                    BattleUnit wunit = ScriptLibrary.Call(eu, race, owner) as BattleUnit;
                    if (wunit != null)
                    {
                        picks.Add(wunit.dbSource);
                    }
                }
            }

            

            //Adding hero from settings to player
            var hero = SettingsHero(owner, list);
            if (hero != null)
            {
                picks.Add(hero);
            }

            if (race != null)
            {
                list = list.FindAll(o => o.t0.race.Get() == race);
            }

            //for development only - since we don't have all units of all races
            if (list.Count < 2)
            {
                return GeneralGroup(1000);
            }

            picks.Add(list.Find(o => o.t0.GetAttFinal(TAG.TOWN_STARTING_DEFENDER) > 0).t0.dbSource.Get());

            var InitialEconomy = DifficultySettingsData.GetSetting("UI_INITIAL_ECONOMY");
            if (InitialEconomy.value == "1" || InitialEconomy.value == "2")
            {
                picks.Add(list.Find(o => o.t0.GetAttributes().Contains(TAG.SETTLER_UNIT)).t0.dbSource.Get());
            }

            return picks;
        }
        static public List<Subrace> NeutralCityDefenders(Race race, int pops)
        {
            List<Multitype<BattleUnit, int>> list = PowerEstimate.GetList();
            list = list.FindAll(o => DataBase.GetType<DBDef.Unit>().Contains(o.t0.dbSource.Get() as DBDef.Unit));

            if (race != null)
            {
                list = list.FindAll(o => o.t0.race.Get() == race);
            }
            else
            {
                Debug.LogError("Neutral City " + race + " do not find.");
            }

            if (list.Count < 1) Debug.LogError("Neutral City possible defenders count 0.");

            List<DBDef.Subrace> picks = new List<DBDef.Subrace>();

            var startingDefender = list.Find(o => o.t0.GetAttFinal(TAG.TOWN_STARTING_DEFENDER) > 0);
            var pick1 = list.Find(o => o.t0.GetAttFinal(TAG.TOWN_DEFENDER_1) > 0);
            var pick2 = list.Find(o => o.t0.GetAttFinal(TAG.TOWN_DEFENDER_2) > 0);
            var pick3 = list.Find(o => o.t0.GetAttFinal(TAG.TOWN_DEFENDER_3) > 0);
            var pick4 = list.Find(o => o.t0.GetAttFinal(TAG.TOWN_DEFENDER_4) > 0);

            var random = new MHRandom();
            var defendersCount = Mathf.Max(pops - random.GetInt(0, 4), 1);
            defendersCount = Mathf.Min(defendersCount, 9);

            if (defendersCount >= 7)
            {
                if (pick2 == null || pick3 == null || pick4 == null)
                {
                    for (int i = 0; i < defendersCount; i++)
                    {
                        picks.Add(startingDefender.t0.dbSource.Get());
                    }
                    return picks;
                }

                int thirdTierUnits = 2;
                int forthTierUnits = 1;

                for (int i = 0; i < defendersCount - thirdTierUnits - forthTierUnits; i++)
                {
                    picks.Add(pick2.t0.dbSource.Get());
                }
                for (int i = 0; i < thirdTierUnits; i++)
                {
                    picks.Add(pick3.t0.dbSource.Get());
                }
                for (int i = 0; i < forthTierUnits; i++)
                {
                    picks.Add(pick4.t0.dbSource.Get());
                }
            }
            else if(defendersCount >= 5)
            {
                if (pick2 == null || pick3 == null )
                {
                    for (int i = 0; i < defendersCount; i++)
                    {
                        picks.Add(startingDefender.t0.dbSource.Get());
                    }
                    return picks;
                }

                int thirdTierUnits = 1;

                for (int i = 0; i < defendersCount - thirdTierUnits ; i++)
                {
                    picks.Add(pick2.t0.dbSource.Get());
                }
                for (int i = 0; i < thirdTierUnits; i++)
                {
                    picks.Add(pick3.t0.dbSource.Get());
                }
            }
            else if (defendersCount >= 3)
            {
                if (pick1 == null || pick2 == null )
                {
                    for (int i = 0; i < defendersCount; i++)
                    {
                        picks.Add(startingDefender.t0.dbSource.Get());
                    }
                    return picks;
                }

                for (int i = 0; i < defendersCount; i++)
                {
                    picks.Add(pick2.t0.dbSource.Get());
                }
            }
            else
            {
                if (pick1 == null )
                {
                    for (int i = 0; i < defendersCount; i++)
                    {
                        picks.Add(startingDefender.t0.dbSource.Get());
                    }
                    return picks;
                }

                for (int i = 0; i < defendersCount; i++)
                {
                    picks.Add(pick1.t0.dbSource.Get());
                }
            }

            return picks;
        }
        static public List<Subrace> SpellTestGroup()
        {
            var list = new List<Subrace>();
            var sources = DataBase.GetType<DBDef.Unit>();
            Subrace s = DataBase.GetType<DBDef.Hero>()[0];
            list.Add(s);

            s = sources.Find(o => o.naturalHealing);
            list.Add(s);

            s = sources.Find(o => !o.naturalHealing);
            list.Add(s);

            s = sources.Find(o => o.figures > 1);
            list.Add(s);

            s = sources.Find(o => o.figures == 1 && System.Array.Find(o.tags, k => k.tag == (Tag)TAG.CAN_FLY) != null);
            list.Add(s);

            s = sources.FindLast(o => o.gainsXP);
            list.Add(s);

            s = sources.FindLast(o => !o.gainsXP);
            list.Add(s);

            return list;
        }
        #region Utility
        private static void LevelUpUnit(int exp, ref int random, DBDef.Subrace dbUnit, int level, ref MOM.Unit unit)
        {

            if (dbUnit.gainsXP)
            {
                if (dbUnit.race != (Race)RACE.HERO && level > 0)
                {
                    var xpLvl = DataBase.Get<XpToLvl>(XP_TO_LVL.COST_UNIT);
                    if (level < xpLvl.expReq.Length)
                    {
                        unit.xp += xpLvl.expReq[level];
                    }
                    else
                    {
                        unit.xp += xpLvl.expReq[xpLvl.expReq.Length - 1];
                    }
                }
                else if (dbUnit.race == (Race)RACE.HERO && level > 0)
                {
                    var xpLvl = DataBase.Get<XpToLvl>(XP_TO_LVL.COST_HERO);
                    if (level < xpLvl.expReq.Length)
                    {
                        unit.xp += xpLvl.expReq[level];
                    }
                    else
                    {
                        unit.xp += xpLvl.expReq[xpLvl.expReq.Length - 1];
                    }
                }
                else if (level != 0)
                {
                    Debug.LogWarning("EventGroup script/ Battle node - wrong lvl for unit/hero.");
                }

                if (exp > 0)
                {
                    unit.xp += exp;
                    random--;
                }
            }
        }
        private static void SimpleLevelUpUnit(DBDef.Subrace dbUnit, int level, ref MOM.Unit unit)
        {

            if (dbUnit.gainsXP)
            {
                if (dbUnit.race != (Race)RACE.HERO && level > 0)
                {
                    var xpLvl = DataBase.Get<XpToLvl>(XP_TO_LVL.COST_UNIT);
                    if (level < xpLvl.expReq.Length)
                    {
                        unit.xp += xpLvl.expReq[level];
                    }
                    else
                    {
                        unit.xp += xpLvl.expReq[xpLvl.expReq.Length - 1];
                    }
                }
                else if (dbUnit.race == (Race)RACE.HERO && level > 0)
                {
                    var xpLvl = DataBase.Get<XpToLvl>(XP_TO_LVL.COST_HERO);
                    if (level < xpLvl.expReq.Length)
                    {
                        unit.xp += xpLvl.expReq[level];
                    }
                    else
                    {
                        unit.xp += xpLvl.expReq[xpLvl.expReq.Length - 1];
                    }
                }
                else if (level != 0)
                {
                    Debug.LogWarning("EventGroup script/ Battle node - wrong lvl for unit/hero.");
                }
            }
        }
        private static Subrace SettingsHero(PlayerWizard owner, List<Multitype<BattleUnit, int>> list)
        {
            var heroDbName = DifficultySettingsData.GetSetting("UI_ADD_HERO").value;
            var hero  = list.Find(o => o.t0.GetDBName() == heroDbName);
            if (owner != null && GameManager.GetHumanWizard() == owner &&
                hero != null)
            {
                return hero.t0.dbSource.Get();
            }
            return null;
        }



        #endregion

    }
}
#endif