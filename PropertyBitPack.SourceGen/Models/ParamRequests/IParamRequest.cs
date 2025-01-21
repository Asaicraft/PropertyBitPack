using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace PropertyBitPack.SourceGen.Models.ParamRequests;
internal interface IParamRequest
{
    public SpecialType SpecialType
    {
        get;
    }

    public string Name
    {
        get;
    }
}
