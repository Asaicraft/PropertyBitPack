using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Microsoft.CodeAnalysis.CSharp;

namespace PropertyBitPack.SourceGen.PropertiesSyntaxGenerators;

/// <summary>
/// Provides helper methods for constructing and manipulating bitwise expressions.
/// </summary>
internal static class BitwiseSyntaxHelpers
{

    /// <summary>
    /// Generates a setter accessor syntax with the specified modifiers and body.
    /// </summary>
    /// <param name="modifiers">The modifiers for the setter accessor.</param>
    /// <param name="body">The body of the setter accessor.</param>
    /// <returns>An <see cref="AccessorDeclarationSyntax"/> for the setter.</returns>
    public static AccessorDeclarationSyntax Setter(SyntaxTokenList modifiers, BlockSyntax body)
    {
        return AccessorDeclaration(
            SyntaxKind.SetAccessorDeclaration,
            List<AttributeListSyntax>(),
            modifiers,
            body
        );
    }


    /// <summary>
    /// Generates a getter accessor syntax with the specified body.
    /// </summary>
    /// <param name="body">The body of the getter accessor.</param>
    /// <returns>An <see cref="AccessorDeclarationSyntax"/> for the getter.</returns>
    public static AccessorDeclarationSyntax Getter(BlockSyntax body)
    {
        return AccessorDeclaration(
            SyntaxKind.GetAccessorDeclaration,
            List<AttributeListSyntax>(),
            TokenList(),
            body
        );
    }

    /// <summary>
    /// Generates an initializer accessor syntax with the specified modifiers and body.
    /// </summary>
    /// <param name="modifiers">The modifiers for the initializer accessor.</param>
    /// <param name="body">The body of the initializer accessor.</param>
    /// <returns>An <see cref="AccessorDeclarationSyntax"/> for the initializer.</returns>
    public static AccessorDeclarationSyntax Initter(SyntaxTokenList modifiers, BlockSyntax body)
    {
        return AccessorDeclaration(
            SyntaxKind.InitAccessorDeclaration,
            List<AttributeListSyntax>(),
            modifiers,
            body
        );
    }

    /// <summary>
    /// Builds a local declaration of the form:
    /// <c>const {FieldType} {variableName} = ((1 &lt;&lt; length) - 1);</c>.
    /// </summary>
    /// <param name="variableName">The name of the const variable (e.g. "maxValue_").</param>
    /// <param name="fieldType">The <see cref="SpecialType"/> for the variable type.</param>
    /// <param name="length">The number of bits to set in the mask.</param>
    /// <returns>A <see cref="LocalDeclarationStatementSyntax"/> representing the const declaration.</returns>
    public static LocalDeclarationStatementSyntax BuildConstMaskDeclaration(
        string variableName,
        SpecialType fieldType,
        byte length)
    {
        // const T variableName = ((1 << length) - 1);
        var fieldTypeSyntax = GetTypeSyntaxFromSpecialType(fieldType);

        var maskLiteral = BuildMaskLiteral(fieldType, length); // ((1 << length) - 1)

        return LocalDeclarationStatement(
            VariableDeclaration(
                fieldTypeSyntax,
                SingletonSeparatedList(
                    VariableDeclarator(variableName)
                        .WithInitializer(
                            EqualsValueClause(maskLiteral)
                        )
                )
            )
        )
        .WithModifiers(
            TokenList(Token(SyntaxKind.ConstKeyword))
        );
    }

