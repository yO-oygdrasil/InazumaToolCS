using Autodesk.Maya.OpenMaya;
using System.Collections.Generic;

namespace InazumaTool.BasicTools
{
    public static class TransformTool
    {
        static void PlaceOneLine(MFnTransform[] transList)
        {
            if (transList.Length < 3)
            {
                return;
            }
            MVector lastPos = transList[1].getTranslation(MSpace.Space.kWorld);
            MVector offset = lastPos - transList[0].getTranslation(MSpace.Space.kWorld);
            

            for (int i = 2; i < transList.Length; i++)
            {
                lastPos = offset + lastPos;
                transList[i].setTranslation(lastPos, MSpace.Space.kWorld);
            }
        }




        const string cmdStr = "TransformTool";
        public static List<CommandData> GetCommandDatas()
        {
            List<CommandData> cmdList = new List<CommandData>();
            cmdList.Add(new CommandData("变换工具", cmdStr, "oneLine", "一字排开", () =>
            {
                PlaceOneLine(BasicFunc.GetSelectedTransforms().ToArray());
            }));

            return cmdList;
        }
    }
}