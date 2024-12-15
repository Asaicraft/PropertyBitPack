using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyBitPack.SourceGen;
public static class PropertyBitPackThrowHelper
{
    [DoesNotReturn]
    public static void ThrowUnreachableException(string message)
    {
        throw new UnreachableException($"{UnreachableException.DefaultMessage}{message}");
    }

    [DoesNotReturn]
    public static T ThrowUnreachableException<T>(string message)
    {
        throw new UnreachableException($"{UnreachableException.DefaultMessage}{message}");
    }

    [DoesNotReturn]
    public static void ThrowUnreachableException()
    {
        throw new UnreachableException();
    }

    [DoesNotReturn]
    public static T ThrowUnreachableException<T>()
    {
        throw new UnreachableException();
    }

    [DoesNotReturn]
    public static void ThrowIndexOutOfRangeException()
    {
        throw new IndexOutOfRangeException();
    }

    [DoesNotReturn]
    public static void ThrowIndexOutOfRangeException(string message)
    {
        throw new IndexOutOfRangeException(message);
    }

    [DoesNotReturn]
    public static T ThrowIndexOutOfRangeException<T>()
    {
        throw new IndexOutOfRangeException();
    }    

    [DoesNotReturn]
    public static T ThrowIndexOutOfRangeException<T>(string message)
    {
        throw new IndexOutOfRangeException(message);
    }
}
