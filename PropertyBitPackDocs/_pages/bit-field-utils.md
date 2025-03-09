---
title: BitFieldUtils
---

**BitFieldUtils** is a static utility class included in PropertyBitPack that provides methods to manipulate bit‑packed data across various numeric types. These methods let you extract and update bit ranges within underlying numeric fields, ensuring efficient and collision‑free storage of multiple properties.

## Key Features

- **Multi-Type Support:**  
  Methods are provided for types such as `byte`, `sbyte`, `short`, `ushort`, `int`, `uint`, `long`, and `ulong`.

- **Flexible Range Specification:**  
  All methods accept a C\# `Range` parameter, allowing you to define bit ranges using standard range syntax (e.g. `start..end`, `start..`, `..end`, and caret notation like `^start`).

- **Collision Prevention:**  
  By operating on specific bit ranges, BitFieldUtils ensures that modifying one property does not interfere with another within the same underlying field.

## Example Usage

### Getting and Setting a Boolean Value
```csharp
byte field = 0;
int bitIndex = 3;
BitFieldUtils.SetBoolValue(ref field, bitIndex, true);
bool result = BitFieldUtils.GetBoolValue(ref field, bitIndex);
Console.WriteLine(result); // Outputs: True
```

### Manipulating a Numeric Value in a Specified Bit Range
```csharp
int field = 0;
var range = 10..20; // 10-bit range within a 32-bit field
int valueToSet = 0b1010101010; // Binary literal for a 10-bit value (682 in decimal)
BitFieldUtils.SetIntValue(ref field, range, valueToSet);
int extractedValue = BitFieldUtils.GetIntValue(ref field, range);
Console.WriteLine(extractedValue); // Outputs: 682
```

## Advanced Usage

### Using Range Operators with Caret Notation

You can use the caret operator (`^`) to specify bit ranges from the end of the field:
```csharp
// For an 8-bit field:
byte field = 0;
BitFieldUtils.SetByteValue(ref field, ^3..^1, 0b10); // Sets a 2-bit range (from index 5 to 7)
byte value = BitFieldUtils.GetByteValue(ref field, ^3..^1);
Console.WriteLine(Convert.ToString(value, 2)); // Outputs the 2-bit value in binary
```

### Combining Multiple Values in a Single Field (Collision Prevention)

BitFieldUtils methods support partitioning a field into multiple segments:
```csharp
// Partition a byte into three segments: bits 0–2, 3–5, and 6–7.
byte field = 0;
BitFieldUtils.SetByteValue(ref field, 0..3, 0b101);  // First 3 bits
BitFieldUtils.SetByteValue(ref field, 3..6, 0b011);  // Next 3 bits
BitFieldUtils.SetByteValue(ref field, 6..8, 0b10);   // Last 2 bits

// Retrieve each value separately.
byte firstSegment = BitFieldUtils.GetByteValue(ref field, 0..3);
byte secondSegment = BitFieldUtils.GetByteValue(ref field, 3..6);
byte thirdSegment = BitFieldUtils.GetByteValue(ref field, 6..8);
Console.WriteLine($"{firstSegment}, {secondSegment}, {thirdSegment}"); // Outputs: 5, 3, 2
```

# Conclusion

BitFieldUtils provides a powerful and flexible API for low‑level bit manipulation. Whether you need to pack multiple boolean values into a single byte or split a 64‑bit field into custom‑sized segments, BitFieldUtils ensures efficient, safe, and collision‑free operations.