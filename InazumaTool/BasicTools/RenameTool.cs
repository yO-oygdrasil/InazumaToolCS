using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Maya.OpenMaya;
using System.IO;
using InazumaTool.BasicTools.UI;

namespace InazumaTool.BasicTools
{
    static class RenameTool
    {
        public static void SaveDagsHierachyNamesToFile(MFnTransform rootTrans,string filePath,bool overwrite = false)
        {
            string result = "";
            IterateHierachy(rootTrans, 0, ref result);
            //test debug
            Debug.Log(result);
            if (File.Exists(filePath))
            {
                if (!overwrite)
                {
                    Debug.Log("文件已存在，请选择覆盖模式或者切换目标路径");
                    return;
                }
            }
            else
            {
                string targetFolderName = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(targetFolderName))
                {
                    Directory.CreateDirectory(targetFolderName);
                }
            }
            File.WriteAllText(filePath, result);
        }

        public static void IterateHierachy(MFnTransform rootTrans, int rootLevel, ref string resultStr)
        {
            for (int i = 0; i < rootLevel; i++)
            {
                resultStr += "\t";
            }
            resultStr += rootTrans.name + "\n";
            for (int i = 0; i < rootTrans.childCount; i++)
            {
                IterateHierachy(new MFnTransform(rootTrans.child((uint)i)), rootLevel + 1, ref resultStr);
            }
            


        }



        const string cmdStr = "RenameTool";
        public static List<CommandData> GetCommandDatas()
        {
            List<CommandData> cmdList = new List<CommandData>();


            cmdList.Add(new CommandData("重命名", "图片"));
            cmdList.Add(new CommandData("重命名", cmdStr, "renameToolWindow", "重命名编辑器", () =>
            {
                RenameToolWindow rtw = new RenameToolWindow();
                rtw.Show();
            }));
            
            return cmdList;
        }

    }
}
