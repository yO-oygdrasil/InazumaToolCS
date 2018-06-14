using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using InazumaTool.BasicTools;
using Autodesk.Maya.OpenMaya;
using Autodesk.Maya.OpenMayaAnim;

namespace InazumaTool.BindTools
{
    public static class BindHumanBody
    {

        #region Finger

        public static bool BindFinger(MDagPath rootJointDagPath, string fingerTag, bool useIK = false)
        {
            MFnIkJoint rootJoint = new MFnIkJoint(rootJointDagPath);

            if (rootJoint.childCount > 0)
            {
                MObject middleJointObject = rootJoint.child(0);
                MDagPath middleJointDagPath = MDagPath.getAPathTo(middleJointObject);
                MFnIkJoint middleJoint = new MFnIkJoint(middleJointObject);
                if (middleJoint.childCount > 0)
                {
                    MObject finalJointObject = middleJoint.child(0);
                    MDagPath finalJointDagPath = MDagPath.getAPathTo(finalJointObject);
                    //MFnIkJoint finalJoint(finalJointObject);
                    //enough, start control
                    return BindFinger(rootJointDagPath, middleJointDagPath, finalJointDagPath, fingerTag, useIK);
                }
            }
            return true;
        }

        public static bool BindFinger(MDagPath rootJointDagPath, MDagPath middleJointDagPath, MDagPath finalJointDagPath, string fingerTag, bool useIK = false)
        {
            JointProcess.SetJointLimit(rootJointDagPath, JointProcess.JointType.FingerRoot);
            JointProcess.SetJointLimit(middleJointDagPath, JointProcess.JointType.FingerMiddle);
            JointProcess.SetJointLimit(finalJointDagPath, JointProcess.JointType.FingerMiddle);

            if (useIK)
            {

            }
            else
            {
                MDagPath ctlDagPath = BasicFunc.AddParentCircle(rootJointDagPath, true);
                MFnDependencyNode remapNode_root = BasicFunc.CreateRemapValueNode(-2, 3, 60, -90);
                MFnDependencyNode remapNode_rootSide = BasicFunc.CreateRemapValueNode(-1, 1, 30, -30);
                MFnDependencyNode remapNode_middle = BasicFunc.CreateRemapValueNode(-1, 3, 30, -90);
                MFnDependencyNode remapNode_final = BasicFunc.CreateRemapValueNode(-1, 3, 30, -90);
                //MFnDependencyNode** ptr_remapNode_root = &remapNode_root,


                //string remapValueNodeName_root = BasicFunc.CreateRemapValueNode(-2, 3, 60, -90, ptr_remapNode_root);
                //string remapValueNodeName_rootSide = BasicFunc.CreateRemapValueNode(-1, 1, 30, -30, ptr_remapNode_rootSide);
                //string remapValueNodeName_middle = BasicFunc.CreateRemapValueNode(-1, 3, 30, -90, ptr_remapNode_middle);
                //string remapValueNodeName_final = BasicFunc.CreateRemapValueNode(-1, 3, 30, -90, ptr_remapNode_final);
                string ctlName = ctlDagPath.fullPathName;
                MFnDependencyNode dn_ctl = new MFnDependencyNode(ctlDagPath.node);
                MFnDependencyNode dn_root = new MFnDependencyNode(rootJointDagPath.node);
                MFnDependencyNode dn_middle = new MFnDependencyNode(middleJointDagPath.node);
                MFnDependencyNode dn_final = new MFnDependencyNode(finalJointDagPath.node);

                /*MPlug plug_ctlTy = dn_ctl.findPlug("translateY");
                MGlobal.displayInfo("plug name:" + plug_ctlTy.partialName() + " fullname:" + plug_ctlTy.name());*/
                //MStatus status;
                //MPlug plug_remapNode_root_input = remapNode_root.findPlug("inputValue", &status);
                //if (status == MStatus::kSuccess)
                //{
                //	MGlobal.displayInfo("success 634634");
                //	//MGlobal.displayInfo("plug name:" + plug_remapNode_root_input.partialName() + " fullname:" + plug_remapNode_root_input.name());
                //}
                //else
                //{
                //	MGlobal.displayInfo("failed a23234234");
                //}
                MDGModifier dgModifier = new MDGModifier();
                dgModifier.doIt();
                dgModifier.connect(dn_ctl.findPlug("translateY"), remapNode_root.findPlug("inputValue"));
                dgModifier.connect(remapNode_root.findPlug("outValue"), dn_root.findPlug("rotateZ"));

                dgModifier.connect(dn_ctl.findPlug("translateZ"), remapNode_rootSide.findPlug("inputValue"));
                dgModifier.connect(remapNode_rootSide.findPlug("outValue"), dn_root.findPlug("rotateY"));

                dgModifier.connect(dn_ctl.findPlug("translateX"), remapNode_middle.findPlug("inputValue"));
                dgModifier.connect(remapNode_middle.findPlug("outValue"), dn_middle.findPlug("rotateZ"));

                dgModifier.connect(dn_ctl.findPlug("translateX"), remapNode_final.findPlug("inputValue"));
                dgModifier.connect(remapNode_final.findPlug("outValue"), dn_final.findPlug("rotateZ"));
                dgModifier.doIt();

                BasicFunc.SetTranslateLimit(new MFnTransform(ctlDagPath), -1, -2, -1, 3, 3, 1);
            }
            return true;
        }

