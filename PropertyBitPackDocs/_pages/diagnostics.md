---
title: PropertyBitPack Diagnostics
---

# PropertyBitPack Diagnostics

This page provides a reference for all diagnostic messages emitted by **PropertyBitPack** when incorrect attribute usage is detected.

---

## **PRBITS001** - Invalid `BitsCount` Value  
**Description:** The `BitsCount` property must be a positive integer.  
**Message:**  
> The `BitsCount` for property `{0}` must be a positive integer.  

**Example (Invalid Code):**  
```csharp
[BitField(BitsCount = -1)] // ❌ Error: BitsCount must be positive
public partial int Value { get; set; }
```

---

## **PRBITS002** - Missing `GetterLargeSizeValueName`  
**Description:** Extended bit fields require a reference to a method, property, or field that provides the large value.  
**Message:**  
> The property `{0}` requires `GetterLargeSizeValueName` for `ExtendedBitFieldAttribute`.  

**Example (Invalid Code):**  
```csharp
[ExtendedBitField(BitsCount = 10)] // ❌ Missing GetterLargeSizeValueName
public partial int Value { get; set; }
```

---

## **PRBITS003** - `GetterLargeSizeValueName` Requires `nameof`  
**Description:** The `GetterLargeSizeValueName` parameter must use `nameof()` to reference a valid method or property.  
**Message:**  
> The `GetterLargeSizeValueName` for property `{0}` must use `nameof` to reference a valid method or property.  

**Example (Invalid Code):**  
```csharp
[ExtendedBitField(BitsCount = 10, GetterLargeSizeValueName = "GetMaxValue")] // ❌ Must use nameof(GetMaxValue)
public partial int Value { get; set; }
```

---

## **PRBITS004** - Invalid Reference in `GetterLargeSizeValueName`  
**Description:** The referenced value in `GetterLargeSizeValueName` must be a valid method or property.  
**Message:**  
> The `GetterLargeSizeValueName` for property `{0}` must reference a valid property or method.  

**Example (Invalid Code):**  
```csharp
[ExtendedBitField(BitsCount = 10, GetterLargeSizeValueName = nameof(SomeVariable))] // ❌ Not a method or property
public partial int Value { get; set; }
```

---

## **PRBITS005** - Invalid Type for Property Reference in `GetterLargeSizeValueName`  
**Description:** The referenced property must return a type compatible with the bit field.  
**Message:**  
> The property `{0}` referenced in `GetterLargeSizeValueName` has an invalid type. Expected type is `{1}`.  

**Example (Invalid Code):**  
```csharp
public string InvalidProperty => "Invalid";

[ExtendedBitField(BitsCount = 10, GetterLargeSizeValueName = nameof(InvalidProperty))] // ❌ Must return int or compatible type
public partial int Value { get; set; }
```

---

## **PRBITS006** - Invalid Return Type for Method Reference in `GetterLargeSizeValueName`  
**Description:** The referenced method must return a value compatible with the bit field.  
**Message:**  
> The method `{0}` referenced in `GetterLargeSizeValueName` has an invalid return type. Expected type is `{1}`.  

**Example (Invalid Code):**  
```csharp
public static string InvalidMethod() => "Invalid";

[ExtendedBitField(BitsCount = 10, GetterLargeSizeValueName = nameof(InvalidMethod))] // ❌ Must return int or compatible type
public partial int Value { get; set; }
```

---

## **PRBITS007** - Method with Parameters Not Allowed in `GetterLargeSizeValueName`  
**Description:** Methods used in `GetterLargeSizeValueName` must be **parameterless** or only contain parameters with default values.  
**Message:**  
> The method `{0}` referenced in `GetterLargeSizeValueName` must either have no parameters or only parameters with default values.  

**Example (Invalid Code):**  
```csharp
public static int InvalidMethod(int value) => value;

[ExtendedBitField(BitsCount = 10, GetterLargeSizeValueName = nameof(InvalidMethod))] // ❌ Method has required parameter
public partial int Value { get; set; }
```

---

