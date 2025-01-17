using CommunityToolkit.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
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
    /// A <see cref="CompilationUnitSyntax"/> containing the namespace, using directives, and type declaration.
    /// </returns>
    protected virtual CompilationUnitSyntax GenerateCompilationUnit(
        GenerateSourceRequest request,
        IEnumerable<MemberDeclarationSyntax> memberDeclarationSyntaxes)
    {
        var owner = request.Properties[0].Owner;
        var ownerNamespace = owner.ContainingNamespace;

        // Generate the type declaration based on the request
        var typeDeclorationSyntax = GenerateTypeDeclaration(request, memberDeclarationSyntaxes);

        // Create a namespace declaration wrapping the class
        var namespaceDeclaration = NamespaceDeclaration(
            IdentifierName(ownerNamespace.ToDisplayString()),
            default,
            default,
            SingletonList<MemberDeclarationSyntax>(typeDeclorationSyntax)
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
    /// Generates a type declaration syntax for the owner based on the type kind and whether it is a record.
    /// </summary>
    /// <param name="request">The request containing property and field information for the type.</param>
    /// <param name="memberDeclarationSyntaxes">
    /// A collection of <see cref="MemberDeclarationSyntax"/> representing the members of the type.
    /// </param>
    /// <returns>
    /// A <see cref="TypeDeclarationSyntax"/> representing the generated type declaration.
    /// </returns>
    /// <remarks>
    /// This method determines the type kind (class, struct) and whether the type is a record
    /// to delegate to the appropriate type-specific generation method.
    /// </remarks>
    protected virtual TypeDeclarationSyntax GenerateTypeDeclaration(
        GenerateSourceRequest request,
        IEnumerable<MemberDeclarationSyntax> memberDeclarationSyntaxes)
    {
        var owner = request.Properties[0].Owner;

        return owner.TypeKind switch
        {

            TypeKind.Class => !owner.IsRecord 
                ? GenerateClassDeclaration(request, memberDeclarationSyntaxes)
                : GenerateClassRecordDeclaration(request, memberDeclarationSyntaxes),

            TypeKind.Struct => !owner.IsRecord
                ? GenerateStructDeclaration(request, memberDeclarationSyntaxes)
                : GenerateStructRecordDeclaration(request, memberDeclarationSyntaxes),
            
            _ => throw new UnreachableException() 
        };
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

        return ClassDeclaration(
            attributeLists: List<AttributeListSyntax>(),
            modifiers: TokenList(Token(SyntaxKind.PartialKeyword)),
            keyword: Token(SyntaxKind.ClassKeyword),
            identifier: Identifier(owner.Name),
            typeParameterList: GenerateTypeParameterList(owner),
            baseList: null,
            constraintClauses: List<TypeParameterConstraintClauseSyntax>(),
            openBraceToken: Token(SyntaxKind.OpenBraceToken),
            members: List(memberDeclarationSyntaxes),
            closeBraceToken: Token(SyntaxKind.CloseBraceToken),
            semicolonToken: default
        );
    }

    /// <summary>
    /// Generates a partial struct declaration for the owner described in the request.
    /// </summary>
    /// <param name="request">The request containing property and field information for the struct.</param>
    /// <param name="memberDeclarationSyntaxes">
    /// A collection of <see cref="MemberDeclarationSyntax"/> representing the struct members.
    /// </param>
    /// <returns>
    /// A <see cref="StructDeclarationSyntax"/> representing the generated struct declaration.
    /// </returns>
    protected virtual StructDeclarationSyntax GenerateStructDeclaration(
        GenerateSourceRequest request,
        IEnumerable<MemberDeclarationSyntax> memberDeclarationSyntaxes)
    {
        var owner = request.Properties[0].Owner;

        return StructDeclaration(
            attributeLists: List<AttributeListSyntax>(),
            modifiers: TokenList(Token(SyntaxKind.PartialKeyword)),
            keyword: Token(SyntaxKind.StructKeyword),
            identifier: Identifier(owner.Name),
            typeParameterList: GenerateTypeParameterList(owner),
            baseList: null,
            constraintClauses: List<TypeParameterConstraintClauseSyntax>(),
            openBraceToken: Token(SyntaxKind.OpenBraceToken),
            members: List(memberDeclarationSyntaxes),
            closeBraceToken: Token(SyntaxKind.CloseBraceToken),
            semicolonToken: default
        );
    }

    /// <summary>
    /// Generates a partial record class declaration for the owner described in the request.
    /// </summary>
    /// <param name="request">The request containing property and field information for the record class.</param>
    /// <param name="memberDeclarationSyntaxes">
    /// A collection of <see cref="MemberDeclarationSyntax"/> representing the record class members.
    /// </param>
    /// <returns>
    /// A <see cref="RecordDeclarationSyntax"/> representing the generated record class declaration.
    /// </returns>
    protected virtual RecordDeclarationSyntax GenerateClassRecordDeclaration(
        GenerateSourceRequest request,
        IEnumerable<MemberDeclarationSyntax> memberDeclarationSyntaxes)
    {
        var owner = request.Properties[0].Owner;

        return RecordDeclaration(
            kind: SyntaxKind.RecordStructDeclaration,
            attributeLists: List<AttributeListSyntax>(),
            modifiers: TokenList(Token(SyntaxKind.PartialKeyword)),
            keyword: Token(SyntaxKind.RecordKeyword),
            classOrStructKeyword: default,
            identifier: Identifier(owner.Name),
            typeParameterList: GenerateTypeParameterList(owner),
            parameterList: null,
            baseList: null,
            constraintClauses: List<TypeParameterConstraintClauseSyntax>(),
            openBraceToken: Token(SyntaxKind.OpenBraceToken),
            members: List(memberDeclarationSyntaxes),
            closeBraceToken: Token(SyntaxKind.CloseBraceToken),
            semicolonToken: default
        );
    }

    /// <summary>
    /// Generates a partial record struct declaration for the owner described in the request.
    /// </summary>
    /// <param name="request">The request containing property and field information for the record struct.</param>
    /// <param name="memberDeclarationSyntaxes">
    /// A collection of <see cref="MemberDeclarationSyntax"/> representing the record struct members.
    /// </param>
    /// <returns>
    /// A <see cref="RecordDeclarationSyntax"/> representing the generated record struct declaration.
    /// </returns>
    protected virtual RecordDeclarationSyntax GenerateStructRecordDeclaration(
        GenerateSourceRequest request,
        IEnumerable<MemberDeclarationSyntax> memberDeclarationSyntaxes)
    {
        var owner = request.Properties[0].Owner;

        return RecordDeclaration(
            attributeLists: List<AttributeListSyntax>(),
            modifiers: TokenList(Token(SyntaxKind.PartialKeyword)),
            keyword: Token(SyntaxKind.RecordKeyword),
            identifier: Identifier(owner.Name),
            typeParameterList: GenerateTypeParameterList(owner),
            parameterList: null,
            baseList: null,
            constraintClauses: List<TypeParameterConstraintClauseSyntax>(),
            openBraceToken: Token(SyntaxKind.OpenBraceToken),
            members: List(memberDeclarationSyntaxes),
            closeBraceToken: Token(SyntaxKind.CloseBraceToken),
            semicolonToken: default
        );
    }

    /// <summary>
    /// Generates a type parameter list for the specified named type symbol.
    /// </summary>
    /// <param name="namedTypeSymbol">The named type symbol describing the type parameters.</param>
    /// <returns>
    /// A <see cref="TypeParameterListSyntax"/> containing the type parameters, or <c>null</c> if none exist.
    /// </returns>
    /// <remarks>
    /// This method iterates through the type parameters of the given symbol and generates a syntax node
    /// representing the list of type parameters.
    /// </remarks>
    protected virtual TypeParameterListSyntax? GenerateTypeParameterList(INamedTypeSymbol namedTypeSymbol)
    {
        var typeParameters = namedTypeSymbol.TypeParameters;

        if (typeParameters.Length == 0)
        {
            return null;
        }

        var rented = ListsPool.Rent<TypeParameterSyntax>();
        var typeParametersList = rented.List;

        for (var i = 0; i < typeParameters.Length; i++)
        {
            var typeParameter = typeParameters[i];
            typeParametersList.Add(TypeParameter(typeParameter.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)));
        }

        return TypeParameterList(SeparatedList(typeParametersList));
    }

    /// <summary>
    /// Processes the filtered candidate requests of the specified type, generating source text and file names
    /// for each, and removing them from the original collection.
    /// </summary>
    /// <typeparam name="T">
    /// The specific subtype of <see cref="GenerateSourceRequest"/> to process.
    /// </typeparam>
    /// <param name="requests">
    /// The linked list of all <see cref="GenerateSourceRequest"/> objects to be examined.
    /// </param>
    /// <param name="candidateRequests">
    /// An immutable array of requests of type <typeparamref name="T"/> that have been filtered out
    /// from the original collection.
    /// </param>
    /// <param name="fileGeneratorRequestsBuilder">
    /// The builder used to accumulate <see cref="FileGeneratorRequest"/> objects for final output.
    /// </param>
    /// <remarks>
    /// This method centralizes the logic for processing requests by generating source text and file names,
    /// ensuring that filtered requests are removed from the original collection and their results are added
    /// to the output builder.
    /// </remarks>
    protected virtual void ProccessCandidates<T>(
        ILinkedList<GenerateSourceRequest> requests,
        ImmutableArray<T> candidateRequests,
        in ImmutableArrayBuilder<FileGeneratorRequest> fileGeneratorRequestsBuilder
    )
        where T : GenerateSourceRequest
    {
        for (var i = 0; i < candidateRequests.Length; i++)
        {
            var candidateRequest = candidateRequests[i];

            // Generate the source code for this request
            var sourceText = GenerateSourceText(candidateRequest);

            // Generate the file name (e.g., "SomeFieldName.BitPack.g.cs")
            var fileName = GetFileName(candidateRequest);

            // Remove the request from the original collection
            requests.Remove(candidateRequest);

            // Add a new file generator request for the generated source
            fileGeneratorRequestsBuilder.Add(new FileGeneratorRequest(sourceText, fileName));
        }
    }


    /// <summary>
    /// Generates a <see cref="SourceText"/> representation for the specified request.
    /// </summary>
    /// <param name="generateSourceRequest">
    /// The request describing the fields and properties to generate in the source code.
    /// </param>
    /// <returns>
    /// A <see cref="SourceText"/> containing the fully generated C# syntax, including namespace,
    /// class definition, fields, and properties.
    /// </returns>
    /// <remarks>
    /// This method processes the given request by generating syntax nodes for fields, properties,
    /// and additional members if required. It combines these members into a complete compilation unit,
    /// normalizes the generated syntax, and returns the resulting source text.
    /// </remarks>
    protected virtual SourceText GenerateSourceText(GenerateSourceRequest generateSourceRequest)
    {
        // Generate any new fields needed (skip existing fields)
        using var fieldsRented = ListsPool.Rent<FieldDeclarationSyntax>();
        var fields = fieldsRented.List;

        for (var i = 0; i < generateSourceRequest.Fields.Length; i++)
        {
            var field = generateSourceRequest.Fields[i];

            // Skip already existing fields
            if (field.IsExist)
            {
                continue;
            }

            fields.Add(GenerateField(generateSourceRequest, field));
        }

        // Generate properties, collecting additional members as needed
        using var propertiesRented = ListsPool.Rent<PropertyDeclarationSyntax>();
        var properties = propertiesRented.List;

        using var additionalMembersRented = ListsPool.Rent<MemberDeclarationSyntax>();
        var additionalMembersList = additionalMembersRented.List;

        for (var i = 0; i < generateSourceRequest.Properties.Length; i++)
        {
            var propertyRequest = generateSourceRequest.Properties[i];

            properties.Add(GenerateProperty(generateSourceRequest, propertyRequest, out var additionalMembers));

            if (!additionalMembers.IsDefaultOrEmpty)
            {
                additionalMembersList.AddRange(additionalMembers);
            }
        }

        // Combine fields, properties, and additional members into a single list
        using var membersRented = ListsPool.Rent<MemberDeclarationSyntax>();
        var members = membersRented.List;

        members.AddRange(fields);
        members.AddRange(properties);
        members.AddRange(additionalMembersList);

        // Generate the compilation unit (namespace, class, etc.)
        var unit = GenerateCompilationUnit(generateSourceRequest, members);

        // Return the final normalized source text
        return unit.NormalizeWhitespace().GetText(Encoding.UTF8);
    }

    /// <summary>
    /// Filters the provided source generation requests to include only those of the specified type.
    /// </summary>
    /// <typeparam name="T">The specific type of requests to filter, derived from <see cref="GenerateSourceRequest"/>.</typeparam>
    /// <param name="generateSourceRequests">
    /// An <see cref="ImmutableArray{T}"/> containing the source generation requests to filter.
    /// </param>
    /// <returns>
    /// An <see cref="ImmutableArray{T}"/> of filtered requests of type <typeparamref name="T"/>.
    /// </returns>
    /// <remarks>
    /// This method uses a simple type check to collect requests of the specified type. 
    /// If the input array is empty or uninitialized, it returns an empty array.
    /// </remarks>
    protected virtual ImmutableArray<T> FilterCandidates<T>(in ICollection<GenerateSourceRequest> generateSourceRequests) where T : GenerateSourceRequest
    {
        if (generateSourceRequests.Count == 0)
        {
            return [];
        }

        using var candidateRequestsBuilder = ImmutableArrayBuilder<T>.Rent(Math.Max(generateSourceRequests.Count / 2, 8));

        foreach (var candidateRequest in generateSourceRequests)
        {
            if (candidateRequest is T request)
            {
                candidateRequestsBuilder.Add(request);
            }
        }

        return candidateRequestsBuilder.ToImmutable();
    }

    /// <summary>
    /// Filters the provided source generation requests using a custom filter function.
    /// </summary>
    /// <typeparam name="T">The specific type of requests to filter, derived from <see cref="GenerateSourceRequest"/>.</typeparam>
    /// <param name="generateSourceRequests">
    /// An <see cref="ImmutableArray{T}"/> containing the source generation requests to filter.
    /// </param>
    /// <param name="filter">
    /// A delegate function that takes a <see cref="GenerateSourceRequest"/> and returns a 
    /// filtered instance of type <typeparamref name="T"/> or <c>null</c> if the request does not match.
    /// </param>
    /// <returns>
    /// An <see cref="ImmutableArray{T}"/> of filtered requests of type <typeparamref name="T"/>.
    /// </returns>
    /// <remarks>
    /// This method allows more flexible filtering logic by applying the provided delegate function 
    /// to each request. It excludes <c>null</c> results from the final array.
    /// </remarks>
    protected virtual ImmutableArray<T> FilterCandidates<T>(ImmutableArray<GenerateSourceRequest> generateSourceRequests, Func<GenerateSourceRequest, T?> filter) where T : GenerateSourceRequest
    {
        if (generateSourceRequests.IsDefaultOrEmpty)
        {
            return [];
        }

        using var candidateRequestsBuilder = ImmutableArrayBuilder<T>.Rent(Math.Max(generateSourceRequests.Length / 2, 8));

        foreach (var candidateRequest in generateSourceRequests)
        {
            var request = filter(candidateRequest);
            if (request is not null)
            {
                candidateRequestsBuilder.Add(request);
            }
        }

        return candidateRequestsBuilder.ToImmutable();
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