        #endregion

        #region IKControl

        public static MDagPath AddRPIKPole(MDagPath middleDagPath = null)
        {
            if (middleDagPath == null)
            {
                middleDagPath = BasicFunc.GetSelectedDagPath(0);
                if (middleDagPath == null)
                {
                    MGlobal.displayInfo("please select middle joint");
                    return null;
                }
            }
            
            MDagPath rootDagPath = new MDagPath(), endDagPath = new MDagPath();
            MFnIkJoint middleJoint = new MFnIkJoint(middleDagPath);
            if (middleJoint.parentCount > 0)
            {
                MDagPath.getAPathTo(middleJoint.parent(0), rootDagPath);
                MFnIkJoint rootJoint = new MFnIkJoint(rootDagPath);
                if (middleJoint.childCount > 0)
                {
                    MDagPath.getAPathTo(middleJoint.child(0), endDagPath);
                    MFnIkJoint endJoint = new MFnIkJoint(endDagPath);
                    MVector rootPos = rootJoint.getTranslation(MSpace.Space.kWorld);
                    MVector middlePos = middleJoint.getTranslation(MSpace.Space.kWorld);
                    MVector endPos = endJoint.getTranslation(MSpace.Space.kWorld);
                    

                    //double len0 = (middlePos - rootPos).length;
                    //double len1 = (endPos - middlePos).length;

                    MVector fitLinePos = (rootPos + endPos) * 0.5;
                    MVector direct_pole = middlePos - fitLinePos;
                    MVector direct_fitLine = rootPos - endPos;
                    MVector direct_projectPolePos = BasicFunc.VerticalProject(direct_pole, direct_fitLine).normal;

                    //MVector nmPos = (rootPos * len0 + endPos * len1) * (1 / (len0 + len1));
                    float factor = 2;
                    MVector polePos = factor * direct_projectPolePos + middlePos;
                    
                    string locName = "loc_" + rootJoint.name + "_" + endJoint.name;
                    return BasicFunc.CreateLocator(polePos, locName);
                }
            }
            return null;
        }


