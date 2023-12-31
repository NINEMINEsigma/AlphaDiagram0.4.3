﻿using System;
using AD.BASE;

namespace AD.Experimental.Neuron
{
    public class App : ADArchitecture<App>
    {
        public override bool FromMap(IBaseMap from)
        {
            throw new NotImplementedException();
        }

        public override IBaseMap ToMap()
        {
            throw new NotImplementedException();
        }

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
                throw new DestoryException();
            }
        }

        internal ulong InstanceTotalGenerated = 0;
    }
}
