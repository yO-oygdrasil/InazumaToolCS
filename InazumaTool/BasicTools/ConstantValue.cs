using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InazumaTool.BasicTools
{
    public class ConstantValue
    {
        public const float PI = 3.14159265f;
        public const float DPR = 57.29578049f;
        public const float Radian15 = 0.2617993875f;
        public const float Radian30 = 0.523598775f;
        public const float Radian45 = 0.7853981625f;
        public const float Radian60 = 1.04719755f;
        public const float Radian75 = 1.3089969375f;
        public const float Radian90 = 1.570796325f;

        public const string
            nodeName_place2dTexture = "place2dTexture",
            nodeName_fileTex = "file";

        public const string plugName_fileTexUVTilingMode = "uvTilingMode";
        public const string plugName_fileTexPath = "fileTextureName";
        public const string plugName_fileTexOutputColor = "outColor";
        public const string plugName_fileTexOutputTransparency = "outTransparency";
        public const string plugName_fileTexDefaultColorR = "defaultColorR";
        public const string plugName_fileTexDefaultColorG = "defaultColorG";
        public const string plugName_fileTexDefaultColorB = "defaultColorB";
        public const string plugName_matColorInput = "color";
        public const string plugName_matColorOutput = "outColor";
        public const string plugName_matTransparency = "transparency";

        public const string plugName_texFileUVCoord = "uvCoord";
        public const string plugName_place2dOutUV = "outUV";
        public const string plugName_dagSetMembers = "dagSetMembers";

        public const string
            command_ConvertSelectionToFaces = "ConvertSelectionToFaces",
            command_ConvertSelectionToEdgePerimeter = "ConvertSelectionToEdgePerimeter";

        public const string
            command_CutUVs = "CutUVs";

        public const string plugName_remapValueInput = "inputValue";
        public const string plugName_remapValueOutput = "outValue";


        public const string plugName_blendShapeWeight = "weight";
        public const string plugName_blendShapeInputTarget = "inputTarget";
        public const string plugName_blendShapeInputTargetGroup = "inputTargetGroup";

        public const string
            plugName_tx = "translateX",
            plugName_ty = "translateY",
            plugName_tz = "translateZ",
            plugName_rx = "rotateX",
            plugName_ry = "rotateY",
            plugName_rz = "rotateZ",
            plugName_sx = "scaleX",
            plugName_sy = "scaleY",
            plugName_sz = "scaleZ";

        public const string plugName_dynamicConstraintMethod = "constraintMethod";

        public const string plugName_nCloth_currentTime = "currentTime";
        public const string plugName_nCloth_isDynamic = "isDynamic";
        public const string plugName_nClothTime_nodeState = "nodeState";

        //public const string plugName_shapeInstanceObjGroup
        //public const string plugName_shadingGroup = "dagSetMembers";

        //public const string cmd_pointConstraint = "pointConstraint";
        //public const string cmd_poleVectorConstraint = "poleVectorConstraint";
        //public const string cmd_parentConstraint = "parentConstraint";
        //public const string cmd_orientConstraint = "orientConstraint";

        public enum TimeNodeState
        {
            Normal = 0,
            NoEffect = 1,
            Stuck = 2
        }

        public enum DynamicConstraintMethod
        {
            Weld = 0,
            Spring = 1,
            Elastic = 2
        }


        public enum SampleType
        {
            Vert,
            Edge,
            Poly,
            ObjectTrans
        }

        //0-none, 1-base, 2-end,3-both
        public enum HairPointLockType
        {
            None = 0,
            Base = 1,
            End = 2,
            Both = 3
        }

        public const string command_DynamicConstraint = "createNConstraint";
        public static readonly string[] dynamicConstraintTypeStr = { "point", "pointToPoint" };
        public enum DynamicConstraintType
        {
            Point = 0,
            PointToPoint = 1
        }
        public static string Param_DynamicConstraintType(DynamicConstraintType type)
        {
            return dynamicConstraintTypeStr[(int)type];
        }

        public enum ConstraintType
        {
            Point,
            PoleVector,
            Parent,
            Orient
        }
        private static readonly string[] cmd_constraint = { "pointConstraint", "poleVectorConstraint", "parentConstraint", "orientConstraint" };
        public static string Command_Constraint(ConstraintType constraintType)
        {
            return cmd_constraint[(int)constraintType];

        }

        public enum PolySelectType
        {
            Vertex = 0,
            Edge = 1,
            Facet = 2
        }
        private static readonly string[] componentSelectionExtType = { "vertex", "edge", "facet" };
        public static string ComponentSelectionExt(PolySelectType pst)
        {
            return componentSelectionExtType[(int)pst];
        }

        public const string plugName_layeredTextureInputs = "inputs";
        public const string plugName_layeredTextureBlendMode = "blendMode";
        public enum LayeredTextureBlendMode
        {
            None = 0,
            Override = 1,
            In = 2,
            Out = 3,
            Add = 4,
            Sub = 5,
            Multiply = 6,
            Differ = 7,
            Greater = 8,
            Lower = 9
        }
        public enum LayeredTextureInputDataIndex
        {
            Color = 0,
            Alpha = 1,
            BlendMode = 2,
            Visible = 3
        }

        public enum UVTilingMode
        {
            None = 0,
            ZBrush = 1,
            MudBox = 2,
            UDIM = 3,
            Obvious = 4
        }



        #region RedShift Nodes
        public const string nodeName_RS_Architectural = "RedshiftArchitectural";

        public const string plugName_RS_diffuse = "diffuse",
            plugName_RS_transColor = "refr_color",
            plugName_RS_transWeight = "transparency",
            plugName_RS_outColor = "outColor";



        #endregion
    }
}
