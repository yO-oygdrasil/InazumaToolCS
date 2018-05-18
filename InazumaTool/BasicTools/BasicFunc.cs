using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Autodesk.Maya.OpenMaya;

namespace InazumaTool.BasicTools
{
    public static class BasicFunc
    {
        #region Set Transform
        public static void SetTranslateLimit(MObject mObject, float minX, float minY, float minZ, float maxX, float maxY, float maxZ)
        {
            MFnTransform mfnTransform = new MFnTransform(mObject);
            SetTranslateLimit(mfnTransform, minX, minY, minZ, maxX, maxY, maxZ);
        }

        public static void SetTranslateLimit(MFnTransform mfnTrans, float minX, float minY, float minZ, float maxX, float maxY, float maxZ)
        {
            mfnTrans.enableLimit(MFnTransform.LimitType.kTranslateMinX, true);
            mfnTrans.enableLimit(MFnTransform.LimitType.kTranslateMinY, true);
            mfnTrans.enableLimit(MFnTransform.LimitType.kTranslateMinZ, true);
            mfnTrans.enableLimit(MFnTransform.LimitType.kTranslateMaxX, true);
            mfnTrans.enableLimit(MFnTransform.LimitType.kTranslateMaxY, true);
            mfnTrans.enableLimit(MFnTransform.LimitType.kTranslateMaxZ, true);
            mfnTrans.setLimit(MFnTransform.LimitType.kTranslateMinX, minX);
            mfnTrans.setLimit(MFnTransform.LimitType.kTranslateMinY, minY);
            mfnTrans.setLimit(MFnTransform.LimitType.kTranslateMinZ, minZ);
            mfnTrans.setLimit(MFnTransform.LimitType.kTranslateMaxX, maxX);
            mfnTrans.setLimit(MFnTransform.LimitType.kTranslateMaxY, maxY);
            mfnTrans.setLimit(MFnTransform.LimitType.kTranslateMaxZ, maxZ);
        }



        public static void SetTransformParent(MFnTransform c, MFnTransform p)
        {
            SetTransformParent(c.fullPathName, p.fullPathName);
        }

        public static void SetTransformParent(string cFullName, string pFullName)
        {
            MGlobal.executePythonCommand("cmds.parent('" + cFullName + "','" + pFullName + "')", true);
        }

        public static void UnparentTransform(MDagPath dagPath)
        {
            MGlobal.executeCommand("parent -w " + dagPath.fullPathName);
        }

        public static void UnparentTransform(MFnTransform mfnTrans)
        {
            MGlobal.executeCommand("parent -w " + mfnTrans.fullPathName);
        }

        public static void FreezeTransform(MFnTransform targetTransform)
        {
            MGlobal.executePythonCommand("cmds.makeIdentity('" + targetTransform.fullPathName + "',apply=True)", true);
        }

        public static void ClearTransform(MFnTransform targetTransform, bool clearPos = true, bool clearPivotRot = true, bool clearPivotScale = true)
        {
            if (clearPos)
            {
                targetTransform.setTranslation(MVector.zero, MSpace.Space.kObject);
            }
            if (clearPivotRot)
            {
                targetTransform.setRotatePivotTranslation(MVector.zero, MSpace.Space.kObject);
            }
            if (clearPivotScale)
            {
                targetTransform.setScalePivotTranslation(MVector.one, MSpace.Space.kObject);
            }
        }
        #endregion

        #region ToStr
        public static string ToCMDSParamStr(MVector vector)
        {
            return "(" + vector.x + "," + vector.y + "," + vector.z + ")";
        }

        public static string ToCMDSParamStr(int[] arr, int count, char concatChar = ',')
        {
            string result = "";
            for (int i = 0; i < count; i++)
            {
                if (i != 0)
                {
                    result += concatChar;
                }
                result += arr[i];
            }
            //MGlobal.displayInfo("result int array str:" + result);
            return result;
        }

        public static string MToString(MColor color)
        {
            return string.Format("({0},{1},{2},{3})", color.r, color.g, color.b, color.a);
        }
        #endregion

        #region Get
        public static MObject GetSelectedObject(int index)
        {
            MSelectionList selected = new MSelectionList();
            MGlobal.getActiveSelectionList(selected);
            MObject mObject = new MObject();
            if (index < selected.length)
            {
                selected.getDependNode((uint)index, mObject);
            }
            return mObject;
        }

