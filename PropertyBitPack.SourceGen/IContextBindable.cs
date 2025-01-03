using System;
using System.Collections.Generic;
using System.Text;

namespace PropertyBitPack.SourceGen;
internal interface IContextBindable
{
    public void BindContext(PropertyBitPackGeneratorContext context);
}
