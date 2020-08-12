using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Maya.OpenMaya;
using Autodesk.Maya.OpenMayaAnim;
using InazumaTool.BasicTools;

namespace InazumaTool.BindTools
{
    public static class BindAttr
    {
        public static MPlug AddFloatAttr(MDagPath dagPath,string attrLongName, float min = 0, float max = 1, float defaultValue = 0,string shortName = "",bool keyable = true)
        {
            if (shortName.Length == 0)
            {
                shortName = attrLongName;
            }
            MFnDependencyNode dnode = new MFnDependencyNode(dagPath.node);
            MFnNumericAttribute na = new MFnNumericAttribute();
            na.create(attrLongName, shortName, MFnNumericData.Type.kFloat);
            na.setMin(min);
            na.setMax(max);
            na.setDefault(defaultValue);
            na.isKeyable = keyable;
            dnode.addAttribute(na.objectProperty);
            return dnode.findPlug(attrLongName);
            //if (dagPath != null)
            //{
            //    string cmdStr = string.Format("addAttr -ln {0} -min {1} -max {2} -at \"float\" -dv {3} -k {4} {5}", attrLongName, min, max, defaultValue, keyable ? 1 : 0, dagPath.fullPathName);
            //    Debug.Log("cmdStr:" + cmdStr);
            //    MGlobal.executeCommand(cmdStr);
            //}
        }

        public static MPlug AddBoolAttr(MDagPath dagPath, string attrLongName, bool defaultValue = true, string shortName = "", bool keyable = true)
        {
            return AddBoolAttr(new MFnDependencyNode(dagPath.node), attrLongName, defaultValue, shortName, keyable);

            //if (shortName.Length == 0)
            //{
            //    shortName = attrLongName;
            //}
            //MFnDependencyNode dnode = new MFnDependencyNode(dagPath.node);
            //MFnNumericAttribute na = new MFnNumericAttribute();
            //na.create(attrLongName, shortName, MFnNumericData.Type.kBoolean);
            //na.setDefault(defaultValue);
            //na.isKeyable = keyable;
            //dnode.addAttribute(na.objectProperty);
            //return dnode.findPlug(attrLongName);
        }

        public static MPlug AddBoolAttr(MFnDependencyNode dnode, string attrLongName, bool defaultValue = true, string shortName = "", bool keyable = true)
        {
            if (shortName.Length == 0)
            {
                shortName = attrLongName;
            }
            MFnNumericAttribute na = new MFnNumericAttribute();
            na.create(attrLongName, shortName, MFnNumericData.Type.kBoolean);
            na.setDefault(defaultValue);
            na.isKeyable = keyable;
            na.setChannelBox(true);
            dnode.addAttribute(na.objectProperty);
            return dnode.findPlug(attrLongName);
            //if (dagPath != null)
            //{
            //    string cmdStr = string.Format("addAttr -ln {0} -min {1} -max {2} -at \"float\" -dv {3} -k {4} {5}", attrLongName, min, max, defaultValue, keyable ? 1 : 0, dagPath.fullPathName);
            //    Debug.Log("cmdStr:" + cmdStr);
            //    MGlobal.executeCommand(cmdStr);
            //}
        }

        public static bool ProjectPlug(MPlug from, MPlug to, float fromMin, float fromMax, float toMin, float toMax)
        {
            MFnDependencyNode remapValueNode = BasicFunc.CreateRemapValueNode(fromMin, fromMax, toMin, toMax);
            MDGModifier dGModifier = new MDGModifier();
            dGModifier.connect(from, remapValueNode.findPlug(ConstantValue.plugName_remapValueInput));
            dGModifier.connect(remapValueNode.findPlug(ConstantValue.plugName_remapValueOutput), to);
            dGModifier.doIt();
            return true;
        }