        public static MDagPath GetSelectedDagPath(int index)
        {
            MSelectionList selected = new MSelectionList();
            MGlobal.getActiveSelectionList(selected);
            MDagPath dagPath = new MDagPath();
            if (index < selected.length)
            {
                selected.getDagPath((uint)index, dagPath);
            }
            return dagPath;
        }

        public static MSelectionList GetSelectedList()
        {
            MSelectionList selected = new MSelectionList();
            MGlobal.getActiveSelectionList(selected);
            return selected;
        }

        public static MSelectionList GetObjectsByName(string name)
        {
            MSelectionList matched = new MSelectionList();
            MGlobal.getSelectionListByName(name, matched);
            //MGlobal.displayInfo("ask if [" + name + "] exist,result count:" + matched.length);
            return matched;
        }

        public static MObject GetObjectByName(string name, int index)
        {
            MSelectionList matched = GetObjectsByName(name);
            MObject mObject = new MObject();
            if (index < matched.length)
            {
                matched.getDependNode((uint)index, mObject);
            }
            return mObject;
        }

        public static MDagPath GetDagPathByName(string name, int index = 0)
        {
            MSelectionList matched = GetObjectsByName(name);
            MDagPath mDagPath = new MDagPath();
            if (index < matched.length)
            {
                matched.getDagPath((uint)index, mDagPath);
            }
            return mDagPath;
        }


        #endregion

        #region Create
        public static MDagPath AddChildCircle(MObject targetObject)
        {
            if (targetObject.hasFn(MFn.Type.kTransform))
            {
                return AddChildCircle(MDagPath.getAPathTo(targetObject));
            }
            else
            {
                return null;
            }
        }

        public static MDagPath AddChildCircle(MDagPath targetDagPath)
        {
            string ctlName = "ctl_" + targetDagPath.partialPathName;
            MDagPath ctlDagPath = BasicFunc.CreateCircle(ctlName);
            ctlName = ctlDagPath.fullPathName;
            SetTransformParent(ctlName, targetDagPath.fullPathName);
            MFnTransform circleTransform = new MFnTransform(ctlDagPath);
            circleTransform.setTranslation(new MVector(0, 0, 0), MSpace.Space.kObject);
            circleTransform.setRotation(new MEulerRotation(0, 90 / ConstantValue.DPR, 0));
            FreezeTransform(circleTransform);
            return ctlDagPath;
        }

        public static MDagPath AddParentCircle(MObject targetObject, bool createParallelGrp)
        {
            if (targetObject.hasFn(MFn.Type.kTransform))
            {
                return AddParentCircle(MDagPath.getAPathTo(targetObject), createParallelGrp);
            }
            else
            {
                return null;
            }
        }

        public static MDagPath AddParentCircle(MDagPath targetDagPath, bool createParallelGrp)
        {
            string ctlName = "ctl_" + targetDagPath.partialPathName;
            MDagPath ctlDagPath = BasicFunc.CreateCircle(ctlName);
            ctlName = ctlDagPath.fullPathName;
            MFnTransform targetTrans = new MFnTransform(targetDagPath);
            if (createParallelGrp)
            {
                MFnTransform parellelGrpTrans = new MFnTransform(AddEmptyGroup(new MFnTransform(targetTrans.parent(0))));
                parellelGrpTrans.setTranslation(targetTrans.getTranslation(MSpace.Space.kTransform), MSpace.Space.kTransform);
                //MGlobal.displayInfo("finalLocalPos:"+BasicFunc.ToCMDSParamStr(parellelGrpTrans.getTranslation(MSpace.kTransform)));
                parellelGrpTrans.setRotatePivotTranslation(targetTrans.rotatePivotTranslation(MSpace.Space.kTransform), MSpace.Space.kTransform);
                SetTransformParent(ctlName, parellelGrpTrans.fullPathName);
            }

            MFnTransform circleTransform = new MFnTransform(ctlDagPath);
            circleTransform.setTranslation(new MVector(0, 0, 0), MSpace.Space.kTransform);
            circleTransform.setRotation(new MEulerRotation(0, 90 / ConstantValue.DPR, 0));
            FreezeTransform(circleTransform);
            return ctlDagPath;
        }

