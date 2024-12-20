using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PropertyBitPack.SourceGen.Collections;
using PropertyBitPack.SourceGen.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace PropertyBitPack.SourceGen;
public static class SyntaxGenerator
{
    public static CompilationUnitSyntax? GenerateFieldAndBindedProperties(PackedFieldStorage packedFieldStorage, in ImmutableArrayBuilder<Diagnostic> diagnosticsBuilder)
    {
        var owner = packedFieldStorage.PropertiesWhichDataStored[0].Owner;

        var namespaceSyntax = NamespaceDeclaration(ParseName(owner.ContainingNamespace.ToDisplayString()));

        var ownerName = owner.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);

        TypeDeclarationSyntax? ownerSyntax = owner.TypeKind switch
        {
            TypeKind.Class => !owner.IsRecord ? ClassDeclaration(ownerName) : RecordDeclaration(SyntaxKind.RecordDeclaration, Token(SyntaxKind.RecordKeyword) ,ownerName),
            TypeKind.Struct => !owner.IsRecord ? StructDeclaration(ownerName) : RecordDeclaration(SyntaxKind.RecordStructDeclaration, Token(SyntaxKind.RecordKeyword), ownerName),
            _ => null
        };

        if(ownerSyntax is null)
        {
            diagnosticsBuilder.Add(Diagnostic.Create(PropertyBitPackDiagnostics.UnsoportedOwnerType, owner.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax()?.GetLocation()));
            return null;
        }
            
        ownerSyntax = ownerSyntax.WithModifiers(
            TokenList(Token(SyntaxKind.PartialKeyword))
        );

        var fieldDeclaration = GenerateFieldDeclaration(packedFieldStorage);
        ownerSyntax = ownerSyntax.AddMembers(fieldDeclaration);

        var properties = GenerateProperties(packedFieldStorage);

        ownerSyntax = ownerSyntax.AddMembers([.. properties]);

        namespaceSyntax = namespaceSyntax.AddMembers(ownerSyntax);

        var compilationUnit = CompilationUnit().AddMembers(namespaceSyntax);

#if DEBUG
        var debugAst = compilationUnit.NormalizeWhitespace().ToFullString();
        Debug.WriteLine(
            $"Generating properties for: {owner.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} \n" +
            $"Properties Count: {packedFieldStorage.PropertiesWhichDataStored.Length} \n" +
            $"Fileds Count: {packedFieldStorage.PropertiesWhichDataStored.Length} \n\n" +
            $"{debugAst} \n\n\n");
#endif

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
        var members = new List<MemberDeclarationSyntax>(properties.Length);

        var offset = 0;
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
        var members = new List<MemberDeclarationSyntax>(properties.Length);

        // Определяем смещения
        var offset = 0;
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

        // mask = ((1U << length) - 1U);
        var maskDeclaration = LocalVar(
            "mask",
            PredefinedTypeUInt(),
            Parenthesized(
                BinaryExpr(
                    Parenthesized(BinaryExpr(NumericLiteral(1U), SyntaxKind.LeftShiftExpression, lengthLiteral)),
                    SyntaxKind.SubtractExpression,
                    NumericLiteral(1U))));

        var fieldAsUInt = CastExpression(PredefinedTypeUInt(), fieldIdentifier);

        // Getter
        AccessorDeclarationSyntax getter;
        if (isBoolean)
        {
            // return (((uint)fieldName >> start) & mask) != 0U;
            var fieldShifted = Parenthesized(BinaryExpr(fieldAsUInt, SyntaxKind.RightShiftExpression, startLiteral));
            var fieldAndMask = Parenthesized(BinaryExpr(fieldShifted, SyntaxKind.BitwiseAndExpression, IdentifierName("mask")));
            var comparison = BinaryExpr(fieldAndMask, SyntaxKind.NotEqualsExpression, NumericLiteral(0U));

            getter = AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                .WithBody(Block(maskDeclaration, ReturnStatement(comparison)));
        }
        else
        {
            // return (PropertyType)(((uint)fieldName >> start) & mask);
            var fieldShifted = Parenthesized(BinaryExpr(fieldAsUInt, SyntaxKind.RightShiftExpression, startLiteral));
            var fieldAndMask = Parenthesized(BinaryExpr(fieldShifted, SyntaxKind.BitwiseAndExpression, IdentifierName("mask")));
            var castToProperty = CastExpression(propertyTypeSyntax, fieldAndMask);

            getter = AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                .WithBody(Block(maskDeclaration, ReturnStatement(castToProperty)));
        }

