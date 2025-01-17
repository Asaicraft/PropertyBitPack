using CommunityToolkit.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PropertyBitPack.SourceGen.Collections;
using PropertyBitPack.SourceGen.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace PropertyBitPack.SourceGen.PropertiesSyntaxGenerators;

/// <summary>
/// Provides a base class for generating syntax related to property bit-packing.
/// </summary>
/// <remarks>
/// This class serves as an abstraction for generating source code for properties and fields 
/// based on bit-packing logic. It handles context binding, file generation, and syntax 
/// construction for classes, properties, fields, and namespaces. Derived classes must 
/// override <see cref="GenerateCore"/> to provide specific implementation logic for processing 
/// requests.
/// </remarks>
/// <seealso cref="IPropertiesSyntaxGenerator"/>
/// <seealso cref="IContextBindable"/>
internal abstract class BasePropertiesSyntaxGenerator : IPropertiesSyntaxGenerator, IContextBindable
{
    private ImmutableArray<IPropertySyntaxGenerator> _propertySyntaxGenerators = [];

    /// <summary>
    /// Gets or sets the context used for property bit-packing generation.
    /// </summary>
    /// <remarks>
    /// The context provides access to shared resources, such as parsers, aggregators, and syntax generators,
    /// that are required during the source generation process.
    /// </remarks>
    public PropertyBitPackGeneratorContext PropertyBitPackGeneratorContext
    {
        get; set;
    } = null!;

    /// <summary>
    /// Gets the collection of property syntax generators initialized by the context.
    /// </summary>
    /// <remarks>
    /// The generators are created by invoking the <see cref="GenereatePropertySyntaxGenerators"/> method
    /// during the binding of the context.
    /// </remarks>
    public ImmutableArray<IPropertySyntaxGenerator> PropertySyntaxGenerators => _propertySyntaxGenerators;

    /// <summary>
    /// Binds the specified context to the generator and initializes the property syntax generators.
    /// </summary>
    /// <param name="context">
    /// The <see cref="PropertyBitPackGeneratorContext"/> to bind and use for initialization.
    /// </param>
    /// <remarks>
    /// This method is typically called during the setup phase to configure the generator with the appropriate context.
    /// </remarks>
    public void BindContext(PropertyBitPackGeneratorContext context)
    {
        PropertyBitPackGeneratorContext = context;

        _propertySyntaxGenerators = GenereatePropertySyntaxGenerators(context);
    }

    /// <summary>
    /// Generates the property syntax generators based on the provided context.
    /// </summary>
    /// <param name="context">
    /// The <see cref="PropertyBitPackGeneratorContext"/> used to initialize the generators.
    /// </param>
    /// <returns>
    /// An <see cref="ImmutableArray{T}"/> containing the initialized property syntax generators.
    /// </returns>
    /// <remarks>
    /// This method can be overridden by derived classes to customize the initialization logic for generators.
    /// </remarks>
    protected virtual ImmutableArray<IPropertySyntaxGenerator> GenereatePropertySyntaxGenerators(PropertyBitPackGeneratorContext context)
    {
        var generators = new IPropertySyntaxGenerator[]
        {
            new PropertySyntaxGenerator(context)
        };

        return Unsafe.As<IPropertySyntaxGenerator[], ImmutableArray<IPropertySyntaxGenerator>>(ref generators);
    }

    /// <inheritdoc/>
    public ImmutableArray<FileGeneratorRequest> Generate(ILinkedList<GenerateSourceRequest> requests)
    {
        using var fileGeneratorRequestsBuilder = ImmutableArrayBuilder<FileGeneratorRequest>.Rent();

        GenerateCore(requests, in fileGeneratorRequestsBuilder);

        if (fileGeneratorRequestsBuilder.Count == 0)
        {
            return [];
        }

        return fileGeneratorRequestsBuilder.ToImmutable();
    }

    protected abstract void GenerateCore(ILinkedList<GenerateSourceRequest> requests, in ImmutableArrayBuilder<FileGeneratorRequest> immutableArrayBuilder);

