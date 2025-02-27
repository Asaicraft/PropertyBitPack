---
title: BitFieldProperty Parser
---

## Overview

The **BitFieldProperty Parser** is responsible for converting a C# **PropertyDeclarationSyntax** node (annotated with one of the bit-field attributes) into a custom data model (`BaseBitFieldPropertyInfo`). This model contains all the metadata needed for subsequent **BitFieldProperty Aggregation** and **Property Syntax Generation** steps.

### Key Components

1. **`IBitFieldPropertyParser`**  
   - Defines the contract for **property parsers**. Each parser can decide if a given property + attribute is a “candidate” and perform parsing logic.  
   - The method `Parse(...)` returns a `BaseBitFieldPropertyInfo` if it recognizes and successfully parses the property.

2. **`BaseBitFieldPropertyInfoParser`** (abstract)  
   - Implements `IBitFieldPropertyParser` and provides common logic to parse a property’s attribute data, detect setter/init accessors, and build a `BitFieldPropertyInfo`.  
   - Subclasses can override `IsCandidate(...)` or `ParseCore(...)` to tailor their parsing logic.

3. **`BitFieldPropertyInfoParser`** (concrete)  
   - A **default** implementation of `BaseBitFieldPropertyInfoParser`. It calls the base logic to parse the property’s attribute, create a `BitFieldPropertyInfo`, and store essential details like whether the property has an `init;` or `set;` accessor.

4. **`BaseBitFieldPropertyInfo`**  
   - An abstract data holder. The final parser returns an instance of this type—usually a subclass like `BitFieldPropertyInfo`.  
   - Stores references to the associated C# symbols (property, containing type), as well as parser results (`IAttributeParsedResult`).

5. **`PropertyBitPackGeneratorContext.ParseBitFieldProperty(...)`**  
   - Iterates over **all** registered `IBitFieldPropertyParser`s.  
   - For each parser, calls `Parse(...)`.  
   - Returns the first **non-null** `BaseBitFieldPropertyInfo`.

---

## Parsing Flow

1. **Check if Parser is a Candidate**  
   - Each `IBitFieldPropertyParser` runs `IsCandidate(...)`. It can inspect the property syntax, the attribute data, and other conditions.

2. **Call `Parse(...)`**  
   - If `IsCandidate` is true, the context calls `Parse(...)`.  
   - Default logic in `BaseBitFieldPropertyInfoParser.Parse(...)`:
     1. **Extract Attributes**: Calls `Context.TryParseAttribute(...)` to retrieve an `IAttributeParsedResult`.
     2. **Extract Accessors**: Looks for `set;` or `init;` accessor modifiers.
     3. **Create** `BitFieldPropertyInfo`: Combines property symbol info and the parsed attribute result.

3. **Return** `BaseBitFieldPropertyInfo`  
   - If parsing succeeds, `Parse(...)` returns a new `BitFieldPropertyInfo`; otherwise, `null` if it’s not recognized.

---

## `IBitFieldPropertyParser` Interface

```csharp
internal interface IBitFieldPropertyParser
{
    bool IsCandidate(
        PropertyDeclarationSyntax propertyDeclarationSyntax,
        AttributeData candidateAttribute,
        AttributeSyntax attributeSyntax,
        SemanticModel semanticModel);

    BaseBitFieldPropertyInfo? Parse(
        PropertyDeclarationSyntax propertyDeclarationSyntax,
        AttributeData candidateAttribute,
        AttributeSyntax attributeSyntax,
        SemanticModel semanticModel,
        in ImmutableArrayBuilder<Diagnostic> diagnostics);
}
```

### Methods

- **`IsCandidate(...)`**: Quickly determines if this parser should handle the given property + attribute.  
- **`Parse(...)`**: Performs the detailed parse (e.g., calls `TryParseAttribute(...)`, reads property symbol details). Returns a `BaseBitFieldPropertyInfo` if successful.

---

## `BaseBitFieldPropertyInfoParser`

