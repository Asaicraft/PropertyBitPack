using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PropertyBitPack.SourceGen.Collections;
using PropertyBitPack.SourceGen.Models;
using PropertyBitPack.SourceGen.Models.GenerateSourceRequests;
using System.Buffers;
using System.Collections.Immutable;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace PropertyBitPack.SourceGen.PropertiesSyntaxGenerators;

/// <summary>
/// Abstract base class for generating property declarations with customizable syntax for getters, setters, and initializers.
/// </summary>
/// <param name="propertyBitPackGeneratorContext">The context for the property bit pack generator.</param>
internal abstract class BasePropertySyntaxGenerator(PropertyBitPackGeneratorContext propertyBitPackGeneratorContext) : IPropertySyntaxGenerator
{

    /// <summary>
    /// Generates a setter accessor syntax with the specified modifiers and body.
    /// </summary>
    /// <param name="modifiers">The modifiers for the setter accessor.</param>
    /// <param name="body">The body of the setter accessor.</param>
    /// <returns>An <see cref="AccessorDeclarationSyntax"/> for the setter.</returns>
    protected static AccessorDeclarationSyntax Setter(SyntaxTokenList modifiers, BlockSyntax body)
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
    protected static AccessorDeclarationSyntax Getter(BlockSyntax body)
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

    protected static AccessorDeclarationSyntax Initter(SyntaxTokenList modifiers, BlockSyntax body)
    {
        return AccessorDeclaration(
            SyntaxKind.InitAccessorDeclaration,
            List<AttributeListSyntax>(),
            modifiers,
            body
        );
    }


    private readonly PropertyBitPackGeneratorContext _propertyBitPackGeneratorContext = propertyBitPackGeneratorContext;

    /// <summary>
    /// Gets the context for the property bit pack generator.
    /// </summary>
    public PropertyBitPackGeneratorContext PropertyBitPackGeneratorContext => _propertyBitPackGeneratorContext;

    /// <inheritdoc/>
    public PropertyDeclarationSyntax? GenerateProperty(GenerateSourceRequest sourceRequest, BitFieldPropertyInfoRequest bitFieldPropertyInfoRequest, out ImmutableArray<MemberDeclarationSyntax> additionalMember)
    {
        if (!IsCandidate(sourceRequest, bitFieldPropertyInfoRequest))
        {
            return null;
        }

        var property = GeneratePropertyCore(sourceRequest, bitFieldPropertyInfoRequest);
        property ??= GeneratePropertyCore(sourceRequest, bitFieldPropertyInfoRequest, out additionalMember);

        return property;
    }

    /// <summary>
    /// Determines whether a property is a candidate for generation.
    /// </summary>
    /// <param name="sourceRequest">The source request containing generation details.</param>
    /// <param name="bitFieldPropertyInfoRequest">The bit field property information.</param>
    /// <returns>
    /// <c>true</c> if the property is a candidate for generation; otherwise, <c>false</c>.
    /// </returns>
    protected virtual bool IsCandidate(GenerateSourceRequest sourceRequest, BitFieldPropertyInfoRequest bitFieldPropertyInfoRequest)
    {
        return false;
    }

    /// <summary>
    /// Generates the core property declaration.
    /// </summary>
    /// <param name="sourceRequest">The source request containing generation details.</param>
    /// <param name="bitFieldPropertyInfoRequest">The bit field property information.</param>
    /// <returns>
    /// A <see cref="PropertyDeclarationSyntax"/> representing the generated property, or <c>null</c> if not applicable.
    /// </returns>
    protected virtual PropertyDeclarationSyntax? GeneratePropertyCore(GenerateSourceRequest sourceRequest, BitFieldPropertyInfoRequest bitFieldPropertyInfoRequest)
    {
        var getterBlock = GetterBlockSyntax(bitFieldPropertyInfoRequest);
        var setterBlock = SetterBlockSyntax(bitFieldPropertyInfoRequest);

        var modifiers = bitFieldPropertyInfoRequest.SetterOrInitModifiers;

        var getter = Getter(getterBlock);
        var setterOrInitter = bitFieldPropertyInfoRequest.HasInitOrSet
            ? bitFieldPropertyInfoRequest.IsInit
                ? Initter(modifiers, setterBlock)
                : Setter(modifiers, setterBlock)
            : null;

        AccessorListSyntax accessors;

        using (var tempAccessorsRented = ListsPool.Rent<AccessorDeclarationSyntax>())
        {
            var tempAccessors = tempAccessorsRented.List;
            tempAccessors.Add(getter);

            if (setterOrInitter is not null)
            {
                tempAccessors.Add(setterOrInitter);
            }

            accessors = AccessorList(List(tempAccessors));
        }

        var propertyTypeSyntax = bitFieldPropertyInfoRequest.PropertyDeclarationSyntax.Type;
        var propertyName = bitFieldPropertyInfoRequest.PropertyDeclarationSyntax.Identifier;
        var propertyModifiers = bitFieldPropertyInfoRequest.PropertyDeclarationSyntax.Modifiers;

        return PropertyDeclaration(
            List<AttributeListSyntax>(),
            propertyModifiers,
            propertyTypeSyntax,
            null,
            propertyName,
            accessors
        );
    }

