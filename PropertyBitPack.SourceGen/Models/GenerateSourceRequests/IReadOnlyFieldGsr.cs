using PropertyBitPack.SourceGen.Models.ConstructorRequests;
using PropertyBitPack.SourceGen.Models.ParamRequests;
using System;
using System.Collections.Generic;
using System.Text;

namespace PropertyBitPack.SourceGen.Models.GenerateSourceRequests;
internal interface IReadOnlyFieldGsr: IGenerateSourceRequest
{
    public IConstructorRequest ConstructorRequest
    {
        get;
    }
}