`BaseBitFieldPropertyInfoParser` implements most of `IBitFieldPropertyParser` and includes the ability to bind a **`PropertyBitPackGeneratorContext`**.

```csharp
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

    public virtual bool IsCandidate(
        PropertyDeclarationSyntax propertyDeclarationSyntax,
        AttributeData candidateAttribute,
        AttributeSyntax attributeSyntax,
        SemanticModel semanticModel
    ) => true;

    public BaseBitFieldPropertyInfo? Parse(
        PropertyDeclarationSyntax propertyDeclarationSyntax,
        AttributeData candidateAttribute,
        AttributeSyntax attributeSyntax,
        SemanticModel semanticModel,
        in ImmutableArrayBuilder<Diagnostic> diagnostics
    )
    {
        return ParseCore(
            propertyDeclarationSyntax,
            candidateAttribute,
            attributeSyntax,
            semanticModel,
            diagnostics
        );
    }

    protected virtual BaseBitFieldPropertyInfo? ParseCore(
        PropertyDeclarationSyntax propertyDeclarationSyntax,
        AttributeData candidateAttribute,
        AttributeSyntax attributeSyntax,
        SemanticModel semanticModel,
        in ImmutableArrayBuilder<Diagnostic> diagnostics
    )
    {
        var setterOrInitModifiers = ExtraxtSetterOrInitModifiers(
            propertyDeclarationSyntax, 
            out var hasInitOrSet, 
            out var isInit
        );

        if (!Context.TryParseAttribute(candidateAttribute, attributeSyntax, propertyDeclarationSyntax, semanticModel, diagnostics, out var attributeResult))
        {
            return null;
        }

        if (semanticModel.GetDeclaredSymbol(propertyDeclarationSyntax) is not IPropertySymbol propertySymbol)
        {
            return null;
        }

        return new BitFieldPropertyInfo(
            propertyDeclarationSyntax,
            attributeResult,
            isInit,
            hasInitOrSet,
            setterOrInitModifiers,
            propertySymbol
        );
    }

    protected virtual SyntaxTokenList ExtraxtSetterOrInitModifiers(
        PropertyDeclarationSyntax propertyDeclaration,
        out bool hasInitOrSet,
        out bool isInit
    )
    {
        hasInitOrSet = false;
        isInit = false;

        if (propertyDeclaration.AccessorList?.Accessors is not SyntaxList<AccessorDeclarationSyntax> accessors)
        {
            return default;
        }

        isInit = accessors.Any(static accessor => accessor.IsKind(SyntaxKind.InitAccessorDeclaration));
        hasInitOrSet = isInit || accessors.Any(static accessor => accessor.IsKind(SyntaxKind.SetAccessorDeclaration));

        var setterOrInitModifiers = accessors
            .Where(static accessor =>
                accessor.IsKind(SyntaxKind.InitAccessorDeclaration) ||
                accessor.IsKind(SyntaxKind.SetAccessorDeclaration)
            )
            .Select(static accessor => accessor.Modifiers)
            .FirstOrDefault();

        return setterOrInitModifiers;
    }
}
```

### Notable Details

- **Context Binding**:  
  Uses `IContextBindable.BindContext(...)` to store a reference to the `PropertyBitPackGeneratorContext`.  

- **`ParseCore(...)`**:  
  - Calls `Context.TryParseAttribute(...)` to get an `IAttributeParsedResult`.  
  - Attempts to get the declared symbol (`IPropertySymbol`) from the `SemanticModel`.  
  - Builds a `BitFieldPropertyInfo`.

---

## `BitFieldPropertyInfoParser`

A final, **concrete** parser that subclasses `BaseBitFieldPropertyInfoParser`. It doesn’t override any logic, so the base functionality is applied directly:

```csharp
internal sealed class BitFieldPropertyInfoParser : BaseBitFieldPropertyInfoParser
{
}
```

---

## `BaseBitFieldPropertyInfo` and `BitFieldPropertyInfo`

