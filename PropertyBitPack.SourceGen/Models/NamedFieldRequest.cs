using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace PropertyBitPack.SourceGen.Models;
internal sealed class NamedFieldRequest(string name, SpecialType fieldType) : FieldRequest(name, fieldType, false)
{
}