    /// <summary>
    /// Generates the core property declaration with additional members if applicable.
    /// </summary>
    /// <param name="sourceRequest">The source request containing generation details.</param>
    /// <param name="bitFieldPropertyInfoRequest">The bit field property information.</param>
    /// <param name="additionalMember">Outputs additional member declarations required for the property.</param>
    /// <returns>
    /// A <see cref="PropertyDeclarationSyntax"/> representing the generated property, or <c>null</c> if not applicable.
    /// </returns>
    public virtual PropertyDeclarationSyntax? GeneratePropertyCore(GenerateSourceRequest sourceRequest, BitFieldPropertyInfoRequest bitFieldPropertyInfoRequest, out ImmutableArray<MemberDeclarationSyntax> additionalMember)
    {
        additionalMember = default;
        return null;
    }

    /// <summary>
    /// Generates the block syntax for a bitfield property's getter.
    /// This block will contain a single 'return' statement that returns the value
    /// extracted from the underlying bitfield.
    /// </summary>
    /// <param name="bitFieldPropertyInfoRequest">
    /// Information about the bitfield property, including bit positions and the property symbol.
    /// </param>
    /// <returns>
    /// A <see cref="BlockSyntax"/> representing the getter's body with a single return statement.
    /// </returns>
    protected virtual BlockSyntax GetterBlockSyntax(BitFieldPropertyInfoRequest bitFieldPropertyInfoRequest)
    {
        // Build the expression that extracts the value from the bitfield
        var getExpression = GetBitwiseExpression(bitFieldPropertyInfoRequest);

        // Create a return statement with that expression
        var returnStatement = ReturnStatement(getExpression);

        // Wrap it into a block
        return Block(SingletonList<StatementSyntax>(returnStatement));
    }