Parsed properties end up as `BaseBitFieldPropertyInfo` instances. The default is `BitFieldPropertyInfo`:

```csharp
internal abstract class BaseBitFieldPropertyInfo
{
    public abstract IAttributeParsedResult AttributeParsedResult { get; }
    public abstract bool IsInit { get; }
    public abstract bool HasInitOrSet { get; }
    public abstract SyntaxTokenList SetterOrInitModifiers { get; }
    public abstract IPropertySymbol PropertySymbol { get; }
    public abstract PropertyDeclarationSyntax PropertyDeclarationSyntax { get; }

    public ITypeSymbol PropertyType => PropertySymbol.Type;
    public INamedTypeSymbol Owner => PropertySymbol.ContainingType;

    public override string ToString()
    {
        var setterOrInitter = HasInitOrSet
            ? IsInit
                ? "init;"
                : "set;"
            : string.Empty;

        if (HasInitOrSet)
        {
            setterOrInitter = $"{SetterOrInitModifiers.ToFullString()} {setterOrInitter}";
        }

        return $"{Owner} => [{AttributeParsedResult}] {PropertySymbol.Type.Name} {PropertySymbol.Name} {{ get; {setterOrInitter} }}";
    }
}

internal sealed class BitFieldPropertyInfo(
    PropertyDeclarationSyntax propertyDeclarationSyntax,
    IAttributeParsedResult attributeParsedResult,
    bool isInit,
    bool hasInitOrSet,
    SyntaxTokenList setterOrInitModifiers,
    IPropertySymbol propertySymbol
) : BaseBitFieldPropertyInfo
{
    public override PropertyDeclarationSyntax PropertyDeclarationSyntax { get; } = propertyDeclarationSyntax;
    public override IAttributeParsedResult AttributeParsedResult { get; } = attributeParsedResult;
    public override bool IsInit { get; } = isInit;
    public override bool HasInitOrSet { get; } = hasInitOrSet;
    public override SyntaxTokenList SetterOrInitModifiers { get; } = setterOrInitModifiers;
    public override IPropertySymbol PropertySymbol { get; } = propertySymbol;
}
```

### Properties

- **`IAttributeParsedResult`**: The outcome of the attribute parsing phase (contains bits count, field info, etc.).  
- **`IsInit`/`HasInitOrSet`**: Tracks whether the property has an `init;` or `set;` accessor.  
- **`PropertySymbol`**: The Roslyn symbol for the property.  
- **`PropertyDeclarationSyntax`**: The original syntax node.

---

## How It Fits Together

1. **`PropertyBitPackGeneratorContext.ParseBitFieldProperty(...)`**  
   - Loops over **all** `BitFieldPropertyParsers` in the context (including `BitFieldPropertyInfoParser`).  
   - Calls `Parse(...)`. The first parser that returns non-null is chosen.

2. **Parser**  
   - Checks if the property is a candidate.  
   - Calls the base `ParseCore(...)`, which uses `Context.TryParseAttribute(...)`.

3. **Generated Model**  
   - A `BitFieldPropertyInfo` is returned, combining attribute data + property symbol details.

4. **Aggregation & Generation**  
   - The `BitFieldPropertyInfo` objects are aggregated by `IBitFieldPropertyAggregator`s and then turned into final C# source by `IPropertiesSyntaxGenerator`s.

---

## Summary

- **BitField Property Parsing** is a crucial step that links **attributes** to **property symbols**.  
- `IBitFieldPropertyParser` checks if a property is recognized and constructs a data model for further processing.  
- **`BaseBitFieldPropertyInfoParser`** handles the heavy lifting of calling attribute parsers and extracting relevant property details.  
- The default `BitFieldPropertyInfoParser` simply defers to the base logic, but you can **extend or replace** it with custom parsing.

This modular approach keeps **PropertyBitPack** flexible and allows you to introduce specialized parsers for different use cases or advanced scenarios.