    /// <summary>
    /// Builds an invocation of Math.Min, e.g. <c>Math.Min(left, right)</c>.
    /// </summary>
    /// <param name="left">The left-hand argument to Math.Min.</param>
    /// <param name="right">The right-hand argument to Math.Min.</param>
    /// <returns>An <see cref="InvocationExpressionSyntax"/> representing the Math.Min call.</returns>
    public static InvocationExpressionSyntax BuildMathMin(ExpressionSyntax left, ExpressionSyntax right)
    {
        return InvocationExpression(
            MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                IdentifierName("Math"),
                IdentifierName("Min")
            ),
            ArgumentList(
                SeparatedList<ArgumentSyntax>(
                    new SyntaxNodeOrToken[]
                    {
                        Argument(left),
                        Token(SyntaxKind.CommaToken),
                        Argument(right)
                    }
                )
            )
        );
    }

    /// <summary>
    /// Builds a local declaration of the form:
    /// <c>var clamped_ = Math.Min((T)valueExpr, IdentifierName(maxValueVar));</c>.
    /// </summary>
    /// <param name="variableName">The name of the local variable (e.g. "clamped_").</param>
    /// <param name="fieldType">The <see cref="SpecialType"/> of the target numeric type.</param>
    /// <param name="valueExpr">Expression from which we clamp (e.g. "value").</param>
    /// <param name="maxValueVar">The name of the variable holding the max mask (e.g. "maxValue_").</param>
    /// <returns>A <see cref="LocalDeclarationStatementSyntax"/> declaring the clamped variable.</returns>
    public static LocalDeclarationStatementSyntax BuildClampedVarDeclaration(
        string variableName,
        SpecialType fieldType,
        ExpressionSyntax valueExpr,
        string maxValueVar)
    {
        var fieldTypeSyntax = GetTypeSyntaxFromSpecialType(fieldType);

        // (T)valueExpr
        var castedValue = CastExpression(
            fieldTypeSyntax,
            ParenthesizedExpression(valueExpr)
        );

        // Math.Min((T)value, maxValue_)
        var minCall = BuildMathMin(castedValue, IdentifierName(maxValueVar));

        // var clamped_ = Math.Min(...);
        return LocalDeclarationStatement(
            VariableDeclaration(
                IdentifierName("var"),
                SingletonSeparatedList(
                    VariableDeclarator(variableName)
                        .WithInitializer(EqualsValueClause(minCall))
                )
            )
        );
    }

    /// <summary>
    /// Builds the expression for <c>1 &lt;&lt; length</c> or <c>1UL &lt;&lt; length</c>, depending on <paramref name="fieldType"/>.
    /// </summary>
    /// <param name="fieldType">The <see cref="SpecialType"/> representing the numeric type.</param>
    /// <param name="length">The number of bits to shift.</param>
    /// <returns>An <see cref="ExpressionSyntax"/> representing the shift operation.</returns>
    public static ExpressionSyntax BuildShiftLiteral(SpecialType fieldType, byte length)
    {
        var oneLiteral = LiteralWithSpecialType(1, fieldType);

        return BinaryExpression(
            SyntaxKind.LeftShiftExpression,
            oneLiteral,
            NumericLiteral(length)
        );
    }

    /// <summary>
    /// Creates an expression for <c>(1 &lt;&lt; length) - 1</c>, effectively generating a bitmask of <paramref name="length"/> bits.
    /// </summary>
    /// <param name="fieldType">The numeric type to use for the mask.</param>
    /// <param name="length">The number of bits to set in the mask.</param>
    /// <returns>An <see cref="ExpressionSyntax"/> representing the bitmask operation.</returns>
    public static ExpressionSyntax BuildMaskLiteral(SpecialType fieldType, byte length)
    {
        // (1 << length)
        var shiftExpr = BuildShiftLiteral(fieldType, length);

        // ((1 << length) - 1)
        return BinaryExpression(
            SyntaxKind.SubtractExpression,
            ParenthesizedExpression(shiftExpr),
            NumericLiteral(1)
        );
    }

    /// <summary>
    /// Constructs an expression for shifting a bitmask left by a specified start position.
    /// </summary>
    /// <param name="fieldType">The numeric type to use for the mask.</param>
    /// <param name="length">The number of bits in the mask.</param>
    /// <param name="start">The number of bits to shift the mask.</param>
    /// <returns>An <see cref="ExpressionSyntax"/> representing the shifted bitmask.</returns>
    public static ExpressionSyntax BuildMaskShifted(SpecialType fieldType, byte length, byte start)
    {
        var maskLiteral = BuildMaskLiteral(fieldType, length);

        // (((1 << length) - 1) << start)
        return BinaryExpression(
            SyntaxKind.LeftShiftExpression,
            ParenthesizedExpression(maskLiteral),
            NumericLiteral(start)
        );
    }

    /// <summary>
    /// Builds an expression for <c>(fieldName &amp; ~maskShifted)</c>.
    /// </summary>
    /// <param name="fieldNameExpr">An expression representing the field name.</param>
    /// <param name="maskShiftedExpr">The expression representing the shifted mask.</param>
    /// <returns>An <see cref="ExpressionSyntax"/> for the bitwise AND operation with the inverted mask.</returns>
    public static ExpressionSyntax BuildLeftAndMask(ExpressionSyntax fieldNameExpr, ExpressionSyntax maskShiftedExpr)
    {
        // ~maskShifted
        var notMask = PrefixUnaryExpression(
            SyntaxKind.BitwiseNotExpression,
            ParenthesizedExpression(maskShiftedExpr)
        );

        // (fieldName & ~(((1 << length) - 1) << start))
        return BinaryExpression(
            SyntaxKind.BitwiseAndExpression,
            fieldNameExpr,
            notMask
        );
    }

    /// <summary>
    /// Builds an expression for <c>((valueExpr &amp; maskLiteral) &lt;&lt; start)</c>.
    /// </summary>
    /// <param name="valueExpr">The expression representing the value to be masked and shifted.</param>
    /// <param name="fieldType">The numeric type used for creating the mask.</param>
    /// <param name="length">The number of bits to include in the mask.</param>
    /// <param name="start">The number of bits to shift the masked value.</param>
    /// <returns>An <see cref="ExpressionSyntax"/> representing the masked and shifted value.</returns>
    public static ExpressionSyntax BuildValueAndShift(
        ExpressionSyntax valueExpr,
        SpecialType fieldType,
        byte length,
        byte start)
    {
        // ( (1 << length) - 1 )
        var maskLiteral = BuildMaskLiteral(fieldType, length);

        // (valueExpr & maskLiteral)
        var valueAndMask = BinaryExpression(
            SyntaxKind.BitwiseAndExpression,
            valueExpr,
            ParenthesizedExpression(maskLiteral)
        );

        // ((valueExpr & maskLiteral) << start)
        return BinaryExpression(
            SyntaxKind.LeftShiftExpression,
            ParenthesizedExpression(valueAndMask),
            NumericLiteral(start)
        );
    }

    /// <summary>
    /// Builds an expression for <c>(leftExpr | rightExpr)</c>.
    /// </summary>
    /// <param name="leftExpr">The left-hand side of the bitwise OR operation.</param>
    /// <param name="rightExpr">The right-hand side of the bitwise OR operation.</param>
    /// <returns>An <see cref="ExpressionSyntax"/> representing the bitwise OR operation.</returns>
    public static ExpressionSyntax BuildOr(ExpressionSyntax leftExpr, ExpressionSyntax rightExpr)
    {
        return BinaryExpression(
            SyntaxKind.BitwiseOrExpression,
            ParenthesizedExpression(leftExpr),
            ParenthesizedExpression(rightExpr)
        );
    }

    /// <summary>
    /// Builds a simple assignment of the form <c>fieldName = (castType)(someExpr)</c>.
    /// </summary>
    /// <param name="fieldName">The name of the field to assign to.</param>
    /// <param name="someExpr">The expression to assign.</param>
    /// <param name="fieldType">The <see cref="SpecialType"/> of the field.</param>
    /// <returns>An <see cref="ExpressionSyntax"/> representing the assignment operation.</returns>
    public static ExpressionSyntax BuildAssignment(
        string fieldName,
        ExpressionSyntax someExpr,
        SpecialType fieldType)
    {
        var castTypeSyntax = GetTypeSyntaxFromSpecialType(fieldType);

        return AssignmentExpression(
            SyntaxKind.SimpleAssignmentExpression,
            IdentifierName(fieldName),
            CastExpression(
                castTypeSyntax,
                ParenthesizedExpression(someExpr)
            )
        );
    }

    /// <summary>
    /// Builds an expression for right shifting the <paramref name="leftExpr"/> by <paramref name="shiftAmount"/>.
    /// </summary>
    /// <param name="leftExpr">The expression to be shifted.</param>
    /// <param name="shiftAmount">The number of bits to shift right.</param>
    /// <returns>An <see cref="ExpressionSyntax"/> for the right shift operation.</returns>
    public static ExpressionSyntax BuildRightShift(ExpressionSyntax leftExpr, byte shiftAmount)
    {
        return BinaryExpression(
            SyntaxKind.RightShiftExpression,
            leftExpr,
            NumericLiteral(shiftAmount)
        );
    }

    /// <summary>
    /// Builds an expression for <c>(expr &amp; maskExpr)</c>.
    /// </summary>
    /// <param name="leftExpr">The left-hand expression.</param>
    /// <param name="maskExpr">The right-hand mask expression.</param>
    /// <returns>An <see cref="ExpressionSyntax"/> representing <c>(leftExpr &amp; maskExpr)</c>.</returns>
    public static ExpressionSyntax BuildAnd(ExpressionSyntax leftExpr, ExpressionSyntax maskExpr)
    {
        return BinaryExpression(
            SyntaxKind.BitwiseAndExpression,
            ParenthesizedExpression(leftExpr),
            ParenthesizedExpression(maskExpr)
        );
    }

    /// <summary>
    /// Builds an expression for extracting bits: <c>(expr &amp; ((1 &lt;&lt; length) - 1))</c>.
    /// </summary>
    /// <param name="expr">The expression to be masked.</param>
    /// <param name="fieldType">The numeric type used for creating the mask.</param>
    /// <param name="length">The number of bits to include in the mask.</param>
    /// <returns>An <see cref="ExpressionSyntax"/> representing <c>(expr &amp; mask)</c>.</returns>
    public static ExpressionSyntax BuildMaskExtract(ExpressionSyntax expr, SpecialType fieldType, byte length)
    {
        var maskLiteral = BuildMaskLiteral(fieldType, length);
        return BuildAnd(expr, maskLiteral);
    }

    /// <summary>
    /// Builds a cast expression to the type represented by <paramref name="propertyTypeSymbol"/>.
    /// </summary>
    /// <param name="propertyTypeSymbol">The symbol of the target type.</param>
    /// <param name="expression">The expression to be cast.</param>
    /// <returns>An <see cref="ExpressionSyntax"/> representing the cast.</returns>
    public static ExpressionSyntax BuildCast(ITypeSymbol propertyTypeSymbol, ExpressionSyntax expression)
    {
        // We can convert the symbol to a string to create an IdentifierName,
        // or you might prefer a more sophisticated approach for known primitives.
        var propertyTypeName = propertyTypeSymbol.ToDisplayString();

        return CastExpression(
            IdentifierName(propertyTypeName),
            ParenthesizedExpression(expression)
        );
    }

    /// <summary>
    /// Retrieves the corresponding <see cref="TypeSyntax"/> for a given <see cref="SpecialType"/>.
    /// </summary>
    /// <param name="specialType">A <see cref="SpecialType"/> indicating the target type.</param>
    /// <returns>A <see cref="TypeSyntax"/> representing the specified special type.</returns>
    /// <exception cref="NotSupportedException">Thrown when the provided special type is not supported.</exception>
    public static TypeSyntax GetTypeSyntaxFromSpecialType(SpecialType specialType) => specialType switch
    {
        SpecialType.System_Boolean => PredefinedType(Token(SyntaxKind.BoolKeyword)),
        SpecialType.System_Byte => PredefinedType(Token(SyntaxKind.ByteKeyword)),
        SpecialType.System_SByte => PredefinedType(Token(SyntaxKind.SByteKeyword)),
        SpecialType.System_Int16 => PredefinedType(Token(SyntaxKind.ShortKeyword)),
        SpecialType.System_UInt16 => PredefinedType(Token(SyntaxKind.UShortKeyword)),
        SpecialType.System_Int32 => PredefinedType(Token(SyntaxKind.IntKeyword)),
        SpecialType.System_UInt32 => PredefinedType(Token(SyntaxKind.UIntKeyword)),
        SpecialType.System_Int64 => PredefinedType(Token(SyntaxKind.LongKeyword)),
        SpecialType.System_UInt64 => PredefinedType(Token(SyntaxKind.ULongKeyword)),
        _ => throw new NotSupportedException()
    };

    /// <summary>
    /// Converts the specified <see cref="SpecialType"/> to its signed variant 
    /// and returns the corresponding <see cref="TypeSyntax"/>.
    /// </summary>
    /// <param name="specialType">A <see cref="SpecialType"/> to convert.</param>
    /// <returns>A <see cref="TypeSyntax"/> that represents the signed variant of the given type.</returns>
    public static TypeSyntax ToSignedVariantSyntax(SpecialType specialType) => GetTypeSyntaxFromSpecialType(ToSignedVariant(specialType));

    /// <summary>
    /// Converts an unsigned <see cref="SpecialType"/> to its signed equivalent.
    /// </summary>
    /// <param name="specialType">The original <see cref="SpecialType"/>.</param>
    /// <returns>The signed variant of the provided <see cref="SpecialType"/>, or the original if already signed.</returns>
    public static SpecialType ToSignedVariant(SpecialType specialType) => specialType switch
    {
        SpecialType.System_Byte => SpecialType.System_SByte,
        SpecialType.System_UInt16 => SpecialType.System_Int16,
        SpecialType.System_UInt32 => SpecialType.System_Int32,
        SpecialType.System_UInt64 => SpecialType.System_Int64,
        _ => specialType
    };

    /// <summary>
    /// Creates a <see cref="LiteralExpressionSyntax"/> for a numeric literal
    /// based on the specified <see cref="SpecialType"/>.
    /// </summary>
    /// <param name="value">A decimal value used to produce the literal.</param>
    /// <param name="specialType">The <see cref="SpecialType"/> determining how to cast the literal.</param>
    /// <returns>A <see cref="LiteralExpressionSyntax"/> representing the correctly typed numeric literal.</returns>
    /// <exception cref="NotSupportedException">Thrown when the provided special type is not supported.</exception>
    public static LiteralExpressionSyntax LiteralWithSpecialType(decimal value, SpecialType specialType) => specialType switch
    {
        SpecialType.System_Byte
        or SpecialType.System_SByte
        or SpecialType.System_Int16
        or SpecialType.System_UInt16
        or SpecialType.System_Int32
        => NumericLiteral((int)value),
        SpecialType.System_UInt32 => NumericLiteral((uint)value),
        SpecialType.System_Int64 => NumericLiteral((long)value),
        SpecialType.System_UInt64 => NumericLiteral((ulong)value),
        SpecialType.System_Decimal => NumericLiteral((decimal)value),
        _ => throw new NotSupportedException()
    };

    /// <summary>
    /// Creates a <see cref="LiteralExpressionSyntax"/> from an <see cref="int"/> value.
    /// </summary>
    /// <param name="value">The integer value for the literal.</param>
    /// <returns>A <see cref="LiteralExpressionSyntax"/> representing the literal.</returns>
    public static LiteralExpressionSyntax NumericLiteral(int value)
        => LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(value));

    /// <summary>
    /// Creates a <see cref="LiteralExpressionSyntax"/> from a <see cref="uint"/> value.
    /// </summary>
    /// <param name="value">The unsigned integer value for the literal.</param>
    /// <returns>A <see cref="LiteralExpressionSyntax"/> representing the literal.</returns>
    public static LiteralExpressionSyntax NumericLiteral(uint value)
        => LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(value));

    /// <summary>
    /// Creates a <see cref="LiteralExpressionSyntax"/> from a <see cref="long"/> value.
    /// </summary>
    /// <param name="value">The long value for the literal.</param>
    /// <returns>A <see cref="LiteralExpressionSyntax"/> representing the literal.</returns>
    public static LiteralExpressionSyntax NumericLiteral(long value)
        => LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(value));

    /// <summary>
    /// Creates a <see cref="LiteralExpressionSyntax"/> from a <see cref="ulong"/> value.
    /// </summary>
    /// <param name="value">The unsigned long value for the literal.</param>
    /// <returns>A <see cref="LiteralExpressionSyntax"/> representing the literal.</returns>
    public static LiteralExpressionSyntax NumericLiteral(ulong value)
        => LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(value));

    /// <summary>
    /// Creates a <see cref="LiteralExpressionSyntax"/> from a <see cref="decimal"/> value.
    /// </summary>
    /// <param name="value">The decimal value for the literal.</param>
    /// <returns>A <see cref="LiteralExpressionSyntax"/> representing the literal.</returns>
    public static LiteralExpressionSyntax NumericLiteral(decimal value)
        => LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(value));
}