## **PRBITS008** - Too Many Bits Required  
**Description:** The total bit count for a field exceeds the **64-bit limit** for a single field.  
**Message:**  
> Properties with `FieldName` `{0}` require `{1}` bits, which is more than the largest available type (64 bits).  

**Example (Invalid Code):**  
```csharp
[BitField(BitsCount = 70)] // ❌ Exceeds 64-bit limit
public partial long Value { get; set; }
```

---

## **PRBITS009** - Unsupported Owner Type  
**Description:** **Only** classes and structs can use `PropertyBitPack` attributes.  
**Message:**  
> The owner type `{0}` is not supported. Only classes and structs are supported.  

**Example (Invalid Code):**  
```csharp
public interface IExample
{
    [BitField] // ❌ Cannot be used in interfaces
    public partial bool Flag { get; set; }
}
```

---

## **PRBITS010** - Invalid Reference in `FieldName`  
**Description:** The `FieldName` property must reference a valid **field** when using `nameof()`.  
If a string is provided instead of `nameof()`, the generator will accept it, but using `nameof()` incorrectly (e.g., pointing to a property or method) will result in an error.  

**Message:**  
> The `FieldName` for property `{0}` must reference a valid field when using the `nameof` operation.  

---

### **Example (Invalid Code - `nameof()` references something other than a field)**  
```csharp
public int SomeProperty { get; set; } 

[BitField(FieldName = nameof(SomeProperty))] // ❌ Error: 'SomeProperty' is not a field
public partial bool Flag { get; set; }
```

### **Example (Valid Code - Using `nameof()` correctly)**  
```csharp
private int _someField;

[BitField(FieldName = nameof(_someField))] // ✅ Correct: References an actual field
public partial bool Flag { get; set; }
```

### **Example (Valid Code - Using a String Literal Instead of `nameof()`)**  
```csharp
[BitField(FieldName = "_someField")] // ✅ Allowed: Direct string reference
public partial bool Flag { get; set; }
```

---

## **PRBITS011** - Conflict Between Attributes  
**Description:** Certain attributes cannot be used together.  
**Message:**  
> Conflict between attributes: `{0}`. Choose only one of them.  

**Example (Invalid Code):**  
```csharp
[BitField]
[ExtendedBitField] // ❌ Cannot use both
public partial int Value { get; set; }
```

---

## **PRBITS012** - Too Many Bits for Specific Type  
**Description:** The total bit count exceeds the storage capacity of the specified type.  
**Message:**  
> The field `{0}` requires `{1}` bits, which exceeds the capacity of type `{2}` that can hold a maximum of `{3}` bits.  

**Example (Invalid Code):**  
```csharp
[BitField(BitsCount = 17)] // ❌ Exceeds short (16-bit) limit
public partial short Value { get; set; }
```

---

## **PRBITS013** - Invalid Reference to Non-Readonly Field  
**Description:** `ReadOnlyBitFieldAttribute` requires the referenced field to be **readonly**.  
**Message:**  
> The `FieldName` for property `{0}` must reference a readonly field when using the `nameof` operation.  

**Example (Invalid Code):**  
```csharp
private int _mutableField; // ❌ Not readonly

[ReadOnlyBitField(FieldName = nameof(_mutableField))]
public partial int Value { get; }
```

✅ **Valid Code:**  
```csharp
private readonly int _readonlyField; // ✅ Allowed

[ReadOnlyBitField(FieldName = nameof(_readonlyField))]
public partial int Value { get; }
```

---

## **PRBITS014** - `ReadOnlyBitFieldAttribute` Requires No Setter or Init-Only  
**Description:** Properties marked with `ReadOnlyBitFieldAttribute` **must not have a setter**, or must use `init`.  
**Message:**  
> The property `{0}` with `ReadOnlyBitFieldAttribute` must either be read-only or have an `init-only` setter.  

**Example (Invalid Code):**  
```csharp
[ReadOnlyBitField] 
public partial int Value { get; set; } // ❌ Cannot have a setter
```

✅ **Valid Code:**  
```csharp
[ReadOnlyBitField] 
public partial int Value { get; } // ✅ Read-only property

[ReadOnlyBitField] 
public partial int Value { get; init; } // ✅ Init-only setter
```
