/**********************************
 *
 * Author:  Dorian Gray
 * Date:    May 13, 2024
 * Version: 1.0.2
 *
 **********************************/

#if !USE_DEBUG_SCRIPT || !UNITY_EDITOR

using DBDef;
using MHUtils;
using MOM;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WorldCode;

namespace MOMScripts_vtv
{
    public class EnchantmentScripts : ScriptBase
    {
        /// <summary>
        /// This function takes an input value and multiplies by the summation of the forest production
        /// values + 1 within 2 hexes of target hex.
        /// </summary>
        /// <param name="target">as IPlanePosition</param>
        /// <param name="es">ignored</param>
        /// <param name="ei">ignored</param>
        /// <param name="iValue">input base production</param>
        /// <returns>int containing the adjusted value by forest production values within 2 hex range of target</returns>
        [ScriptType(ScriptType.Type.EnchantmentActivatorIntScript)]
        public static int ECHIAddProductionFromForestMP(IEnchantable target, EnchantmentScript es, EnchantmentInstance ei, int iValue)
        {
            int cityRange = 2;
            IPlanePosition tl = target as IPlanePosition;
            if (tl == null)
            {
                Debug.LogError("ECHIAddProductionFromForestMP target is not IPlanePosition type!");
                return iValue;
            }

            float fForestProd = 0.0f;

            WorldCode.Plane plane = tl.GetPlane();
            List<Vector3i> posnList = tl.GetSurroundingArea(cityRange);

            foreach (Vector3i posn in posnList)
            {
                WorldCode.Hex hex = plane.GetHexAt(posn);
                if (hex != null && hex.GetTerrain().terrainType == ETerrainType.Forest)
                {  // accumulate the production values of forest hexes
                    fForestProd += hex.GetProduction().ToFloat();
                }
            }

//          fForestProd = (iValue * fForestProd).ReturnRounded();
//          return iValue + fForestProd.ToInt();

            // round value to the nearest integer
            return (int)Math.Round(iValue * (fForestProd + 1));
        }

            /*
             * Regarding previous changes....
             * 
             * consider int iValue = 12
             * consider FInt fForestProd = FInt(0.85) // internally stored as 85
             *          
             *          fForestProd = (iValue * fForestProd).ReturnRounded();
             *                      = (12 * FInt(0.85)).ReturnRounded();
             *                      = (FInt.operator*(12, FInt(0.85)).ReturnRounded();
             *                      = FInt((float)12 * FInt(0.85).ToFloat()).ReturnRounded();
             *                      = FInt((float)12 * (float)0.85).ReturnRounded();
             *                      = Fint(10.2).ReturnRounded();
             *                      = FInt((1020 + 50) / 100);
             *                      = FInt(10.7);
             *                      
             *          return (int)(iValue + fForestProd(10.7).ToInt());
             *                 (int)(12 + (10.7 * 100 / 100));
             *                 (int)(12 + 10.7);
             *                 (int)22.7;
             *                 22;
             *
             * seriously?
             *================================================================
             * now consider int iValue = 12
             * now consider float fForestProd = 0.85
             * 
             *             return (int)Math.Round(iValue * (fForestProd + 1));
             *                    (int)Math.Round(12 * (0.85 + 1));
             *                    (int)Math.Round(12 * 1.85);
             *                    (int)Math.Round(22.2);
             *                    (int)22.0;
             *                    22;
             *
             * conclusion - FInt types contain way too much overhead and should be avoided
             *              when possible.
             */
    }
}
#endif