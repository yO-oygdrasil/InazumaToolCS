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
        public static MFnBlendShapeDeformer GetBlendShape()
        {
            MItDependencyGraph mit = new MItDependencyGraph(BasicFunc.GetSelectedObject(0), MFn.Type.kBlendShape, MItDependencyGraph.Direction.kDownstream);
            MObject mo = new MObject();
            while (!mit.isDone)
            {
                mo = mit.currentItem();
                MFnDependencyNode node = new MFnDependencyNode(mo);
                MGlobal.displayInfo("find:" + node.absoluteName);
                mit.next();
            }

            return null;

        }



        const string cmdStr = "BindAttr";
        public static List<CommandData> GetCommandDatas()
        {
            List<CommandData> cmdList = new List<CommandData>();
            cmdList.Add(new CommandData("绑定属性", cmdStr, "bindBlendShape", "绑定混合变形控制器", () =>
            {
                GetBlendShape();
            }));
            
            return cmdList;
        }


    }
}
