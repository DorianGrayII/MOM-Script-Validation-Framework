#if !USE_DEBUG_SCRIPT || !UNITY_EDITOR
﻿using UnityEngine;
using System.Collections.Generic;
using DBDef;
using MHUtils;

namespace GameScript
{
    public class Scripts : ScriptBase
    {
        static public object TestScript(string a)
        {
            Debug.Log("TestScript(string) produced:\n" + a);
            return null;
        }
        
    }
}
#endif