    /// <summary>
    /// Generates a field declaration syntax for the specified field request.
    /// </summary>
    /// <param name="generateSourceRequest">The source generation request containing context information.</param>
    /// <param name="fieldRequest">The field request containing details about the field to generate.</param>
    /// <returns>A <see cref="FieldDeclarationSyntax"/> representing the generated field.</returns>
    protected virtual FieldDeclarationSyntax GenerateField(GenerateSourceRequest generateSourceRequest, FieldRequest fieldRequest)
    {
        Debug.Assert(!fieldRequest.IsExist);

        var fieldType = GetTypeSyntaxFrom(fieldRequest.FieldType);

        var fieldDeclaration = FieldDeclaration(
            VariableDeclaration(
                fieldType,
                SingletonSeparatedList(
                    VariableDeclarator(
                        fieldRequest.Name
                    )
                )
            )
        );

        return fieldDeclaration;
    }

    /// <summary>
    /// Generates a property declaration syntax for the specified bit field property request.
    /// </summary>
    /// <param name="generateSourceRequest">The source generation request containing context information.</param>
    /// <param name="bitFieldPropertyInfoRequest">The property request containing details about the bit field property to generate.</param>
    /// <returns>A <see cref="PropertyDeclarationSyntax"/> representing the generated property.</returns>
    protected virtual PropertyDeclarationSyntax GenerateProperty(GenerateSourceRequest generateSourceRequest, BitFieldPropertyInfoRequest bitFieldPropertyInfoRequest)
    {
        return GenerateProperty(generateSourceRequest, bitFieldPropertyInfoRequest, out _);
    }

    /// <summary>
    /// Generates a property declaration syntax and additional members for the specified bit field property request.
    /// </summary>
    /// <param name="generateSourceRequest">The source generation request containing context information.</param>
    /// <param name="bitFieldPropertyInfoRequest">The property request containing details about the bit field property to generate.</param>
    /// <param name="additionalMembers">Additional members generated as part of the property generation.</param>
    /// <returns>A <see cref="PropertyDeclarationSyntax"/> representing the generated property.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the property cannot be generated.</exception>
    protected virtual PropertyDeclarationSyntax GenerateProperty(GenerateSourceRequest generateSourceRequest, BitFieldPropertyInfoRequest bitFieldPropertyInfoRequest, out ImmutableArray<MemberDeclarationSyntax> additionalMembers)
    {
        PropertyDeclarationSyntax? propertyDeclaration = null;

        foreach (var propertyGenerator in PropertySyntaxGenerators)
        {
            propertyDeclaration = propertyGenerator.GenerateProperty(generateSourceRequest, bitFieldPropertyInfoRequest, out additionalMembers);
            if (propertyDeclaration is not null)
            {
                break;
            }
        }

        if (propertyDeclaration is null)
        {
            ThrowHelper.ThrowUnreachableException($"Cannot generate property for {bitFieldPropertyInfoRequest.PropertySymbol.Name}");
        }

        return propertyDeclaration;
    }



    /// <summary>
    /// Generates the filename for the source code file based on the given request.
    /// </summary>
    /// <param name="request">The request containing information about the fields used in the generated source.</param>
    /// <returns>
    /// A string representing the filename, including field names concatenated with underscores
    /// and the suffix ".BitPack.g.cs".
    /// </returns>
    protected virtual string GetFileName(GenerateSourceRequest request)
    {
        var owner = request.Properties[0].Owner;

        using var stingBuilderRented = StringBuildersPool.Rent();
        var stringBuilder = stingBuilderRented.StringBuilder;

        stringBuilder.Append(owner.ToDisplayString());
        stringBuilder.Append('.');

        // Iterate through all fields and append their names to the filename
        for (var i = 0; i < request.Fields.Length; i++)
        {
            var fieldRequests = request.Fields[i];
            stringBuilder.Append(fieldRequests.Name);

            // Add separator for all but the last field
            if (i != request.Fields.Length - 1)
            {
                stringBuilder.Append("___");
            }
        }

        // Append the suffix to the filename
        stringBuilder.Append(".BitPack.g.cs");

        return stringBuilder.ToString();
    }

