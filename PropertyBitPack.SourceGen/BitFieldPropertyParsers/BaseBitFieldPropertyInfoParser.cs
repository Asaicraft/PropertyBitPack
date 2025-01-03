using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PropertyBitPack.SourceGen.AttributeParsers;
using PropertyBitPack.SourceGen.Collections;
using PropertyBitPack.SourceGen.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace PropertyBitPack.SourceGen.BitFieldPropertyParsers;
internal abstract class BaseBitFieldPropertyInfoParser : IBitFieldPropertyParser, IContextBindable
{
    private PropertyBitPackGeneratorContext? _context;

    public PropertyBitPackGeneratorContext Context
    {
        get
        {
            Debug.Assert(_context is not null);

            return _context!;
        }
    }

    void IContextBindable.BindContext(PropertyBitPackGeneratorContext context) => _context = context;

    public virtual bool IsCandidate(PropertyDeclarationSyntax propertyDeclarationSyntax,
        AttributeData candidateAttribute,
        SemanticModel semanticModel) => true;

    public BaseBitFieldPropertyInfo? Parse(PropertyDeclarationSyntax propertyDeclarationSyntax, AttributeData candidateAttribute, SemanticModel semanticModel, in ImmutableArrayBuilder<Diagnostic> diagnostics)
    {
        return ParseCore(propertyDeclarationSyntax, candidateAttribute, semanticModel, diagnostics);
    }

    protected virtual BaseBitFieldPropertyInfo? ParseCore(PropertyDeclarationSyntax propertyDeclarationSyntax, AttributeData candidateAttribute, SemanticModel semanticModel, in ImmutableArrayBuilder<Diagnostic> diagnostics)
    {
        var setterOrInitModifiers = ExtraxtSetterOrInitModifiers(propertyDeclarationSyntax, out var hasInitOrSet, out var isInit);

        if(!Context.TryParseAttribute(candidateAttribute, propertyDeclarationSyntax, semanticModel, diagnostics, out var attributeResult))
        {
            return null;
        }

        if (semanticModel.GetDeclaredSymbol(propertyDeclarationSyntax) is not IPropertySymbol propertySymbol)
        {
            return null;
        }

        return new BitFieldPropertyInfo(attributeResult, isInit, hasInitOrSet, setterOrInitModifiers, propertySymbol);
    }

    protected virtual SyntaxTokenList ExtraxtSetterOrInitModifiers(PropertyDeclarationSyntax propertyDeclaration, out bool hasInitOrSet, out bool isInit)
    {
        hasInitOrSet = false;
        isInit = false;

        if(propertyDeclaration.AccessorList?.Accessors is not SyntaxList<AccessorDeclarationSyntax> accessors)
        {
            return [];
        }

        isInit = accessors.Any(static accessor => accessor.IsKind(SyntaxKind.InitAccessorDeclaration));
        hasInitOrSet = isInit || accessors.Any(static accessor => accessor.IsKind(SyntaxKind.SetAccessorDeclaration));

        var setterOrInitModifiers = accessors
            .Where(static accessor =>
                accessor.IsKind(SyntaxKind.InitAccessorDeclaration) ||
                accessor.IsKind(SyntaxKind.SetAccessorDeclaration))
            .Select(static accessor => accessor.Modifiers).FirstOrDefault();

        return setterOrInitModifiers;
    }
}
