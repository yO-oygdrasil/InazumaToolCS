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
            MGlobal.executeCommand("hyperShade -objects " + matName);
            return true;
        }



        static void AssignMat(string matName)
        {
            MGlobal.executeCommand("hyperShade -assign " + matName);
        }


        /// <summary>
        /// well, this action is truely dangerous
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static bool CombineMaterials(MSelectionList list = null,bool deleteRepeated = true)
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
            List<MObject> deleteList = new List<MObject>();
            List<MSelectionList> waitForAssign = new List<MSelectionList>();

            MDGModifier dGModifier = new MDGModifier();

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
                else
                {
                    deleteList.Add(matObject);
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
                    MGlobal.displayInfo("finish select");
                    

                    //waitForAssign.Add(BasicFunc.GetSelectedList());
                    AssignMat(firstMatName);
                    MGlobal.displayInfo("finish assign");
                    BasicFunc.DeleteByCMD(dnode.absoluteName);
                    MGlobal.displayInfo("finish delete");

                }
                else
                {
                    MGlobal.displayInfo("no mat fn");
                }
            }

            dGModifier.doIt();

            //MGlobal.executeCommandOnIdle("hyperShade -objects " + matNode.absoluteName);
            return true;
        }

        public static int SelectMaterialWithSameTex(MObject imageObject)
        {
            //MImage img = new MImage();
            //img.readFromTextureNode(imageObject, MImage.MPixelType.kUnknown);
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



        public static bool CombineSameTextures(MSelectionList list = null,bool deleteRepeated = true)
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
            //string texFilePath = "";
            //List<string> texFilePaths = new List<string>();
            Dictionary<string, int> combineDic = new Dictionary<string, int>();
            List<MPlug> texOutputPlugs = new List<MPlug>();

            MDGModifier dGModifier = new MDGModifier();
            List<MObject> deleteList = new List<MObject>();
            for (int i = 0; i < list.length; i++)
            {
                MObject texObject = new MObject();
                list.getDependNode((uint)i, texObject);
                //MImage img = new MImage();
                //img.readFromTextureNode(texObject, MImage.MPixelType.kUnknown);
                MFnDependencyNode texDN = new MFnDependencyNode(texObject);
                MPlug texPlug = texDN.findPlug(ConstantValue.plugName_fileTexPath);
                MPlug texOutputPlug = texDN.findPlug(ConstantValue.plugName_fileTexOutput);
                //MGlobal.displayInfo("texplug name:" + texPlug.name);
                texOutputPlugs.Add(texOutputPlug);
                string filePath = texPlug.asString();
                //MGlobal.displayInfo("path:" + filePath);
                if (combineDic.ContainsKey(filePath))
                {
                    //combine
                    int targetIndex = combineDic[filePath];
                    //MGlobal.displayInfo("combine " + i + " to " + targetIndex);

                    MPlugArray destPlugs = new MPlugArray();
                    texOutputPlug.destinations(destPlugs);
                    for (int j = 0; j < destPlugs.Count; j++)
                    {
                        //MGlobal.displayInfo("texPlugs[targetIndex]:" + texOutputPlugs[targetIndex].name + " , destPlugs[j]" + destPlugs[j].name);
                        dGModifier.disconnect(texOutputPlug, destPlugs[j]);
                        dGModifier.connect(texOutputPlugs[targetIndex], destPlugs[j]);
                    }
                    deleteList.Add(texObject);
                    

                }
                else
                {
                    combineDic.Add(filePath, i);
                }
            }
            dGModifier.doIt();
            if (deleteRepeated)
            {
                for (int i = 0; i < deleteList.Count; i++)
                {
                    dGModifier.deleteNode(deleteList[i]);
                }

            }
            dGModifier.doIt();
            return true;

        }

        public static void RenameTextures(MSelectionList list = null)
        {
            if (list == null)
            {
                list = BasicFunc.GetSelectedList();
            }
            for (int i = 0; i < list.length; i++)
            {
                MObject mo = new MObject();
                list.getDependNode((uint)i, mo);
                MFnDependencyNode imageNode = new MFnDependencyNode(mo);
                MPlug plug = imageNode.findPlug(ConstantValue.plugName_fileTexPath);
                string filePath = plug.asString();
                MGlobal.displayInfo("filePath:" + filePath);
                string fileName = BasicFunc.GetFileName(filePath);
                MGlobal.displayInfo("fileName:" + fileName);
                imageNode.setName(fileName);


            }


        }

        public static void RenameMaterials(MSelectionList list = null)
        {
            if (list == null)
            {
                list = BasicFunc.GetSelectedList();
            }
            for (int i = 0; i < list.length; i++)
            {
                MObject mo = new MObject();
                list.getDependNode((uint)i, mo);
                MFnDependencyNode matNode = new MFnDependencyNode(mo);
                MPlug plug = matNode.findPlug(ConstantValue.plugName_matColorInput);
                MPlug sourcePlug = plug.source;
                if (sourcePlug != null)
                {

                    MFnDependencyNode sourceNode = new MFnDependencyNode(sourcePlug.node);
                    matNode.setName("mat_" + sourceNode.name);

                }
            }


        }

        public static void RemoveUnusedTextures(MSelectionList list = null)
        {
            if (list == null)
            {
                list = BasicFunc.GetSelectedList();
            }
            List<MObject> deleteList = new List<MObject>();
            for (int i = 0; i < list.length; i++)
            {
                MObject mo = new MObject();
                list.getDependNode((uint)i, mo);
                MFnDependencyNode imageNode = new MFnDependencyNode(mo);
                MPlug texOutputPlug = imageNode.findPlug(ConstantValue.plugName_fileTexOutput);
                MPlugArray destPlugs = new MPlugArray();
                texOutputPlug.destinations(destPlugs);
                if (destPlugs.Count == 0)
                {
                    deleteList.Add(mo);
                    MGlobal.displayInfo("remove no use:" + imageNode.absoluteName);
                }
            }
            BasicFunc.DeleteObjects(deleteList);

        }

        const string cmdStr = "MaterialManage";
        public static List<CommandData> GetCommandDatas()
        {
            List<CommandData> cmdList = new List<CommandData>();
            cmdList.Add(new CommandData("材质", cmdStr, "combineTextures", "合并相同路径图片", () =>
            {
                CombineSameTextures();
            }));
            cmdList.Add(new CommandData("材质", cmdStr, "matsWithSameTex", "选择同图片材质", () =>
            {
                SelectMaterialWithSameTex(BasicFunc.GetSelectedObject(0));
            }));
            cmdList.Add(new CommandData("材质", cmdStr, "combineMats", "合并选中材质", () =>
            {
                CombineMaterials();
            }));
            cmdList.Add(new CommandData("材质", cmdStr, "renameTextures", "重命名图片节点", () =>
            {
                RenameTextures();
            }));
            cmdList.Add(new CommandData("材质", cmdStr, "renameMaterials", "重命名材质节点（根据图片名）", () =>
            {
                RenameMaterials();
            }));
            cmdList.Add(new CommandData("材质", cmdStr, "removeUnused", "删除无用图片", () =>
            {
                RemoveUnusedTextures();
            }));
            cmdList.Add(new CommandData("材质", cmdStr, "combineMatSharingTexture", "合并相同贴图材质", () =>
            {
                SelectMaterialWithSameTex(BasicFunc.GetSelectedObject(0));
                CombineMaterials();
            }));
            return cmdList;
        }

    }
}
