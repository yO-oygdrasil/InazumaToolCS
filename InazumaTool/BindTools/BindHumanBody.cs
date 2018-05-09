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
    public class BindHumanBody
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
                MFnDependencyNode remapNode_final= BasicFunc.CreateRemapValueNode(-1, 3, 30, -90);
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

#region RPIK


        public static bool AddRPIKPole(MDagPath locDagPath)
        {
            MObject selected = BasicFunc.GetSelectedObject(0);
            return AddRPIKPole(locDagPath, selected);
        }

        public static bool AddRPIKPole(MDagPath locDagPath, MObject middleObject)
        {
            MDagPath middleDagPath = new MDagPath();
            if (middleObject.hasFn(MFn.Type.kDagNode))
            {                
                MDagPath.getAPathTo(middleObject, middleDagPath);
            }
            return AddRPIKPole(locDagPath, middleDagPath);
        }

        public static bool AddRPIKPole(MDagPath locDagPath, MDagPath middleDagPath)
        {
            MDagPath rootDagPath = null, endDagPath = null;
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

                    double len0 = (middlePos - rootPos).length;
                    double len1 = (endPos - middlePos).length;
                    MVector nmPos = (rootPos * len0 + endPos * len1) * (1 / (len0 + len1));
                    float factor = 2;
                    MVector polePos = factor * middlePos - nmPos;

                    string locName = "loc_" + rootJoint.name + "_" + endJoint.name;
                    return BasicFunc.CreateLocator(locDagPath, polePos, locName);
                }
            }
            return false;
        }






        public static MDagPath BindRPIK()
        {
            MSelectionList selected = new MSelectionList();
            MGlobal.getActiveSelectionList(selected);
            if (selected.length == 3)
            {
                MDagPath rootObject = new MDagPath(), endObject = new MDagPath(), ctlObject = new MDagPath();
                selected.getDagPath(0, rootObject);
                selected.getDagPath(1, endObject);
                selected.getDagPath(2, ctlObject);
                return BindRPIK(rootObject, endObject);
            }
            else if (selected.length == 2)
            {
                MDagPath rootObject = new MDagPath(), endObject = new MDagPath();
                selected.getDagPath(0, rootObject);
                selected.getDagPath(1, endObject);
                return BindRPIK(rootObject, endObject);
            }
            else
            {
                return null;
            }
        }

        public static MDagPath BindRPIK(MDagPath rootDagPath, MDagPath endDagPath)
        {
            MDagPath ctlDagPath = BasicFunc.AddChildCircle(endDagPath);
            BasicFunc.UnparentTransform(ctlDagPath);
            BasicFunc.FreezeTransform(new MFnTransform(ctlDagPath));
            return BindRPIK(rootDagPath, endDagPath, ctlDagPath);
        }

        public static MDagPath BindRPIK(MDagPath rootDagPath, MDagPath endDagPath, MDagPath ctlDagPath)
        {
            /*MFnIkHandle* ikHandle = new MFnIkHandle;
            MStatus status;
            ikHandle.create(rootObject, endObject, &status);

            if (status == MStatus::kSuccess)
            {
                MGlobal.displayInfo("successsssssss" + ikHandle.name());
            }
            ikHandle.findPlug("");
            MFnIkSolver solver(ikHandle.solver());*/
            //solver

            //string resultStr = MGlobal.executeCommandStringResult("ikHandle -sj " + rootObject.fullPathName() + " -ee " + endObject.fullPathName() + " -sol ikRPsolver -n ik_" + rootObject.partialPathName() + "_" + endObject.partialPathName(),true);
            string resultStr = MGlobal.executePythonCommandStringResult("cmds.ikHandle(sj='" + rootDagPath.fullPathName + "',ee='" + endDagPath.fullPathName + "',sol='ikRPsolver',n='ik_" + rootDagPath.partialPathName + "_" + endDagPath.partialPathName + "')");

            //[u'ik_joint1_joint4', u'effector1']
            string[] resultArr = BasicFunc.SplitPythonResultStr(resultStr);
            MGlobal.displayInfo("begin test");
            for (int i = 0; i < resultArr.Length; i++)
            {
                MGlobal.displayInfo(i + ":" + resultArr[i]);
            }
            MGlobal.displayInfo("end test");

            //MGlobal.displayInfo(resultStr);
            /*for (int i = 0; i < msa.length(); i++)
            {
                MGlobal.displayInfo(msa[i]);
            }*/
            /*MGlobal.displayInfo("ikName:" + msa[0]);
            ikDagPath = &(BasicFunc.GetDagPathByName(msa[0]));
            if (ikDagPath != NULL)
            {
                MGlobal.displayInfo(ikDagPath.fullPathName());
            }*/
            MDagPath middleObject = MDagPath.getAPathTo(rootDagPath.child(0));
            MDagPath locDagPath =new MDagPath();
            if (AddRPIKPole(locDagPath, middleObject))
            {
                BasicFunc.FreezeTransform(new MFnTransform(locDagPath));
                //begin to add constriant
                string poleConstraintResult = MGlobal.executeCommandStringResult("poleVectorConstraint " + locDagPath.fullPathName + " " + resultArr[0]);
                //MGlobal.displayInfo(poleConstraintResult);
                MGlobal.executeCommandStringResult("pointConstraint " + ctlDagPath.fullPathName + " " + resultArr[0]);

            }

            return BasicFunc.GetDagPathByName(resultArr[0]);
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
            MDagPath ikDagPath = BindRPIK(legRootDagPath, ankleDagPath, rbs[7]);
            MGlobal.executeCommandStringResult("orientConstraint -mo " + rbs[4].fullPathName + " " + middleDagPath.fullPathName);
            MGlobal.executeCommandStringResult("orientConstraint -mo " + rbs[5].fullPathName + " " + ankleDagPath.fullPathName);

            MGlobal.displayInfo("try delete rbs");

            MGlobal.displayInfo("delete complete");
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

        public static void BindBodySplineIK(MSelectionList jointList)
        {
            //check if all of selected objects are joint
            int count = (int)jointList.length;
            if (count < 2)
            {
                return;
            }
            MDagPath breastJointDagPath, hipJointDagPath;
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

                    if (i == 0)
                    {
                        breastJointDagPath = jtDagPath;
                    }
                    else if (i == count - 1)
                    {
                        hipJointDagPath = jtDagPath;
                    }
                }
                else
                {
                    return;
                }
            }

            //JointProcess.CreateJoint()



        }

#endregion


    }
}
