using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Maya.OpenMaya;
using Autodesk.Maya.OpenMayaAnim;

namespace InazumaTool.BasicTools
{
    public static class JointProcess
    {
        public enum JointType
        {
            Shoulder,
            FingerMiddle,
            FingerRoot,
            Default
        };

        public static bool SetLimit(MFnTransform mfnTrans, float rxMin, float ryMin, float rzMin, float rxMax, float ryMax, float rzMax, bool inRadian)
        {
            if (!inRadian)
            {
                rxMin /= ConstantValue.DPR;
                ryMin /= ConstantValue.DPR;
                rzMin /= ConstantValue.DPR;
                rxMax /= ConstantValue.DPR;
                ryMax /= ConstantValue.DPR;
                rzMax /= ConstantValue.DPR;
            }
            mfnTrans.setLimit(MFnTransform.LimitType.kRotateMinX, rxMin);
            mfnTrans.setLimit(MFnTransform.LimitType.kRotateMinY, ryMin);
            mfnTrans.setLimit(MFnTransform.LimitType.kRotateMinZ, rzMin);
            mfnTrans.setLimit(MFnTransform.LimitType.kRotateMaxX, rxMax);
            mfnTrans.setLimit(MFnTransform.LimitType.kRotateMaxY, ryMax);
            mfnTrans.setLimit(MFnTransform.LimitType.kRotateMaxZ, rzMax);
            mfnTrans.enableLimit(MFnTransform.LimitType.kRotateMinX, true);
            mfnTrans.enableLimit(MFnTransform.LimitType.kRotateMinY, true);
            mfnTrans.enableLimit(MFnTransform.LimitType.kRotateMinZ, true);
            mfnTrans.enableLimit(MFnTransform.LimitType.kRotateMaxX, true);
            mfnTrans.enableLimit(MFnTransform.LimitType.kRotateMaxY, true);
            mfnTrans.enableLimit(MFnTransform.LimitType.kRotateMaxZ, true);
            return true;
        }

        public static bool SetJointLimit(MDagPath mDagPath, JointType jointType)
        {
            return SetJointLimit(new MFnTransform(mDagPath), jointType);
        }

        public static bool SetJointLimit(MObject mobject, JointType jointType)
        {
            if (mobject.hasFn(MFn.Type.kTransform))
            {
                return SetJointLimit(new MFnTransform(MDagPath.getAPathTo(mobject)), jointType);
            }
            else
            {
                return false;
            }
        }

        public static bool SetJointLimit(MFnTransform mfnTransform, JointType jointType)
        {
            switch (jointType)
            {
                case JointType.Default:
                    {
                        SetLimit(mfnTransform, -360, -360, -360, 360, 360, 360, false);
                        break;
                    }
                case JointType.FingerMiddle:
                    {
                        SetLimit(mfnTransform, 0, 0, -90, 0, 0, 30, false);
                        break;
                    }
                case JointType.FingerRoot:
                    {
                        SetLimit(mfnTransform, 0, -30, -90, 0, 30, 60, false);
                        break;
                    }
                case JointType.Shoulder:
                    {
                        SetLimit(mfnTransform, -80, -90, -120, 30, 90, 120, false);
                        break;
                    }
                default:
                    {
                        return false;
                        //SetLimit(mfnTransform, -360, -360, -360, 360, 360, 360, false);

                    }

            }
            return true;
        }
        
        public static MDagPath CreateJointsCurve(MSelectionList jointDagPathList)
        {
            int count = (int)jointDagPathList.length;
            string curveName = "curve_";
            MVector[] vectors = new MVector[count];
            for (int i = 0; i < count; i++)
            {
                MDagPath dagPath = new MDagPath();
                jointDagPathList.getDagPath((uint)i, dagPath);
                if (i == 0)
                {
                    curveName += dagPath.partialPathName + "_";
                }
                else if (i == count - 1)
                {
                    curveName += dagPath.partialPathName;
                }
                MFnTransform ptTrans = new MFnTransform(dagPath);
                vectors[i] = ptTrans.getTranslation(MSpace.Space.kWorld);
                //MGlobal.displayInfo(BasicFunc::ToCMDSParamStr(vectors[i]));
            }
            MDagPath curveDagPath = BasicFunc.CreateCurve(vectors, curveName);
            //MGlobal.displayInfo("create Finish");
            

            return curveDagPath;
        }

