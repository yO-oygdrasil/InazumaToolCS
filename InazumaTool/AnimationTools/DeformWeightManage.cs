using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Maya.OpenMaya;
using System.IO;
using System.Xml;

namespace InazumaTool.AnimationTools
{
    class DeformWeightImport
    {
        public class JointPower
        {
            public string jointName;
            public string deformerName;
            public string shapeName;
            public int minIndex = int.MaxValue, maxIndex = -1;
            public Dictionary<int, float> weightDic = new Dictionary<int, float>();
            public int IndexCount
            {
                get
                {
                    return weightDic.Keys.Count;
                }
            }
            public JointPower()
            {

            }
            public void UpdateIndexWeight(int index, float weight)
            {
                if (weightDic.ContainsKey(index))
                {
                    weightDic[index] = weight;
                }
                else
                {
                    weightDic.Add(index, weight);
                    if (index < minIndex)
                    {
                        minIndex = index;
                    }
                    if (index > maxIndex)
                    {
                        maxIndex = index;
                    }
                }
            }
            public bool InfluenceIndex(int index)
            {
                return index < maxIndex && index > minIndex;
            }
            public float GetIndexWeight(int index)
            {
                if (weightDic.ContainsKey(index))
                {
                    return weightDic[index];
                }
                else
                {
                    return 0;
                }
            }
        }


        public class ShapePoints
        {
            List<MFloatVector> points;
            Dictionary<int, List<int>> areas = new Dictionary<int, List<int>>();

            public float xMin = float.MaxValue, yMin = float.MaxValue, zMin = float.MaxValue, xMax = float.MinValue, yMax = float.MinValue, zMax = float.MinValue;
            float xWidth, yWidth, zWidth;
            const int maxDivide = 8;
            int xAreaCount, yAreaCount, zAreaCount;
            float gridStep;
            private int GetAreaIndex()
            {
                return 0;
            }

            public ShapePoints(List<MFloatVector> pts)
            {
                points = new List<MFloatVector>(pts);
                for (int i = 0; i < points.Count; i++)
                {
                    if (points[i].x < xMin)
                    {
                        xMin = points[i].x;
                    }
                    else if (points[i].x > xMax)
                    {
                        xMax = points[i].x;
                    }

                    if (points[i].y < yMin)
                    {
                        yMin = points[i].y;
                    }
                    else if (points[i].y > yMax)
                    {
                        yMax = points[i].y;
                    }

                    if (points[i].z < zMin)
                    {
                        zMin = points[i].z;
                    }
                    else if (points[i].z > zMax)
                    {
                        zMax = points[i].z;
                    }
                }
                xWidth = xMax - xMin;
                yWidth = yMax - yMin;
                zWidth = zMax - zMin;

                gridStep = Math.Max(Math.Max(xWidth, yWidth), zWidth) / maxDivide * 1.05f;
                xAreaCount = (int)Math.Ceiling(xWidth / gridStep);
                yAreaCount = (int)Math.Ceiling(yWidth / gridStep);
                zAreaCount = (int)Math.Ceiling(zWidth / gridStep);

                for (int i = 0; i < points.Count; i++)
                {
                    int areaIndex = GetAreaIndex(points[i]);
                    if (areas.ContainsKey(areaIndex))
                    {
                        areas[areaIndex].Add(i);
                    }
                    else
                    {
                        List<int> newList = new List<int>() { i };
                        areas.Add(areaIndex, newList);
                    }
                }

            }

            int GetAreaIndex(MFloatVector pos)
            {
                int ai_x = (int)((pos.x - xMin) / xWidth / gridStep);
                int ai_y = (int)((pos.y - yMin) / yWidth / gridStep);
                int ai_z = (int)((pos.z - zMin) / zWidth / gridStep);
                int areaIndex = ai_x + ai_y * xAreaCount + ai_z * xAreaCount * yAreaCount;
                return areaIndex;
            }

