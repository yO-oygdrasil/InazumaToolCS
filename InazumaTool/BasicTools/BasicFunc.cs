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

        public static MSelectionList GetSelectedDagPaths()
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
            MGlobal.displayInfo("hahahah");
            MDagPath mDagPath = new MDagPath();
            MGlobal.displayInfo("fffff");
            if (index < matched.length)
            {
                matched.getDagPath((uint)index, mDagPath);
            }
            return mDagPath;
        }

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
            MFnTransform circleTransform  = new MFnTransform(ctlDagPath);
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

        public static bool CreateLocator(MDagPath locDagPath, MVector worldPos, string locatorName)
        {
            string cmdStr = "cmds.spaceLocator(n='" + locatorName + "')";


            locatorName = SubUShell(MGlobal.executePythonCommandStringResult(cmdStr));
            locDagPath = BasicFunc.GetDagPathByName(locatorName);
            MFnTransform locatorTrans = new MFnTransform(locDagPath);
            //MGlobal.displayInfo(locatorName+"dag:"+locDagPath.fullPathName);
            locatorTrans.setTranslation(worldPos, MSpace.Space.kWorld);
            return true;
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

        public static void IterateChidren(Action<MDagPath> dealMethod, MDagPath rootNode)
        {

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

        public static void SetTransformParent(MFnTransform c, MFnTransform p)
        {
            SetTransformParent(c.fullPathName, p.fullPathName);
        }

        public static void SetTransformParent(string cFullName, string pFullName)
        {
            MGlobal.executePythonCommand("cmds.parent('" + cFullName + "','" + pFullName + "')", true);
        }

        public static void UnparentTransform(MDagPath  dagPath)
        {
            MGlobal.executeCommand("parent -w " + dagPath.fullPathName);
        }

        public static void UnparentTransform(MFnTransform  mfnTrans)
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

        public static string SubUShell(string originStr)
        {
            return originStr.Replace("[u'", "").Replace("'", "").Replace(" ", "").Replace("]", "");
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







    }
}
