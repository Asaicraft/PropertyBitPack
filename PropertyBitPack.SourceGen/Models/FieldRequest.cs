using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace PropertyBitPack.SourceGen.Models;

[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
internal class FieldRequest(string name, SpecialType fieldType, bool isExist)
{
    private readonly string _name = name;
    private readonly SpecialType _fieldType = fieldType;
    private readonly bool _isExist = isExist;

    public string Name => _name;
    public SpecialType FieldType => _fieldType;
    public bool IsExist => _isExist;

    private string GetDebuggerDisplay()
    {
        return $"IsExist:{_isExist} {_name} ({_fieldType})";
    }
}
