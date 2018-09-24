using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Maya.OpenMayaRender;
using Autodesk.Maya.OpenMaya;
using Autodesk.Maya.OpenMayaUI;
using InazumaTool.TopoTools;

namespace InazumaTool.BasicTools
{
    public static class MaterialManage
    {

        static bool SelectObjectsWithMat(MFnDependencyNode matNode, bool selectInComponentMode = false)
        {
            if (matNode == null)
            {
                return false;
            }            
            return SelectObjectsWithMat(matNode.absoluteName);
        }

        static bool SelectObjectsWithMat(string matName,bool selectInComponentMode = false)
        {
            if (matName == null || matName.Length == 0)
            {
                Debug.Log("matName null or empty");
                return false;
            }
            MGlobal.executeCommand("hyperShade -objects " + matName);
            if (selectInComponentMode)
            {
                Debug.Log("select in component mode");
                MSelectionList list = BasicFunc.GetSelectedList();
                List<MSelectionList> facesList = new List<MSelectionList>();
                if (list.length > 0)
                {
                    //有完整的物体在内，这样的物体是不会被选择为组件模式的
                    for (int i = (int)list.length - 1; i >= 0; i--)
                    {
                        MDagPath dag = new MDagPath();
                        list.getDagPath((uint)i, dag);
                        BasicFunc.SelectComponent(dag.fullPathName, ConstantValue.PolySelectType.Facet, true);
                        facesList.Add(BasicFunc.GetSelectedList());
                        list.remove((uint)i);
                    }

                    MGlobal.setActiveSelectionList(list);
                    for (int i = 0; i < facesList.Count; i++)
                    {
                        MGlobal.setActiveSelectionList(facesList[i], MGlobal.ListAdjustment.kAddToList);
                    }
                }
            }

            return true;
        }



        static void AssignMat(string matName)
        {
            MGlobal.executeCommand("hyperShade -assign " + matName);
        }

        static void MoveUV(float uValue, float vValue, string matName = null)
        {
            if (matName != null)
            {
                Debug.Log("matName:" + matName);
                SelectObjectsWithMat(matName, true);
            }
            MGlobal.executeCommand(string.Format("polyEditUV -u {0} -v {1}", uValue, vValue));
        }

