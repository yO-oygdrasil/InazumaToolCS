using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Maya.OpenMayaUI;
using Autodesk.Maya.OpenMaya;
using InazumaTool.BasicTools;
using InazumaTool.BindTools;



[assembly: ExtensionPlugin(typeof(InazumaTool.InazumaToolEntrance), "Any")]
[assembly: MPxCommandClass(typeof(InazumaTool.MPCMap), "InazumaCommand")]

namespace InazumaTool
{
    public class InazumaToolEntrance : IExtensionPlugin
    {

        public string GetMayaDotNetSdkBuildVersion()
        {
            String version = "Inazuma Ver 0.1";
            return version;
        }

        public static Dictionary<string, string> subMenuDic = new Dictionary<string, string>();
        //commandStr,dic<paramStr,action>
        public static Dictionary<string, Dictionary<string, Action>> commandDic = new Dictionary<string, Dictionary<string, Action>>();
        void AddOneCommand(CommandData cd)
        {
            //MGlobal.displayInfo("try add:" + cd.DebugMessage());
            string realSubMenuName = "";
            if (cd.subMenuName == null)
            {
                realSubMenuName = totalMenuName;
            }
            else
            {
                if (subMenuDic.ContainsKey(cd.subMenuName))
                {
                    realSubMenuName = subMenuDic[cd.subMenuName];
                }
                else
                {
                    realSubMenuName = AddSubMenu(totalMenuName, cd.subMenuName, true);
                    subMenuDic.Add(cd.subMenuName, realSubMenuName);
                }
            }            
            Dictionary<string, Action> paramActionDic;
            if (commandDic.ContainsKey(cd.cmdTypeStr))
            {
                paramActionDic = commandDic[cd.cmdTypeStr];                
            }
            else
            {
                paramActionDic = new Dictionary<string, Action>();
                commandDic.Add(cd.cmdTypeStr, paramActionDic);
            }
            if (paramActionDic.ContainsKey(cd.paramStr))
            {
                MGlobal.displayWarning("Repeated Command Param:" + cd.paramStr);                
                while (paramActionDic.ContainsKey(cd.paramStr))
                {
                    cd.paramStr += "_r";
                }
                MGlobal.displayWarning("Rename to:" + cd.paramStr);
            }
            paramActionDic.Add(cd.paramStr, cd.action);

            AddMenuItem(cd.labelStr, realSubMenuName, "InazumaCommand", cd.cmdTypeStr + " " + cd.paramStr);

        }

        public static bool Execute(string cmdTypeStr,string paramStr)
        {
            if (commandDic.ContainsKey(cmdTypeStr))
            {
                Dictionary<string, Action> paramDic = commandDic[cmdTypeStr];
                if (paramDic.ContainsKey(paramStr))
                {
                    paramDic[paramStr]();
                    return true;
                }
            }
            return false;
        }


