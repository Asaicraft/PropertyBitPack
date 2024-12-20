using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using PropertyBitPack.SourceGen.Collections;
using PropertyBitPack.SourceGen.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PropertyBitPack.SourceGen;
public static class SemanticModelHelper
{
    public static ISymbol? GetCandidateGetterLargeSizeValueNameSymbol(
        ITypeSymbol owner,
        INameOfOperation nameOfOperation,
        IPropertySymbol targetPropertySymbol,
        PropertyDeclarationSyntax propertyDeclarationSyntax,
        AttributeArgumentSyntax getterLargeSizeValueNameSyntax,
        AttributeData attributeData,
        in ImmutableArrayBuilder<Diagnostic> diagnostics)
    {
        // Извлекаем имя из nameof(...)
        if (!nameOfOperation.ConstantValue.HasValue || nameOfOperation.ConstantValue.Value is not string symbolName)
        {
            diagnostics.Add(Diagnostic.Create(
                PropertyBitPackDiagnostics.InvalidReferenceInGetterLargeSizeValueName,
                getterLargeSizeValueNameSyntax.GetLocation(),
                propertyDeclarationSyntax.GetFirstToken().ValueText));
            return null;
        }

        // Ищем кандидатов по имени
        var candidates = owner.GetMembers(symbolName);
        if (candidates.Length == 0)
        {
            diagnostics.Add(Diagnostic.Create(
                PropertyBitPackDiagnostics.InvalidReferenceInGetterLargeSizeValueName,
                getterLargeSizeValueNameSyntax.GetLocation(),
                propertyDeclarationSyntax.GetFirstToken().ValueText));
            return null;
        }

        IPropertySymbol? propertyCandidate = null;
        List<IMethodSymbol>? methodCandidates = null;

        // Собираем все методы и возможное свойство
        foreach (var candidate in candidates)
        {
            switch (candidate)
            {
                case IPropertySymbol p:
                    propertyCandidate ??= p;
                    break;
                case IMethodSymbol m:
                    methodCandidates ??= [];
                    methodCandidates.Add(m);
                    break;
            }
        }

        // Если есть свойство, проверяем его
        if (propertyCandidate is not null)
        {
            var propertySymbol = propertyCandidate;
            if (!targetPropertySymbol.Type.Equals(propertySymbol.Type, SymbolEqualityComparer.Default))
            {
                diagnostics.Add(Diagnostic.Create(
                    PropertyBitPackDiagnostics.InvalidTypeForPropertyReference,
                    getterLargeSizeValueNameSyntax.GetLocation(),
                    propertyDeclarationSyntax.GetFirstToken().ValueText,
                    targetPropertySymbol.Type.ToDisplayString()));
                return null;
            }

            return propertySymbol;
        }

        // Если свойство не найдено, ищем метод
        if (methodCandidates is not null && methodCandidates.Count > 0)
        {
            // Фильтруем по возвращаемому типу
            var filteredByReturnType = methodCandidates
                .Where(m => m.ReturnType.Equals(targetPropertySymbol.Type, SymbolEqualityComparer.Default))
                .ToList();

            if (filteredByReturnType.Count == 0)
            {
                // Нет методов с нужным возвращаемым типом
                diagnostics.Add(Diagnostic.Create(
                    PropertyBitPackDiagnostics.InvalidReferenceInGetterLargeSizeValueName,
                    getterLargeSizeValueNameSyntax.GetLocation(),
                    propertyDeclarationSyntax.GetFirstToken().ValueText));
                return null;
            }

            // Отбираем методы без параметров
            var noParamMethods = filteredByReturnType.Where(m => m.Parameters.Length == 0).ToList();
            IMethodSymbol? chosenMethod = null;

            if (noParamMethods.Count > 0)
            {
                // Есть метод(ы) без параметров — выбираем первый найденный
                chosenMethod = noParamMethods[0];
            }
            else
            {
                // Нет методов без параметров, смотрим методы с параметрами только по умолчанию
                var defaultParamMethods = filteredByReturnType
                    .Where(m => m.Parameters.All(p => p.HasExplicitDefaultValue))
                    .ToList();

                if (defaultParamMethods.Count == 0)
                {
                    // Нет подходящих методов (ни без параметров, ни с параметрами по умолчанию)
                    diagnostics.Add(Diagnostic.Create(
                        PropertyBitPackDiagnostics.MethodWithParametersNotAllowed,
                        getterLargeSizeValueNameSyntax.GetLocation(),
                        filteredByReturnType[0].Name));
                    return null;
                }

                // Среди defaultParamMethods выбираем метод с наименьшим числом параметров
                defaultParamMethods.Sort((m1, m2) => m1.Parameters.Length.CompareTo(m2.Parameters.Length));
                chosenMethod = defaultParamMethods[0];
            }

            return chosenMethod;
        }

        // Ни свойство, ни метод не подошли
        diagnostics.Add(Diagnostic.Create(
            PropertyBitPackDiagnostics.InvalidReferenceInGetterLargeSizeValueName,
            getterLargeSizeValueNameSyntax.GetLocation(),
            propertyDeclarationSyntax.GetFirstToken().ValueText));
        return null;
    }

}