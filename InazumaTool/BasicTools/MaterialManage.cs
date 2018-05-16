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

        public static bool SelectObjectsWithMat(MFnDependencyNode matNode)
        {
            if (matNode == null)
            {
                return false;
            }
            MGlobal.executeCommandOnIdle("hyperShade -objects " + matNode.absoluteName);
            return true;
        }

        /// <summary>
        /// well, this action is truely dangerous
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static bool CombineMaterials(MSelectionList list = null)
        {
            if (list == null)
            {
                list = BasicFunc.GetSelectedList();
            }
            for (uint i = 0; i < list.length; i++)
            {
                MObject matObject = new MObject();
                list.getDependNode(i, matObject);
                MFnDependencyNode dnode = new MFnDependencyNode(matObject);
                if (matObject.hasFn(MFn.Type.kMaterial))
                {
                    MMaterial mat = new MMaterial(matObject);
                    MColor color = new MColor();
                    mat.getDiffuse(color);
                    MGlobal.displayInfo("mat:" + dnode.absoluteName + " ,color:" + BasicFunc.MToString(color));
                }
                
            }




            //MGlobal.executeCommandOnIdle("hyperShade -objects " + matNode.absoluteName);
            return true;
        }

        public static int SelectMaterialWithSameTex(MObject imageObject)
        {
            MImage img = new MImage();
            img.readFromTextureNode(imageObject, MImage.MPixelType.kUnknown);
            MFnDependencyNode imageNode = new MFnDependencyNode(imageObject);
            MPlug plug = imageNode.findPlug(ConstantValue.plugName_fileTexOutput);
            MPlugArray destPlugs = new MPlugArray();
            plug.destinations(destPlugs);
            BasicFunc.PrintPlugs(destPlugs);

            MSelectionList newSelection = new MSelectionList();
            for (int i = 0; i < destPlugs.length; i++)
            {
                newSelection.add(destPlugs[i].node);
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
            cmdList.Add(new CommandData("Select", cmdStr, "objectsWithMats", "Select Objects With Selected Materials", () =>
            {
                SelectMaterialWithSameTex(BasicFunc.GetSelectedObject(0));
            }));
            return cmdList;
        }

    }
}
