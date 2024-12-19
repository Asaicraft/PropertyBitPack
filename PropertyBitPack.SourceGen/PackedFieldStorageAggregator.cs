using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using PropertyBitPack.SourceGen.Collections;
using PropertyBitPack.SourceGen.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using CommunityToolkit.Diagnostics;

namespace PropertyBitPack.SourceGen;
public static class PackedFieldStorageAggregator
{
    public static ImmutableArray<PackedFieldStorage> Aggregate(ImmutableArray<PropertyToBitInfo> propertyToBitInfos, in ImmutableArrayBuilder<Diagnostic> diagnosticsBuilder)
    {
        var availableTypes = new (TypeSyntax TypeSyntax, int Bits)[] {
            (SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ByteKeyword)), 8),
            (SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.UShortKeyword)), 16),
            (SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.UIntKeyword)), 32),
            (SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ULongKeyword)), 64)
        };

        if (propertyToBitInfos.IsEmpty)
        {
            return [];
        }

        using (var validateBuilder = ImmutableArrayBuilder<PropertyToBitInfo>.Rent(propertyToBitInfos.Length))
        { 
            foreach (var property in propertyToBitInfos)
            {
                if (property.BitsCount == 0)
                {
                    diagnosticsBuilder.Add(Diagnostic.Create(
                        PropertyBitPackDiagnostics.InvalidBitsCount,
                        Location.None,
                        property.PropertySymbol.Name));
                }
                else
                {
                    validateBuilder.Add(property);
                }
            }

            propertyToBitInfos = validateBuilder.ToImmutable();
        }

        var properties = propertyToBitInfos.OrderByDescending(p => p.BitsCount).ToList();

        var withFieldNameGroups = properties
            .Where(p => p.FieldName is not null)
            .GroupBy(p => p.FieldName!)
            .ToList();

        var withoutFieldName = properties.Where(p => p.FieldName is null).ToList();

        var wagonsBuilder = ImmutableArrayBuilder<PackedFieldStorage>.Rent(Math.Max((int)Math.Sqrt(propertyToBitInfos.Length), 8));
        try 
        {

            foreach (var group in withFieldNameGroups)
            {
                var groupProperties = group.ToList();
                var totalBitsRequired = groupProperties.Sum(p => p.BitsCount);

                var chosenType = availableTypes.FirstOrDefault(t => t.Bits >= totalBitsRequired);
                if (chosenType.TypeSyntax is null)
                {
                    diagnosticsBuilder.Add(Diagnostic.Create(
                        PropertyBitPackDiagnostics.TooManyBitsForAnyType,
                        Location.None,
                        group.Key,
                        totalBitsRequired));

                    continue;
                }

                var propsArray = groupProperties.ToImmutableArray();
                wagonsBuilder.Add(new PackedFieldStorage(
                    group.Key,
                    chosenType.TypeSyntax,
                    chosenType.Bits,
                    propsArray
                ));

                foreach (var gp in groupProperties)
                {
                    withoutFieldName.Remove(gp);
                }
            }

            while (withoutFieldName.Count > 0)
            {
                var largestProperty = withoutFieldName[0];
                var largestBits = largestProperty.BitsCount;
                var chosenType = availableTypes.FirstOrDefault(t => t.Bits >= largestBits);
                if (chosenType.TypeSyntax is null)
                {
                    diagnosticsBuilder.Add(Diagnostic.Create(
                        PropertyBitPackDiagnostics.TooManyBitsForAnyType,
                        Location.None,
                        largestProperty.FieldName ?? largestProperty.PropertySymbol.Name,
                        largestBits));

                    withoutFieldName.RemoveAt(0);
                    continue;
                }

                using var currentPropertiesBuilder = ImmutableArrayBuilder<PropertyToBitInfo>.Rent();
                var currentBitsUsed = 0;

                currentPropertiesBuilder.Add(largestProperty);
                currentBitsUsed += largestBits;
                withoutFieldName.RemoveAt(0);

                for (var i = 0; i < withoutFieldName.Count;)
                {
                    var bits = withoutFieldName[i].BitsCount;
                    if (currentBitsUsed + bits <= chosenType.Bits)
                    {
                        currentPropertiesBuilder.Add(withoutFieldName[i]);
                        currentBitsUsed += bits;
                        withoutFieldName.RemoveAt(i);
                    }
                    else
                    {
                        i++;
                    }
                }

                var currentProperties = currentPropertiesBuilder.ToImmutable();
                var fieldName = string.Join("__", currentProperties.Select(p => p.PropertySymbol.Name));

                wagonsBuilder.Add(new PackedFieldStorage(
                    fieldName,
                    chosenType.TypeSyntax,
                    chosenType.Bits,
                    currentProperties
                ));
            }

            return wagonsBuilder.ToImmutable();

        }
        finally
        {
            wagonsBuilder.Dispose();
        }
    }
}
