---
title: Attribute Parsing Results
---

## Overview

In the previous [Attribute Parser](#) article, we covered how **`IAttributeParser`** and **`BaseAttributeParser`** identify, validate, and extract metadata from bit-field attributes. This article focuses on **the resulting data** these parsers produce — specifically the **`IAttributeParsedResult`**, **`IFieldName`**, and their concrete implementations.

These data models standardize the **parsed attribute information** (e.g., how many bits were requested, which field name was referenced, or whether the field symbol exists). In more advanced scenarios (like read-only or “extended” attributes), they allow the parser to attach extra metadata (e.g., constructor visibility or large-size symbols).

---

## Core Interfaces

### `IAttributeParsedResult`

```csharp
/// <summary>
/// Represents the result of parsing an attribute.
/// </summary>
/// <remarks>
/// This interface standardizes the properties that store data parsed from attributes, 
/// including information about the attribute's syntax, associated data, and additional metadata.
/// </remarks>
internal interface IAttributeParsedResult
{
    /// <summary>
    /// Gets the <see cref="AttributeData"/> representing the parsed attribute metadata.
    /// </summary>
    public AttributeData AttributeData { get; }

    /// <summary>
    /// Gets the <see cref="AttributeSyntax"/> representing the syntax node of the parsed attribute.
    /// </summary>
    public AttributeSyntax AttributeSyntax { get; }

    /// <summary>
    /// Gets the number of bits specified by the parsed attribute, if applicable.
    /// </summary>
    /// <value>
    /// The number of bits as a nullable <see cref="byte"/>, or <c>null</c> if not specified.
    /// </value>
    public byte? BitsCount { get; }

    /// <summary>
    /// Gets the field name specified by the parsed attribute, if applicable.
    /// </summary>
    /// <value>
    /// The <see cref="IFieldName"/> representing the field name, or <c>null</c> if not specified.
    /// </value>
    public IFieldName? FieldName { get; }
}
```

**Key Points**:
- Combines **parsed** metadata (e.g. `BitsCount`, `FieldName`) with Roslyn references (`AttributeData`, `AttributeSyntax`).
- Enables **polymorphic** handling of different attribute variants (extended vs. read-only, etc.).

---

### `IFieldName`

```csharp
internal interface IFieldName
{
    /// <summary>
    /// The name of the field.
    /// Can be null if the field is unnamed.
    /// </summary>
    public string? Name { get; }

    /// <summary>
    /// Indicates whether the field is linked to an existing symbol.
    /// If true, both <see cref="ExistingSymbol"/> and <see cref="Name"/> are guaranteed to be non-null.
    /// </summary>
    [MemberNotNullWhen(true, nameof(ExistingSymbol))]
    [MemberNotNullWhen(true, nameof(Name))]
    public bool IsSymbolExist { get; }

    /// <summary>
    /// The existing symbol associated with the field, if any.
    /// Can be null if no symbol is associated.
    /// </summary>
    public IFieldSymbol? ExistingSymbol { get; }
}
```

**Key Points**:
- **Field Name** can be **unnamed** (if the attribute did not specify one).  
- If `IsSymbolExist == true`, `ExistingSymbol` references an actual **C# field** declared in the user’s code.  
- Used by the generator to determine if a property shares or references an existing bit-packed field.

---

## Concrete Classes

### `AttributeParsedResult` (abstract)

```csharp
internal abstract class AttributeParsedResult(
    AttributeSyntax attributeSyntax,
    AttributeData attributeData,
    IFieldName? fieldName,
    byte? bitsCount
) : IAttributeParsedResult
{
    public AttributeSyntax AttributeSyntax { get; } = attributeSyntax;
    public AttributeData AttributeData { get; } = attributeData;
    public IFieldName? FieldName { get; } = fieldName;
    public byte? BitsCount { get; } = bitsCount;
}
```

**Purpose**: A base class capturing **common** parsed data (attribute syntax, bits count, optional field name). Concrete classes (like `ParsedBitFiledAttribute`) extend it with **additional** properties.

---

### `ParsedBitFiledAttribute`

```csharp
internal sealed class ParsedBitFiledAttribute : AttributeParsedResult
{
    public ParsedBitFiledAttribute(
        AttributeSyntax attributeSyntax,
        AttributeData attributeData,
        IFieldName? fieldName,
        byte? bitsCount
    )
        : base(attributeSyntax, attributeData, fieldName, bitsCount)
    {
    }

    public override string ToString()
    {
        var nameOfFieldNameOrJustName = FieldName?.IsSymbolExist ?? false
            ? $"nameof({FieldName.Name})"
            : FieldName?.Name ?? "<unnamed>";

        return $"{nameof(BitFieldAttribute)}({nameof(BitsCount)}={BitsCount}, {nameof(FieldName)}={nameOfFieldNameOrJustName})";
    }
}
```

**Standard** implementation for a typical bit-field attribute. Tracks:
- `BitsCount`: Number of bits for the property.  
- `FieldName`: The name (or symbol) for the bit field.

---

### `ParsedExtendedBitFiledAttribute` & `ParsedReadOnlyBitFieldAttribute`

These classes **inherit** `AttributeParsedResult` but add more specialized data:

- **`ParsedExtendedBitFiledAttribute`** implements `IParsedExtendedBitFiledAttribute` and includes a `SymbolGetterLargeSizeValue` property for **large** bit-size handling.
- **`ParsedReadOnlyBitFieldAttribute`** implements `IParsedReadOnlyBitFieldAttribute`, storing a `ConstructorAccessModifier` to control the accessibility of the generated constructor.

```csharp
internal sealed class ParsedExtendedBitFiledAttribute 
    : AttributeParsedResult, IParsedExtendedBitFiledAttribute
{
    public ISymbol SymbolGetterLargeSizeValue { get; }

    public ParsedExtendedBitFiledAttribute(
        AttributeSyntax attributeSyntax,
        AttributeData attributeData,
        IFieldName? fieldName,
        byte? bitsCount,
        ISymbol symbolGetterLargeSizeValue
    ) : base(attributeSyntax, attributeData, fieldName, bitsCount)
    {
        SymbolGetterLargeSizeValue = symbolGetterLargeSizeValue;
    }
}
```

```csharp
internal sealed class ParsedReadOnlyBitFieldAttribute 
    : AttributeParsedResult, IParsedReadOnlyBitFieldAttribute
{
    public AccessModifier ConstructorAccessModifier { get; }

    public ParsedReadOnlyBitFieldAttribute(
        AttributeSyntax attributeSyntax,
        AttributeData attributeData,
        IFieldName? fieldName,
        byte? bitsCount,
        AccessModifier accessModifier
    ) : base(attributeSyntax, attributeData, fieldName, bitsCount)
    {
        ConstructorAccessModifier = accessModifier;
    }
}
```

---

## Mermaid Hierarchy Diagram

Below is a **Mermaid** diagram illustrating how these types relate. Note that **dashed** arrows represent interface implementation:

:::Mermaid
```
classDiagram
    class IAttributeParsedResult {
      <<interface>>
      +AttributeData : AttributeData
      +AttributeSyntax : AttributeSyntax
      +BitsCount : byte?
      +FieldName : IFieldName?
    }
    
    class IParsedExtendedBitFiledAttribute {
      <<interface>>
      +SymbolGetterLargeSizeValue : ISymbol
    }
    
    class IParsedReadOnlyBitFieldAttribute {
      <<interface>>
      +ConstructorAccessModifier : AccessModifier
    }
    
    class IFieldName {
      <<interface>>
      +Name : string
      +IsSymbolExist : bool
    }
    
    class AttributeParsedResult {
      <<abstract>>
      +AttributeData : AttributeData
      +AttributeSyntax : AttributeSyntax
      +BitsCount : byte?
      +FieldName : IFieldName?
      +AttributeParsedResult(attributeSyntax: AttributeSyntax, attributeData: AttributeData, fieldName: IFieldName?, bitsCount: byte?)
    }
    
    class ParsedBitFiledAttribute {
      +ParsedBitFiledAttribute(attributeSyntax: AttributeSyntax, attributeData: AttributeData, fieldName: IFieldName?, bitsCount: byte?)
      +ToString() : string
    }
    
    class ParsedExtendedBitFiledAttribute {
      +ParsedExtendedBitFiledAttribute(attributeSyntax: AttributeSyntax, attributeData: AttributeData, fieldName: IFieldName?, bitsCount: byte?, symbolGetterLargeSizeValue: ISymbol)
      +SymbolGetterLargeSizeValue : ISymbol
      +ToString() : string
    }
    
    class ParsedReadOnlyBitFieldAttribute {
      +ParsedReadOnlyBitFieldAttribute(attributeSyntax: AttributeSyntax, attributeData: AttributeData, fieldName: IFieldName?, bitsCount: byte?, accessModifier: AccessModifier)
      +ConstructorAccessModifier : AccessModifier
      +ToString() : string
    }
    
    class ParsedReadOnlyExtendedBitFieldAttribute {
      +ParsedReadOnlyExtendedBitFieldAttribute(attributeSyntax: AttributeSyntax, attributeData: AttributeData, fieldName: IFieldName?, bitsCount: byte?, accessModifier: AccessModifier, symbolGetterLargeSizeValue: ISymbol)
      +ConstructorAccessModifier : AccessModifier
      +SymbolGetterLargeSizeValue : ISymbol
      +ToString() : string
    }
    
    class FieldName {
      +Name : string
      +IsSymbolExist : bool
      +FieldName(name: string, isSymbolExist: bool)
    }
    
    IAttributeParsedResult <|.. AttributeParsedResult
    AttributeParsedResult <|-- ParsedBitFiledAttribute
    AttributeParsedResult <|-- ParsedExtendedBitFiledAttribute
    AttributeParsedResult <|-- ParsedReadOnlyBitFieldAttribute
    AttributeParsedResult <|-- ParsedReadOnlyExtendedBitFieldAttribute
    
    IParsedExtendedBitFiledAttribute <|.. ParsedExtendedBitFiledAttribute
    IParsedExtendedBitFiledAttribute <|.. ParsedReadOnlyExtendedBitFieldAttribute
    
    IParsedReadOnlyBitFieldAttribute <|.. ParsedReadOnlyBitFieldAttribute
    IParsedReadOnlyBitFieldAttribute <|.. ParsedReadOnlyExtendedBitFieldAttribute
    
    IFieldName <|.. FieldName
```
:::

- **`IAttributeParsedResult`** is the **root** interface.  
- **`AttributeParsedResult`** is an **abstract** base class implementing `IAttributeParsedResult`.  
- **`ParsedBitFiledAttribute`**, **`ParsedExtendedBitFiledAttribute`**, **`ParsedReadOnlyBitFieldAttribute`**, and **`ParsedReadOnlyExtendedBitFieldAttribute`** all derive from **`AttributeParsedResult`**.  
- Additional **interfaces**: `IParsedExtendedBitFiledAttribute` and `IParsedReadOnlyBitFieldAttribute` define extra properties for extended and read-only scenarios.  
- **`IFieldName`** is separate, implemented by **`FieldName`** to represent an existing or unnamed field reference.

---

## Summary

- **`IAttributeParsedResult`** is how the generator standardizes parsed attribute data — guaranteeing consistent fields (`BitsCount`, `FieldName`, `AttributeData`, `AttributeSyntax`).  
- **`AttributeParsedResult`** is the **concrete** base, with specialized subclasses like `ParsedBitFiledAttribute`.  
- **Optional** extended or read-only attributes add further **interfaces** (`IParsedExtendedBitFiledAttribute`, `IParsedReadOnlyBitFieldAttribute`) and derived classes to store extra data (like constructor visibility or large-size symbol references).  
- **`IFieldName`** (and its `FieldName` implementation) helps the generator handle property fields consistently, whether referencing an existing symbol or creating a new field on demand.

These types support the broader **PropertyBitPack** pipeline, ensuring that **attribute parsing** yields robust, type-safe data models that the **BitFieldProperty Parser**, **Aggregators**, and **Syntax Generators** can use to generate final source code.