        public static MDagPath[] BindIKControl(MSelectionList jointList = null,JointProcess.IKSolverType iKSolverType = JointProcess.IKSolverType.RotatePlane)
        {
            if (jointList == null || jointList.isEmpty) 
            {
                jointList = new MSelectionList();
                MGlobal.getActiveSelectionList(jointList);
            }
            
            if (jointList.length == 3)
            {
                MDagPath rootObject = new MDagPath(), endObject = new MDagPath(), ctlDagPath = new MDagPath();
                jointList.getDagPath(0, rootObject);
                jointList.getDagPath(1, endObject);
                jointList.getDagPath(2, ctlDagPath);
                return BindIKControl(rootObject, endObject, iKSolverType, ctlDagPath);
            }
            else if (jointList.length == 2)
            {
                MDagPath rootObject = new MDagPath(), endObject = new MDagPath();
                jointList.getDagPath(0, rootObject);
                jointList.getDagPath(1, endObject);
                return BindIKControl(rootObject, endObject, iKSolverType);
            }
            else
            {
                return null;
            }
        }
        
        public static MDagPath[] BindIKControl(MDagPath rootDagPath, MDagPath endDagPath, JointProcess.IKSolverType iKSolverType = JointProcess.IKSolverType.RotatePlane, MDagPath ctlDagPath = null)
        {
            MFnTransform endTrans = new MFnTransform(endDagPath);
            MDagPath middleDagPath = MDagPath.getAPathTo(endTrans.parent(0));

            if (ctlDagPath == null)
            {
                ctlDagPath = BasicFunc.AddChildCircle(endDagPath);
                BasicFunc.UnparentTransform(ctlDagPath);
                BasicFunc.FreezeTransform(new MFnTransform(ctlDagPath));
            }


            //string resultStr = MGlobal.executeCommandStringResult("ikHandle -sj " + rootObject.fullPathName() + " -ee " + endObject.fullPathName() + " -sol ikRPsolver -n ik_" + rootObject.partialPathName() + "_" + endObject.partialPathName(),true);
            //string resultStr = MGlobal.executePythonCommandStringResult("cmds.ikHandle(sj='" + rootDagPath.fullPathName + "',ee='" + endDagPath.fullPathName + "',sol='ikRPsolver',n='ik_" + rootDagPath.partialPathName + "_" + endDagPath.partialPathName + "')");

            //[u'ik_joint1_joint4', u'effector1']
            string[] resultArr = JointProcess.AddIKHandle(rootDagPath, endDagPath, iKSolverType, ctlDagPath.fullPathName);
            MGlobal.executeCommandStringResult("pointConstraint " + ctlDagPath.fullPathName + " " + resultArr[0]);

            if (iKSolverType == JointProcess.IKSolverType.RotatePlane)
            {
                MDagPath locDagPath = AddRPIKPole(middleDagPath);
                if (locDagPath != null)
                {
                    BasicFunc.FreezeTransform(new MFnTransform(locDagPath));
                    //begin to add constriant
                    BasicFunc.AddConstraint(locDagPath.fullPathName, resultArr[0], ConstantValue.ConstraintType.PoleVector);
                    //string poleConstraintResult = MGlobal.executeCommandStringResult("poleVectorConstraint " + locDagPath.fullPathName + " " + resultArr[0]);
                    //MGlobal.displayInfo(poleConstraintResult);
                    return new MDagPath[3] { BasicFunc.GetDagPathByName(resultArr[0]), ctlDagPath, locDagPath };
                }
            }

            return new MDagPath[2] { BasicFunc.GetDagPathByName(resultArr[0]), ctlDagPath };
        }

        #endregion




        #region Foot

        public static MDagPath[] AddReverseFootBone()
        {
            MSelectionList selected = new MSelectionList();
            MGlobal.getActiveSelectionList(selected);
            if (selected.length == 3)
            {
                MDagPath rootObject = new MDagPath(), endObject = new MDagPath(), ctlObject = new MDagPath();
                selected.getDagPath(0, rootObject);
                selected.getDagPath(1, endObject);
                selected.getDagPath(2, ctlObject);
                return AddReverseFootBone(rootObject, endObject, ctlObject);
            }
            return null;
        }


