/**********************************
 *
 * Author:  Dorian Gray
 * Date:    Feb 23 2024
 * Version: 1.0.1
 *
 **********************************/

#if !USE_DEBUG_SCRIPT || !UNITY_EDITOR

using DBDef;
using DBEnum;
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

        [ScriptType(ScriptType.Type.EnchantmentActivatorIntScript)]
        public static int ECHIAddProductionFromForestMP(IEnchantable target, EnchantmentScript es, EnchantmentInstance ei, int value)
        {
            int cityRange = 2;
            WorldCode.Hex hex;
            IPlanePosition tl = target as IPlanePosition;

            FInt forestProd = FInt.ZERO;

            WorldCode.Plane plane = tl.GetPlane();
            List<Vector3i> positions = tl.GetSurroundingArea(cityRange);

            foreach (Vector3i pos in positions)
            {
                hex = plane.GetHexAt(pos);
                if (hex != null && hex.GetTerrain().terrainType == ETerrainType.Forest)
                {
                    forestProd += hex.GetProduction();
                }
            }

            forestProd = (value * forestProd).ReturnRounded();

            return value + forestProd.ToInt();
        }


    }
}
#endif