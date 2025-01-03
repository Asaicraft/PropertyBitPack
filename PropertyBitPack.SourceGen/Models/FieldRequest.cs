using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace PropertyBitPack.SourceGen.Models;
internal class FieldRequest(string name, SpecialType fieldType)
{
    private readonly string _name = name;
    private readonly SpecialType _fieldType = fieldType;

    public string Name => _name;
    public SpecialType FieldType => _fieldType;
}
