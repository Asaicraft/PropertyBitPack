---
title: Architecture
---

Below is an overview of the internal architecture of **PropertyBitPack**, focusing on how it processes attributes, aggregates bit-packed properties, and generates source code. This documentation is intended for contributors who want to understand or extend the source generator logic.

---

## High-Level Architecture

1. **`PropertyBitPackGeneratorContext`** (abstract class)  
   - Defines arrays of **parsers** and **generators**:
     - `AttributeParsers` (implementations of `IAttributeParser`)
     - `BitFieldPropertyParsers` (implementations of `IBitFieldPropertyParser`)
     - `BitFieldPropertyAggregators` (implementations of `IBitFieldPropertyAggregator`)
     - `PropertySyntaxGenerators` (implementations of `IPropertiesSyntaxGenerator`)
   - Provides methods to:
     - Identify candidate attributes
     - Parse them into internal `BaseBitFieldPropertyInfo` representations
     - Aggregate property info into requests (`IGenerateSourceRequest`)
     - Generate final C# source code (`FileGeneratorRequest`)

2. **`PropertyBitPackGeneratorContextImplementation`**  
   - A sealed nested class that constructs these arrays **without constructor injection** in the typical DI sense.
   - Instead, after constructing arrays of parsers/generators, the method `BindToSelf()` iterates over each component.  
     - If the component implements `IContextBindable`, it calls `BindContext(this)`, providing a direct link to the current context instance.

3. **`PropertyBitPackSourceGenerator : IIncrementalGenerator`**  
   - Holds a static instance of `PropertyBitPackGeneratorContext`.
   - Uses **incremental generation**:
     - Filters candidate properties via `IsCandidateProperty(...)`.
     - For each property, attempts to parse attached attributes.
     - Aggregates bit-packed properties.
     - Calls context methods to generate final source code.

4. **Parsing Workflow**  
   1. **Identify** partial properties with certain attributes.  
   2. **Check** if any of the known attribute parsers (`IAttributeParser`) match.  
   3. **Parse** them into `BaseBitFieldPropertyInfo`.  
   4. **Aggregate** all recognized properties into `IGenerateSourceRequest` objects.  
   5. **Generate** the final `.cs` source files (property implementations) via `IPropertiesSyntaxGenerator`.

---

## Mermaid Diagram

:::Mermaid source

:::

<pre class="mermaid">
flowchart LR
    A[IncrementalGenerator] --> B{PropertyBitPackGeneratorContextImplementation}
    B -->|BindToSelf| D[IContextBindable?]
    B -->|AttributeParsers| C[PropertyDeclaration]
    B -->|BitFieldPropertyParsers| E[BaseBitFieldPropertyInfo]
    B -->|BitFieldPropertyAggregators| F[IGenerateSourceRequest]
    B -->|PropertySyntaxGenerators| G["FileGeneratorRequest (CS)"]

    C --> E
    E --> F
    F --> G
</pre>

```mermaid
flowchart LR
    A[IncrementalGenerator] --> B{PropertyBitPackGeneratorContextImplementation}
    B -->|BindToSelf| D[IContextBindable?]
    B -->|AttributeParsers| C[PropertyDeclaration]
    B -->|BitFieldPropertyParsers| E[BaseBitFieldPropertyInfo]
    B -->|BitFieldPropertyAggregators| F[IGenerateSourceRequest]
    B -->|PropertySyntaxGenerators| G["FileGeneratorRequest (CS)"]

    C --> E
    E --> F
    F --> G
</pre>
```

**Diagram Explanation**:  
1. **IncrementalGenerator** filters candidate properties.  
2. **PropertyBitPackGeneratorContextImplementation**:
   - Holds arrays of parsers/generators.
   - Calls `BindToSelf()`, so each component that implements `IContextBindable` can access the **context**.  
3. The parsers/generators then handle attribute parsing, property analysis, and source code generation.

---

## Notable Classes & Interfaces

### `PropertyBitPackGeneratorContextImplementation`

```csharp
internal sealed class PropertyBitPackGeneratorContextImplementation : PropertyBitPackGeneratorContext
{
    public PropertyBitPackGeneratorContextImplementation(
        ImmutableArray<IAttributeParser> attributeParsers,
        ImmutableArray<IBitFieldPropertyParser> bitFieldPropertyParsers,
        ImmutableArray<IBitFieldPropertyAggregator> bitFieldPropertyAggregators,
        ImmutableArray<IPropertiesSyntaxGenerator> bitFieldPropertyGenerators
    ) {
        AttributeParsers = attributeParsers;
        BitFieldPropertyParsers = bitFieldPropertyParsers;
        BitFieldPropertyAggregators = bitFieldPropertyAggregators;
        PropertySyntaxGenerators = bitFieldPropertyGenerators;

        // Binds the context to all parsers/generators that require it
        BindToSelf();
    }
    
    internal void BindToSelf()
    {
        // If any component implements IContextBindable, pass it this context
    }
}
```

- **No typical DI constructor injection**. The arrays of components are assigned directly, then `BindToSelf()` ensures each component can hold a reference to the context if needed.

### `IContextBindable`
```csharp
internal interface IContextBindable
{
    void BindContext(PropertyBitPackGeneratorContext context);
}
```
- Components (e.g., parsers, aggregators, syntax generators) implement `IContextBindable` to gain access to shared context data or services.

### `PropertyBitPackSourceGenerator`
- **Entry point** for incremental generation. Creates a static `_context` using a `PropertyBitPackGeneratorContextBuilder`.
- Scans the syntax for candidate partial properties, matches attributes, and produces generated `.cs` files.

---

## Customizing Parsers & Generators

To add or override behavior:

1. **Implement** a new `IAttributeParser`, `IBitFieldPropertyParser`, `IBitFieldPropertyAggregator`, or `IPropertiesSyntaxGenerator`.
2. **Attach** it to the context builder:
   ```csharp
   var builder = PropertyBitPackGeneratorContextBuilder.Create();
   builder.AttributeParsers.Add(new MyCustomAttributeParser());
   builder.BitFieldPropertyParsers.Add(new MyCustomBitFieldParser());
   // ...
   var context = builder.Build(); // Returns PropertyBitPackGeneratorContextImplementation
   ```
3. **Use** `BindToSelf()` to allow each component to `BindContext(...)` if it implements `IContextBindable`.

---

## Summary

- **PropertyBitPack** uses a modular approach, allowing you to plug in new attribute parsers, property parsers, aggregators, and syntax generators.  
- **Context binding** is not done through traditional DI constructor injection; instead, **`BindToSelf()`** iterates over all registered components and calls `BindContext(this)` if they implement `IContextBindable`.  
- This design keeps the architecture flexible and straightforward, enabling easy extension for advanced bit-packing scenarios.

For questions or deeper discussion, feel free to join our [Discord](https://discord.gg/RpxD2BeNsZ)!