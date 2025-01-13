using CommunityToolkit.Diagnostics;
using Microsoft.CodeAnalysis;
using PropertyBitPack.SourceGen.Collections;
using PropertyBitPack.SourceGen.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Text;

namespace PropertyBitPack.SourceGen.BitFieldPropertyAggregators;
internal abstract class BaseBitFieldPropertyAggregator : IBitFieldPropertyAggregator, IContextBindable
{
    public static readonly ImmutableArray<SpecialType> AvailableTypes =
    [
        SpecialType.System_Byte,
        SpecialType.System_UInt16,
        SpecialType.System_UInt32,
        SpecialType.System_UInt64
    ];

    private PropertyBitPackGeneratorContext? _context;

    public PropertyBitPackGeneratorContext Context
    {
        get
        {
            Debug.Assert(_context is not null);

            return _context!;
        }
    }

    public void BindContext(PropertyBitPackGeneratorContext context)
    {
        _context = context;
    }

    public ImmutableArray<GenerateSourceRequest> Aggregate(ILinkedList<BaseBitFieldPropertyInfo> properties, in ImmutableArrayBuilder<Diagnostic> diagnostics)
    {
        if (properties.Count == 0)
        {
            return [];
        }

        // We'll produce one or more GenerateSourceRequest objects.
        using var requestsBuilder = ImmutableArrayBuilder<GenerateSourceRequest>.Rent();

        AggregateCore(properties, requestsBuilder, diagnostics);

        return requestsBuilder.ToImmutable();
    }

    protected abstract void AggregateCore(ILinkedList<BaseBitFieldPropertyInfo> properties, in ImmutableArrayBuilder<GenerateSourceRequest> requestsBuilder, ImmutableArrayBuilder<Diagnostic> diagnostics);



    /// <summary>
    /// Groups input properties by (Owner, IFieldName?) without using LINQ,
    /// utilizing DictionariesPool and ListsPool instead of <c>new Dictionary</c> and <c>new List</c>.
    /// </summary>
    protected static ImmutableArray<OwnerFieldNameGroup> GroupPropertiesByFieldNameAndOwner(
        ImmutableArray<BaseBitFieldPropertyInfo> properties)
    {
        // Use a builder to construct the final array of groups.
        using var namedGroupsBuilder = ImmutableArrayBuilder<OwnerFieldNameGroup>.Rent();

        // Rent a dictionary from the pool for grouping.
        // Key: (INamedTypeSymbol Owner, IFieldName? fieldName)
        // Value: a list of properties belonging to this group.
        var groupedPropertiesDictionary = DictionariesPool<(INamedTypeSymbol, IFieldName?), List<BaseBitFieldPropertyInfo>>.Rent();

        try
        {
            // Iterate through all properties
            for (var i = 0; i < properties.Length; i++)
            {
                var property = properties[i];
                var owner = property.Owner;
                var fieldName = property.AttributeParsedResult.FieldName; // may be null

                // Key for the group
                var key = (owner, fieldName);

                // Try to get the existing list
                if (!groupedPropertiesDictionary.TryGetValue(key, out var propertyList))
                {
                    // If the list is not found, rent a new one from the pool
                    propertyList = ListsPool<BaseBitFieldPropertyInfo>.Rent();
                    groupedPropertiesDictionary[key] = propertyList;
                }

                propertyList.Add(property);
            }

            // Transform each (Key, List) pair into OwnerFieldNameGroup
            foreach (var kvp in groupedPropertiesDictionary)
            {
                var (owner, fieldName) = kvp.Key;
                var propertyList = kvp.Value;

                // Create a builder for the specific number of properties
                using var propertiesBuilder = ImmutableArrayBuilder<BaseBitFieldPropertyInfo>.Rent(propertyList.Count);

                // Add all properties from the list to the builder
                propertiesBuilder.AddRange(propertyList);

                // Create an ImmutableArray<BaseBitFieldPropertyInfo>
                var propsImmutable = propertiesBuilder.ToImmutable();

                // Add the new group to the overall builder
                namedGroupsBuilder.Add(new OwnerFieldNameGroup(owner, fieldName, propsImmutable));
            }
        }
        finally
        {
            // Return all lists to the pool, then return the dictionary itself
            foreach (var kvp in groupedPropertiesDictionary)
            {
                var list = kvp.Value;
                ListsPool<BaseBitFieldPropertyInfo>.Return(list);
            }

            // Clear and return the dictionary to the pool
            DictionariesPool<(INamedTypeSymbol, IFieldName?), List<BaseBitFieldPropertyInfo>>.Return(groupedPropertiesDictionary);
        }

        // Return the result
        return namedGroupsBuilder.ToImmutable();
    }

