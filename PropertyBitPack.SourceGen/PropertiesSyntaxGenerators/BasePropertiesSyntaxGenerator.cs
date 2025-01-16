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
    }

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
