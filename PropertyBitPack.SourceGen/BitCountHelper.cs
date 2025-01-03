﻿using Microsoft.CodeAnalysis;
using PropertyBitPack.SourceGen.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PropertyBitPack.SourceGen;

/// <summary>
/// A helper static class to determine how many bits are needed 
/// for a property based on its type or an explicit BitsCount attribute.
/// </summary>
internal static class BitCountHelper
{
    /// <summary>
    /// Gets the bit count for a <see cref="BaseBitFieldPropertyInfo"/>.
    /// If <see cref="AttributeParsedResult.BitsCount"/> is null,
    /// it infers the bit count from the property's type (e.g. byte=8, int=32, etc.).
    /// </summary>
    /// <param name="prop">The bit field property info.</param>
    /// <returns>The number of bits required.</returns>
    public static int GetEffectiveBitsCount(BaseBitFieldPropertyInfo prop)
    {
        // If the attribute explicitly sets BitsCount, return that value.
        if (prop.AttributeParsedResult.BitsCount.HasValue)
        {
            return prop.AttributeParsedResult.BitsCount.Value;
        }

        // Otherwise, infer the bit count from the property's SpecialType.
        // You can expand or modify this switch for other types if needed.
        return prop.PropertyType.SpecialType switch
        {
            SpecialType.System_Boolean => 1,
            SpecialType.System_Byte or SpecialType.System_SByte => 8,
            SpecialType.System_Int16 or SpecialType.System_UInt16 => 16,
            SpecialType.System_Int32 or SpecialType.System_UInt32 => 32,
            SpecialType.System_Int64 or SpecialType.System_UInt64 => 64,

            // Fallback case: if it's something else, you might decide to handle it differently
            // (e.g. return 0 or throw a diagnostic). Here we'll assume "no suitable bits" -> 0
            _ => 0
        };
    }
}