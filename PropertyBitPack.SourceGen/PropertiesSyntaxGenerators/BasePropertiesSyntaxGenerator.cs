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
internal abstract class BasePropertiesSyntaxGenerator : IPropertiesSyntaxGenerator, IContextBindable
{

    private ImmutableArray<IPropertySyntaxGenerator> _propertySyntaxGenerators = [];

    public PropertyBitPackGeneratorContext PropertyBitPackGeneratorContext
    {
        get; set;
    } = null!;

    public ImmutableArray<IPropertySyntaxGenerator> PropertySyntaxGenerators => _propertySyntaxGenerators;

    public void BindContext(PropertyBitPackGeneratorContext context)
    {
        PropertyBitPackGeneratorContext = context;

        _propertySyntaxGenerators = GenereatePropertySyntaxGenerators(context);
    }

    protected virtual ImmutableArray<IPropertySyntaxGenerator> GenereatePropertySyntaxGenerators(PropertyBitPackGeneratorContext context)
    {
        var generators = new IPropertySyntaxGenerator[]
        {
            new PropertySyntaxGenerator(context)
        };

        return Unsafe.As<IPropertySyntaxGenerator[], ImmutableArray<IPropertySyntaxGenerator>>(ref generators);
    }

    public ImmutableArray<FileGeneratorRequest> Generate(ILinkedList<GenerateSourceRequest> requests)
    {
        using var fileGeneratorRequestsBuilder = ImmutableArrayBuilder<FileGeneratorRequest>.Rent();

        GenerateCore(requests, fileGeneratorRequestsBuilder);

        if (requests.Count == 0)
        {
            return [];
        }

        return fileGeneratorRequestsBuilder.ToImmutable();
    }

    protected abstract void GenerateCore(ILinkedList<GenerateSourceRequest> requests, in ImmutableArrayBuilder<FileGeneratorRequest> immutableArrayBuilder);

    protected virtual FieldDeclarationSyntax GenerateField(GenerateSourceRequest generateSourceRequest, FieldRequest fieldRequest)
    {
        Debug.Assert(!fieldRequest.IsExist);

        var fieldType = GetTypeSyntaxFrom(fieldRequest.FieldType);

        var fieldDeclaration = SyntaxFactory.FieldDeclaration(
            SyntaxFactory.VariableDeclaration(
                fieldType,
                SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.VariableDeclarator(
                        fieldRequest.Name
                    )
                )
            )
        );

        return fieldDeclaration;
    }

    protected virtual PropertyDeclarationSyntax GenerateProperty(GenerateSourceRequest generateSourceRequest, BitFieldPropertyInfoRequest bitFieldPropertyInfoRequest)
    {
        return GenerateProperty(generateSourceRequest, bitFieldPropertyInfoRequest, out _);
    }

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
        using var stingBuilderRented = StringBuildersPool.Rent();
        var stringBuilder = stingBuilderRented.StringBuilder;

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
        SpecialType.System_Byte => SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ByteKeyword)),
        SpecialType.System_SByte => SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.SByteKeyword)),
        SpecialType.System_Int16 => SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ShortKeyword)),
        SpecialType.System_UInt16 => SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.UShortKeyword)),
        SpecialType.System_Int32 => SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)),
        SpecialType.System_UInt32 => SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.UIntKeyword)),
        SpecialType.System_Int64 => SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.LongKeyword)),
        SpecialType.System_UInt64 => SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ULongKeyword)),
        SpecialType.System_Boolean => SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword)),

        _ => throw new NotSupportedException()
    };
}
