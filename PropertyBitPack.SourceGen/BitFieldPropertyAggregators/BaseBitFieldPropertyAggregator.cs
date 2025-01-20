using CommunityToolkit.Diagnostics;
using Microsoft.CodeAnalysis;
using PropertyBitPack.SourceGen.Collections;
using PropertyBitPack.SourceGen.Models;
using PropertyBitPack.SourceGen.Models.GenerateSourceRequest;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Runtime.InteropServices;
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

        AggregateCore(properties, requestsBuilder, in diagnostics);

        return requestsBuilder.ToImmutable();
    }

    protected abstract void AggregateCore(ILinkedList<BaseBitFieldPropertyInfo> properties, in ImmutableArrayBuilder<GenerateSourceRequest> requestsBuilder, in ImmutableArrayBuilder<Diagnostic> diagnostics);

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
        var groupedPropertiesDictionary = OwnerFieldNameGroupDictionaryPool.Rent();

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
            OwnerFieldNameGroupDictionaryPool.Return(groupedPropertiesDictionary);
        }

        // Return the result
        return namedGroupsBuilder.ToImmutable();
    }

    /// <summary>
    /// Distributes bits for each property in <paramref name="propertyInfos"/> into fields, 
    /// calling <see cref="DistributeBitsIntoFields(ReadOnlySpan{byte})"/> internally.
    /// </summary>
    /// <param name="propertyInfos">
    /// A collection of <see cref="BaseBitFieldPropertyInfo"/> objects, each specifying its own required bit count.
    /// </param>
    /// <returns>
    /// An immutable array of <see cref="CalculatedBits"/> instances describing how these bits are distributed.
    /// </returns>
    /// <exception cref="UnreachableException">
    /// Thrown if an invalid bit count is encountered or any unexpected logic failure occurs.
    /// </exception>
    protected static ImmutableArray<CalculatedBits> DistributeBitsIntoFields(ImmutableArray<BaseBitFieldPropertyInfo> propertyInfos)
    {
        using var list = ListsPool.Rent<byte>();

        for (var i = 0; i < propertyInfos.Length; i++)
        {
            var property = propertyInfos[i];
            var bits = GetEffectiveBitsCount(property);
            list.Add(bits);
        }

        return DistributeBitsIntoFields(list.ToImmutable());
    }

    /// <summary>
    /// Calculates the distribution of requested bits into fields of appropriate sizes.
    /// </summary>
    /// <param name="requestedBits">An immutable array of byte values representing requested bit sizes.</param>
    /// <returns>An immutable array of <see cref="CalculatedBits"/> objects.</returns>
    protected static ImmutableArray<CalculatedBits> DistributeBitsIntoFields(ImmutableArray<byte> requestedBits)
    {
        return DistributeBitsIntoFields(requestedBits.AsSpan());
    }

    /// <summary>
    /// Distributes the requested bits into fields using the smallest possible type for each chunk.
    /// For example, if the input is [1, 1, 1, 32], it will create two fields:
    /// one using 8 bits (for the three 1-bit properties) and one using 32 bits
    /// (for the 32-bit property).
    /// </summary>
    /// <param name="requestedBits">
    /// A sequence of byte values, each representing how many bits a single property requires.
    /// </param>
    /// <returns>
    /// An immutable array of <see cref="CalculatedBits"/> describing how these bits are split
    /// into fields of minimal capacity (byte, ushort, uint, or ulong).
    /// </returns>
    /// <exception cref="UnreachableException">
    /// Thrown if an invalid bit size occurs or if a property exceeds 64 bits in one chunk.
    /// </exception>
    protected static ImmutableArray<CalculatedBits> DistributeBitsIntoFields(ReadOnlySpan<byte> requestedBits)
    {
        using var calculatedBits = ListsPool.Rent<CalculatedBits>();
        using var chunkBits = ListsPool.Rent<byte>();

        byte currentTotal = 0;

        for (var i = 0; i < requestedBits.Length; i++)
        {
            var bits = requestedBits[i];

            // Throw if a single property requires more than 64 bits
            if (bits > 64)
            {
                ThrowHelper.ThrowUnreachableException($"Property requires {bits} bits, which exceeds 64.");
            }

            // If adding the current property exceeds 64 bits, finalize the current chunk
            if (currentTotal + bits > 64)
            {
                FinalizeChunk(calculatedBits, chunkBits, currentTotal);

                // Start a new chunk
                chunkBits.Add(bits);
                currentTotal = bits;
            }
            else
            {
                // Otherwise, accumulate the current property in the ongoing chunk
                chunkBits.Add(bits);
                currentTotal += bits;
            }
        }

        // Finalize the last chunk if any bits remain
        if (currentTotal > 0)
        {
            FinalizeChunk(calculatedBits, chunkBits, currentTotal);
        }

        return [.. calculatedBits];

        static void FinalizeChunk(
            ListsPool.RentedListPool<CalculatedBits> calcBits,
            ListsPool.RentedListPool<byte> chunk,
            byte total)
        {
            if (total == 0)
            {
                return;
            }

            // Determine the appropriate field size for the total bits (8, 16, 32, 64)
            var fieldSize = DetermineFieldBitSize(total);

            // Throw if an invalid bit size is encountered
            if (fieldSize == BitSize.Invalid)
            {
                ThrowHelper.ThrowUnreachableException("Invalid bit size encountered during finalization.");
            }

            calcBits.Add(new CalculatedBits(fieldSize, [.. chunk]));
            chunk.Clear();
        }
    }


    /// <summary>
    /// Converts a field request and a collection of bit field property information
    /// into an immutable array of <see cref="BitFieldPropertyInfoRequest"/>.
    /// </summary>
    /// <param name="fieldRequest">
    /// The <see cref="FieldRequest"/> containing information about the field to be processed.
    /// </param>
    /// <param name="bitFieldPropertyInfos">
    /// An immutable array of <see cref="BaseBitFieldPropertyInfo"/> that provides information about the bit field properties.
    /// </param>
    /// <returns>
    /// An immutable array of <see cref="BitFieldPropertyInfoRequest"/> representing the bit field requests.
    /// </returns>
    /// <exception cref="UnreachableException">
    /// Thrown if the total required bits exceed the maximum allowed bits for the field type.
    /// </exception>
    protected static ImmutableArray<BitFieldPropertyInfoRequest> ToRequests(FieldRequest fieldRequest, ImmutableArray<BaseBitFieldPropertyInfo> bitFieldPropertyInfos)
    {
        using var requests = ListsPool.Rent<BitFieldPropertyInfoRequest>();

        byte currentOffset = 0;
        var maxBits = (byte)MapSpecialTypeToBitSize(fieldRequest.FieldType);

        for (var i = 0; i < bitFieldPropertyInfos.Length; i++)
        {
            var bitField = bitFieldPropertyInfos[i];
            var requiredBits = GetEffectiveBitsCount(bitField);

            var bitsSpan = new BitsSpan(fieldRequest, currentOffset, requiredBits);

            if (currentOffset + bitsSpan.Length > maxBits)
            {
                // Before using these methods,
                // you should validate the input. That's why this code is "unreachable."
                ThrowHelper.ThrowUnreachableException($"Too many bits. Maximum allowed is {maxBits}.");
            }

            currentOffset += bitsSpan.Length;

            var request = new BitFieldPropertyInfoRequest(bitsSpan, bitField);

            requests.Add(request);
        }

        return [.. requests];

    }

    /// <summary>
    /// Validates whether the total required bits fit within the specified <see cref="BitSize"/>.
    /// </summary>
    /// <param name="bitSize">The maximum allowable bit size.</param>
    /// <param name="requestedBits">The requested bit field properties.</param>
    /// <returns>
    /// <c>true</c> if the total required bits fit within the <see cref="BitSize"/>; otherwise, <c>false</c>.
    /// </returns>
    protected static bool ValidateSize(BitSize bitSize, ImmutableArray<BaseBitFieldPropertyInfo> requestedBits)
    {
        var bits = RequiredBits(requestedBits);

        return (byte)bitSize >= bits;
    }

    /// <summary>
    /// Calculates the total number of bits required for the given bit field properties.
    /// </summary>
    /// <param name="requestedBits">The bit field properties.</param>
    /// <returns>The total number of bits required.</returns>
    protected static int RequiredBits(ImmutableArray<BaseBitFieldPropertyInfo> requestedBits)
    {
        var requiredBits = 0;

        for (var i = 0; i < requestedBits.Length; i++)
        {
            var bit = requestedBits[i];
            requiredBits += GetEffectiveBitsCount(bit);
        }

        return requiredBits;
    }

    /// <summary>
    /// Determines the minimal field size for a given total bit count.
    /// For example, 1..8 => Byte, 9..16 => UInt16, 17..32 => UInt32, 33..64 => UInt64.
    /// </summary>
    /// <param name="bits">The total bit count.</param>
    /// <returns>A <see cref="BitSize"/> indicating the minimal type to hold 'bits'.</returns>
    protected static BitSize DetermineFieldBitSize(byte bits)
    {
        if (bits == 0)
        {
            return BitSize.Invalid;
        }

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

        return bits <= 64 ? BitSize.UInt64 : BitSize.Invalid;
    }

    /// <summary>
    /// Represents the calculated bits required for a bit field, including the field's capacity.
    /// </summary>
    /// <param name="fieldCapacity">The capacity of the field in bits.</param>
    /// <param name="bits">The individual bit sizes required.</param>
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


    /// <summary>
    /// Represents a group of properties associated with a specific owner and field name.
    /// </summary>
    /// <param name="owner">The type symbol representing the owner.</param>
    /// <param name="fieldName">The optional field name.</param>
    /// <param name="properties">The properties associated with the field.</param>
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

    /// <summary>
    /// Maps a <see cref="SpecialType"/> to its corresponding <see cref="BitSize"/>.
    /// </summary>
    /// <param name="specialType">The special type to map.</param>
    /// <returns>The corresponding <see cref="BitSize"/> for the given <see cref="SpecialType"/>.</returns>
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

    /// <summary>
    /// Maps a given <see cref="BitSize"/> value to its corresponding <see cref="SpecialType"/>.
    /// </summary>
    /// <param name="bitSize">The <see cref="BitSize"/> value to map.</param>
    /// <returns>
    /// The corresponding <see cref="SpecialType"/> for the provided <paramref name="bitSize"/>.
    /// If the <paramref name="bitSize"/> does not match any known value, <see cref="SpecialType.None"/> is returned.
    /// </returns>
    protected static SpecialType MapBitSizeToSpecialType(BitSize bitSize)
    {
        return bitSize switch
        {
            BitSize.Bool => SpecialType.System_Boolean,
            BitSize.Byte => SpecialType.System_Byte,
            BitSize.UInt16 => SpecialType.System_UInt16,
            BitSize.UInt32 => SpecialType.System_UInt32,
            BitSize.UInt64 => SpecialType.System_UInt64,
            _ => SpecialType.None
        };
    }

    /// <summary>
    /// Gets the effective number of bits required for the given bit field property information.
    /// </summary>
    /// <param name="baseBitFieldPropertyInfo">The bit field property information.</param>
    /// <returns>The number of bits required.</returns>
    protected static byte GetEffectiveBitsCount(BaseBitFieldPropertyInfo baseBitFieldPropertyInfo)
    {
        return BitCountHelper.GetEffectiveBitsCount(baseBitFieldPropertyInfo);
    }
}
