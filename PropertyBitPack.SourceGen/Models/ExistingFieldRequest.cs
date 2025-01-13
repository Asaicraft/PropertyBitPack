using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace PropertyBitPack.SourceGen.Models;
internal sealed class ExistingFieldRequest(IFieldSymbol fieldSymbol) : FieldRequest(fieldSymbol.Name, fieldSymbol.Type.SpecialType, true)
{
}
