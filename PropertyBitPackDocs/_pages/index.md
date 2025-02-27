---
title: PropertyBitPack
---

![Build](https://github.com/Asaicraft/PropertyBitPack/actions/workflows/dotnet.yml/badge.svg?)
[![NuGet Stats](https://img.shields.io/nuget/v/PropertyBitPack.svg)](https://www.nuget.org/packages/PropertyBitPack?)
[![Discord](https://img.shields.io/badge/chat-discord-purple.svg)](https://discord.gg/RpxD2BeNsZ)

# PropertyBitPack

**PropertyBitPack** is a C# library that allows you to efficiently store multiple properties in a **single integer or byte field**. This reduces memory usage and improves performance for applications requiring compact data storage.

---

## Features

✔ **Bit-Packed Properties** – Store multiple `bool`, `byte`, or `int` properties in a single field.  
✔ **Memory Optimization** – Reduce struct/class memory consumption.  
✔ **Automatic Code Generation** – Define bit-packed properties using simple attributes.  

---

## Installation

Install via NuGet:

```bash
dotnet add package PropertyBitPack
```

---

## Usage

### Define Bit-Packed Boolean Properties

A `bool` in C# typically takes **at least 1 byte** of memory.  
With **PropertyBitPack**, multiple `bool` values can be packed into a **single field**.

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

#### What Happens Internally?
- Instead of **three separate booleans (3 bytes)**, these values are stored inside **one byte**.
- This optimization is especially useful in **games, networking, and embedded systems**.

---

### Define Bit-Packed Numeric Properties

Store **small integer values** inside a **bit-packed field** to optimize memory.

```csharp
public partial class Character
{
    [BitField(BitsCount = 4)]
    public partial int Health { get; set; }

    [BitField(BitsCount = 3)]
    public partial int Armor { get; set; }
}
```

#### Example:
- `Health` can store values **0–15** (4 bits).  
- `Armor` can store values **0–7** (3 bits).  
- Instead of **two separate integers (8 bytes)**, they are packed into **one byte**.

---

### Read-Only Bit-Packed Properties

Use `ReadOnlyBitField` for **immutable** bit-packed properties.

```csharp
public partial class Config
{
    [ReadOnlyBitField(BitsCount = 5)]
    public partial int MaxLimit { get; }
}
```

- `MaxLimit` remains **read-only** but is still efficiently packed.  
- Useful for configurations, settings, and constants.

---

## Summary

**PropertyBitPack** makes **bit-packing easy**, reducing memory usage for boolean and integer properties.  
Ideal for **high-performance applications**, **game development**, **networking**, and **low-level optimizations**.