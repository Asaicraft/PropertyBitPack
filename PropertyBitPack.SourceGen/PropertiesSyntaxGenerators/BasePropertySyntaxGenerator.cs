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

    private readonly PropertyBitPackGeneratorContext _propertyBitPackGeneratorContext = propertyBitPackGeneratorContext;

    /// <summary>
    /// Gets the context for the property bit pack generator.
    /// </summary>
    public PropertyBitPackGeneratorContext PropertyBitPackGeneratorContext => _propertyBitPackGeneratorContext;

    /// <inheritdoc/>
    public PropertyDeclarationSyntax? GenerateProperty(IGenerateSourceRequest sourceRequest, BitFieldPropertyInfoRequest bitFieldPropertyInfoRequest, out ImmutableArray<MemberDeclarationSyntax> additionalMember)
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
    protected virtual bool IsCandidate(IGenerateSourceRequest sourceRequest, BitFieldPropertyInfoRequest bitFieldPropertyInfoRequest)
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
    protected virtual PropertyDeclarationSyntax? GeneratePropertyCore(IGenerateSourceRequest sourceRequest, BitFieldPropertyInfoRequest bitFieldPropertyInfoRequest)
    {
        var getterBlock = GetterBlockSyntax(bitFieldPropertyInfoRequest);
        var setterBlock = SetterBlockSyntax(bitFieldPropertyInfoRequest);

        var modifiers = bitFieldPropertyInfoRequest.SetterOrInitModifiers;

        var getter = BitwiseSyntaxHelpers.Getter(getterBlock);
        var setterOrInitter = bitFieldPropertyInfoRequest.HasInitOrSet
            ? bitFieldPropertyInfoRequest.IsInit
                ? BitwiseSyntaxHelpers.Initter(modifiers, setterBlock)
                : BitwiseSyntaxHelpers.Setter(modifiers, setterBlock)
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
    public virtual PropertyDeclarationSyntax? GeneratePropertyCore(IGenerateSourceRequest sourceRequest, BitFieldPropertyInfoRequest bitFieldPropertyInfoRequest, out ImmutableArray<MemberDeclarationSyntax> additionalMember)
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
    public virtual BlockSyntax SetterBlockSyntax(BitFieldPropertyInfoRequest bitFieldPropertyInfoRequest, 
        string valueVariableName = "value", 
        string maxValueVariableName = "maxValue_",
        string clampedValueVariableName = "clamped_")
    {
        var fieldType = bitFieldPropertyInfoRequest.BitsSpan.FieldRequest.FieldType;
        var fieldTypeSyntax = BitwiseSyntaxHelpers.GetTypeSyntaxFromSpecialType(fieldType);
        var propertyTypeSyntax = bitFieldPropertyInfoRequest.PropertyDeclarationSyntax.Type;
        var propertySymbol = bitFieldPropertyInfoRequest.PropertySymbol;
        var fieldName = bitFieldPropertyInfoRequest.BitsSpan.FieldRequest.Name;
        var start = bitFieldPropertyInfoRequest.BitsSpan.Start;
        var length = bitFieldPropertyInfoRequest.BitsSpan.Length;

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
                valueVariableName
            );

            statements.Add(ExpressionStatement(boolAssignment));
        }
        else
        {
            // 1) const T maxValue_ = ((1 << length) - 1);
            var maxValueDecl = BitwiseSyntaxHelpers.BuildConstMaskDeclaration(maxValueVariableName, fieldType, length);
            statements.Add(maxValueDecl);

            // 2) var clamped_ = Math.Min((T)value, maxValue_);
            var clampedDecl = BitwiseSyntaxHelpers.BuildClampedVarDeclaration(
                clampedValueVariableName,
                fieldType,
                IdentifierName(valueVariableName),
                maxValueVariableName
            );
            statements.Add(clampedDecl);

            // 3) fieldName = (fieldType)((fieldName & ~...) | ... )  // or whatever logic
            var assignmentExpr = SetOrInitBitwiseExpression(
                fieldType,
                fieldName,
                start,
                length,
                propertySymbol,
                clampedValueVariableName
            );
            statements.Add(ExpressionStatement(assignmentExpr));
        }

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
    public virtual ExpressionSyntax SetOrInitBitwiseExpression(BitFieldPropertyInfoRequest bitFieldInfo, string valueVariableName = "value")
    {
        var fieldType = bitFieldInfo.BitsSpan.FieldRequest.FieldType;
        var fieldName = bitFieldInfo.BitsSpan.FieldRequest.Name;
        var start = bitFieldInfo.BitsSpan.Start;
        var length = bitFieldInfo.BitsSpan.Length;
        var propertySymbol = bitFieldInfo.PropertySymbol;

        return SetOrInitBitwiseExpression(fieldType, fieldName, start, length, propertySymbol, valueVariableName);
    }

    /// <summary>
    /// Builds the expression: ((fieldName >> start) & 1) == 1
    /// </summary>
    public virtual ExpressionSyntax BoolGetterExpression(SpecialType fieldType, string fieldName, byte start)
    {
        // Step 1: shift the fieldName by 'start' bits to the right
        var shifted = BitwiseSyntaxHelpers.BuildRightShift(
            BitwiseSyntaxHelpers.ThisAccessExpression(fieldName),
            start
        );

        // Step 2: (shifted & 1)
        // Use LiteralWithSpecialType(1, fieldType) for correct integer literal based on the field's type
        var bitAnd1 = BitwiseSyntaxHelpers.BuildAnd(
            shifted,
            BitwiseSyntaxHelpers.LiteralWithSpecialType(1, fieldType)
        );

        // Step 3: compare to 1 -> ((shifted & 1) == 1)
        var equals1 = BinaryExpression(
            SyntaxKind.EqualsExpression,
            ParenthesizedExpression(bitAnd1),
            BitwiseSyntaxHelpers.LiteralWithSpecialType(1, fieldType)
        );

        return equals1;
    }

    /// <summary>
    /// Builds the expression: (TargetType)(((fieldName >> start) & ((1 << length) - 1)))
    /// </summary>
    public virtual ExpressionSyntax MultiBitGetterExpression(
        SpecialType fieldType,
        string fieldName,
        byte start,
        byte length,
        ITypeSymbol propertyTypeSymbol)
    {
        // Step 1: (fieldName >> start)
        var shifted = BitwiseSyntaxHelpers.BuildRightShift(
            BitwiseSyntaxHelpers.ThisAccessExpression(fieldName),
            start
        );

        // Step 2: Apply mask of length bits to the shifted value:
        //         (shifted & ((1 << length) - 1))
        var masked = BitwiseSyntaxHelpers.BuildMaskExtract(shifted, fieldType, length);

        // Step 3: Cast to the target property type:
        //         (PropertyTypeSymbol)(masked)
        var castExpr = BitwiseSyntaxHelpers.BuildCast(propertyTypeSymbol, masked);

        return castExpr;
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
    public virtual ExpressionSyntax SetOrInitBitwiseExpression(SpecialType fieldType, string fieldName, byte start, byte length, IPropertySymbol propertySymbol, string valueVariableName)
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
    public virtual ExpressionSyntax ConditionalAssignmentExpression(SpecialType fieldType, string fieldName, byte start, string valueVariableName)
    {
        // fieldName | (1 << start)
        var setBit = BitwiseSyntaxHelpers.BuildOr(
            BitwiseSyntaxHelpers.ThisAccessExpression(fieldName),
            BitwiseSyntaxHelpers.BuildMaskShifted(fieldType, 1, start) // (1 << start)
        );

        // fieldName & ~(1 << start)
        var clearBit = BitwiseSyntaxHelpers.BuildLeftAndMask(
            BitwiseSyntaxHelpers.ThisAccessExpression(fieldName),
            BitwiseSyntaxHelpers.BuildMaskShifted(fieldType, 1, start) // (1 << start)
        );

        // (valueVariableName ? setBit : clearBit)
        var conditionExpression = ConditionalExpression(
            IdentifierName(valueVariableName),
            ParenthesizedExpression(setBit),
            ParenthesizedExpression(clearBit)
        );

        return BitwiseSyntaxHelpers.BuildThisAssignment(
            fieldName,
            conditionExpression,
            fieldType
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
    public virtual ExpressionSyntax MultiBitAssignmentExpression(SpecialType fieldType, string fieldName, byte start, byte length, string valueVariableName, TypeSyntax propertyType)
    {
        // Left operand:
        //   (fieldName & ~(((1 << length) - 1) << start))
        var leftMask = BitwiseSyntaxHelpers.BuildLeftAndMask(
            BitwiseSyntaxHelpers.ThisAccessExpression(fieldName),
            BitwiseSyntaxHelpers.BuildMaskShifted(fieldType, length, start)
        );

        // Right operand:
        //   ((valueVariableName & ((1 << length) - 1)) << start)
        var rightMask = BitwiseSyntaxHelpers.BuildValueAndShift(
            IdentifierName(valueVariableName),
            fieldType,
            length,
            start
        );

        // Combine left and right operands using a bitwise OR
        //   leftMask | rightMask
        var combinedExpr = BitwiseSyntaxHelpers.BuildOr(leftMask, rightMask);

        return BitwiseSyntaxHelpers.BuildThisAssignment(
            fieldName,
            combinedExpr,
            fieldType
        );
    }

    public static TypeSyntax GetTypeSyntax(IPropertySymbol propertyTypeSymbol) => BitwiseSyntaxHelpers.GetTypeSyntaxFromSpecialType(propertyTypeSymbol.Type.SpecialType);
}