        public static MDagPath[] AddReverseFootBone(MDagPath rootDagPath, MDagPath middleDagPath, MDagPath endDagPath)
        {
            //啊啊
            //*reverseBones = new MDagPath[8];
            MDagPath[] result = new MDagPath[8];

            MFnIkJoint rootJoint = new MFnIkJoint();
            MVector rootPos = new MFnTransform(rootDagPath).getTranslation(MSpace.Space.kWorld);
            MVector middlePos = new MFnTransform(middleDagPath).getTranslation(MSpace.Space.kWorld);
            MVector endPos = new MFnTransform(endDagPath).getTranslation(MSpace.Space.kWorld);
            //MGlobal.displayInfo("root:" + BasicFunc.ToCMDSParamStr(rootPos) + " middle:" + BasicFunc.ToCMDSParamStr(middlePos) + " end:" + BasicFunc.ToCMDSParamStr(endPos));

            MObject jt_ankle_Object = rootJoint.create();
            result[0] = MDagPath.getAPathTo(jt_ankle_Object);
            MFnIkJoint jt_ankle = new MFnIkJoint(result[0]);
            jt_ankle.setTranslation(rootPos, MSpace.Space.kWorld);

            MObject jt_heel_Object = rootJoint.create(jt_ankle_Object);
            result[1] = MDagPath.getAPathTo(jt_heel_Object);
            MFnIkJoint jt_heel = new MFnIkJoint(result[1]);
            jt_heel.setTranslation(new MVector(rootPos.x, endPos.y, rootPos.z), MSpace.Space.kWorld);

            MObject jt_side_Object = rootJoint.create(jt_heel_Object);
            result[2] = MDagPath.getAPathTo(jt_side_Object);
            MFnIkJoint jt_side = new MFnIkJoint(result[2]);
            float sideFactor = (float)(0.6 * (middlePos - endPos).length / Math.Abs(middlePos.z));
            jt_side.setTranslation(new MVector(middlePos.x, endPos.y, middlePos.z * (1 + sideFactor)), MSpace.Space.kWorld);

            MObject jt_front_Object = rootJoint.create(jt_side_Object);
            result[3] = MDagPath.getAPathTo(jt_front_Object);
            MFnIkJoint jt_front = new MFnIkJoint(result[3]);
            jt_front.setTranslation(endPos, MSpace.Space.kWorld);

            MObject jt_middleF_Object = rootJoint.create(jt_front_Object);
            result[4] = MDagPath.getAPathTo(jt_middleF_Object);
            MFnIkJoint jt_middleF = new MFnIkJoint(result[4]);
            jt_middleF.setTranslation(middlePos, MSpace.Space.kWorld);

            MObject jt_middleB_Object = rootJoint.create(jt_front_Object);
            result[5] = MDagPath.getAPathTo(jt_middleB_Object);
            MFnIkJoint jt_middleB = new MFnIkJoint(result[5]);
            jt_middleB.setTranslation(middlePos, MSpace.Space.kWorld);

            MObject jt_toe_Object = rootJoint.create(jt_middleF_Object);
            result[6] = MDagPath.getAPathTo(jt_toe_Object);
            MFnIkJoint jt_toe = new MFnIkJoint(result[6]);
            jt_toe.setTranslation(endPos, MSpace.Space.kWorld);

            MObject jt_ankleIn_Object = rootJoint.create(jt_middleB_Object);
            result[7] = MDagPath.getAPathTo(jt_ankleIn_Object);
            MFnIkJoint jt_ankleIn = new MFnIkJoint(result[7]);
            jt_ankleIn.setTranslation(rootPos, MSpace.Space.kWorld);

            MGlobal.displayInfo("create joints ok");

            return result;
        }