        // Setter/init
        AccessorDeclarationSyntax setterOrInitter;
        if (isBoolean)
        {
            // if (value)
            //   fieldName = (FieldType)(((uint)fieldName) | (1U << start));
            // else
            //   fieldName = (FieldType)(((uint)fieldName) & ~(1U << start));

            var oneShifted = Parenthesized(BinaryExpr(NumericLiteral(1U), SyntaxKind.LeftShiftExpression, startLiteral));

            var orExpr = BinaryExpr(fieldAsUInt, SyntaxKind.BitwiseOrExpression, oneShifted);
            var orCastToFieldType = CastExpression(backingFieldType, Parenthesized(orExpr));

            var notOneShifted = PrefixUnaryExpression(SyntaxKind.BitwiseNotExpression, oneShifted);
            var andExpr = BinaryExpr(fieldAsUInt, SyntaxKind.BitwiseAndExpression, notOneShifted);
            var andCastToFieldType = CastExpression(backingFieldType, Parenthesized(andExpr));

            var ifStmt = IfStatement(
                valueIdentifier,
                Block(ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, fieldIdentifier, orCastToFieldType))),
                ElseClause(Block(ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, fieldIdentifier, andCastToFieldType)))));

            setterOrInitter = AccessorDeclaration(propertyToBitInfo.IsInit ? SyntaxKind.InitAccessorDeclaration : SyntaxKind.SetAccessorDeclaration)
                .WithBody(Block(ifStmt));

            if (propertyToBitInfo.SetterOrInitModifiers.Count != 0)
            {
                setterOrInitter = setterOrInitter.WithModifiers(propertyToBitInfo.SetterOrInitModifiers);
            }
        }
        else
        {
            // Если (uint)value > mask, установить value = (PropertyType)mask
            var maskRef = IdentifierName("mask");
            var uintValueCast = CastExpression(PredefinedTypeUInt(), valueIdentifier);
            var conditionValueGreaterThanMax = BinaryExpr(uintValueCast, SyntaxKind.GreaterThanExpression, maskRef);

            // value = (PropertyType)(object)(uint)mask; - т.к. propertyTypeSyntax может быть разным, 
            // можно просто сделать двойное приведение: к uint, а потом к нужному типу
            // но учитывая, что mask уже uint, можно сразу: (PropertyType)mask
            var setValueToMax = ExpressionStatement(
                AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    valueIdentifier,
                    CastExpression(propertyTypeSyntax, maskRef)
                )
            );

            // fieldName = (FieldType)(((uint)fieldName & ~(mask << start)) | (((uint)value & mask) << start));
            var maskShifted = Parenthesized(BinaryExpr(maskRef, SyntaxKind.LeftShiftExpression, startLiteral));
            var notMaskShifted = PrefixUnaryExpression(SyntaxKind.BitwiseNotExpression, maskShifted);

            var uintValue = CastExpression(PredefinedTypeUInt(), valueIdentifier);
            var valueAndMask = Parenthesized(BinaryExpr(uintValue, SyntaxKind.BitwiseAndExpression, maskRef));
            var valueShifted = Parenthesized(BinaryExpr(valueAndMask, SyntaxKind.LeftShiftExpression, startLiteral));

            var fieldCleared = Parenthesized(BinaryExpr(fieldAsUInt, SyntaxKind.BitwiseAndExpression, notMaskShifted));
            var fieldFinalExpr = Parenthesized(BinaryExpr(fieldCleared, SyntaxKind.BitwiseOrExpression, valueShifted));
            var fieldFinalCast = CastExpression(backingFieldType, fieldFinalExpr);

            var assignStmt = ExpressionStatement(
                AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, fieldIdentifier, fieldFinalCast));

            // Добавляем if для проверки и клампинга
            var ifClampStmt = IfStatement(
                conditionValueGreaterThanMax,
                Block(setValueToMax),
                null
            );

            setterOrInitter = AccessorDeclaration(propertyToBitInfo.IsInit ? SyntaxKind.InitAccessorDeclaration : SyntaxKind.SetAccessorDeclaration)
                .WithBody(Block(maskDeclaration, ifClampStmt, assignStmt));

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

        // Геттер:
        // int extractedValue = (int)(((uint)fieldName >> start) & mask);
        // return extractedValue == maxValue ? GetLargeValue() : extractedValue;

        var fieldAsUInt = CastExpression(PredefinedTypeUInt(), fieldIdentifier);

        var fieldShifted = Parenthesized(BinaryExpr(fieldAsUInt, SyntaxKind.RightShiftExpression, startLiteral));
        var fieldAndMask = Parenthesized(BinaryExpr(fieldShifted, SyntaxKind.BitwiseAndExpression, IdentifierName("mask")));

        var extractedValueDecl = LocalVar(
            "extractedValue",
            PredefinedType(Token(SyntaxKind.IntKeyword)),
            CastExpression(PredefinedType(Token(SyntaxKind.IntKeyword)), fieldAndMask));

        var largeValueCall = IdentifierName(propertyToBitInfo.GetterLargeSizeValueSymbol!.Name);
        var invocation = InvocationExpression(largeValueCall);

        var condition = BinaryExpr(IdentifierName("extractedValue"), SyntaxKind.EqualsExpression, IdentifierName("maxValue"));
        var ternary = ConditionalExpression(condition, invocation, IdentifierName("extractedValue"));
        var getter = AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
            .WithBody(Block(maskDeclaration, maxValueDeclaration, extractedValueDecl, ReturnStatement(ternary)));

        // Сеттер:
        // Если value > maxValue, то value = maxValue
        // После этого:
        // fieldName = (FieldType)(((uint)fieldName & ~(mask << start)) | (((uint)value & mask) << start));

        var maskRef = IdentifierName("mask");
        var maskShifted = Parenthesized(BinaryExpr(maskRef, SyntaxKind.LeftShiftExpression, startLiteral));
        var notMaskShifted = PrefixUnaryExpression(SyntaxKind.BitwiseNotExpression, maskShifted);

        var uintValue = CastExpression(PredefinedTypeUInt(), valueIdentifier);
        var valueAndMask = Parenthesized(BinaryExpr(uintValue, SyntaxKind.BitwiseAndExpression, maskRef));
        var valueShifted = Parenthesized(BinaryExpr(valueAndMask, SyntaxKind.LeftShiftExpression, startLiteral));

        var fieldCleared = Parenthesized(BinaryExpr(fieldAsUInt, SyntaxKind.BitwiseAndExpression, notMaskShifted));
        var fieldFinalExpr = Parenthesized(BinaryExpr(fieldCleared, SyntaxKind.BitwiseOrExpression, valueShifted));
        var fieldFinalCast = CastExpression(backingFieldType, fieldFinalExpr);

        var conditionValueGreaterThanMax = BinaryExpr(
            CastExpression(PredefinedType(Token(SyntaxKind.IntKeyword)), valueIdentifier),
            SyntaxKind.GreaterThanExpression,
            IdentifierName("maxValue"));

        var setValueToMax = ExpressionStatement(
            AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                valueIdentifier,
                CastExpression(propertyTypeSyntax, IdentifierName("maxValue"))
            )
        );

        var ifClampStmt = IfStatement(
            conditionValueGreaterThanMax,
            Block(setValueToMax),
            null
        );

        var assignStmt = ExpressionStatement(
            AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, fieldIdentifier, fieldFinalCast));

        var setter = AccessorDeclaration(propertyToBitInfo.IsInit ? SyntaxKind.InitAccessorDeclaration : SyntaxKind.SetAccessorDeclaration)
            .WithBody(Block(maskDeclaration, maxValueDeclaration, ifClampStmt, assignStmt));

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