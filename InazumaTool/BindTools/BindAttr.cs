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
        public static void AddFloatAttr(MDagPath dagPath,string attrLongName, float min = 0, float max = 1, float defaultValue = 0,string shortName = "",bool keyable = true)
        {
            if (shortName.Length == 0)
            {
                shortName = attrLongName;
            }
            //MFnDependencyNode dnode = new MFnDependencyNode(dagPath.node);
            //MFnNumericAttribute na = new MFnNumericAttribute();
            //na.create(attrLongName, shortName, MFnNumericData.Type.kFloat);
            //na.setMin(min);
            //na.setMax(max);
            //na.setDefault(defaultValue);
            //dnode.addAttribute(na);

            if (dagPath != null)
            {
                string cmdStr = string.Format("addAttr -ln {0} -min {1} -max {2} -at \"float\" -dv {3} -k {4}|{5}", attrLongName, min, max, defaultValue, keyable ? 1 : 0, dagPath.fullPathName);
                MGlobal.executeCommand(cmdStr);
            }
        }


        public static MFnBlendShapeDeformer GetBlendShape(MObject targetObject = null)
        {
            if (targetObject == null)
            {
                targetObject = BasicFunc.GetSelectedObject(0);
            }
            MItDependencyGraph mit = new MItDependencyGraph(targetObject, MFn.Type.kBlendShape, MItDependencyGraph.Direction.kDownstream);

            while (!mit.isDone)
            {
                MObject mo = mit.currentItem();
                MFnDependencyNode dnode = new MFnDependencyNode(mo);
                MGlobal.displayInfo("moing:" + dnode.absoluteName);
                if (mo.hasFn(MFn.Type.kBlendShape))
                {
                    MGlobal.displayInfo("find blendshape");
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
                MGlobal.displayInfo("null blendShape");
                return false;
            }
            MGlobal.displayInfo("here i am");

            if (ctlDagPath == null)
            {
                ctlDagPath = BasicFunc.CreateCTL_Crystal("ctl_bs_" + bs.name);
            }
            MPlug weightPlug = bs.findPlug(ConstantValue.plugName_blendShapeWeight);
            int count = (int)weightPlug.numElements;
            MGlobal.displayInfo("target count:" + count);
            for (int i = 0; i < count; i++)
            {
                MPlug singleWeightPlug = weightPlug.elementByLogicalIndex((uint)i);
                string weightName = singleWeightPlug.name.Split('.').Last();
                AddFloatAttr(ctlDagPath, weightName);
                BasicFunc.ConnectAttr(ctlDagPath.fullPathName + "." + weightName, singleWeightPlug.name);
            }


            return true;

        }

        
        const string cmdStr = "BindAttr";
        public static List<CommandData> GetCommandDatas()
        {
            List<CommandData> cmdList = new List<CommandData>();
            cmdList.Add(new CommandData("绑定属性", cmdStr, "bindBlendShape", "绑定混合变形控制器", () =>
            {
                MFnBlendShapeDeformer bs = GetBlendShape();
                //BindBlendShapeCtl(bs);
            }));
            return cmdList;
        }


    }
}
