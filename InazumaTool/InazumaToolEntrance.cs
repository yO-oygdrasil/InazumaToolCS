using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Maya.OpenMaya;
using InazumaTool.BasicTools;
using InazumaTool.BindTools;



[assembly: ExtensionPlugin(typeof(InazumaTool.InazumaToolEntrance), "Any")]
[assembly: MPxCommandClass(typeof(InazumaTool.HelloWorldCmd), "helloWorldCmd")]
[assembly: MPxCommandClass(typeof(InazumaTool.MPCMap), "MPCMap")]

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
            MGlobal.displayInfo("yep");
            return true;
        }

        bool IExtensionPlugin.UninitializePlugin()
        {
            MGlobal.displayInfo("nope");
            return true;
        }

    }

    public class HelloWorldCmd : MPxCommand, IMPxCommand
    {
        public override void doIt(MArgList argl)
        {
            MGlobal.displayInfo("Hello World\n");
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
            CreateJointsCurve = 10
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
                        MSelectionList list = BasicTools.BasicFunc.GetSelectedDagPaths();
                        for (int i = 0; i < list.length; i++)
                        {
                            uint index = (uint)i;
                            MGlobal.displayInfo("index:" + index);
                            MDagPath mdp = new MDagPath();
                            list.getDagPath(index, mdp);
                            MGlobal.displayInfo("name:" + mdp.fullPathName);
                        }
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
                        MDagPath locDagPath = new MDagPath();
                        BindHumanBody.AddRPIKPole(locDagPath);
                        MGlobal.displayInfo("add rpik pole loc");
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
                        JointProcess.CreateJointsCurve(BasicFunc.GetSelectedDagPaths());
                        break;
                    }
            }
        }
    }

}
