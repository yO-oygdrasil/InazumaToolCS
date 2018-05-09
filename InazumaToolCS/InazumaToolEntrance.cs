using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Maya.OpenMaya;


[assembly: ExtensionPlugin(typeof(InazumaTool.InazumaToolEntrance), "Any")]
[assembly: MPxCommandClass(typeof(InazumaTool.HelloWorldCmd), "helloWorldCmd")]

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
}