        public static MFnBlendShapeDeformer GetBlendShape(MObject targetObject = null)
        {
            if (targetObject == null)
            {
                targetObject = BasicFunc.GetSelectedObject(0);
            }
            MDagPath dag_target = MDagPath.getAPathTo(targetObject);
            MFnDependencyNode node_target = new MFnDependencyNode(targetObject);
            MPlug plug = node_target.findPlug("inMesh");
            Debug.Log("node_target:" + node_target.name+" plug:"+ plug.name);
            MItDependencyGraph mit = new MItDependencyGraph(plug, MFn.Type.kBlendShape, MItDependencyGraph.Direction.kUpstream);
            //MDagPath dagPath = new MDagPath();
            //MItDependencyNodes mitnode = new MItDependencyNodes(MFn.Type.kBlendShape);


            while (!mit.isDone)
            {
                MObject mo = mit.currentItem();
                MFnDependencyNode dnode = new MFnDependencyNode(mo);
                Debug.Log("moing:" + dnode.absoluteName);
                if (mo.hasFn(MFn.Type.kBlendShape))
                {
                    Debug.Log("find blendshape");
                    return new MFnBlendShapeDeformer(mo);

                }
                mit.next();
            }

            return null;

        }

        public static bool BindBlendShapeCtl(MFnBlendShapeDeformer bs, MDagPath ctlDagPath = null)
        {
            //MFnDependencyNode ctlNode = new MFnDependencyNode(ctlDagPath.node);
            if (bs == null)
            {
                Debug.Log("null blendShape");
                return false;
            }
            //Debug.Log("here i am");

            if (ctlDagPath == null)
            {
                ctlDagPath = BasicFunc.CreateCTL_Crystal("ctl_bs_" + bs.name);
            }

            MFnDependencyNode ctlNode = new MFnDependencyNode(ctlDagPath.node);

            MPlug weightPlug = bs.findPlug(ConstantValue.plugName_blendShapeWeight);
            int count = (int)weightPlug.numElements;
            //Debug.Log("target count:" + count);

            MDGModifier dGModifier = new MDGModifier();
            for (int i = 0; i < count; i++)
            {
                //Debug.Log("process:" + i);
                MPlug singleWeightPlug = weightPlug.elementByLogicalIndex((uint)i);
                string weightName = singleWeightPlug.name.Split('.').Last();
                MPlug ctlAttrPlug = AddFloatAttr(ctlDagPath, weightName);
                dGModifier.connect(ctlAttrPlug, singleWeightPlug);
            }
            dGModifier.doIt();

            return true;

        }

        static void ExecuteSeqForBlendShape(MFnBlendShapeDeformer bs, Action<int,string> dealMethod, float selectWeight = 1, float restWeight = 0)
        {
            MPlug weightPlug = bs.findPlug(ConstantValue.plugName_blendShapeWeight);
            int count = (int)weightPlug.numElements;

            MDGModifier dGModifier = new MDGModifier();
            for (int i = 0; i < count; i++)
            {
                //Debug.Log("process:" + i);
                MPlug singleWeightPlug = weightPlug.elementByLogicalIndex((uint)i);
                string weightName = singleWeightPlug.name.Split('.').Last();
                singleWeightPlug.setFloat(selectWeight);
                dealMethod?.Invoke(i,weightName);
                singleWeightPlug.setFloat(restWeight);
            }
            dGModifier.doIt();
        }


        static void BlendShapeShout(MFnBlendShapeDeformer bs, MDagPath deformTarget, float xMove = 0.5f)
        {
            ExecuteSeqForBlendShape(bs,(morphIndex,weightName)=>
            {
                var newDag = BasicFunc.Duplicate(deformTarget, deformTarget.partialPathName + "_emo_" + weightName);
                var newTrans = new MFnTransform(newDag);
                if (xMove > 0)
                {
                    newTrans.setTranslation(newTrans.getTranslation(MSpace.Space.kWorld) + new MVector(xMove * (morphIndex + 1), 0, 0), MSpace.Space.kWorld);
                }

            });
        }

        
        const string cmdStr = "BindAttr";
        public static List<CommandData> GetCommandDatas()
        {
            List<CommandData> cmdList = new List<CommandData>();
            cmdList.Add(new CommandData("绑定属性", cmdStr, "bindBlendShape", "绑定混合变形控制器", () =>
            {
                MFnBlendShapeDeformer bs = GetBlendShape();
                BindBlendShapeCtl(bs);
            }));
            cmdList.Add(new CommandData("绑定属性", cmdStr, "blendShapeShout", "混合变形序列生成（混合变形 目标物体）", () =>
            {
                MFnBlendShapeDeformer bs = GetBlendShape();
                BlendShapeShout(bs, BasicFunc.GetSelectedDagPath(1));

            }));
            return cmdList;
        }


    }
}
