using System;
using System.Collections.Generic;

using Autodesk.Maya.OpenMaya;

namespace InazumaTool
{
    public static class Debug
    {
        public static void Log(string msg)
        {
            MGlobal.displayInfo(msg);
        }
    }
}
