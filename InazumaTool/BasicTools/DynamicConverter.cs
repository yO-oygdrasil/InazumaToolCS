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
        public static MDagPath[] CurvesToHairs(ref MDagPath hairSystemDagPath, MSelectionList curveList = null, ConstantValue.HairPointLockType pointLock = ConstantValue.HairPointLockType.Base)
        {
            if (curveList == null)
            {
                curveList = BasicFunc.GetSelectedList();
            }

            bool hairSystemReady = false;
            if (!hairSystemDagPath.node.isNull)
            {
                //Debug.Log("hairSystem node is not null:" + hairSystemDagPath.fullPathName);
                curveList.add(hairSystemDagPath);
                hairSystemReady = true;
            }
            else
            {
                //Debug.Log("hairsystem node is null");
            }
            MGlobal.setActiveSelectionList(curveList);
            string cmdStr = "cmds.MakeCurvesDynamic(0,0,0,1,0)";
            string resultStr = MGlobal.executePythonCommandStringResult(cmdStr);

            if (hairSystemReady)
            {
                //Debug.Log("remove hairSystem Selection");
                curveList.remove(curveList.length - 1);
            }
            List<MDagPath> results = new List<MDagPath>();

            for (int i = 0; i < curveList.length; i++)
            {
                MDagPath curveDagPath = new MDagPath();
                curveList.getDagPath((uint)i, curveDagPath);

                //Debug.Log(curveDagPath.fullPathName);
                MFnTransform curveTrans = new MFnTransform(curveDagPath);
                if (curveTrans.parentCount > 0)
                {
                    MDagPath follicleDagPath = MDagPath.getAPathTo(curveTrans.parent(0));
                    MGlobal.executeCommand(string.Format("setAttr {0}.pointLock {1}", follicleDagPath.fullPathName, (int)pointLock));
                    if (follicleDagPath.hasFn(MFn.Type.kFollicle))
                    {
                        Debug.Log("follicle exist!");
                        ConvertHairSelection(HairSelectionType.OutputCurves, follicleDagPath);
                        MDagPath result = BasicFunc.GetSelectedDagPath(0);
                        new MFnDependencyNode(result.node).setName("dy_" + curveDagPath.partialPathName);
                        results.Add(result);
                        if (!hairSystemReady)
                        {
                            ConvertHairSelection(HairSelectionType.HairSystem);
                            hairSystemDagPath = BasicFunc.GetSelectedDagPath(0);
                            //Debug.Log("assign hairSystem:" + hairSystemDagPath.fullPathName);
                            hairSystemReady = true;
                        }
                    }
                }
            }


            //error
            return results.ToArray();
        }

        public static MDagPath CurveToHair(ref MDagPath hairSystemDagPath, MDagPath curveDagPath = null, ConstantValue.HairPointLockType pointLock = ConstantValue.HairPointLockType.Base)
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
                Debug.Log("hair system ready");
                targetList.add(hairSystemDagPath);
            }
            else
            {
                Debug.Log("hair system need to be created!");
            }
            BasicFunc.Select(targetList);


            string cmdStr = "cmds.MakeCurvesDynamic(0,0,0,1,0)";
            string resultStr = MGlobal.executePythonCommandStringResult(cmdStr);

            MDagPath result = new MDagPath();

            //Debug.Log(curveDagPath.fullPathName);
            MFnTransform curveTrans = new MFnTransform(curveDagPath);
            if (curveTrans.parentCount > 0)
            {
                MDagPath follicleDagPath = MDagPath.getAPathTo(curveTrans.parent(0));
                MGlobal.executeCommand(string.Format("setAttr {0}.pointLock {1}", follicleDagPath.fullPathName, (int)pointLock));
                if (follicleDagPath.hasFn(MFn.Type.kFollicle))
                {
                    Debug.Log("follicle exist!");
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


        public static MDagPath AddDynamicChainControl(ref MDagPath hairSystem, MSelectionList jointChains = null, ConstantValue.HairPointLockType pointLock = ConstantValue.HairPointLockType.Base)
        {
            //get bones
            if (jointChains == null)
            {
                jointChains = BasicFunc.GetSelectedList();
            }
            if (jointChains.length == 0)
            {
                return null;
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
            MDagPath outCurveDagPath = CurveToHair(ref hairSystem, startCurveDagPath, pointLock);

            JointProcess.AddIKHandle(dagPath_startJoint, dagPath_endJoint, JointProcess.IKSolverType.Spline, outCurveDagPath.fullPathName);

            BasicFunc.SetTransformParent(BasicFunc.GetParent(startCurveDagPath), BasicFunc.GetParent(dagPath_startJoint));

            return outCurveDagPath;
        }

        public static List<MDagPath> AddDynamicChainControlPerChain(MSelectionList rootJointsList = null)
        {
            if (rootJointsList == null)
            {
                rootJointsList = BasicFunc.GetSelectedList();
            }
            MDagPath oneJoint = new MDagPath();
            MDagPath hairSystem = new MDagPath();
            List<MDagPath> result = new List<MDagPath>();
            for (int i = 0; i < rootJointsList.length; i++)
            {
                rootJointsList.getDagPath((uint)i, oneJoint);
                MSelectionList tempList = new MSelectionList();
                tempList.add(oneJoint);
                AddDynamicChainControl(ref hairSystem, tempList);
            }
            return result;
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

        public static MDagPath CreateLoopCircleByPos(List<MVector> posList, bool reOrder = true, bool closedArc = true, string ctlName = "loopCircle_0")
        {
            List<MVector> vectors = new List<MVector>(posList);
            if (vectors.Count < 2)
            {
                return default(MDagPath);
            }
            if (reOrder)
            {
                int count = vectors.Count;
                //List<float> radians = new List<float>();
                Dictionary<MVector, float> radianDic = new Dictionary<MVector, float>();
                for (int i = 0; i < count; i++)
                {
                    radianDic.Add(vectors[i], BasicFunc.CalPosRadian(vectors[i]));
                }
                vectors.Sort((a, b) =>
                {
                    if (radianDic[a] > radianDic[b])
                    {
                        return 1;
                    }
                    else
                    {
                        return -1;
                    }
                });
            }
            if (closedArc)
            {
                vectors.Add(vectors[0]);
            }
            return BasicFunc.CreateCurve(vectors.ToArray(), ctlName, 1, closedArc ? MFnNurbsCurve.Form.kClosed : MFnNurbsCurve.Form.kOpen);
        }
        
        public static void CreateRelativeCurve(MSelectionList selected = null, ConstantValue.SampleType st = ConstantValue.SampleType.ObjectTrans, bool reOrder = true, bool closedArc = true,string ctlName = null)
        {
            if (selected == null)
            {
                selected = BasicFunc.GetSelectedList();
            }
            List<MVector> positions = new List<MVector>();
            switch (st)
            {
                case ConstantValue.SampleType.Vert:
                    {
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
                                //Debug.Log(it_verts.index().ToString());
                                MPoint point = it_verts.position(MSpace.Space.kWorld);
                                MVector pos = new MVector(point.x, point.y, point.z);
                                //BasicFunc.CreateLocator(pos, "vert_" + it_verts.index());
                                positions.Add(pos);
                                totalWeight += pos;
                            }

                        }
                        break;
                    }
                case ConstantValue.SampleType.Edge:
                    {

                        break;
                    }
                case ConstantValue.SampleType.Poly:
                    {

                        break;
                    }
                case ConstantValue.SampleType.ObjectTrans:
                    {
                        foreach (MDagPath dag in selected.DagPaths())
                        {
                            MFnTransform trans = new MFnTransform(dag);
                            positions.Add(trans.getTranslation(MSpace.Space.kWorld));
                        }
                        break;
                    }
            }
            if (ctlName == null)
            {
                ctlName = "samplerCurve_00";
            }
            CreateLoopCircleByPos(positions, reOrder, closedArc, ctlName);

        }

        public static void CreateSpiderNetCurves(MSelectionList selected = null, bool convertToHair = true, bool addConstraint = true, bool bindJoints = true)
        {
            if (selected == null)
            {
                selected = BasicFunc.GetSelectedList();
            }
            List<MDagPath> verticalCurves = new List<MDagPath>(), horizontalCurves = new List<MDagPath>();
            List<List<MVector>> colList = new List<List<MVector>>();

            List<MDagPath[]> jointPairToBind = new List<MDagPath[]>();
            
            int rowCount = 0;
            foreach (MDagPath dag in selected.DagPaths())
            {
                List<MFnTransform> transList = BasicFunc.GetHierachyChainTrans(dag);
                List<MVector> vectorList = new List<MVector>();
                if (transList.Count > rowCount)
                {
                    rowCount = transList.Count;
                }
                for (int i = 0; i < transList.Count; i++)
                {
                    vectorList.Add(transList[i].getTranslation(MSpace.Space.kWorld));
                }
                if (bindJoints)
                {
                    jointPairToBind.Add(new MDagPath[2] { transList[0].dagPath, transList[transList.Count - 1].dagPath });
                }
                colList.Add(vectorList);
                verticalCurves.Add(CreateLoopCircleByPos(vectorList, false, false, string.Format("netCurve_column_{0:d4}", (colList.Count - 1))));
            }
            List<List<MVector>> rowList = new List<List<MVector>>();
            for (int i = 0; i < rowCount; i++)
            {
                List<MVector> rowCircle = new List<MVector>();
                for (int j = 0; j < colList.Count; j++)
                {
                    rowCircle.Add(colList[j][i]);
                }
                rowList.Add(rowCircle);
                horizontalCurves.Add(CreateLoopCircleByPos(rowCircle, false, true, string.Format("netCurve_row_{0:d4}", (rowCircle.Count - 1))));
            }
            if (convertToHair)
            {
                MDagPath hairSystem = new MDagPath();
                MSelectionList listToDynamicCurve = new MSelectionList();
                Debug.Log("vertical to dynamic count:" + verticalCurves.Count);
                for (int i = 0; i < verticalCurves.Count; i++)
                {
                    listToDynamicCurve.add(verticalCurves[i]);
                }
                MDagPath[] verticalOutputCurves = CurvesToHairs(ref hairSystem, listToDynamicCurve, ConstantValue.HairPointLockType.Base);
                Debug.Log("vertical dynamic result count:" + verticalOutputCurves.Length);
                BasicFunc.RenameDagList(verticalOutputCurves, "netDyCurve_vertical_{0:d2}");
                listToDynamicCurve.clear();
                Debug.Log("horizontal to dynamic count:" + horizontalCurves.Count);
                for (int i = 0; i < horizontalCurves.Count; i++)
                {
                    listToDynamicCurve.add(horizontalCurves[i]);
                }
                MDagPath[] horizontalOutputCurves = CurvesToHairs(ref hairSystem, listToDynamicCurve, ConstantValue.HairPointLockType.None);
                Debug.Log("horizontal dynamic result count:" + horizontalOutputCurves.Length);
                BasicFunc.RenameDagList(horizontalOutputCurves, "netDyCurve_horizontal_{0:d2}");
                if (bindJoints)
                {
                    for (int i = 0; i < verticalOutputCurves.Length; i++)
                    {
                        JointProcess.AddIKHandle(jointPairToBind[i][0], jointPairToBind[i][1], JointProcess.IKSolverType.Spline, verticalOutputCurves[i].fullPathName);
                    }
                }
                if (addConstraint)
                {
                    MSelectionList outputCurveList = new MSelectionList();
                    for (int i = 0; i < verticalOutputCurves.Length; i++)
                    {
                        outputCurveList.add(verticalOutputCurves[i]);
                    }
                    for (int i = 0; i < horizontalOutputCurves.Length; i++)
                    {
                        outputCurveList.add(horizontalOutputCurves[i]);
                    }
                    AddDynamicConstraint(outputCurveList);
                }
            }
            else if (bindJoints)
            {
                for (int i = 0; i < jointPairToBind.Count; i++)
                {
                    JointProcess.AddIKHandle(jointPairToBind[i][0], jointPairToBind[i][1], JointProcess.IKSolverType.Spline, verticalCurves[i].fullPathName);
                }
            }
        }
        #endregion

        public static void AddDynamicConstraint(MSelectionList list, ConstantValue.DynamicConstraintType dynamicConstraintType = ConstantValue.DynamicConstraintType.PointToPoint, ConstantValue.DynamicConstraintMethod method = ConstantValue.DynamicConstraintMethod.Weld)
        {
            if (list == null)
            {
                list = BasicFunc.GetSelectedList();
            }
            BasicFunc.Select(list);
            string typeStr = ConstantValue.Param_DynamicConstraintType(dynamicConstraintType);
            string cmdStr = "createNConstraint " + typeStr + " 0";
            string dynamicConstraintShapeName = MGlobal.executeCommandStringResult(cmdStr);
            BasicFunc.SetAttr(dynamicConstraintShapeName, ConstantValue.plugName_dynamicConstraintMethod, "" + (int)method);
        }


        const string cmdStr = "DynamicConverter";
        public static List<CommandData> GetCommandDatas()
        {
            List<CommandData> cmdList = new List<CommandData>();
            cmdList.Add(new CommandData("动力学", cmdStr, "vertsToCurve", "顶点连成环线", () =>
            {
                CreateRelativeCurve(null, ConstantValue.SampleType.Vert, true, true);
            }));
            cmdList.Add(new CommandData("动力学", cmdStr, "vertsToCurve_origin", "顶点连成环线-原序", () =>
            {
                CreateRelativeCurve(null, ConstantValue.SampleType.Vert, false, true);
            }));
            cmdList.Add(new CommandData("动力学", cmdStr, "posToCurve", "物体坐标连成环线", () =>
            {
                CreateRelativeCurve();
            }));
            cmdList.Add(new CommandData("动力学", cmdStr, "posToCurve_origin", "物体坐标连成环线-原序", () =>
            {
                CreateRelativeCurve(null, ConstantValue.SampleType.ObjectTrans, false, true);
            }));
            cmdList.Add(new CommandData("动力学", cmdStr, "chainNet", "链骨网络", () =>
            {
                CreateSpiderNetCurves(null, false, false, false);
            }));
            cmdList.Add(new CommandData("动力学", cmdStr, "chainNetDynamic", "链骨网络-动力学附加(仿MMD刚体)", () =>
            {
                CreateSpiderNetCurves(null, true, true, true);
            }));
            cmdList.Add(new CommandData("动力学", cmdStr, "multiJointChainsToHair", "为链骨们增加动力学", () =>
            {
                AddDynamicChainControlPerChain();
            }));
            return cmdList;
        }

    }


}
