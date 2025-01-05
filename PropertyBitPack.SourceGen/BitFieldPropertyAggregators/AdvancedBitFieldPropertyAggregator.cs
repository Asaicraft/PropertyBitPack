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

        // Step 1: Separate named vs unnamed.
        var namedProperties = new List<BaseBitFieldPropertyInfo>();
        var unnamedProperties = new List<BaseBitFieldPropertyInfo>();

        foreach (var p in properties)
        {
            if (!string.IsNullOrWhiteSpace(p.AttributeParsedResult.FieldName?.Name))
            {
                namedProperties.Add(p);
            }
            else
            {
                unnamedProperties.Add(p);
            }
        }

        // Step 2: Handle named properties.
        // Group by (Owner, FieldName), because each distinct FieldName within each Owner 
        // must go into exactly ONE field.
        // If multiple properties share the same (Owner, FieldName), 
        // we sum their bits and pick the type or use the existing symbol's type.
        var namedGroups = namedProperties
            .GroupBy(
                keySelector: p => (p.Owner, Name: p.AttributeParsedResult.FieldName!.Name!),
                comparer: new OwnerFieldNameComparer()) // This is a custom IEqualityComparer, or you can do .ToLookup instead
            .ToList();

        foreach (var group in namedGroups)
        {
            var owner = group.Key.Owner;
            var fieldName = group.Key.Name;
            var propsList = group.ToList();

            // Sum the bits required
            var totalBits = 0;
            var hasInvalid = false;
            foreach (var prop in propsList)
            {
                var bits = BitCountHelper.GetEffectiveBitsCount(prop);
                if (bits <= 0)
                {
                    diagnostics.Add(Diagnostic.Create(
                        PropertyBitPackDiagnostics.InvalidBitsCount,
                        Location.None,
                        prop.PropertySymbol.Name));
                    hasInvalid = true;
                }
                else
                {
                    totalBits += bits;
                }
            }

            if (hasInvalid)
            {
                // Skip the group if it has invalid bit counts
                continue;
            }

            // Check if there's an existing symbol for this FieldName 
            // (in practice, you might only expect 1 property in the group to have ExistingSymbol, 
            //  or they might all share the same existing symbol).
            // Here, for simplicity, we'll assume if ANY property says IsSymbolExist==true, 
            // we treat the field as existing. If multiple differ => that's a scenario to handle or diagnose.
            var anyPropertyWithExistingSymbol = propsList
                .FirstOrDefault(static p => p.AttributeParsedResult.FieldName!.IsSymbolExist);

            FieldRequest fieldRequest;
            if (anyPropertyWithExistingSymbol != null)
            {
                // Must use the existing field's type
                var existingSym = anyPropertyWithExistingSymbol.AttributeParsedResult.FieldName!.ExistingSymbol;
                // For simplicity, assume we can map this symbol to a known SpecialType 
                // or a bit capacity. Otherwise, you might need reflection or Symbol analysis.
                var existingType = MapSymbolToSpecialType(existingSym, diagnostics);

                if (existingType == SpecialType.None)
                {
                    // We can't determine a valid type from the symbol => skip or diag.
                    continue;
                }

                var capacity = BitCountHelper.GetTypeBitCapacity(existingType);
                if (totalBits > capacity)
                {
                    // Too many bits for existing field
                    diagnostics.Add(Diagnostic.Create(
                        PropertyBitPackDiagnostics.TooManyBitsForAnyType,
                        Location.None,
                        fieldName,
                        totalBits));
                    continue;
                }

                fieldRequest = new FieldRequest(fieldName, existingType, isExist: true);
            }
            else
            {
                // No existing symbol => we pick the smallest new type that can hold totalBits
                var chosenType = availableTypes.FirstOrDefault(t => BitCountHelper.GetTypeBitCapacity(t) >= totalBits);
                if (chosenType == default)
                {
                    // Not found => means >64 bits => diag
                    diagnostics.Add(Diagnostic.Create(
                        PropertyBitPackDiagnostics.TooManyBitsForAnyType,
                        Location.None,
                        fieldName,
                        totalBits));
                    continue;
                }

                fieldRequest = new FieldRequest(fieldName, chosenType, isExist: false);
            }

            // Build property requests with offsets.
            using var propertyRequestsBuilder = ImmutableArrayBuilder<BitFieldPropertyInfoRequest>.Rent();
            int offset = 0;
            foreach (var prop in propsList)
            {
                var bits = BitCountHelper.GetEffectiveBitsCount(prop);
                if (bits <= 0)
                {
                    continue; // Already diagnosed above.
                }

                var span = new BitsSpan(fieldRequest, (byte)offset, (byte)bits);
                offset += bits;

                propertyRequestsBuilder.Add(new BitFieldPropertyInfoRequest(span, prop));
                // You may also remove these props from the main linked list if needed.
                properties.Remove(prop);
            }

            var finalProps = propertyRequestsBuilder.ToImmutable();
            var fieldsImmutable = ImmutableArray.Create(fieldRequest);
            requestsBuilder.Add(new SimpleGenerateSourceRequest(fieldsImmutable, finalProps));
        }

        // Step 3: Handle unnamed properties.
        // We group them by (Owner). Then, for each group, we do the "packing approach" 
        // using as few fields as possible.
        var unnamedGroups = unnamedProperties
            .GroupBy(static p => p.Owner, SymbolEqualityComparer.Default)
            .ToList();

        foreach (var group in unnamedGroups)
        {
            var owner = group.Key;
            var propsList = group.ToList();

            // Sort descending by bits, so we pack large ones first.
            propsList.Sort((a, b) =>
            {
                var bitsA = BitCountHelper.GetEffectiveBitsCount(a);
                var bitsB = BitCountHelper.GetEffectiveBitsCount(b);
                return bitsB.CompareTo(bitsA);
            });

            // We'll keep a working list
            var leftover = new List<BaseBitFieldPropertyInfo>(propsList);

            while (leftover.Count > 0)
            {
                var largest = leftover[0];
                var largestBits = BitCountHelper.GetEffectiveBitsCount(largest);

                // Check if there's an existing symbol for this property.
                // (Corner case: multiple unnamed properties might share the same existing field name with IsSymbolExist?)
                // Здесь, ради простоты, представим ситуацию:
                // - Если IsSymbolExist == true, то это "один" уже существующий field 
                //   (ведь имя отсутствует, но symbol есть — специфичный кейс).
                //   Либо вы можете решать, что если несколько безымянных свойств указывают на один и тот же ExistingSymbol, 
                //   они упакуются вместе. 
                //   Для brevity — мы покажем логику на одно свойство. Если не влезает, diag.
                var isAnyExist = largest.AttributeParsedResult.FieldName?.IsSymbolExist == true;
                SpecialType chosenType;
                bool isExistField;

                if (isAnyExist)
                {
                    // Use the existing symbol from the property
                    var existSym = largest.AttributeParsedResult.FieldName!.ExistingSymbol;
                    chosenType = MapSymbolToSpecialType(existSym, diagnostics);
                    if (chosenType == SpecialType.None)
                    {
                        leftover.RemoveAt(0);
                        continue;
                    }
                    isExistField = true;
                }
                else
                {
                    // Pick the smallest new type for the largest property
                    chosenType = availableTypes.FirstOrDefault(t => BitCountHelper.GetTypeBitCapacity(t) >= largestBits);
                    if (chosenType == default)
                    {
                        // >64 => diag
                        diagnostics.Add(Diagnostic.Create(
                            PropertyBitPackDiagnostics.TooManyBitsForAnyType,
                            Location.None,
                            largest.PropertySymbol.Name,
                            largestBits));
                        leftover.RemoveAt(0);
                        continue;
                    }
                    isExistField = false;
                }

                var capacity = BitCountHelper.GetTypeBitCapacity(chosenType);
                if (largestBits > capacity)
                {
                    // Even for the largest property alone, we can't fit => diag
                    diagnostics.Add(Diagnostic.Create(
                        PropertyBitPackDiagnostics.TooManyBitsForAnyType,
                        Location.None,
                        largest.PropertySymbol.Name,
                        largestBits));
                    leftover.RemoveAt(0);
                    continue;
                }

                using var propertyRequestsBuilder = ImmutableArrayBuilder<BitFieldPropertyInfoRequest>.Rent();
                var packedNames = new List<string>();
                int offset = 0, bitsUsed = 0;

                // Try to fit as many properties as we can into 'chosenType'
                for (int i = 0; i < leftover.Count;)
                {
                    var prop = leftover[i];
                    var bits = BitCountHelper.GetEffectiveBitsCount(prop);
                    // Skip invalid bits
                    if (bits <= 0)
                    {
                        leftover.RemoveAt(i);
                        diagnostics.Add(Diagnostic.Create(
                            PropertyBitPackDiagnostics.InvalidBitsCount,
                            Location.None,
                            prop.PropertySymbol.Name));
                        continue;
                    }

                    // If this property has an existing symbol that conflicts 
                    // (i.e., a different symbol or a different type?), 
                    // в реальном коде нужно обрабатывать. Для простоты предполагаем, что:
                    // - Если "isExistField" true, мы упаковываем только то же самое поле. 
                    //   Если у следующего prop другой existing symbol — пропускаем его, например.
                    bool thisPropHasExist = prop.AttributeParsedResult.FieldName?.IsSymbolExist == true;
                    if (isExistField && thisPropHasExist)
                    {
                        // Check if it's the same symbol => if not, skip
                        var otherSym = prop.AttributeParsedResult.FieldName!.ExistingSymbol;
                        var otherType = MapSymbolToSpecialType(otherSym, diagnostics);
                        if (otherType != chosenType)
                        {
                            i++;
                            continue;
                        }
                    }
                    else if (thisPropHasExist != isExistField)
                    {
                        // Can't mix existing with newly created in the same field => skip
                        i++;
                        continue;
                    }

                    if (bitsUsed + bits <= capacity)
                    {
                        var placeholderSpan = new BitsSpan(default, (byte)offset, (byte)bits);
                        propertyRequestsBuilder.Add(new BitFieldPropertyInfoRequest(placeholderSpan, prop));

                        packedNames.Add(prop.PropertySymbol.Name);
                        offset += bits;
                        bitsUsed += bits;
                        leftover.RemoveAt(i);
                    }
                    else
                    {
                        i++;
                    }
                }

                if (packedNames.Count == 0)
                {
                    // We didn't pack anything => to avoid infinite loop, remove the first item
                    leftover.RemoveAt(0);
                    continue;
                }

                // Create the final field name from all packed properties
                var combinedFieldName = string.Join("__", packedNames);

                // Create the FieldRequest
                var fieldRequest = new FieldRequest(combinedFieldName, chosenType, isExistField);

                // Update each BitsSpan with the real FieldRequest
                var itemsArray = propertyRequestsBuilder.ToArray();
                for (var i = 0; i < itemsArray.Length; i++)
                {
                    var oldSpan = itemsArray[i].BitsSpan;
                    var newSpan = new BitsSpan(fieldRequest, oldSpan.Start, oldSpan.Length);
                    itemsArray[i] = new BitFieldPropertyInfoRequest(newSpan, itemsArray[i].BitFieldPropertyInfo);
                }

                var finalProps = itemsArray.ToImmutableArray();
                var fieldsImmutable = ImmutableArray.Create(fieldRequest);
                requestsBuilder.Add(new SimpleGenerateSourceRequest(fieldsImmutable, finalProps));
            }
        }

        // Step 4: Done
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