using Autodesk.Maya.OpenMaya;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: MPxCommandClass(typeof(InazumaTool.BasicTools.BasicWindowCommand), "InazumaBasicWindow")]

namespace InazumaTool.BasicTools
{


    public class BasicWindow
    {
        private static Dictionary<string, BasicWindow> basicWindowDic = new Dictionary<string, BasicWindow>();
        public static bool ExcuteInDic(string wName, int lineIndex, int btnIndex)
        {
            bool result = false;
            if (basicWindowDic.ContainsKey(wName))
            {
                //Debug.Log("window data exist");
                result = basicWindowDic[wName].Excute(lineIndex, btnIndex);
            }
            return result;
        }

        public const int btnWidth = 100;
        public const int btnHeight = 50;

        private class ActionPair
        {
            public Action action;
            public string label;
            public ActionPair(Action a, string l)
            {
                action = a;
                label = l;
            }
        }
        private List<List<ActionPair>> pairs = new List<List<ActionPair>>();
        public string windowName = "";

        public BasicWindow(string wName)
        {
            if (wName != null)
            {
                windowName = wName;
            }
            if (basicWindowDic.ContainsKey(wName))
            {
                basicWindowDic[wName] = this;
            }
            else
            {
                basicWindowDic.Add(wName, this);
            }

        }

        public void AddButtons(System.Action[] actions, string[] labels, int rowIndex = -1)
        {
            List<ActionPair> targetPair;
            if (rowIndex < 0 || rowIndex >= pairs.Count)
            {
                targetPair = new List<ActionPair>();
                pairs.Add(targetPair);
            }
            else
            {
                targetPair = pairs[rowIndex];
            }

            for (int i = 0; i < actions.Length; i++)
            {
                targetPair.Add(new ActionPair(actions[i], labels[i]));
            }
        }

        public bool Excute(int lineIndex, int buttonIndex)
        {
            bool result = false;
            if (pairs.Count > lineIndex)
            {
                if (pairs[lineIndex].Count > buttonIndex)
                {
                    Debug.Log("excute");
                    pairs[lineIndex][buttonIndex].action.Invoke();
                    result = true;
                }
            }
            return result;
        }

        private string runtimeWindowName;
        public void Show()
        {
            int columnCount = 0, rowCount = pairs.Count;
            for (int i = 0; i < rowCount; i++)
            {
                if (pairs[i].Count > columnCount)
                {
                    columnCount = pairs[i].Count;
                }
            }

            runtimeWindowName = MGlobal.executeCommandStringResult(string.Format("window -title \"{0}\" -widthHeight {1} {2};", windowName, columnCount * btnWidth, rowCount * btnHeight), true);

            MGlobal.executeCommand("columnLayout");
            //string btnCmdStr = "";
            for (int i = 0; i < rowCount; i++)
            {
                //btnCmdStr += "columnLayout -adjustableColumn true;\n";
                string rowLayoutCmdStr = string.Format("rowLayout -nc {0} ", pairs[i].Count);
                for (int m = 0; m < pairs[i].Count; m++)
                {
                    rowLayoutCmdStr += string.Format(" -cw {0} {1}", m + 1, btnWidth);
                }

                MGlobal.executeCommand(rowLayoutCmdStr, true);
                for (int j = 0; j < pairs[i].Count; j++)
                {
                    //btnCmdStr += string.Format("button -label \"{0}\" -command \"InazumaBasicWindow {1} {2} {3}\";\n", pairs[i][j].label, windowName, i, j);
                    MGlobal.executeCommand(string.Format("button -label \"{0}\" -command \"InazumaBasicWindow {1} {2} {3}\";", pairs[i][j].label, windowName, i, j),true);
                }
                //btnCmdStr += "setParent ..;\n";
                MGlobal.executeCommand("setParent ..;",true);
            }

            //MGlobal.executeCommandOnIdle(btnCmdStr, true);
            MGlobal.executeCommand("showWindow " + runtimeWindowName, true);

        }




    }


    public class BasicWindowCommand : MPxCommand
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args">order: windowName, lineIndex , buttonIndex</param>
        public override void doIt(MArgList args)
        {
            //Debug.Log("at least do it....");
            if (args.length != 3)
            {
                //Debug.Log("param count error:" + args.length);
                return;
            }
            string wName = args.asString(0);
            int lineIndex = args.asInt(1);
            int buttonIndex = args.asInt(2);

            //Debug.Log("name:" + wName + ",line:" + lineIndex + ",btn:" + buttonIndex);

            BasicWindow.ExcuteInDic(wName, lineIndex, buttonIndex);
            

        }
    }
}