            public int FindClosestIndex(MFloatVector pos, float tolerance = 0.001f)
            {
                int resultIndex = -1;
                int areaIndex = GetAreaIndex(pos);
                if (areas.ContainsKey(areaIndex))
                {
                    List<int> areaIndexList = areas[areaIndex];
                    float minDist = float.MaxValue;
                    for (int i = 0; i < areaIndexList.Count; i++)
                    {
                        MFloatVector p = points[areaIndexList[i]];
                        if (Math.Abs(p.y - pos.y) > tolerance || Math.Abs(p.x - pos.x) > tolerance || Math.Abs(p.z - pos.z) > tolerance)
                        {
                            //如果某一个轴已经超过tolerance，没救告辞
                            continue;
                        }
                        float dist = p.minus(pos).length;
                        if (minDist > dist)
                        {
                            resultIndex = i;
                            minDist = dist;
                        }
                    }



                }


                return resultIndex;
            }


        }






        public string filePath;
        public bool ReadWeightXML(string path)
        {
            if (path == null)
            {
                return false;
            }
            if (!File.Exists(path) || Path.GetExtension(path) != ".xml")
            {
                Debug.Log("extension:" + Path.GetExtension(path));
                Debug.Log("not a valid xml file path:" + path);
                return false;
            }
            XmlReader reader = XmlReader.Create(path);
            while (reader.Read())
            {
                //if (reader.NodeType == XmlNodeType.Element)
                //{
                //    Debug.Log("element!");
                //}
                switch (reader.Name)
                {
                    case "xml":
                        {
                            Debug.Log("xml version:" + reader["version"]);
                            break;
                        }
                    case "headerInfo":
                        {
                            Debug.Log("fileName:" + reader["fileName"] + " worldMatrix:" + reader["worldMatrix"]);
                            break;
                        }
                    case "shape":
                        {
                            Debug.Log("shape:" + reader["shape"]);
                            XmlReader subReader = reader.ReadSubtree();

                            while (subReader.Read())
                            {
                                if (subReader.Name == "point")
                                {
                                    points.Add(StringToMFloatVector(subReader["value"], ' '));
                                }
                                //Debug.Log("name:" + subReader.Name + ",value:" + subReader["value"]);
                                //points.Add(StringToMFloatVector(subReader["value"],' '));
                            }
                            break;
                        }

                    case "weights":
                        {
                            JointPower jp = new JointPower()
                            {
                                deformerName = reader["deformer"],
                                jointName = reader["source"],
                                shapeName = reader["shape"]
                            };

                            XmlReader subReader = reader.ReadSubtree();

                            while (subReader.Read())
                            {
                                if (subReader.Name == "point")
                                {
                                    points.Add(StringToMFloatVector(subReader["value"], ' '));
                                }
                                //Debug.Log("name:" + subReader.Name + ",value:" + subReader["value"]);
                                //points.Add(StringToMFloatVector(subReader["value"],' '));
                            }
                            break;
                        }
                }
            }


            return true;
        }

        #region StringToValue
        public static MFloatVector StringToMFloatVector(string str, char splitChar = ',')
        {
            if (str == null || str.Length == 0)
            {
                return MFloatVector.one;
            }
            float[] vFloats = new float[3];
            string[] values = str.Trim().Split(splitChar);
            //Debug.Log("length:" + values.Length);
            for (int i = 0; i < values.Length; i++)
            {
                //Debug.Log(values[i]);
                float f;
                if (float.TryParse(values[i], out f))
                {
                    vFloats[i] = f;
                }
            }
            return new MFloatVector(vFloats[0], vFloats[1], vFloats[2]);
        }

        public static float StringToFloat(string str)
        {
            if (str == null || str.Length == 0)
            {
                return 0;
            }
            float f;
            if (float.TryParse(str, out f))
            {
                return f;
            }
            else
            {
                return 0;
            }
        }

        #endregion




        private void Start()
        {
            ReadWeightXML(filePath);
            //foreach (MFloatVector v in points)
            //{
            //    Transform newTrans = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
            //    newTrans.parent = transform;
            //    newTrans.localPosition = v;
            //    newTrans.localScale = MFloatVector.one * 0.1f;

            //}


        }



    }
}
