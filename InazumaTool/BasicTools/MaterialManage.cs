using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Maya.OpenMayaRender;
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
        public static bool CombineMaterials(MSelectionList list, bool deleteRepeated = true)
        {
            if (list == null)
            {
                Debug.Log("list null");
                return false;
            }
            if (list.length <= 1)
            {
                Debug.Log("please choose at least 2 materials");
                return false;
            }
            string firstMatName = "";
            List<MObject> deleteList = new List<MObject>();
            List<MSelectionList> waitForAssign = new List<MSelectionList>();

            MDGModifier dGModifier = new MDGModifier();

            for (uint i = 0; i < list.length; i++)
            {
                //Debug.Log(i + " mat test");
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
                //Debug.Log(i + " node:" + dnode.absoluteName);
                if (matObject.hasFn(MFn.Type.kLambert) || matObject.hasFn(MFn.Type.kBlinn) || matObject.hasFn(MFn.Type.kPhong))
                {
                    Debug.Log("has mat fn");
                    //MMaterial mat = new MMaterial(matObject);
                    //MColor color = new MColor();
                    //mat.getDiffuse(color);
                    //Debug.Log("mat:" + dnode.absoluteName + " ,color:" + BasicFunc.MToString(color));
                    SelectObjectsWithMat(dnode);
                    Debug.Log("finish select");
                    

                    //waitForAssign.Add(BasicFunc.GetSelectedList());
                    AssignMat(firstMatName);
                    Debug.Log("finish assign");
                    BasicFunc.DeleteByCMD(dnode.absoluteName);
                    Debug.Log("finish delete");

                }
                else
                {
                    Debug.Log("no mat fn");
                }
            }

            dGModifier.doIt();

            //MGlobal.executeCommandOnIdle("hyperShade -objects " + matNode.absoluteName);
            return true;
        }

        public static MSelectionList GetMaterialsWithSameTex(MObject imageObject)
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
            //BasicFunc.Select(newSelection);
            return newSelection;
        }



        public static bool CombineSameTextures(MSelectionList list,bool deleteRepeated = true)
        {
            if (list == null)
            {
                Debug.Log("list null");
                return false;
            }
            if (list.length <= 1)
            {
                Debug.Log("please choose at least 2 materials");
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
                //Debug.Log("texplug name:" + texPlug.name);
                texOutputPlugs.Add(texOutputPlug);
                string filePath = texPlug.asString();
                //Debug.Log("path:" + filePath);
                if (combineDic.ContainsKey(filePath))
                {
                    //combine
                    int targetIndex = combineDic[filePath];
                    //Debug.Log("combine " + i + " to " + targetIndex);

                    MPlugArray destPlugs = new MPlugArray();
                    texOutputPlug.destinations(destPlugs);
                    for (int j = 0; j < destPlugs.Count; j++)
                    {
                        //Debug.Log("texPlugs[targetIndex]:" + texOutputPlugs[targetIndex].name + " , destPlugs[j]" + destPlugs[j].name);
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

        public static void RenameTextures(MSelectionList list)
        {
            if (list == null)
            {
                Debug.Log("list null");
                return;
            }
            for (int i = 0; i < list.length; i++)
            {
                MObject mo = new MObject();
                list.getDependNode((uint)i, mo);
                MFnDependencyNode imageNode = new MFnDependencyNode(mo);
                MPlug plug = imageNode.findPlug(ConstantValue.plugName_fileTexPath);
                string filePath = plug.asString();
                Debug.Log("filePath:" + filePath);
                string fileName = BasicFunc.GetFileName(filePath);
                Debug.Log("fileName:" + fileName);
                imageNode.setName(fileName);


            }


        }

        public static void RenameMaterials(MSelectionList list)
        {
            if (list == null)
            {
                Debug.Log("list null");
                return;
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

        public static void RemoveUnusedTextures(MSelectionList list)
        {
            if (list == null)
            {
                Debug.Log("list null");
                return;
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
                    Debug.Log("remove no use:" + imageNode.absoluteName);
                }
            }
            BasicFunc.DeleteObjects(deleteList);

        }

        public class ShapeData
        {
            public MFnMesh mesh;

        }

        public static List<MFnDependencyNode> GetMaterialsOfDag(MDagPath dag)
        {
            if (dag == null)
            {
                Debug.Log("dag null");
            }
            dag.extendToShape();
            MFnDependencyNode dn = new MFnDependencyNode(dag.node);
            //Debug.Log(dn.absoluteName);
            //dn.findPlug("connectAttr pCubeShape3.instObjGroups[0] blinn2SG.dagSetMembers[1]");
            MFnMesh shapeNode = new MFnMesh(dag);
            
            //int instanceCount = (int)shapeNode.instanceCount(false);
            //Debug.Log("dn instanceCount:" + instanceCount);
            uint instanceNumber = dag.instanceNumber;
            Debug.Log("dag instanceNumber:" + instanceNumber);

            MObjectArray sets = new MObjectArray(), comps = new MObjectArray();
            shapeNode.getConnectedSetsAndMembers(instanceNumber, sets, comps, true);


            List<MFnDependencyNode> result = new List<MFnDependencyNode>();
            for (int i = 0; i < sets.length; ++i)
            {
                MFnDependencyNode fnDepSGNode = new MFnDependencyNode(sets[i]);
                result.Add(fnDepSGNode);
                //Debug.Log(fnDepSGNode.name);
            }
            return result;

        }


        public static void CombineDagsWithSameMat(MSelectionList list)
        {
            if (list == null)
            {
                Debug.Log("list null");
                return;
            }
            foreach (MDagPath dag in list.DagPaths())
            {
                GetMaterialsOfDag(dag);
            }
        }


        const string cmdStr = "MaterialManage";
        public static List<CommandData> GetCommandDatas()
        {
            List<CommandData> cmdList = new List<CommandData>();


            cmdList.Add(new CommandData("材质", "图片"));
            cmdList.Add(new CommandData("材质", cmdStr, "matsWithSameTex", "选择同图片材质", () =>
            {
                GetMaterialsWithSameTex(BasicFunc.GetSelectedObject(0));
            }));
            cmdList.Add(new CommandData("材质", cmdStr, "combineMats", "合并选中材质", () =>
            {
                CombineMaterials(BasicFunc.GetSelectedList());
            }));
            cmdList.Add(new CommandData("材质", cmdStr, "renameMaterials", "重命名材质节点（根据图片名）", () =>
            {
                RenameMaterials(BasicFunc.GetSelectedList());
            }));
            cmdList.Add(new CommandData("材质", cmdStr, "combineMatSharing", "合并相同贴图材质（选中的每个图片）", () =>
            {
                BasicFunc.IterateSelectedObjects((imgObject) =>
                {
                    CombineMaterials(GetMaterialsWithSameTex(imgObject));
                }, MFn.Type.kFileTexture);
                
            }));

            cmdList.Add(new CommandData("材质", "材质"));
            cmdList.Add(new CommandData("材质", cmdStr, "combineTextures", "合并相同路径图片", () =>
            {
                CombineSameTextures(BasicFunc.GetSelectedList());
            }));
            cmdList.Add(new CommandData("材质", cmdStr, "renameTextures", "重命名图片节点", () =>
            {
                RenameTextures(BasicFunc.GetSelectedList());
            }));
            cmdList.Add(new CommandData("材质", cmdStr, "removeUnused", "删除无用图片", () =>
            {
                RemoveUnusedTextures(BasicFunc.GetSelectedList());
            }));

            cmdList.Add(new CommandData("材质", "物体"));
            cmdList.Add(new CommandData("材质", cmdStr, "combineSameMatObjects", "合并材质完全相同的物体", () =>
            {
                CombineDagsWithSameMat(BasicFunc.GetSelectedList());
            }));

            return cmdList;
        }

    }
}
