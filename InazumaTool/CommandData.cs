using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InazumaTool
{
    public class CommandData
    {
        public string cmdTypeStr;
        public string paramStr;
        public string subMenuName;
        public string labelStr;
        public Action action;
        public CommandData(string subMenu, string cmdStr, string param, string label, Action act)
        {
            cmdTypeStr = cmdStr;
            paramStr = param;
            labelStr = label;
            action = act;
            subMenuName = subMenu;
        }
        public bool TryResponse(string cmdStr, string param)
        {
            if (cmdTypeStr == cmdStr && paramStr == param)
            {
                action();
                return true;
            }
            else
            {
                return false;
            }
        }
        public string DebugMessage()
        {
            return cmdTypeStr + " " + paramStr + " [to] " + (subMenuName == null ? "" : subMenuName);
        }

    }
}