        public static List<CommandData> cds = new List<CommandData>();
        string totalMenuName;
        bool IExtensionPlugin.InitializePlugin()
        {
            totalMenuName = GetMayaWindowName();
            cds.AddRange(BasicFunc.GetCommandDatas());
            cds.AddRange(BindHumanBody.GetCommandDatas());
            cds.AddRange(DynamicConverter.GetCommandDatas());
            cds.AddRange(JointProcess.GetCommandDatas());
            cds.AddRange(MaterialManage.GetCommandDatas());
            cds.AddRange(BindAttr.GetCommandDatas());
            foreach (CommandData cd in cds)
            {
                AddOneCommand(cd);
            }


            //int paramInt = (int)MPCMap.MPCType.Test;
            //AddMenuItem("Test", totalMenuName, "InazumaCommand", paramInt);

            //string subMenuName_bodyBind = AddSubMenu(totalMenuName, "Body Bind", true);
            //string subMenuName_create = AddSubMenu(totalMenuName, "Create", true);

            //paramInt = (int)MPCMap.MPCType.AddRPIK;
            //AddMenuItem("add rpik", subMenuName_bodyBind, "InazumaCommand", paramInt);

            //paramInt = (int)MPCMap.MPCType.AddRPIKPole;
            //AddMenuItem("add rpik pole", subMenuName_create, "InazumaCommand", paramInt);

            //paramInt = (int)MPCMap.MPCType.CreateCTL_CrysTal;
            //AddMenuItem("create cystal ctl", subMenuName_create, "InazumaCommand", paramInt);

            //paramInt = (int)MPCMap.MPCType.AddChildCtl;
            //AddMenuItem("add child ctl", subMenuName_create, "InazumaCommand", paramInt);

            //paramInt = (int)MPCMap.MPCType.AddParentCtl;
            //AddMenuItem("add parent ctl", subMenuName_create, "InazumaCommand", paramInt);

            //paramInt = (int)MPCMap.MPCType.AddReverseFootBones;
            //AddMenuItem("add reverse foot bones", subMenuName_create, "InazumaCommand", paramInt);

            //paramInt = (int)MPCMap.MPCType.BindReverseFootRPIK;
            //AddMenuItem("Bind Reverse Foot RPIK", subMenuName_bodyBind, "InazumaCommand", paramInt);

            //paramInt = (int)MPCMap.MPCType.BindFinger_CTL_L;
            //AddMenuItem("Bind Finger using CTL L", subMenuName_bodyBind, "InazumaCommand", paramInt);

            //paramInt = (int)MPCMap.MPCType.CreateJointsCurve;
            //AddMenuItem("Create Joints Curve", subMenuName_create, "InazumaCommand", paramInt);

            //paramInt = (int)MPCMap.MPCType.MakeHairJointsChain;
            //AddMenuItem("Make Hair Joints To Chain", subMenuName_bodyBind, "InazumaCommand", paramInt);
            MGlobal.displayInfo("-回港了-");
            return true;
        }

        bool IExtensionPlugin.UninitializePlugin()
        {
            //MFnPluginData plugin = new MFnPluginData();
            subMenuDic.Clear();
            commandDic.Clear();
            cds.Clear();


            string menus = MGlobal.executePythonCommandStringResult("cmds.window(mel.eval('$temp1=$gMainWindow'), q=True, menuArray=True)");
            //BasicFunc.Print(menus);
            menus.TrimStart('[');
            menus.TrimEnd(']');
            string[] menuNames = menus.Split(',');
            for (int i = 0; i < menuNames.Length; i++)
            {
                //BasicFunc.Print(menuNames[i]); 
                string labelName = MGlobal.executePythonCommandStringResult("cmds.menu(" + menuNames[i] + ",q=True,label=True)");
                if (labelName == "InazumaTool")
                {
                    MGlobal.executePythonCommandOnIdle("cmds.deleteUI(" + menuNames[i] + ",m=True)");
                }
            }
            MGlobal.displayInfo("-远征去了-");
            return true;
        }

        string GetMayaWindowName()
        {
            MGlobal.executePythonCommand("import maya.cmds as cmds");
            string totalMenuName = MGlobal.executePythonCommandStringResult("cmds.menu(parent=mel.eval('$temp1=$gMainWindow'), label='InazumaTool',tearOff = True)");
            return totalMenuName;
        }

        string AddSubMenu(string parentMenuName, string labelStr, bool tearOff)
        {
            MGlobal.executePythonCommand("import maya.cmds as cmds");
            string cmdStr = (tearOff ? "cmds.menuItem(tearOff = True, parent='" : "cmds.menuItem(parent='") + parentMenuName + "',subMenu = True, label='" + labelStr + "')";
            //MGlobal.displayInfo("cmdStr:" + cmdStr);
            string subMenuName = MGlobal.executePythonCommandStringResult(cmdStr);
            return subMenuName;
        }

