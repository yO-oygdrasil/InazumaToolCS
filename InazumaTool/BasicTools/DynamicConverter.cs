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


        #region HairBone

        /// <summary>
        /// return Dynamic Output Curve
        /// </summary>
        /// <param name="hairSystemDagPath">exist hairsystem or container of created hairsystem</param>
        /// <param name="curveList"></param>
        /// <param name="pointLock">0-none, 1-base, 2-end,3-both</param>
        /// <returns></returns>
        public static MDagPath[] CurvesToHairs(ref MDagPath hairSystemDagPath, MSelectionList curveList = null, int pointLock = 1)
        {
            if (curveList == null)
            {
                curveList = BasicFunc.GetSelectedList();
            }
            MGlobal.setActiveSelectionList(curveList);

            bool hairSystemReady = true;
            if (!hairSystemDagPath.node.isNull)
            {
                curveList.add(hairSystemDagPath);
                hairSystemReady = false;
            }
            string cmdStr = "cmds.MakeCurvesDynamic(0,0,0,1,0)";
            string resultStr = MGlobal.executePythonCommandStringResult(cmdStr);

            if (hairSystemReady)
            {
                curveList.remove(curveList.length - 1);
            }
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
                        ConvertHairSelection(HairSelectionType.OutputCurves, follicleDagPath);
                        MDagPath result = BasicFunc.GetSelectedDagPath(0);
                        new MFnDependencyNode(result.node).setName("dy_" + curveDagPath.partialPathName);
                        results.Add(result);
                        if (!hairSystemReady)
                        {
                            ConvertHairSelection(HairSelectionType.HairSystem);
                            hairSystemDagPath = BasicFunc.GetSelectedDagPath(0);
                            hairSystemReady = true;
                        }
                    }
                }
            }


            //error
            return results.ToArray();
        }

        public static MDagPath CurveToHair(ref MDagPath hairSystemDagPath, MDagPath curveDagPath = null, int pointLock = 1)
        {
            if (curveDagPath == null)
            {
                curveDagPath = BasicFunc.GetSelectedDagPath(0);
            }
            bool hairSystemReady = !hairSystemDagPath.node.isNull;
            MSelectionList targetList = new MSelectionList();
            targetList.add(curveDagPath);
            if (hairSystemReady)
            {
                MGlobal.displayInfo("hair system ready");
                targetList.add(hairSystemDagPath);
            }
            else
            {
                MGlobal.displayInfo("hair system need to be created!");
            }
            BasicFunc.Select(targetList);


            string cmdStr = "cmds.MakeCurvesDynamic(0,0,0,1,0)";
            string resultStr = MGlobal.executePythonCommandStringResult(cmdStr);

            MDagPath result = new MDagPath();

            //MGlobal.displayInfo(curveDagPath.fullPathName);
            MFnTransform curveTrans = new MFnTransform(curveDagPath);
            if (curveTrans.parentCount > 0)
            {
                MDagPath follicleDagPath = MDagPath.getAPathTo(curveTrans.parent(0));
                MGlobal.executeCommand(string.Format("setAttr {0}.pointLock {1}", follicleDagPath.fullPathName, pointLock));
                if (follicleDagPath.hasFn(MFn.Type.kFollicle))
                {
                    MGlobal.displayInfo("follicle exist!");
                    ConvertHairSelection(HairSelectionType.OutputCurves, follicleDagPath);
                    result = BasicFunc.GetSelectedDagPath(0);
                    new MFnDependencyNode(result.node).setName("dy_" + curveDagPath.partialPathName);
                    if (!hairSystemReady)
                    {
                        ConvertHairSelection(HairSelectionType.HairSystem);
                        hairSystemDagPath = BasicFunc.GetSelectedDagPath(0);
                        hairSystemReady = true;
                    }
                }
            }

            return result;
        }


        public static void AddDynamicChainControl(ref MDagPath hairSystem, MSelectionList jointChains = null)
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

            MDagPath dagPath_startJoint = new MDagPath(), dagPath_endJoint = new MDagPath();
            if (jointChains.length == 1)
            {
                BasicFunc.Select(jointChains);
                jointChains.getDagPath((uint)0, dagPath_startJoint);
                MGlobal.executeCommand("select -hierarchy " + dagPath_startJoint.fullPathName);
                jointChains = BasicFunc.GetSelectedList(MFn.Type.kJoint);
            }


            jointChains.getDagPath(0, dagPath_startJoint);
            jointChains.getDagPath(jointChains.length - 1, dagPath_endJoint);

            MDagPath startCurveDagPath = JointProcess.CreateJointsCurve(jointChains);
            MDagPath outCurveDagPath = CurveToHair(ref hairSystem, startCurveDagPath);

            JointProcess.AddIKHandle(dagPath_startJoint, dagPath_endJoint, JointProcess.IKSolverType.Spline, outCurveDagPath.fullPathName);
        }

        public static void AddDynamicChainControlPerChain(MSelectionList rootJointsList = null)
        {
            if (rootJointsList == null)
            {
                rootJointsList = BasicFunc.GetSelectedList();
            }
            MDagPath oneJoint = new MDagPath();
            MDagPath hairSystem = new MDagPath();
            for (int i = 0; i < rootJointsList.length; i++)
            {
                rootJointsList.getDagPath((uint)i, oneJoint);
                MSelectionList tempList = new MSelectionList();
                tempList.add(oneJoint);
                AddDynamicChainControl(ref hairSystem, tempList);
            }
        }


        public enum HairSelectionType
        {
            Follicles,
            HairSystem,
            StartCurves,
            OutputCurves
        }



        public static void ConvertHairSelection(HairSelectionType hairSelectionType, MDagPath dagPath = null)
        {
            //MGlobal.executeCommand("convertHairSelection \"current\"");
            if (dagPath != null)
            {
                BasicFunc.Select(dagPath);
            }
            switch (hairSelectionType)
            {
                case HairSelectionType.Follicles:
                    {
                        MGlobal.executeCommand("convertHairSelection \"follicles\"");
                        break;
                    }
                case HairSelectionType.HairSystem:
                    {
                        MGlobal.executeCommand("convertHairSelection \"hairSystems\"");
                        break;
                    }
                case HairSelectionType.OutputCurves:
                    {
                        MGlobal.executeCommand("convertHairSelection \"current\"");
                        break;
                    }
                case HairSelectionType.StartCurves:
                    {
                        MGlobal.executeCommand("convertHairSelection \"startCurves\"");
                        break;
                    }
            }




        }

        #endregion

        #region ProxyModel
        public enum ProxyBaseShape
        {
            Box,
            Tube,
            Sphere
        }

        public static MFnMesh CreateVertLoopCircle(List<MVector> verts, bool reOrder = true)
        {
            int count = verts.Count;
            float[] radians = new float[count];
            for (int i = 0; i < count; i++)
            {
                radians[i] = BasicFunc.CalPosRadian(verts[i]);
            }


        }


        public static void CreateProxyModel(MSelectionList selected = null, ProxyBaseShape baseShape = ProxyBaseShape.Tube)
        {
            if (selected == null)
            {
                //
                selected = BasicFunc.GetSelectedList();
            }

            List<MVector> positions = new List<MVector>();
            MItSelectionList it_selectionList = new MItSelectionList(selected);
            MVector totalWeight = MVector.zero;
            for (; !it_selectionList.isDone; it_selectionList.next())
            {
                MObject component = new MObject();
                MDagPath item = new MDagPath();
                it_selectionList.getDagPath(item, component);




                MItMeshVertex it_verts = new MItMeshVertex(item, component);
                for (; !it_verts.isDone; it_verts.next())
                {
                    MPoint point = it_verts.position(MSpace.Space.kWorld);
                    MVector pos = new MVector(point.x, point.y, point.z);
                    BasicFunc.CreateLocator(pos, "vert_" + it_verts.index());
                    positions.Add(pos);
                    totalWeight += pos;
                }

            }
            //merge the nearest
            float rangeSize = 1;

            
            
            //MVector center = totalWeight * (1.0f / positions.Count);
            MFnMesh newMesh = new MFnMesh();



        }



        #endregion


        const string cmdStr = "DynamicConverter";
        public static List<CommandData> GetCommandDatas()
        {
            List<CommandData> cmdList = new List<CommandData>();
            cmdList.Add(new CommandData("动力学", cmdStr, "curveToHair", "曲线转头发 测试", () =>
            {
                MDagPath hairSystem = new MDagPath();
                CurveToHair(ref hairSystem);
            }));
            cmdList.Add(new CommandData("动力学", cmdStr, "multiJointChainsToHair", "为链骨们增加动力学", () =>
            {
                AddDynamicChainControlPerChain();
            }));
            cmdList.Add(new CommandData("动力学", cmdStr, "createProxyModel", "创建代理", () =>
            {
                CreateProxyModel();
            }));
            return cmdList;
        }

    }


}