        public static bool BindReverseFootRPIK()
        {
            MSelectionList selected = new MSelectionList();
            MGlobal.getActiveSelectionList(selected);
            if (selected.length == 4)
            {
                MDagPath legRootDagPath = new MDagPath(), ankleDagPath = new MDagPath(), middleDagPath = new MDagPath(), endDagPath = new MDagPath();
                selected.getDagPath(0, legRootDagPath);
                selected.getDagPath(1, ankleDagPath);
                selected.getDagPath(2, middleDagPath);
                selected.getDagPath(3, endDagPath);
                return BindReverseFootRPIK(legRootDagPath, ankleDagPath, middleDagPath, endDagPath);
            }
            return false;
        }

        public static bool BindReverseFootRPIK(MDagPath legRootDagPath, MDagPath ankleDagPath, MDagPath middleDagPath, MDagPath endDagPath)
        {
            //ypa
            //create reverseBones
            MDagPath[] rbs = AddReverseFootBone(ankleDagPath, middleDagPath, endDagPath);
            if (rbs == null) 
            {
                return false;
            }

            MGlobal.displayInfo(rbs[7].fullPathName);
            MDagPath ikDagPath = BindIKControl(legRootDagPath, ankleDagPath, JointProcess.IKSolverType.RotatePlane, rbs[7])[0];
            MGlobal.executeCommandStringResult("orientConstraint -mo " + rbs[4].fullPathName + " " + middleDagPath.fullPathName);
            MGlobal.executeCommandStringResult("orientConstraint -mo " + rbs[5].fullPathName + " " + ankleDagPath.fullPathName);




            return true;
        }

        #endregion

        #region Hair

        public static void ConvertJointLinesToHair(MSelectionList jointList)
        {
            int count =(int) jointList.length;
        }


        #endregion


        #region MiddleBody

        public static void BindBodySplineIK(MSelectionList jointList = null)
        {
            if (jointList == null)
            {
                jointList = BasicFunc.GetSelectedList();
            }
            //check if all of selected objects are joint
            int count = (int)jointList.length;
            if (count < 2)
            {
                return;
            }
            MDagPath dag_breastJoint = new MDagPath(), dag_hipJoint = new MDagPath();
            for (int i = 0; i < count; i++)
            {
                MDagPath jtDagPath = new MDagPath();
                jointList.getDagPath((uint)i, jtDagPath);
                if (jtDagPath != null)
                {
                    if (!jtDagPath.hasFn(MFn.Type.kJoint))
                    {
                        return;
                    }

                }
                else
                {
                    return;
                }
            }

            jointList.getDagPath((uint)(count - 1), dag_breastJoint);
            jointList.getDagPath(0, dag_hipJoint);

            MFnIkJoint breastJoint = new MFnIkJoint(dag_breastJoint);
            MFnIkJoint hipJoint = new MFnIkJoint(dag_hipJoint);
            MDagPath dag_curve = JointProcess.CreateJointsCurve(jointList);
            MDagPath dag_jtctl_breast = JointProcess.CreateJoint(breastJoint, "jtctl_breast");
            MDagPath dag_jtctl_hip = JointProcess.CreateJoint(hipJoint, "jtctl_hip");
            MSelectionList bindSelectionList = new MSelectionList();
            bindSelectionList.add(dag_curve);
            bindSelectionList.add(dag_jtctl_breast);
            bindSelectionList.add(dag_jtctl_hip);
            BasicFunc.Select(bindSelectionList);

            MGlobal.executeCommand("SmoothBindSkin");
            string ikName = JointProcess.AddIKHandle(dag_hipJoint, dag_breastJoint, JointProcess.IKSolverType.Spline, dag_curve.fullPathName)[0];
            MDagPath dag_ik = BasicFunc.GetDagPathByName(ikName);
            BasicFunc.ConnectAttr(dag_jtctl_breast.fullPathName + ".rotate.rotateY", dag_ik.fullPathName + ".twist", true, true);

        }

