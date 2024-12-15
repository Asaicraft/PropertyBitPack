using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PropertyBitPack.SourceGen.Collections;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Diagnostics;
using System.Text;
using System.Linq;
using System.Diagnostics;

namespace PropertyBitPack.SourceGen;
public ref struct BitFlagsForClassGeneratorHelper
{
    private int _index;
    private string? _cuurentClassName;

    private readonly ImmutableArray<ImmutableArray<PropertyToBitInfo>> _propertyToBitInfos;

    public BitFlagsForClassGeneratorHelper()
    {
        ThrowHelper.ThrowInvalidOperationException($"Use the {nameof(Create)} method to create an instance of this type.");
    }

    private BitFlagsForClassGeneratorHelper(ImmutableArray<ImmutableArray<PropertyToBitInfo>> propertyToBitInfos)
    {
        _propertyToBitInfos = propertyToBitInfos;
    }


    [MemberNotNullWhen(false, nameof(ClassName))]
    public readonly bool IsEnd => _index >= _propertyToBitInfos.Length;

    public readonly string? ClassName => _cuurentClassName;

    public bool TryGenerateNextClass([NotNullWhen(true)] out CompilationUnitSyntax? unitSyntax,[NotNullWhen(false)] out Diagnostic? diagnostic)
    {
        var current = _propertyToBitInfos[_index];

        if (current.Length == 0)
        {
            PropertyBitPackThrowHelper.ThrowUnreachableException($"{nameof(current)}.{nameof(current.Length)} == 0");
        }

        var owner = current[0].Owner;

        var namespaceSyntax = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(owner.ContainingNamespace.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)));
        var classSyntax = SyntaxFactory.ClassDeclaration(
                owner.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)
            )
            .WithModifiers(
                SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PartialKeyword))
            );

        var allBitsCount = current.Sum(x =>
        {
            return x.BitsCount is null ? PropertyBitPackThrowHelper.ThrowUnreachableException<int>($"Invalid {nameof(PropertyToBitInfo)} ") : x.BitsCount.Value;
        });

        var bitTypeStorages = GetBitTypeSyntaxes(current);

        foreach (var bitTypeStorage in bitTypeStorages)
        {
            classSyntax = classSyntax.AddMembers(CreateFieldDeclarationSyntax(bitTypeStorage));
        }

        // add space between fields and properties

        foreach (var bitTypeStorage in bitTypeStorages)
        {
            var bitsCount = 0;
            foreach (var propertyToBitInfo in bitTypeStorage.PropertiesWhichDataStored)
            {
                var length = propertyToBitInfo.BitsCount!.Value;
                classSyntax = classSyntax.AddMembers(CreatePropertyDeclarationSyntax(bitTypeStorage.FieldName, propertyToBitInfo, new(bitsCount, length, bitTypeStorage.TypeBitsCount)));
                bitsCount += propertyToBitInfo.BitsCount!.Value;
            }
        }

        namespaceSyntax = namespaceSyntax.WithMembers(SyntaxFactory.SingletonList<MemberDeclarationSyntax>(classSyntax));

        diagnostic = null;
        unitSyntax = SyntaxFactory.CompilationUnit().WithMembers(SyntaxFactory.SingletonList<MemberDeclarationSyntax>(namespaceSyntax));

        _index++;

        _cuurentClassName = owner.ToDisplayParts(SymbolDisplayFormat.FullyQualifiedFormat).Skip(2).ToImmutableArray().ToDisplayString();
        _cuurentClassName = _cuurentClassName + ".BitFlags.g.cs";

#if DEBUG
        var debugAst = unitSyntax.NormalizeWhitespace().ToFullString();
        Debug.WriteLine(
            $"Generating properties for: {_cuurentClassName} \n" +
            $"Properties Count: {current.Length} \n" +
            $"Fileds Count: {bitTypeStorages.Length} \n\n" +
            $"{debugAst} \n\n\n");
