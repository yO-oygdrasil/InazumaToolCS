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
        public static void BindNCloth(MSelectionList nclothList = null)
        {
            if (nclothList == null)
            {
                nclothList = BasicFunc.GetSelectedList();
            }
            int count = (int)nclothList.length;
            if (count > 1)
            {
                
            }

        }

        public static void TestNClothNodeRecognize()
        {
            BasicFunc.IterateSelectedDags((dag) =>
            {
                Debug.Log(dag.fullPathName);
            }, MFn.Type.kNCloth);
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
                TestNClothNodeRecognize();
            }));
            return cmdList;
        }
    }
}
