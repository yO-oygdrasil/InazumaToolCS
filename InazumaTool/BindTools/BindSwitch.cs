using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Maya.OpenMaya;
using Autodesk.Maya.OpenMayaFX;
using InazumaTool.BasicTools;

namespace InazumaTool.BindTools
{
    public static class BindSwitch
    {
        public static void BindNCloth(MSelectionList selecteList = null)
        {
            if (selecteList == null)
            {
                selecteList = BasicFunc.GetSelectedList();
            }
            int count = (int)selecteList.length;
            if (count <= 1)
            {
                return;
            }
            MDagPath ctlDag = new MDagPath();
            selecteList.getDagPath(0, ctlDag);
            selecteList.remove(0);
            MFnDependencyNode dn_ctl = new MFnDependencyNode(ctlDag.node);
            BasicFunc.IterateSelectedDags((dag) =>
            {
                MPlug ctlNewAttrPlug = BindAttr.AddBoolAttr(dn_ctl, "Dynamic_" + dag.partialPathName, true);
                MFnDependencyNode dn_nCloth = new MFnDependencyNode(dag.node);
                MPlug plug_isDynamic = dn_nCloth.findPlug(ConstantValue.plugName_nCloth_isDynamic);
                MPlug plug_currentTime = dn_nCloth.findPlug(ConstantValue.plugName_nCloth_currentTime);
                if (plug_isDynamic != null && plug_currentTime != null)
                {
                    MPlugArray inputArr_currentTime = new MPlugArray();
                    plug_currentTime.connectedTo(inputArr_currentTime, true, false);
                    MFnDependencyNode dn_time = new MFnDependencyNode(inputArr_currentTime[0].node);

                    BindAttr.ProjectPlug(plug_isDynamic, dn_time.findPlug(ConstantValue.plugName_nClothTime_nodeState), 0, 1, (int)ConstantValue.TimeNodeState.Stuck, (int)ConstantValue.TimeNodeState.Normal);
                }
                BasicFunc.ConnectPlug(ctlNewAttrPlug, plug_isDynamic);

            }, MFn.Type.kNCloth, selecteList);


            //List<string> nclothNameList = new List<string>();
            //foreach (MDagPath dag in nclothList.DagPaths())
            //{
            //    nclothNameList.Add()
            //}
        }

        public static void StuckNClothTimeWhenNotDynamic(MFnDependencyNode dn)
        {
            MPlug plug_isDynamic = dn.findPlug(ConstantValue.plugName_nCloth_isDynamic);
            MPlug plug_currentTime = dn.findPlug(ConstantValue.plugName_nCloth_currentTime);
            if (plug_isDynamic != null && plug_currentTime != null)
            {
                MPlugArray inputArr_currentTime = new MPlugArray();
                plug_currentTime.connectedTo(inputArr_currentTime, true, false);
                MFnDependencyNode dn_time = new MFnDependencyNode(inputArr_currentTime[0].node);

                BindAttr.ProjectPlug(plug_isDynamic, dn_time.findPlug(ConstantValue.plugName_nClothTime_nodeState), 0, 1, (int)ConstantValue.TimeNodeState.Stuck, (int)ConstantValue.TimeNodeState.Normal);

            }
        }


        const string cmdStr = "BindSwitch";
        public static List<CommandData> GetCommandDatas()
        {
            List<CommandData> cmdList = new List<CommandData>();
            cmdList.Add(new CommandData("绑定切换", cmdStr, "switchNCloth", "添加nCloth到纯蒙皮的切换", () =>
            {
                BindNCloth();
            }));
            cmdList.Add(new CommandData("绑定切换", cmdStr, "testNClothNodeRecognize", "测试寻找ncloth节点", () =>
            {

            }));
            return cmdList;
        }
    }
}
