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



        const string cmdStr = "MeshTool";
        public static List<CommandData> GetCommandDatas()
        {
            List<CommandData> cmdList = new List<CommandData>();

            
            //cmdList.Add(new CommandData("网格", "材质"));
            cmdList.Add(new CommandData("网格", cmdStr, "printEdges", "打印所有边", () =>
            {
                BasicFunc.IterateSelectedDags((dag) =>
                {
                    
                }, MFn.Type.kMesh);
            }));

            return cmdList;
        }

    }
}
