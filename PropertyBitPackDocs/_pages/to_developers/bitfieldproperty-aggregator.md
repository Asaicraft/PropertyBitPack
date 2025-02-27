---
title: BitFieldProperty Aggregator
---

## Overview

After **attribute parsing** and **property parsing**, **bit-field aggregators** organize the parsed property data, grouping them into fields and producing **source generation requests**. Each aggregator implements `IBitFieldPropertyAggregator`—it inspects a collection of `BaseBitFieldPropertyInfo`, then decides how to:

1. **Group** or **filter** those properties.
2. **Distribute** their bits into suitable fields (1–64 bits).
3. **Generate** one or more `IGenerateSourceRequest` objects describing the final layout.

This step ensures that partially processed properties (e.g., existing-field references, unnamed fields) are **removed** from further processing, so subsequent aggregators do not re-handle them.

---

## The `IBitFieldPropertyAggregator` Interface

```csharp
internal interface IBitFieldPropertyAggregator
{
    /// <summary>
    /// Remove properties in <paramref name="properties"/> which aggregated 
    /// </summary>
    ImmutableArray<IGenerateSourceRequest> Aggregate(
        ILinkedList<BaseBitFieldPropertyInfo> properties, 
        in ImmutableArrayBuilder<IGenerateSourceRequest> readyRequests, 
        in ImmutableArrayBuilder<Diagnostic> diagnostics
    );
}
```

**Key Points**:

- The aggregator **mutates** the `properties` list by removing properties it has successfully aggregated.
- Returns **new** `IGenerateSourceRequest` instances describing how the grouped properties should be generated (e.g., fields, bits, offsets).

---

## `BaseBitFieldPropertyAggregator` Abstract Class

Many aggregators inherit from `BaseBitFieldPropertyAggregator`, which:

```csharp
internal abstract class BaseBitFieldPropertyAggregator : IBitFieldPropertyAggregator, IContextBindable
{
    public virtual ImmutableArray<IGenerateSourceRequest> Aggregate(
        ILinkedList<BaseBitFieldPropertyInfo> properties, 
        in ImmutableArrayBuilder<IGenerateSourceRequest> readyRequests, 
        in ImmutableArrayBuilder<Diagnostic> diagnostics
    ) {
        if (properties.Count == 0)
        {
            return [];
        }

        using var requestsBuilder = ImmutableArrayBuilder<IGenerateSourceRequest>.Rent();
        AggregateCore(properties, requestsBuilder, in diagnostics);
        return requestsBuilder.ToImmutable();
    }

    protected virtual void AggregateCore(
        ILinkedList<BaseBitFieldPropertyInfo> properties,
        in ImmutableArrayBuilder<IGenerateSourceRequest> requestsBuilder,
        in ImmutableArrayBuilder<Diagnostic> diagnostics
    ) { }

    // Additional helpers for:
    //   - Grouping properties by owner + field name
    //   - Distributing bits into 8/16/32/64-bit fields
    //   - Validating bit sizes
    //   - Creating requests
}
```

### Shared Responsibilities

- **`SelectCandiadates(...)`**: Identifies aggregator-specific property sets (e.g., unnamed fields, existing fields, or named fields).  
- **`GroupPropertiesByFieldNameAndOwner(...)`**: Gathers properties into groups ( `(INamedTypeSymbol Owner, IFieldName? fieldName)` ), allowing subsequent distribution.  
- **`DistributeBitsIntoFields(...)`**: Splits a sequence of bit counts into the smallest number of typed fields. For example, if total is 12 bits, aggregator might choose a `ushort` (16 bits).

---

## Aggregator Workflow

1. **Aggregate**:
   1. The aggregator calls `SelectCandiadates(...)` to filter matching properties.  
   2. Groups them by **owner** (the containing type) and (optionally) **field name**.  
   3. Validates that total bits do not exceed 64.  
   4. Builds **requests** describing each field + set of properties.

2. **Remove Aggregated**:
   - Once the aggregator has processed a group, it removes those properties from the `ILinkedList`, so subsequent aggregators don’t reprocess them.

3. **Return Requests**:
   - New `IGenerateSourceRequest` objects are appended to the aggregator’s local builder.  
   - The aggregator returns these to the **PropertyBitPackGeneratorContext**, which collects them for final code generation.