    /// <summary>
    /// Generates a <see cref="BlockSyntax"/> for the setter of a bit field property,
    /// handling both 1-bit (boolean) and multi-bit properties with clamping where necessary.
    /// </summary>
    /// <param name="bitFieldPropertyInfoRequest">
    /// The bit field property information, including field name, start position, length, and property symbol.
    /// </param>
    /// <returns>
    /// A <see cref="BlockSyntax"/> containing the logic to set the value of the property.
    /// </returns>
    /// <summary>
    /// Generates a <see cref="BlockSyntax"/> for the setter of a bit field property,
    /// handling both 1-bit (boolean) and multi-bit properties with clamping where necessary.
    /// </summary>
    /// <param name="bitFieldPropertyInfoRequest">
    /// The bit field property information, including field name, start position, length, and property symbol.
    /// </param>
    /// <returns>
    /// A <see cref="BlockSyntax"/> containing the logic to set the value of the property.
    /// </returns>
    protected virtual BlockSyntax SetterBlockSyntax(BitFieldPropertyInfoRequest bitFieldPropertyInfoRequest)
    {
        var fieldType = bitFieldPropertyInfoRequest.BitsSpan.FieldRequest.FieldType;
        var fieldTypeSyntax = GetTypeSyntaxFromSpecialType(fieldType);
        var propertyTypeSyntax = bitFieldPropertyInfoRequest.PropertyDeclarationSyntax.Type;
        var propertySymbol = bitFieldPropertyInfoRequest.PropertySymbol;
        var fieldName = bitFieldPropertyInfoRequest.BitsSpan.FieldRequest.Name;
        var start = bitFieldPropertyInfoRequest.BitsSpan.Start;
        var length = bitFieldPropertyInfoRequest.BitsSpan.Length;

        // Collect statements for the setter block
        using var statementsRented = ListsPool.Rent<StatementSyntax>();
        var statements = statementsRented.List;

        // Handle 1-bit (boolean) properties
        if (propertySymbol.Type.SpecialType == SpecialType.System_Boolean)
        {
            // Generate the expression "fieldName = value ? ... : ..."
            var boolAssignment = SetOrInitBitwiseExpression(
                fieldType,
                fieldName,
                start,
                length,
                propertySymbol,
                "value" // In the setter, the value is represented by "value"
            );

            // Wrap the expression in an ExpressionStatement and add to the block
            statements.Add(ExpressionStatement(boolAssignment));
        }
        else
        {
            // For multi-bit properties, clamp the value to ensure it fits within the specified bit length

            // 1) Define a constant maxValue_: const {PropertyType} maxValue_ = (1 << length) - 1;
            var maxValueDecl = LocalDeclarationStatement(
                VariableDeclaration(
                    fieldTypeSyntax,
                    SingletonSeparatedList(
                        VariableDeclarator("maxValue_")
                            .WithInitializer(
                                EqualsValueClause(
                                    BinaryExpression(
                                        SyntaxKind.SubtractExpression,
                                        ParenthesizedExpression(
                                            BinaryExpression(
                                                SyntaxKind.LeftShiftExpression,
                                                LiteralWithSpecialType(1, fieldType),
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
            statements.Add(maxValueDecl);


            // 2) Declare a clamped variable: var clamped_ = Math.Min(({PropertyType})value, maxValue_);
            using var mathMinArguments = ListsPool.Rent<SyntaxNodeOrToken>();
            mathMinArguments.Add(
                Argument(
                    CastExpression(
                        fieldTypeSyntax,
                        IdentifierName("value")
                    )
                )
            );
            mathMinArguments.Add(Token(SyntaxKind.CommaToken));
            mathMinArguments.Add(Argument(IdentifierName("maxValue_")));

            var clampedDecl = LocalDeclarationStatement(
                VariableDeclaration(
                    IdentifierName("var"),
                    SingletonSeparatedList(
                        VariableDeclarator("clamped_")
                            .WithInitializer(
                                EqualsValueClause(
                                    InvocationExpression(
                                        MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            IdentifierName("Math"),
                                            IdentifierName("Min")
                                        ),
                                        ArgumentList(
                                            SeparatedList<ArgumentSyntax>(mathMinArguments.List)
                                        )
                                    )
                                )
                            )
                    )
                )
            );
            statements.Add(clampedDecl);

            // 3) Assign the clamped value using SetOrInitBitwiseExpression(...), replacing "value" with "clamped_"
            var assignmentExpr = SetOrInitBitwiseExpression(
                fieldType,
                fieldName,
                start,
                length,
                propertySymbol,
                "clamped_"
            );

            // Add the assignment as a separate statement
            statements.Add(ExpressionStatement(assignmentExpr));
        }

        // Return the full block containing all statements
        return Block(statements);
    }


    /// <summary>
    /// Generates a bitwise expression to extract the value of a property from its bitfield representation.
    /// </summary>
    /// <param name="bitFieldInfo">
    /// Information about the bitfield property, including its span, field, and associated symbol.
    /// </param>
    /// <returns>
    /// An <see cref="ExpressionSyntax"/> representing the extraction logic for the bitfield property.
    /// </returns>
    protected virtual ExpressionSyntax GetBitwiseExpression(BitFieldPropertyInfoRequest bitFieldInfo)
    {
        var fieldType = bitFieldInfo.BitsSpan.FieldRequest.FieldType;
        var fieldName = bitFieldInfo.BitsSpan.FieldRequest.Name;
        var start = bitFieldInfo.BitsSpan.Start;
        var length = bitFieldInfo.BitsSpan.Length;
        var propertySymbol = bitFieldInfo.PropertySymbol;

        return GetBitwiseExpression(fieldType, fieldName, start, length, propertySymbol);
    }

    /// <summary>
    /// Generates a bitwise expression to extract the value of a property from its bitfield representation,
    /// using explicit parameters for the field name, start position, and length.
    /// </summary>
    /// <param name="fieldName">The name of the field containing the bitfield.</param>
    /// <param name="start">The starting bit position within the field.</param>
    /// <param name="length">The number of bits allocated for the property.</param>
    /// <param name="propertySymbol">The symbol representing the property being extracted.</param>
    /// <returns>
    /// An <see cref="ExpressionSyntax"/> representing the extraction logic for the bitfield property.
    /// </returns>
    protected virtual ExpressionSyntax GetBitwiseExpression(
        SpecialType fieldType,
        string fieldName,
        byte start,
        byte length,
        IPropertySymbol propertySymbol)
    {
        // Determine if this property is a boolean (only 1 bit).
        // If so, we generate the expression:  ((field >> start) & 1) == 1
        // Otherwise, we generate: (TargetType)((field >> start) & ((1 << length) - 1))

        var propertyTypeSymbol = propertySymbol.Type;

        if (propertyTypeSymbol.SpecialType == SpecialType.System_Boolean && length == 1)
        {
            // Build: ((fieldName >> start) & 1) == 1
            return BoolGetterExpression(fieldType, fieldName, start);
        }
        else
        {
            // Build: (TargetType)(((fieldName >> start) & ((1 << length) - 1)))
            return MultiBitGetterExpression(fieldType, fieldName, start, length, propertyTypeSymbol);
        }
    }

    /// <summary>
    /// Creates an expression to set or initialize a value into a bit field using bitwise operations.
    /// </summary>
    /// <param name="bitFieldInfo">
    /// Information about the bitfield property, including its field name, start, length, and symbol.
    /// </param>
    /// <returns>
    /// An <see cref="ExpressionSyntax"/> representing the bitwise operation to set or initialize the value.
    /// </returns>
    protected virtual ExpressionSyntax SetOrInitBitwiseExpression(BitFieldPropertyInfoRequest bitFieldInfo)
    {
        var fieldType = bitFieldInfo.BitsSpan.FieldRequest.FieldType;
        var fieldName = bitFieldInfo.BitsSpan.FieldRequest.Name;
        var start = bitFieldInfo.BitsSpan.Start;
        var length = bitFieldInfo.BitsSpan.Length;
        var propertySymbol = bitFieldInfo.PropertySymbol;

        return SetOrInitBitwiseExpression(fieldType, fieldName, start, length, propertySymbol, "value");
    }

    /// <summary>
    /// Builds the expression: ((fieldName >> start) & 1) == 1
    /// </summary>
    protected virtual ExpressionSyntax BoolGetterExpression(SpecialType fieldType, string fieldName, byte start)
    {
        // Step by step:
        // 1) (fieldName >> start)
        var shifted = BinaryExpression(
            SyntaxKind.RightShiftExpression,
            IdentifierName(fieldName),
            LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(start))
        );

        // 2) ((fieldName >> start) & 1)
        var bitAnd1 = BinaryExpression(
            SyntaxKind.BitwiseAndExpression,
            ParenthesizedExpression(shifted),
            LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(1))
        );

        // 3) ((fieldName >> start) & 1) == 1
        var equals1 = BinaryExpression(
            SyntaxKind.EqualsExpression,
            ParenthesizedExpression(bitAnd1),
            LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(1))
        );

        return equals1;
    }

    /// <summary>
    /// Builds the expression: (TargetType)(((fieldName >> start) & ((1 << length) - 1)))
    /// </summary>
    protected virtual ExpressionSyntax MultiBitGetterExpression(
        SpecialType fieldType,
        string fieldName,
        byte start,
        byte length,
        ITypeSymbol propertyTypeSymbol)
    {
        // Example final shape:
        // (PropertyType)(
        //     (
        //       (fieldName >> start)
        //       & ((1 << length) - 1)
        //     )
        // )

        // 1) (fieldName >> start)
        var shifted = BinaryExpression(
            SyntaxKind.RightShiftExpression,
            IdentifierName(fieldName),
            LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(start))
        );

        // 2) ((1 << length) - 1)
        var mask = BinaryExpression(
            SyntaxKind.SubtractExpression,
            ParenthesizedExpression(
                BinaryExpression(
                    SyntaxKind.LeftShiftExpression,
                    LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(1)),
                    LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(length))
                )
            ),
            LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(1))
        );

        // 3) ((fieldName >> start) & mask)
        var masked = BinaryExpression(
            SyntaxKind.BitwiseAndExpression,
            ParenthesizedExpression(shifted),
            ParenthesizedExpression(mask)
        );

        // 4) (PropertyType)(masked)
        // We need a TypeSyntax for the cast. For simplicity, let's do:
        // IdentifierName(propertyTypeSymbol.ToDisplayString())
        // But you might want a more robust approach in real code.
        var propertyTypeName = propertyTypeSymbol.ToDisplayString();
        var cast = CastExpression(
            // If this is a known primitive, you could also consider using PredefinedType(...)
            IdentifierName(propertyTypeName),
            ParenthesizedExpression(masked)
        );

        return cast;
    }

