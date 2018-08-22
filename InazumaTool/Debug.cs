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
        
        public static void LogVector(MVector vec,string tag)
        {
            Log(string.Format("{0}:({1},{2},{3})", tag, vec.x, vec.y, vec.z));
        }

        public static void LogVector(MPoint point, string tag)
        {
            Log(string.Format("{0}:({1},{2},{3},{4})", tag, point.x, point.y, point.z,point.w));
        }

        public static void LogEuler(MEulerRotation euler, string tag)
        {
            Log(string.Format("{0}:({1},{2},{3})", tag, euler.x, euler.y, euler.z));
        }
    }
}
