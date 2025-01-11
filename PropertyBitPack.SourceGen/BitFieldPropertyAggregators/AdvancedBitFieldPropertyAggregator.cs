using Microsoft.CodeAnalysis;
using PropertyBitPack.SourceGen.Collections;
using PropertyBitPack.SourceGen.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace PropertyBitPack.SourceGen.BitFieldPropertyAggregators;

/// <summary>
/// An aggregator that packs bit field properties into as few fields as possible, 
/// respecting existing fields (if any) and user-defined or inferred bit sizes.
/// </summary>
internal sealed class AdvancedBitFieldPropertyAggregator : BaseBitFieldPropertyAggregator
{
    // Available "new" field types (if we need to create fields from scratch).
    private static readonly SpecialType[] availableTypes =
    {
        SpecialType.System_Byte,
        SpecialType.System_UInt16,
        SpecialType.System_UInt32,
        SpecialType.System_UInt64
    };

    public override ImmutableArray<GenerateSourceRequest> Aggregate(
        ILinkedList<BaseBitFieldPropertyInfo> properties,
        in ImmutableArrayBuilder<Diagnostic> diagnostics)
    {
        // We'll produce one or more GenerateSourceRequest objects.
        using var requestsBuilder = ImmutableArrayBuilder<GenerateSourceRequest>.Rent();

        if (properties.Count == 0)
        {
            return requestsBuilder.ToImmutable();
        }

        var namedFieldProperties = new List<BaseBitFieldPropertyInfo>();
        var unnamedFieldProperties = new List<BaseBitFieldPropertyInfo>();

        foreach (var property in properties)
        {
            if (!string.IsNullOrWhiteSpace(property.AttributeParsedResult.FieldName?.Name))
            {
                namedFieldProperties.Add(property);
            }
            else
            {
                unnamedFieldProperties.Add(property);
            }
        }

        var namedGroups = namedFieldProperties
            .GroupBy(
                keySelector: p => (p.Owner, Name: p.AttributeParsedResult.FieldName!.Name!),
                comparer: OwnerFieldNameComparer.Instance) // This is a custom IEqualityComparer, or you can do .ToLookup instead
            .ToList();

        foreach (var group in namedGroups)
        {
            var owner = group.Key.Owner;
            var fieldName = group.Key.Name;
            var propertiesInGroup = group.ToImmutableArray();

            var totalBits = 0;
            var hasInvalid = false;

            for (var i = 0; i < propertiesInGroup.Length; i++)
            {
                var property = propertiesInGroup[i];

                var bits = BitCountHelper.GetEffectiveBitsCount(property);

                if(bits < 0)
                {
                    var location = property.AttributeParsedResult.BitsCountArgument()?.GetLocation();

                    diagnostics.Add(Diagnostic.Create(
                        PropertyBitPackDiagnostics.InvalidBitsCount,
                        location,
                        property.PropertySymbol.Name));


                    hasInvalid = true;
                    break;
                }
                else
                {
                    totalBits += bits;
                }
            }

            if (hasInvalid)
            {
                continue;
            }
        }

        return requestsBuilder.ToImmutable();
    }

    /// <summary>
    /// Example utility to convert an existing symbol into a known <see cref="SpecialType"/>.
    /// If the symbol's type isn't recognized, returns <see cref="SpecialType.None"/>.
    /// In a real scenario, you'd probably do a more detailed check with Roslyn APIs.
    /// </summary>
    private static SpecialType MapSymbolToSpecialType(ISymbol? symbol, in ImmutableArrayBuilder<Diagnostic> diagnostics)
    {
        if (symbol is IFieldSymbol fs)
        {
            // Check fs.Type.SpecialType 
            return fs.Type.SpecialType;
        }
        // Could also handle IPropertySymbol, etc. if your design allows that.

        // If you can't map the symbol to a known type, return None or produce a diagnostic.
        return SpecialType.None;
    }

    /// <summary>
    /// A custom comparer to group named properties by (Owner, FieldName).
    /// </summary>
    private sealed class OwnerFieldNameComparer
        : IEqualityComparer<(INamedTypeSymbol Owner, string Name)>
    {

        public static readonly OwnerFieldNameComparer Instance = new();

        public bool Equals((INamedTypeSymbol Owner, string Name) x, (INamedTypeSymbol Owner, string Name) y)
        {
            return SymbolEqualityComparer.Default.Equals(x.Owner, y.Owner)
                   && StringComparer.Ordinal.Equals(x.Name, y.Name);
        }

        public int GetHashCode((INamedTypeSymbol Owner, string Name) obj)
        {
            return obj.GetHashCode();
        }
    }
}