---

## Built-in Aggregators

### `ExistingFieldAggregator`

```csharp
internal sealed class ExistingFieldAggregator : BaseBitFieldPropertyAggregator
{
    protected override void AggregateCore(
        ILinkedList<BaseBitFieldPropertyInfo> properties,
        in ImmutableArrayBuilder<IGenerateSourceRequest> requestsBuilder,
        in ImmutableArrayBuilder<Diagnostic> diagnostics)
    {
        // Finds properties that reference an existing field (IFieldName.IsSymbolExist == true).
        // Groups them by (Owner, Existing Field), checks if all bits fit into that field's size.
        // Generates an "ExistingFieldRequest" and an "ExistingFieldGsr" for each group.
    }
}
```

**Usage**:  
- **Existing** fields (e.g., `uint _myField;`).  
- Aggregator ensures all requested bits fit in the underlying field type (8, 16, 32, or 64 bits).

### `UnnamedFieldAggregator`

```csharp
internal sealed class UnnamedFieldAggregator : BaseUnnamedFieldAggregator
{
    // Processes properties that do NOT specify a field name, distributing them into
    // newly allocated fields. It merges them if possible (for efficiency),
    // then produces "UnnamedFieldGsr" requests.
}
```

**Usage**:  
- **No field name** (e.g., `[BitField(8)] partial int Age { get; set; }`).  
- Generates a **new** unnamed field for these properties, grouping them if collectively < 64 bits.

### `NamedFieldPropertyAggregator`

```csharp
internal sealed class NamedFieldPropertyAggregator : BaseBitFieldPropertyAggregator
{
    // Collects properties that have a custom field name but do not reference an existing symbol.
    // Groups them by that name + owner, ensures bits <= 64, and produces "NamedFieldGsr".
}
```

**Usage**:  
- **Named** but **non-existent** fields. For instance, `[BitField(BitsCount=16, FieldName="MyField")]`.

### `ReadOnlyBitFieldAggregator`

```csharp
internal sealed class ReadOnlyBitFieldAggregator : BaseBitFieldPropertyAggregator
{
    // Delegates to a "ReadOnlyAggregatorComponent" to handle read-only attributes.
    // It ultimately yields "IReadOnlyFieldGsr" requests.
}
```

**Usage**:  
- **Read-only** attributes that require special constructor logic or different generation patterns.

---

## Mermaid Diagram

