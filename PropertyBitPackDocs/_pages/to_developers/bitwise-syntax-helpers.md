---
title: Bitwise Syntax Helpers in C#
---

# BitwiseSyntaxHelpers: Efficient Bitwise Expression Generation in C#

## Overview

`BitwiseSyntaxHelpers` is a **utility class** designed to construct **bitwise expressions** and **C# syntax nodes**. It provides **helper methods** for generating **bitwise operations, field declarations, and accessor methods** using **Roslyn syntax APIs**.

This class is **especially useful** when working with **code generation, binary data manipulation, and performance-critical applications** that require precise bitwise control.

---

## 🚀 Key Features

✔ **Bitwise Expression Builders** – Generate efficient bitwise expressions like shifting, masking, and setting bits.  
✔ **Field and Property Accessors** – Easily create **C# field declarations**, access modifiers, and accessor methods.  
✔ **Syntax Tree Manipulation** – Use Roslyn APIs to build **structured C# code** dynamically.  
✔ **Performance Optimized** – Uses **direct expression composition** to minimize overhead.

---

## 🔹 Getting Access Modifiers

### 🔹 `GetAccessModifiers`
This method **converts an `AccessModifier` enum** into **C# syntax tokens**.

```csharp
var tokens = BitwiseSyntaxHelpers.GetAccessModifers(AccessModifier.Public);
// Produces: public
```

### 📌 Code:
```csharp
public static SyntaxTokenList GetAccessModifers(AccessModifier accessModifier)
{
    return accessModifier switch
    {
        AccessModifier.Public => TokenList(Token(SyntaxKind.PublicKeyword)),
        AccessModifier.Protected => TokenList(Token(SyntaxKind.ProtectedKeyword)),
        AccessModifier.Internal => TokenList(Token(SyntaxKind.InternalKeyword)),
        AccessModifier.ProtectedInternal => TokenList(Token(SyntaxKind.ProtectedKeyword), Token(SyntaxKind.InternalKeyword)),
        AccessModifier.Private => TokenList(Token(SyntaxKind.PrivateKeyword)),
        AccessModifier.PrivateProtected => TokenList(Token(SyntaxKind.PrivateKeyword), Token(SyntaxKind.ProtectedKeyword)),
        AccessModifier.Default => TokenList(),
        _ => throw new NotSupportedException()
    };
}
```

---

## 📌 Generating Field Declarations

### 🔹 `BuildField`
Creates a **field declaration syntax** with the specified type, name, and modifiers.

```csharp
var field = BitwiseSyntaxHelpers.BuildField(SpecialType.System_Int32, "myField", AccessModifier.Private, true);
```

### 📌 Code:
```csharp
public static FieldDeclarationSyntax BuildField(
    SpecialType specialType,
    string fieldName,
    AccessModifier accessModifier = AccessModifier.Private,
    bool isReadOnly = false)
{
    var fieldType = GetTypeSyntaxFromSpecialType(specialType);
    var accessModifierTokens = GetAccessModifers(accessModifier);

    var modifiers = isReadOnly ? TokenList(Token(SyntaxKind.ReadOnlyKeyword)) : TokenList();
    modifiers = accessModifierTokens.AddRange(modifiers);

    return FieldDeclaration(
        List<AttributeListSyntax>(),
        modifiers,
        VariableDeclaration(
            fieldType,
            SingletonSeparatedList(
                VariableDeclarator(fieldName)
            )
        )
    );
}
```

---

## 🔥 Bitwise Expression Builders

### 🔹 `BuildBitSet`
Generates a **bitwise expression** for setting specific bits in a value.

```csharp
var expression = BitwiseSyntaxHelpers.BuildBitSet(
    IdentifierName("currentValue"),
    IdentifierName("newValue"),
    SpecialType.System_Int32,
    start: 3,
    length: 5
);
```

### 📌 Code:
```csharp
public static ExpressionSyntax BuildBitSet(
    ExpressionSyntax currentValueExpr,
    ExpressionSyntax newValueExpr,
    SpecialType fieldType,
    byte start,
    byte length)
{
    var leftAndMask = BuildLeftAndMask(
        currentValueExpr,
        BuildMaskShifted(fieldType, length, start)
    );

    var rightValueShift = BuildValueAndShift(
        newValueExpr,
        fieldType,
        length,
        start
    );

    return BuildOr(leftAndMask, rightValueShift);
}
```

### 🏆 Produces:
```c
(currentValue & ~(mask << 3)) | ((newValue & mask) << 3)
```

---

## 🔢 Accessor Helpers

### 🔹 `Setter`, `Getter`, `Initter`
Creates **property accessors** for `set`, `get`, and `init`.

```csharp
var setter = BitwiseSyntaxHelpers.Setter(TokenList(Token(SyntaxKind.PrivateKeyword)), Block());
var getter = BitwiseSyntaxHelpers.Getter(Block());
var initter = BitwiseSyntaxHelpers.Initter(TokenList(Token(SyntaxKind.InternalKeyword)), Block());
```

### 📌 Code:
```csharp
public static AccessorDeclarationSyntax Setter(SyntaxTokenList modifiers, BlockSyntax body)
{
    return AccessorDeclaration(
        SyntaxKind.SetAccessorDeclaration,
        List<AttributeListSyntax>(),
        modifiers,
        body
    );
}

public static AccessorDeclarationSyntax Getter(BlockSyntax body)
{
    return AccessorDeclaration(
        SyntaxKind.GetAccessorDeclaration,
        List<AttributeListSyntax>(),
        TokenList(),
        body
    );
}

public static AccessorDeclarationSyntax Initter(SyntaxTokenList modifiers, BlockSyntax body)
{
    return AccessorDeclaration(
        SyntaxKind.InitAccessorDeclaration,
        List<AttributeListSyntax>(),
        modifiers,
        body
    );
}
```

---

## 🏗️ Creating Constants and Math Expressions

### 🔹 `BuildConstMaskDeclaration`
Generates a **constant bitmask** expression.

```csharp
var constMask = BitwiseSyntaxHelpers.BuildConstMaskDeclaration("maskVar", SpecialType.System_Int32, 5);
```

### 📌 Code:
```csharp
public static LocalDeclarationStatementSyntax BuildConstMaskDeclaration(
    string variableName,
    SpecialType fieldType,
    byte length)
{
    var fieldTypeSyntax = GetTypeSyntaxFromSpecialType(fieldType);
    var maskLiteral = BuildMaskLiteral(fieldType, length);

    return LocalDeclarationStatement(
        VariableDeclaration(
            fieldTypeSyntax,
            SingletonSeparatedList(
                VariableDeclarator(variableName)
                    .WithInitializer(
                        EqualsValueClause(maskLiteral)
                    )
            )
        )
    )
    .WithModifiers(
        TokenList(Token(SyntaxKind.ConstKeyword))
    );
}
```

---

## 🎯 Summary

✔ **BitwiseSyntaxHelpers** makes it easy to generate **bitwise expressions**, **fields**, and **accessors** in **C# syntax trees**.  
✔ **Great for code generation, Roslyn-based analysis, and binary data manipulation.**  
✔ **Optimized for performance and clarity.**  
