using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace PropertyBitPack.SourceGen;
internal static class StringBuildersPool
{
    private static readonly ConcurrentStack<StringBuilder> _stringBuilders = [];

    static StringBuildersPool()
    {
        for (var i = 0; i < 3; i++)
        {
            _stringBuilders.Push(new());
        }
    }

    public static void Return(StringBuilder stringBuilder)
    {
        if(stringBuilder == null)
        {
            return;
        }

        if(_stringBuilders.Count > 4)
        {
            return;
        }

        stringBuilder.Clear();
        _stringBuilders.Push(stringBuilder);
    }

    public static StringBuilder RentInternal()
    {
        return _stringBuilders.TryPop(out var stringBuilder) ? stringBuilder : new();
    }

    public static RentedStringBuilderPool Rent()
    {
        return new RentedStringBuilderPool(RentInternal());
    }


    public static void Return(RentedStringBuilderPool stringBuilder)
    {
        stringBuilder.Dispose();
    }

    public readonly ref struct RentedStringBuilderPool(StringBuilder stringBuilder)
    {
        private readonly StringBuilder _stringBuilder = stringBuilder;

        public readonly StringBuilder StringBuilder =>  _stringBuilder;

        public void Dispose()
        {
            Return(_stringBuilder);
        }

        public override string ToString()
        {
            return _stringBuilder.ToString();
        }
    }
    
}
