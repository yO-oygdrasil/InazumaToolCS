using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Maya.OpenMaya;

namespace InazumaTool.BasicTools.DependModules
{
    public abstract class BaseDependModule
    {
        public MFnDependencyNode dependNode;
        public abstract MPlug GetPlugByName(string plugName);
        public abstract MPlug GetPlugByType(int plugType);

    }

    public class NClothShapeModule: BaseDependModule
    {
        public enum PlugType
        {
            IsDynamic = 0,
            CurrentTime = 1
        }


        public NClothShapeModule(MFnDependencyNode node)
        {
            dependNode = node;
        }

        public override MPlug GetPlugByType(int plugType)
        {
            switch((PlugType)plugType)
            {
                case PlugType.CurrentTime:
                    {
                        return dependNode.findPlug(ConstantValue.plugName_nCloth_currentTime);
                    }
                case PlugType.IsDynamic:
                    {
                        return dependNode.findPlug(ConstantValue.plugName_nCloth_isDynamic);
                    }
                default:
                    {
                        return null;
                    }
            }
        }

        public override MPlug GetPlugByName(string plugName)
        {
            throw new NotImplementedException();
        }
    }

}
