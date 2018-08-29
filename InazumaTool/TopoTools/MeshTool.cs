using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Maya.OpenMaya;
using InazumaTool.BasicTools;

namespace InazumaTool.TopoTools
{
    public static class MeshTool
    {
        public enum MergeUVSetMethod
        {
            Dont = 0,
            ByName = 1,
            ByUVLink = 2
        }

        public enum MergeCenterType
        {
            Middle = 0,
            Last = 1,
            Origin = 2
        }

        public class Edge
        {
            public int indice0, indice1;
            public MPoint point0, point1;

        }

        public static MDagPath CombineMeshesUsingMEL(MSelectionList list, string resultName = null, bool keepHistory = true, MergeUVSetMethod mergeUVSetMethod = MergeUVSetMethod.ByName)
        {
            if (list == null || list.length == 0)
            {
                return null;
            }
            List<string> dagNames = new List<string>();
            foreach (MDagPath dag in list.DagPaths())
            {
                dagNames.Add(dag.fullPathName);
            }
            return CombineMeshesUsingMEL(dagNames, resultName, keepHistory, mergeUVSetMethod);
        }

        public static MDagPath CombineMeshesUsingMEL(List<MDagPath> list, string resultName = null, bool keepHistory = true, MergeUVSetMethod mergeUVSetMethod = MergeUVSetMethod.ByName)
        {
            List<string> dagNames = new List<string>();
            foreach (MDagPath dag in list)
            {
                dagNames.Add(dag.fullPathName);
            }
            return CombineMeshesUsingMEL(dagNames, resultName, keepHistory, mergeUVSetMethod);
        }

        public static MDagPath CombineMeshesUsingMEL(List<string> dagNames, string resultName = null, bool keepHistory = true, MergeUVSetMethod mergeUVSetMethod = MergeUVSetMethod.ByName)
        {
            if (dagNames == null|| dagNames.Count==0)
            {
                return null;
            }
            CmdStrConstructor cc = new CmdStrConstructor("polyUnite");
            //polyUnite -ch 1 -mergeUVSets 1 -centerPivot -name resultName pCube2 pCube1
            cc.UpdateParm("ch", keepHistory);
            cc.UpdateParm("mergeUVSets", 1);
            cc.UpdateToggle("centerPivot", true);
            cc.UpdateParm("name", resultName);
            cc.UpdateTargets(dagNames);
            resultName = MGlobal.executeCommandStringResult(cc.ToString(), true);
            //Debug.Log("resultName:" + resultName);
            return null;// BasicFunc.GetDagPathByName(resultName);
        }

        public static bool CombineMeshes(List<MFnMesh> meshes, bool smartMergeEdge = false)
        {
            
            for (int i = 0; i < meshes.Count; i++)
            {
                MFnMesh m = new MFnMesh();

                MIntArray vertCounts = new MIntArray(), indiceLists = new MIntArray();
                meshes[i].getVertices(vertCounts, indiceLists);
            }

            return false;
        }

        public static int CombineOverLappingEdge(MFnMesh mesh,float threshold)
        {
            int edgeCount = mesh.numEdges;
            
            for (int i = 0; i < edgeCount; i++)
            {
                int[] edgeVerts = new int[2];
                mesh.getEdgeVertices(i, edgeVerts);
                MPoint a =new MPoint(), b = new MPoint();
                mesh.getPoint(edgeVerts[0], a);
                mesh.getPoint(edgeVerts[1], b);
                Debug.LogVector(a, "pointA");
                Debug.LogVector(b, "pointB");
            }



            return 0;
        }

        public static void ExtractFacesIntoOneObject(MSelectionList faceList)
        {
            if (faceList == null)
            {
                return;
            }

            MDagPath originDag = new MDagPath();
            faceList.getDagPath(0, originDag);

            Selector selector = new Selector();
            selector.SetFromSelection(faceList);
            MDagPath newDag = BasicFunc.Duplicate(originDag);
            MSelectionList targetPartInDuplicated = selector.RestoreSelectionOnDag(newDag, true);
            //new MFnTransform(newDag).setTranslation(new MVector(0, 2, 0), MSpace.Space.kWorld);

            //BasicFunc.Select(faceList);
            BasicFunc.DoDelete(faceList);
            MDagPath dag_targetPartInD = new MDagPath();

            MSelectionList invertSelection = BasicFunc.InvertSelect(targetPartInDuplicated, newDag, ConstantValue.PolySelectType.Facet);
            
            BasicFunc.DoDelete(invertSelection);
        }

        public static MDagPath DuplicateMesh(MDagPath targetDag)
        {
            if (targetDag == null)
            {
                return null;
            }
            MFnMesh newMesh = new MFnMesh();
            targetDag.extendToShape();
            return MDagPath.getAPathTo(newMesh.copy(targetDag.node));

        }

        const string cmdStr = "MeshTool";
        public static List<CommandData> GetCommandDatas()
        {
            List<CommandData> cmdList = new List<CommandData>();

            
            //cmdList.Add(new CommandData("网格", "材质"));
            cmdList.Add(new CommandData("网格", cmdStr, "printEdges", "打印所有边", () =>
            {
                BasicFunc.IterateSelectedDags((dag) =>
                {
                    dag.extendToShape();
                    MFnMesh mesh = new MFnMesh(dag);
                    CombineOverLappingEdge(mesh, 0.01f);
                }, MFn.Type.kMesh);
            }));

            cmdList.Add(new CommandData("网格", cmdStr, "realExtract", "老实地提取", () =>
            {
                ExtractFacesIntoOneObject(BasicFunc.GetSelectedList());
            }));


            return cmdList;
        }

    }
}
