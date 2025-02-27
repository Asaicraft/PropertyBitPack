---
title: BitFieldAttribute
---

# BitFieldAttribute

`BitFieldAttribute` allows multiple properties to be **packed into a single numeric field**, significantly reducing memory usage. It is useful in **game development, serialization, networking, and embedded systems**.

## Basic Usage

To define a **bit-packed** property, apply `[BitField]` to a property inside a `partial` class:

```csharp
using PropertyBitPack;

public partial class Example
{
    [BitField]
    public partial bool IsActive { get; set; }

    [BitField]
    public partial bool IsVisible { get; set; }

    [BitField]
    public partial bool IsEnabled { get; set; }
}
```

### What Happens Internally?
- Instead of **three separate booleans (3 bytes)**, these values are **packed into a single byte**.
- The bit layout is automatically determined based on the **declaration order**.

---

## Example: Defining Bit-Packed Boolean Properties

When multiple boolean properties are defined, they **share a single byte field**.

```csharp
public partial class BitPackedBools
{
    [BitField]
    public partial bool Bool1 { get; set; }

    [BitField]
    public partial bool Bool2 { get; set; }

    [BitField]
    public partial bool Bool3 { get; set; }

    [BitField]
    public partial bool Bool4 { get; set; }
}
```

### **Generated Code:**
```csharp
partial class BitPackedBools
{
    private byte _Bool1__Bool2__Bool3__Bool4__;

    public partial bool Bool1
    {
        get { return ((this._Bool1__Bool2__Bool3__Bool4__ >> 0) & 1) == 1; }
        set { this._Bool1__Bool2__Bool3__Bool4__ = (byte)(value 
            ? (this._Bool1__Bool2__Bool3__Bool4__ | (1 << 0)) 
            : (this._Bool1__Bool2__Bool3__Bool4__ & ~(1 << 0)));
        }
    }

    public partial bool Bool2
    {
        get { return ((this._Bool1__Bool2__Bool3__Bool4__ >> 1) & 1) == 1; }
        set { this._Bool1__Bool2__Bool3__Bool4__ = (byte)(value 
            ? (this._Bool1__Bool2__Bool3__Bool4__ | (1 << 1)) 
            : (this._Bool1__Bool2__Bool3__Bool4__ & ~(1 << 1)));
        }
    }

    public partial bool Bool3
    {
        get { return ((this._Bool1__Bool2__Bool3__Bool4__ >> 2) & 1) == 1; }
        set { this._Bool1__Bool2__Bool3__Bool4__ = (byte)(value 
            ? (this._Bool1__Bool2__Bool3__Bool4__ | (1 << 2)) 
            : (this._Bool1__Bool2__Bool3__Bool4__ & ~(1 << 2)));
        }
    }

    public partial bool Bool4
    {
        get { return ((this._Bool1__Bool2__Bool3__Bool4__ >> 3) & 1) == 1; }
        set { this._Bool1__Bool2__Bool3__Bool4__ = (byte)(value 
            ? (this._Bool1__Bool2__Bool3__Bool4__ | (1 << 3)) 
            : (this._Bool1__Bool2__Bool3__Bool4__ & ~(1 << 3)));
        }
    }
}
```

### **How Does It Work?**
- A **single private byte field** `_Bool1__Bool2__Bool3__Bool4__` stores all four boolean values.
- Each boolean **occupies a specific bit** in the byte:
  - `Bool1` → **Bit 0**
  - `Bool2` → **Bit 1**
  - `Bool3` → **Bit 2**
  - `Bool4` → **Bit 3**
- The **getter** extracts the correct bit using:
  - `>>` (bitwise right shift)
  - `& 1` (bit mask)
- The **setter** modifies the bit using:
  - `|` (bitwise OR) to set the bit to `1`
  - `& ~` (bitwise AND with NOT) to set the bit to `0`

---

## **Bit Packing Across Multiple Fields**
If the total size of bit-packed properties **exceeds 64 bits**, additional fields will be created automatically.

### **Example:**
```csharp
public partial class Data
{
    [BitField] public partial bool Flag1 { get; set; }  // 1 bit
    [BitField] public partial bool Flag2 { get; set; }  // 1 bit
    [BitField] public partial int Value { get; set; }   // 32 bits
    [BitField] public partial short Small { get; set; } // 16 bits
    [BitField] public partial byte Byte1 { get; set; }  // 8 bits
    [BitField] public partial byte Byte2 { get; set; }  // 8 bits
    [BitField] public partial byte Byte3 { get; set; }  // 8 bits
    [BitField] public partial bool Flag3 { get; set; }  // 1 bit
}
```
### **Result:**
- **First field (64 bits):** `_Flag1__Flag2__Value__Small__Byte1__Byte2__Byte3_`
- **Second field (16 bits):** `_Flag3_`

Each field will store **as many properties as possible** before a new field is generated.

---

## **Field Naming Rules**
The generated field name is a **combination of all packed properties**.

### **Example:**
```csharp
public partial class Example
{
    [BitField] public partial bool A { get; set; }
    [BitField] public partial bool B { get; set; }
    [BitField] public partial int C { get; set; }
}
```
#### **Generated Field Name:**
```csharp
private int _A__B__C__;
```
- **Properties sharing a field** will be included in the name.
- If multiple fields are needed, additional fields will be created with **new names**.

---

## **Read-Only (`init`) and No Setter**
By default, `BitFieldAttribute` generates both `get` and `set`.  
However, you can **remove the setter** or use `init` instead:

```csharp
public partial class Example
{
    [BitField]
    public partial byte Hi { get; } // No setter (read-only)

    [BitField]
    public partial byte Lo { get; init; } // Can only be set in constructors
}
```

---

## **Custom Setter Modifiers**
You can specify additional **modifiers** for the setter:

```csharp
public partial class Example
{
    [BitField]
    public partial int Value { get; private set; } // Private setter

    [BitField]
    public partial int AnotherValue { get; internal set; } // Internal setter
}
```

- `private set;` – Can only be modified inside the class.
- `internal set;` – Can be modified within the same assembly.

---

## **Advantages of Bit Packing**
✔ **Memory Efficiency** – Multiple properties are stored in a **single field**, reducing memory usage.  
✔ **Better Performance** – Less memory usage leads to better **cache efficiency**.  
✔ **Automatic Packing** – Properties are **automatically grouped** into 64-bit fields.  
✔ **Flexible Accessors** – Use `set`, `init`, or remove the setter entirely.  
✔ **Supports Modifiers** – Customize setters with `private`, `internal`, etc.  

📖 **[Next: Custom Field Name →](PropertyBitPack/bit-field-attribute-custom-field-name)**
