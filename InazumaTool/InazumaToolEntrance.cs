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


        bool IExtensionPlugin.InitializePlugin()
        {
            string totalMenuName = GetMayaWindowName();
            int paramInt = (int)MPCMap.MPCType.Test;
            AddMenuItem("Test", totalMenuName, "InazumaCommand", paramInt);

            string subMenuName_bodyBind = AddSubMenu(totalMenuName, "Body Bind", true);
            string subMenuName_create = AddSubMenu(totalMenuName, "Create", true);

            paramInt = (int)MPCMap.MPCType.AddRPIK;
            AddMenuItem("add rpik", subMenuName_bodyBind, "InazumaCommand", paramInt);

            paramInt = (int)MPCMap.MPCType.AddRPIKPole;
            AddMenuItem("add rpik pole", subMenuName_create, "InazumaCommand", paramInt);

            paramInt = (int)MPCMap.MPCType.CreateCTL_CrysTal;
            AddMenuItem("create cystal ctl", subMenuName_create, "InazumaCommand", paramInt);

            paramInt = (int)MPCMap.MPCType.AddChildCtl;
            AddMenuItem("add child ctl", subMenuName_create, "InazumaCommand", paramInt);

            paramInt = (int)MPCMap.MPCType.AddParentCtl;
            AddMenuItem("add parent ctl", subMenuName_create, "InazumaCommand", paramInt);

            paramInt = (int)MPCMap.MPCType.AddReverseFootBones;
            AddMenuItem("add reverse foot bones", subMenuName_create, "InazumaCommand", paramInt);

            paramInt = (int)MPCMap.MPCType.BindReverseFootRPIK;
            AddMenuItem("Bind Reverse Foot RPIK", subMenuName_bodyBind, "InazumaCommand", paramInt);

            paramInt = (int)MPCMap.MPCType.BindFinger_CTL_L;
            AddMenuItem("Bind Finger using CTL L", subMenuName_bodyBind, "InazumaCommand", paramInt);

            paramInt = (int)MPCMap.MPCType.CreateJointsCurve;
            AddMenuItem("Create Joints Curve", subMenuName_create, "InazumaCommand", paramInt);

            paramInt = (int)MPCMap.MPCType.MakeHairJointsChain;
            AddMenuItem("Make Hair Joints To Chain", subMenuName_bodyBind, "InazumaCommand", paramInt);

            return true;
        }

        bool IExtensionPlugin.UninitializePlugin()
        {
            //MFnPluginData plugin = new MFnPluginData();
            

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
                    MGlobal.executePythonCommand("cmds.deleteUI(" + menuNames[i] + ",m=True)");
                }
            }

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
            string subMenuName = MGlobal.executePythonCommandStringResult((tearOff ? "cmds.menuItem(tearOff = True, parent='" : "cmds.menuItem(parent='") + parentMenuName + "',subMenu = True, label='" + labelStr + "')");
            return subMenuName;
        }

        void AddMenuItem(string label, string parentMenuName, string command, int paramInt)
        {
            string cmdStr = "menuItem -l \"" + label + "\" -p \"" + parentMenuName + "\" -c \"" + command + " " + paramInt + "\"";
            //MGlobal.displayInfo(cmdStr);
            //MGlobal.executeCommandOnIdle(cmdStr);
            MGlobal.executeCommand(cmdStr);
        }

        void AddMenuItem(string label, string parentMenuName, string command, string paramStr)
        {
            string cmdStr = "menuItem -l \"" + label + "\" -p \"" + parentMenuName + "\" -c \"" + command + " " + paramStr + "\"";
            //MGlobal.displayInfo(cmdStr);
            //MGlobal.executeCommandOnIdle(cmdStr);
            MGlobal.executeCommand(cmdStr);
        }

    }
    
    public class MPCMap : MPxCommand 
    {
        public enum MPCType
        {
            Test = 0,
            BindFinger_CTL_L = 1,
            BindFinger_CTL_R = 2,
            AddRPIK = 3,
            AddRPIKPole = 4,
            AddChildCtl = 5,
            AddParentCtl = 6,
            CreateCTL_CrysTal = 7,
            AddReverseFootBones = 8,
            BindReverseFootRPIK = 9,
            CreateJointsCurve = 10,
            MakeHairJointsChain = 11
        }
        
        public override void doIt(MArgList args)
        {
            
            MGlobal.displayInfo("at least it enters");
            if (args.length == 0)
            {
                MGlobal.displayInfo("no param!");
                return;
            }
            int type = args.asInt(0);

            MPCType mpcType = (MPCType)type;


            switch (mpcType)
            {
                case MPCType.Test:
                    {
                        BasicFunc.PrintObjects(BasicFunc.GetSelectedList());
                       
                        break;
                    }
                case MPCType.BindFinger_CTL_L:
                    {
                        BindHumanBody.BindFinger(BasicFunc.GetSelectedDagPath(0), "test", false);
                        break;
                    }
                case MPCType.BindFinger_CTL_R:
                    {
                        MGlobal.displayInfo("oh my god it works333");
                        break;
                    }
                case MPCType.AddRPIK:
                    {
                        BindHumanBody.BindRPIK();
                        break;
                    }
                case MPCType.AddRPIKPole:
                    {
                        BindHumanBody.AddRPIKPole();
                        break;
                    }
                case MPCType.AddChildCtl:
                    {
                        BasicFunc.AddChildCircle(BasicFunc.GetSelectedDagPath(0));
                        break;
                    }
                case MPCType.AddParentCtl:
                    {
                        BasicFunc.AddParentCircle(BasicFunc.GetSelectedDagPath(0), true);
                        break;
                    }
                case MPCType.CreateCTL_CrysTal:
                    {
                        BasicFunc.CreateCTL_Crystal("ctl_sample");
                        break;
                    }
                case MPCType.AddReverseFootBones:
                    {
                        BindHumanBody.AddReverseFootBone();
                        break;
                    }
                case MPCType.BindReverseFootRPIK:
                    {
                        BindHumanBody.BindReverseFootRPIK();
                        break;
                    }
                case MPCType.CreateJointsCurve:
                    {
                        JointProcess.CreateJointsCurve(BasicFunc.GetSelectedList());
                        break;
                    }
                case MPCType.MakeHairJointsChain:
                    {
                        DynamicConverter.CurveToHair();

                        //JointProcess.MakeJointsHairChain(BasicFunc.GetSelectedList());
                        break;
                    }
            }
        }
    }

}
