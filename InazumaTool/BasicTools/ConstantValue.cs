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

        public const string plugName_fileTexPath = "fileTextureName";
        public const string plugName_fileTexOutput = "outColor";
        public const string plugName_matColorInput = "color";

        public const string plugName_remapValueInput = "inputValue";
        public const string plugName_remapValueOutput = "outValue";


        public const string plugName_blendShapeWeight = "weight";

        public const string plugName_tx = "translateX";
        public const string plugName_ty = "translateY";
        public const string plugName_tz = "translateZ";
        public const string plugName_rx = "rotateX";
        public const string plugName_ry = "rotateY";
        public const string plugName_rz = "rotateZ";
        public const string plugName_sx = "scaleX";
        public const string plugName_sy = "scaleY";
        public const string plugName_sz = "scaleZ";

        public const string plugName_dynamicConstraintMethod = "constraintMethod";

        //public const string cmd_pointConstraint = "pointConstraint";
        //public const string cmd_poleVectorConstraint = "poleVectorConstraint";
        //public const string cmd_parentConstraint = "parentConstraint";
        //public const string cmd_orientConstraint = "orientConstraint";

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
        public static readonly string[] dynamicConstraintTypeStr = {"point", "pointToPoint" };
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

    }
}