        public static MDagPath CreateLocator(MVector worldPos, string locatorName)
        {
            string cmdStr = "cmds.spaceLocator(n='" + locatorName + "')";

            locatorName = SubUShell(MGlobal.executePythonCommandStringResult(cmdStr));
            MDagPath locDagPath = BasicFunc.GetDagPathByName(locatorName);
            MFnTransform locatorTrans = new MFnTransform(locDagPath);
            //MGlobal.displayInfo(locatorName+"dag:"+locDagPath.fullPathName);
            locatorTrans.setTranslation(worldPos, MSpace.Space.kWorld);
            return locDagPath;
        }

        //error here
        public static MDagPath CreateCircle(string ctlName)
        {
            string resultStr = MGlobal.executePythonCommandStringResult("cmds.circle(n='" + ctlName + "')");
            //string resultStr = MGlobal.executeCommandStringResult("circle -n " + ctlName);
            string[] resultArr = SplitPythonResultStr(resultStr);
            for (int i = 0; i < resultArr.Length; i++)
            {
                MGlobal.displayInfo(resultArr[i]);
            }
            return GetDagPathByName(resultArr[0]);
        }

        public static MDagPath CreateCTL_Crystal(string ctlName)
        {
            string resultName = MGlobal.executePythonCommandStringResult("cmds.curve(n='" + ctlName + @"', d=1,
p=[(0,1,0),(0,0,1),(1,0,0),(0,-1,0),(0,0,-1),(1,0,0),(0,1,0),
(0,0,-1),(-1,0,0),(0,-1,0),(0,0,1),(-1,0,0),(0,1,0)],
k=[0,1,2,3,4,5,6,7,8,9,10,11,12])");
            //MGlobal.displayInfo("circleName_BeforeSub:" + resultName);
            //resultName = SubUShell(resultName);
            //MGlobal.displayInfo("circleName_AfterSub:" + resultName);
            return GetDagPathByName(resultName);
        }

        public static MDagPath CreateCurve(MVector[] points, int ptCount, string curveName)
        {
            string cmdStr = "cmds.curve(n='" + curveName + "',d=1,p=[";
            for (int i = 0; i < ptCount; i++)
            {
                if (i != 0)
                {
                    cmdStr += ",";
                }
                cmdStr += ToCMDSParamStr(points[i]);
            }
            int[] indices = new int[ptCount];
            for (int i = 0; i < ptCount; i++)
            {
                indices[i] = i;
            }
            cmdStr += "],k=[" + ToCMDSParamStr(indices, ptCount) + "])";
            MGlobal.displayInfo(cmdStr);
            string resultName = MGlobal.executePythonCommandStringResult(cmdStr);
            return GetDagPathByName(resultName);
        }

        public static MFnDependencyNode CreateRemapValueNode(float inputMin, float inputMax, float outputMin, float outputMax)
        {
            MFnDependencyNode dependencyNode = new MFnDependencyNode();
            dependencyNode.create("remapValue");
            //MGlobal.displayInfo("created node:" + (*dependencyNode).absoluteName());
            dependencyNode.findPlug("inputMin").setFloat(inputMin);
            dependencyNode.findPlug("inputMax").setFloat(inputMax);
            dependencyNode.findPlug("outputMin").setFloat(outputMin);
            dependencyNode.findPlug("outputMax").setFloat(outputMax);
            return dependencyNode;
        }
        public static MDagPath AddEmptyGroup(MFnTransform parent)
        {
            return AddEmptyGroup("grp_childrenOf" + parent.partialPathName, parent.fullPathName);
        }

        public static MDagPath AddEmptyGroup(string grpName, string parentName)
        {
            string resultGrpName = MGlobal.executeCommandStringResult("group -em -n " + grpName);
            MDagPath resultGrpDagPath = GetDagPathByName(resultGrpName);
            if (parentName.Length > 0)
            {
                MFnTransform resultGrpTrans = new MFnTransform(resultGrpDagPath);
                SetTransformParent(resultGrpName, parentName);
                ClearTransform(resultGrpTrans);
                FreezeTransform(resultGrpTrans);
            }
            return resultGrpDagPath;
        }

        #endregion

        public static void IterateChidren(Action<MDagPath> dealMethod, MDagPath rootNode)
        {

        }

