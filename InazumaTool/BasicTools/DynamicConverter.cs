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
        /// <param name="curveDagPath"></param>
        /// <param name="pointLock">0-none, 1-base, 2-end,3-both</param>
        /// <returns></returns>
        public static MDagPath[] CurveToHair(MSelectionList curveList = null, int pointLock = 1)
        {
            if (curveList == null)
            {
                curveList = BasicFunc.GetSelectedList();
            }
            MGlobal.setActiveSelectionList(curveList);
            
            string cmdStr = "cmds.MakeCurvesDynamic(0,0,0,1,0)";
            string resultStr = MGlobal.executePythonCommandStringResult(cmdStr);

            List<MDagPath> results = new List<MDagPath>();

            for (int i = 0; i < curveList.length; i++)
            {
                MDagPath curveDagPath = new MDagPath();
                curveList.getDagPath((uint)i, curveDagPath);

                //MGlobal.displayInfo(curveDagPath.fullPathName);
                MFnTransform curveTrans = new MFnTransform(curveDagPath);
                if (curveTrans.parentCount > 0)
                {
                    MDagPath follicleDagPath = MDagPath.getAPathTo(curveTrans.parent(0));
                    MGlobal.executeCommand(string.Format("setAttr {0}.pointLock {1}", follicleDagPath.fullPathName, pointLock));
                    if (follicleDagPath.hasFn(MFn.Type.kFollicle))
                    {
                        MGlobal.displayInfo("follicle exist!");
                        BasicFunc.Select(follicleDagPath);
                        MGlobal.executeCommand("convertHairSelection \"current\"");
                        results.Add(BasicFunc.GetSelectedDagPath(0));
                    }
                }
            }

            
            //error
            return results.ToArray();
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

            MDagPath dagPath_startJoint = new MDagPath(),dagPath_endJoint = new MDagPath();
            if (jointChains.length == 1)
            {
                BasicFunc.Select(jointChains);
                jointChains.getDagPath((uint)0, dagPath_startJoint);
                MGlobal.executeCommand("select -hierarchy " + dagPath_startJoint.fullPathName);
                jointChains = BasicFunc.GetSelectedList(MFn.Type.kJoint);
                //BasicFunc.PrintDags(jointChains);
                //bool endJointFound = false;
                //for (int i = (int)(jointChains.length - 1); i >= 0; i--)
                //{
                //    MGlobal.displayInfo("jointChains[" + i + "]");
                //    MObject selectedObj = new MObject();
                //    jointChains.getDependNode((uint)i, selectedObj);
                //    if (!selectedObj.hasFn(MFn.Type.kJoint))
                //    {
                //        jointChains.remove((uint)i);
                //    }
                //    else if (!endJointFound)
                //    {
                //        endJointFound = true;
                //        jointChains.getDagPath((uint)i, dagPath_endJoint);
                //    }
                //}
                //BasicFunc.Select(jointChains);
            }


            jointChains.getDagPath(0, dagPath_startJoint);
            jointChains.getDagPath(jointChains.length - 1, dagPath_endJoint);

            MDagPath startCurveDagPath = JointProcess.CreateJointsCurve(jointChains);
            MDagPath outCurveDagPath = CurveToHair(startCurveDagPath);

            JointProcess.AddIKHandle(dagPath_startJoint, dagPath_endJoint, JointProcess.IKSolverType.Spline, outCurveDagPath.fullPathName);
        }

        public static void AddDynamicChainControlPerChain(MSelectionList rootJointsList = null)
        {
            if (rootJointsList == null)
            {
                rootJointsList = BasicFunc.GetSelectedList();
            }
            MDagPath oneJoint = new MDagPath();
            for (int i = 0; i < rootJointsList.length; i++)
            {
                rootJointsList.getDagPath((uint)i, oneJoint);
                MSelectionList tempList = new MSelectionList();
                tempList.add(oneJoint);
                AddDynamicChainControl(tempList);
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
            BasicFunc.Select(dagPath);
            MGlobal.executeCommand("convertHairSelection \"hairSystem\"");
            
            


        }


        const string cmdStr = "DynamicConverter";
        public static List<CommandData> GetCommandDatas()
        {
            List<CommandData> cmdList = new List<CommandData>();
            cmdList.Add(new CommandData("动力学", cmdStr, "curveToHair", "曲线转头发 测试", () =>
            {
                CurveToHair();
            }));
            cmdList.Add(new CommandData("动力学", cmdStr, "jointChainToHair", "为链骨增加动力学", () =>
            {
                AddDynamicChainControl();
            }));
            cmdList.Add(new CommandData("动力学", cmdStr, "multiJointChainsToHair", "为链骨们增加动力学", () =>
            {
                AddDynamicChainControlPerChain();
            }));
            return cmdList;
        }

    }


}
