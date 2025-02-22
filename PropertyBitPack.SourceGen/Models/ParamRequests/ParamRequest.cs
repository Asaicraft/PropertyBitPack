using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace PropertyBitPack.SourceGen.Models.ParamRequests;
internal sealed class ParamRequest(SpecialType specialType, string name) : IParamRequest
{
    public SpecialType SpecialType
    {
        get;
    } = specialType;

    public string Name
    {
        get;
    } = name;
}
