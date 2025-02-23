using CommunityToolkit.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PropertyBitPack.SourceGen.Collections;
using PropertyBitPack.SourceGen.Models;
using PropertyBitPack.SourceGen.Models.AttributeParsedResults;
using PropertyBitPack.SourceGen.Models.GenerateSourceRequests;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace PropertyBitPack.SourceGen.PropertiesSyntaxGenerators;

/// <summary>
/// Generates syntax for properties annotated with <see cref="ExtendedBitFieldAttribute"/>.
/// </summary>
/// <remarks>
/// This generator produces syntax to handle properties with multi-bit fields, 
/// ensuring that values exceeding the defined range are appropriately processed. 
/// Example syntax generated:
/// <code>
/// const {PropertyType} maxValue_ = {MaxValue};
/// var value = {CurrentValue};
///
/// if (value == maxValue_)
/// {
///     return {GetterLargeSizeValueName};
/// }
///
/// return value;
/// </code>
/// Boolean properties are not supported for <see cref="ExtendedBitFieldAttribute"/>.
/// </remarks>
internal sealed class ExtendedPropertySyntaxGenerator(PropertyBitPackGeneratorContext propertyBitPackGeneratorContext) : BasePropertySyntaxGenerator(propertyBitPackGeneratorContext)
{

    /// <summary>
    /// Determines if the specified property is a candidate for extended property syntax generation.
    /// </summary>
    /// <param name="sourceRequest">The source request containing generation context.</param>
    /// <param name="bitFieldPropertyInfoRequest">The property request to evaluate.</param>
    /// <returns>
    /// <c>true</c> if the property is annotated with <see cref="IParsedExtendedBitFiledAttribute"/>; otherwise, <c>false</c>.
    /// </returns>
    protected override bool IsCandidate(IGenerateSourceRequest sourceRequest, BitFieldPropertyInfoRequest bitFieldPropertyInfoRequest)
    {
        return bitFieldPropertyInfoRequest.AttributeParsedResult is IParsedExtendedBitFiledAttribute;
    }

    /// <summary>
    /// Generates the getter block syntax for a property with <see cref="ExtendedBitFieldAttribute"/>.
    /// </summary>
    /// <param name="bitFieldPropertyInfoRequest">
    /// The request containing details about the property to generate syntax for.
    /// </param>
    /// <returns>
    /// A <see cref="BlockSyntax"/> representing the getter implementation for the property.
    /// </returns>
    /// <exception cref="UnreachableException">
    /// Thrown if the property type is <c>bool</c>, as it is not supported for <see cref="ExtendedBitFieldAttribute"/>.
    /// </exception>
    protected override BlockSyntax GetterBlockSyntax(BitFieldPropertyInfoRequest bitFieldPropertyInfoRequest)
    {
        if(bitFieldPropertyInfoRequest.PropertyType.SpecialType == SpecialType.System_Boolean)
        {
            ThrowHelper.ThrowUnreachableException("Boolean type is not supported for ExtendedBitFieldAttribute.");
        }

        var propertyTypeSyntax = bitFieldPropertyInfoRequest.PropertyDeclarationSyntax.Type;
        var length = bitFieldPropertyInfoRequest.BitsSpan.Length;

        // Collect statements for the getter block
        using var statementsRented = ListsPool.Rent<StatementSyntax>();
        var statements = statementsRented.List;

        // For multi-bit properties, clamp the value to ensure it fits within the specified bit length

        // 1) Define a constant maxValue_: const {PropertyType} maxValue_ = (1 << length) - 1;
        var maxValueDecloration = LocalDeclarationStatement(
            VariableDeclaration(
                propertyTypeSyntax,
                SingletonSeparatedList(
                    VariableDeclarator("maxValue_")
                        .WithInitializer(
                            EqualsValueClause(
                                BinaryExpression(
                                    SyntaxKind.SubtractExpression,
                                    ParenthesizedExpression(
                                        BinaryExpression(
                                            SyntaxKind.LeftShiftExpression,
                                            LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(1)),
                                            LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(length))
                                        )
                                    ),
                                    LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(1))
                                )
                            )
                        )
                )
            )
        )
        .WithModifiers(
            TokenList(Token(SyntaxKind.ConstKeyword))
        );

        statements.Add(maxValueDecloration);

        // Build the expression that extracts the value from the bitfield
        var getExpression = GetBitwiseExpression(bitFieldPropertyInfoRequest);

        var valueStatement = LocalDeclarationStatement(
            VariableDeclaration(
                propertyTypeSyntax,
                SingletonSeparatedList(
                    VariableDeclarator("value_")
                        .WithInitializer(
                            EqualsValueClause(
                                getExpression
                            )
                        )
                )
            )
        );
        statements.Add(valueStatement);

        // 2) If(value == maxValue_)
        var ifStatement = IfStatement(
            BinaryExpression(
                SyntaxKind.EqualsExpression,
                IdentifierName("value_"),
                IdentifierName("maxValue_")
            ),
            Block(
                SingletonList<StatementSyntax>(
                    ReturnStatement(
                        GetterLargeValueExpression(bitFieldPropertyInfoRequest)
                    )
                )
            )
        );
        statements.Add(ifStatement);

        // 3) return value;
        statements.Add(
            ReturnStatement(
                IdentifierName("value_")
            )
        );

        return Block(statements);
    }

    /// <summary>
    /// Generates the syntax for handling large values in the getter for extended properties.
    /// </summary>
    /// <param name="bitFieldPropertyInfoRequest">
    /// The request containing details about the property and its associated large value handler.
    /// </param>
    /// <returns>
    /// An <see cref="ExpressionSyntax"/> representing the large value handling logic.
    /// </returns>
    /// <exception cref="UnreachableException">
    /// Thrown if the symbol used for the large value handler is not a property, field, or method.
    /// </exception>
    private ExpressionSyntax GetterLargeValueExpression(BitFieldPropertyInfoRequest bitFieldPropertyInfoRequest)
    {
        var extendedParsed = Unsafe.As<ParsedExtendedBitFiledAttribute>(bitFieldPropertyInfoRequest.AttributeParsedResult);

        var symbolGetterLargeSizeValue = extendedParsed.SymbolGetterLargeSizeValue;


        if(symbolGetterLargeSizeValue is IPropertySymbol propertySymbol)
        {
            return IdentifierName(propertySymbol.Name);
        }

        if(symbolGetterLargeSizeValue is IFieldSymbol fieldSymbol)
        {
            return IdentifierName(fieldSymbol.Name);
        }

        if(symbolGetterLargeSizeValue is IMethodSymbol methodSymbol)
        {
            return InvocationExpression(
                IdentifierName(methodSymbol.Name)
            );
        }

        return ThrowHelper.ThrowUnreachableException<ExpressionSyntax>("SymbolGetterLargeSizeValue is not supported.");
    }
}
