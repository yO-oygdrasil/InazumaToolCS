using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InazumaTool.BasicTools
{
    class BasicWindow
    {
        public const int btnWidth = 40;
        public const int btnHeight = 10;

        private class ActionPair
        {
            public Action action;
            public string label;
        }
        private List<List<ActionPair>> pairs = new List<List<ActionPair>>();



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

        public void Show()
        {
            
        }


    }
}