:::Mermaid
```
classDiagram
    class IBitFieldPropertyAggregator {
      <<interface>>
      +Aggregate(properties: ILinkedList, readyRequests: ImmutableArrayBuilder, diagnostics: ImmutableArrayBuilder) : ImmutableArray
    }
    class IGenerateSourceRequest {
      +Fields : ImmutableArray
      +Properties : BitFieldPropertyInfoRequest[]
    }
    class IFieldRequest {
      +FieldType : SpecialType
      +IsExist : bool
      +Name : string
    }
    
    class BaseBitFieldPropertyAggregator {
      <<abstract>>
      - _context : PropertyBitPackGeneratorContext?
      +Context : PropertyBitPackGeneratorContext
      +BindContext(context: PropertyBitPackGeneratorContext) : void
      +Aggregate(properties: ILinkedList, readyRequests: ImmutableArrayBuilder, diagnostics: ImmutableArrayBuilder) : ImmutableArray
      +AggregateCore(properties: ILinkedList, reqBuilder: ImmutableArrayBuilder, diagnostics: ImmutableArrayBuilder) : void
      +SelectCandidatesCore(props: IReadOnlyCollection, diagnostics: ImmutableArrayBuilder, candidates: ImmutableArrayBuilder) : void
      +SelectCandidates(props: ILinkedList, diagnostics: ImmutableArrayBuilder) : ImmutableArray
      +GroupPropertiesByFieldNameAndOwner(props: ImmutableArray) : ImmutableArray
      +DistributeBitsIntoFields(info: ImmutableArray) : ImmutableArray
      +ToRequests(req: IFieldRequest, infos: ImmutableArray) : ImmutableArray
    }
    
    class BaseUnnamedFieldAggregator {
      <<abstract>>
      +SelectCandidatesCore() : void
      +AggregateCore() : void
      +AddUnnamedFieldRequests(group: OwnerFieldNameGroup, bits: ImmutableArray, reqBuilder: ImmutableArrayBuilder) : void
      +GetFieldName(candidateProps: RentedListPool) : string
    }
    class ExistingFieldAggregator {
      +AggregateCore(props, reqBuilder, diagnostics) : void
    }
    class NamedFieldPropertyAggregator {
      +SelectCandidatesCore() : void
      +AggregateCore(props, reqBuilder, diagnostics) : void
      +CreateGsr(group: OwnerFieldNameGroup, bitSize: BitSize) : NamedFieldGsr
    }
    class ReadOnlyBitFieldAggregator {
      +Aggregate(props, readyRequests, diagnostics) : ImmutableArray
      - ReadOnlyAggregatorComponent : ReadOnlyAggregatorComponent
    }
    class UnnamedFieldAggregator {
      <<final>>
    }
    
    class FieldRequest {
      <<class>>
      - _name : string
      - _fieldType : SpecialType
      - _isExist : bool
      +FieldRequest(name: string, fieldType: SpecialType, isExist: bool)
      +Name : string
      +FieldType : SpecialType
      +IsExist : bool
      +ToString() : string
    }
    class ExistingFieldRequest {
      <<final>>
      +FieldSymbol : IFieldSymbol
    }
    class NonExistingFieldRequest {
      <<class>>
    }
    class NamedFieldRequest {
      <<class>>
    }
    FieldRequest <|-- ExistingFieldRequest
    FieldRequest <|-- NonExistingFieldRequest
    NonExistingFieldRequest <|-- NamedFieldRequest
    
    class BitFieldPropertyInfoRequest {
      - _bitsSpan : BitsSpan
      - _info : BaseBitFieldPropertyInfo
      +BitsSpan : BitsSpan
      +BitFieldPropertyInfo : BaseBitFieldPropertyInfo
      +ToString() : string
    }
    class BitsSpan {
      - _req : IFieldRequest
      - _start : byte
      - _length : byte
      +FieldRequest : IFieldRequest
      +Start : byte
      +Length : byte
      +ToString() : string
    }
    
    IBitFieldPropertyAggregator <|.. BaseBitFieldPropertyAggregator
    BaseBitFieldPropertyAggregator <|-- BaseUnnamedFieldAggregator
    BaseBitFieldPropertyAggregator <|-- ExistingFieldAggregator
    BaseBitFieldPropertyAggregator <|-- NamedFieldPropertyAggregator
    BaseBitFieldPropertyAggregator <|-- ReadOnlyBitFieldAggregator
    BaseUnnamedFieldAggregator <|-- UnnamedFieldAggregator

```
:::

1. **`IBitFieldPropertyAggregator`** is the root interface.  
2. **`BaseBitFieldPropertyAggregator`** provides common grouping/distribution logic.  
3. **Concrete** aggregators each handle a specific scenario.

---

## Putting It All Together

1. **Context Calls `AggregateBitFieldProperties(...)`**  
   - Loops through all **registered** aggregators in the `PropertyBitPackGeneratorContext`.  
   - Each aggregator is given a chance to process + remove matching properties.

2. **Aggregators Create Requests**  
   - **Fields**: Each aggregator decides if new fields are needed (e.g., unnamed) or if an **existing** field is used.  
   - **Property Mappings**: Distributes property bits within those fields, generating offset data (`BitsSpan`).

3. **Resulting `IGenerateSourceRequest`s**  
   - Aggregators produce requests like `UnnamedFieldGsr`, `NamedFieldGsr`, `ExistingFieldGsr`, or specialized read-only ones.  
   - These requests flow into **property syntax generators** that emit final C# code.

---

## Summary

- **BitFieldProperty Aggregators** are the **bridge** between parsed properties and final code generation.  
- They **group** properties, **validate** bit sizes, and produce **requests** describing how to lay out bits in fields (8–64 bits).  
- **Each aggregator** focuses on a different scenario: existing fields, unnamed fields, named fields, or read-only logic.  
- By **removing** processed properties, multiple aggregators can run in sequence without duplicating work.

Together, **aggregators** ensure **PropertyBitPack** remains modular and flexible, enabling advanced scenarios like partial code reuse, custom field packing, and specialized read-only constructs.
