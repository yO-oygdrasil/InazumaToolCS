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
        /// <summary>
        /// return Dynamic Output Curve
        /// </summary>
        /// <param name="curveDagPath">curve DagPath</param>
        /// <returns></returns>
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
                    MGlobal.executeCommand("convertHairSelection \"current\"");
                    return BasicFunc.GetSelectedDagPath(0);
                }
            }
            //error
            return curveDagPath;
        }

        public static void AddDynamicChainControl(MSelectionList jointChains = null)
        {
            //get bones
            if (jointChains == null)
            {
                jointChains = BasicFunc.GetSelectedList();
            }
            if (jointChains.length == 0)
            {
                return;
            }
            else if (jointChains.length == 1)
            {
                BasicFunc.Select(jointChains);
                MDagPath dagPath_first = new MDagPath();
                jointChains.getDagPath((uint)0, dagPath_first);
                MGlobal.executeCommand("select -hierarchy " + dagPath_first.fullPathName);
                for (uint i = jointChains.length - 1; i >= 0; i--)
                {
                    
                }

            }
        }


        public enum HairSelectionType
        {
            Follicles,
            HairSystem,
            StartCurves,
            OutputCurves
        }

        

        public static void ConvertHairSelection(MDagPath dagPath)
        {
            //MGlobal.executeCommand("convertHairSelection \"current\"");

            MPlug plug = new MPlug();

            


        }


        const string cmdStr = "DynamicConverter";
        public static List<CommandData> GetCommandDatas()
        {
            List<CommandData> cmdList = new List<CommandData>();
            cmdList.Add(new CommandData("动力学", cmdStr, "curveToHair", "为链骨增加动力学", () =>
            {
                CurveToHair();
            }));
            return cmdList;
        }

    }


}
