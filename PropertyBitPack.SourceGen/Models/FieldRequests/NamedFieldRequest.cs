using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace PropertyBitPack.SourceGen.Models.FieldRequests;
internal sealed class NamedFieldRequest(string name, SpecialType fieldType) : NonExistingFieldRequest(name, fieldType)
{
}
