using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;

namespace PropertyBitPack.SourceGen.Models.FieldRequests;
internal class NonExistingFieldRequest(string fieldName, SpecialType specialType) : FieldRequest(fieldName, specialType, false)
{
}
