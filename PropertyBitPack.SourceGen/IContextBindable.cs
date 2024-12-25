using System;
using System.Collections.Generic;
using System.Text;

namespace PropertyBitPack.SourceGen;
public interface IContextBindable
{
    public void BindContext(PropertyBitPackGeneratorContext context);
}