        public static void BindShoulder(MSelectionList jointList = null)
        {
            if (jointList == null)
            {
                jointList = BasicFunc.GetSelectedList(MFn.Type.kJoint);
            }
            if (jointList.length != 4)
            {
                MGlobal.displayInfo("please select joints");
                return;
            }

            MDagPath dag_shoulder = new MDagPath(), dag_armRoot = new MDagPath(), dag_elbow = new MDagPath(), dag_wrist = new MDagPath();
            jointList.getDagPath((uint)0, dag_shoulder);
            jointList.getDagPath((uint)1, dag_armRoot);
            jointList.getDagPath((uint)2, dag_elbow);
            jointList.getDagPath((uint)3, dag_wrist);
            //string shoulderIkName = JointProcess.AddIKHandle(dag_shoulder, dag_armRoot, JointProcess.IKSolverType.SingleChain)[0];
            MDagPath[] shoulderResult = BindIKControl(dag_shoulder, dag_armRoot, JointProcess.IKSolverType.SingleChain);
            MDagPath[] armResult = BindIKControl(dag_armRoot, dag_wrist, JointProcess.IKSolverType.RotatePlane);

            MDagPath dag_ctl_shoulder = shoulderResult[1], dag_ctl_arm = armResult[1], dag_ctl_pole = armResult[2];
            BasicFunc.SetTransformParent(dag_ctl_arm.fullPathName, dag_ctl_shoulder.fullPathName);
            BasicFunc.SetTransformParent(dag_ctl_pole.fullPathName, dag_ctl_shoulder.fullPathName);
            BasicFunc.AddConstraint(dag_ctl_arm.fullPathName, dag_wrist.fullPathName, ConstantValue.ConstraintType.Orient);
            //MFnTransform trans_ctl_shoulder = new MFnTransform(dag_ctl_shoulder);
            //MFnTransform trans_ctl_arm = new MFnTransform(dag_ctl_arm);
            //MFnTransform trans_ctl_pole = new MFnTransform(dag_ctl_pole);

            //BasicFunc.SetTransformParent(trans_ctl_arm, trans_ctl_shoulder);
            //BasicFunc.SetTransformParent(trans_ctl_pole, trans_ctl_shoulder);


        }



        #endregion

        #region Face
        public static void BindEyes(MSelectionList jointList = null)
        {
            if (jointList == null)
            {
                jointList = BasicFunc.GetSelectedList();
            }
            
            for (int i = 0; i < jointList.length; i++)
            {
                MDagPath dag_joint = new MDagPath();
                jointList.getDagPath((uint)i, dag_joint);

            }
        }

