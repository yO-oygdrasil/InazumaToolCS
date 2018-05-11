using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Maya.OpenMaya;
using Autodesk.Maya.OpenMayaFX;
using Autodesk.Maya.OpenMayaAnim;
using Autodesk.Maya.OpenMayaUI;
using Autodesk.Maya.OpenMayaRender;


namespace InazumaTool.BasicTools
{
    public static class DynamicConverter
    {
        
        public static MDagPath CurveToHair(MDagPath curveDagPath = null)
        {
            if (curveDagPath != null)
            {
                MSelectionList list = new MSelectionList();
                list.add(curveDagPath);
                MGlobal.setActiveSelectionList(list);
            }
            else
            {
                curveDagPath = BasicFunc.GetSelectedDagPath(0);
            }
            string cmdStr = "cmds.MakeCurvesDynamic(0,0,0,1,0)";
            string resultStr = MGlobal.executePythonCommandStringResult(cmdStr);

            MGlobal.displayInfo(curveDagPath.fullPathName);
            MFnTransform curveTrans = new MFnTransform(curveDagPath);
            if (curveTrans.parentCount > 0)
            {
                MDagPath follicleDagPath = MDagPath.getAPathTo(curveTrans.parent(0));
                if (follicleDagPath.hasFn(MFn.Type.kFollicle))
                {
                    MGlobal.displayInfo("follicle exist!");
                    BasicFunc.Select(follicleDagPath);




                }
            }


            //error
            return curveDagPath;
        }

        public enum HairSelectionType
        {
            Follicles,
            HairSystem,
            StartCurves,
            OutputCurves
        }

        

        public static void ConvertHairSelection()
        {
            //MGlobal.executeCommand("convertHairSelection \"current\"");

            MPlug plug = new MPlug();

            //plug.destinations


        }
    }


}
