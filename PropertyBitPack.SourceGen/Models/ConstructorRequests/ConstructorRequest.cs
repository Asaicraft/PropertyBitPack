using PropertyBitPack.SourceGen.Models.ParamRequests;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace PropertyBitPack.SourceGen.Models.ConstructorRequests;

internal sealed class ConstructorRequest(AccessModifier constructorAccessModifier, ImmutableArray<IParamRequest> paramRequests) : IConstructorRequest
{
    public AccessModifier ConstructorAccessModifier
    {
        get;
    } = constructorAccessModifier;

    public ImmutableArray<IParamRequest> ParamRequests
    {
        get;
    } = paramRequests;
}
