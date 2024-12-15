using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;
using PropertyBitPack.SourceGen.Collections;

namespace PropertyBitPack.SourceGen;

[Generator(LanguageNames.CSharp)]
public sealed class BitHelperSourceCodeGenerator : IIncrementalGenerator
{
    private const string BitsToFlagAttribute = nameof(BitsToFlagAttribute);
    private const string SmallBitsToFlagAttribute = nameof(SmallBitsToFlagAttribute);

    private static readonly string[] s_candidateAttributes = [BitsToFlagAttribute, SmallBitsToFlagAttribute];

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var propertiesWithAttributes = context.SyntaxProvider
            .CreateSyntaxProvider(
                static (syntaxNode, _) => IsCandidateProperty(syntaxNode),
                transform: static (context, _) => GetPropertyToBitInfo(context))
            .Where(static propertyInfo => propertyInfo is not null)
            .Select(static (propertyInfo, _) => propertyInfo!)
            .Collect();

        context.RegisterSourceOutput(propertiesWithAttributes, static (context, properties) =>
        {
            using (var validatedPropertiesBuilder = ImmutableArrayBuilder<PropertyToBitInfo>.Rent())
            {
                foreach (var property in properties)
                {
                    if (property is PropertyToBitInfo propertyToBitInfo)
                    {
                        foreach (var diagnostic in propertyToBitInfo.Diagnostics)
                        {
                            context.ReportDiagnostic(diagnostic);
                        }

                        if (propertyToBitInfo.IsError)
                        {
                            continue;
                        }
                        else
                        {
                            validatedPropertiesBuilder.Add(propertyToBitInfo);
                        }
                    }
                }

                properties = validatedPropertiesBuilder.ToImmutable();
            }

            if (properties.IsEmpty)
            {
                return;
            }

            var helper = BitFlagsForClassGeneratorHelper.Create(properties, context);

            while (!helper.IsEnd)
            {
                helper.TryGenerateNextClass(out var unitSyntax, out var helperDiagnostic);

                if (unitSyntax is not null)
                {
                    context.AddSource(helper.ClassName, unitSyntax.NormalizeWhitespace().ToFullString());

                }
            }

        });
    }

    private static bool IsCandidateProperty(SyntaxNode syntaxNode)
    {
        if (ContainsErrors(syntaxNode))
        {
            return false;
        }

        return syntaxNode is PropertyDeclarationSyntax propertyDeclaration
            && propertyDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword)
            && propertyDeclaration.AttributeLists.Count > 0
            && propertyDeclaration.AttributeLists.Any(static attrbiuteList => IsCandidateAttribute(attrbiuteList));

        static bool IsCandidateAttribute(AttributeListSyntax attributeList)
        {
            return attributeList.Attributes.Any();
        }
    }

    private static PropertyToBitInfo? GetPropertyToBitInfo(GeneratorSyntaxContext context)
    {
        using var diaognosticsBuilder = ImmutableArrayBuilder<Diagnostic>.Rent();

        var propertyDeclaration = (PropertyDeclarationSyntax)context.Node;

        var semanticModel = context.SemanticModel;

        var propertySymbol = semanticModel.GetDeclaredSymbol(propertyDeclaration);

        if (propertySymbol is null)
        {
            return null;
        }

        string? attributeName = null;
        AttributeSyntax? attributeSyntax = null;

        foreach (var attribute in propertySymbol.GetAttributes())
        {
            if (s_candidateAttributes.Contains(attribute.AttributeClass?.Name))
            {
                attributeSyntax = (AttributeSyntax?)attribute.ApplicationSyntaxReference?.GetSyntax();
                attributeName = attribute.AttributeClass?.Name;
                break;
            }
        }

        if (attributeName is null || attributeSyntax is null)
        {
            return null;
        }

        if (propertySymbol.Type is not ITypeSymbol propertyType)
        {
            return null;
        }

        if (propertySymbol.ContainingType is not ITypeSymbol owner)
        {
            return null;
        }

        var isSmallBits = attributeName == SmallBitsToFlagAttribute;

        var attributeInfo = GetAttributeInfo(attributeSyntax, context);

        if(attributeInfo.Diagnostic is not null)
        {
            diaognosticsBuilder.Add(attributeInfo.Diagnostic);
        }

        attributeInfo.BitsCount ??= GetDeafultBitsCount(propertyType);

        if (attributeInfo.BitsCount is null)
        {
            diaognosticsBuilder.Add(
                Diagnostic.Create(
                    NoitraDiagnostics.UnknowPropertyType,
                    attributeSyntax.GetLocation(),
                    propertySymbol.Name
                )
            );
        }

        attributeInfo.FieldName ??= $"_{propertySymbol.Name}_data";
        attributeInfo.GetterLargeSizeValueName ??= $"Get{propertySymbol.Name}LargeSizeValue";

        var isInit = propertyDeclaration.AccessorList?.Accessors.Any(static accessor => accessor.IsKind(SyntaxKind.InitAccessorDeclaration)) ?? false;
        var setterOrInitModifiers = propertyDeclaration.AccessorList?.Accessors
            .Where(static accessor =>
                accessor.IsKind(SyntaxKind.InitAccessorDeclaration) ||
                accessor.IsKind(SyntaxKind.SetAccessorDeclaration))
            .Select(static accessor => accessor.Modifiers).FirstOrDefault() ?? default;

        return new()
        {
            PropertyDeclaration = propertyDeclaration,
            GetterLargeSizeValueSymbol = attributeInfo.GetterLargeSizeValueSymbol,
            IsInit = isInit,
            SetterOrInitModifiers = setterOrInitModifiers,
            PropertySymbol = propertySymbol,
            BitsCount = attributeInfo.BitsCount,
            FieldName = attributeInfo.FieldName,
            PropertyType = propertyType,
            Owner = owner,
            IsSmallBits = isSmallBits,
            GetterLargeSizeValueName = attributeInfo.GetterLargeSizeValueName,
            Diagnostics = diaognosticsBuilder.ToImmutable()
        };

        static (int? BitsCount, string? FieldName, string? GetterLargeSizeValueName, ISymbol? GetterLargeSizeValueSymbol, Diagnostic? Diagnostic) GetAttributeInfo(AttributeSyntax attributeSyntax, GeneratorSyntaxContext context)
        {
            const string BitsCount = nameof(BitsCount);
            const string FieldName = nameof(FieldName);
            const string GetterLargeSizeValueName = nameof(GetterLargeSizeValueName);

            int? bitsCount = null;
            string? fieldName = null;
            string? getterLargeSizeValueName = null;
            ISymbol? getterLargeSizeValueSymbol = null;

            if (attributeSyntax.ArgumentList is null)
            {
                return (bitsCount, fieldName, getterLargeSizeValueName, getterLargeSizeValueSymbol, null);
            }

            foreach (var argument in attributeSyntax.ArgumentList.Arguments)
            {
                var name = argument.NameEquals?.Name.Identifier.Text;

                if (name == BitsCount)
                {
                    bitsCount = argument.Expression is LiteralExpressionSyntax literalExpression
                        && literalExpression.IsKind(SyntaxKind.NumericLiteralExpression)
                        ? int.TryParse(literalExpression.Token.ValueText, out var value) ? value : null
                        : null;
                }

                if (name == FieldName)
                {
                    fieldName = argument.Expression is LiteralExpressionSyntax literalExpression
                        && literalExpression.IsKind(SyntaxKind.StringLiteralExpression)
                        ? literalExpression.Token.ValueText
                        : null;
                }

                if (name == GetterLargeSizeValueName)
                {
                    if (argument.Expression is InvocationExpressionSyntax invocation
                        && invocation.Expression is IdentifierNameSyntax identifier
                        && identifier.Identifier.Text == "nameof")
                    {
                        var methodName = invocation.ArgumentList.Arguments[0];
                        var methodIdentifier = (IdentifierNameSyntax)methodName.Expression;

                        var nameofOperation = context.SemanticModel.GetOperation(invocation);

                        if (nameofOperation is INameOfOperation nameOfOperation)
                        {
                            var argumentOfOperation = nameOfOperation.Argument;

                            if (argumentOfOperation is IPropertyReferenceOperation propertyReference)
                            {
                                getterLargeSizeValueName = propertyReference.Property.Name;
                                getterLargeSizeValueSymbol = propertyReference.Property;
                                continue;
                            }

                            if (argumentOfOperation is IMethodReferenceOperation methodReference)
                            {
                                getterLargeSizeValueName = methodReference.Method.Name;
                                getterLargeSizeValueSymbol = methodReference.Method;
                                continue;
                            }

                            return (null, null, null, null, Diagnostic.Create(NoitraDiagnostics.GetterLargeSizeValueNameNameOfShoudReferenceToPropertyOrMethod, argument.GetLocation()));
                        }


                    }

                    return (null, null, null, null, Diagnostic.Create(NoitraDiagnostics.MandatoryOfNameofInGetterLargeSizeValueName, argument.GetLocation()));

                }
            }

            return (bitsCount, fieldName, getterLargeSizeValueName, getterLargeSizeValueSymbol, null);
        }

        static int? GetDeafultBitsCount(ITypeSymbol typeSymbol)
        {

            Dictionary<SpecialType, int> defaultBitsSize = new()
            {
                { SpecialType.System_Boolean, 1 },
                { SpecialType.System_Byte, 8 },
                { SpecialType.System_SByte, 8 },
                { SpecialType.System_Int16, 16 },
                { SpecialType.System_UInt16, 16 },
                { SpecialType.System_Int32, 32 },
                { SpecialType.System_UInt32, 32 },
                { SpecialType.System_Int64, 64 },
                { SpecialType.System_UInt64, 64 },
            };

            var type = typeSymbol.SpecialType;

            if (defaultBitsSize.TryGetValue(type, out var bitsCount))
            {
                return bitsCount;
            }

            return null;
        }

    }

    private static bool ContainsErrors(SyntaxNode node)
        => node
            .GetDiagnostics()
            .Any(d => d.Severity == DiagnosticSeverity.Error);

}
