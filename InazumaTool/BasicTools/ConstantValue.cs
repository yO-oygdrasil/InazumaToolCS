using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InazumaTool.BasicTools
{
    public class ConstantValue
    {
        public const float PI = 3.1415926f;
        public const float DPR = 57.29578049f;
        

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


        //public const string cmd_pointConstraint = "pointConstraint";
        //public const string cmd_poleVectorConstraint = "poleVectorConstraint";
        //public const string cmd_parentConstraint = "parentConstraint";
        //public const string cmd_orientConstraint = "orientConstraint";

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
