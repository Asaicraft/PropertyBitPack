using PropertyBitPack.SourceGen.Models.ParamRequests;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace PropertyBitPack.SourceGen.Models.ConstructorRequests;
internal interface IConstructorRequest
{
    public AccessModifier ConstructorAccessModifier
    {
        get;
    }

    public ImmutableArray<IParamRequest> ParamRequests
    {
        get;
    }
}
