using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Maya.OpenMaya;


namespace InazumaTool.BasicTools
{
    class Selector
    {
        public enum SelectType
        {
            Face,
            Edge,
            Vert
        }
        private SelectType selectType;

        public Selector(SelectType st = SelectType.Face)
        {
            selectType = st;
        }
        
        private List<List<int>> selectedIndicesList = new List<List<int>>();
        public int[] SetFromSelection(MSelectionList selected = null)
        {
            if (selected == null)
            {
                //
                selected = BasicFunc.GetSelectedList();
            }

            //List<MVector> positions = new List<MVector>();
            MItSelectionList it_selectionList = new MItSelectionList(selected);
            for (; !it_selectionList.isDone; it_selectionList.next())
            {
                MObject component = new MObject();
                MDagPath item = new MDagPath();
                it_selectionList.getDagPath(item, component);


                List<int> selectedIndcies = new List<int>();
                Action<int> xx = (indice) => { selectedIndcies.Add(indice); };
                
                switch (selectType)
                {
                    case SelectType.Edge:
                        {
                            MItMeshEdge it_edges = new MItMeshEdge(item, component);
                            for (; !it_edges.isDone; it_edges.next())
                            {
                                selectedIndcies.Add(it_edges.index(0));
                                selectedIndcies.Add(it_edges.index(1));
                            }
                            break;
                        }
                    case SelectType.Face:
                        {
                            MItMeshPolygon it_poly = new MItMeshPolygon(item, component);

                            for (; !it_poly.isDone; it_poly.next())
                            {
                                selectedIndcies.Add((int)it_poly.index());
                            }

                            break;
                        }
                    case SelectType.Vert:
                        {
                            MItMeshVertex it_verts = new MItMeshVertex(item, component);

                            for (; !it_verts.isDone; it_verts.next())
                            {
                                selectedIndcies.Add(it_verts.index());
                            }

                            break;
                        }
                }
                
                selectedIndicesList.Add(selectedIndcies);
            }
            int[] resultCount = new int[selectedIndicesList.Count];
            for (int i = 0; i < resultCount.Length; i++)
            {
                resultCount[i] = selectedIndicesList[i].Count;
            }
            return resultCount;
        }

        public void CreateButtonWindow()
        {
            BasicWindow bw = new BasicWindow("Selector");
            Action[] actions = new Action[2];
            actions[0] = () =>
            {

                MGlobal.displayInfo("hahahahahhhhhhhhhhhh");
                //SetFromSelection();
            };
            actions[1] = () =>
            {
                MGlobal.displayInfo("hohohohohoh");
            };

            bw.AddButtons(actions, new string[2] { "testBtn0", "testBtn1" });
            bw.AddButtons(actions, new string[2] { "testBtn0_line2", "testBtn1_line2" });

            bw.Show();

        }




        const string cmdStr = "Selector";
        public static List<CommandData> GetCommandDatas()
        {
            List<CommandData> cmdList = new List<CommandData>();
            cmdList.Add(new CommandData("选择", cmdStr, "testBtnWindow", "测试创建选择集成器", () =>
            {
                new Selector(SelectType.Face).CreateButtonWindow();
            }));
            return cmdList;
        }

    }
}