        void AddMenuItem(string label, string parentMenuName, string command, int paramInt)
        {
            string cmdStr = "menuItem -l \"" + label + "\" -p \"" + parentMenuName + "\" -c \"" + command + " " + paramInt + "\"";
            //MGlobal.displayInfo(cmdStr);
            MGlobal.executeCommandOnIdle(cmdStr);
            //MGlobal.executeCommand(cmdStr);
        }

        void AddMenuItem(string label, string parentMenuName, string command, string paramStr)
        {
            string cmdStr = "menuItem -l \"" + label + "\" -p \"" + parentMenuName + "\" -c \"" + command + " " + paramStr + "\"";
            //MGlobal.displayInfo(cmdStr);
            MGlobal.executeCommandOnIdle(cmdStr);
            //MGlobal.executeCommand(cmdStr);
        }

    }
    
    public class MPCMap : MPxCommand 
    {
        //public enum MPCType
        //{
        //    Test = 0,
        //    BindFinger_CTL_L = 1,
        //    BindFinger_CTL_R = 2,
        //    AddRPIK = 3,
        //    AddRPIKPole = 4,
        //    AddChildCtl = 5,
        //    AddParentCtl = 6,
        //    CreateCTL_CrysTal = 7,
        //    AddReverseFootBones = 8,
        //    BindReverseFootRPIK = 9,
        //    CreateJointsCurve = 10,
        //    MakeHairJointsChain = 11,

        //}
        
        public override void doIt(MArgList args)
        {
            
            MGlobal.displayInfo("at least it enters");
            if (args.length == 0)
            {
                MGlobal.displayInfo("no param!");
                return;
            }
            string cmdTypeStr = args.asString(0);
            string paramStr = args.asString(1);
            bool success = InazumaToolEntrance.Execute(cmdTypeStr, paramStr);
            if (success)
            {
                MGlobal.displayInfo("yep");
            }
            else
            {
                MGlobal.displayInfo("nope");
            }




            //int type = args.asInt(0);

            //MPCType mpcType = (MPCType)type;
            

            //switch (mpcType)
            //{
            //    case MPCType.Test:
            //        {
            //            BasicFunc.PrintObjects(BasicFunc.GetSelectedList());
                       
            //            break;
            //        }
            //    case MPCType.BindFinger_CTL_L:
            //        {
            //            BindHumanBody.BindFinger(BasicFunc.GetSelectedDagPath(0), "test", false);
            //            break;
            //        }
            //    case MPCType.BindFinger_CTL_R:
            //        {
            //            MGlobal.displayInfo("oh my god it works333");
            //            break;
            //        }
            //    case MPCType.AddRPIK:
            //        {
            //            BindHumanBody.BindRPIK();
            //            break;
            //        }
            //    case MPCType.AddRPIKPole:
            //        {
            //            BindHumanBody.AddRPIKPole();
            //            break;
            //        }
            //    case MPCType.AddChildCtl:
            //        {
            //            BasicFunc.AddChildCircle(BasicFunc.GetSelectedDagPath(0));
            //            break;
            //        }
            //    case MPCType.AddParentCtl:
            //        {
            //            BasicFunc.AddParentCircle(BasicFunc.GetSelectedDagPath(0), true);
            //            break;
            //        }
            //    case MPCType.CreateCTL_CrysTal:
            //        {
            //            BasicFunc.CreateCTL_Crystal("ctl_sample");
            //            break;
            //        }
            //    case MPCType.AddReverseFootBones:
            //        {
            //            BindHumanBody.AddReverseFootBone();
            //            break;
            //        }
            //    case MPCType.BindReverseFootRPIK:
            //        {
            //            BindHumanBody.BindReverseFootRPIK();
            //            break;
            //        }
            //    case MPCType.CreateJointsCurve:
            //        {
            //            JointProcess.CreateJointsCurve(BasicFunc.GetSelectedList());
            //            break;
            //        }
            //    case MPCType.MakeHairJointsChain:
            //        {
            //            DynamicConverter.CurveToHair();

            //            //JointProcess.MakeJointsHairChain(BasicFunc.GetSelectedList());
            //            break;
            //        }
            //}
        }
    }

}
