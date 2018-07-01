using Autodesk.Maya.OpenMaya;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: MPxCommandClass(typeof(InazumaTool.BasicTools.BasicWindow), "InazumaBasicWindow")]

namespace InazumaTool.BasicTools
{


    class BasicWindow : MPxCommand
    {
        private static Dictionary<string, BasicWindow> basicWindowDic = new Dictionary<string, BasicWindow>(); 


        public const int btnWidth = 40;
        public const int btnHeight = 10;

        private class ActionPair
        {
            public Action action;
            public string label;
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

            //default next line
            for (int i = 0; i < actions.Length; i++)
            {

            }
        }

        public void Excute(int lineIndex, int buttonIndex)
        {
            if (pairs.Count > lineIndex)
            {
                if (pairs[lineIndex].Count > buttonIndex)
                {
                    pairs[lineIndex][buttonIndex].action.Invoke();
                }
            }
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

            runtimeWindowName = MGlobal.executeCommandStringResult(string.Format("window -title \"{0}\" -widthHeight {1} {2};", windowName, columnCount * btnWidth, rowCount * btnHeight));

            string btnCmdStr = "";
            for (int i = 0; i < rowCount; i++)
            {
                btnCmdStr += "columnLayout -adjustableColumn true:\n";
                //MGlobal.executeCommandOnIdle("columnLayout -adjustableColumn true:");
                for (int j = 0; j < pairs[i].Count; j++)
                {
                    btnCmdStr += string.Format("button -label \"{0}\" -command \"InazumaBasicWindow {1} {2} {3}\";\n", pairs[i][j].label, windowName, i, j);
                    //MGlobal.executeCommandOnIdle(string.Format("button -label \"{0}\" -command \"InazumaBasicWindow {1} {2} {3}\";", pairs[i][j].label, windowName, i, j));
                }
                btnCmdStr += "setParent ..;\n";
                //MGlobal.executeCommandOnIdle("setParent ..;");
            }

            MGlobal.executeCommandOnIdle(btnCmdStr);
            MGlobal.executeCommandOnIdle("showWindow " + runtimeWindowName);

        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="args">order: windowName, lineIndex , buttonIndex</param>
        public override void doIt(MArgList args)
        {
            if (args.length != 0)
            {
                MGlobal.displayInfo("param count error:" + args.length);
                return;
            }
            string wName = args.asString(0);
            int lineIndex = args.asInt(1);
            int buttonIndex = args.asInt(2);
            if (basicWindowDic.ContainsKey(wName))
            {
                basicWindowDic[wName].Excute(lineIndex, buttonIndex);
            }

        }
    }
}
