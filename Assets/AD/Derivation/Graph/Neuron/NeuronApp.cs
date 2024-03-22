using System;
using AD.BASE;

namespace AD.Experimental.Neuron
{
    public class App : ADArchitecture<App>
    {

        public override void Init()
        {

        }

        public static void Assert(VectorAsset asset)
        {
            if(asset == null)
            {
                throw new ArgumentNullException();
            }
            else if(asset.IsDestory)
            {
                throw new DestroyException();
            }
        }

        internal ulong InstanceTotalGenerated = 0;
    }
}
