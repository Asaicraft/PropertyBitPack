using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PropertyBitPack.SourceGen.Models;
using System;
using System.Collections.Generic;
using System.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace PropertyBitPack.SourceGen;
public static class SyntaxGenerator
{
    public static CompilationUnitSyntax GenerateFieldAndBindedProperties(PackedFieldStorage packedFieldStorage)
    {
        var owner = packedFieldStorage.PropertiesWhichDataStored[0].Owner;

        var namespaceSyntax = NamespaceDeclaration(ParseName(owner.ContainingNamespace.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)));

        var classSyntax = ClassDeclaration(
                owner.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)
            )
            .WithModifiers(
                TokenList(Token(SyntaxKind.PartialKeyword))
            );

        var fieldDeclaration = GenerateFieldDeclaration(packedFieldStorage);
        classSyntax = classSyntax.AddMembers(fieldDeclaration);

        var properties = GenerateProperties(packedFieldStorage);

        classSyntax = classSyntax.AddMembers([.. properties]);

        namespaceSyntax = namespaceSyntax.AddMembers(classSyntax);

        var compilationUnit = CompilationUnit().AddMembers(namespaceSyntax);

        return compilationUnit;
    }

    public static FieldDeclarationSyntax GenerateFieldDeclaration(PackedFieldStorage packedFieldStorage)
    {
        var fieldDeclaration = FieldDeclaration(
            VariableDeclaration(
                packedFieldStorage.TypeSyntax,
                SeparatedList([
                    VariableDeclarator(
                        Identifier(packedFieldStorage.FieldName)
                    )
                ])
            )
        );

        return fieldDeclaration;
    }

    public static SeparatedSyntaxList<MemberDeclarationSyntax> GenerateProperties(PackedFieldStorage packedFieldStorage)
    {
        if (packedFieldStorage.PropertiesWhichDataStored[0].AttributeType == BitsMappingAttributeType.BitField)
        {
            return GenerateBitFieldProperties(packedFieldStorage);
        }
        else
        {
            return GenerateExtendedBitFieldProperties(packedFieldStorage);
        }
    }

    private static SeparatedSyntaxList<MemberDeclarationSyntax> GenerateBitFieldProperties(PackedFieldStorage packedFieldStorage)
    {
        var fieldName = packedFieldStorage.FieldName;
        var properties = packedFieldStorage.PropertiesWhichDataStored;
        var members = new System.Collections.Generic.List<MemberDeclarationSyntax>(properties.Length);

        // Определяем смещения
        int offset = 0;
        foreach (var prop in properties)
        {
            var length = prop.BitsCount;
            var start = offset;
            offset += length;

            var bitsSpan = new BitsSpan(start, length);

            var propertyDecl = CreateBitFieldPropertyDeclarationSyntax(fieldName, prop, bitsSpan, packedFieldStorage.TypeSyntax);
            members.Add(propertyDecl);
        }

        return SeparatedList(members);
    }


    private static SeparatedSyntaxList<MemberDeclarationSyntax> GenerateExtendedBitFieldProperties(PackedFieldStorage packedFieldStorage)
    {
        var fieldName = packedFieldStorage.FieldName;
        var properties = packedFieldStorage.PropertiesWhichDataStored;
        var members = new System.Collections.Generic.List<MemberDeclarationSyntax>(properties.Length);

        // Определяем смещения
        int offset = 0;
        foreach (var prop in properties)
        {
            var length = prop.BitsCount;
            var start = offset;
            offset += length;

            var bitsSpan = new BitsSpan(start, length);

            var propertyDecl = CreateExtendedBitFieldPropertyDeclarationSyntax(fieldName, prop, bitsSpan, packedFieldStorage.TypeSyntax);
            members.Add(propertyDecl);
        }

        return SeparatedList(members);
    }

    private static PropertyDeclarationSyntax CreateBitFieldPropertyDeclarationSyntax(
        string fieldName,
        PropertyToBitInfo propertyToBitInfo,
        BitsSpan bitsSpan,
        TypeSyntax backingFieldType)
    {
        var propertyTypeName = propertyToBitInfo.PropertyType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var propertyName = propertyToBitInfo.PropertySymbol.Name;
        var start = bitsSpan.Start;
        var length = bitsSpan.Length;

        var isBoolean = propertyToBitInfo.PropertyType.SpecialType == SpecialType.System_Boolean && length == 1;

        var fieldIdentifier = IdentifierName(fieldName);
        var valueIdentifier = IdentifierName("value");
        var propertyTypeSyntax = ParseTypeName(propertyTypeName);

        var startLiteral = NumericLiteralInt(start);
        var lengthLiteral = NumericLiteralInt(length);

        // mask = ( (1U << length) - 1U );
        var maskDeclaration = LocalVar(
            "mask",
            PredefinedTypeUInt(),
            Parenthesized(
                BinaryExpr(
                    Parenthesized(BinaryExpr(NumericLiteral(1U), SyntaxKind.LeftShiftExpression, lengthLiteral)),
                    SyntaxKind.SubtractExpression,
                    NumericLiteral(1U))));

        // Getter
        AccessorDeclarationSyntax getter;
        if (isBoolean)
        {
            // return ((fieldName >> start) & mask) != 0U;
            var fieldShifted = Parenthesized(BinaryExpr(fieldIdentifier, SyntaxKind.RightShiftExpression, startLiteral));
            var fieldAndMask = Parenthesized(BinaryExpr(fieldShifted, SyntaxKind.BitwiseAndExpression, IdentifierName("mask")));
            var comparison = BinaryExpr(fieldAndMask, SyntaxKind.NotEqualsExpression, NumericLiteral(0U));
            getter = AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                .WithBody(Block(maskDeclaration, ReturnStatement(comparison)));
        }
        else
        {
            // return (PropertyType)((fieldName >> start) & mask);
            var fieldShifted = Parenthesized(BinaryExpr(fieldIdentifier, SyntaxKind.RightShiftExpression, startLiteral));
            var fieldAndMask = Parenthesized(BinaryExpr(fieldShifted, SyntaxKind.BitwiseAndExpression, IdentifierName("mask")));
            var castToProperty = CastExpression(propertyTypeSyntax, fieldAndMask);
            getter = AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                .WithBody(Block(maskDeclaration, ReturnStatement(castToProperty)));
        }

        // Setter/init
        AccessorDeclarationSyntax setterOrInitter;
        if (isBoolean)
        {
            // if (value) fieldName |= (1U << start); else fieldName &= ~(1U << start);
            var oneShifted = Parenthesized(BinaryExpr(NumericLiteral(1U), SyntaxKind.LeftShiftExpression, startLiteral));
            var ifStmt = IfStatement(
                valueIdentifier,
                Block(ExpressionStatement(AssignmentExpression(SyntaxKind.OrAssignmentExpression, fieldIdentifier, oneShifted))),
                ElseClause(Block(ExpressionStatement(
                    AssignmentExpression(SyntaxKind.AndAssignmentExpression, fieldIdentifier,
                        PrefixUnaryExpression(SyntaxKind.BitwiseNotExpression, oneShifted))))));

            setterOrInitter = AccessorDeclaration(propertyToBitInfo.IsInit ? SyntaxKind.InitAccessorDeclaration : SyntaxKind.SetAccessorDeclaration)
                .WithBody(Block(ifStmt));

            if (propertyToBitInfo.SetterOrInitModifiers.Count != 0)
            {
                setterOrInitter = setterOrInitter.WithModifiers(propertyToBitInfo.SetterOrInitModifiers);
            }
        }
        else
        {
            // fieldName = (fieldName & ~(mask << start)) | (((uint)value & mask) << start);
            var maskRef = IdentifierName("mask");
            var maskShifted = Parenthesized(BinaryExpr(maskRef, SyntaxKind.LeftShiftExpression, startLiteral));
            var notMaskShifted = PrefixUnaryExpression(SyntaxKind.BitwiseNotExpression, maskShifted);

            var uintValue = CastExpression(PredefinedTypeUInt(), valueIdentifier);
            var valueAndMask = Parenthesized(BinaryExpr(uintValue, SyntaxKind.BitwiseAndExpression, maskRef));
            var valueShifted = Parenthesized(BinaryExpr(valueAndMask, SyntaxKind.LeftShiftExpression, startLiteral));

            var fieldCleared = Parenthesized(BinaryExpr(fieldIdentifier, SyntaxKind.BitwiseAndExpression, notMaskShifted));
            var fieldFinal = Parenthesized(BinaryExpr(fieldCleared, SyntaxKind.BitwiseOrExpression, valueShifted));

            var assignStmt = ExpressionStatement(
                AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, fieldIdentifier, fieldFinal));

            setterOrInitter = AccessorDeclaration(propertyToBitInfo.IsInit ? SyntaxKind.InitAccessorDeclaration : SyntaxKind.SetAccessorDeclaration)
                .WithBody(Block(maskDeclaration, assignStmt));

            if (propertyToBitInfo.SetterOrInitModifiers.Count != 0)
            {
                setterOrInitter = setterOrInitter.WithModifiers(propertyToBitInfo.SetterOrInitModifiers);
            }
        }

        var propertyDeclaration = PropertyDeclaration(propertyTypeSyntax, Identifier(propertyName))
            .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.PartialKeyword))
            .AddAccessorListAccessors(getter, setterOrInitter);

        return propertyDeclaration;
    }

    private static PropertyDeclarationSyntax CreateExtendedBitFieldPropertyDeclarationSyntax(
        string fieldName,
        PropertyToBitInfo propertyToBitInfo,
        BitsSpan bitsSpan,
        TypeSyntax backingFieldType)
    {
        // Логика сходна с BitField, но в геттере и сеттере учитываем "большое значение"
        var propertyTypeName = propertyToBitInfo.PropertyType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var propertyName = propertyToBitInfo.PropertySymbol.Name;
        var start = bitsSpan.Start;
        var length = bitsSpan.Length;

        var fieldIdentifier = IdentifierName(fieldName);
        var valueIdentifier = IdentifierName("value");
        var propertyTypeSyntax = ParseTypeName(propertyTypeName);

        var startLiteral = NumericLiteralInt(start);
        var lengthLiteral = NumericLiteralInt(length);

        // mask = ((1U << length) - 1U);
        var maskDeclaration = LocalVar(
            "mask",
            PredefinedTypeUInt(),
            Parenthesized(
                BinaryExpr(
                    Parenthesized(BinaryExpr(NumericLiteral(1U), SyntaxKind.LeftShiftExpression, lengthLiteral)),
                    SyntaxKind.SubtractExpression,
                    NumericLiteral(1U))));

        // maxValue = (int)((1 << length) - 1);
        var maxValueDeclaration = LocalVar(
            "maxValue",
            PredefinedType(Token(SyntaxKind.IntKeyword)),
            Parenthesized(
                BinaryExpr(
                    Parenthesized(BinaryExpr(NumericLiteral(1), SyntaxKind.LeftShiftExpression, lengthLiteral)),
                    SyntaxKind.SubtractExpression,
                    NumericLiteral(1))));

        // Геттер для ExtendedBitField:
        // int extractedValue = (int)((fieldName >> start) & mask);
        // return extractedValue == maxValue ? GetLargeValue() : extractedValue;

        var fieldShifted = Parenthesized(BinaryExpr(fieldIdentifier, SyntaxKind.RightShiftExpression, startLiteral));
        var fieldAndMask = Parenthesized(BinaryExpr(fieldShifted, SyntaxKind.BitwiseAndExpression, IdentifierName("mask")));

        var extractedValueDecl = LocalVar(
            "extractedValue",
            PredefinedType(Token(SyntaxKind.IntKeyword)),
            CastExpression(PredefinedType(Token(SyntaxKind.IntKeyword)), fieldAndMask));

        // Вызов GetterLargeSizeValueSymbol. Предположим, что это метод без параметров.
        var largeValueCall = IdentifierName(propertyToBitInfo.GetterLargeSizeValueSymbol!.Name);
        var invocation = InvocationExpression(largeValueCall);

        var condition = BinaryExpr(IdentifierName("extractedValue"), SyntaxKind.EqualsExpression, IdentifierName("maxValue"));
        var ternary = ConditionalExpression(condition, invocation, IdentifierName("extractedValue"));
        var getter = AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
            .WithBody(Block(maskDeclaration, maxValueDeclaration, extractedValueDecl, ReturnStatement(ternary)));

        // Сеттер для ExtendedBitField:
        // if (value > maxValue)
        // {
        //   // sentinel
        //   fieldName = (fieldName & ~(mask << start)) | ((uint)maxValue << start);
        // }
        // else
        // {
        //   fieldName = (fieldName & ~(mask << start)) | (((uint)value & mask) << start);
        // }

        var maskRef = IdentifierName("mask");
        var maskShifted = Parenthesized(BinaryExpr(maskRef, SyntaxKind.LeftShiftExpression, startLiteral));
        var notMaskShifted = PrefixUnaryExpression(SyntaxKind.BitwiseNotExpression, maskShifted);

        var uintValue = CastExpression(PredefinedTypeUInt(), valueIdentifier);
        var valueAndMask = Parenthesized(BinaryExpr(uintValue, SyntaxKind.BitwiseAndExpression, maskRef));
        var valueShifted = Parenthesized(BinaryExpr(valueAndMask, SyntaxKind.LeftShiftExpression, startLiteral));

        var fieldCleared = Parenthesized(BinaryExpr(fieldIdentifier, SyntaxKind.BitwiseAndExpression, notMaskShifted));

        // sentinel
        var maxValueRef = IdentifierName("maxValue");
        var maxValueUInt = CastExpression(PredefinedTypeUInt(), maxValueRef);
        var sentinelShifted = Parenthesized(BinaryExpr(maxValueUInt, SyntaxKind.LeftShiftExpression, startLiteral));
        var sentinelFieldFinal = Parenthesized(BinaryExpr(fieldCleared, SyntaxKind.BitwiseOrExpression, sentinelShifted));

        var normalFieldFinal = Parenthesized(BinaryExpr(fieldCleared, SyntaxKind.BitwiseOrExpression, valueShifted));

        var ifSetStmt = IfStatement(
            BinaryExpr(valueIdentifier, SyntaxKind.GreaterThanExpression, IdentifierName("maxValue")),
            Block(ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, fieldIdentifier, sentinelFieldFinal))),
            ElseClause(Block(ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, fieldIdentifier, normalFieldFinal)))));

        var setter = AccessorDeclaration(propertyToBitInfo.IsInit ? SyntaxKind.InitAccessorDeclaration : SyntaxKind.SetAccessorDeclaration)
            .WithBody(Block(maskDeclaration, maxValueDeclaration, ifSetStmt));

        if (propertyToBitInfo.SetterOrInitModifiers.Count != 0)
        {
            setter = setter.WithModifiers(propertyToBitInfo.SetterOrInitModifiers);
        }

        var propertyDeclaration = PropertyDeclaration(propertyTypeSyntax, Identifier(propertyName))
            .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.PartialKeyword))
            .AddAccessorListAccessors(getter, setter);

        return propertyDeclaration;
    }

    private readonly struct BitsSpan(int start, int length)
    {
        public readonly int Start => start;
        public readonly int Length => length;
    }

    private static LiteralExpressionSyntax NumericLiteral(uint value)
        => LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(value));

    private static LiteralExpressionSyntax NumericLiteralInt(int value)
        => LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(value));

    private static BinaryExpressionSyntax BinaryExpr(ExpressionSyntax left, SyntaxKind kind, ExpressionSyntax right)
        => BinaryExpression(kind, left, right);

    private static ParenthesizedExpressionSyntax Parenthesized(ExpressionSyntax expr)
        => ParenthesizedExpression(expr);

    private static LocalDeclarationStatementSyntax LocalVar(string name, TypeSyntax type, ExpressionSyntax valueExpr)
    {
        return LocalDeclarationStatement(
            VariableDeclaration(type)
                .AddVariables(
                    VariableDeclarator(name)
                        .WithInitializer(EqualsValueClause(valueExpr))));
    }

    private static PredefinedTypeSyntax PredefinedTypeUInt()
        => PredefinedType(Token(SyntaxKind.UIntKeyword));
}