using Microsoft.CodeAnalysis;
using PropertyBitPack.SourceGen.Collections;
using PropertyBitPack.SourceGen.Models;
using PropertyBitPack.SourceGen.Models.AttributeParsedResults;
using PropertyBitPack.SourceGen.Models.GenerateSourceRequests;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace PropertyBitPack.SourceGen.BitFieldPropertyAggregators;

/// <summary>
/// Aggregates read-only properties annotated with <see cref="ParsedReadOnlyBitFieldAttribute"/>.
/// </summary>
/// <remarks>
/// This class inherits from <see cref="BaseBitFieldPropertyAggregator"/> and provides functionality 
/// to filter and aggregate properties marked as read-only for further processing.
/// </remarks>
internal sealed class ReadOnlyAggregator : BaseUnnamedFieldAggregator
{

}
