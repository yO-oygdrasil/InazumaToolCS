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

        static bool SelectObjectsWithMat(MFnDependencyNode matNode)
        {
            if (matNode == null)
            {
                return false;
            }            
            return SelectObjectsWithMat(matNode.absoluteName);
        }

        static bool SelectObjectsWithMat(string matName)
        {
            MGlobal.executeCommandOnIdle("hyperShade -objects " + matName);
            return true;
        }



        static void AssignMat(string matName)
        {
            MGlobal.executeCommandOnIdle("hyperShade -assign " + matName);
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
            if (list.length <= 1)
            {
                MGlobal.displayInfo("please choose at least 2 materials");
                return false;
            }
            string firstMatName = "";

            for (uint i = 0; i < list.length; i++)
            {
                //MGlobal.displayInfo(i + " mat test");
                MObject matObject = new MObject();
                list.getDependNode(i, matObject);
                MFnDependencyNode dnode = new MFnDependencyNode(matObject);
                if (i == 0)
                {
                    firstMatName = dnode.absoluteName;
                    continue;
                }
                //MGlobal.displayInfo(i + " node:" + dnode.absoluteName);
                if (matObject.hasFn(MFn.Type.kLambert) || matObject.hasFn(MFn.Type.kBlinn) || matObject.hasFn(MFn.Type.kPhong))
                {
                    MGlobal.displayInfo("has mat fn");
                    //MMaterial mat = new MMaterial(matObject);
                    //MColor color = new MColor();
                    //mat.getDiffuse(color);
                    //MGlobal.displayInfo("mat:" + dnode.absoluteName + " ,color:" + BasicFunc.MToString(color));
                    SelectObjectsWithMat(dnode);
                    AssignMat(firstMatName);
                }
                else
                {
                    MGlobal.displayInfo("no mat fn");
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
            cmdList.Add(new CommandData("材质", cmdStr, "matsWithSameTex", "选择同图片材质", () =>
            {
                SelectMaterialWithSameTex(BasicFunc.GetSelectedObject(0));
            }));
            cmdList.Add(new CommandData("材质", cmdStr, "combineMats", "合并选中材质", () =>
            {
                CombineMaterials();
            }));
            return cmdList;
        }

    }
}