        public static void MakeJointsHairChain(MSelectionList jointDagPaths)
        {
            //begin convert curve to dynamic
            MDagPath curveDagPath = CreateJointsCurve(jointDagPaths);
            string cmdStr = "cmds.makeCurveDynamic(0,0,0,1,0)";
            string resultStr = MGlobal.executePythonCommandStringResult(cmdStr);

            //string resultStr = MGlobal.executeCommandStringResult("makeCurvesDynamic 2 {\"0\",\"0\",\"0\",\"1\",\"0\"}");
            MGlobal.displayInfo("message" + resultStr);
        }

        public static MDagPath CreateJoint(string jtName)
        {
            MFnIkJoint joint = new MFnIkJoint();
            MObject jtObject = joint.create();
            return MDagPath.getAPathTo(jtObject);
        }

        public static MDagPath CreateJoint(MVector worldPos, string jtName)
        {
            MDagPath jointDagPath = CreateJoint(jtName);
            MFnIkJoint joint = new MFnIkJoint(jointDagPath);
            joint.setTranslation(worldPos, MSpace.Space.kWorld);
            return jointDagPath;
        }

        public static MDagPath CreateJoint(MFnIkJoint targetPosJoint, string jtName)
        {
            MVector worldPos = targetPosJoint.getTranslation(MSpace.Space.kWorld);
            return CreateJoint(worldPos, jtName);
        }

        public enum IKSolverType
        {
            SingleChain,
            RotatePlane,
            Spline
        }
        public static string[] AddIKHandle(MDagPath startJointDagPath, MDagPath endJointDagPath, IKSolverType solverType = IKSolverType.RotatePlane, string curveName = "")
        {
            //string typeStr = "";
            string resultStr = "";

            CmdStrConstructor csc = new CmdStrConstructor("ikHandle", CmdStrConstructor.CmdType.Python);
            csc.UpdateParm("sj", startJointDagPath.fullPathName);
            csc.UpdateParm("ee", endJointDagPath.fullPathName);

            string ikMainName = startJointDagPath.partialPathName + "_" + endJointDagPath.partialPathName;
            
            switch (solverType)
            {
                case IKSolverType.SingleChain:
                    {
                        csc.UpdateParm("sol", "ikSCsolver");
                        csc.UpdateParm("n", "ik_" + ikMainName);
                        string excuteStr = csc.ToString();
                        resultStr = MGlobal.executePythonCommandStringResult(excuteStr);
                        //typeStr = "ikSCsolver";
                        //resultStr = MGlobal.executePythonCommandStringResult("cmds.ikHandle(sj='" + startJointDagPath.fullPathName + "',ee='" + endJointDagPath.fullPathName + "',sol='" + typeStr + "',n='ik_" + ikMainName + "')");
                        break;
                    }
                case IKSolverType.RotatePlane:
                    {
                        csc.UpdateParm("sol", "ikRPsolver");
                        csc.UpdateParm("n", "ik_" + ikMainName);
                        string excuteStr = csc.ToString();
                        resultStr = MGlobal.executePythonCommandStringResult(excuteStr);
                        //typeStr = "ikRPsolver";
                        //resultStr = MGlobal.executePythonCommandStringResult("cmds.ikHandle(sj='" + startJointDagPath.fullPathName + "',ee='" + endJointDagPath.fullPathName + "',sol='" + typeStr + "',n='ik_" + ikMainName + "')");
                        break;
                    }
                case IKSolverType.Spline:
                    {
                        csc.UpdateParm("sol", "ikSplineSolver");
                        csc.UpdateParm("n", "ik_" + ikMainName);
                        csc.UpdateParm("ccv", curveName == null || curveName.Length == 0);
                        csc.UpdateParm("c", curveName);
                        csc.UpdateParm("pcv", false);
                        string excuteStr = csc.ToString();
                        resultStr = MGlobal.executePythonCommandStringResult(excuteStr);
                        //resultStr = MGlobal.executePythonCommandStringResult("cmds.ikHandle(sj='" + startJointDagPath.fullPathName + "',ee='" + endJointDagPath.fullPathName + "',sol='" + typeStr + "',c='" + curveName + "',n='ik_" + ikMainName + "')",true);
                        break;
                    }
            }
            //[u'ik_joint1_joint4', u'effector1']
            string[] resultArr = BasicFunc.SplitPythonResultStr(resultStr);

            return resultArr;
        }

        public static void ClearHierachyJointsRotation(MSelectionList selectionList = null)
        {
            if (selectionList == null || selectionList.length == 0)
            {
                selectionList = BasicFunc.GetSelectedList();
            }
            foreach (MDagPath dag in selectionList.DagPaths())
            {
                List<MFnTransform> transList = BasicFunc.GetHierachyAllTrans(dag, MFn.Type.kJoint);
                foreach (MFnTransform trans in transList)
                {
                    //Debug.Log("trans:" + trans.fullPathName);
                    trans.setRotation(new MEulerRotation(0, 0, 0));
                    MEulerRotation result = new MEulerRotation();
                    trans.getRotation(result);
                    //Debug.LogEuler(result, "value");
                }
            }

        }