#endif

        return true;
    }

    private static FieldDeclarationSyntax CreateFieldDeclarationSyntax(BitTypeStorage bitTypeStorage)
    {
        var fieldDeclaration = SyntaxFactory.FieldDeclaration(
            SyntaxFactory.VariableDeclaration(
                bitTypeStorage.TypeSyntax,
                SyntaxFactory.SeparatedList([
                    SyntaxFactory.VariableDeclarator(bitTypeStorage.FieldName)
                ])))
            .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PrivateKeyword)));

        return fieldDeclaration;
    }

    private static PropertyDeclarationSyntax CreatePropertyDeclarationSyntax(
        string fieldName,
        PropertyToBitInfo propertyToBitInfo,
        BitsSpan bitsSpan)
    {

        var propertyTypeName = propertyToBitInfo.PropertyType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var propertyName = propertyToBitInfo.PropertySymbol.Name;
        var start = bitsSpan.Start;
        var length = bitsSpan.Length;

        var isBoolean = propertyToBitInfo.PropertyType.SpecialType == SpecialType.System_Boolean && length == 1;

        var fieldIdentifier = SyntaxFactory.IdentifierName(fieldName);
        var valueIdentifier = SyntaxFactory.IdentifierName("value");
        var propertyTypeSyntax = SyntaxFactory.ParseTypeName(propertyTypeName);

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

        // Геттер
        AccessorDeclarationSyntax getter;
        if (isBoolean)
        {
            // bool get:
            // {
            //    uint mask = ((1U << length) - 1U);
            //    return ((fieldName >> start) & mask) != 0U;
            // }

            // (fieldName >> start)
            var fieldShifted = Parenthesized(BinaryExpr(fieldIdentifier, SyntaxKind.RightShiftExpression, startLiteral));
            // ((fieldName >> start) & mask)
            var fieldAndMask = Parenthesized(BinaryExpr(fieldShifted, SyntaxKind.BitwiseAndExpression, SyntaxFactory.IdentifierName("mask")));
            // (((fieldName >> start) & mask) != 0U)
            var comparison = BinaryExpr(fieldAndMask, SyntaxKind.NotEqualsExpression, NumericLiteral(0U));

            var returnStmt = SyntaxFactory.ReturnStatement(comparison);

            getter = SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                .WithBody(SyntaxFactory.Block(maskDeclaration, returnStmt));
        }
        else
        {
            // numeric get:
            // {
            //    uint mask = ((1U << length) - 1U);
            //    return (PropertyType)((fieldName >> start) & mask);
            // }

            var fieldShifted = Parenthesized(BinaryExpr(fieldIdentifier, SyntaxKind.RightShiftExpression, startLiteral));
            var fieldAndMask = Parenthesized(BinaryExpr(fieldShifted, SyntaxKind.BitwiseAndExpression, SyntaxFactory.IdentifierName("mask")));
            var castToProperty = SyntaxFactory.CastExpression(propertyTypeSyntax, fieldAndMask);
            var returnStmt = SyntaxFactory.ReturnStatement(castToProperty);

            getter = SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                .WithBody(SyntaxFactory.Block(maskDeclaration, returnStmt));
        }

        // Сеттер
        AccessorDeclarationSyntax setterOrInitter;
        if (isBoolean)
        {
            // bool set:
            // {
            //    if (value)
            //        fieldName |= (1U << start);
            //    else
            //        fieldName &= ~(1U << start);
            // }

            var oneShifted = Parenthesized(BinaryExpr(NumericLiteral(1U), SyntaxKind.LeftShiftExpression, startLiteral));
            var ifStmt = SyntaxFactory.IfStatement(
                valueIdentifier,
                SyntaxFactory.Block(
                    SyntaxFactory.ExpressionStatement(
                        SyntaxFactory.AssignmentExpression(SyntaxKind.OrAssignmentExpression, fieldIdentifier, oneShifted))
                ),
                SyntaxFactory.ElseClause(
                    SyntaxFactory.Block(
                        SyntaxFactory.ExpressionStatement(
                            SyntaxFactory.AssignmentExpression(
                                SyntaxKind.AndAssignmentExpression,
                                fieldIdentifier,
                                SyntaxFactory.PrefixUnaryExpression(SyntaxKind.BitwiseNotExpression, oneShifted)
                            )
                        )
                    )
                )
            );

            // Для булевого сеттера mask не нужен

            setterOrInitter = SyntaxFactory.AccessorDeclaration(propertyToBitInfo.IsInit ? SyntaxKind.InitAccessorDeclaration : SyntaxKind.SetAccessorDeclaration)
                .WithBody(SyntaxFactory.Block(ifStmt));

            if(propertyToBitInfo.SetterOrInitModifiers.Count != 0)
            {
                setterOrInitter = setterOrInitter.WithModifiers(propertyToBitInfo.SetterOrInitModifiers);
            }
        }
        else
        {
            // numeric set:
            // {
            //    uint mask = ((1U << length) - 1U);
            //    fieldName = (fieldName & ~(mask << start)) | (((uint)value & mask) << start);
            // }

            var maskRef = SyntaxFactory.IdentifierName("mask");
            var maskShifted = Parenthesized(BinaryExpr(maskRef, SyntaxKind.LeftShiftExpression, startLiteral));
            var notMaskShifted = SyntaxFactory.PrefixUnaryExpression(SyntaxKind.BitwiseNotExpression, maskShifted);

            var uintValue = SyntaxFactory.CastExpression(PredefinedTypeUInt(), valueIdentifier);
            var valueAndMask = Parenthesized(BinaryExpr(uintValue, SyntaxKind.BitwiseAndExpression, maskRef));
            var valueShifted = Parenthesized(BinaryExpr(valueAndMask, SyntaxKind.LeftShiftExpression, startLiteral));

            var fieldCleared = Parenthesized(BinaryExpr(fieldIdentifier, SyntaxKind.BitwiseAndExpression, notMaskShifted));
            var fieldFinal = Parenthesized(BinaryExpr(fieldCleared, SyntaxKind.BitwiseOrExpression, valueShifted));

            var assignStmt = SyntaxFactory.ExpressionStatement(
                SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, fieldIdentifier, fieldFinal));

            setterOrInitter = SyntaxFactory.AccessorDeclaration(propertyToBitInfo.IsInit ? SyntaxKind.InitAccessorDeclaration : SyntaxKind.SetAccessorDeclaration)
                .WithBody(SyntaxFactory.Block(maskDeclaration, assignStmt));

            if (propertyToBitInfo.SetterOrInitModifiers.Count != 0)
            {
                setterOrInitter = setterOrInitter.WithModifiers(propertyToBitInfo.SetterOrInitModifiers);
            }
        }

        var propertyDeclaration = SyntaxFactory.PropertyDeclaration(propertyTypeSyntax, SyntaxFactory.Identifier(propertyName))
            .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.PartialKeyword))
            .AddAccessorListAccessors(getter, setterOrInitter);

        return propertyDeclaration;

        // Вспомогательные локальные функции
        static LiteralExpressionSyntax NumericLiteral(uint value)
            => SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(value));

        static LiteralExpressionSyntax NumericLiteralInt(int value)
            => SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(value));

        static BinaryExpressionSyntax BinaryExpr(ExpressionSyntax left, SyntaxKind kind, ExpressionSyntax right)
            => SyntaxFactory.BinaryExpression(kind, left, right);

        static ParenthesizedExpressionSyntax Parenthesized(ExpressionSyntax expr)
            => SyntaxFactory.ParenthesizedExpression(expr);

        static LocalDeclarationStatementSyntax LocalVar(string name, TypeSyntax type, ExpressionSyntax valueExpr)
        {
            return SyntaxFactory.LocalDeclarationStatement(
                SyntaxFactory.VariableDeclaration(type)
                    .AddVariables(
                        SyntaxFactory.VariableDeclarator(name)
                            .WithInitializer(SyntaxFactory.EqualsValueClause(valueExpr))));
        }

        static PredefinedTypeSyntax PredefinedTypeUInt()
            => SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.UIntKeyword));
    }


    private static ImmutableArray<BitTypeStorage> GetBitTypeSyntaxes(ImmutableArray<PropertyToBitInfo> propertyToBitInfos)
    {
        var availableTypes = new (TypeSyntax TypeSyntax, int Bits)[] {
            (SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ByteKeyword)), 8),
            (SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.UShortKeyword)), 16),
            (SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.UIntKeyword)), 32),
            (SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.LongKeyword)), 64)
        };

        if (propertyToBitInfos.IsEmpty)
        {
            return [];
        }

        foreach (var p in propertyToBitInfos)
        {
            if (p.BitsCount is null)
            {
                PropertyBitPackThrowHelper.ThrowUnreachableException($"Property {p.PropertySymbol.Name} has no BitsCount.");
            }
        }

        var properties = propertyToBitInfos.OrderByDescending(p => p.BitsCount!.Value).ToList();

        var maxPropertyBits = properties[0].BitsCount!.Value;

        // Определяем минимально подходящий тип вагона
        var chosenType = availableTypes.FirstOrDefault(t => t.Bits >= maxPropertyBits);
        if (chosenType.TypeSyntax is null)
        {
            PropertyBitPackThrowHelper.ThrowUnreachableException($"There is a property that doesn't fit in any available size. Maybe {maxPropertyBits} > 64");
        }

        var wagonBits = chosenType.Bits;
        var wagonsBuilder = ImmutableArrayBuilder<BitTypeStorage>.Rent();

        try
        {
            // Упаковываем свойства в вагоны выбранного типа
            while (properties.Count > 0)
            {
                using var currentPropertiesBuilder = ImmutableArrayBuilder<PropertyToBitInfo>.Rent();
                var currentBitsUsed = 0;

                for (var i = 0; i < properties.Count;)
                {
                    var bits = properties[i].BitsCount!.Value;
                    if (currentBitsUsed + bits <= wagonBits)
                    {
                        currentPropertiesBuilder.Add(properties[i]);
                        currentBitsUsed += bits;
                        properties.RemoveAt(i);
                    }
                    else
                    {
                        i++;
                    }
                }
                var currentProperties = currentPropertiesBuilder.ToImmutable();
                // Добавляем текущий вагон в результат
                wagonsBuilder.Add(new BitTypeStorage(
                    string.Join("__", currentProperties.Select(p => p.FieldName)),
                    chosenType.TypeSyntax,
                    chosenType.Bits,
                    currentProperties));
            }

            return wagonsBuilder.ToImmutable();
        }
        finally
        {
            wagonsBuilder.Dispose();
        }
    }

    public sealed class BitTypeStorage
    {
        public string FieldName { get; }
        public TypeSyntax TypeSyntax { get; }
        public int TypeBitsCount { get; }
        public ImmutableArray<PropertyToBitInfo> PropertiesWhichDataStored { get; }

        public BitTypeStorage(string fieldName, TypeSyntax typeSyntax, int typeBitsCount, ImmutableArray<PropertyToBitInfo> propertiesWhichDataStored)
        {
            FieldName = fieldName;
            TypeSyntax = typeSyntax;
            TypeBitsCount = typeBitsCount;
            PropertiesWhichDataStored = propertiesWhichDataStored;
        }
    }

    // for debugging
    [SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "<Pending>")]
    [SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "<Pending>")]
    private readonly ITypeSymbol[] Owners
    {
        get
        {
            var owners = new ITypeSymbol[_propertyToBitInfos.Length];

            for (var i = 0; i < _propertyToBitInfos.Length; i++)
            {
                owners[i] = _propertyToBitInfos[i][0].Owner;
            }

            return owners;
        }
    }

    public static BitFlagsForClassGeneratorHelper Create(ImmutableArray<PropertyToBitInfo> propertyToBitInfos, SourceProductionContext incrementalGeneratorInitializationContext)
    {
        using var groupedBuilder = ImmutableArrayBuilder<ImmutableArray<PropertyToBitInfo>>.Rent();

        // Используем ArrayPool для временного хранения уникальных Owners
        var ownerPool = ArrayPool<ITypeSymbol>.Shared;
        var uniqueOwners = ownerPool.Rent(propertyToBitInfos.Length);
        var uniqueOwnerCount = 0;

        try
        {
            foreach (var property in propertyToBitInfos)
            {
                var isNewOwner = true;

                // Проверяем, есть ли уже этот Owner в списке уникальных
                for (var i = 0; i < uniqueOwnerCount; i++)
                {
                    if (SymbolEqualityComparer.Default.Equals(uniqueOwners[i], property.Owner))
                    {
                        isNewOwner = false;
                        break;
                    }
                }

                // Если новый Owner, добавляем в список уникальных
                if (isNewOwner)
                {
                    uniqueOwners[uniqueOwnerCount++] = property.Owner;
                }
            }

            // Для каждого уникального Owner собираем его PropertyToBitInfo
            foreach (var owner in uniqueOwners.AsSpan(0, uniqueOwnerCount))
            {
                using var ownerPropertiesBuilder = ImmutableArrayBuilder<PropertyToBitInfo>.Rent();

                foreach (var property in propertyToBitInfos)
                {
                    if (SymbolEqualityComparer.Default.Equals(property.Owner, owner))
                    {
                        ownerPropertiesBuilder.Add(property);
                    }
                }

                groupedBuilder.Add(ownerPropertiesBuilder.ToImmutable());
            }
        }
        finally
        {
            // Возвращаем память в пул
            ownerPool.Return(uniqueOwners, clearArray: true);
        }

        var grouped = groupedBuilder.ToImmutable();

        return new BitFlagsForClassGeneratorHelper(grouped);
    }

}