        public static List<MDagPath> AddBonesCTL(MSelectionList jointList = null, MFnTransform parentTrans = null)
        {
            if (jointList == null)
            {
                jointList = BasicFunc.GetSelectedList();
            }
            if (parentTrans == null)
            {
                parentTrans = new MFnTransform(BasicFunc.CreateEmptyGroup("grp_bonesCTL"));
            }
            List<MDagPath> jointDags = new List<MDagPath>();
            for (int i = 0; i < jointList.length; i++)
            {
                MDagPath dag = new MDagPath();
                jointList.getDagPath((uint)i, dag);
                jointDags.Add(dag);
            }
            int count = jointDags.Count;

            MFnTransform[] jointTrans = new MFnTransform[count];
            for (int i = 0; i < jointDags.Count; i++)
            {
                jointTrans[i] = new MFnTransform(jointDags[i]);
            }
            MVector[] jointWorldPositions = new MVector[jointDags.Count];
            MVector centerPos = MVector.zero;
            for (int i = 0; i < count; i++)
            {
                jointWorldPositions[i] = jointTrans[i].getTranslation(MSpace.Space.kWorld);
                centerPos += jointWorldPositions[i];
            }
            centerPos = centerPos * (1.0f / count);

            double[] minDist_y = new double[count];
            double[] minDist_x = new double[count];

            for (int i = 0; i < count; i++)
            {
                double closestY = double.MaxValue, closestX = double.MaxValue;
                //int minDistIndex = 0;

                for (int j = 0; j < count; j++)
                {
                    if (i == j)
                    {
                        continue;
                    }
                    MVector direct = jointWorldPositions[i] - jointWorldPositions[j];
                    direct.x = Math.Abs(direct.x);
                    direct.y = Math.Abs(direct.y);
                    if (direct.x >= direct.y)
                    {
                        if (direct.x < closestX)
                        {
                            closestX = direct.x;
                            //minDistIndex = j;
                        }
                    }
                    if (direct.y >= direct.x)
                    {
                        if (direct.y < closestY)
                        {
                            closestY = direct.y;
                        }
                    }
                }
                minDist_y[i] = closestY;
                minDist_x[i] = closestX;
            }
            List<MDagPath> curves = new List<MDagPath>();
            for (int i = 0; i < count; i++)
            {
                MDagPath curve = BasicFunc.CreateCTL_Square("ctl_" + jointDags[i].partialPathName, (float)minDist_y[i] / 2, (float)minDist_x[i] / 2);
                MFnTransform curveTrans = new MFnTransform(curve);
                BasicFunc.SetTransformParent(curveTrans, parentTrans);
                curveTrans.setTranslation(jointWorldPositions[i] - centerPos, MSpace.Space.kTransform);
                BasicFunc.FreezeTransform(curveTrans);
                curves.Add(curve);
            }
            return curves;
        }

        #endregion




        const string cmdStr = "BindHumanBody";
        public static List<CommandData> GetCommandDatas()
        {
            List<CommandData> cmdList = new List<CommandData>();

            cmdList.Add(new CommandData("绑定", "手指"));

            cmdList.Add(new CommandData("绑定", cmdStr, "bindFinger", "绑定手指控制器", () =>
            {
                BindFinger(BasicFunc.GetSelectedDagPath(0), "test", false);
            }));

            cmdList.Add(new CommandData("绑定", "旋转平面"));
            cmdList.Add(new CommandData("绑定", cmdStr, "rpik", "绑定旋转平面IK控制器", () =>
            {
                BindIKControl();
            }));
            cmdList.Add(new CommandData("绑定", cmdStr, "rpikPole", "生成极向量控制器", () =>
            {
                AddRPIKPole();
            }));

            cmdList.Add(new CommandData("绑定", "JIO"));
            cmdList.Add(new CommandData("绑定", cmdStr, "reverseFootBones", "生成反向脚部控制骨", () =>
            {
                AddReverseFootBone();
            }));
            cmdList.Add(new CommandData("绑定", cmdStr, "reverseFootRPIK", "绑定反向脚控制器", () =>
            {
                BindReverseFootRPIK();
            }));


            cmdList.Add(new CommandData("绑定", "上半身"));
            cmdList.Add(new CommandData("绑定", cmdStr, "bindBreast", "绑定上半身与腰部", () =>
            {
                BindBodySplineIK();
            }));
            cmdList.Add(new CommandData("绑定", cmdStr, "bindShoulder", "绑定人类肩膀-手臂", () =>
            {
                BindShoulder();
            }));


            cmdList.Add(new CommandData("绑定", "面部"));
            cmdList.Add(new CommandData("绑定", cmdStr, "bindTongue", "绑定花京院型舌头", () =>
            {
                //BindReverseFootRPIK();
            }));
            cmdList.Add(new CommandData("绑定", cmdStr, "bindSight", "绑定平视控制器", () =>
            {
                //BindReverseFootRPIK();
            }));
            cmdList.Add(new CommandData("绑定", cmdStr, "createJointCTLs", "根据骨骼位置创建控制器阵列", () =>
            {
                AddBonesCTL();
            }));

            return cmdList;
        }
    }
}
