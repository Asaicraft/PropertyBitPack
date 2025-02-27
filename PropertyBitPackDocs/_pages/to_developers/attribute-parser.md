---
title: Attribute Parser
---

## Overview

This section covers how **PropertyBitPack** identifies and processes attributes on properties using **Attribute Parsers**. Each parser implements `IAttributeParser`, and optionally extends `BaseAttributeParser`, to handle common logic like retrieving field names, bit counts, or diagnosing invalid usage.

### How It Works

1. **Identify Candidate Attributes**  
   The source generator scans property attributes and tests each `IAttributeParser` via `IsCandidate(...)`.

2. **Attempt Parsing**  
   If a parser is a valid candidate, it calls `TryParse(...)` to extract metadata (e.g., `FieldName`, `BitsCount`) and build a result model (an `IAttributeParsedResult`).

3. **Fallback**  
   If the initial parser fails but has `FallbackOnCandidateFailure = true`, the generator tries subsequent parsers that also indicate fallback support.

4. **Context Binding**  
   Parsers can implement `IContextBindable` so the source generator can pass a shared `PropertyBitPackGeneratorContext`. This allows parsers to access shared logic or diagnostics handling.

---

## `IAttributeParser` Interface

Below is the core interface defining attribute parsing capabilities. It includes whether a parser can **fallback** if another parser fails, and how to **match** + **parse** an attribute into a model.

```csharp
internal interface IAttributeParser
{
    bool FallbackOnCandidateFailure { get; }

    bool IsCandidate(AttributeData attributeData, AttributeSyntax attributeSyntax);

    bool TryParse(
        AttributeData attributeData, 
        AttributeSyntax attributeSyntax, 
        PropertyDeclarationSyntax propertyDeclarationSyntax, 
        SemanticModel semanticModel, 
        in ImmutableArrayBuilder<Diagnostic> diagnostics,
        [NotNullWhen(true)] out IAttributeParsedResult? result
    );
}
```

### Key Points

- **FallbackOnCandidateFailure**: Indicates if the parser should be tried again if a previous parser could not parse the attribute. Useful for scenarios where multiple parsers handle similar attributes or slightly different attribute forms.

- **IsCandidate(...)**: Called first to check if the attribute is relevant to that parser. If it returns true, the source generator attempts `TryParse(...)`.

- **TryParse(...)**: The main logic to interpret an attribute’s data (named arguments, `nameof()` usage, constants, etc.). Returns `true` if parsing succeeds and populates `out result` with a parsed model.

---

## `BaseAttributeParser` Abstract Class

`BaseAttributeParser` provides shared logic, like reading `BitsCount`, extracting `FieldName`, and diagnosing errors. It also implements `IContextBindable`, letting it access the shared context.

```csharp
internal abstract class BaseAttributeParser : IAttributeParser, IContextBindable
{
    protected static readonly SymbolDisplayFormat SymbolDisplayFormatNameAndContainingTypesAndNamespaces 
        = new(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

    private PropertyBitPackGeneratorContext? _context;

    public PropertyBitPackGeneratorContext Context
    {
        get
        {
            Debug.Assert(_context is not null);
            return _context!;
        }
    }

    public virtual bool FallbackOnCandidateFailure => false;

    public void BindContext(PropertyBitPackGeneratorContext context)
    {
        _context = context;
    }

    public abstract bool IsCandidate(AttributeData attributeData, AttributeSyntax attributeSyntax);

    public abstract bool TryParse(
        AttributeData attributeData, 
        AttributeSyntax attributeSyntax, 
        PropertyDeclarationSyntax propertyDeclarationSyntax, 
        SemanticModel semanticModel, 
        in ImmutableArrayBuilder<Diagnostic> diagnostics,
        [NotNullWhen(true)] out IAttributeParsedResult? result
    );

    // Shared helper methods for extracting FieldName, BitsCount, attribute arguments, etc.
    // ...
}
```