    /// <summary>
    /// Calculates the distribution of requested bits into fields of appropriate sizes.
    /// </summary>
    /// <param name="requestedBits">An immutable array of byte values representing requested bit sizes.</param>
    /// <returns>An immutable array of <see cref="CalculatedBits"/> objects.</returns>
    protected static ImmutableArray<CalculatedBits> DistributeBitsIntoFields(ImmutableArray<byte> requestedBits)
    {
        byte totalBits = 0;
        var calculatedBits = ImmutableArrayBuilder<CalculatedBits>.Rent();
        var list = ListsPool<byte>.Rent();

        try
        {
            for (var i = 0; i < requestedBits.Length; i++)
            {
                var bits = requestedBits[i];
                list.Add(bits);

                if (totalBits + bits > 64)
                {
                    var fieldCapacity = DetermineFieldBitSize(totalBits);

                    if (fieldCapacity == BitSize.Invalid)
                    {
                        ThrowHelper.ThrowUnreachableException("Invalid bit size.");
                    }

                    var calculated = new CalculatedBits(fieldCapacity, [.. list]);

                    calculatedBits.Add(calculated);

                    totalBits = 0;
                    list.Clear();
                }
            }

            if (totalBits > 0)
            {
                var fieldCapacity = DetermineFieldBitSize(totalBits);

                if (fieldCapacity == BitSize.Invalid)
                {
                    ThrowHelper.ThrowUnreachableException("Invalid bit size.");
                }

                var calculated = new CalculatedBits(fieldCapacity, [.. list]);
                calculatedBits.Add(calculated);
            }
        }
        finally
        {
            ListsPool<byte>.Return(list);
        }

        return calculatedBits.ToImmutable();
    }


    protected static bool ValidateSize(BitSize bitSize, ImmutableArray<BaseBitFieldPropertyInfo> requestedBits)
    {
        var bits = requestedBits.Sum(x => GetEffectiveBitsCount(x));

        return (byte)bitSize > bits;
    }

    /// <summary>
    /// Determines the appropriate field size in bits for a given bit count.
    /// </summary>
    /// <param name="bits">The number of bits to evaluate.</param>
    /// <returns>A <see cref="BitSize"/> value representing the appropriate field size.</returns>
    protected static BitSize DetermineFieldBitSize(byte bits)
    {
        if (bits <= 8)
        {
            return BitSize.Byte;
        }

        if (bits <= 16)
        {
            return BitSize.UInt16;
        }

        if (bits <= 32)
        {
            return BitSize.UInt32;
        }

        if (bits <= 64)
        {
            return BitSize.UInt64;
        }

        return BitSize.Invalid;
    }

    protected readonly struct CalculatedBits(BitSize fieldCapacity, ImmutableArray<byte> bits)
    {
        public BitSize FieldCapacity { get; } = fieldCapacity;
        public ImmutableArray<byte> Bits { get; } = bits;
    }

    /// <summary>
    /// A custom comparer to group named properties by (Owner, FieldName).
    /// </summary>
    protected sealed class OwnerFieldNameComparer : IEqualityComparer<(INamedTypeSymbol Owner, string Name)>
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


    protected sealed class OwnerFieldNameGroup(INamedTypeSymbol owner, IFieldName? fieldName, ImmutableArray<BaseBitFieldPropertyInfo> properties)
    {
        public INamedTypeSymbol Owner { get; } = owner;
        public IFieldName? FieldName { get; } = fieldName;
        public ImmutableArray<BaseBitFieldPropertyInfo> Properties { get; } = properties;
    }

    /// <summary>
    /// Example utility to convert an existing symbol into a known <see cref="SpecialType"/>.
    /// If the symbol's type isn't recognized, returns <see cref="SpecialType.None"/>.
    /// In a real scenario, you'd probably do a more detailed check with Roslyn APIs.
    /// </summary>
    protected static SpecialType MapSymbolToSpecialType(ISymbol? symbol)
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

    protected static BitSize MapSpecialTypeToBitSize(SpecialType specialType)
    {
        return specialType switch
        {
            SpecialType.System_Boolean => BitSize.Bool,
            SpecialType.System_Byte => BitSize.Byte,
            SpecialType.System_Int16 or SpecialType.System_UInt16 => BitSize.UInt16,
            SpecialType.System_Int32 or SpecialType.System_UInt32 => BitSize.UInt32,
            SpecialType.System_Int64 or SpecialType.System_UInt64 => BitSize.UInt64,
            _ => BitSize.Invalid
        };
    }

    protected static byte GetEffectiveBitsCount(BaseBitFieldPropertyInfo baseBitFieldPropertyInfo)
    {
        return BitCountHelper.GetEffectiveBitsCount(baseBitFieldPropertyInfo);
    }
}