    /// <summary>
    /// Creates an expression to set or initialize a value into a bit field using bitwise operations,
    /// with explicit parameters for the field name, start position, length, and value variable name.
    /// </summary>
    /// <param name="fieldName">The name of the field that holds the bitfield.</param>
    /// <param name="start">The starting bit position within the field.</param>
    /// <param name="length">The number of bits allocated for the property.</param>
    /// <param name="propertySymbol">The symbol representing the property being set or initialized.</param>
    /// <param name="valueVariableName">The name of the variable representing the value to set or initialize.</param>
    /// <returns>
    /// An <see cref="ExpressionSyntax"/> representing the bitwise operation to set or initialize the value.
    /// </returns>
    protected virtual ExpressionSyntax SetOrInitBitwiseExpression(SpecialType fieldType, string fieldName, byte start, byte length, IPropertySymbol propertySymbol, string valueVariableName)
    {
        var propertyTypeSyntax = GetTypeSyntax(propertySymbol);

        // If the field represents a 1-bit boolean value
        if (propertySymbol.Type.SpecialType == SpecialType.System_Boolean)
        {
            // Generate a conditional assignment expression
            return ConditionalAssignmentExpression(fieldType, fieldName, start, valueVariableName);
        }
        else
        {
            // Generate an assignment expression for multi-bit fields
            return MultiBitAssignmentExpression(fieldType, fieldName, start, length, valueVariableName, propertyTypeSyntax);
        }
    }

