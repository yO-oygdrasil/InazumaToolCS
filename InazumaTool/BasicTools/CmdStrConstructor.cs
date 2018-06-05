using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InazumaTool.BasicTools
{
    class CmdStrConstructor
    {
        public CmdStrConstructor(string order, CmdType t = CmdType.Mel)
        {
            mainOrder = order;
            type = t;
        }
        public enum CmdType
        {
            Python,
            Mel
        }
        string mainOrder;
        CmdType type = CmdType.Mel;
        Dictionary<string, string> parmsDic = new Dictionary<string, string>();

        public void UpdateParm(string parmName, float value)
        {
            UpdateCMDParmStr(parmName, value + "");
        }
        public void UpdateParm(string parmName, int value)
        {
            UpdateCMDParmStr(parmName, value + "");
        }
        public void UpdateParm(string parmName, bool value)
        {
            UpdateCMDParmStr(parmName, (value ? 1 : 0) + "");
        }
        public void UpdateParm(string parmName, string value)
        {
            UpdateCMDParmStr(parmName, "\"" + value + "\"");
        }

        private void UpdateCMDParmStr(string parmName,string cmdValue)
        {
            if (!parmsDic.ContainsKey(parmName))
            {
                parmsDic[parmName] = cmdValue;
            }
            else
            {
                parmsDic.Add(parmName, cmdValue);
            }
        }

        public override string ToString()
        {
            string result = "";
            switch (type)
            {
                case CmdType.Mel:
                    {
                        result = mainOrder;
                        foreach (string key in parmsDic.Keys)
                        {
                            result += string.Format(" -{0} {1}", key, parmsDic[key]);
                        }                        
                        break;
                    }
                case CmdType.Python:
                    {
                        result = "cmds." + mainOrder + "(";
                        bool firstParm = true;
                        foreach (string key in parmsDic.Keys)
                        {
                            if (!firstParm)
                            {
                                result += string.Format(",{0}={1}", key, parmsDic[key]);
                            }
                            else
                            {
                                result += string.Format("{0}={1}", key, parmsDic[key]);
                                firstParm = false;
                            }
                        }
                        result += ")";
                        break;
                    }
            }
            return result;
        }




    }
}
