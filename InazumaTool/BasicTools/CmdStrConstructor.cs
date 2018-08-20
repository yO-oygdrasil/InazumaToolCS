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
        List<string> targetNames = new List<string>();
        List<string> toggles = new List<string>();
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
        /// <summary>
        /// 暂时只有mel模式下加入了
        /// </summary>
        /// <param name="toggleName"></param>
        /// <param name="exist"></param>
        public void UpdateToggle(string toggleName,bool exist = true)
        {
            if (toggles.Contains(toggleName))
            {
                if (!exist)
                {
                    toggles.Remove(toggleName);
                }
            }
            else if (exist)
            {
                toggles.Add(toggleName);
            }
        }


        public bool UpdateTargets(List<string> names)
        {
            if (type == CmdType.Mel)
            {
                targetNames = names;
                return true;
            }
            else
            {
                return false;
            }
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
                        for (int i = 0; i < toggles.Count; i++)
                        {
                            result += " -" + toggles[i];
                        }
                        for (int i = 0; i < targetNames.Count; i++)
                        {
                            result += " " + targetNames[i];
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