    /// <summary>
    /// Generates a conditional assignment expression for 1-bit (boolean) fields.
    /// </summary>
    /// <param name="fieldName">The name of the field that holds the bitfield.</param>
    /// <param name="start">The starting bit position within the field.</param>
    /// <param name="valueVariableName">The name of the variable representing the value to set.</param>
    /// <returns>
    /// An <see cref="ExpressionSyntax"/> representing the conditional assignment expression for a boolean field.
    /// </returns>
    protected virtual ExpressionSyntax ConditionalAssignmentExpression(SpecialType fieldType, string fieldName, byte start, string valueVariableName)
    {
        var fieldTypeSyntax = GetTypeSyntaxFromSpecialType(fieldType);

        // Generate the following expression:
        // fieldName =  (fieldTypeSyntax)(valueVariableName ? (fieldName | (1 << start)) : (fieldName & ~(1 << start)))
        return AssignmentExpression(
            SyntaxKind.SimpleAssignmentExpression,
            IdentifierName(fieldName),
            CastExpression(fieldTypeSyntax,
                ParenthesizedExpression(
                    ConditionalExpression(
                        IdentifierName(valueVariableName),
                        ParenthesizedExpression(
                            BinaryExpression(
                                SyntaxKind.BitwiseOrExpression,
                                IdentifierName(fieldName),
                                BinaryExpression(
                                    SyntaxKind.LeftShiftExpression,
                                    LiteralWithSpecialType(1, fieldType),
                                    LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(start))
                                )
                            )
                        ),
                        ParenthesizedExpression(
                            BinaryExpression(
                                SyntaxKind.BitwiseAndExpression,
                                IdentifierName(fieldName),
                                PrefixUnaryExpression(
                                    SyntaxKind.BitwiseNotExpression,
                                    ParenthesizedExpression(
                                        BinaryExpression(
                                            SyntaxKind.LeftShiftExpression,
                                            LiteralWithSpecialType(1, fieldType),
                                            LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(start))
                                        )
                                    )
                                )
                            )
                        )
                    )
                )
            )
        );
    }

    /// <summary>
    /// Generates an assignment expression for multi-bit fields.
    /// </summary>
    /// <param name="fieldName">The name of the field that holds the bitfield.</param>
    /// <param name="start">The starting bit position within the field.</param>
    /// <param name="length">The number of bits allocated for the property.</param>
    /// <param name="valueVariableName">The name of the variable representing the value to set.</param>
    /// <returns>
    /// An <see cref="ExpressionSyntax"/> representing the assignment expression for a multi-bit field.
    /// </returns>
    protected virtual ExpressionSyntax MultiBitAssignmentExpression(SpecialType fieldType, string fieldName, byte start, byte length, string valueVariableName, TypeSyntax propertyType)
    {
        // Left operand: (fieldName & ~(((1 << length) - 1) << start))
        var leftAndMask = BinaryExpression(
            SyntaxKind.BitwiseAndExpression,
            IdentifierName(fieldName),
            PrefixUnaryExpression(
                SyntaxKind.BitwiseNotExpression,
                ParenthesizedExpression(
                    BinaryExpression(
                        SyntaxKind.LeftShiftExpression,
                        ParenthesizedExpression(
                            BinaryExpression(
                                SyntaxKind.SubtractExpression,
                                ParenthesizedExpression(
                                    BinaryExpression(
                                        SyntaxKind.LeftShiftExpression,
                                        LiteralWithSpecialType(1, fieldType),
                                        LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(length))
                                    )
                                ),
                                LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(1))
                            )
                        ),
                        LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(start))
                    )
                )
            )
        );

        // Right operand: ((valueVariableName & ((1 << length) - 1)) << start)
        var rightMaskAndShift = BinaryExpression(
            SyntaxKind.LeftShiftExpression,
            ParenthesizedExpression(
                BinaryExpression(
                    SyntaxKind.BitwiseAndExpression,
                    IdentifierName(valueVariableName),
                    ParenthesizedExpression(
                        BinaryExpression(
                            SyntaxKind.SubtractExpression,
                            ParenthesizedExpression(
                                BinaryExpression(
                                    SyntaxKind.LeftShiftExpression,
                                    LiteralWithSpecialType(1, fieldType),
                                    LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(length))
                                )
                            ),
                            LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(1))
                        )
                    )
                )
            ),
            LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(start))
        );

        var fieldTypeSyntax = GetTypeSyntaxFromSpecialType(fieldType);

        // Combine left and right operands using a bitwise OR
        return AssignmentExpression(
            SyntaxKind.SimpleAssignmentExpression,
            IdentifierName(fieldName),
            CastExpression(
                fieldTypeSyntax,
                ParenthesizedExpression(
                    BinaryExpression(
                        SyntaxKind.BitwiseOrExpression,
                        leftAndMask,
                        rightMaskAndShift
                    )
                )
            )
        );
    }

    protected static TypeSyntax GetTypeSyntax(IPropertySymbol propertyTypeSymbol) => GetTypeSyntaxFromSpecialType(propertyTypeSymbol.Type.SpecialType);

    protected static TypeSyntax GetTypeSyntaxFromSpecialType(SpecialType specialType) => specialType switch
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

    protected static TypeSyntax ToSignedVariantSyntax(SpecialType specialType) => GetTypeSyntaxFromSpecialType(ToSignedVariant(specialType));

    protected static SpecialType ToSignedVariant(SpecialType specialType) => specialType switch
    {
        SpecialType.System_Byte => SpecialType.System_SByte,
        SpecialType.System_UInt16 => SpecialType.System_Int16,
        SpecialType.System_UInt32 => SpecialType.System_Int32,
        SpecialType.System_UInt64 => SpecialType.System_Int64,
        _ => specialType
    };

    protected static LiteralExpressionSyntax LiteralWithSpecialType(decimal value, SpecialType specialType) => specialType switch
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
    protected static LiteralExpressionSyntax NumericLiteral(int value) => LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(value));
    protected static LiteralExpressionSyntax NumericLiteral(uint value) => LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(value));
    protected static LiteralExpressionSyntax NumericLiteral(long value) => LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(value));
    protected static LiteralExpressionSyntax NumericLiteral(ulong value) => LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(value));
    protected static LiteralExpressionSyntax NumericLiteral(decimal value) => LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(value));
}