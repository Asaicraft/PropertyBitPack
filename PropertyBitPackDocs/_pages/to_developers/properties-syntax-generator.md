---
title: Properties Syntax Generator
---

## Overview

After **attribute parsing**, **property parsing**, and **property aggregation**, the **Properties Syntax Generator** phase produces actual C# code for bit-packed properties and fields. The main interface is `IPropertiesSyntaxGenerator`, which transforms `IGenerateSourceRequest` objects into one or more `FileGeneratorRequest` objects containing:

1. **Fields** (if needed).
2. **Properties** (with getters/setters or init accessors).
3. **Any additional syntax** (e.g., constructors, helper methods).

Each `FileGeneratorRequest` has a **`SourceText`** and **`FileName`**; the source generator then **adds** these to the user’s compilation.

---

## `IPropertiesSyntaxGenerator` Interface

```csharp
internal interface IPropertiesSyntaxGenerator
{
    /// <summary>
    /// Generates file generator requests based on the provided source generation requests,
    /// excluding aggregated properties.
    /// </summary>
    ImmutableArray<FileGeneratorRequest> Generate(ILinkedList<IGenerateSourceRequest> requests);
}
```

**Key Points**:
- **Generates** zero or more `FileGeneratorRequest`s.
- **Removes** or **ignores** requests it handles from the `ILinkedList`, so other generators don’t duplicate work.

---

## `FileGeneratorRequest`

