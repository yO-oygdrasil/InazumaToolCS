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

            MSelectionList newSelection = new MSelectionList();
            for (int i = 0; i < destPlugs.length; i++)
            {
                newSelection.add(destPlugs[i]);
            }
            BasicFunc.Select(newSelection);
            return 0;
        }

        
        const string cmdStr = "MaterialManage";
        public static List<CommandData> GetCommandDatas()
        {
            List<CommandData> cmdList = new List<CommandData>();
            cmdList.Add(new CommandData("Select", cmdStr, "matsWithSameTex", "Select Materials With Same Tex", () =>
            {
                SelectMaterialWithSameTex(BasicFunc.GetSelectedObject(0));
            }));
            return cmdList;
        }

    }
}
