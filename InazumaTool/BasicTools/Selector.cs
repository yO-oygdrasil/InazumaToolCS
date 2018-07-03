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
        
        private List<int[]> selectedIndicesList = new List<int[]>();
        public int[] SetFromSelection(MSelectionList selected = null)
        {
            selectedIndicesList.Clear();
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
                
                selectedIndicesList.Add(selectedIndcies.ToArray());
            }
            int[] resultCount = new int[selectedIndicesList.Count];
            for (int i = 0; i < resultCount.Length; i++)
            {
                resultCount[i] = selectedIndicesList[i].Length;
            }
            return resultCount;
        }

        public MSelectionList RestoreSelection(MSelectionList targetList = null, bool selectResult = false)
        {
            
            if (targetList == null || targetList.length == 0)
            {
                targetList = BasicFunc.GetSelectedList();
            }

            MSelectionList resultSelection = new MSelectionList();

            for (int i = 0; i < targetList.length; i++)
            {
                if (i >= selectedIndicesList.Count)
                {
                    break;
                }
                MDagPath dag = new MDagPath();
                targetList.getDagPath((uint)i, dag);
                MFnSingleIndexedComponent sic = new MFnSingleIndexedComponent();
                MObject components = sic.create(MFn.Type.kMeshPolygonComponent);
                sic.addElements(new MIntArray(selectedIndicesList[i]));
                resultSelection.add(dag, components);

            }
            if (selectResult)
            {
                BasicFunc.Select(resultSelection);
            }
            return resultSelection;

        }

        public void DoForMultiSelection(Action<MSelectionList> dealMethod, MSelectionList targetList = null)
        {
            if (targetList == null || targetList.length == 0)
            {
                targetList = BasicFunc.GetSelectedList();
            }

            int groupCount = selectedIndicesList.Count;
            for (int i = 0; i * groupCount < targetList.length; i++)
            {
                MSelectionList groupList = new MSelectionList();
                for (int j = 0; j < groupCount; j++)
                {
                    int index = i * groupCount + j;
                    MDagPath dag = new MDagPath();
                    targetList.getDagPath((uint)index, dag);
                    groupList.add(dag);
                }
                dealMethod(RestoreSelection(groupList));
            }
        }


        public void CreateButtonWindow()
        {
            BasicWindow bw = new BasicWindow("Selector");
            Action[] actions = new Action[3];
            actions[0] = () =>
            {
                for (int i = 0; i < selectedIndicesList.Count; i++)
                {
                    string msg = "";
                    for (int j = 0; j < selectedIndicesList[i].Length; j++)
                    {
                        msg += selectedIndicesList[i][j];
                        if (j != 0)
                        {
                            msg += ",";
                        }
                    }
                    MGlobal.displayInfo("Selected " + i + ":" + msg);
                }
            };
            actions[1] = () =>
            {
                SetFromSelection();
            };
            actions[2] = () =>
            {
                RestoreSelection(null, true);
            };

            Action[] actions2 = new Action[2]
            {
                ()=>
                {
                    //Delete For One Group
                    BasicFunc.DoDelete(RestoreSelection(), true);
                },
                ()=>
                {
                    //Delete For Multi Group
                    DoForMultiSelection((list)=>{BasicFunc.DoDelete(list); });
                }
            };

            bw.AddButtons(actions, new string[3] { "Print", "Refresh Select", "Restore Selection" });
            bw.AddButtons(actions2, new string[2] { "Delete Topo","Delete Topo For Multi"});

            bw.Show();

        }




        const string cmdStr = "Selector";
        public static List<CommandData> GetCommandDatas()
        {
            List<CommandData> cmdList = new List<CommandData>();
            cmdList.Add(new CommandData("选择", cmdStr, "testBtnWindow", "测试创建选择集成器", () =>
            {
                Selector selector = new Selector();
                selector.SetFromSelection();
                selector.CreateButtonWindow();
            }));
            return cmdList;
        }

    }
}
