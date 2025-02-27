# 🚀 ThrowHelper: Efficient Exception Throwing in .NET

## Overview

`ThrowHelper` is a **high-performance exception throwing utility** that helps improve **code clarity** and **runtime performance** by reducing exception-related overhead. It is adapted from the [CommunityToolkit.Diagnostics](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/diagnostics/throwhelper) package.

Instead of **directly throwing exceptions**, `ThrowHelper` provides **dedicated methods** to throw them efficiently, reducing the impact on **JIT optimizations** and **inlining behavior**.

---

## ⚡ Why Use ThrowHelper?

### ✅ Performance Benefits
- **Reduces JIT overhead** → Helps the .NET Just-In-Time (JIT) compiler optimize the code better.
- **Minimizes exception metadata impact** → Avoids unnecessary exception construction in inline paths.
- **Improves maintainability** → Centralizes exception handling logic.

### 📌 Example: Direct Exception vs. ThrowHelper
❌ **Traditional Exception Throwing:**
```csharp
if (value == null)
{
    throw new ArgumentNullException(nameof(value));
}
```
✅ **Using ThrowHelper:**
```csharp
if (value == null)
{
    ThrowHelper.ThrowArgumentNullException(nameof(value));
}
```
💡 **Key Benefit**: The exception is constructed **only when needed**, reducing the performance impact on methods that normally execute without errors.

---

## 🔹 How It Works

### 🚀 Common Exception Helpers
ThrowHelper provides a **dedicated method for each exception type**:

#### Argument-Related Exceptions
```csharp
ThrowHelper.ThrowArgumentNullException("parameterName");
ThrowHelper.ThrowArgumentException("parameterName", "Invalid parameter.");
ThrowHelper.ThrowArgumentOutOfRangeException("parameterName");
```

#### Invalid State Exceptions
```csharp
ThrowHelper.ThrowInvalidOperationException("Operation is not valid.");
ThrowHelper.ThrowNotSupportedException("This feature is not supported.");
ThrowHelper.ThrowObjectDisposedException("objectName");
```

#### Type & Format Errors
```csharp
ThrowHelper.ThrowFormatException("Invalid format detected.");
ThrowHelper.ThrowArrayTypeMismatchException();
```

#### Custom Messages & Inner Exceptions
Each method has **overloads** that allow for detailed exception messages and inner exceptions:
```csharp
ThrowHelper.ThrowArgumentException("parameterName", "Invalid parameter.", new Exception("Inner exception details"));
```

---

## 🏗️ Advanced Use Cases

### ✅ Using Generics for Inline Return
For some exceptions, you may want to **throw and return a generic type**:
```csharp
public static T ThrowUnreachableException<T>(string message)
{
    throw new UnreachableException($"This code should never be reached. {message}");
}
```
**Usage Example:**
```csharp
var result = someCondition ? validValue : ThrowHelper.ThrowUnreachableException<int>("Unexpected case.");
```
🚀 **Benefit:** This allows exception-throwing methods to be **used inside expressions**.

---

## 🛠️ Implementation Details

### 📌 `ThrowHelper` Class Structure
The `ThrowHelper` class is a **static utility** that centralizes exception throwing:
```csharp
internal static partial class ThrowHelper
{
    [DoesNotReturn]
    public static void ThrowArgumentNullException(string? name)
    {
        throw new ArgumentNullException(name);
    }

    [DoesNotReturn]
    public static void ThrowInvalidOperationException(string? message)
    {
        throw new InvalidOperationException(message);
    }

    [DoesNotReturn]
    public static void ThrowUnreachableException(string message)
    {
        throw new UnreachableException($"{UnreachableException.DefaultMessage} {message}");
    }
}
```

### 📌 `DoesNotReturn` Attribute
🔹 Methods are annotated with `[DoesNotReturn]` to help **static analysis tools** understand that these methods **always throw exceptions**, improving **code flow analysis**.

---

## 🔥 Summary

🚀 `ThrowHelper` is a **performance-friendly way** to throw exceptions while improving **code maintainability and JIT optimizations**.

✔ **Use it to reduce metadata overhead in exception paths.**  
✔ **Encapsulate exception logic for better maintainability.**  
✔ **Ensure better inline optimizations and runtime performance.**

---
📖 **Reference:** [CommunityToolkit.Diagnostics ThrowHelper](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/diagnostics/throwhelper)