        private static void MoveSkinJointsTool(MDagPath dag = null)
        {
            if (dag != null)
            {
                BasicFunc.Select(dag);
            }
            MGlobal.executeCommand("MoveSkinJointsTool");
        }

        public static MVector GetLinearPosByWeight(MVector middleWorldPos, MVector parentWorldPos, MVector childWorldPos)
        {
            float length0 = (float)(middleWorldPos - parentWorldPos).length;
            float length1 = (float)(middleWorldPos - childWorldPos).length;
            MVector posByWeight = BasicFunc.Lerp(parentWorldPos, childWorldPos, length0 / (length0 + length1));
            return posByWeight;
        }

        public static void BeautifulPole(MDagPath middleDag = null, bool clearFlag_x = true,bool clearFlag_y = true,bool clearFlag_z =true, bool useMoveSkinJointsTool = true)
        {
            if (middleDag == null || middleDag.node.isNull)
            {
                middleDag = BasicFunc.GetSelectedDagPath(0);
                if (middleDag == null || middleDag.node.isNull)
                {
                    Debug.Log("please select middle joint");
                    return;
                }
            }

            MFnTransform middleTrans = new MFnTransform(middleDag);
            if (middleTrans.parentCount == 0 || middleTrans.childCount == 0)
            {
                return;
            }
            MDagPath parentDag = MDagPath.getAPathTo(middleTrans.parent(0));
            MDagPath childDag = MDagPath.getAPathTo(middleTrans.child(0));
            MFnTransform parentTrans = new MFnTransform(parentDag);
            MFnTransform childTrans = new MFnTransform(childDag);
            MVector parentWorldPos = parentTrans.getTranslation(MSpace.Space.kWorld);
            MVector middleWorldPos = middleTrans.getTranslation(MSpace.Space.kWorld);
            MVector childWorldPos = childTrans.getTranslation(MSpace.Space.kWorld);
            //MVector vec_middleToParent = parentWorldPos - middleWorldPos;
            //MVector vec_childToMiddle = middleWorldPos - childWorldPos;
            //MVector verticalBack = BasicFunc.Cross(vec_middleToParent, vec_childToMiddle);
            
            float length0 = (float) (middleWorldPos - parentWorldPos).length;
            float length1 = (float) (middleWorldPos - childWorldPos).length;
            MVector posByWeight = BasicFunc.Lerp(parentWorldPos, childWorldPos, length0 / (length0 + length1));
            if (useMoveSkinJointsTool)
            {
                MoveSkinJointsTool(middleDag);
            }
            //middleTrans.setTranslation(posByWeight, MSpace.Space.kWorld);
            if (!clearFlag_x)
            {
                posByWeight.x = middleWorldPos.x;
            }
            if (!clearFlag_y)
            {
                posByWeight.y = middleWorldPos.y;
            }
            if (!clearFlag_z)
            {
                posByWeight.z = middleWorldPos.z;
            }
            middleTrans.setTranslation(posByWeight, MSpace.Space.kWorld);
            childTrans.setTranslation(childWorldPos, MSpace.Space.kWorld);
            //MFnIkJoint middleJoint = new MFnIkJoint(middleDag);
        }

        private static void BeautifulPoleWindow()
        {
            BasicWindow bw = new BasicWindow("Beautiful Pole");
            Action[] acts = new Action[8];

        }



        const string cmdStr = "JointProcess";
        public static List<CommandData> GetCommandDatas()
        {
            List<CommandData> cmdList = new List<CommandData>();
            cmdList.Add(new CommandData("骨骼", cmdStr, "add", "沿骨骼生成曲线", () =>
            {
                CreateJointsCurve(BasicFunc.GetSelectedList());
            }));
            cmdList.Add(new CommandData("骨骼", cmdStr, "clearRotation", "清除层级下骨骼旋转",()=>
            {
                ClearHierachyJointsRotation();
            }));
            cmdList.Add(new CommandData("骨骼", cmdStr, "beautifulPole", "为漂亮的极向量铺路", () =>
            {
                BeautifulPole(null);
            }));
            cmdList.Add(new CommandData("骨骼", cmdStr, "manyBeautifulPole", "为很多漂亮的极向量铺路", () =>
            {
                BasicFunc.IterateSelectedDags((dag) => { BeautifulPole(dag, true); });
            }));
            return cmdList;
        }



    }
}
