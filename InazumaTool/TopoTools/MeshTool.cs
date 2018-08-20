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