        /// <summary>
        /// well, this action is truely dangerous
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static bool CombineMaterials(MSelectionList list, bool deleteRepeated = true)
        {
            if (!BasicFunc.CheckSelectionList(list, 2))
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
                    //Debug.Log("has mat fn");
                    //MMaterial mat = new MMaterial(matObject);
                    //MColor color = new MColor();
                    //mat.getDiffuse(color);
                    //Debug.Log("mat:" + dnode.absoluteName + " ,color:" + BasicFunc.MToString(color));
                    SelectObjectsWithMat(dnode);
                    //Debug.Log("finish select");
                    

                    //waitForAssign.Add(BasicFunc.GetSelectedList());
                    AssignMat(firstMatName);
                    //Debug.Log("finish assign");
                    BasicFunc.DeleteByCMD(dnode.absoluteName);
                    //Debug.Log("finish delete");

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

        public static bool CombineMaterials(List<MObject> list, bool deleteRepeated = true)
        {
            if (list.Count <= 2)
            {
                Debug.Log("please choose at least 2 materials");
                return false;
            }
            string firstMatName = "";
            List<MObject> deleteList = new List<MObject>();
            List<MSelectionList> waitForAssign = new List<MSelectionList>();

            MDGModifier dGModifier = new MDGModifier();

            for (int i = 0; i < list.Count; i++)
            {
                //Debug.Log(i + " mat test");
                MObject matObject = list[i];
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
                    SelectObjectsWithMat(dnode);
                    AssignMat(firstMatName);
                    BasicFunc.DeleteByCMD(dnode.absoluteName);
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

        public static List<MObject> GetMaterialsWithTex(MObject imageObject)
        {
            //MImage img = new MImage();
            //img.readFromTextureNode(imageObject, MImage.MPixelType.kUnknown);
            MFnDependencyNode imageNode = new MFnDependencyNode(imageObject);
            return GetMaterialsWithTex(imageNode);

        }

        public static List<MObject> GetMaterialsWithTex(MFnDependencyNode imageNode)
        {
            MPlug plug = imageNode.findPlug(ConstantValue.plugName_fileTexOutput);
            MPlugArray destPlugs = new MPlugArray();
            plug.destinations(destPlugs);
            BasicFunc.PrintPlugs(destPlugs);

            List<MObject> newSelection = new List<MObject>();
            for (int i = 0; i < destPlugs.length; i++)
            {
                newSelection.Add(destPlugs[i].node);
            }
            //BasicFunc.Select(newSelection);
            return newSelection;
        }


        public static bool CombineSameTextures(MSelectionList list,bool deleteRepeated = true)
        {
            if (!BasicFunc.CheckSelectionList(list, 2))
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

        public static MFnDependencyNode GetPlace2dTextureForTex(MFnDependencyNode imageNode, bool createIfNotExist = true)
        {
            if (imageNode == null)
            {
                Debug.Log("image Node null");
                return null;
            }
            MPlug uvPlug = imageNode.findPlug(ConstantValue.plugName_texFileUVCoord);
            MPlugArray sourcePlugs = new MPlugArray();
            uvPlug.connectedTo(sourcePlugs, true, false);
            if (sourcePlugs.length == 0)
            {
                //no input
                if (createIfNotExist)
                {
                    MFnDependencyNode place2dTexNode = new MFnDependencyNode();
                    place2dTexNode.create("place2dTexture");
                    MPlug p2tUVOut = place2dTexNode.findPlug(ConstantValue.plugName_place2dOutUV);
                    string nodeName = place2dTexNode.absoluteName;
                    MDGModifier dgModifier = new MDGModifier();
                    dgModifier.connect(p2tUVOut, uvPlug);
                    dgModifier.doIt();

                    return place2dTexNode;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return new MFnDependencyNode(sourcePlugs[0].node);
            }
        }

        public static MFnDependencyNode GetPlace2dTextureForTex(MObject imageObject,bool createIfNotExist = true)
        {
            if (imageObject == null)
            {
                Debug.Log("image object null");
                return null;
            }
            MFnDependencyNode imageNode = new MFnDependencyNode(imageObject);
            return GetPlace2dTextureForTex(imageNode, createIfNotExist);
        }

        public static MFnDependencyNode CreateLayeredTextureNode(List<MObject> imageObjects)
        {
            List<MFnDependencyNode> dnNodes = new List<MFnDependencyNode>();
            for (int i = 0; i < imageObjects.Count; i++)
            {
                dnNodes.Add(new MFnDependencyNode(imageObjects[i]));
            }

            return CreateLayeredTextureNode(dnNodes);
        }

        public static MFnDependencyNode CreateLayeredTextureNode(List<MFnDependencyNode> imageNodes)
        {
            MFnDependencyNode layeredTextureNode = new MFnDependencyNode();
            layeredTextureNode.create("layeredTexture");
            MPlug layeredTexInputsPlug = layeredTextureNode.findPlug(ConstantValue.plugName_layeredTextureInputs);
            //check place2DTextures
            MDGModifier dGModifier = new MDGModifier();
            for (int i = 0; i < imageNodes.Count; i++)
            {
                //place2dTexture setting
                MFnDependencyNode p2dNode = GetPlace2dTextureForTex(imageNodes[i]);
                p2dNode.findPlug("wrapU").setBool(false);
                p2dNode.findPlug("translateFrameU").setFloat(i);
                
                //set tex default color to 0
                imageNodes[i].findPlug(ConstantValue.plugName_fileTexDefaultColorR).setFloat(0);
                imageNodes[i].findPlug(ConstantValue.plugName_fileTexDefaultColorG).setFloat(0);
                imageNodes[i].findPlug(ConstantValue.plugName_fileTexDefaultColorB).setFloat(0);

                //move uv
                List<MObject> matObjects = GetMaterialsWithTex(imageNodes[i]);
                foreach (MObject matObj in matObjects)
                {
                    string matName = new MFnDependencyNode(matObj).absoluteName;
                    Debug.Log("move uv for mat:" + matName);
                    MoveUV(i, 0, matName);
                }

                MPlug layeredTexInputPlug = layeredTexInputsPlug.elementByLogicalIndex((uint)i);
                MPlug texOutColorPlug = imageNodes[i].findPlug(ConstantValue.plugName_fileTexOutput);
                MPlug layeredTexInputColor = layeredTexInputPlug.child((int)ConstantValue.LayeredTextureInputDataIndex.Color);
                dGModifier.connect(texOutColorPlug, layeredTexInputColor);
                
                //set blendMode to add
                MPlug blendMode = layeredTexInputPlug.child((int)ConstantValue.LayeredTextureInputDataIndex.BlendMode);
                if (i < imageNodes.Count - 1)
                {
                    blendMode.setInt((int)ConstantValue.LayeredTextureBlendMode.Add);
                }
                else
                {
                    blendMode.setInt((int)ConstantValue.LayeredTextureBlendMode.None);
                }

            }
            dGModifier.doIt();

            return layeredTextureNode;
        }

        public class ShapeData
        {
            public MFnMesh mesh;
            public MDagPath dag;
            public List<string> sgList;
            public ShapeData(MFnMesh m, MDagPath d, List<string> list)
            {
                mesh = m;
                dag = d;
                sgList = list;
            }
        }

        public static ShapeData GetMaterialsOfDag(MDagPath dag)
        {
            if (dag == null)
            {
                Debug.Log("dag null");
            }
            dag.extendToShape();
            MFnDependencyNode dn = new MFnDependencyNode(dag.node);
            //Debug.Log(dn.absoluteName);
            //dn.findPlug("connectAttr pCubeShape3.instObjGroups[0] blinn2SG.dagSetMembers[1]");
            MFnMesh mesh = new MFnMesh(dag);
            
            //int instanceCount = (int)shapeNode.instanceCount(false);
            //Debug.Log("dn instanceCount:" + instanceCount);
            uint instanceNumber = dag.instanceNumber;
            Debug.Log("dag instanceNumber:" + instanceNumber);

            MObjectArray sets = new MObjectArray(), comps = new MObjectArray();
            mesh.getConnectedSetsAndMembers(instanceNumber, sets, comps, true);


            List<string> sgList = new List<string>();
            for (int i = 0; i < sets.length; ++i)
            {
                MFnDependencyNode fnDepSGNode = new MFnDependencyNode(sets[i]);
                sgList.Add(fnDepSGNode.absoluteName);
                //Debug.Log(fnDepSGNode.name);
            }
            //Debug.Log("sgList Count:" + sgList.Count);
            return new ShapeData(mesh, dag, sgList);

        }

        public static void DeleteUnusedMats(MSelectionList list)
        {
            if (list == null)
            {
                Debug.Log("list null");
                return;
            }
            List<MFnDependencyNode> deleteList = new List<MFnDependencyNode>();
            for (int i = 0; i < list.length; i++)
            {
                MObject mo = new MObject();
                list.getDependNode((uint)i, mo);
                MFnDependencyNode matNode = new MFnDependencyNode(mo);
                MPlug plug = matNode.findPlug(ConstantValue.plugName_matColorOutput);
                MPlugArray plugArr = new MPlugArray();
                plug.destinations(plugArr);
                MFnDependencyNode sgNode = new MFnDependencyNode(plugArr[0].node);
                Debug.Log(sgNode.name);

                

                MPlug plug_dagSetMemebers = sgNode.findPlug(ConstantValue.plugName_dagSetMembers);
                Debug.Log("numelements:" + plug_dagSetMemebers.numElements);
                if (plug_dagSetMemebers.numElements == 0)
                {
                    deleteList.Add(matNode);
                    deleteList.Add(sgNode);
                }
            }
            BasicFunc.DeleteObjects(deleteList);
        }

        public static void DeleteUnusedShadingNode(MSelectionList list)
        {
            if (list == null)
            {
                Debug.Log("list null");
                return;
            }
            List<MFnDependencyNode> deleteList = new List<MFnDependencyNode>();
            for (int i = 0; i < list.length; i++)
            {
                MObject mo = new MObject();
                list.getDependNode((uint)i, mo);
                if (mo.hasFn(MFn.Type.kShadingEngine))
                {
                    MFnDependencyNode sgNode = new MFnDependencyNode(mo);
                    MPlug plug_dagSetMemebers = sgNode.findPlug(ConstantValue.plugName_dagSetMembers);
                    Debug.Log("numelements:" + plug_dagSetMemebers.numElements);
                    if (plug_dagSetMemebers.numElements == 0)
                    {
                        deleteList.Add(sgNode);
                    }
                }
                //Debug.Log(sgNode.name);
            }
            BasicFunc.DeleteObjects(deleteList);
        }

        public static void CombineDagsWithSameMat(MSelectionList list)
        {
            if (list == null)
            {
                Debug.Log("list null");
                return;
            }
            List<List<string>> matClusterList = new List<List<string>>();
            List<List<MDagPath>> dagClusterList = new List<List<MDagPath>>();
            //Dictionary<List<string>, List<MDagPath>> matDic = new Dictionary<List<MFnDependencyNode>, List<MDagPath>>();
            foreach (MDagPath dag in list.DagPaths())
            {
                ShapeData sd = GetMaterialsOfDag(dag);
                bool exist = false;
                for (int i = 0; i < matClusterList.Count; i++)
                {
                    if (BasicFunc.IsSame(sd.sgList, matClusterList[i]))
                    {
                        exist = true;
                        dagClusterList[i].Add(dag);
                    }
                }
                if (!exist)
                {
                    matClusterList.Add(sd.sgList);
                    List<MDagPath> newDagList = new List<MDagPath>();
                    newDagList.Add(dag);
                    dagClusterList.Add(newDagList);
                }
            }
            for (int i = 0; i < matClusterList.Count; i++)
            {
                string matStr = "combined_";
                for (int j = 0; j < matClusterList[i].Count; j++)
                {
                    matStr += matClusterList[i][j] + "_";
                }
                MeshTool.CombineMeshesUsingMEL(dagClusterList[i], matStr);

                //matStr += ":";
                //for (int j = 0; j < dagClusterList[i].Count; j++)
                //{
                //    matStr += "," + dagClusterList[i][j].fullPathName;
                //}
                //Debug.Log(matStr);
            }
            
        }




        const string cmdStr = "MaterialManage";
        public static List<CommandData> GetCommandDatas()
        {
            List<CommandData> cmdList = new List<CommandData>();


            cmdList.Add(new CommandData("材质", "图片"));
            cmdList.Add(new CommandData("材质", cmdStr, "matsWithSameTex", "选择同图片材质", () =>
            {
                GetMaterialsWithTex(BasicFunc.GetSelectedObject(0));
            }));
            cmdList.Add(new CommandData("材质", cmdStr, "combineMats", "合并选中材质", () =>
            {
                CombineMaterials(BasicFunc.GetSelectedList());
            }));
            cmdList.Add(new CommandData("材质", cmdStr, "confirmPlace2dTexture", "贴图place2dTexture修复", () =>
            {
                RenameMaterials(BasicFunc.GetSelectedList());
            }));
            cmdList.Add(new CommandData("材质", cmdStr, "renameMaterials", "重命名材质节点（根据图片名）", () =>
            {
                RenameMaterials(BasicFunc.GetSelectedList());
            }));
            cmdList.Add(new CommandData("材质", cmdStr, "combineMatSharing", "合并相同贴图材质（选中的每个图片）", () =>
            {
                BasicFunc.IterateSelectedObjects((imgObject) =>
                {
                    CombineMaterials(GetMaterialsWithTex(imgObject));
                }, MFn.Type.kFileTexture);
                
            }));
            cmdList.Add(new CommandData("材质", cmdStr, "deleteUnusedMats", "删除无用材质", () =>
            {
                DeleteUnusedMats(BasicFunc.GetSelectedList());
            }));
            cmdList.Add(new CommandData("材质", cmdStr, "deleteUnusedMats", "删除无用着色组", () =>
            {
                DeleteUnusedShadingNode(BasicFunc.GetSelectedList());
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

            cmdList.Add(new CommandData("材质", "整合"));
            cmdList.Add(new CommandData("材质", cmdStr, "moveMatUV", "移动此材质UV", () =>
            {
                CombineDagsWithSameMat(BasicFunc.GetSelectedList());
            }));
            cmdList.Add(new CommandData("材质", cmdStr, "convertToLayered", "转换为LayeredTextures", () =>
            {
                CreateLayeredTextureNode(BasicFunc.GetSelectedObjectList());
            }));
            return cmdList;
        }

    }
}
