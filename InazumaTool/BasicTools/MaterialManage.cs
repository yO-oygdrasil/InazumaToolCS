using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Maya.OpenMayaRender;
using Autodesk.Maya.OpenMaya;
using Autodesk.Maya.OpenMayaUI;
using InazumaTool.TopoTools;
using System.IO;

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
                BasicFunc.Select(list, true);
                //List<MSelectionList> facesListToAdd = new List<MSelectionList>();
                
                //if (list.length > 0)
                //{


                //    for (int i = (int)list.length - 1; i >= 0; i--)
                //    {
                //        MObject mo = new MObject();
                //        list.getDependNode((uint)i, mo);
                //        if (mo.apiType == MFn.Type.kMesh)
                //        {
                //            //component,no change
                //        }
                //        else if (mo.apiType == MFn.Type.kTransform)
                //        {
                //            //full object,select the faces
                //            BasicFunc.SelectComponent(MDagPath.getAPathTo(mo).fullPathName, ConstantValue.PolySelectType.Facet, true);
                //            facesListToAdd.Add(BasicFunc.GetSelectedList());
                //        }


                //    }

                //    MGlobal.setActiveSelectionList(list);
                //    for (int i = 0; i < facesListToAdd.Count; i++)
                //    {
                //        MGlobal.setActiveSelectionList(facesListToAdd[i], MGlobal.ListAdjustment.kAddToList);
                //    }
                //}
            }

            return true;
        }



        static void AssignMat(string matName)
        {
            MGlobal.executeCommand("hyperShade -assign " + matName);
        }

        static void MoveUV(float uValue, float vValue, string matName = null, bool splitUVBlockBeforeMove = true)
        {
            if (matName != null)
            {
                Debug.Log("matName:" + matName);
                SelectObjectsWithMat(matName, true);
            }
            if (splitUVBlockBeforeMove)
            {
                SplitUVBlock(BasicFunc.GetSelectedList(), true);
            }
            MGlobal.executeCommand(string.Format("polyEditUV -u {0} -v {1}", uValue, vValue), true);
        }

        static void SplitUVBlock(MSelectionList faceSelection, bool recoverSelection = true)
        {
            if (recoverSelection)
            {
                MSelectionList originList = BasicFunc.GetSelectedList();
                BasicFunc.Select(faceSelection);
                MGlobal.executeCommand(ConstantValue.command_ConvertSelectionToEdgePerimeter);
                MGlobal.executeCommand(ConstantValue.command_CutUVs);
                BasicFunc.Select(originList);
            }
            else
            {
                BasicFunc.Select(faceSelection);
                MGlobal.executeCommand(ConstantValue.command_ConvertSelectionToEdgePerimeter);
                MGlobal.executeCommand(ConstantValue.command_CutUVs);
            }
        }

        public enum ShaderParamType
        {
            Float,
            Color
        }
        static bool CopyShaderParam(MPlug from, MPlug to,ShaderParamType spt = ShaderParamType.Color)
        {
            if (from == null || to == null)
            {
                return false;
            }
            MPlugArray plugArr = new MPlugArray();
            from.connectedTo(plugArr, true, false);
            if (plugArr.length == 0)
            {
                switch (spt)
                {
                    case ShaderParamType.Color:
                        {
                            to.child(0).setFloat(from.child(0).asFloat());
                            to.child(1).setFloat(from.child(1).asFloat());
                            to.child(2).setFloat(from.child(2).asFloat());
                            break;
                        }
                    case ShaderParamType.Float:
                        {
                            to.setFloat(from.asFloat());
                            break;
                        }
                }
            }
            else
            {
                MDGModifier dGModifier = new MDGModifier();
                Debug.Log(from.source.partialName(true));
                dGModifier.connect(from.source, to);
                dGModifier.doIt();
            }
            return true;
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
            if (list.Count < 2)
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

        public static MSelectionList GetMaterialsWithTex(MObject imageObject)
        {
            //MImage img = new MImage();
            //img.readFromTextureNode(imageObject, MImage.MPixelType.kUnknown);
            MFnDependencyNode imageNode = new MFnDependencyNode(imageObject);
            return GetMaterialsWithTex(imageNode);

        }
        public static MSelectionList GetMaterialsWithTex(MFnDependencyNode imageNode)
        {
            MPlug plug = imageNode.findPlug(ConstantValue.plugName_fileTexOutputColor);
            MPlugArray destPlugs = new MPlugArray();
            plug.destinations(destPlugs);
            //BasicFunc.PrintPlugs(destPlugs);

            MSelectionList newSelection = new MSelectionList();
            for (int i = 0; i < destPlugs.length; i++)
            {
                newSelection.add(destPlugs[i].node);
            }
            //BasicFunc.Select(newSelection);
            return newSelection;
        }

        public static MSelectionList GetMaterialsWithTex(MFnDependencyNode imageNode,out MPlugArray texColorDestPlugs,out MPlugArray texTransparencyDestPlugs)
        {
            MPlug plug = imageNode.findPlug(ConstantValue.plugName_fileTexOutputColor);
            texColorDestPlugs = new MPlugArray();
            plug.destinations(texColorDestPlugs);
            MPlug plug_outTransparency = imageNode.findPlug(ConstantValue.plugName_fileTexOutputTransparency);
            texTransparencyDestPlugs = new MPlugArray();
            plug_outTransparency.destinations(texTransparencyDestPlugs);
            //BasicFunc.PrintPlugs(destPlugs);

            MSelectionList newSelection = new MSelectionList();
            for (int i = 0; i < texColorDestPlugs.length; i++)
            {
                newSelection.add(texColorDestPlugs[i].node);
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
                MPlug texOutputPlug = texDN.findPlug(ConstantValue.plugName_fileTexOutputColor);
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
                MPlug texOutputPlug = imageNode.findPlug(ConstantValue.plugName_fileTexOutputColor);
                MPlugArray destPlugs = new MPlugArray();
                texOutputPlug.destinations(destPlugs);
                if (destPlugs.Count == 0)
                {
                    deleteList.Add(mo);
                    Debug.Log("remove no use:" + imageNode.absoluteName);
                }
                else
                {
                    Debug.Log("still used:" + imageNode.absoluteName);
                    for (int j = 0; j < destPlugs.length; j++)
                    {
                        Debug.Log(" by:" + destPlugs[0].partialName(true));
                    }
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
                    MFnDependencyNode place2dTexNode = CreateShadingNode(ShadingNodeType.Utility, "place2dTexture");
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
            //MFnDependencyNode layeredTextureNode = new MFnDependencyNode();
            //layeredTextureNode.create("layeredTexture");
            MFnDependencyNode layeredTextureNode = CreateShadingNode(ShadingNodeType.Utility, "layeredTexture");
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
                MSelectionList matList = GetMaterialsWithTex(imageNodes[i]);
                for (int j = 0; j < matList.length; j++)
                {
                    MObject matObj = new MObject();
                    matList.getDependNode((uint)j, matObj);
                    string matName = new MFnDependencyNode(matObj).absoluteName;
                    Debug.Log("move uv for mat:" + matName);
                    MoveUV(i, 0, matName);
                }
                MPlug layeredTexInputPlug = layeredTexInputsPlug.elementByLogicalIndex((uint)i);
                MPlug texOutColorPlug = imageNodes[i].findPlug(ConstantValue.plugName_fileTexOutputColor);
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

        public enum ShadingNodeType
        {
            Light,
            PostProcess,
            Rendering,
            Shader,
            Texture,
            Utility
        }
        public static MFnDependencyNode CreateShadingNode(ShadingNodeType snt, string nodeType)
        {
            //Debug.Log("try create shadingNode:" + snt + " :" + nodeType);
            CmdStrConstructor csc = new CmdStrConstructor("shadingNode");
            switch (snt)
            {                
                case ShadingNodeType.Texture:
                    {
                        csc.UpdateToggle("at", true);
                        break;
                    }
                case ShadingNodeType.Utility:
                    {
                        csc.UpdateToggle("au", true);
                        break;
                    }
                case ShadingNodeType.Shader:
                    {
                        csc.UpdateToggle("as", true);
                        break;
                    }
                case ShadingNodeType.Light:
                    {
                        csc.UpdateToggle("al", true);
                        break;
                    }
                case ShadingNodeType.PostProcess:
                    {
                        csc.UpdateToggle("app", true);
                        break;
                    }
                case ShadingNodeType.Rendering:
                    {
                        csc.UpdateToggle("ar", true);
                        break;
                    }                    
            }
            csc.UpdateFinalAppend(nodeType);
            string cmdStr = csc.ToString();
            //Debug.Log("command:" + cmdStr);
            string nodeName = MGlobal.executeCommandStringResult(cmdStr, true);
            //Debug.Log("create node result:" + nodeName);
            return new MFnDependencyNode(BasicFunc.GetObjectByName(nodeName));
        }

        public static MFnDependencyNode CombineToUDIM(List<MObject> imageObjects, string prename, string newFolder = "UDIM", int maxUCount = 5)
        {
            List<MFnDependencyNode> dnNodes = new List<MFnDependencyNode>();
            for (int i = 0; i < imageObjects.Count; i++)
            {
                dnNodes.Add(new MFnDependencyNode(imageObjects[i]));
            }

            return CombineToUDIM(dnNodes, prename, newFolder, maxUCount);
        }

        public static MFnDependencyNode CombineToUDIM(List<MFnDependencyNode> imageNodes, string prename,string newFolder = "UDIM", int maxUCount = 5)
        {
            if (imageNodes.Count < 2)
            {
                return null;
            }
            MFnDependencyNode udimImgNode = CreateShadingNode(ShadingNodeType.Texture, ConstantValue.nodeName_fileTex);

            //MFnDependencyNode udimImgNode = new MFnDependencyNode();
            //udimImgNode.create("file");

            MPlug texOutColorPlug = udimImgNode.findPlug(ConstantValue.plugName_fileTexOutputColor);
            MPlug texOutTransparencyPlug = udimImgNode.findPlug(ConstantValue.plugName_fileTexOutputTransparency);
            udimImgNode.findPlug(ConstantValue.plugName_fileTexUVTilingMode).setInt((int)ConstantValue.UVTilingMode.UDIM);
            

            MDGModifier dGModifier = new MDGModifier();
            for (int i = 0; i < imageNodes.Count; i++)
            {                
                MPlug plug_fileTexPath = imageNodes[i].findPlug(ConstantValue.plugName_fileTexPath);
                string originFullPath = plug_fileTexPath.asString();

                int uIndex = i % maxUCount;
                int vIndex = i / maxUCount;
                string mariIndexStr = string.Format("{0}.10{1}{2}", prename, vIndex, uIndex + 1);
                string newFullPath = RenameTexFile(imageNodes[i], mariIndexStr, newFolder);
                if (i == 0)
                {
                    udimImgNode.findPlug(ConstantValue.plugName_fileTexPath).setString(newFullPath);
                }

                //move uv and reconnect link
                MPlugArray plugArr_transparencyDest, plugArr_colorDest;
                MSelectionList matList = GetMaterialsWithTex(imageNodes[i], out plugArr_colorDest, out plugArr_transparencyDest);
                //move uv for every material
                for (int j = 0; j < matList.length; j++)
                {
                    MObject matObj = new MObject();
                    matList.getDependNode((uint)j, matObj);
                    MFnDependencyNode matNode = new MFnDependencyNode(matObj);
                    string matName = matNode.absoluteName;
                    //Debug.Log("move uv for mat:" + matName);
                    MoveUV(uIndex, vIndex, matName);
                    //MPlug plug_matColorInput = matNode.findPlug(ConstantValue.plugName_matColorInput);
                    //if (plug_matColorInput != null)
                    //{
                    //    dGModifier.disconnect(plug_matColorInput.source, plug_matColorInput);
                    //    dGModifier.connect(texOutColorPlug, plug_matColorInput);
                    //    dGModifier.doIt();
                    //}
                }
                //reconnect alpha
                for (int j = 0; j < plugArr_transparencyDest.length; j++)
                {
                    dGModifier.disconnect(plugArr_transparencyDest[j].source, plugArr_transparencyDest[j]);
                    dGModifier.connect(texOutTransparencyPlug, plugArr_transparencyDest[j]);
                }
                //reconnect color
                for (int j = 0; j < plugArr_colorDest.length; j++)
                {
                    dGModifier.disconnect(plugArr_colorDest[j].source, plugArr_colorDest[j]);
                    dGModifier.connect(texOutColorPlug, plugArr_colorDest[j]);
                }
            }
            dGModifier.doIt();

            return udimImgNode;
        }
        
        public static string RenameTexFile(MFnDependencyNode imageNode, string newPartialName, string newFolder = null, bool relinkImgNode = false, bool deleteOrigin = false,bool overwrite = false)
        {
            MPlug plug_fileTexPath = imageNode.findPlug(ConstantValue.plugName_fileTexPath);
            string originFullPath = plug_fileTexPath.asString();
            string newFullPath = BasicFunc.RenameFile(originFullPath, newPartialName, newFolder, deleteOrigin, overwrite);
            if (relinkImgNode)
            {
                plug_fileTexPath.setString(newFullPath);
            }
            return newFullPath;
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
                if (plugArr.Count > 0)
                {
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
                else
                {
                    deleteList.Add(matNode);
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
                Debug.Log(matStr);
                matStr.Replace(':', '_');
                Debug.Log("after deal:" + matStr);
                MeshTool.CombineMeshesUsingMEL(dagClusterList[i], matStr);

                //matStr += ":";
                //for (int j = 0; j < dagClusterList[i].Count; j++)
                //{
                //    matStr += "," + dagClusterList[i][j].fullPathName;
                //}
                //Debug.Log(matStr);
            }
            
        }

        public static void ChangeTexturesPrefix(MSelectionList list,string newFolderPath)
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
                string fileName = Path.GetFileName(filePath);
                plug.setString(Path.Combine(newFolderPath, fileName));
                
            }
        }



        #region RedShift


        public static void ConvertToRSMaterial(MFnDependencyNode matNode,bool deleteOrigin)
        {
            //replace output to shadingEngine
            MPlug plug_matColorOutput = matNode.findPlug(ConstantValue.plugName_matColorOutput);
            MPlugArray plugArr_matColorOutDest = new MPlugArray();
            plug_matColorOutput.destinations(plugArr_matColorOutDest);
            //get textures
            MPlug plug_matColorInput = matNode.findPlug(ConstantValue.plugName_matColorInput);
            MPlug plug_matTransparency = matNode.findPlug(ConstantValue.plugName_matTransparency);

            MFnDependencyNode rsArchiNode = CreateShadingNode(ShadingNodeType.Shader, ConstantValue.nodeName_RS_Architectural);
            if (matNode.name.StartsWith("mat_"))
            {
                rsArchiNode.setName("rs" + matNode.name);
            }
            else
            {
                rsArchiNode.setName("rsmat_" + matNode.name);
            }
            MPlug plug_rsArchiDiffuse = rsArchiNode.findPlug(ConstantValue.plugName_RS_diffuse);
            MPlug plug_rsArchiTransColor = rsArchiNode.findPlug(ConstantValue.plugName_RS_transColor);
            MPlug plug_rsArchiTransWeight = rsArchiNode.findPlug(ConstantValue.plugName_RS_transWeight);
            MPlug plug_rsArchiOutColor = rsArchiNode.findPlug(ConstantValue.plugName_RS_outColor);
            MDGModifier dGModifier = new MDGModifier();
            for (int i = 0; i < plugArr_matColorOutDest.length; i++)
            {
                dGModifier.disconnect(plug_matColorOutput, plugArr_matColorOutDest[i]);
                dGModifier.connect(plug_rsArchiOutColor, plugArr_matColorOutDest[i]);
            }

            CopyShaderParam(plug_matColorInput, plug_rsArchiDiffuse);
            //if (plug_matColorInput.source == null)
            //{
            //    plug_rsArchiDiffuse.child(0).setFloat(plug_matColorInput.child(0).asFloat());
            //    plug_rsArchiDiffuse.child(1).setFloat(plug_matColorInput.child(1).asFloat());
            //    plug_rsArchiDiffuse.child(2).setFloat(plug_matColorInput.child(2).asFloat());
            //}
            //else
            //{
            //    dGModifier.connect(plug_matColorInput.source, plug_rsArchiDiffuse);
            //}

            CopyShaderParam(plug_matTransparency, plug_rsArchiTransColor);
            if (plug_matTransparency.child(0).asFloat() == 0 && plug_matTransparency.child(1).asFloat() == 0 && plug_matTransparency.child(2).asFloat() == 0)
            {
                plug_rsArchiTransWeight.setFloat(1);
            }
            else
            {
                plug_rsArchiTransWeight.setFloat(0);
            }
            //if (plug_matTransparency.source == null)
            //{
            //    //plug_rsArchiTransColor.setValue(plug_matColorInput.asMObject());
            //    float matTransparency = plug_matTransparency.asFloat();
            //    if (matTransparency == 0)
            //    {
            //        plug_rsArchiTransWeight.setFloat(0);
            //        plug_rsArchiTransColor.child(0).setFloat(0);
            //        plug_rsArchiTransColor.child(1).setFloat(0);
            //        plug_rsArchiTransColor.child(2).setFloat(0);
            //    }
            //    else
            //    {
            //        plug_rsArchiTransWeight.setFloat(1);
            //        plug_rsArchiTransColor.child(0).setFloat(plug_matTransparency.child(0).asFloat());
            //        plug_rsArchiTransColor.child(1).setFloat(plug_matTransparency.child(1).asFloat());
            //        plug_rsArchiTransColor.child(2).setFloat(plug_matTransparency.child(2).asFloat());
            //    }
            //}
            //else
            //{
            //    dGModifier.connect(plug_matTransparency.source, plug_rsArchiTransColor);
            //    plug_rsArchiTransWeight.setFloat(1);
            //}
            if (deleteOrigin)
            {
                dGModifier.deleteNode(matNode.objectProperty);
            }
            dGModifier.doIt();
        }
        #endregion


        const string cmdStr = "MaterialManage";
        public static List<CommandData> GetCommandDatas()
        {
            List<CommandData> cmdList = new List<CommandData>();


            cmdList.Add(new CommandData("材质", "图片"));
            cmdList.Add(new CommandData("材质", cmdStr, "matsWithSameTex", "选择同图片材质", () =>
            {
                BasicFunc.Select(GetMaterialsWithTex(BasicFunc.GetSelectedObject(0)));
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
            cmdList.Add(new CommandData("材质", cmdStr, "deleteUnusedSGs", "删除无用着色组", () =>
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
            cmdList.Add(new CommandData("材质", cmdStr, "selectMatComponent", "选择属于此材质的所有组件", () =>
            {
                SelectObjectsWithMat(new MFnDependencyNode(BasicFunc.GetSelectedObject(0)), true);
            }));

            //cmdList.Add(new CommandData("材质", cmdStr, "moveMatUV", "移动此材质UV", () =>
            //{
            //    CombineDagsWithSameMat(BasicFunc.GetSelectedList());
            //}));
            cmdList.Add(new CommandData("材质", cmdStr, "convertToLayered", "转换为LayeredTextures", () =>
            {
                CreateLayeredTextureNode(BasicFunc.GetSelectedObjectList());
            }));
            //cmdList.Add(new CommandData("材质", cmdStr, "conbineToUDIM1", "合并为UDIM-1", () =>
            //{
            //    CombineToUDIM(BasicFunc.GetSelectedObjectList(), "udim","UDIM",1);
            //}));
            //cmdList.Add(new CommandData("材质", cmdStr, "conbineToUDIM2", "合并为UDIM-2", () =>
            //{
            //    CombineToUDIM(BasicFunc.GetSelectedObjectList(), "udim", "UDIM", 2);
            //}));
            //cmdList.Add(new CommandData("材质", cmdStr, "conbineToUDIM3", "合并为UDIM-3", () =>
            //{
            //    CombineToUDIM(BasicFunc.GetSelectedObjectList(), "udim", "UDIM", 3);
            //}));
            //cmdList.Add(new CommandData("材质", cmdStr, "conbineToUDIM4", "合并为UDIM-4", () =>
            //{
            //    CombineToUDIM(BasicFunc.GetSelectedObjectList(), "udim", "UDIM", 4);
            //}));
            //cmdList.Add(new CommandData("材质", cmdStr, "conbineToUDIM5", "合并为UDIM-5", () =>
            //{
            //    CombineToUDIM(BasicFunc.GetSelectedObjectList(), "udim", "UDIM", 5);
            //}));
            cmdList.Add(new CommandData("材质", cmdStr, "udimEditor", "UDIM编辑器", () =>
            {
                UI.MaterialManageWindow window = new UI.MaterialManageWindow();
                window.Show();
            }));
            cmdList.Add(new CommandData("材质", "RS"));
            cmdList.Add(new CommandData("材质", cmdStr, "convertMatToRS", "材质转为RS-Architecture", () =>
            {
                ConvertToRSMaterial(new MFnDependencyNode(BasicFunc.GetSelectedObject(0)),false);
            }));

            cmdList.Add(new CommandData("材质", cmdStr, "tempConvert_delete", "临时-批量改贴图路径文件夹", () =>
            {
                Debug.Log("没做好编辑器所以就不调用了");
                //ChangeTexturesPrefix(BasicFunc.GetSelectedList(), @"D:\testTextures");
            }));


            return cmdList;
        }

    }
}