A simple record containing the **generated `SourceText`** (C# code) and a **`FileName`**:

```csharp
internal sealed record FileGeneratorRequest(SourceText SourceText, string FileName);
```

---

## `BasePropertiesSyntaxGenerator`

`BasePropertiesSyntaxGenerator` implements most of `IPropertiesSyntaxGenerator` logic and binds to a `PropertyBitPackGeneratorContext`.

```csharp
internal abstract class BasePropertiesSyntaxGenerator : IPropertiesSyntaxGenerator, IContextBindable
{
    public PropertyBitPackGeneratorContext PropertyBitPackGeneratorContext { get; set; } = null!;

    public ImmutableArray<IPropertySyntaxGenerator> PropertySyntaxGenerators => _propertySyntaxGenerators;

    public void BindContext(PropertyBitPackGeneratorContext context)
    {
        PropertyBitPackGeneratorContext = context;
        _propertySyntaxGenerators = GenereatePropertySyntaxGenerators(context);
    }

    public ImmutableArray<FileGeneratorRequest> Generate(ILinkedList<IGenerateSourceRequest> requests)
    {
        using var fileGeneratorRequestsBuilder = ImmutableArrayBuilder<FileGeneratorRequest>.Rent();
        GenerateCore(requests, in fileGeneratorRequestsBuilder);
        if (fileGeneratorRequestsBuilder.Count == 0) return [];
        return fileGeneratorRequestsBuilder.ToImmutable();
    }

    protected abstract void GenerateCore(
        ILinkedList<IGenerateSourceRequest> requests,
        in ImmutableArrayBuilder<FileGeneratorRequest> immutableArrayBuilder
    );

    // ...
}
```

### Responsibilities

1. **`Generate(...)`**: Public entry point. Calls `GenerateCore(...)` to gather all `FileGeneratorRequest`s, then returns them.  
2. **`GenerateCore(...)`**: An abstract method each subclass **must** override. It usually:
   - **Filters** requests relevant to that generator.
   - **Generates** code for each matched request.
   - **Removes** processed requests from the list.
3. **Utility Methods**:
   - **`GetFileName(...)`**: Produces a file name for the resulting `.g.cs` file (based on field names, property owners, etc.).
   - **`GenerateSourceText(...)`**: Builds a final `SourceText` by:
     - Generating fields (if needed).
     - Generating property syntax (via `IPropertySyntaxGenerator`).
     - Creating a compilation unit (namespace, partial class/struct).
     - Normalizing whitespace.

---

## How It Works

1. **Filter Requests**  
   Subclasses (e.g., `ExistingFieldPropertiesGenerator`) find relevant `IGenerateSourceRequest`s with `FilterCandidates<T>(...)`.

2. **Generate**  
   For each request, `GenerateSourceText(...)`:
   - Creates **field declarations** if needed.  
   - Calls **property syntax** generators to produce property code.

3. **Output**  
   - Returns a list of `FileGeneratorRequest`s for each handled request.  
   - The source generator merges them into the user’s compilation.

---

## `IPropertySyntaxGenerator`

These **helpers** (`ExtendedPropertySyntaxGenerator`, `PropertySyntaxGenerator`, etc.) convert a single `BitFieldPropertyInfoRequest` into a `PropertyDeclarationSyntax` (plus any additional members):

```csharp
internal interface IPropertySyntaxGenerator
{
    PropertyDeclarationSyntax? GenerateProperty(
        IGenerateSourceRequest sourceRequest,
        BitFieldPropertyInfoRequest bitFieldPropertyInfoRequest,
        out ImmutableArray<MemberDeclarationSyntax> additionalMember
    );
}
```

**Why?**  
- Different property styles: read-only, extended bit fields, special accessor logic.  
- **`BasePropertySyntaxGenerator`** provides shared bitwise extraction/insertion.  
- A “chain” of property syntax generators can handle specific scenarios.

---

## Built-In Generators

1. **`NonExistingFieldPropertiesSyntaxGenerator`**  
   - Creates **new** fields if they don’t exist.  
   - Generates partial properties referencing them.

2. **`ExistingFieldPropertiesGenerator`**  
   - Handles properties referencing **existing** fields, generating only property code.

3. **`ConstructorGenerator`**  
   - Builds a **constructor** (`ConstructorDeclarationSyntax`) that sets bitfield properties from parameters.

4. **`ExtendedPropertySyntaxGenerator`**  
   - Specialized for `IParsedExtendedBitFiledAttribute` usage.  
   - Allows a “large size” fallback if the bits are at max value.

5. **`PropertySyntaxGenerator`**  
   - The **default** fallback if a property doesn’t match other specialized scenarios.

---

## Mermaid Diagram

:::Mermaid
```
classDiagram
    class IPropertiesSyntaxGenerator {
      <<interface>>
      +Generate(requests: ILinkedList<IGenerateSourceRequest>) : ImmutableArray<FileGeneratorRequest>
    }
    class IPropertySyntaxGenerator {
      <<interface>>
      +GenerateProperty(sourceRequest: IGenerateSourceRequest, bitFieldPropertyInfoRequest: BitFieldPropertyInfoRequest, out additionalMember: ImmutableArray<MemberDeclarationSyntax>) : PropertyDeclarationSyntax?
    }
    class IFileNameModifier {
      <<interface>>
      +ModifyFileName(stringBuilder: StringBuilder) : void
    }
    class IContextBindable {
      <<interface>>
      +BindContext(context: PropertyBitPackGeneratorContext) : void
    }
    
    class FileGeneratorRequest {
      <<record>>
      +SourceText : SourceText
      +FileName : string
    }
    
    class BasePropertiesSyntaxGenerator {
      <<abstract>>
      - _propertySyntaxGenerators : ImmutableArray<IPropertySyntaxGenerator>
      +PropertyBitPackGeneratorContext : PropertyBitPackGeneratorContext
      +BindContext(context: PropertyBitPackGeneratorContext) : void
      +Generate(requests: ILinkedList<IGenerateSourceRequest>) : ImmutableArray<FileGeneratorRequest>
      #GenerateCore(requests: ILinkedList<IGenerateSourceRequest>, fileRequestsBuilder: ImmutableArrayBuilder<FileGeneratorRequest>) : void
      +GetFileName(request: IGenerateSourceRequest) : string
    }
    IPropertiesSyntaxGenerator <|.. BasePropertiesSyntaxGenerator
    IContextBindable <|.. BasePropertiesSyntaxGenerator

    class BasePropertySyntaxGenerator {
      <<abstract>>
      +PropertyBitPackGeneratorContext : PropertyBitPackGeneratorContext
      +GenerateProperty(sourceRequest: IGenerateSourceRequest, bitFieldPropertyInfoRequest: BitFieldPropertyInfoRequest, out additionalMember: ImmutableArray<MemberDeclarationSyntax>) : PropertyDeclarationSyntax?
      #GeneratePropertyCore(sourceRequest: IGenerateSourceRequest, bitFieldPropertyInfoRequest: BitFieldPropertyInfoRequest) : PropertyDeclarationSyntax?
      #GeneratePropertyCore(sourceRequest: IGenerateSourceRequest, bitFieldPropertyInfoRequest: BitFieldPropertyInfoRequest, out additionalMember: ImmutableArray<MemberDeclarationSyntax>) : PropertyDeclarationSyntax?
      +GetterBlockSyntax(bitFieldPropertyInfoRequest: BitFieldPropertyInfoRequest) : BlockSyntax
      +SetterBlockSyntax(bitFieldPropertyInfoRequest: BitFieldPropertyInfoRequest, valueVariableName: string, maxValueVariableName: string, clampedValueVariableName: string) : BlockSyntax
    }
    IPropertySyntaxGenerator <|.. BasePropertySyntaxGenerator

    class ExtendedPropertySyntaxGenerator {
      +GetterBlockSyntax(bitFieldPropertyInfoRequest: BitFieldPropertyInfoRequest) : BlockSyntax
      // Äîïîëíèòåëüíàÿ ëîãèêà äëÿ ðàáîòû ñ ExtendedBitFieldAttribute
    }
    ExtendedPropertySyntaxGenerator --|> BasePropertySyntaxGenerator

    class PropertySyntaxGenerator {
      // Ïðîñòîé ãåíåðàòîð, âñåãäà âîçâðàùàþùèé ñãåíåðèðîâàííîå ñâîéñòâî
    }
    PropertySyntaxGenerator --|> BasePropertySyntaxGenerator

    class ConstructorGenerator {
      +GenerateCore(requests: ILinkedList<IGenerateSourceRequest>, fileGeneratorRequestsBuilder: ImmutableArrayBuilder<FileGeneratorRequest>) : void
    }
    ConstructorGenerator --|> BasePropertiesSyntaxGenerator

    class ExistingFieldPropertiesGenerator {
      +GenerateCore(requests: ILinkedList<IGenerateSourceRequest>, fileGeneratorRequestsBuilder: ImmutableArrayBuilder<FileGeneratorRequest>) : void
    }
    ExistingFieldPropertiesGenerator --|> BasePropertiesSyntaxGenerator

    class NonExistingFieldPropertiesSyntaxGenerator {
      +GenerateCore(requests: ILinkedList<IGenerateSourceRequest>, fileGeneratorRequestsBuilder: ImmutableArrayBuilder<FileGeneratorRequest>) : void
    }
    NonExistingFieldPropertiesSyntaxGenerator --|> BasePropertiesSyntaxGenerator

```
:::

1. **`IPropertiesSyntaxGenerator`** is the **entry** for generating `.g.cs` files.  
2. **`BasePropertiesSyntaxGenerator`** includes the **common** logic (filtering, file naming, partial class structure).  
3. **Concrete** classes handle **existing fields**, **unnamed** fields, or **constructors**.  
4. **`IPropertySyntaxGenerator`** is a sub-component for generating the **actual** property code.

---

## Example Generation Flow

1. **`PropertyBitPackGeneratorContext.GeneratePropertySyntax()`**  
   - Calls each **syntax generator** in its array (e.g., `NonExistingFieldPropertiesSyntaxGenerator`, `ExistingFieldPropertiesGenerator`, etc.).
2. **A Syntax Generator**:
   - **Filters** relevant requests (e.g., `NonExistingFieldGsr`), building partial classes with new fields.  
   - For each property in the request, calls `IPropertySyntaxGenerator.GenerateProperty(...)`.
   - Produces a final `FileGeneratorRequest` with the created `SourceText`.
3. **Result**  
   - The source generator merges all these `.g.cs` files into the user’s compilation.

---

## Summary

- **Properties Syntax Generators** transform aggregated property data into **complete** `.g.cs` files.  
- They handle **file structure** (partial classes, namespaces) and **call** `IPropertySyntaxGenerator` to create individual property accessors.  
- By using **multiple** syntax generators, **PropertyBitPack** can produce specialized code for existing fields, extended fields, read-only attributes, or new fields.  
- This modular design keeps each generator **focused** on a single concern, ensuring flexibility and maintainability as bitfield use cases evolve.
