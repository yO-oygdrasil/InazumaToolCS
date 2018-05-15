using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Maya.OpenMaya;
using Autodesk.Maya.OpenMayaUI;

namespace InazumaTool.BasicTools
{
    public static class MaterialManage
    {



        public static int SelectMaterialWithSameTex(MObject imageObject)
        {
            MImage img = new MImage();
            img.readFromTextureNode(imageObject, MImage.MPixelType.kUnknown);
            MFnDependencyNode imageNode = new MFnDependencyNode(imageObject);
            MPlug plug = imageNode.findPlug(ConstantValue.plugName_fileTexPath);
            MPlugArray destPlugs = new MPlugArray();
            plug.destinations(destPlugs);
            BasicFunc.PrintPlugs(destPlugs);


            return 0;
        }

        //public static int SelectMaterialWithSameTex(MImage img)
        //{
            


        //    return null;
        //}



    }
}
