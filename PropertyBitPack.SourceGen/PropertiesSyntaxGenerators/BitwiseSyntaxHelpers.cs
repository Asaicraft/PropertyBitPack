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
