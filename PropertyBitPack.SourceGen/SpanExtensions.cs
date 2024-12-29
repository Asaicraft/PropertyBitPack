using System;
using System.Collections.Generic;
using System.Text;

namespace PropertyBitPack.SourceGen;
public static class SpanExtensions
{
    public static bool Contains<T>(this in ReadOnlySpan<T> span, T value, IEqualityComparer<T> comparer)
    {
        for (var i = 0; i < span.Length; i++)
        {
            if (comparer.Equals(span[i], value))
            {
                return true;
            }
        }
        return false;
    }

    public static bool Contains<T>(this in ReadOnlySpan<T> span, T value) => Contains(span, value, EqualityComparer<T>.Default);

    public static bool Any<T>(this in ReadOnlySpan<T> span, Func<T, bool> predicate)
    {
        foreach (var item in span)
        {
            if (predicate(item))
            {
                return true;
            }
        }
        return false;
    }
}
