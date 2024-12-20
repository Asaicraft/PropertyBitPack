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
    public static ImmutableArray<PackedFieldStorage> Aggregate(
        ImmutableArray<PropertyToBitInfo> propertyToBitInfos,
        in ImmutableArrayBuilder<Diagnostic> diagnosticsBuilder)
    {
        var availableTypes = new (TypeSyntax TypeSyntax, int Bits)[] {
            (SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ByteKeyword)), 8),
            (SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.UShortKeyword)), 16),
            (SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.UIntKeyword)), 32),
            (SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ULongKeyword)), 64)
        };

        if (propertyToBitInfos.IsEmpty)
        {
            return ImmutableArray<PackedFieldStorage>.Empty;
        }

        // Валидируем BitsCount
        using (var validateBuilder = ImmutableArrayBuilder<PropertyToBitInfo>.Rent(propertyToBitInfos.Length))
        {
            foreach (var property in propertyToBitInfos)
            {
                if (property.BitsCount <= 0)
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

        // Если после фильтрации нет свойств — выходим
        if (propertyToBitInfos.IsEmpty)
        {
            return [];
        }

        // Группируем по Owner
        var ownerGroups = propertyToBitInfos
            .GroupBy<PropertyToBitInfo, ITypeSymbol>(p => p.Owner, SymbolEqualityComparer.Default)
            .ToList();

        var resultBuilder = ImmutableArrayBuilder<PackedFieldStorage>.Rent();

        try
        {
            foreach (var ownerGroup in ownerGroups)
            {
                var owner = ownerGroup.Key;
                var properties = ownerGroup.OrderByDescending(p => p.BitsCount).ToList();

                // Группы с FieldName
                var withFieldNameGroups = properties
                    .Where(p => p.FieldName is not null)
                    .GroupBy(p => p.FieldName!)
                    .ToList();

                // Свойства без FieldName
                var withoutFieldName = properties.Where(p => p.FieldName is null).ToList();

                // Обрабатываем группы с FieldName (каждая группа в своем поле)
                foreach (var group in withFieldNameGroups)
                {
                    var groupProperties = group.ToList();
                    var totalBitsRequired = groupProperties.Sum(p => p.BitsCount);

                    // Находим минимально подходящий тип для всей группы
                    var chosenType = availableTypes.FirstOrDefault(t => t.Bits >= totalBitsRequired);
                    if (chosenType.TypeSyntax is null)
                    {
                        // Не помещается ни в один тип — фиксируем диагностику
                        diagnosticsBuilder.Add(Diagnostic.Create(
                            PropertyBitPackDiagnostics.TooManyBitsForAnyType,
                            Location.None,
                            group.Key,
                            totalBitsRequired));
                        continue;
                    }

                    var propsArray = groupProperties.ToImmutableArray();
                    resultBuilder.Add(new PackedFieldStorage(
                        FieldName: group.Key,
                        TypeSyntax: chosenType.TypeSyntax,
                        TypeBitsCount: chosenType.Bits,
                        StoredBitsCount: totalBitsRequired,
                        PropertiesWhichDataStored: propsArray,
                        Owner: owner
                    ));

                    // Удаляем эти свойства из списка без FieldName
                    foreach (var gp in groupProperties)
                    {
                        withoutFieldName.Remove(gp);
                    }
                }

                // Теперь обрабатываем свойства без FieldName
                if (withoutFieldName.Count > 0)
                {
                    // Считаем суммарные биты
                    var totalWithoutFieldNameBits = withoutFieldName.Sum(p => p.BitsCount);

                    // Пытаемся уместить ВСЕ свойства без FieldName в один доступный тип
                    var singleType = availableTypes.FirstOrDefault(t => t.Bits >= totalWithoutFieldNameBits);
                    if (singleType.TypeSyntax is not null)
                    {
                        // Все свойства без FieldName помещаются в один тип
                        var propsArray = withoutFieldName.ToImmutableArray();
                        var fieldName = "_"+string.Join("__", propsArray.Select(p => p.PropertySymbol.Name));

                        resultBuilder.Add(new PackedFieldStorage(
                            FieldName: fieldName,
                            TypeSyntax: singleType.TypeSyntax,
                            TypeBitsCount: singleType.Bits,
                            StoredBitsCount: totalWithoutFieldNameBits,
                            PropertiesWhichDataStored: propsArray,
                            Owner: owner
                        ));
                    }
                    else
                    {
                        // Не можем уместить все свойства в один тип.
                        // Тогда подбираем тип по самому крупному свойству:
                        var largestProperty = withoutFieldName[0];
                        var largestBits = largestProperty.BitsCount;
                        var minimalSuitableType = availableTypes.FirstOrDefault(t => t.Bits >= largestBits);
                        if (minimalSuitableType.TypeSyntax is null)
                        {
                            // Есть свойство, которое не помещается ни в один тип
                            diagnosticsBuilder.Add(Diagnostic.Create(
                                PropertyBitPackDiagnostics.TooManyBitsForAnyType,
                                Location.None,
                                largestProperty.FieldName ?? largestProperty.PropertySymbol.Name,
                                largestBits));

                            // Ничего не упаковываем
                        }
                        else
                        {
                            // Разбиваем на несколько полей выбранного типа
                            var chosenBits = minimalSuitableType.Bits;
                            // Снова сортируем по убыванию (должно быть уже так, но на всякий случай)
                            withoutFieldName.Sort((a, b) => b.BitsCount.CompareTo(a.BitsCount));

                            while (withoutFieldName.Count > 0)
                            {
                                using var currentPropertiesBuilder = ImmutableArrayBuilder<PropertyToBitInfo>.Rent();
                                var currentBitsUsed = 0;

                                for (var i = 0; i < withoutFieldName.Count;)
                                {
                                    var bits = withoutFieldName[i].BitsCount;
                                    if (currentBitsUsed + bits <= chosenBits)
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

                                resultBuilder.Add(new PackedFieldStorage(
                                    FieldName: fieldName,
                                    TypeSyntax: minimalSuitableType.TypeSyntax,
                                    TypeBitsCount: chosenBits,
                                    StoredBitsCount: currentBitsUsed,
                                    PropertiesWhichDataStored: currentProperties,
                                    Owner: owner
                                ));
                            }
                        }
                    }
                }
            }

            return resultBuilder.ToImmutable();
        }
        finally
        {
            resultBuilder.Dispose();
        }
    }
}