    /// <summary>
    /// Generates a <see cref="CompilationUnitSyntax"/> that represents the entire source file.
    /// </summary>
    /// <param name="request">The request containing field and property information.</param>
    /// <param name="memberDeclarationSyntaxes">
    /// A collection of <see cref="MemberDeclarationSyntax"/> representing the class members.
    /// </param>
    /// <returns>
    /// A <see cref="CompilationUnitSyntax"/> containing the namespace, using directives, and class declaration.
    /// </returns>
    protected virtual CompilationUnitSyntax GenerateCompilationUnit(
        GenerateSourceRequest request,
        IEnumerable<MemberDeclarationSyntax> memberDeclarationSyntaxes)
    {
        var owner = request.Properties[0].Owner;
        var ownerNamespace = owner.ContainingNamespace;

        // Generate the class declaration based on the request
        var classDeclaration = GenerateClassDeclaration(request, memberDeclarationSyntaxes);

        // Create a namespace declaration wrapping the class
        var namespaceDeclaration = NamespaceDeclaration(
            IdentifierName(ownerNamespace.Name),
            default,
            default,
            SingletonList<MemberDeclarationSyntax>(classDeclaration)
        );

        // Add using directives
        using var usingsRented = ListsPool.Rent<UsingDirectiveSyntax>();
        var usings = usingsRented.List;
        usings.Add(UsingDirective(IdentifierName("System")));

        // Create the compilation unit
        return CompilationUnit(
            default,
            List(usings),
            default,
            SingletonList<MemberDeclarationSyntax>(namespaceDeclaration)
        );
    }

    /// <summary>
    /// Generates a <see cref="ClassDeclarationSyntax"/> for the owner class described in the request.
    /// </summary>
    /// <param name="request">The request containing property and field information for the class.</param>
    /// <param name="memberDeclarationSyntaxes">
    /// A collection of <see cref="MemberDeclarationSyntax"/> representing the class members.
    /// </param>
    /// <returns>
    /// A <see cref="ClassDeclarationSyntax"/> representing a partial class definition for the owner.
    /// </returns>
    protected virtual ClassDeclarationSyntax GenerateClassDeclaration(
        GenerateSourceRequest request,
        IEnumerable<MemberDeclarationSyntax> memberDeclarationSyntaxes)
    {
        var owner = request.Properties[0].Owner;

        // Create a partial class declaration with the provided members
        return ClassDeclaration(
            List<AttributeListSyntax>(),
            TokenList(Token(SyntaxKind.PartialKeyword)),
            Token(SyntaxKind.ClassKeyword),
            Identifier(owner.Name),
            null,
            null,
            List<TypeParameterConstraintClauseSyntax>(),
            Token(SyntaxKind.OpenBraceToken),
            List(memberDeclarationSyntaxes),
            Token(SyntaxKind.CloseBraceToken),
            default
        );
    }

    /// <summary>
    /// Gets the corresponding <see cref="TypeSyntax"/> for a given <see cref="SpecialType"/>.
    /// </summary>
    /// <param name="specialType">The <see cref="SpecialType"/> value to convert.</param>
    /// <returns>
    /// A <see cref="TypeSyntax"/> representing the type keyword associated with the given <paramref name="specialType"/>.
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// Thrown if the provided <paramref name="specialType"/> is not supported.
    /// </exception>
    protected static TypeSyntax GetTypeSyntaxFrom(SpecialType specialType) => specialType switch
    {
        SpecialType.System_Byte => PredefinedType(Token(SyntaxKind.ByteKeyword)),
        SpecialType.System_SByte => PredefinedType(Token(SyntaxKind.SByteKeyword)),
        SpecialType.System_Int16 => PredefinedType(Token(SyntaxKind.ShortKeyword)),
        SpecialType.System_UInt16 => PredefinedType(Token(SyntaxKind.UShortKeyword)),
        SpecialType.System_Int32 => PredefinedType(Token(SyntaxKind.IntKeyword)),
        SpecialType.System_UInt32 => PredefinedType(Token(SyntaxKind.UIntKeyword)),
        SpecialType.System_Int64 => PredefinedType(Token(SyntaxKind.LongKeyword)),
        SpecialType.System_UInt64 => PredefinedType(Token(SyntaxKind.ULongKeyword)),
        SpecialType.System_Boolean => PredefinedType(Token(SyntaxKind.BoolKeyword)),

        _ => throw new NotSupportedException()
    };
}
