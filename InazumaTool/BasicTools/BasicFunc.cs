using System;
using System.Collections.Generic;
using System.IO;
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

        public static void SetRotationLimit(MFnTransform mfnTrans, float minX, float minY, float minZ, float maxX, float maxY, float maxZ)
        {
            mfnTrans.enableLimit(MFnTransform.LimitType.kRotateMinX, true);
            mfnTrans.enableLimit(MFnTransform.LimitType.kRotateMinY, true);
            mfnTrans.enableLimit(MFnTransform.LimitType.kRotateMinZ, true);
            mfnTrans.enableLimit(MFnTransform.LimitType.kRotateMaxX, true);
            mfnTrans.enableLimit(MFnTransform.LimitType.kRotateMaxY, true);
            mfnTrans.enableLimit(MFnTransform.LimitType.kRotateMaxZ, true);
            mfnTrans.setLimit(MFnTransform.LimitType.kRotateMinX, minX);
            mfnTrans.setLimit(MFnTransform.LimitType.kRotateMinY, minY);
            mfnTrans.setLimit(MFnTransform.LimitType.kRotateMinZ, minZ);
            mfnTrans.setLimit(MFnTransform.LimitType.kRotateMaxX, maxX);
            mfnTrans.setLimit(MFnTransform.LimitType.kRotateMaxY, maxY);
            mfnTrans.setLimit(MFnTransform.LimitType.kRotateMaxZ, maxZ);
        }

        public static void SetTransformLimit(MFnTransform mfnTrans, float value, MFnTransform.LimitType type)
        {
            mfnTrans.enableLimit(type, true);
            mfnTrans.setLimit(type, value);
        }

        public static MFnTransform GetParent(MDagPath dag, int index = 0)
        {
            return new MFnTransform(MDagPath.getAPathTo(new MFnTransform(dag).parent((uint)index)));
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
            //Debug.Log("result int array str:" + result);
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

        public static MSelectionList GetSelectedList(MFn.Type type = MFn.Type.kInvalid)
        {
            MSelectionList selected = new MSelectionList();
            MGlobal.getActiveSelectionList(selected);
            if (type != MFn.Type.kInvalid)
            {
                for (int i = (int)(selected.length - 1); i >= 0; i--)
                {
                    MObject mo = new MObject();
                    selected.getDependNode((uint)i, mo);
                    if (!mo.hasFn(type))
                    {
                        selected.remove((uint)i);
                    }
                }
            }            
            return selected;
        }
        

        public static MSelectionList GetObjectsByName(string name)
        {
            MSelectionList matched = new MSelectionList();
            MGlobal.getSelectionListByName(name, matched);
            //Debug.Log("ask if [" + name + "] exist,result count:" + matched.length);
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

        public static string GetFileName(string fullPath,bool subSuffix = true)
        {
            return subSuffix ? Path.GetFileNameWithoutExtension(fullPath) : Path.GetFileName(fullPath);
            
        }


        public static List<MDagPath> GetHierachyChain(MDagPath startDag, MFn.Type filterType = MFn.Type.kInvalid, MDagPath endDag = null)
        {
            MFnTransform currentTrans = new MFnTransform(startDag);
            //no need for trans data , so mfntransform(mobject) is used
            List<MDagPath> result = new List<MDagPath>();
            result.Add(startDag);
            while (currentTrans.childCount > 0)
            {
                int nextIndex = 0;
                if (filterType != MFn.Type.kInvalid)
                {
                    bool filterOK = false;
                    for (int i = 0; i < currentTrans.childCount; i++)
                    {
                        MObject mo = currentTrans.child((uint)i);
                        if (mo.hasFn(filterType))
                        {
                            nextIndex = i;
                            filterOK = true;
                            break;
                        }
                    }
                    if (!filterOK)
                    {
                        break;
                    }
                }
                currentTrans = new MFnTransform(currentTrans.child((uint)nextIndex));
                MDagPath dag = currentTrans.dagPath;
                result.Add(dag);
                if (endDag != null && dag.fullPathName == endDag.fullPathName)
                {
                    break;
                }
            }
            return result;
        }

        public static List<MDagPath> GetHierachyAll(MDagPath startDag, MFn.Type filterType = MFn.Type.kInvalid)
        {
            MFnTransform currentTrans = new MFnTransform(startDag);
            //no need for trans data , so mfntransform(mobject) is used
            List<MDagPath> result = new List<MDagPath>();

            AddChildrenToList(result, currentTrans, filterType);

            return result;
        }

        public static List<MFnTransform> GetHierachyAllTrans(MDagPath startDag, MFn.Type filterType = MFn.Type.kInvalid)
        {
            MFnTransform currentTrans = new MFnTransform(startDag);
            //no need for trans data , so mfntransform(mobject) is used
            List<MFnTransform> result = new List<MFnTransform>();

            AddChildrenToList(result, currentTrans, filterType);

            return result;
        }

        private static void AddChildrenToList(List<MFnTransform> list, MFnTransform rootTrans, MFn.Type filterType = MFn.Type.kInvalid)
        {
            if (filterType == MFn.Type.kInvalid || rootTrans.dagPath.hasFn(filterType))
            {
                list.Add(rootTrans);
            }
            if (rootTrans.childCount > 0)
            {
                for (int i = 0; i < rootTrans.childCount; i++)
                {
                    MObject mo = rootTrans.child((uint)i);
                    MDagPath dag = MDagPath.getAPathTo(mo);
                    AddChildrenToList(list, new MFnTransform(dag), filterType);
                }                
            }
        }

        private static void AddChildrenToList(List<MDagPath> dagList, MFnTransform rootTrans, MFn.Type filterType = MFn.Type.kInvalid)
        {
            if (filterType == MFn.Type.kInvalid || rootTrans.dagPath.hasFn(filterType))
            {
                dagList.Add(rootTrans.dagPath);
            }
            if (rootTrans.childCount > 0)
            {
                for (int i = 0; i < rootTrans.childCount; i++)
                {
                    MObject mo = rootTrans.child((uint)i);
                    MDagPath dag = MDagPath.getAPathTo(mo);
                    AddChildrenToList(dagList, new MFnTransform(dag), filterType);
                }
            }
        }

        public static List<MFnTransform> GetHierachyChainTrans(MDagPath startDag, MFn.Type filterType = MFn.Type.kInvalid, MDagPath endDag = null)
        {
            MFnTransform currentTrans = new MFnTransform(startDag);
            //no need for trans data , so mfntransform(mobject) is used
            List<MFnTransform> result = new List<MFnTransform>();
            result.Add(currentTrans);
            while (currentTrans.childCount > 0)
            {
                MObject filteredMO = new MObject();
                if (filterType != MFn.Type.kInvalid)
                {
                    bool filterOK = false;
                    for (int i = 0; i < currentTrans.childCount; i++)
                    {
                        filteredMO = currentTrans.child((uint)i);
                        if (filteredMO.hasFn(filterType))
                        {
                            filterOK = true;
                            break;
                        }
                    }
                    if (!filterOK)
                    {
                        break;
                    }
                }
                else
                {
                    filteredMO = currentTrans.child(0);
                }
                MDagPath dag = MDagPath.getAPathTo(filteredMO);
                currentTrans = new MFnTransform(dag);
                result.Add(currentTrans);
                if (endDag != null && dag.fullPathName == endDag.fullPathName)
                {
                    break;
                }
            }
            return result;
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
                //Debug.Log("finalLocalPos:"+BasicFunc.ToCMDSParamStr(parellelGrpTrans.getTranslation(MSpace.kTransform)));
                MEulerRotation rot = new MEulerRotation();
                targetTrans.getRotation(rot);
                parellelGrpTrans.setRotation(rot);
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
            //Debug.Log(locatorName+"dag:"+locDagPath.fullPathName);
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
                Debug.Log(resultArr[i]);
            }
            return GetDagPathByName(resultArr[0]);
        }

        public static MDagPath CreateCTL_Crystal(string ctlName = "crystal")
        {
            MPointArray points = new MPointArray();
            points.Add(new MPoint(0, 1, 0));
            points.Add(new MPoint(0, 0, 1));
            points.Add(new MPoint(1, 0, 0));
            points.Add(new MPoint(0, -1, 0));

            points.Add(new MPoint(0, 0, -1));
            points.Add(new MPoint(1, 0, 0));
            points.Add(new MPoint(0, 1, 0));
            points.Add(new MPoint(0, 0, -1));

            points.Add(new MPoint(-1, 0, 0));
            points.Add(new MPoint(0, -1, 0));
            points.Add(new MPoint(0, 0, 1));
            points.Add(new MPoint(-1, 0, 0));
            points.Add(new MPoint(0, 1, 0));
            return CreateCurve(points, ctlName, 1, MFnNurbsCurve.Form.kClosed);            
        }

        public static MDagPath CreateCTL_Square(string ctlName = "square",float height = 1, float width = 1)
        {
            MPointArray points = new MPointArray();
            float up = height / 2;
            float left = width / 2;
            points.Add(new MPoint(left, up, 0));
            points.Add(new MPoint(left, -up, 0));
            points.Add(new MPoint(-left, -up, 0));
            points.Add(new MPoint(-left, up, 0));
            points.Add(new MPoint(left, up, 0));
            return CreateCurve(points, ctlName, 1, MFnNurbsCurve.Form.kClosed);
        }

        public static MDagPath CreateCTL_Square(string ctlName = "square", float up = 0.5f, float down = -0.5f, float left = -0.5f, float right = 0.5f)
        {
            MPointArray points = new MPointArray();
            points.Add(new MPoint(left, up, 0));
            points.Add(new MPoint(left, down, 0));
            points.Add(new MPoint(right, down, 0));
            points.Add(new MPoint(right, up, 0));
            points.Add(new MPoint(left, up, 0));
            return CreateCurve(points, ctlName, 1, MFnNurbsCurve.Form.kClosed);
        }

        public static MDagPath CreateEmptyGroup(string grpName = "grp_empty")
        {
            return BasicFunc.GetDagPathByName(MGlobal.executeCommandStringResult(string.Format("group -em -n \"{0}\"", grpName)));
        }

        public static MDagPath CreateCurve(MPoint[] pts, string curveName, int degree = 1, MFnNurbsCurve.Form form = MFnNurbsCurve.Form.kOpen)
        {
            MPointArray points = new MPointArray(pts);
            return CreateCurve(points, curveName, degree, form);
        }

        public static MDagPath CreateCurve(MVector[] positions, string curveName, int degree = 1, MFnNurbsCurve.Form form = MFnNurbsCurve.Form.kOpen)
        {
            MPointArray points = new MPointArray();
            for (int i = 0; i < positions.Length; i++)
            {
                points.Add(new MPoint(positions[i]));
            }
            return CreateCurve(points, curveName, degree, form);
            //string cmdStr = "cmds.curve(n='" + curveName + "',d=1,p=[";
            //for (int i = 0; i < ptCount; i++)
            //{
            //    if (i != 0)
            //    {
            //        cmdStr += ",";
            //    }
            //    cmdStr += ToCMDSParamStr(points[i]);
            //}
            //int[] indices = new int[ptCount];
            //for (int i = 0; i < ptCount; i++)
            //{
            //    indices[i] = i;
            //}
            //cmdStr += "],k=[" + ToCMDSParamStr(indices, ptCount) + "])";
            //Debug.Log(cmdStr);
            //string resultName = MGlobal.executePythonCommandStringResult(cmdStr);
            //return GetDagPathByName(resultName);
        }

        public static MDagPath CreateCurve(MPointArray points, string curveName, int degree = 1, MFnNurbsCurve.Form form = MFnNurbsCurve.Form.kOpen)
        {
            MDoubleArray indices = new MDoubleArray();
            for (int i = 0; i < points.Count; i++)
            {
                indices.Add(i);
            }

            MFnNurbsCurve nc = new MFnNurbsCurve();
            MObject curveObject = nc.create(points, indices, (uint)degree, form, false, false);
            MDagPath curveDagPath = MDagPath.getAPathTo(curveObject);
            MFnDependencyNode dn = new MFnDependencyNode(curveObject);
            dn.setName(curveName);
            return curveDagPath;
        }

        

        public static MFnDependencyNode CreateRemapValueNode(float inputMin, float inputMax, float outputMin, float outputMax)
        {
            MFnDependencyNode dependencyNode = new MFnDependencyNode();
            dependencyNode.create("remapValue");
            //Debug.Log("created node:" + (*dependencyNode).absoluteName());
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

        public static MDagPath Duplicate(MDagPath targetDag)
        {
            string resultName = MGlobal.executePythonCommandStringResult(string.Format("cmds.duplicate(\"{0}\",rr = 1)", targetDag.fullPathName), true);
            Debug.Log("duplicate result:" + resultName);
            resultName = SubUShell(resultName);
            return GetDagPathByName(resultName);
        }

        public static MSelectionList DuplicateDags(MSelectionList list)
        {
            if (list == null)
            {
                return null;
            }
            MSelectionList originSelect = GetSelectedList();

            CmdStrConstructor cc = new CmdStrConstructor("duplicate");
            cc.UpdateToggle("rr", true);
            List<string> targets = new List<string>();
            foreach (MDagPath dag in list.DagPaths())
            {
                targets.Add(dag.fullPathName);
            }
            cc.UpdateTargets(targets);
            MGlobal.executeCommand(cc.ToString());

            //string resultName = MGlobal.executeCommandStringResult("duplicate -rr");
            MSelectionList newList = GetSelectedList();
            Select(originSelect);
            return newList;
        }

        #endregion
        
        #region Delete
        public static void DeleteByCMD(string name)
        {
            MGlobal.executeCommand("delete " + name);

        }

        public static void DoDelete(MSelectionList target = null,bool recoverOriginSelection = false)
        {
            if (target != null)
            {
                if (recoverOriginSelection)
                {
                    MSelectionList originSelected = GetSelectedList();
                    Select(target);
                    MGlobal.executeCommand("doDelete");
                    Select(originSelected);
                }
                else
                {
                    Select(target);
                    MGlobal.executeCommand("doDelete");
                }
            }
            else
            {
                MGlobal.executeCommand("doDelete");
            }
        }

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

        public static bool DeleteObjects(List<MFnDependencyNode> list)
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
                dGModifier.deleteNode(list[i].objectProperty);
            }
            dGModifier.doIt();
            return true;
        }


        #endregion

        #region Select

        public static void Select(MSelectionList list)
        {

            MGlobal.setActiveSelectionList(list);

            //bool hasDag = false;
            
            //if (hasDag)
            //{
            //    Debug.Log("has dag length:" + list.length);
            //    foreach (MDagPath dag in list.DagPaths())
            //    {
            //        Debug.Log(dag.fullPathName);
            //    }
            //    MGlobal.setActiveSelectionList(list);
            //}
            //else
            //{
            //    Debug.Log("no dag but length:" + list.length);
            //    MGlobal.setActiveSelectionList(list);
            //}
        }

        public static void Select(MDagPath dagPath)
        {
            MSelectionList list = new MSelectionList();
            list.add(dagPath);
            Select(list);
        }

        public static MSelectionList InvertSelect(MSelectionList list,MDagPath targetDag,ConstantValue.PolySelectType pst, bool recoverOriginSelection = false)
        {
            if (list == null)
            {
                return null;
            }
            MSelectionList originSelection = GetSelectedList();
            if (targetDag != null)
            {
                Select(targetDag);
            }
            Select(list);
            MGlobal.executeCommand(string.Format("doMenuComponentSelectionExt(\"{0}\", \"{1}\", 0)", targetDag.fullPathName, ConstantValue.ComponentSelectionExt(pst)));
            MGlobal.executeCommand("InvertSelection");
            MSelectionList result = GetSelectedList();
            if (recoverOriginSelection)
            {
                Select(originSelection);
            }
            return result;
        }

        #endregion

        #region Modify

        public static void ConnectAttr(string from, string to, bool force = true,bool showInIdle = false)
        {
            string cmdStr = string.Format("connectAttr {0} {1}", from, to);
            if (force)
            {
                cmdStr += " -f";
            }
            MGlobal.executeCommand(cmdStr, showInIdle);
        }

        public static bool ConnectPlug(MFnDependencyNode dn_from, string plugName_from, MFnDependencyNode dn_to, string plugName_to, MDGModifier dGModifier = null, bool doit = true)
        {
            if (dn_from != null && dn_to != null)
            {
                MPlug from = dn_from.findPlug(plugName_from);
                if (from != null)
                {
                    MPlug to = dn_to.findPlug(plugName_to);
                    if (to != null)
                    {
                        return ConnectPlug(from, to, dGModifier, doit);
                    }
                }
            }

            return false;
        }

        public static bool ConnectPlug(MPlug from, MPlug to,MDGModifier dGModifier = null,bool doit = true)
        {
            if (from != null && to != null)
            {
                if (dGModifier == null)
                {
                    dGModifier = new MDGModifier();
                    doit = true;
                }
                dGModifier.connect(from, to);
                if (doit)
                {
                    dGModifier.doIt();
                }
                return true;
            }

            return false;
        }
        
        public static void AddConstraint(string from, string to, ConstantValue.ConstraintType ct, bool maintainOffset = true)
        {
            string cmdStr = string.Format(maintainOffset ? "{0} -mo {1} {2}" : "{0} {1} {2}", ConstantValue.Command_Constraint(ct), from, to);
            MGlobal.executeCommandStringResult(cmdStr);
        }

        public static void Rename(MDagPath dag, string newName)
        {
            new MFnDependencyNode(dag.node).setName(newName);
        }

        public static void RenameDagList(MDagPath[] dagList, string formatStr)
        {
            MDagModifier dagModifier = new MDagModifier();
            for (int i = 0; i < dagList.Length; i++)
            {
                dagModifier.renameNode(dagList[i].node, string.Format(formatStr, i));
            }
            dagModifier.doIt();
        }

        public static void RenameDgList(MObject[] dgList, string formatStr)
        {
            MDGModifier dgModifier = new MDGModifier();
            for (int i = 0; i < dgList.Length; i++)
            {
                dgModifier.renameNode(dgList[i], string.Format(formatStr, i));
            }
        }

        public static void SetAttr(string attrFullName, string value)
        {
            MGlobal.executeCommand(string.Format("setAttr \"{0}\" {1}", attrFullName, value));
        }
        public static void SetAttr(string targetName, string attrName, string value)
        {
            MGlobal.executeCommand(string.Format("setAttr \"{0}.{1}\" {2}", targetName, attrName, value));
        }

        public static void IterateSelectedDags(Action<MDagPath> dealMethod, MFn.Type typeFilter = MFn.Type.kInvalid,MSelectionList list = null)
        {
            if (list == null)
            {
                list = GetSelectedList();
            }
            foreach (MDagPath dag in list.DagPaths())
            {
                if (typeFilter != MFn.Type.kInvalid)
                {
                    if (!dag.hasFn(typeFilter))
                    {
                        continue;
                    }
                }
                dealMethod(dag);
            }
        }

        public static void IterateSelectedObjects(Action<MObject> dealMethod, MFn.Type typeFilter = MFn.Type.kInvalid, MSelectionList list = null)
        {
            if (list == null)
            {
                list = GetSelectedList();
            }
            int count = (int)list.length;
            for (int i = 0; i < count; i++)
            {
                MObject mo = new MObject();
                list.getDependNode((uint)i, mo);
                if (typeFilter != MFn.Type.kInvalid)
                {
                    if (!mo.hasFn(typeFilter))
                    {
                        continue;
                    }
                }
                dealMethod(mo);
            }

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
                Debug.Log(result[i]);
            }
            return result;
        }
        #endregion

        #region Compare
        public static bool IsSame(List<string> strList0, List<string> strList1)
        {
            int count = strList0.Count;

            Dictionary<string, int> freqDic = new Dictionary<string, int>();
            if (count != strList1.Count)
            {
                return false;
            }

            for (int i = 0; i < count; i++)
            {
                int freq;
                if (freqDic.TryGetValue(strList0[i], out freq))
                {
                    freqDic[strList0[i]] = freq + 1;
                }
                else
                {
                    freqDic.Add(strList0[i], 1);
                }

                if (freqDic.TryGetValue(strList1[i], out freq))
                {
                    freqDic[strList1[i]] = freq - 1;
                }
                else
                {
                    freqDic.Add(strList1[i], -1);
                }
            }

            foreach (int value in freqDic.Values)
            {
                if (value != 0)
                {
                    return false;
                }
            }

            return true;
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

        public static float CalPosRadian(MVector pos)
        {
            MVector normalizedPos = pos.normal;

            float radian = (float)Math.Acos(normalizedPos.z);
            if (normalizedPos.x < 0)
            {
                radian = -radian;
            }
            return radian;
        }

        public static MVector Lerp(MVector v0, MVector v1,float percent)
        {
            return v0 * (1 - percent) + v1 * percent;
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
                Debug.Log(index + ":" + mdp.fullPathName);
            }
        }

        public static void PrintObjects(MSelectionList list)
        {
            for (int i = 0; i < list.length; i++)
            {
                uint index = (uint)i;
                MObject mo = new MObject();
                list.getDependNode(index, mo);
                Debug.Log(index + ":typeof:" + mo.apiTypeStr);

            }


        }

        public static void PrintPlugs(MPlugArray plugs)
        {
            for (int i = 0; i < plugs.length; i++)
            {
                Debug.Log(i + ":" + plugs[i].name);
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
            cmdList.Add(new CommandData(null, cmdStr, "testWindow", "test window", () =>
            {
                BasicTools.UI.TestWPFWindow window = new UI.TestWPFWindow();
                window.Show();
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
            cmdList.Add(new CommandData("基本操作", cmdStr, "cthulhu", "创建偏方三八面体", () =>
            {
                BasicFunc.CreateCTL_Crystal("ctl_sample");
            }));
            return cmdList;
        }
    }
}