        #region Delete
        public static bool DeleteObject(MObject mo)
        {
            if (mo == null)
            {
                return false;
            }
            MDGModifier dGModifier = new MDGModifier();
            dGModifier.deleteNode(mo);
            dGModifier.doIt();
            return true;
        }

        public static bool DeleteObjects(List<MObject> list)
        {
            if (list == null)
            {
                return false;
            }
            MDGModifier dGModifier = new MDGModifier();
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == null)
                {
                    continue;
                }
                dGModifier.deleteNode(list[i]);
            }
            dGModifier.doIt();
            return true;
        }


        
        #endregion

        #region Select

        public static void Select(MSelectionList list)
        {
            MGlobal.setActiveSelectionList(list);
        }

        public static void Select(MDagPath dagPath)
        {
            MSelectionList list = new MSelectionList();
            list.add(dagPath);
            Select(list);
        }
        #endregion


        #region DealResultStr
        public static string SubUShell(string originStr)
        {
            int startIndex = originStr.IndexOf('\'');
            int endIndex = originStr.LastIndexOf('\'');
            return originStr.Substring(startIndex + 1, endIndex - startIndex - 1);
            //return originStr.Replace("[u'", "").Replace("u'", "").Replace("'", "").Replace(" ", "").Replace("]", "");
        }

        public static string[] SplitPythonResultStr(string pythonStr)
        {
            string[] result = pythonStr.Split(',');
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = SubUShell(result[i]);
                MGlobal.displayInfo(result[i]);
            }
            return result;
        }
        #endregion

        #region Calculate

        public static MVector Cross(MVector u, MVector v)
        {
            return new MVector(u.y * v.z - u.z * v.y, u.x * v.z - u.z * v.x, u.x * v.y - u.y * v.x);
        }

        public static double Dot(MVector u, MVector v)
        {
            return u.x * v.x + u.y * v.y + u.z * v.z;
        }

        public static MVector Project(MVector vec, MVector direction)
        {
            double dot = Dot(vec, direction);
            return vec * (dot / direction.length);
        }

        public static MVector VerticalProject(MVector vec, MVector direction)
        {
            return vec - Project(vec, direction);
        }

        #endregion

        #region Debug Output

        public static void PrintDags(MSelectionList list)
        {
            for (int i = 0; i < list.length; i++)
            {
                uint index = (uint)i;
                MDagPath mdp = new MDagPath();
                list.getDagPath(index, mdp);
                MGlobal.displayInfo(index + ":" + mdp.fullPathName);
            }
        }

        public static void PrintObjects(MSelectionList list)
        {
            for (int i = 0; i < list.length; i++)
            {
                uint index = (uint)i;
                MObject mo = new MObject();
                list.getDependNode(index, mo);
                MGlobal.displayInfo(index + ":typeof:" + mo.apiTypeStr);

            }


        }

        public static void PrintPlugs(MPlugArray plugs)
        {
            for (int i = 0; i < plugs.length; i++)
            {
                MGlobal.displayInfo(i + ":" + plugs[i].name);
            }
        }

        #endregion

        const string cmdStr = "BasicFunc";
        public static List<CommandData> GetCommandDatas()
        {
            List<CommandData> cmdList = new List<CommandData>();
            cmdList.Add(new CommandData(null, cmdStr, "test", "test", () => 
            {
                BasicFunc.PrintObjects(BasicFunc.GetSelectedList());
            }));
            cmdList.Add(new CommandData("基本操作", cmdStr, "childCircle", "增加子层级圆环", () =>
            {
                AddChildCircle(GetSelectedDagPath(0));                
            }));
            cmdList.Add(new CommandData("基本操作", cmdStr, "parentCircle", "增加同层级圆环", () =>
            {
                AddParentCircle(GetSelectedDagPath(0), true);
            }));
            cmdList.Add(new CommandData("基本操作", cmdStr, "crystal", "创建八面体控制器", () =>
            {
                BasicFunc.CreateCTL_Crystal("ctl_sample");
            }));
            cmdList.Add(new CommandData("基本操作", cmdStr, "crystal", "创建偏方三八面体", () =>
            {
                BasicFunc.CreateCTL_Crystal("ctl_sample");
            }));
            return cmdList;
        }
    }
}