### Common Utilities

- **`HasInterface(...)`**: Checks if the attribute class implements a given interface.  
- **`TryGetFieldName(...)`**: Retrieves a `FieldName` from named arguments or `nameof()` usage.  
- **`TryGetBitsCount(...)`**: Ensures `BitsCount` is valid (1–63 bits).  
- **`GetConstantValue(...)`**: Retrieves named argument constants.  
- **`GetAttributeArgument(...)`**: Finds specific arguments within the attribute syntax.

---

## Parsing Flow Example

1. **IsCandidate(...)**  
   Returns true if the attribute name matches (e.g., `MyBitFieldAttribute`).

2. **TryParse(...)**  
   - Validate attribute arguments (e.g., `BitsCount` in range, `FieldName` references a valid field).  
   - If valid, construct an `IAttributeParsedResult` containing relevant metadata for the generator.

3. **Context Usage**  
   The parser may call `Context.SomeHelperMethod(...)` if needed. This is provided via `BindContext(...)`.

---

## Integration with `PropertyBitPackGeneratorContext`

The context iterates over its `ImmutableArray<IAttributeParser>` to match and parse each attribute:

```csharp
public virtual bool IsCandidateAttribute(AttributeData attributeData, AttributeSyntax attributeSyntax)
{
    for (var i = 0; i < AttributeParsers.Length; i++)
    {
        var parser = AttributeParsers[i];
        if (parser.IsCandidate(attributeData, attributeSyntax))
        {
            return true;
        }
    }
    return false;
}

public virtual bool TryParseAttribute(
    AttributeData attributeData,
    AttributeSyntax attributeSyntax,
    PropertyDeclarationSyntax propertyDeclarationSyntax,
    SemanticModel semanticModel,
    in ImmutableArrayBuilder<Diagnostic> diagnostics,
    out IAttributeParsedResult? result
)
{
    for (var i = 0; i < AttributeParsers.Length; i++)
    {
        var parser = AttributeParsers[i];
        if (parser.IsCandidate(attributeData, attributeSyntax))
        {
            if (parser.TryParse(attributeData, attributeSyntax, propertyDeclarationSyntax, semanticModel, diagnostics, out result))
            {
                return true;
            }
            else
            {
                // Attempt fallback logic if supported
                for (var j = i + 1; i > AttributeParsers.Length; j++)
                {
                    var nextParser = AttributeParsers[j];

                    if (!nextParser.FallbackOnCandidateFailure) continue;
                    if (!nextParser.IsCandidate(attributeData, attributeSyntax)) continue;

                    if (nextParser.TryParse(attributeData, attributeSyntax, propertyDeclarationSyntax, semanticModel, diagnostics, out result))
                    {
                        return true;
                    }
                }

                result = null;
                return false;
            }
        }
    }

    result = null;
    return false;
}
```

### Why This Approach?

- **Flexibility**: Multiple parser implementations can handle different attribute flavors or fallback scenarios.  
- **Single Entry Point**: The generator calls `TryParseAttribute(...)`, which loops through all registered `IAttributeParser`s in `PropertyBitPackGeneratorContext`.  
- **Loose Coupling**: Parsers register themselves with the context. Additional parsers can be introduced without modifying the core generator logic.

---

## Summary

- **`IAttributeParser`** defines how the source generator recognizes and interprets bit-field attributes.  
- **`BaseAttributeParser`** centralizes common parsing logic (e.g., `nameof` extraction, validations).  
- **Fallback** logic allows a second (or subsequent) parser to handle an attribute if the primary parser fails.  
- **Context binding** (`BindToSelf()`) lets parsers access shared or advanced functionality through `PropertyBitPackGeneratorContext`.

For deeper usage examples, see the actual implementations in `ExtendedBitFieldAttributeParser`, `ReadOnlyBitFieldAttributeParser`, etc. You can also explore the aggregator and syntax generator pages to see how parsed attributes eventually become generated source code.
